namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal class SizedReference : IDisposable
    {
        internal volatile IntPtr _handle;

        [SecuritySafeCritical]
        public SizedReference(object target)
        {
            IntPtr zero = IntPtr.Zero;
            zero = CreateSizedRef(target);
            this._handle = zero;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IntPtr CreateSizedRef(object o);
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Free();
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        ~SizedReference()
        {
            this.Free();
        }

        [SecuritySafeCritical]
        private void Free()
        {
            IntPtr comparand = this._handle;
            if ((comparand != IntPtr.Zero) && (Interlocked.CompareExchange(ref this._handle, IntPtr.Zero, comparand) == comparand))
            {
                FreeSizedRef(comparand);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void FreeSizedRef(IntPtr h);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern long GetApproximateSizeOfSizedRef(IntPtr h);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object GetTargetOfSizedRef(IntPtr h);

        public long ApproximateSize
        {
            [SecuritySafeCritical]
            get
            {
                IntPtr h = this._handle;
                if (h == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }
                long approximateSizeOfSizedRef = GetApproximateSizeOfSizedRef(h);
                if (this._handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }
                return approximateSizeOfSizedRef;
            }
        }

        public object Target
        {
            [SecuritySafeCritical]
            get
            {
                IntPtr h = this._handle;
                if (h != IntPtr.Zero)
                {
                    object targetOfSizedRef = GetTargetOfSizedRef(h);
                    if (this._handle != IntPtr.Zero)
                    {
                        return targetOfSizedRef;
                    }
                }
                return null;
            }
        }
    }
}

