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
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    public class MoveNubRenderer
        : SurfaceBoxRenderer
    {
        private PointF location;
        private int size;
        private Matrix transform;
        private float transformAngle;
        private int alpha;
        private bool drawCompass;

        public PointF Location
        {
            get
            {
                return this.location;
            }

            set
            {
                InvalidateOurself();
                this.location = value;
                InvalidateOurself();
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }

            set
            {
                if (value != this.size)
                {
                    InvalidateOurself();
                    this.size = value;
                    InvalidateOurself();
                }
            }
        }

        public Matrix Transform
        {
            get
            {
                return this.transform.Clone();
            }

            set
            {
                InvalidateOurself();

                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (this.transform != null)
                {
                    this.transform.Dispose();
                    this.transform = null;
                }

                this.transform = value.Clone();
                this.transformAngle = Utility.GetAngleOfTransform(this.transform);
                InvalidateOurself();
            }
        }

        public int Alpha
        {
            get
            {
                return this.alpha;
            }

            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentOutOfRangeException("value", value, "value must be [0, 255]");
                }

                if (this.alpha != value)
                {
                    this.alpha = value;
                    InvalidateOurself();
                }
            }
        }

        public bool DrawCompass
        {
            get
            {
                return this.drawCompass;
            }

            set
            {
                if (this.drawCompass != value)
                {
                    this.drawCompass = value;
                    InvalidateOurself();
                }
            }
        }

        private RectangleF GetOurRectangle()
        {
            PointF[] ptFs = new PointF[1] { this.location };
            this.transform.TransformPoints(ptFs);
            float ratio = 1.0f / (float)OwnerList.ScaleFactor.Ratio;
            RectangleF rectF = new RectangleF(ptFs[0], new SizeF(0, 0));
            rectF.Inflate(ratio * (float)this.size, ratio * (float)this.size);
            return rectF;
        }

        private void InvalidateOurself()
        {
            RectangleF rectF = GetOurRectangle();
            Rectangle rect = Utility.RoundRectangle(rectF);
            rect.Inflate(1, 1);
            Invalidate(rect);
        }

        public bool IsPointTouching(Point pt, bool pad)
        {
            RectangleF rectF = GetOurRectangle();

            if (pad)
            {
                float padding = 2.0f * 1.0f / (float)this.OwnerList.ScaleFactor.Ratio;
                rectF.Inflate(padding + 1.0f, padding + 1.0f);
            }

            return pt.X >= rectF.Left && pt.Y >= rectF.Top && pt.X < rectF.Right && pt.Y < rectF.Bottom;
        }

        protected override void OnVisibleChanged()
        {
            InvalidateOurself();
        }

        public override void Render(Surface dst, Point offset)
        {
            lock (this)
            {
                using (RenderArgs ra = new RenderArgs(dst))
                {
                    ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    ra.Graphics.TranslateTransform(-offset.X, -offset.Y, MatrixOrder.Append);

                    PointF ptF = (PointF)this.Location;

                    ptF = Utility.TransformOnePoint(this.transform, ptF);

                    ptF.X *= (float)OwnerList.ScaleFactor.Ratio;
                    ptF.Y *= (float)OwnerList.ScaleFactor.Ratio;
                    
                    float angle1 = Utility.GetAngleOfTransform(this.transform);
                    while (angle1 > 180.0f)
                    {
                        angle1 -= 180.0f;
                    }

                    while (angle1 < 0.0f)
                    {
                        angle1 += 180.0f;
                    }

                    if (Math.Abs(angle1) < 0.01)
                    {
                        ptF = (PointF)Point.Truncate(ptF);
                    }

                    PointF[] pts = new PointF[4] { 
                                                     new PointF(-1, -1),
                                                     new PointF(+1, -1),
                                                     new PointF(+1, +1),
                                                     new PointF(-1, +1)
                                                 };
                      
                    Utility.RotateVectors(pts, this.transformAngle);
                    Utility.NormalizeVectors(pts);

                    using (Pen white = new Pen(Color.FromArgb(this.alpha, Color.White), -1.0f), 
                               black = new Pen(Color.FromArgb(this.alpha, Color.Black), -1.0f))
                    {
                        PixelOffsetMode oldPOM = ra.Graphics.PixelOffsetMode;
                        ra.Graphics.PixelOffsetMode = PixelOffsetMode.None;

                        ra.Graphics.DrawPolygon(white, new PointF[] {
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], this.size)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], this.size)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], this.size)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], this.size))
                                                                    });

                        ra.Graphics.DrawPolygon(black, new PointF[] {
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], this.size - 1)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], this.size - 1)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], this.size - 1)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], this.size - 1))
                                                                    });

                        ra.Graphics.DrawPolygon(white, new PointF[] {
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], this.size - 2)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], this.size - 2)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], this.size - 2)),
                                                                        Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], this.size - 2))
                                                                    });

                        if (this.drawCompass)
                        {
                            black.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                            black.EndCap = LineCap.ArrowAnchor;
                            black.StartCap = LineCap.ArrowAnchor;
                            white.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                            white.EndCap = LineCap.ArrowAnchor;
                            white.StartCap = LineCap.ArrowAnchor;

                            PointF ul = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[0], this.size - 1));
                            PointF ur = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[1], this.size - 1));
                            PointF lr = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[2], this.size - 1));
                            PointF ll = Utility.AddVectors(ptF, Utility.MultiplyVector(pts[3], this.size - 1));

                            PointF top = Utility.MultiplyVector(Utility.AddVectors(ul, ur), 0.5f);
                            PointF left = Utility.MultiplyVector(Utility.AddVectors(ul, ll), 0.5f);
                            PointF right = Utility.MultiplyVector(Utility.AddVectors(ur, lr), 0.5f);
                            PointF bottom = Utility.MultiplyVector(Utility.AddVectors(ll, lr), 0.5f);

                            using (SolidBrush whiteBrush = new SolidBrush(white.Color))
                            {
                                PointF[] poly = new PointF[] { ul, ur, lr, ll };
                                ra.Graphics.FillPolygon(whiteBrush, poly, FillMode.Winding);
                            }

                            ra.Graphics.DrawLine(black, top, bottom);
                            ra.Graphics.DrawLine(black, left, right);
                        }

                        ra.Graphics.PixelOffsetMode = oldPOM;
                    }
                }
            }
        }
            
        public MoveNubRenderer(SurfaceBoxRendererList ownerList)
            : base(ownerList)
        {
            this.location = new Point(0, 0);
            this.size = 5;
            this.drawCompass = false;
            this.transform = new Matrix();
            this.transform.Reset();
            this.alpha = 255;
        }
    }
}
