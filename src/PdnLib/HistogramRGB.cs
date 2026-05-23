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
    public class HistogramRgb : Histogram
    {
        public HistogramRgb()
        {
            this.histogram = new long[3, 256];
            visualColors = new ColorBgra[]{     
                                              ColorBgra.Blue,
                                              ColorBgra.Green,
                                              ColorBgra.Red
                                          };
        }

        public override ColorBgra GetMeanColor() 
        {
            float[] mean = GetMean();

            return ColorBgra.FromBgr((byte)(mean[0] + 0.5f), (byte)(mean[1] + 0.5f), (byte)(mean[2] + 0.5f));
        }

        public override ColorBgra GetPercentileColor(float fraction) 
        {
            int[] perc = GetPercentile(fraction);

            return ColorBgra.FromBgr((byte)(perc[0]), (byte)(perc[1]), (byte)(perc[2]));
        }

        protected override unsafe void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect)
        {
            for (int y = rect.Top; y < rect.Bottom; ++y)
            {
                ColorBgra* ptr = surface.GetPointAddressUnchecked(rect.Left, y);
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    ++histogram[0, ptr->B];
                    ++histogram[1, ptr->G];
                    ++histogram[2, ptr->R];
                    ++ptr;
                }
            }
        }

        public void SetFromLeveledHistogram(HistogramRgb inputHistogram, UnaryPixelOps.Level upo)
        {
            if (inputHistogram == null || upo == null) 
            {
                return;
            }

            Clear();

            for (int v = 0; v <= 255; v ++)
            {
                ColorBgra after = ColorBgra.FromRgb((byte)v, (byte)v, (byte)v);
                float [] before = new float[3];
                float [] slopes = new float[3];

                upo.UnApply(after, before, slopes);

                for (int c = 0; c < 3; c++) 
                {
                    if (after[c] > upo.ColorOutHigh[c]
                        || after[c] < upo.ColorOutLow[c]
                        || (int)Math.Floor(before[c]) < 0
                        || (int)Math.Ceiling(before[c]) > 255
                        || float.IsNaN(before[c])) 
                    {
                        histogram[c, v] = 0;
                    }
                    else if (before[c] <= upo.ColorInLow[c]) 
                    {
                        histogram[c, v] = 0;

                        for (int i = 0; i <= upo.ColorInLow[c]; i++)
                        {
                            histogram[c, v] += inputHistogram.histogram[c, i];
                        }
                    } 
                    else if (before[c] >= upo.ColorInHigh[c])
                    {
                        histogram[c, v] = 0;

                        for (int i = upo.ColorInHigh[c]; i < 256; i++)
                        {
                            histogram[c, v] += inputHistogram.histogram[c, i];
                        }
                    }
                    else
                    {
                        histogram[c, v] = (int)(slopes[c] * Utility.Lerp(
                            inputHistogram.histogram[c, (int)Math.Floor(before[c])],
                            inputHistogram.histogram[c, (int)Math.Ceiling(before[c])],
                            before[c] - Math.Floor(before[c])));
                    }
                }
            }

            OnHistogramUpdated();
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
