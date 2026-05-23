/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.IO;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for DeferredSerializer.
	/// </summary>
	public sealed class DeferredFormatter
	{
        private ArrayList objects = ArrayList.Synchronized(new ArrayList());
        private bool used = false;
        private object context;
        private long totalSize;
        private long totalReportedBytes;
        private int compressionLevel;
        private object lockObject = new object();

        public object Context
        {
            get
            {
                return this.context;
            }
        }

        public int CompressionLevel
        {
            get
            {
                return this.compressionLevel;
            }
        }

        public DeferredFormatter()
            : this(6, null)
        {
        }

        public DeferredFormatter(int compressionLevel, object context)
        {
            if (compressionLevel < 0 || compressionLevel > 9)
            {
                throw new ArgumentOutOfRangeException("Compression level should be between 0 and 9, inclusive");
            }

            this.compressionLevel = compressionLevel;
            this.context = context;
        }

        public void AddDeferredObject(IDeferredSerializable theObject, long objectByteSize)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished serialization");
            }

            this.totalSize += objectByteSize;
            objects.Add(theObject);
        }

        public event EventHandler ReportedBytesChanged;
        private void OnReportedBytesChanged()
        {
            if (ReportedBytesChanged != null)
            {
                ReportedBytesChanged(this, EventArgs.Empty);
            }
        }

        public long ReportedBytes
        {
            get
            {
                lock (lockObject)
                {
                    return totalReportedBytes;
                }
            }
        }

        public void ReportBytes(long bytes)
        {
            lock (lockObject)
            {
                totalReportedBytes += bytes;
            }

            OnReportedBytesChanged();
        }

        public void FinishSerialization(Stream output)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished deserialization or serialization");
            }

            used = true;

            foreach (IDeferredSerializable obj in this.objects)
            {
                obj.FinishSerialization(output, this);
            }

            this.objects = null;
        }

        public void FinishDeserialization(Stream input)
        {
            if (used)
            {
                throw new InvalidOperationException("object already finished deserialization or serialization");
            }

            used = true;

            foreach (IDeferredSerializable obj in this.objects)
            {
                obj.FinishDeserialization(input, this);
            }

            this.objects = null;
        }
    }
}
