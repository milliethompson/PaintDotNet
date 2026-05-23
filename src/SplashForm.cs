using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for SplashForm.
    /// </summary>
    public class SplashForm 
        : PdnBaseForm
    {
        private System.Windows.Forms.PictureBox logoPicture;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Panel panel;
        private System.ComponentModel.IContainer components = null;

        public SplashForm()
        {
            Utility.TraceMe();
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Initialize logo
            Utility.TraceMe("loading logo");
            logoPicture.Image = Utility.GetImageResource("PaintDotNetLogo.png");
            Utility.TraceMe("done loading logo");

            // Fill in the status label
            statusLabel.Text = "Starting ...";

            // Fill in the copyright label
            copyrightLabel.Text = PdnInfo.GetCopyrightString();

            // Figure out the sizes
            Size padding = new Size(panel.Size.Width - panel.ClientSize.Width, 
                panel.Size.Height - panel.ClientSize.Height);

            this.panel.ClientSize = new Size(logoPicture.Image.Width, 
                logoPicture.Height + statusLabel.Height + copyrightLabel.Height);

            this.Size = this.panel.ClientSize + padding;

            Utility.TraceMe("done with ctor");
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel = new System.Windows.Forms.Panel();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.logoPicture = new System.Windows.Forms.PictureBox();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Controls.Add(this.copyrightLabel);
            this.panel.Controls.Add(this.statusLabel);
            this.panel.Controls.Add(this.logoPicture);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(292, 128);
            this.panel.TabIndex = 3;
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.BackColor = System.Drawing.Color.White;
            this.copyrightLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.copyrightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.copyrightLabel.Location = new System.Drawing.Point(0, 64);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(290, 32);
            this.copyrightLabel.TabIndex = 3;
            this.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.Color.White;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(0, 50);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(290, 14);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Starting ...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // logoPicture
            // 
            this.logoPicture.Dock = System.Windows.Forms.DockStyle.Top;
            this.logoPicture.Location = new System.Drawing.Point(0, 0);
            this.logoPicture.Name = "logoPicture";
            this.logoPicture.Size = new System.Drawing.Size(290, 50);
            this.logoPicture.TabIndex = 1;
            this.logoPicture.TabStop = false;
            // 
            // SplashForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 128);
            this.ControlBox = false;
            this.Controls.Add(this.panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            this.TopMost = true;
            this.Controls.SetChildIndex(this.panel, 0);
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
