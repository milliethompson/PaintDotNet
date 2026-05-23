using System;
using System.Diagnostics;
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

        public Startup(string[] args)
        {
            this.args = args;
        }

        private static void StartNewInstance_Process(string fileName)
        {
            string arg;

            if (fileName != null && fileName.Length != 0)
            {
                arg = "/nosplash \"" + fileName + "\"";
            }
            else
            {
                arg = "/nosplash";
            }

            ProcessStartInfo psi = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, arg);
            System.Diagnostics.Process.Start(psi);
        }

        private static void StartNewInstance_Thread(string fileName)
        {
            string[] args;

            if (fileName == null)
            {
                args = new string[] { "/nosplash" };
            }
            else
            {
                args = new string[] { "/nosplash", fileName };
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
            Utility.TraceMe("the beginning");

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif

            // Create our self ...
            MainForm mainForm = new MainForm(args);
            Utility.TraceMe("created MainForm");

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

            Utility.TraceMe("calling Application.Run");

            // 3 2 1 go
            Application.Run(mainForm);
            mainForm.Dispose();
            mainForm = null;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args) 
        {
            new Startup(args).Start();
            return 0;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string fullName = Path.GetFullPath("pdncrash.log");

            using (StreamWriter stream = new System.IO.StreamWriter(fullName, true))
            {
                stream.WriteLine("Crash log for " + PdnInfo.GetFullAppName());
                stream.WriteLine("Time of crash: " + DateTime.Now.ToString());
                stream.WriteLine();

                Exception writeMe = (Exception)e.ExceptionObject;
                bool first = true;

                while (writeMe != null)
                {
                    if (first != true)
                    {
                        stream.WriteLine();
                        stream.Write("Inner ");
                    }
                        
                    stream.WriteLine("Exception details:");
                    stream.WriteLine(writeMe.ToString());
                    writeMe = writeMe.InnerException;
                    first = false;
                }

                stream.WriteLine("------------------------------------------------------------------------------");
            }

            Utility.ErrorBox(null, "There was an unhandled error, and Paint.NET must be closed. Refer to '" + fullName + "' for more information.");

            if (!e.IsTerminating)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
