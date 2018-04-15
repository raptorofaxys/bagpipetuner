namespace TunerSimulator
{
    partial class TunerChannelDisplay
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
            this.lblTemporaryDisplay = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTemporaryDisplay
            // 
            this.lblTemporaryDisplay.AutoSize = true;
            this.lblTemporaryDisplay.Location = new System.Drawing.Point(3, 0);
            this.lblTemporaryDisplay.Name = "lblTemporaryDisplay";
            this.lblTemporaryDisplay.Size = new System.Drawing.Size(35, 13);
            this.lblTemporaryDisplay.TabIndex = 0;
            this.lblTemporaryDisplay.Text = "label1";
            // 
            // TunerChannelDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblTemporaryDisplay);
            this.Name = "TunerChannelDisplay";
            this.Size = new System.Drawing.Size(339, 14);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTemporaryDisplay;
    }
}
