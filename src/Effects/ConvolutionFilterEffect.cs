/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for ConvolutionFilterEffect.
    /// </summary>
    public unsafe abstract class ConvolutionFilterEffect
        : Effect
    {
        public void RenderConvolutionFilter(int[][] weights, int offset, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                RenderConvolutionFilter(weights, offset, dstArgs, srcArgs, rect);
            }
        }

        private sealed class FExtentKey
        {
            private int srcLength;
            private int weightsLength;

            public override int GetHashCode()
            {
                return unchecked(srcLength + (weightsLength << 16));
            }

            public override bool Equals(object obj)
            {
                FExtentKey fek = (FExtentKey)obj;
                return srcLength == fek.srcLength && weightsLength == fek.weightsLength;
            }

            public FExtentKey(int srcLength, int weightsLength)
            {
                this.srcLength = srcLength;
                this.weightsLength = weightsLength;
            }
        }

        private class FExtent
        {
            public int[] fStarts;
            public int[] fEnds;
        }

        private static Hashtable fCache = new Hashtable();
        private static Queue fCacheQ = new Queue();

        private static FExtent GetFExtent(int srcLength, int weightsLength)
        {
            FExtentKey key = new FExtentKey(srcLength, weightsLength);

            FExtent extent;
            lock (fCache)
            {
                extent = (FExtent)fCache[key];
            }

            int fOffset = -weightsLength / 2;

            if (extent == null)
            {
                extent = new FExtent();
                extent.fStarts = new int[srcLength];
                extent.fEnds = new int[srcLength];

                for (int dst = 0; dst < srcLength; ++dst)
                {
                    int startSrc = dst + fOffset;

                    if (startSrc < 0)
                    {
                        extent.fStarts[dst] = -startSrc;
                    }
                    else
                    {
                        extent.fStarts[dst] = 0;
                    }

                    int end = startSrc + weightsLength;
                    int endDelta = srcLength - end;
                
                    if (endDelta < 0)
                    {
                        extent.fEnds[dst] = weightsLength + endDelta;
                    }
                    else
                    {
                        extent.fEnds[dst] = weightsLength;
                    }
                }
                
                lock (fCache)
                {
                    if (fCache.Count > 16)
                    {
                        object top = fCacheQ.Dequeue();
                        fCache.Remove(top);
                    }

                    fCache[key] = extent;
                    fCacheQ.Enqueue(key);
                }
            }

            return extent;
        }

        /// <summary>
        /// Normalizes the weight matrix so that it does not overflow an int when
        /// multiplied with a 1-byte channel intensity, and a 1-byte alpha. In
        /// order to do this, the sum and maximum must be less than 32768. The
        /// values of the matrix will be scaled by a power of two to be just below 32768
        /// </summary>
        /// <param name="weights">The weight matrix to be normalized.</param>
        protected void NormalizeWeightMatrix(int[][] weights) 
        {
            int max = 0;
            int sum = 0;
            int shift = 0;
            int width = weights[0].Length;
            int height = weights.Length;

            // Find the magnitude sum and maximum of the weights matrix
            for (int y = 0; y < weights.Length; y++) 
            {
                int[] row = weights[y];

                for (int x = 0; x < row.Length; x++) 
                {
                    int mag = Math.Abs(row[x]);
                    max = Math.Max(max, mag);
                    sum += mag;
                }
            }

            max = Math.Max(sum, max);
            while ((1 << shift) < max)
            {
                shift++;
            }

            shift = 15 - shift;
            //shift it so that it's less than 32768
            for (int y = 0; y < weights.Length; y++) 
            {
                int[] row = weights[y];

                for (int x = 0; x < row.Length; x++) 
                {
                    if (shift < 0) 
                    {
                        row[x] >>= -shift;
                    } 
                    else if (shift > 0)
                    {
                        row[x] <<= shift;
                    }
                }
            }
        }

        public void RenderConvolutionFilter(int[][] weights, int offset, RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            base.Render (dstArgs, srcArgs, roi);

            int weightsWidth = weights[0].Length;
            int weightsHeight = weights.Length;

            int fYOffset = -(weightsHeight / 2);
            int fXOffset = -(weightsWidth / 2);

            // we cache the beginning and ending horizontal indices into the weights matrix
            // for every source pixel X location
            // i.e. for src[x,y], where we're concerned with x, what weight[x,y] horizontal
            // extent should we worry about?
            // this way we end up with less branches and faster code (hopefully?!)
            FExtent fxExtent = GetFExtent(srcArgs.Surface.Width, weightsWidth);
            FExtent fyExtent = GetFExtent(srcArgs.Surface.Height, weightsHeight);

            for (int y = roi.Top; y < roi.Bottom; ++y)
            {
                ColorBgra *dstPixel = dstArgs.Surface.GetPointAddress(roi.Left, y);
                int fyStart = fyExtent.fStarts[y];
                int fyEnd = fyExtent.fEnds[y];

                for (int x = roi.Left; x < roi.Right; ++x)
                {
                    int redSum = 0;
                    int greenSum = 0;
                    int blueSum = 0;
                    int alphaSum = 0;
                    int colorFactor = 0;
                    int alphaFactor = 0;
                    int fxStart = fxExtent.fStarts[x];
                    int fxEnd = fxExtent.fEnds[x];

                    for (int fy = fyStart; fy < fyEnd; ++fy)
                    {
                        int srcY = y + fy + fYOffset;
                        int srcX1 = x + fXOffset + fxStart;

                        ColorBgra *srcPixel = srcArgs.Surface.GetPointAddress(srcX1, srcY);
                        int[] wRow = weights[fy];

                        for (int fx = fxStart; fx < fxEnd; ++fx)
                        {
                            int srcX = fx + srcX1;
                            int weight = wRow[fx];

                            ColorBgra c = *srcPixel;

                            alphaFactor += weight;
                            weight = weight * (c.A + (c.A >> 7));
                            colorFactor += weight;
                            weight >>= 8;

                            redSum += c.R * weight;
                            blueSum += c.B * weight;
                            greenSum += c.G * weight;
                            alphaSum += c.A * weight;

                            ++srcPixel;
                        }
                    }

                    colorFactor /= 256;

                    if (colorFactor != 0)
                    {
                        redSum /= colorFactor;
                        greenSum /= colorFactor;
                        blueSum /= colorFactor;
                    }
                    else
                    {
                        redSum = 0;
                        greenSum = 0;
                        blueSum = 0;
                    }

                    if (alphaFactor != 0)
                    {
                        alphaSum /= alphaFactor;
                    }
                    else
                    {
                        alphaSum = 0;
                    }

                    redSum += offset;
                    greenSum += offset;
                    blueSum += offset;
                    alphaSum += offset;

                    #region clamp values to [0,255]
                    if (redSum < 0)
                    {
                        redSum = 0;
                    }
                    else
                    if (redSum > 255)
                    {
                        redSum = 255;
                    }

                    if (greenSum < 0)
                    {
                        greenSum = 0;
                    }
                    else
                    if (greenSum > 255)
                    {
                        greenSum = 255;
                    }
                   
                    if (blueSum < 0)
                    {
                        blueSum = 0;
                    }
                    else
                    if (blueSum > 255)
                    {
                        blueSum = 255;
                    }

                    if (alphaSum < 0)
                    {
                        alphaSum = 0;
                    }
                    else
                    if (alphaSum > 255)
                    {
                        alphaSum = 255;
                    }
                    #endregion

                    *dstPixel = ColorBgra.FromRgba((byte)redSum, (byte)greenSum, (byte)blueSum, (byte)alphaSum);
                    ++dstPixel;
                }
            }
        }


        [Obsolete("The description property is obsolete.", true)]
        public ConvolutionFilterEffect(string name, string description, Image image)
            : this(name, image)
        {
        }

        public ConvolutionFilterEffect(string name, Image image)
            : this(name, image, System.Windows.Forms.Shortcut.None)
        {
        }

        [Obsolete("The description property is obsolete.", true)]
        public ConvolutionFilterEffect(string name, string description, Image image, System.Windows.Forms.Shortcut shortcut)
            : base(name, image, shortcut)
        {
        }

        public ConvolutionFilterEffect(string name, Image image, System.Windows.Forms.Shortcut shortcut)
            : base(name, image, shortcut)
        {
        }
    }
}
