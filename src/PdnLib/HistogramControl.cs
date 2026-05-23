/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for HistogramControl.
	/// </summary>
	public class HistogramControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Surface renderedHistogram = new Surface(100, 258);
		public Histogram Histogram = new Histogram();
		private bool isValid = false;

		private ColorBgra mask;
		public ColorBgra Mask 
		{
			get 
			{
				return mask;
			}
			set 
			{
				if (mask != value) 
				{
					isValid = false;
					mask = value;
					this.Invalidate();
				}
			}
		}

		private bool flipHorizontal;
		public bool FlipHorizontal
		{
			get 
			{
				return flipHorizontal;
			}
			set 
			{
				flipHorizontal = value;
			}
		}

		private bool flipVertical;
		public bool FlipVertical
		{
			get 
			{
				return flipVertical;
			}
			set 
			{
				flipVertical = value;
			}
		}

		public HistogramControl()
		{
			Histogram.HistogramChanged += new EventHandler(Histogram_HistogramChanged);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) 
			{
				renderedHistogram.Dispose();
			}
			base.Dispose (disposing);
		}

		private const int tickSize = 4;
		unsafe private void UpdateRenderedHistogram() 
		{
			long max = Histogram.GetMax() + 1;
			mean = Histogram.GetMeanColor();
			ColorBgra *ptr;

			for (int y = 0; y < renderedHistogram.Height; y++) 
			{
				ptr = renderedHistogram.GetRowAddress(y);

				for (int x = 0; x < renderedHistogram.Width; x++) 
				{
					ptr[x].Bgra = 0x000000;
				}
			}
			for (int c = 0; c < 3; c++) 
			{
				byte onColor = 255;

				if (mask[c] == 0) 
				{
					onColor /= 4;
				}

				for (int v = 0; v <= 255; v++) 
				{
					long cutoff = Histogram.GetOccurrences(c, (byte)v) * renderedHistogram.Width / max, x;

					if (flipVertical) 
					{
						ptr = renderedHistogram.GetRowAddress(1 + v);
					}
					else
					{
						ptr = renderedHistogram.GetRowAddress(256 - v);
					}
					if (flipHorizontal) 
					{
						for (x = renderedHistogram.Width - cutoff; x < renderedHistogram.Width; x++) 
						{
							ptr[x].A = 255;
							ptr[x][c] = onColor;
						}
					}
					else 
					{
						for (x = 0; x < cutoff; x++) 
						{
							ptr[x].A = 255;
							ptr[x][c] = onColor;
						}
					}
				}
			}

			this.isValid = true;
		}

		private ColorBgra mean;
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);

			if (!isValid) 
			{
				UpdateRenderedHistogram();
			}

            using (Bitmap bmp = renderedHistogram.CreateAliasedBitmap())
            {
                if (this.Height < 256) 
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                } 
                else 
                {
                    e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                }

                e.Graphics.DrawImage(bmp, Rectangle.Inflate(this.ClientRectangle, -1, -1), renderedHistogram.Bounds, GraphicsUnit.Pixel);
                e.Graphics.DrawRectangle(Pens.Black, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

                for (int c = 0; c < 3; c++)
                {
                    int x = flipHorizontal ? 0 : this.Width;
                    int y = flipVertical ? mean[c] + 1 : 256 - mean[c];
                    y = (y * this.Height) / 258;
                    Point l, t, r, b;

                    ColorBgra col = ColorBgra.FromBgr(0, 0, 0);

                    col[c] = 255;
                    l = new Point(x - tickSize, y);
                    t = new Point(x, y - tickSize);
                    r = new Point(x + tickSize, y);
                    b = new Point(x, y + tickSize);

                    e.Graphics.FillPolygon(new SolidBrush(col.ToColor()), new Point[] {l, t, r, b});
                }
            }
		}

		private void Histogram_HistogramChanged(object sender, EventArgs e)
		{
			isValid = false;
			Invalidate();
		}
	}
}
