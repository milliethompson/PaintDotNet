using System;
using System.Collections;
using System.Reflection;

namespace PaintDotNet
{
    /// <summary>
    /// This class contains all the render ops that can be used by the user
    /// to configure a layer's blending mode. It also contains helper
    /// functions to aid in enumerating and using these blend ops.
    /// 
    /// Each class should inherit from BinaryPixelOp, require no parameters
    /// for construction, and provide a static property called StaticName
    /// that provides a friendly name for UI purposes.
    /// 
    /// Each op should be marked with the [Serializable] attribute.
    /// 
    /// Also, all 3 overrides for Apply() should be implemented, for 
    /// performance reasons. All blend op classes should also be sealed.
    /// 
    /// The alpha channel of the destination should be preserved, although
    /// it can definitely be used in calculating the 3 color channels.
    /// 
    /// The default blend op for any layer is NormalBlendOp.
    /// 
    /// Credit for mathematical descriptions of many of the blend modes goes to
    /// a page on Pegtop Software's website called, "Blend Modes"
    /// http://www.pegtop.net/delphi/blendmodes/
    /// </summary>
    public sealed class UserBlendOps
    {
        private UserBlendOps()
        {
        }

        /// <summary>
        /// Returns an array of Type objects that lists all of the pixel ops contained
        /// within this class. You can then use Utility.GetStaticName to retrieve the
        /// value of the StaticName property.
        /// </summary>
        /// <returns></returns>
        public static Type[] GetBlendOps()
        {
            Type[] allTypes = typeof(UserBlendOps).GetNestedTypes();
            ArrayList types = new ArrayList();

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(UserBlendOp)) && !type.IsAbstract)
                {
                    types.Add(type);
                }
            }

            return (Type[])types.ToArray(typeof(Type));
        }

        public static UserBlendOp CreateBlendOp(Type opType)
        {
            ConstructorInfo ci = opType.GetConstructor(System.Type.EmptyTypes);
            UserBlendOp op = (UserBlendOp)ci.Invoke(null);
            return op;
        }

        public static Type GetDefaultBlendOp()
        {
            return typeof(NormalBlendOp);
        }

        [Serializable]
        public sealed class NormalBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Normal";
                }
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

            public NormalBlendOp()
            {
            }
        }

        [Serializable]
        public sealed class MultiplyBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Multiply";
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = rhs.A;
                int invA = 256 - a;

                return ColorBgra.FromBgra((byte)(((invA * lhs.B) + ((a * (1 + lhs.B) * (1 + rhs.B)) / 256)) / 256),
                                          (byte)(((invA * lhs.G) + ((a * (1 + lhs.G) * (1 + rhs.G)) / 256)) / 256),
                                          (byte)(((invA * lhs.R) + ((a * (1 + lhs.R) * (1 + rhs.R)) / 256)) / 256),
                                          lhs.A);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = rhs->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(((invA * lhs->B) + ((a * (1 + lhs->B) * (1 + rhs->B)) / 256)) / 256)) +
                        ((uint)(((invA * lhs->G) + ((a * (1 + lhs->G) * (1 + rhs->G)) / 256)) / 256) << 8) +
                        ((uint)(((invA * lhs->R) + ((a * (1 + lhs->R) * (1 + rhs->R)) / 256)) / 256) << 16) +
                        ((uint)255 << 24);
                    
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
                    int a = src->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(((invA * dst->B) + ((a * (1 + dst->B) * (1 + src->B)) / 256)) / 256)) +
                        ((uint)(((invA * dst->G) + ((a * (1 + dst->G) * (1 + src->G)) / 256)) / 256) << 8) +
                        ((uint)(((invA * dst->R) + ((a * (1 + dst->R) * (1 + src->R)) / 256)) / 256) << 16) +
                        ((uint)255 << 24);
                    
                    ++dst;
                    ++src;
                    --length;
                }
            }

            public MultiplyBlendOp()
            {
            }
        }

        [Serializable]
            public sealed class AdditiveBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Additive";
                }
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = rhs->A + 1;

                    *dst = ColorBgra.FromBgra((byte)Math.Min(255, lhs->B + ((rhs->B * rhsA) / 256)),
                        (byte)Math.Min(255, lhs->G + ((rhs->G * rhsA) / 256)),
                        (byte)Math.Min(255, lhs->R + ((rhs->R * rhsA) / 256)),
                        lhs->A);

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
                    int srcA = src->A + 1;

                    *dst = ColorBgra.FromBgra((byte)Math.Min(255, dst->B + ((src->B * srcA) / 256)),
                        (byte)Math.Min(255, dst->G + ((src->G * srcA) / 256)),
                        (byte)Math.Min(255, dst->R + ((src->R * srcA) / 256)),
                        dst->A);

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int rhsA = rhs.A + 1;

                return ColorBgra.FromBgra((byte)Math.Min(255, lhs.B + ((rhs.B * rhsA) / 256)),
                    (byte)Math.Min(255, lhs.G + ((rhs.G * rhsA) / 256)),
                    (byte)Math.Min(255, lhs.R + ((rhs.R * rhsA) / 256)),
                    lhs.A);
            }

        }

        [Serializable]
        public sealed class ScreenBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Screen";
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = rhs.A + 1;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)(((invA * lhs.B) + (a * ((65536 - ((256 - lhs.B) * (256 - ((a * rhs.B) / 256)))) / 256))) / 256),
                    (byte)(((invA * lhs.G) + (a * ((65536 - ((256 - lhs.G) * (256 - ((a * rhs.G) / 256)))) / 256))) / 256),
                    (byte)(((invA * lhs.R) + (a * ((65536 - ((256 - lhs.R) * (256 - ((a * rhs.R) / 256)))) / 256))) / 256),
                    lhs.A);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = rhs->A + 1;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(((invA * lhs->B) + (a * ((65536 - ((256 - lhs->B) * (256 - ((a * rhs->B) / 256)))) / 256))) / 256)) + 
                        ((uint)(((invA * lhs->G) + (a * ((65536 - ((256 - lhs->G) * (256 - ((a * rhs->G) / 256)))) / 256))) / 256) << 8) +
                        ((uint)(((invA * lhs->R) + (a * ((65536 - ((256 - lhs->R) * (256 - ((a * rhs->R) / 256)))) / 256))) / 256) << 16) +
                        ((uint)lhs->A << 24);

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
                    int a = src->A + 1;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(((invA * dst->B) + (a * ((65536 - ((256 - dst->B) * (256 - ((a * src->B) / 256)))) / 256))) / 256)) + 
                        ((uint)(((invA * dst->G) + (a * ((65536 - ((256 - dst->G) * (256 - ((a * src->G) / 256)))) / 256))) / 256) << 8) +
                        ((uint)(((invA * dst->R) + (a * ((65536 - ((256 - dst->R) * (256 - ((a * src->R) / 256)))) / 256))) / 256) << 16) +
                        ((uint)dst->A << 24);

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public ScreenBlendOp()
            {
            }
        }

        [Serializable]
        public sealed class LightenBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Lighten";
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = 1 + rhs.A;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)Math.Max(lhs.B, ((lhs.B * invA) + (a * ((a * rhs.B) / 256))) / 256),
                    (byte)Math.Max(lhs.G, ((lhs.G * invA) + (a * ((a * rhs.G) / 256))) / 256),
                    (byte)Math.Max(lhs.R, ((lhs.R * invA) + (a * ((a * rhs.R) / 256))) / 256),
                    lhs.A);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(Math.Max(lhs->B, ((lhs->B * invA) + (a * ((a * rhs->B) / 256))) / 256))) +
                        ((uint)(Math.Max(lhs->G, ((lhs->G * invA) + (a * ((a * rhs->G) / 256))) / 256) << 8)) +
                        ((uint)(Math.Max(lhs->R, ((lhs->R * invA) + (a * ((a * rhs->R) / 256))) / 256) << 16)) + 
                        ((uint)lhs->A << 24);

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
                    int a = 1 + src->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(Math.Max(dst->B, ((dst->B * invA) + (a * ((a * src->B) / 256))) / 256))) +
                        ((uint)(Math.Max(dst->G, ((dst->G * invA) + (a * ((a * src->G) / 256))) / 256) << 8)) +
                        ((uint)(Math.Max(dst->R, ((dst->R * invA) + (a * ((a * src->R) / 256))) / 256) << 16)) + 
                        ((uint)dst->A << 24);

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public LightenBlendOp()
            {
            }
        }

        [Serializable]
        public sealed class DarkenBlendOp
            : UserBlendOp
        {
            public static string StaticName
            {
                get
                {
                    return "Darken";
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = 1 + rhs.A;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)Math.Min(lhs.B, ((lhs.B * invA) + (a * ((a * rhs.B) / 256))) / 256),
                    (byte)Math.Min(lhs.G, ((lhs.G * invA) + (a * ((a * rhs.G) / 256))) / 256),
                    (byte)Math.Min(lhs.R, ((lhs.R * invA) + (a * ((a * rhs.R) / 256))) / 256),
                    lhs.A);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(Math.Min(lhs->B, ((lhs->B * invA) + (a * ((a * rhs->B) / 256))) / 256))) +
                        ((uint)(Math.Min(lhs->G, ((lhs->G * invA) + (a * ((a * rhs->G) / 256))) / 256) << 8)) +
                        ((uint)(Math.Min(lhs->R, ((lhs->R * invA) + (a * ((a * rhs->R) / 256))) / 256) << 16)) + 
                        ((uint)lhs->A << 24);

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
                    int a = 1 + src->A;
                    int invA = 256 - a;

                    dst->Bgra = 
                        ((uint)(Math.Min(dst->B, ((dst->B * invA) + (a * ((a * src->B) / 256))) / 256))) +
                        ((uint)(Math.Min(dst->G, ((dst->G * invA) + (a * ((a * src->G) / 256))) / 256) << 8)) +
                        ((uint)(Math.Min(dst->R, ((dst->R * invA) + (a * ((a * src->R) / 256))) / 256) << 16)) + 
                        ((uint)dst->A << 24);

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public DarkenBlendOp()
            {
            }
        }

    }
}
