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

        protected override PointF[] CreateShape(PointF[] tracePoints)
        {
            PointF a = tracePoints[0];
            PointF b = tracePoints[tracePoints.Length - 1];

            RectangleF rect;
            if ((ModifierKeys & Keys.Shift) != 0)
            {
                rect = Utility.PointsToConstrainedRectangle(a, b);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            // disallow coordinates on a fractional coordinate
            RectangleF roundedRect = Rectangle.FromLTRB((int)Math.Floor(rect.Left),
                                                        (int)Math.Floor(rect.Top),
                                                        (int)Math.Floor(rect.Right),
                                                        (int)Math.Floor(rect.Bottom));

            roundedRect.Intersect(Workspace.Document.Bounds);

            PointF[] shape = new PointF[5];

            shape[0] = new PointF(roundedRect.Left, roundedRect.Top);
            shape[1] = new PointF(roundedRect.Right, roundedRect.Top);
            shape[2] = new PointF(roundedRect.Right, roundedRect.Bottom);
            shape[3] = new PointF(roundedRect.Left, roundedRect.Bottom);
            shape[4] = shape[0];
            return shape;
        }


        public RectangleSelectTool(DocumentWorkspace workspace)
            : base(workspace,
                   Utility.GetImageResource("Icons.RectangleSelectToolIcon.bmp"),
                   "Rectangle Select",
                   "Allows you to select a rectangular region of the image.",
                   "Click and move the mouse to select a rectangular region of the image. Hold shift to constrain to a square.",
                   's')
        {
			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.RectangleSelectToolCursor.cur"));
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.RectangleSelectToolCursorMouseDown.cur"));
			Cursor = cursorMouseUp;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

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
            }
        }

    }
}
