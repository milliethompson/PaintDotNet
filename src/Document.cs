using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.GZip;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for Document.
	/// </summary>
	[Serializable]
	public sealed class Document
        : IDeserializationCallback,
		  IInvalidate,
          IDisposable,
          ICloneable
	{
		private LayerList layers;
		private int width;
		private int height;
		private NameValueCollection userMetaData;

        [NonSerialized]
        private InvalidateEventHandler layerInvalidatedDelegate;

        [NonSerialized]
		private Region updateRegion;

        [NonSerialized]
        private bool dirty;

        /// <summary>
        /// Keeps track of whether the document has changed at all since it was last opened
        /// or saved. This is something that is not reset to true by any method in the Document
        /// class, but is set to false anytime anything is changed.
        /// This way we can prompt the user to save a changed document when they go to quit.
        /// </summary>
        public bool Dirty
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return dirty;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                dirty = value;
            }
        }

        /// <summary>
        /// Returns a copy of the current region that has been changed since the last
        /// time Update() was called.
        /// </summary>
		public Region UpdateRegion
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

				return updateRegion.Clone();
			}
		}

        [NonSerialized]
        private string name;

        /// <summary>
        /// Exposes a collection for access to the layers, and for manipulation of
        /// the way the document contains the layers (add/remove/move).
        /// </summary>
        public LayerList Layers
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return layers;
            }
        }

        /// <summary>
        /// Width of the document, in pixels. All contained layers must be this wide as well.
        /// </summary>
		public int Width
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return width;
			}
		}

        /// <summary>
        /// Height of the document, in pixels. All contained layers must be this tall as well.
        /// </summary>
		public int Height
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return height;
			}
		}

        /// <summary>
        /// The size of the document, in pixels. This is a convenience property that wraps up
        /// the Width and Height properties in one Size structure.
        /// </summary>
		public Size Size
		{
			get
			{
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
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
                    throw new ObjectDisposedException("Document");
                }

                return new Rectangle(0, 0, Width, Height);
            }
        }

        /// <summary>
        /// User-defined metadata consists simply as name-value pairs. For instance, the
        /// user may wish to add "camera=Nokia 300sx" to indicate what camera they took
        /// the picture with. And actually, if we were opening a JPEG created with that
        /// camera, we might just add that for them automatically since many cameras add
        /// certain metadata to the JPEGs they create.
        /// </summary>
        public NameValueCollection UserMetaData
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return userMetaData;
            }
        }

        /// <summary>
        /// The name is not serialized because it just reflects the system's filename.
        /// Therefore we'd rather avoid the possibility of it getting out of sync.
        /// If this is null then the document is untitled, and "Untitled" will be the
        /// name listed in the program's titlebar.
        /// The full path of the filename should be stored in this variable otherwise.
        /// </summary>
        public string Name
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return name;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                name = value;
            }
        }

        /// <summary>
        /// Explicitely renders a requested region of the document.
        /// </summary>
        /// <param name="args">Contains information used to control where rendering occurs.</param>
        /// <param name="roi">The rectangular region to render.</param>
        public void Render(RenderArgs args, Rectangle roi)
		{
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            new UnaryPixelOps.Constant(ColorBgra.FromUInt32(0xffffffff)).Apply(args.Surface, roi);

            foreach (Layer layer in Layers)
            {
				if (layer.Visible)
				{
					layer.Render(args, roi);
				}
            }
		}

        public void Render(RenderArgs args, Rectangle[] roi)
        {
            this.Render(args, roi, 0, roi.Length);
        }

        public void Render(RenderArgs args, Scanline[] scans)
        {
            this.Render(args, scans, 0, scans.Length);
        }

        public void Render(RenderArgs args, Scanline[] scans, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            UnaryPixelOp constant = new UnaryPixelOps.Constant(ColorBgra.FromUInt32(0xffffffff));

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                constant.Apply(args.Surface, scans[i]);
            }

            foreach (Layer layer in Layers)
            {
                if (layer.Visible)
                {
                    for (int i = startIndex; i < startIndex + length; ++i)
                    {
                        layer.Render(args, scans[i]);
                    }
                }
            }
        }

        public void Render(RenderArgs args, Rectangle[] roi, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            UnaryPixelOp constant = new UnaryPixelOps.Constant(ColorBgra.FromUInt32(0xffffffff));

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                constant.Apply(args.Surface, roi[i]);
            }

            foreach (Layer layer in Layers)
            {
                if (layer.Visible)
                {
                    for (int i = startIndex; i < startIndex + length; ++i)
                    {
                        layer.Render(args, roi[i]);
                    }
                }
            }
        }

        public void Render(RenderArgs args, RectangleF[] roi)
        {
            this.Render(args, roi, 0, roi.Length);
        }

        public void Render(RenderArgs args, RectangleF[] roi, int startIndex, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            UnaryPixelOp constant = new UnaryPixelOps.Constant(ColorBgra.FromUInt32(0xffffffff));
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                constant.Apply(args.Surface, Rectangle.Truncate(roi[i]));
            }

            foreach (Layer layer in Layers)
            {
                if (layer.Visible)
                {
                    for (int i = startIndex; i < startIndex + length; ++i)
                    {
                        Rectangle rect = Rectangle.Truncate(roi[i]);
                        layer.Render(args, rect);
                    }
                }
            }
        }

        /// <summary>
        /// Explicitely renders a requested region of the document.
        /// </summary>
        /// <param name="args">Contains information used to control where rendering occurs.</param>
        /// <param name="roi">The Region to render.</param>
        public void Render(RenderArgs args, Region roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            RectangleF[] rectsF = roi.GetRegionScans(Utility.IdentityMatrix);
            this.Render(args, rectsF);
        }   

        private class UpdateScansContext
        {
            private Document document;
            private RenderArgs dst;
            private Scanline[] scans;
            private int startIndex;
            private int length;

            public void UpdateScans(object context)
            {
                document.Render(dst, scans, startIndex, length);
            }

            public UpdateScansContext(Document document, RenderArgs dst, Scanline[] scans, int startIndex, int length)
            {
                this.document = document;
                this.dst = dst;
                this.scans = scans;
                this.startIndex = startIndex;
                this.length = length;
            }
        }
        
        /// <summary>
        /// Renders only the portions of the document that have changed (been Invalidated) since 
        /// the last call to this function.
        /// </summary>
        /// <param name="args">Contains information used to control where rendering occurs.</param>
        public void Update(RenderArgs dst)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            updateRegion.Intersect(Bounds);
            RectangleF[] rectsF = updateRegion.GetRegionScans(Utility.IdentityMatrix);
            Scanline[] scans = Utility.GetRegionScans(rectsF);

            // Split the task of rendering into 2 batches, and use background threads to handle them.
            if (Utility.PhysicalCpuCount == 1)
            {
                UpdateScansContext usc = new UpdateScansContext(this, dst, scans, 0, scans.Length);
                usc.UpdateScans(null);
            }
            else
            {
                UpdateScansContext usc1 = new UpdateScansContext(this, dst, scans, 0, scans.Length / 2);
                UpdateScansContext usc2 = new UpdateScansContext(this, dst, scans, scans.Length / 2, scans.Length - (scans.Length / 2));
                Utility.ThreadPool.QueueUserWorkItem(new WaitCallback(usc1.UpdateScans));
                Utility.ThreadPool.QueueUserWorkItem(new WaitCallback(usc2.UpdateScans));
                Utility.ThreadPool.Drain();
            }

            Validate();
        }

        /// <summary>
        /// Constructs a blank document (zero layers) of the given width and height.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Document(int width, int height)
		{
            this.width = width;
			this.height = height;
            this.name = null;
            this.dirty = true;
            this.updateRegion = new Region();
            this.updateRegion.MakeEmpty();
			layers = new LayerList(this);
            SetupEvents();
			userMetaData = new NameValueCollection();
            Invalidate();
		}

        /// <summary>
        /// Sets up event handling for contained objects.
        /// </summary>
        private void SetupEvents()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            layers.Changed += new EventHandler(LayerListChangedHandler);
            layers.Changing += new EventHandler(LayerListChangingHandler);
            layerInvalidatedDelegate = new InvalidateEventHandler(LayerInvalidatedHandler);

            foreach (Layer l in layers)
            {
                l.Invalidated += layerInvalidatedDelegate;
            }
        }

        /// <summary>
        /// Called after deserialization occurs so that certain things that are non-serializable
        /// can be set up.
        /// </summary>
        /// <param name="sender"></param>
        public void OnDeserialization(object sender)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            updateRegion = new Region(this.Bounds);

            SetupEvents();
            dirty = true;
        }

        [NonSerialized]
        private InvalidateEventHandler invalidated;
        
        /// <summary>
        /// Occurs when a part of the document has changed.
        /// </summary>
        public event InvalidateEventHandler Invalidated
        {
            add
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                invalidated += value;
            }

            remove
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                invalidated -= value;
            }
        }

        /// <summary>
        /// Raises the Invalidated event.
        /// </summary>
        /// <param name="e"></param>
        private void OnInvalidated(InvalidateEventArgs e)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            if (invalidated != null)
            {
                invalidated(this, e);
            }
        }

        /// <summary>
        /// Handles the Changing event that is raised from the contained LayerList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerListChangingHandler(object sender, EventArgs e)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            foreach (Layer layer in Layers)
            {
                layer.Invalidated -= layerInvalidatedDelegate;
            }
        }

        /// <summary>
        /// Handles the Changed event that is raised from the contained LayerList.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerListChangedHandler(object sender, EventArgs e)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            foreach (Layer layer in Layers)
            {
                layer.Invalidated += layerInvalidatedDelegate;
            }

            Invalidate();
        }

        /// <summary>
        /// Handles the Invalidated event that is raised from any contained Layer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerInvalidatedHandler(object sender, InvalidateEventArgs e)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            Invalidate(e.InvalidRect);
        }

        /// <summary>
        /// Causes the whole document to be invalidated, forcing a full rerender on
        /// the next call to Update.
        /// </summary>
        public void Invalidate()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            dirty = true;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            updateRegion.MakeEmpty();

            updateRegion.Union(rect);

            OnInvalidated(new InvalidateEventArgs(rect));
        }

        /// <summary>
        /// Invalidates a portion of the document. The given region is then tagged
        /// for rerendering during the next call to Update.
        /// </summary>
        /// <param name="roi">The region of interest to be invalidated.</param>
        public void Invalidate(Region roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            foreach (RectangleF rectF in roi.GetRegionScans(Utility.IdentityMatrix))
            {
                Invalidate(Rectangle.Truncate(rectF));
            }
        }

        /// <summary>
        /// Invalidates a portion of the document. The given region is then tagged
        /// for rerendering during the next call to Update.
        /// </summary>
        /// <param name="roi">The region of interest to be invalidated.</param>
        public void Invalidate(Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            dirty = true;
            Rectangle rect = Rectangle.Intersect(roi, this.Bounds);
            updateRegion.Union(rect);
            OnInvalidated(new InvalidateEventArgs(rect));
        }

        /// <summary>
        /// Clears the document's update region. This is called at the end of the
        /// Update method.
        /// </summary>
        private void Validate()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            updateRegion.Dispose();
            updateRegion = new Region();
            updateRegion.MakeEmpty();
        }

        /// <summary>
        /// Excludes a portion of the document from the update region.
        /// </summary>
        /// <param name="roi">The region of interest to be validated.</param>
        private void Validate(Region roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            updateRegion.Exclude(roi);
        }

        /// <summary>
        /// Excludes a portion of the document from the update region.
        /// </summary>
        /// <param name="roi">The region of interest to be validated.</param>
        private void Validate(Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            updateRegion.Exclude(roi);
        }

        /// <summary>
        /// Creates a document that consists of one BitmapLayer.
        /// </summary>
        /// <param name="image">The Image to make a copy of that will be the first layer ("Background") in the document.</param>
        public static Document FromImage(Image image)
        {
            Document document = new Document(image.Width, image.Height);
            BitmapLayer layer = Layer.CreateBackgroundLayer(image.Width, image.Height);
            layer.Name = "Background";

            RenderArgs args = new RenderArgs(layer.Surface);
            args.Graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            args.Dispose();

            document.Layers.Add(layer);
            document.Invalidate();
            return document;
        }

        /// <summary>
        /// Deserializes a Document from a stream.
        /// </summary>
        /// <param name="stream">The stream to deserialize from.</param>
        /// <returns>The Document that was stored in stream.</returns>
        public static Document FromStream(Stream stream)
        {
            GZipInputStream gZipStream = new GZipInputStream(stream, 4096);
            Document document = (Document)Utility.DeserializeObjectFromStream(gZipStream);
            document.Dirty = true;
            document.Invalidate();
            return document;
        }

        /// <summary>
        /// Serializes the object to the given data stream.
        /// </summary>
        /// <param name="stream">The Stream to serialize data into. This data will be compressed.</param>
        /// <param name="callback">This can be used to keep track of the number of uncompressed bytes 
        /// that are written. The values reported through the IOEventArgs.Count+Offset will vary from 
        /// 1 to approximately Layers.Count*Width*Height*sizeof(ColorBgra). The final number will actually
        /// be higher because of hierarchical overhead, so make sure to cap any progress reports to 100%.
        /// This callback will be wired to the IOFinished event of a SiphonStream.</param>
        public void SaveToStream(Stream stream, IOEventHandler callback)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            GZipOutputStream gZipStream = new GZipOutputStream(stream, 4096, ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_SPEED);
            SiphonStream siphonStream = new SiphonStream(gZipStream);

            if (callback != null)
            {
                siphonStream.IOFinished += callback;
            }
            
            Utility.SerializeObjectToStream(this, siphonStream);

            if (callback != null)
            {
                siphonStream.IOFinished -= callback;
            }

            gZipStream.Finish();
        }

        public void SaveToStream(Stream stream)
        {
            SaveToStream(stream, null);
        }

        /// <summary>
        /// Returns a new Document that is a flattened version of this one
        /// "Flattened" means it is one layer that is simply a bitmap of
        /// the compositied image.
        /// </summary>
        /// <returns></returns>
        public Document Flatten()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Document");
            }

            Surface surface = new Surface(Width, Height);

            using (RenderArgs renderArgs = new RenderArgs(surface))
            {
                renderArgs.Graphics.Clear(Color.White);
                Render(renderArgs, surface.Bounds);            
                Document newDocument = Document.FromImage(renderArgs.Bitmap);
                newDocument.Name = Name;
                newDocument.userMetaData = new NameValueCollection(userMetaData);

                return newDocument;
            }
        }

        ~Document()
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
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (Layer layer in layers)
                    {
                        layer.Dispose();
                    }

                    updateRegion.Dispose();
                }

                disposed = true;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            // I cheat.
            MemoryStream stream = new MemoryStream();
            SaveToStream(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return Document.FromStream(stream);
        }

        #endregion
    }
}
