namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel;

    internal class TransactionProxyBuilder : IProxyCreator, IDisposable
    {
        private ComProxy comProxy;
        private TransactionProxy txProxy;

        private TransactionProxyBuilder(TransactionProxy proxy)
        {
            this.txProxy = proxy;
        }

        public static IntPtr CreateTransactionProxyTearOff(TransactionProxy txProxy)
        {
            IProxyCreator proxyCreator = new TransactionProxyBuilder(txProxy);
            IProxyManager proxyManager = new ProxyManager(proxyCreator);
            Guid gUID = typeof(ITransactionProxy).GUID;
            return OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref gUID);
        }

        void IDisposable.Dispose()
        {
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if (riid != typeof(ITransactionProxy).GUID)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(System.ServiceModel.SR.GetString("NoInterface", new object[] { (Guid) riid })));
            }
            if (outer == IntPtr.Zero)
            {
                DiagnosticUtility.FailFast("OuterProxy cannot be null");
            }
            if (this.comProxy == null)
            {
                this.comProxy = ComProxy.Create(outer, this.txProxy, null);
                return this.comProxy;
            }
            return this.comProxy.Clone();
        }

        bool IProxyCreator.SupportsDispatch()
        {
            return false;
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if (riid != typeof(ITransactionProxy).GUID)
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

