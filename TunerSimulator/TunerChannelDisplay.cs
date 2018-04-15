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
    public partial class TunerChannelDisplay : UserControl
    {
        float m_instantFrequency;
        public float InstantFrequency
        {
            get
            {
                return m_instantFrequency;
            }
            set
            {
                m_instantFrequency = value;
                OnValueSet();
            }
        }

        float m_filteredFrequency;
        public float FilteredFrequency
        {
            get
            {
                return m_filteredFrequency;
            }
            set
            {
                m_filteredFrequency = value;
                OnValueSet();
            }
        }

        float m_centerFrequency;
        public float CenterFrequency
        {
            get
            {
                return m_centerFrequency;
            }
            set
            {
                m_centerFrequency = value;
                OnValueSet();
            }
        }

        float m_minFrequency;
        public float MinFrequency
        {
            get
            {
                return m_minFrequency;
            }
            set
            {
                m_minFrequency = value;
                OnValueSet();
            }
        }

        float m_maxFrequency;
        public float MaxFrequency
        {
            get
            {
                return m_maxFrequency;
            }
            set
            {
                m_maxFrequency = value;
                OnValueSet();
            }
        }

        public TunerChannelDisplay()
        {
            InitializeComponent();

            OnValueSet();
        }

        private void OnValueSet()
        {
            lblTemporaryDisplay.Text = string.Format("I: {0} F: {1} <: {2} C: {3} >: {4}", m_instantFrequency, m_filteredFrequency, m_minFrequency, m_centerFrequency, m_maxFrequency);
        }
    }
}
