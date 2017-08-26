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

        void TestAudioAsync()
        {
            var chanter = new Sample("chanter.raw", 594);
            var tenor = new Sample("tenor drone.raw", 239);
            var so = new SoundOutput();
            so.Sample = tenor;
            so.DesiredFrequency = 220;

            so.Start();

            var rand = new Random();

            for (int i = 0; (i < 100) && !m_closing; ++i)
            {
                Thread.Sleep(500);
                so.Sample = ((rand.Next() % 2) == 0) ? chanter : tenor;
                so.DesiredFrequency = 60 + (rand.Next() % 1000);
            }
            so.Stop();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_audioTask = Task.Run(() => TestAudioAsync());
            m_serialTask = Task.Run(() => DumpSerial());
        }

        private void DumpSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();
            //while (spSerial.BytesToRead == 0) { }
            for (; !m_closing ;)
            {
                while (spSerial.BytesToRead > 0)
                {
                    Console.Write("{0}", (char)spSerial.ReadByte());
                }
                //byte[] bytes = new byte[193];
                //spSerial.Write(bytes, 0, bytes.Length);
            }
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
