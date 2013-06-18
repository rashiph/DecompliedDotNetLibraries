namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LSA_FOREST_TRUST_COLLISION_INFORMATION
    {
        public int RecordCount;
        public IntPtr Entries;
    }
}

