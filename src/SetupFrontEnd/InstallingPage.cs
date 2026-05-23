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
using System.Globalization;
using System.IO;
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

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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

                string ngenExe = PdnInfo.GetNgenPath();
                const string ngenArg = "executeQueuedItems";

                try
                {
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

                // "Ping" the last update time. See bug #1458
                try
                {
                    long ticks = DateTime.Now.Ticks;
                    string ticksStr = ticks.ToString();
                    const string regKeyName = @"SOFTWARE\Paint.NET";
                    const string regKeyValue = "LastUpdateCheckTimeTicks";

                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(regKeyName))
                    {
                        if (key != null)
                        {
                            key.SetValue(regKeyValue, ticksStr);
                        }
                    }
                }

                catch
                {
                    // Do not care if it fails.
                }
            }
            else
            {
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
            this.infoText = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.errorLabel = new System.Windows.Forms.Label();
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
            this.progressBar.Location = new System.Drawing.Point(42, 59);
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
            // InstallingPage
            // 
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
