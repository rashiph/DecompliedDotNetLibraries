namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal class CriticalAllocHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        public static CriticalAllocHandle FromSize(int size)
        {
            CriticalAllocHandle handle = new CriticalAllocHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                handle.SetHandle(Marshal.AllocHGlobal(size));
            }
            return handle;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static implicit operator IntPtr(CriticalAllocHandle safeHandle)
        {
            if (safeHandle == null)
            {
                return IntPtr.Zero;
            }
            return safeHandle.handle;
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(base.handle);
            return true;
        }
    }
}

