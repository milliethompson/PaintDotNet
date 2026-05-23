/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Provides static methods and properties related to the CPU.
	/// </summary>
    public sealed class Processor
    {
        private Processor()
        {
        }

        static Processor()
        {
            logicalCpuCount = ConcreteLogicalCpuCount;
        }

        private static int logicalCpuCount;

        /// <summary>
        /// Gets the number of logical or "virtual" processors installed in the computer.
        /// </summary>
        /// <remarks>
        /// This value may not return the actual number of processors installed in the system.
        /// It may be set to another number for testing and benchmarking purposes. It is
        /// recommended that you use this property instead of ConcreteLogicalCpuCount for the
        /// purposes of optimizing thread usage.
        /// </remarks>
        public static int LogicalCpuCount
        {
            get
            {
                return logicalCpuCount;
            }

            set
            {
                if (value < 1 || value > (IntPtr.Size * 8))
                {
                    throw new ArgumentOutOfRangeException("value", value, "must be in the range [0, " + (IntPtr.Size * 8).ToString() + "]");
                }

                logicalCpuCount = value;
            }
        }

        /// <summary>
        /// Gets the number of logical or "virtual" processors installed in the computer.
        /// </summary>
        /// <remarks>
        /// This property will always return the actual number of logical processors installed
        /// in the system. 
        /// </remarks>
        public static int ConcreteLogicalCpuCount
        {
            get
            {
                NativeStructs.SYSTEM_INFO systemInfo = new NativeStructs.SYSTEM_INFO();
                SafeNativeMethods.GetSystemInfo(ref systemInfo);
                return (int)systemInfo.dwNumberOfProcessors;
            }
        }
    }
}
