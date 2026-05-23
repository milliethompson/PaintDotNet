/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// These methods are used because we found some bugs in GDI+ / WinForms. Some
	/// were the cause of major flickering with the transparent toolforms.
	/// Other implementations of this class, or more generic implementations, may safely 
	/// thunk straight to equivelants in System.Drawing.Graphics.
	/// </summary>
    public sealed class PdnGraphics
    {
        private PdnGraphics()
        {
        }

        /// <summary>
        /// Loads and returns the application's main icon.
        /// </summary>
        /// <returns>An Icon instance. You must Dispose() this when you no longer need it.</returns>
        /// <remarks>
        /// This will load the icon that is displayed in Explorer for the application. This method
        /// is provided so that the icon can be retrieved without duplicating it as an embedded
        /// resource. This allows the executable size to be smaller.
        /// </remarks>
        public static Icon LoadApplicationIcon()
        {
            IntPtr hModule = SafeNativeMethods.GetModuleHandleW(null);
            IntPtr hIcon = SafeNativeMethods.LoadIconW(hModule, (IntPtr)NativeConstants.IDI_APPLICATION);
            Icon icon = Icon.FromHandle(hIcon);
            SafeNativeMethods.DeleteObject(hIcon);

            return icon;
        }
      
        /// <summary>
        /// Retrieves the properties stored within an Image instance.
        /// </summary>
        /// <param name="image"></param>
        /// <returns>An array containing Object instances that must be cast to System.Drawing.Imaging.PropertyItem instances.</returns>
        /// <remarks>
        /// System.Drawing.Image.get_PropertyItems has a bug where it will throw a null-reference exception
        /// if an image contains an EXIF tag with Len=0. So we reach around its back and get the property
        /// items via GDI+ directly.
        /// See: http://groups-beta.google.com/group/microsoft.public.dotnet.framework.drawing/browse_thread/thread/bb4f6ec70868b3c7/0a5d836c177c932c?q=PropertyItems+GDI%2B+bug&rnum=1#0a5d836c177c932c
        /// </remarks>
        public static object[] GetPropertyItems(Image image)
        {
            // Major HACK: We use reflection to sort of reach behind the Image class' back and snag its 'native' GDI+ handle
            // If Image.get_PropertyItems worked correctly, this function would only have to do the following: return image.PropertyItems;
            Type imageType = image.GetType();
            FieldInfo fi = imageType.GetField("nativeImage", BindingFlags.Instance | BindingFlags.NonPublic);
            object nativeImageObject = fi.GetValue(image);
            IntPtr nativeImage = (IntPtr)nativeImageObject;

            int result;
            int propertyCount;

            result = SafeNativeMethods.GdipGetPropertyCount(nativeImage, out propertyCount);
            if (result != 0)
            {
                return new object[0];
            }

            uint bytes;
            uint count;
            result = SafeNativeMethods.GdipGetPropertySize(nativeImage, out bytes, out count);
            if (result != 0)
            {
                return new object[0];
            }
           
            unsafe 
            {
                IntPtr buffer = IntPtr.Zero;

                try
                {
                    buffer = Memory.Allocate(bytes);
                    result = SafeNativeMethods.GdipGetAllPropertyItems(nativeImage, bytes, count, buffer);

                    object[] properties = new object[count];
                    NativeStructs.PropertyItem *pProperties = (NativeStructs.PropertyItem *)buffer.ToPointer();

                    for (int i = 0; i < count; ++i)
                    {
                        NativeStructs.PropertyItem pi = pProperties[i];
                        byte[] value = new byte[pi.length];

                        for (int j = 0; j < pi.length; ++j)
                        {
                            value[j] = ((byte *)pi.value)[j];
                        }

                        PropertyItem2 pi2 = new PropertyItem2(pi.id, (int)pi.length, pi.type, value);
                        properties[i] = pi2.ToPropertyItem();
                    }

                    return properties;
                }

                finally
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Memory.Free(buffer);
                        buffer = IntPtr.Zero;
                    }

                    GC.KeepAlive(image);
                }
            }

        }

        public static void SetPropertyItems(Image image, PropertyItem[] items)
        {
            PropertyItem[] pis = image.PropertyItems;

            foreach (PropertyItem pi in pis)
            {
                image.RemovePropertyItem(pi.Id);
            }

            foreach (PropertyItem pi in items)
            {
                image.SetPropertyItem(pi);
            }
        }

        public static PropertyItem CreatePropertyItem()
        {
            PropertyItem2 pi2 = new PropertyItem2(0, 0, 0, new byte[0]);
            return pi2.ToPropertyItem();
        }

        public static string SerializePropertyItem(PropertyItem pi)
        {
            PropertyItem2 pi2 = PropertyItem2.FromPropertyItem(pi);
            return pi2.ToBlob();
        }

        public static PropertyItem DeserializePropertyItem(string piBlob)
        {
            PropertyItem2 pi2 = PropertyItem2.FromBlob(piBlob);
            return pi2.ToPropertyItem();
        }

        /// <summary>
        /// Draws a Bitmap onto a Graphics at the specify Rectangle.
        /// </summary>
        /// <param name="dst">The destination surface.</param>
        /// <param name="dstRect">The destination rectangle.</param>
        /// <param name="srcBitmap">The source bitmap.</param>
        /// <remarks>This method uses Win32 GDI functions and avoids flickering.</remarks>
        public static void DrawBitmap(Graphics dst, Rectangle dstRect, Bitmap srcBitmap)
        {
            DrawBitmap(dst, dstRect, srcBitmap, new Point(0, 0));
        }

        /// <summary>
        /// Draws a Bitmap onto a Graphics at the specified Rectangle.
        /// </summary>
        /// <param name="dst">The destination surface.</param>
        /// <param name="dstRect">The destination rectangle.</param>
        /// <param name="srcBitmap">The source bitmap.</param>
        /// <remarks>This method uses Win32 GDI functions and avoids flickering.</remarks>
        public static void DrawBitmap(Graphics dst, Rectangle dstRect, Bitmap srcBitmap, Point srcOffset)
        {
            Matrix dstMatrix = dst.Transform;
            DrawBitmap(dst, dstRect, dstMatrix, srcBitmap, srcOffset);
        }
		
        public static void DrawBitmap(Graphics dst, Rectangle dstRect, Matrix dstMatrix, Bitmap srcBitmap, Point srcOffset)
        {
            Point[] points = new Point[] { dstRect.Location };
            dstMatrix.TransformPoints(points);
            dstRect.Location = points[0];

            IntPtr hdc = IntPtr.Zero;
            IntPtr hbitmap = IntPtr.Zero;
            IntPtr chdc = IntPtr.Zero;
            IntPtr old = IntPtr.Zero;

            try
            {
                hdc = dst.GetHdc();
                hbitmap = srcBitmap.GetHbitmap();
                chdc = SafeNativeMethods.CreateCompatibleDC(hdc);
                old = SafeNativeMethods.SelectObject(chdc, hbitmap);
                SafeNativeMethods.BitBlt(hdc, dstRect.Left, dstRect.Top, dstRect.Width, dstRect.Height, chdc, srcOffset.X, srcOffset.Y, NativeConstants.SRCCOPY);
            }

            finally
            {
                if (old != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(chdc, old);
                    old = IntPtr.Zero;
                }

                if (chdc != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteDC(chdc);
                    chdc = IntPtr.Zero;
                }
			
                if (hbitmap != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(hbitmap);
                    hbitmap = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    dst.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Draws a Bitmap onto a Graphics at the specify Rectangle, and stretches to fit using nearest neighbor resampling.
        /// </summary>
        /// <param name="dst">The destination surface.</param>
        /// <param name="dstRect">The destination rectangle.</param>
        /// <param name="srcBitmap">The source bitmap.</param>
        /// <param name="srcRect">The source rectangle.</param>
        /// <remarks>This method uses Win32 GDI functions and avoids flickering.</remarks>
        public static void DrawBitmap(Graphics dst, Rectangle dstRect, Bitmap srcBitmap, Rectangle srcRect)
        {
            Point[] points = new Point[] { dstRect.Location };
            dst.Transform.TransformPoints(points);
            dstRect.Location = points[0];

            IntPtr hdc = IntPtr.Zero;
            IntPtr hbitmap = IntPtr.Zero;
            IntPtr chdc = IntPtr.Zero;
            IntPtr old = IntPtr.Zero;

            try
            {
                hdc = dst.GetHdc();
                hbitmap = srcBitmap.GetHbitmap();
                chdc = SafeNativeMethods.CreateCompatibleDC(hdc);
                old = SafeNativeMethods.SelectObject(chdc, hbitmap);

                SafeNativeMethods.SetStretchBltMode(hdc, NativeConstants.COLORONCOLOR);
                SafeNativeMethods.StretchBlt(hdc, dstRect.Left, dstRect.Top, dstRect.Width, dstRect.Height,
                    chdc, srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, NativeConstants.SRCCOPY);
            }

            finally
            {
                if (old != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(chdc, old);
                    old = IntPtr.Zero;
                }

                if (chdc != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteDC(chdc);
                    chdc = IntPtr.Zero;
                }
			
                if (hbitmap != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(hbitmap);
                    hbitmap = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    dst.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }
        }
        
        public static void DrawGrid(Graphics g, Rectangle rect, PointFPointFDelegate surfaceToClient)
        {
            IntPtr hdc = IntPtr.Zero;
            IntPtr pen = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            PointF topLeft = new PointF(rect.Left, rect.Top);
            PointF topLeftSurface = surfaceToClient(topLeft);
            PointF topLeftRight1 = new PointF(rect.Left + 1, rect.Top);
            PointF topLeftRight1Surface = surfaceToClient(topLeftRight1);
            PointF topLeftDown1 = new PointF(rect.Left, rect.Top + 1);
            PointF topLeftDown1Surface = surfaceToClient(topLeftDown1);

            float dx = topLeftRight1Surface.X - topLeftSurface.X;
            float dy = topLeftDown1Surface.Y - topLeftSurface.Y;

            // We use some native Win32 GDI voodoo to make the grid drawing waaaaaaaay faster
            try
            {
                using (Matrix matrix = g.Transform)
                {                
                    hdc = g.GetHdc();
                    float xOffset = matrix.OffsetX;
                    float yOffset = matrix.OffsetY;

                    NativeStructs.LOGBRUSH logbrush = new NativeStructs.LOGBRUSH();
                    logbrush.lbColor = 0xff808080;
                    logbrush.lbHatch = 0;
                    logbrush.lbStyle = NativeConstants.BS_SOLID;

                    unsafe
                    {
                        pen = SafeNativeMethods.ExtCreatePen(NativeConstants.PS_COSMETIC | NativeConstants.PS_ALTERNATE, 1, ref logbrush, 0, null);
                    }

                    oldObject = SafeNativeMethods.SelectObject(hdc, pen);
                    NativeStructs.POINT point; // not used except as 'out' param for MoveToEx

                    //for (int x = rect.Left; x <= rect.Right; x++) 
                    for (int x = 0; x <= rect.Width; ++x)
                    {
                        PointF start = new PointF(xOffset + topLeftSurface.X + (x * dx), yOffset + topLeftSurface.Y);
                        PointF end = new PointF(start.X, yOffset + topLeftSurface.Y + (rect.Height * dy));
                        Point clientStart = Point.Truncate(start);
                        Point clientEnd = Point.Truncate(end);

                        if (0 == SafeNativeMethods.MoveToEx(hdc, clientStart.X, clientStart.Y, out point))
                        {
                            NativeMethods.ThrowOnWin32Error();
                        }

                        if (0 == SafeNativeMethods.LineTo(hdc, clientEnd.X, clientEnd.Y))
                        {
                            NativeMethods.ThrowOnWin32Error();
                        }
                    }

                    for (int y = 0; y <= rect.Height; ++y)
                    {
                        PointF start = new PointF(xOffset + topLeftSurface.X, yOffset + topLeftSurface.Y + (y * dy));
                        PointF end = new PointF(xOffset + topLeftSurface.X + (rect.Width * dx), start.Y);
                        Point clientStart = Point.Truncate(start);
                        Point clientEnd = Point.Truncate(end);

                        if (0 == SafeNativeMethods.MoveToEx(hdc, clientStart.X, clientStart.Y, out point))
                        {
                            NativeMethods.ThrowOnWin32Error();
                        }

                        if (0 == SafeNativeMethods.LineTo(hdc, clientEnd.X, clientEnd.Y))
                        {
                            NativeMethods.ThrowOnWin32Error();
                        }
                    }
                }
            }

            finally
            {
                if (oldObject != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(hdc, oldObject);
                    oldObject = IntPtr.Zero;
                }

                if (pen != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(pen);
                    pen = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    g.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }        
        }

        private const int screwUpMax = 10;

        /// <summary>
        /// Retrieves an array of rectangles that approximates a region, and computes the
        /// pixel area of it. This method is necessary to work around some bugs in .NET
        /// and to increase performance for the way in which we typically use this data.
        /// Generic implementations may safely thunk to Region.GetRegionScans().
        /// </summary>
        /// <param name="region">The Region to retrieve data from.</param>
        /// <param name="scans">An array of Rectangle to put the scans into.</param>
        /// <param name="area">An integer to write the computed area of the region into.</param>
        public static void GetRegionScans(Region region, out Rectangle[] scans, out int area)
        {
            unsafe
            {
                using (NullGraphics nullGraphics = new NullGraphics())
                {
                    IntPtr hRgn = IntPtr.Zero;
                
                    try
                    {
                        hRgn = region.GetHrgn(nullGraphics.Graphics);
                        HandleRef hRgnRef = new HandleRef(region, hRgn);

                        uint bytes = 0;
                        int countdown = screwUpMax;
                    
                        // HACK: It seems that someimtes the GetRegionData will return ERROR_INVALID_HANDLE
                        //       even though the handle (the HRGN) is fine. Maybe the function is not
                        //       re-entrant? I'm not sure, but trying it again seems to fix it.
                        while (countdown > 0)
                        {
                            bytes = SafeNativeMethods.GetRegionData(hRgnRef, 0, (NativeStructs.RGNDATA *)IntPtr.Zero);

                            if (bytes == 0)
                            {
                                --countdown;
                                System.Threading.Thread.Sleep(5);
                            }
                            else
                            {
                                break;
                            }
                        }

                        // But if we retry several times and it still messes up then we will finally give up.
                        if (bytes == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw new Win32Exception(error, "GetRegionData returned " + bytes.ToString() + ", GetLastError() = " + error.ToString());
                        }

                        byte *data;
                        
                        if (bytes <= 512)
                        {
                            byte *data1 = stackalloc byte[(int)bytes];
                            data = data1;
                        }
                        else
                        {
                            data = (byte *)Memory.Allocate(bytes).ToPointer();
                        }                        

                        NativeStructs.RGNDATA *pRgnData = (NativeStructs.RGNDATA *)data;
                        uint result = SafeNativeMethods.GetRegionData(hRgnRef, bytes, pRgnData);

                        if (result != bytes)
                        {
                            throw new OutOfMemoryException("SafeNativeMethods.GetRegionData returned 0");
                        }

                        NativeStructs.RECT *pRects = NativeStructs.RGNDATA.GetRectsPointer(pRgnData);
                        scans = new Rectangle[pRgnData->rdh.nCount];
                        area = 0;
            
                        for (int i = 0; i < scans.Length; ++i)
                        {
                            scans[i] = Rectangle.FromLTRB(pRects[i].left, pRects[i].top, pRects[i].right, pRects[i].bottom);
                            area += scans[i].Width * scans[i].Height;
                        }

                        pRects = null;
                        pRgnData = null;

                        if (bytes > 512)
                        {
                            Memory.Free(new IntPtr(data));
                        }
                    }

                    finally
                    {
                        if (hRgn != IntPtr.Zero)
                        {
                            SafeNativeMethods.DeleteObject(hRgn);
                        }
                    }
                }
            }

            GC.KeepAlive(region);
        }

        /// <summary>
        /// Draws a polygon. The last point is not joined to the beginning point.
        /// </summary>
        /// <param name="g">The Graphics context to draw to.</param>
        /// <param name="points">The points to draw. Lines are drawn between every point N to point N+1.</param>
        /// <param name="color">The color to draw with.</param>
        public static void DrawPolyLine(Graphics g, Color color, Point[] points)
        {
            if (points.Length < 1)
            {
                return;
            }

            uint nativeColor = (uint)(color.R  + (color.G << 8) + (color.B << 16));

            IntPtr hdc = IntPtr.Zero;
            IntPtr pen = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            try
            {
                hdc = g.GetHdc();
                pen = SafeNativeMethods.CreatePen(NativeConstants.PS_SOLID, 1, nativeColor);

                if (pen == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error();
                }

                oldObject = SafeNativeMethods.SelectObject(hdc, pen);

                NativeStructs.POINT pt;
                uint result = SafeNativeMethods.MoveToEx(hdc, points[0].X, points[0].Y, out pt);
                
                if (result == 0)
                {
                    NativeMethods.ThrowOnWin32Error();
                }

                for (int i = 1; i < points.Length; ++i)
                {
                    result = SafeNativeMethods.LineTo(hdc, points[i].X, points[i].Y);

                    if (result == 0)
                    {
                        NativeMethods.ThrowOnWin32Error();
                    }
                }
            }

            finally
            {
                if (oldObject != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(hdc, oldObject);
                    oldObject = IntPtr.Zero;
                }

                if (pen != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(pen);
                    pen = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    g.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// Draws several filled rectangles using the same color.
        /// </summary>
        /// <param name="g">The Graphics context to draw to.</param>
        /// <param name="rects">A list of rectangles to draw.</param>
        /// <param name="color">The color to fill the rectangles with.</param>
        public static void FillRectangles(Graphics g, Color color, Rectangle[] rects)
        {
            uint nativeColor = (uint)(color.R  + (color.G << 8) + (color.B << 16));

            IntPtr hdc = IntPtr.Zero;
            IntPtr brush = IntPtr.Zero;
            IntPtr oldObject = IntPtr.Zero;

            try
            {
                hdc = g.GetHdc();
                brush = SafeNativeMethods.CreateSolidBrush(nativeColor);

                if (brush == IntPtr.Zero)
                {
                    NativeMethods.ThrowOnWin32Error();
                }

                oldObject = SafeNativeMethods.SelectObject(hdc, brush);

                foreach (Rectangle rect in rects)
                {
                    NativeStructs.RECT nativeRect;

                    nativeRect.left = rect.Left;
                    nativeRect.top = rect.Top;
                    nativeRect.right = rect.Right;
                    nativeRect.bottom = rect.Bottom;

                    int result = SafeNativeMethods.FillRect(hdc, ref nativeRect, brush);

                    if (result == 0)
                    {
                        NativeMethods.ThrowOnWin32Error();
                    }
                }
            }

            finally
            {
                if (oldObject != IntPtr.Zero)
                {
                    SafeNativeMethods.SelectObject(hdc, oldObject);
                    oldObject = IntPtr.Zero;
                }

                if (brush != IntPtr.Zero)
                {
                    SafeNativeMethods.DeleteObject(brush);
                    brush = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    g.ReleaseHdc(hdc);
                    hdc = IntPtr.Zero;
                }
            }
        }
	}
}

