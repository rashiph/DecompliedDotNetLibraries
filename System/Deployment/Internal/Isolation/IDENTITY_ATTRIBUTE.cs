namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IDENTITY_ATTRIBUTE
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Namespace;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }
}

