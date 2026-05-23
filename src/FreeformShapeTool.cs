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
    /// Summary description for FreeformShapeTool.
    /// </summary>
    public class FreeformShapeTool
        : ShapeTool 
    {
        private Cursor freeformShapeToolCursor;

        protected override RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] points, PdnGraphicsPath path)
        {
            return Utility.SimplifyTrace(path.PathPoints);
        }

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            // make sure we don't screw them up
            if (points.Length < 2)
            {
                return null;
            }

            // make sure the shape has an area of at least 1
            // we can determine this by making sure that all the Points in points are not all the same
            bool allTheSame = true;
            foreach (PointF pt in points)
            {
                if (pt != points[0])
                {
                    allTheSame = false;
                    break;
                }
            }

            if (allTheSame)
            {
                return null;
            }

            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddLines(points);
            path.AddLine(points[points.Length - 1], points[0]);
            path.CloseAllFigures();
            return path;
        }

        public FreeformShapeTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.FreeformShapeToolIcon.bmp"),
                   "Freeform Shape",
                   "Draws a freeform shape",
                   "Left click to draw a freeform shape with the foreground color, right click to use the background color")
        {
            this.freeformShapeToolCursor = new Cursor(Utility.GetResourceStream("Cursors.FreeformShapeToolCursor.cur"));
            this.Cursor = this.freeformShapeToolCursor;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (this.freeformShapeToolCursor != null)
                {
                    this.freeformShapeToolCursor.Dispose();
                    this.freeformShapeToolCursor = null;
                }
            }
        }

    }
}
