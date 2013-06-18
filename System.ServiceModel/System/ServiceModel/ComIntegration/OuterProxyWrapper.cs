namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class OuterProxyWrapper
    {
        private static ProxySupportWrapper proxySupport = new ProxySupportWrapper();

        public static IntPtr CreateDispatchProxy(IntPtr pOuter, IPseudoDispatch proxy)
        {
            IntPtr zero = IntPtr.Zero;
            IProxyProvider proxyProvider = proxySupport.GetProxyProvider();
            if (proxyProvider == null)
            {
                throw Fx.AssertAndThrowFatal("Proxy Provider cannot be NULL");
            }
            int errorCode = proxyProvider.CreateDispatchProxyInstance(pOuter, proxy, out zero);
            Marshal.ReleaseComObject(proxyProvider);
            if (errorCode != HR.S_OK)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("FailedProxyProviderCreation"), errorCode));
            }
            return zero;
        }

        public static IntPtr CreateOuterProxyInstance(IProxyManager proxyManager, ref Guid riid)
        {
            IntPtr zero = IntPtr.Zero;
            IProxyProvider proxyProvider = proxySupport.GetProxyProvider();
            if (proxyProvider == null)
            {
                throw Fx.AssertAndThrowFatal("Proxy Provider cannot be NULL");
            }
            Guid guid = riid;
            int errorCode = proxyProvider.CreateOuterProxyInstance(proxyManager, ref guid, out zero);
            Marshal.ReleaseComObject(proxyProvider);
            if (errorCode != HR.S_OK)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("FailedProxyProviderCreation"), errorCode));
            }
            return zero;
        }
    }
}

