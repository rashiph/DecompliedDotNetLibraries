namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
    public abstract class CriticalHandleMinusOneIsInvalid : CriticalHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected CriticalHandleMinusOneIsInvalid() : base(new IntPtr(-1))
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return (base.handle == new IntPtr(-1));
            }
        }
    }
}

