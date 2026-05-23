/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates functionality for zooming/scaling coordinates.
    /// Includes methods for Size[F]'s, Point[F]'s, Rectangle[F]'s,
    /// and various scalars
    /// </summary>
    public struct ScaleFactor
    {
        private int denominator;
        private int numerator;

        public int Denominator 
        {
            get 
            {
                return denominator;
            }
        }

        public int Numerator 
        {
            get 
            {
                return numerator;
            }
        }

        public double Ratio
        {
            get
            {
                return (double)numerator / (double)denominator;
            }
        }

        public static readonly ScaleFactor OneToOne = new ScaleFactor(1, 1);
        public static readonly ScaleFactor MinValue = new ScaleFactor(1, 100);
        public static readonly ScaleFactor MaxValue = new ScaleFactor(32, 1);

        private void Clamp() 
        {
            if (this < MinValue)
            {
                this = MinValue;
            }
            else if (this > MaxValue)
            {
                this = MaxValue;
            }
        }

        public static ScaleFactor UseIfValid(int numerator, int denominator, ScaleFactor lastResort)
        {
            if (numerator <= 0 || denominator <= 0)
            {
                return lastResort;
            }
            else
            {
                return new ScaleFactor(numerator, denominator);
            }
        }

        public static ScaleFactor Min(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor a = UseIfValid(n1, d1, lastResort);
            ScaleFactor b = UseIfValid(n2, d2, lastResort);
            return ScaleFactor.Min(a, b);
        }

        public static ScaleFactor Max(int n1, int d1, int n2, int d2, ScaleFactor lastResort)
        {
            ScaleFactor a = UseIfValid(n1, d1, lastResort);
            ScaleFactor b = UseIfValid(n2, d2, lastResort);
            return ScaleFactor.Max(a, b);
        }

        public static ScaleFactor Min(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs < rhs)
            {
                return lhs;
            }
            else
            {
                return rhs;
            }
        }

        public static ScaleFactor Max(ScaleFactor lhs, ScaleFactor rhs)
        {
            if (lhs > rhs)
            {
                return lhs;
            }
            else
            {
                return lhs;
            }
        }

        public static bool operator==(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.numerator * rhs.denominator) == (rhs.numerator * lhs.denominator);
        }

        public static bool operator!=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator<(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.numerator * rhs.denominator) < (rhs.numerator * lhs.denominator);
        }

        public static bool operator<=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.numerator * rhs.denominator) <= (rhs.numerator * lhs.denominator);
        }

        public static bool operator>(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.numerator * rhs.denominator) > (rhs.numerator * lhs.denominator);
        }

        public static bool operator>=(ScaleFactor lhs, ScaleFactor rhs)
        {
            return (lhs.numerator * rhs.denominator) >= (rhs.numerator * lhs.denominator);
        }

        public override bool Equals(object obj)
        {
            if (obj is ScaleFactor) 
            {
                ScaleFactor rhs = (ScaleFactor)obj;
                return this == rhs;
            }
            else
            {
                return false;
            }
        } 

        public override int GetHashCode()
        {
            return numerator.GetHashCode() ^ denominator.GetHashCode();
        }

        private static string percentageFormat = PdnResources.GetString("ScaleFactor.Percentage.Format");
        public override string ToString()
        {
            return string.Format(percentageFormat, Math.Round(100 * Ratio));
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

        public Point ScalePoint(Point p)
        {
            return new Point(ScaleScalar(p.X), ScaleScalar(p.Y));
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

        /// <summary>
        /// Rounds the current scaling factor up to the next power of two.
        /// </summary>
        /// <returns>The new ScaleFactor value.</returns>
        public ScaleFactor GetNextLarger()
        {
            //Add 0.01 so that we zoom in by at least 0.5%, regardless of rounding
            double log = Math.Log(Ratio + 0.005, 2.0f);
            double newLog = Math.Floor(log + 1); 
            double newzoom = Math.Pow(2.0, newLog);

            if (newzoom > MaxValue.Ratio)
            {
                newzoom = MaxValue.Ratio;
            }

            return ScaleFactor.FromDouble(newzoom);
        }

        public ScaleFactor GetNextSmaller()
        {
            //Add 0.01 so that we zoom out by at least 0.5%, regardless of rounding
            double log = Math.Log(Ratio - 0.005, 2.0f);
            double newLog = Math.Ceiling(log - 1);
            double newzoom = Math.Pow(2.0, newLog);

            if (newzoom < MinValue.Ratio)
            {
                newzoom = MinValue.Ratio;
            }

            return ScaleFactor.FromDouble(newzoom);
        }

        private static ScaleFactor Reduce(int numerator, int denominator)
        {
            int factor = 2;

            while (factor < denominator && factor < numerator)
            {
                if ((numerator % factor) == 0 && (denominator % factor) == 0)
                {
                    numerator /= factor;
                    denominator /= factor;
                }
                else
                {
                    ++factor;
                }
            }

            return new ScaleFactor(numerator, denominator);
        }

        public static ScaleFactor FromDouble(double scalar)
        {
            int numerator = (int)(Math.Floor(scalar * 1000.0));
            int denominator = 1000;
            return Reduce(numerator, denominator);
        }

        public ScaleFactor(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }

            if (numerator < 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }

            this.numerator = numerator;
            this.denominator = denominator;
            this.Clamp();
        }
    }
}
