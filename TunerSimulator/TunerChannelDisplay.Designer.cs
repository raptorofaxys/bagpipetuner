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
            this.lblMinFrequency = new System.Windows.Forms.Label();
            this.lblMaxFrequency = new System.Windows.Forms.Label();
            this.lblCenterFrequency = new System.Windows.Forms.Label();
            this.pnlGraph = new System.Windows.Forms.Panel();
            this.lblCurrentFrequency = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblMinFrequency
            // 
            this.lblMinFrequency.AutoSize = true;
            this.lblMinFrequency.Location = new System.Drawing.Point(3, 0);
            this.lblMinFrequency.Name = "lblMinFrequency";
            this.lblMinFrequency.Size = new System.Drawing.Size(24, 13);
            this.lblMinFrequency.TabIndex = 1;
            this.lblMinFrequency.Text = "Min";
            // 
            // lblMaxFrequency
            // 
            this.lblMaxFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaxFrequency.AutoSize = true;
            this.lblMaxFrequency.Location = new System.Drawing.Point(299, 0);
            this.lblMaxFrequency.Name = "lblMaxFrequency";
            this.lblMaxFrequency.Size = new System.Drawing.Size(37, 13);
            this.lblMaxFrequency.TabIndex = 2;
            this.lblMaxFrequency.Text = "Maxxx";
            this.lblMaxFrequency.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCenterFrequency
            // 
            this.lblCenterFrequency.AutoSize = true;
            this.lblCenterFrequency.Location = new System.Drawing.Point(157, 0);
            this.lblCenterFrequency.Name = "lblCenterFrequency";
            this.lblCenterFrequency.Size = new System.Drawing.Size(38, 13);
            this.lblCenterFrequency.TabIndex = 3;
            this.lblCenterFrequency.Text = "Center";
            this.lblCenterFrequency.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlGraph
            // 
            this.pnlGraph.Location = new System.Drawing.Point(6, 16);
            this.pnlGraph.Name = "pnlGraph";
            this.pnlGraph.Size = new System.Drawing.Size(330, 18);
            this.pnlGraph.TabIndex = 4;
            // 
            // lblCurrentFrequency
            // 
            this.lblCurrentFrequency.AutoSize = true;
            this.lblCurrentFrequency.Location = new System.Drawing.Point(3, 37);
            this.lblCurrentFrequency.Name = "lblCurrentFrequency";
            this.lblCurrentFrequency.Size = new System.Drawing.Size(41, 13);
            this.lblCurrentFrequency.TabIndex = 5;
            this.lblCurrentFrequency.Text = "Current";
            // 
            // TunerChannelDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblCurrentFrequency);
            this.Controls.Add(this.pnlGraph);
            this.Controls.Add(this.lblCenterFrequency);
            this.Controls.Add(this.lblMaxFrequency);
            this.Controls.Add(this.lblMinFrequency);
            this.Name = "TunerChannelDisplay";
            this.Size = new System.Drawing.Size(339, 56);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMinFrequency;
        private System.Windows.Forms.Label lblMaxFrequency;
        private System.Windows.Forms.Label lblCenterFrequency;
        private System.Windows.Forms.Panel pnlGraph;
        private System.Windows.Forms.Label lblCurrentFrequency;
    }
}
