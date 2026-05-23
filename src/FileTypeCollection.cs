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
        private ArrayList fileTypes;
        public FileType[] FileTypes
        {
            get
            {
                return (FileType[])fileTypes.ToArray(typeof(FileType));
            }
        }

        public FileType this[int index]
        {
            get
            {
                return (FileType)fileTypes[index];
            }
        }

        public FileTypeCollection()
        {
            fileTypes = new ArrayList();
        }

        public FileTypeCollection(IEnumerable fileTypes)
            : this()
        {
            foreach (FileType ft in fileTypes)
            {
                this.fileTypes.Add(ft);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < fileTypes.Count; ++i)
            {
                sb.Append(fileTypes[i].ToString());

                if (i != fileTypes.Count - 1)
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
        /// but includeAll is true, then this defaults to the string "All types"</param>
        public string ToString(bool includeAll, string allName)
        {
            if (allName == null)
            {
                allName = "All types";
            }

            if (includeAll)
            {
                StringBuilder description = new StringBuilder(allName);
                StringBuilder formats = new StringBuilder();

                for (int i = 0; i < fileTypes.Count; ++i)
                {
                    if (i == 0)
                    {
                        description.Append(" (");
                    }

                    string[] extensions = ((FileType)fileTypes[i]).Extensions;
                    for (int j = 0; j < extensions.Length; ++j)
                    {
                        description.Append("*");
                        description.Append(extensions[j]);
                        formats.Append("*");
                        formats.Append(extensions[j]);

                        // if this is NOT the last extension in the whole list ...
                        if (!(j == extensions.Length - 1 && i == fileTypes.Count - 1))
                        {
                            description.Append(", ");
                            formats.Append(";");
                        }
                    }

                    if (i == fileTypes.Count - 1)
                    {
                        description.Append(")");
                    }
                }    
            
                string ret = description.ToString() + "|" + formats.ToString();

                if (fileTypes.Count != 0)
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

        public int IndexOfExtension(string findMeExt)
        {
            int i = 0;

            if (findMeExt == null)
            {
                return -1;
            }

            foreach (FileType ft in fileTypes)
            {
                foreach (string ext in ft.Extensions)
                {
                    if (ext.ToLower() == findMeExt.ToLower())
                    {
                        return i;
                    }
                }

                ++i;
            }

            return -1;
        }
    }
}
