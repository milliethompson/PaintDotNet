/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class HistoryForm
        : FloatingToolForm
    {
        private PaintDotNet.HistoryControl historyControl;
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem clearHistoryButton;
        private System.Windows.Forms.ImageList imageList;
        private DotNetWidgets.DotNetToolbarButtonItem undoButton;
        private DotNetWidgets.DotNetToolbarButtonItem redoButton;
        private DotNetWidgets.DotNetToolbarButtonItem fastForwardButton;
        private DotNetWidgets.DotNetToolbarButtonItem rewindButton;
        private DotNetWidgets.DotNetToolbarButtonItem limitButton;
        private System.ComponentModel.IContainer components;

        public HistoryControl HistoryControl
        {
            get
            {
                return historyControl;
            }
        }

        public HistoryForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            imageList.TransparentColor = Color.FromArgb(192, 192, 192);

            int clearHistoryIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuLayersDeleteLayerIcon.bmp"), imageList.TransparentColor);
            int rewindIndex = imageList.Images.Add(Utility.GetImageResource("Icons.HistoryRewindIcon.bmp"), imageList.TransparentColor);
            int undoIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditUndoIcon.bmp"), imageList.TransparentColor);
            int redoIndex = imageList.Images.Add(Utility.GetImageResource("Icons.MenuEditRedoIcon.bmp"), imageList.TransparentColor);
            int fastForwardIndex = imageList.Images.Add(Utility.GetImageResource("Icons.HistoryFastForwardIcon.bmp"), imageList.TransparentColor);
            int limitIndex = imageList.Images.Add(Utility.GetImageResource("Icons.HistoryLimitIcon.bmp"), imageList.TransparentColor);

            clearHistoryButton.ImageIndex = clearHistoryIndex;
            rewindButton.ImageIndex = rewindIndex;
            undoButton.ImageIndex = undoIndex;
            redoButton.ImageIndex = redoIndex;
            fastForwardButton.ImageIndex = fastForwardIndex;
            limitButton.ImageIndex = limitIndex;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);

            // We have to test for null in case Layout is raised before our 
            // InitializeComponent is called (or is finished)
            if (historyControl != null)
            {
                historyControl.Size = new Size(ClientRectangle.Width, ClientRectangle.Height - (dotNetToolbar.Height + (ClientRectangle.Height - dotNetToolbar.Bottom)));
            }
        }

        protected override void OnEnableStyles()
        {
            //base.OnEnableStyles ();
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.historyControl = new PaintDotNet.HistoryControl();
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.rewindButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.undoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.redoButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.fastForwardButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.clearHistoryButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.limitButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // historyControl
            // 
            this.historyControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.historyControl.HistoryStack = null;
            this.historyControl.Location = new System.Drawing.Point(0, 0);
            this.historyControl.Name = "historyControl";
            this.historyControl.Size = new System.Drawing.Size(160, 152);
            this.historyControl.TabIndex = 0;
            this.historyControl.HistoryChanged += new System.EventHandler(this.historyControl_HistoryChanged);
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.rewindButton);
            this.dotNetToolbar.Buttons.Add(this.undoButton);
            this.dotNetToolbar.Buttons.Add(this.redoButton);
            this.dotNetToolbar.Buttons.Add(this.fastForwardButton);
            this.dotNetToolbar.Buttons.Add(this.clearHistoryButton);
            this.dotNetToolbar.Buttons.Add(this.limitButton);
            this.dotNetToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = this.imageList;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 132);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.NegotiateToolTips = true;
            this.dotNetToolbar.Size = new System.Drawing.Size(160, 26);
            this.dotNetToolbar.TabIndex = 1;
            this.dotNetToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.dotNetToolbar_ButtonClick);
            // 
            // rewindButton
            // 
            this.rewindButton.ToolTipText = "Undo All (Rewind)";
            // 
            // undoButton
            // 
            this.undoButton.ToolTipText = "Undo (Step Backward)";
            // 
            // redoButton
            // 
            this.redoButton.ToolTipText = "Redo (Step Forward)";
            // 
            // fastForwardButton
            // 
            this.fastForwardButton.ToolTipText = "Redo All (Fast Forward)";
            // 
            // clearHistoryButton
            // 
            this.clearHistoryButton.ToolTipText = "Clear History";
            //
            // limitHistoryButton
            //
            this.limitButton.ToolTipText = "Limit History";
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // HistoryForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(160, 158);
            this.Controls.Add(this.dotNetToolbar);
            this.Controls.Add(this.historyControl);
            this.Name = "HistoryForm";
            this.Text = "History";
            this.Enter += new System.EventHandler(this.HistoryForm_Enter);
            this.Controls.SetChildIndex(this.historyControl, 0);
            this.Controls.SetChildIndex(this.dotNetToolbar, 0);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler ClearHistoryButtonClicked;
        protected virtual void OnClearHistoryButtonClicked()
        {
            if (ClearHistoryButtonClicked != null)
            {
                ClearHistoryButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformClearHistoryClick()
        {
            OnClearHistoryButtonClicked();
        }

        public event EventHandler UndoButtonClicked;
        protected virtual void OnUndoButtonClicked()
        {
            if (UndoButtonClicked != null)
            {
                UndoButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformUndoClick()
        {
            OnUndoButtonClicked();
        }

        public event EventHandler RedoButtonClicked;
        protected virtual void OnRedoButtonClicked()
        {
            if (RedoButtonClicked != null)
            {
                RedoButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformRedoClick()
        {
            OnRedoButtonClicked();
        }
        public event EventHandler RewindButtonClicked;
        protected virtual void OnRewindButtonClicked()
        {
            if (RewindButtonClicked != null)
            {
                RewindButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformRewindClick()
        {
            OnRewindButtonClicked();
        }

        public event EventHandler FastForwardButtonClicked;
        protected virtual void OnFastForwardButtonClicked()
        {
            if (FastForwardButtonClicked != null)
            {
                FastForwardButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformFastForwardClick()
        {
            OnFastForwardButtonClicked();
        }

        public event EventHandler LimitButtonClicked;
        protected virtual void OnLimitButtonClicked()
        {
            if (LimitButtonClicked != null)
            {
                LimitButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformLimitButtonClick()
        {
            OnLimitButtonClicked();
        }

        private void dotNetToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == clearHistoryButton)
            {
                OnClearHistoryButtonClicked();
            }
            else if (e.Button == undoButton)
            {
                OnUndoButtonClicked();                
            }
            else if (e.Button == redoButton)
            {
                OnRedoButtonClicked();                
            }
            else if (e.Button == rewindButton)
            {
                OnRewindButtonClicked();
            }
            else if (e.Button == fastForwardButton)
            {
                OnFastForwardButtonClicked();
            }
            else if (e.Button == limitButton)
            {
                OnLimitButtonClicked();
            }
        }

        private void HistoryForm_Enter(object sender, System.EventArgs e)
        {
            PerformLayout();
        }

        private void historyControl_HistoryChanged(object sender, System.EventArgs e)
        {
            // TODO: remove this abomination
            OnRelinquishFocus();

            // Find reasons to disable the rewind and undo buttons
            if (historyControl.HistoryStack.UndoStack.Count <= 1)
            {
                rewindButton.Enabled = false;
                undoButton.Enabled = false;
            }
            else
            {
                rewindButton.Enabled = true;
                undoButton.Enabled = true;
            }

            // Find reasons to disable the redo and fast forward buttons
            if (historyControl.HistoryStack.RedoStack.Count == 0)
            {
                fastForwardButton.Enabled = false;
                redoButton.Enabled = false;
            }
            else
            {
                fastForwardButton.Enabled = true;
                redoButton.Enabled = true;
            }

            // Find reasons to disable the "clear history" button
            if (historyControl.HistoryStack.UndoStack.Count == 1 &&
                historyControl.HistoryStack.RedoStack.Count == 0)
            {
                this.clearHistoryButton.Enabled = false;
            }
            else
            {
                this.clearHistoryButton.Enabled = true;
            }
        }
    }
}
