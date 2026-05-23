/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// The HistoryStack class for the History "concept".  
	/// Serves as the undo and redo stacks.  
	/// </summary>
    [Serializable]
    public class HistoryStack
    {
        private ArrayList undoStack;
        private ArrayList redoStack;
        private DocumentWorkspace workspace;

        public ArrayList UndoStack
        {
            get
            {
                return undoStack;
            }
        }

        public ArrayList RedoStack
        {
            get
            {
                return redoStack;
            }
        }

        public event EventHandler SteppedBackward;
        protected void OnSteppedBackward()
        {
            if (SteppedBackward != null)
            {
                SteppedBackward(this, EventArgs.Empty);
            }
        }

        public event EventHandler SteppedForward;
        protected void OnSteppedForward()
        {
            if (SteppedForward != null)
            {
                SteppedForward(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for when a new history action has been added.
        /// </summary>
        public event EventHandler NewHistoryAction;
        protected void OnNewHistoryAction()
        {
            if (NewHistoryAction != null)
            {
                NewHistoryAction(this, EventArgs.Empty);
            }
        }
                
        /// <summary>
		/// Event handler for when changes have been made to the history.
        /// </summary>
        public event EventHandler Changed;
        protected void OnChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        public event EventHandler Changing;
        protected void OnChanging()
        {
            if (Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }
        }

        public event EventHandler HistoryFlushed;
        protected void OnHistoryFlushed()
        {
            if (HistoryFlushed != null)
            {
                HistoryFlushed(this, EventArgs.Empty);
            }
        }

		public event EventHandler HistoryTruncated;
		protected void OnHistoryTruncated()
		{
			if (HistoryTruncated != null)
			{
				HistoryTruncated(this, EventArgs.Empty);
			}
		}

        public void PerformChanged()
        {
            OnChanged();
        }

        public HistoryStack(DocumentWorkspace workspace)
        {
            this.workspace = workspace;
            undoStack = new ArrayList();
            redoStack = new ArrayList();
        }

        private HistoryStack(ArrayList undoStack, ArrayList redoStack)
        {
            this.undoStack = (ArrayList)undoStack.Clone();
            this.redoStack = (ArrayList)redoStack.Clone();
        }

        /// <summary>
        /// When the user does something new, it will clear out the redo stack.
        /// </summary>
        public void PushNewAction(HistoryAction value)
        {
            OnChanging();

			ClearRedoStack();
            undoStack.Add(value);
			OnNewHistoryAction();

            OnChanged();

			if ((undoStack.Count > limit) && (limit > 1))
			{
				Truncate();
			}

            value.Flush();
        }

        /// <summary>
        /// Takes one item from the redo stack, "redoes" it, then places the redo
        /// action object to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            OnChanging();
            Tool oldTool = workspace.Environment.Tool;
            workspace.Environment.SetTool(null);

            HistoryAction redoAction = (HistoryAction)redoStack[0];
            HistoryAction undoAction = redoAction.PerformUndo();
			
            redoStack.RemoveAt(0);
            undoStack.Add(undoAction);

            OnChanged();
            OnSteppedForward();

            undoAction.Flush();
            //redoAction.Flush();
            workspace.Environment.SetTool(oldTool);
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo action object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            OnChanging();
            HistoryAction topAction = (HistoryAction)undoStack[undoStack.Count - 1];

            Tool oldTool = null;
            if (!(topAction is SentinelHistoryAction))
            {                                                    
                oldTool = workspace.Environment.Tool;
                workspace.Environment.SetTool(null);
            }

            HistoryAction undoAction = (HistoryAction)undoStack[undoStack.Count - 1];
            HistoryAction redoAction = ((HistoryAction)undoStack[undoStack.Count - 1]).PerformUndo();
            undoStack.RemoveAt(undoStack.Count - 1);
            redoStack.Insert(0, redoAction);

            OnChanged();
            OnSteppedBackward();

            redoAction.Flush();

            if (oldTool != null)
            {
                workspace.Environment.SetTool(oldTool);
            }
        }

        public void ClearAll()
        {
            OnChanging();

            foreach (HistoryAction ha in undoStack)
            {
                ha.Flush();
            }

            foreach (HistoryAction ha in redoStack)
            {
                ha.Flush();
            }

            undoStack = new ArrayList();
            redoStack = new ArrayList();
            OnChanged();
            OnHistoryFlushed();
        }

        public void ClearRedoStack()
        {
            foreach (HistoryAction ha in redoStack)
            {
                ha.Flush();
            }

            OnChanging();
            redoStack = new ArrayList();
            OnChanged();
        }

		/// <summary>
		/// Truncates the history stack(s) to the length specified by
		///  the Limit property.
		/// </summary>
		public void Truncate()
		{
			if (limit < 1)
			{
				return;
			}

			int redoToDrop = Math.Min(Math.Max(redoStack.Count + (undoStack.Count - limit), 0), redoStack.Count);
			int undoToDrop = Math.Max(undoStack.Count - limit, 0) + redoToDrop;

			while (redoToDrop > 0)
			{
				StepForward();
				redoToDrop--;
			}

            OnChanging();

            for (int i = 0; i < undoToDrop; ++i)
            {
                ((HistoryAction)undoStack[i]).Flush();
            }

			undoStack.RemoveRange(0, undoToDrop);
            OnChanged();
			OnHistoryTruncated();
		}

		/// <summary>
		/// Sets or gets the limit on the HistoryStack.
		/// </summary>
		private int limit = -1;
		public int Limit
		{
			set
			{
				if ((value == -1) || (value > 9))
				{
					limit = value;
					Truncate();
				}
			}

			get
			{
				return(limit);
			}
		}
    }
}
