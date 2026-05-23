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
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for RectangleTool.
    /// </summary>
    public class RectangleTool
        : ShapeTool 
    {
        private Cursor rectangleToolCursor;

        protected override ArrayList TrimShapePath(ArrayList points)
        {
            ArrayList array = new ArrayList();

            if (points.Count > 0)
            {
                array.Add(points[0]);

                if (points.Count > 1)
                {
                    array.Add(points[points.Count - 1]);
                }
            }

            return array;
        }

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            if (Workspace.Environment.PenInfo.Width == 1.0f)
            {
                return PixelOffsetMode.None;
            }

            return base.GetPixelOffsetMode ();
        }

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            PointF a = points[0];
            PointF b = points[points.Length - 1];
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
            path.AddRectangle(rect);
            path.CloseFigure();
            path.Reverse();
            return path;
        }

        public RectangleTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.RectangleToolIcon.bmp"),
                   "Rectangle",
                   "Draws a rectangle",
                   "Click and drag to draw a rectangle (right click for background color). Hold shift to constrain to a square.")
        {
            rectangleToolCursor = new Cursor(Utility.GetResourceStream("Cursors.RectangleToolCursor.cur"));
            this.Cursor = rectangleToolCursor;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (rectangleToolCursor != null)
                {
                    rectangleToolCursor.Dispose();
                    rectangleToolCursor = null;
                }
            }
        }
    }
}
