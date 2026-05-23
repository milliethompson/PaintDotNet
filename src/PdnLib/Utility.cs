/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using PaintDotNet.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Defines miscellaneous constants and static functions.
    /// </summary>
    public sealed class Utility
    {
        private Utility()
        {
        }

        public static readonly Color TransparentKey = Color.FromArgb(192, 192, 192);

        private static DateTime startTime = DateTime.Now;
        private static DateTime lastTime = DateTime.Now;

        public static bool IsDotNetVersionInstalled(int major, int minor, int build)
        {
            const string regKeyNameFormat = "Software\\Microsoft\\NET Framework Setup\\NDP\\v{0}.{1}.{2}";
            const string regValueName = "Install";

            string regKeyName = string.Format(regKeyNameFormat, major.ToString(CultureInfo.InvariantCulture),
                minor.ToString(CultureInfo.InvariantCulture), build.ToString(CultureInfo.InvariantCulture));

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regKeyName, false))
            {
                object value = null;

                if (key != null)
                {
                    value = key.GetValue(regValueName);
                }

                return (value != null && value is int && (int)value == 1);
            }
        }

        public static void GCFullCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static int defaultSimplificationFactor = 50;
        public static int DefaultSimplificationFactor
        {
            get
            {
                return defaultSimplificationFactor;
            }

            set
            {
                defaultSimplificationFactor = value;
            }
        }

        public static bool IsArrowKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;

            if (key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool DoesControlHaveMouseCaptured(Control control)
        {
            bool result = false;

            result |= control.Capture;

            foreach (Control c in control.Controls)
            {
                result |= DoesControlHaveMouseCaptured(c);
            }

            return result;
        }

        public static void SplitRectangle(Rectangle rect, Rectangle[] rects)
        {
            int height = rect.Height;

            for (int i = 0; i < rects.Length; ++i)
            {
                Rectangle newRect = Rectangle.FromLTRB(rect.Left,
                                                       rect.Top + ((height * i) / rects.Length),
                                                       rect.Right,
                                                       rect.Top + ((height * (i + 1)) / rects.Length));

                rects[i] = newRect;
            }
        }

        public static long TicksToMs(long ticks)
        {
            return ticks / 10000;
        }

        public static string GetStaticName(Type type)
        {
            PropertyInfo pi = type.GetProperty("StaticName", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            return (string)pi.GetValue(null, null);
        }

        public static readonly float[][] Identity5x5F = new float[][] {
                                                                          new float[] { 1, 0, 0, 0, 0 },
                                                                          new float[] { 0, 1, 0, 0, 0 },
                                                                          new float[] { 0, 0, 1, 0, 0 },
                                                                          new float[] { 0, 0, 0, 1, 0 },
                                                                          new float[] { 0, 0, 0, 0, 1 } 
                                                                      };

        public static readonly ColorMatrix IdentityColorMatrix = new ColorMatrix(Identity5x5F);

        [ThreadStatic]
        private static Matrix identityMatrix = null;
        public static Matrix IdentityMatrix
        {
            get
            {
                if (identityMatrix == null)
                {
                    identityMatrix = new Matrix();
                    identityMatrix.Reset();
                }

                return identityMatrix;
            }
        }

        /// <summary>
        /// Rounds an integer to the smallest power of 2 that is greater
        /// than or equal to it.
        /// </summary>
        public static int Log2RoundUp(int x)
        {
            if (x == 0)
            {
                return 1;
            }

            if (x == 1)
            {
                return 1;
            }

            return 1 << (1 + HighestBit(x - 1));
        }

        private static int HighestBit(int x)
        {
            if (x == 0)
            {
                return 0;
            }

            int b = 0;
            int hi = 0;

            while (b <= 30)
            {
                if ((x & (1 << b)) != 0)
                {
                    hi = b;
                }

                ++b;
            }

            return hi;
        }

        private int CountBits(int x)
        {
            uint y = (uint)x;
            int count = 0;

            for (int bit = 0; bit < 32; ++bit)
            {
                if ((y & ((uint)1 << bit)) != 0)
                {
                    ++count;
                }
            }

            return count;
        }

        public static string RemoveSpaces(string s)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in s)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
        
        public static int Max(int[,] array)
        {
            int max = int.MinValue;

            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                {
                    if (array[i,j] > max)
                    {
                        max = array[i,j];
                    }
                }
            }

            return max;
        }

        public static int Sum(int[][] array)
        {
            int sum = 0;

            for (int i = 0; i < array.Length; ++i)
            {
                int[] row = array[i];

                for (int j = 0; j < row.Length; ++j)
                {
                    sum += row[j];
                }
            }

            return sum;
        }

        public static void ClipNumericUpDown(NumericUpDown upDown)
        {
            if (upDown.Value < upDown.Minimum)
            {
                upDown.Value = upDown.Minimum;
            }
            else if (upDown.Value > upDown.Maximum)
            {
                upDown.Value = upDown.Maximum;
            }
        }

        public static bool GetUpDownValueFromText(NumericUpDown nud, out double val)
        {
            if (nud.Text == string.Empty)
            {
                val = 0;
                return false;
            }
            else
            {
                try
                {
                    if (nud.DecimalPlaces == 0)
                    {
                        val = (double)int.Parse(nud.Text);
                    }
                    else
                    {
                        val = double.Parse(nud.Text);
                    }
                }

                catch
                {
                    val = 0;
                    return false;
                }

                return true;
            }
        }

        public static bool CheckNumericUpDown(NumericUpDown upDown)
        {
            int a;
        
            try
            {
                a = int.Parse(upDown.Text);
            }

            catch (FormatException)
            {
                return false;
            }

            catch (OverflowException)
            {
                return false;
            }

            if ((a <= (int)upDown.Maximum) && (a >= (int)upDown.Minimum))
            {
                return true;
            }   
            else
            {
                return false;
            }
        }

        public static void SetNumericUpDownValue(NumericUpDown upDown, decimal newValue)
        {
            if (upDown.Value != newValue)
            {
                upDown.Value = newValue;
            }
        }

        public static void SetNumericUpDownValue(NumericUpDown upDown, int newValue)
        {
            SetNumericUpDownValue(upDown, (decimal)newValue);
        }

        public static string SizeStringFromBytes(long bytes)
        {
            string returnMe;
            double bytesDouble = (double)bytes;

            if (bytesDouble > (1024 * 1024 * 1024))
            {
                // Gigs
                bytesDouble /= 1024 * 1024 * 1024;
                returnMe = bytesDouble.ToString("F1") + " GB";
            }
            else if (bytesDouble > (1024 * 1024))
            {
                // Megs
                bytesDouble /= 1024 * 1024;
                returnMe = bytesDouble.ToString("F1") + " MB";
            }
            else if (bytesDouble > (1024))
            {
                // K
                bytesDouble /= 1024;
                returnMe = bytesDouble.ToString("F1") + " KB";
            }
            else
            {
                // Bytes
                returnMe = bytesDouble.ToString("F0") + " Bytes";
            }

            return returnMe;
        }

        public static void ShowNonAdminErrorBox(IWin32Window parent)
        {
            ErrorBox(parent, PdnResources.GetString("NonAdminErrorBox.Message"));
        }

        public static void ErrorBox(IWin32Window parent, string message)
        {
            MessageBox.Show(parent, message, PdnInfo.GetProductName(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ErrorBoxOKCancel(IWin32Window parent, string message)
        {
            return MessageBox.Show(parent, message, PdnInfo.GetProductName(), MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
        }

        public static void InfoBox(IWin32Window parent, string message)
        {
            MessageBox.Show(parent, message, PdnInfo.GetProductName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult InfoBoxOKCancel(IWin32Window parent, string message)
        {
            return MessageBox.Show(parent, message, PdnInfo.GetProductName(), MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        public static DialogResult AskOKCancel(IWin32Window parent, string question)
        {
            return MessageBox.Show(parent, question, PdnInfo.GetProductName(), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }

        public static DialogResult AskYesNo(IWin32Window parent, string question)
        {
            return MessageBox.Show(parent, question, PdnInfo.GetProductName(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static DialogResult AskYesNoCancel(IWin32Window parent, string question)
        {
            return MessageBox.Show(parent, question, PdnInfo.GetProductName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        public static Icon ImageToIcon(Image image)
        {
            return ImageToIcon(image, Utility.TransparentKey);
        }

        public static Icon ImageToIcon(Image image, bool disposeImage)
        {
            return ImageToIcon(image, Utility.TransparentKey, disposeImage);
        }

        public static Icon ImageToIcon(Image image, Color seeThru)
        {
            return ImageToIcon(image, seeThru, false);
        }

        /// <summary>
        /// Converts an Image to an Icon.
        /// </summary>
        /// <param name="image">The Image to convert to an icon. Must be an appropriate icon size (32x32, 16x16, etc).</param>
        /// <param name="seeThru">The color that will be treated as transparent in the icon.</param>
        /// <param name="disposeImage">Whether or not to dispose the passed-in Image.</param>
        /// <returns>An Icon representation of the Image.</returns>
        public static Icon ImageToIcon(Image image, Color seeThru, bool disposeImage)
        {
            Bitmap bitmap = new Bitmap(image);

            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    if (bitmap.GetPixel(x, y) == seeThru)
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(0));
                    }
                }
            }

            Icon icon = Icon.FromHandle(bitmap.GetHicon());
            bitmap.Dispose();

            if (disposeImage)
            {
                image.Dispose();
            }

            return icon;
        }

        public static Icon BitmapToIcon(Bitmap bitmap, bool disposeBitmap)
        {
            Icon icon = Icon.FromHandle(bitmap.GetHicon());

            if (disposeBitmap)
            {
                bitmap.Dispose();
            }

            return icon;
        }

        public static Icon SurfaceToIcon(Surface surface, bool disposeSurface)
        {
            Bitmap bitmap = surface.CreateAliasedBitmap();
            Icon icon = Icon.FromHandle(bitmap.GetHicon());

            bitmap.Dispose();

            if (disposeSurface)
            {
                surface.Dispose();
            }

            return icon;
        }

        private static Assembly mainAssembly;
        private static Assembly MainAssembly
        {
            get
            {
                if (mainAssembly == null)
                {
                    mainAssembly = Assembly.GetEntryAssembly();
                }

                return mainAssembly;
            }
        }

        public static Point GetRectangleCenter(Rectangle rect)
        {
            return new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        }

        public static PointF GetRectangleCenter(RectangleF rect)
        {
            return new PointF((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        }

        public static Scanline[] GetRectangleScans(Rectangle rect)
        {
            Scanline[] scans = new Scanline[rect.Height];

            for (int y = 0; y < rect.Height; ++y)
            {
                scans[y] = new Scanline(rect.X, rect.Y + y, rect.Width);
            }

            return scans;
        }

        public static Scanline[] GetRegionScans(Rectangle[] region)
        {
            int scanCount = 0;

            for (int i = 0; i < region.Length; ++i)
            {
                scanCount += region[i].Height;
            }

            Scanline[] scans = new Scanline[scanCount];
            int scanIndex = 0;

            foreach (Rectangle rect in region)
            {
                for (int y = 0; y < rect.Height; ++y)
                {
                    scans[scanIndex] = new Scanline(rect.X, rect.Y + y, rect.Width);
                    ++scanIndex;
                }
            }

            return scans;
        }

        public static Rectangle[] ScanlinesToRectangles(Scanline[] scans)
        {
            return ScanlinesToRectangles(scans, 0, scans.Length);
        }

        public static Rectangle[] ScanlinesToRectangles(Scanline[] scans, int startIndex, int length)
        {
            Rectangle[] rects = new Rectangle[length];

            for (int i = 0; i < length; ++i)
            {
                Scanline scan = scans[i + startIndex];
                rects[i] = new Rectangle(scan.X, scan.Y, scan.Length, 1);
            }

            return rects;
        }

        /// <summary>
        /// Found on Google Groups when searching for "Region.Union" while looking
        /// for bugs:
        /// ---
        /// Hello,
        /// 
        /// I did not run your code, but I know Region.Union is flawed in both 1.0 and
        /// 1.1, so I assume it is in the gdi+ unmanged code dll.  The best workaround,
        /// in terms of speed, is to use a PdnGraphicsPath, but it must be a path with
        /// FillMode = FillMode.Winding. You add the rectangles to the path, then you do
        /// union onto an empty region with the path. The important point is to do only
        /// one union call on a given empty region. We created a "super region" object
        /// to hide all these bugs and optimize clipping operations. In fact, it is much
        /// faster to use the path than to call Region.Union for each rectangle.
        /// 
        /// Too bad about Region.Union. A lot of people will hit this bug, as it is
        /// essential in high-performance animation.
        /// 
        /// Regards,
        /// Frank Hileman
        /// Prodige Software Corporation
        /// ---
        /// </summary>
        /// <param name="rectsF"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static PdnRegion RectanglesToRegion(RectangleF[] rectsF, int startIndex, int length)
        {
            PdnRegion region;

            if (rectsF == null || rectsF.Length == 0 || length == 0)
            {
                region = PdnRegion.CreateEmpty();
            }
            else
            {
                using (PdnGraphicsPath path = new PdnGraphicsPath())
                {
                    path.FillMode = FillMode.Winding;

                    if (startIndex == 0 && length == rectsF.Length)
                    {
                        path.AddRectangles(rectsF);
                    }
                    else
                    {
                        for (int i = startIndex; i < startIndex + length; ++i)
                        {
                            path.AddRectangle(rectsF[i]);
                        }
                    }

                    region = new PdnRegion(path);
                }
            }

            return region;
        }

        public static PdnRegion RectanglesToRegion(RectangleF[] rectsF)
        {
            return RectanglesToRegion(rectsF, 0, rectsF != null ? rectsF.Length : 0);
        }

        public static PdnRegion RectanglesToRegion(RectangleF[] rectsF1, RectangleF[] rectsF2, params RectangleF[][] rectsFA)
        {
            using (PdnGraphicsPath path = new PdnGraphicsPath())
            {
                path.FillMode = FillMode.Winding;

                if (rectsF1 != null && rectsF1.Length > 0)
                {
                    path.AddRectangles(rectsF1);
                }

                if (rectsF2 != null && rectsF2.Length > 0)
                {
                    path.AddRectangles(rectsF2);
                }
               
                foreach (RectangleF[] rectsF in rectsFA)
                {
                    if (rectsF != null && rectsF.Length > 0)
                    {
                        path.AddRectangles(rectsF);
                    }
                }

                return new PdnRegion(path);
            }
        }

        public static PdnRegion RectanglesToRegion(Rectangle[] rects, int startIndex, int length)
        {
            PdnRegion region;

            if (length == 0)
            {
                region = PdnRegion.CreateEmpty();
            }
            else
            {
                using (PdnGraphicsPath path = new PdnGraphicsPath())
                {
                    path.FillMode = FillMode.Winding;
                    if (startIndex == 0 && length == rects.Length)
                    {
                        path.AddRectangles(rects);
                    }
                    else
                    {
                        for (int i = startIndex; i < startIndex + length; ++i)
                        {
                            path.AddRectangle(rects[i]);
                        }
                    }

                    region = new PdnRegion(path);
                    path.Dispose();
                }
            }

            return region;
        }

        public static PdnRegion RectanglesToRegion(Rectangle[] rects)
        {
            return RectanglesToRegion(rects, 0, rects.Length);
        }

        public static int GetRegionArea(RectangleF[] rectsF)
        {
            int area = 0;

            foreach (RectangleF rectF in rectsF)
            {
                Rectangle rect = Rectangle.Truncate(rectF);
                area += rect.Width * rect.Height;
            }

            return area;
        }

        public static RectangleF RectangleFromCenter(PointF center, float halfSize) 
        {
            RectangleF ret = new RectangleF(center.X, center.Y, 0, 0);
            ret.Inflate(halfSize, halfSize);
            return ret;
        }

        public static List<PointF> PointListToPointFList(List<Point> ptList)
        {
            List<PointF> ret = new List<PointF>(ptList.Count);

            for (int i = 0; i < ptList.Count; ++i)
            {
                ret.Add((PointF)ptList[i]);
            }

            return ret;
        }

        public static PointF[] PointArrayToPointFArray(Point[] ptArray)
        {
            PointF[] ret = new PointF[ptArray.Length];

            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = (PointF)ptArray[i];
            }

            return ret;
        }

        public static Rectangle[] InflateRectangles(Rectangle[] rects, int amount)
        {
            Rectangle[] inflated = new Rectangle[rects.Length];

            for (int i = 0; i < rects.Length; ++i)
            {
                inflated[i] = Rectangle.Inflate(rects[i], amount, amount);
            }

            return inflated;
        }

        public static void InflateRectanglesInPlace(Rectangle[] rects, int amount)
        {
            for (int i = 0; i < rects.Length; ++i)
            {
                rects[i].Inflate(amount, amount);
            }
        }

        public static RectangleF[] InflateRectangles(RectangleF[] rectsF, int amount)
        {
            RectangleF[] inflated = new RectangleF[rectsF.Length];

            for (int i = 0; i < rectsF.Length; ++i)
            {
                inflated[i] = RectangleF.Inflate(rectsF[i], amount, amount);
            }

            return inflated;
        }

        public static void InflateRectanglesInPlace(RectangleF[] rectsF, float amount)
        {
            for (int i = 0; i < rectsF.Length; ++i)
            {
                rectsF[i].Inflate(amount, amount);
            }
        }

        public static Rectangle PointsToConstrainedRectangle(Point a, Point b)
        {
            Rectangle rect = Utility.PointsToRectangle(a, b);
            int minWH = Math.Min(rect.Width, rect.Height);

            rect.Width = minWH;
            rect.Height = minWH;

            if (rect.Y != a.Y)
            {
                rect.Location = new Point(rect.X, a.Y - minWH);
            }

            if (rect.X != a.X)
            {
                rect.Location = new Point(a.X - minWH, rect.Y);
            }

            return rect;
        }

        public static RectangleF PointsToConstrainedRectangle(PointF a, PointF b)
        {
            RectangleF rect = Utility.PointsToRectangle(a, b);
            float minWH = Math.Min(rect.Width, rect.Height);

            rect.Width = minWH;
            rect.Height = minWH;

            if (rect.Y != a.Y)
            {
                rect.Location = new PointF(rect.X, a.Y - minWH);
            }

            if (rect.X != a.X)
            {
                rect.Location = new PointF(a.X - minWH, rect.Y);
            }

            return rect;
        }

        /// <summary>
        /// Takes two points and creates a bounding rectangle from them.
        /// </summary>
        /// <param name="a">One corner of the rectangle.</param>
        /// <param name="b">The other corner of the rectangle.</param>
        /// <returns>A Rectangle instance that bounds the two points.</returns>
        public static Rectangle PointsToRectangle(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            int width = Math.Abs(a.X - b.X) + 1;
            int height = Math.Abs(a.Y - b.Y) + 1;
 
            return new Rectangle(x, y, width, height);
        }

        public static RectangleF PointsToRectangle(PointF a, PointF b)
        {
            float x = Math.Min(a.X, b.X);
            float y = Math.Min(a.Y, b.Y);
            float width = Math.Abs(a.X - b.X) + 1;
            float height = Math.Abs(a.Y - b.Y) + 1;
 
            return new RectangleF(x, y, width, height);
        }

        public static Rectangle PointsToRectangleExclusive(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            int width = Math.Abs(a.X - b.X);
            int height = Math.Abs(a.Y - b.Y);
 
            return new Rectangle(x, y, width, height);
        }

        public static RectangleF PointsToRectangleExclusive(PointF a, PointF b)
        {
            float x = Math.Min(a.X, b.X);
            float y = Math.Min(a.Y, b.Y);
            float width = Math.Abs(a.X - b.X);
            float height = Math.Abs(a.Y - b.Y);
 
            return new RectangleF(x, y, width, height);
        }

        public static RectangleF[] PointsToRectangles(PointF[] pointsF)
        {
            if (pointsF.Length == 0)
            {
                return new RectangleF[] { };
            }

            if (pointsF.Length == 1)
            {
                return new RectangleF[] { new RectangleF(pointsF[0].X, pointsF[0].Y, 1, 1) };
            }

            RectangleF[] rectsF = new RectangleF[pointsF.Length - 1];

            for (int i = 0; i < pointsF.Length - 1; ++i)
            {
                rectsF[i] = PointsToRectangle(pointsF[i], pointsF[i + 1]);
            }

            return rectsF;
        }

        public static Rectangle[] PointsToRectangles(Point[] points)
        {
            if (points.Length == 0)
            {
                return new Rectangle[] { };
            }

            if (points.Length == 1)
            {
                return new Rectangle[] { new Rectangle(points[0].X, points[0].Y, 1, 1) };
            }

            Rectangle[] rects = new Rectangle[points.Length - 1];

            for (int i = 0; i < points.Length - 1; ++i)
            {
                rects[i] = PointsToRectangle(points[i], points[i + 1]);
            }

            return rects;
        }

        /// <summary>
        /// Converts a RectangleF to RectangleF by rounding down the Location and rounding
        /// up the Size.
        /// </summary>
        public static Rectangle RoundRectangle(RectangleF rectF)
        {
            float left = (float)Math.Floor(rectF.Left);
            float top = (float)Math.Floor(rectF.Top);
            float right = (float)Math.Ceiling(rectF.Right);
            float bottom = (float)Math.Ceiling(rectF.Bottom);
            
            return Rectangle.Truncate(RectangleF.FromLTRB(left, top, right, bottom));
        }

        public static Stack Reverse(Stack reverseMe)
        {
            Stack reversed = new Stack();

            foreach (object o in reverseMe)
            {
                reversed.Push(o);
            }

            return reversed;
        }

        public static void SerializeObjectToStream(object graph, Stream stream) 
        {
            new BinaryFormatter().Serialize(stream, graph);
        }

        public static object DeserializeObjectFromStream(Stream stream)
        {
            return new BinaryFormatter().Deserialize(stream);
        }

        public static bool IsPointInRectangle(Point pt, Rectangle rect)
        {
            return IsPointInRectangle(pt.X, pt.Y, rect);
        }
        
        public static bool IsPointInRectangle(int x, int y, Rectangle rect)
        {
            if ((x < rect.X) || (y < rect.Y) || (x >= rect.Right) || (y >= rect.Bottom))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Disposes an object for you. This function is here just to keep code a little
        /// cleaner so you don't have to test an object for null every time you want to
        /// dispose it.
        /// </summary>
        /// <param name="obj">A reference to the object to dispose.</param>
        /// <returns>true is the object was disposed, false if it wasn't (if obj was null)</returns>
        public static bool Dispose (IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                return true;
            }

            return false;
        }

        public static Bitmap FullCloneBitmap(Bitmap cloneMe)
        {
            Bitmap bitmap = new Bitmap(cloneMe.Width, cloneMe.Height, cloneMe.PixelFormat);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(cloneMe, 0, 0, cloneMe.Width, cloneMe.Height);
            }            

            return bitmap;
        }

        /// <summary>
        /// Allows you to find the bounding box for a Region object without requiring
        /// the presence of a Graphics object.
        /// (Region.GetBounds takes a Graphics instance as its only parameter.)
        /// </summary>
        /// <param name="region">The region you want to find a bounding box for.</param>
        /// <returns>A RectangleF structure that surrounds the Region.</returns>
        public static Rectangle GetRegionBounds(PdnRegion region)
        {
            Rectangle[] rects = region.GetRegionScansReadOnlyInt();
            return GetRegionBounds(rects, 0, rects.Length);
        }

        /// <summary>
        /// Allows you to find the bounding box for a "region" that is described as an
        /// array of bounding boxes.
        /// </summary>
        /// <param name="rectsF">The "region" you want to find a bounding box for.</param>
        /// <returns>A RectangleF structure that surrounds the Region.</returns>
        public static RectangleF GetRegionBounds(RectangleF[] rectsF, int startIndex, int length)
        {
            if (rectsF.Length == 0)
            {
                return RectangleF.Empty;
            }

            float left = rectsF[startIndex].Left;
            float top = rectsF[startIndex].Top;
            float right = rectsF[startIndex].Right;
            float bottom = rectsF[startIndex].Bottom;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                RectangleF rectF = rectsF[i];

                if (rectF.Left < left)
                {
                    left = rectF.Left;
                }

                if (rectF.Top < top)
                {
                    top = rectF.Top;
                }

                if (rectF.Right > right)
                {
                    right = rectF.Right;
                }

                if (rectF.Bottom > bottom)
                {
                    bottom = rectF.Bottom;
                }
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static RectangleF GetTraceBounds(PointF[] pointsF, int startIndex, int length)
        {
            if (pointsF.Length == 0)
            {
                return RectangleF.Empty;
            }

            float left = pointsF[startIndex].X;
            float top = pointsF[startIndex].Y;
            float right = 1 + pointsF[startIndex].X;
            float bottom = 1 + pointsF[startIndex].Y;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                PointF pointF = pointsF[i];

                if (pointF.X < left)
                {
                    left = pointF.X;
                }

                if (pointF.Y < top)
                {
                    top = pointF.Y;
                }

                if (pointF.X > right)
                {
                    right = pointF.X;
                }

                if (pointF.Y > bottom)
                {
                    bottom = pointF.Y;
                }
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static Rectangle GetTraceBounds(Point[] points, int startIndex, int length)
        {
            if (points.Length == 0)
            {
                return Rectangle.Empty;
            }

            int left = points[startIndex].X;
            int top = points[startIndex].Y;
            int right = 1 + points[startIndex].X;
            int bottom = 1 + points[startIndex].Y;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                Point point = points[i];

                if (point.X < left)
                {
                    left = point.X;
                }

                if (point.Y < top)
                {
                    top = point.Y;
                }

                if (point.X > right)
                {
                    right = point.X;
                }

                if (point.Y > bottom)
                {
                    bottom = point.Y;
                }
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Allows you to find the bounding box for a "region" that is described as an
        /// array of bounding boxes.
        /// </summary>
        /// <param name="rectsF">The "region" you want to find a bounding box for.</param>
        /// <returns>A RectangleF structure that surrounds the Region.</returns>
        public static Rectangle GetRegionBounds(Rectangle[] rects, int startIndex, int length)
        {
            if (rects.Length == 0)
            {
                return Rectangle.Empty;
            }

            int left = rects[startIndex].Left;
            int top = rects[startIndex].Top;
            int right = rects[startIndex].Right;
            int bottom = rects[startIndex].Bottom;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                Rectangle rect = rects[i];

                if (rect.Left < left)
                {
                    left = rect.Left;
                }

                if (rect.Top < top)
                {
                    top = rect.Top;
                }

                if (rect.Right > right)
                {
                    right = rect.Right;
                }

                if (rect.Bottom > bottom)
                {
                    bottom = rect.Bottom;
                }
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public static RectangleF GetRegionBounds(RectangleF[] rectsF)
        {
            return GetRegionBounds(rectsF, 0, rectsF.Length);
        }

        public static Rectangle GetRegionBounds(Rectangle[] rects)
        {
            return GetRegionBounds(rects, 0, rects.Length);
        }

        private static float DistanceSquared(RectangleF[] rectsF, int indexA, int indexB)
        {
            PointF centerA = new PointF(rectsF[indexA].Left + (rectsF[indexA].Width / 2), rectsF[indexA].Top + (rectsF[indexA].Height / 2));
            PointF centerB = new PointF(rectsF[indexB].Left + (rectsF[indexB].Width / 2), rectsF[indexB].Top + (rectsF[indexB].Height / 2));
            
            return ((centerA.X - centerB.X) * (centerA.X - centerB.X)) + 
                ((centerA.Y - centerB.Y) * (centerA.Y - centerB.Y));
        }
       
        /// <summary>
        /// Simplifies a Region into N number of bounding boxes.
        /// </summary>
        /// <param name="region">The Region to simplify.</param>
        /// <param name="complexity">The maximum number of bounding boxes to return, or 0 for however many are necessary (equivalent to using Region.GetRegionScans).</param>
        /// <returns></returns>
        public static Rectangle[] SimplifyRegion(PdnRegion region, int complexity)
        {
            Rectangle[] rects = region.GetRegionScansReadOnlyInt();
            return SimplifyRegion(rects, complexity);
        }

        public static Rectangle[] SimplifyRegion(Rectangle[] rects, int complexity)
        {
            if (complexity == 0 || rects.Length < complexity)
            {
                return (Rectangle[])rects.Clone();
            }

            Rectangle[] boxes = new Rectangle[complexity];

            for (int i = 0; i < complexity; ++i)
            {
                int startIndex = (i * rects.Length) / complexity;
                int length = Math.Min(rects.Length, ((i + 1) * rects.Length) / complexity) - startIndex;
                boxes[i] = GetRegionBounds(rects, startIndex, length);
            }

            return boxes;
        }


        public static RectangleF[] SimplifyTrace(PointF[] pointsF, int complexity)
        {
            if (complexity == 0 || 
                (pointsF.Length - 1) < complexity)
            {
                return PointsToRectangles(pointsF);
            }

            RectangleF[] boxes = new RectangleF[complexity];
            int parLength = pointsF.Length - 1; // "(points as Rectangles).Length"
            
            for (int i = 0; i < complexity; ++i)
            {
                int startIndex = (i * parLength) / complexity;
                int length = Math.Min(parLength, ((i + 1) * parLength) / complexity) - startIndex;
                boxes[i] = GetTraceBounds(pointsF, startIndex, length + 1);
            }

            return boxes;
        }

        public static Rectangle[] SimplifyTrace(PdnGraphicsPath trace, int complexity)
        {
            return SimplifyRegion(TraceToRectangles(trace), complexity);
        }

        public static Rectangle[] SimplifyTrace(PdnGraphicsPath trace)
        {
            return SimplifyTrace(trace, DefaultSimplificationFactor);
        }

        public static Rectangle[] TraceToRectangles(PdnGraphicsPath trace, int complexity)
        {
            int pointCount = trace.PointCount;

            if (pointCount == 0)
            {
                return new Rectangle[0];
            }

            PointF[] pathPoints = trace.PathPoints;
            byte[] pathTypes = trace.PathTypes;
            int figureStart = 0;

            // first get count of rectangles we'll need
            Rectangle[] rects = new Rectangle[pointCount];

            for (int i = 0; i < pointCount; ++i)
            {
                byte type = pathTypes[i];

                Point a = Point.Truncate(pathPoints[i]);
                Point b;
            
                if ((type & (byte)PathPointType.CloseSubpath) != 0)
                {
                    b = Point.Truncate(pathPoints[figureStart]);
                    figureStart = i + 1;
                }
                else
                {
                    b = Point.Truncate(pathPoints[i + 1]);
                }

                rects[i] = Utility.PointsToRectangle(a, b);
            }

            return rects;
        }

        public static Rectangle[] TraceToRectangles(PdnGraphicsPath trace)
        {
            return TraceToRectangles(trace, DefaultSimplificationFactor);
        }

        public static RectangleF[] SimplifyTrace(PointF[] pointsF)
        {
            return SimplifyTrace(pointsF, defaultSimplificationFactor);
        }

        public static Rectangle[] SimplifyAndInflateRegion(Rectangle[] rects, int complexity, int inflationAmount)
        {
            Rectangle[] simplified = SimplifyRegion(rects, complexity);

            for (int i = 0; i < simplified.Length; ++i)
            {
                simplified[i].Inflate(inflationAmount, inflationAmount);
            }

            return simplified;
        }

        public static Rectangle[] SimplifyAndInflateRegion(Rectangle[] rects)
        {
            return SimplifyAndInflateRegion(rects, defaultSimplificationFactor, 1);
        }

        public static PdnRegion SimplifyAndInflateRegion(PdnRegion region, int complexity, int inflationAmount)
        {
            Rectangle[] rectRegion = SimplifyRegion(region, complexity);
            
            for (int i = 0; i < rectRegion.Length; ++i)
            {
                rectRegion[i].Inflate(inflationAmount, inflationAmount);
            }

            return RectanglesToRegion(rectRegion);
        }

        public static PdnRegion SimplifyAndInflateRegion(PdnRegion region)
        {
            return SimplifyAndInflateRegion(region, defaultSimplificationFactor, 1);
        }

        public static RectangleF[] TranslateRectangles(RectangleF[] rectsF, PointF offset)
        {
            RectangleF[] retRectsF = new RectangleF[rectsF.Length];
            int i = 0;

            foreach (RectangleF rectF in rectsF)
            {
                retRectsF[i] = new RectangleF(rectF.X + offset.X, rectF.Y + offset.Y, rectF.Width, rectF.Height);
                ++i;
            }

            return retRectsF;
        }

        public static Rectangle[] TranslateRectangles(Rectangle[] rects, int dx, int dy)
        {
            Rectangle[] retRects = new Rectangle[rects.Length];

            for (int i = 0; i < rects.Length; ++i)
            {
                retRects[i] = new Rectangle(rects[i].X + dx, rects[i].Y + dy, rects[i].Width, rects[i].Height);
            }

            return retRects;
        }

        public static void TranslatePointsInPlace(PointF[] ptsF, float dx, float dy)
        {
            for (int i = 0; i < ptsF.Length; ++i)
            {
                ptsF[i].X += dx;
                ptsF[i].Y += dy;
            }
        }

        public static void TranslatePointsInPlace(Point[] pts, int dx, int dy)
        {
            for (int i = 0; i < pts.Length; ++i)
            {
                pts[i].X += dx;
                pts[i].Y += dy;
            }
        }

        public static Rectangle[] TruncateRectangles(RectangleF[] rectsF)
        {
            Rectangle[] rects = new Rectangle[rectsF.Length];

            for (int i = 0; i < rectsF.Length; ++i)
            {
                rects[i] = Rectangle.Truncate(rectsF[i]);
            }

            return rects;
        }

        public static Point[] TruncatePoints(PointF[] pointsF)
        {
            Point[] points = new Point[pointsF.Length];

            for (int i = 0; i < pointsF.Length; ++i)
            {
                points[i] = Point.Truncate(pointsF[i]);
            }

            return points;
        }

        public static Point[] RoundPoints(PointF[] pointsF)
        {
            Point[] points = new Point[pointsF.Length];

            for (int i = 0; i < pointsF.Length; ++i)
            {
                points[i] = Point.Round(pointsF[i]);
            }

            return points;
        }

        /// <summary>
        /// The Sutherland-Hodgman clipping alrogithm.
        /// http://ezekiel.vancouver.wsu.edu/~cs442/lectures/clip/clip/index.html
        /// 
        /// # Clipping a convex polygon to a convex region (e.g., rectangle) will always produce a convex polygon (or no polygon if completely outside the clipping region).
        /// # Clipping a concave polygon to a rectangle may produce several polygons (see figure above) or, as the following algorithm does, produce a single, possibly degenerate, polygon.
        /// # Divide and conquer: Clip entire polygon against a single edge (i.e., half-plane). Repeat for each edge in the clipping region.
        ///
        /// The input is a sequence of vertices: {v0, v1, ... vn} given as an array of Points
        /// the result is a sequence of vertices, given as an array of Points. This result may have
        /// less than, equal, more than, or 0 vertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<PointF> SutherlandHodgman(RectangleF bounds, List<PointF> v)
        {
            List<PointF> p1 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Left, v);
            List<PointF> p2 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Right, p1);
            List<PointF> p3 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Top, p2);
            List<PointF> p4 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Bottom, p3);

            return p4;
        }

        private enum RectangleEdge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private static List<PointF> SutherlandHodgmanOneAxis(RectangleF bounds, RectangleEdge edge, List<PointF> v)
        {
            if (v.Count == 0)
            {
                return new List<PointF>();
            }

            List<PointF> polygon = new List<PointF>();
            
            PointF s = v[v.Count - 1];

            for (int i = 0; i < v.Count; ++i)
            {
                PointF p = v[i];
                bool pIn = IsInside(bounds, edge, p);
                bool sIn = IsInside(bounds, edge, s);

                if (sIn && pIn)
                {   
                    // case 1: inside -> inside
                    polygon.Add(p);
                }
                else if (sIn && !pIn)
                {   
                    // case 2: inside -> outside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                }
                else if (!sIn && !pIn)
                {   
                    // case 3: outside -> outside
                    // emit nothing
                }
                else if (!sIn && pIn)
                {   
                    // case 4: outside -> inside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                    polygon.Add(p);
                }

                s = p;
            }

            return polygon;
        }

        private static bool IsInside(RectangleF bounds, RectangleEdge edge, PointF p)
        {
            switch (edge)
            {
                case RectangleEdge.Left:
                    return !(p.X < bounds.Left);
                        
                case RectangleEdge.Right:
                    return !(p.X >= bounds.Right);

                case RectangleEdge.Top:
                    return !(p.Y < bounds.Top);

                case RectangleEdge.Bottom:
                    return !(p.Y >= bounds.Bottom);

                default:
                    throw new InvalidEnumArgumentException("edge");
            }
        }

        private static Point LineIntercept(Rectangle bounds, RectangleEdge edge, Point a, Point b)
        {
            if (a == b)
            {
                return a;
            }

            switch (edge)
            {
                case RectangleEdge.Bottom:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(a.X + (((b.X - a.X) * (bounds.Bottom - a.Y)) / (b.Y - a.Y)), bounds.Bottom);

                case RectangleEdge.Left:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(bounds.Left, a.Y + (((b.Y - a.Y) * (bounds.Left - a.X)) / (b.X - a.X)));

                case RectangleEdge.Right:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(bounds.Right, a.Y + (((b.Y - a.Y) * (bounds.Right - a.X)) / (b.X - a.X)));

                case RectangleEdge.Top:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(a.X + (((b.X - a.X) * (bounds.Top - a.Y)) / (b.Y - a.Y)), bounds.Top);                                    
            }

            throw new ArgumentException("no intercept found");
        }

        private static PointF LineIntercept(RectangleF bounds, RectangleEdge edge, PointF a, PointF b)
        {
            if (a == b)
            {
                return a;
            }

            switch (edge)
            {
                case RectangleEdge.Bottom:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(a.X + (((b.X - a.X) * (bounds.Bottom - a.Y)) / (b.Y - a.Y)), bounds.Bottom);

                case RectangleEdge.Left:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(bounds.Left, a.Y + (((b.Y - a.Y) * (bounds.Left - a.X)) / (b.X - a.X)));

                case RectangleEdge.Right:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(bounds.Right, a.Y + (((b.Y - a.Y) * (bounds.Right - a.X)) / (b.X - a.X)));

                case RectangleEdge.Top:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(a.X + (((b.X - a.X) * (bounds.Top - a.Y)) / (b.Y - a.Y)), bounds.Top);                                    
            }

            throw new ArgumentException("no intercept found");
        }

        public static Point[] GetLinePoints(Point first, Point second)
        {
            Point[] coords = null;

            int x1 = first.X;
            int y1 = first.Y;
            int x2 = second.X;
            int y2 = second.Y;
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dxabs = Math.Abs(dx);
            int dyabs = Math.Abs(dy);
            int px = x1;
            int py = y1;
            int sdx = Math.Sign(dx);
            int sdy = Math.Sign(dy);
            int x = 0;
            int y = 0;

            if (dxabs > dyabs)
            {
                coords = new Point[dxabs + 1];

                for (int i = 0; i <= dxabs; i++)
                {
                    y += dyabs;

                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }

                    coords[i] = new Point(px, py);
                    px += sdx;
                }
            }
            else 
                // had to add in this cludge for slopes of 1 ... wasn't drawing half the line
                if (dxabs == dyabs)
            {
                coords = new Point[dxabs + 1];

                for (int i = 0; i <= dxabs; i++)
                {
                    coords[i] = new Point(px, py);
                    px += sdx;
                    py += sdy;
                }
            }
            else
            {
                coords = new Point[dyabs + 1];

                for (int i = 0; i <= dyabs; i++)
                {
                    x += dxabs;

                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }

                    coords[i] = new Point(px, py);
                    py += sdy;
                }
            }

            return coords;
        }

        public static long GetTimeMs()
        {
            return Utility.TicksToMs(DateTime.Now.Ticks);        
        }

        /// <summary>
        /// Returns the Distance between two points
        /// </summary>
        public static float Distance(PointF a, PointF b)
        {
            return Magnitude(new PointF(a.X - b.X, a.Y - b.Y));
        }

        /// <summary>
        /// Returns the Magnitude (distance to origin) of a point
        /// </summary>
        public static float Magnitude(PointF p)
        {
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static double Clamp(double x, double min, double max) 
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        public static float Clamp(float x, float min, float max) 
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        public static int Clamp(int x, int min, int max)
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }
        
        public static byte ClampToByte(double x) 
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }
        
        public static byte ClampToByte(float x) 
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }
        
        public static byte ClampToByte(int x) 
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }

        public static float Lerp(float from, float to, float frac) 
        {
            return (from * (1 - frac) + to * frac);
        }

        public static double Lerp(double from, double to, double frac) 
        {
            return (from * (1 - frac) + to * frac);
        }

        public static PointF Lerp(PointF from, PointF to, float frac)
        {
            return new PointF(Lerp(from.X, to.X, frac), Lerp(from.Y, to.Y, frac));
        }

        public static int ColorDifference(ColorBgra a, ColorBgra b) 
        {
            return (int)Math.Ceiling(Math.Sqrt(ColorDifferenceSquared(a, b)));
        }

        public static int ColorDifferenceSquared(ColorBgra a, ColorBgra b) 
        {
            int diffSq = 0, tmp;

            tmp = a.R - b.R;
            diffSq += tmp * tmp;
            tmp = a.G - b.G;
            diffSq += tmp * tmp;
            tmp = a.B - b.B;
            diffSq += tmp * tmp;

            return diffSq / 3;
        }

        public static DialogResult ShowDialog(Form showMe, IWin32Window owner)
        {
            DialogResult dr;

            if (showMe is PdnBaseForm)
            {
                PdnBaseForm showMe2 = (PdnBaseForm)showMe;
                double oldOpacity = showMe2.Opacity;
                showMe2.Opacity = 0.9;
                dr = showMe2.ShowDialog(owner);
                showMe2.Opacity = oldOpacity;
            }
            else
            {
                double oldOpacity = showMe.Opacity;
                showMe.Opacity = 0.9;
                dr = showMe.ShowDialog(owner);
                showMe.Opacity = oldOpacity;
            }

            Control control = owner as Control;
            if (control != null)
            {
                control.Update();
            }

            return dr;
        }

        public static void ShowHelp(Control parent)
        {
            string[] locales = PdnResources.GetLocaleNameChain();
            const string indexHtml = "index.html";
            string baseDir = PdnInfo.GetApplicationDir();
            string helpDir = Path.Combine(baseDir, "Help");

            // Walk the chain of locale names and try to find Help/[locale]/index.html
            // If all else fails, try Help/index.html
            for (int i = 0; i < locales.Length; ++i)
            {
                string localeHelpDir = Path.Combine(helpDir, locales[i]);
                string indexHtmlPath = Path.Combine(localeHelpDir, indexHtml);

                if (File.Exists(indexHtmlPath))
                {
                    SystemLayer.Shell.OpenUrl(parent, indexHtmlPath);
                    return;
                }
            }

            string lastChancePath = Path.Combine(helpDir, indexHtml);
            if (File.Exists(lastChancePath))
            {
                SystemLayer.Shell.OpenUrl(parent, lastChancePath);
            }
            else
            {
                // TODO: error
            }
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 16-bit unsigned integer that was read.</returns>
        public static int ReadUInt16(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            return byte1 + (byte2 << 8);
        }

        public static void WriteUInt16(Stream stream, UInt16 word)
        {
            stream.WriteByte((byte)(word & 0xff));
            stream.WriteByte((byte)(word >> 8));
        }

        public static void WriteUInt24(Stream stream, int uint24)
        {
            stream.WriteByte((byte)(uint24 & 0xff));
            stream.WriteByte((byte)((uint24 >> 8) & 0xff));
            stream.WriteByte((byte)((uint24 >> 16) & 0xff));
        }

        public static void WriteUInt32(Stream stream, UInt32 uint32)
        {
            stream.WriteByte((byte)(uint32 & 0xff));
            stream.WriteByte((byte)((uint32 >> 8) & 0xff));
            stream.WriteByte((byte)((uint32 >> 16) & 0xff));
            stream.WriteByte((byte)((uint32 >> 24) & 0xff));
        }

        /// <summary>
        /// Reads a 24-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 24-bit unsigned integer that was read.</returns>
        public static int ReadUInt24(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            int byte3 = stream.ReadByte();

            if (byte3 == -1)
            {
                return -1;
            }

            return byte1 + (byte2 << 8) + (byte3 << 16);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 32-bit unsigned integer that was read.</returns>
        public static long ReadUInt32(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            int byte3 = stream.ReadByte();

            if (byte3 == -1)
            {
                return -1;
            }

            int byte4 = stream.ReadByte();

            if (byte4 == -1)
            {
                return -1;
            }

            return unchecked((long)((uint)(byte1 + (byte2 << 8) + (byte3 << 16) + (byte4 << 24))));
        }

        public static int ReadFromStream(Stream input, byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = input.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    throw new IOException("ran out of data");
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public static long CopyStream(Stream input, Stream output, long maxBytes)
        {
            long bytesCopied = 0;
            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = input.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    break;
                }
                else
                {
                    int bytesToCopy;

                    if (maxBytes != -1 && (bytesCopied + bytesRead) > maxBytes)
                    {
                        bytesToCopy = (int)(maxBytes - bytesCopied);
                    }
                    else
                    {
                        bytesToCopy = bytesRead;
                    }

                    output.Write(buffer, 0, bytesRead);
                    bytesCopied += bytesToCopy;

                    if (bytesToCopy != bytesRead)
                    {
                        break;
                    }
                }
            }

            return bytesCopied;
        }

        public static long CopyStream(Stream input, Stream output)
        {
            return CopyStream(input, output, -1);
        }


        private struct Edge
        {
            public int miny;   // int
            public int maxy;   // int
            public int x;      // fixed point: 24.8
            public int dxdy;   // fixed point: 24.8

            public Edge(int miny, int maxy, int x, int dxdy)
            {
                this.miny = miny;
                this.maxy = maxy;
                this.x = x;
                this.dxdy = dxdy;
            }
        }

        public static Scanline[] GetScans(Point[] vertices)
        {
            return GetScans(vertices, 0, vertices.Length);
        }

        public static Scanline[] GetScans(Point[] vertices, int startIndex, int length)
        {
            if (length > vertices.Length - startIndex)
            {
                throw new ArgumentException("out of bounds: length > vertices.Length - startIndex");
            }

            int ymax = 0;

            // Build edge table
            Edge[] edgeTable = new Edge[length];
            int edgeCount = 0;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Point top = vertices[i];
                Point bottom = vertices[(((i + 1) - startIndex) % length) + startIndex];
                int dy;

                if (top.Y > bottom.Y)
                {
                    Point temp = top;
                    top = bottom;
                    bottom = temp;
                }
                
                dy = bottom.Y - top.Y;

                if (dy != 0)
                {
                    edgeTable[edgeCount] = new Edge(top.Y, bottom.Y, top.X << 8, (((bottom.X - top.X) << 8) / dy));
                    ymax = Math.Max(ymax, bottom.Y);
                    ++edgeCount;
                }
            }

            // Sort edge table by miny
            for (int i = 0; i < edgeCount - 1; ++i)
            {
                int min = i;

                for (int j = i + 1; j < edgeCount; ++j)
                {
                    if (edgeTable[j].miny < edgeTable[min].miny)
                    {
                        min = j;
                    }
                }

                if (min != i)
                {
                    Edge temp = edgeTable[min];
                    edgeTable[min] = edgeTable[i];
                    edgeTable[i] = temp;
                }
            }

            // Compute how many scanlines we will be emitting
            int scanCount = 0;
            int activeLow = 0;
            int activeHigh = 0;
            int yscan1 = edgeTable[0].miny;

            // we assume that edgeTable[0].miny == yscan
            while (activeHigh < edgeCount - 1 && 
                   edgeTable[activeHigh + 1].miny == yscan1)
            {
                ++activeHigh;
            }

            while (yscan1 <= ymax)
            {
                // Find new edges where yscan == miny
                while (activeHigh < edgeCount - 1 &&
                       edgeTable[activeHigh + 1].miny == yscan1)
                {
                    ++activeHigh;
                }

                int count = 0;
                for (int i = activeLow; i <= activeHigh; ++i)
                {
                    if (edgeTable[i].maxy > yscan1)
                    {
                        ++count;
                    }
                }

                scanCount += count / 2;
                ++yscan1;

                // Remove edges where yscan == maxy
                while (activeLow < edgeCount - 1 &&
                       edgeTable[activeLow].maxy <= yscan1)
                {
                    ++activeLow;
                }

                if (activeLow > activeHigh)
                {
                    activeHigh = activeLow;
                }
            }

            // Allocate scanlines that we'll return
            Scanline[] scans = new Scanline[scanCount];

            // Active Edge Table (AET): it is indices into the Edge Table (ET)
            int[] active = new int[edgeCount];
            int activeCount = 0;
            int yscan2 = edgeTable[0].miny;
            int scansIndex = 0;
            
            // Repeat until both the ET and AET are empty
            while (yscan2 <= ymax)
            {
                // Move any edges from the ET to the AET where yscan == miny
                for (int i = 0; i < edgeCount; ++i)
                {
                    if (edgeTable[i].miny == yscan2)
                    {
                        active[activeCount] = i;
                        ++activeCount;
                    }
                }

                // Sort the AET on x
                for (int i = 0; i < activeCount - 1; ++i)
                {
                    int min = i;

                    for (int j = i + 1; j < activeCount; ++j)
                    {
                        if (edgeTable[active[j]].x < edgeTable[active[min]].x)
                        {
                            min = j;
                        }
                    }

                    if (min != i)
                    {
                        int temp = active[min];
                        active[min] = active[i];
                        active[i] = temp;
                    }
                }

                // For each pair of entries in the AET, fill in pixels between their info
                for (int i = 0; i < activeCount; i += 2)
                {
                    Edge el = edgeTable[active[i]];
                    Edge er = edgeTable[active[i + 1]];
                    int startx = (el.x + 0xff) >> 8; // ceil(x)
                    int endx = er.x >> 8;      // floor(x)

                    scans[scansIndex] = new Scanline(startx, yscan2, endx - startx);
                    ++scansIndex;
                }

                ++yscan2;

                // Remove from the AET any edge where yscan == maxy
                int k = 0;
                while (k < activeCount && activeCount > 0)
                {
                    if (edgeTable[active[k]].maxy == yscan2)
                    {
                        // remove by shifting everything down one
                        for (int j =  k + 1; j < activeCount; ++j)
                        {
                            active[j - 1] = active[j];
                        }

                        --activeCount;
                    }
                    else
                    {
                        ++k;
                    }
                }

                // Update x for each entry in AET
                for (int i = 0; i < activeCount; ++i)
                {
                    edgeTable[active[i]].x += edgeTable[active[i]].dxdy;
                }
            }

            return scans;
        }

        public static PointF TransformOnePoint(Matrix matrix, PointF ptF)
        {
            PointF[] ptFs = new PointF[1] { ptF };
            matrix.TransformPoints(ptFs);
            return ptFs[0];
        }

        public static PointF TransformOneVector(Matrix matrix, PointF ptF)
        {
            PointF[] ptFs = new PointF[1] { ptF };
            matrix.TransformVectors(ptFs);
            return ptFs[0];
        }

        public static PointF NormalizeVector(PointF vecF)
        {
            float magnitude = Magnitude(vecF);
            vecF.X /= magnitude;
            vecF.Y /= magnitude;
            return vecF;
        }

        public static PointF NormalizeVector2(PointF vecF)
        {
            float magnitude = Magnitude(vecF);

            if (magnitude == 0)
            {
                vecF.X = 0;
                vecF.Y = 0;
            }
            else
            {
                vecF.X /= magnitude;
                vecF.Y /= magnitude;
            }

            return vecF;
        }

        public static void NormalizeVectors(PointF[] vecsF)
        {
            for (int i = 0; i < vecsF.Length; ++i)
            {
                vecsF[i] = NormalizeVector(vecsF[i]);
            }
        }

        public static PointF RotateVector(PointF vecF, float angleDelta)
        {
            angleDelta *= (float)( Math.PI / 180.0);
            float vecFLen = Magnitude(vecF);
            float vecFAngle = angleDelta + (float)Math.Atan2(vecF.Y, vecF.X);
            vecF.X = (float)Math.Cos(vecFAngle);
            vecF.Y = (float)Math.Sin(vecFAngle);
            return vecF;
        }

        public static void RotateVectors(PointF[] vecFs, float angleDelta)
        {
            for (int i = 0; i < vecFs.Length; ++i)
            {
                vecFs[i] = RotateVector(vecFs[i], angleDelta);
            }
        }

        public static PointF MultiplyVector(PointF vecF, float scalar)
        {
            return new PointF(vecF.X * scalar, vecF.Y * scalar);
        }

        public static PointF AddVectors(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        public static PointF SubtractVectors(PointF lhs, PointF rhs)
        {
            return new PointF(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static PointF NegateVector(PointF v)
        {
            return new PointF(-v.X, -v.Y);
        }

        public static float GetAngleOfTransform(Matrix matrix)
        {
            PointF[] pts = new PointF[] { new PointF(1.0f, 0.0f) };
            matrix.TransformVectors(pts);
            double atan2 = Math.Atan2(pts[0].Y, pts[0].X);
            double angle = atan2 * (180.0f / Math.PI);

            return (float)angle;
        }

        public static bool IsTransformFlipped(Matrix matrix)
        {
            PointF ptX = new PointF(1.0f, 0.0f);
            PointF ptXT = Utility.TransformOneVector(matrix, ptX);
            double atan2X = Math.Atan2(ptXT.Y, ptXT.X);
            double angleX = atan2X * (180.0 / Math.PI);

            PointF ptY = new PointF(0.0f, 1.0f);
            PointF ptYT = Utility.TransformOneVector(matrix, ptY);
            double atan2Y = Math.Atan2(ptYT.Y, ptYT.X);
            double angleY = (atan2Y * (180.0 / Math.PI)) - 90.0;

            while (angleX < 0)
            {
                angleX += 360;
            }

            while (angleY < 0)
            {
                angleY += 360;
            }

            double angleDelta = Math.Abs(angleX - angleY);

            return angleDelta > 1.0 && angleDelta < 359.0;
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        public static float DotProduct(PointF lhs, PointF rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y;
        }

        /// <summary>
        /// Calculates the orthogonal projection of y on to u.
        /// yhat = u * ((y dot u) / (u dot u))
        /// z = y - yhat
        /// Section 6.2 (pg. 381) of Linear Algebra and its Applications, Second Edition, by David C. Lay
        /// </summary>
        /// <param name="y">The vector to decompose</param>
        /// <param name="u">The non-zero vector to project y on to</param>
        /// <param name="yhat">The orthogonal projection of y onto u</param>
        /// <param name="yhatLen">The length of yhat such that yhat = yhatLen * u</param>
        /// <param name="z">The component of y orthogonal to u</param>
        public static void GetProjection(PointF y, PointF u, out PointF yhat, out float yhatLen, out PointF z)
        {
            if (u.X == 0 && u.Y == 0)
            {
                yhat = new PointF(0, 0);
                yhatLen = 0;
                z = new PointF(0, 0);
            }
            else
            {
                float yDotU = DotProduct(y, u);
                float uDotU = DotProduct(u, u);
                yhatLen = yDotU / uDotU;
                yhat = MultiplyVector(u, yhatLen);
                z = SubtractVectors(y, yhat);
            }
        }

        public static int GreatestCommonDivisor(int a, int b)
        {
            int r;

            if (a < b)
            {
                r = a;
                a = b;
                b = r;
            }

            do
            {
                r = a % b;
                a = b;
                b = r;
            }
            while (r != 0);

            return a;
        }

        public static void Swap(ref int a, ref int b)
        {
            int t;

            t = a;
            a = b;
            b = t;
        }
    }
}
