namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
    public abstract class CriticalHandleZeroOrMinusOneIsInvalid : CriticalHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected CriticalHandleZeroOrMinusOneIsInvalid() : base(IntPtr.Zero)
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                if (!this.handle.IsNull())
                {
                    return (base.handle == new IntPtr(-1));
                }
                return true;
            }
        }
    }
}

