namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("81c85208-fe61-4c15-b5bb-ff5ea66baad9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IManifestInformation
    {
        [SecurityCritical]
        void get_FullPath([MarshalAs(UnmanagedType.LPWStr)] out string FullPath);
    }
}

