namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Transactions;

    internal sealed class OciEnlistContext : SafeHandle
    {
        private OciServiceContextHandle _serviceContextHandle;

        internal OciEnlistContext(byte[] userName, byte[] password, byte[] serverName, OciServiceContextHandle serviceContextHandle, OciErrorHandle errorHandle) : base(IntPtr.Zero, true)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._serviceContextHandle = serviceContextHandle;
                int rc = 0;
                try
                {
                    rc = TracedNativeMethods.OraMTSEnlCtxGet(userName, password, serverName, this._serviceContextHandle, errorHandle, out this.handle);
                }
                catch (DllNotFoundException exception)
                {
                    throw System.Data.Common.ADP.DistribTxRequiresOracleServicesForMTS(exception);
                }
                if (rc != 0)
                {
                    OracleException.Check(errorHandle, rc);
                }
                serviceContextHandle.AddRef();
            }
        }

        internal static IntPtr HandleValueToTrace(OciEnlistContext handle)
        {
            return handle.DangerousGetHandle();
        }

        internal void Join(OracleInternalConnection internalConnection, Transaction indigoTransaction)
        {
            IDtcTransaction oletxTransaction = System.Data.Common.ADP.GetOletxTransaction(indigoTransaction);
            int rc = TracedNativeMethods.OraMTSJoinTxn(this, oletxTransaction);
            if (rc != 0)
            {
                OracleException.Check(rc, internalConnection);
            }
        }

        protected override bool ReleaseHandle()
        {
            IntPtr handle = base.handle;
            base.handle = IntPtr.Zero;
            if (IntPtr.Zero != handle)
            {
                TracedNativeMethods.OraMTSEnlCtxRel(handle);
            }
            if (this._serviceContextHandle != null)
            {
                this._serviceContextHandle.Release();
                this._serviceContextHandle = null;
            }
            return true;
        }

        internal static void SafeDispose(ref OciEnlistContext ociEnlistContext)
        {
            if (ociEnlistContext != null)
            {
                ociEnlistContext.Dispose();
            }
            ociEnlistContext = null;
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }
    }
}

