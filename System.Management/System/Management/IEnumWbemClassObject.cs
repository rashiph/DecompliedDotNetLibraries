namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("027947E1-D731-11CE-A357-000000000001"), InterfaceType((short) 1), TypeLibType((short) 0x200)]
    internal interface IEnumWbemClassObject
    {
        [PreserveSig]
        int Reset_();
        [PreserveSig]
        int Next_([In] int lTimeout, [In] uint uCount, [In, Out, MarshalAs(UnmanagedType.LPArray)] IWbemClassObject_DoNotMarshal[] apObjects, out uint puReturned);
        [PreserveSig]
        int NextAsync_([In] uint uCount, [In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink);
        [PreserveSig]
        int Clone_([MarshalAs(UnmanagedType.Interface)] out IEnumWbemClassObject ppEnum);
        [PreserveSig]
        int Skip_([In] int lTimeout, [In] uint nCount);
    }
}

