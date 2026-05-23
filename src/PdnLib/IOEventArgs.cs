/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for IOEventArgs.
    /// </summary>
    public class IOEventArgs
        : System.EventArgs
    {
        /// <summary>
        /// Whether we are reporting a Read or Write operation.
        /// </summary>
        private IOOperationType ioOperationType;
        public IOOperationType IOOperationType
        {
            get
            {
                return ioOperationType;
            }
        }

        /// <summary>
        /// The offset within the file that the operation is to begin, or has finished, at.
        /// </summary>
        private long position;
        public long Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// The number of bytes that were read or written.
        /// </summary>
        private int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        public IOEventArgs(IOOperationType ioOperationType, long position, int count)
        {
            this.ioOperationType = ioOperationType;
            this.position = position;
            this.count = count;
        }
    }
}
