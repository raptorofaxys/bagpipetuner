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
            this.txtCorrelationDipPercent = new System.Windows.Forms.TextBox();
            this.txtGcfStep = new System.Windows.Forms.TextBox();
            this.txtBaseOffsetStep = new System.Windows.Forms.TextBox();
            this.txtBaseOffsetStepIncrement = new System.Windows.Forms.TextBox();
            this.txtMaxFrequency = new System.Windows.Forms.TextBox();
            this.txtMinFrequency = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtCorrelationDipPercent
            // 
            this.txtCorrelationDipPercent.Location = new System.Drawing.Point(215, 2);
            this.txtCorrelationDipPercent.Name = "txtCorrelationDipPercent";
            this.txtCorrelationDipPercent.Size = new System.Drawing.Size(100, 35);
            this.txtCorrelationDipPercent.TabIndex = 0;
            this.txtCorrelationDipPercent.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // txtGcfStep
            // 
            this.txtGcfStep.Location = new System.Drawing.Point(321, 2);
            this.txtGcfStep.Name = "txtGcfStep";
            this.txtGcfStep.Size = new System.Drawing.Size(100, 35);
            this.txtGcfStep.TabIndex = 1;
            this.txtGcfStep.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // txtBaseOffsetStep
            // 
            this.txtBaseOffsetStep.Location = new System.Drawing.Point(427, 2);
            this.txtBaseOffsetStep.Name = "txtBaseOffsetStep";
            this.txtBaseOffsetStep.Size = new System.Drawing.Size(100, 35);
            this.txtBaseOffsetStep.TabIndex = 2;
            this.txtBaseOffsetStep.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // txtBaseOffsetStepIncrement
            // 
            this.txtBaseOffsetStepIncrement.Location = new System.Drawing.Point(534, 2);
            this.txtBaseOffsetStepIncrement.Name = "txtBaseOffsetStepIncrement";
            this.txtBaseOffsetStepIncrement.Size = new System.Drawing.Size(100, 35);
            this.txtBaseOffsetStepIncrement.TabIndex = 3;
            this.txtBaseOffsetStepIncrement.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // txtMaxFrequency
            // 
            this.txtMaxFrequency.Location = new System.Drawing.Point(109, 2);
            this.txtMaxFrequency.Name = "txtMaxFrequency";
            this.txtMaxFrequency.Size = new System.Drawing.Size(100, 35);
            this.txtMaxFrequency.TabIndex = 4;
            this.txtMaxFrequency.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // txtMinFrequency
            // 
            this.txtMinFrequency.Location = new System.Drawing.Point(3, 3);
            this.txtMinFrequency.Name = "txtMinFrequency";
            this.txtMinFrequency.Size = new System.Drawing.Size(100, 35);
            this.txtMinFrequency.TabIndex = 5;
            this.txtMinFrequency.TextChanged += new System.EventHandler(this.OnConfigurationChanged);
            // 
            // TunerChannelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtMinFrequency);
            this.Controls.Add(this.txtMaxFrequency);
            this.Controls.Add(this.txtBaseOffsetStepIncrement);
            this.Controls.Add(this.txtBaseOffsetStep);
            this.Controls.Add(this.txtGcfStep);
            this.Controls.Add(this.txtCorrelationDipPercent);
            this.Name = "TunerChannelControl";
            this.Size = new System.Drawing.Size(640, 42);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCorrelationDipPercent;
        private System.Windows.Forms.TextBox txtGcfStep;
        private System.Windows.Forms.TextBox txtBaseOffsetStep;
        private System.Windows.Forms.TextBox txtBaseOffsetStepIncrement;
        private System.Windows.Forms.TextBox txtMaxFrequency;
        private System.Windows.Forms.TextBox txtMinFrequency;
    }
}
