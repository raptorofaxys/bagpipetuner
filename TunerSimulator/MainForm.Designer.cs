namespace TunerSimulator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.spSerial = new System.IO.Ports.SerialPort(this.components);
            this.lblReading = new System.Windows.Forms.Label();
            this.pnlSamples = new System.Windows.Forms.Panel();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.chkDumpOnNull = new System.Windows.Forms.CheckBox();
            this.txtMinDumpFrequency = new System.Windows.Forms.TextBox();
            this.lblMinDumpFrequency = new System.Windows.Forms.Label();
            this.chkDumpOnOctaveError = new System.Windows.Forms.CheckBox();
            this.lblCorrelationDipPct = new System.Windows.Forms.Label();
            this.txtCorrelationDipPct = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // spSerial
            // 
            this.spSerial.BaudRate = 115200;
            this.spSerial.DtrEnable = true;
            this.spSerial.PortName = "COM14";
            // 
            // lblReading
            // 
            this.lblReading.AutoSize = true;
            this.lblReading.Location = new System.Drawing.Point(12, 9);
            this.lblReading.Name = "lblReading";
            this.lblReading.Size = new System.Drawing.Size(65, 13);
            this.lblReading.TabIndex = 0;
            this.lblReading.Text = "(No reading)";
            // 
            // pnlSamples
            // 
            this.pnlSamples.Location = new System.Drawing.Point(15, 33);
            this.pnlSamples.Name = "pnlSamples";
            this.pnlSamples.Size = new System.Drawing.Size(543, 542);
            this.pnlSamples.TabIndex = 1;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(564, 213);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Full Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(564, 112);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // chkDumpOnNull
            // 
            this.chkDumpOnNull.AutoSize = true;
            this.chkDumpOnNull.Location = new System.Drawing.Point(564, 141);
            this.chkDumpOnNull.Name = "chkDumpOnNull";
            this.chkDumpOnNull.Size = new System.Drawing.Size(126, 17);
            this.chkDumpOnNull.TabIndex = 3;
            this.chkDumpOnNull.Text = "Dump on null reading";
            this.chkDumpOnNull.UseVisualStyleBackColor = true;
            this.chkDumpOnNull.CheckedChanged += new System.EventHandler(this.chkDumpOnNull_CheckedChanged);
            // 
            // txtMinDumpFrequency
            // 
            this.txtMinDumpFrequency.Location = new System.Drawing.Point(638, 164);
            this.txtMinDumpFrequency.Name = "txtMinDumpFrequency";
            this.txtMinDumpFrequency.Size = new System.Drawing.Size(100, 20);
            this.txtMinDumpFrequency.TabIndex = 4;
            this.txtMinDumpFrequency.TextChanged += new System.EventHandler(this.txtMinDumpFrequency_TextChanged);
            // 
            // lblMinDumpFrequency
            // 
            this.lblMinDumpFrequency.AutoSize = true;
            this.lblMinDumpFrequency.Location = new System.Drawing.Point(563, 167);
            this.lblMinDumpFrequency.Name = "lblMinDumpFrequency";
            this.lblMinDumpFrequency.Size = new System.Drawing.Size(69, 13);
            this.lblMinDumpFrequency.TabIndex = 5;
            this.lblMinDumpFrequency.Text = "Dump below:";
            // 
            // chkDumpOnOctaveError
            // 
            this.chkDumpOnOctaveError.AutoSize = true;
            this.chkDumpOnOctaveError.Location = new System.Drawing.Point(566, 190);
            this.chkDumpOnOctaveError.Name = "chkDumpOnOctaveError";
            this.chkDumpOnOctaveError.Size = new System.Drawing.Size(152, 17);
            this.chkDumpOnOctaveError.TabIndex = 6;
            this.chkDumpOnOctaveError.Text = "Auto-dump on octave error";
            this.chkDumpOnOctaveError.UseVisualStyleBackColor = true;
            this.chkDumpOnOctaveError.CheckedChanged += new System.EventHandler(this.chkDumpOnOctaveError_CheckedChanged);
            // 
            // lblCorrelationDipPct
            // 
            this.lblCorrelationDipPct.AutoSize = true;
            this.lblCorrelationDipPct.Location = new System.Drawing.Point(563, 36);
            this.lblCorrelationDipPct.Name = "lblCorrelationDipPct";
            this.lblCorrelationDipPct.Size = new System.Drawing.Size(88, 13);
            this.lblCorrelationDipPct.TabIndex = 8;
            this.lblCorrelationDipPct.Text = "Correlation dip %:";
            // 
            // txtCorrelationDipPct
            // 
            this.txtCorrelationDipPct.Location = new System.Drawing.Point(657, 33);
            this.txtCorrelationDipPct.Name = "txtCorrelationDipPct";
            this.txtCorrelationDipPct.Size = new System.Drawing.Size(81, 20);
            this.txtCorrelationDipPct.TabIndex = 7;
            this.txtCorrelationDipPct.TextChanged += new System.EventHandler(this.txtCorrelationDipPct_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 587);
            this.Controls.Add(this.lblCorrelationDipPct);
            this.Controls.Add(this.txtCorrelationDipPct);
            this.Controls.Add(this.chkDumpOnOctaveError);
            this.Controls.Add(this.lblMinDumpFrequency);
            this.Controls.Add(this.txtMinDumpFrequency);
            this.Controls.Add(this.chkDumpOnNull);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.pnlSamples);
            this.Controls.Add(this.lblReading);
            this.Name = "MainForm";
            this.Text = "Kiwi Tuner Test Harness";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort spSerial;
        private System.Windows.Forms.Label lblReading;
        private System.Windows.Forms.Panel pnlSamples;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.CheckBox chkDumpOnNull;
        private System.Windows.Forms.TextBox txtMinDumpFrequency;
        private System.Windows.Forms.Label lblMinDumpFrequency;
        private System.Windows.Forms.CheckBox chkDumpOnOctaveError;
        private System.Windows.Forms.Label lblCorrelationDipPct;
        private System.Windows.Forms.TextBox txtCorrelationDipPct;
    }
}