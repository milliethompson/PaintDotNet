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
    public class RoundedRectangleTool
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

        public RoundedRectangleTool(DocumentWorkspace parent)
            : base(parent)
        {
            toolBarImage = Utility.GetImageResource("Icons.RoundedRectangleToolIcon.bmp");
            cursor = new Cursor(Utility.GetResourceStream("Cursors.RectangleToolCursor.cur"));
            name = "Rounded Rectangle";
            description = "Draws a rounded rectangle";
        }

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        #region Get the desired Rounded Rectangle path. 
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
        #endregion 

        // credit for the this function is given to Aaron Reginald http://www.codeproject.com/cs/media/ExtendedGraphics.asp
        #region Gets the desired Capsular path.
        private PdnGraphicsPath GetCapsule(RectangleF baseRect) 
        { 
            float diameter; 
            RectangleF arc; 
            PdnGraphicsPath path = new PdnGraphicsPath(); 

            try 
            { 
                if (baseRect.Width>baseRect.Height) 
                {   // return horizontal capsule 
                    diameter = baseRect.Height; 
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF); 
                    path.AddArc(arc, 90, 180); 
                    arc.X = baseRect.Right-diameter; 
                    path.AddArc(arc, 270, 180); 
                } 
                else if (baseRect.Width < baseRect.Height) 
                {   // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF( baseRect.Location, sizeF ); 
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
        #endregion
    }
}
