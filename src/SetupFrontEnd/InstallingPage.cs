/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

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
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public InstallingPage()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            string introFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Installing.Format");
            this.appName = PdnResources.GetString("Application.ProductName.WithTag");
            this.infoText.Text = string.Format(introFormat, appName);
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
            NativeMethods.MsiSetInternalUI(NativeConstants.INSTALLUILEVEL_BASIC | NativeConstants.INSTALLUILEVEL_HIDECANCEL, ref hWnd);

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

            // Some of the logic changed with 2.5 Beta 3. Before that we always recreated the desktop
            // and Programs shortcut. Updating from Beta 2 to Beta 3+ needs to be intelligent so that
            // we always create these two shortcuts. Otherwise Beta 2- will uninstall and delete these
            // shortcuts, and then the Beta 3+ installer will not create them again.
            // So, if we do not see the Pdn25Beta3Plus property set to "1", then we force PdnUpdating
            // and SkipCleanup to "0".
            // ... And then there was a bug in SetupNgen for 2.5 Beta 3 that made it delete the
            // desktop shortcut anyway. But it's fixed now in Beta 4. Which is why things refer to
            // Pdn25Beta4Plus and not Pdn25Beta3Plus.
            string pdn25Beta4PlusStr = WizardHost.GetMsiProperty(PropertyNames.Pdn25Beta4Plus, "0");
            bool pdn25Beta4Plus = (pdn25Beta4PlusStr == "1");

            // Uninstallers should skip certain parts of cleanup when we're going to turn around
            // and install a newer version right away
            WizardHost.SetMsiProperty(PropertyNames.SkipCleanup, "0");

            // Uninstall anything already in the staging directory (should only be the previous version)
            if (Directory.Exists(oldStagingDir))
            {
                WizardHost.SetMsiProperty(PropertyNames.SkipCleanup, "1");

                foreach (string filePath in Directory.GetFiles(oldStagingDir, "*.msi"))
                {
                    NativeMethods.MsiInstallProduct(filePath, "REMOVE=ALL " + 
                        PropertyNames.SkipCleanup + "=1 " + 
                        PropertyNames.DesktopShortcut + "=" + WizardHost.GetMsiProperty(PropertyNames.DesktopShortcut, "1"));
                }
            }

            // If we're not upgrading from 2.5 Beta 3+ to another 2.5 Beta 3+ build, then
            // we do not want to skip creation of the desktop and Programs icons.
            if (!pdn25Beta4Plus)
            {
                WizardHost.SetMsiProperty(PropertyNames.PdnUpdating, "0");
                WizardHost.SetMsiProperty(PropertyNames.SkipCleanup, "0");
            }

            // Proceed with installation
            Directory.CreateDirectory(stagingDir);
            string msiPath = Path.Combine(stagingDir, msiName);
            string dstPackagePath = GetOriginalMsiName(msiPath);

            FileInfo info = new FileInfo(originalPackagePath);
            info.CopyTo(dstPackagePath, true);

            string commandLine1 = WizardHost.GetMsiCommandLine();
            string commandLine = commandLine1;
            
            if (commandLine.Length > 0)
            {
                commandLine += " ";
            }
            
            commandLine += "FRONTEND=1";

            // Install newest package
            uint result = NativeMethods.MsiInstallProduct(dstPackagePath, commandLine);

            WizardHost.SetFinished(true);

            if (result == NativeConstants.ERROR_SUCCESS ||
                result == NativeConstants.ERROR_SUCCESS_REBOOT_INITIATED ||
                result == NativeConstants.ERROR_SUCCESS_REBOOT_REQUIRED)
            {
                WizardHost.SetMsiProperty(PropertyNames.Pdn25Beta4Plus, "1");
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

                // set text to indicate success
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.InstallingPage.HeaderText.Success");
                string infoFormat;
                    
                if (result != NativeConstants.ERROR_SUCCESS)
                {
                    infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Success.RebootRequired.Format");
                }
                else
                {
                    infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Success.Format");
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
                // set text to indicate failure
                WizardHost.HeaderText = PdnResources.GetString("SetupWizard.InstallingPage.HeaderText.Failure");
                string infoFormat = PdnResources.GetString("SetupWizard.InstallingPage.InfoText.Text.Failure.Format");
                this.infoText.Text = string.Format(infoFormat, this.appName);
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
                this.BeginInvoke(new VoidVoidDelegate(this.DoInstallation), null);
            }

            base.OnLoad(e);
        }

        public override void OnNextClicked()
        {
            WizardHost.Close();
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
            this.infoText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // infoText
            // 
            this.infoText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.infoText.Location = new System.Drawing.Point(12, 6);
            this.infoText.Name = "infoText";
            this.infoText.Size = new System.Drawing.Size(468, 38);
            this.infoText.TabIndex = 0;
            this.infoText.Text = "label1";
            // 
            // InstallingPage
            // 
            this.Controls.Add(this.infoText);
            this.Name = "InstallingPage";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
