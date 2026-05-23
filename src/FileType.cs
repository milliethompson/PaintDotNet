using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Represents one type of file that PaintDotNet can load or save.
    /// Objects of this type are immutable once created.
    /// </summary>
    [Serializable]
    public class FileType
    {
        // should be of the format ".ext" ... like ".bmp" or ".jpg"
        // The first extension in this list is the default extension (".jpg" for JPEG, for instance, as ".jfif" etc. are not seen very often)
        private string[] extensions; 
        public string[] Extensions
        {
            get
            {
                return extensions;
            }
        }

        public bool SupportsExtension(string ext)
        {
            foreach (string fExt in extensions)
            {
                if (fExt.ToLower() == ext.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        public string DefaultExtension
        {
            get
            {
                return extensions[0];
            }
        }

        // example: "Bitmap" or "JPEG"
        private string name; 
        public string Name
        {
            get
            {
                return name;
            }
        }

        // Refers to one of the ImageFormat static members, or null if there is no 
        // correspondence to a standard image type (like with the Layered Bitmap type)
        private ImageFormat format; 
        public ImageFormat ImageFormat
        {
            get
            {
                return format;
            }
        }

        // Does this format support layers? If not, when we go to save a multi-layer doc 
        // we gotta say "Hey, we are gonna flatten it!"
        // NOTE: If ImageFormat is not null, then this is implicitely false.
        private bool supportsLayers;
        public bool SupportsLayers
        {
            get
            {
                if (format != null)
                {
                    return false;
                }
                else
                {
                    return supportsLayers;
                }
            }
        }

        // Saves this document to a stream in the given format!
        public void Save(Document input, Stream output)
        {
            if (format == null)
            {
                CustomSave(input, output);
            }
            else
            {
                // flatten the document
                using (Surface surface = new Surface(input.Width, input.Height))
                {
                    new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 255)).Apply(surface, surface.Bounds);

                    using (RenderArgs ra = new RenderArgs(surface))
                    {
                        input.Render(ra, surface.Bounds);
                    }

                    using (Bitmap bitmap = surface.CreateAliasedBitmap())
                    {
                        // HACK: disallow lame-quality JPEG saving
                        if (this.ImageFormat.Guid == ImageFormat.Jpeg.Guid)
                        {
                            ImageCodecInfo icf = FileType.GetImageCodecInfo(this.ImageFormat);
                            EncoderParameters parms = new EncoderParameters(1);
                            EncoderParameter parm = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L); // force '90% quality'
                            parms.Param[0] = parm;

                            bitmap.Save(output, icf, parms);
                        }
                        else
                        {
                            bitmap.Save(output, format);
                        }
                    }
                }
            }
        }

        public Document Load(Stream input)
        {
            if (format == null)
            {
                return CustomLoad(input);
            }
            else
            {
                Image image = Image.FromStream(input);
                return Document.FromImage(image);
            }
        }

        // If there is a necessity to implement saving without using Image.Save, override
        // this method. This is required when this.ImageFormat is null, and will throw
        // an exception if not implemented!
        protected  virtual void CustomSave(Document input, Stream output)
        {
            throw new InvalidOperationException();
        }

        protected virtual Document CustomLoad(Stream input)
        {
            throw new InvalidOperationException();
        }

        public FileType(string name, ImageFormat format, bool supportsLayers, string[] extensions)
        {
            this.name = name;
            this.supportsLayers = supportsLayers;
            this.format = format;
            this.extensions = extensions;
        }

        /// <summary>
        /// Returns a string that can be used while populating a *FileDialog common dialog.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append(" (");

            for (int i = 0; i < extensions.Length; ++i)
            {
                sb.Append("*");
                sb.Append(extensions[i]);

                if (i != extensions.Length - 1)
                {
                    sb.Append("; ");
                }
                else
                {
                    sb.Append(")");
                }
            }

            sb.Append("|");

            for (int i = 0; i < extensions.Length; ++i)
            {
                sb.Append("*");
                sb.Append(extensions[i]);

                if (i != extensions.Length - 1)
                {
                    sb.Append(";");
                }
            }

            return sb.ToString();
        }

        private static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
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

    }
}


