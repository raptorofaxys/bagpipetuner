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
        bool m_closing = false;
        Task m_audioTask;
        Task m_serialTask;

        Sample[] m_samples;
        SoundOutput m_so;

        object m_frequencyReadingQueueLock = new object();

        //bool m_runTest = true;

        struct TunerReading
        {
            public float SignalFrequency;
            public float MinSignalFrequency;
            public float MaxSignalFrequency;
        }

        Queue<TunerReading> m_tunerReadingQueue = new Queue<TunerReading>();

        class TestResult
        {
            public float Frequency;
            public int NumRejected;
            public int NumReadings;
            public int NumPassed;
            public int NumFailed;
            public List<TunerReading> Readings = new List<TunerReading>();

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

        public MainForm()
        {
            InitializeComponent();
        }

        void EnqueueTunerReading(TunerReading tr)
        {
            lock (m_frequencyReadingQueueLock)
            {
                m_tunerReadingQueue.Enqueue(tr);
            }
        }

        TunerReading DequeueTunerReading()
        {
            while (m_tunerReadingQueue.Count == 0)
            {
                Thread.Sleep(10);
            }
            lock (m_frequencyReadingQueueLock)
            {
                return m_tunerReadingQueue.Dequeue();
            }
        }

        void OnTunerReading(TunerReading tr)
        {
            lblReading.Text = string.Format("Instant Frequency: {0:###0.00} ({1:###0.00} - {2:###0.00})", tr.SignalFrequency, tr.MinSignalFrequency, tr.MaxSignalFrequency);
        }

        void FullTunerTest()
        {
            //var chanter = new Sample("chanter 594.raw", 594);
            //var tenor = new Sample("tenor drone 239.raw", 239);

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
            var numSteps = 12.0;
            var numTestsPerStep = 4;
            var mulStep = (float)Math.Pow(maxFrequency / minFrequency, 1.0f / (numSteps - 1));

            var results = new Dictionary<string, List<TestResult>>();

            foreach (var s in m_samples)
            {
                results[s.Filename] = new List<TestResult>();
                Console.WriteLine("Testing {0}", s.Filename);
                //for (var frequency = minFrequency; frequency <= maxFrequency; frequency *= mulStep)
                {
                    var frequency = s.Frequency;
                    var result = TestPoint(m_so, s, frequency, numTestsPerStep);
                    results[s.Filename].Add(result);
                    Console.WriteLine("{0}: {1:0.00}% ({2})", frequency, result.PassRatio * 100.0f, string.Join(", ", result.Readings.Select(r => r.SignalFrequency)));
                }
            }

            Console.WriteLine("Frequency, {0}", string.Join(", ", m_samples.Select(s => s.Filename)));
            var anyListOfResults = results[m_samples[0].Filename];
            for (var i = 0; i < anyListOfResults.Count; ++i)
            {
                Console.WriteLine("{0}, {1}", anyListOfResults[i].Frequency, string.Join(", ", results.Select(kv => kv.Value[i].PassRatio)));
            }
        }

        void WaitForRejectedReadings(int numReadings)
        {
            var rejectedReadings = 0;
            for (; rejectedReadings < numReadings;)
            {
                if (DequeueTunerReading().SignalFrequency < 0)
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
                if (DequeueTunerReading().SignalFrequency >= 0.0f)
                {
                    break;
                }
            }

            var maxVariation = 0.1f;
            var minFrequency = frequency * (1.0f / (1 + maxVariation));
            var maxFrequency = frequency * (1 + maxVariation);
            for (; result.NumReadings < numReadings; )
            {
                var tr = DequeueTunerReading();
                result.Readings.Add(tr);

                if (tr.SignalFrequency >= 0.0f)
                {
                    ++result.NumReadings;
                    var valid = ((tr.SignalFrequency >= minFrequency) && (tr.SignalFrequency <= maxFrequency))
                        || ((frequency >= tr.MinSignalFrequency) && (frequency <= tr.MaxSignalFrequency));

                    if (valid)
                    {
                        ++result.NumPassed;
                    }
                    else
                    {
                        ++result.NumFailed;
                    }

                    Console.WriteLine("Expected: {0} got: {1} {2} ({3} - {4})", frequency, tr.SignalFrequency, valid ? "" : "X", tr.MinSignalFrequency, tr.MaxSignalFrequency);
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
            m_samples = new Sample[]
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

            var numOffsetsPerSide = 3;

            var y = 0;
            foreach (var s in m_samples)
            {
                var l = new Label();
                l.Text = s.Filename;
                l.Top = y;
                l.Width = 130;
                pnlSamples.Controls.Add(l);

                var x = l.Width;

                for (int i = -numOffsetsPerSide; i <= numOffsetsPerSide; ++i)
                {
                    var b = new Button();
                    b.Left = x;
                    b.Top = y;
                    b.Width = 50;
                    b.Text = i.ToString();
                    b.Tag = Tuple.Create(s, i);
                    b.Click += SampleButtonClicked;
                    pnlSamples.Controls.Add(b);
                    x += b.Width;
                }
                y += 25;
            }

            m_so = new SoundOutput();
            m_so.Start();

            //m_audioTask = Task.Run(() => FullTunerTest());
            //m_serialTask = Task.Run(() => DumpSerial());
            m_serialTask = Task.Run(() => ReadTunerFrequencyFromSerial());
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_closing = true;
            while (!(((m_audioTask == null) || m_audioTask.IsCompleted) && m_serialTask.IsCompleted))
            {
                Thread.Sleep(50);
            }

            m_so.Stop();
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
                        var reading = new TunerReading();
                        var values = buffer.Split(',');
                        if (values.Length == 3)
                        {
                            float.TryParse(values[0], out reading.SignalFrequency);
                            float.TryParse(values[1], out reading.MinSignalFrequency);
                            float.TryParse(values[2], out reading.MaxSignalFrequency);
                            EnqueueTunerReading(reading);
                            BeginInvoke((Action)(() => OnTunerReading(reading)));
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

        private void SampleButtonClicked(object sender, EventArgs e)
        {
            var t = (Tuple<Sample, int>)((Button)sender).Tag;
            var sample = t.Item1;
            var offset = t.Item2;
            m_so.Sample = sample;
            m_so.DesiredFrequency = sample.Frequency * (float)Math.Pow(1.1f, offset);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //@TODO
        }
    }
}
