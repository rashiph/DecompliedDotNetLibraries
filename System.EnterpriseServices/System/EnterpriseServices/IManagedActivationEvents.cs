namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5f325af-572f-46da-b8ab-827c3d95d99e")]
    internal interface IManagedActivationEvents
    {
        void CreateManagedStub(IManagedObjectInfo pInfo, [MarshalAs(UnmanagedType.Bool)] bool fDist);
        void DestroyManagedStub(IManagedObjectInfo pInfo);
    }
}

