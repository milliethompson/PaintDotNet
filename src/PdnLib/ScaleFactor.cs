using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ScaleFactor.
    /// </summary>
    public struct ScaleFactor
    {
        private int numerator;
        private int denominator;

        public static readonly ScaleFactor OneToOne = new ScaleFactor(1, 1);

        public static bool operator== (ScaleFactor lhs, ScaleFactor rhs)
        {
            return lhs.numerator == rhs.numerator && lhs.denominator == rhs.denominator;
        }

        public static bool operator!= (ScaleFactor lhs, ScaleFactor rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            ScaleFactor rhs = (ScaleFactor)obj;
            return (numerator == rhs.numerator) && (denominator == rhs.denominator);
        } 

        public override int GetHashCode()
        {
            return (int)((ushort)denominator + ((ushort)numerator << 16));
        }

        public int Numerator
        {
            get
            {
                return numerator;
            }
        }

        public int Denominator
        {
            get
            {
                return denominator;
            }
        }

        public override string ToString()
        {
            return numerator.ToString() + ":" + denominator.ToString();
        }

        public double Ratio
        {
            get
            {
                double d = (double)numerator / (double)denominator;
                return d;
            }
        }

        private int CountBits(int x)
        {
            uint y = (uint)x;
            int count = 0;

            for (int bit = 0; bit < 32; ++bit)
            {
                if ((y & ((uint)1 << bit)) != 0)
                {
                    ++count;
                }
            }

            return count;
        }

        public int ScaleScalar(int x)
        {
            return (x * numerator) / denominator;
        }

        public int UnscaleScalar(int x)
        {
            return (x * denominator) / numerator;
        }

        public float ScaleScalar(float x)
        {
            return (x * (float)numerator) / (float)denominator;
        }

        public float UnscaleScalar(float x)
        {
            return (x * (float)denominator) / (float)numerator;
        }

        public double ScaleScalar(double x)
        {
            return (x * (double)numerator) / (double)denominator;
        }

        public double UnscaleScalar(double x)
        {
            return (x * (double)denominator) / (double)numerator;
        }

        public PointF ScalePoint(PointF p)
        {
            return new PointF(ScaleScalar(p.X), ScaleScalar(p.Y));
        }

        public PointF ScalePointJustX(PointF p)
        {
            return new PointF(ScaleScalar(p.X), p.Y);
        }

        public PointF ScalePointJustY(PointF p)
        {
            return new PointF(p.X, ScaleScalar(p.Y));
        }

        public PointF UnscalePoint(PointF p)
        {
            return new PointF(UnscaleScalar(p.X), UnscaleScalar(p.Y));
        }

        public PointF UnscalePointJustX(PointF p)
        {
            return new PointF(UnscaleScalar(p.X), p.Y);
        }

        public PointF UnscalePointJustY(PointF p)
        {
            return new PointF(p.X, UnscaleScalar(p.Y));
        }

        public Point ScalePoint(Point p)
        {
            return new Point(ScaleScalar(p.X), ScaleScalar(p.Y));
        }

        public Point ScalePointJustX(Point p)
        {
            return new Point(ScaleScalar(p.X), p.Y);
        }

        public Point ScalePointJustY(Point p)
        {
            return new Point(p.X, ScaleScalar(p.Y));
        }

        public Point UnscalePoint(Point p)
        {
            return new Point(UnscaleScalar(p.X), UnscaleScalar(p.Y));
        }

        public Point UnscalePointJustX(Point p)
        {
            return new Point(UnscaleScalar(p.X), p.Y);
        }

        public Point UnscalePointJustY(Point p)
        {
            return new Point(p.X, UnscaleScalar(p.Y));
        }

        public SizeF ScaleSize(SizeF s)
        {
            return new SizeF(ScaleScalar(s.Width), ScaleScalar(s.Height));
        }

        public SizeF UnscaleSize(SizeF s)
        {
            return new SizeF(UnscaleScalar(s.Width), UnscaleScalar(s.Height));
        }

        public Size ScaleSize(Size s)
        {
            return new Size(ScaleScalar(s.Width), ScaleScalar(s.Height));
        }

        public Size UnscaleSize(Size s)
        {
            return new Size(UnscaleScalar(s.Width), UnscaleScalar(s.Height));
        }

        public RectangleF ScaleRectangle(RectangleF rectF)
        {
            return new RectangleF(ScalePoint(rectF.Location), ScaleSize(rectF.Size));
        }

        public RectangleF UnscaleRectangle(RectangleF rectF)
        {
            return new RectangleF(UnscalePoint(rectF.Location), UnscaleSize(rectF.Size));
        }

        public Rectangle ScaleRectangle(Rectangle rect)
        {
            return new Rectangle(ScalePoint(rect.Location), ScaleSize(rect.Size));
        }

        public Rectangle UnscaleRectangle(Rectangle rect)
        {
            return new Rectangle(UnscalePoint(rect.Location), UnscaleSize(rect.Size));
        }

        public ScaleFactor GetNextLarger()
        {
            if (numerator == 1 && denominator > 1)
            {
                return new ScaleFactor(1, denominator / 2);
            }
            else
            //if (numerator >= 1 && denominator == 1)
            {
                return new ScaleFactor(2 * numerator, 1);
            }
        }

        public ScaleFactor GetNextSmaller()
        {
            if (numerator == 1 && denominator >= 1)
            {
                return new ScaleFactor(1, 2 * denominator);
            }
            else
            //if (numerator > 1 && denominator == 1)
            {
                return new ScaleFactor(numerator / 2, 1);
            }
        }

        public ScaleFactor(int numerator, int denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;

            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }

            if (numerator <= 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }

            if (denominator != 1 && numerator != 1)
            {
                throw new ArgumentOutOfRangeException("numerator, denominator", "either numerator or denominator must equal 1");
            }

            // If we have a 1/x situation, make sure that it is 1/(2^n) where x=2^n
            if (CountBits(denominator) != 1)
            {
                throw new ArgumentOutOfRangeException("denominator", "denominator must be a power of 2");
            }
        }
    }
}
