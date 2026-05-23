/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet.Setup
{
    /// <summary>
    /// Summary description for LicensePage.
    /// </summary>
    public class LicensePage 
        : WizardPage
    {
        private static bool choseButton = false;
        private static bool choseAgreeButton = false;

        private System.Windows.Forms.Label introText;
        private System.Windows.Forms.TextBox licenseAgreementText;
        private System.Windows.Forms.RadioButton dontAgreeButton;
        private System.Windows.Forms.RadioButton agreeButton;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public LicensePage()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.introText.Text = PdnResources.GetString("SetupWizard.LicensePage.IntroText.Text");
            this.agreeButton.Text = PdnResources.GetString("SetupWizard.LicensePage.AgreeButton.Text");
            this.dontAgreeButton.Text = PdnResources.GetString("SetupWizard.LicensePage.DontAgreeButton.Text");

            Stream licenseStream = PdnResources.GetResourceStream("Files.License.txt");
            StreamReader reader = new StreamReader(licenseStream);
            string licenseText = reader.ReadToEnd();
            licenseText = licenseText.Replace("\n", "\r\n");
            this.licenseAgreementText.Text = licenseText;

            if (choseButton)
            {
                this.agreeButton.Checked = choseAgreeButton;
                this.dontAgreeButton.Checked = !choseAgreeButton;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (WizardHost != null)
            {
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.LicensePage.HeaderText");
                this.agreeButton.Font = WizardHost.NormalTextFont;
                this.introText.Font = WizardHost.NormalTextFont;
                this.dontAgreeButton.Font = WizardHost.NormalTextFont;
                this.licenseAgreementText.Font = WizardHost.FixedWidthFont;
                WizardHost.SetNextEnabled(agreeButton.Checked);
            }

            base.OnLoad(e);
        }

        public override void OnNextClicked()
        {
            choseButton = true;
            choseAgreeButton = agreeButton.Checked;

            if (IntroPage.UserChoseQuickSetup)
            {
                WizardHost.GoToPage(typeof(InstallingPage));
            }
            else
            {
                WizardHost.GoToPage(typeof(OptionsPage));
            }

            base.OnNextClicked ();
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.introText = new System.Windows.Forms.Label();
            this.licenseAgreementText = new System.Windows.Forms.TextBox();
            this.dontAgreeButton = new System.Windows.Forms.RadioButton();
            this.agreeButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // introText
            // 
            this.introText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.introText.Location = new System.Drawing.Point(12, 6);
            this.introText.Name = "introText";
            this.introText.Size = new System.Drawing.Size(464, 38);
            this.introText.TabIndex = 1;
            this.introText.Text = "introText";
            // 
            // licenseAgreementText
            // 
            this.licenseAgreementText.BackColor = System.Drawing.Color.White;
            this.licenseAgreementText.Location = new System.Drawing.Point(12, 44);
            this.licenseAgreementText.Multiline = true;
            this.licenseAgreementText.Name = "licenseAgreementText";
            this.licenseAgreementText.ReadOnly = true;
            this.licenseAgreementText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.licenseAgreementText.Size = new System.Drawing.Size(472, 184);
            this.licenseAgreementText.TabIndex = 2;
            this.licenseAgreementText.Text = "";
            // 
            // dontAgreeButton
            // 
            this.dontAgreeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.dontAgreeButton.Location = new System.Drawing.Point(12, 234);
            this.dontAgreeButton.Name = "dontAgreeButton";
            this.dontAgreeButton.Size = new System.Drawing.Size(148, 24);
            this.dontAgreeButton.TabIndex = 3;
            this.dontAgreeButton.Text = "radioButton1";
            // 
            // agreeButton
            // 
            this.agreeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.agreeButton.Location = new System.Drawing.Point(172, 234);
            this.agreeButton.Name = "agreeButton";
            this.agreeButton.Size = new System.Drawing.Size(136, 24);
            this.agreeButton.TabIndex = 4;
            this.agreeButton.Text = "radioButton1";
            this.agreeButton.CheckedChanged += new System.EventHandler(this.agreeButton_CheckedChanged);
            // 
            // LicensePage
            // 
            this.Controls.Add(this.agreeButton);
            this.Controls.Add(this.dontAgreeButton);
            this.Controls.Add(this.licenseAgreementText);
            this.Controls.Add(this.introText);
            this.Name = "LicensePage";
            this.ResumeLayout(false);

        }
        #endregion

        private void agreeButton_CheckedChanged(object sender, System.EventArgs e)
        {
            if (WizardHost != null)
            {
                WizardHost.SetNextEnabled(agreeButton.Checked);
            }
        }
    }
}
