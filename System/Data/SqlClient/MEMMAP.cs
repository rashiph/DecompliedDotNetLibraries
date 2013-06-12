namespace System.Data.SqlClient
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct MEMMAP
    {
        [MarshalAs(UnmanagedType.U4)]
        internal uint dbgpid;
        [MarshalAs(UnmanagedType.U4)]
        internal uint fOption;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
        internal byte[] rgbMachineName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        internal byte[] rgbDllName;
        [MarshalAs(UnmanagedType.U4)]
        internal uint cbData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0xff)]
        internal byte[] rgbData;
    }
}

