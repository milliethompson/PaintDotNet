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
using System.Windows.Forms;

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
            : base(PdnResources.GetString("MotionBlurEffect.Name"),
                   PdnResources.GetImage("Icons.MotionBlurEffect.bmp"),
                   Shortcut.None,
                   PdnResources.GetString("Effects.Blurring.Submenu.Name"),
                   EffectDirectives.None)
        {
        }

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            base.Render(dstArgs, srcArgs, roi);
            throw new InvalidOperationException("MotionBlurEffect must be used via the other Render overload");
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            return new MotionBlurEffectConfigDialog();
        }

        private unsafe ColorBgra DoLineAverage(Point[] points, int x, int y, Surface dst, Surface src)
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

                if (Utility.IsPointInRectangle(srcPoint, src.Bounds))
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

            return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
        }
        private unsafe ColorBgra DoLineAverageUnclipped(Point[] points, int x, int y, Surface dst, Surface src)
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
                ColorBgra c = src.GetPointUnchecked(srcPoint.X, srcPoint.Y);

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

            int b = (int)(bSum /= cDiv);
            int g = (int)(gSum /= cDiv);
            int r = (int)(rSum /= cDiv);
            int a = (int)(aSum /= aDiv);

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
                        if (Utility.IsPointInRectangle(a, src.Bounds) && 
                            Utility.IsPointInRectangle(b, src.Bounds))
                        {
                            *dstPtr = DoLineAverageUnclipped(points, x, y, dst, src);
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
    }
}