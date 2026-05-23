/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates the arguments passed to a Render function.
    /// This way we can do on-demand and once-only creation of Bitmap and Graphics
    /// objects from a given Surface object.
    /// </summary>
    /// <remarks>
    /// Use of the Bitmap and Graphics objects is not thread safe because of how GDI+ works.
    /// You must wrap use of these objects with a critical section, like so:
    ///     object lockObject = new object();
    ///     lock (lockObject)
    ///     {
    ///         Graphics g = ra.Graphics;
    ///         g.DrawRectangle(...);
    ///         // etc.
    ///     }
    /// </remarks>
    public class RenderArgs
        : IDisposable
    {
        private Surface surface;

        /// <summary>
        /// Gets the Surface that has been associated with this instance of RenderArgs.
        /// </summary>
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

        /// <summary>
        /// Gets a Bitmap reference that aliases the Surface.
        /// </summary>
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

        /// <summary>
        /// Retrieves a Graphics instance that can be used to draw on to the Surface.
        /// </summary>
        /// <remarks>
        /// Use of this object is not thread-safe. You must wrap retrieval and consumption of this 
        /// property with a critical section.
        /// </remarks>
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

        /// <summary>
        /// Gets the size of the associated Surface object.
        /// </summary>
        /// <remarks>
        /// This is a convenience method equivalent to using RenderArgs.Surface.Bounds.
        /// </remarks>
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

        /// <summary>
        /// Creates an instance of the RenderArgs class.
        /// </summary>
        /// <param name="surface">The Surface to associate with this instance. This instance of RenderArgs does not take ownership of this Surface.</param>
        public RenderArgs(Surface surface)
        {
            this.surface = surface;
            this.bitmap = null;
            this.graphics = null;
        }

        #region IDisposable Members

        ~RenderArgs()
        {
            Dispose(false);
        }

        private bool disposed = false;

        /// <summary>
        /// Disposes of the contained Bitmap and Graphics instances, if necessary.
        /// </summary>
        /// <remarks>
        /// Note that since this class does not take ownership of the Surface, it
        /// is not disposed.
        /// </remarks>
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
                    if (graphics != null)
                    {
                        graphics.Dispose();
                        graphics = null;
                    }

                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        bitmap = null;
                    }
                }
            }
        }
        #endregion
    }
}
