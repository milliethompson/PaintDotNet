/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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

        // Provided for compatability with some older 1.x beta builds
        [Obsolete]
        [Serializable]
        public class AlphaFromRhsBlend : AlphaBlend
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
                int rhsA = rhs.A + 1;
                int invRhsA = 256 - rhsA;
                int lhsA = lhs.A + 1;
                int invLhsA = 256 - lhsA;

                int r = (((invRhsA * (lhsA * lhs.R)) / 256) + (rhsA * rhs.R)) / 256;
                int g = (((invRhsA * (lhsA * lhs.G)) / 256) + (rhsA * rhs.G)) / 256;
                int b = (((invRhsA * (lhsA * lhs.B)) / 256) + (rhsA * rhs.B)) / 256;
                int a = ComputeAlpha(lhs.A, rhs.A);

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                return AlphaBlend.ApplyStatic(lhs, rhs);
            }

            [CLSCompliant(false)]
            public static unsafe void ApplyStatic(ColorBgra *dst, ColorBgra *lhs, ColorBgra *rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = rhs->A + 1;
                    int invRhsA = 256 - rhsA;
                    int lhsA = lhs->A + 1;
                    int invLhsA = 256 - lhsA;

                    int r = (((invRhsA * (lhsA * lhs->R)) / 256) + (rhsA * rhs->R)) / 256;
                    int g = (((invRhsA * (lhsA * lhs->G)) / 256) + (rhsA * rhs->G)) / 256;
                    int b = (((invRhsA * (lhsA * lhs->B)) / 256) + (rhsA * rhs->B)) / 256;
                    int a = ComputeAlpha(lhs->A, rhs->A);
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)a << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra *dst, ColorBgra *lhs, ColorBgra *rhs, int length)
            {
                AlphaBlend.ApplyStatic(dst, lhs, rhs, length);
            }

            [CLSCompliant(false)]
            public static unsafe void ApplyStatic(ColorBgra *dst, ColorBgra *src, int length)
            {
                while (length > 0)
                {
                    int srcA = src->A + 1;
                    int invSrcA = 256 - srcA;
                    int dstA = dst->A + 1;

                    int r = (((invSrcA * (dstA * dst->R)) / 256) + (srcA * src->R)) / 256;
                    int g = (((invSrcA * (dstA * dst->G)) / 256) + (srcA * src->G)) / 256;
                    int b = (((invSrcA * (dstA * dst->B)) / 256) + (srcA * src->B)) / 256;
                    int a = ComputeAlpha(dst->A, src->A);
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)a << 24));

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
