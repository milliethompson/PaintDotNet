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
        protected override void OnActivate()
        {
            base.OnActivate ();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();
        }

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

        protected override PdnGraphicsPath CreateShapePath(Point[] points)
        {
            Point a = points[0];
            Point b = points[points.Length - 1];
            Rectangle rect;

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
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.RectangleToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.RectangleToolCursor.cur"));
            name = "Rectangle";
            description = "Draws a rectangle";
			helpText = "Left click to draw a rectangle with the foreground color, right click to use the background color";
        }
    }
}
