namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeDeleteContext_SECURITY : SafeDeleteContext
    {
        private const string SECURITY = "security.Dll";

        internal SafeDeleteContext_SECURITY()
        {
        }

        protected override bool ReleaseHandle()
        {
            if (base._EffectiveCredential != null)
            {
                base._EffectiveCredential.DangerousRelease();
            }
            return (UnsafeNclNativeMethods.SafeNetHandles_SECURITY.DeleteSecurityContext(ref this._handle) == 0);
        }
    }
}

