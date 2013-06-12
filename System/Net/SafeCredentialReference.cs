namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class SafeCredentialReference : CriticalHandleMinusOneIsInvalid
    {
        internal SafeFreeCredentials _Target;

        private SafeCredentialReference(SafeFreeCredentials target)
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target.DangerousAddRef(ref success);
            }
            catch
            {
                if (success)
                {
                    target.DangerousRelease();
                    success = false;
                }
            }
            finally
            {
                if (success)
                {
                    this._Target = target;
                    base.SetHandle(new IntPtr(0));
                }
            }
        }

        internal static SafeCredentialReference CreateReference(SafeFreeCredentials target)
        {
            SafeCredentialReference reference = new SafeCredentialReference(target);
            if (reference.IsInvalid)
            {
                return null;
            }
            return reference;
        }

        protected override bool ReleaseHandle()
        {
            SafeFreeCredentials credentials = this._Target;
            if (credentials != null)
            {
                credentials.DangerousRelease();
            }
            this._Target = null;
            return true;
        }
    }
}

