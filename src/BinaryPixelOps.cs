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
        /// Thus: result(lhs,rhs) = (1 - rhs.a)(lhs.a * lhs) + (rhs.a * rhs)
        ///       result.a is 255 because all alpha-based scaling has already been
        ///       performed.
        /// </summary>
        [Serializable]
        public class AlphaBlend
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int rhsA = rhs.a + 1;
                int invRhsA = 256 - rhsA;
                int lhsA = lhs.a + 1;

                int r = (((invRhsA * (lhsA * lhs.r)) / 256) + (rhsA * rhs.r)) / 256;
                int g = (((invRhsA * (lhsA * lhs.g)) / 256) + (rhsA * rhs.g)) / 256;
                int b = (((invRhsA * (lhsA * lhs.b)) / 256) + (rhsA * rhs.b)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, 255);
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = rhs->a + 1;
                    int invRhsA = 256 - rhsA;
                    int lhsA = lhs->a + 1;

                    int r = (((invRhsA * (lhsA * lhs->r)) / 256) + (rhsA * rhs->r)) / 256;
                    int g = (((invRhsA * (lhsA * lhs->g)) / 256) + (rhsA * rhs->g)) / 256;
                    int b = (((invRhsA * (lhsA * lhs->b)) / 256) + (rhsA * rhs->b)) / 256;
                
                    dst->bgra = (uint)(b + (g << 8) + (r << 16)) + ((uint)255 << 24);

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int srcA = src->a + 1;
                    int invSrcA = 256 - srcA;
                    int dstA = dst->a + 1;

                    int r = (((invSrcA * (dstA * dst->r)) / 256) + (srcA * src->r)) / 256;
                    int g = (((invSrcA * (dstA * dst->g)) / 256) + (srcA * src->g)) / 256;
                    int b = (((invSrcA * (dstA * dst->b)) / 256) + (srcA * src->b)) / 256;
                
                    dst->bgra = (uint)(b + (g << 8) + (r << 16)) + ((uint)255 << 24);

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        /// <summary>
        /// F(lhs, rhs) = rhs.a + lhs.r,g,b
        /// </summary>
        public class SetAlphaChannel
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                lhs.a = rhs.a;
                return lhs;
            }
        }

        /// <summary>
        /// F(lhs, rhs) = lhs.r,g,b + rhs.a
        /// </summary>
        public class SetColorChannels
            : BinaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                rhs.a = lhs.a;
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

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                MemoryBlock.CopyMemory(dst, rhs, length * 4);
            }

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
                int a = 1 + rhs.a;
                int b = (rhs.b * a) / 256;
                int g = (rhs.g * a) / 256;
                int r = (rhs.r * a) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, lhs.a);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->a;
                    int b = (rhs->b * a) / 256;
                    int g = (rhs->g * a) / 256;
                    int r = (rhs->r * a) / 256;
                    
                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)lhs->a << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int a = 1 + src->a;
                    int b = (src->b * a) / 256;
                    int g = (src->g * a) / 256;
                    int r = (src->r * a) / 256;
                    
                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)dst->a << 24));

                    ++dst;
                    ++src;
                    --length;
                }
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
