/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Represents one type of file that PaintDotNet can load or save.
    /// </summary>
    public abstract class FileType
    {
        // should be of the format ".ext" ... like ".bmp" or ".jpg"
        // The first extension in this list is the default extension (".jpg" for JPEG, for instance, as ".jfif" etc. are not seen very often)
        private string[] extensions; 
        public string[] Extensions
        {
            get
            {
                return (string[])extensions.Clone();
            }
        }

        /// <summary>
        /// Gets the default extension for the FileType.
        /// </summary>
        /// <remarks>
        /// This is always the first extension that is supported
        /// </remarks>
        public string DefaultExtension
        {
            get
            {
                return extensions[0];
            }
        }

        /// <summary>
        /// Returns the friendly name of the file type, such as "Bitmap" or "JPEG".
        /// </summary>
        private string name; 
        public string Name
        {
            get
            {
                return name;
            }
        }

        private bool supportsLayers;

        /// <summary>
        /// Gets a flag indicating whether this FileType supports layers.
        /// </summary>
        /// <remarks>
        /// If a FileType is asked to save a Document that has more than one layer,
        /// it will flatten it before it saves it.
        /// </remarks>
        public bool SupportsLayers
        {
            get
            {
                return supportsLayers;
            }
        }

        private bool supportsCustomHeaders;

        /// <summary>
        /// Gets a flag indicating whether this FileType supports custom headers.
        /// </summary>
        /// <remarks>
        /// If this returns false, then the Document's CustomHeaders will be discarded
        /// on saving.
        /// </remarks>
        public bool SupportsCustomHeaders
        {
            get
            {
                return supportsCustomHeaders;
            }
        }

        public FileType(string name, bool supportsLayers, bool supportsCustomHeaders, string[] extensions)
        {
            this.name = name;
            this.supportsLayers = supportsLayers;
            this.supportsCustomHeaders = supportsCustomHeaders;
            this.extensions = extensions;
        }

        public bool SupportsExtension(string ext)
        {
            foreach (string ext2 in extensions)
            {
                if (ext2.ToLower() == ext.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void Save(Document input, Stream output, SaveConfigToken token);

        /// <summary>
        /// Determines if saving with a given SaveConfigToken would alter the image
        /// in any way. Put another way, if the document is saved with these settings
        /// and then immediately loaded, would it have exactly the same pixel values?
        /// Any lossy codec should return 'false'.
        /// This value is used to optimizing preview rendering memory usage, and as such
        /// flattening should not be taken in to consideration. For example, the codec
        /// for PNG returns true.
        /// </summary>
        /// <param name="token">The SaveConfigToken to determine reflexiveness for.</param>
        /// <returns>true if the save would be reflexive, false if not</returns>
        public virtual bool IsReflexive(SaveConfigToken token)
        {
            return false;
        }

        public virtual SaveConfigWidget CreateSaveConfigWidget()
        {
            return new NoSaveConfigWidget();
        }

        private sealed class NoSaveConfigToken
            : SaveConfigToken
        {
        }

        /// <summary>
        /// Gets a flag indicating whether or not the file type supports configuration
        /// via a SaveConfigToken and SaveConfigWidget.
        /// </summary>
        /// <remarks>
        /// Implementers of FileType derived classes don't need to do anything special
        /// for this property to be accurate. If your FileType implements
        /// CreateDefaultSaveConfigToken, this will correctly return true.
        /// </remarks>
        public bool SupportsConfiguration
        {
            get
            {
                SaveConfigToken token = CreateDefaultSaveConfigToken();
                return !(token is NoSaveConfigToken);
            }
        }

        public virtual SaveConfigToken CreateDefaultSaveConfigToken()
        {
            return new NoSaveConfigToken();
        }

        public abstract Document Load(Stream input);

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FileType))
            {
                return false;
            }

            return this.Name.Equals(((FileType)obj).Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Returns a string that can be used for populating a *FileDialog common dialog.
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
    }
}
