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
        protected override void OnActivate()
        {
            base.OnActivate ();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();
        }

        protected override RectangleF[] GetOptimizedShapeOutlineRegion(Point[] points, PdnGraphicsPath path)
        {
            return Utility.SimplifyTrace(path.PathPoints);
        }

        protected override PdnGraphicsPath CreateShapePath(Point[] points)
        {
            // make sure we don't screw them up
            if (points.Length < 2)
            {
                return null;
            }

            // make sure the shape has an area of at least 1
            // we can determine this by making sure that all the Points in points are not all the same
            bool allTheSame = true;
            foreach (Point pt in points)
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
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.FreeformShapeToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.FreeformShapeToolCursor.cur"));
            name = "Freeform Shape";
            description = "Draws a freeform shape";
        }
    }
}
