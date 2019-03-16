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
            this.btnQuickTest = new System.Windows.Forms.Button();
            this.lblMisc = new System.Windows.Forms.Label();
            this.chkSingleChannelMode = new System.Windows.Forms.CheckBox();
            this.chkFullRangeOnAllChannels = new System.Windows.Forms.CheckBox();
            this.tunerChannelDisplay4 = new TunerSimulator.TunerChannelDisplay();
            this.tunerChannelDisplay3 = new TunerSimulator.TunerChannelDisplay();
            this.tunerChannelDisplay2 = new TunerSimulator.TunerChannelDisplay();
            this.tunerChannelDisplay1 = new TunerSimulator.TunerChannelDisplay();
            this.tunerChannelControl4 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl3 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl2 = new TunerSimulator.TunerChannelControl();
            this.tunerChannelControl1 = new TunerSimulator.TunerChannelControl();
            this.chkDumpBufferOnLowAmplitude = new System.Windows.Forms.CheckBox();
            this.gbDumping.SuspendLayout();
            this.SuspendLayout();
            // 
            // spSerial
            // 
            this.spSerial.BaudRate = 115200;
            this.spSerial.DtrEnable = true;
            this.spSerial.PortName = "COM5";
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
            this.pnlSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlSamples.Location = new System.Drawing.Point(15, 33);
            this.pnlSamples.Name = "pnlSamples";
            this.pnlSamples.Size = new System.Drawing.Size(493, 590);
            this.pnlSamples.TabIndex = 1;
            // 
            // btnFullTest
            // 
            this.btnFullTest.Enabled = false;
            this.btnFullTest.Location = new System.Drawing.Point(707, 33);
            this.btnFullTest.Name = "btnFullTest";
            this.btnFullTest.Size = new System.Drawing.Size(75, 23);
            this.btnFullTest.TabIndex = 2;
            this.btnFullTest.Text = "Full Test";
            this.btnFullTest.UseVisualStyleBackColor = true;
            this.btnFullTest.Click += new System.EventHandler(this.btnFullTest_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(514, 33);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop Audio";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // gbDumping
            // 
            this.gbDumping.Controls.Add(this.chkDumpBufferOnLowAmplitude);
            this.gbDumping.Controls.Add(this.cmbDumpMode);
            this.gbDumping.Controls.Add(this.chkDumpOnOctaveError);
            this.gbDumping.Controls.Add(this.lblMinDumpFrequency);
            this.gbDumping.Controls.Add(this.txtMinDumpFrequency);
            this.gbDumping.Controls.Add(this.chkDumpOnNull);
            this.gbDumping.Location = new System.Drawing.Point(514, 470);
            this.gbDumping.Margin = new System.Windows.Forms.Padding(1);
            this.gbDumping.Name = "gbDumping";
            this.gbDumping.Padding = new System.Windows.Forms.Padding(1);
            this.gbDumping.Size = new System.Drawing.Size(256, 141);
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
            this.cmbDumpMode.Location = new System.Drawing.Point(4, 112);
            this.cmbDumpMode.Name = "cmbDumpMode";
            this.cmbDumpMode.Size = new System.Drawing.Size(121, 21);
            this.cmbDumpMode.TabIndex = 14;
            this.cmbDumpMode.SelectedIndexChanged += new System.EventHandler(this.cmbDumpMode_SelectedIndexChanged);
            // 
            // chkDumpOnOctaveError
            // 
            this.chkDumpOnOctaveError.AutoSize = true;
            this.chkDumpOnOctaveError.Location = new System.Drawing.Point(6, 89);
            this.chkDumpOnOctaveError.Name = "chkDumpOnOctaveError";
            this.chkDumpOnOctaveError.Size = new System.Drawing.Size(152, 17);
            this.chkDumpOnOctaveError.TabIndex = 13;
            this.chkDumpOnOctaveError.Text = "Auto-dump on octave error";
            this.chkDumpOnOctaveError.UseVisualStyleBackColor = true;
            this.chkDumpOnOctaveError.CheckedChanged += new System.EventHandler(this.chkDumpOnOctaveError_CheckedChanged);
            // 
            // lblMinDumpFrequency
            // 
            this.lblMinDumpFrequency.AutoSize = true;
            this.lblMinDumpFrequency.Location = new System.Drawing.Point(3, 66);
            this.lblMinDumpFrequency.Name = "lblMinDumpFrequency";
            this.lblMinDumpFrequency.Size = new System.Drawing.Size(69, 13);
            this.lblMinDumpFrequency.TabIndex = 12;
            this.lblMinDumpFrequency.Text = "Dump below:";
            // 
            // txtMinDumpFrequency
            // 
            this.txtMinDumpFrequency.Location = new System.Drawing.Point(78, 63);
            this.txtMinDumpFrequency.Name = "txtMinDumpFrequency";
            this.txtMinDumpFrequency.Size = new System.Drawing.Size(100, 20);
            this.txtMinDumpFrequency.TabIndex = 11;
            this.txtMinDumpFrequency.TextChanged += new System.EventHandler(this.txtMinDumpFrequency_TextChanged);
            // 
            // chkDumpOnNull
            // 
            this.chkDumpOnNull.AutoSize = true;
            this.chkDumpOnNull.Location = new System.Drawing.Point(4, 40);
            this.chkDumpOnNull.Name = "chkDumpOnNull";
            this.chkDumpOnNull.Size = new System.Drawing.Size(247, 17);
            this.chkDumpOnNull.TabIndex = 10;
            this.chkDumpOnNull.Text = "Dump on null reading (with sufficient amplitude)";
            this.chkDumpOnNull.UseVisualStyleBackColor = true;
            this.chkDumpOnNull.CheckedChanged += new System.EventHandler(this.chkDumpOnNull_CheckedChanged);
            // 
            // lblMinF
            // 
            this.lblMinF.AutoSize = true;
            this.lblMinF.Location = new System.Drawing.Point(558, 123);
            this.lblMinF.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblMinF.Name = "lblMinF";
            this.lblMinF.Size = new System.Drawing.Size(30, 13);
            this.lblMinF.TabIndex = 16;
            this.lblMinF.Text = "MinF";
            // 
            // lblMaxF
            // 
            this.lblMaxF.AutoSize = true;
            this.lblMaxF.Location = new System.Drawing.Point(604, 123);
            this.lblMaxF.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblMaxF.Name = "lblMaxF";
            this.lblMaxF.Size = new System.Drawing.Size(33, 13);
            this.lblMaxF.TabIndex = 17;
            this.lblMaxF.Text = "MaxF";
            // 
            // lblCDP
            // 
            this.lblCDP.AutoSize = true;
            this.lblCDP.Location = new System.Drawing.Point(649, 123);
            this.lblCDP.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblCDP.Name = "lblCDP";
            this.lblCDP.Size = new System.Drawing.Size(30, 13);
            this.lblCDP.TabIndex = 18;
            this.lblCDP.Text = "CD%";
            // 
            // lblGcfs
            // 
            this.lblGcfs.AutoSize = true;
            this.lblGcfs.Location = new System.Drawing.Point(693, 123);
            this.lblGcfs.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblGcfs.Name = "lblGcfs";
            this.lblGcfs.Size = new System.Drawing.Size(35, 13);
            this.lblGcfs.TabIndex = 19;
            this.lblGcfs.Text = "GCFS";
            // 
            // lblBos
            // 
            this.lblBos.AutoSize = true;
            this.lblBos.Location = new System.Drawing.Point(741, 123);
            this.lblBos.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblBos.Name = "lblBos";
            this.lblBos.Size = new System.Drawing.Size(29, 13);
            this.lblBos.TabIndex = 20;
            this.lblBos.Text = "BOS";
            // 
            // lblBosi
            // 
            this.lblBosi.AutoSize = true;
            this.lblBosi.Location = new System.Drawing.Point(785, 123);
            this.lblBosi.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblBosi.Name = "lblBosi";
            this.lblBosi.Size = new System.Drawing.Size(32, 13);
            this.lblBosi.TabIndex = 21;
            this.lblBosi.Text = "BOSI";
            // 
            // btnQuickTest
            // 
            this.btnQuickTest.Enabled = false;
            this.btnQuickTest.Location = new System.Drawing.Point(788, 33);
            this.btnQuickTest.Name = "btnQuickTest";
            this.btnQuickTest.Size = new System.Drawing.Size(75, 23);
            this.btnQuickTest.TabIndex = 22;
            this.btnQuickTest.Text = "Quick Test";
            this.btnQuickTest.UseVisualStyleBackColor = true;
            this.btnQuickTest.Click += new System.EventHandler(this.btnQuickTest_Click);
            // 
            // lblMisc
            // 
            this.lblMisc.AutoSize = true;
            this.lblMisc.Location = new System.Drawing.Point(512, 9);
            this.lblMisc.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.lblMisc.Name = "lblMisc";
            this.lblMisc.Size = new System.Drawing.Size(51, 13);
            this.lblMisc.TabIndex = 24;
            this.lblMisc.Text = "XXXX ms";
            // 
            // chkSingleChannelMode
            // 
            this.chkSingleChannelMode.AutoSize = true;
            this.chkSingleChannelMode.Location = new System.Drawing.Point(514, 62);
            this.chkSingleChannelMode.Name = "chkSingleChannelMode";
            this.chkSingleChannelMode.Size = new System.Drawing.Size(126, 17);
            this.chkSingleChannelMode.TabIndex = 29;
            this.chkSingleChannelMode.Text = "Single-channel Mode";
            this.chkSingleChannelMode.UseVisualStyleBackColor = true;
            this.chkSingleChannelMode.CheckedChanged += new System.EventHandler(this.chkSingleChannelMode_CheckedChanged);
            // 
            // chkFullRangeOnAllChannels
            // 
            this.chkFullRangeOnAllChannels.AutoSize = true;
            this.chkFullRangeOnAllChannels.Location = new System.Drawing.Point(514, 85);
            this.chkFullRangeOnAllChannels.Name = "chkFullRangeOnAllChannels";
            this.chkFullRangeOnAllChannels.Size = new System.Drawing.Size(196, 17);
            this.chkFullRangeOnAllChannels.TabIndex = 30;
            this.chkFullRangeOnAllChannels.Text = "Full frequency range on all channels";
            this.chkFullRangeOnAllChannels.UseVisualStyleBackColor = true;
            this.chkFullRangeOnAllChannels.CheckedChanged += new System.EventHandler(this.chkFullRangeOnAllChannels_CheckedChanged);
            // 
            // tunerChannelDisplay4
            // 
            this.tunerChannelDisplay4.CenterFrequency = 0F;
            this.tunerChannelDisplay4.FilteredFrequency = 0F;
            this.tunerChannelDisplay4.InstantFrequency = 0F;
            this.tunerChannelDisplay4.Location = new System.Drawing.Point(514, 410);
            this.tunerChannelDisplay4.MaxFrequency = 0F;
            this.tunerChannelDisplay4.MidiNoteIndex = 0;
            this.tunerChannelDisplay4.MinFrequency = 0F;
            this.tunerChannelDisplay4.Name = "tunerChannelDisplay4";
            this.tunerChannelDisplay4.Size = new System.Drawing.Size(339, 56);
            this.tunerChannelDisplay4.TabIndex = 28;
            // 
            // tunerChannelDisplay3
            // 
            this.tunerChannelDisplay3.CenterFrequency = 0F;
            this.tunerChannelDisplay3.FilteredFrequency = 0F;
            this.tunerChannelDisplay3.InstantFrequency = 0F;
            this.tunerChannelDisplay3.Location = new System.Drawing.Point(514, 348);
            this.tunerChannelDisplay3.MaxFrequency = 0F;
            this.tunerChannelDisplay3.MidiNoteIndex = 0;
            this.tunerChannelDisplay3.MinFrequency = 0F;
            this.tunerChannelDisplay3.Name = "tunerChannelDisplay3";
            this.tunerChannelDisplay3.Size = new System.Drawing.Size(339, 56);
            this.tunerChannelDisplay3.TabIndex = 27;
            // 
            // tunerChannelDisplay2
            // 
            this.tunerChannelDisplay2.CenterFrequency = 0F;
            this.tunerChannelDisplay2.FilteredFrequency = 0F;
            this.tunerChannelDisplay2.InstantFrequency = 0F;
            this.tunerChannelDisplay2.Location = new System.Drawing.Point(514, 286);
            this.tunerChannelDisplay2.MaxFrequency = 0F;
            this.tunerChannelDisplay2.MidiNoteIndex = 0;
            this.tunerChannelDisplay2.MinFrequency = 0F;
            this.tunerChannelDisplay2.Name = "tunerChannelDisplay2";
            this.tunerChannelDisplay2.Size = new System.Drawing.Size(339, 56);
            this.tunerChannelDisplay2.TabIndex = 26;
            // 
            // tunerChannelDisplay1
            // 
            this.tunerChannelDisplay1.CenterFrequency = 0F;
            this.tunerChannelDisplay1.FilteredFrequency = 0F;
            this.tunerChannelDisplay1.InstantFrequency = 0F;
            this.tunerChannelDisplay1.Location = new System.Drawing.Point(514, 224);
            this.tunerChannelDisplay1.MaxFrequency = 0F;
            this.tunerChannelDisplay1.MidiNoteIndex = 0;
            this.tunerChannelDisplay1.MinFrequency = 0F;
            this.tunerChannelDisplay1.Name = "tunerChannelDisplay1";
            this.tunerChannelDisplay1.Size = new System.Drawing.Size(339, 56);
            this.tunerChannelDisplay1.TabIndex = 25;
            // 
            // tunerChannelControl4
            // 
            this.tunerChannelControl4.ChannelIndex = 3;
            this.tunerChannelControl4.DetailedSearchEnabled = false;
            this.tunerChannelControl4.Location = new System.Drawing.Point(514, 202);
            this.tunerChannelControl4.Margin = new System.Windows.Forms.Padding(0);
            this.tunerChannelControl4.Name = "tunerChannelControl4";
            this.tunerChannelControl4.Size = new System.Drawing.Size(332, 19);
            this.tunerChannelControl4.SuspendChanges = false;
            this.tunerChannelControl4.TabIndex = 15;
            this.tunerChannelControl4.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl3
            // 
            this.tunerChannelControl3.ChannelIndex = 2;
            this.tunerChannelControl3.DetailedSearchEnabled = false;
            this.tunerChannelControl3.Location = new System.Drawing.Point(514, 180);
            this.tunerChannelControl3.Margin = new System.Windows.Forms.Padding(0);
            this.tunerChannelControl3.Name = "tunerChannelControl3";
            this.tunerChannelControl3.Size = new System.Drawing.Size(332, 19);
            this.tunerChannelControl3.SuspendChanges = false;
            this.tunerChannelControl3.TabIndex = 14;
            this.tunerChannelControl3.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl2
            // 
            this.tunerChannelControl2.ChannelIndex = 1;
            this.tunerChannelControl2.DetailedSearchEnabled = false;
            this.tunerChannelControl2.Location = new System.Drawing.Point(514, 159);
            this.tunerChannelControl2.Margin = new System.Windows.Forms.Padding(0);
            this.tunerChannelControl2.Name = "tunerChannelControl2";
            this.tunerChannelControl2.Size = new System.Drawing.Size(332, 19);
            this.tunerChannelControl2.SuspendChanges = false;
            this.tunerChannelControl2.TabIndex = 13;
            this.tunerChannelControl2.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // tunerChannelControl1
            // 
            this.tunerChannelControl1.ChannelIndex = 0;
            this.tunerChannelControl1.DetailedSearchEnabled = false;
            this.tunerChannelControl1.Location = new System.Drawing.Point(514, 137);
            this.tunerChannelControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tunerChannelControl1.Name = "tunerChannelControl1";
            this.tunerChannelControl1.Size = new System.Drawing.Size(332, 19);
            this.tunerChannelControl1.SuspendChanges = false;
            this.tunerChannelControl1.TabIndex = 12;
            this.tunerChannelControl1.ConfigurationChanged += new System.EventHandler(this.tunerChannelControl_ConfigurationChanged);
            // 
            // chkDumpBufferOnLowAmplitude
            // 
            this.chkDumpBufferOnLowAmplitude.AutoSize = true;
            this.chkDumpBufferOnLowAmplitude.Location = new System.Drawing.Point(4, 17);
            this.chkDumpBufferOnLowAmplitude.Name = "chkDumpBufferOnLowAmplitude";
            this.chkDumpBufferOnLowAmplitude.Size = new System.Drawing.Size(166, 17);
            this.chkDumpBufferOnLowAmplitude.TabIndex = 15;
            this.chkDumpBufferOnLowAmplitude.Text = "Dump buffer on low amplitude";
            this.chkDumpBufferOnLowAmplitude.UseVisualStyleBackColor = true;
            this.chkDumpBufferOnLowAmplitude.CheckedChanged += new System.EventHandler(this.chkDumpBufferOnLowAmplitude_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 635);
            this.Controls.Add(this.chkFullRangeOnAllChannels);
            this.Controls.Add(this.chkSingleChannelMode);
            this.Controls.Add(this.tunerChannelDisplay4);
            this.Controls.Add(this.tunerChannelDisplay3);
            this.Controls.Add(this.tunerChannelDisplay2);
            this.Controls.Add(this.tunerChannelDisplay1);
            this.Controls.Add(this.lblMisc);
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
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnFullTest);
            this.Controls.Add(this.pnlSamples);
            this.Controls.Add(this.lblReading);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Kiwi Tuner Test Harness";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
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
        private System.Windows.Forms.Label lblMisc;
        private TunerChannelDisplay tunerChannelDisplay1;
        private TunerChannelDisplay tunerChannelDisplay2;
        private TunerChannelDisplay tunerChannelDisplay3;
        private TunerChannelDisplay tunerChannelDisplay4;
        private System.Windows.Forms.CheckBox chkSingleChannelMode;
        private System.Windows.Forms.CheckBox chkFullRangeOnAllChannels;
        private System.Windows.Forms.CheckBox chkDumpBufferOnLowAmplitude;
    }
}