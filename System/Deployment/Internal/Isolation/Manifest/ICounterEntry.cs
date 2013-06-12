namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("8CD3FC86-AFD3-477a-8FD5-146C291195BB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICounterEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CounterEntry AllData { [SecurityCritical] get; }
        Guid CounterSetGuid { [SecurityCritical] get; }
        uint CounterId { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint CounterType { [SecurityCritical] get; }
        ulong Attributes { [SecurityCritical] get; }
        uint BaseId { [SecurityCritical] get; }
        uint DefaultScale { [SecurityCritical] get; }
    }
}

