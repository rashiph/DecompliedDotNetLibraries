namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class FileAssociationEntry
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Extension;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Description;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ProgID;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DefaultIcon;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Parameter;
    }
}

