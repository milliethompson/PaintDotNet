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
    /// This was written as a workaround for a bug in SharpZipLib that prevents it
    /// from working right with huge Write() commands. So we split the incoming
    /// requests into smaller requests, like 4KB each or so.
    /// 
    /// However, this didn't work around the bug. But now I use this class so that
    /// I can keep tabs on a serialization or deserialization operation and have a
    /// dialog box with a progress bar.
    /// </summary>
    public class SiphonStream
        : Stream
    {
        private Exception throwMe;

        private Stream stream;
        private int siphonSize;

        private object tag = null;
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

        /// <summary>
        /// Causes the next call to Read() or Write() to throw an IOException instead. The
        /// exception passed to this method will be used as the InnerException.
        /// </summary>
        /// <param name="throwMe"></param>
        public void Abort(Exception throwMe)
        {
            if (throwMe == null)
            {
                throw new ArgumentException("throwMe may not be null", "throwMe");
            }

            this.throwMe = throwMe;
        }

        public event IOEventHandler IOFinished;
        protected void OnIOFinished(IOEventArgs e)
        {
            if (IOFinished != null)
            {
                IOFinished(this, e);
            }
        }

        int readAccumulator = 0;
        int writeAccumulator = 0;

        private void ReadAccumulate(int count)
        {
            if (count == -1)
            {
                if (this.readAccumulator > 0)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Read, this.Position, this.readAccumulator));
                    this.readAccumulator = 0;
                }
            }
            else
            {
                WriteAccumulate(-1);
                this.readAccumulator += count;

                while (this.readAccumulator > this.siphonSize)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Read, this.Position - this.readAccumulator + this.siphonSize, this.siphonSize));
                    this.readAccumulator -= this.siphonSize;
                }
            }
        }

        private void WriteAccumulate(int count)
        {
            if (count == -1)
            {
                if (this.writeAccumulator > 0)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Write, this.Position, writeAccumulator));
                    this.writeAccumulator = 0;
                }
            }
            else
            {
                ReadAccumulate(-1);
                this.writeAccumulator += count;

                while (this.writeAccumulator > this.siphonSize)
                {
                    OnIOFinished(new IOEventArgs(IOOperationType.Write, this.Position - this.writeAccumulator + this.siphonSize, this.siphonSize));
                    this.writeAccumulator -= this.siphonSize;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (throwMe != null)
            {
                throw new IOException("Aborted", this.throwMe);
            }

            int countLeft = count;
            int amountRead = 0;

            for (int cursor = 0; cursor < count; cursor += siphonSize)
            {
                int count2 = Math.Min(siphonSize, countLeft);    

                amountRead += stream.Read(buffer, cursor, count2);
                ReadAccumulate(count2);

                countLeft -= siphonSize;
            }

            return amountRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (throwMe != null)
            {
                throw new IOException("Aborted", this.throwMe);
            }

            int countLeft = count;

            for (int cursor = 0; cursor < count; cursor += siphonSize)
            {
                int count2 = Math.Min(siphonSize, countLeft);               

                stream.Write(buffer, cursor, count2);
                WriteAccumulate(count2);

                countLeft -= siphonSize;
            }
        }

        public override bool CanRead
        {
            get
            {
                return stream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return stream.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public SiphonStream(Stream underlyingStream)
            : this(underlyingStream, 65536)
        {
        }

        public SiphonStream(Stream underlyingStream, int siphonSize)
        {
            this.stream = underlyingStream;
            this.siphonSize = siphonSize;
        }
    }
}
