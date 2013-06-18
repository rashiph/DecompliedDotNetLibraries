namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel.Diagnostics;

    internal class SafeKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private System.IdentityModel.SafeProvHandle provHandle;

        private SafeKeyHandle() : base(true)
        {
        }

        private SafeKeyHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            bool flag = System.IdentityModel.NativeMethods.CryptDestroyKey(base.handle);
            if (this.provHandle != null)
            {
                this.provHandle.DangerousRelease();
                this.provHandle = null;
            }
            return flag;
        }

        internal static unsafe System.IdentityModel.SafeKeyHandle SafeCryptImportKey(System.IdentityModel.SafeProvHandle provHandle, void* pbDataPtr, int cbData)
        {
            bool success = false;
            int error = 0;
            System.IdentityModel.SafeKeyHandle phKey = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                provHandle.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    provHandle.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    success = System.IdentityModel.NativeMethods.CryptImportKey(provHandle, pbDataPtr, (uint) cbData, IntPtr.Zero, 0, out phKey);
                    if (!success)
                    {
                        error = Marshal.GetLastWin32Error();
                        provHandle.DangerousRelease();
                    }
                    else
                    {
                        phKey.provHandle = provHandle;
                    }
                }
            }
            if (!success)
            {
                Utility.CloseInvalidOutSafeHandle(phKey);
                string str = (error != 0) ? new Win32Exception(error).Message : string.Empty;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("AESCryptImportKeyFailed", new object[] { str })));
            }
            return phKey;
        }

        internal static System.IdentityModel.SafeKeyHandle InvalidHandle
        {
            get
            {
                return new System.IdentityModel.SafeKeyHandle(IntPtr.Zero);
            }
        }
    }
}

