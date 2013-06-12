namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCredential_SECUR32 : SafeFreeCredentials
    {
        private const string SECUR32 = "secur32.Dll";

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SECUR32.FreeCredentialsHandle(ref this._handle) == 0);
        }
    }
}

