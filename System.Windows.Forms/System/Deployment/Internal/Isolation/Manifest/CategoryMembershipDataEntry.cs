namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class CategoryMembershipDataEntry
    {
        public uint index;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Xml;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Description;
    }
}

