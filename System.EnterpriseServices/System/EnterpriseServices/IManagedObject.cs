namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C3FCC19E-A970-11d2-8B5A-00A0C9B7C9C4")]
    internal interface IManagedObject
    {
        void GetSerializedBuffer(ref string s);
        void GetObjectIdentity(ref string s, ref int AppDomainID, ref int ccw);
    }
}

