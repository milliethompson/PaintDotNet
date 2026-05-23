//#define FORCE_1_CPU

using System;
using System.Runtime.InteropServices;

namespace CpuCount
{
    public sealed class Info
    {
        private Info()
        {
        }

#if FORCE_1_CPU
        public static int LogicalCpuCount
        {
            get { return 1; }
        }

        public static int PhysicalCpuCount
        {
            get { return 1; }
        }

        public static int LogicalPerPhysicalCpuCount
        {
            get { return 1; }
        }

        public static bool IsHyperThreadingEnabled
        {
            get { return false; }
        }
#else
        private static int logicalCpuCount = -1;
        public static int LogicalCpuCount
        {
            get
            {
                if (logicalCpuCount == -1)
                {
                    logicalCpuCount = NativeMethods.GetPhysicalCpuCount() * NativeMethods.GetLogicalPerPhysicalCpuCount();
                }

                return logicalCpuCount;
            }
        }

        private static int physicalCpuCount = -1;
        public static int PhysicalCpuCount
        {
            get
            {
                if (physicalCpuCount == -1)
                {
                    physicalCpuCount = NativeMethods.GetPhysicalCpuCount();
                }

                return physicalCpuCount;
            }
        }

        private static int logicalPerPhysicalCpuCount = -1;
        public static int LogicalPerPhysicalCpuCount
        {
            get
            {
                if (logicalPerPhysicalCpuCount == -1)
                {
                    logicalPerPhysicalCpuCount = NativeMethods.GetLogicalPerPhysicalCpuCount();
                }

                return logicalPerPhysicalCpuCount;
            }
        }

        public static bool IsHyperThreadingEnabled
        {
            get
            {
                return (NativeMethods.IsHTSupported() == 1) ? true : false;
            }
        }
#endif

        private sealed class NativeMethods
        {
            private NativeMethods()
            {
            }

            [DllImport("CpuCount.dll")]
            public extern static int GetPhysicalCpuCount();

            [DllImport("CpuCount.dll")]
            public extern static int GetLogicalPerPhysicalCpuCount();

            [DllImport("CpuCount.dll")]
            public extern static int IsHTSupported();
        }
    }
}
