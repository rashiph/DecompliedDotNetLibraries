namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("c7b67079-8255-42c6-9ec0-6994a3548780")]
    internal interface IAppDomainHelper
    {
        void Initialize(IntPtr pUnkAD, IntPtr pfnShutdownCB, IntPtr data);
        void DoCallback(IntPtr pUnkAD, IntPtr pfnCallbackCB, IntPtr data);
    }
}

