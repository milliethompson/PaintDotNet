using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for AboutDialog.
	/// </summary>
	public class AboutDialog 
        : PdnBaseForm
	{
        private System.Windows.Forms.PictureBox logoBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox richCreditsBox;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.TextBox versionLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            this.Text = "About " + Application.ProductName;
            this.logoBox.Image = Utility.GetImageResource("PaintDotNetLogo.bmp");
            this.logoBox.Size = logoBox.Image.Size;
            this.SetClientSizeCore (logoBox.Image.Width, ClientRectangle.Height);

            this.versionLabel.Text = Utility.GetFullAppName();
            
            this.richCreditsBox.LoadFile(Utility.GetResourceStream("AboutCredits.rtf"), RichTextBoxStreamType.RichText);

            copyrightLabel.Text = Utility.GetCopyrightString();
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
            this.okButton = new System.Windows.Forms.Button();
            this.logoBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.richCreditsBox = new System.Windows.Forms.RichTextBox();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(144, 280);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            // 
            // logoBox
            // 
            this.logoBox.Location = new System.Drawing.Point(0, 0);
            this.logoBox.Name = "logoBox";
            this.logoBox.TabIndex = 3;
            this.logoBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Credits:";
            // 
            // richCreditsBox
            // 
            this.richCreditsBox.CausesValidation = false;
            this.richCreditsBox.Location = new System.Drawing.Point(10, 116);
            this.richCreditsBox.Name = "richCreditsBox";
            this.richCreditsBox.ReadOnly = true;
            this.richCreditsBox.Size = new System.Drawing.Size(331, 152);
            this.richCreditsBox.TabIndex = 7;
            this.richCreditsBox.Text = "";
            this.richCreditsBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richCreditsBox_LinkClicked);
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.copyrightLabel.Location = new System.Drawing.Point(8, 78);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(336, 16);
            this.copyrightLabel.TabIndex = 9;
            this.copyrightLabel.Text = "label2";
            // 
            // versionLabel
            // 
            this.versionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.versionLabel.Location = new System.Drawing.Point(8, 61);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.ReadOnly = true;
            this.versionLabel.Size = new System.Drawing.Size(328, 13);
            this.versionLabel.TabIndex = 10;
            this.versionLabel.Text = "textBox1";
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.okButton;
            this.ClientSize = new System.Drawing.Size(360, 309);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.richCreditsBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logoBox);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutDialog";
            this.ResumeLayout(false);

        }
		#endregion

        private void richCreditsBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (null != e.LinkText && e.LinkText.StartsWith("http://"))
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
        }
	}
}
