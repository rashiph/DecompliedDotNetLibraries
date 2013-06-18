namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("8AD3FC86-AFD3-477a-8FD5-146C291195BD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEventTagEntry
    {
        System.Deployment.Internal.Isolation.Manifest.EventTagEntry AllData { [SecurityCritical] get; }
        string TagData { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint EventID { [SecurityCritical] get; }
    }
}

