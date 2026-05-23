using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for Document.
    /// </summary>
    [Serializable]
    public sealed class Document
        : IDeserializationCallback,
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
        private PdnRegion updateRegion;

        [NonSerialized]
        private bool dirty;

        private Version savedWith;

        /// <summary>
        /// This is provided for future use.
        /// If you want to add new stuff that must be serialized, create a new class,
        /// then point 'tag' to a new instance of this class that is initialized
        /// during construction. Make sure the new class has a 'tag' variable as well.
        /// We effectively set up a 'linked list' where new versions of the code
        /// can open old versions of the document, as .NET serialization is fickle in
        /// certain areas. You might also add a new property to use simplify using 
        /// this stuff...
        ///    public DocumentVersion2Data DocV2Data { get { return (DocumentVersion2Data)tag; } }
        /// </summary>
        private object tag = null;

        /// <summary>
        /// Reports the version of Paint.NET that this file was saved with.
        /// This is reset when SaveToStream is used. This can be used to
        /// determine file format compatibility if necessary.
        /// </summary>
        public Version SavedWithVersion
        {
            get
            {
                return savedWith;
            }
        }

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
                return dirty;
            }

            set
            {
                dirty = value;
            }
        }

        /// <summary>
        /// Returns a reference to the current region that has been changed since the last
        /// time Update() was called. This does not return a copy, and thus changes made
        /// to the object can possible disrupt coherency.
        /// </summary>
        public PdnRegion UpdateRegion
        {
            get
            {
                return updateRegion;
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
                return new Size(Width, Height);
            }
        }

        public Rectangle Bounds
        {
            get
            {
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
                return userMetaData;
            }
        }

		public void CopyProperties(Document other) 
		{
			foreach (string key in other.userMetaData)
			{
				userMetaData.Set(key, other.userMetaData[key]);
			}
		}

        /// <summary>
        /// The "tag" is used in case we want to extend the file format but keep backwards
        /// compatibility.
        /// </summary>
        public object Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
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
                return name;
            }

            set
            {
                name = value;
            }
        }


        /// <summary>
        /// Clears a portion of the surface to a background checkerboard
        /// pattern. The x and y position are necessary to determine where in
        /// the checkerboard pattern the target pixel lies.
        /// </summary>
        /// <param name="ptr">The address of the first pixel to clear</param>
        /// <param name="length">The number of pixels (including the start pixel) to clear</param>
        /// <param name="x">The x-value of the start pixel</param>
        /// <param name="y">The y-value of the start pixel</param>
        private unsafe void ClearToBackground(ColorBgra *ptr, int length, int x, int y) 
        {
            for (int i = 0; i < length; i++) {
                //We take the xor or the 3rd bit the color used.
                //Ends up being 128 or 192
                int v = (((x + i) ^ y) & 8) * 8 + 128;
                //Set the pixel value to be opaque + RGB(1, 1, 1) * v
                ptr[i].Bgra = (uint)0xff000000 | ((uint)0x00010101 * (uint)v);
            }
        }

        /// <summary>
        /// Clears a scanline of a surface to a background checkerboard
        /// pattern.
        /// </summary>
        /// <param name="surface">The surface to partially clear</param>
        /// <param name="scan">The scanline within the surface to clear</param>
        private unsafe void ClearToBackground(Surface surface, Scanline scan) 
        {
            ClearToBackground(surface.GetPointAddress(scan.Point), scan.Length, scan.Point.X, scan.Point.Y);
        }

        /// <summary>
        /// Clears a portion of a surface to a background checkerboard
        /// pattern.
        /// </summary>
        /// <param name="surface">The surface to partially clear</param>
        /// <param name="roi">The rectangle to clear</param>
        private unsafe void ClearToBackground(Surface surface, Rectangle roi) 
        {
            for (int y = roi.Top; y < roi.Bottom; y++)
            {
                ClearToBackground(surface.GetPointAddress(roi.Left, y), roi.Width, roi.Left, y);
            }
        }

        /// <summary>
        /// Clears a portion of a surface to a background checkerboard
        /// pattern.
        /// </summary>
        /// <param name="surface">The surface to partially clear</param>
        /// <param name="rois">The array of Rectangles designating the areas to clear</param>
        /// <param name="startIndex">The start index within the rois array to clear</param>
        /// <param name="length">The number of Rectangles in the rois array (staring with startIndex) to clear</param>
        private void ClearToBackground(Surface surface, Rectangle [] rois, int startIndex, int length) 
        {
            for (int i = startIndex; i < startIndex + length; i++) 
            {
                ClearToBackground(surface, rois[i]);
            }
        }

        /// <summary>
        /// Clears a portion of a surface to a background checkerboard
        /// pattern.
        /// </summary>
        /// <param name="surface">The surface to partially clear</param>
        /// <param name="roifs">The array of RectangleFs designating the areas to clear</param>
        /// <param name="startIndex">The start index within the rois array to clear</param>
        /// <param name="length">The number of RectangleFs in the rois array (staring with startIndex) to clear</param>
        private void ClearToBackground(Surface surface, RectangleF [] roifs, int startIndex, int length) 
        {
            for (int i = startIndex; i < startIndex + length; i++) 
            {
                ClearToBackground(surface, Rectangle.Truncate(roifs[i]));
            }
        }

        /// <summary>
        /// Clears a portion of a surface to a background checkerboard
        /// pattern.
        /// </summary>
        /// <param name="surface">The surface to partially clear</param>
        /// <param name="scans">The array of Scanlines designating the areas to clear</param>
        /// <param name="startIndex">The start index within the scans array to clear</param>
        /// <param name="length">The number of Scanlines in the scans array (staring with startIndex) to clear</param>
        private void ClearToBackground(Surface surface, Scanline [] scans, int startIndex, int length) 
        {
            for (int i = startIndex; i < startIndex + length; i++) 
            {
                ClearToBackground(surface, scans[i]);
            }
        }
        

        private static readonly UnaryPixelOps.Constant constantWhite = new UnaryPixelOps.Constant(ColorBgra.FromUInt32(0xffffffff)); 

        /// <summary>
        /// Renders the document onto the given RenderArgs as one opaque layer.
        /// </summary>
        /// <param name="args">The target RenderArgs</param>
        public void RenderFlat(RenderArgs args)
        {
            constantWhite.Apply(args.Surface, args.Surface.Bounds);

            foreach (Layer layer in Layers)
            {
                if (layer.Visible)
                {
                    layer.Render(args, args.Surface.Bounds);
                }
            }
        }

        /// <summary>
        /// Explicitely renders a requested region of the document.
        /// </summary>
        /// <param name="args">Contains information used to control where rendering occurs.</param>
        /// <param name="roi">The rectangular region to render.</param>
        public void Render(RenderArgs args, Rectangle roi)
        {
            ClearToBackground(args.Surface, roi);

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
            ClearToBackground(args.Surface, scans, startIndex, length);

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
            ClearToBackground(args.Surface, roi, startIndex, length);

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
            ClearToBackground(args.Surface, roi, startIndex, length);

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
        public void Render(RenderArgs args, PdnRegion roi)
        {
            Rectangle[] rects = roi.GetRegionScansReadOnlyInt();
            this.Render(args, rects);
        }   

        private class UpdateScansContext
        {
            private Document document;
            private RenderArgs dst;
            private Rectangle[] scans;
            private int startIndex;
            private int length;

            public void UpdateScans(object context)
            {
                document.Render(dst, scans, startIndex, length);
            }

            public UpdateScansContext(Document document, RenderArgs dst, Rectangle[] scans, int startIndex, int length)
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
            Rectangle[] rectsOriginal = updateRegion.GetRegionScansReadOnlyInt();
            Rectangle[] rectsToUse;

            // Special case where we're drawing 1 big rectangle: split it in half!
            // This case happens quite frequently, but we don't want to spend a lot of
            // time analyzing any other case that is more complicated.
            if (rectsOriginal.Length == 1 && rectsOriginal[0].Height > 1)
            {
                Rectangle[] rectsNew = new Rectangle[2];
                rectsNew[0] = Rectangle.FromLTRB(rectsOriginal[0].Left, rectsOriginal[0].Top, rectsOriginal[0].Right, rectsOriginal[0].Top + (rectsOriginal[0].Bottom - rectsOriginal[0].Top) / 2);
                rectsNew[1] = Rectangle.FromLTRB(rectsOriginal[0].Left, rectsOriginal[0].Top + (rectsOriginal[0].Bottom - rectsOriginal[0].Top) / 2, rectsOriginal[0].Right, rectsOriginal[0].Bottom);
                rectsToUse = rectsNew;
            }
            else
            {
                rectsToUse = rectsOriginal;
            }
        
            // Split the task of rendering into 2 batches, and use background threads to handle them. 
            // Runs faster on multi-CPU/multi-core systems, maybe on those with HyperThreading.
            UpdateScansContext usc1 = new UpdateScansContext(this, dst, rectsToUse, 0, rectsToUse.Length / 2);
            UpdateScansContext usc2 = new UpdateScansContext(this, dst, rectsToUse, rectsToUse.Length / 2, rectsToUse.Length - (rectsToUse.Length / 2));
            Utility.ThreadPool.QueueUserWorkItem(new WaitCallback(usc1.UpdateScans));
            Utility.ThreadPool.QueueUserWorkItem(new WaitCallback(usc2.UpdateScans));
            Utility.ThreadPool.Drain();

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
            this.updateRegion = new PdnRegion();
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
            updateRegion = new PdnRegion(this.Bounds);
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
                invalidated += value;
            }

            remove
            {
                invalidated -= value;
            }
        }

        /// <summary>
        /// Raises the Invalidated event.
        /// </summary>
        /// <param name="e"></param>
        private void OnInvalidated(InvalidateEventArgs e)
        {
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
            Invalidate(e.InvalidRect);
        }

        /// <summary>
        /// Causes the whole document to be invalidated, forcing a full rerender on
        /// the next call to Update.
        /// </summary>
        public void Invalidate()
        {
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
        public void Invalidate(PdnRegion roi)
        {
            dirty = true;
            updateRegion.Union(roi);
            updateRegion.Intersect(this.Bounds);
            
            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                rect.Intersect(this.Bounds);

                if (!rect.IsEmpty)
                {
                    InvalidateEventArgs iea = new InvalidateEventArgs(rect);
                    OnInvalidated(iea);
                }
            }
        }

        public void Invalidate(RectangleF[] roi)
        {
            foreach (RectangleF rectF in roi)
            {
                Invalidate(Rectangle.Truncate(rectF));
            }
        }

        public void Invalidate(RectangleF roi)
        {
            Invalidate(Rectangle.Truncate(roi));
        }

        public void Invalidate(Rectangle[] roi)
        {
            foreach (Rectangle rect in roi)
            {
                Invalidate(rect);
            }
        }

        /// <summary>
        /// Invalidates a portion of the document. The given region is then tagged
        /// for rerendering during the next call to Update.
        /// </summary>
        /// <param name="roi">The region of interest to be invalidated.</param>
        public void Invalidate(Rectangle roi)
        {
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
            updateRegion.Dispose();
            updateRegion = new PdnRegion();
            updateRegion.MakeEmpty();
        }

        /// <summary>
        /// Excludes a portion of the document from the update region.
        /// </summary>
        /// <param name="roi">The region of interest to be validated.</param>
        private void Validate(PdnRegion roi)
        {
            updateRegion.Exclude(roi);
        }

        /// <summary>
        /// Excludes a portion of the document from the update region.
        /// </summary>
        /// <param name="roi">The region of interest to be validated.</param>
        private void Validate(Rectangle roi)
        {
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
        /// <param name="stream">
        /// The Stream to serialize data into. This data will be compressed.
        /// </param>
        /// <param name="callback">
        /// This can be used to keep track of the number of uncompressed bytes that are written. The 
        /// values reported through the IOEventArgs.Count+Offset will vary from 1 to approximately 
        /// Layers.Count*Width*Height*sizeof(ColorBgra). The final number will actually be higher 
        /// because of hierarchical overhead, so make sure to cap any progress reports to 100%. This
        /// callback will be wired to the IOFinished event of a SiphonStream.
        /// </param>
        public void SaveToStream(Stream stream, IOEventHandler callback)
        {
            this.savedWith = new Version(Application.ProductVersion);

            GZipOutputStream gZipStream = new GZipOutputStream(stream);
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
            Surface surface = new Surface(Width, Height);

            using (RenderArgs renderArgs = new RenderArgs(surface))
            {
                RenderFlat(renderArgs);
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
