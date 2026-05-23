using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintDotNet
{
    /// <summary>
    /// This is our pixel format that we will work with. It is always 32-bits / 4-bytes and is
    /// always laid out in BGRA order.
    /// Generally used with the Surface class.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorBgra
    {
        [FieldOffset(0)] public byte b;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte r;
        [FieldOffset(3)] public byte a;

		/// <summary>
		/// Lets you change b, g, r, and a at the same time.
		/// </summary>
		[FieldOffset(0)] public uint bgra;

		public const int BlueChannel = 0;
        public const int GreenChannel = 1;
        public const int RedChannel = 2;
        public const int AlphaChannel = 3;

        public unsafe byte this[int channel]
        {
            get
            {
#if DEBUG
                if (channel < 0 || channel > 3)
                {
                    throw new ArgumentOutOfRangeException("channel", "valid range is [0,3]");
                }
#endif

                fixed (byte *p = &b)
                {
                    return p[channel];
                }
            }

            set
            {
#if DEBUG
                if (channel < 0 || channel > 3)
                {
                    throw new ArgumentOutOfRangeException("channel", "valid range is [0,3]");
                }
#endif
                fixed (byte *p = &b)
                {
                    p[channel] = value;
                }
            }
        }

		public static bool operator == (ColorBgra lhs, ColorBgra rhs)
		{
			return lhs.bgra == rhs.bgra;
		}

		public static bool operator != (ColorBgra lhs, ColorBgra rhs)
		{
			return lhs.bgra != rhs.bgra;
		}

		public override bool Equals(object obj)
		{
			return (ColorBgra)obj == this; 
		}

    	public override int GetHashCode()
		{
            unchecked
            {
                return (int)bgra;
            }
		}

        public static PixelFormat PixelFormat
        {
            get
            {
                return PixelFormat.Format32bppArgb;
            }
        }

        public static ColorBgra FromRgba(byte r, byte g, byte b, byte a)
        {
            ColorBgra color = new ColorBgra();

            color.r = r;
            color.g = g;
            color.b = b;
            color.a = a;

            return color;
        }

        public static ColorBgra FromRgb(byte r, byte g, byte b)
        {
            return FromRgba(r, g, b, 255);
        }

        public static ColorBgra FromBgra(byte b, byte g, byte r, byte a)
        {
            return FromRgba(r, g, b, a);
        }

        public static ColorBgra FromBgr(byte b, byte g, byte r)
        {
            return FromRgb(r, g, b);
        }

        public static ColorBgra FromUInt32(UInt32 bgra)
        {
            ColorBgra color = new ColorBgra();

            color.bgra = bgra;
            return color;
        }

        public static ColorBgra FromColor(Color c)
        {
            return FromRgba(c.R, c.G, c.B, c.A);
        }

        public Color ToColor()
        {
            return Color.FromArgb(a, r, g, b);
        }
    }
}
