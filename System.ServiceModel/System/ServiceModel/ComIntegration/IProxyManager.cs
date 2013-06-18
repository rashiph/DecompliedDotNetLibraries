namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C05307A7-70CE-4670-92C9-52A757744A02")]
    internal interface IProxyManager
    {
        void GetIDsOfNames([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr dispid);
        [PreserveSig]
        int Invoke(uint dispIdMember, IntPtr outerProxy, IntPtr pVarResult, IntPtr pExcepInfo);
        [PreserveSig]
        int FindOrCreateProxy(IntPtr outerProxy, ref Guid riid, out IntPtr tearOff);
        void TearDownChannels();
        [PreserveSig]
        int InterfaceSupportsErrorInfo(ref Guid riid);
        [PreserveSig]
        int SupportsDispatch();
    }
}

