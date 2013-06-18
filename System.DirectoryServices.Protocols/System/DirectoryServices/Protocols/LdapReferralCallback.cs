namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct LdapReferralCallback
    {
        public int sizeofcallback;
        public QUERYFORCONNECTIONInternal query;
        public NOTIFYOFNEWCONNECTIONInternal notify;
        public DEREFERENCECONNECTIONInternal dereference;
    }
}

