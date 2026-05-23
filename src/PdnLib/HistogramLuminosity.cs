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

namespace PaintDotNet
{
    /// <summary>
    /// Histogram is used to calculate a histogram for a surface (in a selection,
    /// if desired). This can then be used to retrieve percentile, average, peak,
    /// and distribution information.
    /// </summary>
    public class HistogramLuminosity : Histogram
    {
        public HistogramLuminosity()
            : base(1, 256)
        {
            visualColors = new ColorBgra[]{     
                                              ColorBgra.Black
                                          };
        }

        public override ColorBgra GetMeanColor() 
        {
            float[] mean = GetMean();

            return ColorBgra.FromBgr((byte)(mean[0] + 0.5f), (byte)(mean[0] + 0.5f), (byte)(mean[0] + 0.5f));
        }

        public override ColorBgra GetPercentileColor(float fraction) 
        {
            int[] perc = GetPercentile(fraction);

            return ColorBgra.FromBgr((byte)(perc[0]), (byte)(perc[0]), (byte)(perc[0]));
        }

        protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
        {
            long[] histogramLuminosity = histogram[0];
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {

                ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    ++histogramLuminosity[ptr->GetIntensityByte()];
                    ++ptr;
                }
            }
        }

        public UnaryPixelOps.Level MakeLevelsAuto() 
        {
            ColorBgra lo, md, hi;

            lo = GetPercentileColor(0.005f);
            md = GetMeanColor();
            hi = GetPercentileColor(0.995f);

            return UnaryPixelOps.Level.AutoFromLoMdHi(lo, md, hi);
        }
    }
}
