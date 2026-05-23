/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class CallbackWithProgressDialog
    {
        private ProgressDialog dialog;
        private string dialogTitle;
        private string dialogDescription;
        private int progress;
        private Thread thread;
        private IWin32Window owner;
        private ThreadStart threadCallback;
        private Exception exception = null;
        private Point startPos = Point.Empty;
        private bool setStartPos = false;
        private Icon icon = null;

        /// <summary>
        /// Used to define the top center of the dialog window when it is created.
        /// If this property is not set, then a Windows-chosen location will be used.
        /// </summary>
        public Point StartPos
        {
            get
            {
                return startPos;
            }

            set
            {
                setStartPos = true;
                startPos = value;
            }
        }

        public Icon Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
            }
        }

        protected int Progress
        {
            get
            {
                return progress;
            }

            set
            {
                progress = value;
                dialog.BeginInvoke(new VoidVoidDelegate(DoProgressUpdate), null);
            }
        }

        private void DoProgressUpdate()
        {
            dialog.Value = progress;
            dialog.Update();
        }

        private void BackgroundCallback()
        {
            this.exception = null;

            try
            {
                threadCallback();
            }

            catch (Exception ex)
            {
                this.exception = ex;
            }

            finally
            {
                try
                {
                    dialog.BeginInvoke(new VoidVoidDelegate(dialog.ExternalFinish), null);
                }

                catch
                {
                }
            }
        }

        public CallbackWithProgressDialog(IWin32Window owner, string dialogTitle, string dialogDescription)
        {
            this.owner = owner;
            this.dialogTitle = dialogTitle;
            this.dialogDescription = dialogDescription;
        }

        protected DialogResult ShowDialog(bool Cancellable, ThreadStart callback)
        {
            this.threadCallback = callback;
            DialogResult dr = DialogResult.Cancel;
            
            using (dialog = new ProgressDialog())
            {
                dialog.Text = dialogTitle;
                dialog.Description = dialogDescription;

                if (icon != null)
                {
                    dialog.Icon = icon;
                }

                EventHandler leh = new EventHandler(dialog_Load);
                dialog.Load += leh;
                dialog.Cancellable = Cancellable;
                thread = new Thread(new ThreadStart(BackgroundCallback));
                Progress = 0;
            
                if (setStartPos)
                {
                    dialog.Location = new Point(StartPos.X - (dialog.Width / 2), StartPos.Y);;
                    dialog.StartPosition = FormStartPosition.Manual;
                }
                else
                {
                    dialog.StartPosition = FormStartPosition.CenterParent;
                }

                dr = Utility.ShowDialog(dialog, owner);
                dialog.Load -= leh;

                if (exception != null)
                {
                    throw exception;
                }
            }

            return dr;
        }

        private void dialog_Load(object sender, EventArgs e)
        {
            thread.Start();
        }
    }

}
