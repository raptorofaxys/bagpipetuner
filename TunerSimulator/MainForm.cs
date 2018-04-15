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
        bool m_serialEnabled = true;

        bool m_closing = false;
        Task m_audioTask;
        Task m_serialTask;

        Sample[] m_samples;
        SoundOutput m_soundOutput;
        SamplePlayback m_samplePlayback;

        SoundOutput[] m_bagpipeOutputs;
        Sample[] m_chanterNotes;

        object m_frequencyReadingQueueLock = new object();
        object m_serialSendBufferLock = new object();
        Queue<String> m_serialSendBuffer = new Queue<string>();

        const string SPINNER = @"\|/-";
        int m_spinnerIndex;

        TunerChannelDisplay[] m_channelDisplays;

        int m_serialSendCounter;

        string m_commentBuffer;

        //bool m_runTest = true;

        bool m_initializedTuner = false;

        struct TunerReading
        {
            public int ChannelIndex;
            public float InstantFrequency;
            public float MinSignalFrequency;
            public float MaxSignalFrequency;
            public float MinSignalAmplitude;
            public float MaxSignalAmplitude;
            public long TotalMs;
            public int MaxCorrelationDipPercent;
            public float FilteredFrequency;
            public float CenterDisplayFrequency;
            public float MinDisplayFrequency;
            public float MaxDisplayFrequency;
            public int MidiNoteIndex;
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

            m_channelDisplays = new TunerChannelDisplay[] { tunerChannelDisplay1, tunerChannelDisplay2, tunerChannelDisplay3, tunerChannelDisplay4 };

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
            tunerChannelControl1.DetailedSearchEnabled = true;
            tunerChannelControl1.SuspendChanges = false;

            const int tenorDroneFreq = 239;
            tunerChannelControl2.SuspendChanges = true;
            tunerChannelControl2.MinFrequency.Value = (int)(tenorDroneFreq * (1 - frequencyDeviation));
            tunerChannelControl2.MaxFrequency.Value = (int)(tenorDroneFreq * (1 + frequencyDeviation));
            tunerChannelControl2.CorrelationDipPercent.Value = 25;
            tunerChannelControl2.GcfStep.Value = 2;
            tunerChannelControl2.BaseOffsetStep.Value = 4;
            tunerChannelControl2.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl1.DetailedSearchEnabled = true;
            tunerChannelControl2.SuspendChanges = false;

            tunerChannelControl3.SuspendChanges = true;
            tunerChannelControl3.MinFrequency.Value = (int)(tenorDroneFreq * (1 - frequencyDeviation));
            tunerChannelControl3.MaxFrequency.Value = (int)(tenorDroneFreq * (1 + frequencyDeviation));
            tunerChannelControl3.CorrelationDipPercent.Value = 25;
            tunerChannelControl3.GcfStep.Value = 2;
            tunerChannelControl3.BaseOffsetStep.Value = 4;
            tunerChannelControl3.BaseOffsetStepIncrement.Value = 2;
            tunerChannelControl1.DetailedSearchEnabled = true;
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
            tunerChannelControl1.DetailedSearchEnabled = true;
            tunerChannelControl4.SuspendChanges = false;

            const bool fullRangeChannel0 = true;
            const bool fullRangeAllChannels = true;
            if (fullRangeChannel0 || fullRangeAllChannels)
            {
                tunerChannelControl1.SuspendChanges = true;
                tunerChannelControl1.MinFrequency.Value = 75;
                tunerChannelControl1.MaxFrequency.Value = 1100;
                tunerChannelControl1.CorrelationDipPercent.Value = 25;
                tunerChannelControl1.GcfStep.Value = 8;
                tunerChannelControl1.BaseOffsetStep.Value = 4;
                tunerChannelControl1.BaseOffsetStepIncrement.Value = 2;
                tunerChannelControl1.DetailedSearchEnabled = true;
                tunerChannelControl1.SuspendChanges = false;
            }

            if (fullRangeAllChannels)
            {
                tunerChannelControl2.SuspendChanges = true;
                tunerChannelControl2.MinFrequency.Value = tunerChannelControl1.MinFrequency.Value;
                tunerChannelControl2.MaxFrequency.Value = tunerChannelControl1.MaxFrequency.Value;
                tunerChannelControl2.CorrelationDipPercent.Value = tunerChannelControl1.CorrelationDipPercent.Value;
                tunerChannelControl2.GcfStep.Value = tunerChannelControl1.GcfStep.Value;
                tunerChannelControl2.BaseOffsetStep.Value = tunerChannelControl1.BaseOffsetStep.Value;
                tunerChannelControl2.BaseOffsetStepIncrement.Value = tunerChannelControl1.BaseOffsetStepIncrement.Value;
                tunerChannelControl2.DetailedSearchEnabled = tunerChannelControl1.DetailedSearchEnabled;
                tunerChannelControl2.SuspendChanges = false;

                tunerChannelControl3.SuspendChanges = true;
                tunerChannelControl3.MinFrequency.Value = tunerChannelControl1.MinFrequency.Value;
                tunerChannelControl3.MaxFrequency.Value = tunerChannelControl1.MaxFrequency.Value;
                tunerChannelControl3.CorrelationDipPercent.Value = tunerChannelControl1.CorrelationDipPercent.Value;
                tunerChannelControl3.GcfStep.Value = tunerChannelControl1.GcfStep.Value;
                tunerChannelControl3.BaseOffsetStep.Value = tunerChannelControl1.BaseOffsetStep.Value;
                tunerChannelControl3.BaseOffsetStepIncrement.Value = tunerChannelControl1.BaseOffsetStepIncrement.Value;
                tunerChannelControl3.DetailedSearchEnabled = tunerChannelControl1.DetailedSearchEnabled;
                tunerChannelControl3.SuspendChanges = false;

                tunerChannelControl4.SuspendChanges = true;
                tunerChannelControl4.MinFrequency.Value = tunerChannelControl1.MinFrequency.Value;
                tunerChannelControl4.MaxFrequency.Value = tunerChannelControl1.MaxFrequency.Value;
                tunerChannelControl4.CorrelationDipPercent.Value = tunerChannelControl1.CorrelationDipPercent.Value;
                tunerChannelControl4.GcfStep.Value = tunerChannelControl1.GcfStep.Value;
                tunerChannelControl4.BaseOffsetStep.Value = tunerChannelControl1.BaseOffsetStep.Value;
                tunerChannelControl4.BaseOffsetStepIncrement.Value = tunerChannelControl1.BaseOffsetStepIncrement.Value;
                tunerChannelControl4.DetailedSearchEnabled = tunerChannelControl1.DetailedSearchEnabled;
                tunerChannelControl4.SuspendChanges = false;
            }

            if (!m_serialEnabled)
            {
                lblReading.Text = "Serial transfer disabled";
            }
        }

        void EnqueueTunerReading(TunerReading tr)
        {
            lock (m_frequencyReadingQueueLock)
            {
                m_tunerReadingQueue.Enqueue(tr);
            }
        }

        bool IsTunerReadingAvailable()
        {
            return m_tunerReadingQueue.Count > 0;
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
            var sb = new StringBuilder();
            sb.AppendFormat("C{0}: ", tr.ChannelIndex);
            sb.AppendFormat("Instant Frequency: {0:###0.00} ({1:###0.00} - {2:###0.00})",
                tr.InstantFrequency,
                tr.MinSignalFrequency,
                tr.MaxSignalFrequency);
            sb.AppendFormat("(expecting: {0}) [{1}, {2}]",
                m_soundOutput.Sample != null ? m_soundOutput.DesiredFrequency.ToString() : "-",
                tr.MinSignalAmplitude,
                tr.MaxSignalAmplitude);
            sb.Append(SPINNER[m_spinnerIndex]);
            lblReading.Text = sb.ToString();

            lblReading.ForeColor = IsReadingValid(m_samplePlayback.Frequency, tr) ? Color.DarkGreen
                : ((tr.InstantFrequency >= 0.0f) ? Color.DarkRed
                : (tr.MaxSignalAmplitude - tr.MinSignalAmplitude > 20.0f) ? Color.Black
                : Color.DarkGray);

            lblMisc.Text = string.Format("{0} ms, {1} max CDP", tr.TotalMs, tr.MaxCorrelationDipPercent);

            m_spinnerIndex = (m_spinnerIndex + 1) % SPINNER.Length;

            if ((tr.ChannelIndex >= 0) && (tr.ChannelIndex < m_channelDisplays.Length))
            {
                m_channelDisplays[tr.ChannelIndex].SetDisplayValues(tr.InstantFrequency, tr.FilteredFrequency, tr.CenterDisplayFrequency, tr.MinDisplayFrequency, tr.MaxDisplayFrequency, tr.MidiNoteIndex);
            }
        }

        void RunTunerTest(int testsPerPoint = 16, int substepsPerSample = 5)
        {
            //var numTestsPerStep = 16;
            while (IsTunerReadingAvailable())
            {
                DequeueTunerReading();
            }
            WaitForRejectedReadings(2);

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
                    Console.WriteLine("{0}: {1:0.00}% ({2})", frequency, result.PassRatio * 100.0f, string.Join(", ", result.Readings.Select(r => r.InstantFrequency)));

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
            var resultsWithFailures = results.Where(tr => tr.NumFailed > 0);
            if (resultsWithFailures.Any())
            {
                Console.WriteLine("Total Error Octave Error Ratio: {0:00.00}%", resultsWithFailures.Average(tr => tr.WrongOctaveFailReasonRatio) * 100.0f);
            }

            //BeginInvoke((Action)(() => { btnFullTest.Enabled = true; }));
            BeginInvoke((Action)(() => OnTestCompleted()));
        }

        void WaitForRejectedReadings(int numReadings)
        {
            var rejectedReadings = 0;
            for (; rejectedReadings < numReadings;)
            {
                if (DequeueTunerReading().InstantFrequency < 0)
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
            return IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency)
                || ((targetFrequency >= tr.MinSignalFrequency) && (targetFrequency <= tr.MaxSignalFrequency));
        }

        bool IsReadingOctaveError(float targetFrequency, TunerReading tr)
        {
            return IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency * 2)
                || IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency * 3)
                || IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency * 4)
                || IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency * 5)
                || IsReadingFrequencyValid(targetFrequency, tr.InstantFrequency * 6);
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
                if (!seenFirstValidReading && (tr.InstantFrequency < 0.0f))
                {
                    //Console.WriteLine("(waiting until first valid reading to start sampling)");
                    continue;
                }

                seenFirstValidReading = true;

                result.Readings.Add(tr);

                if (tr.InstantFrequency >= 0.0f)
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

                    Console.WriteLine("Expected: {0,7:F2} got: {1,7:F2}  {2,7:F2}  ({3} - {4})", frequency, tr.InstantFrequency, valid ? " " : (octaveError ? "O" : "X"), tr.MinSignalFrequency, tr.MaxSignalFrequency);
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

        private void StartDrones()
        {
            m_bagpipeOutputs = new SoundOutput[4];

            m_bagpipeOutputs[0] = new SoundOutput();
            m_bagpipeOutputs[0].Sample = m_samples[0];
            m_bagpipeOutputs[0].DesiredFrequency = m_samples[0].Frequency;
            m_bagpipeOutputs[0].RandomizePosition();
            m_bagpipeOutputs[0].Volume = 0.7f;
            m_bagpipeOutputs[0].Start();

            m_bagpipeOutputs[1] = new SoundOutput();
            m_bagpipeOutputs[1].Sample = m_samples[1];
            m_bagpipeOutputs[1].DesiredFrequency = m_samples[1].Frequency;
            m_bagpipeOutputs[1].RandomizePosition();
            m_bagpipeOutputs[1].Volume = 0.7f;
            m_bagpipeOutputs[1].Start();

            m_bagpipeOutputs[2] = new SoundOutput();
            m_bagpipeOutputs[2].Sample = m_samples[1];
            m_bagpipeOutputs[2].DesiredFrequency = m_samples[1].Frequency + 1;
            m_bagpipeOutputs[2].RandomizePosition();
            m_bagpipeOutputs[2].Volume = 0.7f;
            m_bagpipeOutputs[2].Start();

            m_bagpipeOutputs[3] = new SoundOutput();
            m_bagpipeOutputs[3].Sample = m_chanterNotes[5];
            m_bagpipeOutputs[3].DesiredFrequency = m_chanterNotes[5].Frequency;
            m_bagpipeOutputs[3].RandomizePosition();
            m_bagpipeOutputs[3].Start();
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

            m_chanterNotes = new Sample[]
            {
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

            //StartDrones();

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
            if (!m_serialEnabled)
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
                        //Console.WriteLine("<={0}", buffer);

                        ProcessSerialInput(buffer);

                        buffer = "";
                    }
                }

                Thread.Sleep(3);
            }
            spSerial.Close();
        }

        private void ProcessSerialInput(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            Console.WriteLine("(raw: \"{0}\")", line);

            char lineType = line[0];
            line = line.Substring(1);

            if (lineType != '#')
            {
                // If we got any type of line that is not a comment, flush out the comment buffer
                if (!string.IsNullOrEmpty(m_commentBuffer))
                {
                    Console.Write(string.Concat(m_commentBuffer.Trim().Split('\n').Select(l => string.Format("#{0}\n", l))));
                    //Console.WriteLine(string.Format("\"{0}\"", commentBuffer));
                    //Console.Out.Flush();

                    var bufferCopy = string.Copy(m_commentBuffer);
                    BeginInvoke((Action)(() => Clipboard.SetText(bufferCopy)));
                }
                m_commentBuffer = "";
            }

            switch (lineType)
            {
                case '#': m_commentBuffer += line + Console.Out.NewLine; break;
                case 'r':
                    {
                        var reading = new TunerReading();
                        var values = line.Split(',');
                        if (values.Length == 13)
                        {
                            int.TryParse(values[0], out reading.ChannelIndex);
                            float.TryParse(values[1], out reading.InstantFrequency);
                            float.TryParse(values[2], out reading.MinSignalFrequency);
                            float.TryParse(values[3], out reading.MaxSignalFrequency);
                            float.TryParse(values[4], out reading.MinSignalAmplitude);
                            float.TryParse(values[5], out reading.MaxSignalAmplitude);
                            long.TryParse(values[6], out reading.TotalMs);
                            int.TryParse(values[7], out reading.MaxCorrelationDipPercent);
                            float.TryParse(values[8], out reading.FilteredFrequency);
                            float.TryParse(values[9], out reading.CenterDisplayFrequency);
                            float.TryParse(values[10], out reading.MinDisplayFrequency);
                            float.TryParse(values[11], out reading.MaxDisplayFrequency);
                            int.TryParse(values[12], out reading.MidiNoteIndex);
                            EnqueueTunerReading(reading);
                            BeginInvoke((Action)(() => OnTunerReading(reading)));
                            //Console.WriteLine("Tuner frequency: {0} (raw: {1})", reading.SignalFrequency, line);
                        }
                    }
                    break;
                case 'e':
                    {
                        // Read back an echo
                        Console.WriteLine("Echo: {0}", line);
                    }
                    break;
                case 'c':
                    {
                        // Tuner is ready to accept a command; send one
                        SendNextPendingSerialLine();
                    }
                    break;
            }
        }

        private void SendNextPendingSerialLine()
        {
            if (m_serialSendBuffer.Count > 0)
            {
                var sendBuffer = "";
                lock (m_serialSendBufferLock)
                {
                    sendBuffer = m_serialSendBuffer.Dequeue();
                }

                if (!string.IsNullOrEmpty(sendBuffer))
                {
                    spSerial.Write(sendBuffer);
                    Console.WriteLine("=>{0}", sendBuffer.Trim());
                }
            }
        }

        private void SendSerialLine(string toSend)
        {
            if (!m_serialEnabled)
            {
                return;
            }

            lock (m_serialSendBufferLock)
            {
                m_serialSendBuffer.Enqueue(toSend + "\n");

                ++m_serialSendCounter;
                //m_serialSendBuffer.Enqueue(string.Format("e{0}\n", m_serialSendCounter));

                //Console.WriteLine("({0})", toSend);
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
            SendSerialLine(chkDumpOnNull.Checked ? "I" : "i");
        }

        private void txtMinDumpFrequency_TextChanged(object sender, EventArgs e)
        {
            float frequency;
            if (float.TryParse(txtMinDumpFrequency.Text, out frequency))
            {
                SendSerialLine(string.Format("f{0}", frequency));
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
            SendSerialLine($"d{cmbDumpMode.SelectedIndex}");
        }

        private void tunerChannelControl_ConfigurationChanged(object sender, EventArgs e)
        {
            var ctl = (TunerChannelControl)sender;
            SendSerialLine($"l1");
            SendSerialLine($"c{ctl.ChannelIndex}");
            SendSerialLine($"m{ctl.MinFrequency.Value}");
            SendSerialLine($"M{ctl.MaxFrequency.Value}");
            SendSerialLine($"p{ctl.CorrelationDipPercent.Value}");
            SendSerialLine($"g{ctl.GcfStep.Value}");
            SendSerialLine($"o{ctl.BaseOffsetStep.Value}");
            SendSerialLine($"s{ctl.BaseOffsetStepIncrement.Value}");
            SendSerialLine($"x{(ctl.DetailedSearchEnabled ? 1 : 0)}");
            SendSerialLine($"l0");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_bagpipeOutputs == null)
            {
                return;
            }

            var noteIndex = 0;
            switch (e.KeyCode)
            {
                case Keys.Q: noteIndex = 0; break;
                case Keys.W: noteIndex = 1; break;
                case Keys.E: noteIndex = 2; break;
                case Keys.R: noteIndex = 3; break;
                case Keys.T: noteIndex = 4; break;
                case Keys.Y: noteIndex = 5; break;
                case Keys.U: noteIndex = 6; break;
                case Keys.I: noteIndex = 7; break;
                case Keys.O: noteIndex = 8; break;
            }

            m_bagpipeOutputs[3].Sample = m_chanterNotes[noteIndex];
            m_bagpipeOutputs[3].DesiredFrequency = m_chanterNotes[noteIndex].Frequency;
        }
    }
}
