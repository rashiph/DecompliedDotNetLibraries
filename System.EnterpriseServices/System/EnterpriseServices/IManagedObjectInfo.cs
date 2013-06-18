namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("1427c51a-4584-49d8-90a0-c50d8086cbe9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IManagedObjectInfo
    {
        void GetIUnknown(out IntPtr pUnk);
        void GetIObjectControl(out IObjectControl pCtrl);
        void SetInPool([MarshalAs(UnmanagedType.Bool)] bool fInPool, IntPtr pPooledObject);
        void SetWrapperStrength([MarshalAs(UnmanagedType.Bool)] bool bStrong);
    }
}

