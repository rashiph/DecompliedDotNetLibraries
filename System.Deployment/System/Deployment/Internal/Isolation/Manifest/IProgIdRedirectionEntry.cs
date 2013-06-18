namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("54F198EC-A63A-45ea-A984-452F68D9B35B")]
    internal interface IProgIdRedirectionEntry
    {
        System.Deployment.Internal.Isolation.Manifest.ProgIdRedirectionEntry AllData { [SecurityCritical] get; }
        string ProgId { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        Guid RedirectedGuid { [SecurityCritical] get; }
    }
}

