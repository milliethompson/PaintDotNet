/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;

namespace PaintDotNet
{
    /// <summary>
    /// Represents a collection of FileType instances.
    /// </summary>
    [Serializable]
    public class FileTypeCollection
    {
        private FileType[] fileTypes;
        public FileType[] FileTypes
        {
            get
            {
                return (FileType[])fileTypes.Clone();
            }
        }

        public int Length
        {
            get
            {
                return fileTypes.Length;
            }
        }

        public FileType this[int index]
        {
            get
            {
                return fileTypes[index];
            }
        }

        internal FileTypeCollection(FileType[] fileTypes)
        {
            this.fileTypes = fileTypes;
        }

        public FileTypeCollection(ICollection fileTypes)
        {
            this.fileTypes = new FileType[fileTypes.Count];
            int dstIndex = 0;

            foreach (FileType ft in fileTypes)
            {
                this.fileTypes[dstIndex] = ft;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < fileTypes.Length; ++i)
            {
                sb.Append(fileTypes[i].ToString());

                if (i != fileTypes.Length - 1)
                {
                    sb.Append("|");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Allows you to include an "All" type at the top that includes all the filetypes
        /// "All images (*.bmp, *.gif, ...)" for instance.
        /// </summary>
        /// <param name="includeAll">Whether or not to include the 'all' file type at the top</param>
        /// <param name="allName">The name of the 'all' type (example: "All images"). If this is null
        /// but includeAll is true, then this defaults to the string "All image types"</param>
        public string ToString(bool includeAll, string allName)
        {
            if (allName == null)
            {
                allName = "All image types";
            }

            if (includeAll)
            {
                StringBuilder description = new StringBuilder(allName);
                StringBuilder formats = new StringBuilder();

                for (int i = 0; i < fileTypes.Length; ++i)
                {
                    if (i == 0)
                    {
                        description.Append(" (");
                    }

                    string[] extensions = (fileTypes[i]).Extensions;

                    for (int j = 0; j < extensions.Length; ++j)
                    {
                        description.Append("*");
                        description.Append(extensions[j]);
                        formats.Append("*");
                        formats.Append(extensions[j]);

                        // if this is NOT the last extension in the whole list ...
                        if (!(j == extensions.Length - 1 && i == fileTypes.Length - 1))
                        {
                            description.Append(", ");
                            formats.Append(";");
                        }
                    }

                    if (i == fileTypes.Length - 1)
                    {
                        description.Append(")");
                    }
                }    
            
                string ret = description.ToString() + "|" + formats.ToString();

                if (fileTypes.Length != 0)
                {
                    ret += "|" + ToString();
                }

                return ret;
            }
            else
            {
                return ToString();
            }
        }

        public int IndexOfFileType(FileType fileType)
        {
            if (fileType == null)
            {
                return -1;
            }

            for (int i = 0; i < fileTypes.Length; ++i)
            {
                if (fileTypes[i].Name == fileType.Name)
                {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOfExtension(string findMeExt)
        {
            if (findMeExt == null)
            {
                return -1;
            }

            for (int i = 0; i < fileTypes.Length; ++i)
            {
                foreach (string ext in fileTypes[i].Extensions)
                {
                    if (ext.ToLower() == findMeExt.ToLower())
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int IndexOfName(string name)
        {
            for (int i = 0; i < fileTypes.Length; ++i)
            {
                if (fileTypes[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
