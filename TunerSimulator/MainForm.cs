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

        void TestAudioAsync()
        {
            var chanter = new Sample("chanter.raw", 594);
            var tenor = new Sample("tenor drone.raw", 239);

            var samples = new Sample[] { chanter, tenor };

            var so = new SoundOutput();
            so.Start();

            var rand = new Random();

            //for (int i = 0; (i < 100) && !m_closing; ++i)
            //{
            //    Thread.Sleep(500);
            //    so.Sample = ((rand.Next() % 2) == 0) ? chanter : tenor;
            //    so.DesiredFrequency = 60 + (rand.Next() % 1000);
            //}

            var minFrequency = 60.0f;
            var maxFrequency = 1100.0f;
            var numSteps = 24.0;
            var mulStep = (float)Math.Pow(maxFrequency / minFrequency, 1.0f / (numSteps - 1));

            var results = new Dictionary<string, List<TestResult>>();

            foreach (var s in samples)
            {
                results[s.Filename] = new List<TestResult>();
                Console.WriteLine("Testing {0}", s.Filename);
                for (var frequency = minFrequency; frequency <= maxFrequency; frequency *= mulStep)
                {
                    var result = TestPoint(so, tenor, frequency);
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

        TestResult TestPoint(SoundOutput so, Sample sample, float frequency)
        {
            so.Sample = sample;
            so.DesiredFrequency = frequency;

            var result = new TestResult();
            result.Frequency = frequency;
            
            // Ignore the first few readings
            for (var i = 0; i < 2; ++i)
            {
                DequeueFrequencyReading();
            }

            var maxVariation = 0.1f;
            var minFrequency = frequency * (1.0f / (1 + maxVariation));
            var maxFrequency = frequency * (1 + maxVariation);
            for (; result.NumReadings < 24; )
            {
                var f = DequeueFrequencyReading();
                result.Readings.Add(f);

                //@HACK: change this to a >= 0.0f once the bug where the tuner outputs large negative numbers is figured out
                if (f != -1.0f)
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
            m_audioTask = Task.Run(() => TestAudioAsync());
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
