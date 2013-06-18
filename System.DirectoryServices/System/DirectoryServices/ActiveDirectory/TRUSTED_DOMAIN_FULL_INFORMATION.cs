namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class TRUSTED_DOMAIN_FULL_INFORMATION
    {
        public TRUSTED_DOMAIN_INFORMATION_EX Information;
        internal TRUSTED_POSIX_OFFSET_INFO PosixOffset;
        public TRUSTED_DOMAIN_AUTH_INFORMATION AuthInformation;
    }
}

