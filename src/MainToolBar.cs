using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using DotNetWidgets;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MainToolBar.
	/// </summary>
	public class MainToolBar : System.Windows.Forms.UserControl
	{
		private PaintDotNet.ColorDisplayWidget colorDisplayWidget;
        private DotNetWidgets.DotNetToolbar[] dotNetToolbars;
        private ImageList imageList;
        private DotNetWidgets.DotNetToolbar.ButtonClickEventHandler toolClickedDelegate;
        private const int tbWidth = 2; // two buttons per line in the toolbars

        /// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public class DotNetToolbarButtonItemWithTag 
            : DotNetToolbarButtonItem
        {
            private object tag;
            public object Tag
            {
                get
                {
                    return tag;
                }

                set
                {
                    tag = value;
                }
            }
        }

        public event ToolClickedEventHandler ToolClicked;
        protected virtual void OnToolClicked(Type toolType)
        {
            if (ToolClicked != null)
            {
                ToolClicked(this, new ToolClickedEventArgs(toolType));
            }
        }

		public ColorDisplayWidget ColorDisplay
		{
			get
			{
				return colorDisplayWidget;
			}
		}

        public void SetTools(Type[] toolTypes, DocumentWorkspace workspace)
        {
            imageList = new ImageList();
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            int tbIndex = 0;

            if (dotNetToolbars != null)
            {
                foreach(DotNetToolbar tb in dotNetToolbars)
                {
                    tb.ButtonClick -= this.toolClickedDelegate;
                    this.Controls.Remove(tb);
                }
            }

            dotNetToolbars = new DotNetWidgets.DotNetToolbar[(toolTypes.Length + (tbWidth - 1)) / tbWidth];

            for (int i = 0; i < dotNetToolbars.Length; ++i)
            {
                dotNetToolbars[i] = new DotNetWidgets.DotNetToolbar();
                dotNetToolbars[i].Dock = DockStyle.Top;
                dotNetToolbars[i].ButtonClick += toolClickedDelegate;
                dotNetToolbars[i].DrawGrabHandle = false;
                dotNetToolbars[i].ImageList = imageList;
            }

            // We add them in reverse order so they show up in the correct order
            for (int i = dotNetToolbars.Length - 1; i >= 0; --i)
            {
                this.Controls.Add(dotNetToolbars[i]);
            }

            foreach (Type type in toolTypes)
            {
                Tool tool = Tool.CreateTool(type, workspace);
                DotNetToolbarButtonItemWithTag tbb = new DotNetToolbarButtonItemWithTag();
                int index = imageList.Images.Add(tool.Image, imageList.TransparentColor);
                tbb.ImageIndex = index;
                tbb.Tag = type;
                tbb.ToolTipText = tool.Name;
                dotNetToolbars[tbIndex / tbWidth].Buttons.Add(tbb);
                tool = null;

                ++tbIndex;
            }
        }

        public void SelectTool(Type toolType)
        {
            foreach (DotNetToolbar dotNetToolbar in dotNetToolbars)
            {
                foreach (DotNetToolbarButtonItemWithTag tbb in dotNetToolbar.Buttons)
                {
                    if ((Type)tbb.Tag == toolType)
                    {
                        dotNetToolbar_ButtonClick(this, new DotNetToolbarItemClickEventArgs(tbb));
                        return;
                    }
                }
            }

            throw new ArgumentException("Tool type not found");
        }

        public int ToolbarsHeight()
        {
            int total = 0;

            foreach(Control c in dotNetToolbars)
            {
                total += c.Height;
            }

            return total;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            this.ClientSize = new Size(dotNetToolbars[0].Width, colorDisplayWidget.Height + ToolbarsHeight());
        }

        public MainToolBar()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            this.toolClickedDelegate = new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(dotNetToolbar_ButtonClick);
        }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
            this.colorDisplayWidget = new PaintDotNet.ColorDisplayWidget();
            this.SuspendLayout();
            // 
            // colorDisplayWidget
            // 
            this.colorDisplayWidget.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.colorDisplayWidget.Location = new System.Drawing.Point(0, 256);
            this.colorDisplayWidget.Name = "colorDisplayWidget";
            this.colorDisplayWidget.TabIndex = 0;
            // 
            // MainToolBar
            // 
            this.Controls.Add(this.colorDisplayWidget);
            this.Name = "MainToolBar";
            this.Size = new System.Drawing.Size(48, 304);
            this.ResumeLayout(false);

        }
		#endregion

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            DotNetToolbarButtonItemWithTag button = (DotNetToolbarButtonItemWithTag)e.Button;

            foreach (DotNetToolbar dotNetToolbar in dotNetToolbars)
            {
                foreach (DotNetToolbarButtonItemWithTag tbb in dotNetToolbar.Buttons)
                {
                    if (tbb != button)
                    {
                        tbb.Pushed = false;
                    }
                }
            }

            button.Pushed = true;
            OnToolClicked((Type)button.Tag);
        }
	}
}

