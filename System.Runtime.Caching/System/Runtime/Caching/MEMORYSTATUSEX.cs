namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct MEMORYSTATUSEX
    {
        internal int dwLength;
        internal int dwMemoryLoad;
        internal long ullTotalPhys;
        internal long ullAvailPhys;
        internal long ullTotalPageFile;
        internal long ullAvailPageFile;
        internal long ullTotalVirtual;
        internal long ullAvailVirtual;
        internal long ullAvailExtendedVirtual;
        internal void Init()
        {
            this.dwLength = Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }
}

