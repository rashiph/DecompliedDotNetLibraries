namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePolicyElement
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string targetEndpointAddress;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string issuerEndpointAddress;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string issuedTokenParameters;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string policyNoticeLink;
        [MarshalAs(UnmanagedType.U4)]
        public int policyNoticeVersion;
        [MarshalAs(UnmanagedType.Bool)]
        public bool isManagedCardProvider;
    }
}

