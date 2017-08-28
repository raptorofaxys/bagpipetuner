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
            this.SuspendLayout();
            // 
            // spSerial
            // 
            this.spSerial.BaudRate = 115200;
            this.spSerial.DtrEnable = true;
            this.spSerial.PortName = "COM10";
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
            this.pnlSamples.Size = new System.Drawing.Size(543, 334);
            this.pnlSamples.TabIndex = 1;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(483, 4);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Full Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(402, 4);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 379);
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
    }
}