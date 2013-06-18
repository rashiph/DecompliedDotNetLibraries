namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("E245105B-B06E-11D0-AD61-00C04FD8FDFF"), InterfaceType((short) 1), TypeLibType((short) 0x200)]
    internal interface IWbemEventProvider
    {
        [PreserveSig]
        int ProvideEvents_([In, MarshalAs(UnmanagedType.Interface)] IWbemObjectSink pSink, [In] int lFlags);
    }
}

