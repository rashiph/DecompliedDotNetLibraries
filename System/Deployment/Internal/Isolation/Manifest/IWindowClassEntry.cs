namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWindowClassEntry
    {
        System.Deployment.Internal.Isolation.Manifest.WindowClassEntry AllData { [SecurityCritical] get; }
        string ClassName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string HostDll { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        bool fVersioned { [SecurityCritical] get; }
    }
}

