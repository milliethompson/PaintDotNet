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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for SelectionTool.
    /// </summary>
    public class SelectionTool
        : Tool
    {
        private bool tracking = false;
        private bool moveOriginMode = false;
        private Point lastXY;
        private SelectionHistoryAction undoAction;
        private CombineMode combineMode;
        private ArrayList tracePoints = null;
        private DateTime startTime;
        private bool hasMoved = false;
        private bool append = false;
        private bool wasNotEmpty = false;

        protected override void OnActivate()
        {
            Workspace.EnableSelectionTinting = true;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Workspace.EnableSelectionTinting = false;
            base.OnDeactivate ();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (tracking)
            {
                moveOriginMode = true;
                lastXY = new Point(e.X, e.Y);
                OnMouseMove(e);
            }
            else if ((e.Button & MouseButtons.Left) == MouseButtons.Left ||
                (e.Button & MouseButtons.Right) == MouseButtons.Right)
            {
                tracking = true;
                hasMoved = false;
                startTime = DateTime.Now;

                tracePoints = new ArrayList();
                tracePoints.Add(new Point(e.X, e.Y));

                undoAction = new SelectionHistoryAction("sentinel", this.Image, Workspace);

                wasNotEmpty = !Workspace.Environment.Selection.IsEmpty;

                // if the user is holding down the control key then we don't want to reset the path, merely append to it
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    append = true;

                    if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                    {
                        this.combineMode = CombineMode.Xor;
                    }
                    else
                    {
                        this.combineMode = CombineMode.Union;
                    }

                    Workspace.Environment.Selection.ResetContinuation();
                }
                else if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                {
                    append = true;
                    this.combineMode = CombineMode.Exclude;
                    Workspace.Environment.Selection.ResetContinuation();
                }
                else
                {
                    append = false;
                    this.combineMode = CombineMode.Replace;
                    Workspace.Environment.Selection.Reset();
                }
            }
        }

        protected virtual ArrayList TrimShapePath(ArrayList tracePoints)
        {
            return tracePoints;
        }

        protected virtual PointF[] CreateShape(Point[] tracePoints)
        {
            return Utility.PointArrayToPointFArray(tracePoints);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (moveOriginMode)
            {
                Size delta = new Size(e.X - lastXY.X, e.Y - lastXY.Y);
                
                for (int i = 0; i < tracePoints.Count; ++i)
                {
                    Point pt = (Point)tracePoints[i];
                    pt.X += delta.Width;
                    pt.Y += delta.Height;
                    tracePoints[i] = pt;
                }

                lastXY = new Point(e.X, e.Y);
                Render();
            }
            else if (tracking)
            {
                Point mouseXY = new Point(e.X, e.Y);

                if (mouseXY != (Point)tracePoints[tracePoints.Count - 1])
                {
                    tracePoints.Add(mouseXY);
                }
                
                hasMoved = true;
                Render();
            }
        }

        private PointF[] CreateSelectionPolygon()
        {
            ArrayList trimmedTrace = this.TrimShapePath(tracePoints);
            Point[] points = (Point[])trimmedTrace.ToArray(typeof(Point));
            PointF[] shapePoints = CreateShape(points);
            PointF[] polygon = Utility.SutherlandHodgman(Workspace.Document.Bounds, shapePoints);
            return polygon;
        }

        private void Render()
        {
            if (tracePoints != null && tracePoints.Count > 2)
            {
                PointF[] polygon = CreateSelectionPolygon();

                if (polygon.Length > 2)
                {
                    Workspace.ResetOutlineWhiteOpacity();
                    Workspace.Environment.Selection.SetContinuation(polygon, this.combineMode);
                    Update();
                }
            }
        }

        private enum WhatToDo
        {
            Clear,
            Emit,
            Reset,
        }

        private void Done()
        {
            if (tracking)
            {
                // Truth table for what we should do based on three flags:
                //  append  | moved | tooQuick | result                             | optimized expression to yield true
                // ---------+-------+----------+-----------------------------------------------------------------------
                //     F    |   T   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   T   |    F     | emit new selected area             | !append && moved && !tooQuick
                //     F    |   F   |    T     | clear selection                    | !append && (!moved || tooQuick)
                //     F    |   F   |    F     | clear selection                    | !append && (!moved || tooQuick)
                //     T    |   T   |    T     | append to selection                | append && moved
                //     T    |   T   |    F     | append to selection                | append && moved
                //     T    |   F   |    T     | reset selection                    | append && !moved
                //     T    |   F   |    F     | reset selection                    | append && !moved
                //
                // append   --> If the user was holding control, then true. Else false.
                // moved    --> If they never moved the mouse, false. Else true.
                // tooQuick --> If they held the mouse button down for more than 50ms, false. Else true.
                //
                // "Clear selection" means to result in no selected area. If the selection area was previously empty,
                //    then no HistoryAction is emitted. Otherwise a Deselect HistoryAction is emitted.
                // "Reset selection" means to reset the selected area to how it was before interaction with the tool,
                //    without a HistoryAction.

                PointF[] polygon = CreateSelectionPolygon();

                // They were "too quick" if they weren't doing a selection for more than 50ms
                // This takes care of the case where someone wants to click to deselect, but accidentally moves
                // the mouse. This happens VERY frequently.
                bool tooQuick = Utility.TicksToMs((DateTime.Now - startTime).Ticks) <= 50;

                // If their selection was completedly out of bounds, it will be clipped
                bool clipped = (polygon.Length == 0);
                WhatToDo whatToDo;

                hasMoved &= (polygon.Length > 1);

                // If their selection gets completely clipped (i.e. outside the image canvas),
                // then result in a no-op
                if (append)
                {
                    if (!hasMoved || clipped)
                    {   
                        whatToDo = WhatToDo.Reset;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                }
                else
                {
                    if (hasMoved && !tooQuick && !clipped)
                    {   
                        whatToDo = WhatToDo.Emit;
                    }
                    else
                    {   
                        whatToDo = WhatToDo.Clear;
                    }
                }

                switch (whatToDo)
                {
                    case WhatToDo.Clear:
                        if (wasNotEmpty)
                        {
                            // emit a deselect history action
                            undoAction.Name = DeselectAction.StaticName;
                            undoAction.Image = DeselectAction.StaticImage;
                            Workspace.History.PushNewAction(undoAction);
                        }

                        Workspace.Environment.Selection.Reset();
                        break;

                    case WhatToDo.Emit:
                        // emit newly selected area
                        undoAction.Name = this.Name;
                        Workspace.History.PushNewAction(undoAction);
                        Workspace.Environment.Selection.CommitContinuation();
                        break;

                    case WhatToDo.Reset:
                        // reset selection, no HistoryAction
                        Workspace.Environment.Selection.ResetContinuation();
                        break;
                }

                Workspace.ResetOutlineWhiteOpacity();
                tracking = false;
                Workspace.DocumentView.InvalidateSurface(Workspace.DocumentView.VisibleDocumentRectangle);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnMouseMove(e);

            if (moveOriginMode)
            {
                moveOriginMode = false;
            }
            else
            {
                Done();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (tracking)
            {
                Render();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (tracking)
            {
                Render();
            }
        }

        protected override void OnClick()
        {
            base.OnClick();
            
            if (!moveOriginMode)
            {
                Done();
            }
        }

        public SelectionTool(DocumentWorkspace workspace,
            Image toolBarImage,
            string name,
            string helpText,
            char hotKey)
            : base(workspace,
            toolBarImage,
            name,
            helpText,
            hotKey)
        {
            tracking = false;
        }
    }
}
