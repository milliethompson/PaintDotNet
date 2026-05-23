//#define SBDEBUG

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Renders a Surface to the screen. Uses tile based rendering so that the size of 
    /// anything drawn to the screen is no larger than tileSize.
    /// </summary>
    public class SurfaceBox : 
        System.Windows.Forms.Control
    {
        private InterpolationMode zoomOutInterpMode = InterpolationMode.Bilinear;
        private InterpolationMode zoomInInterpMode = InterpolationMode.NearestNeighbor;

        private Surface surface;
        public Surface Surface
        {
            get
            {
                return surface;
            }

            set
            {
                surface = value;

                if (surface != null)
                {
                    Size = surface.Size;
                }

                Invalidate();
            }
        }

        public ScaleFactor ScaleFactor
        {
            get
            {
                if (surface == null)
                {
                    return new ScaleFactor(1, 1);
                }
                else if (this.Width == surface.Width)
                {
                    return new ScaleFactor(1, 1);
                }
                else if (this.Width > surface.Width)
                {   // zoom in
                        return new ScaleFactor(Utility.Log2RoundUp(this.Width / surface.Width), 1);
                }
                else // if (this.Width < surface.Width)
                {   // zoom out                    
                    return new ScaleFactor(1, Utility.Log2RoundUp(surface.Width / this.Width));
                }
            }
        }

        public SurfaceBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// This event is raised after painting has been performed. This is required because
        /// the normal Paint event is raised *before* painting has been performed.
        /// </summary>
        public event PaintEventHandler Painted;
        protected void OnPainted(PaintEventArgs e)
        {
            if (Painted != null)
            {
                Painted(this, e);
            }
        }

        public event PaintEventHandler PrePaint;
        protected void OnPrePaint(PaintEventArgs e)
        {
            if (PrePaint != null)
            {
                PrePaint(this, e);
            }
        }

// SDEBUG is #defined at the beginning of the file
#if SBDEBUG
		// Useful for debugging
		private int updates = 0;
#endif

		protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);

            if (surface == null)
            {
                return;
            }

#if SBDEBUG
            // Useful for debugging
            ++updates;
            if ((updates % 2) == 0)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), e.ClipRectangle);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.ClipRectangle);
            }
