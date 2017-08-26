using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TunerSimulator
{
    class Program
    {
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

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            //var samples = tenor.Samples;

            //var log = new StringBuilder();

            //var numCorrelations = 0;
            //var sum = 0.0f;
            //var step = 1.0f / 4;
            //var gcfStep = 1;
            //var baseScale = 24;
            //for (var offset = step; offset < MAX_SAMPLES; offset += step)
            //{
            //    ++numCorrelations;
            //    var correlation = GetCorrelationFactor(samples, offset, gcfStep);
            //    sum += correlation;
            //    var prime = GetCorrelationFactorPrime(correlation, numCorrelations, sum);
            //    log.AppendFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}\n", offset, correlation, //prime,
            //        GetCorrelationFactor(samples, offset, gcfStep * 1 * baseScale),
            //        GetCorrelationFactor(samples, offset, gcfStep * 2 * baseScale),
            //        GetCorrelationFactor(samples, offset, gcfStep * 3 * baseScale),
            //        GetCorrelationFactor(samples, offset, gcfStep * 4 * baseScale),
            //        GetCorrelationFactor(samples, offset, gcfStep * 5 * baseScale));
            //}

            //File.WriteAllText("output.txt", log.ToString());
        }
    }
}
