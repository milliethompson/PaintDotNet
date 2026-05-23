using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for MotionBlurEffect.
    /// </summary>
    public class MotionBlurEffect
        : Effect,
          IConfigurableEffect
    {
        public MotionBlurEffect()
            : base("Motion Blur", "Blurs an image to give the effect of motion.", Utility.GetImageResource("Icons.MotionBlurEffect.bmp"))
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

        private unsafe ColorBgra DoLineAverage(bool clip, Point[] points, int x, int y, Surface dst, Surface src)
        {
            long bSum = 0;
            long gSum = 0;
            long rSum = 0;
            long aSum = 0;
            int cDiv = 1;
			int aDiv = 0;
                        
            foreach (Point p in points)
            {
                Point srcPoint = new Point(x + p.X, y + p.Y);

                if (!clip || Utility.IsPointInRectangle(srcPoint, src.Bounds))
                {
                    ColorBgra c = *src.GetPointAddress(srcPoint.X, srcPoint.Y);
					if (c.A > 0) 
					{
						bSum += c.B * c.A;
						gSum += c.G * c.A;
						rSum += c.R * c.A;
						aSum += c.A;
					}

					aDiv++;
					cDiv += c.A;
                }
            }

            int b = (int)(bSum /= cDiv);
            int g = (int)(gSum /= cDiv);
            int r = (int)(rSum /= cDiv);
            int a = (int)(aSum /= aDiv);

			if (cDiv > 1000)
				cDiv++;

            return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
        }

        public unsafe void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            Point[] points = ((MotionBlurEffectConfigToken)properties).LinePoints;
            Surface dst = dstArgs.Surface;
            Surface src = srcArgs.Surface;

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
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
                            *dstPtr = DoLineAverage(false, points, x, y, dst, src);
                        }
                        else
                        {
                            *dstPtr = DoLineAverage(true, points, x, y, dst, src);
                        }

                        ++dstPtr;
                    }
                }
            }
        }

        #endregion
    }
}


