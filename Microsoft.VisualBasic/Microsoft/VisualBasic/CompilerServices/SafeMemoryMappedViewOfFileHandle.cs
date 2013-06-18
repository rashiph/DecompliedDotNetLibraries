namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeMemoryMappedViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeMemoryMappedViewOfFileHandle() : base(true)
        {
        }

        internal SafeMemoryMappedViewOfFileHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
        {
            this.SetHandle(handle);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected override bool ReleaseHandle()
        {
            bool flag;
            try
            {
                if (UnsafeNativeMethods.UnmapViewOfFile(base.handle))
                {
                    return true;
                }
                flag = false;
            }
            finally
            {
                base.handle = IntPtr.Zero;
            }
            return flag;
        }
    }
}

