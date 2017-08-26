using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        void TestAudioAsync()
        {
            var chanter = new SoundOutput.Sample("chanter.raw", 594);
            var tenor = new SoundOutput.Sample("tenor drone.raw", 239);
            SoundOutput.TestAudio(tenor);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Task.Run(() => TestAudioAsync());
            Task.Run(() => DumpSerial());
        }

        private void DumpSerial()
        {
            spSerial.Handshake = System.IO.Ports.Handshake.None;
            spSerial.Open();
            //while (spSerial.BytesToRead == 0) { }
            for (;;)
            {
                while (spSerial.BytesToRead > 0)
                {
                    Console.Write("{0}", (char)spSerial.ReadByte());
                }
                //byte[] bytes = new byte[193];
                //spSerial.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
