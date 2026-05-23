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
    /// This class handles rendering something to a SurfaceBox.
    /// </summary>
    public abstract class SurfaceBoxRenderer
        : IDisposable
    {
        private SurfaceBoxRendererList ownerList;
        private bool visible;

        protected object SyncRoot
        {
            get
            {
                return OwnerList.SyncRoot;
            }
        }

        protected SurfaceBoxRendererList OwnerList
        {
            get
            {
                return this.ownerList;
            }
        }

        public virtual void OnSourceSizeChanged()
        {
        }

        public virtual void OnDestinationSizeChanged()
        {
        }

        public Size SourceSize
        {
            get
            {
                return this.OwnerList.SourceSize;
            }
        }

        public Size DestinationSize
        {
            get
            {
                return this.OwnerList.DestinationSize;
            }
        }

        protected abstract void OnVisibleChanged();

        public bool Visible
        {
            get
            {
                return this.visible;
            }

            set
            {
                if (this.visible != value)
                {
                    this.visible = value;
                    OnVisibleChanged();
                }
            }
        }
        
        protected delegate void RenderDelegate(Surface dst, Point offset);

        /// <summary>
        /// Renders, at the appropriate scale, the layer's imagery.
        /// </summary>
        /// <param name="dst">The Surface to render to.</param>
        /// <param name="dstTranslation">The (x,y) location of the upper-left corner of dst within DestinationSize.</param>
        public abstract void Render(Surface dst, Point offset);

        protected virtual void OnInvalidate(Rectangle rect)
        {
            this.OwnerList.Invalidate(rect);
        }

        public void Invalidate(Rectangle rect)
        {
            OnInvalidate(rect);
        }

        public void Invalidate(RectangleF rectF)
        {
            Rectangle rect = Utility.RoundRectangle(rectF);
            Invalidate(rect);
        }

        public void Invalidate(PdnRegion region)
        {
            foreach (Rectangle rect in region.GetRegionScansReadOnlyInt())
            {
                Invalidate(rect);
            }
        }

        public void Invalidate()
        {
            Invalidate(new Rectangle(0, 0, SourceSize.Width, SourceSize.Height));
        }

        public SurfaceBoxRenderer(SurfaceBoxRendererList ownerList)
        {
            this.ownerList = ownerList;
            this.visible = true;
        }

        ~SurfaceBoxRenderer()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
