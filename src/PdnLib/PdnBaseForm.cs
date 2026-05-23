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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
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

        private System.ComponentModel.IContainer components;
        private bool instanceEnableOpacity = true;
        private static bool globalEnableOpacity = true;
        private FormEx formEx;

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            if (!hevent.Handled)
            {
                Utility.ShowHelp(this);
                hevent.Handled = true;
            }

            base.OnHelpRequested(hevent);
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
        /// If a particular form's EnableInstanceOpacity property is false, that will override
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

        /// <summary>
        /// Gets or sets the titlebar rendering behavior for when the form is deactivated.
        /// </summary>
        /// <remarks>
        /// If this property is false, the titlebar will be rendered in a different color when the form
        /// is inactive as opposed to active. If this property is true, it will always render with the
        /// active style. If the whole application is deactivated, the title bar will still be drawn in
        /// an inactive state.
        /// </remarks>
        public bool ForceActiveTitleBar
        {
            get
            {
                return this.formEx.ForceActiveTitleBar;
            }

            set
            {
                this.formEx.ForceActiveTitleBar = value;
            }
        }

        public PdnBaseForm()
        {
            UI.InitScaling(this);

            this.SuspendLayout();
            InitializeComponent();

            this.formEx = new PaintDotNet.SystemLayer.FormEx(this, new RealParentWndProcDelegate(this.RealWndProc));
            this.Controls.Add(this.formEx);
            this.formEx.Visible = false;
            DecideOpacitySetting();
            this.ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!this.DesignMode)
            {
                LoadResources();
            }

            base.OnLoad(e);
        }

        public virtual void LoadResources()
        {
            if (!this.DesignMode)
            {
                string stringName = this.Name + ".Localized";
                string stringValue = StringsResourceManager.GetString(stringName);

                if (stringValue != null)
                {
                    try
                    {
                        bool boolValue = bool.Parse(stringValue);

                        if (boolValue)
                        {
                            LoadLocalizedResources();
                        }
                    }

                    catch
                    {
                    }
                }
            }
        }

        protected virtual ResourceManager StringsResourceManager
        {
            get
            {
                return PdnResources.Strings;
            }
        }

        private void LoadLocalizedResources()
        {
            LoadLocalizedResources(this.Name, this);
        }

        private void ParsePair(string theString, out int x, out int y)
        {
            string[] split = theString.Split(',');
            x = int.Parse(split[0]);
            y = int.Parse(split[1]);
        }

        private void LoadLocalizedResources(string baseName, Control control)
        {
            // Text
            string textStringName = baseName + ".Text";
            string textString = this.StringsResourceManager.GetString(textStringName);

            if (textString != null)
            {
                control.Text = textString;
            }

            // Location
            string locationStringName = baseName + ".Location";
            string locationString = this.StringsResourceManager.GetString(locationStringName);

            if (locationString != null)
            {
                try
                {
                    int x;
                    int y;

                    ParsePair(locationString, out x, out y);
                    control.Location = new Point(x, y);
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(locationStringName + " is invalid: " + locationString + ", exception: " + ex.ToString());
                }
            }

            // Size
            string sizeStringName = baseName + ".Size";
            string sizeString = this.StringsResourceManager.GetString(sizeStringName);

            if (sizeString != null)
            {
                try
                {
                    int width;
                    int height;

                    ParsePair(sizeString, out width, out height);
                    control.Size = new Size(width, height);
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(sizeStringName + " is invalid: " + sizeString + ", exception: " + ex.ToString());
                }
            }

            // Recurse
            foreach (Control child in control.Controls)
            {
                if (child.Name == null || child.Name.Length > 0)
                {
                    string newBaseName = baseName + "." + child.Name;
                    LoadLocalizedResources(newBaseName, child);
                }
                else
                {
                    Debug.WriteLine("Name property not set for an instance of " + child.GetType().Name + " within " + baseName);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                this.ForceActiveTitleBar = false;
            }
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
            base.OnHandleDestroyed(e);

            PdnBaseForm.EnableOpacityChanged -= new EventHandler(EnableOpacityChangedHandler);
            UserSessions.SessionChanged -= new EventHandler(UserSessions_SessionChanged);
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
                return this.ourOpacity;
            }

            set
            {
                if (enableOpacity)
                {
                    // Bypassing Form.Opacity eliminates a "black flickering" that occurs when
                    // the form transitions from Opacity=1.0 to Opacity != 1.0, or vice versa.
                    // It appears to be a result of toggling the WS_EX_LAYERED style, or the
                    // fact that Form.Opacity re-applies visual styles when this value transition
                    // takes place.
                    SystemLayer.UI.SetFormOpacity(this, value);
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
                if (this.enableOpacity)
                {
                    try
                    {
                        UI.SetFormOpacity(this, 1.0);
                    }

                    // This fails in certain odd situations (bug #746), so we just eat the exception.
                    catch (System.ComponentModel.Win32Exception)
                    {
                    }
                }

                this.enableOpacity = false;
            }
            else
            {
                if (!this.enableOpacity)
                {
                    // This fails in certain odd situations (bug #746), so we just eat the exception.
                    try
                    {
                        UI.SetFormOpacity(this, this.ourOpacity);
                    }

                    catch (System.ComponentModel.Win32Exception)
                    {
                    }
                }

                this.enableOpacity = true;
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
            this.SuspendLayout();
            // 
            // PdnBaseForm
            // 
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(291, 270);
            this.Name = "PdnBaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "PdnBaseForm";
            this.ResumeLayout(false);

        }
        #endregion

        public event MovingEventHandler Moving;
        protected virtual void OnMoving(MovingEventArgs mea)
        {
            if (Moving != null)
            {
                Moving(this, mea);
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

        private void UserSessions_SessionChanged(object sender, EventArgs e)
        {
            this.DecideOpacitySetting();
        }

        void RealWndProc(ref Message m)
        {
            OurWndProc(ref m);
        }

        protected override void WndProc(ref Message m)
        {
            if (this.formEx == null)
            {
                base.WndProc(ref m);
            }
            else if (!this.formEx.HandleParentWndProc(ref m))
            {
                OurWndProc(ref m);
            }
        }

        private void OurWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0216: // WM_MOVING
                    unsafe
                    {
                        int *p = (int *)m.LParam;
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
                case 0x0011: // WM_QUERYENDSESSION
                    CancelEventArgs e = new CancelEventArgs();
                    OnQueryEndSession(e);
                    m.Result = e.Cancel ? IntPtr.Zero : new IntPtr(1);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
