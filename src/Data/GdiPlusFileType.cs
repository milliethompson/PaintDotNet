/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PaintDotNet
{
    /// <summary>
    /// Implements FileType for generic GDI+ codecs.
    /// </summary>
    /// <remarks>
    /// GDI+ file types do not support custom headers.
    /// </remarks>
    public class GdiPlusFileType
        : FileType
    {
        private ImageFormat imageFormat; 
        public ImageFormat ImageFormat
        {
            get
            {
                return this.imageFormat;
            }
        }

        protected override void OnSave(Document input, System.IO.Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            GdiPlusFileType.Save(input, output, this.ImageFormat, callback);
        }

        public static void Save(Document input, Stream output, ImageFormat format, ProgressEventHandler callback)
        {
            // flatten the document
            using (Surface surface = new Surface(input.Width, input.Height))
            {
                surface.Clear(ColorBgra.FromBgra(255, 255, 255, 0));

                using (RenderArgs ra = new RenderArgs(surface))
                {
                    input.RenderFlat(ra);
                }

                using (Bitmap bitmap = surface.CreateAliasedBitmap())
                {
                    LoadProperties(bitmap, input);
                    bitmap.Save(output, format);
                }
            }    
        }

        public static void LoadProperties(Image dstImage, Document srcDoc)
        {
            MetaData metaData = srcDoc.MetaData;

            foreach (string key in metaData.GetKeys(MetaData.ExifSectionName))
            {
                string blob = metaData.GetValue(MetaData.ExifSectionName, key);
                PropertyItem pi = PdnGraphics.DeserializePropertyItem(blob);

                try
                {
                    dstImage.SetPropertyItem(pi);
                }

                catch (ArgumentException)
                {
                    // Ignore error: the image does not support property items
                }
            }
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = LoadImage(input))
            {
                Document document = Document.FromImage(image);
                MetaData metaData = document.MetaData;

                object[] pis = (object[])PdnGraphics.GetPropertyItems(image);
                for (int i = 0; i < pis.Length; ++i)
                {
                    metaData.AddExifValues(new PropertyItem[] { (PropertyItem)pis[i] });
                }

                return document;
            }
        }

        public static Image LoadImage(Stream input)
        {
            return Image.FromStream(input);
        }
        
        public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo icf in encoders)
            {
                if (icf.FormatID == format.Guid)
                {
                    return icf;
                }
            }

            return null;
        }

        public GdiPlusFileType(string name, ImageFormat imageFormat, bool supportsLayers, string[] extensions)
            : this(name, imageFormat, supportsLayers, extensions, false)
        {
        }

        public GdiPlusFileType(string name, ImageFormat imageFormat, bool supportsLayers, string[] extensions, bool savesWithProgress)
            : base(name, supportsLayers, false, true, true, savesWithProgress, extensions)
        {
            this.imageFormat = imageFormat;
        }
    }
}
