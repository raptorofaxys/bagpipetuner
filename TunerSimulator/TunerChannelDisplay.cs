using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;

namespace TunerSimulator
{
    public partial class TunerChannelDisplay : UserControl
    {
        private bool RealDesignMode
        {
            get
            {
                // from http://geekswithblogs.net/mrnat/archive/2005/04/27/38594.aspx
                return (this.GetService(typeof(IDesignerHost)) != null)
                    || (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime);
            }
        }

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

        public void SetFrequencies(float instantFrequency, float filteredFrequency, float centerFrequency, float minFrequency, float maxFrequency)
        {
            m_instantFrequency = instantFrequency;
            m_filteredFrequency = filteredFrequency;
            m_centerFrequency = centerFrequency;
            m_minFrequency = minFrequency;
            m_maxFrequency = maxFrequency;
            OnValueSet();
        }

        private void RenderTick(Graphics gfx, float frequency, Color color)
        {
            //var midRange = Math.Log(m_centerFrequency) / Math.Log(m_minFrequency);
            if ((m_maxFrequency > 0.0f) && (m_minFrequency > 0.0f) && (frequency > 0.0f))
            {
                var rangeLog = Math.Log(m_maxFrequency) / Math.Log(m_minFrequency);
                var frequencyLog = Math.Log(frequency) / Math.Log(m_minFrequency);
                var t = (frequencyLog - 1) / (rangeLog - 1);
                using (var b = new SolidBrush(color))
                {
                    using (var p = new Pen(b))
                    {
                        var x = (int)(t * pnlGraph.ClientRectangle.Width);
                        gfx.DrawLine(p, x, 0, x, pnlGraph.ClientRectangle.Height);
                    }
                }
            }
        }

        private void DoPaint()
        {
            Bitmap backBuffer = new Bitmap(pnlGraph.ClientRectangle.Width, pnlGraph.ClientRectangle.Height);
            Graphics backGfx = Graphics.FromImage(backBuffer);
            backGfx.Clear(Color.Black);
            //backGfx.SmoothingMode = SmoothingMode.HighQuality;
            //backGfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
            backGfx.SmoothingMode = SmoothingMode.HighSpeed;
            backGfx.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            if (pnlGraph.ClientRectangle.Width > 0)
            {
                using (var b = new SolidBrush(Color.Black))
                {
                    using (var p = new Pen(b))
                    {
                        for (var i = 0; i < pnlGraph.ClientRectangle.Width; ++i)
                        {
                            var t = ((float)Math.Abs(i - pnlGraph.ClientRectangle.Width / 2)) / (pnlGraph.ClientRectangle.Width / 2);
                            var c = Color.FromArgb((int)(t * 255), (int)((1.0 - t) * 255), 0);
                            b.Color = c;
                            const int gap = 4;
                            p.Brush = b;
                            backGfx.DrawLine(p, i, gap, i, pnlGraph.ClientRectangle.Height - gap);
                        }
                    }
                }

                RenderTick(backGfx, m_instantFrequency, Color.DarkGray);
                RenderTick(backGfx, m_filteredFrequency, Color.White);
            }

            Graphics frontGfx = pnlGraph.CreateGraphics();
            frontGfx.DrawImage(backBuffer, 0, 0);
            frontGfx.Dispose();
            backGfx.Dispose();
            backBuffer.Dispose();
        }

        private void TunerChannelDisplay_Paint(object sender, PaintEventArgs e)
        {
            if (RealDesignMode)
            {
                Graphics gfx = CreateGraphics();
                gfx.DrawRectangle(Pens.Black, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                gfx.DrawLine(Pens.Black, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
                gfx.Dispose();
            }
            else
            {
                DoPaint();
            }
        }

        private void OnValueSet()
        {
            DoPaint();
            lblCenterFrequency.Text = m_centerFrequency.ToString("0000.0");
            lblMinFrequency.Text = m_minFrequency.ToString("0000.0");
            lblMaxFrequency.Text = m_maxFrequency.ToString("0000.0");
            lblCurrentFrequency.Text = string.Format("Instant: {0:0000.0} Filtered: {1:0000.0}", m_instantFrequency, m_filteredFrequency);
            //lblTemporaryDisplay.Text = "";
            //lblTemporaryDisplay.Text = string.Format("I: {0:0000.0} F: {1:0000.0} <: {2:0000.0} C: {3:0000.0} >: {4:0000.0}", m_instantFrequency, m_filteredFrequency, m_minFrequency, m_centerFrequency, m_maxFrequency);
        }
    }
}
