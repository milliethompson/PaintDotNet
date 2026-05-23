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
    /// Summary description for LineTool.
    /// </summary>
    public class LineTool
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

            if (a == b)
            {
                return null;
            }
            else
            {
                PdnGraphicsPath path = new PdnGraphicsPath();
                path.AddLine(a, b);
                return path;
            }
        }

        public LineTool(DocumentWorkspace parent)
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.LineToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.LineToolCursor.cur"));
            name = "Line";
            description = "Draws a Line";
            this.ForceShapeDrawType = true;
            this.ForcedShapeDrawType = ShapeDrawType.Outline;
        }
    }
}
