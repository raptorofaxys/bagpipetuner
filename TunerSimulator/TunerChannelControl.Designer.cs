namespace TunerSimulator
{
    partial class TunerChannelControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CorrelationDipPercent = new TunerSimulator.IntegerTextBox();
            this.GcfStep = new TunerSimulator.IntegerTextBox();
            this.BaseOffsetStep = new TunerSimulator.IntegerTextBox();
            this.BaseOffsetStepIncrement = new TunerSimulator.IntegerTextBox();
            this.MaxFrequency = new TunerSimulator.IntegerTextBox();
            this.MinFrequency = new TunerSimulator.IntegerTextBox();
            this.lblChannelName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CorrelationDipPercent
            // 
            this.CorrelationDipPercent.Location = new System.Drawing.Point(300, 2);
            this.CorrelationDipPercent.Name = "CorrelationDipPercent";
            this.CorrelationDipPercent.Size = new System.Drawing.Size(100, 35);
            this.CorrelationDipPercent.TabIndex = 0;
            this.CorrelationDipPercent.Value = 0;
            this.CorrelationDipPercent.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // GcfStep
            // 
            this.GcfStep.Location = new System.Drawing.Point(406, 2);
            this.GcfStep.Name = "GcfStep";
            this.GcfStep.Size = new System.Drawing.Size(100, 35);
            this.GcfStep.TabIndex = 1;
            this.GcfStep.Value = 0;
            this.GcfStep.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // BaseOffsetStep
            // 
            this.BaseOffsetStep.Location = new System.Drawing.Point(512, 2);
            this.BaseOffsetStep.Name = "BaseOffsetStep";
            this.BaseOffsetStep.Size = new System.Drawing.Size(100, 35);
            this.BaseOffsetStep.TabIndex = 2;
            this.BaseOffsetStep.Value = 0;
            this.BaseOffsetStep.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // BaseOffsetStepIncrement
            // 
            this.BaseOffsetStepIncrement.Location = new System.Drawing.Point(619, 2);
            this.BaseOffsetStepIncrement.Name = "BaseOffsetStepIncrement";
            this.BaseOffsetStepIncrement.Size = new System.Drawing.Size(100, 35);
            this.BaseOffsetStepIncrement.TabIndex = 3;
            this.BaseOffsetStepIncrement.Value = 0;
            this.BaseOffsetStepIncrement.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // MaxFrequency
            // 
            this.MaxFrequency.Location = new System.Drawing.Point(194, 2);
            this.MaxFrequency.Name = "MaxFrequency";
            this.MaxFrequency.Size = new System.Drawing.Size(100, 35);
            this.MaxFrequency.TabIndex = 4;
            this.MaxFrequency.Value = 0;
            this.MaxFrequency.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // MinFrequency
            // 
            this.MinFrequency.Location = new System.Drawing.Point(88, 3);
            this.MinFrequency.Name = "MinFrequency";
            this.MinFrequency.Size = new System.Drawing.Size(100, 35);
            this.MinFrequency.TabIndex = 5;
            this.MinFrequency.Value = 0;
            this.MinFrequency.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // lblChannelName
            // 
            this.lblChannelName.AutoSize = true;
            this.lblChannelName.Location = new System.Drawing.Point(3, 6);
            this.lblChannelName.Name = "lblChannelName";
            this.lblChannelName.Size = new System.Drawing.Size(64, 29);
            this.lblChannelName.TabIndex = 6;
            this.lblChannelName.Text = "CHX";
            // 
            // TunerChannelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblChannelName);
            this.Controls.Add(this.MinFrequency);
            this.Controls.Add(this.MaxFrequency);
            this.Controls.Add(this.BaseOffsetStepIncrement);
            this.Controls.Add(this.BaseOffsetStep);
            this.Controls.Add(this.GcfStep);
            this.Controls.Add(this.CorrelationDipPercent);
            this.Name = "TunerChannelControl";
            this.Size = new System.Drawing.Size(723, 40);
            this.Load += new System.EventHandler(this.TunerChannelControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public TunerSimulator.IntegerTextBox CorrelationDipPercent;
        public TunerSimulator.IntegerTextBox GcfStep;
        public TunerSimulator.IntegerTextBox BaseOffsetStep;
        public TunerSimulator.IntegerTextBox BaseOffsetStepIncrement;
        public TunerSimulator.IntegerTextBox MaxFrequency;
        public TunerSimulator.IntegerTextBox MinFrequency;
        private System.Windows.Forms.Label lblChannelName;
    }
}
