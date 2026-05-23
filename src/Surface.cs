using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintDotNet
{
    /// <summary>
    /// This is our own Surface type. We allocate our own blocks of memory for this,
    /// and provide ways to create a GDI+ Bitmap object that aliases our surface.
    /// That way we can do everything fast and in memory and have complete control,
    /// while still being able to use GDI+ for drawing and rendering.
    /// </summary>
    [Serializable]
    public unsafe class Surface
        : IDisposable
    {
        private MemoryBlock scan0;
        private int width;
        private int height;
        private int stride;

        public MemoryBlock Scan0
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return scan0;
            }
        }

        public int Width
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return width;
            }
        }

        public int Height
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return height;
            }
        }

        public int Stride
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return stride;
            }
        }

        public Size Size
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return new Size(width, height);
            }
        }

        public PixelFormat PixelFormat
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return PixelFormat.Format32bppArgb;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                return new Rectangle(0, 0, width, height);
            }
        }

        public Surface(Size size)
            : this(size.Width, size.Height)
        {

        }

        public Surface(int width, int height)
        {
            int stride;
            int bytes;

            try
            {
                stride = checked(width * sizeof(ColorBgra));
                bytes = checked((height + 1) * stride);
            }

            catch (OverflowException ex)
            {
                throw new OutOfMemoryException("Not enough memory", ex);
            }

            MemoryBlock scan0 = new MemoryBlock(bytes);
            Create(width, height, stride, scan0);
        }

        public Surface(int width, int height, int stride)
        {
            int bytes;

            try
            {
                bytes = (height + 1) * stride;
            }

            catch (OverflowException ex)
            {
                throw new OutOfMemoryException("Not enough memory", ex);
            }

            MemoryBlock scan0 = new MemoryBlock(bytes);
            Create(width, height, stride, scan0);
        }

        private Surface(int width, int height, int stride, MemoryBlock scan0)
        {
            Create(width, height, stride, scan0);
        }

        private void Create(int width, int height, int stride, MemoryBlock scan0)
        {
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.scan0 = scan0;
        }

        ~Surface()
        {
            //Debug.WriteLine("Surface finalizing", "memory");            
            Dispose(false);
        }

        public Surface CreateWindow(Rectangle bounds)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle original = this.Bounds;
            Rectangle sub = new Rectangle(bounds.Location, bounds.Size);
            Rectangle clipped = Rectangle.Intersect(original, sub);

            if (clipped != sub)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new Surface(bounds.Width, bounds.Height, stride, 
                new MemoryBlock(scan0, 
                        ((stride * bounds.Y) + (sizeof(ColorBgra) * bounds.X)), 
                        (((bounds.Height - 1) * stride) + bounds.Width * sizeof(ColorBgra))));
        }

        public int GetRowByteOffset(int y)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return y * stride;
        }

        public ColorBgra *GetRowAddress(int y)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return (ColorBgra *)(((byte *)scan0.Pointer.ToPointer()) + GetRowByteOffset(y));
        }

        public int GetColumnByteOffset(int x)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return x * sizeof(ColorBgra);
        }

        public int GetPointByteOffset(int x, int y)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return GetRowByteOffset(y) + GetColumnByteOffset(x);
        }

        public ColorBgra *GetPointAddress(int x, int y)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return GetRowAddress(y) + x;
        }

        public ColorBgra *GetPointAddress(Point pt)
        {
            return GetPointAddress(pt.X, pt.Y);
        }

        public MemoryBlock GetRow(int y)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            return new MemoryBlock(scan0, GetRowByteOffset(y), y * stride);
        }

        public ColorBgra this[int x, int y]
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    throw new ArgumentOutOfRangeException("x,y", "out of bounds");
                }

                return *GetPointAddress(x, y);
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    throw new ArgumentOutOfRangeException("x,y", "out of bounds");
                }

                *GetPointAddress(x, y) = value;
            }
        }

        public Bitmap CreateAliasedBitmap(Rectangle bounds)
        {
            return CreateAliasedBitmap(bounds, true);
        }

        /// <summary>
        /// Creates a GDI+ Bitmap object that aliases the same memory that this Surface does.
        /// Then you can use GDI+ to draw on to this surface.
        /// Note: Since the Bitmap does not hold a reference to this Surface object, nor to
        /// the MemoryBlock that it contains, you must hold a reference to the Surface object
        /// for as long as you wish to use the aliased Bitmap. Otherwise the memory will be
        /// freed and the Bitmap will look corrupt or cause other errors.
        /// </summary>
        /// <param name="bounds">The rectangle of interest within this Surface that you wish to alias.</param>
        /// <param name="alpha">If true, the returned bitmap will use PixelFormat.Format32bppArgb. 
        /// If false, the returned bitmap will use PixelFormat.Format32bppRgb.</param>
        /// <returns>A GDI+ Bitmap that aliases the requested portion of the Surface.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><b>bounds</b> was not entirely within the boundaries of the Surface</exception>
        public Bitmap CreateAliasedBitmap(Rectangle bounds, bool alpha)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            if (bounds.IsEmpty)
            {
                throw new ArgumentOutOfRangeException();
            }

            Rectangle clipped = Rectangle.Intersect(this.Bounds, bounds);

            if (clipped != bounds)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new Bitmap(bounds.Width, bounds.Height, stride, alpha ? this.PixelFormat : PixelFormat.Format32bppRgb, 
                new IntPtr((void *)((byte *)scan0.Pointer.ToPointer() + GetPointByteOffset(bounds.X, bounds.Y))));
        }

        /// <summary>
        /// Helper function. Same as calling CreateAliasedBounds(Bounds).
        /// </summary>
        /// <returns>A GDI+ Bitmap that aliases the entire Surface.</returns>
        public Bitmap CreateAliasedBitmap()
        {
            return CreateAliasedBitmap(this.Bounds);
        }

        public PlacedImage CreateAliasedPlacedImage(Rectangle bounds)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Bitmap bitmap = CreateAliasedBitmap(bounds);
            return PlacedImage.CreateAliased(bitmap, bounds.Location);
        }

        public static Surface CopyFromBitmap (Bitmap bitmap)
        {
            Surface surface = new Surface(bitmap.Width, bitmap.Height);
            BitmapData bd = bitmap.LockBits(surface.Bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            for (int y = 0; y < bd.Height; ++y)
            {
                MemoryBlock.CopyMemory((void *)surface.GetRowAddress(y), 
					(byte *)bd.Scan0.ToPointer() + (y * bd.Stride), bd.Stride);
            }

            bitmap.UnlockBits(bd);
            return surface;
        }

        public void CopySurface(Surface source)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            int width = Math.Min(Width, source.Width);
            int height = Math.Min(Height, source.Height);

            for (int y = 0; y < height; ++y)
            {
                MemoryBlock.CopyMemory(GetRowAddress(y), source.GetRowAddress(y), width * sizeof(ColorBgra));
            }
        }

        // copies the given surface to a certain point on ourself, with clipping
        public void CopySurface(Surface source, Point dstOffset)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle dstRect = new Rectangle(dstOffset, source.Size);
            dstRect.Intersect(Bounds);

            if (dstRect.Width == 0 || dstRect.Height == 0)
            {
                return;
            }

            Point sourceOffset = new Point(dstRect.Location.X - dstOffset.X, dstRect.Location.Y - dstOffset.Y);
            Rectangle sourceRect = new Rectangle(sourceOffset, dstRect.Size);
            Surface sourceWindow = source.CreateWindow(sourceRect);
            Surface dstWindow = this.CreateWindow(dstRect);
            dstWindow.CopySurface(sourceWindow);

            dstWindow.Dispose();
            sourceWindow.Dispose();
        }

        // copies a portion of the given surface to a certain offset on ourself, with clipping
        public void CopySurface(Surface source, Point dstOffset, Rectangle sourceRoi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle dstRoi = new Rectangle(dstOffset, sourceRoi.Size);
            dstRoi.Intersect(Bounds);

            if (dstRoi.Height == 0 || dstRoi.Width == 0)
            {
                return;
            }

            sourceRoi.X += dstRoi.X - dstOffset.X;
            sourceRoi.Y += dstRoi.Y - dstOffset.Y;
            sourceRoi.Width = dstRoi.Width;
            sourceRoi.Height = dstRoi.Height;

            Surface src = source.CreateWindow(sourceRoi);
            CopySurface(src, dstOffset);
            src.Dispose();
        }

        public void CopySurface(Surface source, Region region)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            foreach(RectangleF rectF in region.GetRegionScans(Utility.IdentityMatrix))
            {
                Rectangle rect = Rectangle.Truncate(rectF);
                CopySurface(source, rect.Location, rect);
            }
        }

        /// <summary>
        /// Clones the Surface and its contents. Note that this method name is used instead
        /// of implementing ICloneable because the entire contents of the Surface are copied,
        /// even if it is a window onto another Surface. Because of that, copying a window
        /// will produce a Surface instance that is not aliased onto another Surface.
        /// </summary>
        /// <param name="source">The Surface to duplicate.</param>
        public Surface CopyContents()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Surface ret = new Surface(Size);
			ret.CopySurface(this);
			return ret;
		}

        /// <summary>
        /// Use this when shrinking an image to dimensions that are "much smaller".
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        public void SuperSamplingFitSurface(Surface source)
        {
            if (source.Width < Width || source.Height < Height)
            {
                this.BilinearFitSurface(source);
            }
            else for (int dstY = 0; dstY < Height; ++dstY)
            {
                int srcY = (dstY * source.Height) / Height;
                int srcYplus1 = ((dstY + 1) * source.Height) / Height;
                int srcHeight = srcYplus1 - srcY;

                for (int dstX = 0; dstX < Width; ++dstX)
                {
                    int srcX = (dstX * source.Width) / Width;
                    int srcXplus1 = ((dstX + 1) * source.Width) / Width;
                    int srcWidth = srcXplus1 - srcX;
                    int blueSum = 0;
                    int greenSum = 0;
                    int redSum = 0;
                    int alphaSum = 0;
                    int factor = 0;

                    for (int sy = srcY; sy < (srcY + srcHeight); ++sy)
                    {
                        for (int sx = srcX; sx < (srcX + srcWidth); ++sx)
                        {
                            ColorBgra c = source[sx, sy];

                            blueSum += c.b;
                            greenSum += c.g;
                            redSum += c.r;
                            alphaSum += c.a;
                            ++factor;
                        }
                    }

                    if (factor == 0)
                    {
                        continue;
                    }

                    int blueAvg = blueSum / factor;
                    int greenAvg = greenSum / factor;
                    int redAvg = redSum / factor;
                    int alphaAvg = alphaSum / factor;

                    this[dstX, dstY] = ColorBgra.FromBgra((byte)blueAvg, (byte)greenAvg, (byte)redAvg, (byte)alphaAvg);
                }
            }
        }

        public void BilinearFitSurface(Surface source)
        {
            int x;
            int y;
            uint mx;
            uint my;
            uint inRow;
            uint outRow;
            int inMaxX;
            int inMaxY;
            int srcStride;
            ColorBgra *inPtr;
            ColorBgra *outPtr;
            int dstWidth;
            int dstHeight;

            // figure out "slopes" for both horizontal and vertical, using fixed-point math
            // the slop is the x/y increment for the original image. ie, increment the x value
            // every 2 pixels for every 1 pixel being written to the Dest (that would be
            // if half-sizing Source into Dest)
            mx = ((uint)source.Width << 12) / (uint)this.Width;
            my = ((uint)source.Height << 12) / (uint)this.Height;

            outRow = 0;
            inMaxX = (source.Width - 1) << 12;
            inMaxY = (source.Height - 1) << 12;
            dstWidth = this.Width;
            dstHeight = this.Height;
            srcStride = source.Stride;

            // In order to avoid a black line on the right hand and bottom sides, we resize to
            // 1 pixel wide and 1 pixel taller.
            dstWidth++;
            dstHeight++;

            for (y = 0, inRow = 0; y < dstHeight - 1; ++y)
            {
                uint inColumn;
                uint inRowFract;
                uint inRowFractInv;

                inRowFract = inRow & 0xfff;
                inRowFractInv = 0x1000 - inRowFract;

                inPtr  = source.GetRowAddress((int)(inRow >> 12));
                outPtr = this.GetRowAddress((int)outRow);

                for (x = 0, inColumn = 0; x < dstWidth - 1; x++)
                {
                    uint inColumnFract;
                    uint inColumnFractInv;
                    uint wa, wb, wc, wd;    // weight values

                    inColumnFract = inColumn & 0xfff;
                    inColumnFractInv = 0x1000 - inColumnFract;

                    // Compute weight values for (x,y), (x+1,y), (x,y+1), (x+1,y+1)
                    wa = (inColumnFractInv * inRowFractInv) >> 12;
                    wb = (inColumnFract    * inRowFractInv) >> 12;
                    wc = (inColumnFractInv * inRowFract)    >> 12;
                    wd = (inColumnFract    * inRowFract)    >> 12;

                    // get texel samples: (x,y), (x+1,y), (x,y+1), (x+1,y+1)
                    ColorBgra *ta = inPtr + (inColumn >> 12);
                    ColorBgra *tb = ta + 1;
                    // btw, pointer is casted to uint8* so that SrcStride can be used to increment
                    // pointer in bytes, not pixels (ie, 4 bytes)
                    ColorBgra *tc = (ColorBgra *)(((byte *)ta) + srcStride);
                    ColorBgra *td = tc + 1;

                    // now compute the resultant pixel
                    *outPtr = ColorBgra.FromBgra
                        (
                        (byte)(((ta->b * wa) + (tb->b * wb) + (tc->b * wc) + (td->b * wd)) >> 12),
                        (byte)(((ta->g * wa) + (tb->g * wb) + (tc->g * wc) + (td->g * wd)) >> 12),
                        (byte)(((ta->r * wa) + (tb->r * wb) + (tc->r * wc) + (td->r * wd)) >> 12),
                        (byte)(((ta->a * wa) + (tb->a * wb) + (tc->a * wc) + (td->a * wd)) >> 12)
                        );

                    outPtr++;
                    inColumn += mx;
                }

                outRow++;
                inRow += my;
            }
        }

        #region IDisposable Members
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    scan0.Dispose();
                    scan0 = null;
                }
            }
        }
        #endregion
    }
}
