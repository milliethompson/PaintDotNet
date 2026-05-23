using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// This Form class is used to fix a few bugs in Windows Forms, and to add a few performance
    /// enhancements, such as disabling opacity != 1.0 when running in a remote TS/RD session.
    /// We derive from this class instead of Windows.Forms.Form directly.
    /// </summary>
    public class PdnBaseForm 
        : System.Windows.Forms.Form
    {
        private bool enableOpacity = true;
        private double ourOpacity = 1.0; // store opacity setting so that when we go from disabled->enabled opacity we can set the correct value
        private System.Windows.Forms.ToolTip toolTipSentinel;
        private System.Windows.Forms.Control fixNoToolTipsAndFocusBugSentinel;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Timer win2kTsDetectionTimer; // this timer is used in Win2K so we can poll every few seconds to see if we are running in a remote session

        private static readonly Skybound.VisualStyles.VisualStyleProvider visualStyleProvider = 
            new Skybound.VisualStyles.VisualStyleProvider();

        public PdnBaseForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            toolTipSentinel.SetToolTip(fixNoToolTipsAndFocusBugSentinel, "fixed");
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated (e);

            try
            {
                SafeNativeMethods.WTSRegisterSessionNotification(this.Handle, SafeNativeMethods.NOTIFY_FOR_ALL_SESSIONS);
            }

            catch (EntryPointNotFoundException)
            {
                win2kTsDetectionTimer.Enabled = true;
            }

            DecideOpacitySetting();
            OnRemoteSessionChange();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed (e);

            win2kTsDetectionTimer.Enabled = false;

            try
            {
                SafeNativeMethods.WTSUnRegisterSessionNotification(this.Handle);
            }

            catch (EntryPointNotFoundException)
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            EnableStyles(this);
        }

        /// <summary>
        /// This method is used to make sure everything is rendered using XP Themes.
        /// Important when running with .NET 1.1.
        /// </summary>
        /// <param name="control"></param>
        private void EnableStyles(Control control)
        {
            if (control is ButtonBase)
            {
                ((ButtonBase)control).FlatStyle = FlatStyle.System;
            }

            if (control is GroupBox)
            {
                ((GroupBox)control).FlatStyle = FlatStyle.System;
            }

            if (control is NumericUpDown)
            {
                visualStyleProvider.SetVisualStyleSupport(control, true);
            }

            foreach (Control c in control.Controls)
            {
                EnableStyles(c);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        /// <summary>
        /// Sets the opacity of the form.
        /// </summary>
        /// <param name="newOpacity">The new opacity value.</param>
        /// <remarks>
        /// Depending on the system configuration, this request may be ignored. For example,
        /// when running within a Terminal Service (or Remote Desktop) session, opacity will
        /// always be set to 1.0 for performance reasons.
        /// </remarks>
        public new double Opacity
        {
            get
            {
                return ourOpacity;
            }

            set
            {
                if (enableOpacity)
                {
                    base.Opacity = value;
                }

                this.ourOpacity = value;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private class SafeNativeMethods
        {
            private SafeNativeMethods()
            {
            }

            public const int SM_REMOTESESSION = 0x1000;
            public const int WM_WTSSESSION_CHANGE = 0x2b1;
            public const int WM_MOVING = 0x0216;
            public const uint NOTIFY_FOR_ALL_SESSIONS = 1;
            public const uint NOTIFY_FOR_THIS_SESSION = 0;

            [DllImport("User32.dll")]
            internal static extern int GetSystemMetrics(int nIndex);

            [DllImport("wtsapi32.dll")]
            internal static extern uint WTSRegisterSessionNotification(IntPtr hWnd, uint dwFlags);

            [DllImport("wtsapi32.dll")]
            internal static extern uint WTSUnRegisterSessionNotification(IntPtr hWnd);
        }

        /// <summary>
        /// Determines whether the form is running within a remoted session (Terminal Server, Remote Desktop).
        /// </summary>
        /// <returns>
        /// <b>true</b> if we're running in a remote session, <b>false</b> otherwise.
        /// </returns>
        public static bool IsRemoteSession()
        {
            return 0 != SafeNativeMethods.GetSystemMetrics(SafeNativeMethods.SM_REMOTESESSION);
        }

        /// <summary>
        /// Decides whether or not to have opacity be enabled.
        /// </summary>
        private void DecideOpacitySetting()
        {
            if (IsRemoteSession())
            {
                try
                {
                    base.Opacity = 1.0;
                }

                // This fails in certain odd situations (bug #746), so we just eat the exception.
                catch (System.ComponentModel.Win32Exception)
                {
                }

                enableOpacity = false;
            }
            else
            {
                enableOpacity = true;

                // This fails in certain odd situations (bug #746), so we just eat the exception.
                try
                {
                    base.Opacity = ourOpacity;
                }

                catch (System.ComponentModel.Win32Exception)
                {
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case SafeNativeMethods.WM_WTSSESSION_CHANGE:
                    DecideOpacitySetting();
                    OnRemoteSessionChange();
                    break;

                case SafeNativeMethods.WM_MOVING:
                unsafe
                {
                    int* p = (int*)m.LParam;

                    // The location is defined as two points, one in the top left, and one in the bottom right
                    Point a = new Point((int)p[0],(int)p[1]);
                    Point b = new Point((int)p[2],(int)p[3]);

                    // Calculates the x,y and width, height of the rectangle
                    Rectangle rect = new Rectangle(a.X,a.Y,b.X - a.X,b.Y - a.Y);
                    
                    MovingEventArgs mea = new MovingEventArgs(rect);
                    
                    OnMoving(mea); // The call

                    // Grabs the new position of the rectangle, and turns it into the two points version
                    p[0] = mea.Rectangle.X;
                    p[1] = mea.Rectangle.Y;
                    p[2] = mea.Rectangle.X + mea.Rectangle.Width;
                    p[3] = mea.Rectangle.Y + mea.Rectangle.Height;

                    m.Result = new IntPtr(1); // return that it was successfull
                }
                break;

                default:
                    base.WndProc (ref m);
                    break;
            }
        }
        
        public event MovingEventHandler Moving;
        protected virtual void OnMoving(MovingEventArgs mea)
        {
            if (Moving != null)
            {
                Moving(this, mea);
            }
        }

        /// <summary>
        /// This event is raised when the remote session changes (i.e. whenever the WM_WTSSESSION_CHANGE
        /// window message is received).
        /// </summary>
        public event EventHandler RemoteSessionChange;

        /// <summary>
        /// Raises the RemoteSessionChange event.
        /// </summary>
        protected virtual void OnRemoteSessionChange()
        {
            if (RemoteSessionChange != null)
            {
                RemoteSessionChange(this, EventArgs.Empty);
            }
        }

        public double ScreenAspect
        {
            get
            {
                Screen ourScreen = Screen.FromControl(this);
                double aspect = (double)ourScreen.Bounds.Width / (double)ourScreen.Bounds.Height;
                return aspect;
            }
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTipSentinel = new System.Windows.Forms.ToolTip(this.components);
            this.fixNoToolTipsAndFocusBugSentinel = new System.Windows.Forms.Control();
            this.win2kTsDetectionTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // fixNoToolTipsAndFocusBugSentinel
            // 
            this.fixNoToolTipsAndFocusBugSentinel.Location = new System.Drawing.Point(10000, 10000);
            this.fixNoToolTipsAndFocusBugSentinel.Name = "fixNoToolTipsAndFocusBugSentinel";
            this.fixNoToolTipsAndFocusBugSentinel.Size = new System.Drawing.Size(75, 23);
            this.fixNoToolTipsAndFocusBugSentinel.TabIndex = 0;
            this.fixNoToolTipsAndFocusBugSentinel.TabStop = false;
            this.fixNoToolTipsAndFocusBugSentinel.Text = "control1";
            // 
            // win2kTsDetectionTimer
            // 
            this.win2kTsDetectionTimer.Interval = 5000;
            this.win2kTsDetectionTimer.Tick += new System.EventHandler(this.win2kTsDetectionTimer_Tick);
            // 
            // PdnBaseForm
            // 
            this.AutoScale = false;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(291, 270);
            this.Controls.Add(this.fixNoToolTipsAndFocusBugSentinel);
            this.Name = "PdnBaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PdnBaseForm";
            this.ResumeLayout(false);

        }
        #endregion

        private void win2kTsDetectionTimer_Tick(object sender, System.EventArgs e)
        {
            DecideOpacitySetting();
        }
    }
}
