﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TunerSimulator
{
    class Program
    {
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

        static void Main(string[] args)
        {
            var sample = LoadSample("Chanter.raw");
            //var sample = LoadSample("Tenor drone.raw");
            var log = new StringBuilder();

            var numCorrelations = 0;
            var sum = 0.0f;
            var step = 1.0f / 4;
            var gcfStep = 1;
            var baseScale = 24;
            for (var offset = step; offset < MAX_SAMPLES; offset += step)
            {
                ++numCorrelations;
                var correlation = GetCorrelationFactor(sample, offset, gcfStep);
                sum += correlation;
                var prime = GetCorrelationFactorPrime(correlation, numCorrelations, sum);
                log.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}\n", offset, correlation, //prime,
                    GetCorrelationFactor(sample, offset, gcfStep * 1 * baseScale),
                    GetCorrelationFactor(sample, offset, gcfStep * 2 * baseScale),
                    GetCorrelationFactor(sample, offset, gcfStep * 3 * baseScale),
                    GetCorrelationFactor(sample, offset, gcfStep * 4 * baseScale),
                    GetCorrelationFactor(sample, offset, gcfStep * 5 * baseScale));
            }

            File.WriteAllText("output.txt", log.ToString());
        }
    }
}