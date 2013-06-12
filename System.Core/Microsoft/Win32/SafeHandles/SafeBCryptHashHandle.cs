namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeBCryptHashHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private IntPtr m_hashObject;

        private SafeBCryptHashHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("bcrypt")]
        private static extern BCryptNative.ErrorCode BCryptDestroyHash(IntPtr hHash);
        protected override bool ReleaseHandle()
        {
            bool flag = BCryptDestroyHash(base.handle) == BCryptNative.ErrorCode.Success;
            if (this.m_hashObject != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.m_hashObject);
            }
            return flag;
        }

        internal IntPtr HashObject
        {
            get
            {
                return this.m_hashObject;
            }
            set
            {
                this.m_hashObject = value;
            }
        }
    }
}

