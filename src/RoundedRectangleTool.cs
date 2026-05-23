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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for RoundedRectangleTool.
    /// </summary>
    public class RoundedRectangleTool
        : ShapeTool 
    {
        private Cursor roundedRectangleCursor;

        protected override RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] points, PdnGraphicsPath path)
        {
            return Utility.SimplifyTrace(path.PathPoints);
        }

        protected override List<PointF> TrimShapePath(List<PointF> points)
        {
            List<PointF> array = new List<PointF>();

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
            RectangleF rect;
            float radius = 10;

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                rect = Utility.PointsToConstrainedRectangle(a, b);
            }
            else
            {
                rect = Utility.PointsToRectangle(a, b);
            }

            PdnGraphicsPath path = this.GetRoundedRect(rect, radius); 
            path.Flatten();

            if (path.PathPoints[0] != path.PathPoints[path.PathPoints.Length - 1])
            {
                path.AddLine(path.PathPoints[0], path.PathPoints[path.PathPoints.Length - 1]);
                path.CloseFigure();
            }

            return path;
        }

        protected override void OnActivate()
        {
            this.roundedRectangleCursor = new Cursor(PdnResources.GetResourceStream("Cursors.RoundedRectangleToolCursor.cur"));
            this.Cursor = this.roundedRectangleCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.roundedRectangleCursor != null)
            {
                this.roundedRectangleCursor.Dispose();
                this.roundedRectangleCursor = null;
            }

            base.OnDeactivate();
        }

        public RoundedRectangleTool(DocumentWorkspace parent)
            : base(parent,
                   PdnResources.GetImage("Icons.RoundedRectangleToolIcon.png"),
                   PdnResources.GetString("RoundedRectangleTool.Name"),
                   PdnResources.GetString("RoundedRectangleTool.HelpText"))
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeImage();
            }

            base.Dispose (disposing);
        }

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        protected PdnGraphicsPath GetRoundedRect(RectangleF baseRect, float radius) 
        {
            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radius <= 0.0f) 
            { 
                PdnGraphicsPath mPath = new PdnGraphicsPath(); 
                mPath.AddRectangle(baseRect); 
                mPath.CloseFigure(); 
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the width, or height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0) 
            {
                return GetCapsule(baseRect); 
            }

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing 
            float diameter = radius * 2.0f; 
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(baseRect.Location, sizeF); 
            PdnGraphicsPath path = new PdnGraphicsPath(); 

            // top left arc 
            path.AddArc (arc, 180, 90); 

            // top right arc 
            arc.X = baseRect.Right - diameter; 
            path.AddArc (arc, 270, 90); 

            // bottom right arc 
            arc.Y = baseRect.Bottom - diameter; 
            path.AddArc (arc, 0, 90); 

            // bottom left arc
            arc.X = baseRect.Left;     
            path.AddArc (arc, 90, 90);     

            path.CloseFigure(); 
            return path; 
        } 

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        private PdnGraphicsPath GetCapsule(RectangleF baseRect) 
        { 
            float diameter; 
            RectangleF arc; 
            PdnGraphicsPath path = new PdnGraphicsPath(); 

            try 
            { 
                if (baseRect.Width>baseRect.Height) 
                {   
                    // return horizontal capsule 
                    diameter = baseRect.Height; 
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF); 
                    path.AddArc(arc, 90, 180); 
                    arc.X = baseRect.Right-diameter; 
                    path.AddArc(arc, 270, 180); 
                } 
                else if (baseRect.Width < baseRect.Height) 
                {   
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180); 
                    arc.Y = baseRect.Bottom-diameter; 
                    path.AddArc(arc, 0, 180); 
                } 
                else
                {   // return circle 
                    path.AddEllipse(baseRect); 
                }
            } 

            catch (Exception)
            {
                path.AddEllipse(baseRect);
            } 

            finally 
            { 
                path.CloseFigure(); 
            } 

            return path; 
        } 
    }
}
