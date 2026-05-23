/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    public class RotateNubRenderer
        : SurfaceBoxRenderer
    {
        private PointF location;
        private int size;
        private float angle;

        public PointF Location
        {
            get
            {
                return this.location;
            }

            set
            {
                InvalidateOurself();
                this.location = value;
                InvalidateOurself();
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }

            set
            {
                InvalidateOurself();
                this.size = value;
                InvalidateOurself();
            }
        }

        public float Angle
        {
            get
            {
                return this.angle;
            }

            set
            {
                InvalidateOurself();
                this.angle = value;
                InvalidateOurself();
            }
        }

        private RectangleF GetOurRectangle()
        {
            RectangleF rectF = new RectangleF(this.Location, new SizeF(0, 0));
            float ratio = 1.0f / (float)OwnerList.ScaleFactor.Ratio;
            rectF.Inflate(ratio * this.size, ratio * this.size);
            return rectF;
        }

        private void InvalidateOurself()
        {
            RectangleF rectF = GetOurRectangle();
            Rectangle rect = Utility.RoundRectangle(rectF);
            rect.Inflate(2, 2);
            Invalidate(rect);
        }

        public bool IsPointTouching(Point pt)
        {
            RectangleF rectF = GetOurRectangle();
            Rectangle rect = Utility.RoundRectangle(rectF);
            return pt.X >= rect.Left && pt.Y >= rect.Top && pt.X < rect.Right && pt.Y < rect.Bottom;
        }

        protected override void OnVisibleChanged()
        {
            InvalidateOurself();
        }

        public override void Render(Surface dst, Point offset)
        {
            using (RenderArgs ra = new RenderArgs(dst))
            {
                // We round these values to the nearest integer to avoid an interesting rendering
                // anomaly (or bug? what a surprise ... GDI+) where the nub appears to rotate
                // off-center, or the 'screw-line' is off-center
                float centerX = this.Location.X * (float)OwnerList.ScaleFactor.Ratio;
                float centerY = this.Location.Y * (float)OwnerList.ScaleFactor.Ratio;
                Point center = new Point((int)Math.Round(centerX), (int)Math.Round(centerY));

                ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ra.Graphics.TranslateTransform(-center.X, -center.Y, MatrixOrder.Append);
                ra.Graphics.RotateTransform(this.angle, MatrixOrder.Append);
                ra.Graphics.TranslateTransform(center.X - offset.X, center.Y - offset.Y, MatrixOrder.Append);

                using (Pen white = new Pen(Color.FromArgb(128, Color.White), -1.0f), 
                           black = new Pen(Color.FromArgb(128, Color.Black), -1.0f))
                {
                    RectangleF rectF = new RectangleF(center, new SizeF(0, 0));
                    rectF.Inflate(3, 3);

                    ra.Graphics.DrawEllipse(white, Rectangle.Truncate(rectF));
                    rectF.Inflate(1, 1);
                    ra.Graphics.DrawEllipse(black, Rectangle.Truncate(rectF));
                    rectF.Inflate(1, 1);
                    ra.Graphics.DrawEllipse(white, Rectangle.Truncate(rectF));

                    rectF.Inflate(-2, -2);
                    ra.Graphics.DrawLine(white, rectF.X + rectF.Width / 2.0f - 1.0f, rectF.Top, rectF.X + rectF.Width / 2.0f - 1.0f, rectF.Bottom);
                    ra.Graphics.DrawLine(white, rectF.X + rectF.Width / 2.0f + 1.0f, rectF.Top, rectF.X + rectF.Width / 2.0f + 1.0f, rectF.Bottom);
                    ra.Graphics.DrawLine(black, rectF.X + rectF.Width / 2.0f, rectF.Top, rectF.X + rectF.Width / 2.0f, rectF.Bottom);
                }
            }
        }
            
        public RotateNubRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
            this.location = new Point(0, 0);
            this.size = 6;
        }
    }
}
