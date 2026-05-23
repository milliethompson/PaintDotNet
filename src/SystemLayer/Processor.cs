/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
        private static int logicalCpuCount;

        private Processor()
        {
        }

        static Processor()
        {
            logicalCpuCount = ConcreteLogicalCpuCount;
        }

        private static ProcessorArchitecture Convert(ushort wProcessorArchitecture)
        {
            ProcessorArchitecture platform;

            switch (wProcessorArchitecture)
            {
                case NativeConstants.PROCESSOR_ARCHITECTURE_AMD64:
                    platform = ProcessorArchitecture.X64;
                    break;

                case NativeConstants.PROCESSOR_ARCHITECTURE_IA64:
                    platform = ProcessorArchitecture.IA64;
                    break;

                case NativeConstants.PROCESSOR_ARCHITECTURE_INTEL:
                    platform = ProcessorArchitecture.X86;
                    break;

                default:
                case NativeConstants.PROCESSOR_ARCHITECTURE_UNKNOWN:
                    platform = ProcessorArchitecture.Unknown;
                    break;
            }

            return platform;
        }

        /// <summary>
        /// Returns the processor architecture that the current process is using.
        /// </summary>
        /// <remarks>
        /// Note that if the current process is 32-bit, but the OS is 64-bit, this
        /// property will still return X86 and not X64 (or IA64).
        /// </remarks>
        public static ProcessorArchitecture Architecture
        {
            get
            {
                NativeStructs.SYSTEM_INFO sysInfo = new NativeStructs.SYSTEM_INFO();
                NativeMethods.GetSystemInfo(ref sysInfo);
                ProcessorArchitecture architecture = Convert(sysInfo.wProcessorArchitecture);
                return architecture;
            }
        }

        /// <summary>
        /// Returns the processor architecture of the installed operating system.
        /// </summary>
        /// <remarks>
        /// Note that this may differ from the Architecture property if, for instance,
        /// this is a 32-bit process on a 64-bit OS.
        /// </remarks>
        public static ProcessorArchitecture NativeArchitecture
        {
            get
            {
                if (Environment.OSVersion.Version >= OS.WindowsXP)
                {
                    NativeStructs.SYSTEM_INFO sysInfo = new NativeStructs.SYSTEM_INFO();
                    NativeMethods.GetNativeSystemInfo(ref sysInfo);
                    ProcessorArchitecture architecture = Convert(sysInfo.wProcessorArchitecture);
                    return architecture;
                }
                else
                {
                    return Architecture;
                }
            }
        }

        /// <summary>
        /// Gets the number of logical or "virtual" processors installed in the computer.
        /// </summary>
        /// <remarks>
        /// This value may not return the actual number of processors installed in the system.
        /// It may be set to another number for testing and benchmarking purposes. It is
        /// recommended that you use this property instead of ConcreteLogicalCpuCount for the
        /// purposes of optimizing thread usage.
        /// The maximum value for this property is 32 when running as a 32-bit process, or
        /// 64 for a 64-bit process. Note that this implies the maximum is 32 for a 32-bit process
        /// even when running on a 64-bit system.
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
        /// in the system. Note that processors such as Intel Xeons and Pentium 4's with
        /// HyperThreading will result in values that are twice the number of physical processor
        /// packages that have been installed (i.e. 2 Xeons w/ HT => ConcreteLogicalCpuCount = 4).
        /// </remarks>
        public static int ConcreteLogicalCpuCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }
    }
}
