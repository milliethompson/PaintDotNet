using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates functionality for zooming/scaling coordinates.
    /// Includes functionality for Size[F]'s, Point[F]'s, Rectangle[F]'s,
    /// and various scalars
    /// </summary>
    public struct ScaleFactor
    {
		private float factor;

		public int Denominator 
		{
			get 
			{
				return (int)Math.Ceiling(1.0f / factor);
			}
		}

		public int Numerator 
		{
			get 
			{
				return (int)Math.Ceiling(factor);
			}
		}

        public static readonly ScaleFactor OneToOne = new ScaleFactor(1.0f);
		public const float MinZoom = 0.01f, MaxZoom = 16.0f;
		private void Clamp() 
		{
			factor = Utility.Clamp(factor, MinZoom, MaxZoom);
		}

        public static bool operator== (ScaleFactor lhs, ScaleFactor rhs)
        {
            return lhs.factor == rhs.factor;
        }

        public static bool operator!= (ScaleFactor lhs, ScaleFactor rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
			if (obj is ScaleFactor) 
			{
				ScaleFactor rhs = (ScaleFactor)obj;
				return factor == rhs.factor;
			}
			else
				return false;
        } 

        public override int GetHashCode()
        {
            return factor.GetHashCode();
        }

        public override string ToString()
        {
			return Math.Round(100 * factor).ToString() + "%";
        }

        public double Ratio
        {
            get
            {
                return factor;
            }
        }

        public int ScaleScalar(int x)
        {
            return (int)Math.Round(x * factor);
        }

        public int UnscaleScalar(int x)
        {
            return (int)Math.Round(x / factor);
        }

        public float ScaleScalar(float x)
        {
            return x * factor;
        }

        public float UnscaleScalar(float x)
        {
            return x / factor;
        }

        public double ScaleScalar(double x)
        {
            return x * factor;
        }

        public double UnscaleScalar(double x)
        {
            return x / factor;
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
			double log = Math.Log(factor, 2.0f), newzoom;
			log = Math.Ceiling(log + 0.25);
			newzoom = Math.Pow(2.0, log);
			if (newzoom > MaxZoom)
				newzoom = MaxZoom;
			return new ScaleFactor((float)newzoom);
        }

        public ScaleFactor GetNextSmaller()
		{
			double log = Math.Log(factor, 2.0f), newzoom;
			log = Math.Floor(log - 0.25);
			newzoom = Math.Pow(2.0, log);
			if (newzoom < MinZoom) 
				newzoom = MinZoom;
			return new ScaleFactor((float)newzoom);
		}

		public ScaleFactor(float ratio) 
		{
			factor = Utility.Clamp(ratio, MinZoom, MaxZoom);
			this.Clamp();
		}

        public ScaleFactor(int numerator, int denominator)
        {
            if (denominator <= 0)
            {
                throw new ArgumentOutOfRangeException("denominator", "must be greater than 0");
            }

            if (numerator <= 0)
            {
                throw new ArgumentOutOfRangeException("numerator", "must be greater than 0");
            }
			factor = (float)numerator / (float)denominator;
			this.Clamp();
        }
    }
}
