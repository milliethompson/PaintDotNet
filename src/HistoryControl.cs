using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// History User Control : Chris Trevino
	/// Last Modified: 2-21-2004 (Optimization)
	/// </summary>
	///
	public class HistoryControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private HistoryStack historyStack;
		private PanelEx historyControlPanel;
		private ArrayList undoActions;
		private ArrayList redoActions;
		private EventHandler elementClickedDelegate;
        private EventHandler historyChangedDelegate;
		private EventHandler historySteppedBackwardDelegate;
		private EventHandler historySteppedForwardDelegate;
		private EventHandler historyNewActionDelegate;
		private EventHandler historyFlushedDelegate;
        private bool scrollIntoView = true; // we use this flag for when the user clicks on a HistoryElementControl and we don't want to do lots of crazy scrolling and confuse the user

        private static int elementHeight = 16;

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
                    historyStack.Changed -= historyChangedDelegate;
				}

				historyStack = value;

				if (historyStack != null)
				{
					historyStack.NewHistoryAction += historyNewActionDelegate;
					historyStack.SteppedForward += historySteppedForwardDelegate;
					historyStack.SteppedBackward += historySteppedBackwardDelegate;
					historyStack.HistoryFlushed += historyFlushedDelegate;
                    historyStack.Changed += historyChangedDelegate;

                    DrawHistory();
                }

				PerformLayout();
			}
		}
		
		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout (levent);
            int cursor = historyControlPanel.AutoScrollPosition.Y;

			foreach (Control c in this.undoActions)
			{
                c.Width = historyControlPanel.ClientRectangle.Width;
                c.Top = cursor;
                cursor += c.Height;
            }

			foreach (Control c in this.redoActions)
			{
				c.Width = historyControlPanel.ClientRectangle.Width;
                c.Top = cursor;
                cursor += c.Height;
            }
		}

    	private void DrawHistory()
		{
			Point spt = historyControlPanel.AutoScrollPosition;
			this.historyControlPanel.SuspendLayout();

			int cursor = 0;

			foreach (HistoryAction ha in Utility.Reverse(historyStack.UndoStack))
			{
				HistoryElementControl hec = new HistoryElementControl();
				
				InitializeHistoryElementControl(hec, ha, true);
				hec.Location = new Point(0, elementHeight * cursor);
				undoActions.Add(hec);
				historyControlPanel.Controls.Add(hec);
				cursor++;
			}
			
			foreach (HistoryAction ha in historyStack.RedoStack)
			{
				HistoryElementControl hec = new HistoryElementControl();

				InitializeHistoryElementControl(hec, ha, false);
				hec.Location = new Point(0, elementHeight * cursor);
				redoActions.Add(hec);
				historyControlPanel.Controls.Add(hec);
				cursor++;
			}

			historyControlPanel.ResumeLayout(true);
		}

		public void DrawHistoryElementControl(HistoryElementControl hec)
		{
			int cursor = historyStack.UndoStack.Count - 1;
            // we add AutoScrollPosition.Y to the location because it seems this offset
            // is used when you add a control to a Panel ... undocumented?
			hec.Location = new Point(0, this.historyControlPanel.AutoScrollPosition.Y + elementHeight * cursor);
			historyControlPanel.Controls.Add(hec);			
		}

		private void ClearRedoHistoryControl()
		{
			foreach (HistoryElementControl hec in redoActions)
			{
				hec.Click -= elementClickedDelegate;
                historyControlPanel.Controls.Remove(hec);
                hec.Dispose();
			}

            redoActions = new ArrayList();
		}


		private void ClearHistoryControl()
		{
			foreach (HistoryElementControl hec in undoActions)
			{
				hec.Click -= elementClickedDelegate;
                historyControlPanel.Controls.Remove(hec);
                hec.Dispose();
			}

            undoActions = new ArrayList();

            foreach (HistoryElementControl hec in redoActions)
			{
				hec.Click -= elementClickedDelegate;
                historyControlPanel.Controls.Remove(hec);
                hec.Dispose();
			}

            redoActions = new ArrayList();
		}

		private void InitializeHistoryElementControl( HistoryElementControl hec, HistoryAction ha, bool isUndo )
		{
			hec.Height = elementHeight;
			hec.Width = historyControlPanel.ClientRectangle.Width;
			hec.Description = ha.Name;
			hec.Click += elementClickedDelegate;
			hec.Tag = ha.ID;
			hec.Image = ha.Image;
			hec.IsUndo = isUndo;
            //hec.Dock = DockStyle.Top;
		}

		private void HistorySteppedForwardHandler(object sender, EventArgs e)
		{
			// Pull first redo action, make it last undo action
			if( redoActions.Count > 0 )
			{
				HistoryElementControl hec = (HistoryElementControl)redoActions[0];
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

				//historyControlPanel.Invalidate(); //Redraw
				//historyControlPanel.ScrollControlIntoView(temp);
			}
			else 
			{ // Nothing to Step Forward to
				//Debug.WriteLine("Nothing to Redo!");
			}
		}

		private void HistorySteppedBackwardHandler(object sender, EventArgs e)
		{
			// Pull last undo action, make it first redo action
			if( undoActions.Count > 0 )
			{
				HistoryElementControl hec = (HistoryElementControl)undoActions[undoActions.Count - 1];
				undoActions.Remove(hec);
				hec.IsUndo = false;
				redoActions.Insert(0, hec);

                if (scrollIntoView)
                {
                    if (undoActions.Count > 0)
                    {
                        historyControlPanel.ScrollControlIntoView((Control)undoActions[undoActions.Count - 1]);
                        ((Control)undoActions[undoActions.Count - 1]).Select();
                    }
                    else
                    {
                        historyControlPanel.ScrollControlIntoView(hec);
                        hec.Select();
                    }
                }

				//historyControlPanel.Invalidate(); // Redraw
				//historyControlPanel.ScrollControlIntoView(temp);
			}
			else
			{ // Nothing to Step Backward To
				//Debug.WriteLine("Nothing to Undo!");
			}
		}

		private void HistoryFlushedHandler(object sender, EventArgs e)
		{
			ClearHistoryControl();
		}

		private void HistoryNewActionHandler(object sender, EventArgs e)
		{
			HistoryElementControl hec = new HistoryElementControl();
			HistoryAction ha = (HistoryAction)this.historyStack.UndoStack.Peek();
			InitializeHistoryElementControl(hec, ha, true);

			// Clear the Redo Stack and Control
			ClearRedoHistoryControl();

			DrawHistoryElementControl(hec);
			undoActions.Add(hec);

			//historyControlPanel.ScrollControlIntoView(hec);
            //hec.Select();
			
			PerformLayout();

			historyControlPanel.ScrollControlIntoView(hec);
            hec.Select();

            this.PerformLayout();
            historyControlPanel.PerformLayout();
		}
	
		private void HistoryChangedHandler(object sender, EventArgs e)
		{
            OnHistoryChanged();
		}

		public HistoryControl()
		{
            // This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			historyStack = null;
			undoActions = new ArrayList();
			redoActions = new ArrayList();
			historySteppedForwardDelegate = new EventHandler(HistorySteppedForwardHandler);
            historySteppedBackwardDelegate = new EventHandler(HistorySteppedBackwardHandler);
			historyNewActionDelegate = new EventHandler(HistoryNewActionHandler);
			elementClickedDelegate = new EventHandler(ElementClickedHandler);
			historyFlushedDelegate = new EventHandler(HistoryFlushedHandler);
            historyChangedDelegate = new EventHandler(HistoryChangedHandler);
		}

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

		private void ElementClickedHandler(object sender, EventArgs e)
		{
			HistoryElementControl hec = (HistoryElementControl) sender;
			int haId = (int)hec.Tag;

            scrollIntoView = false;

			// Step Back To Undo
			if (hec.IsUndo)
			{
				while(((HistoryAction)historyStack.UndoStack.Peek()).ID != haId)
				{
					historyStack.StepBackward();
				}
			}
			else // Step Forward Re-Doing
			{
				while(((HistoryAction)historyStack.UndoStack.Peek()).ID != haId)
				{
					historyStack.StepForward();
				}				   
			}

            scrollIntoView = true;
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
            this.historyControlPanel = new PaintDotNet.PanelEx();
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
            this.historyControlPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.historyControlPanel_Layout);
            // 
            // HistoryControl
            // 
            this.Controls.Add(this.historyControlPanel);
            this.Name = "HistoryControl";
            this.Size = new System.Drawing.Size(248, 152);
            this.ResumeLayout(false);

        }
		#endregion

        private void historyControlPanel_Layout(object sender, System.Windows.Forms.LayoutEventArgs e)
        {
            PerformLayout();
        }
    }
}
