using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TunerSimulator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        bool m_closing = false;
        Task m_audioTask;
        Task m_serialTask;

        object m_frequencyReadingQueueLock = new object();
        Queue<float> m_frequencyReadingQueue = new Queue<float>();

        void EnqueueFrequencyReading(float f)
        {
            lock (m_frequencyReadingQueueLock)
            {
                m_frequencyReadingQueue.Enqueue(f);
            }
        }

        float DequeueFrequencyReading()
        {
            while (m_frequencyReadingQueue.Count == 0)
            {
                Thread.Sleep(10);
            }
            lock (m_frequencyReadingQueueLock)
            {
                return m_frequencyReadingQueue.Dequeue();
            }
        }

        void FullTunerTest()
        {
            //var chanter = new Sample("chanter 594.raw", 594);
            //var tenor = new Sample("tenor drone 239.raw", 239);

            var samples = new Sample[]
            {
                new Sample("Base Drone 119.raw", 119),
                new Sample("Tenor drone 239.raw", 239),
                new Sample("Chanter LowG 420.raw", 420),
                new Sample("Chanter LowA 477.raw", 477),
                new Sample("Chanter B 536.raw", 536),
                new Sample("Chanter C 594.raw", 594),
                new Sample("Chanter D 627.raw", 627),
                new Sample("Chanter E 716.raw", 716),
                new Sample("Chanter F 805.raw", 805),
                new Sample("Chanter HighG 815.raw", 815),
                new Sample("Chanter HighA 943.raw", 943),
            };

            var so = new SoundOutput();
            so.Start();

            //foreach (var s in samples)
            //{
            //    so.Sample = s;
            //    so.DesiredFrequency = s.Frequency;
            //    Thread.Sleep(6000);
            //}

            var rand = new Random();

            //for (int i = 0; (i < 100) && !m_closing; ++i)
            //{
            //    Thread.Sleep(500);
            //    so.Sample = ((rand.Next() % 2) == 0) ? chanter : tenor;
            //    so.DesiredFrequency = 60 + (rand.Next() % 1000);
            //}

            var minFrequency = 80.0f;
            var maxFrequency = 1050.0f;
            var numSteps = 10.0;
            var numTestsPerStep = 10;
            var mulStep = (float)Math.Pow(maxFrequency / minFrequency, 1.0f / (numSteps - 1));

            var results = new Dictionary<string, List<TestResult>>();

            foreach (var s in samples)
            {
                results[s.Filename] = new List<TestResult>();
                Console.WriteLine("Testing {0}", s.Filename);
                for (var frequency = minFrequency; frequency <= maxFrequency; frequency *= mulStep)
                {
                    var result = TestPoint(so, s, frequency, numTestsPerStep);
                    results[s.Filename].Add(result);
                    Console.WriteLine("{0}: {1:0.00}% ({2})", frequency, result.PassRatio * 100.0f, string.Join(", ", result.Readings));
                }
            }

            Console.WriteLine("Frequency, {0}", string.Join(", ", samples.Select(s => s.Filename)));
            var anyListOfResults = results[samples[0].Filename];
            for (var i = 0; i < anyListOfResults.Count; ++i)
            {
                Console.WriteLine("{0}, {1}", anyListOfResults[i].Frequency, string.Join(", ", results.Select(kv => kv.Value[i].PassRatio)));
            }

            so.Stop();
        }

        class TestResult
        {
            public float Frequency;
            public int NumRejected;
            public int NumReadings;
            public int NumPassed;
            public int NumFailed;
            public List<float> Readings = new List<float>();

            public bool Passed
            {
                get
                {
                    return NumPassed == NumReadings;
                }
            }

            public float PassRatio
            {
                get
                {
                    return (float)NumPassed / NumReadings;
                }
            }
        }

        void WaitForRejectedReadings(int numReadings)
        {
            var rejectedReadings = 0;
            for (; rejectedReadings < numReadings;)
            {
                if (DequeueFrequencyReading() < 0)
                {
                    ++rejectedReadings;
                }
            }
        }

        TestResult TestPoint(SoundOutput so, Sample sample, float frequency, int numReadings)
        {
            // Flush out the sound pipeline before starting the new test
            so.Sample = null;

            WaitForRejectedReadings(2);

            so.Sample = sample;
            so.DesiredFrequency = frequency;

            var result = new TestResult();
            result.Frequency = frequency;

            // Wait until our first non-rejected reading
            for (;;)
            {
                if (DequeueFrequencyReading() >= 0.0f)
                {
                    break;
                }
            }

            var maxVariation = 0.1f;
            var minFrequency = frequency * (1.0f / (1 + maxVariation));
            var maxFrequency = frequency * (1 + maxVariation);
            for (; result.NumReadings < numReadings; )
            {
                var f = DequeueFrequencyReading();
                result.Readings.Add(f);

                if (f >= 0.0f)
                {
                    ++result.NumReadings;
                    var valid = (f >= minFrequency) && (f <= maxFrequency);
                    if (valid)
                    {
                        ++result.NumPassed;
                    }
                    else
                    {
                        ++result.NumFailed;
                    }
                    Console.WriteLine("Expected: {0} got: {1} {2}", frequency, f, valid ? "" : "X");
                }
                else
                {
                    ++result.NumRejected;
                    Console.WriteLine("(Rejected)");
                }
            }

            //Console.WriteLine("{0} readings, {1} in tolerance", numReadings, inTolerance);
            return result;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_audioTask = Task.Run(() => FullTunerTest());
            //m_serialTask = Task.Run(() => DumpSerial());
            m_serialTask = Task.Run(() => ReadTunerFrequencyFromSerial());
        }

        private void ReadTunerFrequencyFromSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();
            
            // This loop is quite inefficient in terms of latency but it'll do fine for the purposes of this test harness
            for (; !m_closing;)
            {
                var buffer = "";
                while (spSerial.BytesToRead > 0)
                {
                    var c = (char)spSerial.ReadByte();
                    if (c != '\n')
                    {
                        buffer += c;
                    }
                    else
                    {
                        buffer = buffer.Trim();
                        float f;
                        if (float.TryParse(buffer, out f))
                        {
                            //m_tunerFrequency = f;
                            EnqueueFrequencyReading(f);
                            //Console.WriteLine("Tuner frequency: {0} (raw: {1})", f, buffer);
                        }
                    }
                }

                Thread.Sleep(5);
            }
            spSerial.Close();
        }

        private void DumpSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();
            for (; !m_closing ;)
            {
                while (spSerial.BytesToRead > 0)
                {
                    Console.Write("{0}", (char)spSerial.ReadByte());
                }
            }
            spSerial.Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_closing = true;
            while (!(m_audioTask.IsCompleted && m_serialTask.IsCompleted))
            {
                Thread.Sleep(50);
            }
        }
    }
}
