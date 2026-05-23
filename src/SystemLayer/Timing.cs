/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Methods for keeping track of time in a high precision manner.
    /// </summary>
    /// <remarks>
    /// This class guarantees precision of 1 millisecond granularity.
    /// </remarks>
    public sealed class Timing
    {
        private ulong countsPerMs;
        private ulong birthTick;

        /// <summary>
        /// The number of milliseconds that elapsed between system startup
        /// and creation of this instance of Timing.
        /// </summary>
        public ulong BirthTick
        {
            get
            {
                return birthTick;
            }
        }

        /// <summary>
        /// Returns the number of milliseconds that have elapsed since
        /// system startup.
        /// </summary>
        public ulong GetTickCount()
        {
            ulong tick;
            SafeNativeMethods.QueryPerformanceCounter(out tick);
            return tick / countsPerMs;
        }

        /// <summary>
        /// Constructs an instance of the Timing class.
        /// </summary>
        public Timing()
        {
            ulong frequency;

            if (!SafeNativeMethods.QueryPerformanceFrequency(out frequency))
            {
                NativeMethods.ThrowOnWin32Error();
            }

            countsPerMs = frequency / 1000;
            birthTick = GetTickCount();
        }
    }
}
