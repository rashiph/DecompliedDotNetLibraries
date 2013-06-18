namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("87A5AD68-A38A-43ef-ACA9-EFE910E5D24C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWmiEventSource
    {
        [PreserveSig]
        void Indicate(IntPtr pIWbemClassObject);
        [PreserveSig]
        void SetStatus(int lFlags, int hResult, [MarshalAs(UnmanagedType.BStr)] string strParam, IntPtr pObjParam);
    }
}

