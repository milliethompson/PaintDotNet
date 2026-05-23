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
        private IOOperation ioOperation;
        public IOOperation IOOperation
        {
            get
            {
                return ioOperation;
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

		public IOEventArgs(IOOperation ioOperation, long position, int count)
		{
            this.ioOperation = ioOperation;
            this.position = position;
            this.count = count;
		}
	}
}
