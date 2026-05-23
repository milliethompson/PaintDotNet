/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class Startup
    {
        private string[] args;
        private static DateTime startupTime;
        private static bool enableCrashLog = true;

        public static bool EnableCrashLog
        {
            get
            {
                return enableCrashLog;
            }

            set
            {
                enableCrashLog = value;
            }
        }

        public Startup(string[] args)
        {
            this.args = args;
        }

        private static void StartNewInstance_Process(string fileName)
        {
            string arg;

            if (fileName != null && fileName.Length != 0)
            {
                arg = "\"" + fileName + "\"";
            }
            else
            {
                arg = "";
            }

            ProcessStartInfo psi = new ProcessStartInfo(Application.ExecutablePath, arg);
            System.Diagnostics.Process.Start(psi);
        }

        private static void StartNewInstance_Thread(string fileName)
        {
            string[] args;

            if (fileName == null)
            {
                args = new string[] { };
            }
            else
            {
                args = new string[] { fileName };
            }

            new Thread(new ThreadStart(new Startup(args).Start)).Start();
        }

        /// <summary>
        /// Starts a new instance of Paint.NET and opens the requested file.
        /// This may or may not start a new process to host the new instance.
        /// </summary>
        /// <param name="fileName">The name of the filename to open, or null to start with a blank canvas.</param>
        public static void StartNewInstance(string fileName)
        {
            StartNewInstance_Process(fileName);

            // It would be preferable to share the same process for all instances of PDN.
            // This would save memory! Lots of it, potentially!
            // However, there are some weird weird issues to contend with ... 
            // Like toolbars freaking out when they redraw because their images are in use elsewhere.
            //StartNewInstance_Thread(fileName);
        }

        public void Start()
        {
#if DEBUG
#else
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
#endif

            string locale = Settings.CurrentUser.GetString(PdnSettings.LanguageName, null);

            if (locale == null)
            {
                locale = Settings.SystemWide.GetString(PdnSettings.LanguageName, null);
            }

            if (locale != null)
            {
                CultureInfo ci = new CultureInfo(locale, true);
                Thread.CurrentThread.CurrentUICulture = ci;
            }

            if (!PdnInfo.HandleExpiration())
            {
                return;
            }

            // Create our self ...
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            SystemLayer.UI.EnableDPIAware();

            MainForm mainForm = new MainForm(args);

            // if the display is set to a portrait mode (tall), then orient the PDN window the same way
            if (mainForm.ScreenAspect < 1.0)
            {
                int width = mainForm.Width;
                int height = mainForm.Height;

                mainForm.Width = height;
                mainForm.Height = width;
            }

            // if the window opens and part of it is off screen, correct this
            Screen screen = Screen.FromControl(mainForm);

            int left = mainForm.Left;
            int right = mainForm.Right;

            if (screen.WorkingArea.Right < mainForm.Right)
            {
                mainForm.Left -= mainForm.Right - screen.WorkingArea.Right;
            }

            if (screen.WorkingArea.Left > mainForm.Left)
            {
                mainForm.Left = screen.WorkingArea.Left;
            }

            if (screen.WorkingArea.Bottom < mainForm.Bottom)
            {
                mainForm.Top -= mainForm.Bottom - screen.WorkingArea.Bottom;
            }

            if (screen.WorkingArea.Top > mainForm.Top)
            {
                mainForm.Top = screen.WorkingArea.Top;
            }

            // if the window is not big enough, correct this
            if (mainForm.Width < 100)
            {
                mainForm.Width = 100; // this value was chosen arbitrarily
            }

            if (mainForm.Height < 100)
            {
                mainForm.Height = 100; // this value was chosen arbitrarily
            }

            // 3 2 1 go
            Application.Run(mainForm);
            mainForm.Dispose();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args) 
        {
            startupTime = DateTime.Now;

#if !DEBUG
            try
            {
#endif
                new Startup(args).Start();
#if !DEBUG
            }

            catch (Exception ex)
            {
                try
                {
                    UnhandledException(ex);
                }

                catch (Exception)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
#endif

            return 0;
        }

        private static void UnhandledException(Exception ex)
        {
            if (!enableCrashLog)
            {
                return;
            }

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            const string fileName = "pdncrash.log";
            string fullName = Path.Combine(dir, fileName);

            using (StreamWriter stream = new System.IO.StreamWriter(fullName, true))
            {
                // This text need not be localized.
                stream.AutoFlush = true;
                stream.WriteLine("This text file was created because Paint.NET crashed.");
                stream.WriteLine("Please e-mail this file to " + InvariantStrings.FeedbackEmail + " so we can diagnose and fix the problem.");
                stream.WriteLine();

                try
                {
                    string fullAppName;

                    try
                    {
                        fullAppName = PdnInfo.GetFullAppName();
                    }

                    catch (Exception ex1)
                    {
                        fullAppName = Application.ProductVersion + ", --- Exception while calling PdnInfo.GetFullPathName(): " + ex1.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("Application version: " + fullAppName);

                    string timeOfCrash;

                    try
                    {
                        timeOfCrash = DateTime.Now.ToString();
                    }

                    catch (Exception ex2)
                    {
                        timeOfCrash = "--- Exception while populating timeOfCrash: " + ex2.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("Time of crash: " + timeOfCrash);

                    string appUptime;

                    try
                    {
                        appUptime = (DateTime.Now - startupTime).ToString();
                    }

                    catch (Exception ex13)
                    {
                        appUptime = "--- Exception while populating appUptime: " + ex13.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("Application uptime: " + appUptime);

                    // Example: 5.1.2600.0 Service Pack 2 Workstation x86
                    string osVersion;

                    try
                    {
                        osVersion = System.Environment.OSVersion.Version.ToString();
                    }

                    catch (Exception ex3)
                    {
                        osVersion = "--- Exception while populating osVersion: " + ex3.ToString() + Environment.NewLine;
                    }

                    string osRevision;

                    try
                    {
                        osRevision = OS.Revision;
                    }

                    catch (Exception ex4)
                    {
                        osRevision = "--- Exception while populating osRevision: " + ex4.ToString() + Environment.NewLine;
                    }

                    string osType;

                    try
                    {
                        osType = OS.Type.ToString();
                    }

                    catch (Exception ex5)
                    {
                        osType = "--- Exception while populating osType: " + ex5.ToString() + Environment.NewLine;
                    }

                    string processorNativeArchitecture;

                    try
                    {
                        processorNativeArchitecture = Processor.NativeArchitecture.ToString().ToLower();
                    }

                    catch (Exception ex6)
                    {
                        processorNativeArchitecture = "--- Exception while populating processorNativeArchitecture: " + ex6.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("OS Version: " + osVersion + " " + osRevision + " " + osType + " " + processorNativeArchitecture);

                    string fxVersion;

                    try
                    {
                        fxVersion = System.Environment.Version.ToString();
                    }

                    catch (Exception ex7)
                    {
                        fxVersion = "--- Exception while populating fxVersion: " + ex7.ToString() + Environment.NewLine;
                    }

                    string processorArchitecture;

                    try
                    {
                        processorArchitecture = Processor.Architecture.ToString().ToLower();
                    }

                    catch (Exception ex8)
                    {
                        processorArchitecture = "--- Exception while populating processorArchitecture: " + ex8.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine(".NET Framework version: " + fxVersion + " " + processorArchitecture);

                    string cpuName;

                    try
                    {
                        cpuName = SystemLayer.Processor.CpuName;
                    }

                    catch (Exception ex9)
                    {
                        cpuName = "--- Exception while populating cpuName: " + ex9.ToString() + Environment.NewLine;
                    }

                    string cpuCount;

                    try
                    {
                        cpuCount = SystemLayer.Processor.LogicalCpuCount.ToString() + "x";
                    }

                    catch (Exception ex10)
                    {
                        cpuCount = "--- Exception while populating cpuCount: " + ex10.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("Processor: " + cpuCount + " " + cpuName);

                    string totalPhysicalBytes;

                    try
                    {
                        totalPhysicalBytes = ((SystemLayer.Memory.TotalPhysicalBytes / 1024) / 1024) + " MB";
                    }

                    catch (Exception ex11)
                    {
                        totalPhysicalBytes = "--- Exception while populating totalPhysicalBytes: " + ex11.ToString() + Environment.NewLine;
                    }

                    stream.WriteLine("Physical memory: " + totalPhysicalBytes);

                    stream.WriteLine();
                }

                catch (Exception ex12)
                {
                    stream.WriteLine("Exception while gathering app and system info: " + ex12.ToString());
                }

                stream.WriteLine("Exception details:");
                stream.WriteLine(ex.ToString());

                stream.WriteLine("------------------------------------------------------------------------------");
            }

            string errorFormat = PdnResources.GetString("Startup.UnhandledError.Format");
            string error = string.Format(errorFormat, fileName);
            Utility.ErrorBox(null, error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException((Exception)e.ExceptionObject);

            if (!e.IsTerminating)
            {
                Process.GetCurrentProcess().Kill();
            }        
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledException(e.Exception);
            Process.GetCurrentProcess().Kill();
        }
    }
}
