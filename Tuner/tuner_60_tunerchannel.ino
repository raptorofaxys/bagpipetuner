class TunerChannel
{
public:
    TunerChannel()
    {
        m_a440.f = DEFAULT_A440;

        m_tunerNote = -1;

        m_enableDetailedSearch = true;

        m_gcfStep = 4;
        m_correlationDipThresholdPercent = 25;
        m_baseOffsetStep = 4;
        m_baseOffsetStepIncrement = 2;

        m_minFrequency = ABSOLUTE_MIN_FREQUENCY;
        m_maxFrequency = ABSOLUTE_MAX_FREQUENCY;
    }

    void SetPin(int audioPin)
    {
        m_audioPin = audioPin;
    }

    void SelectADCChannel()
    {
        // Select input channel + set reference to Vcc
        unsigned char admux = ADMUX;
        admux &= ~(0x0F);
        admux |= m_audioPin;
        ADMUX = admux;
    }

    // This function requires proper setup in Tuner::Start() and SelectADCChannel()
    unsigned int ReadInput8BitsUnsigned()
    {
        while ((ADCSRA & _BV(ADIF)) == 0)
        {
        }

        unsigned int result = ADCH;
        sbi(ADCSRA, ADIF);
        return result;
    }

    int ReadInput8BitsSigned()
    {
        int result = ReadInput8BitsUnsigned() - 128;
        return result;
    }

    // Linearly interpolates between two 8-bit signed values.
    char InterpolateChar(char a, char b, char tFrac)
    {
        int d = b - a;
        return a + static_cast<char>((d * tFrac) >> FIXED_SHIFT);
    }

    unsigned long /*__attribute__((noinline))*/ GetCorrelationFactorFixed(Fixed fixedOffset, int windowSize, int correlationStep)
    {
        unsigned long result = 0;
        int integer = FIXED_INT(fixedOffset);
        int frac = FIXED_FRAC(fixedOffset);

        char* pA = &g_recordingBuffer[0];
        char* pB = &g_recordingBuffer[integer];
        char* pB2 = pB + 1;
        char* pEndA = pA + windowSize;

        for (; pA < pEndA; pA += correlationStep, pB += correlationStep, pB2 += correlationStep)
        {
            // Note this is done with 16-bit math; this is slower, but gives more precision. In tests, using 8-bit fixed-point
            // math did not yield sufficient precision.
            int a = *pA;
            int b = InterpolateChar(*pB, *pB2, frac);
            result += abs(b - a);
        }
        return result;
    }

    // Compute the frequency corresponding to a given a fixed-point offset into our sampling buffer (usually where
    // the best/minimal autocorrelation was achieved).
    float GetFrequencyForOffsetFixed(Fixed offset)
    {
        float floatOffset = FIXED2F(offset);
        return F_CPU / (floatOffset * CPU_CYCLES_PER_SAMPLE);
    }

    float DetermineSignalPitch(float& minFrequency, float& maxFrequency, int& signalMin, int& signalMax, int& maxCorrelationDipPercent)
    {
        int minSamples = SAMPLES_PER_SECOND / m_maxFrequency;
        int maxSamples = SAMPLES_PER_SECOND / m_minFrequency;
        int bufferSize = 2 * maxSamples + 1;

        minFrequency = -1.0f;
        maxFrequency = -1.0f;

        //DEFAULT_PRINT->print("DetermineSignalPitch()"); Ln();
        static int const AMPLITUDE_THRESHOLD = 40;

        DEBUG_PRINT_STATEMENTS(Serial.write("DetermineSignalPitch()"); Ln(););

        // Sample the signal into our buffer, and track its amplitude.
        signalMin = INT_MAX;
        signalMax = INT_MIN;
        int maxAmplitude = -1;
        for (int i = 0; i < bufferSize; ++i)
        {
            g_recordingBuffer[i] = ReadInput8BitsSigned();
            signalMin = min(g_recordingBuffer[i], signalMin);
            signalMax = max(g_recordingBuffer[i], signalMax);
            maxAmplitude = max(maxAmplitude, abs(signalMax - signalMin));
        }

        DEBUG_PRINT_STATEMENTS(
        {
            PrintStringInt("cdp", m_correlationDipThresholdPercent); Ln();
        PrintStringInt("gcfs", m_gcfStep); Ln();
        PrintStringInt("bos", m_baseOffsetStep); Ln();
        PrintStringInt("bosi", m_baseOffsetStepIncrement); Ln();
        PrintStringInt("minf", m_minFrequency); Ln();
        PrintStringInt("maxf", m_maxFrequency); Ln();
        PrintStringInt("bufferSize", bufferSize); Ln();
        PrintStringInt("signalMin", signalMin); Ln();
        PrintStringInt("signalMax", signalMax); Ln();
        PrintStringInt("maxAmplitude", maxAmplitude); Ln();
        });

        // If we haven't reached the amplitude threshold, don't try to determine pitch.
        if (maxAmplitude < AMPLITUDE_THRESHOLD)
        {
            //DEFAULT_PRINT->print("DetermineSignalPitch() no amplitude"); Ln();
            return -1.0f;
        }

        bool doPrint = false;
        Fixed minBestOffset = ~0;
        Fixed maxBestOffset = ~0;
        Fixed bestOffset = ~0;

        // Normally we only run once through the below code. This loop is only here in case we need to print the result of some computations. This is typically done when an abnormal result is obtained.
        for (;;)
        {
            // Alright, now try to figure what the ballpark note this is by calculating autocorrelation
            bool inThreshold = false;

            const Fixed maxSamplesFixed = I2FIXED(maxSamples);
            unsigned long maxCorrelation = 0;
            unsigned long correlationDipThreshold = 0;
            unsigned long bestCorrelation = ~0;
            const Fixed offsetToStartPreciseSampling = max(I2FIXED(minSamples - 2), 0);
            unsigned long lastCorrelation = ~0;

            Fixed offsetStep = m_baseOffsetStep;

            if (doPrint && (g_dumpMode == DumpMode::DumpBuffer))
            {
                for (int i = 0; i < bufferSize; ++i)
                {
                    DEFAULT_PRINT->print("#");
                    DEFAULT_PRINT->print(static_cast<int>(g_recordingBuffer[i]));
                    Ln();
                }
            }

            // We start a bit before the minimum offset to prime the thresholds
            //for (Fixed offset = max(offsetAtMinFrequency - OFFSET_STEP * 4, 0); offset < maxSamplesFixed; offset += OFFSET_STEP)
            // make a function out of the subdivision loop; scan from offset to offset with a given step and adaptive parameters, with a given skip for GCF
            for (Fixed offset = (offsetToStartPreciseSampling >> 1); offset < maxSamplesFixed; )
            {
                //@TODO: why the shift here? is this a legacy artifact?
                //unsigned long curCorrelation = GetCorrelationFactorFixed(offset, 2) << 8; // was using 96, which worked for the simple function generator but didn't work quite as well for the bagpipe signal
                unsigned long curCorrelation = GetCorrelationFactorFixed(offset, maxSamples, m_gcfStep);

                if (doPrint && (g_dumpMode == DumpMode::DumpGcf))
                {
                    DEFAULT_PRINT->print("#");
                    DEFAULT_PRINT->print(offset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    DEFAULT_PRINT->print(curCorrelation); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    DEFAULT_PRINT->print(maxCorrelation); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    DEFAULT_PRINT->print(correlationDipThreshold);
                    Ln();
                }

                if (curCorrelation > maxCorrelation)
                {
                    maxCorrelation = curCorrelation;
                    correlationDipThreshold = (maxCorrelation * m_correlationDipThresholdPercent) / 100;
                }

                if (offset < offsetToStartPreciseSampling)
                {
                    offset += m_baseOffsetStep * 4;
                    continue;
                }

                if (curCorrelation < correlationDipThreshold)
                {
                    if (curCorrelation < bestCorrelation)
                    {
                        bestCorrelation = curCorrelation;
                        minBestOffset = offset;
                        maxBestOffset = offset;
                    }
                    else if (curCorrelation == bestCorrelation)
                    {
                        maxBestOffset = offset;
                    }

                    inThreshold = true;
                }
                else if (inThreshold) // was in threshold, now exited, have best minimum in threshold
                {
                    break;
                }

                if (curCorrelation >= lastCorrelation)
                {
                    offsetStep += m_baseOffsetStepIncrement;
                }
                else
                {
                    offsetStep = m_baseOffsetStep;
                }

                offset += offsetStep;
                lastCorrelation = curCorrelation;
            }

            if (!doPrint
                && (
                ((g_dumpBelowFrequency >= 0.0f) && (minBestOffset != ~0) && (GetFrequencyForOffsetFixed(minBestOffset) < g_dumpBelowFrequency))
                    || (g_dumpOnNullReading && (minBestOffset == ~0))
                    )
                )
            {
                doPrint = true;
                continue;
            }

            if (minBestOffset == ~0)
            {
                return -1.0f;
            }

            maxCorrelationDipPercent = (bestCorrelation * 100) / maxCorrelation;

            if (!m_enableDetailedSearch)
            {
                break;
            }

            //@TODO: upsample in stages, reducing the GCF step gradually
            Fixed lowOffset = max(minBestOffset - offsetStep, 0);
            unsigned long lowGcf = GetCorrelationFactorFixed(lowOffset, maxSamples, 2);
            Fixed highOffset = maxBestOffset + offsetStep;
            unsigned long highGcf = GetCorrelationFactorFixed(highOffset, maxSamples, 2);

            DEBUG_PRINT_STATEMENTS(
            {
                PrintStringInt("minBestOffset", minBestOffset); Ln();
            PrintStringInt("maxBestOffset", maxBestOffset); Ln();
            });

            int iterationCount = 0;
            for (; iterationCount < 100; ++iterationCount)
            {
                Fixed lowOffsetOption = (2 * static_cast<DoubleFixed>(lowOffset) + highOffset) / 3;
                unsigned long lowGcfOption = GetCorrelationFactorFixed(lowOffsetOption, maxSamples, 2);
                Fixed highOffsetOption = (lowOffset + 2 * static_cast<DoubleFixed>(highOffset)) / 3;
                unsigned long highGcfOption = GetCorrelationFactorFixed(highOffsetOption, maxSamples, 2);

                //DEFAULT_PRINT->print("#");
                //DEFAULT_PRINT->print(m_minFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(m_maxFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(m_correlationDipThresholdPercent); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(m_gcfStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(m_baseOffsetStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(m_baseOffsetStepIncrement);
                //Ln();

                //DEFAULT_PRINT->print("#");
                //DEFAULT_PRINT->print(lowOffset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(lowGcf); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(highOffset); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(highGcf); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(lowOffsetOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(lowGcfOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(highOffsetOption); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //DEFAULT_PRINT->print(highGcfOption); // DEFAULT_PRINT->print(COMMA_SEPARATOR);
                //Ln();

                if (highOffset - lowOffset <= 3)
                {
                    // We are ready to break out - simply find which of the four points we have has the minimum
                    // GCF
                    unsigned long minGcf = min(min(min(lowGcf, lowGcfOption), highGcfOption), highGcf);

                    // Set lowOffset as our best result; bias low
                    if (lowGcf == minGcf)
                    {
                        bestOffset = lowOffset;
                    }
                    else if (lowGcfOption == minGcf)
                    {
                        bestOffset = lowOffsetOption;
                    }
                    else if (highGcfOption == minGcf)
                    {
                        bestOffset = highOffsetOption;
                    }
                    else if (highGcf == minGcf)
                    {
                        bestOffset = highOffset;
                    }

                    //DEFAULT_PRINT->print("#");
                    //DEFAULT_PRINT->print(lowOffset);
                    //Ln();

                    // We're done
                    break;
                }

                // Home in on the minimum. We want to keep the lowest point inside the extrema of our search.
                if (lowGcfOption < highGcfOption)
                {
                    highOffset = highOffsetOption;
                    highGcf = highGcfOption;
                }
                else
                {
                    lowOffset = lowOffsetOption;
                    lowGcf = lowGcfOption;
                }
            }

            break;
        }

        if (bestOffset == ~0)
        {
            bestOffset = (minBestOffset + maxBestOffset) / 2;
        }

        float result = GetFrequencyForOffsetFixed(bestOffset);
        minFrequency = GetFrequencyForOffsetFixed(maxBestOffset);
        maxFrequency = GetFrequencyForOffsetFixed(minBestOffset);

        DEBUG_PRINT_STATEMENTS(
        {
            PrintStringFloat("result", result); Ln();
        PrintStringFloat("minFrequency", minFrequency); Ln();
        PrintStringFloat("maxFrequency", maxFrequency); Ln();
        });

        return result;
    }

    void Update()
    {
        // Select the right ADC channel
        SelectADCChannel();

        DEBUG_PRINT_STATEMENTS(Serial.write("DetermineSignalPitch"); Ln(););
        float minSignalFrequency = -1.0f;
        float maxSignalFrequency = -1.0f;
        int signalMin = 0;
        int signalMax = 0;
        int maxCorrelationDipPercent = 0;
        long dspTotalMs = -millis();
        float instantFrequency = DetermineSignalPitch(minSignalFrequency, maxSignalFrequency, signalMin, signalMax, maxCorrelationDipPercent);

        dspTotalMs += millis();
        DEBUG_PRINT_STATEMENTS(PrintStringFloat("instantFrequency", instantFrequency); Ln(););

        DEBUG_PRINT_STATEMENTS(Serial.write("GetMidiNoteIndexForFrequency"); Ln(););
        m_tunerNote = GetMidiNoteIndexForFrequency(instantFrequency, m_a440.f);
        float centerDisplayFrequency = GetFrequencyForMidiNoteIndex(m_tunerNote, m_a440.f);
        float minDisplayFrequency = centerDisplayFrequency * FREQUENCY_FILTER_WINDOW_RATIO_DOWN;
        float maxDisplayFrequency = centerDisplayFrequency * FREQUENCY_FILTER_WINDOW_RATIO_UP;

        DEBUG_PRINT_STATEMENTS(Serial.write("instantFrequency"); Ln(););
        if (instantFrequency >= 0.0f)
        {
            if ((m_filteredFrequency >= 0.0f) && (m_filteredFrequency >= minDisplayFrequency) && (m_filteredFrequency <= maxDisplayFrequency))
            {
                static float const RATE = 0.1f;
                m_filteredFrequency = (1.0f - RATE) * m_filteredFrequency + (RATE * instantFrequency);
            }
            else
            {
                m_filteredFrequency = instantFrequency;
            }
        }
        else
        {
            m_filteredFrequency = -1.0f;
        }

        DEFAULT_PRINT->print('r');
        Serial.print(m_audioPin); Serial.print(COMMA_SEPARATOR);
        PrintFloat(instantFrequency); Serial.print(COMMA_SEPARATOR);
        PrintFloat(minSignalFrequency); Serial.print(COMMA_SEPARATOR);
        PrintFloat(maxSignalFrequency); Serial.print(COMMA_SEPARATOR);
        Serial.print(signalMin); Serial.print(COMMA_SEPARATOR);
        Serial.print(signalMax); Serial.print(COMMA_SEPARATOR);
        Serial.print(dspTotalMs); Serial.print(COMMA_SEPARATOR);
        Serial.print(maxCorrelationDipPercent);
        Ln();
    }

    bool m_enableDetailedSearch;

    int m_minFrequency;
    int m_maxFrequency;
    int m_correlationDipThresholdPercent;
    int m_gcfStep;
    int m_baseOffsetStep;
    int m_baseOffsetStepIncrement;

    FourByteUnion m_a440;

private:
    int m_audioPin;
    int m_tunerNote;
    float m_filteredFrequency;
};