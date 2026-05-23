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
    public class EllipseSelectTool
        : SelectionTool
    {
        private Cursor cursorMouseUp, cursorMouseDown;

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

            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddEllipse(rect);
            path.Flatten(Utility.IdentityMatrix, 0.25f);

            PointF[] pointsF = path.PathPoints;
            path.Dispose();
            return pointsF;
        }

        public EllipseSelectTool(DocumentWorkspace workspace)
            : base(workspace,
                   Utility.GetImageResource("Icons.EllipseSelectToolIcon.bmp"),
                   "Ellipse Select",
                   "Allows you to select an elliptical region of the image.",
                   "Click and move the mouse to select an elliptical region of the image. Hold shift to constrain to a circle.",
                   's')
        {
            cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.EllipseSelectToolCursor.cur"));
            cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.EllipseSelectToolCursorMouseDown.cur"));
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
