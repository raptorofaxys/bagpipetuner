class Tuner
{
public:
    static int const NUM_CHANNELS = 4;

    Tuner()
    {
        for (int i = 0; i < NUM_CHANNELS; ++i)
        {
            m_channels[i].SetPin(i);
        }

        m_oneChannelMode = false;

        DEBUG_PRINT_STATEMENTS(Serial.write("Constructing tuner..."); Ln(););
    }

    void Start()
    {
        DEBUG_PRINT_STATEMENTS(Serial.write("Starting tuner..."); Ln(););

        // Enable auto-trigger enable
        sbi(ADCSRA, ADATE);
        // Set auto-trigger to free-running mode
        cbi(ADCSRB, ADTS0);
        cbi(ADCSRB, ADTS1);
        cbi(ADCSRB, ADTS2);

        ADMUX = (1 << 6);

        // Left-adjust result so we only have to read 8 bits
        sbi(ADMUX, ADLAR); // right-adjust for 8 bits

                           // Setup the prescaler
        unsigned char adcsra = ADCSRA;
        adcsra = ((adcsra & 0xF8) | PRESCALER); // mask off / re-set prescaler bits
        ADCSRA = adcsra;

        // Disable the conversion complete interrupt so we can read the flag
        cbi(ADCSRA, ADIE); // Interrupt Enable

                           // Start the shebang
        sbi(ADCSRA, ADSC);

        LoadTuning();
        //TunePitch();
    }

    void Stop()
    {
        DEBUG_PRINT_STATEMENTS(Serial.write("Stopping tuner..."); Ln(););

        // Disable auto-trigger mode
        cbi(ADCSRA, ADATE);

        // Right-adjust result (default)
        cbi(ADMUX, ADLAR);
    }

    unsigned long LoadEepromLong(int address)
    {
        unsigned long result = 0;
        unsigned long t = 0; // to avoid spurious warning :/ 
        result |= EEPROM.read(address);
        t = EEPROM.read(address + 1);
        result |= (t << 8);
        t = EEPROM.read(address + 2);
        result |= (t << 16);
        t = EEPROM.read(address + 3);
        result |= (t << 24);
        return result;
    }

    void SaveEepromLong(int address, unsigned long i)
    {
        EEPROM.write(address, i & 0xFF);
        EEPROM.write(address + 1, (i >> 8) & 0xFF);
        EEPROM.write(address + 2, (i >> 16) & 0xFF);
        EEPROM.write(address + 3, (i >> 24) & 0xFF);
    }

    void SaveTuning()
    {
        SaveEepromLong(0, m_a440.ul);
    }

    void LoadTuning()
    {
        //Serial.println("Saving default tuning");
        m_a440.f = DEFAULT_A440; //@HACK
        SaveTuning();
        m_a440.ul = LoadEepromLong(0);
        //PrintStringFloat("Loaded test tuning", m_a440.f);

        //m_a440.ul = LoadEepromLong(0);
        m_a440.f = DEFAULT_A440; //@HACK
        if ((m_a440.f < ABSOLUTE_MIN_FREQUENCY) || (m_a440.f > ABSOLUTE_MAX_FREQUENCY))
        {
            DEBUG_PRINT_STATEMENTS(
            {
                PrintStringFloat("Bad tuning", m_a440.f); Ln();
            Serial.println("No saved tuning or corrupted value, loading default");
            });
            m_a440.f = DEFAULT_A440;
            SaveTuning();
        }
        DEBUG_PRINT_STATEMENTS(PrintStringFloat("Loaded tuning", m_a440.f); Ln(););
    }

    // Given a MIDI note index, returns the corresponding index into the global string array of note names.
    int GetNoteNameIndex(int note)
    {
        return (note + 3) % 12;
    }

    // Given a MIDI note index, get the name of the note as ASCII.
    const char* GetNoteName(int note)
    {
        if (note >= 0)
        {
            return g_noteNames[GetNoteNameIndex(note)];
        }
        else
        {
            return "  ";
        }
    }

    void TunePitch()
    {
        unsigned long lastPress = millis();
        bool firstTime = true;
        while (millis() - lastPress < 2000)
        {
            g_pitchDownButton.Update();
            g_pitchUpButton.Update();

            bool buttonPressed = firstTime;
            if (g_pitchDownButton.JustPressed())
            {
                m_a440.f -= 1.0f;
                buttonPressed = true;
            }

            if (g_pitchUpButton.JustPressed())
            {
                m_a440.f += 1.0f;
                buttonPressed = true;
            }

            if (m_a440.f < ABSOLUTE_MIN_FREQUENCY)
            {
                m_a440.f = ABSOLUTE_MIN_FREQUENCY;
            }

            if (m_a440.f > ABSOLUTE_MAX_FREQUENCY)
            {
                m_a440.f = ABSOLUTE_MAX_FREQUENCY;
            }

            if (buttonPressed)
            {
                lastPress = millis();
            }

            firstTime = false;
        }

        SaveTuning();
    }

    void Go()
    {
        DEFAULT_PRINT->print("#Tuner start"); Ln();

        DEBUG_PRINT_STATEMENTS(Serial.write("Initializing..."); Ln(););

        Start();

        int activeChannelIndex = 0;

        while (1)
        {
            if (Serial.available())
            {
                bool locked = false;
                while (Serial.available() || locked)
                {
                    while (locked && !Serial.available())
                    {
                    }

                    char command = Serial.read();

                    int value = 0;
                    if (command != '\n')
                    {
                        long startMs = millis();
                        value = Serial.parseInt();
                        long endMs = millis();
                        if (endMs - startMs > 500)
                        {
                            DEFAULT_PRINT->print("#long parseInt on command: "); Ln();
                        }
                    }

                    char newLine = Serial.read();
                    if (newLine != '\n')
                    {
                        DEFAULT_PRINT->print("#WARNING: BAD SYNC"); Ln();
                    }

                    // This command is done reading from the serial buffer. Indicate that we're ready for another one.
                    DEFAULT_PRINT->print("c"); Ln();

                    //DEFAULT_PRINT->print("#read command: "); DEFAULT_PRINT->print(command); DEFAULT_PRINT->print("("); DEFAULT_PRINT->print(static_cast<int>(command)); DEFAULT_PRINT->print(")"); Ln();
                    //DEFAULT_PRINT->print("#value: "); DEFAULT_PRINT->print(value); Ln(); //where is all the echo output?
                    switch (command)
                    {
                    case 'l': locked = (value != 0); break;
                    case 'a': g_dumpOnLowAmplitude = (value != 0); break;
                    case 'i': g_dumpOnNullReading = (value != 0); break;
                    case 'f': g_dumpBelowFrequency = value; break;
                    case 'c': activeChannelIndex = value; break;
                    case 'm': m_channels[activeChannelIndex].m_minFrequency = max(value, ABSOLUTE_MIN_FREQUENCY); break;
                    case 'M': m_channels[activeChannelIndex].m_maxFrequency = min(value, ABSOLUTE_MAX_FREQUENCY); break;
                    case 'p': m_channels[activeChannelIndex].m_correlationDipThresholdPercent = value; break;
                    case 'g': m_channels[activeChannelIndex].m_gcfStep = max(value, 1); break;
                    case 'o': m_channels[activeChannelIndex].m_baseOffsetStep = max(value, 1); break;
                    case 's': m_channels[activeChannelIndex].m_baseOffsetStepIncrement = value; break;
                    case 'x': m_channels[activeChannelIndex].m_enableDetailedSearch = (value != 0); break;
                    case 'd': g_dumpMode = static_cast<DumpMode::Type>(value); break;
                    case 'X': m_oneChannelMode = (value != 0); break;
                    case 'e':
                    {
                        DEFAULT_PRINT->print("e");
                        DEFAULT_PRINT->print(value);
                        Ln();
                    }
                    break;
                    case '\n':
                        // In principle this should not occur
                        DEFAULT_PRINT->print("#WARNING: STANDALONE NEWLINE"); Ln();
                        break;
                    }

                    //DEFAULT_PRINT->print("#state ");
                    //DEFAULT_PRINT->print(g_dumpOnNullReading); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(g_dumpBelowFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(g_dumpMode); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(activeChannelIndex); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_minFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_maxFrequency); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_correlationDipThresholdPercent); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_gcfStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_baseOffsetStep); DEFAULT_PRINT->print(COMMA_SEPARATOR);
                    //DEFAULT_PRINT->print(m_channels[activeChannelIndex].m_baseOffsetStepIncrement);
                    //Ln();

                    // Give a little time to react in case the host wants to send multiple commands in a row
                    //delay(25);
                }
            }
            else
            {
                // Nothing received - we are ready for more
                DEFAULT_PRINT->print("c"); Ln();
            }

            DEBUG_PRINT_STATEMENTS(Serial.write("Main tuner loop"); Ln(););

            DEBUG_PRINT_STATEMENTS(Serial.write("Button updates"); Ln(););
            g_pitchDownButton.Update();
            g_pitchUpButton.Update();

            if (g_pitchDownButton.IsPressed() || g_pitchUpButton.IsPressed())
            {
                TunePitch();
            }

            if (!m_oneChannelMode)
            {
                for (unsigned int i = 0; i < ARRAY_COUNT(m_channels); ++i)
                {
                    m_channels[i].m_a440 = m_a440;
                    m_channels[i].Update();
                }
            }
            else
            {
                m_channels[0].Update();
            }
        }

        Stop();
    }

private:
    FourByteUnion m_a440;

    bool m_oneChannelMode;
    TunerChannel m_channels[NUM_CHANNELS];
};