using System;
using System.Collections;

namespace PaintDotNet
{
	/// <summary>
	/// The HistoryStack class for the History "concept".  
	/// Serves as the undo and redo stacks.  
	/// </summary>
	/// Parameters: Overloaded 1) document workspace or 2) undo stack, redo stack
	/// Properties: Limit, RedoStack, UndoStack
	/// Returns: nothing
	/// Outstanding: Stack is now a misnomer and probably should be changed to reflect the object type used in the History implementation.
	/// Initial Conception: Paint.NET v1.0 Team
	/// ..Alterations: provided the History "stack" concept to the History Control
	/// Changes: Michael Kelsey
	/// ..Alterations: changed to an ArrayList to accomodate Limited History Length
	/// ..Alterations: modified the following:
	///    public Stack UndoStack -> public ArrayList UndoStack
	///    public Stack RedoStack -> public ArrayList RedoStack
	///    public HistoryStack(DocumentWorkspace workspace)
	///    private HistoryStack(Stack undoStack, Stack redoStack) -> private HistoryStack(ArrayList undoStack, ArrayList redoStack)
	///    public void PushNewAction(HistoryAction value)
	///    public void StepForward()
	///    public void StepBackward()
	///    public void ClearAll()
	///    public void ClearRedoStack()
	/// ..Alterations: added the following:
	///    public event EventHandler HistoryTruncated
	///       Purpose: provides an event handler to which delegates can subscribe
	///	   protected void OnHistoryTruncated()
	///	      Purpose: provides a manual invocation of the HistoryTruncated event
	///	   public void Truncate()
	///	      Purpose: truncates the UndoStack and RedoStack to the value held in limit.
	///    private int limit
	///       Purpose: holds the limit as configured by the HistoryLimitDialog
	///    public int Limit
	///       Purpose: property for setting limit
    [Serializable]
    public class HistoryStack
        : ICloneable
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
			Utility.GCFullCollect();

			ClearRedoStack();
            undoStack.Add(value);
			OnNewHistoryAction();

			if ((undoStack.Count > limit) && (limit > 1))
			{
				Truncate();
			}
			else
			{
				OnChanged();
			}
			Utility.GCFullCollect();
        }

        /// <summary>
        /// Takes one item from the redo stack, "redoes" it, then places the redo
        /// action object to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            Tool oldTool = workspace.Environment.Tool;
            workspace.Environment.SetTool(null);

            HistoryAction undoAction = ((HistoryAction)redoStack[0]).PerformUndo();
			
            redoStack.RemoveAt(0);
            undoStack.Add(undoAction);

            OnChanged();
            OnSteppedForward();

            workspace.Environment.SetTool(oldTool);
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo action object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            Tool oldTool = workspace.Environment.Tool;
            workspace.Environment.SetTool(null);

            HistoryAction redoAction = ((HistoryAction)undoStack[undoStack.Count - 1]).PerformUndo();
            undoStack.RemoveAt(undoStack.Count - 1);
            redoStack.Insert(0,redoAction);

            OnChanged();
            OnSteppedBackward();

            workspace.Environment.SetTool(oldTool);
        }

        public void ClearAll()
        {
			Utility.GCFullCollect();        
			undoStack = new ArrayList();
            redoStack = new ArrayList();
            OnChanged();
            OnHistoryFlushed();
            Utility.GCFullCollect();        
        }

        public void ClearRedoStack()
        {
			Utility.GCFullCollect();        
			redoStack = new ArrayList();
            OnChanged();
            Utility.GCFullCollect();        
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

			undoStack.RemoveRange(0, undoToDrop);
			OnHistoryTruncated();
			Utility.GCFullCollect();  

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

        #region ICloneable Members

        public object Clone()
        {
            return new HistoryStack(undoStack, redoStack);
        }

        #endregion
    }
}
