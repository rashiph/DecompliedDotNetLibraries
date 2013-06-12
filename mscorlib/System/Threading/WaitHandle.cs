namespace System.Threading
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public abstract class WaitHandle : MarshalByRefObject, IDisposable
    {
        private const int ERROR_TOO_MANY_POSTS = 0x12a;
        internal bool hasThreadAffinity;
        protected static readonly IntPtr InvalidHandle = GetInvalidHandle();
        private const int MAX_WAITHANDLES = 0x40;
        [SecurityCritical]
        internal Microsoft.Win32.SafeHandles.SafeWaitHandle safeWaitHandle;
        private const int WAIT_ABANDONED = 0x80;
        private const int WAIT_FAILED = 0x7fffffff;
        private const int WAIT_OBJECT_0 = 0;
        private IntPtr waitHandle;
        public const int WaitTimeout = 0x102;

        protected WaitHandle()
        {
            this.Init();
        }

        public virtual void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool explicitDisposing)
        {
            if (this.safeWaitHandle != null)
            {
                this.safeWaitHandle.Close();
            }
        }

        [SecuritySafeCritical]
        private static IntPtr GetInvalidHandle()
        {
            return Win32Native.INVALID_HANDLE_VALUE;
        }

        [SecuritySafeCritical]
        private void Init()
        {
            this.safeWaitHandle = null;
            this.waitHandle = InvalidHandle;
            this.hasThreadAffinity = false;
        }

        [SecurityCritical]
        internal static bool InternalWaitOne(SafeHandle waitableSafeHandle, long millisecondsTimeout, bool hasThreadAffinity, bool exitContext)
        {
            if (waitableSafeHandle == null)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
            }
            int num = WaitOneNative(waitableSafeHandle, (uint) millisecondsTimeout, hasThreadAffinity, exitContext);
            if (num == 0x80)
            {
                ThrowAbandonedMutexException();
            }
            return (num != 0x102);
        }

        [SecurityCritical]
        internal void SetHandleInternal(Microsoft.Win32.SafeHandles.SafeWaitHandle handle)
        {
            this.safeWaitHandle = handle;
            this.waitHandle = handle.DangerousGetHandle();
        }

        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn)
        {
            return SignalAndWait(toSignal, toWaitOn, -1, false);
        }

        [SecuritySafeCritical]
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext)
        {
            if (toSignal == null)
            {
                throw new ArgumentNullException("toSignal");
            }
            if (toWaitOn == null)
            {
                throw new ArgumentNullException("toWaitOn");
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            int num = SignalAndWaitOne(toSignal.safeWaitHandle, toWaitOn.safeWaitHandle, millisecondsTimeout, toWaitOn.hasThreadAffinity, exitContext);
            if ((0x7fffffff != num) && toSignal.hasThreadAffinity)
            {
                Thread.EndCriticalRegion();
                Thread.EndThreadAffinity();
            }
            if (0x80 == num)
            {
                ThrowAbandonedMutexException();
            }
            if (0x12a == num)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Threading.WaitHandleTooManyPosts"));
            }
            return (num == 0);
        }

        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, TimeSpan timeout, bool exitContext)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((-1L > totalMilliseconds) || (0x7fffffffL < totalMilliseconds))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return SignalAndWait(toSignal, toWaitOn, (int) totalMilliseconds, exitContext);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int SignalAndWaitOne(Microsoft.Win32.SafeHandles.SafeWaitHandle waitHandleToSignal, Microsoft.Win32.SafeHandles.SafeWaitHandle waitHandleToWaitOn, int millisecondsTimeout, bool hasThreadAffinity, bool exitContext);
        private static void ThrowAbandonedMutexException()
        {
            throw new AbandonedMutexException();
        }

        private static void ThrowAbandonedMutexException(int location, WaitHandle handle)
        {
            throw new AbandonedMutexException(location, handle);
        }

        [SecuritySafeCritical]
        public static bool WaitAll(WaitHandle[] waitHandles)
        {
            return WaitAll(waitHandles, -1, true);
        }

        [SecuritySafeCritical]
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            return WaitAll(waitHandles, millisecondsTimeout, true);
        }

        [SecuritySafeCritical]
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout)
        {
            return WaitAll(waitHandles, timeout, true);
        }

        [SecuritySafeCritical]
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_Waithandles"));
            }
            if (waitHandles.Length == 0)
            {
                throw new ArgumentNullException(Environment.GetResourceString("Argument_EmptyWaithandleArray"));
            }
            if (waitHandles.Length > 0x40)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_MaxWaitHandles"));
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            WaitHandle[] handleArray = new WaitHandle[waitHandles.Length];
            for (int i = 0; i < waitHandles.Length; i++)
            {
                WaitHandle proxy = waitHandles[i];
                if (proxy == null)
                {
                    throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayElement"));
                }
                if (RemotingServices.IsTransparentProxy(proxy))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
                }
                handleArray[i] = proxy;
            }
            int num2 = WaitMultiple(handleArray, millisecondsTimeout, exitContext, true);
            if ((0x80 <= num2) && ((0x80 + handleArray.Length) > num2))
            {
                ThrowAbandonedMutexException();
            }
            GC.KeepAlive(handleArray);
            return (num2 != 0x102);
        }

        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((-1L > totalMilliseconds) || (0x7fffffffL < totalMilliseconds))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return WaitAll(waitHandles, (int) totalMilliseconds, exitContext);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int WaitAny(WaitHandle[] waitHandles)
        {
            return WaitAny(waitHandles, -1, true);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            return WaitAny(waitHandles, millisecondsTimeout, true);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout)
        {
            return WaitAny(waitHandles, timeout, true);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_Waithandles"));
            }
            if (waitHandles.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyWaithandleArray"));
            }
            if (0x40 < waitHandles.Length)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_MaxWaitHandles"));
            }
            if (-1 > millisecondsTimeout)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            WaitHandle[] handleArray = new WaitHandle[waitHandles.Length];
            for (int i = 0; i < waitHandles.Length; i++)
            {
                WaitHandle proxy = waitHandles[i];
                if (proxy == null)
                {
                    throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayElement"));
                }
                if (RemotingServices.IsTransparentProxy(proxy))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
                }
                handleArray[i] = proxy;
            }
            int num2 = WaitMultiple(handleArray, millisecondsTimeout, exitContext, false);
            if ((0x80 <= num2) && ((0x80 + handleArray.Length) > num2))
            {
                int location = num2 - 0x80;
                if ((0 <= location) && (location < handleArray.Length))
                {
                    ThrowAbandonedMutexException(location, handleArray[location]);
                }
                else
                {
                    ThrowAbandonedMutexException();
                }
            }
            GC.KeepAlive(handleArray);
            return num2;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((-1L > totalMilliseconds) || (0x7fffffffL < totalMilliseconds))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return WaitAny(waitHandles, (int) totalMilliseconds, exitContext);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        private static extern int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll);
        public virtual bool WaitOne()
        {
            return this.WaitOne(-1, false);
        }

        public virtual bool WaitOne(int millisecondsTimeout)
        {
            return this.WaitOne(millisecondsTimeout, false);
        }

        public virtual bool WaitOne(TimeSpan timeout)
        {
            return this.WaitOne(timeout, false);
        }

        public virtual bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return this.WaitOne((long) millisecondsTimeout, exitContext);
        }

        [SecuritySafeCritical]
        private bool WaitOne(long timeout, bool exitContext)
        {
            return InternalWaitOne(this.safeWaitHandle, timeout, this.hasThreadAffinity, exitContext);
        }

        public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((-1L > totalMilliseconds) || (0x7fffffffL < totalMilliseconds))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return this.WaitOne(totalMilliseconds, exitContext);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int WaitOneNative(SafeHandle waitableSafeHandle, uint millisecondsTimeout, bool hasThreadAffinity, bool exitContext);

        [Obsolete("Use the SafeWaitHandle property instead.")]
        public virtual IntPtr Handle
        {
            [SecuritySafeCritical]
            get
            {
                if (this.safeWaitHandle != null)
                {
                    return this.safeWaitHandle.DangerousGetHandle();
                }
                return InvalidHandle;
            }
            [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                if (value == InvalidHandle)
                {
                    if (this.safeWaitHandle != null)
                    {
                        this.safeWaitHandle.SetHandleAsInvalid();
                        this.safeWaitHandle = null;
                    }
                }
                else
                {
                    this.safeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(value, true);
                }
                this.waitHandle = value;
            }
        }

        public Microsoft.Win32.SafeHandles.SafeWaitHandle SafeWaitHandle
        {
            [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (this.safeWaitHandle == null)
                {
                    this.safeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(InvalidHandle, false);
                }
                return this.safeWaitHandle;
            }
            [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (value == null)
                    {
                        this.safeWaitHandle = null;
                        this.waitHandle = InvalidHandle;
                    }
                    else
                    {
                        this.safeWaitHandle = value;
                        this.waitHandle = this.safeWaitHandle.DangerousGetHandle();
                    }
                }
            }
        }
    }
}

