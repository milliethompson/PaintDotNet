/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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

        public event ExecutingHistoryActionEventHandler ExecutingHistoryAction;
        protected void OnExecutingHistoryAction(ExecutingHistoryActionEventArgs e)
        {
            if (ExecutingHistoryAction != null)
            {
                ExecutingHistoryAction(this, e);
            }
        }

        public event ExecutedHistoryActionEventHandler ExecutedHistoryAction;
        protected void OnExecutedHistoryAction(ExecutedHistoryActionEventArgs e)
        {
            if (ExecutedHistoryAction != null)
            {
                ExecutedHistoryAction(this, e);
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

            OnChanging();

            ClearRedoStack();
            undoStack.Add(value);
            OnNewHistoryAction();

            OnChanged();

            value.Flush();
            Utility.GCFullCollect();
        }

        /// <summary>
        /// Takes one item from the redo stack, "redoes" it, then places the redo
        /// action object to the top of the undo stack.
        /// </summary>
        public void StepForward()
        {
            HistoryAction topAction = (HistoryAction)(HistoryAction)redoStack[0];
            ToolHistoryAction asToolHistoryAction = topAction as ToolHistoryAction;

            if (asToolHistoryAction != null && asToolHistoryAction.ToolType != workspace.Environment.GetToolType())
            {
                workspace.Environment.SetTool(asToolHistoryAction.ToolType, this.workspace);
                StepForward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryActionEventArgs ehaea1 = new ExecutingHistoryActionEventArgs(topAction, true, false);

                if (asToolHistoryAction == null && (!(topAction is SentinelHistoryAction) || topAction.SeriesGuid != Guid.Empty))
                {
                    ehaea1.SuspendTool = true;
                }

                Tool oldTool = null;
                OnExecutingHistoryAction(ehaea1);

                if (ehaea1.SuspendTool)
                {                                                    
                    oldTool = workspace.Environment.Tool;
                    workspace.Environment.SetTool(null);
                }
            
                HistoryAction redoAction = (HistoryAction)redoStack[0];

                // Possibly useful invariant here:
                //     ehaea1.HistoryAction.SeriesGuid == ehaea2.HistoryAction.SeriesGuid == ehaea3.HistoryAction.SeriesGuid
                ExecutingHistoryActionEventArgs ehaea2 = new ExecutingHistoryActionEventArgs(redoAction, false, ehaea1.SuspendTool);
                OnExecutingHistoryAction(ehaea2);

                HistoryAction undoAction = redoAction.PerformUndo();
            
                redoStack.RemoveAt(0);
                undoStack.Add(undoAction);

                ExecutedHistoryActionEventArgs ehaea3 = new ExecutedHistoryActionEventArgs(undoAction);
                OnExecutedHistoryAction(ehaea3);

                OnChanged();
                OnSteppedForward();

                undoAction.Flush();

                if (oldTool != null)
                {
                    workspace.Environment.SetTool(oldTool);
                }       
            }
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo action object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            HistoryAction topAction = (HistoryAction)undoStack[undoStack.Count - 1];
            ToolHistoryAction asToolHistoryAction = topAction as ToolHistoryAction;

            if (asToolHistoryAction != null && asToolHistoryAction.ToolType != workspace.Environment.GetToolType())
            {
                workspace.Environment.SetTool(asToolHistoryAction.ToolType, this.workspace);
                StepBackward();
            }
            else
            {
                OnChanging();

                ExecutingHistoryActionEventArgs ehaea1 = new ExecutingHistoryActionEventArgs(topAction, true, false);

                if (asToolHistoryAction == null && (topAction.SeriesGuid == Guid.Empty && !(topAction is SentinelHistoryAction)))
                {
                    ehaea1.SuspendTool = true;
                }

                OnExecutingHistoryAction(ehaea1);

                Tool oldTool = null;
                if (ehaea1.SuspendTool)
                {
                    oldTool = workspace.Environment.Tool;
                    workspace.Environment.SetTool(null);
                }

                HistoryAction undoAction = (HistoryAction)undoStack[undoStack.Count - 1];

                ExecutingHistoryActionEventArgs ehaea2 = new ExecutingHistoryActionEventArgs(undoAction, false, ehaea1.SuspendTool);
                OnExecutingHistoryAction(ehaea2);

                HistoryAction redoAction = ((HistoryAction)undoStack[undoStack.Count - 1]).PerformUndo();
                undoStack.RemoveAt(undoStack.Count - 1);
                redoStack.Insert(0, redoAction);

                // Possibly useful invariant here:
                //     ehaea1.HistoryAction.SeriesGuid == ehaea2.HistoryAction.SeriesGuid == ehaea3.HistoryAction.SeriesGuid
                ExecutedHistoryActionEventArgs ehaea3 = new ExecutedHistoryActionEventArgs(redoAction);
                OnExecutedHistoryAction(ehaea3);

                OnChanged();
                OnSteppedBackward();

                redoAction.Flush();

                if (oldTool != null)
                {
                    workspace.Environment.SetTool(oldTool);
                }
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
    }
}
