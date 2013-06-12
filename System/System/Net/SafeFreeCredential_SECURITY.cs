namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCredential_SECURITY : SafeFreeCredentials
    {
        private const string SECURITY = "security.Dll";

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeCredentialsHandle(ref this._handle) == 0);
        }
    }
}

