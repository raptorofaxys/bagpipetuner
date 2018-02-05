using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace TunerSimulator
{
    public class Sample
    {
        public readonly float[] Samples;
        public readonly float Frequency;
        public readonly string Filename;

        public Sample(string filename, float frequency)
        {
            Filename = filename;
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
        object m_lock = new object();

        Sample m_sample;
        public Sample Sample
        {
            get
            {
                return m_sample;
            }
            set
            {
                lock (m_lock)
                {
                    m_sample = value;
                }
            }
        }

        public float DesiredFrequency;

        public float SampleCursor { get; private set; }

        public void RandomizePosition()
        {
            var r = new Random();
            lock (m_lock)
            {
                SampleCursor = r.Next() % m_sample.Samples.Length;
            }
        }

        float GetSample(float index)
        {
            var firstSampleIndex = (int)index % m_sample.Samples.Length;
            var nextSampleIndex = (firstSampleIndex + 1) % m_sample.Samples.Length;
            var frac = index - (int)index;

            return (m_sample.Samples[firstSampleIndex] * (1.0f - frac)) + (m_sample.Samples[nextSampleIndex] * frac);
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            lock (m_lock)
            {
                if (m_sample != null)
                {
                    SampleCursor %= m_sample.Samples.Length;

                    float samplingRatio = DesiredFrequency / m_sample.Frequency;

                    for (var i = 0; i < sampleCount; ++i, SampleCursor += samplingRatio)
                    {
                        buffer[offset + i] = GetSample(SampleCursor);
                    }
                }
                else
                {
                    for (var i = 0; i < sampleCount; ++i)
                    {
                        buffer[offset + i] = 0.0f;
                    }
                }
            }

            return sampleCount;
        }
    }

    class SoundOutput
    {
        public Sample Sample
        {
            get
            {
                return m_provider.Sample;
            }
            set
            {
                m_provider.Sample = value;
            }
        }

        public float DesiredFrequency
        {
            get
            {
                return m_provider.DesiredFrequency;
            }
            set
            {
                m_provider.DesiredFrequency = value;
            }
        }

        BufferResamplerProvider m_provider = new BufferResamplerProvider();
        WaveOut m_waveOut;

        public void Start()
        {
            Trace.Assert(m_waveOut == null);
            m_provider.SetWaveFormat(44100, 1);

            var outputNumber = -1;
            for (var i = 0; i < WaveOut.DeviceCount; ++i)
            {
                var caps = WaveOut.GetCapabilities(i);
                Console.WriteLine("{0}: {1}", i, caps.ProductName);
                if (caps.ProductName.Contains("3/4"))
                {
                    outputNumber = i;
                }
            }

            if (outputNumber < 0)
            {
                Console.WriteLine("Unable to locate Saffire 3/4 output; defaulting to output 0");
                outputNumber = 0;

                if (WaveOut.DeviceCount < 1)
                {
                    throw new Exception("No audio device outputs available");
                }
            }

            m_waveOut = new WaveOut();
            m_waveOut.DeviceNumber = outputNumber;
            m_waveOut.DesiredLatency = 100;

            m_waveOut.Init(m_provider);
            m_waveOut.Play();

            //for (int i = 0; i < 10; ++i)
            //{
            //    Thread.Sleep(1000);
            //}
        }

        public void RandomizePosition()
        {
            m_provider.RandomizePosition();
        }

        public void Stop()
        {
            Trace.Assert(m_waveOut != null);
            m_waveOut.Stop();
            m_waveOut.Dispose();
            m_waveOut = null;
        }

        //public void TestAudio(Sample sample)
        //{
        //    // Could use SignalGenerator for sine/triangle/saw/etc.
        //    var provider = new BufferResamplerProvider();
        //    provider.Sample = sample;
        //    provider.SignalFrequency = sample.Frequency;
        //    provider.DesiredFrequency = 220;
        //    provider.SetWaveFormat(44100, 1);

        //    for (var i = 0; i < WaveOut.DeviceCount; ++i)
        //    {
        //        var caps = WaveOut.GetCapabilities(i);
        //        Console.WriteLine("{0}: {1}", i, caps.ProductName);
        //    }

        //    var waveOut = new WaveOut();
        //    waveOut.DeviceNumber = 8;
        //    waveOut.Init(provider);
        //    Console.WriteLine(waveOut);
        //    waveOut.Play();

        //    for (int i = 0; i < 10; ++i)
        //    {
        //        Thread.Sleep(1000);
        //    }

        //    waveOut.Stop();
        //    waveOut.Dispose();
        //    waveOut = null;
        //}
    }
}
