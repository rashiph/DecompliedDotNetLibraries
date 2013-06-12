namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal class IUnknownSafeHandle : SafeHandle
    {
        public IUnknownSafeHandle() : base(IntPtr.Zero, true)
        {
        }

        internal object Clone()
        {
            IUnknownSafeHandle clonedContext = new IUnknownSafeHandle();
            if (!this.IsInvalid)
            {
                HostExecutionContextManager.CloneHostSecurityContext(this, clonedContext);
            }
            return clonedContext;
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            HostExecutionContextManager.ReleaseHostSecurityContext(base.handle);
            return true;
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

