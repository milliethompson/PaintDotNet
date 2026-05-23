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
    /// Renders a Surface to the screen.
    /// </summary>
    public class SurfaceBox : 
        System.Windows.Forms.Control
    {
        private InterpolationMode zoomOutInterpMode = InterpolationMode.Bilinear;
        private InterpolationMode zoomInInterpMode = InterpolationMode.NearestNeighbor;
        private ScaleFactor scaleFactor;
		private Surface scaledBuffer = null;

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
				if (scaledBuffer != null) 
				{
					scaledBuffer.Dispose();
					scaledBuffer = null;
				}
                if (surface != null)
                {
					//Maintain the scalefactor
					this.Size = this.scaleFactor.ScaleSize(surface.Size);
					scaledBuffer = new Surface(surface.Size);
                }
                Invalidate();
            }
        }

		private bool drawGrid;
		public bool DrawGrid 
		{
			get 
			{
				return drawGrid;
			}
			set 
			{
				drawGrid = value;
			}
		}

		public void FitToSize(Size fit)
		{
			this.scaleFactor = new ScaleFactor(Math.Min((float)fit.Width / surface.Width, (float)fit.Height / surface.Height));
			this.Size = this.scaleFactor.ScaleSize(surface.Size);
		}

        private ScaleFactor ComputeScaleFactor()
        {
			if (surface == null)
			{
				return ScaleFactor.OneToOne;
			}
			else
			{
				return new ScaleFactor(Math.Max(0.01f, Math.Max((float)this.Width / surface.Width, (float)this.Height / surface.Height)));
			}
			/*
			if (this.Width == surface.Width)
            {
                return ScaleFactor.OneToOne;
            }
            else if (this.Width > surface.Width)
            {   // zoom in
                return new ScaleFactor(Utility.Log2RoundUp(this.Width / surface.Width), 1);
            }
            else // if (this.Width < surface.Width)
            {   // zoom out                    
                return new ScaleFactor(1, Utility.Log2RoundUp(surface.Width / this.Width));
            }*/
        }

		/* This code fixes the size of the surfaceBox as necessary to 
		 * maintain the aspect ratio of the surface. Keeping the mouse
		 * within 32767 is delegated to the new overflow-checking code
		 * in Tool.cs.
		 */
        protected override void OnResize(EventArgs e)
        {
			base.OnResize (e);
			Size mySize = this.Size;
			if (this.Width == 32767 && surface != null)
			{ //Windows forms clamped this control's width, so we have to fix the height.
				mySize.Height = 32768 * surface.Height / surface.Width;
			}
			else if (mySize.Width == 0)
			{
				mySize.Width = 1;
			} 
			
			if (this.Width == 32767 && surface != null)
			{ //Windows forms clamped this control's height, so we have to fix the width.
				mySize.Width = 32768 * surface.Width / surface.Height;
			}
			else if (mySize.Height == 0) 
			{
				mySize.Height = 1;
			}

			if (mySize != this.Size) 
			{
				this.Size = mySize;
			}
            this.scaleFactor = ComputeScaleFactor();
        }


        public ScaleFactor ScaleFactor
        {
            get
            {
                return scaleFactor;
            }
        }

        public SurfaceBox()
        {
            InitializeComponent();
			this.scaleFactor = ScaleFactor.OneToOne;
			this.drawGrid = false;
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

			Rectangle surfaceRect;
			Rectangle clientRect = new Rectangle(0, 0, Width, Height);

			if (surface != null)
			{
				surfaceRect = this.ScaleFactor.ScaleRectangle(surface.Bounds);
			}
			else
			{
				surfaceRect = Rectangle.Empty;
			}

			if (surfaceRect != clientRect)
			{
				using (HatchBrush hb = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.White))
				{
					using (PdnRegion missing = new PdnRegion(clientRect))
                    {
						missing.Exclude(surfaceRect);
						e.Graphics.FillRegion(hb, missing);
					}
				}
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
            return ScaleFactor.UnscalePoint(clientPt);
        }

        public Point ClientToSurface(Point clientPt)
        {
            return ScaleFactor.UnscalePoint(clientPt);
        }

        public SizeF ClientToSurface(SizeF clientSize)
        {
            return ScaleFactor.UnscaleSize(clientSize);
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
        }

        public Point SurfaceToClient(Point surfacePt)
        {
            return ScaleFactor.ScalePoint(surfacePt);
        }

        public SizeF SurfaceToClient(SizeF surfaceSize)
        {
            return ScaleFactor.ScaleSize(surfaceSize);
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

        private static Rectangle AlignRectangle(Rectangle rect, int alignFactor)
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

		public const float DrawGridMinimumZoom = 8.0f;
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
				g.InterpolationMode = zoomOutInterpMode;
#if DOMETHOD1
				ScaleFactor scale8 = new ScaleFactor(8.0f);
                Rectangle aligned = scale8.ScaleRectangle(Utility.RoundRectangle(scale8.UnscaleRectangle((RectangleF)roi)));
				RectangleF surfaceRect = RectangleF.Intersect((RectangleF)surface.Bounds, ClientToSurface((RectangleF)aligned));
				Rectangle surfaceRectRounded = Utility.RoundRectangle(surfaceRect);
				Rectangle clientRect = Rectangle.Truncate(SurfaceToClient(surfaceRect));
                

				if (!clientRect.IsEmpty) 
				{
					using (Bitmap src = surface.CreateAliasedBitmap(surfaceRectRounded)) 
					{
						RectangleF subarea =
							new RectangleF(
							new PointF(
							surfaceRect.Left - surfaceRectRounded.Left,
							surfaceRect.Top  - surfaceRectRounded.Top),
							surfaceRect.Size);
						g.DrawImage(src, (RectangleF)clientRect, subarea, GraphicsUnit.Pixel);
					}
					/*	Nearest Neightbor/Bilinear attempt */
				}
#else
				RectangleF surfaceRect = RectangleF.Intersect((RectangleF)surface.Bounds, ClientToSurface((RectangleF)roi));
				Rectangle surfaceRectRounded = Utility.RoundRectangle(surfaceRect);
				Rectangle clientRect = Utility.RoundRectangle(SurfaceToClient(surfaceRect));
				if (clientRect.Width > 0 && clientRect.Height > 0) 
				{
					unsafe
					{
						int stride = surface.Stride;
						using (Surface scaled = scaledBuffer.CreateWindow(clientRect)) 
						{
							ColorBgra col = new ColorBgra();
							int scale = scaleFactor.UnscaleScalar(1023);

							for (int y = clientRect.Top; y < clientRect.Bottom; y++) 
							{
								int fy = y * scale;
								int sy = fy >> 10;
								int v  = fy & 0x3ff;
								ColorBgra *srcrow = surface.GetRowAddress(sy);
								ColorBgra *dstrow = scaled.GetRowAddress(y - clientRect.Top);
								

								for (int x = clientRect.Left; x < clientRect.Right; x++) 
								{
									int fx = x * scale;
									int sx = fx >> 10;
									int u = fx & 0x3ff;
									ColorBgra *src = srcrow + sx;
/** /
									for (int c = 0; c < 4; c++) 
									{
#if DEBUG
										try 
										{
#endif
											col[c] = 
												(byte)
												(
												(
												src[0]			[c] * (1024 - u) * (1024 - v) +
												src[1]			[c] * (       u) * (1024 - v) +
												src[stride]		[c] * (1024 - u) * (       v) +
												src[1 + stride]	[c] * (       u) * (       v)
												) >> 20
												);
#if DEBUG
										}
										catch (NullReferenceException ) 
										{
											;
										}
#endif
									}
									dstrow[x - clientRect.Left] = col;/**/
									dstrow[x - clientRect.Left] = *src;/**/
								}
							}

							using (Bitmap alias = scaled.CreateAliasedBitmap(scaled.Bounds, false))
							{
								g.DrawImage(alias, clientRect, scaled.Bounds, GraphicsUnit.Pixel);
							}
						}
					}
				}
#endif
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
					if (drawGrid && this.Width >= surface.Width * DrawGridMinimumZoom) 
					{
						Pen gridPen = new Pen(Color.Gray);
						gridPen.Width = 1.0f;
						gridPen.DashPattern = new float[]{ 1.0f, 1.0f };

						for (int x = surfaceRect.Left; x <= surfaceRect.Right; x++) 
						{
							PointF start = new PointF(x, surfaceRect.Top);
							PointF end = new PointF(x, surfaceRect.Bottom);
							g.DrawLine(gridPen, SurfaceToClient(start), SurfaceToClient(end));
						}
						for (int y = surfaceRect.Top; y <= surfaceRect.Bottom; y++) 
						{
							PointF start = new PointF(surfaceRect.Left, y);
							PointF end = new PointF(surfaceRect.Right, y);
							g.DrawLine(gridPen, SurfaceToClient(start), SurfaceToClient(end));
						}
					}
				}
			}
		}

		private sealed class NativeMethods
        {
            internal sealed class WmConstants
            {
                public static int WM_SETFOCUS = 7;

                private WmConstants()
                {
                }
            }

            private NativeMethods()
            {
            }
        }

        protected override void WndProc(ref Message m)
        {
            IntPtr preR = m.Result;

            // Ignore focus
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

