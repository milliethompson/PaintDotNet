/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private ScaleFactor scaleFactor;
        private PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();

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
                    // Maintain the scalefactor
                    this.Size = this.scaleFactor.ScaleSize(surface.Size);
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

        [ThreadStatic]
        private static Pen gridPen = null;
        private static Pen GridPen
        {
            get
            {
                if (gridPen == null)
                {
                    gridPen = new Pen(Color.Gray);
                    gridPen.DashStyle = DashStyle.Dot;
                }

                return gridPen;
            }
        }
        
        public void FitToSize(Size fit)
		{
            ScaleFactor newSF = ScaleFactor.Min(fit.Width, surface.Width,
                                                fit.Height, surface.Height,
                                                ScaleFactor.MinValue);

            this.scaleFactor = newSF;
			this.Size = this.scaleFactor.ScaleSize(surface.Size);
		}

        protected override void OnResize(EventArgs e)
        {
            /* This code fixes the size of the surfaceBox as necessary to 
             * maintain the aspect ratio of the surface. Keeping the mouse
             * within 32767 is delegated to the new overflow-checking code
             * in Tool.cs.
             */
            base.OnResize (e);

			Size mySize = this.Size;
			if (this.Width == 32767 && surface != null)
			{ 
                //Windows forms clamped this control's width, so we have to fix the height.
				mySize.Height = 32768 * surface.Height / surface.Width;
			}
			else if (mySize.Width == 0)
			{
				mySize.Width = 1;
			} 
			
			if (this.Width == 32767 && surface != null)
			{ 
                //Windows forms clamped this control's height, so we have to fix the width.
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
            
            if (surface == null)
            {
                this.scaleFactor = ScaleFactor.OneToOne;
            }
            else
            {
                ScaleFactor newSF = ScaleFactor.Max(this.Width, surface.Width,
                                                    this.Height, surface.Height,
                                                    ScaleFactor.OneToOne);
                this.scaleFactor = newSF;
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.doubleBufferSurface != null)
                {
                    this.doubleBufferSurface.Dispose();
                    this.doubleBufferSurface = null;
                }
            }

            base.Dispose (disposing);
        }


        /// <summary>
        /// This event is raised after painting has been performed. This is required because
        /// the normal Paint event is raised *before* painting has been performed.
        /// </summary>
        public event PaintEventHandler2 Painted;
        protected void OnPainted(PaintEventArgs2 e)
        {
            if (Painted != null)
            {
                Painted(this, e);
            }
        }

        public event PaintEventHandler2 PrePaint;
        protected void OnPrePaint(PaintEventArgs2 e)
        {
            if (PrePaint != null)
            {
                PrePaint(this, e);
            }
        }

        private const int paintTileSize = 2048;
        private Surface doubleBufferSurface = null;
        private Surface GetDoubleBuffer(Size size)
        {
            if (doubleBufferSurface == null || 
                doubleBufferSurface.Width < size.Width || 
                doubleBufferSurface.Height < size.Height)
            {
                Size oldSize;

                if (doubleBufferSurface == null)
                {
                    oldSize = new Size(0, 0);
                }
                else
                {
                    oldSize = doubleBufferSurface.Size;
                    doubleBufferSurface.Dispose();
                }

                Size newSize = new Size(Math.Max(oldSize.Width, size.Width),
                                        Math.Max(oldSize.Height, size.Height));

                doubleBufferSurface = new Surface(newSize);
            }

            return doubleBufferSurface.CreateWindow(new Rectangle(new Point(0, 0), size));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.surface == null)
            {
                return;
            }

            Rectangle rect = e.ClipRectangle;
            for (int top = rect.Top; top < rect.Bottom; top += paintTileSize)
            {
                int bottom = Math.Min(top + paintTileSize, rect.Bottom);

                for (int left = rect.Left; left < rect.Right; left += paintTileSize)
                {
                    int right = Math.Min(left + paintTileSize, rect.Right);

                    Rectangle clipRect2 = Rectangle.FromLTRB(left, top, right, bottom);

                    if (e.Graphics.IsVisible(clipRect2))
                    {
                        PaintEventArgs2 e2 = new PaintEventArgs2(e.Graphics, clipRect2);
                        OnPaintImpl(e2);
                    }
                }
            }
        }

        private void OnPaintImpl(PaintEventArgs2 e)
        {
            Surface doubleBuffer = GetDoubleBuffer(e.ClipRectangle.Size);

            using (RenderArgs renderArgs = new RenderArgs(doubleBuffer))
            {
                renderArgs.Graphics.TranslateTransform(-e.ClipRectangle.X, -e.ClipRectangle.Y);
                PaintEventArgs2 e2 = new PaintEventArgs2(renderArgs.Graphics, e.ClipRectangle);
    
                OnPrePaint(e2);
                DrawArea(renderArgs, e2.ClipRectangle);
                OnPainted(e2);

                PdnGraphics.DrawBitmap(e.Graphics, e.ClipRectangle, renderArgs.Bitmap);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing so as to avoid flicker
            // tip: for debugging, uncomment the next line!
            //base.OnPaintBackground(pevent);
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

		public const float DrawGridMinimumZoom = 4.0f;

        /// <summary>
        /// Draws an area of the SurfaceBox.
        /// </summary>
        /// <param name="ra">The rendering surface object to draw to.</param>
        /// <param name="roi">The rectangle of interest to draw, in client coordinates.</param>
        /// <remarks>
        /// If drawing to ra.Surface or ra.Bitmap, copy the roi of the source surface to (0,0) of ra.Surface or ra.Bitmap
        /// If drawing to ra.Graphics, copy the roi of the surface to (roi.X,roi.Y) of ra.Graphics
        /// </remarks>
        private void DrawArea(RenderArgs ra, Rectangle roi)
        {
            if (surface == null)
            {
                return;
            }

            if (surface.Width < this.Width)
            {   
                // zoom in
                PixelOffsetMode oldPOM = ra.Graphics.PixelOffsetMode;
                ra.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                int alignFactor = ((this.Width + surface.Width - 1) / surface.Width);
                Rectangle clientRect2 = AlignRectangle(roi, alignFactor);
                Rectangle surfaceRect = Rectangle.Intersect(surface.Bounds, Utility.RoundRectangle(ClientToSurface((RectangleF)clientRect2)));
                Rectangle clientRect3 = SurfaceToClient(surfaceRect);

                if (!surfaceRect.IsEmpty)
                {
                    InterpolationMode oldIM = ra.Graphics.InterpolationMode;
                    ra.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                    using (Bitmap alias = surface.CreateAliasedBitmap(surfaceRect, false))
                    {
                        ra.Graphics.DrawImage(alias, clientRect3, new Rectangle(new Point(0, 0), surfaceRect.Size), GraphicsUnit.Pixel);
                    }

                    if (drawGrid && this.Width >= surface.Width * DrawGridMinimumZoom) 
                    {
                        PdnGraphics.DrawGrid(ra.Graphics, surfaceRect, new PointFPointFDelegate(SurfaceToClient));
                    }

                    ra.Graphics.InterpolationMode = oldIM;
                }

                ra.Graphics.PixelOffsetMode = oldPOM;
            }
            else
            {
                WaitCallback callback;

                if (surface.Width == this.Width)
                {   
                    callback = new WaitCallback(RenderOneToOne);
                }
                else // if (surface.Width > this.Width)
                {
                    callback = new WaitCallback(RenderZoomOutRotatedGridMultisampling);
                }

                Rectangle[] rects;
                
                if (roi.Height < 16)
                {
                    rects = new Rectangle[1] { roi };
                }
                else
                {
                    rects = new Rectangle[SystemLayer.Processor.LogicalCpuCount];
                    Utility.SplitRectangle(roi, rects);
                }

                foreach (Rectangle rect in rects)
                {
                    if (rect.Width == 0 || rect.Height == 0)
                    {
                        continue;
                    }

                    RenderContext rc = new RenderContext(ra.Surface, rect, new Point(rect.Left - roi.Left, rect.Top - roi.Top));
                    threadPool.QueueUserWorkItem(callback, rc);
                }

                threadPool.Drain();
            }
        }

        private class RenderContext
        {
            public Surface dstSurface;
            public Rectangle roi;
            public Point offset;

            public RenderContext(Surface dstSurface, Rectangle roi, Point offset)
            {
                this.dstSurface = dstSurface;
                this.roi = roi;
                this.offset = offset;
            }
        }

        private void RenderOneToOne(object context)
        {
            RenderOneToOne((RenderContext)context);
        }

        private void RenderOneToOne(RenderContext rc)
        {
            rc.dstSurface.CopySurface(this.surface, rc.offset, rc.roi);
        }

        private void RenderZoomOutRotatedGridMultisampling(object context)
        {
            RenderZoomOutRotatedGridMultisampling((RenderContext)context);
        }

        private void RenderZoomOutRotatedGridMultisampling(RenderContext rc)
        {
            unsafe
            {
                using (Surface scaled = rc.dstSurface.CreateWindow(new Rectangle(rc.offset, rc.roi.Size)))
                {
                    long fDstLeftLong = ((long)rc.roi.Left * 4096 * (long)surface.Width) / (long)this.Width;
                    long fDstTopLong = ((long)rc.roi.Top * 4096 * (long)surface.Height) / (long)this.Height;
                    long fDstRightLong = ((long)rc.roi.Right * 4096 * (long)surface.Width) / (long)this.Width;
                    long fDstBottomLong = ((long)rc.roi.Bottom * 4096 * (long)surface.Height) / (long)this.Height;
                    int fDstLeft = (int)fDstLeftLong;
                    int fDstTop = (int)fDstTopLong;
                    int fDstRight = (int)fDstRightLong;
                    int fDstBottom = (int)fDstBottomLong;
                    int dx = (fDstRight - fDstLeft) / rc.roi.Width;
                    int dy = (fDstBottom - fDstTop) / rc.roi.Height;

                    for (int dstRow = 0, fDstY = fDstTop; 
                         dstRow < rc.roi.Height && fDstY < fDstBottom; 
                         ++dstRow, fDstY += dy)
                    {
                        int srcY1 = fDstY >> 12;                            // y
                        int srcY2 = (fDstY + (dy >> 2)) >> 12;              // y + 0.25
                        int srcY3 = (fDstY + (dy >> 1)) >> 12;              // y + 0.50
                        int srcY4 = (fDstY + (dy >> 1) + (dy >> 2)) >> 12;  // y + 0.75

                        Debug.Assert(this.surface.IsRowVisible(srcY1));
                        Debug.Assert(this.surface.IsRowVisible(srcY2));
                        Debug.Assert(this.surface.IsRowVisible(srcY3));
                        Debug.Assert(this.surface.IsRowVisible(srcY4));
                        Debug.Assert(scaled.IsRowVisible(dstRow));

                        ColorBgra *src1 = this.surface.GetRowAddressUnchecked(srcY1);
                        ColorBgra *src2 = this.surface.GetRowAddressUnchecked(srcY2);
                        ColorBgra *src3 = this.surface.GetRowAddressUnchecked(srcY3);
                        ColorBgra *src4 = this.surface.GetRowAddressUnchecked(srcY4);
                        ColorBgra *dst = scaled.GetRowAddressUnchecked(dstRow);

                        for (int dstCol = 0, fDstX = fDstLeft;
                             dstCol < rc.roi.Width && fDstX < fDstRight;
                             ++dstCol, fDstX += dx)
                        {
                            int srcX1 = (fDstX + (dx >> 2)) >> 12;             // x + 0.25
                            int srcX2 = (fDstX + (dx >> 1) + (dx >> 2)) >> 12; // x + 0.75
                            int srcX3 = fDstX >> 12;                           // x
                            int srcX4 = (fDstX + (dx >> 1)) >> 12;             // x + 0.50

                            Debug.Assert(this.surface.IsColumnVisible(srcX1));
                            Debug.Assert(this.surface.IsColumnVisible(srcX2));
                            Debug.Assert(this.surface.IsColumnVisible(srcX3));
                            Debug.Assert(this.surface.IsColumnVisible(srcX4));
                            Debug.Assert(scaled.IsColumnVisible(dstCol));

                            ColorBgra *p1 = src1 + srcX1;
                            ColorBgra *p2 = src2 + srcX2;
                            ColorBgra *p3 = src3 + srcX3;
                            ColorBgra *p4 = src4 + srcX4;

                            int r = (2 + p1->R + p2->R + p3->R + p4->R) >> 2;
                            int g = (2 + p1->G + p2->G + p3->G + p4->G) >> 2;
                            int b = (2 + p1->B + p2->B + p3->B + p4->B) >> 2;
                            int a = (2 + p1->A + p2->A + p3->A + p4->A) >> 2;

                            dst->Bgra = (uint)b + ((uint)g << 8) + ((uint)r << 16) + ((uint)a << 24);

                            ++dst;
                        }
                    }
                }
            }
        }        

        private void RenderZoomOutNearestNeighbor(object rc)
        {
            RenderZoomOutNearestNeighbor((RenderContext)rc);
        }

        private void RenderZoomOutNearestNeighbor(RenderContext rc)
        {
            unsafe
            {
                using (Surface scaled = rc.dstSurface.CreateWindow(new Rectangle(rc.offset, rc.roi.Size)))
                {
                    long fDstLeftLong = ((long)rc.roi.Left * 4096 * (long)surface.Width) / (long)this.Width;
                    long fDstTopLong = ((long)rc.roi.Top * 4096 * (long)surface.Height) / (long)this.Height;
                    long fDstRightLong = ((long)rc.roi.Right * 4096 * (long)surface.Width) / (long)this.Width;
                    long fDstBottomLong = ((long)rc.roi.Bottom * 4096 * (long)surface.Height) / (long)this.Height;
                    int fDstLeft = (int)fDstLeftLong;
                    int fDstTop = (int)fDstTopLong;
                    int fDstRight = (int)fDstRightLong;
                    int fDstBottom = (int)fDstBottomLong;
                    int dx = (fDstRight - fDstLeft) / rc.roi.Width;
                    int dy = (fDstBottom - fDstTop) / rc.roi.Height;

                    for (int dstRow = 0, fDstY = fDstTop; 
                        dstRow < rc.roi.Height && fDstY < fDstBottom; 
                        ++dstRow, fDstY += dy)
                    {
                        int srcY = fDstY >> 12;

                        Debug.Assert(this.surface.IsRowVisible(srcY));
                        Debug.Assert(scaled.IsRowVisible(dstRow));

                        ColorBgra *src = this.surface.GetRowAddress(srcY);
                        ColorBgra *dst = scaled.GetRowAddress(dstRow);

                        for (int dstCol = 0, fDstX = fDstLeft;
                            dstCol < rc.roi.Width && fDstX < fDstRight;
                            ++dstCol, fDstX += dx)
                        {
                            int srcX = fDstX >> 12;

                            Debug.Assert(this.surface.IsColumnVisible(srcX));
                            Debug.Assert(scaled.IsColumnVisible(dstCol));

                            *dst = *(src + srcX);
                            ++dst;
                        }
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            IntPtr preR = m.Result;

            // Ignore focus
            if (m.Msg == 7 /* WM_SETFOCUS */)
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

