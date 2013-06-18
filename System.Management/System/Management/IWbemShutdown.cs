namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("B7B31DF9-D515-11D3-A11C-00105A1F515A"), InterfaceType((short) 1)]
    internal interface IWbemShutdown
    {
        [PreserveSig]
        int Shutdown_([In] int uReason, [In] uint uMaxMilliseconds, [In, MarshalAs(UnmanagedType.Interface)] IWbemContext pCtx);
    }
}

