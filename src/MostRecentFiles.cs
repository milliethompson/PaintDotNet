/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Manages the Most Recently Used list of files.
    /// </summary>
    public class MostRecentFiles
    {
        private Queue files; // contains MostRecentFile instances
        private int maxCount;

        public MostRecentFiles(int maxCount)
        {
            this.maxCount = maxCount;
            files = new Queue();
        }

        public int Count
        {
            get
            {
                return files.Count;
            }
        }

        public int MaxCount
        {
            get
            {
                return maxCount;
            }
        }

        public MostRecentFile[] GetFileList()
        {
            object[] array = files.ToArray();
            MostRecentFile[] mrfArray = new MostRecentFile[array.Length];
            array.CopyTo(mrfArray, 0);
            return mrfArray;
        }

        public bool Contains(string fileName)
        {
            string lcFileName = fileName.ToLower();

            foreach (MostRecentFile mrf in files)
            {
                string lcMrf = mrf.FileName.ToLower();

                if (0 == String.Compare(lcMrf, lcFileName))
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(MostRecentFile mrf)
        {
            if (!Contains(mrf.FileName))
            {
                files.Enqueue(mrf);

                while (files.Count > maxCount)
                {
                    files.Dequeue();
                }
            }
        }

        public void Remove(string fileName)
        {
            if (!Contains(fileName))
            {
                return;
            }

            Queue newQueue = new Queue();

            foreach (MostRecentFile mrf in files)
            {
                if (0 != string.Compare(mrf.FileName, fileName, true))
                {
                    newQueue.Enqueue(mrf);
                }
            }

            this.files = newQueue;
        }
    }
}
