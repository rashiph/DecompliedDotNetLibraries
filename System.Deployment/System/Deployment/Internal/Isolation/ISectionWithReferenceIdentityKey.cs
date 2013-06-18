namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("285a8876-c84a-11d7-850f-005cd062464f")]
    internal interface ISectionWithReferenceIdentityKey
    {
        void Lookup(System.Deployment.Internal.Isolation.IReferenceIdentity ReferenceIdentityKey, [MarshalAs(UnmanagedType.Interface)] out object ppUnknown);
    }
}

