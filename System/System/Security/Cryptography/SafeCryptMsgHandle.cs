namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

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
        protected override bool ReleaseHandle()
        {
            return CryptMsgClose(base.handle);
        }

        internal static SafeCryptMsgHandle InvalidHandle
        {
            get
            {
                return new SafeCryptMsgHandle(IntPtr.Zero);
            }
        }
    }
}

