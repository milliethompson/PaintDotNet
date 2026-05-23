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
        protected long[][] histogram;
        public long[][] HistogramValues
        {
            get
            {
                return histogram;
            }
            set
            {
                if (value.Length == histogram.Length && value[0].Length == histogram[0].Length)
                {
                    histogram = value;
                    OnHistogramUpdated();
                }
                else
                {
                    throw new ArgumentException("value muse be an array of arrays of matching size", "value");
                }
            }
        }
     
        public int Channels
        {
            get
            {
                return histogram.Length;
            }
        }
     
        public int Entries
        {
            get
            {
                return histogram[0].Length;
            }
        }

        protected Histogram(int channels, int entries)
        {
            histogram = new long[channels][];

            for (int channel = 0; channel < channels; ++channel)
            {
                histogram[channel] = new long[entries];
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
            return histogram[channel][val];
        }

        public long GetMax() 
        {
            long max = -1;

            foreach (long[] channelHistogram in histogram)
            {
                foreach (long i in channelHistogram)
                {
                    if (i > max)
                    {
                        max = i;
                    }
                }
            }
            
            return max;
        }

        public long GetMax(int channel)
        {
            long max = -1;

            foreach (long i in histogram[channel])
            {
                if (i > max)
                {
                    max = i;
                }
            }

            return max;
        }

        public float[] GetMean() 
        {
            float[] ret = new float[Channels];

            for (int channel = 0; channel < Channels; ++channel)
            {
                long[] channelHistogram = histogram[channel];
                long avg = 0;
                long sum = 0;

                for (int j = 0; j < channelHistogram.Length; j++)
                {
                    avg += j * channelHistogram[j];
                    sum += channelHistogram[j];
                }

                if (sum != 0)
                {
                    ret[channel] = (float)avg / (float)sum;
                }
                else
                {
                    ret[channel] = 0;
                }
            }

            return ret;
        }

        public int[] GetPercentile(float fraction) 
        {
            int[] ret = new int[Channels];

            for (int channel = 0; channel < Channels; ++channel)
            {
                long[] channelHistogram = histogram[channel];
                long integral = 0;
                long sum = 0;

                for (int j = 0; j < channelHistogram.Length; j++) 
                {
                    sum += channelHistogram[j];
                }

                for (int j = 0; j < channelHistogram.Length; j++)
                {
                    integral += channelHistogram[j];

                    if (integral > sum * fraction) 
                    {
                        ret[channel] = j;
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
