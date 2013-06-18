namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("8CD3FC85-AFD3-477a-8FD5-146C291195BB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICounterSetEntry
    {
        System.Deployment.Internal.Isolation.Manifest.CounterSetEntry AllData { [SecurityCritical] get; }
        Guid CounterSetGuid { [SecurityCritical] get; }
        Guid ProviderGuid { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        bool InstanceType { [SecurityCritical] get; }
    }
}

