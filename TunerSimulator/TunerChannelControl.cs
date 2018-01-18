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
    public partial class TunerChannelControl : UserControl
    {
        public delegate void ConfigurationChangedDelegate();
        public event ConfigurationChangedDelegate ConfigurationChanged;

        private bool m_changesSuspended = false;
        public bool SuspendChanges
        {
            get
            {
                return m_changesSuspended;
            }
            set
            {
                m_changesSuspended = value;
                if (!m_changesSuspended)
                {
                    OnConfigurationChanged(null, null);
                }
            }
        }

        public string ChannelName
        {
            get
            {
                return lblChannelName.Text;
            }
            set
            {
                lblChannelName.Text = value;
            }
        }

        public TunerChannelControl()
        {
            InitializeComponent();
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            if (m_changesSuspended)
            {
                return;
            }

            var evt = ConfigurationChanged;
            if (evt != null)
            {
                evt();
            }
        }
    }
}
