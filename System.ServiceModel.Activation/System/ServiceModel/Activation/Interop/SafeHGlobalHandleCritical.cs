namespace System.ServiceModel.Activation.Interop
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Text;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeHGlobalHandleCritical : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeHGlobalHandleCritical() : base(true)
        {
        }

        private SafeHGlobalHandleCritical(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(byte[] bytes)
        {
            SafeHGlobalHandleCritical critical = AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, critical.DangerousGetHandle(), bytes.Length);
            return critical;
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(int cb)
        {
            if (cb < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("cb", cb, System.ServiceModel.Activation.SR.ValueMustBeNonNegative);
            }
            SafeHGlobalHandleCritical critical = new SafeHGlobalHandleCritical();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr handle = Marshal.AllocHGlobal(cb);
                critical.SetHandle(handle);
            }
            return critical;
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(string s)
        {
            byte[] bytes = DiagnosticUtility.Utility.AllocateByteArray((s.Length + 1) * 2);
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
            return AllocHGlobal(bytes);
        }

        public static SafeHGlobalHandleCritical AllocHGlobal(uint cb)
        {
            return AllocHGlobal((int) cb);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(base.handle);
            return true;
        }

        public static SafeHGlobalHandleCritical InvalidHandle
        {
            get
            {
                return new SafeHGlobalHandleCritical(IntPtr.Zero);
            }
        }
    }
}

