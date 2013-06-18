namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct SupportedCapability
    {
        public static string ADOid;
        public static string ADAMOid;
        static SupportedCapability()
        {
            ADOid = "1.2.840.113556.1.4.800";
            ADAMOid = "1.2.840.113556.1.4.1851";
        }
    }
}

