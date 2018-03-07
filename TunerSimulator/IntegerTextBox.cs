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
    public partial class IntegerTextBox : UserControl
    {
        public IntegerTextBox()
        {
            InitializeComponent();
        }

        //public delegate void ValueChangedDelegate();
        public event EventHandler ValueChanged;

        public int Value
        {
            get
            {
                int result;
                int.TryParse(txtValue.Text, out result);
                return result;
            }
            set
            {
                txtValue.Text = value.ToString();
            }
        }

        private void txtValue_TextChanged(object sender, EventArgs e)
        {
            var evt = ValueChanged;
            if (evt != null)
            {
                evt(sender, e);
            }
        }
    }
}
