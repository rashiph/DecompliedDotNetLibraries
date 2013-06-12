namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Threading;

    [SecurityCritical]
    public sealed class SafeWaitHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool bIsMutex;
        private bool bIsReservedMutex;

        private SafeWaitHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeWaitHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(existingHandle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            if (!this.bIsMutex || Environment.HasShutdownStarted)
            {
                return Win32Native.CloseHandle(base.handle);
            }
            bool flag = false;
            bool bHandleObtained = false;
            try
            {
                if (!this.bIsReservedMutex)
                {
                    Mutex.AcquireReservedMutex(ref bHandleObtained);
                }
                flag = Win32Native.CloseHandle(base.handle);
            }
            finally
            {
                if (bHandleObtained)
                {
                    Mutex.ReleaseReservedMutex();
                }
            }
            return flag;
        }

        internal void SetAsMutex()
        {
            this.bIsMutex = true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetAsReservedMutex()
        {
            this.bIsReservedMutex = true;
        }
    }
}

