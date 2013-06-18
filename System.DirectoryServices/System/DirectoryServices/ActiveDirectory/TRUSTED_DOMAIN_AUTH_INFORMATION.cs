namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class TRUSTED_DOMAIN_AUTH_INFORMATION
    {
        public int IncomingAuthInfos;
        public IntPtr IncomingAuthenticationInformation;
        public IntPtr IncomingPreviousAuthenticationInformation;
        public int OutgoingAuthInfos;
        public IntPtr OutgoingAuthenticationInformation;
        public IntPtr OutgoingPreviousAuthenticationInformation;
    }
}

