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
        //public delegate void ConfigurationChangedDelegate();
        public event EventHandler ConfigurationChanged;
        public int ChannelIndex { get; set; }
        public bool DetailedSearchEnabled
        {
            get
            {
                return chkDetailedSearch.Checked;
            }
            set
            {
                chkDetailedSearch.Checked = value;
            }
        }

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
                return $"CH{ChannelIndex + 1}";
            }
            //set
            //{
            //    lblChannelName.Text = value;
            //}
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
                evt(this, null);
            }
        }

        private void TunerChannelControl_Load(object sender, EventArgs e)
        {
            lblChannelName.Text = ChannelName;
        }
    }
}
