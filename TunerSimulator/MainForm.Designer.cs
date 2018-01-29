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
            this.btnFullTest = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblCorrelationDipPct = new System.Windows.Forms.Label();
            this.txtCorrelationDipPct = new System.Windows.Forms.TextBox();
            this.gbDumping = new System.Windows.Forms.GroupBox();
            this.cmbDumpMode = new System.Windows.Forms.ComboBox();
            this.chkDumpOnOctaveError = new System.Windows.Forms.CheckBox();
            this.lblMinDumpFrequency = new System.Windows.Forms.Label();
            this.txtMinDumpFrequency = new System.Windows.Forms.TextBox();
            this.chkDumpOnNull = new System.Windows.Forms.CheckBox();
            this.lblMinF = new System.Windows.Forms.Label();
            this.lblMaxF = new System.Windows.Forms.Label();
            this.lblCDP = new System.Windows.Forms.Label();
            this.lblGcfs = new System.Windows.Forms.Label();
            this.lblBos = new System.Windows.Forms.Label();
            this.lblBosi = new System.Windows.Forms.Label();
            this.tunerChannelControl4 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl3 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl2 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl1 = new TunerSimulator.TunerChannelControl();
            this.btnQuickTest = new System.Windows.Forms.Button();
            this.gbDumping.SuspendLayout();
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
            this.lblReading.Location = new System.Drawing.Point(28, 20);
            this.lblReading.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblReading.Name = "lblReading";
            this.lblReading.Size = new System.Drawing.Size(149, 29);
            this.lblReading.TabIndex = 0;
            this.lblReading.Text = "(No reading)";
            // 
            // pnlSamples
            // 
            this.pnlSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlSamples.Location = new System.Drawing.Point(35, 74);
            this.pnlSamples.Margin = new System.Windows.Forms.Padding(7);
            this.pnlSamples.Name = "pnlSamples";
            this.pnlSamples.Size = new System.Drawing.Size(1150, 948);
            this.pnlSamples.TabIndex = 1;
            // 
            // btnFullTest
            // 
            this.btnFullTest.Location = new System.Drawing.Point(1213, 964);
            this.btnFullTest.Margin = new System.Windows.Forms.Padding(7);
            this.btnFullTest.Name = "btnFullTest";
            this.btnFullTest.Size = new System.Drawing.Size(175, 51);
            this.btnFullTest.TabIndex = 2;
            this.btnFullTest.Text = "Full Test";
            this.btnFullTest.UseVisualStyleBackColor = true;
            this.btnFullTest.Click += new System.EventHandler(this.btnFullTest_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(1199, 74);
            this.btnStop.Margin = new System.Windows.Forms.Padding(7);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(175, 51);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop Audio";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblCorrelationDipPct
            // 
            this.lblCorrelationDipPct.AutoSize = true;
            this.lblCorrelationDipPct.Location = new System.Drawing.Point(1211, 147);
            this.lblCorrelationDipPct.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblCorrelationDipPct.Name = "lblCorrelationDipPct";
            this.lblCorrelationDipPct.Size = new System.Drawing.Size(206, 29);
            this.lblCorrelationDipPct.TabIndex = 8;
            this.lblCorrelationDipPct.Text = "Correlation dip %:";
            // 
            // txtCorrelationDipPct
            // 
            this.txtCorrelationDipPct.Location = new System.Drawing.Point(1430, 141);
            this.txtCorrelationDipPct.Margin = new System.Windows.Forms.Padding(7);
            this.txtCorrelationDipPct.Name = "txtCorrelationDipPct";
            this.txtCorrelationDipPct.Size = new System.Drawing.Size(184, 35);
            this.txtCorrelationDipPct.TabIndex = 7;
            this.txtCorrelationDipPct.TextChanged += new System.EventHandler(this.txtCorrelationDipPct_TextChanged);
            // 
            // gbDumping
            // 
            this.gbDumping.Controls.Add(this.cmbDumpMode);
            this.gbDumping.Controls.Add(this.chkDumpOnOctaveError);
            this.gbDumping.Controls.Add(this.lblMinDumpFrequency);
            this.gbDumping.Controls.Add(this.txtMinDumpFrequency);
            this.gbDumping.Controls.Add(this.chkDumpOnNull);
            this.gbDumping.Location = new System.Drawing.Point(1213, 588);
            this.gbDumping.Name = "gbDumping";
            this.gbDumping.Size = new System.Drawing.Size(485, 354);
            this.gbDumping.TabIndex = 10;
            this.gbDumping.TabStop = false;
            this.gbDumping.Text = "Dumping";
            // 
            // cmbDumpMode
            // 
            this.cmbDumpMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDumpMode.FormattingEnabled = true;
            this.cmbDumpMode.Items.AddRange(new object[] {
            "Dump Buffer",
            "Dump GCF"});
            this.cmbDumpMode.Location = new System.Drawing.Point(10, 198);
            this.cmbDumpMode.Margin = new System.Windows.Forms.Padding(7);
            this.cmbDumpMode.Name = "cmbDumpMode";
            this.cmbDumpMode.Size = new System.Drawing.Size(277, 37);
            this.cmbDumpMode.TabIndex = 14;
            // 
            // chkDumpOnOctaveError
            // 
            this.chkDumpOnOctaveError.AutoSize = true;
            this.chkDumpOnOctaveError.Location = new System.Drawing.Point(15, 147);
            this.chkDumpOnOctaveError.Margin = new System.Windows.Forms.Padding(7);
            this.chkDumpOnOctaveError.Name = "chkDumpOnOctaveError";
            this.chkDumpOnOctaveError.Size = new System.Drawing.Size(329, 33);
            this.chkDumpOnOctaveError.TabIndex = 13;
            this.chkDumpOnOctaveError.Text = "Auto-dump on octave error";
            this.chkDumpOnOctaveError.UseVisualStyleBackColor = true;
            // 
            // lblMinDumpFrequency
            // 
            this.lblMinDumpFrequency.AutoSize = true;
            this.lblMinDumpFrequency.Location = new System.Drawing.Point(8, 96);
            this.lblMinDumpFrequency.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblMinDumpFrequency.Name = "lblMinDumpFrequency";
            this.lblMinDumpFrequency.Size = new System.Drawing.Size(155, 29);
            this.lblMinDumpFrequency.TabIndex = 12;
            this.lblMinDumpFrequency.Text = "Dump below:";
            // 
            // txtMinDumpFrequency
            // 
            this.txtMinDumpFrequency.Location = new System.Drawing.Point(183, 89);
            this.txtMinDumpFrequency.Margin = new System.Windows.Forms.Padding(7);
            this.txtMinDumpFrequency.Name = "txtMinDumpFrequency";
            this.txtMinDumpFrequency.Size = new System.Drawing.Size(228, 35);
            this.txtMinDumpFrequency.TabIndex = 11;
            // 
            // chkDumpOnNull
            // 
            this.chkDumpOnNull.AutoSize = true;
            this.chkDumpOnNull.Location = new System.Drawing.Point(10, 38);
            this.chkDumpOnNull.Margin = new System.Windows.Forms.Padding(7);
            this.chkDumpOnNull.Name = "chkDumpOnNull";
            this.chkDumpOnNull.Size = new System.Drawing.Size(274, 33);
            this.chkDumpOnNull.TabIndex = 10;
            this.chkDumpOnNull.Text = "Dump on null reading";
            this.chkDumpOnNull.UseVisualStyleBackColor = true;
            // 
            // lblMinF
            // 
            this.lblMinF.AutoSize = true;
            this.lblMinF.Location = new System.Drawing.Point(1301, 274);
            this.lblMinF.Name = "lblMinF";
            this.lblMinF.Size = new System.Drawing.Size(67, 29);
            this.lblMinF.TabIndex = 16;
            this.lblMinF.Text = "MinF";
            // 
            // lblMaxF
            // 
            this.lblMaxF.AutoSize = true;
            this.lblMaxF.Location = new System.Drawing.Point(1409, 274);
            this.lblMaxF.Name = "lblMaxF";
            this.lblMaxF.Size = new System.Drawing.Size(72, 29);
            this.lblMaxF.TabIndex = 17;
            this.lblMaxF.Text = "MaxF";
            // 
            // lblCDP
            // 
            this.lblCDP.AutoSize = true;
            this.lblCDP.Location = new System.Drawing.Point(1514, 274);
            this.lblCDP.Name = "lblCDP";
            this.lblCDP.Size = new System.Drawing.Size(69, 29);
            this.lblCDP.TabIndex = 18;
            this.lblCDP.Text = "CD%";
            // 
            // lblGcfs
            // 
            this.lblGcfs.AutoSize = true;
            this.lblGcfs.Location = new System.Drawing.Point(1616, 274);
            this.lblGcfs.Name = "lblGcfs";
            this.lblGcfs.Size = new System.Drawing.Size(79, 29);
            this.lblGcfs.TabIndex = 19;
            this.lblGcfs.Text = "GCFS";
            // 
            // lblBos
            // 
            this.lblBos.AutoSize = true;
            this.lblBos.Location = new System.Drawing.Point(1728, 274);
            this.lblBos.Name = "lblBos";
            this.lblBos.Size = new System.Drawing.Size(64, 29);
            this.lblBos.TabIndex = 20;
            this.lblBos.Text = "BOS";
            // 
            // lblBosi
            // 
            this.lblBosi.AutoSize = true;
            this.lblBosi.Location = new System.Drawing.Point(1832, 274);
            this.lblBosi.Name = "lblBosi";
            this.lblBosi.Size = new System.Drawing.Size(70, 29);
            this.lblBosi.TabIndex = 21;
            this.lblBosi.Text = "BOSI";
            // 
            // tunerChannelControl4
            // 
            this.tunerChannelControl4.ChannelIndex = 3;
            this.tunerChannelControl4.Location = new System.Drawing.Point(1199, 450);
            this.tunerChannelControl4.Name = "tunerChannelControl4";
            this.tunerChannelControl4.Size = new System.Drawing.Size(740, 42);
            this.tunerChannelControl4.SuspendChanges = false;
            this.tunerChannelControl4.TabIndex = 15;
            this.tunerChannelControl4.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl3
            // 
            this.tunerChannelControl3.ChannelIndex = 2;
            this.tunerChannelControl3.Location = new System.Drawing.Point(1199, 402);
            this.tunerChannelControl3.Name = "tunerChannelControl3";
            this.tunerChannelControl3.Size = new System.Drawing.Size(740, 42);
            this.tunerChannelControl3.SuspendChanges = false;
            this.tunerChannelControl3.TabIndex = 14;
            this.tunerChannelControl3.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl2
            // 
            this.tunerChannelControl2.ChannelIndex = 1;
            this.tunerChannelControl2.Location = new System.Drawing.Point(1199, 354);
            this.tunerChannelControl2.Name = "tunerChannelControl2";
            this.tunerChannelControl2.Size = new System.Drawing.Size(740, 42);
            this.tunerChannelControl2.SuspendChanges = false;
            this.tunerChannelControl2.TabIndex = 13;
            this.tunerChannelControl2.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl1
            // 
            this.tunerChannelControl1.ChannelIndex = 0;
            this.tunerChannelControl1.Location = new System.Drawing.Point(1199, 306);
            this.tunerChannelControl1.Name = "tunerChannelControl1";
            this.tunerChannelControl1.Size = new System.Drawing.Size(740, 42);
            this.tunerChannelControl1.SuspendChanges = false;
            this.tunerChannelControl1.TabIndex = 12;
            this.tunerChannelControl1.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // btnQuickTest
            // 
            this.btnQuickTest.Location = new System.Drawing.Point(1402, 964);
            this.btnQuickTest.Margin = new System.Windows.Forms.Padding(7);
            this.btnQuickTest.Name = "btnQuickTest";
            this.btnQuickTest.Size = new System.Drawing.Size(175, 51);
            this.btnQuickTest.TabIndex = 22;
            this.btnQuickTest.Text = "Quick Test";
            this.btnQuickTest.UseVisualStyleBackColor = true;
            this.btnQuickTest.Click += new System.EventHandler(this.btnQuickTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1994, 1048);
            this.Controls.Add(this.btnQuickTest);
            this.Controls.Add(this.lblBosi);
            this.Controls.Add(this.lblBos);
            this.Controls.Add(this.lblGcfs);
            this.Controls.Add(this.lblCDP);
            this.Controls.Add(this.lblMaxF);
            this.Controls.Add(this.lblMinF);
            this.Controls.Add(this.tunerChannelControl4);
            this.Controls.Add(this.tunerChannelControl3);
            this.Controls.Add(this.tunerChannelControl2);
            this.Controls.Add(this.tunerChannelControl1);
            this.Controls.Add(this.gbDumping);
            this.Controls.Add(this.lblCorrelationDipPct);
            this.Controls.Add(this.txtCorrelationDipPct);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnFullTest);
            this.Controls.Add(this.pnlSamples);
            this.Controls.Add(this.lblReading);
            this.Margin = new System.Windows.Forms.Padding(7);
            this.Name = "MainForm";
            this.Text = "Kiwi Tuner Test Harness";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.gbDumping.ResumeLayout(false);
            this.gbDumping.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort spSerial;
        private System.Windows.Forms.Label lblReading;
        private System.Windows.Forms.Panel pnlSamples;
        private System.Windows.Forms.Button btnFullTest;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblCorrelationDipPct;
        private System.Windows.Forms.TextBox txtCorrelationDipPct;
        private System.Windows.Forms.GroupBox gbDumping;
        private System.Windows.Forms.ComboBox cmbDumpMode;
        private System.Windows.Forms.CheckBox chkDumpOnOctaveError;
        private System.Windows.Forms.Label lblMinDumpFrequency;
        private System.Windows.Forms.TextBox txtMinDumpFrequency;
        private System.Windows.Forms.CheckBox chkDumpOnNull;
        private TunerChannelControl tunerChannelControl1;
        private TunerChannelControl tunerChannelControl2;
        private TunerChannelControl tunerChannelControl3;
        private TunerChannelControl tunerChannelControl4;
        private System.Windows.Forms.Label lblMinF;
        private System.Windows.Forms.Label lblMaxF;
        private System.Windows.Forms.Label lblCDP;
        private System.Windows.Forms.Label lblGcfs;
        private System.Windows.Forms.Label lblBos;
        private System.Windows.Forms.Label lblBosi;
        private System.Windows.Forms.Button btnQuickTest;
    }
}