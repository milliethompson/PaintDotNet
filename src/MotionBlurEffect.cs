using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MotionBlurEffect.
	/// </summary>
	public class MotionBlurEffect
        : Effect,
          IConfigurableEffect
	{
		public MotionBlurEffect()
            : base("Motion Blur", "Blurs an image to give the effect of motion.", null)
		{

        }

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            base.Render (dstArgs, srcArgs, roi);
            throw new InvalidOperationException("MotionBlurEffect must be used via the other Render overload");
        }

        #region IConfigurableEffect Members

        public EffectConfigDialog CreateConfigDialog()
        {
            return new MotionBlurEffectConfigDialog();
        }

        private unsafe ColorBgra DoLineAverage(Point[] points, int x, int y, Surface dst, Surface src)
        {
            int bSum = 0;
            int gSum = 0;
            int rSum = 0;
            int aSum = 0;
            int div = 0;
                        
            foreach(Point p in points)
            {
                Point srcPoint = new Point(x + p.X, y + p.Y);

                if (Utility.IsPointInRectangle(srcPoint, src.Bounds))
                {
                    ColorBgra c = *src.GetPointAddress(srcPoint.X, srcPoint.Y);
                    bSum += c.b;
                    gSum += c.g;
                    rSum += c.r;
                    aSum += c.a;
                    ++div;
                }
            }

            int b = bSum /= div;
            int g = gSum /= div;
            int r = rSum /= div;
            int a = aSum /= div;

            return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
        }

        private unsafe ColorBgra DoLineAverage_NoClip(Point[] points, int x, int y, Surface dst, Surface src)
        {
            int bSum = 0;
            int gSum = 0;
            int rSum = 0;
            int aSum = 0;
            int div = 0;
                        
            foreach(Point p in points)
            {
                int srcX = x + p.X;
                int srcY = y + p.Y;
                ColorBgra c = *src.GetPointAddress(srcX, srcY);

                bSum += c.b;
                gSum += c.g;
                rSum += c.r;
                aSum += c.a;
                ++div;
            }

            int b = bSum /= div;
            int g = gSum /= div;
            int r = rSum /= div;
            int a = aSum /= div;

            return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
        }

        public unsafe void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Region roi)
        {
            Point[] points = ((MotionBlurEffectConfigToken)properties).LinePoints;
            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            foreach (RectangleF rectF in roi.GetRegionScans(Utility.IdentityMatrix))
            {
                Rectangle rect = Rectangle.Truncate(rectF);
                
                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    ColorBgra *dstPtr = dst.GetPointAddress(rect.Left, y);

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        Point a = new Point(x + points[0].X, y + points[0].Y);
                        Point b = new Point(x + points[points.Length - 1].X, y + points[points.Length - 1].Y);

                        // If both ends of this line are in bounds, we don't need to do silly clipping
                        if (Utility.IsPointInRectangle(a, src.Bounds) && Utility.IsPointInRectangle(b, src.Bounds))
                        {
                            *dstPtr = DoLineAverage_NoClip(points, x, y, dst, src);
                        }
                        else
                        {
                            *dstPtr = DoLineAverage(points, x, y, dst, src);
                        }

                        ++dstPtr;
                    }
                }
            }
        }

        #endregion
    }
}


