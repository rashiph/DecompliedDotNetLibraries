namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeDeleteContext_SCHANNEL : SafeDeleteContext
    {
        private const string SCHANNEL = "schannel.Dll";

        internal SafeDeleteContext_SCHANNEL()
        {
        }

        protected override bool ReleaseHandle()
        {
            if (base._EffectiveCredential != null)
            {
                base._EffectiveCredential.DangerousRelease();
            }
            return (UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.DeleteSecurityContext(ref this._handle) == 0);
        }
    }
}

