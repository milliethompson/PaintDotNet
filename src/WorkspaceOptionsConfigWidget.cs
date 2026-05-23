using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for WorkspaceOptionsConfigWidget.
    /// </summary>
    public class WorkspaceOptionsConfigWidget 
        : System.Windows.Forms.UserControl
    {
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem aaToggleButton;
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem rulersToggleButton;
        private System.ComponentModel.IContainer components;
        
        public bool AntiAliasing
        {
            get
            {
                return aaToggleButton.Pushed;
            }

            set
            {
                if (aaToggleButton.Pushed != value)
                {
                    aaToggleButton.Pushed = value;
                    this.OnAntiAliasChanged();
                }
            }
        }

        public bool RulersEnabled
        {
            get
            {
                return rulersToggleButton.Pushed;
            }

            set
            {
                if (rulersToggleButton.Pushed != value)
                {
                    rulersToggleButton.Pushed = value;
                    this.OnRulersEnabledChanged();
                }
            }
        }

        public WorkspaceOptionsConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            int aaIndex = imageList.Images.Add(Utility.GetImageResource("Icons.AntiAliasingButtonIcon.bmp"), imageList.TransparentColor);
            int rulersIndex = imageList.Images.Add(Utility.GetImageResource("Icons.RulersEnabledButtonIcon.bmp"), imageList.TransparentColor);

            aaToggleButton.ImageIndex = aaIndex;
            rulersToggleButton.ImageIndex = rulersIndex;
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.aaToggleButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.rulersToggleButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.aaToggleButton);
            this.dotNetToolbar.Buttons.Add(this.rulersToggleButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = this.imageList;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.NegotiateToolTips = true;
            this.dotNetToolbar.Size = new System.Drawing.Size(224, 26);
            this.dotNetToolbar.TabIndex = 0;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // aaToggleButton
            // 
            this.aaToggleButton.BeginGroup = true;
            this.aaToggleButton.ToolTipText = "Toggle Anti-Aliasing";
            // 
            // rulersToggleButton
            // 
            this.rulersToggleButton.ToolTipText = "Toggle Rulers";
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // WorkspaceOptionsConfigWidget
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "WorkspaceOptionsConfigWidget";
            this.Size = new System.Drawing.Size(224, 32);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler AntiAliasChanged;
        protected virtual void OnAntiAliasChanged()
        {
            if (AntiAliasChanged != null)
            {
                AntiAliasChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler RulersEnabledChanged;
        protected virtual void OnRulersEnabledChanged()
        {
            if (RulersEnabledChanged != null)
            {
                RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == this.aaToggleButton)
            {
                this.AntiAliasing = !this.AntiAliasing;
            }
            else if (e.Button == this.rulersToggleButton)
            {
                this.RulersEnabled = !this.RulersEnabled;
            }
        }
    }
}
