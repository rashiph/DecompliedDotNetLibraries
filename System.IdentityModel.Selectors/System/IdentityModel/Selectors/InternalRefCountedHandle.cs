namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class InternalRefCountedHandle : SafeHandle
    {
        private DateTime m_expiration;
        private object m_parameters;
        private int m_refcount;

        private InternalRefCountedHandle() : base(IntPtr.Zero, true)
        {
            this.m_refcount = 1;
        }

        public void AddRef()
        {
            this.ThrowIfInvalid();
            Interlocked.Increment(ref this.m_refcount);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("infocardapi.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        private static extern bool CloseCryptoHandle([In] IntPtr hKey);
        public void Initialize(DateTime expiration, object parameters)
        {
            this.m_expiration = expiration;
            this.m_parameters = parameters;
        }

        public void Release()
        {
            this.ThrowIfInvalid();
            if (Interlocked.Decrement(ref this.m_refcount) == 0)
            {
                base.Dispose();
            }
        }

        protected override bool ReleaseHandle()
        {
            return CloseCryptoHandle(base.handle);
        }

        private void ThrowIfInvalid()
        {
            if (this.IsInvalid)
            {
                throw InfoCardTrace.ThrowHelperError(new ObjectDisposedException("InternalRefCountedHandle"));
            }
        }

        public DateTime Expiration
        {
            get
            {
                this.ThrowIfInvalid();
                return this.m_expiration;
            }
        }

        public override bool IsInvalid
        {
            get
            {
                return (IntPtr.Zero == base.handle);
            }
        }

        public object Parameters
        {
            get
            {
                this.ThrowIfInvalid();
                return this.m_parameters;
            }
        }
    }
}

