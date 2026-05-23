using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using PaintDotNet.Threading;

namespace PaintDotNet
{
	/// <summary>
	/// Defines a few miscellaneous constants and static functions.
	/// </summary>
    public sealed class Utility
    {
        private Utility()
        {
        }

        private static int defaultSimplificationFactor = 20;
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

        public static string GetCopyrightString()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyCopyrightAttribute aca = (AssemblyCopyrightAttribute)attributes[0];            
            return aca.Copyright;
        }

        /// <summary>
        /// Returns a full version string of the form: ApplicationConfiguration + BuildType + BuildVersion
        /// i.e.: "Beta 2 Debug build 1.0.*.*"
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];

            return aca.Configuration + 
#if DEBUG
                " Debug" +
#else
                    " Release" +
#endif
                " build " +
                Application.ProductVersion;
        }

        /// <summary>
        /// Returns the application name, with the version string. i.e., "Paint.NET (Beta 2 Debug build 1.0.*.*)"
        /// </summary>
        /// <returns></returns>
        public static string GetFullAppName()
        {
            return Application.ProductName + " (" + GetVersionString() + ")";
        }

        /// <summary>
        /// For final builds, this returns Application.ProductName (i.e., "Paint.NET")
        /// For non-final builds, this returns GetFullAppName()
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];

            if (aca.Configuration.IndexOf("Final") == -1)
            {
                return GetFullAppName();
            }
            else
            {
                return Application.ProductName;
            }
        }

        public static bool DoesControlHaveMouseCaptured(Control control)
        {
            bool result = false;

            result |= control.Capture;

            foreach(Control c in control.Controls)
            {
                result |= DoesControlHaveMouseCaptured(c);
            }

            return result;
        }


        public static Stream GetZipOutputStream(Stream stream)
        {
            return new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(stream);
        }

        public static Stream GetZipInputStream(Stream stream)
        {
            return new ICSharpCode.SharpZipLib.GZip.GZipInputStream(stream);
        }

        public static long TicksToMs(long ticks)
		{
			return ticks/10000;
		}

        public static string GetStaticName(Type type)
        {
            PropertyInfo pi = type.GetProperty("StaticName", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            return (string)pi.GetValue(null, null);
        }

        [ThreadStatic]
        private static PaintDotNet.Threading.ThreadPool threadPool;
        public static PaintDotNet.Threading.ThreadPool ThreadPool
        {
            get
            {
                if (threadPool == null)
                {
                    threadPool = new PaintDotNet.Threading.ThreadPool();
                }

                return threadPool;
            }
        }

        public static readonly float[][] Identity5x5F = new float[][] { new float[] { 1, 0, 0, 0, 0 },
                                                                        new float[] { 0, 1, 0, 0, 0 },
                                                                        new float[] { 0, 0, 1, 0, 0 },
                                                                        new float[] { 0, 0, 0, 1, 0 },
                                                                        new float[] { 0, 0, 0, 0, 1 } };

        public static readonly ColorMatrix IdentityColorMatrix = new ColorMatrix(Identity5x5F);

        // initialized in the static constructor
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

        /// <summary>
        /// Converts a string name into a more "user friendly" style. Useful for enumeration names.
        /// Example: Converts "TopLeftCenter" to "Top Left Center"
        /// </summary>
        /// <param name="str1"></param>
        /// <returns></returns>
        public static string InsertSpaces(String str1)
        {
            string str2 = string.Copy(str1);
            bool number = false;

            for (int i = 1; i < str2.Length; i++)
            {
                char ch = str2[i];
                if (char.IsUpper(ch))
                {
                    str2 = str2.Insert(i, " ");
                    i++;
                    number = false;
                }
                if (char.IsNumber(ch))
                {
                    if(!number)
                    {
                        str2 = str2.Insert(i, " ");
                        i++;
                    }
                    number = true;
                }
            }

            return str2;
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

        public static int Sum(int[,] array)
        {
            int sum = 0;

            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                {
                    sum += array[i,j];
                }
            }

            return sum;
        }

		public static bool CheckNumericUpDown(NumericUpDown upDown)
		{
			int a;
		
			try
			{
				a = int.Parse(upDown.Text);
			}

			catch(FormatException)
			{
				return false;
			}

			catch(OverflowException)
			{
				return false;
			}

			if((a <= (int)upDown.Maximum) && (a >= (int)upDown.Minimum))
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

		public static Point GetPointFromMouseXY(System.Windows.Forms.MouseEventArgs e)
		{
			Point p = new Point();
			p.X = e.X;
			p.Y = e.Y;
			return p;
		}

		public static string SizeStringFromBytes(double bytes)
		{
			string returnMe;

			// Gigs
			if(bytes > (1024*1024*1024))
			{
				bytes /= 1024*1024*1024;
				returnMe = bytes.ToString("F1") + " GB";
			}
				// Megs
			else if(bytes > (1024*1024))
			{
				bytes /= 1024*1024;
				returnMe = bytes.ToString("F1") + " MB";
			}
				// K
			else if(bytes > (1024))
			{
				bytes /= 1024;
				returnMe = bytes.ToString("F1") + " KB";
			}
				// Bytes
			else
			{
				returnMe = bytes.ToString("F0") + " Bytes";
			}

			return returnMe;
		}

        public static void ErrorBox(IWin32Window parent, string message)
        {
            MessageBox.Show(parent, message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

		public static DialogResult AskYesNo(IWin32Window parent, string question)
		{
			return MessageBox.Show(parent, question, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		public static DialogResult AskYesNoCancel(IWin32Window parent, string question)
		{
			return MessageBox.Show(parent, question, Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
		}

        public static Image GetImageResource(string fileName)
        {
            Stream stream = GetResourceStream(fileName);
            Image image = Image.FromStream(stream);
            return image;
        }

        public static Stream GetResourceStream(string fileName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Utility).Namespace + "." + fileName);
        }

		/// <summary>
		/// Invalidates an object through its IInvalidate methods. If the given region
		/// is "too complex", then the bounding box of the region will be invalidated.
		/// </summary>
		/// <param name="thing">The object to invalidate.</param>
		/// <param name="region">The region in the object to invalidate.</param>
		public static void FastInvalidate(IInvalidate thing, Region region, int maxComplexity)
		{
            RectangleF[] rectsF = SimplifyRegion(region, maxComplexity);
            
            foreach (RectangleF rectF in rectsF)
            {
                thing.Invalidate(Rectangle.Truncate(rectF));
            }
		}

        public static Scanline[] GetRectangleScans(Rectangle rect)
        {
            Scanline[] scans = new Scanline[rect.Height];

            for (int y = 0; y < rect.Height; ++y)
            {
                scans[y] = new Scanline(new Point(rect.X, rect.Y + y), rect.Width);
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
                    scans[scanIndex] = new Scanline(new Point(rect.X, rect.Y + y), rect.Width);
                    ++scanIndex;
                }
            }

            return scans;
        }

        public static Region ScanlinesToRegion(Scanline[] scans)
        {
            return ScanlinesToRegion(scans, 0, scans.Length);
        }

        public static Region ScanlinesToRegion(Scanline[] scans, int startIndex, int length)
        {
            return RectanglesToRegion(ScanlinesToRectangles(scans, startIndex, length));
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
                rects[i] = new Rectangle(scans[i + startIndex].Point, new Size(scans[i + startIndex].Length, 1));
            }

            return rects;
        }

        public static Scanline[] GetRegionScans(RectangleF[] region)
        {
            int scanCount = 0;

            for (int i = 0; i < region.Length; ++i)
            {
                scanCount += (int)region[i].Height;
            }

            Scanline[] scans = new Scanline[scanCount];
            int scanIndex = 0;

            foreach (RectangleF rectF in region)
            {
                Rectangle rect = Rectangle.Truncate(rectF);

                for (int y = 0; y < (int)rectF.Height; ++y)
                {
                    scans[scanIndex] = new Scanline(new Point(rect.X, rect.Y + y), rect.Width);
                    ++scanIndex;
                }
            }

            return scans;
        }

        /// <summary>
        /// Same as FastInvalidate, but inflates the regions by a given amount.
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="region"></param>
        /// <param name="maxComplexity"></param>
        /// <param name="inflationFactor"></param>
        public static void FastInvalidate(IInvalidate thing, Region region, int maxComplexity, int inflationFactor)
        {
            RectangleF[] rectsF = SimplifyRegion(region, maxComplexity);
            
            foreach (RectangleF rectF in rectsF)
            {
                thing.Invalidate(Rectangle.Inflate(Rectangle.Truncate(rectF), inflationFactor, inflationFactor));
            }
        }

        /// <summary>
        /// Found on Google Groups when searching for "Region.Union" while looking
        /// for bugs:
        /// ---
        /// Hello,
        /// 
        /// I did not run your code, but I know Region.Union is flawed in both 1.0 and
        /// 1.1, so I assume it is in the gdi+ unmanged code dll.  The best workaround,
        /// in terms of speed, is to use a GraphicsPath, but it must be a path with
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
        public static Region RectanglesToRegion(RectangleF[] rectsF, int startIndex, int length)
        {
            Region region;

            if (length == 0)
            {
                region = new Region();
                region.MakeEmpty();
                return region;
            }

            using (GraphicsPath path = new GraphicsPath())
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

                region = new Region(path);
                path.Dispose();
            }

            return region;
        }

        public static Region RectanglesToRegion(RectangleF[] rectsF)
        {
            return RectanglesToRegion(rectsF, 0, rectsF.Length);
        }

        public static Region RectanglesToRegion(Rectangle[] rects, int startIndex, int length)
        {
            Region region;

            if (length == 0)
            {
                region = new Region();
                region.MakeEmpty();
                return region;
            }

            using (GraphicsPath path = new GraphicsPath())
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

                region = new Region(path);
                path.Dispose();
            }

            return region;
        }

        public static Region RectanglesToRegion(Rectangle[] rects)
        {
            return RectanglesToRegion(rects, 0, rects.Length);
        }

        public static int RegionArea(RectangleF[] rectsF)
        {
            int area = 0;

            foreach (RectangleF rectF in rectsF)
            {
                Rectangle rect = Rectangle.Truncate(rectF);
                area += rect.Width * rect.Height;
            }

            return area;
        }

        public static int RegionArea(Region region)
        {
            return RegionArea(region.GetRegionScans(IdentityMatrix));
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

        public static bool IsPointInRectangle(Point p, Rectangle r)
        {
            if((p.X < r.X) || (p.Y < r.Y) || (p.X >= r.Right) || (p.Y >= r.Bottom))
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
		public static RectangleF GetRegionBounds(Region region)
		{
			RectangleF[] rectsF = region.GetRegionScans(Utility.IdentityMatrix);
			return GetRegionBounds(rectsF, 0, rectsF.Length);
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
        public static RectangleF[] SimplifyRegion(Region region, int complexity)
        {
            RectangleF[] rectsF = region.GetRegionScans(Utility.IdentityMatrix);

            return SimplifyRegion(rectsF, complexity);
        }

        public static RectangleF[] SimplifyRegion(RectangleF[] rectsF, int complexity)
        {
            if (complexity == 0 ||
                rectsF.Length < complexity)
            {
                return rectsF;
            }

            RectangleF[] boxes = new RectangleF[complexity];

            for (int i = 0; i < complexity; ++i)
            {
                int startIndex = (i * rectsF.Length) / complexity;
                int length = Math.Min(rectsF.Length, ((i + 1) * rectsF.Length) / complexity) - startIndex;
                boxes[i] = GetRegionBounds(rectsF, startIndex, length);
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

        public static RectangleF[] SimplifyTrace(PointF[] pointsF)
        {
            return SimplifyTrace(pointsF, defaultSimplificationFactor);
        }

        public static RectangleF[] SimplifyAndInflateRegion(RectangleF[] rectsF, int complexity, int inflationAmount)
        {
            RectangleF[] simplified = SimplifyRegion(rectsF, complexity);

            for (int i = 0; i < simplified.Length; ++i)
            {
                simplified[i].Inflate(inflationAmount, inflationAmount);
            }

            return simplified;
        }

        public static RectangleF[] SimplifyAndInflateRegion(RectangleF[] rectsF)
        {
            return SimplifyAndInflateRegion(rectsF, defaultSimplificationFactor, 1);
        }

        public static Region SimplifyAndInflateRegion(Region region, int complexity, int inflationAmount)
        {
            RectangleF[] rectRegion = SimplifyRegion(region, complexity);
            
            for (int i = 0; i < rectRegion.Length; ++i)
            {
                rectRegion[i].Inflate(inflationAmount, inflationAmount);
            }

            return RectanglesToRegion(rectRegion);
        }

        public static Region SimplifyAndInflateRegion(Region region)
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

        public static Rectangle[] TruncateRectangles(RectangleF[] rectsF)
        {
            Rectangle[] rects = new Rectangle[rectsF.Length];

            for (int i = 0; i < rectsF.Length; ++i)
            {
                rects[i] = Rectangle.Truncate(rectsF[i]);
            }

            return rects;
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
        public static Point[] SutherlandHodgman(Rectangle bounds, Point[] v)
        {
            Point[] p1 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Left, v);
            Point[] p2 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Right, p1);
            Point[] p3 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Top, p2);
            Point[] p4 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Bottom, p3);

            return p4;
        }

        public static Point[] SutherlandHodgman(Rectangle bounds, ArrayList v)
        {
            return SutherlandHodgman(bounds, (Point[])v.ToArray(typeof(Point)));
        }

        private enum RectangleEdge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private static Point[] SutherlandHodgmanOneAxis(Rectangle bounds, RectangleEdge edge, Point[] v)
        {
            if (v.Length == 0)
            {
                return new Point[0];
            }

            ArrayList polygon = new ArrayList();
            
            Point s = v[v.Length - 1];

            for (int i = 0; i < v.Length; ++i)
            {
                Point p = v[i];
                bool pIn = IsInside(bounds, edge, p);
                bool sIn = IsInside(bounds, edge, s);

                if (sIn && pIn)
                {   // case 1: inside -> inside
                    polygon.Add(p);
                }
                else if (sIn && !pIn)
                {   // case 2: inside -> outside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                }
                else if (!sIn && !pIn)
                {   // case 3: outside -> outside
                    // emit nothing
                }
                else if (!sIn && pIn)
                {   // case 4: outside -> inside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                    polygon.Add(p);
                }

                s = p;
            }

            return (Point[])polygon.ToArray(typeof(Point));
        }

        private static bool IsInside(Rectangle bounds, RectangleEdge edge, Point p)
        {
            switch (edge)
            {
                case RectangleEdge.Left:
                    if (p.X < bounds.Left)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                        
                case RectangleEdge.Right:
                    if (p.X >= bounds.Right)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                case RectangleEdge.Top:
                    if (p.Y < bounds.Top)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                case RectangleEdge.Bottom:
                    if (p.Y >= bounds.Bottom)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

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

        public static Point[] GetLinePoints(Point first, Point second)
        {
            ArrayList coords = new ArrayList();
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
                for (int i = 0; i <= dxabs; i++)
                {
                    y += dyabs;

                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }

                    coords.Add(new Point(px, py));
                    px += sdx;
                }
            }
            else 
            // had to add in this cludge for slopes of 1 ... wasn't drawing half the line
            if (dxabs == dyabs)
            {
                for (int i = 0; i <= dxabs; i++)
                {
                    coords.Add(new Point(px, py));
                    px += sdx;
                    py += sdy;
                }
            }
            else
            {
                for (int i = 0; i <= dyabs; i++)
                {
                    x += dxabs;

                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }

                    coords.Add(new Point(px, py));

                    py += sdy;
                }
            }

            return (Point[])coords.ToArray(typeof(Point));
        }

        public static long GetTimeMs()
        {
            return Utility.TicksToMs(DateTime.Now.Ticks);        
        }

        public static int logicalCpuCount = -1;
        public static int LogicalCpuCount
        {
            get
            {
                if (logicalCpuCount == -1)
                {
                    logicalCpuCount = CpuCount.Info.GetPhysicalCpuCount() * CpuCount.Info.GetLogicalPerPhysicalCpuCount();
                }

                return logicalCpuCount;
            }
        }

        public static int physicalCpuCount = -1;
        public static int PhysicalCpuCount
        {
            get
            {
                if (physicalCpuCount == -1)
                {
                    physicalCpuCount = CpuCount.Info.GetPhysicalCpuCount();
                }

                return physicalCpuCount;
            }
        }

        public static int logicalPerPhysicalCpuCount = -1;
        public static int LogicalPerPhysicalCpuCount
        {
            get
            {
                if (logicalPerPhysicalCpuCount == -1)
                {
                    logicalPerPhysicalCpuCount = CpuCount.Info.GetLogicalPerPhysicalCpuCount();
                }

                return logicalPerPhysicalCpuCount;
            }
        }

        public static bool IsHyperThreadingEnabled
        {
            get
            {
                return CpuCount.Info.IsHTSupported();
            }
        }

#if DEBUG
        //
        public static string Indent(int indentLevel)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 4 * indentLevel; ++i)
            {
                sb.Append(" ");
            }

            return sb.ToString();
        }

        public static void WalkGraph(object walkMe)
        {
            Hashtable walkedObjects = new Hashtable();
            StreamWriter output = new StreamWriter(@"c:\walk.txt");
            output.WriteLine("GC.GetTotalMemory returned " + GC.GetTotalMemory(true).ToString());
            output.WriteLine("Starting walk:");
            
            WalkGraph(output, walkMe, walkedObjects, 0);

            output.Close();
        }

        public static void AddToSet(Hashtable objects, object o)
        {
            Type type = o.GetType();
            Set set = (Set)objects[type];

            if (set == null)
            {
                set = new Set();
                objects[type] = set;
            }

            if (set.Contains(o))
            {
                throw new ArgumentException();
            }

            set.Add(o);
        }

        private unsafe static void WalkGraph(TextWriter output, object walkMe, Hashtable walkedObjects, int indentLevel)
        {
            if (walkMe == null)
            {
                output.WriteLine(Indent(indentLevel) + "(null)");
                return;
            }

            Type walkMeType = walkMe.GetType();
            MemberInfo[] fields = walkMeType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            MemberInfo[] properties = walkMeType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            ArrayList members = new ArrayList();

            if (fields.Length > 0)
            {
                output.WriteLine(Indent(indentLevel) + " fields:");
            }

            foreach (FieldInfo fi in fields)
            {
                object fieldValue = fi.GetValue(walkMe);
                bool traversed = false;

                try
                {
                    if (fieldValue != null)
                    {
                        AddToSet(walkedObjects, fieldValue);
                    }
                }

                catch (ArgumentException)
                {
                    traversed = true;
                }

                output.WriteLine(Indent(indentLevel + 1)  + fi.FieldType.ToString() + " " + fi.Name + " (hash=" + (fieldValue == null ? "n/a" : fieldValue.GetHashCode().ToString()) + ")" + (traversed ? (fieldValue == null ? "" : " (already traversed)") : ":"));

                if (!traversed && fi.FieldType != typeof(void*) && fieldValue != null)
                {
                    WalkGraph(output, fieldValue, walkedObjects, indentLevel + 1);
                }
            }

            if (properties.Length > 0)
            {
                output.WriteLine(Indent(indentLevel) + " properties:");
            }

            foreach (PropertyInfo pi in properties)
            {
                if (pi.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                object fieldValue = null;
                try
                {
                    fieldValue = pi.GetValue(walkMe, new object[0] {} );
                }

                catch
                {
                    output.WriteLine(Indent(indentLevel) + "exception calling PropertyInfo.GetValue(" + pi.Name + ")");
                }


                bool traversed = false;

                try
                {
                    if (fieldValue != null)
                    {
                        AddToSet(walkedObjects, fieldValue);
                    }
                }

                catch (ArgumentException)
                {
                    traversed = true;
                }

                output.WriteLine(Indent(indentLevel + 1)  + pi.PropertyType.ToString() + " " + pi.Name + " (hash=" + (fieldValue == null ? "n/a" : fieldValue.GetHashCode().ToString()) + ")" + (traversed ? (fieldValue == null ? "" : " (already traversed)") : ":"));

                if (!traversed && pi.PropertyType != typeof(void*) && fieldValue != null)
                {
                    WalkGraph(output, fieldValue, walkedObjects, indentLevel + 1);
                }
            }

            if (walkMe is IEnumerable && !(walkMe is string))
            {
                int count = 0;

                output.WriteLine(Indent(indentLevel) + " enumerated items via IEnumerable:");

                foreach(object o in (IEnumerable)walkMe)
                {
                    if (count > 10)
                    {
                        break;
                    }

                    ++count;

                    if (o != null)
                    {
                        bool traversed = false;

                        try
                        {
                            AddToSet(walkedObjects, o);
                        }

                        catch (ArgumentException)
                        {
                            traversed = true;
                        }

                        output.WriteLine(Indent(indentLevel + 1)  + count.ToString() + ": typeof(" + o.GetType().ToString() + ") (hash=" + (o == null ? "n/a" : o.GetHashCode().ToString()) + ")" + (traversed ? (o == null ? "" : " (already traversed)") : ":"));
                        WalkGraph(output, o, walkedObjects, indentLevel + 1);
                    }
                }
            }
        }
        //
#endif
    }
}
