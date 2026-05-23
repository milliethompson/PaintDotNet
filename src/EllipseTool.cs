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
        private Icon ellipseToolIcon;
        private string statusTextFormat = PdnResources.GetString("EllipseTool.StatusText.Format");
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

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.None;
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
                PointF dir = new PointF(b.X - a.X, b.Y - a.Y);
                float len = (float)Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
                PointF center = new PointF((a.X + b.X) / 2.0f, (a.Y + b.Y) / 2.0f);
                float radius = len / 2.0f;
                rect = Utility.RectangleFromCenter(center, radius);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            if (rect.Width == 0 || rect.Height == 0)
            {
                return null;
            }

            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddEllipse(rect);
            path.Flatten(Utility.IdentityMatrix, 0.10f);

            MeasurementUnit units = Workspace.Environment.Units;

            double widthPhysical = Math.Abs(Workspace.Document.PixelToPhysicalX(rect.Width, units));
            double heightPhysical = Math.Abs(Workspace.Document.PixelToPhysicalY(rect.Height, units));
            double areaPhysical = Math.PI * (widthPhysical / 2.0) * (heightPhysical / 2.0);
            
            string numberFormat;
            string unitsAbbreviation;

            if (units != MeasurementUnit.Pixel)
            {
                string unitsAbbreviationName = "MeasurementUnit." + units.ToString() + ".Abbreviation";
                unitsAbbreviation = PdnResources.GetString(unitsAbbreviationName);
                numberFormat = "F2";
            }
            else
            {
                unitsAbbreviation = string.Empty;
                numberFormat = "F0";
            }

            string unitsString = PdnResources.GetString("MeasurementUnit." + units.ToString() + ".Plural");

            string statusText = string.Format(
                this.statusTextFormat,
                widthPhysical.ToString(numberFormat),
                unitsAbbreviation,
                heightPhysical.ToString(numberFormat),
                unitsAbbreviation,
                areaPhysical.ToString(numberFormat),
                unitsString);

            this.SetStatus(this.ellipseToolIcon, statusText);
            return path;
        }

        protected override void OnActivate()
        {
            this.ellipseToolCursor = new Cursor(PdnResources.GetResourceStream("Cursors.EllipseToolCursor.cur"));
            this.ellipseToolIcon = Utility.ImageToIcon(this.Image);
            this.Cursor = this.ellipseToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.ellipseToolCursor != null)
            {
                this.ellipseToolCursor.Dispose();
                this.ellipseToolCursor = null;
            }

            if (this.ellipseToolIcon != null)
            {
                this.ellipseToolIcon.Dispose();
                this.ellipseToolIcon = null;
            }

            base.OnDeactivate();
        }

        public EllipseTool(DocumentWorkspace parent)
            : base(parent,
                   PdnResources.GetImage("Icons.EllipseToolIcon.bmp"),
                   PdnResources.GetString("EllipseTool.Name"),
                   PdnResources.GetString("EllipseTool.HelpText"))
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

