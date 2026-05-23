using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates a bitmap ("what") along with a pixel offset ("where") which 
    /// defines where the bitmap would be drawn on to another image.
    /// Instances of this object are immutable -- once you create it, you can not
    /// change it.
    /// </summary>
    [Serializable]
    public class PlacedImage
        : IDisposable,
          ICloneable
    {
        Point where;
        Image what;
        bool ours; // indicates whether or not we allocated the 'what' (usually true)

        public Point Where
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedImage");
                }

                return where;
            }
        }

        public Image What
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedImage");
                }

                return what;
            }
        }

        public bool Aliased
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedImage");
                }

                return !ours;
            }
        }

        public Size Size
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedImage");
                }

                return What.Size;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("PlacedImage");
                }

                return new Rectangle(Where, Size);
            }
        }

        public void Draw(Graphics g)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedImage");
            }

            InterpolationMode oldIM = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(what, where.X, where.Y, what.Width, what.Height);
            g.InterpolationMode = oldIM;
        }

        public void Draw(Graphics g, int transformX, int transformY)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedImage");
            }

            Point oldWhere = where;

            try
            {
                where.X += transformX;
                where.Y += transformY;
                Draw(g);
            }

            finally
            {
                where = oldWhere;
            }
        }

        public PlacedImage (Image source, Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedImage");
            }

            where = new Point(roi.X, roi.Y);

            if (source is Bitmap)
            {
                what = ((Bitmap)source).Clone(roi, source.PixelFormat);
                ours = true;
            }
            else
            {
                what = (Bitmap)new Bitmap(roi.Width, roi.Height, source.PixelFormat);
                ours = true;

                using (Graphics g = Graphics.FromImage(what))
                {
                    g.DrawImage(source, new Rectangle(0, 0, what.Width, what.Height), roi, GraphicsUnit.Pixel);
                }
            }
        }

        private PlacedImage (PlacedImage pi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedImage");
            }

            where = pi.where;

            if (pi.ours)
            {
                what = (Image)pi.what.Clone();
                ours = true;
            }
            else
            {
                what = pi.what;
                ours = false;
            }
        }

        private PlacedImage()
        {
        }

        ~PlacedImage()
        {
            Dispose(false);
        }

        public static PlacedImage CreateAliased (Image what, Point where)
        {
            PlacedImage pi = new PlacedImage();

            pi.what = what;
            pi.where = where;
            pi.ours = false;

            return pi;
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

                if (disposing && ours)
                {
                    what.Dispose();
                    what = null;
                }
            }
        }
        #endregion

        #region ICloneable Members

        /// <summary>
        /// If this PlacedImage was created 'aliased', then the contained image will not
        /// be cloned, but will still be aliased.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("PlacedImage");
            }

            return new PlacedImage(this);
        }

        #endregion
    }
}
