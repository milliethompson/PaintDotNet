using System;
using System.Runtime.InteropServices;

namespace CpuCount
{
	public sealed class Info
	{
		private Info()
		{
		}

        [DllImport("CpuCount.dll")]
        public extern static int GetPhysicalCpuCount();

        [DllImport("CpuCount.dll")]
        public extern static int GetLogicalPerPhysicalCpuCount();

        private sealed class NativeMethods
        {
            private NativeMethods()
            {
            }

            [DllImport("CpuCount.dll")]
            public extern static int IsHTSupported();
        }

        public static bool IsHTSupported()
        {
            return (NativeMethods.IsHTSupported() == 1) ? true : false;
        }
	}
}
