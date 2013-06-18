namespace System.ServiceModel.Activation
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation.Interop;
    using System.Threading;

    internal class HostedImpersonationContext
    {
        [SecurityCritical]
        private bool isImpersonated;
        [SecurityCritical]
        private int refCount;
        [SecurityCritical]
        private SafeCloseHandleCritical tokenHandle;

        [SecurityCritical]
        public HostedImpersonationContext()
        {
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                int num;
                if (SafeNativeMethods.OpenCurrentThreadTokenCritical(TokenAccessLevels.Query | TokenAccessLevels.Impersonate, true, out this.tokenHandle, out num))
                {
                    this.isImpersonated = true;
                    Interlocked.Increment(ref this.refCount);
                }
                else
                {
                    CloseInvalidOutSafeHandleCritical(this.tokenHandle);
                    this.tokenHandle = null;
                    if (num != 0x3f0)
                    {
                        throw FxTrace.Exception.AsError(new Win32Exception(num, System.ServiceModel.Activation.SR.Hosting_ImpersonationFailed));
                    }
                }
            }
        }

        [SecurityCritical]
        public void AddRef()
        {
            if (this.IsImpersonated)
            {
                Interlocked.Increment(ref this.refCount);
            }
        }

        [SecurityCritical]
        private static void CloseInvalidOutSafeHandleCritical(SafeHandle handle)
        {
            if (handle != null)
            {
                handle.SetHandleAsInvalid();
            }
        }

        [SecurityCritical]
        public IDisposable Impersonate()
        {
            if (!this.isImpersonated)
            {
                return null;
            }
            HostedInnerImpersonationContext context = null;
            lock (this.tokenHandle)
            {
                context = HostedInnerImpersonationContext.UnsafeCreate(this.tokenHandle.DangerousGetHandle());
                GC.KeepAlive(this.tokenHandle);
            }
            return context;
        }

        [SecurityCritical]
        public void Release()
        {
            if (this.IsImpersonated && (Interlocked.Decrement(ref this.refCount) == 0))
            {
                lock (this.tokenHandle)
                {
                    this.tokenHandle.Close();
                    this.tokenHandle = null;
                }
            }
        }

        public bool IsImpersonated
        {
            [SecuritySafeCritical]
            get
            {
                return this.isImpersonated;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class HostedInnerImpersonationContext : IDisposable
        {
            private IDisposable impersonatedContext;

            private HostedInnerImpersonationContext(IDisposable impersonatedContext)
            {
                if (impersonatedContext == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ImpersonationFailed));
                }
                this.impersonatedContext = impersonatedContext;
            }

            public void Dispose()
            {
                if (this.impersonatedContext != null)
                {
                    this.impersonatedContext.Dispose();
                    this.impersonatedContext = null;
                }
            }

            public static HostedImpersonationContext.HostedInnerImpersonationContext UnsafeCreate(IntPtr token)
            {
                return new HostedImpersonationContext.HostedInnerImpersonationContext(HostingEnvironmentWrapper.UnsafeImpersonate(token));
            }
        }
    }
}

