/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
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
    /// This is our Surface type. We allocate our own blocks of memory for this,
    /// and provide ways to create a GDI+ Bitmap object that aliases our surface.
    /// That way we can do everything fast, in memory and have complete control,
    /// and still have the ability to use GDI+ for drawing and rendering where
    /// appropriate.
    /// </summary>
    [Serializable]
    public sealed class Surface
        : IDisposable,
          ICloneable
    {
        private MemoryBlock scan0;
        private int width;
        private int height;
        private int stride;

        /// <summary>
        /// Gets a MemoryBlock which is the buffer holding the pixels associated
        /// with this Surface.
        /// </summary>
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

        /// <summary>
        /// Gets the width, in pixels, of this Surface.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// Gets the height, in pixels, of this Surface.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Gets the stride, in bytes, for this Surface.
        /// </summary>
        /// <remarks>
        /// Stride is defined as the number of bytes between the beginning of a row and
        /// the beginning of the next row. Thus, in loose C notation: stride = (byte *)&this[0, 1] - (byte *)&this[0, 0].
        /// Stride will always be equal to <b>or greater than</b> Width * ColorBgra.SizeOf.
        /// </remarks>
        public int Stride
        {
            get
            {
                return stride;
            }
        }

        /// <summary>
        /// Gets the size, in pixels, of this Surface.
        /// </summary>
        /// <remarks>
        /// This is a convenience function that creates a new Size instance based
        /// on the values of the Width and Height properties.
        /// </remarks>
        public Size Size
        {
            get
            {
                return new Size(width, height);
            }
        }

        /// <summary>
        /// Gets the GDI+ PixelFormat of this Surface.
        /// </summary>
        /// <remarks>
        /// This property always returns PixelFormat.Format32bppArgb.
        /// </remarks>
        public PixelFormat PixelFormat
        {
            get
            {
                return PixelFormat.Format32bppArgb;
            }
        }

        /// <summary>
        /// Gets the bounds of this Surface, in pixels.
        /// </summary>
        /// <remarks>
        /// This is a convenience function that returns Rectangle(0, 0, Width, Height).
        /// </remarks>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(0, 0, width, height);
            }
        }

        /// <summary>
        /// Creates a new instance of the Surface class.
        /// </summary>
        /// <param name="size">The size, in pixels, of the new Surface.</param>
        public Surface(Size size)
            : this(size.Width, size.Height)
        {
        }

        /// <summary>
        /// Creates a new instance of the Surface class.
        /// </summary>
        /// <param name="width">The width, in pixels, of the new Surface.</param>
        /// <param name="height">The height, in pixels, of the new Surface.</param>
        public Surface(int width, int height)
        {
            int stride;
            long bytes;

            try
            {
                stride = checked(width * ColorBgra.SizeOf);
                bytes = (long)height * (long)stride;
            }

            catch (OverflowException ex)
            {
                throw new OutOfMemoryException("Dimensions are too large - not enough memory, width=" + width.ToString() + ", height=" + height.ToString(), ex);
            }

            MemoryBlock scan0 = new MemoryBlock(width, height);
            Create(width, height, stride, scan0);
        }

        /// <summary>
        /// Creates a new instance of the Surface class that reuses a block of memory that was previously allocated.
        /// </summary>
        /// <param name="width">The width, in pixels, for the Surface.</param>
        /// <param name="height">The height, in pixels, for the Surface.</param>
        /// <param name="stride">The stride, in bytes, for the Surface.</param>
        /// <param name="scan0">The MemoryBlock to use. The beginning of this buffer defines the upper left (0, 0) pixel of the Surface.</param>
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
            Dispose(false);
        }

        /// <summary>
        /// Creates a Surface that aliases a portion of this Surface.
        /// </summary>
        /// <param name="bounds">The portion of this Surface that will be aliased.</param>
        /// <remarks>The upper left corner of the new Surface will correspond to the 
        /// upper left corner of this rectangle in the original Surface.</remarks>
        /// <returns>A Surface that aliases the requested portion of this Surface.</returns>
        public Surface CreateWindow(Rectangle bounds)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            if (bounds.Height == 0)
            {
                throw new ArgumentOutOfRangeException("height", "must be greater than zero");
            }

            Rectangle original = this.Bounds;
            Rectangle sub = new Rectangle(bounds.Location, bounds.Size);
            Rectangle clipped = Rectangle.Intersect(original, sub);

            if (clipped != sub)
            {
                throw new ArgumentOutOfRangeException("bounds", bounds, "bounds parameter must be a subset of this Surface's bounds");
            }

            long offset = ((long)stride * (long)bounds.Y) + ((long)ColorBgra.SizeOf * (long)bounds.X);
            long length = ((bounds.Height - 1) * (long)stride) + (long)bounds.Width * (long)ColorBgra.SizeOf;
            MemoryBlock block = new MemoryBlock(this.scan0, offset, length);
            return new Surface(bounds.Width, bounds.Height, this.stride, block);
        }

        /// <summary>
        /// Gets the offset, in bytes, of the requested row from the start of the surface.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>The number of bytes between (0,0) and (0,y).</returns>
        public long GetRowByteOffset(int y)
        {
            if (y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("y", "Out of bounds: y=" + y.ToString());
            }

            return (long)y * (long)stride;
        }

        /// <summary>
        /// Gets the offset, in bytes, of the requested row from the start of the surface.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>The number of bytes between (0,0) and (0,y)</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetRowByteOffset().
        /// </remarks>
        public unsafe long GetRowByteOffsetUnchecked(int y)
        {
            return (long)y * (long)stride;
        }

        /// <summary>
        /// Gets a pointer to the beginning of the requested row in the surface.
        /// </summary>
        /// <param name="y">The row</param>
        /// <returns>A pointer that references (0,y) in this surface.</returns>
        /// <remarks>Since this returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra *GetRowAddress(int y)
        {
            return (ColorBgra *)(((byte *)scan0.VoidStar) + GetRowByteOffset(y));
        }

        /// <summary>
        /// Gets a pointer to the beginning of the requested row in the surface.
        /// </summary>
        /// <param name="y">The row</param>
        /// <returns>A pointer that references (0,y) in this surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetRowAddress().
        /// </remarks>
        public unsafe ColorBgra *GetRowAddressUnchecked(int y)
        {
            return (ColorBgra *)(((byte *)scan0.VoidStar) + GetRowByteOffsetUnchecked(y));
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of a row to the requested column.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>
        /// The number of bytes between (0,n) and (x,n) where n is in the range [0, Height).
        /// </returns>
        public long GetColumnByteOffset(int x)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", x, "Out of bounds");
            }

            return (long)x * (long)ColorBgra.SizeOf;
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of a row to the requested column.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>
        /// The number of bytes between (0,n) and (x,n) where n is in the range [0, Height).
        /// </returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetColumnByteOffset().
        /// </remarks>
        public long GetColumnByteOffsetUnchecked(int x)
        {
            return (long)x * (long)ColorBgra.SizeOf;
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of the surface's buffer to
        /// the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>
        /// The number of bytes between (0,0) and (x,y).
        /// </returns>
        public long GetPointByteOffset(int x, int y)
        {
            return GetRowByteOffset(y) + GetColumnByteOffset(x);
        }

        /// <summary>
        /// Gets the number of bytes from the beginning of the surface's buffer to
        /// the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>
        /// The number of bytes between (0,0) and (x,y).
        /// </returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointByteOffset().
        /// </remarks>
        public long GetPointByteOffsetUnchecked(int x, int y)
        {
            return GetRowByteOffsetUnchecked(y) + GetColumnByteOffsetUnchecked(x);
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>The color at the requested location.</returns>
        public ColorBgra GetPoint(int x, int y)
        {
            return this[x, y];
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>The color at the requested location.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPoint().
        /// </remarks>
        public unsafe ColorBgra GetPointUnchecked(int x, int y)
        {
            return *(x + (ColorBgra *)(((byte *)scan0.VoidStar) + (y * stride)));
        }

        /// <summary>
        /// Gets the color at a specified point in the surface.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>The color at the requested location.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPoint().
        /// </remarks>
        public unsafe ColorBgra GetPointUnchecked(Point pt)
        {
            return GetPointUnchecked(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>Since this method returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra *GetPointAddress(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", "Out of bounds: x=" + x.ToString());
            }

            return GetRowAddress(y) + x;
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>Since this method returns a pointer, it is potentially unsafe to use.</remarks>
        public unsafe ColorBgra *GetPointAddress(Point pt)
        {
            return GetPointAddress(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointAddress().
        /// </remarks>
        public unsafe ColorBgra *GetPointAddressUnchecked(int x, int y)
        {
            return unchecked(x + (ColorBgra *)(((byte *)scan0.VoidStar) + (y * stride)));
        }

        /// <summary>
        /// Gets the address in memory of the requested point.
        /// </summary>
        /// <param name="pt">The point to retrieve.</param>
        /// <returns>A pointer to the requested point in the surface.</returns>
        /// <remarks>
        /// This method does not do any bounds checking and is potentially unsafe to use,
        /// but faster than GetPointAddress().
        /// </remarks>
        public unsafe ColorBgra *GetPointAddressUnchecked(Point pt)
        {
            return GetPointAddressUnchecked(pt.X, pt.Y);
        }

        /// <summary>
        /// Gets a MemoryBlock that references the row requested.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>A MemoryBlock that gives access to the bytes in the specified row.</returns>
        /// <remarks>This method is the safest to use for direct memory access to a row's pixel data.</remarks>
        public MemoryBlock GetRow(int y)
        {
            return new MemoryBlock(scan0, GetRowByteOffset(y), (long)width * (long)ColorBgra.SizeOf);
        }

        public bool IsContiguousMemoryRegion(Rectangle bounds)
        {
            bool oneRow = (bounds.Height == 1);
            bool manyRows = (this.Stride == (this.Width * ColorBgra.SizeOf) && 
                this.Width == bounds.Width);

            return oneRow || manyRows;
        }

        /// <summary>
        /// Determines if the requested pixel coordinate is within bounds.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>true if (x,y) is in bounds, false if it's not.</returns>
        public bool IsVisible(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// Determines if the requested pixel coordinate is within bounds.
        /// </summary>
        /// <param name="pt">The coordinate.</param>
        /// <returns>true if (pt.X, pt.Y) is in bounds, false if it's not.</returns>
        public bool IsVisible(Point pt)
        {
            return IsVisible(pt.X, pt.Y);
        }

        /// <summary>
        /// Determines if the requested row offset is within bounds.
        /// </summary>
        /// <param name="y">The row.</param>
        /// <returns>true if y &gt;= 0 and y &lt; height, otherwise false</returns>
        public bool IsRowVisible(int y)
        {
            return y >= 0 && y < Height;
        }

        /// <summary>
        /// Determines if the requested column offset is within bounds.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <returns>true if x &gt;= 0 and x &lt; width, otherwise false.</returns>
        public bool IsColumnVisible(int x)
        {
            return x >= 0 && x < Width;
        }

        /*
        // This code is currently not used anywhere, so I am commenting it out.
        // However, we will probably use it in the future (Zoom Blur, optimized Motion Blur)
        // so I'm keeping it here for now instead of just nuking it.
        // Note that this code is NOT up to coding standards and is not necessarily
        // robust enough that we can just uncomment and go straight to using it.
        // -Rick
        public ColorBgra GetLineSampleChecked(PointF start, PointF end)
        {
            float dx = start.X - end.X;
            float dy = start.Y - end.Y;

            if (dx * dx > dy * dy)
            {
                if (start.X <= end.X)
                {
                    return GetLineSampleHorizontalChecked(start, end);
                }
                else
                {
                    return GetLineSampleHorizontalChecked(end, start);
                }
            }
            else
            {
                if (start.Y <= end.Y)
                {
                    return GetLineSampleVerticalChecked(start, end);
                }
                else
                {
                    return GetLineSampleVerticalChecked(end, start);
                }
            }

        }

        //end.X >= start.X, and dy/dx <= 1
        private ColorBgra GetLineSampleHorizontalChecked(PointF start, PointF end)
        {
            //Clip the line, pass to unchecked method
            if (end.X >= 0 && start.X < width)
            {
                float slope = (end.Y - start.Y) / (end.X - start.X);

                if (end.X > width - 1)
                {
                    end.Y = (width - 1 - start.X) * slope + start.Y;
                    end.X = width - 1;
                }

                if (start.X < 0)
                {
                    start.Y = end.Y - slope * end.X;
                    start.X = 0;
                }

                if (start.Y < 0 && end.Y < 0)
                {
                    return ColorBgra.FromUInt32(0);
                }
                else if (end.Y > height - 1 && start.Y > height - 1)
                {
                    return ColorBgra.FromUInt32(0);
                }
                else
                {
                    if (start.Y < 0)
                    {
                        start.Y = 0;
                        start.X = end.Y / slope + end.X;
                    }
                    else if (start.Y > height - 1)
                    {
                        start.Y = height - 1;
                        start.X = end.X - (height - 1 - end.Y) / slope;
                    }

                    if (end.Y < 0)
                    {
                        end.Y = 0;
                        end.X = start.X - start.Y / slope;
                    }
                    else if (end.Y > height - 1)
                    {
                        start.Y = height - 1;
                        start.X = start.X + (height - 1 - start.Y) / slope;
                    }

                    return GetLineSampleHorizontalUnchecked(start, end);
                }
            }
            else
            {
                return ColorBgra.FromUInt32(0);
            }
        }

        private unsafe ColorBgra GetLineSampleHorizontalUnchecked(PointF start, PointF end)
        {
            int fsx = (int)(start.X * 256);
            int fsy = (int)(start.Y * 256);
            int fex = (int)(end.X * 256);
            int fey = (int)(end.Y * 256);

            if (fex - fsx < 256)
            {
                return GetBilinearSample((start.X + end.X) / 2, (start.Y + end.Y) / 2, false);
            }
            else
            {
                int dx = fex - fsx;
                int dy = fey - fsy;
                int dir = (dy < 0) ? -1 : 1;

                dy *= dir;

                int gcd = Utility.GreatestCommonDivisor(dx, dy);

                dx /= gcd;
                dy /= gcd;

                int sx = (1 + ((fsx - 1) | 0xFF)) >> 8;
                int ex = fex >> 8;
                int fsy_ = fsy + ((sx << 8) - fsx) * dy / dx;
                int y = fsy_ >> 8;

                //TODO: add tips
                //add dy until you get dx
                int accum = (((y << 8) - fsy_) * dir * dx) >> 8;

                ColorBgra* ptr1 = GetPointAddressUnchecked(sx, y);
                ColorBgra* ptr2 = GetPointAddressUnchecked(sx, (y + dir >= 0 && y + dir < height) ? y + dir : y); //boundscheck
                int ptrIncr = dir * stride / sizeof(ColorBgra);

                int sr = 0;
                int sg = 0;
                int sb = 0;
                int sa = 0;
                int sw = 0;
                int sc = 0;

                for (int x = sx; x <= ex; ++x)
                {
                    if (accum >= dx)
                    {
                        accum -= dx;
                        y += dir;

                        ptr1 += ptrIncr;
                        if (y > 0 && y < height - 1)
                        {
                            ptr2 += ptrIncr;
                        }
                    }

                    int w1 = ((ptr1->A >> 7) + ptr1->A) * (dx - accum);
                    int w2 = ((ptr2->A >> 7) + ptr2->A) * accum;

                    sr += w1 * ptr1->R + w2 * ptr2->R;
                    sg += w1 * ptr1->G + w2 * ptr2->G;
                    sb += w1 * ptr1->B + w2 * ptr2->B;
                    sa += (dx - accum) * ptr1->A + accum * ptr2->A;
                    sc += dx;
                    sw += w1 + w2;

                    accum += dy;
                    ++ptr1;
                    ++ptr2;
                }

                sr /= sw;
                sg /= sw;
                sb /= sw;
                sa /= sc;

                return ColorBgra.FromBgra((byte)sr, (byte)sg, (byte)sb, (byte)sa);
            }
        }

        private ColorBgra GetLineSampleVerticalChecked(PointF start, PointF end)
        {
            return ColorBgra.FromUInt32(0);
        }

        private ColorBgra GetLineSampleVerticalUnchecked(PointF start, PointF end)
        {
            return ColorBgra.FromUInt32(0);
        }
        */

        public ColorBgra GetBilinearSample(float x, float y, bool wrap)
        {
            uint tilingMask = wrap ? 0xffffffff : 0;
            float u = x;
            float v = y;

            if (wrap || (u >= -1 && v >= -1 && u <= width && v <= height))
            {
                unchecked
                {
                    int iu = (int)Math.Floor(u);
                    uint sxfrac = (uint)(256 * (u - (float)iu));
                    uint sxfracinv = 256 - sxfrac;

                    int iv = (int)Math.Floor(v);
                    uint syfrac = (uint)(256 * (v - (float)iv));
                    uint syfracinv = 256 - syfrac;

                    uint wul = (uint)(sxfracinv * syfracinv);
                    uint wur = (uint)(sxfrac * syfracinv);
                    uint wll = (uint)(sxfracinv * syfrac);
                    uint wlr = (uint)(sxfrac * syfrac);

                    uint inBoundsMaskLeft = tilingMask;
                    uint inBoundsMaskTop = tilingMask;
                    uint inBoundsMaskRight = tilingMask;
                    uint inBoundsMaskBottom = tilingMask;

                    int sx = iu;
                    if (sx < 0)
                    {
                        sx = (width - 1) + ((sx + 1) % width);
                    }
                    else if (sx > (width - 1))
                    {
                        sx = sx % width;
                    }
                    else
                    {
                        inBoundsMaskLeft = 0xffffffff;
                    }

                    int sy = iv;
                    if (sy < 0)
                    {
                        sy = (height - 1) + ((sy + 1) % height);
                    }
                    else if (sy > (height - 1))
                    {
                        sy = sy % height;
                    }
                    else
                    {
                        inBoundsMaskTop = 0xffffffff;
                    }

                    int sleft = sx;
                    int sright;

                    if (sleft == (width - 1))
                    {
                        sright = 0;
                        inBoundsMaskRight = (iu == -1) ? 0xffffffff : tilingMask;
                    }
                    else
                    {
                        sright = sleft + 1;
                        inBoundsMaskRight = inBoundsMaskLeft & 0xffffffff;
                    }

                    int stop = sy;
                    int sbottom;

                    if (stop == (height - 1))
                    {
                        sbottom = 0;
                        inBoundsMaskBottom = (iv == -1) ? 0xffffffff : tilingMask;
                    }
                    else
                    {
                        sbottom = stop + 1;
                        inBoundsMaskBottom = inBoundsMaskTop & 0xffffffff;
                    }
                                   
                    const uint edgeColor = 0x00ffffff;
                                     
                    uint maskUL = inBoundsMaskLeft & inBoundsMaskTop;
                    ColorBgra cul = ColorBgra.FromUInt32((GetPointUnchecked(sleft, stop).Bgra & maskUL) | (edgeColor & ~maskUL));

                    uint maskUR = inBoundsMaskRight & inBoundsMaskTop;
                    ColorBgra cur = ColorBgra.FromUInt32((GetPointUnchecked(sright, stop).Bgra & maskUR) | (edgeColor & ~maskUR));

                    uint maskLL = inBoundsMaskLeft & inBoundsMaskBottom;
                    ColorBgra cll = ColorBgra.FromUInt32((GetPointUnchecked(sleft, sbottom).Bgra & maskLL) | (edgeColor & ~maskLL));

                    uint maskLR = inBoundsMaskRight & inBoundsMaskBottom;
                    ColorBgra clr = ColorBgra.FromUInt32((GetPointUnchecked(sright, sbottom).Bgra & maskLR) | (edgeColor & ~maskLR));

                    uint b = ((cul.B * wul) + (cur.B * wur) + (cll.B * wll) + (clr.B * wlr)) >> 16;
                    uint g = ((cul.G * wul) + (cur.G * wur) + (cll.G * wll) + (clr.G * wlr)) >> 16;
                    uint r = ((cul.R * wul) + (cur.R * wur) + (cll.R * wll) + (clr.R * wlr)) >> 16;
                    uint a = ((cul.A * wul) + (cur.A * wur) + (cll.A * wll) + (clr.A * wlr)) >> 16;

                    return ColorBgra.FromUInt32(b + (g << 8) + (r << 16) + (a << 24));
                }
            }
            else
            {
                return ColorBgra.FromUInt32(0x00ffffff);
            }
        }

        public unsafe ColorBgra GetBilinearSample2(float x, float y)
        {
            float u = x;
            float v = y;

            if (u >= 0 && v >= 0 && u < width && v < height)
            {
                unchecked
                {
                    int iu = (int)Math.Floor(u);
                    uint sxfrac = (uint)(256 * (u - (float)iu));
                    uint sxfracinv = 256 - sxfrac;

                    int iv = (int)Math.Floor(v);
                    uint syfrac = (uint)(256 * (v - (float)iv));
                    uint syfracinv = 256 - syfrac;

                    uint wul = (uint)(sxfracinv * syfracinv);
                    uint wur = (uint)(sxfrac * syfracinv);
                    uint wll = (uint)(sxfracinv * syfrac);
                    uint wlr = (uint)(sxfrac * syfrac);

                    int sx = iu;
                    int sy = iv;
                    int sleft = sx;
                    int sright;

                    if (sleft == (width - 1))
                    {
                        sright = sleft;
                    }
                    else
                    {
                        sright = sleft + 1;
                    }

                    int stop = sy;
                    int sbottom;

                    if (stop == (height - 1))
                    {
                        sbottom = stop;
                    }
                    else
                    {
                        sbottom = stop + 1;
                    }
                                   
                    ColorBgra *cul = GetPointAddressUnchecked(sleft, stop);
                    ColorBgra *cur = cul + (sright - sleft);
                    ColorBgra *cll = GetPointAddressUnchecked(sleft, sbottom);
                    ColorBgra *clr = cll + (sright - sleft);

                    const uint ww = 32768;
                    uint b = (ww + (cul->B * wul) + (cur->B * wur) + (cll->B * wll) + (clr->B * wlr)) >> 16;
                    uint g = (ww + (cul->G * wul) + (cur->G * wur) + (cll->G * wll) + (clr->G * wlr)) >> 16;
                    uint r = (ww + (cul->R * wul) + (cur->R * wur) + (cll->R * wll) + (clr->R * wlr)) >> 16;
                    uint a = (ww + (cul->A * wul) + (cur->A * wur) + (cll->A * wll) + (clr->A * wlr)) >> 16;

                    return ColorBgra.FromUInt32(b + (g << 8) + (r << 16) + (a << 24));
                }
            }
            else
            {
                return ColorBgra.FromUInt32(0x00ffffff);
            }
        }

        public unsafe ColorBgra GetBilinearSample2Clamped(float x, float y)
        {
            float u = x;
            float v = y;

            if (u < 0)
            {
                u = 0;
            }
            else if (u > this.Width - 1)
            {
                u = this.Width - 1;
            }

            if (v < 0)
            {
                v = 0;
            }
            else if (v > this.Height - 1)
            {
                v = this.Height - 1;
            }

            if (u >= 0 && v >= 0 && u < width && v < height)
            {
                unchecked
                {
                    int iu = (int)Math.Floor(u);
                    uint sxfrac = (uint)(256 * (u - (float)iu));
                    uint sxfracinv = 256 - sxfrac;

                    int iv = (int)Math.Floor(v);
                    uint syfrac = (uint)(256 * (v - (float)iv));
                    uint syfracinv = 256 - syfrac;

                    uint wul = (uint)(sxfracinv * syfracinv);
                    uint wur = (uint)(sxfrac * syfracinv);
                    uint wll = (uint)(sxfracinv * syfrac);
                    uint wlr = (uint)(sxfrac * syfrac);

                    int sx = iu;
                    int sy = iv;
                    int sleft = sx;
                    int sright;

                    if (sleft == (width - 1))
                    {
                        sright = sleft;
                    }
                    else
                    {
                        sright = sleft + 1;
                    }

                    int stop = sy;
                    int sbottom;

                    if (stop == (height - 1))
                    {
                        sbottom = stop;
                    }
                    else
                    {
                        sbottom = stop + 1;
                    }
                                   
                    ColorBgra *cul = GetPointAddressUnchecked(sleft, stop);
                    ColorBgra *cur = cul + (sright - sleft);
                    ColorBgra *cll = GetPointAddressUnchecked(sleft, sbottom);
                    ColorBgra *clr = cll + (sright - sleft);

                    uint ww = 32768;
                    uint b = (ww + (cul->B * wul) + (cur->B * wur) + (cll->B * wll) + (clr->B * wlr)) >> 16;
                    uint g = (ww + (cul->G * wul) + (cur->G * wur) + (cll->G * wll) + (clr->G * wlr)) >> 16;
                    uint r = (ww + (cul->R * wul) + (cur->R * wur) + (cll->R * wll) + (clr->R * wlr)) >> 16;
                    uint a = (ww + (cul->A * wul) + (cur->A * wur) + (cll->A * wll) + (clr->A * wlr)) >> 16;

                    return ColorBgra.FromUInt32(b + (g << 8) + (r << 16) + (a << 24));
                }
            }
            else
            {
                return ColorBgra.FromUInt32(0x00ffffff);
            }
        }

        /// <summary>
        /// Gets or sets the pixel value at the requested offset.
        /// </summary>
        public ColorBgra this[int x, int y]
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || y < 0 || x >= width || y >= height)
                {
                    throw new ArgumentOutOfRangeException("(x,y)", new Point(x, y), "Coordinates out of range, max=" + new Size(width - 1, height - 1).ToString());
                }

                unsafe
                {
                    return *GetPointAddressUnchecked(x, y);
                }
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Surface");
                }

                if (x < 0 || y < 0 || x >= width || y >= height)
                {
                    throw new ArgumentOutOfRangeException("(x,y)", new Point(x, y), "Coordinates out of range, max=" + new Size(width - 1, height - 1).ToString());
                }

                unsafe
                {
                    *GetPointAddressUnchecked(x, y) = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the pixel value at the requested offset.
        /// </summary>
        public ColorBgra this[Point pt]
        {
            get
            {
                return this[pt.X, pt.Y];
            }

            set
            {
                this[pt.X, pt.Y] = value;
            }
        }

        /// <summary>
        /// Helper function. Same as calling CreateAliasedBounds(Bounds).
        /// </summary>
        /// <returns>A GDI+ Bitmap that aliases the entire Surface.</returns>
        public Bitmap CreateAliasedBitmap()
        {
            return CreateAliasedBitmap(this.Bounds);
        }

        /// <summary>
        /// Helper function. Same as calling CreateAliasedBounds(bounds, true).
        /// </summary>
        /// <returns>A GDI+ Bitmap that aliases the entire Surface.</returns>
        public Bitmap CreateAliasedBitmap(Rectangle bounds)
        {
            return CreateAliasedBitmap(bounds, true);
        }

        /// <summary>
        /// Creates a GDI+ Bitmap object that aliases the same memory that this Surface does.
        /// Then you can use GDI+ to draw on to this surface.
        /// Note: Since the Bitmap does not hold a reference to this Surface object, nor to
        /// the MemoryBlock that it contains, you must hold a reference to the Surface object
        /// for as long as you wish to use the aliased Bitmap. Otherwise the memory may be
        /// freed and the Bitmap will look corrupt or cause other errors. You may use the
        /// RenderArgs class to help manage this lifetime instead.
        /// </summary>
        /// <param name="bounds">The rectangle of interest within this Surface that you wish to alias.</param>
        /// <param name="alpha">If true, the returned bitmap will use PixelFormat.Format32bppArgb. 
        /// If false, the returned bitmap will use PixelFormat.Format32bppRgb.</param>
        /// <returns>A GDI+ Bitmap that aliases the requested portion of the Surface.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><b>bounds</b> was not entirely within the boundaries of the Surface</exception>
        /// <exception cref="ObjectDisposedException">This Surface instance is already disposed.</exception>
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

            unsafe
            {
                return new Bitmap(bounds.Width, bounds.Height, stride, alpha ? this.PixelFormat : PixelFormat.Format32bppRgb, 
                    new IntPtr((void *)((byte *)scan0.VoidStar + GetPointByteOffsetUnchecked(bounds.X, bounds.Y))));
            }
        }

        /// <summary>
        /// Creates a new Surface and copies the pixels from a Bitmap to it.
        /// </summary>
        /// <param name="bitmap">The Bitmap to duplicate.</param>
        /// <returns>A new Surface that is the same size as the given Bitmap and that has the same pixel values.</returns>
        public static Surface CopyFromBitmap(Bitmap bitmap)
        {
            Surface surface = new Surface(bitmap.Width, bitmap.Height);
            BitmapData bd = bitmap.LockBits(surface.Bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                for (int y = 0; y < bd.Height; ++y)
                {
                    Memory.Copy((void *)surface.GetRowAddress(y), 
                        (byte *)bd.Scan0.ToPointer() + (y * bd.Stride), (ulong)bd.Width * ColorBgra.SizeOf);
                }
            }

            bitmap.UnlockBits(bd);
            return surface;
        }

        /// <summary>
        /// Copies the contents of the given surface to the upper left corner of this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <remarks>
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            if (this.stride == source.stride &&
                (this.width * ColorBgra.SizeOf) == this.stride &&
                this.width == source.width &&
                this.height == source.height)
            {
                unsafe
                {
                    Memory.Copy(this.scan0.VoidStar, 
                                source.scan0.VoidStar, 
                                ((ulong)(height - 1) * (ulong)stride) + ((ulong)width * (ulong)ColorBgra.SizeOf));
                }
            }
            else
            {
                int copyWidth = Math.Min(width, source.width);
                int copyHeight = Math.Min(height, source.height);

                unsafe
                {
                    for (int y = 0; y < copyHeight; ++y)
                    {
                        Memory.Copy(GetRowAddressUnchecked(y), source.GetRowAddressUnchecked(y), (ulong)copyWidth * (ulong)ColorBgra.SizeOf);
                    }
                }
            }
        }

        /// <summary>
        /// Copies the contents of the given surface to a location within this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="dstOffset">
        /// The offset within this surface to start copying pixels to. This will map to (0,0) in the source.
        /// </param>
        /// <remarks>
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
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

        /// <summary>
        /// Copies the contents of the given surface to the upper left of this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="sourceRoi">
        /// The region of the source to copy from. The upper left of this rectangle
        /// will be mapped to (0,0) on this surface.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </param>
        public void CopySurface(Surface source, Rectangle sourceRoi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            sourceRoi.Intersect(source.Bounds);
            int width = Math.Min(this.width, sourceRoi.Width);
            int height = Math.Min(this.Height, sourceRoi.Height);

            if (width == 0 || height == 0)
            {
                return;
            }

            using (Surface src = source.CreateWindow(sourceRoi))
            {
                CopySurface(src);
            }
        }

        /// <summary>
        /// Copies a rectangular region of the given surface to a specific location on this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="dstOffset">The location on this surface to start copying pixels to.</param>
        /// <param name="sourceRoi">The region of the source surface to copy pixels from.</param>
        /// <remarks>
        /// sourceRoi.Location will be mapped to dstOffset.Location.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
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

            using (Surface src = source.CreateWindow(sourceRoi))
            {
                CopySurface(src, dstOffset);
            }
        }

        /// <summary>
        /// Copies a region of the given surface to this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="region">The region to clip copying to.</param>
        /// <remarks>
        /// The upper left corner of the source surface will be mapped to the upper left of this
        /// surface, and only those pixels that are defined by the region will be copied.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source, PdnRegion region)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Rectangle[] scans = region.GetRegionScansReadOnlyInt();
            for (int i = 0; i < scans.Length; ++i)
            {
                Rectangle rect = scans[i];

                rect.Intersect(this.Bounds);
                rect.Intersect(source.Bounds);

                if (rect.Width == 0 || rect.Height == 0)
                {
                    continue;
                }

                unsafe
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra *dst = this.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra *src = source.GetPointAddressUnchecked(rect.Left, y);
                        Memory.Copy(dst, src, (ulong)rect.Width * (ulong)ColorBgra.SizeOf);
                    }
                }
            }
        }

        /// <summary>
        /// Copies a region of the given surface to this surface.
        /// </summary>
        /// <param name="source">The surface to copy pixels from.</param>
        /// <param name="region">The region to clip copying to.</param>
        /// <remarks>
        /// The upper left corner of the source surface will be mapped to the upper left of this
        /// surface, and only those pixels that are defined by the region will be copied.
        /// The source surface does not need to have the same dimensions as this surface. Clipping
        /// will be handled automatically. No resizing will be done.
        /// </remarks>
        public void CopySurface(Surface source, Rectangle[] region, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = region[i];

                rect.Intersect(this.Bounds);
                rect.Intersect(source.Bounds);

                if (rect.Width == 0 || rect.Height == 0)
                {
                    continue;
                }

                unsafe
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* dst = this.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* src = source.GetPointAddressUnchecked(rect.Left, y);
                        Memory.Copy(dst, src, (ulong)rect.Width * (ulong)ColorBgra.SizeOf);
                    }
                }
            }
        }

        public void CopySurface(Surface source, Rectangle[] region)
        {
            CopySurface(source, region, 0, region.Length);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Creates a new surface with the same dimensions and pixel values as this one.
        /// </summary>
        /// <returns>A new surface that is a clone of the current one.</returns>
        public Surface Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Surface");
            }

            Surface ret = new Surface(this.Size);
            ret.CopySurface(this);
            return ret;
        }

        /// <summary>
        /// Clears the surface to all-white (BGRA = [255,255,255,255]).
        /// </summary>
        public void Clear()
        {
            Clear(ColorBgra.FromBgra(255, 255, 255, 255));
        }

        /// <summary>
        /// Clears the surface to the given color value.
        /// </summary>
        /// <param name="color">The color value to fill the surface with.</param>
        public void Clear(ColorBgra color)
        {
            new UnaryPixelOps.Constant(color).Apply(this, this.Bounds);
        }

        /// <summary>
        /// Clears the given rectangular region within the surface to the given color value.
        /// </summary>
        /// <param name="color">The color value to fill the rectangular region with.</param>
        /// <param name="rect">The rectangular region to fill.</param>
        public void Clear(ColorBgra color, Rectangle rect)
        {
            Rectangle rect2 = Rectangle.Intersect(this.Bounds, rect);

            if (rect2 != rect)
            {
                throw new ArgumentOutOfRangeException("rectangle is out of bounds");
            }

            new UnaryPixelOps.Constant(color).Apply(this, rect);
        }

        /// <summary>
        /// Fits the source surface to this surface using super sampling. If the source surface is less wide
        /// or less tall than this surface (i.e. magnification), bicubic resampling is used instead. If either
        /// the source or destination has a dimension that is only 1 pixel, nearest neighbor is used.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void SuperSamplingFitSurface(Surface source)
        {
            SuperSamplingFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using super sampling. If the source surface is less wide
        /// or less tall than this surface (i.e. magnification), bicubic resampling is used instead. If either
        /// the source or destination has a dimension that is only 1 pixel, nearest neighbor is used.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void SuperSamplingFitSurface(Surface source, Rectangle dstRoi)
        {
            if (source.Width <= Width || source.Height <= Height)
            {
                if (source.width < 2 || source.height < 2 || this.width < 2 || this.height < 2)
                {
                    this.NearestNeighborFitSurface(source, dstRoi);
                }
                else
                {
                    this.BicubicFitSurface(source, dstRoi);
                }
            }
            else unsafe
            {
                Rectangle dstRoi2 = Rectangle.Intersect(dstRoi, this.Bounds);

                for (int dstY = dstRoi2.Top; dstY < dstRoi2.Bottom; ++dstY)
                {
                    double srcTop = (double)(dstY * source.height) / (double)height;
                    double srcTopFloor = Math.Floor(srcTop);
                    double srcTopWeight = 1 - (srcTop - srcTopFloor);
                    int srcTopInt = (int)srcTopFloor;

                    double srcBottom = (double)((dstY + 1) * source.height) / (double)height;
                    double srcBottomFloor = Math.Floor(srcBottom - 0.00001);
                    double srcBottomWeight = srcBottom - srcBottomFloor;
                    int srcBottomInt = (int)srcBottomFloor;

                    ColorBgra *dstPtr = this.GetPointAddressUnchecked(dstRoi2.Left, dstY);

                    for (int dstX = dstRoi2.Left; dstX < dstRoi2.Right; ++dstX)
                    {
                        double srcLeft = (double)(dstX * source.width) / (double)width;
                        double srcLeftFloor = Math.Floor(srcLeft);
                        double srcLeftWeight = 1 - (srcLeft - srcLeftFloor);
                        int srcLeftInt = (int)srcLeftFloor;

                        double srcRight = (double)((dstX + 1) * source.width) / (double)width;
                        double srcRightFloor = Math.Floor(srcRight - 0.00001);
                        double srcRightWeight = srcRight - srcRightFloor;
                        int srcRightInt = (int)srcRightFloor;

                        double blueSum = 0;
                        double greenSum = 0;
                        double redSum = 0;
                        double alphaSum = 0;

                        // left fractional edge
                        ColorBgra *srcLeftPtr = source.GetPointAddressUnchecked(srcLeftInt, srcTopInt + 1);

                        for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                        {
                            blueSum += srcLeftPtr->B * srcLeftWeight;
                            greenSum += srcLeftPtr->G * srcLeftWeight;
                            redSum += srcLeftPtr->R * srcLeftWeight;
                            alphaSum += srcLeftPtr->A * srcLeftWeight;
                            srcLeftPtr = (ColorBgra *)((byte *)srcLeftPtr + source.stride);
                        }

                        // right fractional edge
                        ColorBgra *srcRightPtr = source.GetPointAddressUnchecked(srcRightInt, srcTopInt + 1);
                        for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                        {
                            blueSum += srcRightPtr->B * srcRightWeight;
                            greenSum += srcRightPtr->G * srcRightWeight;
                            redSum += srcRightPtr->R * srcRightWeight;
                            alphaSum += srcRightPtr->A * srcRightWeight;
                            srcRightPtr = (ColorBgra *)((byte *)srcRightPtr + source.stride);
                        }

                        // top fractional edge
                        ColorBgra *srcTopPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcTopInt);
                        for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                        {
                            blueSum += srcTopPtr->B * srcTopWeight;
                            greenSum += srcTopPtr->G * srcTopWeight;
                            redSum += srcTopPtr->R * srcTopWeight;
                            alphaSum += srcTopPtr->A * srcTopWeight;
                            ++srcTopPtr;
                        }

                        // bottom fractional edge
                        ColorBgra *srcBottomPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcBottomInt);
                        for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                        {
                            blueSum += srcBottomPtr->B * srcBottomWeight;
                            greenSum += srcBottomPtr->G * srcBottomWeight;
                            redSum += srcBottomPtr->R * srcBottomWeight;
                            alphaSum += srcBottomPtr->A * srcBottomWeight;
                            ++srcBottomPtr;
                        }

                        // center area
                        for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                        {
                            ColorBgra *srcPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcY);

                            for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                            {
                                blueSum += (double)srcPtr->B;
                                greenSum += (double)srcPtr->G;
                                redSum += (double)srcPtr->R;
                                alphaSum += (double)srcPtr->A;
                                ++srcPtr;
                            }
                        }

                        // four corner pixels
                        ColorBgra srcTL = source.GetPoint(srcLeftInt, srcTopInt);
                        blueSum += srcTL.B * (srcTopWeight * srcLeftWeight);
                        greenSum += srcTL.G * (srcTopWeight * srcLeftWeight);
                        redSum += srcTL.R * (srcTopWeight * srcLeftWeight);
                        alphaSum += srcTL.A * (srcTopWeight * srcLeftWeight);

                        ColorBgra srcTR = source.GetPoint(srcRightInt, srcTopInt);
                        blueSum += srcTR.B * (srcTopWeight * srcRightWeight);
                        greenSum += srcTR.G * (srcTopWeight * srcRightWeight);
                        redSum += srcTR.R * (srcTopWeight * srcRightWeight);
                        alphaSum += srcTR.A * (srcTopWeight * srcRightWeight);

                        ColorBgra srcBL = source.GetPoint(srcLeftInt, srcBottomInt);
                        blueSum += srcBL.B * (srcBottomWeight * srcLeftWeight);
                        greenSum += srcBL.G * (srcBottomWeight * srcLeftWeight);
                        redSum += srcBL.R * (srcBottomWeight * srcLeftWeight);
                        alphaSum += srcBL.A * (srcBottomWeight * srcLeftWeight);

                        ColorBgra srcBR = source.GetPoint(srcRightInt, srcBottomInt);
                        blueSum += srcBR.B * (srcBottomWeight * srcRightWeight);
                        greenSum += srcBR.G * (srcBottomWeight * srcRightWeight);
                        redSum += srcBR.R * (srcBottomWeight * srcRightWeight);
                        alphaSum += srcBR.A * (srcBottomWeight * srcRightWeight);

                        double area = (srcRight - srcLeft) * (srcBottom - srcTop);
                        double brightnessCorrection = area / 2;

                        blueSum += brightnessCorrection;
                        greenSum += brightnessCorrection;
                        redSum += brightnessCorrection;
                        alphaSum += brightnessCorrection;

                        double blue = blueSum / area;
                        double green = greenSum / area;
                        double red = redSum / area;
                        double alpha = alphaSum / area;

                        dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                        ++dstPtr;
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using nearest neighbor resampling.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        public void NearestNeighborFitSurface(Surface source)
        {
            NearestNeighborFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using nearest neighbor resampling.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        public void NearestNeighborFitSurface(Surface source, Rectangle dstRoi)
        {
            Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);

            unsafe
            {
                for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                {
                    int srcY = (dstY * source.height) / height;
                    ColorBgra *srcRow = source.GetRowAddressUnchecked(srcY);
                    ColorBgra *dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        int srcX = (dstX * source.width) / width;
                        *dstPtr = *(srcRow + srcX);
                        ++dstPtr;
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using bicubic interpolation.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <remarks>
        /// This method was implemented with correctness, not performance, in mind. 
        /// Based on: "Bicubic Interpolation for Image Scaling" by Paul Bourke,
        ///           http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </remarks>
        public void BicubicFitSurface(Surface source)
        {
            BicubicFitSurface(source, this.Bounds);
        }

        private float CubeClamped(float x)
        {
            if (x >= 0)
            {
                return x * x * x;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Implements R() as defined at http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </summary>
        private float R(float x)
        {
            return (CubeClamped(x + 2) - (4 * CubeClamped(x + 1)) + (6 * CubeClamped(x)) - (4 * CubeClamped(x - 1))) / 6;
        }

        /// <summary>
        /// Fits the source surface to this surface using bicubic interpolation.
        /// </summary>
        /// <param name="source">The Surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>
        /// This method was implemented with correctness, not performance, in mind. 
        /// Based on: "Bicubic Interpolation for Image Scaling" by Paul Bourke,
        ///           http://astronomy.swin.edu.au/%7Epbourke/colour/bicubic/
        /// </remarks>
        public void BicubicFitSurface(Surface source, Rectangle dstRoi)
        {
            float leftF = (1 * (float)(width - 1)) / (float)(source.width - 1);
            float topF = (1 * (height - 1)) / (float)(source.height - 1);
            float rightF = ((float)(source.width - 3) * (float)(width - 1)) / (float)(source.width - 1);
            float bottomF = ((float)(source.Height - 3) * (float)(height - 1)) / (float)(source.height - 1);

            int left = (int)Math.Ceiling((double)leftF);
            int top = (int)Math.Ceiling((double)topF);
            int right = (int)Math.Floor((double)rightF);
            int bottom = (int)Math.Floor((double)bottomF);

            Rectangle[] rois = new Rectangle[] {
                                                   Rectangle.FromLTRB(left, top, right, bottom),
                                                   new Rectangle(0, 0, width, top),
                                                   new Rectangle(0, top, left, height - top),
                                                   new Rectangle(right, top, width - right, height - top),
                                                   new Rectangle(left, bottom, right - left, height - bottom)
                                               };

            for (int i = 0; i < rois.Length; ++i)
            {
                rois[i].Intersect(dstRoi);

                if (rois[i].Width > 0  && rois[i].Height > 0)
                {
                    if (i == 0)
                    {
                        BicubicFitSurfaceUnchecked(source, rois[i]);
                    }
                    else
                    {
                        BicubicFitSurfaceChecked(source, rois[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Implements bicubic filtering with bounds checking at every pixel.
        /// </summary>
        public void BicubicFitSurfaceChecked(Surface source, Rectangle dstRoi)
        {
            if (this.width < 2 || this.height < 2 || source.width < 2 || source.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else 
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);
                    Rectangle roiIn = Rectangle.Intersect(dstRoi, new Rectangle(1, 1, width - 1, height - 1));

                    IntPtr rColCacheIP = Memory.Allocate(4 * (ulong)roi.Width * (ulong)sizeof(float));
                    float *rColCache = (float *)rColCacheIP.ToPointer();

                    // Precompute and then cache the value of R() for each column
                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                        float srcColumnFloor = (float)Math.Floor(srcColumn);
                        float srcColumnFrac = srcColumn - srcColumnFloor;
                        int srcColumnInt = (int)srcColumn;

                        for (int m = -1; m <= 2; ++m)
                        {
                            int index = (m + 1) + ((dstX - roi.Left) * 4);
                            float x = m - srcColumnFrac;
                            rColCache[index] = R(x);
                        }
                    }

                    // Set this up so we can cache the R()'s for every row
                    float *rRowCache = stackalloc float[4];
                
                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        float srcRow = (float)(dstY * (source.height - 1)) / (float)(height - 1);
                        float srcRowFloor = (float)Math.Floor(srcRow);
                        float srcRowFrac = srcRow - srcRowFloor;
                        int srcRowInt = (int)srcRow;
                        ColorBgra *dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                        // Compute the R() values for this row
                        for (int n = -1; n <= 2; ++n)
                        {
                            float x = srcRowFrac - n;
                            rRowCache[n + 1] = R(x);
                        }

                        // See Perf Note below
                        //int nFirst = Math.Max(-srcRowInt, -1);
                        //int nLast = Math.Min(source.height - srcRowInt - 1, 2);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                            float srcColumnFloor = (float)Math.Floor(srcColumn);
                            float srcColumnFrac = srcColumn - srcColumnFloor;
                            int srcColumnInt = (int)srcColumn;

                            float blueSum = 0;
                            float greenSum = 0;
                            float redSum = 0;
                            float alphaSum = 0;
                            float totalWeight = 0;

                            // See Perf Note below
                            //int mFirst = Math.Max(-srcColumnInt, -1);
                            //int mLast = Math.Min(source.width - srcColumnInt - 1, 2);

                            ColorBgra *srcPtr = source.GetPointAddressUnchecked(srcColumnInt - 1, srcRowInt - 1);
                            for (int n = -1; n <= 2; ++n)
                            {
                                int srcY = srcRowInt + n;

                                for (int m = -1; m <= 2; ++m)
                                {
                                    // Perf Note: It actually benchmarks faster on my system to do
                                    // a bounds check for every (m,n) than it is to limit the loop
                                    // to nFirst-Last and mFirst-mLast.
                                    // I'm leaving the code above, albeit commented out, so that
                                    // benchmarking between these two can still be performed.
                                    if (source.IsVisible(srcColumnInt + m, srcY))
                                    {
                                        float w0 = rColCache[(m + 1) + (4 * (dstX - roi.Left))];
                                        float w1 = rRowCache[n + 1];
                                        float w = w0 * w1;

                                        blueSum += srcPtr->B * w;
                                        greenSum += srcPtr->G * w;
                                        redSum += srcPtr->R * w;
                                        alphaSum += srcPtr->A * w;

                                        totalWeight += w;
                                    }

                                    ++srcPtr;
                                }

                                srcPtr = (ColorBgra *)((byte *)(srcPtr - 4) + source.stride);
                            }

                            float totalWeightDiv2 = totalWeight / 2;
                            blueSum += totalWeightDiv2;
                            greenSum += totalWeightDiv2;
                            redSum += totalWeightDiv2;
                            alphaSum += totalWeightDiv2;

                            float blue = blueSum / totalWeight;
                            float green = greenSum / totalWeight;
                            float red = redSum / totalWeight;
                            float alpha = alphaSum / totalWeight;

                            dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                            ++dstPtr;
                        } // for (dstX...
                    } // for (dstY...

                    Memory.Free(rColCacheIP);
                } // unsafe
            }
        }

        /// <summary>
        /// Implements bicubic filtering with NO bounds checking at any pixel.
        /// </summary>
        public void BicubicFitSurfaceUnchecked(Surface source, Rectangle dstRoi)
        {
            if (this.width < 2 || this.height < 2 || source.width < 2 || source.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else 
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);
                    Rectangle roiIn = Rectangle.Intersect(dstRoi, new Rectangle(1, 1, width - 1, height - 1));

                    IntPtr rColCacheIP = Memory.Allocate(4 * (ulong)roi.Width * (ulong)sizeof(float));
                    float *rColCache = (float *)rColCacheIP.ToPointer();

                    // Precompute and then cache the value of R() for each column
                    for (int dstX = roi.Left; dstX < roi.Right; ++dstX)
                    {
                        float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                        float srcColumnFloor = (float)Math.Floor(srcColumn);
                        float srcColumnFrac = srcColumn - srcColumnFloor;
                        int srcColumnInt = (int)srcColumn;

                        for (int m = -1; m <= 2; ++m)
                        {
                            int index = (m + 1) + ((dstX - roi.Left) * 4);
                            float x = m - srcColumnFrac;
                            rColCache[index] = R(x);
                        }
                    }

                    // Set this up so we can cache the R()'s for every row
                    float *rRowCache = stackalloc float[4];
                
                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        float srcRow = (float)(dstY * (source.height - 1)) / (float)(height - 1);
                        float srcRowFloor = (float)Math.Floor(srcRow);
                        float srcRowFrac = srcRow - srcRowFloor;
                        int srcRowInt = (int)srcRow;
                        ColorBgra *dstPtr = this.GetPointAddressUnchecked(roi.Left, dstY);

                        // Compute the R() values for this row
                        for (int n = -1; n <= 2; ++n)
                        {
                            float x = srcRowFrac - n;
                            rRowCache[n + 1] = R(x);
                        }

                        rColCache = (float *)rColCacheIP.ToPointer();
                        ColorBgra *srcRowPtr = source.GetRowAddressUnchecked(srcRowInt - 1);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                            float srcColumnFloor = (float)Math.Floor(srcColumn);
                            float srcColumnFrac = srcColumn - srcColumnFloor;
                            int srcColumnInt = (int)srcColumn;

                            float blueSum = 0;
                            float greenSum = 0;
                            float redSum = 0;
                            float alphaSum = 0;
                            float totalWeight = 0;

                            ColorBgra *srcPtr = srcRowPtr + srcColumnInt - 1;
                            for (int n = 0; n <= 3; ++n)
                            {
                                float w0 = rColCache[0] * rRowCache[n];
                                float w1 = rColCache[1] * rRowCache[n];
                                float w2 = rColCache[2] * rRowCache[n];
                                float w3 = rColCache[3] * rRowCache[n];

                                blueSum += (srcPtr[0].B * w0) + (srcPtr[1].B * w1) + (srcPtr[2].B * w2) + (srcPtr[3].B * w3);
                                greenSum += (srcPtr[0].G * w0) + (srcPtr[1].G * w1) + (srcPtr[2].G * w2) + (srcPtr[3].G * w3);
                                redSum += (srcPtr[0].R * w0) + (srcPtr[1].R * w1) + (srcPtr[2].R * w2) + (srcPtr[3].R * w3);
                                alphaSum += (srcPtr[0].A * w0) + (srcPtr[1].A * w1) + (srcPtr[2].A * w2) + (srcPtr[3].A * w3);
                                totalWeight += w0 + w1 + w2 + w3;

                                srcPtr = (ColorBgra *)((byte *)srcPtr + source.stride);
                            }

                            float totalWeightDiv2 = totalWeight / 2;
                            blueSum += totalWeightDiv2;
                            greenSum += totalWeightDiv2;
                            redSum += totalWeightDiv2;
                            alphaSum += totalWeightDiv2;

                            float blue = blueSum / totalWeight;
                            float green = greenSum / totalWeight;
                            float red = redSum / totalWeight;
                            float alpha = alphaSum / totalWeight;

                            dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                            ++dstPtr;
                            rColCache += 4;
                        } // for (dstX...
                    } // for (dstY...

                    Memory.Free(rColCacheIP);
                } // unsafe
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using bilinear interpolation.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void BilinearFitSurface(Surface source)
        {
            BilinearFitSurface(source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using bilinear interpolation.
        /// </summary>
        /// <param name="source">The surface to read pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <remarks>This method was implemented with correctness, not performance, in mind.</remarks>
        public void BilinearFitSurface(Surface source, Rectangle dstRoi)
        {
            if (dstRoi.Width < 2 || dstRoi.Height < 2 || this.width < 2 || this.height < 2)
            {
                SuperSamplingFitSurface(source, dstRoi);
            }
            else
            {
                unsafe
                {
                    Rectangle roi = Rectangle.Intersect(dstRoi, this.Bounds);

                    for (int dstY = roi.Top; dstY < roi.Bottom; ++dstY)
                    {
                        ColorBgra *dstRowPtr = this.GetRowAddressUnchecked(dstY);
                        float srcRow = (float)(dstY * (source.height - 1)) / (float)(height - 1);

                        for (int dstX = roi.Left; dstX < roi.Right; dstX++)
                        {
                            float srcColumn = (float)(dstX * (source.width - 1)) / (float)(width - 1);
                            *dstRowPtr = source.GetBilinearSample2(srcColumn, srcRow);
                            ++dstRowPtr;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fits the source surface to this surface using the given algorithm.
        /// </summary>
        /// <param name="algorithm">The surface to copy pixels from.</param>
        /// <param name="source">The algorithm to use.</param>
        public void FitSurface(ResamplingAlgorithm algorithm, Surface source)
        {
            FitSurface(algorithm, source, this.Bounds);
        }

        /// <summary>
        /// Fits the source surface to this surface using the given algorithm.
        /// </summary>
        /// <param name="algorithm">The surface to copy pixels from.</param>
        /// <param name="dstRoi">The rectangle to clip rendering to.</param>
        /// <param name="source">The algorithm to use.</param>
        public void FitSurface(ResamplingAlgorithm algorithm, Surface source, Rectangle dstRoi)
        {
            switch (algorithm)
            {
                case ResamplingAlgorithm.Bicubic:
                    BicubicFitSurface(source, dstRoi);
                    break;

                case ResamplingAlgorithm.Bilinear:
                    BilinearFitSurface(source, dstRoi);
                    break;

                case ResamplingAlgorithm.NearestNeighbor:
                    NearestNeighborFitSurface(source, dstRoi);
                    break;
                    
                case ResamplingAlgorithm.SuperSampling:
                    SuperSamplingFitSurface(source, dstRoi);
                    break;

                default:
                    throw new InvalidEnumArgumentException("algorithm");
            }
        }

        private MemoryBlock GetRootMemoryBlock(MemoryBlock block)
        {
            MemoryBlock p = block;

            while (p.Parent != null)
            {
                p = p.Parent;
            }

            return p;
        }

        public void GetDrawBitmapInfo(out IntPtr bitmapHandle, out Point childOffset, out Size parentSize)
        {
            MemoryBlock rootBlock = GetRootMemoryBlock(this.scan0);
            long childOffsetBytes = this.scan0.Pointer.ToInt64() - rootBlock.Pointer.ToInt64();
            int childY = (int)(childOffsetBytes / this.stride);
            int childX = (int)((childOffsetBytes - (childY * this.stride)) / ColorBgra.SizeOf);
            childOffset = new Point(childX, childY);
            parentSize = new Size(this.stride / ColorBgra.SizeOf, childY + this.height);
            bitmapHandle = rootBlock.BitmapHandle;
        }

        #region IDisposable Members
        private bool disposed = false;

        /// <summary>
        /// Releases all resources held by this Surface object.
        /// </summary>
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
