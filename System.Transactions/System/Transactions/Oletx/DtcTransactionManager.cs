namespace System.Transactions.Oletx
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class DtcTransactionManager
    {
        private bool initialized;
        private string nodeName;
        private OletxTransactionManager oletxTm;
        private IDtcProxyShimFactory proxyShimFactory;
        private byte[] whereabouts;
        private uint whereaboutsSize;

        internal DtcTransactionManager(string nodeName, OletxTransactionManager oletxTm)
        {
            this.nodeName = nodeName;
            this.oletxTm = oletxTm;
            this.initialized = false;
            this.proxyShimFactory = OletxTransactionManager.proxyShimFactory;
        }

        internal static uint AdjustTimeout(TimeSpan timeout)
        {
            try
            {
                return Convert.ToUInt32(timeout.TotalMilliseconds, CultureInfo.CurrentCulture);
            }
            catch (OverflowException exception)
            {
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
                return uint.MaxValue;
            }
        }

        private void Initialize()
        {
            if (!this.initialized)
            {
                OletxInternalResourceManager internalResourceManager = this.oletxTm.internalResourceManager;
                IntPtr zero = IntPtr.Zero;
                IResourceManagerShim resourceManagerShim = null;
                bool nodeNameMatches = false;
                CoTaskMemHandle whereaboutsBuffer = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    zero = HandleTable.AllocHandle(internalResourceManager);
                    this.proxyShimFactory.ConnectToProxy(this.nodeName, internalResourceManager.Identifier, zero, out nodeNameMatches, out this.whereaboutsSize, out whereaboutsBuffer, out resourceManagerShim);
                    if (!nodeNameMatches)
                    {
                        throw new NotSupportedException(System.Transactions.SR.GetString("ProxyCannotSupportMultipleNodeNames"));
                    }
                    if ((whereaboutsBuffer != null) && (this.whereaboutsSize != 0))
                    {
                        this.whereabouts = new byte[this.whereaboutsSize];
                        Marshal.Copy(whereaboutsBuffer.DangerousGetHandle(), this.whereabouts, 0, Convert.ToInt32(this.whereaboutsSize));
                    }
                    internalResourceManager.resourceManagerShim = resourceManagerShim;
                    internalResourceManager.CallReenlistComplete();
                    this.initialized = true;
                }
                catch (COMException exception)
                {
                    if (System.Transactions.Oletx.NativeMethods.XACT_E_NOTSUPPORTED == exception.ErrorCode)
                    {
                        throw new NotSupportedException(System.Transactions.SR.GetString("CannotSupportNodeNameSpecification"));
                    }
                    OletxTransactionManager.ProxyException(exception);
                    throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionManagerCommunicationException"), exception);
                }
                finally
                {
                    if (whereaboutsBuffer != null)
                    {
                        whereaboutsBuffer.Close();
                    }
                    if (!this.initialized)
                    {
                        if ((zero != IntPtr.Zero) && (resourceManagerShim == null))
                        {
                            HandleTable.FreeHandle(zero);
                        }
                        if (this.whereabouts != null)
                        {
                            this.whereabouts = null;
                            this.whereaboutsSize = 0;
                        }
                    }
                }
            }
        }

        internal void ReleaseProxy()
        {
            lock (this)
            {
                this.whereabouts = null;
                this.whereaboutsSize = 0;
                this.initialized = false;
            }
        }

        internal IDtcProxyShimFactory ProxyShimFactory
        {
            get
            {
                if (!this.initialized)
                {
                    lock (this)
                    {
                        this.Initialize();
                    }
                }
                return this.proxyShimFactory;
            }
        }

        internal byte[] Whereabouts
        {
            get
            {
                if (!this.initialized)
                {
                    lock (this)
                    {
                        this.Initialize();
                    }
                }
                return this.whereabouts;
            }
        }
    }
}

