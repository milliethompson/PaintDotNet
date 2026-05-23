using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for UpdatesDialog.
    /// </summary>
    public class UpdatesDialog 
        : PdnBaseForm
    {
        private System.Windows.Forms.Label betaWarningLabel;
        private System.Windows.Forms.Label newVersionName;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label currentVersionName;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private PdnVersionManifest pdnVersionManifest;
        private PaintDotNet.HeaderLabel currentVersionHeader;
        private PaintDotNet.HeaderLabel newVersionHeader;
        private System.Windows.Forms.LinkLabel newVersionInfoLink;
        private System.Windows.Forms.Button installButton;

        [Browsable(false)]
        public PdnVersionManifest PdnVersionManifest
        {
            get
            {
                return this.pdnVersionManifest;
            }

            set
            {
                this.pdnVersionManifest = value;
                UpdateDynamicText();
            }
        }

        private int pdnVersionManifestIndex;

        [Browsable(false)]
        public int PdnVersionManifestIndex
        {
            get
            {
                return this.pdnVersionManifestIndex;
            }

            set
            {
                this.pdnVersionManifestIndex = value;
                UpdateDynamicText();
            }
        }

        private void UpdateDynamicText()
        {
            if (this.pdnVersionManifest != null)
            {
                PdnVersionInfo newVersionInfo = this.pdnVersionManifest.VersionInfos[this.pdnVersionManifestIndex];
                this.newVersionName.Text = newVersionInfo.FriendlyName;
                this.betaWarningLabel.Visible = !newVersionInfo.IsFinal;
            }
        }

        public UpdatesDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Image iconImage = PdnResources.GetImage("Icons.MenuFileUpdatesIcon.png");
            this.Icon = Utility.ImageToIcon(iconImage, Utility.TransparentKey, true);
            this.installButton.Select();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString("UpdatesDialog.Text");
            this.infoLabel.Text = PdnResources.GetString("UpdatesDialog.InfoLabel.Text");
            this.currentVersionHeader.Text = PdnResources.GetString("UpdatesDialog.CurrentVersionHeader.Text");
            this.newVersionHeader.Text = PdnResources.GetString("UpdatesDialog.NewVersionHeader.Text");
            this.newVersionInfoLink.Text = PdnResources.GetString("UpdatesDialog.NewVersionInfoLink.Text");
            this.betaWarningLabel.Text = PdnResources.GetString("UpdatesDialog.BetaWarningLabel.Text");

            this.installButton.Text = PdnResources.GetString("UpdatesDialog.InstallButton.Text");
            this.closeButton.Text = PdnResources.GetString("UpdatesDialog.CloseButton.Text");

            this.currentVersionName.Text = PdnInfo.GetAppName();
            base.LoadResources();
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.betaWarningLabel = new System.Windows.Forms.Label();
            this.newVersionName = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.currentVersionName = new System.Windows.Forms.Label();
            this.currentVersionHeader = new PaintDotNet.HeaderLabel();
            this.newVersionHeader = new PaintDotNet.HeaderLabel();
            this.newVersionInfoLink = new System.Windows.Forms.LinkLabel();
            this.installButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // betaWarningLabel
            // 
            this.betaWarningLabel.Location = new System.Drawing.Point(16, 125);
            this.betaWarningLabel.Name = "betaWarningLabel";
            this.betaWarningLabel.Size = new System.Drawing.Size(392, 16);
            this.betaWarningLabel.TabIndex = 5;
            this.betaWarningLabel.Text = "betaWarningLabel";
            // 
            // newVersionName
            // 
            this.newVersionName.Location = new System.Drawing.Point(16, 110);
            this.newVersionName.Name = "newVersionName";
            this.newVersionName.Size = new System.Drawing.Size(392, 16);
            this.newVersionName.TabIndex = 4;
            this.newVersionName.Text = "newVersionName";
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(212, 163);
            this.closeButton.Name = "closeButton";
            this.closeButton.TabIndex = 8;
            this.closeButton.Text = "close";
            // 
            // infoLabel
            // 
            this.infoLabel.Location = new System.Drawing.Point(8, 8);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(400, 38);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "label1";
            // 
            // currentVersionName
            // 
            this.currentVersionName.Location = new System.Drawing.Point(16, 69);
            this.currentVersionName.Name = "currentVersionName";
            this.currentVersionName.Size = new System.Drawing.Size(392, 23);
            this.currentVersionName.TabIndex = 2;
            this.currentVersionName.Text = "label1";
            // 
            // currentVersionHeader
            // 
            this.currentVersionHeader.Location = new System.Drawing.Point(8, 52);
            this.currentVersionHeader.Name = "currentVersionHeader";
            this.currentVersionHeader.Size = new System.Drawing.Size(408, 14);
            this.currentVersionHeader.TabIndex = 1;
            this.currentVersionHeader.TabStop = false;
            this.currentVersionHeader.Text = "currentVersionHeader                                              ";
            // 
            // newVersionHeader
            // 
            this.newVersionHeader.Location = new System.Drawing.Point(8, 93);
            this.newVersionHeader.Name = "newVersionHeader";
            this.newVersionHeader.Size = new System.Drawing.Size(408, 14);
            this.newVersionHeader.TabIndex = 3;
            this.newVersionHeader.TabStop = false;
            this.newVersionHeader.Text = "headerLabel1                                              ";
            // 
            // newVersionInfoLink
            // 
            this.newVersionInfoLink.Location = new System.Drawing.Point(16, 140);
            this.newVersionInfoLink.Name = "newVersionInfoLink";
            this.newVersionInfoLink.Size = new System.Drawing.Size(152, 16);
            this.newVersionInfoLink.TabIndex = 6;
            this.newVersionInfoLink.TabStop = true;
            this.newVersionInfoLink.Text = "linkLabel1";
            this.newVersionInfoLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.newVersionZipLink_LinkClicked);
            // 
            // installButton
            // 
            this.installButton.Location = new System.Drawing.Point(131, 163);
            this.installButton.Name = "installButton";
            this.installButton.TabIndex = 7;
            this.installButton.Text = "button1";
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // UpdatesDialog
            // 
            this.AcceptButton = this.installButton;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(418, 192);
            this.Controls.Add(this.betaWarningLabel);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.newVersionInfoLink);
            this.Controls.Add(this.newVersionHeader);
            this.Controls.Add(this.currentVersionHeader);
            this.Controls.Add(this.currentVersionName);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.newVersionName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UpdatesDialog";
            this.Controls.SetChildIndex(this.newVersionName, 0);
            this.Controls.SetChildIndex(this.closeButton, 0);
            this.Controls.SetChildIndex(this.infoLabel, 0);
            this.Controls.SetChildIndex(this.currentVersionName, 0);
            this.Controls.SetChildIndex(this.currentVersionHeader, 0);
            this.Controls.SetChildIndex(this.newVersionHeader, 0);
            this.Controls.SetChildIndex(this.newVersionInfoLink, 0);
            this.Controls.SetChildIndex(this.installButton, 0);
            this.Controls.SetChildIndex(this.betaWarningLabel, 0);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (this.pdnVersionManifest.VersionInfos[this.pdnVersionManifestIndex].IsFinal)
            {
                foreach (Control control in this.Controls)
                {
                    if (control.Top > this.betaWarningLabel.Top)
                    {
                        control.Location = new Point(control.Left, control.Top - this.betaWarningLabel.Height);
                    }
                }

                this.Height -= this.betaWarningLabel.Height;
            }

            base.OnLoad (e);
        }

        private void newVersionZipLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = (LinkLabel)sender;

            string linkText = linkLabel.Text;

            if (sender == this.newVersionInfoLink)
            {
                linkText = this.pdnVersionManifest.VersionInfos[this.pdnVersionManifestIndex].InfoUrl;
            }
            
            if (null != linkText && (linkText.StartsWith("http://") || linkText.StartsWith("https://") || linkText.StartsWith("ftp://")))
            {
                SystemLayer.Shell.OpenUrl(this, linkText);
            }

            e.Link.Visited = true;
        }

        private void installButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}
