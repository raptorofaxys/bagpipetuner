using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Threading;

namespace TunerSimulator
{
    class SoundOutput
    {
        public class Sample
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

        public static void TestAudio(Sample sample)
        {
            // Could use SignalGenerator for sine/triangle/saw/etc.
            var provider = new BufferResamplerProvider();
            provider.Samples = sample.Samples;
            provider.SignalFrequency = sample.Frequency;
            provider.DesiredFrequency = 220;
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

            for (int i = 0; i < 10; ++i)
            {
                Thread.Sleep(1000);
            }

            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
        }
    }
}
