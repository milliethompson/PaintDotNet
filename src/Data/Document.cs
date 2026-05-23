/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using ICSharpCode.SharpZipLib.GZip;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

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
        /// <summary>
        /// This is our compatibility shim so that PDN files saved with previous versions load
        /// correctly. They wouldn't load correctly because the assembly name encoded into them is
        /// PaintDotNet, and not PaintDotNet.Data.
        /// </summary>
        private sealed class OurSerializationBinder
            : SerializationBinder
        {
            private string ourAssemblyName;

            public OurSerializationBinder()
            {
                ourAssemblyName = Assembly.GetExecutingAssembly().FullName;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                // First try to load what they are asking for
                string firstFullTypeName = string.Format(typeName + ", " + assemblyName);
                Type firstTryType = Type.GetType(firstFullTypeName, false);

                if (firstTryType != null)
                {
                    return firstTryType; // success!
                }

                // Hmm, try substituting their assembly name with *our* assembly name, then retry
                string secondFullTypeName = string.Format(typeName + ", " + ourAssemblyName);
                Type secondTryType = Type.GetType(secondFullTypeName);

                if (secondTryType != null)
                {
                    return secondTryType; // success!
                }

                // Try PdnLib ...
                string thirdFullTypeName = string.Format(typeName + ", " + typeof(Utility).Assembly.FullName);
                Type thirdTryType = Type.GetType(thirdFullTypeName);

                if (thirdTryType != null)
                {
                    return thirdTryType;
                }

                // Yeah, I have no idea either.
                return null;
            }
        }

        private LayerList layers;
        private int width;
        private int height;
        private NameValueCollection userMetaData;

        [NonSerialized]
        private PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool();

        [NonSerialized]
        private InvalidateEventHandler layerInvalidatedDelegate;

        [NonSerialized]
        private PdnRegion updateRegion;

        [NonSerialized]
        private bool dirty;

        private Version savedWith;

        [NonSerialized]
        private MetaData metaData = null;

        [NonSerialized]
        private XmlDocument headerXml;

        private const string headerXmlSkeleton = "<pdnImage><custom></custom></pdnImage>";

        private XmlDocument HeaderXml
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                if (this.headerXml == null)
                {
                    this.headerXml = new XmlDocument();
                    this.headerXml.LoadXml(headerXmlSkeleton);
                }

                return this.headerXml;
            }
        }

        public string Header
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return this.HeaderXml.OuterXml;
            }
        }

        public string CustomHeaders
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                return this.HeaderXml.SelectSingleNode("/pdnImage/custom").InnerXml;
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                this.HeaderXml.SelectSingleNode("/pdnImage/custom").InnerXml = value;
            }
        }

        /// <summary>
        /// This is provided for future use.
        /// If you want to add new stuff that must be serialized, create a new class,
        /// then point 'tag' to a new instance of this class that is initialized
        /// during construction. Make sure the new class has a 'tag' variable as well.
        /// We effectively set up a 'linked list' where new versions of the code
        /// can open old versions of the document, as .NET serialization is fickle in
        /// certain areas. You might also add a new property to simplify using 
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
                if (disposed)
                {
                    throw new ObjectDisposedException("Document");
                }

                if (savedWith == null)
                {
                    savedWith = new Version(Utility.VersionFromFullAssemblyName(Assembly.GetExecutingAssembly().FullName));
                }

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
        [Obsolete("user MetaData instead")]
        public NameValueCollection UserMetaData
        {
            get
            {
                return userMetaData;
            }
        }

        public MetaData MetaData
        {
            get
            {
                if (metaData == null)
                {
                    metaData = new MetaData(userMetaData);
                }

                return metaData;
            }
        }

		public void CopyPropertiesFrom(Document other) 
		{
			foreach (string key in other.userMetaData)
			{
				userMetaData.Set(key, other.userMetaData[key]);
			}
		}

        [Obsolete("don't use this property; implementors should expose type-safe properties instead")]
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

        // TODO: This should be part of DocumentView, not Document.
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
            for (int i = 0; i < length; i++) 
            {
                //We take the xor or the 3rd bit the color used.
                //Ends up being 128 or 192
                int v = (((x + i) ^ y) & 8) * 8 + 128;
                //Set the pixel value to be opaque + RGB(1, 1, 1) * v
                ptr[i].Bgra = (uint)0xff000000 | ((uint)0x00010101 * (uint)v);
            }
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
        /// Renders the document onto the given RenderArgs. Assumes you have already cleared the surface
        /// to whatever color value you wish to blend with (recommend BGRA=[255,255,255,0]).
        /// </summary>
        /// <param name="args">The target RenderArgs</param>
        public void RenderFlat(RenderArgs args)
        {
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

        private sealed class UpdateScansContext
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

            Rectangle[] rectsOriginal = updateRegion.GetRegionScansReadOnlyInt();
            Rectangle[] rectsToUse;

            // Special case where we're drawing 1 big rectangle: split it in half!
            // This case happens quite frequently, but we don't want to spend a lot of
            // time analyzing any other case that is more complicated.
            if (rectsOriginal.Length == 1 && rectsOriginal[0].Height > 1)
            {
                Rectangle[] rectsNew = new Rectangle[Processor.LogicalCpuCount];
                Utility.SplitRectangle(rectsOriginal[0], rectsNew);
                rectsToUse = rectsNew;
            }
            else
            {
                rectsToUse = rectsOriginal;
            }

            int cpuCount = Processor.LogicalCpuCount;
            for (int i = 0; i < cpuCount; ++i)
            {
                int start = (i * rectsToUse.Length) / cpuCount;
                int end = ((i + 1) * rectsToUse.Length) / cpuCount;

                UpdateScansContext usc = new UpdateScansContext(this, dst, rectsToUse, start, end - start);
                threadPool.QueueUserWorkItem(new WaitCallback(usc.UpdateScans), usc);
            }
        
            threadPool.Drain();
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
            this.Dirty = true;
            this.updateRegion = PdnRegion.CreateEmpty();
            layers = new LayerList(this);
            SetupEvents();
            userMetaData = new NameValueCollection();
            Invalidate();
        }

        public Document(Size size)
            : this(size.Width, size.Height)
        {
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
            this.updateRegion = new PdnRegion(this.Bounds);
            this.threadPool = new PaintDotNet.Threading.ThreadPool();
            SetupEvents();
            Dirty = true;
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
            Dirty = true;
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
            Dirty = true;
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
            Dirty = true;
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
            updateRegion.MakeEmpty();
        }

        /// <summary>
        /// Creates a document that consists of one BitmapLayer.
        /// </summary>
        /// <param name="image">The Image to make a copy of that will be the first layer ("Background") in the document.</param>
        public static Document FromImage(Image image)
        {
            Document document = new Document(image.Width, image.Height);
            BitmapLayer layer = Layer.CreateBackgroundLayer(image.Width, image.Height);
            layer.Surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));
            layer.Name = "Background";

            using (RenderArgs args = new RenderArgs(layer.Surface))
            {
                args.Graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            }

            document.Layers.Add(layer);
            document.Invalidate();
            return document;
        }

        public static byte[] MagicBytes
        {
            get
            {
                return Encoding.UTF8.GetBytes("PDN3");
            }
        }

        /// <summary>
        /// Deserializes a Document from a stream.
        /// </summary>
        /// <param name="stream">The stream to deserialize from. This stream must be seekable.</param>
        /// <returns>The Document that was stored in stream.</returns>
        public static Document FromStream(Stream stream)
        {
            long oldPosition = stream.Position;
            bool pdn21Format = true;

            // Version 2.1 file format:
            //   Starts with bytes as defined by MagicBytes 
            //   Next two bytes are 24-bit unsigned int 'N' (first byte is low-word, second byte is middle-word, third byte is high word)
            //   The next N bytes are a string, this is the document header (it is XML, UTF-8 encoded)
            //     Important: 'N' indicates a byte count, not a character count. 'N' bytes may result in less than 'N' characters,
            //                depending on how the characters decode as per UTF8!
            //   If the next 2 bytes are 0x00, 0x01: This signifies that non-compressed .NET serialized data follows.
            //   If the next 2 bytes are 0x1f, 0x8b: This signifies the start of the gzip compressed .NET serialized data
            //
            // Version 2.0 and previous file format:
            //   Starts with 0x1f, 0x8b: this signifies the start of the gzip compressed .NET serialized data.

            // Read in the 'magic' bytes
            for (int i = 0; i < MagicBytes.Length; ++i)
            {
                int theByte = stream.ReadByte();

                if (theByte == -1)
                {
                    throw new EndOfStreamException();
                }

                if (theByte != MagicBytes[i])
                {
                    pdn21Format = false;
                    break;
                }
            }

            // Read in the header if we found the 'magic' bytes identifying a PDN 2.1 file
            XmlDocument headerXml = null;
            if (pdn21Format)
            {
                // This is a Paint.NET v2.1+ file.  
                int low = stream.ReadByte();

                if (low == -1)
                {
                    throw new EndOfStreamException();
                }

                int mid = stream.ReadByte();

                if (mid == -1)
                {
                    throw new EndOfStreamException();
                }

                int high = stream.ReadByte();

                if (high == -1)
                {
                    throw new EndOfStreamException();
                }

                int byteCount = low + (mid << 8) + (high << 16);
                byte[] bytes = new byte[byteCount];
                int bytesRead = stream.Read(bytes, 0, byteCount);

                if (bytesRead != byteCount)
                {
                    throw new EndOfStreamException("expected " + byteCount + " bytes, but only got " + bytesRead);
                }

                string xml = Encoding.UTF8.GetString(bytes);
                headerXml = new XmlDocument();
                headerXml.LoadXml(xml);
            }
            else
            {
                stream.Position = oldPosition; // rewind and try as v2.0-or-earlier file
            }

            // Start reading the data section of the file. Determine if it's gzip or regular
            long oldPosition2 = stream.Position;
            int first = stream.ReadByte();

            if (first == -1)
            {
                throw new EndOfStreamException();
            }

            int second = stream.ReadByte();

            if (second == -1)
            {
                throw new EndOfStreamException();
            }

            Document document;
            object docObject;
            BinaryFormatter formatter = new BinaryFormatter();
            Document.OurSerializationBinder ourBinder = new Document.OurSerializationBinder();
            formatter.Binder = ourBinder;

            if (first == 0 && second == 1)
            {
                DeferredFormatter deferred = new DeferredFormatter();
                formatter.Context = new StreamingContext(formatter.Context.State, deferred);
                docObject = formatter.UnsafeDeserialize(stream, null);
                deferred.FinishDeserialization(stream);
            }
            else if (first == 0x1f && second == 0x8b)
            {
                stream.Position = oldPosition2; // rewind to the start of 0x1f, 0x8b
                GZipInputStream gZipStream = new GZipInputStream(stream, 4096);
                docObject = formatter.UnsafeDeserialize(gZipStream, null);
            }
            else
            {
                throw new FormatException("file is not a valid Paint.NET document");
            }

            document = (Document)docObject;
            document.Dirty = true;
            document.headerXml = headerXml;
            document.Invalidate();
            return document;
        }

        /// <summary>
        /// Saves the Document to the given Stream with only the default headers and no
        /// IO completion callback.
        /// </summary>
        /// <param name="stream">The Stream to serialize the Document to.</param>
        public void SaveToStream(Stream stream)
        {
            SaveToStream(stream, null);
        }

        /// <summary>
        /// Saves the Document to the given Stream with the default and given headers, and
        /// using the given IO completion callback.
        /// </summary>
        /// <param name="stream">The Stream to serialize the Document to.</param>
        /// <param name="callback">
        /// This can be used to keep track of the number of uncompressed bytes that are written. The 
        /// values reported through the IOEventArgs.Count+Offset will vary from 1 to approximately 
        /// Layers.Count*Width*Height*sizeof(ColorBgra). The final number will actually be higher 
        /// because of hierarchical overhead, so make sure to cap any progress reports to 100%. This
        /// callback will be wired to the IOFinished event of a SiphonStream. Events may be raised
        /// from any thread. May be null.
        /// </param>
        public void SaveToStream(Stream stream, IOEventHandler callback)
        {
            PrepareHeader();
            string headerText = this.HeaderXml.OuterXml;

            // Write the header
            byte[] magicBytes = Document.MagicBytes;
            stream.Write(magicBytes, 0, magicBytes.Length);
            byte[] headerBytes = Encoding.UTF8.GetBytes(headerText);
            stream.WriteByte((byte)(headerBytes.Length & 0xff));
            stream.WriteByte((byte)((headerBytes.Length & 0xff00) >> 8));
            stream.WriteByte((byte)((headerBytes.Length & 0xff0000) >> 16));
            stream.Write(headerBytes, 0, headerBytes.Length);
            stream.Flush();

            // Get version info
            Assembly a = Assembly.GetExecutingAssembly();
            string version = Utility.VersionFromFullAssemblyName(a.FullName);
            this.savedWith =  new Version(version);

            // Write 0x00, 0x01 to indicate normal .NET serialized data
            stream.WriteByte(0x00);
            stream.WriteByte(0x01);

            // Write the remainder of the file (gzip compressed)
            SiphonStream siphonStream = new SiphonStream(stream);

            BinaryFormatter formatter = new BinaryFormatter();
            DeferredFormatter deferred = new DeferredFormatter();
            SaveProgressRelay relay = new SaveProgressRelay(deferred, callback);
            formatter.Context = new StreamingContext(formatter.Context.State, deferred);
            formatter.Serialize(siphonStream, this);
            deferred.FinishSerialization(siphonStream);

            stream.Flush();
        }

        private class SaveProgressRelay
        {
            private DeferredFormatter formatter;
            private IOEventHandler ioCallback;
            private long lastReportedBytes;

            public SaveProgressRelay(DeferredFormatter formatter, IOEventHandler ioCallback)
            {
                this.formatter = formatter;
                this.ioCallback = ioCallback;
                this.formatter.ReportedBytesChanged += new EventHandler(formatter_ReportedBytesChanged);
            }

            private void formatter_ReportedBytesChanged(object sender, EventArgs e)
            {
                long reportedBytes = formatter.ReportedBytes;
                bool raiseEvent;
                long length = 0;
                
                lock (this)
                {
                    raiseEvent = (reportedBytes > lastReportedBytes);

                    if (raiseEvent)
                    {
                        length = reportedBytes - this.lastReportedBytes;
                        this.lastReportedBytes = reportedBytes;
                    }
                }

                if (raiseEvent)
                {
                    ioCallback(this, new IOEventArgs(IOOperationType.Write, reportedBytes - length, (int)length));
                }
            }
        }

        private void PrepareHeader()
        {
            XmlDocument xd = this.HeaderXml;
            XmlElement pdnImage = (XmlElement)xd.SelectSingleNode("/pdnImage");
            pdnImage.SetAttribute("width", this.Width.ToString());
            pdnImage.SetAttribute("height", this.Height.ToString());
            pdnImage.SetAttribute("layers", this.Layers.Count.ToString());
            pdnImage.SetAttribute("savedWithVersion", this.SavedWithVersion.ToString(4));
        }

        public void Flatten(Surface dst)
        {
            if (dst.Size != this.Size)
            {
                throw new ArgumentOutOfRangeException("dst.Size must match this.Size");
            }

            dst.Clear(ColorBgra.White.NewAlpha(0));

            using (RenderArgs renderArgs = new RenderArgs(dst))
            {
                RenderFlat(renderArgs);
            }
        }

        /// <summary>
        /// Returns a new Document that is a flattened version of this one
        /// "Flattened" means it is one layer that is simply a bitmap of
        /// the compositied image.
        /// </summary>
        /// <returns></returns>
        public Document Flatten()
        {
            Document newDocument = new Document(width, height);
            newDocument.CopyPropertiesFrom(this);
            BitmapLayer layer = Layer.CreateBackgroundLayer(width, height);
            newDocument.Layers.Add(layer);
            Flatten(layer.Surface);
            return newDocument;
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
