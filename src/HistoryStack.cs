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
using System.Collections.Generic;
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
        private List<HistoryAction> undoStack;
        private List<HistoryAction> redoStack;
        private DocumentWorkspace workspace;
        private int stepGroupDepth;

        public List<HistoryAction> UndoStack
        {
            get
            {
                return undoStack;
            }
        }

        public List<HistoryAction> RedoStack
        {
            get
            {
                return redoStack;
            }
        }

        public void BeginStepGroup()
        {
            ++this.stepGroupDepth;
        }

        public void EndStepGroup()
        {
            --this.stepGroupDepth;

            if (this.stepGroupDepth == 0)
            {
                OnFinishedStepGroup();
            }
        }

        public event EventHandler FinishedStepGroup;
        protected void OnFinishedStepGroup()
        {
            if (FinishedStepGroup != null)
            {
                FinishedStepGroup(this, EventArgs.Empty);
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
            undoStack = new List<HistoryAction>();
            redoStack = new List<HistoryAction>();
        }

        private HistoryStack(
            List<HistoryAction> undoStack,
            List<HistoryAction> redoStack)
        {
            this.undoStack = new List<HistoryAction>(undoStack);
            this.redoStack = new List<HistoryAction>(redoStack);
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
            if (redoStack.Count == 0)
            {
                throw new InvalidOperationException("nothing to redo! redo stack is empty");
            }

            try
            {
                HistoryAction topAction = redoStack[0];
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

                    HistoryAction redoAction = redoStack[0];

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

                if (this.stepGroupDepth == 0)
                {
                    OnFinishedStepGroup();
                }
            }

            catch (InvalidOperationException ex)
            {
                // Need to distinguish non-fatal InvalidOperationException from a fatal one
                // (the fatal one is at the top of this method)
                throw new PdnException("Unexpected exception while redoing", ex);
            }
        }

        /// <summary>
        /// Undoes the top of the undo stack, then places the redo action object to the
        /// top of the redo stack.
        /// </summary>
        public void StepBackward()
        {
            if (undoStack.Count == 0)
            {
                throw new InvalidOperationException("nothing to undo! undo stack is empty");
            }

            if (undoStack[undoStack.Count - 1] is NullHistoryAction)
            {
                throw new InvalidOperationException("nothing to undo! undoStack[last] is NullHistoryAction");
            }

            try
            {
                HistoryAction topAction = undoStack[undoStack.Count - 1];
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

                    HistoryAction undoAction = undoStack[undoStack.Count - 1];

                    ExecutingHistoryActionEventArgs ehaea2 = new ExecutingHistoryActionEventArgs(undoAction, false, ehaea1.SuspendTool);
                    OnExecutingHistoryAction(ehaea2);

                    HistoryAction redoAction = undoStack[undoStack.Count - 1].PerformUndo();
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

                if (this.stepGroupDepth == 0)
                {
                    OnFinishedStepGroup();
                }
            }

            catch (InvalidOperationException ex)
            {
                // Need to distinguish non-fatal InvalidOperationException from a fatal one
                // (the fatal one is at the top of this method)
                throw new PdnException("Unexpected exception while undoing", ex);
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

            undoStack = new List<HistoryAction>();
            redoStack = new List<HistoryAction>();
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
            redoStack = new List<HistoryAction>();
            OnChanged();
        }
    }
}
