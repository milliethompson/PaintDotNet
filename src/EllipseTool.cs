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
    public class EllipseTool
        : ShapeTool 
    {
        private Cursor ellipseToolCursor;

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

        protected override RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] points, PdnGraphicsPath path)
        {
            return Utility.SimplifyTrace(path.PathPoints);
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
            path.AddEllipse(rect);
            path.Flatten(Utility.IdentityMatrix, 0.25f);
            return path;
        }

        public EllipseTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.EllipseToolIcon.bmp"),
                   "Ellipse",
                   "Draws an Ellipse",
                   "Click and drag to draw an ellipse (right click for background color). Hold shift to constrain to a circle.")
        {
            this.ellipseToolCursor = new Cursor(Utility.GetResourceStream("Cursors.EllipseToolCursor.cur"));
            this.Cursor = this.ellipseToolCursor;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (this.ellipseToolCursor != null)
                {
                    this.ellipseToolCursor.Dispose();
                    this.ellipseToolCursor = null;
                }
            }
        }
    }
}

