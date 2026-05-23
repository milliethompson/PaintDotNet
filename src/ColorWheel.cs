using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Portions adapted from: 
    /// "A Primer on Building a Color Picker User Control with GDI+ in Visual Basic .NET or C#"
    /// http://www.msdnaa.net/Resources/display.aspx?ResID=2460
    /// </summary>
    public class ColorWheel : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private Bitmap renderBitmap = null; // what we draw to the screen
        private Bitmap extractBitmap = null; // what we extract colors from given (x,y) mouse coords. This surface is not anti-aliased, so it won't have the background color mixed in with valid selection areas
        private PdnRegion wheelRegion = null;
        private bool tracking = false;
        private Point lastMouseXY;

        // this number controls what you might call the tesselation of the color wheel. higher #'s = slower, lower #'s = looks worse
        private const int colorCount = 48;

        private System.Windows.Forms.PictureBox wheelPictureBox; 

        private HsvColor hsvColor;
        public HsvColor HsvColor
        {
            get
            {
                return hsvColor;
            }

            set
            {
                if (hsvColor != value)
                {
                    HsvColor oldColor = hsvColor;
                    hsvColor = value;
                    this.OnColorChanged();
                    Invalidate(true);
                }
            }
        }
                
        public ColorWheel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            wheelRegion = new PdnRegion();
            hsvColor = new HsvColor(0, 0, 0);
        }

        private static PointF SphericalToCartesian(float r, float theta)
        {
            float x;
            float y;

            x = r * (float)Math.Cos(theta);
            y = r * (float)Math.Sin(theta);

            return new PointF(x,y);
        }

        private static PointF[] GetCirclePoints(float r, PointF center)
        {
            PointF[] points = new PointF[colorCount];
            
            for(int i = 0; i < colorCount; i++)
            {
                float theta = ((float)i / (float)colorCount) * 2 * (float)Math.PI;
                points[i] = SphericalToCartesian(r, theta);
                points[i].X += center.X;
                points[i].Y += center.Y;
            }
            
            return points;
        }

        private Color[] GetColors()
        {
            Color[] colors = new Color[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                int hue = (i * 360) / colorCount;
                colors[i] = new HsvColor(hue, 100, 100).ToColor();
            }

            return colors;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            if (renderBitmap == null)
            {
                InitRenderSurface();
                this.wheelPictureBox.Size = renderBitmap.Size;
                this.wheelPictureBox.Image = renderBitmap;
            }

            //e.Graphics.DrawImage(renderBitmap, 0, 0, extractBitmap.Width, extractBitmap.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (renderBitmap == null)
            {
                InitRenderSurface();
                this.wheelPictureBox.Size = renderBitmap.Size;
                this.wheelPictureBox.Image = renderBitmap;
            }

            using (PdnRegion eraseRegion = new PdnRegion(new Rectangle(new Point(0, 0), Size)))
            {
                if (extractBitmap != null)
                {
                    eraseRegion.Exclude(new Rectangle(new Point(0, 0), extractBitmap.Size));
                }

                using (SolidBrush bb = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillRegion(bb, eraseRegion);
                }
            }

            //e.Graphics.DrawImage(renderBitmap, 0, 0, extractBitmap.Width, extractBitmap.Height);
        }

        private void wheelPictureBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            float radius = ComputeRadius(Size);
            float theta = ((float)HsvColor.Hue / 360.0f) * 2.0f * (float)Math.PI;
            float alpha = ((float)HsvColor.Saturation / 100.0f);
            float x = (alpha * (radius - 1) * (float)Math.Cos(theta)) + radius;
            float y = (alpha * (radius - 1) * (float)Math.Sin(theta)) + radius;

			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawRectangle(Pens.Black, x - 1, y - 1, 3, 3);
			e.Graphics.DrawRectangle(Pens.White, x, y, 1, 1);
        }

        private void InitRenderSurface()
        {
            if (renderBitmap != null)
            {
                renderBitmap.Dispose();
            }

            if (extractBitmap != null)
            {
                extractBitmap.Dispose();
            }

            int wheelDiameter = (int)ComputeDiameter(Size);

            renderBitmap = new Bitmap(Math.Max(1, wheelDiameter), Math.Max(1, wheelDiameter), PixelFormat.Format24bppRgb);
            extractBitmap = new Bitmap(renderBitmap.Width, renderBitmap.Height);

            using (Graphics g1 = Graphics.FromImage(renderBitmap))
            {
                using (Surface drawMe = new Surface(renderBitmap.Width * 2, renderBitmap.Height * 2))
                {
                    using (RenderArgs ra = new RenderArgs(drawMe))
                    {
                        ra.Graphics.Clear(this.BackColor);
                        DrawWheel (ra.Graphics, drawMe.Width, drawMe.Height, null);

                        g1.Clear(this.BackColor);
                        g1.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g1.DrawImage(ra.Bitmap, 0, 0, renderBitmap.Width, renderBitmap.Width);
                    }
                }
            }

            using (Graphics g2 = Graphics.FromImage(extractBitmap))
            {
                g2.Clear(this.BackColor);
                DrawWheel(g2, extractBitmap.Width, extractBitmap.Height, wheelRegion);
            }
        }

        private void DrawWheel(Graphics g, int width, int height, PdnRegion wheelRegion)
        {
            float radius = ComputeRadius(new Size(width, height));
            PointF[] points = GetCirclePoints(Math.Max(1.0f, (float)radius - 1), new PointF(radius, radius));
            
            using (PathGradientBrush pgb = new PathGradientBrush(points))
            {
                pgb.CenterColor = new HsvColor(0, 0, 100).ToColor();
                pgb.CenterPoint = new PointF(radius, radius);
                pgb.SurroundColors = GetColors();

                g.FillEllipse(pgb, 0, 0, radius * 2, radius * 2);

                if (wheelRegion != null)
                {
                    using (PdnGraphicsPath path = new PdnGraphicsPath())
                    {
                        path.AddEllipse(0, 0, radius * 2, radius * 2);
                        wheelRegion.MakeEmpty();
                        wheelRegion.Union(path);
                    }
                }
            }
        }

        private static float ComputeRadius(Size size)
        {
            return Math.Min((float)size.Width / 2, (float)size.Height / 2);
        }

        private static float ComputeDiameter(Size size)
        {
            return Math.Min((float)size.Width, (float)size.Height);       
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            if (renderBitmap != null && (ComputeRadius(Size) != ComputeRadius(extractBitmap.Size)))
            {
                renderBitmap.Dispose();
                renderBitmap = null;
            }

            Invalidate();
        }

        public event EventHandler ColorChanged;
        protected virtual void OnColorChanged()
        {
            if (ColorChanged != null)
            {
                ColorChanged(this, EventArgs.Empty);
            }
        }

        private void GrabColor(Point mouseXY)
        {
            // center our coordinate system so the middle is (0,0), and positive Y is facing up
            int cx = mouseXY.X - (Width / 2);
            int cy = mouseXY.Y - (Height / 2);

            double theta = Math.Atan2(cy, cx);

            if (theta < 0)
            {
                theta += 2 * Math.PI;
            }

            double alpha = Math.Sqrt((cx * cx) + (cy * cy));

            int h = (int)((theta / (Math.PI * 2)) * 360.0);
            int s = (int)Math.Min(100.0, (alpha / (double)(Width / 2)) * 100);
            int v = 100;

            hsvColor = new HsvColor(h, s, v);
            OnColorChanged();
            Invalidate(true);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            GrabColor(lastMouseXY);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if (e.Button == MouseButtons.Left)
            {
                tracking = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);
            tracking = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            lastMouseXY = new Point(e.X, e.Y);

            if (tracking)
            {
                GrabColor(new Point(e.X, e.Y));
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wheelPictureBox = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // wheelPictureBox
            // 
            this.wheelPictureBox.Location = new System.Drawing.Point(0, 0);
            this.wheelPictureBox.Name = "wheelPictureBox";
            this.wheelPictureBox.TabIndex = 0;
            this.wheelPictureBox.TabStop = false;
            this.wheelPictureBox.Click += new System.EventHandler(this.wheelPictureBox_Click);
            this.wheelPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.wheelPictureBox_Paint);
            this.wheelPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseUp);
            this.wheelPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseMove);
            this.wheelPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.wheelPictureBox_MouseDown);
            // 
            // ColorWheel
            // 
            this.Controls.Add(this.wheelPictureBox);
            this.Name = "ColorWheel";
            this.ResumeLayout(false);

        }
        #endregion

        private void wheelPictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void wheelPictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void wheelPictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void wheelPictureBox_Click(object sender, System.EventArgs e)
        {
            OnClick(e);
        }
    }
}
