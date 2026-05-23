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

        public event IOEventHandler IOBeginning;
        protected void OnIOBeginning(IOEventArgs e)
        {
            if (IOBeginning != null)
            {
                IOBeginning(this, e);
            }
        }

        public event IOEventHandler IOFinished;
        protected void OnIOFinished(IOEventArgs e)
        {
            if (IOFinished != null)
            {
                IOFinished(this, e);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int countLeft = count;
            int amountRead = 0;

            for (int cursor = 0; cursor < count; cursor += siphonSize)
            {
                int count2 = Math.Min(siphonSize, countLeft);    
                long position = this.Position;

                OnIOBeginning(new IOEventArgs(IOOperation.Read, position, count2));
                amountRead += stream.Read(buffer, cursor, count2);
                OnIOFinished(new IOEventArgs(IOOperation.Read, position, count2));

                countLeft -= siphonSize;
            }

            return amountRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int countLeft = count;

            for (int cursor = 0; cursor < count; cursor += siphonSize)
            {
                int count2 = Math.Min(siphonSize, countLeft);               
                long position = this.Position;

                OnIOBeginning(new IOEventArgs(IOOperation.Write, position, count2));
                stream.Write(buffer, cursor, count2);
                OnIOFinished(new IOEventArgs(IOOperation.Write, position, count2));

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
