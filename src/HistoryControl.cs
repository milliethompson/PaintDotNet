/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for HistoryControl
    /// </summary>
    public class HistoryControl : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private HistoryStack historyStack;
        private PanelWithLayout historyControlPanel;
        private ArrayList undoActions;
        private ArrayList redoActions;
        private EventHandler elementClickedDelegate;
        private EventHandler historyChangedDelegate;
        private EventHandler historySteppedBackwardDelegate;
        private EventHandler historySteppedForwardDelegate;
        private EventHandler historyNewActionDelegate;
        private EventHandler historyFlushedDelegate;
		private EventHandler historyTruncatedDelegate;
        private bool scrollIntoView = true; // we use this flag for when the user clicks on a HistoryElement and we don't want to do lots of crazy scrolling and confuse the user

        private sealed class PanelWithLayout
            : PanelEx
        {
            private HistoryControl parentHistoryControl;
            public HistoryControl ParentHistoryControl
            {
                get
                {
                    return parentHistoryControl;
                }

                set
                {
                    this.parentHistoryControl = value;
                    Invalidate();
                }
            }

            public PanelWithLayout()
            {
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                if (this.parentHistoryControl != null)
                {
                    int cursor = this.AutoScrollPosition.Y;
                    int newWidth = this.ClientRectangle.Width;

                    foreach (Control c in parentHistoryControl.undoActions)
                    {
                        if (c.Width != newWidth)
                        {
                            c.Width = newWidth;
                        }

                        if (c.Top != cursor)
                        {
                            c.Top = cursor;
                        }

                        cursor += c.Height;
                    } // foreach

                    foreach (Control c in parentHistoryControl.redoActions)
                    {
                        if (c.Width != newWidth)
                        {
                            c.Width = newWidth;
                        }

                        if (c.Top != cursor)
                        {
                            c.Top = cursor;
                        }

                        cursor += c.Height;
                    } // foreach
                } // if

                base.OnLayout(levent);
            }
        }

        private const int elementHeight = 16;

        public event EventHandler HistoryChanged;
        protected virtual void OnHistoryChanged()
        {
            if (HistoryChanged != null)
            {
                HistoryChanged(this, EventArgs.Empty);
            }
        }

        public HistoryStack HistoryStack
        {
            get
            {
                return historyStack;
            }

            set
            {
                if (historyStack != null )
                {
                    historyStack.NewHistoryAction -= historyNewActionDelegate;
                    historyStack.SteppedForward -= historySteppedForwardDelegate;
                    historyStack.SteppedBackward -= historySteppedBackwardDelegate;
                    historyStack.HistoryFlushed -= historyFlushedDelegate;
					historyStack.HistoryTruncated -= historyTruncatedDelegate;
                    historyStack.Changed -= historyChangedDelegate;
                }

                historyStack = value;

                if (historyStack != null)
                {
                    historyStack.NewHistoryAction += historyNewActionDelegate;
                    historyStack.SteppedForward += historySteppedForwardDelegate;
                    historyStack.SteppedBackward += historySteppedBackwardDelegate;
                    historyStack.HistoryFlushed += historyFlushedDelegate;
					historyStack.HistoryTruncated += historyTruncatedDelegate;
                    historyStack.Changed += historyChangedDelegate;

                    DrawHistory();
                }

                PerformLayout();
            }
        }
        
        private void DrawHistory()
        {
            Point spt = historyControlPanel.AutoScrollPosition;
            this.historyControlPanel.SuspendLayout();

            int cursor = 0;

            foreach (HistoryAction ha in historyStack.UndoStack)
            {
                HistoryElement hec = new HistoryElement();
                
                InitializeHistoryElement(hec, ha, true);
                hec.Location = new Point(0, elementHeight * cursor);
                undoActions.Add(hec);
                historyControlPanel.Controls.Add(hec);
                cursor++;
            }
            
            foreach (HistoryAction ha in historyStack.RedoStack)
            {
                HistoryElement hec = new HistoryElement();

                InitializeHistoryElement(hec, ha, false);
                hec.Location = new Point(0, elementHeight * cursor);
                redoActions.Add(hec);
                historyControlPanel.Controls.Add(hec);
                cursor++;
            }

            historyControlPanel.ResumeLayout(true);
        }

        public void DrawHistoryElement(HistoryElement hec)
        {
            int cursor = historyStack.UndoStack.Count - 1;
            hec.Location = new Point(0, this.historyControlPanel.AutoScrollPosition.Y + elementHeight * cursor);
            historyControlPanel.Controls.Add(hec);          
        }

        private void ClearRedoHistoryControl()
        {
			foreach (HistoryElement hec in redoActions)
			{
				hec.Click -= elementClickedDelegate;
				historyControlPanel.Controls.Remove(hec);
				hec.Dispose();
			}

			redoActions = new ArrayList();
        }

		private void RefreshHistoryControl()
		{
			if ((undoActions.Count - historyStack.UndoStack.Count) > 0)
			{
				for (int i = 0; i < (undoActions.Count - historyStack.UndoStack.Count); i++)
				{
					((HistoryElement)undoActions[i]).Click -= elementClickedDelegate;
					historyControlPanel.Controls.Remove((HistoryElement)undoActions[i]);
				}

				undoActions.RemoveRange(0, undoActions.Count - historyStack.UndoStack.Count);
			}

			if ((redoActions.Count - historyStack.RedoStack.Count) > 0)
			{
				for (int i = 0; i < (redoActions.Count - historyStack.RedoStack.Count); i++)
				{
					((HistoryElement)redoActions[i]).Click -= elementClickedDelegate;
					historyControlPanel.Controls.Remove((HistoryElement)redoActions[i]);
				}

				redoActions.RemoveRange(0, redoActions.Count - historyStack.RedoStack.Count);
			}

			this.PerformLayout();
		}

        private void ClearHistoryControl()
        {
            foreach (HistoryElement hec in undoActions)
            {
                hec.Click -= elementClickedDelegate;
                historyControlPanel.Controls.Remove(hec);
                hec.Dispose();
            }

            undoActions = new ArrayList();

            foreach (HistoryElement hec in redoActions)
            {
                hec.Click -= elementClickedDelegate;
                historyControlPanel.Controls.Remove(hec);
                hec.Dispose();
            }

            redoActions = new ArrayList();
        }

        private void InitializeHistoryElement(HistoryElement hec, HistoryAction ha, bool isUndo)
        {
            hec.Height = elementHeight;
            hec.Width = historyControlPanel.ClientRectangle.Width;
            hec.Description = ha.Name;
            hec.Click += elementClickedDelegate;
            hec.Tag = ha.ID;
            hec.Image = ha.Image;
            hec.IsUndo = isUndo;
        }

        private void HistorySteppedForwardHandler(object sender, EventArgs e)
        {
            // Pull first redo action, make it last undo action
            if (redoActions.Count > 0)
            {
                HistoryElement hec = (HistoryElement)redoActions[0];
                hec.IsUndo = true;
                undoActions.Add(hec);
                redoActions.Remove(hec);

                if (scrollIntoView)
                {
                    if (redoActions.Count > 0)
                    {
                        historyControlPanel.ScrollControlIntoView((Control)redoActions[0]);
                        ((Control)redoActions[0]).Select();
                    }
                    else
                    {
                        historyControlPanel.ScrollControlIntoView(hec);
                        hec.Select();
                    }
                }
            }
        }

        private void HistorySteppedBackwardHandler(object sender, EventArgs e)
        {
            // Pull last undo action, make it first redo action
            if (undoActions.Count > 0)
            {
                HistoryElement hec = (HistoryElement)undoActions[undoActions.Count - 1];
                undoActions.Remove(hec);
                hec.IsUndo = false;
                redoActions.Insert(0, hec);

                if (scrollIntoView)
                {
                    if (undoActions.Count > 0)
                    {
                        historyControlPanel.ScrollControlIntoView((HistoryElement)undoActions[undoActions.Count - 1]);
                        ((HistoryElement)undoActions[undoActions.Count - 1]).Select();
                    }
                    else
                    {
                        historyControlPanel.ScrollControlIntoView(hec);
                        hec.Select();
                    }
                }

            }
        }

        private void HistoryFlushedHandler(object sender, EventArgs e)
        {
            ClearHistoryControl();
        }

		private void HistoryTruncatedHandler(object sender, EventArgs e)
		{
			RefreshHistoryControl();
		}

        private void HistoryNewActionHandler(object sender, EventArgs e)
        {
            HistoryElement hec = new HistoryElement();
            HistoryAction ha = (HistoryAction)this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1];
            InitializeHistoryElement(hec, ha, true);

            // Clear the Redo Stack and Control
            ClearRedoHistoryControl();

            DrawHistoryElement(hec);
            undoActions.Add(hec);

            historyControlPanel.ScrollControlIntoView(hec);
            hec.Select();

            this.PerformLayout();
            historyControlPanel.PerformLayout();
        }
    
        private void HistoryChangedHandler(object sender, EventArgs e)
        {
            OnHistoryChanged();
            Update();
        }

        public HistoryControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            historyStack = null;
            undoActions = new ArrayList();
            redoActions = new ArrayList();
            historySteppedForwardDelegate = new EventHandler(HistorySteppedForwardHandler);
            historySteppedBackwardDelegate = new EventHandler(HistorySteppedBackwardHandler);
            historyNewActionDelegate = new EventHandler(HistoryNewActionHandler);
            elementClickedDelegate = new EventHandler(ElementClickedHandler);
            historyFlushedDelegate = new EventHandler(HistoryFlushedHandler);
            historyChangedDelegate = new EventHandler(HistoryChangedHandler);
			historyTruncatedDelegate = new EventHandler(HistoryTruncatedHandler);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void ElementClickedHandler(object sender, EventArgs e)
        {
            HistoryElement hec = (HistoryElement)sender;
            int haId = (int)hec.Tag;

            scrollIntoView = false;

            if (hec.IsUndo)
            {   // Step back to undo
                if (haId == ((HistoryAction)historyStack.UndoStack[historyStack.UndoStack.Count - 1]).ID)
                {
                    if (historyStack.UndoStack.Count > 1)
                    {
                        using (new WaitCursorChanger(this))
                        {
                            historyStack.StepBackward();
                        }
                    }
                }
                else
                {
                    while (((HistoryAction)historyStack.UndoStack[historyStack.UndoStack.Count - 1]).ID != haId)
                    {
                        using (new WaitCursorChanger(this))
                        {
                            historyStack.StepBackward();
                        }
                    }
                }
            }
            else 
            {   // Step forward to redo
                while (((HistoryAction)historyStack.UndoStack[historyStack.UndoStack.Count - 1]).ID != haId)
                {
                    using (new WaitCursorChanger(this))
                    {
                        historyStack.StepForward();
                    }
                }                  
            }            

            scrollIntoView = true;
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
            this.historyControlPanel = new PanelWithLayout(); //new PaintDotNet.PanelEx();
            this.SuspendLayout();
            // 
            // historyControlPanel
            // 
            this.historyControlPanel.AutoScroll = true;
            this.historyControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyControlPanel.Location = new System.Drawing.Point(0, 0);
            this.historyControlPanel.Name = "historyControlPanel";
            this.historyControlPanel.ScrollPosition = new System.Drawing.Point(0, 0);
            this.historyControlPanel.Size = new System.Drawing.Size(248, 152);
            this.historyControlPanel.TabIndex = 0;
            this.historyControlPanel.ParentHistoryControl = this;
            // 
            // HistoryControl
            // 
            this.Controls.Add(this.historyControlPanel);
            this.Name = "HistoryControl";
            this.Size = new System.Drawing.Size(248, 152);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
