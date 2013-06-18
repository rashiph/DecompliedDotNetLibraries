namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Security;
    using System.ServiceModel;

    [SuppressUnmanagedCodeSecurity]
    internal class PipeHandle : SafeHandleMinusOneIsInvalid
    {
        internal PipeHandle() : base(true)
        {
        }

        internal PipeHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        internal int GetClientPid()
        {
            int num;
            if (!UnsafeNativeMethods.GetNamedPipeClientProcessId(this, out num))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
            return num;
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNativeMethods.CloseHandle(base.handle) != 0);
        }
    }
}

