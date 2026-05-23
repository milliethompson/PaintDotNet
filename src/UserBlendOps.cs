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
                int a = rhs.a;
                int invA = 256 - a;

                return ColorBgra.FromBgra((byte)(((invA * lhs.b) + ((a * (1 + lhs.b) * (1 + rhs.b)) / 256)) / 256),
                                          (byte)(((invA * lhs.g) + ((a * (1 + lhs.g) * (1 + rhs.g)) / 256)) / 256),
                                          (byte)(((invA * lhs.r) + ((a * (1 + lhs.r) * (1 + rhs.r)) / 256)) / 256),
                                          lhs.a);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = rhs->a;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(((invA * lhs->b) + ((a * (1 + lhs->b) * (1 + rhs->b)) / 256)) / 256)) +
                        ((uint)(((invA * lhs->g) + ((a * (1 + lhs->g) * (1 + rhs->g)) / 256)) / 256) << 8) +
                        ((uint)(((invA * lhs->r) + ((a * (1 + lhs->r) * (1 + rhs->r)) / 256)) / 256) << 16) +
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
                    int a = src->a;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(((invA * dst->b) + ((a * (1 + dst->b) * (1 + src->b)) / 256)) / 256)) +
                        ((uint)(((invA * dst->g) + ((a * (1 + dst->g) * (1 + src->g)) / 256)) / 256) << 8) +
                        ((uint)(((invA * dst->r) + ((a * (1 + dst->r) * (1 + src->r)) / 256)) / 256) << 16) +
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
                    int rhsA = rhs->a + 1;

                    *dst = ColorBgra.FromBgra((byte)Math.Min(255, lhs->b + ((rhs->b * rhsA) / 256)),
                                              (byte)Math.Min(255, lhs->g + ((rhs->g * rhsA) / 256)),
                                              (byte)Math.Min(255, lhs->r + ((rhs->r * rhsA) / 256)),
                                              lhs->a);

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

                    *dst = ColorBgra.FromBgra((byte)Math.Min(255, dst->b + ((src->b * srcA) / 256)),
                                              (byte)Math.Min(255, dst->g + ((src->g * srcA) / 256)),
                                              (byte)Math.Min(255, dst->r + ((src->r * srcA) / 256)),
                                              dst->a);

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int rhsA = rhs.a + 1;

                return ColorBgra.FromBgra((byte)Math.Min(255, lhs.b + ((rhs.b * rhsA) / 256)),
                                          (byte)Math.Min(255, lhs.g + ((rhs.g * rhsA) / 256)),
                                          (byte)Math.Min(255, lhs.r + ((rhs.r * rhsA) / 256)),
                                          lhs.a);
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
                int a = rhs.a + 1;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)(((invA * lhs.b) + (a * ((65536 - ((256 - lhs.b) * (256 - ((a * rhs.b) / 256)))) / 256))) / 256),
                    (byte)(((invA * lhs.g) + (a * ((65536 - ((256 - lhs.g) * (256 - ((a * rhs.g) / 256)))) / 256))) / 256),
                    (byte)(((invA * lhs.r) + (a * ((65536 - ((256 - lhs.r) * (256 - ((a * rhs.r) / 256)))) / 256))) / 256),
                    lhs.a);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = rhs->a + 1;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(((invA * lhs->b) + (a * ((65536 - ((256 - lhs->b) * (256 - ((a * rhs->b) / 256)))) / 256))) / 256)) + 
                        ((uint)(((invA * lhs->g) + (a * ((65536 - ((256 - lhs->g) * (256 - ((a * rhs->g) / 256)))) / 256))) / 256) << 8) +
                        ((uint)(((invA * lhs->r) + (a * ((65536 - ((256 - lhs->r) * (256 - ((a * rhs->r) / 256)))) / 256))) / 256) << 16) +
                        ((uint)lhs->a << 24);

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
                    int a = src->a + 1;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(((invA * dst->b) + (a * ((65536 - ((256 - dst->b) * (256 - ((a * src->b) / 256)))) / 256))) / 256)) + 
                        ((uint)(((invA * dst->g) + (a * ((65536 - ((256 - dst->g) * (256 - ((a * src->g) / 256)))) / 256))) / 256) << 8) +
                        ((uint)(((invA * dst->r) + (a * ((65536 - ((256 - dst->r) * (256 - ((a * src->r) / 256)))) / 256))) / 256) << 16) +
                        ((uint)dst->a << 24);

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
                int a = 1 + rhs.a;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)Math.Max(lhs.b, ((lhs.b * invA) + (a * ((a * rhs.b) / 256))) / 256),
                    (byte)Math.Max(lhs.g, ((lhs.g * invA) + (a * ((a * rhs.g) / 256))) / 256),
                    (byte)Math.Max(lhs.r, ((lhs.r * invA) + (a * ((a * rhs.r) / 256))) / 256),
                    lhs.a);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->a;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(Math.Max(lhs->b, ((lhs->b * invA) + (a * ((a * rhs->b) / 256))) / 256))) +
                        ((uint)(Math.Max(lhs->g, ((lhs->g * invA) + (a * ((a * rhs->g) / 256))) / 256) << 8)) +
                        ((uint)(Math.Max(lhs->r, ((lhs->r * invA) + (a * ((a * rhs->r) / 256))) / 256) << 16)) + 
                        ((uint)lhs->a << 24);

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
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(Math.Max(dst->b, ((dst->b * invA) + (a * ((a * src->b) / 256))) / 256))) +
                        ((uint)(Math.Max(dst->g, ((dst->g * invA) + (a * ((a * src->g) / 256))) / 256) << 8)) +
                        ((uint)(Math.Max(dst->r, ((dst->r * invA) + (a * ((a * src->r) / 256))) / 256) << 16)) + 
                        ((uint)dst->a << 24);

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
                int a = 1 + rhs.a;
                int invA = 256 - a;

                return ColorBgra.FromBgra(
                    (byte)Math.Min(lhs.b, ((lhs.b * invA) + (a * ((a * rhs.b) / 256))) / 256),
                    (byte)Math.Min(lhs.g, ((lhs.g * invA) + (a * ((a * rhs.g) / 256))) / 256),
                    (byte)Math.Min(lhs.r, ((lhs.r * invA) + (a * ((a * rhs.r) / 256))) / 256),
                    lhs.a);
            }

            protected unsafe override void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = 1 + rhs->a;
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(Math.Min(lhs->b, ((lhs->b * invA) + (a * ((a * rhs->b) / 256))) / 256))) +
                        ((uint)(Math.Min(lhs->g, ((lhs->g * invA) + (a * ((a * rhs->g) / 256))) / 256) << 8)) +
                        ((uint)(Math.Min(lhs->r, ((lhs->r * invA) + (a * ((a * rhs->r) / 256))) / 256) << 16)) + 
                        ((uint)lhs->a << 24);

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
                    int invA = 256 - a;

                    dst->bgra = 
                        ((uint)(Math.Min(dst->b, ((dst->b * invA) + (a * ((a * src->b) / 256))) / 256))) +
                        ((uint)(Math.Min(dst->g, ((dst->g * invA) + (a * ((a * src->g) / 256))) / 256) << 8)) +
                        ((uint)(Math.Min(dst->r, ((dst->r * invA) + (a * ((a * src->r) / 256))) / 256) << 16)) + 
                        ((uint)dst->a << 24);

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
