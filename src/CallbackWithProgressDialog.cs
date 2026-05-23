using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class CallbackWithProgressDialog
    {
        private ProgressDialog pd;
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
                pd.BeginInvoke(new VoidVoidDelegate(DoProgressUpdate), null);
            }
        }

        private void DoProgressUpdate()
        {
            pd.Value = progress;
            pd.Update();
        }

        private void BackgroundCallback()
        {
            this.exception = null;

            try
            {
                threadCallback();
            }

#if !DEBUG
            catch (Exception ex)
            {
                this.exception = ex;
            }
#endif

            finally
            {
                try
                {
                    pd.BeginInvoke(new VoidVoidDelegate(pd.ExternalFinish), null);
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
            this.threadCallback = threadCallback;
        }

        protected DialogResult ShowDialog(bool Cancellable, ThreadStart callback)
        {
            this.threadCallback = callback;
            DialogResult dr = DialogResult.Cancel;
            
            using (pd = new ProgressDialog())
            {
                pd.Text = dialogTitle;
                pd.Description = dialogDescription;

                if (icon != null)
                {
                    pd.Icon = icon;
                }

                EventHandler leh = new EventHandler(pd_Load);
                pd.Load += leh;
                pd.Cancellable = Cancellable;
                thread = new Thread(new ThreadStart(BackgroundCallback));
                Progress = 0;
            
                if (setStartPos)
                {
                    pd.Location = new Point(StartPos.X - (pd.Width / 2), StartPos.Y);;
                    pd.StartPosition = FormStartPosition.Manual;
                }
                else
                {
                    pd.StartPosition = FormStartPosition.CenterParent;
                }

                dr = pd.ShowDialog(owner);
                pd.Load -= leh;

                if (exception != null)
                {
                    throw exception;
                }
            }

            return dr;
        }

        private void pd_Load(object sender, EventArgs e)
        {
            thread.Start();
        }
    }

}
