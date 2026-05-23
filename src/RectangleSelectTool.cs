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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for RectangleSelectTool.
    /// </summary>
    public class RectangleSelectTool
        : SelectionTool
    {
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            Cursor = cursorMouseDown;
            base.OnMouseDown (e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            Cursor = cursorMouseUp;
            base.OnMouseUp (e);
        }

        protected override ArrayList TrimShapePath(ArrayList tracePoints)
        {
            ArrayList array = new ArrayList();

            if (tracePoints.Count > 0)
            {
                array.Add(tracePoints[0]);

                if (tracePoints.Count > 1)
                {
                    array.Add(tracePoints[tracePoints.Count - 1]);
                }
            }

            return array;
        }

        protected override PointF[] CreateShape(Point[] tracePoints)
        {
            Point a = tracePoints[0];
            Point b = tracePoints[tracePoints.Length - 1];

            Rectangle rect;
            if ((ModifierKeys & Keys.Shift) != 0)
            {
                rect = Utility.PointsToConstrainedRectangle(a, b);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            rect.Intersect(Workspace.Document.Bounds);

            PointF[] shape = new PointF[5];

            shape[0] = new PointF(rect.Left, rect.Top);
            shape[1] = new PointF(rect.Right, rect.Top);
            shape[2] = new PointF(rect.Right, rect.Bottom);
            shape[3] = new PointF(rect.Left, rect.Bottom);
            shape[4] = shape[0];

            return shape;
        }

        protected override void OnActivate()
        {
            this.cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.RectangleSelectToolCursor.cur"));
            this.cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.RectangleSelectToolCursorMouseDown.cur"));
            this.Cursor = cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (cursorMouseUp != null)
            {
                cursorMouseUp.Dispose();
                cursorMouseUp = null;
            }

            if (cursorMouseDown != null)
            {
                cursorMouseDown.Dispose();
                cursorMouseDown = null;
            }
            
            base.OnDeactivate();
        }

        public RectangleSelectTool(DocumentWorkspace workspace)
            : base(workspace,
                   PdnResources.GetImage("Icons.RectangleSelectToolIcon.bmp"),
                   PdnResources.GetString("RectangleSelectTool.Name"),
                   PdnResources.GetString("RectangleSelectTool.HelpText"),
                   's')
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
            }
        }

    }
}
