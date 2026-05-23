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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for MoveSelectionTool.
    /// </summary>
    public class MoveSelectionTool
        : MoveToolBase
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("MoveSelectionTool.Name");
            }
        }

        private class ContextHistoryAction
            : ToolHistoryAction
        {
            [Serializable]
            private class OurHistoryActionData
                : HistoryActionData
            {
                public Context context;

                public OurHistoryActionData(Context context)
                {
                    this.context = (Context)context.Clone();
                }
            }

            protected override HistoryAction OnToolUndo()
            {
                MoveSelectionTool moveSelectionTool = Workspace.Environment.Tool as MoveSelectionTool;

                if (moveSelectionTool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveSelectionTool");
                }

                ContextHistoryAction cha = new ContextHistoryAction(Workspace, moveSelectionTool.context, this.Name, this.Image);
                OurHistoryActionData ohad = (OurHistoryActionData)this.Data;
                Context newContext = ohad.context;

                moveSelectionTool.context.Dispose();
                moveSelectionTool.context = newContext;

                moveSelectionTool.DestroyNubs();

                if (moveSelectionTool.context.lifted)
                {
                    moveSelectionTool.PositionNubs(moveSelectionTool.context.currentMode);
                }

                return cha;
            }

            public ContextHistoryAction(DocumentWorkspace workspace, Context context, string name, Image image)
                : base(workspace, name, image)
            {
                this.Data = new OurHistoryActionData(context);
            }
        }

        protected override void OnActivate()
        {
            Workspace.EnableSelectionTinting = true;

            this.moveToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.MoveSelectionToolCursor.cur"));
            this.Cursor = this.moveToolCursor;

            this.context.offset = new Point(0, 0);
            this.context.liftedBounds = Workspace.Environment.Selection.GetBoundsF();

            this.tracking = false;
            PositionNubs(this.context.currentMode);

            base.OnActivate ();
        }

        protected override void OnDeactivate()
        {
            Workspace.EnableSelectionTinting = false;

            if (this.moveToolCursor != null)
            {
                this.moveToolCursor.Dispose();
                this.moveToolCursor = null;
            }

            if (context.lifted)
            {   
                Drop();
            }

            this.tracking = false;
            DestroyNubs();

            base.OnDeactivate();
        }

        protected override void Drop()
        {
            ContextHistoryAction cha = new ContextHistoryAction(this.Workspace, this.context, this.Name, this.Image);
            this.currentHistoryActions.Add(cha);

            string name = this.Name;
            Image image = this.Image;

            SelectionHistoryAction sha = new SelectionHistoryAction(this.Name, this.Image, this.Workspace);
            this.currentHistoryActions.Add(sha);
            //Workspace.Environment.Selection.CommitInterimTransform();

            this.context.Dispose();
            this.context = new Context();

            this.FlushHistoryActions(PdnResources.GetString("MoveSelectionTool.HistoryAction.DropSelection"));
        }

        protected override void OnSelectionChanging()
        {
            base.OnSelectionChanging();

            if (!dontDrop)
            {
                if (context.lifted)
                {
                    Drop();
                }

                if (tracking)
                {
                    tracking = false;
                }
            }
        }

        protected override void OnSelectionChanged()
        {
            if (!this.context.lifted)
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            base.OnSelectionChanged();
        }

        protected override void OnLift(MouseEventArgs e)
        {
            // do nothing
        }

        protected override void PushContextHistoryAction()
        {
            ContextHistoryAction cha = new ContextHistoryAction(this.Workspace, this.context, null, null);
            this.currentHistoryActions.Add(cha);
        }

        protected override void Render(Point newOffset, bool useNewOffset)
        {
            PositionNubs(this.context.currentMode);
        }

        protected override void PreRender()
        {
            // do nothing
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!tracking)
            {
                return;
            }

            OnMouseMove(e);

            this.rotateNub.Visible = false;
            tracking = false;
            PositionNubs(this.context.currentMode);

            string resourceName;
            switch (this.context.currentMode)
            {
                default:
                    throw new InvalidEnumArgumentException();

                case Mode.Rotate:
                    resourceName = "MoveSelectionTool.HistoryAction.Rotate";
                    break;

                case Mode.Scale:
                    resourceName = "MoveSelectionTool.HistoryAction.Scale";
                    break;

                case Mode.Translate:
                    resourceName = "MoveSelectionTool.HistoryAction.Translate";
                    break;
            }

            this.context.startAngle += this.angleDelta;
            
            string actionName = PdnResources.GetString(resourceName);
            FlushHistoryActions(actionName);
        }

        private void FlushHistoryActions(string name)
        {
            if (this.currentHistoryActions.Count > 0)
            {
                CompoundHistoryAction cha = new CompoundHistoryAction(null, null,
                    (HistoryAction[])this.currentHistoryActions.ToArray(typeof(HistoryAction)));

                string haName;

                if (name == null)
                {
                    haName = this.Name;
                }
                else
                {
                    haName = name;
                }

                Image image = this.Image;

                CompoundToolHistoryAction ctha = new CompoundToolHistoryAction(cha, this.Workspace, haName, image);

                ctha.SeriesGuid = context.seriesGuid;
                Workspace.History.PushNewAction(ctha);

                this.currentHistoryActions.Clear();
            }
        }

        public MoveSelectionTool(DocumentWorkspace workspace)
            : base(workspace,
                   PdnResources.GetImage("Icons.MoveSelectionToolIcon.bmp"),
                   MoveSelectionTool.StaticName,
                   PdnResources.GetString("MoveSelectionTool.HelpText"), // "Click and drag to move a selected region",
                   'm')
        {
            this.context = new Context();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
                DestroyNubs();

                if (this.context != null)
                {
                    this.context.Dispose();
                    this.context = null;
                }
            }
        }

        protected override void OnExecutingHistoryAction(ExecutingHistoryActionEventArgs e)
        {
            this.dontDrop = true;

            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnExecutedHistoryAction(ExecutedHistoryActionEventArgs e)
        {
            if (this.context.lifted)
            {
                Render(context.offset, true);
            }
            else
            {
                DestroyNubs();
                PositionNubs(this.context.currentMode);
            }

            this.dontDrop = false;
        }
    }
}
