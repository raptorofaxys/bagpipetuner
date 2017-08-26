using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Threading;
using NAudio.Wave.SampleProviders;

namespace TunerSimulator
{
    class Program
    {
        class Sample
        {
            public float[] Samples;
            public float Frequency;

            public Sample(string filename, float frequency)
            {
                Frequency = frequency;
                Samples = LoadSample(filename);
            }

            static float[] LoadSample(string filepath)
            {
                var bytes = File.ReadAllBytes(filepath);
                var numSamples = bytes.Length / sizeof(float);
                var result = new float[numSamples];
                Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

                for (var i = 0; i < result.Length; ++i)
                {
                    result[i] = (int)(result[i] * 128) / 128.0f;
                }

                return result;
            }
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        const int SAMPLES_PER_SECOND = 44100;
        const int MIN_FREQUENCY = 60;
        const int MAX_FREQUENCY = 1100;
        const int MAX_SAMPLES = SAMPLES_PER_SECOND / MIN_FREQUENCY;
        const int WINDOW_SIZE = MAX_SAMPLES;

        static float GetCorrelationFactor(float[] sample, float offset, int step)
        {
            var result = 0.0f;
            var integer = (int)offset;
            var frac = offset - integer;
            //var step = 1;

            for (int i = 0; i < WINDOW_SIZE; i += step)
            {
                var a = sample[i];
                var b = Lerp(sample[i + integer], sample[i + integer + 1], frac);
                result += Math.Abs(b - a);
            }

            return result * step;
            //return result;
        }

        static float GetCorrelationFactorPrime(float correlation, float numCorrelationsToDate, float sumToDate)
        {
            //if (numCorrelationsToDate == 0)
            //{
            //    return 1.0f;
            //}

            return correlation * numCorrelationsToDate / sumToDate;
        }

        class BufferResamplerProvider : WaveProvider32
        {
            public float[] Samples;
            public float SignalFrequency;
            public float DesiredFrequency;

            private float m_sampleCursor;

            float GetSample(float index)
            {
                var firstSampleIndex = (int)index % Samples.Length;
                var nextSampleIndex = (firstSampleIndex + 1) % Samples.Length;
                var frac = index - (int)index;

                return (Samples[firstSampleIndex] * (1.0f - frac)) + (Samples[nextSampleIndex] * frac);
            }

            public override int Read(float[] buffer, int offset, int sampleCount)
            {
                float samplingRatio = DesiredFrequency / SignalFrequency;

                for (var i = 0; i < sampleCount; ++i, m_sampleCursor += samplingRatio)
                {
                    buffer[offset + i] = GetSample(m_sampleCursor);
                }

                return sampleCount;
            }
        }

        static void TestAudio(Sample sample)
        {
            // Could use SignalGenerator for sine/triangle/saw/etc.
            var provider = new BufferResamplerProvider();
            provider.Samples = sample.Samples;
            provider.SignalFrequency = sample.Frequency;
            provider.DesiredFrequency = 440;
            provider.SetWaveFormat(44100, 1);

            for (var i = 0; i < WaveOut.DeviceCount; ++i)
            {
                var caps = WaveOut.GetCapabilities(i);
                Console.WriteLine("{0}: {1}", i, caps.ProductName);
            }

            var waveOut = new WaveOut();
            waveOut.DeviceNumber = 8;
            waveOut.Init(provider);
            Console.WriteLine(waveOut);
            waveOut.Play();

            for (int i = 0; i < 100; ++i)
            {
                Thread.Sleep(10000);
            }

            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
        }

        static void Main(string[] args)
        {
            var chanter = new Sample("chanter.raw", 594);
            var tenor = new Sample("tenor drone.raw", 239);
            TestAudio(tenor);

            var samples = tenor.Samples;

            var log = new StringBuilder();

            var numCorrelations = 0;
            var sum = 0.0f;
            var step = 1.0f / 4;
            var gcfStep = 1;
            var baseScale = 24;
            for (var offset = step; offset < MAX_SAMPLES; offset += step)
            {
                ++numCorrelations;
                var correlation = GetCorrelationFactor(samples, offset, gcfStep);
                sum += correlation;
                var prime = GetCorrelationFactorPrime(correlation, numCorrelations, sum);
                log.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}\n", offset, correlation, //prime,
                    GetCorrelationFactor(samples, offset, gcfStep * 1 * baseScale),
                    GetCorrelationFactor(samples, offset, gcfStep * 2 * baseScale),
                    GetCorrelationFactor(samples, offset, gcfStep * 3 * baseScale),
                    GetCorrelationFactor(samples, offset, gcfStep * 4 * baseScale),
                    GetCorrelationFactor(samples, offset, gcfStep * 5 * baseScale));
            }

            File.WriteAllText("output.txt", log.ToString());
        }
    }
}
