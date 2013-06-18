namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class TRUSTED_DOMAIN_INFORMATION_EX
    {
        public LSA_UNICODE_STRING Name;
        public LSA_UNICODE_STRING FlatName;
        public IntPtr Sid;
        public int TrustDirection;
        public int TrustType;
        public TRUST_ATTRIBUTE TrustAttributes;
    }
}

