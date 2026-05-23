/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
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

        // NOTE: This is done as an object and not VisualStyleProvider so that we can delay loading
        //       the Skybound.VisualStyles.dll until the user clicks "More >>" on the ColorsForm
        //       (or brings up any other dialog with a control that requires this class' assitance)
        private static Skybound.VisualStyles.VisualStyleProvider visualStyleProvider;
        
        private Skybound.VisualStyles.VisualStyleProvider VisualStyleProvider
        {
            get
            {
                if (visualStyleProvider == null)
                {
                    visualStyleProvider = new Skybound.VisualStyles.VisualStyleProvider();
                }

                return (Skybound.VisualStyles.VisualStyleProvider)visualStyleProvider;
            }
        }

        private bool instanceEnableOpacity = true;
        private static bool globalEnableOpacity = true;

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            if (!hevent.Handled)
            {
                Utility.ShowHelp(this);
                hevent.Handled = true;
            }

            base.OnHelpRequested (hevent);
        }

        public static EventHandler EnableOpacityChanged;
        private static void OnEnableOpacityChanged()
        {
            if (EnableOpacityChanged != null)
            {
                EnableOpacityChanged(null, EventArgs.Empty);
            }
        }

        public bool EnableInstanceOpacity
        {
            get
            {
                return instanceEnableOpacity;
            }

            set
            {
                instanceEnableOpacity = value;
                this.DecideOpacitySetting();
            }
        }

        /// <summary>
        /// Gets or sets a flag that enables or disables opacity for all PdnBaseForm instances.
        /// If a particular form's EnableInstanceOpacity property is false, this will override
        /// this property being 'true'.
        /// </summary>
        public static bool EnableOpacity
        {
            get
            {
                return globalEnableOpacity;
            }

            set
            {
                globalEnableOpacity = value;
                OnEnableOpacityChanged();
            }
        }

        public PdnBaseForm()
        {
            InitializeComponent();
            toolTipSentinel.SetToolTip(fixNoToolTipsAndFocusBugSentinel, "fixed");
        }

        private void EnableOpacityChangedHandler(object sender, EventArgs e)
        {
            this.DecideOpacitySetting();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated (e);

            PdnBaseForm.EnableOpacityChanged += new EventHandler(EnableOpacityChangedHandler);
            UserSessions.SessionChanged += new EventHandler(UserSessions_SessionChanged);
            DecideOpacitySetting();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed (e);

            PdnBaseForm.EnableOpacityChanged -= new EventHandler(EnableOpacityChangedHandler);
            UserSessions.SessionChanged -= new EventHandler(UserSessions_SessionChanged);
        }

        protected override void OnLoad(EventArgs e)
        {
            OnEnableStyles();
            base.OnLoad (e);
        }

        /// <summary>
        /// This method is called to recursively enable visual style support on the form.
        /// Normally this is called during OnLoad but you can override this method to
        /// disable that and defer style support initialization until later.
        /// All this method does is call EnableStyles(this).
        /// </summary>
        protected virtual void OnEnableStyles()
        {
            EnableStyles(this);
        }

        /// <summary>
        /// This method is used to make sure everything is rendered using XP Themes.
        /// Important when running with .NET 1.1.
        /// </summary>
        /// <param name="control"></param>
        protected void EnableStyles(Control control)
        {
            // This terrible nesting is necessary to limit ourself to 1 typecast per test
            ButtonBase buttonBase = control as ButtonBase;
            if (buttonBase != null)
            {
                buttonBase.FlatStyle = FlatStyle.System;
            }
            else
            {
                GroupBox groupBox = control as GroupBox;
                if (groupBox != null)
                {
                    groupBox.FlatStyle = FlatStyle.System;
                }
                else
                {
                    NumericUpDown numericUpDown = control as NumericUpDown;

                    if (numericUpDown != null)
                    {
                        EnableVisualStyleSupport(numericUpDown);
                    }
                }
            }

            foreach (Control c in control.Controls)
            {
                EnableStyles(c);
            }
        }

        private void EnableVisualStyleSupport(Control control)
        {
            VisualStyleProvider.SetVisualStyleSupport(control, true);
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

        /// <summary>
        /// Decides whether or not to have opacity be enabled.
        /// </summary>
        private void DecideOpacitySetting()
        {
            if (UserSessions.IsRemote() || !PdnBaseForm.globalEnableOpacity || !this.EnableInstanceOpacity)
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

        public event CancelEventHandler QueryEndSession;
        protected virtual void OnQueryEndSession(CancelEventArgs e)
        {
            if (QueryEndSession != null)
            {
                QueryEndSession(this, e);
            }
        }

        private const int WM_MOVING = 0x0216;
        private const int WM_QUERYENDSESSION = 0x0011;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOVING:
                    unsafe
                    {
                        int *p = (int*)m.LParam;
                        Rectangle rect = Rectangle.FromLTRB(p[0], p[1], p[2], p[3]);
                       
                        MovingEventArgs mea = new MovingEventArgs(rect);
                        OnMoving(mea);

                        p[0] = mea.Rectangle.Left;
                        p[1] = mea.Rectangle.Top;
                        p[2] = mea.Rectangle.Right;
                        p[3] = mea.Rectangle.Bottom;

                        m.Result = new IntPtr(1);
                    }
                    break;

                // WinForms doesn't handle this message correctly and wrongly returns 0 instead of 1.
                case WM_QUERYENDSESSION:
                    CancelEventArgs e = new CancelEventArgs();
                    OnQueryEndSession(e);
                    m.Result = e.Cancel ? IntPtr.Zero : new IntPtr(1);
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

        public double ScreenAspect
        {
            get
            {
                Rectangle bounds = System.Windows.Forms.Screen.FromControl(this).Bounds;
                double aspect = (double)bounds.Width / (double)bounds.Height;
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

        private void UserSessions_SessionChanged(object sender, EventArgs e)
        {
            this.DecideOpacitySetting();
        }
    }
}
