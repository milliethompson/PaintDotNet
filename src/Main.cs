using System;

namespace PaintDotNet
{
	public sealed class Main
	{
        private Main()
        {
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args) 
        {
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
            return 0;
        }        
	}
}
