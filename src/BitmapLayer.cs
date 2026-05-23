using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Threading;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for BitmapLayer.
	/// </summary>
    [Serializable]
    public class BitmapLayer
        : Layer,
          IDeserializationCallback
	{
        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                try
                {
                    surface.Dispose();
                    surface = null;
                }
                    
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        private IPixelOp compiledBlendOp = null;

        /// <summary>
        /// This handles the case when blendOp is null, but opacity is not equal to 255
        /// </summary>
        [Serializable]
        private class BlendWithOpacityOp
            : BinaryPixelOp
        {
            private int opacity;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                int a = (rhs.a * opacity) / 256;
                int invA = 256 - a;

                int r = ((invA * lhs.r) + (a * rhs.r)) / 256;
                int g = ((invA * lhs.g) + (a * rhs.g)) / 256;
                int b = ((invA * lhs.b) + (a * rhs.b)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, lhs.a);
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int a = ((rhs->a * opacity) / 256);
                    int invA = 256 - a;

                    int r = ((invA * lhs->r) + (a * rhs->r)) / 256;
                    int g = ((invA * lhs->g) + (a * rhs->g)) / 256;
                    int b = ((invA * lhs->b) + (a * rhs->b)) / 256;

                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)lhs->a << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int a = ((src->a * opacity) / 256);
                    int invA = 256 - a;

                    int r = ((invA * dst->r) + (a * src->r)) / 256;
                    int g = ((invA * dst->g) + (a * src->g)) / 256;
                    int b = ((invA * dst->b) + (a * src->b)) / 256;

                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)dst->a << 24));

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public BlendWithOpacityOp(int opacity)
            {
                this.opacity = opacity;
            }
        }

        /// <summary>
        /// This handles the case when blendOp is not null, and opacity is not 255
        /// </summary>
        [Serializable]
        private class BlendWithBlendOpAndOpacityOp
            : BinaryPixelOp
        {
            private int opacity;
            private BinaryPixelOp op;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                ColorBgra rhs2 = op.Apply(lhs, rhs);

                int a = (rhs2.a * opacity) / 256;
                int invA = 256 - a;

                int r = ((invA * lhs.r) + (a * rhs2.r)) / 256;
                int g = ((invA * lhs.g) + (a * rhs2.g)) / 256;
                int b = ((invA * lhs.b) + (a * rhs2.b)) / 256;

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, rhs2.a);
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    ColorBgra rhs2 = op.Apply(*lhs, *rhs);

                    int a = ((rhs2.a * opacity) / 256);
                    int invA = 256 - a;

                    int r = ((invA * lhs->r) + (a * rhs2.r)) / 256;
                    int g = ((invA * lhs->g) + (a * rhs2.g)) / 256;
                    int b = ((invA * lhs->b) + (a * rhs2.b)) / 256;

                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + (rhs2.a << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    ColorBgra src2 = op.Apply(*dst, *src);

                    int a = ((src2.a * opacity) / 256);
                    int invA = 256 - a;

                    int r = ((invA * dst->r) + (a * src2.r)) / 256;
                    int g = ((invA * dst->g) + (a * src2.g)) / 256;
                    int b = ((invA * dst->b) + (a * src2.b)) / 256;

                    dst->bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)src2.a << 24));

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public BlendWithBlendOpAndOpacityOp(int opacity, BinaryPixelOp op)
            {
                this.opacity = opacity;
                this.op = op;
            }
        }

        private void CompileBlendOp()
        {
            bool isDefaultOp = (properties.blendOp.GetType() == UserBlendOps.GetDefaultBlendOp());

            if (isDefaultOp && properties.opacity == 255)
            {
                compiledBlendOp = new BinaryPixelOps.AlphaBlend();
            }
            else if (isDefaultOp && properties.opacity != 255)
            {
                compiledBlendOp = new BitmapLayer.BlendWithOpacityOp(properties.opacity);
            }
            else if (!isDefaultOp && properties.opacity == 255)
            {
                compiledBlendOp = properties.blendOp;
            }
            else if (!isDefaultOp && properties.opacity != 255)
            {
                compiledBlendOp = new BitmapLayer.BlendWithBlendOpAndOpacityOp(properties.opacity, properties.blendOp);
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            compiledBlendOp = null;
            base.OnPropertyChanged (propertyName);
        }

		[NonSerialized]
		private int needToUpdatePreview = 0; // set to non-zero when the preview should be updated

		[Serializable]
		private class BitmapLayerProperties
			: ICloneable
		{
			public UserBlendOp blendOp;
            public byte opacity;

			public BitmapLayerProperties(UserBlendOp blendOp, byte opacity)
			{
				this.blendOp = blendOp;
                this.opacity = opacity;
			}

			public BitmapLayerProperties(BitmapLayerProperties cloneMe)
			{
				this.blendOp = cloneMe.blendOp;
                this.opacity = cloneMe.opacity;
			}

			#region ICloneable Members

			public object Clone()
			{
				return new BitmapLayerProperties(this);
			}

			#endregion
		}

		private BitmapLayerProperties properties;
		private Surface surface;

		public override object SaveProperties()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            object baseProperties = base.SaveProperties();
			return new List(properties.Clone(), new List(baseProperties, null));
		}

		public override void LoadProperties(object oldState, bool suppressEvents)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            List list = (List)oldState;
            base.LoadProperties(list.Tail.Head, suppressEvents);

            BitmapLayerProperties blp = (BitmapLayerProperties)(((List)oldState).Head);
            bool raiseBlendOp = false;
            bool raiseOpacity = false;

            if (blp.blendOp.GetType() != properties.blendOp.GetType())
            {
                if (!suppressEvents)
                {
                    raiseBlendOp = true;
                    OnPropertyChanging("Blend Mode");
                }
            }

            if (blp.opacity != properties.opacity)
            {
                if (!suppressEvents)
                {
                    raiseOpacity = true;
                    OnPropertyChanging("Opacity");
                }
            }

            this.properties = (BitmapLayerProperties)blp.Clone();
            this.compiledBlendOp = null;

            Invalidate();

            if (raiseBlendOp)
            {
                OnPropertyChanged("Blend Mode");
            }

            if (raiseOpacity)
            {
                OnPropertyChanged("Opacity");
            }
    	}

        public void SetBlendOp(UserBlendOp blendOp)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (blendOp.GetType() != properties.blendOp.GetType())
            {
                OnPropertyChanging("Blend Mode");
                properties.blendOp = blendOp;
                compiledBlendOp = null;
                Invalidate();
                OnPropertyChanged("Blend Mode");
            }
        }

		public override object Clone()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            return (object)new BitmapLayer(this);
		}

		public void UpdatePreview()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (Surface != null)
            {
                int previewSide = 30;
                Size previewSize;

                // decide size ... are we 'tall' or 'wide' ?
                if (Width > Height)
                {   // wide
                    previewSize = new Size(previewSide, (Height * previewSide) / Width);
                }
                else
                {   
                    previewSize = new Size((Width * previewSide) / Height, previewSide);
                }

                Surface surface = new Surface(previewSide, previewSide);
                new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 255)).Apply(surface, surface.Bounds);
                Surface previewWindow = surface.CreateWindow(new Rectangle(new Point((previewSide - previewSize.Width) / 2, (previewSide - previewSize.Height) / 2), previewSize));
                previewWindow.SuperSamplingFitSurface(Surface);
                previewWindow.Dispose();

                Bitmap bitmap = new Bitmap(surface.Width, surface.Height);

                for (int y = 0; y < bitmap.Height; ++y)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        bitmap.SetPixel(x, y, surface[x,y].ToColor());
                    }
                }

                surface.Dispose();
                this.Preview = bitmap;
            }
        }

        [NonSerialized]
        public object previewLock = new object();

		public void UpdatePreviewHandler(object context)
		{
            int delay = (int)context;

            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            lock (previewLock) // make sure to serialize preview updates ... sometimes we get more than one in there
            {
                ThreadPriority oldPriority = Thread.CurrentThread.Priority;
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                while (this.SuppressPreviewChanges)
                {
                    Thread.Sleep(delay);
                }

                this.needToUpdatePreview = 0;
                UpdatePreview();
                Thread.CurrentThread.Priority = oldPriority;
            }
		}

		public Surface Surface
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return surface;
			}
		}

		public UserBlendOp BlendOp
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return properties.blendOp;
			}
		}

        public byte Opacity
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return properties.opacity;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                if (properties.opacity != value)
                {
                    OnPropertyChanging("Opacity");
                    properties.opacity = value;
                    OnPropertyChanged("Opacity");
                    Invalidate();
                }
            }
        }

		protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            base.OnInvalidated (e);
            if (1 == Interlocked.Increment(ref this.needToUpdatePreview))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdatePreviewHandler), 250);
            }
		}

		public BitmapLayer(int width, int height)
            : base(width, height)
		{
			this.surface = new Surface(width, height);
			// clear to see-through white, 0x00ffffff
			new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 0)).Apply(Surface, Surface.Bounds);
			this.properties = new BitmapLayerProperties(UserBlendOps.CreateBlendOp(UserBlendOps.GetDefaultBlendOp()), 255);
        }

        /// <summary>
        /// Creates a new BitmapLayer of the same size as the given Surface, and copies the 
        /// pixels from the given Surface.
        /// </summary>
        /// <param name="surface">The Surface to copy pixels from.</param>
		public BitmapLayer(Surface surface)
			: base(surface.Width, surface.Height)
		{
			this.surface = surface.CopyContents();
			this.properties = new BitmapLayerProperties(UserBlendOps.CreateBlendOp(UserBlendOps.GetDefaultBlendOp()), 255);
		}

		private BitmapLayer(BitmapLayer copyMe)
			: base(copyMe)
		{
			this.surface = copyMe.Surface.CopyContents();
			this.properties = (BitmapLayerProperties)copyMe.properties.Clone();
		}

        public BitmapLayer(Image image)
            : base(image.Width, image.Height)
        {
            using (Bitmap bitmap = Surface.CreateAliasedBitmap())
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
		}

		/// <summary>
		/// Creates a HistoryAction by saving the requested region.
		/// </summary>
		/// <param name="name">The name that will show up in the History box.</param>
		/// <param name="image">The icon that will show up in the History box.</param>
		/// <param name="roi">The region that you want to save.</param>
		/// <returns></returns>
        public HistoryAction CreateHistoryAction(string name, Image image, Region roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            return (HistoryAction)new BitmapLayerHistoryAction(name, image, this, roi);
        }

		/// <summary>
		/// Creates a HistoryAction using a region that you have already saved.
		/// </summary>
		/// <param name="name">The name that will show up in the History box.</param>
		/// <param name="image">The icon that will show up in the History box.</param>
		/// <param name="saved">The region of the layer that you have already saved.</param>
		/// <returns></returns>
		public HistoryAction CreateHistoryAction(string name, Image image, IrregularSurface saved)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            return (HistoryAction)new BitmapLayerHistoryAction(name, image, this, saved);
		}

        public override void Render(RenderArgs args, Scanline roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }
                        
            base.Render (args, roi);

            if (compiledBlendOp == null)
            {
                CompileBlendOp();
            }

            compiledBlendOp.Apply(args.Surface, roi.Point, this.Surface, roi.Point, roi.Length);
        }

        public override System.Windows.Forms.Form CreateConfigDialog()
        {
            BitmapLayerPropertiesDialog blpd = new BitmapLayerPropertiesDialog();
            blpd.Layer = this;
            return blpd;
        }

        private class BitmapLayerHistoryAction
            : HistoryAction
        {
            private BitmapLayer layer;
            private IrregularSurface undoImage;

            public BitmapLayerHistoryAction(string name, Image image, BitmapLayer layer, Region changedRegion)
                : base(name, image)
            {
                this.layer = layer;

                using (Region r = changedRegion.Clone())
                {
                    r.Intersect(layer.Bounds);
                    undoImage = new IrregularSurface(this.layer.Surface, r);
                }
            }

			public BitmapLayerHistoryAction(string name, Image image, BitmapLayer layer, IrregularSurface saved)
				: base(name, image)
			{
				this.layer = layer;
				this.undoImage = (IrregularSurface)saved.Clone();
			}

            protected override HistoryAction OnUndo()
            {
                if (undoImage == null)
                {
                    throw new InvalidOperationException("BitmapLayerHistoryAction was used twice");
                }

                BitmapLayerHistoryAction redo = new BitmapLayerHistoryAction(Name, Image, layer, undoImage.Region);
				redo.id = id;

				undoImage.Draw(this.layer.Surface);

                Utility.FastInvalidate(layer, undoImage.Region, 10);

                undoImage.Dispose();
                undoImage = null;
                return redo;
            }
        }

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            this.previewLock = new object();
        }

        #endregion
    }
}
