namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, TypeLibType((short) 0x200), InterfaceType((short) 1), Guid("E246107B-B06E-11D0-AD61-00C04FD8FDFF")]
    internal interface IWbemUnboundObjectSink
    {
        [PreserveSig]
        int IndicateToConsumer_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pLogicalConsumer, [In] int lNumObjects, [In, MarshalAs(UnmanagedType.Interface)] ref IWbemClassObject_DoNotMarshal apObjects);
    }
}

