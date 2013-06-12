namespace System.Net
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCredential_SCHANNEL : SafeFreeCredentials
    {
        private const string SCHANNEL = "schannel.Dll";

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.FreeCredentialsHandle(ref this._handle) == 0);
        }
    }
}

