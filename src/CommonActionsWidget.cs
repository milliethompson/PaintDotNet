using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for CommonActionsWidget.
    /// </summary>
    public class CommonActionsWidget : System.Windows.Forms.UserControl
    {
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem newButton;
        private DotNetWidgets.DotNetToolbarButtonItem openButton;
        private DotNetWidgets.DotNetToolbarButtonItem saveButton;
        private DotNetWidgets.DotNetToolbarButtonItem cutButton;
        private DotNetWidgets.DotNetToolbarButtonItem copyButton;
        private DotNetWidgets.DotNetToolbarButtonItem pasteButton;
        private DotNetWidgets.DotNetToolbarButtonItem undoButton;
        private DotNetWidgets.DotNetToolbarButtonItem redoButton;
        private DotNetWidgets.DotNetToolbarButtonItem zoomInButton;
        private DotNetWidgets.DotNetToolbarButtonItem zoomOutButton;
        private DotNetWidgets.DotNetToolbarButtonItem deselectButton;
        private System.ComponentModel.IContainer components;

        public CommonActionsWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            dotNetToolbar.ImageList = imageList;

            newButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuFileNewIcon.bmp"), imageList.TransparentColor);
            openButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuFileOpenIcon.bmp"), imageList.TransparentColor);
            saveButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuFileSaveIcon.bmp"), imageList.TransparentColor);
            cutButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditCutIcon.bmp"), imageList.TransparentColor);
            copyButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditCopyIcon.bmp"), imageList.TransparentColor);
            pasteButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditPasteIcon.bmp"), imageList.TransparentColor);
            deselectButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditDeselectIcon.bmp"), imageList.TransparentColor);
            undoButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditUndoIcon.bmp"), imageList.TransparentColor);
            redoButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditRedoIcon.bmp"), imageList.TransparentColor);            
            zoomInButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuImageZoomInIcon.bmp"), imageList.TransparentColor);            
            zoomOutButton.ImageIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuImageZoomOutIcon.bmp"), imageList.TransparentColor);            
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
            this.newButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.openButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.saveButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.cutButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.copyButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.pasteButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.deselectButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.undoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.redoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.zoomInButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.zoomOutButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.newButton);
            this.dotNetToolbar.Buttons.Add(this.openButton);
            this.dotNetToolbar.Buttons.Add(this.saveButton);
            this.dotNetToolbar.Buttons.Add(this.cutButton);
            this.dotNetToolbar.Buttons.Add(this.copyButton);
            this.dotNetToolbar.Buttons.Add(this.pasteButton);
            this.dotNetToolbar.Buttons.Add(this.deselectButton);
            this.dotNetToolbar.Buttons.Add(this.undoButton);
            this.dotNetToolbar.Buttons.Add(this.redoButton);
            this.dotNetToolbar.Buttons.Add(this.zoomInButton);
            this.dotNetToolbar.Buttons.Add(this.zoomOutButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.NegotiateToolTips = true;
            this.dotNetToolbar.Size = new System.Drawing.Size(344, 26);
            this.dotNetToolbar.TabIndex = 0;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // newButton
            // 
            this.newButton.BeginGroup = true;
            this.newButton.ToolTipText = "New";
            // 
            // openButton
            // 
            this.openButton.ToolTipText = "Open";
            // 
            // saveButton
            // 
            this.saveButton.ToolTipText = "Save";
            // 
            // cutButton
            // 
            this.cutButton.BeginGroup = true;
            this.cutButton.ToolTipText = "Cut";
            // 
            // copyButton
            // 
            this.copyButton.ToolTipText = "Copy";
            // 
            // pasteButton
            // 
            this.pasteButton.ToolTipText = "Paste";
            // 
            // deselectButton
            // 
            this.deselectButton.ToolTipText = "Deselect";
            // 
            // undoButton
            // 
            this.undoButton.BeginGroup = true;
            this.undoButton.ToolTipText = "Undo";
            // 
            // redoButton
            // 
            this.redoButton.ToolTipText = "Redo";
            // 
            // zoomInButton
            // 
            this.zoomInButton.BeginGroup = true;
            this.zoomInButton.ToolTipText = "Zoom In";
            // 
            // zoomOutButton
            // 
            this.zoomOutButton.ToolTipText = "Zoom Out";
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // CommonActionsWidget
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "CommonActionsWidget";
            this.Size = new System.Drawing.Size(344, 72);
            this.ResumeLayout(false);

        }
        #endregion

        /*
        public string[] GetButtonNames()
        {
            ArrayList names = new ArrayList();

            for (int i = 0; i < dotNetToolbar.Buttons.Count; ++i)
            {
                if (dotNetToolbar.Buttons[i] is DotNetWidgets.DotNetToolbarButtonItem) // exclude dropdowns, etc
                {
                    names.Add(dotNetToolbar.Buttons[i].ToolTipText);
                }
            }

            return (string[])names.ToArray(typeof(string));
        }
        */

        public void SetButtonEnabled(CommonAction action, bool enabled)
        {
            for (int i = 0; i < dotNetToolbar.Buttons.Count; ++i)
            {
                if (0 == string.Compare(Utility.RemoveSpaces(dotNetToolbar.Buttons[i].ToolTipText), action.ToString(), true))
                {
                    dotNetToolbar.Buttons[i].Enabled = enabled;
                    return;
                }
            }

            throw new ArgumentException("Button name '" + action.ToString() + "' not found");
        }

        public bool GetButtonEnabled(CommonAction action)
        {
            for (int i = 0; i < dotNetToolbar.Buttons.Count; ++i)
            {
                if (0 == string.Compare(Utility.RemoveSpaces(dotNetToolbar.Buttons[i].ToolTipText), action.ToString(), true))
                {
                    return dotNetToolbar.Buttons[i].Enabled;
                }
            }

            throw new ArgumentException("Button name '" + action.ToString() + "' not found");
        }

        public event EnumValueEventHandler ButtonClick;
        protected virtual void OnButtonClick(CommonAction action)
        {
            if (ButtonClick != null)
            {
                ButtonClick(this, new EnumValueEventArgs(action));
            }
        }

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            OnButtonClick((CommonAction)Enum.Parse(typeof(CommonAction), Utility.RemoveSpaces(e.Button.ToolTipText), true));
        }
    }
}
