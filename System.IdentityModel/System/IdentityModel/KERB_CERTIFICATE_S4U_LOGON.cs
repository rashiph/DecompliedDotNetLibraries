namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct KERB_CERTIFICATE_S4U_LOGON
    {
        internal KERB_LOGON_SUBMIT_TYPE MessageType;
        internal uint Flags;
        internal UNICODE_INTPTR_STRING UserPrincipalName;
        internal UNICODE_INTPTR_STRING DomainName;
        internal uint CertificateLength;
        internal IntPtr Certificate;
        internal static int Size;
        static KERB_CERTIFICATE_S4U_LOGON()
        {
            Size = Marshal.SizeOf(typeof(KERB_CERTIFICATE_S4U_LOGON));
        }
    }
}

