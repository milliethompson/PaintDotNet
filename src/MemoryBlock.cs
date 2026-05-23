using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// Manages an arbitrarily sized block of memory. You can also create child MemoryBlocks
    /// which reference a portion of the memory allocated by a parent MemoryBlock. If the parent
    /// is disposed, the children will not be valid.
    /// </summary>
    [Serializable]
    public unsafe class MemoryBlock
        : IDisposable,
          ISerializable
    {
        private int length;

        // if parentBlock == null, then we allocated the pointer and are responsible for deallocating it
        // if parentBlock != null, then the parentBlock allocated it, not us
        [NonSerialized]
        private IntPtr pointer;

        private MemoryBlock parentBlock = null;

        public int Length
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return length;
            }
        }

        public IntPtr Pointer
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                return pointer;
            }
        }

        public unsafe byte this[int index]
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index must be posittive and less than Length");
                }

                return ((byte *)pointer.ToPointer())[index];
            }

            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("MemoryBlock");
                }

                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index must be posittive and less than Length");
                }

                ((byte *)pointer.ToPointer())[index] = value;
            }
        }

        /// <summary>
        /// Copies bytes from one area of memory to another. Since this function only
        /// takes pointers, it can not do any bounds checking.
        /// </summary>
        /// <param name="dst">The starting address of where to copy bytes to.</param>
        /// <param name="src">The starting address of where to copy bytes from.</param>
        /// <param name="length">The number of bytes to copy</param>
        public static void CopyMemory(void *dst, void *src, int length)
        {
            // Note: This accomplishes essentially the same thing as memcpy() from C.
            //       The loop is unrolled to 16 bytes
            int outerCount = length / 16;
            int innerCount = length & 15;

            int *uintDst = (int *)dst;
            int *uintSrc = (int *)src;

            while (outerCount > 0)
            {
                *uintDst = *uintSrc;
                *(uintDst + 1) = *(uintSrc + 1);
                *(uintDst + 2) = *(uintSrc + 2);
                *(uintDst + 3) = *(uintSrc + 3);

                uintDst += 4;
                uintSrc += 4;
                
                --outerCount;
            }

            byte *byteDst = (byte *)uintDst;
            byte *byteSrc = (byte *)uintSrc;

            while (innerCount > 0)
            {
                *byteDst = *byteSrc;
                ++byteDst;
                ++byteSrc;
                --innerCount;
            }

            return;
        }

        /// <summary>
        /// Copies bytes from one area of memory to another. Since this function works
        /// with MemoryBlock instances, it does bounds checking.
        /// </summary>
        /// <param name="dst">The MemoryBlock to copy bytes to.</param>
        /// <param name="dstOffset">The offset within dst to copy bytes to.</param>
        /// <param name="src">The MemoryBlock to copy bytes from.</param>
        /// <param name="srcOffset">The offset within src to copy bytes from.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public static void CopyBlock(MemoryBlock dst, int dstOffset, MemoryBlock src, int srcOffset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length must be greater than or equal to 0");
            }

            if ((dstOffset + length >= dst.Length) || (srcOffset + length >= src.Length))
            {
                throw new ArgumentOutOfRangeException("copy ranges were out of bounds");
            }

            void *dstPtr = ((byte *)dst.Pointer.ToPointer()) + dstOffset;
            void *srcPtr = ((byte *)src.Pointer.ToPointer()) + srcOffset;
            CopyMemory (dstPtr, srcPtr, length);
        }

        /// <summary>
        /// Creates a new MemoryBlock instance and allocates the requested number of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        public MemoryBlock(int bytes)
        {
            //Debug.WriteLine("Creating parent MemoryBlock (" + GetHashCode().ToString() + ")");

            length = bytes;
            parentBlock = null;
            pointer = Alloc(bytes);
        }

        /// <summary>
        /// This is what we use internally to allocate memory when you create a MemoryBlock instance.
        /// </summary>
        /// <param name="bytes">The number of bytes we want.</param>
        /// <returns>A pointer to a block of memory that can address at least as many bytes as was requested.</returns>
        private static IntPtr Alloc(int bytes)
        {
            try
            {
                return AllocBlock(bytes);
            }

            catch (OutOfMemoryException ex)
            {
                throw new OutOfMemoryException("Unable to allocate " + bytes.ToString() + " bytes", ex);
            }
        }

        /// <summary>
        /// Creates a new MemoryBlock instance that refers to part of another MemoryBlock.
        /// The other MemoryBlock is the parent, and this new instance is the child.
        /// </summary>
        public unsafe MemoryBlock(MemoryBlock parentBlock, int offset, int length)
        {
            //Debug.WriteLine("Creating child MemoryBlock (" + GetHashCode().ToString() + ")");
            
            if (offset + length > parentBlock.Length)
            {
                throw new ArgumentOutOfRangeException();
            }   

            this.parentBlock = parentBlock;
            byte *bytePointer = (byte *)parentBlock.Pointer.ToPointer();
            bytePointer += offset;
            this.pointer = new IntPtr((void *)bytePointer);
            this.length = length;
        }

        ~MemoryBlock()
        {
            //Debug.WriteLine ("MemoryBlock (" + GetHashCode().ToString() + ") Finalize()");
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            //Debug.WriteLine ("MemoryBlock (" + GetHashCode().ToString() + ") Dispose()");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            //Debug.WriteLine ("MemoryBlock (" + GetHashCode().ToString() + ") Dispose(" + disposing.ToString() + ")");
            if (!disposed)
            {
                disposed = true;

                if (disposing)
                {
                }

                if (parentBlock == null)
                {
                    FreeBlock(pointer);
                    pointer = IntPtr.Zero;
                }

                parentBlock = null;
                pointer = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Allocates a block of memory at least as large as the amount requested.
        /// </summary>
        /// <param name="bytes">The number of bytes you want to allocate.</param>
        /// <returns>A pointer to a block of memory at least as large as <b>bytes</b>.</returns>
        /// <exception cref="OutOfMemoryException">Thrown if the memory manager could not fulfill the request for a memory block at least as large as <b>bytes</b>.</exception>
        public static IntPtr AllocBlock(int bytes)
		{
			IntPtr block = NativeMethods.HeapAlloc(NativeMethods.GetProcessHeap(), 0, (uint)bytes);

			if (block == IntPtr.Zero)
			{
				throw new OutOfMemoryException("HeapAlloc returned a null pointer");
			}

			//Debug.WriteLine("Allocated " + bytes.ToString() + " bytes", "memory");
			return block;
		}

		public static void FreeBlock(IntPtr block)
		{
			//int bytes = (int)NativeMethods.HeapSize(NativeMethods.GetProcessHeap(), 0, block);
			int result = (int)NativeMethods.HeapFree(NativeMethods.GetProcessHeap(), 0, block);

			if (result == 0)
			{
				throw new InvalidOperationException("HeapFree returned an error");
			}

			//Debug.WriteLine("Freed " + bytes.ToString() + " bytes", "memory");
		}

        #region ISerializable Members

        public byte[] ToByteArray()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            byte[] array = new byte[this.length];
            Marshal.Copy(this.pointer, array, 0, array.Length);
            return array;
        }

        public byte[] ToByteArray(int startOffset, int length)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            if (startOffset + length > this.Length)
            {
                throw new ArgumentOutOfRangeException("startOffset + length must be less than Length");
            }

            byte[] array = new byte[length];
            Marshal.Copy(new IntPtr((byte *)this.pointer.ToPointer() + startOffset), array, 0, length);
            return array;
        }

        protected MemoryBlock(SerializationInfo info, StreamingContext context)
        {
            length = info.GetInt32("length");
            disposed = info.GetBoolean("disposed");
            bool hasParent = info.GetBoolean("hasParent");

            if (hasParent)
            {
                parentBlock = (MemoryBlock)info.GetValue("parentBlock", typeof(MemoryBlock));
                pointer = new IntPtr((byte *)parentBlock.Pointer.ToPointer() + info.GetInt32("parentOffset"));
            }
            else
            {
                // TODO: This takes extra memory to perform. It would be nice if
                //       we could allocate our block with Alloc, and then read
                //       straight from the stream into the IntPtr.
                byte[] array = (byte[])info.GetValue("pointerData", typeof(byte[]));
                this.pointer = Alloc(array.Length);
                Marshal.Copy(array, 0, this.pointer, array.Length);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("MemoryBlock");
            }

            info.AddValue("length", length);
            info.AddValue("disposed", disposed);
            info.AddValue("hasParent", parentBlock != null);

            if (parentBlock == null)
            {
                // TODO: This takes extra memory to perform. It would be nice
                //       if we could convert our IntPtr to a byte[] and pass
                //       that instead
                byte[] array = ToByteArray();
                info.AddValue("pointerData", array, typeof(byte[]));
            }
            else
            {
                info.AddValue("parentBlock", parentBlock, typeof(MemoryBlock));
                info.AddValue("parentOffset", ((byte *)pointer.ToPointer() - (byte *)parentBlock.Pointer.ToPointer()));
            }
        }

        #endregion
    }
}
