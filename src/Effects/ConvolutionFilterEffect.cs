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
        public void RenderConvolutionFilter(int[,] weights, int offset, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                RenderConvolutionFilter(weights, offset, dstArgs, srcArgs, rect);
            }
        }

        private class FExtentKey
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

        public void RenderConvolutionFilter(int[,] weights, int offset, RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            base.Render (dstArgs, srcArgs, roi);

            int weightsWidth = weights.GetLength(1);
            int weightsHeight = weights.GetLength(0);

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
                    int factor = 0;
                    int fxStart = fxExtent.fStarts[x];
                    int fxEnd = fxExtent.fEnds[x];

                    for (int fy = fyStart; fy < fyEnd; ++fy)
                    {
                        int srcY = y + fy + fYOffset;
                        int srcX1 = x + fXOffset + fxStart;

                        ColorBgra *srcPixel = srcArgs.Surface.GetPointAddress(srcX1, srcY);

                        for (int fx = fxStart; fx < fxEnd; ++fx)
                        {
                            int srcX = fx + srcX1;
                            int weight = weights[fy,fx];

                            ColorBgra c = *srcPixel;

                            redSum += c.R * weight;
                            greenSum += c.G * weight;
                            blueSum += c.B * weight;
                            alphaSum += c.A * weight;
                            factor += weight;

                            ++srcPixel;
                        }
                    }

                    if (factor != 0)
                    {
                        redSum /= factor;
                        greenSum /= factor;
                        blueSum /= factor;
                        alphaSum /= factor;
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

        public ConvolutionFilterEffect(string name, string description, Image image)
            : base(name, description, image)
        {
        }
    }
}
