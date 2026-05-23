/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

//#define DEBUGSPEW

#if !DEBUG
#undef DEBUGSPEW
#endif

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Contains methods for allocating, freeing, and performing operations on memory 
	/// that is fixed (pinned) in memory.
	/// </summary>
	[CLSCompliant(false)]
	public unsafe sealed class Memory
	{
        private static IntPtr hHeap;

#if DEBUGSPEW
        private static ulong totalBytes = 0;
        private static System.Threading.Timer timer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerCallbackHandler), null, 5000, 5000);
        private static Hashtable blockStackTraces = Hashtable.Synchronized(new Hashtable()); // maps IntPtr -> StackTrace
        private static Hashtable blockSizes = Hashtable.Synchronized(new Hashtable()); // maps IntPtr -> ulong
        private static void TimerCallbackHandler(object context)
        {
            System.Diagnostics.Debug.WriteLine("total bytes allocated = " + totalBytes.ToString());
        }
#endif

		private Memory()
		{
		}

        static Memory()
        {
            hHeap = SafeNativeMethods.HeapCreate(0, IntPtr.Zero, IntPtr.Zero);

            uint info = 2;

            try
            {
                // Enable the low-fragmentation heap (LFH)
                SafeNativeMethods.HeapSetInformation(hHeap, 
                    NativeConstants.HeapCompatibilityInformation,
                    (void *)&info,
                    sizeof(uint));
            } 

            catch
            {
                // If that method isn't available, like on Win2K, don't worry about it.
            }                    
            
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        private static void DestroyHeap()
        {
            IntPtr hHeap2 = hHeap;
            hHeap = IntPtr.Zero;
            SafeNativeMethods.HeapDestroy(hHeap2);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            DestroyHeap();
        }

        /// <summary>
        /// Allocates a block of memory at least as large as the amount requested.
        /// </summary>
        /// <param name="bytes">The number of bytes you want to allocate.</param>
        /// <returns>A pointer to a block of memory at least as large as <b>bytes</b>.</returns>
        /// <exception cref="OutOfMemoryException">Thrown if the memory manager could not fulfill the request for a memory block at least as large as <b>bytes</b>.</exception>
        public static IntPtr Allocate(ulong bytes)
        {
            if (hHeap == IntPtr.Zero)
            {
                throw new InvalidOperationException("heap has already been destroyed");
            }
            else
            {
                IntPtr block = SafeNativeMethods.HeapAlloc(hHeap, 0, new UIntPtr(bytes));

                if (block == IntPtr.Zero)
                {
                    throw new OutOfMemoryException("HeapAlloc returned a null pointer");
                }

#if DEBUGSPEW
                Debug.WriteLine("allocing block #" + block.ToString() + ", " + bytes.ToString() + " bytes");
                StackTrace st = new StackTrace();
                blockStackTraces.Add(block, st);
                blockSizes.Add(block, bytes);
                totalBytes += bytes;
#endif
                return block;
            }

        }

        /// <summary>
        /// Frees a block of memory previously allocated with Allocate().
        /// </summary>
        /// <param name="block">The block to free.</param>
        /// <exception cref="InvalidOperationException">There was an error freeing the block.</exception>
        public static void Free(IntPtr block)
        {
            if (Memory.hHeap != IntPtr.Zero)
            {
#if DEBUGSPEW
                UIntPtr bytes = SafeNativeMethods.HeapSize(hHeap, 0, block);
                Debug.WriteLine("freeing block #" + block.ToString() + ", " + bytes.ToUInt64().ToString() + " bytes");
#endif

                bool result = SafeNativeMethods.HeapFree(hHeap, 0, block);

                if (!result)
                {
                    int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    throw new InvalidOperationException("HeapFree returned an error");
                }

#if DEBUGSPEW
                blockStackTraces.Remove(block);
                blockSizes.Remove(block);
                totalBytes -= bytes.ToUInt64();
#endif
            }
            else
            {
#if DEBUGSPEW
                ulong bytes = (ulong)blockSizes[block];
                StackTrace stackTrace = (StackTrace)blockStackTraces[block];
                Debug.WriteLine("Memory leak! " + bytes + " bytes, Object #" + block.ToString());
                Debug.WriteLine(stackTrace);
#endif

#if DEBUG
                throw new InvalidOperationException("memory leak! check the debug output for more info, and http://blogs.msdn.com/ricom/archive/2004/12/10/279612.aspx to track it down");
#endif
            } 
        }

        /// <summary>
        /// Copies bytes from one area of memory to another. Since this function only
        /// takes pointers, it can not do any bounds checking.
        /// </summary>
        /// <param name="dst">The starting address of where to copy bytes to.</param>
        /// <param name="src">The starting address of where to copy bytes from.</param>
        /// <param name="length">The number of bytes to copy</param>
        public static void Copy(IntPtr dst, IntPtr src, ulong length)
        {
            Copy(dst.ToPointer(), src.ToPointer(), length);
        }

        /// <summary>
        /// Copies bytes from one area of memory to another. Since this function only
        /// takes pointers, it can not do any bounds checking.
        /// </summary>
        /// <param name="dst">The starting address of where to copy bytes to.</param>
        /// <param name="src">The starting address of where to copy bytes from.</param>
        /// <param name="length">The number of bytes to copy</param>
        public static void Copy(void *dst, void *src, ulong length)
        {
            SafeNativeMethods.memcpy(dst, src, new UIntPtr(length));
        }
    }
}
