/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Provides a set of standard BinaryPixelOps.
    /// </summary>
    public sealed class BinaryPixelOps
    {
        private BinaryPixelOps()
        {
        }

        /// <summary>
        /// "Method for combining two images that uses both pixel colors and alpha values 
        /// to determine the color of the resulting pixel. This allows an image to be 
        /// rendered on top of another image, with a blend of both images showing. When 
        /// blending two pixels, the color components of both pixels are first scaled by 
        /// their alpha values. Then, the bottom pixel is scaled by the inverse of the 
        /// top pixel alpha value and added to the top pixel to form the final blended 
        /// color." -- DirectX C++ Documentation Glossary definition for "alpha blend"   
        /// Thus: result(lhs,rhs) = (1 - rhs.A)(lhs.A * lhs) + (rhs.A * rhs)
        /// Alpha channel is computed as 1 - (lha.A * rhs.A)
        /// </summary>
        [Serializable]
        public class AlphaBlend
            : BinaryPixelOp
        {
            public static ColorBgra ApplyStatic(ColorBgra lhs, ColorBgra rhs)
            {
                int rhsA = rhs.A + (rhs.A >> 7);
                int lhsA = lhs.A + (lhs.A >> 7);
                int lhsAMult = (256 - rhsA) * lhsA;
                int totalA = ((lhsA * (256 - rhsA)) >> 8) + rhsA;
                ColorBgra ret;

                if (totalA == 0)
                {
                    ret = ColorBgra.FromUInt32(0);
                }
                else
                {
                    int b = (((lhsAMult * lhs.B) >> 8) + (rhsA * rhs.B)) / totalA;
                    int g = (((lhsAMult * lhs.G) >> 8) + (rhsA * rhs.G)) / totalA;
                    int r = (((lhsAMult * lhs.R) >> 8) + (rhsA * rhs.R)) / totalA;
                    int a = ComputeAlpha(lhs.A, rhs.A);

                    ret = ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
                }

                return ret;
            }

            [CLSCompliant(false)]
            public static unsafe void ApplyStatic(ColorBgra *dst, ColorBgra *lhs, ColorBgra *rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = rhs->A + (rhs->A >> 7);
                    int lhsA = lhs->A + (lhs->A >> 7);
                    int lhsAMult = (256 - rhsA) * lhsA;
                    int totalA = ((lhsA * (256 - rhsA)) >> 8) + rhsA;

                    if (totalA == 0)
                    {
                        dst->Bgra = 0;
                    }
                    else
                    {
                        int b = (((lhsAMult * lhs->B) >> 8) + (rhsA * rhs->B)) / totalA;
                        int g = (((lhsAMult * lhs->G) >> 8) + (rhsA * rhs->G)) / totalA;
                        int r = (((lhsAMult * lhs->R) >> 8) + (rhsA * rhs->R)) / totalA;
                        int a = ComputeAlpha(lhs->A, rhs->A);

                        dst->Bgra = ColorBgra.BgraToUInt32(b, g, r, a);
                    }

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            [CLSCompliant(false)]
            public static unsafe void ApplyStatic(ColorBgra *dst, ColorBgra *src, int length)
            {
                while (length > 0)
                {
                    if (src->A == 255)
                    {
                        *dst = *src;
                    }
                    else
                    {
                        int srcA = src->A + (src->A >> 7);
                        int dstA = dst->A + (dst->A >> 7);
                        int dstAMult = (256 - srcA) * dstA;
                        int totalA = ((dstA * (256 - srcA)) >> 8) + srcA;

                        if (totalA == 0)
                        {
                            dst->Bgra = 0;
                        }
                        else
                        {
                            int b = (((dstAMult * dst->B) >> 8) + (srcA * src->B)) / totalA;
                            int g = (((dstAMult * dst->G) >> 8) + (srcA * src->G)) / totalA;
                            int r = (((dstAMult * dst->R) >> 8) + (srcA * src->R)) / totalA;
                            int a = ComputeAlpha(dst->A, src->A);

                            dst->Bgra = ColorBgra.BgraToUInt32(b, g, r, a);
                        }
                    }

                    ++dst;
                    ++src;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra *dst, ColorBgra *src, int length)
            {
                AlphaBlend.ApplyStatic(dst, src, length);
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra *dst, ColorBgra *lhs, ColorBgra *rhs, int length)
            {
                AlphaBlend.ApplyStatic(dst, lhs, rhs, length);
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                return AlphaBlend.ApplyStatic(lhs, rhs);
            }
        }

        /// <summary>
        /// F(lhs, rhs) = rhs.A + lhs.R,g,b
        /// </summary>
        public class SetAlphaChannel
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                lhs.A = rhs.A;
                return lhs;
            }
        }

        /// <summary>
        /// F(lhs, rhs) = lhs.R,g,b + rhs.A
        /// </summary>
        public class SetColorChannels
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                rhs.A = lhs.A;
                return rhs;
            }
        }

        /// <summary>
        /// result(lhs,rhs) = rhs
        /// </summary>
        [Serializable]
        public class AssignFromRhs
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                return rhs;
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra *dst, ColorBgra *lhs, ColorBgra *rhs, int length)
            {
                Memory.Copy(dst, rhs, (ulong)length * (ulong)ColorBgra.SizeOf);
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra *dst, ColorBgra *src, int length)
            {
                Memory.Copy(dst, src, (ulong)length * (ulong)ColorBgra.SizeOf);
            }
            
            public AssignFromRhs()
            {
            }
        }

        /// <summary>
        /// result(lhs,rhs) = lhs
        /// </summary>
        [Serializable]
        public class AssignFromLhs
            : BinaryPixelOp
        {
            UnaryPixelOp op;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                return lhs;
            }

            public AssignFromLhs()
            {
                op = new UnaryPixelOps.Identity();
            }
        }

        [Serializable]
        public class Swap
            : BinaryPixelOp
        {
            BinaryPixelOp swapMyArgs;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                return swapMyArgs.Apply(rhs, lhs);
            }

            public Swap(BinaryPixelOp swapMyArgs)
            {
                this.swapMyArgs = swapMyArgs;
            }
        }
    }
}
