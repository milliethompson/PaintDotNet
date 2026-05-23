using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MotionBlurEffectConfigToken.
	/// </summary>
	public class MotionBlurEffectConfigToken
        : EffectConfigToken
	{
        private int angle;
        public int Angle
        {
            get
            {
                return angle;
            }

            set
            {
                this.angle = value;
                linePoints = null;
            }
        }

        private int distance;
        public int Distance
        {
            get
            {
                return distance;
            }

            set
            {
                this.distance = value;
                linePoints = null;
            }
        }

        private bool centered;
        public bool Centered
        {
            get
            {
                return centered;
            }

            set
            {
                centered = value;
                linePoints = null;
            }
        }

        private Point[] linePoints = null;
        public Point[] LinePoints
        {
            get
            {
                if (linePoints == null)
                {
                    Point start = new Point(0, 0);
                    double theta = ((double)(angle + 180) * 2 * Math.PI) / 360.0;
                    double alpha = (double)distance;
                    double x = alpha * Math.Cos(theta);
                    double y = alpha * Math.Sin(theta);
                    Point end = new Point((int)x, -(int)y);

                    if (centered)
                    {
                        start.X = -end.X / 2;
                        start.Y = -end.Y / 2;
                        end.X /= 2;
                        end.Y /= 2;
                    }

                    linePoints = Utility.GetLinePoints(start, end);
                }

                return linePoints;
            }
        }

        public override object Clone()
        {
            return new MotionBlurEffectConfigToken(this);
        }

        public MotionBlurEffectConfigToken(int angle, int distance, bool centered)
            : base()
        {
            this.angle = angle;
            this.distance = distance;
            this.centered = centered;
        }

        protected MotionBlurEffectConfigToken(MotionBlurEffectConfigToken copyMe)
            : base(copyMe)
        {
            this.angle = copyMe.angle;
            this.distance = copyMe.distance;
            this.centered = copyMe.centered;
        }
	}
}
