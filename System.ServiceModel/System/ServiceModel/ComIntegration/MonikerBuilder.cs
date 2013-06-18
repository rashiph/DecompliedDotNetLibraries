namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Services;
    using System.ServiceModel;

    internal class MonikerBuilder : IProxyCreator, IDisposable
    {
        private ComProxy comProxy;

        private MonikerBuilder()
        {
        }

        public static MarshalByRefObject CreateMonikerInstance()
        {
            IProxyCreator proxyCreator = new MonikerBuilder();
            IProxyManager proxyManager = new ProxyManager(proxyCreator);
            Guid gUID = typeof(IMoniker).GUID;
            IntPtr punk = OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref gUID);
            MarshalByRefObject obj2 = EnterpriseServicesHelper.WrapIUnknownWithComObject(punk) as MarshalByRefObject;
            Marshal.Release(punk);
            return obj2;
        }

        void IDisposable.Dispose()
        {
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if ((riid != typeof(IMoniker).GUID) && (riid != typeof(IParseDisplayName).GUID))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(System.ServiceModel.SR.GetString("NoInterface", new object[] { (Guid) riid })));
            }
            if (outer == IntPtr.Zero)
            {
                throw Fx.AssertAndThrow("OuterProxy cannot be null");
            }
            if (this.comProxy == null)
            {
                ServiceMonikerInternal internal2 = null;
                try
                {
                    internal2 = new ServiceMonikerInternal();
                    this.comProxy = ComProxy.Create(outer, internal2, internal2);
                    return this.comProxy;
                }
                finally
                {
                    if ((this.comProxy == null) && (internal2 != null))
                    {
                        ((IDisposable) internal2).Dispose();
                    }
                }
            }
            return this.comProxy.Clone();
        }

        bool IProxyCreator.SupportsDispatch()
        {
            return false;
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if ((riid != typeof(IMoniker).GUID) && (riid != typeof(IParseDisplayName).GUID))
            {
                return false;
            }
            return true;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return false;
        }
    }
}

