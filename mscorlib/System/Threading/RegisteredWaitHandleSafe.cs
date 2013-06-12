namespace System.Threading
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class RegisteredWaitHandleSafe : CriticalFinalizerObject
    {
        private bool bReleaseNeeded;
        private WaitHandle m_internalWaitObject;
        private int m_lock;
        private IntPtr registeredWaitHandle = InvalidHandle;

        internal RegisteredWaitHandleSafe()
        {
        }

        [SecuritySafeCritical]
        ~RegisteredWaitHandleSafe()
        {
            bool flag = false;
            do
            {
                if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                {
                    flag = true;
                    try
                    {
                        if (this.ValidHandle())
                        {
                            WaitHandleCleanupNative(this.registeredWaitHandle);
                            if (this.bReleaseNeeded)
                            {
                                this.m_internalWaitObject.SafeWaitHandle.DangerousRelease();
                                this.bReleaseNeeded = false;
                            }
                            this.SetHandle(InvalidHandle);
                            this.m_internalWaitObject = null;
                        }
                    }
                    finally
                    {
                        this.m_lock = 0;
                    }
                }
                Thread.SpinWait(1);
            }
            while (!flag);
        }

        internal IntPtr GetHandle()
        {
            return this.registeredWaitHandle;
        }

        internal void SetHandle(IntPtr handle)
        {
            this.registeredWaitHandle = handle;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void SetWaitObject(WaitHandle waitObject)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.m_internalWaitObject = waitObject;
                if (waitObject != null)
                {
                    this.m_internalWaitObject.SafeWaitHandle.DangerousAddRef(ref this.bReleaseNeeded);
                }
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal bool Unregister(WaitHandle waitObject)
        {
            bool flag = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                bool flag2 = false;
                do
                {
                    if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                    {
                        flag2 = true;
                        try
                        {
                            if (this.ValidHandle())
                            {
                                flag = UnregisterWaitNative(this.GetHandle(), (waitObject == null) ? null : waitObject.SafeWaitHandle);
                                if (flag)
                                {
                                    if (this.bReleaseNeeded)
                                    {
                                        this.m_internalWaitObject.SafeWaitHandle.DangerousRelease();
                                        this.bReleaseNeeded = false;
                                    }
                                    this.SetHandle(InvalidHandle);
                                    this.m_internalWaitObject = null;
                                    GC.SuppressFinalize(this);
                                }
                            }
                        }
                        finally
                        {
                            this.m_lock = 0;
                        }
                    }
                    Thread.SpinWait(1);
                }
                while (!flag2);
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool UnregisterWaitNative(IntPtr handle, SafeHandle waitObject);
        private bool ValidHandle()
        {
            return ((this.registeredWaitHandle != InvalidHandle) && (this.registeredWaitHandle != IntPtr.Zero));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void WaitHandleCleanupNative(IntPtr handle);

        private static IntPtr InvalidHandle
        {
            [SecuritySafeCritical]
            get
            {
                return Win32Native.INVALID_HANDLE_VALUE;
            }
        }
    }
}

