/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using PaintDotNet;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet.Setup
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class SetupWizard 
        : System.Windows.Forms.Form
    {
        private const string mutexName = "Paint.NET.SetupWizard";
        private const string regSubKey = @"SOFTWARE\Paint.NET";
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.PictureBox headerImage;
        private System.Windows.Forms.Label headingText;
        private System.Windows.Forms.Control headingSpacer;
        private System.ComponentModel.Container components = null;
        private Hashtable msiProperties = new Hashtable();
        private System.Windows.Forms.Label separator1;
        private ComboBox languageBox;
        private System.Windows.Forms.Label separator2;
        private System.Windows.Forms.Control whiteBackground;
        private Stack<System.Type> pages = new Stack<System.Type>();
        private WizardPage wizardPage;
        private bool finished = false;
        private bool skipConfig = false;
        private bool autoMode = false;
        private bool languageInitDone = false;
        private bool rebootRequired = false;
        private float xScale;
        private float yScale;

        public int ScaleX(int x)
        {
            return (int)ScaleX((float)x);
        }

        public float ScaleX(float x)
        {
            return x * xScale;
        }

        public int ScaleY(int y)
        {
            return (int)ScaleY((float)y);
        }

        public float ScaleY(float y)
        {
            return y * yScale;
        }

        public bool RebootRequired
        {
            get
            {
                return this.rebootRequired;
            }

            set
            {
                this.rebootRequired = value;
            }
        }

        public bool SkipConfig
        {
            get
            {
                return this.skipConfig;
            }

            set
            {
                this.skipConfig = value;
            }
        }

        public bool AutoMode
        {
            get
            {
                return this.autoMode;

            }

            set
            {
                this.autoMode = value;
            }
        }

        public string HeaderText
        {
            get
            {
                return this.headingText.Text;
            }

            set
            {
                this.headingText.Text = value;
            }
        }

        public void SetNextEnabled(bool enabled)
        {
            this.nextButton.Enabled = enabled;
        }

        public void SetBackEnabled(bool enabled)
        {
            this.backButton.Enabled = enabled;
        }

        public void SetCancelEnabled(bool enabled)
        {
            this.cancelButton.Enabled = enabled;
        }

        public void SetFinished(bool finished)
        {
            this.finished = finished;

            if (finished)
            {
                SetNextEnabled(true);
                SetBackEnabled(false);
                SetCancelEnabled(false);
                this.AcceptButton = this.nextButton;
                this.nextButton.Text = PdnResources.GetString("SetupWizard.NextButton.Text.Finished");

                if (this.autoMode)
                {
                    Close();
                }
            }
            else
            {
                this.AcceptButton = null;
                this.nextButton.Text = PdnResources.GetString("SetupWizard.NextButton.Text");
            }
        }

        public void SetMsiProperty(string property, string value)
        {
            msiProperties[property] = value;
        }

        public void SaveMsiProperties()
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(regSubKey))
            {
                if (key != null)
                {
                    foreach (string name in msiProperties.Keys)
                    {
                        string value = (string)msiProperties[name];
                        key.SetValue(name, value);
                    }
                }
            }
        }

        public string GetMsiCommandLine()
        {
            string commandLine = string.Empty;

            foreach (string property in this.msiProperties.Keys)
            {
                string value = (string)this.msiProperties[property];
                commandLine += property + "=" + "\"" + value + "\" ";
            }

            // Remove trailing space
            if (commandLine.Length > 1)
            {
                commandLine = commandLine.Substring(0, commandLine.Length - 1);
            }

            return commandLine;
        }

        public void AddPropertyFromArg(string arg)
        {
            int indexOfEq = arg.IndexOf('=');

            if (indexOfEq != -1)
            {
                string property = arg.Substring(0, indexOfEq);
                string value = arg.Substring(indexOfEq + 1, arg.Length - indexOfEq - 1);
                SetMsiProperty(property, value);
            }
        }

        public Hashtable MsiProperties
        {
            get
            {
                return (Hashtable)this.msiProperties.Clone();
            }
        }

        public string GetMsiProperty(string property, string defaultValue)
        {
            return GetMsiProperty(property, defaultValue, false);
        }

        public string GetMsiProperty(string property, string defaultValue, bool forceRegRead)
        {
            string returnVal;

            if (msiProperties.Contains(property) && !forceRegRead)
            {
                returnVal = (string)msiProperties[property];
            }
            else
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regSubKey, false))
                {
                    if (key == null)
                    {
                        returnVal = defaultValue;
                    }
                    else
                    {
                        returnVal = (string)key.GetValue(property, defaultValue);
                    }
                }
            }

            if (defaultValue != null)
            {
                msiProperties[property] = returnVal;
            }

            return returnVal;
        }

        private Font CreateFont(string name, float size, FontStyle style)
        {
            Font returnFont;

            try
            {
                returnFont = new Font(name, size, style);
            }

            catch
            {
                try
                {
                    returnFont = new Font("Arial", size);
                }

                catch (Exception)
                {
                    returnFont = new Font(FontFamily.GenericSansSerif, size);
                }
            }

            return returnFont;
        }

        public Font HeadingTextFont
        {
            get
            {
                return CreateFont("Tahoma", 12.5f, FontStyle.Regular);
            }
        }

        public Font NormalTextFont
        {
            get
            {
                return CreateFont("Verdana", 8.0f, FontStyle.Regular);
            }
        }

        public Font FixedWidthFont
        {
            get
            {
                return CreateFont("Courier New", 9.0f, FontStyle.Regular);
            }
        }

        public Font FootNoteFont
        {
            get
            {
                return CreateFont("Verdana", 6.5f, FontStyle.Regular);
            }
        }

        public Color TextColor
        {
            get
            {
                return Color.Black;
            }
        }

        private void SetPage(Type pageType)
        {
            ConstructorInfo ci = pageType.GetConstructor(System.Type.EmptyTypes);
            object obj = ci.Invoke(new object[0]);
            WizardPage page = (WizardPage)obj;
            page.WizardHost = this;
            this.Controls.Remove(this.wizardPage);
            page.Location = new Point(0, ScaleY(77));
            page.Size = new Size(ClientSize.Width, ScaleY(259));
            SetNextEnabled(true);
            SetBackEnabled(true);
            SetCancelEnabled(true);
            this.wizardPage = page;
            this.Controls.Add(this.wizardPage);
            this.Controls.SetChildIndex(this.wizardPage, 0);

            this.languageBox.Visible = (pageType == typeof(IntroPage));
        }

        public void ClearPageStack()
        {
            this.pages.Clear();
        }

        public void GoToPage(Type type)
        {
            if (this.wizardPage != null)
            {
                this.pages.Push(this.wizardPage.GetType());
            }

            SetPage(type);
        }

        public SetupWizard()
        {
            this.SuspendLayout();

            using (Graphics g = this.CreateGraphics())
            {
                this.xScale = g.DpiX / 96.0f;
                this.yScale = g.DpiY / 96.0f;
            }

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();


            this.ResumeLayout(false);
            LoadResources();
        }

        private string GetProgramFilesDir()
        {
            // Environment.GetFolderPath() has a bug in that it will freak out when %PROGRAMFILES% is set
            // to something like "D:" (note the lack of a backslash).
            string path = NativeMethods.SHGetFolderPath((int)(NativeConstants.CSIDL_PROGRAM_FILES | NativeConstants.CSIDL_FLAG_CREATE));

            if (path.Length == 2 && path[1] == ':')
            {
                path = path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        private string GetCultureInfoName(CultureInfo ci)
        {
            if (ci.Parent.Name == "")
            {
                return ci.NativeName;
            }
            else
            {
                return GetCultureInfoName(ci.Parent);
            }
        }

        private class CultureNameAndInfo
        {
            private CultureInfo cultureInfo;
            private string displayName;

            public CultureInfo CultureInfo
            {
                get
                {
                    return this.cultureInfo;
                }
            }

            public string DisplayName
            {
                get
                {
                    return this.displayName;
                }
            }

            public CultureNameAndInfo(CultureInfo cultureInfo, string displayName)
            {
                this.cultureInfo = cultureInfo;
                this.displayName = displayName;
            }
        }

        private void LoadResources()
        {
            this.Icon = PdnResources.GetIcon("Icons.PaintDotNet.ico");
            this.Text = PdnInfo.GetProductName();
            this.cancelButton.Text = PdnResources.GetString("SetupWizard.CancelButton.Text");
            this.backButton.Text = PdnResources.GetString("SetupWizard.BackButton.Text");
            this.nextButton.Text = PdnResources.GetString("SetupWizard.NextButton.Text");

            Image pdnLogo = PdnResources.GetImage("Images.TransparentLogo.png");
            Image gradient = PdnResources.GetImage("Images.BannerGradient.png");
            Bitmap logoAndGradient = new Bitmap(495, 71, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(logoAndGradient))
            {
                g.Clear(Color.White);
                Rectangle gradientSrcBounds = new Rectangle(new Point(0, 0), gradient.Size);
                Rectangle gradientDstBounds = new Rectangle(new Point(logoAndGradient.Width - gradient.Width, 0), gradient.Size);
                g.DrawImage(gradient, gradientDstBounds, gradientSrcBounds, GraphicsUnit.Pixel);
                Rectangle pdnLogoBounds = new Rectangle(new Point(0, 0), pdnLogo.Size);
                g.DrawImage(pdnLogo, pdnLogoBounds, pdnLogoBounds, GraphicsUnit.Pixel);
            }

            Bitmap useThis;

            if (this.headerImage.Size == logoAndGradient.Size)
            {
                useThis = logoAndGradient;
            }
            else
            {
                Bitmap highQuality = new Bitmap(this.headerImage.Width, this.headerImage.Height,
                    PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(highQuality))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(
                        logoAndGradient,
                        new Rectangle(0, 0, highQuality.Width, highQuality.Height),
                        new Rectangle(0, 0, logoAndGradient.Width, logoAndGradient.Height),
                        GraphicsUnit.Pixel);
                }

                useThis = highQuality;
            }

            this.headerImage.SizeMode = PictureBoxSizeMode.CenterImage;
            this.headerImage.Image = useThis;
            gradient.Dispose();
            pdnLogo.Dispose();
        }

        protected override void OnLoad(EventArgs e)
        {
            // Setup default install dir
            string targetSubDir = PdnResources.GetString("SetupWizard.InstallDirPage.DefaultTargetSubDir");
            string programFilesDir = GetProgramFilesDir();
            string defaultTargetDir = Path.Combine(programFilesDir, targetSubDir);
            string targetDir = GetMsiProperty(PropertyNames.TargetDir, defaultTargetDir);

            // Set other default properties
            string jpgPngBmpEditor = GetMsiProperty(PropertyNames.JpgPngBmpEditor, "1");
            string tgaEditor = GetMsiProperty(PropertyNames.TgaEditor, "1");
            string checkForUpdates = GetMsiProperty(PropertyNames.CheckForUpdates, "1");
            string checkForBetas = GetMsiProperty(PropertyNames.CheckForBetas, PropertyNames.CheckForBetasDefault);

            this.headingText.Font = this.HeadingTextFont;
            this.headingText.ForeColor = this.TextColor;

            if (this.wizardPage == null || this.wizardPage.GetType() == typeof(IntroPage))
            {
                // Populate the language combo box
                string[] locales = PdnResources.GetInstalledLocales();
                CultureNameAndInfo[] cnais = new CultureNameAndInfo[locales.Length];

                for (int i = 0; i < locales.Length; ++i)
                {
                    string locale = locales[i];
                    CultureInfo ci = new CultureInfo(locale);
                    CultureNameAndInfo cnai = new CultureNameAndInfo(ci, GetCultureInfoName(ci));
                    cnais[i] = cnai;
                }

                Array.Sort(
                    cnais,
                    delegate(CultureNameAndInfo lhs, CultureNameAndInfo rhs)
                    {
                        return string.Compare(lhs.DisplayName, rhs.DisplayName,
                            StringComparison.InvariantCultureIgnoreCase);
                    });

                this.languageBox.DataSource = cnais;
                this.languageBox.DisplayMember = "DisplayName";

                // Choose the current locale

                // First find English
                CultureNameAndInfo englishCnai;

                englishCnai = Array.Find(
                    cnais,
                    delegate(CultureNameAndInfo cnai)
                    {
                        if (cnai.CultureInfo.Name.Length == 0 || cnai.CultureInfo.Name == "en-US")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });

                // Next, figure out what culture we're currently set to
                CultureNameAndInfo currentCnai;

                currentCnai = Array.Find(
                    cnais,
                    delegate(CultureNameAndInfo cnai)
                    {
                        return (cnai.CultureInfo == PdnResources.Culture);
                    });

                if (currentCnai == null)
                {
                    this.languageBox.SelectedItem = englishCnai;
                }
                else
                {
                    this.languageBox.SelectedItem = currentCnai;
                }

                this.languageInitDone = true;

                // Go to the appropriate page
                if (this.skipConfig)
                {
                    GoToPage(typeof(InstallingPage));
                }
                else
                {
                    GoToPage(typeof(IntroPage));
                }
            }

            base.OnLoad (e);
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
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.finished)
            {
                string title = PdnInfo.GetProductName();
                string message = PdnResources.GetString("SetupWizard.CancelDialog.Message");
                DialogResult result = MessageBox.Show(this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nextButton = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.separator1 = new System.Windows.Forms.Label();
            this.headerImage = new System.Windows.Forms.PictureBox();
            this.headingText = new System.Windows.Forms.Label();
            this.headingSpacer = new System.Windows.Forms.Control();
            this.separator2 = new System.Windows.Forms.Label();
            this.languageBox = new System.Windows.Forms.ComboBox();
            this.whiteBackground = new System.Windows.Forms.Control();
            ((System.ComponentModel.ISupportInitialize)(this.headerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(412, 350);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 0;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // backButton
            // 
            this.backButton.Location = new System.Drawing.Point(332, 350);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(75, 23);
            this.backButton.TabIndex = 1;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(244, 350);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // headerImage
            // 
            this.headerImage.BackColor = System.Drawing.Color.White;
            this.headerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerImage.Location = new System.Drawing.Point(0, 0);
            this.headerImage.Name = "headerImage";
            this.headerImage.Size = new System.Drawing.Size(495, 71);
            this.headerImage.SizeMode = PictureBoxSizeMode.StretchImage;
            this.headerImage.TabIndex = 0;
            this.headerImage.TabStop = false;
            this.headerImage.Controls.Add(this.headingText);
            // 
            // headingText
            // 
            this.headingText.BackColor = System.Drawing.Color.Transparent;
            this.headingText.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headingText.Location = new System.Drawing.Point(52, 47);
            this.headingText.Name = "headingText";
            this.headingText.Size = new System.Drawing.Size(441, 25);
            this.headingText.TabIndex = 4;
            this.headingText.Text = "headingText";
            // 
            // headingSpacer
            // 
            this.headingSpacer.BackColor = System.Drawing.Color.White;
            this.headingSpacer.Location = new System.Drawing.Point(0, 48);
            this.headingSpacer.Name = "headingSpacer";
            this.headingSpacer.Size = new System.Drawing.Size(55, 23);
            this.headingSpacer.TabIndex = 5;
            this.headingSpacer.Text = "headingSpacer";
            // 
            // separator1
            // 
            this.separator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separator1.Location = new System.Drawing.Point(0, 339);
            this.separator1.Name = "separator1";
            this.separator1.Size = new System.Drawing.Size(503, 2);
            this.separator1.TabIndex = 3;
            // 
            // separator2
            // 
            this.separator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separator2.Location = new System.Drawing.Point(0, 71);
            this.separator2.Name = "separator2";
            this.separator2.Size = new System.Drawing.Size(503, 2);
            this.separator2.TabIndex = 6;
            //
            // whiteBackground
            //
            this.whiteBackground.Location = new System.Drawing.Point(0, 73);
            this.whiteBackground.Size = new System.Drawing.Size(503, 266);
            this.whiteBackground.BackColor = Color.FromArgb(255, 255, 255);
            // 
            // languageBox
            // 
            this.languageBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageBox.Location = new System.Drawing.Point(10, 350);
            this.languageBox.Name = "languageBox";
            this.languageBox.Size = new System.Drawing.Size(140, 21);
            this.languageBox.TabIndex = 7;
            this.languageBox.SelectedIndexChanged += new System.EventHandler(this.languageBox_SelectedIndexChanged);
            // 
            // SetupWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(495, 382);
            this.Controls.Add(this.languageBox);
            this.Controls.Add(this.separator2);
            this.Controls.Add(this.separator1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.headerImage);
            this.Controls.Add(this.headingSpacer);
            this.Controls.Add(this.whiteBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SetupWizard";
            this.StartPosition = FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.headerImage)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        void languageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CultureNameAndInfo cnai = this.languageBox.SelectedItem as CultureNameAndInfo;

            if (this.languageInitDone && cnai != null && 
                cnai.CultureInfo != PdnResources.Culture)
            {
                PdnResources.Culture = cnai.CultureInfo;
                LoadResources();
                this.pages.Clear();
                this.GoToPage(typeof(IntroPage));
            }
        }

        private static bool CheckOSVersion(int major, int minor, short servicePack)
        {
            NativeStructs.OSVERSIONINFOEX osvi = new NativeStructs.OSVERSIONINFOEX();
            osvi.dwOSVersionInfoSize = (uint)NativeStructs.OSVERSIONINFOEX.SizeOf;
            osvi.dwMajorVersion = (uint)major;
            osvi.dwMinorVersion = (uint)minor;
            osvi.wServicePackMajor = (ushort)servicePack;

            ulong mask = 0;
            mask = NativeMethods.VerSetConditionMask(mask, NativeConstants.VER_MAJORVERSION, NativeConstants.VER_GREATER_EQUAL);
            mask = NativeMethods.VerSetConditionMask(mask, NativeConstants.VER_MINORVERSION, NativeConstants.VER_GREATER_EQUAL);
            mask = NativeMethods.VerSetConditionMask(mask, NativeConstants.VER_SERVICEPACKMAJOR, NativeConstants.VER_GREATER_EQUAL);

            bool result = NativeMethods.VerifyVersionInfo(
                ref osvi, 
                NativeConstants.VER_MAJORVERSION | 
                    NativeConstants.VER_MINORVERSION |
                    NativeConstants.VER_SERVICEPACKMAJOR, 
                mask);

            return result;
        }

        // Requires:
        // * Windows 2000 SP4 or later
        // * Windows XP SP2 or later
        // * Windows Server 2003 SP1 or later
        // * Windows Vista
        // * or newer
        private static bool CheckOSRequirement()
        {
            // Just say "no" to Windows 9x
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return false;
            }
            
            // Windows Vista or later?
            bool winVista = CheckOSVersion(6, 0, 0);

            // Windows 2003 or later?
            bool win2k3 = CheckOSVersion(5, 2, 0);

            // Windows 2003 SP1 or later?
            bool win2k3SP1 = CheckOSVersion(5, 2, 1);

            // Windows XP or later?
            bool winXP = CheckOSVersion(5, 1, 0);

            // Windows XP SP2 or later?
            bool winXPSP2 = CheckOSVersion(5, 1, 2);

            // Windows 2000 SP4 or later?
            bool win2kSP4 = CheckOSVersion(5, 0, 4);

            if (winVista)
            {
                return true;
            }
            else if (win2k3SP1)
            {
                return true;
            }
            else if (win2k3 && !win2k3SP1)
            {
                return false;
            }
            else if (winXPSP2)
            {
                return true;
            }
            else if (winXP && !winXPSP2)
            {
                return false;
            }
            else if (win2kSP4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsUserAdministrator()
        {
            AppDomain domain = Thread.GetDomain();
            domain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isAdmin;
        }

        private static bool CheckRequirements()
        {
            bool pass = true;

            // Check for Win2K or later
            bool osRequirement = CheckOSRequirement();
            pass &= osRequirement;

            // Check for admin
            bool isAdmin = IsUserAdministrator();
            pass &= isAdmin;

            // Show error if necessary
            if (!pass)
            {
                string title = PdnInfo.GetProductName();
                string message = "internal error";

                if (!osRequirement)
                {
                    message = PdnResources.GetString("SetupWizard.Error.Win2KRequired");
                }
                else if (!isAdmin)
                {
                    message = PdnResources.GetString("SetupWizard.Error.AdminRequired");
                }

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return pass;
        }

        private static void ShowHelp()
        {
            string title = PdnInfo.GetProductName();
            string helpText = PdnResources.GetString("SetupWizard.HelpText");
            MessageBox.Show(helpText, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) 
        {
            try
            {
                MainImpl(args);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        static void MainImpl(string[] args)
        {
            if (!PdnInfo.HandleExpiration())
            {
                return;
            }

            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            SystemLayer.UI.EnableDPIAware();

            // Uncomment to test German
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de");
                    
            bool doInstall = true;
            bool doMsiDump = false;
            bool restartPdnOnExit = false;
            string[] propertyDefaults = PropertyNames.Defaults;

            SetupWizard setupWizard = new SetupWizard();

            // Parse through command-line options
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                string argLower = arg.ToLower();

                switch (argLower)
                {
                    case "-?":
                    case "/?":
                    case "-help":
                    case "/help":
                        ShowHelp();
                        doInstall = false;
                        break;

                    case "-restartpdnonexit":
                    case "/restartpdnonexit":
                        restartPdnOnExit = true;
                        break;

                    case "-skipconfig":
                    case "/skipconfig":
                        setupWizard.SkipConfig = true;
                        setupWizard.SetMsiProperty(PropertyNames.PdnUpdating, "1");
                        break;

                    case "-auto":
                    case "/auto":
                        setupWizard.AutoMode = true;
                        setupWizard.SkipConfig = true;
                        break;

                    case "-createmsi":
                    case "/createmsi":
                        doMsiDump = true;
                        propertyDefaults = PropertyNames.AdGpoDefaults;
                        break;

                    default:
                        setupWizard.AddPropertyFromArg(arg);
                        break;
                }
            }

            // Load all the propreties that we always need to have. Defaults will be loaded
            // for properties that are not already set.
            for (int i = 0; i < propertyDefaults.Length; i += 2)
            {
                setupWizard.GetMsiProperty(propertyDefaults[i], propertyDefaults[i + 1]);
            }

            setupWizard.SetMsiProperty(PropertyNames.PdnUpdating, "0");

            // Only allow 1 instance of setup wizard running at a time...
            bool createdNew;
            Mutex mutex = new Mutex(false, mutexName, out createdNew);

            doInstall &= createdNew;

            if (doInstall)
            {
                if (CheckRequirements())
                {
                    if (doMsiDump)
                    {
                        setupWizard.ClearPageStack();
                        setupWizard.GoToPage(typeof(CreateMsiPage));
                    }

                    setupWizard.ShowDialog();
                }

                // When we do an update, Paint.NET launches our installer with the /restartPdnOnExit
                // flag. This tells us to run Paint.NET when the update is finished. This accomplishes
                // two things:
                //
                // 1. Adds a slight amount of continuity for the user. When they're installing an update
                //    they'll probably want to start-up Paint.NET right away. So we do it for them.
                // 2. Cleans up (deletes) the downloaded setup file. Otherwise, the user has to make sure
                //    that they immediately re-run Paint.NET under the same user account in order to
                //    clean this up.
                // 
                // #2 does introduce a slight race condition, so PaintDotNet.exe will actually retry
                // up to 3 times to delete the file with a 1 second pause between retries. This should
                // give enough time for the setup processes to unwind without causing too horrible of
                // a delay in the worst case.
                if (restartPdnOnExit && !setupWizard.RebootRequired)
                {
                    string targetDir = setupWizard.GetMsiProperty(PropertyNames.TargetDir, null);

                    if (targetDir != null)
                    {
                        string pdnPathName = Path.Combine(targetDir, "PaintDotNet.exe");

                        if (File.Exists(pdnPathName))
                        {
                            Process.Start(pdnPathName);
                        }
                    }
                }

                setupWizard.Dispose();
            }

            mutex.Close();
        }

        private void nextButton_Click(object sender, System.EventArgs e)
        {
            this.wizardPage.OnNextClicked();
        }

        private void backButton_Click(object sender, System.EventArgs e)
        {
            if (this.pages.Count > 0)
            {
                Type pageType = (Type)this.pages.Pop();
                SetPage(pageType);
            }
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}

