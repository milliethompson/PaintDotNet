using System;
using System.Collections;

namespace PaintDotNet
{
    [Serializable]
    public class HistoryStack
		: ICloneable
	{
		private Stack undoStack;
		private Stack redoStack;
        private DocumentWorkspace workspace;

		public Stack UndoStack
		{
			get
			{
				return undoStack;
			}
		}

		public Stack RedoStack
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
		/// Added by C. Trevino - For Optimization
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
		/// DO NOT USE THIS EVENT HANDLER, IT IS EVIL
		/// -CT
		/// 
		/// Dude, you're going to make me cry! 
		/// -RB
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

		public void PerformChanged()
		{
			OnChanged();
		}

		public HistoryStack(DocumentWorkspace workspace)
		{
            this.workspace = workspace;
			undoStack = new Stack();
			redoStack = new Stack();
		}

		private HistoryStack(Stack undoStack, Stack redoStack)
		{
			this.undoStack = (Stack)undoStack.Clone();
			this.redoStack = (Stack)redoStack.Clone();
		}

		/// <summary>
		/// When the user does something new, it will clear out the redo stack.
		/// </summary>
		public void PushNewAction(HistoryAction value)
		{
			ClearRedoStack();
			undoStack.Push(value);
			OnChanged();
			OnNewHistoryAction();
            GC.Collect();
        }

		/// <summary>
		/// Takes one item from the redo stack, "redoes" it, then places the redo
		/// action object to the top of the undo stack.
		/// </summary>
		public void StepForward()
		{
            Tool oldTool = workspace.Environment.Tool;
            workspace.Environment.SetTool(null);

			HistoryAction undoAction = ((HistoryAction)redoStack.Peek()).PerformUndo();
			redoStack.Pop();
            undoStack.Push(undoAction);

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

			HistoryAction redoAction = ((HistoryAction)undoStack.Peek()).PerformUndo();
			undoStack.Pop();
			redoStack.Push(redoAction);

			OnChanged();
            OnSteppedBackward();

            workspace.Environment.SetTool(oldTool);
        }

		public void ClearAll()
		{
            undoStack = new Stack();
            redoStack = new Stack();
            OnChanged();
			OnHistoryFlushed();
            GC.Collect();
		}

		public void ClearRedoStack()
		{
			//redoStack.Clear();
            redoStack = new Stack();
			OnChanged();
            GC.Collect();
        }

		#region ICloneable Members

		public object Clone()
		{
			return new HistoryStack(undoStack, redoStack);
		}

		#endregion
	}
}
