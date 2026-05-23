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
    /// Summary description for LineTool.
    /// </summary>
    public class LineTool
        : ShapeTool 
    {
        private Cursor lineToolCursor;

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

        protected override PdnGraphicsPath CreateShapePath(PointF[] points)
        {
            PointF a = points[0];
            PointF b = points[points.Length - 1];

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

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.None;
        }


        public LineTool(DocumentWorkspace parent)
            : base(parent,
                   Utility.GetImageResource("Icons.LineToolIcon.bmp"),
                   "Line",
                   "Draws a Line",
                   "Left click to draw a line with the foreground color, right click to use the background color")
        {
            this.lineToolCursor = new Cursor(Utility.GetResourceStream("Cursors.LineToolCursor.cur"));
            this.Cursor = this.lineToolCursor;
            this.ForceShapeDrawType = true;
            this.ForcedShapeDrawType = ShapeDrawType.Outline;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

                if (this.lineToolCursor != null)
                {
                    this.lineToolCursor.Dispose();
                    this.lineToolCursor = null;
                }
            }
        }
    }
}
