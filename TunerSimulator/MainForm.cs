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
        SoundOutput m_soundOutput;
        SamplePlayback m_samplePlayback;

        object m_frequencyReadingQueueLock = new object();
        object m_serialSendBufferLock = new object();
        string m_serialSendBuffer;

        const string SPINNER = @"\|/-";
        int m_spinnerIndex;

        //bool m_runTest = true;

        struct TunerReading
        {
            public float SignalFrequency;
            public float MinSignalFrequency;
            public float MaxSignalFrequency;
            public float MinSignalAmplitude;
            public float MaxSignalAmplitude;
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

        struct SamplePlayback
        {
            public Sample Sample;
            public float Frequency;
        }

        public MainForm()
        {
            InitializeComponent();

            txtCorrelationDipPct.Text = 17.ToString();
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
            lblReading.Text = string.Format("Instant Frequency: {0:###0.00} ({1:###0.00} - {2:###0.00}) (expecting: {3}) [{4}, {5}] {6}",
                tr.SignalFrequency,
                tr.MinSignalFrequency,
                tr.MaxSignalFrequency,
                m_samplePlayback.Frequency,
                tr.MinSignalAmplitude,
                tr.MaxSignalAmplitude,
                SPINNER[m_spinnerIndex]);
            lblReading.ForeColor = IsReadingValid(m_samplePlayback.Frequency, tr) ? Color.DarkGreen : ((tr.SignalFrequency >= 0.0f) ? Color.DarkRed : Color.Black);

            m_spinnerIndex = (m_spinnerIndex + 1) % SPINNER.Length;
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
                    var result = TestPoint(m_soundOutput, s, frequency, numTestsPerStep);
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

        bool IsReadingValid(float frequency, TunerReading tr)
        {
            var maxVariation = 0.1f;
            var minFrequency = frequency * (1.0f / (1 + maxVariation));
            var maxFrequency = frequency * (1 + maxVariation);

            return ((tr.SignalFrequency >= minFrequency) && (tr.SignalFrequency <= maxFrequency))
                || ((frequency >= tr.MinSignalFrequency) && (frequency <= tr.MaxSignalFrequency));
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

            for (; result.NumReadings < numReadings; )
            {
                var tr = DequeueTunerReading();
                result.Readings.Add(tr);

                if (tr.SignalFrequency >= 0.0f)
                {
                    ++result.NumReadings;
                    var valid = IsReadingValid(frequency, tr);

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

            var offsetFrequencyScale = (float)Math.Pow(2.0f, 1.0f / 12.0f);
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
                    b.Tag = new SamplePlayback { Sample = s, Frequency = s.Frequency * (float)Math.Pow(offsetFrequencyScale, i)};
                    b.Click += SampleButtonClicked;
                    pnlSamples.Controls.Add(b);
                    x += b.Width;
                }
                y += 25;
            }

            m_soundOutput = new SoundOutput();
            m_soundOutput.Start();

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

            m_soundOutput.Stop();
        }

        private void ReadTunerFrequencyFromSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();

            // This loop is quite inefficient in terms of latency but it'll do fine for the purposes of this test harness
            var buffer = "";
            var commentBuffer = "";
            for (; !m_closing;)
            {
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
                        //Console.WriteLine("Buf: {0}", buffer);

                        if (!buffer.StartsWith("#"))
                        {
                            // Flush out any comment
                            //var o = string.Format("Buf: {0}", commentBuffer);
                            Console.Write(commentBuffer);
                            //Console.WriteLine(commentBuffer);
                            //Console.Out.Flush();
                            if (!string.IsNullOrEmpty(commentBuffer))
                            {
                                var bufferCopy = string.Copy(commentBuffer);
                                BeginInvoke((Action)(() => Clipboard.SetText(bufferCopy)));
                            }
                            commentBuffer = "";

                            var reading = new TunerReading();
                            var values = buffer.Split(',');
                            if (values.Length == 5)
                            {
                                float.TryParse(values[0], out reading.SignalFrequency);
                                float.TryParse(values[1], out reading.MinSignalFrequency);
                                float.TryParse(values[2], out reading.MaxSignalFrequency);
                                float.TryParse(values[3], out reading.MinSignalAmplitude);
                                float.TryParse(values[4], out reading.MaxSignalAmplitude);
                                EnqueueTunerReading(reading);
                                BeginInvoke((Action)(() => OnTunerReading(reading)));
                                //Console.WriteLine("Tuner frequency: {0} (raw: {1})", reading.SignalFrequency, buffer);
                            }
                        }
                        else
                        {
                            commentBuffer += buffer.Substring(1) + Console.Out.NewLine;
                        }

                        buffer = "";
                    }
                }

                var sendBuffer = "";
                lock (m_serialSendBufferLock)
                {
                    if (!string.IsNullOrEmpty(m_serialSendBuffer))
                    {
                        sendBuffer = string.Copy(m_serialSendBuffer);
                        m_serialSendBuffer = "";
                    }
                }

                if (!string.IsNullOrEmpty(sendBuffer))
                {
                    spSerial.Write(sendBuffer);
                }

                Thread.Sleep(5);
            }
            spSerial.Close();
        }

        private void SerialSend(string toSend)
        {
            lock (m_serialSendBufferLock)
            {
                m_serialSendBuffer += toSend;
            }
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

        private void UpdateAutoDumpFrequency()
        {
            if (chkDumpOnOctaveError.Checked)
            {
                txtMinDumpFrequency.Text = (m_soundOutput.DesiredFrequency * (5.0f / 8.0f)).ToString();
            }
        }

        private void SampleButtonClicked(object sender, EventArgs e)
        {
            var sp = (SamplePlayback)((Button)sender).Tag;
            m_soundOutput.Sample = sp.Sample;
            m_soundOutput.DesiredFrequency = sp.Frequency;

            m_samplePlayback = sp;

            UpdateAutoDumpFrequency();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //@TODO
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_soundOutput.Sample = null;
            chkDumpOnNull.Checked = false;
            chkDumpOnOctaveError.Checked = false;
            txtMinDumpFrequency.Text = "-1";
        }

        private void chkDumpOnNull_CheckedChanged(object sender, EventArgs e)
        {
            SerialSend(chkDumpOnNull.Checked ? "I" : "i");
        }

        private void txtMinDumpFrequency_TextChanged(object sender, EventArgs e)
        {
            float frequency;
            if (float.TryParse(txtMinDumpFrequency.Text, out frequency))
            {
                SerialSend(string.Format("f{0}", frequency));
            }
        }

        private void chkDumpOnOctaveError_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAutoDumpFrequency();
        }

        private void txtCorrelationDipPct_TextChanged(object sender, EventArgs e)
        {
            int percent;
            if (int.TryParse(txtCorrelationDipPct.Text, out percent))
            {
                SerialSend(string.Format("d{0}", percent));
            }
        }
    }
}
