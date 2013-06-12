namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("54F198EC-A63A-45ea-A984-452F68D9B35B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProgIdRedirectionEntry
    {
        System.Deployment.Internal.Isolation.Manifest.ProgIdRedirectionEntry AllData { [SecurityCritical] get; }
        string ProgId { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        Guid RedirectedGuid { [SecurityCritical] get; }
    }
}

