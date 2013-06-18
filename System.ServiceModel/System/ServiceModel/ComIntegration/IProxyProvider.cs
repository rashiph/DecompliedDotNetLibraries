namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("11281BB7-1253-45ef-B98F-D551F79499FD")]
    internal interface IProxyProvider
    {
        [PreserveSig]
        int CreateOuterProxyInstance(IProxyManager proxyManager, [In] ref Guid riid, out IntPtr ppv);
        [PreserveSig]
        int CreateDispatchProxyInstance(IntPtr outer, IPseudoDispatch proxy, out IntPtr ppvInner);
    }
}

