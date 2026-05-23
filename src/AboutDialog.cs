/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
        private System.Windows.Forms.Label creditsLabel;
        private System.Windows.Forms.RichTextBox richCreditsBox;
        private System.Windows.Forms.TextBox copyrightLabel;
        private System.Windows.Forms.TextBox versionLabel;
        private System.Windows.Forms.LinkLabel linkLabel;
        private System.Windows.Forms.Control whiteBackground;

        public AboutDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            string textFormat = PdnResources.GetString("AboutDialog.Text.Format");
            this.Text = string.Format(textFormat, PdnInfo.GetBareProductName());
            this.logoBox.Image = PdnResources.GetImage("Images.Logo.png");
            this.logoBox.Size = logoBox.Image.Size;
            this.logoBox.SizeMode = PictureBoxSizeMode.Normal;
            this.logoBox.BackColor = Color.White;

            this.versionLabel.Text = PdnInfo.GetFriendlyVersionString(); //PdnInfo.GetFullAppName();
            this.versionLabel.Font = new Font("Verdana", 8.0f);
            this.linkLabel.Text = PdnResources.GetString("AboutDialog.WebSiteLink.Text"); //"Go to the Paint.NET website"
            this.richCreditsBox.LoadFile(PdnResources.GetResourceStream("Files.AboutCredits.rtf"), RichTextBoxStreamType.RichText);
            this.copyrightLabel.Text = PdnInfo.GetCopyrightString();

            this.Icon = new Icon(PdnResources.GetResourceStream("Icons.PaintDotNet.ico"));

            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.creditsLabel.Text = PdnResources.GetString("AboutDialog.CreditsLabel.Text");
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
            this.creditsLabel = new System.Windows.Forms.Label();
            this.richCreditsBox = new System.Windows.Forms.RichTextBox();
            this.copyrightLabel = new System.Windows.Forms.TextBox();
            this.versionLabel = new System.Windows.Forms.TextBox();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.whiteBackground = new System.Windows.Forms.Control();
            this.SuspendLayout();
            //
            // whiteBackground
            //
            this.whiteBackground.BackColor = Color.White;
            this.whiteBackground.TabStop = false;
            this.whiteBackground.Dock = DockStyle.Top;
            this.whiteBackground.Size = new Size(350, 73);
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(139, 346);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0;
            // 
            // logoBox
            // 
            this.logoBox.Location = new System.Drawing.Point(45, 0);
            this.logoBox.Name = "logoBox";
            this.logoBox.TabIndex = 1;
            this.logoBox.TabStop = false;
            // 
            // creditsLabel
            // 
            this.creditsLabel.Location = new System.Drawing.Point(7, 136);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new System.Drawing.Size(200, 16);
            this.creditsLabel.TabIndex = 5;
            // 
            // richCreditsBox
            // 
            this.richCreditsBox.CausesValidation = false;
            this.richCreditsBox.Location = new System.Drawing.Point(10, 152);
            this.richCreditsBox.Name = "richCreditsBox";
            this.richCreditsBox.ReadOnly = true;
            this.richCreditsBox.Size = new System.Drawing.Size(331, 188);
            this.richCreditsBox.TabIndex = 6;
            this.richCreditsBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richCreditsBox_LinkClicked);
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.copyrightLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.copyrightLabel.WordWrap = true;
            this.copyrightLabel.Multiline = true;
            this.copyrightLabel.Location = new System.Drawing.Point(9, 76);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.ReadOnly = true;
            this.copyrightLabel.Size = new System.Drawing.Size(336, 36);
            this.copyrightLabel.TabIndex = 4;
            this.copyrightLabel.Text = "";
            // 
            // versionLabel
            // 
            this.versionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.versionLabel.Location = new System.Drawing.Point(101, 47);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.ReadOnly = true;
            this.versionLabel.Size = new System.Drawing.Size(239, 50);
            this.versionLabel.TabIndex = 2;
            this.versionLabel.BackColor = Color.White;
            this.versionLabel.ForeColor = Color.Black;
            // 
            // linkLabel
            // 
            this.linkLabel.Location = new System.Drawing.Point(7, 114);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(337, 18);
            this.linkLabel.TabIndex = 5;
            this.linkLabel.TabStop = true;
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // AboutDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.okButton;
            this.ClientSize = new System.Drawing.Size(350, 375);
            this.Controls.Add(this.whiteBackground);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.richCreditsBox);
            this.Controls.Add(this.creditsLabel);
            this.Controls.Add(this.logoBox);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.whiteBackground, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.logoBox, 0);
            this.Controls.SetChildIndex(this.creditsLabel, 0);
            this.Controls.SetChildIndex(this.richCreditsBox, 0);
            this.Controls.SetChildIndex(this.copyrightLabel, 0);
            this.Controls.SetChildIndex(this.versionLabel, 0);
            this.Controls.SetChildIndex(this.linkLabel, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void richCreditsBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (null != e.LinkText && e.LinkText.StartsWith("http://"))
            {
                try
                {
                    System.Diagnostics.Process.Start(e.LinkText);
                }

                catch
                {
                    string message = PdnResources.GetString("LaunchLink.Error");
                    Utility.ErrorBox(this, message);
                }
            }
        }

        private void linkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string aboutRedirect = PdnResources.GetString("PdnInfo.AboutRedirectPage");
                PdnInfo.LaunchWebSite(aboutRedirect);
                e.Link.Visited = true;
            }

            catch
            {
                string message = PdnResources.GetString("LaunchLink.Error");
                Utility.ErrorBox(this, message);
            }
        }
    }
}
