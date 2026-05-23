using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// Defines an image that is irregularly shaped, defined by a Region.
	/// Works by containing an array of PlacedImage instances.
	/// </summary>
    [Serializable]
    public sealed class IrregularImage
        : IDisposable,
          ICloneable
	{
		private ArrayList placedImages;
		private Region region;

        /// <summary>
        /// The Region that the irregular image fills.
        /// </summary>
		public Region Region
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("IrregularImage");
                }

				return region.Clone();
			}
		}

        public void Add (Image source, Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            using (Region newRoi = new Region(roi))
            {
                Add(source, newRoi);
            }
        }

        public void Add (Image source, Region roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            using (Region newRoi = roi.Clone())
            {
                newRoi.Exclude(this.region);
            
                foreach (RectangleF rectF in newRoi.GetRegionScans(Utility.IdentityMatrix))
                {
                    Add(new PlacedImage(source, Rectangle.Truncate(rectF)));
                }
            }
        }

        public void Add (PlacedImage pi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            placedImages.Add(pi.Clone());
            region.Union(pi.Bounds);
        }

        /// <summary>
        /// Constructs a blank IrregularImage.
        /// </summary>
        public IrregularImage ()
        {
            Create();
        }

        /// <summary>
        /// Constructs an IrregularImage by copying the given region-of-interest from an Image.
        /// </summary>
        /// <param name="source">The Image to copy pixels from.</param>
        /// <param name="roi">Defines the Region from which to copy pixels from the Image.</param>
        public IrregularImage (Image source, Region roi)
            : this()
		{   
            Add(source, roi);
		}

        /// <summary>
        /// Constructs an IrregularImage by copying the given rectangle-of-interest from an Image.
        /// </summary>
        /// <param name="source">The Image to copy pixels from.</param>
        /// <param name="roi">Defines the Rectangle from which to copy pixels from the Image.</param>
        public IrregularImage (Image source, Rectangle roi)
            : this()
        {
            Add(source, roi);
        }

        private void Create()
        {
            placedImages = new ArrayList();
            region = new Region();
            region.MakeEmpty();
        }

        ~IrregularImage()
        {
            Dispose(false);
        }

        /// <summary>
        /// Draws the IrregularImage using the given Graphics object.
        /// </summary>
        /// <param name="g">The Graphics object to draw to.</param>
		public void Draw(Graphics g)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            foreach (PlacedImage pi in placedImages)
			{
				pi.Draw(g);
			}
        }

        /// <summary>
        /// Draws the IrregularImage using the given Graphics object and an offset.
        /// </summary>
        /// <param name="g">The Graphics object to draw to.</param>
        /// <param name="transformX">The value to be added to every X coordinate that is used for drawing.</param>
        /// <param name="transformY">The value to be added to every Y coordinate that is used for drawing.</param>
        public void Draw(Graphics g, int transformX, int transformY)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            foreach (PlacedImage pi in placedImages)
            {
                pi.Draw(g, transformX, transformY);
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
                    foreach(PlacedImage pi in placedImages)
                    {
                        pi.Dispose();
                    }

                    placedImages.Clear();
                    placedImages = null;
                }
            }
        }
        #endregion

        #region ICloneable Members
        public object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("IrregularImage");
            }

            IrregularImage ii = new IrregularImage();

            foreach (PlacedImage pi in placedImages)
            {
                ii.Add(pi);
            }

            return ii;
        }
        #endregion
    }
}
