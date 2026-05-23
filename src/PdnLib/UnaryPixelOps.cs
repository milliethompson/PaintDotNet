using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Provides a set of standard UnaryPixelOps.
    /// </summary>
    public sealed class UnaryPixelOps
    {
        private UnaryPixelOps()
        {
        }

        /// <summary>
        /// Passes through the given color value.
        /// result(color) = color
        /// </summary>
        [Serializable]
        public class Identity
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return color;
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra *dst, ColorBgra *src, int length)
            {
                MemoryBlock.CopyMemory(dst, src, length * sizeof(ColorBgra));
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra * ptr, int length)
            {
                return;
            }
        }

        /// <summary>
        /// Always returns a constant color.
        /// </summary>
        [Serializable]
        public class Constant
            : UnaryPixelOp
        {
            private ColorBgra setColor;

            public override ColorBgra Apply(ColorBgra color)
            {
                return setColor;
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra *dst, ColorBgra *src, int length)
            {
                while (length > 0)
                {
                    *dst = setColor;
                    ++dst;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected unsafe override void Apply(ColorBgra *ptr, int length)
            {
                while (length > 0)
                {
                    *ptr = setColor;
                    ++ptr;
                    --length;
                }
            }

            public Constant(ColorBgra setColor)
            {
                this.setColor = setColor;
            }
        }

        /// <summary>
        /// Blends pixels with the specified constant color. The alpha channel
        /// is passed through.
        /// </summary>
        [Serializable]
        public class BlendConstant
            : UnaryPixelOp
        {
            private ColorBgra blendColor;

            public override ColorBgra Apply(ColorBgra color)
            {
                int a = blendColor.A;
                int invA = 255 - a;

                int r = ((color.R * invA) + (blendColor.R * a)) / 256;
                int g = ((color.G * invA) + (blendColor.G * a)) / 256;
                int b = ((color.B * invA) + (blendColor.B * a)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, color.A);
            }

            public BlendConstant(ColorBgra blendColor)
            {
                this.blendColor = blendColor;
            }
        }

        [Serializable]
        public class MultiplyByAlpha
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                int a = color.A;
                int r = (color.R * a) / 256;
                int g = (color.G * a) / 256;
                int b = (color.B * a) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, color.A);
            }
        }

        /// <summary>
        /// Used to set a given channel of a pixel to a given, predefined color.
        /// Useful if you want to set only the alpha value of a given region.
        /// </summary>
        [Serializable]
        public class SetChannel
            : UnaryPixelOp
        {
            private int channel;
            private byte setValue;

            public override ColorBgra Apply(ColorBgra color)
            {
                color[channel] = setValue;
                return color;
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    *dst = *src;
                    (*dst)[channel] = setValue;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * ptr, int length)
            {
                while (length > 0)
                {
                    (*ptr)[channel] = setValue;
                    ++ptr;
                    --length;
                }
            }


            public SetChannel(int channel, byte setValue)
            {
                this.channel = channel;
                this.setValue = setValue;
            }
        }

        /// <summary>
        /// Specialization of SetChannel that sets the alpha channel.
        /// </summary>
        /// <remarks>This class depends on the system being litte-endian with the alpha channel 
        /// occupying the 8 most-significant-bits of a ColorBgra instance.
        /// By the way, we use addition instead of bitwise-OR because an addition can be
        /// perform very fast (0.5 cycles) on a Pentium 4.</remarks>
        [Serializable]
        public class SetAlphaChannel
            : UnaryPixelOp
        {
            private UInt32 addValue;

            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromUInt32((color.Bgra & 0x00ffffff) + addValue);
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    dst->Bgra = (src->Bgra & 0x00ffffff) + addValue;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * ptr, int length)
            {
                while (length > 0)
                {
                    ptr->Bgra = (ptr->Bgra & 0x00ffffff) + addValue;
                    ++ptr;
                    --length;
                }
            }

            public SetAlphaChannel(byte alphaValue)
            {
                addValue = (uint)alphaValue << 24;
            }
        }

        /// <summary>
        /// Specialization of SetAlphaChannel that always sets alpha to 255.
        /// </summary>
        [Serializable]
        public class SetAlphaChannelTo255
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromUInt32(color.Bgra | 0xff000000);
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    dst->Bgra = src->Bgra | 0xff000000;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            [CLSCompliant(false)]
            protected override unsafe void Apply(ColorBgra * ptr, int length)
            {
                while (length > 0)
                {
                    ptr->Bgra |= 0xff000000;
                    ++ptr;
                    --length;
                }
            }
        }

        /// <summary>
        /// Inverts a pixel's color, and passes through the alpha component.
        /// </summary>
        [Serializable]
        public class Invert
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra((byte)(255 - color.B), (byte)(255 - color.G), (byte)(255 - color.R), color.A);
            }
        }

        /// <summary>
        /// Inverts a pixel's color and its alpha component.
        /// </summary>
        [Serializable]
        public class InvertWithAlpha
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra((byte)(255 - color.B), (byte)(255 - color.G), (byte)(255 - color.R), (byte)(255 - color.A));
            }
        }

        /// <summary>
        /// Averages the input color's red, green, and blue channels. The alpha component
        /// is unaffected.
        /// </summary>
        [Serializable]
        public class AverageChannels
            : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                byte average = (byte)(((int)color.R + (int)color.G + (int)color.B) / 3);
                return ColorBgra.FromBgra(average, average, average, color.A);
            }
        }

        [Serializable]
        public class Desaturate
            : UnaryPixelOp
        {
            // These numbers taken from http://www.codeproject.com/cs/media/csharpgraphicfilters11.asp
            public override ColorBgra Apply(ColorBgra color)
            {
                byte x = (byte)((0.114 * (float)color.B) + (0.587 * (float)color.G) + (0.299 * (float)color.R));
                return ColorBgra.FromBgra(x, x, x, color.A);
            }
        }
    }
}
