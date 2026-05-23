using System;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// Encapsulates the arguments passed to a Render function.
	/// This way we can do on-demand and once-only creation of Bitmap and Graphics
	/// objects from a given Surface object.
	/// </summary>
	public class RenderArgs
        : IDisposable
	{
        private Surface surface;
        public Surface Surface
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                return surface;
            }
        }

        private Bitmap bitmap;
        public Bitmap Bitmap
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                if (bitmap == null)
                {
                    bitmap = surface.CreateAliasedBitmap();
                }

                return bitmap;
            }
        }

        private Graphics graphics;
        public Graphics Graphics
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }

                if (graphics == null)
                {
                    graphics = Graphics.FromImage(Bitmap);
                }

                return graphics;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("RenderArgs");
                }
                
                return Surface.Bounds;
            }
        }

        public RenderArgs(Surface surface)
        {
            this.surface = surface;
            this.bitmap = null;
            this.graphics = null;
            return;
        }

        #region IDisposable Members

        ~RenderArgs()
        {
            Dispose(false);
        }

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
                    Utility.Dispose(graphics);
                    graphics = null;
                    Utility.Dispose(bitmap);
                    bitmap = null;
                }
            }
        }
        #endregion
	}
}
