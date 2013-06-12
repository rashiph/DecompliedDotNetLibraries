namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BB")]
    internal interface IEventEntry
    {
        System.Deployment.Internal.Isolation.Manifest.EventEntry AllData { [SecurityCritical] get; }
        uint EventID { [SecurityCritical] get; }
        uint Level { [SecurityCritical] get; }
        uint Version { [SecurityCritical] get; }
        System.Guid Guid { [SecurityCritical] get; }
        string SubTypeName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint SubTypeValue { [SecurityCritical] get; }
        string DisplayName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint EventNameMicrodomIndex { [SecurityCritical] get; }
    }
}

