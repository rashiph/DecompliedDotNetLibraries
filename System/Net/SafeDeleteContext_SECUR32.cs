namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeDeleteContext_SECUR32 : SafeDeleteContext
    {
        private const string SECUR32 = "secur32.Dll";

        internal SafeDeleteContext_SECUR32()
        {
        }

        protected override bool ReleaseHandle()
        {
            if (base._EffectiveCredential != null)
            {
                base._EffectiveCredential.DangerousRelease();
            }
            return (UnsafeNclNativeMethods.SafeNetHandles_SECUR32.DeleteSecurityContext(ref this._handle) == 0);
        }
    }
}

