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
    public abstract class Histogram
    {
        protected long [,] histogram = new long[3, 256];
        public long[,] HistogramValues
        {
            get
            {
                return histogram;
            }
            set
            {
                if (value.GetLength(0) == histogram.GetLength(0) && value.GetLength(1) == histogram.GetLength(1))
                {
                    histogram = value;
                    OnHistogramUpdated();
                }
                else
                {
                    throw new ArgumentException("value muse be a 3x256 array", "value");
                }
            }
        }
     
        public int Channels
        {
            get
            {
                return histogram.GetLength(0);
            }
        }
     
        public int Entries
        {
            get
            {
                return histogram.GetLength(1);
            }
        }

        public event EventHandler HistogramChanged;
        protected void OnHistogramUpdated()
        {
            if (HistogramChanged != null)
            {
                HistogramChanged(this, EventArgs.Empty);
            }
        }

        protected ColorBgra[] visualColors;
        public ColorBgra GetVisualColor(int channel)
        {
            return visualColors[channel];
        }

        public long GetOccurrences(int channel, int val) 
        {
            return histogram[channel, val];
        }

        public long GetMax() 
        {
            long max = -1;

            foreach (long i in histogram)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            
            return max;
        }

        public long GetMax(int channel) 
        {
            if (channel < 0 || channel >= 3) 
            {
                throw new ArgumentOutOfRangeException("channel", channel, "Channel must be between 0 and 2");
            }

            long max = -1;

            for (int v = 0; v < 256; v++) 
            {
                if (max < histogram[channel, v]) 
                {
                    max = histogram[channel, v];
                }
            }

            return max;
        }

        public float[] GetMean() 
        {
            float[] ret = new float[histogram.GetLength(0)];

            for (int i = 0; i < histogram.GetLength(0); i++) 
            {
                long avg = 0;
                long sum = 0;

                for (int j = 0; j < histogram.GetLength(1); j++) 
                {
                    avg += j * histogram[i, j];
                    sum += histogram[i, j];
                }

                if (sum != 0)
                {
                    ret[i] = (float)avg / (float)sum;
                }
                else
                {
                    ret[i] = 0;
                }
            }

            return ret;
        }

        public int[] GetPercentile(float fraction) 
        {
            int[] ret = new int[histogram.GetLength(0)];

            for (int i = 0; i < histogram.GetLength(0); i++) 
            {
                long integral = 0;
                long sum = 0;

                for (int j = 0; j < histogram.GetLength(1); j++) 
                {
                    sum += histogram[i, j];
                }

                for (int j = 0; j < histogram.GetLength(1); j++)
                {
                    integral += histogram[i, j];

                    if (integral > sum * fraction) 
                    {
                        ret[i] = j;
                        break;
                    }
                }
            }

            return ret;
        }

        public abstract ColorBgra GetMeanColor();

        public abstract ColorBgra GetPercentileColor(float fraction);

        /// <summary>
        /// Sets the histogram to be all zeros.
        /// </summary>
        protected void Clear()
        {
            histogram.Initialize();
        }

        protected abstract void AddSurfaceRectangleToHistogram(Surface surface, Rectangle rect);

        public void UpdateHistogram(Surface surface)
        {
            Clear();
            AddSurfaceRectangleToHistogram(surface, surface.Bounds);
            OnHistogramUpdated();
        }

        public void UpdateHistogram(Surface surface, Rectangle rect)
        {
            Clear();
            AddSurfaceRectangleToHistogram(surface, rect);
            OnHistogramUpdated();
        }

        public void UpdateHistogram(Surface surface, PdnRegion roi)
        {
            Clear();

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt()) 
            {
                AddSurfaceRectangleToHistogram(surface, rect);
            }

            OnHistogramUpdated();
        }
    }
}
