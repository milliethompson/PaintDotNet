/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
	public class Histogram
	{
		private long [,] hist = new long[3, 256];

		public event EventHandler HistogramChanged;
		private void OnHistogramUpdated()
		{
			if (HistogramChanged != null)
			{
				HistogramChanged(this, EventArgs.Empty);
			}
		}

		public long GetOccurrences(int channel, byte val) 
		{
			if (channel < 0 || channel >= 3) 
			{
				throw new ArgumentOutOfRangeException("channel", channel, "Channel must be between 0 and 2");
			}

			//val does not need to be bounds-checked because it
			//has been intentionally limited to a byte.
			return hist[channel, val];
		}

		public long GetMax() 
		{
			return Math.Max(Math.Max(GetMax(0), GetMax(1)), GetMax(2));
		}

		public long GetMax(int channel) 
		{
			if (channel < 0 || channel >= 3) 
			{
				throw new ArgumentOutOfRangeException("channel", channel, "Channel must be between 0 and 2");
			}

			long peak = -1, max = -1;

			for (int v = 0; v < 256; v++) 
			{
				if (max < hist[channel, v]) 
				{
					max = hist[channel, v];
					peak = v;
				}
			}
			return max;
		}

		public ColorBgra GetMeanColor() 
		{
			ColorBgra ret = new ColorBgra();

			for (int i = 0; i < 3; i++) 
			{
				long avg = 0;
                long sum = 1;

				for (int j = 0; j < 256; j++) 
				{
					avg += j * hist[i, j];
					sum += hist[i, j];
				}

				ret[i] = (byte)(avg / sum);
			}

			ret.A = 255;
			return ret;
		}

		public ColorBgra GetPercentileColor(float fraction) 
		{
			ColorBgra ret = new ColorBgra();

			for (int i = 0; i < 3; i++) 
			{
				long sum = 0;
                long len = 0;

				for (int j = 0; j < 256; j++) 
				{
					len += hist[i, j];
				}

				for (int j = 0; j < 256; j++)
				{
					sum += hist[i, j];

					if (sum > len * fraction) 
					{
						ret[i] = (byte)j;
						break;
					}
				}
			}

			ret.A = 255;
			return ret;
		}


		/// <summary>
		/// Sets the histogram to be all zeros.
		/// </summary>
		private void Clear()
		{
			for (int c = 0; c < 3; c++) 
			{
				for (int v = 0; v < 256; v++) 
				{
					hist[c, v] = 0;
				}
			}
		}

	    public void UpdateHistogram(Surface surface)
		{
			using (PdnRegion total = new PdnRegion(new Rectangle(0, 0, surface.Width, surface.Height)))
			{
				UpdateHistogram(surface, total);	
			}
		}

		public void UpdateHistogram(Surface surface, PdnRegion roi)
		{
			if (roi == null) 
			{
				throw new ArgumentNullException("roi", "roi must reference a valid PdnRegion.");
			}

			Clear();

			foreach (Rectangle r in roi.GetRegionScansReadOnlyInt()) 
			{
                for (int y = r.Top; y < r.Bottom; ++y)
                {
                    for (int x = r.Left; x < r.Right; ++x)
                    {
                        ColorBgra pixel = surface[x, y];
                        ++hist[0, pixel.B];
                        ++hist[1, pixel.G];
                        ++hist[2, pixel.R];
                    }
                }
			}

			OnHistogramUpdated();
		}

		public void SetFromLeveledHistogram(Histogram inputHistogram, UnaryPixelOps.Level upo)
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
						hist[c, v] = 0;
					}
					else if (before[c] <= upo.ColorInLow[c]) 
					{
						hist[c, v] = 0;

						for (int i = 0; i <= upo.ColorInLow[c]; i++)
						{
							hist[c, v] += inputHistogram.hist[c, i];
						}
					} 
					else if (before[c] >= upo.ColorInHigh[c])
					{
						hist[c, v] = 0;

						for (int i = upo.ColorInHigh[c]; i < 256; i++)
						{
							hist[c, v] += inputHistogram.hist[c, i];
						}
					}
					else
					{
						hist[c, v] = (int)(slopes[c] * Utility.Lerp(
							inputHistogram.hist[c, (int)Math.Floor(before[c])],
							inputHistogram.hist[c, (int)Math.Ceiling(before[c])],
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
