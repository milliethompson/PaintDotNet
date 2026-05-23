using System;
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

        public void Start()
        {
#if !DEBUG
            /*
            try
            {
            */
#endif
                // Create our self ...
                MainForm mainForm = new MainForm(args); // command line args are passed in order to handle things like opening a file via the arguments, "paintdotnet.exe whatever.bmp"

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
                mainForm = null;
#if !DEBUG
                /*
            }

            catch
            {
                Utility.ErrorBox(null, "There was an unhandled error, and Paint.NET must be closed.");
                return;
            }
            */
#endif

            GC.Collect();
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
	}
}
