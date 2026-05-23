using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// A layer's properties are immutable. That is, you can modify the surface
	/// of a layer all you want, but to change its dimensions requires creating
	/// a new layer.
	/// </summary>
	[Serializable]
	public abstract class Layer
		: IInvalidate,
		  ICloneable,
          IDisposable
	{
		private int width;
		private int height;

        public bool IsBackground
        {
            get
            {
                return properties.isBackground;
            }

			set
			{
				properties.isBackground = false;
			}
        }

        /// <summary>
        /// If this value is 0, then preview updates should be suppressed
        /// until this value is set back to 0. This is a performance 
        /// optimization used when rendering an effect.
        /// </summary>
        [NonSerialized]
        private int suppressPreviewChanges;

		/// <summary>
		/// Encapsulates the mutable properties of the Layer class.
		/// </summary>
		[Serializable]
		private class LayerProperties
			: ICloneable
		{
			public string name;
			public NameValueCollection userMetaData;
			public bool visible;
			public bool isBackground;

			public LayerProperties(string name, NameValueCollection userMetaData, bool visible, bool isBackground)
			{
				this.name = name;
				this.userMetaData = new NameValueCollection(userMetaData);
				this.visible = visible;
				this.isBackground = isBackground;
			}

			public LayerProperties(LayerProperties copyMe)
			{
				this.name = copyMe.name;
				this.userMetaData = new NameValueCollection(copyMe.userMetaData);
				this.visible = copyMe.visible;
				this.isBackground = copyMe.isBackground;
			}
			#region ICloneable Members

			public object Clone()
			{
				return new LayerProperties(this);
			}

			#endregion
		}

		private LayerProperties properties;

		/// <summary>
		/// Allows you to save the mutable properties of the layer so you can restore them later
		/// (esp. important for undo!). Mutable properties include the layer's name, whether it's
		/// visible, and the metadata. This list might expand later.
		/// </summary>
		/// <returns>An object that can be used later in a call to LoadProperties.</returns>
		public virtual object SaveProperties()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

			return properties.Clone();
		}

        public void LoadProperties(object oldState)
        {
            LoadProperties(oldState, false);
        }
        
        public virtual void LoadProperties(object oldState, bool suppressEvents)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            LayerProperties lp = (LayerProperties)oldState;
            bool raiseName = false;
            bool raiseVisible = false;

            if (!suppressEvents)
            {
                if (lp.name != properties.name)
                {
                    OnPropertyChanging("Name");
                    raiseName = true;
                }

                if (lp.visible != properties.visible)
                {
                    OnPropertyChanging("Visibility");
                    raiseVisible = true;
                }
            }

            properties = (LayerProperties)((LayerProperties)oldState).Clone();

            Invalidate();

            if (raiseName)
            {
                OnPropertyChanged("Name");
            }

            if (raiseVisible)
            {
                OnPropertyChanged("Visibility");
            }
		}

		public int Width
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return width;
			}
		}

		public int Height
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return height;
			}
		}

		public Size Size
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return new Size(Width, Height);
			}
		}

		public Rectangle Bounds
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return new Rectangle(new Point(0, 0), Size);
			}
		}

		[NonSerialized]
		private Image preview = null;
		public Image Preview
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return preview;
			}

			set
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                preview = value;
				OnPreviewChanged();
			}
		}

        protected bool SuppressPreviewChanges
        {
            get
            {
                return suppressPreviewChanges > 0;
            }
        }

        public void PushSuppressPreviewChanges()
        {
            Interlocked.Increment(ref suppressPreviewChanges);
        }

        public void PopSuppressPreviewChanges()
        {
            if (0 > Interlocked.Decrement(ref suppressPreviewChanges))
            {
                throw new InvalidProgramException("suppressPreviewChanged is less than zero");
            }
        }

		[NonSerialized]
		private EventHandler previewChanged;

        /// <summary>
        /// Signifies that the layer's preview has changed. Note that this event
        /// is not guaranteed to be on the main thread, so you should not assume
        /// that, and use Invoke as necessary.
        /// </summary>
		public event EventHandler PreviewChanged
		{
			add
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                previewChanged += value;
			}

			remove
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                previewChanged -= value;
			}
		}

		protected virtual void OnPreviewChanged()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            if (previewChanged != null)
			{
				previewChanged(this, EventArgs.Empty);
			}
		}

        /// <summary>
        /// This event is raised before a property is changed. Note that the name given
        /// in the PropertyEventArgs is for descriptive (UI) purposes only and serves no
        /// programmatic purpose. When this event is raised you should not make any
        /// assumptions about which property was changed based on this description/
        /// </summary>
        [NonSerialized]
        private PropertyEventHandler propertyChanging;
        public event PropertyEventHandler PropertyChanging
        {
            add
            {
                propertyChanging += value;
            }

            remove
            {
                propertyChanging -= value;
            }
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (propertyChanging != null)
            {
                propertyChanging(this, new PropertyEventArgs(propertyName));
            }
        }

        /// <summary>
        /// This event is raised after a property is changed. Note that the name given
        /// in the PropertyEventArgs is for descriptive (UI) purposes only and serves no
        /// programmatic purpose. When this event is raised you should not make any
        /// assumptions about which property was changed based on this description/
        /// </summary>
        [NonSerialized]
        private PropertyEventHandler propertyChanged;
        public event PropertyEventHandler PropertyChanged
        {
            add
            {
                propertyChanged += value;
            }

            remove
            {
                propertyChanged -= value;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyEventArgs(propertyName));
            }
        }

        /// <summary>
        /// You can call this to raise the PropertyChanged event. Note that is will
        /// raise the event with an empty string for the property name description.
        /// Thus it is useful only for syncing up UI elements that require notification
        /// of events but that otherwise don't really track it.
        /// </summary>
        public void PerformPropertyChanged()
        {
            OnPropertyChanged(string.Empty);
        }

		/// <summary>
		/// A user-definable name.
		/// </summary>
		public string Name
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return properties.name;
			}

			set
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                if (properties.name != value)
                {
                    OnPropertyChanging("Name");
                    properties.name = value;
                    OnPropertyChanged("Name");
                }
			}
		}

		public NameValueCollection UserMetaData
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return properties.userMetaData;
			}
		}

		/// <summary>
		/// Determines whether the layer is part of a document's composition. If this
		/// property is false, the composition engine will ignore this layer.
		/// </summary>
		public bool Visible
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                return properties.visible;
			}
			set
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                bool oldValue = properties.visible;


				if (oldValue != value)
				{
                    OnPropertyChanging("Visibility");
                    properties.visible = value;
                    OnPropertyChanged("Visibility");
                    Invalidate();
				}
			}
		}


		/// <summary>
		/// Determines whether a rectangle is fully in bounds or not. This is determined by checking
		/// to make sure the left, top, right, and bottom edges are within bounds.
		/// </summary>
		/// <param name="roi"></param>
		/// <returns></returns>
		private bool IsInBounds(Rectangle roi)
		{
			if (roi.Left < 0 || roi.Top < 0 || roi.Left > Width || roi.Top > Height ||
				roi.Right > Width || roi.Bottom > Height)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Causes the layer to render a given rectangle of interest (roi) to the given destination surface.
		/// Note: Override this method to provide your layer's rendering capabilities.
		/// </summary>
		/// <param name="args">Contains information about which objects to use for rendering</param>
		/// <param name="roi">The rectangular region to be rendered.</param>
		public void Render(RenderArgs args, Rectangle roi)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            // the bitmap we're rendering to must match the size of the layer we're rendering from
			if (args.Surface.Width != Width || args.Surface.Height != Height)
			{
				throw new SizeMismatchException();
			}

			// the region of interest can not be out of bounds!
			if (!IsInBounds(roi))
			{
				throw new ArgumentOutOfRangeException("roi");
			}

            for (int y = 0; y < roi.Height; ++y)
            {
                Render(args, new Scanline(new Point(roi.X, roi.Y + y), roi.Width));
            }
		}

        public virtual void Render(RenderArgs args, Scanline scan)
        {

        }

		/// <summary>
		/// Causes the layer to render a given region of interest (roi) to the given destination surface.
		/// </summary>
		/// <param name="args">Contains information about which objects to use for rendering</param>
		/// <param name="roi">The region to be rendered.</param>
		public void Render(RenderArgs args, Region roi)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            RectangleF[] rectsF = roi.GetRegionScans(Utility.IdentityMatrix);

			foreach (RectangleF rectF in rectsF)
			{
				Rectangle rect = Rectangle.Truncate(rectF);
				Render(args, rect);
			}
		}

		[NonSerialized]
		private InvalidateEventHandler invalidated;
		public event InvalidateEventHandler Invalidated
		{
			add
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                invalidated += value;
			}

			remove
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Layer");
                }

                invalidated -= value;
			}
		}

		protected virtual void OnInvalidated(InvalidateEventArgs e)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            if (invalidated != null)
			{
				invalidated(this, e);
			}
		}

		/// <summary>
		/// Causes the entire layer surface to be invalidated.
		/// </summary>
		public void Invalidate()
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            Rectangle rect = new Rectangle(0, 0, Width, Height);
			OnInvalidated(new InvalidateEventArgs(rect));
		}

		/// <summary>
		/// Causes a portion of the layer surface to be invalidated.
		/// Implements IInvalidate.
		/// </summary>
		/// <param name="roi">The region of interest to be invalidated.</param>
		public void Invalidate(Region roi)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            foreach (RectangleF rectF in roi.GetRegionScans(Utility.IdentityMatrix))
			{
                Invalidate(Rectangle.Truncate(rectF));
			}
		}

        /// <summary>
        /// Causes a portion of the layer surface to be invalidated.
        /// Implements IInvalidate.
        /// </summary>
        /// <param name="roi">The region of interest to be invalidated.</param>
        public void Invalidate(RectangleF[] roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            foreach (RectangleF rectF in roi)
            {
                Invalidate(Rectangle.Truncate(rectF));
            }        
        }

		/// <summary>
		/// Causes a portion of the layer surface to be invalidated.
		/// Implements IInvalidate.
		/// </summary>
		/// <param name="roi">The rectangle of interest to be invalidated.</param>
		public void Invalidate(Rectangle roi)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Layer");
            }

            Rectangle rect = Rectangle.Intersect(roi, this.Bounds);
            OnInvalidated(new InvalidateEventArgs(rect));
		}

		public Layer(int width, int height)
		{
			this.width = width;
			this.height = height;
			this.properties = new LayerProperties(null, new NameValueCollection(), true, false);
            this.suppressPreviewChanges = 0;
			Invalidate();
		}

		protected Layer(Layer copyMe)
		{
			this.width = copyMe.width;
			this.height = copyMe.height;
			this.properties = (LayerProperties)copyMe.properties.Clone();
		}

        public static BitmapLayer CreateBackgroundLayer(int width, int height)
        {
            BitmapLayer layer = new BitmapLayer(width, height);
            layer.Name = "Background";
            //layer.ForcedVisible = true;

            // set colors to 0xffffffff
            // note: we use alpha of 255 here so that "invert colors" works as expected
            // that is, for just 1 layer we invert the initial white->black
            // but on subsequent layers we invert transparent white -> transparent black, which shows up as white for the most part
            new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 255)).Apply(layer.Surface, layer.Bounds);

            // make sure the alpha channel always renders as 255
            //layer.SetRenderOp(new UnaryPixelOps.SetAlphaChannelTo255());
            //layer.SetRenderOp(new BinaryPixelOps.AlphaBlend());

            // tag it as a background layer
            // This simply prevents its name from being changed
			// HACK: kind of a hack? we're reaching behind its back? wiggidy wiggidy wack?
            layer.properties.isBackground = true;

            return layer;
        }

        /// <summary>
        /// This allows a layer to provide a dialog for configuring
        /// the layer's properties.
        /// </summary>
        public abstract Form CreateConfigDialog();

		#region ICloneable Members

		public abstract object Clone();

		#endregion

        ~Layer()
        {
            Dispose(false);
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                    if (preview != null)
                    {
                        preview.Dispose();
                        preview = null;
                    }
                }
            }
        }
        #endregion
    }
}
