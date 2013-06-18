namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000013E-0000-0000-C000-000000000046")]
    internal interface IServerSecurity
    {
        void QueryBlanket(IntPtr authnSvc, IntPtr authzSvc, IntPtr serverPrincipalName, IntPtr authnLevel, IntPtr impLevel, IntPtr clientPrincipalName, IntPtr Capabilities);
        [PreserveSig]
        int ImpersonateClient();
        [PreserveSig]
        int RevertToSelf();
        [return: MarshalAs(UnmanagedType.Bool)]
        [PreserveSig]
        bool IsImpersonating();
    }
}

