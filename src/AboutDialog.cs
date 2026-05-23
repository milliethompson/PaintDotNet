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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.LinkLabel linkLabel;

        public AboutDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.richCreditsBox.BackColor = SystemColors.Window;

            string textFormat = PdnResources.GetString("AboutDialog.Text.Format");
            this.Text = string.Format(textFormat, PdnInfo.GetBareProductName());

            Image logo = PdnResources.GetImage("Images.TransparentLogo.png");
            Image gradient = PdnResources.GetImage("Images.BannerGradient.png");
            Bitmap bitmap = new Bitmap(350, 71);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);

                Rectangle gradientSrcRect = new Rectangle(0, 0, gradient.Width, gradient.Height);

                const int gradientScrunch = 125;
                const int gradientOffset = 50;

                Rectangle gradientDstRect = new Rectangle(gradientOffset + gradientScrunch + bitmap.Width - gradient.Width, 0, 
                    gradient.Width - gradientScrunch, gradient.Height);

                gradientDstRect.Inflate(1, 1);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(gradient, gradientDstRect, gradientSrcRect, GraphicsUnit.Pixel);

                Rectangle logoRect = new Rectangle(0, 0, logo.Width, logo.Height);
                g.DrawImage(logo, logoRect, logoRect, GraphicsUnit.Pixel);

            }

            int newWidth = this.ClientSize.Width;
            int newHeight = (bitmap.Height * newWidth) / bitmap.Width;
            this.logoBox.Size = new Size(newWidth, newHeight);
            this.logoBox.SizeMode = PictureBoxSizeMode.CenterImage;
            this.logoBox.BackColor = Color.White;

            Bitmap useThis;

            if (this.logoBox.Size == bitmap.Size)
            {
                useThis = bitmap;
            }
            else
            {
                Bitmap highQuality = new Bitmap(this.logoBox.Width, this.logoBox.Height,
                    PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(highQuality))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(
                        bitmap,
                        new Rectangle(0, 0, logoBox.Width, logoBox.Height),
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        GraphicsUnit.Pixel);
                }

                useThis = highQuality;
            }

            this.logoBox.Image = useThis;

            this.versionLabel.Text = PdnInfo.GetFriendlyVersionString();

            try
            {
                this.versionLabel.Font = new Font("Verdana", 8.0f);
            }

            catch (Exception)
            {
                this.versionLabel.Font = new Font(FontFamily.GenericSansSerif, 8.0f);
            }

            this.linkLabel.Text = PdnResources.GetString("AboutDialog.WebSiteLink.Text");
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
            this.versionLabel = new System.Windows.Forms.Label();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
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
            this.logoBox.Location = new System.Drawing.Point(0, 0);
            this.logoBox.Name = "logoBox";
            this.logoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            this.logoBox.TabIndex = 1;
            this.logoBox.TabStop = false;
            this.logoBox.Controls.Add(this.versionLabel);
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
            // 
            // versionLabel
            // 
            this.versionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.versionLabel.Location = new System.Drawing.Point(53, 47);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(239, 50);
            this.versionLabel.TabIndex = 2;
            this.versionLabel.BackColor = Color.Transparent;
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
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.okButton;
            this.ClientSize = new System.Drawing.Size(350, 375);
            this.Controls.Add(this.linkLabel);
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
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.logoBox, 0);
            this.Controls.SetChildIndex(this.creditsLabel, 0);
            this.Controls.SetChildIndex(this.richCreditsBox, 0);
            this.Controls.SetChildIndex(this.copyrightLabel, 0);
            this.Controls.SetChildIndex(this.linkLabel, 0);
            this.ResumeLayout(false);

        }
        #endregion

        private void richCreditsBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            if (null != e.LinkText && e.LinkText.StartsWith("http://"))
            {
                SystemLayer.Shell.OpenUrl(this, e.LinkText);
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
