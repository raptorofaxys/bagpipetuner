using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TunerSimulator
{
    public partial class TunerChannelControl: UserControl
    {
        public delegate void ConfigurationChangedDelegate();
        public event ConfigurationChangedDelegate ConfigurationChanged;

        public TunerChannelControl()
        {
            InitializeComponent();
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            var evt = ConfigurationChanged;
            if (evt != null)
            {
                evt();
            }
        }
    }
}
