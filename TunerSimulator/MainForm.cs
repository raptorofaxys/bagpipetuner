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

        bool m_initializedTuner = false;

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
            public string SampleName;
            public float Frequency;
            public int NumRejectedReadings;
            public int NumValidReadings;

            public int NumPassed;
            public int NumFailed;
            public int NumWrongOctaveReadings;

            public List<TunerReading> Readings = new List<TunerReading>();

            public bool Passed
            {
                get
                {
                    return NumPassed == NumValidReadings;
                }
            }

            public float PassRatio
            {
                get
                {
                    return (float)NumPassed / NumValidReadings;
                }
            }

            public float RejectionRatio
            {
                get
                {
                    return (float)NumRejectedReadings / (NumValidReadings + NumRejectedReadings);
                }
            }

            public float WrongOctaveFailReasonRatio
            {
                get
                {
                    return (NumFailed > 0) ? (float)NumWrongOctaveReadings / NumFailed : 0.0f;
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

            cmbDumpMode.SelectedIndex = 1;
        }

        void InitializeTuner()
        {
            txtCorrelationDipPct.Text = 25.ToString();
            const float frequencyDeviation = 0.1f;

            const int bassDroneFreq = 119;
            tunerChannelControl1.SuspendChanges = true;
            tunerChannelControl1.MinFrequency.Value = (int)(bassDroneFreq * (1 - frequencyDeviation));
            tunerChannelControl1.MaxFrequency.Value = (int)(bassDroneFreq * (1 + frequencyDeviation));
            tunerChannelControl1.CorrelationDipPercent.Value = 25;
            tunerChannelControl1.GcfStep.Value = 2;
            tunerChannelControl1.BaseOffsetStep.Value = 4;
            tunerChannelControl1.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl1.SuspendChanges = false;

            const int tenorDroneFreq = 239;
            tunerChannelControl2.SuspendChanges = true;
            tunerChannelControl2.MinFrequency.Value = (int)(tenorDroneFreq * (1 - frequencyDeviation));
            tunerChannelControl2.MaxFrequency.Value = (int)(tenorDroneFreq * (1 + frequencyDeviation));
            tunerChannelControl2.CorrelationDipPercent.Value = 25;
            tunerChannelControl2.GcfStep.Value = 2;
            tunerChannelControl2.BaseOffsetStep.Value = 4;
            tunerChannelControl2.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl2.SuspendChanges = false;

            tunerChannelControl3.SuspendChanges = true;
            tunerChannelControl3.MinFrequency.Value = (int)(tenorDroneFreq * (1 - frequencyDeviation));
            tunerChannelControl3.MaxFrequency.Value = (int)(tenorDroneFreq * (1 + frequencyDeviation));
            tunerChannelControl3.CorrelationDipPercent.Value = 25;
            tunerChannelControl3.GcfStep.Value = 2;
            tunerChannelControl3.BaseOffsetStep.Value = 4;
            tunerChannelControl3.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl3.SuspendChanges = false;

            const int chanterMinFreq = 420;
            const int chanterMaxFreq = 943;
            tunerChannelControl4.SuspendChanges = true;
            tunerChannelControl4.MinFrequency.Value = (int)(chanterMinFreq * (1 - frequencyDeviation));
            tunerChannelControl4.MaxFrequency.Value = (int)(chanterMaxFreq * (1 + frequencyDeviation));
            tunerChannelControl4.CorrelationDipPercent.Value = 25;
            tunerChannelControl4.GcfStep.Value = 2;
            tunerChannelControl4.BaseOffsetStep.Value = 4;
            tunerChannelControl4.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl4.SuspendChanges = false;

            const bool fullRangeChannel0 = true;
            if (fullRangeChannel0)
            {
                tunerChannelControl1.SuspendChanges = true;
                tunerChannelControl1.MinFrequency.Value = 75;
                tunerChannelControl1.MaxFrequency.Value = 1100;
                tunerChannelControl1.CorrelationDipPercent.Value = 25;
                tunerChannelControl1.GcfStep.Value = 2;
                tunerChannelControl1.BaseOffsetStep.Value = 4;
                tunerChannelControl1.BaseOffsetStepIncrement.Value = 2;
                tunerChannelControl1.SuspendChanges = false;
            }
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
                var reading = m_tunerReadingQueue.Dequeue();
                //Console.WriteLine("(read: {0})", reading.SignalFrequency);
                return reading;
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
            lblReading.ForeColor = IsReadingValid(m_samplePlayback.Frequency, tr) ? Color.DarkGreen
                : ((tr.SignalFrequency >= 0.0f) ? Color.DarkRed
                : (tr.MaxSignalAmplitude - tr.MinSignalAmplitude > 20.0f) ? Color.Black
                : Color.DarkGray);

            m_spinnerIndex = (m_spinnerIndex + 1) % SPINNER.Length;
        }

        void RunTunerTest(int testsPerPoint = 16, int substepsPerSample = 5)
        {
            //var numTestsPerStep = 16;

            var results = new List<TestResult>();

            foreach (var s in m_samples)
            {
                Console.WriteLine("Testing {0}", s.Filename);

                //const int subSteps = 5;
                const float topFrequencyMultiplier = 1.1f;
                float frequencyStepMultiplier = (float)Math.Pow(topFrequencyMultiplier, 1.0f / ((substepsPerSample - 1) / 2.0f));
                var frequency = s.Frequency / ((float)Math.Pow(frequencyStepMultiplier, (substepsPerSample - 1) / 2.0f));

                for (var i = 0; i < substepsPerSample; ++i)
                {
                    var result = TestPoint(m_soundOutput, s, frequency, testsPerPoint);
                    results.Add(result);
                    Console.WriteLine("{0}: {1:0.00}% ({2})", frequency, result.PassRatio * 100.0f, string.Join(", ", result.Readings.Select(r => r.SignalFrequency)));

                    frequency *= frequencyStepMultiplier;
                }
            }

            Console.WriteLine("Frequency, Sample, PassRatio");

            foreach (var tr in results)
            {
                Console.WriteLine("{0}, {1}, {2}, {3}, {4}", tr.Frequency, tr.SampleName, tr.PassRatio, tr.RejectionRatio, tr.WrongOctaveFailReasonRatio);
            }

            Console.WriteLine("Total Pass Ratio: {0:00.00}%", results.Average(tr => tr.PassRatio) * 100.0f);
            Console.WriteLine("Total Rejection Ratio: {0:00.00}%", results.Average(tr => tr.RejectionRatio) * 100.0f);
            Console.WriteLine("Total Error Octave Error Ratio: {0:00.00}%", results.Where(tr => tr.NumFailed > 0).Average(tr => tr.WrongOctaveFailReasonRatio) * 100.0f);

            //BeginInvoke((Action)(() => { btnFullTest.Enabled = true; }));
            BeginInvoke((Action)(() => OnTestCompleted()));
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

        const float MAX_READING_ERROR_RATIO = 0.1f;

        bool IsReadingFrequencyValid(float targetFrequency, float readingFrequency)
        {
            var minFrequency = targetFrequency * (1.0f / (1 + MAX_READING_ERROR_RATIO));
            var maxFrequency = targetFrequency * (1 + MAX_READING_ERROR_RATIO);

            return (readingFrequency >= minFrequency) && (readingFrequency <= maxFrequency);
        }

        bool IsReadingValid(float targetFrequency, TunerReading tr)
        {
            return IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency)
                || ((targetFrequency >= tr.MinSignalFrequency) && (targetFrequency <= tr.MaxSignalFrequency));
        }

        bool IsReadingOctaveError(float targetFrequency, TunerReading tr)
        {
            return IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency * 2)
                || IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency * 3)
                || IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency * 4)
                || IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency * 5)
                || IsReadingFrequencyValid(targetFrequency, tr.SignalFrequency * 6);
        }

        TestResult TestPoint(SoundOutput so, Sample sample, float frequency, int numReadings)
        {
            // Flush out the sound pipeline before starting the new test
            so.Sample = null;

            WaitForRejectedReadings(1);

            so.Sample = sample;
            so.DesiredFrequency = frequency;

            var result = new TestResult();
            result.Frequency = frequency;
            result.SampleName = sample.Filename;

            var seenFirstValidReading = false;
            for (; result.NumValidReadings < numReadings;)
            {
                var tr = DequeueTunerReading();

                // Wait until our first non-rejected reading
                if (!seenFirstValidReading && (tr.SignalFrequency < 0.0f))
                {
                    //Console.WriteLine("(waiting until first valid reading to start sampling)");
                    continue;
                }

                seenFirstValidReading = true;

                result.Readings.Add(tr);

                if (tr.SignalFrequency >= 0.0f)
                {
                    ++result.NumValidReadings;
                    var valid = IsReadingValid(frequency, tr);
                    var octaveError = IsReadingOctaveError(frequency, tr);

                    if (valid)
                    {
                        ++result.NumPassed;
                    }
                    else
                    {
                        ++result.NumFailed;
                        if (octaveError)
                        {
                            ++result.NumWrongOctaveReadings;
                        }
                    }

                    Console.WriteLine("Expected: {0,7:F2} got: {1,7:F2}  {2,7:F2}  ({3} - {4})", frequency, tr.SignalFrequency, valid ? " " : (octaveError ? "O" : "X"), tr.MinSignalFrequency, tr.MaxSignalFrequency);
                }
                else
                {
                    ++result.NumRejectedReadings;
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
                    b.Tag = new SamplePlayback { Sample = s, Frequency = s.Frequency * (float)Math.Pow(offsetFrequencyScale, i) };
                    b.Click += SampleButtonClicked;
                    pnlSamples.Controls.Add(b);
                    x += b.Width;
                }
                y += 25;
            }

            m_soundOutput = new SoundOutput();
            m_soundOutput.Start();

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
            const bool serialEnabled = true;
            if (!serialEnabled)
            {
                BeginInvoke((Action)(() => { InitializeTuner(); }));
                return;
            }

            //var portNames = System.IO.Ports.SerialPort.GetPortNames();
            //foreach (var s in portNames)
            //{
            //    Console.WriteLine(s);
            //}

            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();

            // This loop is quite inefficient in terms of latency but it'll do fine for the purposes of this test harness
            var buffer = "";
            var commentBuffer = "";
            for (; !m_closing;)
            {
                while (spSerial.BytesToRead > 0)
                {
                    if (!m_initializedTuner)
                    {
                        BeginInvoke((Action)(() => { InitializeTuner(); }));
                        m_initializedTuner = true;
                    }

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

                Thread.Sleep(3);
            }
            spSerial.Close();
        }

        private void SerialSend(string toSend)
        {
            while (!string.IsNullOrEmpty(m_serialSendBuffer))
            {
                Thread.Sleep(2);
            }
            lock (m_serialSendBufferLock)
            {
                m_serialSendBuffer += toSend;
                Console.WriteLine(toSend);
            }
        }

        private void DumpSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();
            for (; !m_closing;)
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

        private void LaunchTest(Action test)
        {
            m_audioTask = Task.Run(test);
            btnFullTest.Enabled = false;
            btnQuickTest.Enabled = false;
        }

        private void OnTestCompleted()
        {
            btnFullTest.Enabled = true;
            btnQuickTest.Enabled = true;
        }

        private void btnFullTest_Click(object sender, EventArgs e)
        {
            LaunchTest(() => RunTunerTest());
        }

        private void btnQuickTest_Click(object sender, EventArgs e)
        {
            LaunchTest(() => RunTunerTest(4, 1));
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
            //int percent;
            //if (int.TryParse(txtCorrelationDipPct.Text, out percent))
            //{
            //    SerialSend(string.Format("d{0}", percent));
            //}
        }

        private void cmbDumpMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            SerialSend($"d{cmbDumpMode.SelectedIndex}");
        }

        private void tunerChannelControl_ConfigurationChanged(object sender, EventArgs e)
        {
            var ctl = (TunerChannelControl)sender;
            SerialSend($"c{ctl.ChannelIndex}");
            SerialSend($"m{ctl.MinFrequency.Value}");
            SerialSend($"M{ctl.MaxFrequency.Value}");
            SerialSend($"p{ctl.CorrelationDipPercent.Value}");
            SerialSend($"g{ctl.GcfStep.Value}");
            SerialSend($"o{ctl.BaseOffsetStep.Value}");
            SerialSend($"s{ctl.BaseOffsetStepIncrement.Value}");
            //SerialSend($"s{ctl.BaseOffsetStepIncrement}");
        }
    }
}