#endif
            OnPrePaint(e);
            DrawArea(e.Graphics, e.ClipRectangle);           
            OnPainted(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing so as to avoid flicker
        }

        /// <summary>
        /// Converts from control client coordinates to surface coordinates
        /// This is useful when this.Bounds != surface.Bounds (i.e. some sort of zooming is in effect)
        /// </summary>
        /// <param name="clientPt"></param>
        /// <returns></returns>
        public PointF ClientToSurface(PointF clientPt)
        {
            if (clientPt.X != 0 && clientPt.Y != 0)
            {
                int x = 5; ++x;
            }

            return ScaleFactor.UnscalePoint(clientPt);

            /*
            return new PointF(((float)clientPt.X * (float)surface.Width) / (float)this.Width, 
				((float)clientPt.Y * (float)surface.Height) / (float)this.Height);
                */
        }

		public Point ClientToSurface(Point clientPt)
		{
            return ScaleFactor.UnscalePoint(clientPt);

            /*
			return new Point((clientPt.X * surface.Width) / this.Width,
				(clientPt.Y * surface.Height) / this.Height);
                */
		}

		public SizeF ClientToSurface(SizeF clientSize)
        {
            return ScaleFactor.UnscaleSize(clientSize);

            /*
            return new SizeF(((float)clientSize.Width * (float)surface.Width) / (float)this.Width, 
				((float)clientSize.Height * (float)surface.Height) / (float)this.Height);
                */
        }

		public Size ClientToSurface(Size clientSize)
		{
            return Size.Round(ClientToSurface((SizeF)clientSize));
		}

        public RectangleF ClientToSurface(RectangleF clientRect)
        {
			return new RectangleF(ClientToSurface(clientRect.Location), ClientToSurface(clientRect.Size));
        }

		public Rectangle ClientToSurface(Rectangle clientRect)
		{
			return new Rectangle(ClientToSurface(clientRect.Location), ClientToSurface(clientRect.Size));
		}

        public PointF SurfaceToClient(PointF surfacePt)
        {
            return ScaleFactor.ScalePoint(surfacePt);

            /*
            return new PointF(((float)surfacePt.X * (float)this.Width) / (float)surface.Width, 
				((float)surfacePt.Y * (float)this.Height) / (float)surface.Height);
                */
        }

		public Point SurfaceToClient(Point surfacePt)
		{
            return ScaleFactor.ScalePoint(surfacePt);
            /*
			return new Point((surfacePt.X * this.Width) / surface.Width,
				(surfacePt.Y * this.Height) / surface.Height);
                */
		}

        public SizeF SurfaceToClient(SizeF surfaceSize)
        {
            return ScaleFactor.ScaleSize(surfaceSize);
            /*
            return new SizeF(((float)surfaceSize.Width * (float)this.Width) / (float)surface.Width, 
				((float)surfaceSize.Height * (float)this.Height) / (float)surface.Height);
                */
        }

		public Size SurfaceToClient(Size surfaceSize)
		{
            return Size.Round(SurfaceToClient((SizeF)surfaceSize));
		}

        public RectangleF SurfaceToClient(RectangleF surfaceRect)
        {
			return new RectangleF(SurfaceToClient(surfaceRect.Location), SurfaceToClient(surfaceRect.Size));
        }        

		public Rectangle SurfaceToClient(Rectangle surfaceRect)
		{
			return new Rectangle(SurfaceToClient(surfaceRect.Location), SurfaceToClient(surfaceRect.Size));
		}

        public static Rectangle AlignRectangle(Rectangle rect, int alignFactor)
        {
            if (alignFactor == 0)
            {
                throw new ArgumentOutOfRangeException("alignFactor", "Must not equal zero");
            }

            int left = (rect.Left / alignFactor) * alignFactor;
            int top = (rect.Top / alignFactor) * alignFactor;
            int right = ((rect.Right + alignFactor - 1) / alignFactor) * alignFactor;
            int bottom = ((rect.Bottom + alignFactor - 1) / alignFactor) * alignFactor;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Draws an area of the SurfaceBox.
        /// </summary>
        /// <param name="g">The Graphics object to draw to.</param>
        /// <param name="roi">The rectangle of interest to draw, in client coordinates.</param>
        private void DrawArea(Graphics g, Rectangle roi)
        {
            if (surface == null)
            {
                return;
            }

            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            if (surface.Width == this.Width)
            {   // 100% zoom = no funny business
                Rectangle rect = Rectangle.Intersect(roi, surface.Bounds);

				if (!rect.IsEmpty)
				{
					using (Bitmap b = surface.CreateAliasedBitmap(rect, false))
					{
						g.DrawImage(b, rect, new Rectangle(new Point(0, 0), rect.Size), GraphicsUnit.Pixel);
					}
				}
            }
            else if (surface.Width > this.Width)
            {   // zoom out
                Rectangle surfaceRect = Utility.RoundRectangle(ClientToSurface((RectangleF)roi));

                // The alignment factor was found experimentally:
                // We take the ratio of zoom-out (i.e. "1/8 zoom" (or 12.5%) translates to an initial
                // alignFactor of 8). Then we square this number.
                int alignFactor = ScaleFactor.Denominator; //((surface.Width + this.Width - 1) / this.Width);
                alignFactor *= alignFactor;

                Rectangle surfaceRect2 = Rectangle.Intersect(surface.Bounds, AlignRectangle(surfaceRect, alignFactor));
                Rectangle clientRect = Utility.RoundRectangle(SurfaceToClient((RectangleF)surfaceRect2));

				if (!surfaceRect2.IsEmpty)
				{
					g.InterpolationMode = zoomOutInterpMode;

					using (Bitmap b = surface.CreateAliasedBitmap(surfaceRect2, false))
					{
						g.DrawImage(b, clientRect, new Rectangle(new Point(0, 0), surfaceRect2.Size), GraphicsUnit.Pixel);
					}
				}
            }
            else
            {   // zoom in
                int alignFactor = ((this.Width + surface.Width - 1) / surface.Width);
                Rectangle clientRect2 = AlignRectangle(roi, alignFactor);
                Rectangle surfaceRect = Rectangle.Intersect(surface.Bounds, Utility.RoundRectangle(ClientToSurface((RectangleF)clientRect2)));
                Rectangle clientRect3 = SurfaceToClient(surfaceRect);

				if (!surfaceRect.IsEmpty)
				{
					g.InterpolationMode = zoomInInterpMode;

					using (Bitmap b = surface.CreateAliasedBitmap(surfaceRect, false))
					{
						g.DrawImage(b, clientRect3, new Rectangle(new Point(0, 0), surfaceRect.Size), GraphicsUnit.Pixel);
					}
				}
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WmConstants.WM_SETFOCUS)
            {
                return;
            }

            base.WndProc (ref m);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion
    }
}

