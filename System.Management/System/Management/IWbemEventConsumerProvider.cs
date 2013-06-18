namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("E246107A-B06E-11D0-AD61-00C04FD8FDFF"), TypeLibType((short) 0x200), InterfaceType((short) 1)]
    internal interface IWbemEventConsumerProvider
    {
        [PreserveSig]
        int FindConsumer_([In, MarshalAs(UnmanagedType.Interface)] IWbemClassObject_DoNotMarshal pLogicalConsumer, [MarshalAs(UnmanagedType.Interface)] out IWbemUnboundObjectSink ppConsumer);
    }
}

