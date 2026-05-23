/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
        private SelectionHistoryAction undoAction;
        private PdnGraphicsPath originalCopy;
        private ArrayList tracePoints = null;
        private DateTime startTime;
        private bool hasMoved = false;
        private bool append = false;
        private bool wasNotEmpty = false;

        protected override bool SupportsInk
        {
            get
            {
                return true;
            }
        }

        protected override void OnStylusDown(StylusEventArgs e)
        {
            base.OnStylusDown(e);

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                tracking = true;
                hasMoved = false;
                startTime = DateTime.Now;

                tracePoints = new ArrayList();
                tracePoints.Add(new PointF(e.Fx, e.Fy));

                undoAction = new SelectionHistoryAction("sentinel", this.Image, Workspace);

                wasNotEmpty = !Workspace.Environment.IsSelectionEmpty;

                // if the user is holding down the control key then we don't want to reset the path, merely append to it
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    append = true;
                }
                else
                {
                    append = false;
                    Workspace.Environment.PerformSelectedPathChanging();
                    Workspace.Environment.SelectedPath.Reset();
                    Workspace.Environment.SelectedPath.CloseAllFigures();
                    Workspace.Environment.PerformSelectedPathChanged();
                }

                if (Workspace.Environment.IsSelectionEmpty)
                {
                    originalCopy = null;
                }
                else
                {
                    originalCopy = (PdnGraphicsPath)Workspace.Environment.SelectedPath.Clone();
                }
            }
        }

        protected virtual ArrayList TrimShapePath(ArrayList tracePoints)
        {
            return tracePoints;
        }

        protected virtual PointF[] CreateShape(PointF[] tracePoints)
        {
            return tracePoints;
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);

            if (tracking)
            {
                PointF mouseXY = new PointF(e.Fx, e.Fy);

                if (mouseXY != (PointF)tracePoints[tracePoints.Count - 1])
                {
                    tracePoints.Add(mouseXY);
                }

                hasMoved = true;
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //OnStylusMove(new StylusEventArgs(e));

            if (tracking)
            {
                if (tracePoints.Count > 2)
                {
                    ArrayList trimmedTrace = this.TrimShapePath(tracePoints);
                    PointF[] points = (PointF[])trimmedTrace.ToArray(typeof(PointF));
                    PointF[] shapePointsF = CreateShape(points);
                    PointF[] polygon = Utility.SutherlandHodgman(Workspace.Document.Bounds, shapePointsF);

                    if (polygon.Length > 2)
                    {
                        Workspace.DocumentView.ResetOutlineWhiteOpacity();
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.Reset();

                        if (originalCopy != null)
                        {
                            Workspace.Environment.SelectedPath.AddPath(originalCopy, false);
                        }

                        Workspace.Environment.SelectedPath.CloseFigure();
                        //Workspace.Environment.SelectedPath.AddPolygon(polygon);

                        for (int i = 0; i < polygon.Length - 1; ++i)
                        {
                            Workspace.Environment.SelectedPath.AddLine(polygon[i], polygon[i + 1]);
                        }

                        //Workspace.Environment.SelectedPath.CloseFigure();
                        Workspace.Environment.PerformSelectedPathChanged();
                        Workspace.Update();
                    }
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

                PointF[] polygon = Utility.SutherlandHodgman((RectangleF)Workspace.Document.Bounds, tracePoints);

                // They were "too quick" if they weren't doing a selection for more than 50ms
                // This takes care of the case where someone wants to click to deselect, but accidentally moves
                // the mouse or stylus. This happens VERY frequently.
                bool tooQuick = Utility.TicksToMs((DateTime.Now - startTime).Ticks) <= 50;

                // If their selection was completedly out of bounds, it will be clipped
                bool clipped = (polygon.Length == 0);
                WhatToDo whatToDo;

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

                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.Reset();
                        Workspace.Environment.PerformSelectedPathChanged();

                        break;

                    case WhatToDo.Emit:
                        // emit newly selected area
                        undoAction.Name = this.Name;
                        Workspace.History.PushNewAction(undoAction);
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.CloseFigure();
                        Workspace.Environment.PerformSelectedPathChanged();
                        break;

                    case WhatToDo.Reset:
                        // reset selection, no HistoryAction
                        Workspace.Environment.PerformSelectedPathChanging();
                        Workspace.Environment.SelectedPath.Reset();

                        if (originalCopy != null)
                        {
                            Workspace.Environment.SelectedPath.AddPath(originalCopy, false);
                        }

                        Workspace.Environment.PerformSelectedPathChanged();
                        break;
                }

                Workspace.DocumentView.ResetOutlineWhiteOpacity();
                tracking = false;
                Workspace.DocumentView.InvalidateSurface(Workspace.DocumentView.VisibleDocumentRectangle);
            }
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);
            Done();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        protected override void OnClick()
        {
            base.OnClick ();
            Done();
        }

        public SelectionTool(DocumentWorkspace workspace,
                             Image toolBarImage,
                             string name,
                             string description,
                             string helpText,
                             char hotKey)
            : base(workspace,
                   toolBarImage,
                   name,
                   description,
                   helpText,
                   hotKey)
        {
            tracking = false;
        }
    }
}
