/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Encapsulates an array of WaitHandles and methods for waiting on them.
    /// This class does not take ownership of the WaitHandles; you must still
    /// Dispose() them yourself.
    /// </summary>
    /// <remarks>
    /// This class exists because System.Threading.WaitHandle.Wait[Any|All] will throw an exception
    /// in an STA apartment. So we must P/Invoke down to WaitForMultipleObjects().
    /// </remarks>
    public sealed class WaitHandleArray
    {
        private WaitHandle[] waitHandles;
        private IntPtr[] nativeHandles;

        /// <summary>
        /// The minimum value that may be passed to the constructor for initialization.
        /// </summary>
        public const int MinimumCount = 1;

        /// <summary>
        /// The maximum value that may be passed to the construct for initialization.
        /// </summary>
        public const int MaximumCount = 64; // WaitForMultipleObjects() can only wait on up to 64 objects at once

        /// <summary>
        /// Gets or sets the WaitHandle at the specified index.
        /// </summary>
        public WaitHandle this[int index]
        {
            get
            {
                return waitHandles[index];
            }

            set
            {
                waitHandles[index] = value;
                nativeHandles[index] = value.Handle;
            }
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length
        {
            get
            {
                return waitHandles.Length;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the WaitHandleArray class.
        /// </summary>
        /// <param name="count">The size of the array.</param>
        public WaitHandleArray(int count)
        {
            if (count < 1 || count > 64)
            {
                throw new ArgumentOutOfRangeException("count", "must be between 1 and 64, inclusive");
            }

            this.waitHandles = new WaitHandle[count];
            this.nativeHandles = new IntPtr[count];
        }
        
        /// <summary>
        /// Waits for all of the WaitHandles to be signaled.
        /// </summary>
        public void WaitAll()
        {
            uint result = SafeNativeMethods.WaitForMultipleObjects(this.nativeHandles, true, NativeConstants.INFINITE);
        }

        /// <summary>
        /// Waits for any of the WaitHandles to be signaled.
        /// </summary>
        /// <returns>
        /// The index of the first item in the array that completed the wait operation.
        /// If this value is outside the bounds of the array, it is an indication of an
        /// error.
        /// </returns>
        public int WaitAny()
        {
            int returnVal = (int)SafeNativeMethods.WaitForMultipleObjects(this.nativeHandles, false, NativeConstants.INFINITE);
            return returnVal;
        }
    }
}
