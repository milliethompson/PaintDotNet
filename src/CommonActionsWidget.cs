/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using DotNetWidgets;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
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
        private DotNetWidgets.DotNetToolbarButtonItem updatesButton;
        private DotNetWidgets.DotNetToolbarButtonItem newButton;
        private DotNetWidgets.DotNetToolbarButtonItem openButton;
        private DotNetWidgets.DotNetToolbarButtonItem saveButton;
        private DotNetWidgets.DotNetToolbarButtonItem cutButton;
        private DotNetWidgets.DotNetToolbarButtonItem copyButton;
        private DotNetWidgets.DotNetToolbarButtonItem pasteButton;
        private DotNetWidgets.DotNetToolbarButtonItem undoButton;
        private DotNetWidgets.DotNetToolbarButtonItem redoButton;
        private DotNetWidgets.DotNetToolbarButtonItem deselectButton;
        private DotNetWidgets.DotNetToolbarButtonItem printButton;
        private System.ComponentModel.IContainer components;

        private int blankIndex;
        private System.Windows.Forms.Timer blinkTimer;

        private EnumWrapper commonActionNames = EnumWrapper.Create(typeof(CommonAction));

        public CommonActionsWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            dotNetToolbar.ImageList = imageList;

            updatesButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuFileUpdatesIcon.bmp"), imageList.TransparentColor);
            newButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuFileNewIcon.bmp"), imageList.TransparentColor);
            openButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuFileOpenIcon.bmp"), imageList.TransparentColor);
            saveButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuFileSaveIcon.bmp"), imageList.TransparentColor);
            printButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuFilePrintIcon.bmp"), imageList.TransparentColor);
            cutButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditCutIcon.bmp"), imageList.TransparentColor);
            copyButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditCopyIcon.bmp"), imageList.TransparentColor);
            pasteButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditPasteIcon.bmp"), imageList.TransparentColor);
            deselectButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditDeselectIcon.bmp"), imageList.TransparentColor);
            undoButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditUndoIcon.bmp"), imageList.TransparentColor);
            redoButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuEditRedoIcon.bmp"), imageList.TransparentColor);            

            this.updatesButton.ToolTipText = PdnResources.GetString("CommonAction.CheckForUpdates");
            this.newButton.ToolTipText = PdnResources.GetString("CommonAction.New"); 
            this.openButton.ToolTipText = PdnResources.GetString("CommonAction.Open"); 
            this.saveButton.ToolTipText = PdnResources.GetString("CommonAction.Save");
            this.printButton.ToolTipText = PdnResources.GetString("CommonAction.Print");
            this.cutButton.ToolTipText = PdnResources.GetString("CommonAction.Cut");
            this.copyButton.ToolTipText = PdnResources.GetString("CommonAction.Copy");
            this.pasteButton.ToolTipText = PdnResources.GetString("CommonAction.Paste");
            this.deselectButton.ToolTipText = PdnResources.GetString("CommonAction.Deselect");
            this.undoButton.ToolTipText = PdnResources.GetString("CommonAction.Undo");
            this.redoButton.ToolTipText = PdnResources.GetString("CommonAction.Redo");

            Bitmap blank = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height);
            using (Graphics g = Graphics.FromImage(blank))
            {
                g.Clear(imageList.TransparentColor);
            }
            
            blankIndex = imageList.Images.Add(blank, imageList.TransparentColor);
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.updatesButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.newButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.openButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.saveButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.printButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.cutButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.copyButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.pasteButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.deselectButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.undoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.redoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.blinkTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.updatesButton);
            this.dotNetToolbar.Buttons.Add(this.newButton);
            this.dotNetToolbar.Buttons.Add(this.openButton);
            this.dotNetToolbar.Buttons.Add(this.saveButton);
            this.dotNetToolbar.Buttons.Add(this.printButton);
            this.dotNetToolbar.Buttons.Add(this.cutButton);
            this.dotNetToolbar.Buttons.Add(this.copyButton);
            this.dotNetToolbar.Buttons.Add(this.pasteButton);
            this.dotNetToolbar.Buttons.Add(this.deselectButton);
            this.dotNetToolbar.Buttons.Add(this.undoButton);
            this.dotNetToolbar.Buttons.Add(this.redoButton);
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.NegotiateToolTips = true;
            this.dotNetToolbar.Size = new System.Drawing.Size(256, 26);
            this.dotNetToolbar.TabIndex = 0;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // updatesButton
            // 
            this.updatesButton.BeginGroup = true;
            this.updatesButton.Enabled = false;
            this.updatesButton.Visible = false;
            // 
            // newButton
            // 
            this.newButton.BeginGroup = true;
            // 
            // cutButton
            // 
            this.cutButton.BeginGroup = true;
            // 
            // undoButton
            // 
            this.undoButton.BeginGroup = true;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // blinkTimer
            // 
            this.blinkTimer.Tick += new System.EventHandler(this.blinkTimer_Tick);
            // 
            // CommonActionsWidget
            // 
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "CommonActionsWidget";
            this.Size = new System.Drawing.Size(256, 26);
            this.ResumeLayout(false);

        }
        #endregion

        private int blinksLeft = 0;
        private DotNetWidgets.DotNetToolbarButtonItem blinkMe;
        private int blinkMeImageIndex;

        public void BlinkButton(CommonAction action, int times, int interval)
        {
            if (blinkTimer.Enabled)
            {
                blinksLeft = 0;
                blinkMe.ImageIndex = blinkMeImageIndex;
                blinkMe = null;
                blinkTimer.Enabled = false;
            }

            blinkTimer.Enabled = true;
            blinkTimer.Interval = interval;
            blinksLeft = 2 * times;

            DotNetToolbarButtonItem button = FindButton(action);
            this.blinkMe = button;
            this.blinkMeImageIndex = this.blinkMe.ImageIndex;
            this.blinkMe.ImageIndex = this.blankIndex;
        }

        private void blinkTimer_Tick(object sender, System.EventArgs e)
        {
            if (blinksLeft == 0)
            {
                blinkMe.ImageIndex = blinkMeImageIndex;
                blinkTimer.Enabled = false;
                blinkMe = null;
            }
            else
            {
                --blinksLeft;

                if (blinkMe.ImageIndex == this.blankIndex)
                {
                    blinkMe.ImageIndex = this.blinkMeImageIndex;
                }
                else
                {
                    blinkMe.ImageIndex = this.blankIndex;
                }
            }
        }

        public void SetButtonEnabled(CommonAction action, bool enabled)
        {
            DotNetToolbarButtonItem button = FindButton(action);
            button.Enabled = enabled;
        }

        public void SetButtonVisible(CommonAction action, bool visible)
        {
            DotNetToolbarButtonItem button = FindButton(action);
            button.Visible = visible;        
        }

        public bool GetButtonEnabled(CommonAction action)
        {
            DotNetToolbarButtonItem button = FindButton(action);
            return button.Enabled;
        }

        public bool GetButtonVisible(CommonAction action)
        {
            DotNetToolbarButtonItem button = FindButton(action);
            return button.Visible;
        }

        private DotNetToolbarButtonItem FindButton(CommonAction action)
        {
            string actionName = commonActionNames.EnumValueToLocalizedName(action);

            for (int i = 0; i < dotNetToolbar.Buttons.Count; ++i)
            {
                string toolTip = dotNetToolbar.Buttons[i].ToolTipText;

                if (0 == string.Compare(actionName, toolTip, true))
                {
                    return (DotNetToolbarButtonItem)dotNetToolbar.Buttons[i];
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
            string actionName = e.Button.ToolTipText;
            object action = commonActionNames.LocalizedNameToEnumValue(actionName);
            OnButtonClick((CommonAction)action);
        }
    }
}
