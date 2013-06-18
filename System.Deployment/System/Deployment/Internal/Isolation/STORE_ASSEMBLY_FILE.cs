namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct STORE_ASSEMBLY_FILE
    {
        public uint Size;
        public uint Flags;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string FileName;
        public uint FileStatusFlags;
    }
}

