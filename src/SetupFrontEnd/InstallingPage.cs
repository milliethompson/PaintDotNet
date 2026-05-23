/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PaintDotNet.Setup
{
    /// <summary>
    /// Summary description for InstallingPage.
    /// </summary>
    public class InstallingPage 
        : WizardPage
    {
        private const string msiName = "PaintDotNet.msi";
        private const string stagingDirName = "Staging";
        private string appName;
        private System.Windows.Forms.Label infoText;
        private ProgressBar progressBar;

        private string installingText;
        private string uninstallingText;
        private string optimizingText;
        private Label errorLabel;

        private bool adLoaded = false;
        private Label adLabel;
        private PictureBox adBox;
        private const string adClickUrl = "http://www.getpaint.net/redirect/donate_setup.html";
        private readonly DateTime adExpireDate = DateTime.MaxValue;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private void LoadAd()
        {
            if (!this.adLoaded)
            {
                this.adLoaded = true;

                this.adLabel.Text = "";
                this.adLabel.Font = new Font(this.adLabel.Font.FontFamily, this.adLabel.Font.Size - 1);
                Assembly ourAssembly = Assembly.GetExecutingAssembly();
                Stream adStream = ourAssembly.GetManifestResourceStream("PaintDotNet.Setup.DonateAd_en.png");
                Image adImage = Image.FromStream(adStream);
                this.adBox.SizeMode = PictureBoxSizeMode.StretchImage;
                this.adBox.Image = adImage;
                this.adBox.Click += new EventHandler(adBox_Click);
            }
        }

        private bool ShouldShowAd
        {
            get
            {
                return true;

                /*
                if (DateTime.Now < adExpireDate &&
                    string.Compare(CultureInfo.CurrentUICulture.Name, "en-US", StringComparison.InvariantCultureIgnoreCase) == 0 && 
                    string.Compare(RegionInfo.CurrentRegion.Name, "US", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
#if DEBUG
                    return true;
#else
                    string fileName = Assembly.GetExecutingAssembly().Location;
                    bool validSig;

                    try
                    {
                        validSig = SystemLayer.Security.VerifySignedFile(this, fileName, false, false);
                    }

                    catch
                    {
                        validSig = false;
                    }

                    return validSig;
#endif
                }
                else
                {
                    return false;
                }
                 * */
            }
        }

        private void ShowAd()
        {
            if (ShouldShowAd)
            {
                this.adBox.Size = new Size(WizardHost.ScaleX(this.adBox.Image.Width), WizardHost.ScaleY(this.adBox.Image.Height));
                this.adBox.Location = new Point((this.ClientSize.Width - this.adBox.Width) / 2, this.ClientSize.Height - this.adBox.Height - WizardHost.ScaleY(5));

                this.adLabel.Location = new Point((this.ClientSize.Width - this.adLabel.Width) / 2, this.adBox.Top - this.adLabel.Height - WizardHost.ScaleY(2));
                this.adLabel.Visible = true;

                this.adBox.Enabled = true;
                this.adBox.Visible = true;
            }
        }

        private void HideAd()
        {
            this.adLabel.Visible = false;
            this.adBox.Enabled = false;
            this.adBox.Visible = false;
        }

        private void adBox_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.adBox.Cursor;
            this.adBox.Cursor = Cursors.AppStarting;
            SystemLayer.Shell.OpenUrl(this, adClickUrl);
            System.Threading.Thread.Sleep(250);
            this.adBox.Cursor = oldCursor;
        }

        public InstallingPage()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            string introFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Installing.Format");
            this.appName = PdnInfo.GetProductName();
            this.installingText = string.Format(introFormat, appName);

            this.uninstallingText = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Uninstalling");
            this.optimizingText = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Optimizing");
        }

        private string GetOriginalMsiName(string msiPath)
        {
            Random random = new Random();
            
            string dirName = Path.GetDirectoryName(msiPath);
            string fileName = Path.GetFileNameWithoutExtension(msiPath);
            string ext = Path.GetExtension(msiPath);

            while (true)
            {
                int salt = random.Next();
                string newFileName = Path.Combine(dirName, Path.ChangeExtension(fileName + "_" + salt, ext));

                FileInfo info = new FileInfo(newFileName);

                if (!info.Exists)
                {
                    return newFileName;
                }
            }
        }

        private void DoInstallation()
        {
            WizardHost.SetNextEnabled(false);
            WizardHost.SetBackEnabled(false);
            WizardHost.SetCancelEnabled(false);

            IntPtr hWnd = this.Handle;

            uint result = NativeMethods.MsiSetInternalUI(
                NativeConstants.INSTALLUILEVEL_BASIC | NativeConstants.INSTALLUILEVEL_HIDECANCEL, 
                ref hWnd);

            // value of result is discarded

            string ourDir = Path.GetDirectoryName(Application.ExecutablePath);
            string originalPackagePath = Path.Combine(ourDir, msiName);
            string targetDir = WizardHost.GetMsiProperty(PropertyNames.TargetDir, null);
            string stagingDir = Path.Combine(targetDir, stagingDirName);

            // The 'old' target dir is read from the registry.
            // This way if they are reinstalling to a new directory, we will propertly uninstall and cleanup
            // from the old directory.
            // The 'old' target dir defaults to the 'new' target dir (in case of new installation)
            string oldTargetDir = WizardHost.GetMsiProperty(PropertyNames.TargetDir, targetDir, true);
            string oldStagingDir = Path.Combine(oldTargetDir, stagingDirName);

            // Uninstallers should skip certain parts of cleanup when we're going to turn around
            // and install a newer version right away
            WizardHost.SetMsiProperty(PropertyNames.SkipCleanup, "0");

            // Uninstall anything already in the staging directory (should only be the previous version)
            if (Directory.Exists(oldStagingDir))
            {
                this.infoText.Text = this.uninstallingText;
                WizardHost.SetMsiProperty(PropertyNames.SkipCleanup, "1");

                foreach (string filePath in Directory.GetFiles(oldStagingDir, "*.msi"))
                {
                    NativeMethods.MsiInstallProduct(
                        filePath, 
                        "REMOVE=ALL " + 
                        PropertyNames.SkipCleanup + "=1 " + 
                        PropertyNames.DesktopShortcut + "=" + WizardHost.GetMsiProperty(PropertyNames.DesktopShortcut, "1"));
                }
            }

            // Proceed with installation
            this.infoText.Text = this.installingText;

            Directory.CreateDirectory(stagingDir);
            string msiPath = Path.Combine(stagingDir, msiName);
            string dstPackagePath = GetOriginalMsiName(msiPath);

            // Copy the MSI to the Staging directory before installing. This way it will always
            // be available when Windows Installer needs to refer to it.
            FileInfo info = new FileInfo(originalPackagePath);
            info.CopyTo(dstPackagePath, true);

            // Keep an open file handle so that setupngen.exe cannot delete the file.
            // This happens if the current installation of Paint.NET 

            // We need to set the Target Platform property of the MSI before we install it.
            // This way if the user types "C:\Program Files\Whatever" on an x64 system, it will
            // not get redirected over to "C:\Program Files (x86)\Whatever"
            Msi.SetMsiTargetPlatform(dstPackagePath, PaintDotNet.SystemLayer.Processor.NativeArchitecture);

            string commandLine1 = WizardHost.GetMsiCommandLine();
            string commandLine = commandLine1;
            
            if (commandLine.Length > 0)
            {
                commandLine += " ";
            }

            commandLine += PropertyNames.QueueNgen + "=1";

            // Install newest package
            result = NativeMethods.MsiInstallProduct(dstPackagePath, commandLine);

            if (result == NativeConstants.ERROR_SUCCESS ||
                result == NativeConstants.ERROR_SUCCESS_REBOOT_INITIATED ||
                result == NativeConstants.ERROR_SUCCESS_REBOOT_REQUIRED)
            {
                WizardHost.SaveMsiProperties();

                // clean up staging dir
                string msiFileName = Path.GetFileName(dstPackagePath);
                foreach (string filePath in Directory.GetFiles(stagingDir, "*.msi"))
                {
                    string fileName = Path.GetFileName(filePath);

                    if (0 != string.Compare(msiFileName, fileName, true, CultureInfo.InvariantCulture))
                    {
                        File.Delete(filePath);
                    }
                }

                // Run "ngen.exe executeQueuedItems"
                if (Application.VisualStyleState == VisualStyleState.ClientAreaEnabled ||
                    Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled)
                {
                    this.progressBar.Style = ProgressBarStyle.Marquee;
                    this.progressBar.Visible = true;
                }

                string ngenExe = PdnInfo.GetNgenPath(false);
                const string ngenArg = "executeQueuedItems";

                try
                {
                    ShowAd();
                    this.infoText.Text = this.optimizingText;
                    ProcessStartInfo psi = new ProcessStartInfo(ngenExe, ngenArg);
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    Process process = Process.Start(psi);

                    while (!process.HasExited)
                    {
                        System.Threading.Thread.Sleep(10);
                        Application.DoEvents();
                    }
                }

                catch
                {
                    // If this fails, do not fail the installation
                }

                WizardHost.SetFinished(true);
                this.progressBar.Visible = false;

                // set text to indicate success
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.InstallingPage.HeaderText.Success");
                string infoFormat;
                    
                if (result == NativeConstants.ERROR_SUCCESS)
                {
                    WizardHost.RebootRequired = false;
                    infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Success.Format");
                }
                else
                {
                    WizardHost.RebootRequired = true;
                    infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Success.RebootRequired.Format");
                }

                this.infoText.Text = string.Format(infoFormat, this.appName);
                WizardHost.SetBackEnabled(false);
            }
            else
            {
                HideAd();

                WizardHost.SetFinished(true);
                this.progressBar.Visible = false;

                // set text to indicate failure
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.InstallingPage.HeaderText.Failure");
                string infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Failure.Format");
                string errorString = NativeMethods.FormatMessageW(result);
                this.errorLabel.Font = WizardHost.NormalTextFont;
                this.errorLabel.Visible = true;
                this.errorLabel.Text = errorString;
                this.infoText.Text = string.Format(infoFormat, this.appName) + " (" + result.ToString() + ")";
                WizardHost.SetBackEnabled(false);
            }
        }

        private delegate void VoidVoidDelegate();

        protected override void OnLoad(EventArgs e)
        {
            if (WizardHost != null)
            {
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.InstallingPage.HeaderText.Installing");
                this.infoText.Font = WizardHost.NormalTextFont;
                this.infoText.ForeColor = WizardHost.TextColor;
                this.errorLabel.Font = WizardHost.NormalTextFont;
                this.errorLabel.ForeColor = WizardHost.TextColor;
                this.BeginInvoke(new VoidVoidDelegate(this.DoInstallation), null);
            }

            base.OnLoad(e);
        }

        public override void OnNextClicked()
        {
            WizardHost.Close();
            base.OnNextClicked();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }

                if (this.adBox.Image != null)
                {
                    Image adImage = this.adBox.Image;
                    this.adBox.Image = null;
                    adImage.Dispose();
                    adImage = null;
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
            this.infoText = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.errorLabel = new System.Windows.Forms.Label();
            this.adBox = new PictureBox();
            this.adLabel = new Label();
            this.SuspendLayout();
            // 
            // infoText
            // 
            this.infoText.Location = new System.Drawing.Point(12, 6);
            this.infoText.Name = "infoText";
            this.infoText.Size = new System.Drawing.Size(468, 44);
            this.infoText.TabIndex = 0;
            this.infoText.Text = "label1";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(42, 39);
            this.progressBar.MarqueeAnimationSpeed = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(408, 19);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // errorLabel
            // 
            this.errorLabel.Location = new System.Drawing.Point(12, 50);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(468, 200);
            this.errorLabel.TabIndex = 2;
            this.errorLabel.Text = "label1";
            this.errorLabel.Visible = false;
            //
            // adBox
            //
            this.adBox.Name = "adBox";
            this.adBox.Enabled = false;
            this.adBox.Visible = false;
            this.adBox.Cursor = Cursors.Hand;
            //
            // adLabel
            //
            this.adLabel.Name = "adLabel";
            this.adLabel.AutoSize = true;
            this.adLabel.Enabled = false;
            this.adLabel.Visible = false;
            //
            // ad
            //
            LoadAd();
            // 
            // InstallingPage
            // 
            this.Controls.Add(this.adLabel);
            this.Controls.Add(this.adBox);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.infoText);
            this.Controls.Add(this.errorLabel);
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Name = "InstallingPage";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
