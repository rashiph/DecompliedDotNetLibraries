namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeCryptMsgHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCryptMsgHandle() : base(true)
        {
        }

        internal SafeCryptMsgHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("crypt32.dll", SetLastError=true)]
        private static extern bool CryptMsgClose(IntPtr handle);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return CryptMsgClose(base.handle);
        }

        internal static System.Security.Cryptography.SafeCryptMsgHandle InvalidHandle
        {
            get
            {
                return new System.Security.Cryptography.SafeCryptMsgHandle(IntPtr.Zero);
            }
        }
    }
}

