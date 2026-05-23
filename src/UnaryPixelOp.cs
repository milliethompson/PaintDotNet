using System;
using System.Drawing;
using System.Threading;

namespace PaintDotNet
{
	/// <summary>
	/// Defines a way to operate on a pixel, or a region of pixels, in a unary fashion.
	/// That is, it is a simple function F that takes one parameter and returns a
	/// result of the form: d = F(c)
	/// </summary>
	[Serializable]
	public unsafe abstract class UnaryPixelOp
        : PixelOp
	{
        public abstract ColorBgra Apply(ColorBgra color);

        protected unsafe override void Apply(ColorBgra *dst, ColorBgra *src, int length)
        {
            unsafe
            {
                while (length > 0)
                {
                    *dst = Apply(*src);
                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        protected unsafe virtual void Apply(ColorBgra *ptr, int length)
        {
            unsafe
            {
                while (length > 0)
                {
                    *ptr = Apply(*ptr);
                    ++ptr;
                    --length;
                }
            }
        }

        public void ApplyBase(Surface surface, Rectangle[] roi)
        {
            Rectangle regionBounds = Utility.GetRegionBounds(roi);

            if (regionBounds != Rectangle.Intersect(surface.Bounds, regionBounds))
            {
                throw new ArgumentOutOfRangeException("roi", "Region is out of bounds");
            }

            unsafe
            {
                foreach(Rectangle rect in roi)
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra *ptr = surface.GetPointAddress(rect.Left, y);
                        Apply(ptr, rect.Width);
                    }
                }
            }
        }

        public void Apply(Surface surface, RectangleF[] roi)
        {
            ApplyBase(surface, Utility.TruncateRectangles(roi));
        }

        public void Apply(Surface surface, Rectangle roi)
        {
            ApplyBase(surface, new Rectangle[] { roi });
        }

        public void Apply(Surface surface, Scanline scan)
        {
            // TODO: bounds checking!
            Apply(surface.GetPointAddress(scan.Point), scan.Length);
        }

        public void Apply(Surface surface, Scanline[] scans)
        {
            foreach (Scanline scan in scans)
            {
                Apply(surface, scan);
            }
        }

        public override void Apply(Surface dst, Point dstOffset, Surface src, Point srcOffset, int scanLength)
        {
            Apply(dst.GetPointAddress(dstOffset), src.GetPointAddress(srcOffset), scanLength);
        }

        public void Apply(Surface surface, Region roi)
		{
            Apply(surface, roi.GetRegionScans(Utility.IdentityMatrix));
		}

		public UnaryPixelOp()
		{
		}
	}
}
