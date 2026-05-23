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

        // Provided for compatability with beta builds
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
        ///       result.A is 255 because all alpha-based scaling has already been
        ///       performed.
        /// </summary>
        [Serializable]
        public class AlphaBlend
            : BinaryPixelOp
        {
            public static ColorBgra Apply(ColorBgra lhs, ColorBgra rhs, double alpha)
            {
                return Apply(lhs, rhs, Math.Min(255, Math.Max(0, 255.0 * alpha)));
            }

            public static ColorBgra Apply(ColorBgra lhs, ColorBgra rhs, byte rhsAlpha)
            {
                int rhsA = rhsAlpha + 1;
                int invRhsA = 256 - rhsA;
                int lhsA = lhs.A + 1;

                int r = (((invRhsA * (lhsA * lhs.R)) / 256) + (rhsA * rhs.R)) / 256;
                int g = (((invRhsA * (lhsA * lhs.G)) / 256) + (rhsA * rhs.G)) / 256;
                int b = (((invRhsA * (lhsA * lhs.B)) / 256) + (rhsA * rhs.B)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, 255);
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int rhsA = rhs.A + 1;
                int invRhsA = 256 - rhsA;
                int lhsA = lhs.A + 1;

                int r = (((invRhsA * (lhsA * lhs.R)) / 256) + (rhsA * rhs.R)) / 256;
                int g = (((invRhsA * (lhsA * lhs.G)) / 256) + (rhsA * rhs.G)) / 256;
                int b = (((invRhsA * (lhsA * lhs.B)) / 256) + (rhsA * rhs.B)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, 255);
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = rhs->A + 1;
                    int invRhsA = 256 - rhsA;
                    int lhsA = lhs->A + 1;

                    int r = (((invRhsA * (lhsA * lhs->R)) / 256) + (rhsA * rhs->R)) / 256;
                    int g = (((invRhsA * (lhsA * lhs->G)) / 256) + (rhsA * rhs->G)) / 256;
                    int b = (((invRhsA * (lhsA * lhs->B)) / 256) + (rhsA * rhs->B)) / 256;
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16)) + ((uint)255 << 24);

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int srcA = src->A + 1;
                    int invSrcA = 256 - srcA;
                    int dstA = dst->A + 1;

                    int r = (((invSrcA * (dstA * dst->R)) / 256) + (srcA * src->R)) / 256;
                    int g = (((invSrcA * (dstA * dst->G)) / 256) + (srcA * src->G)) / 256;
                    int b = (((invSrcA * (dstA * dst->B)) / 256) + (srcA * src->B)) / 256;
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16)) + ((uint)255 << 24);

                    ++dst;
                    ++src;
                    --length;
                }
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
            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                MemoryBlock.CopyMemory(dst, rhs, length * 4);
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                MemoryBlock.CopyMemory(dst, src, length * 4);
            }
            
            public AssignFromRhs()
            {
            }
        }

        [Serializable]
        public class MultipliedAssignFromRhs
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = 1 + rhs.A;
                int b = (rhs.B * a) / 256;
                int g = (rhs.G * a) / 256;
                int r = (rhs.R * a) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, lhs.A);
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->A;
                    int b = (rhs->B * a) / 256;
                    int g = (rhs->G * a) / 256;
                    int r = (rhs->R * a) / 256;
                    
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)lhs->A << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int a = 1 + src->A;
                    int b = (src->B * a) / 256;
                    int g = (src->G * a) / 256;
                    int r = (src->R * a) / 256;
                    
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)dst->A << 24));

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        /// <summary>
        /// result(lhs,rhs) = min(1, lhs + rhs)
        /// </summary>
        [Serializable]
        public class Add
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int r = lhs.R + rhs.R;
                int g = lhs.G + rhs.G;
                int b = lhs.B + rhs.B;
                int a = lhs.A + rhs.A;

                if (r > 255)
                {
                    r = 255;
                }

                if (g > 255)
                {
                    g = 255;
                }

                if (b > 255)
                {
                    b = 255;
                }

                if (a > 255)
                {
                    a = 255;
                }

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, (byte)a);
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
