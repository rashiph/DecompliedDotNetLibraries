namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical(SecurityCriticalScope.Everything), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
    public sealed class SafeNCryptProviderHandle : SafeNCryptHandle
    {
        internal SafeNCryptProviderHandle Duplicate()
        {
            return base.Duplicate<SafeNCryptProviderHandle>();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("ncrypt.dll")]
        private static extern int NCryptFreeObject(IntPtr hObject);
        protected override bool ReleaseNativeHandle()
        {
            return (NCryptFreeObject(base.handle) == 0);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetHandleValue(IntPtr newHandleValue)
        {
            base.SetHandle(newHandleValue);
        }
    }
}

