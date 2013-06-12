namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public static class Monitor
    {
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void Enter(object obj);
        [SecuritySafeCritical]
        public static void Enter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeFalse"), "lockTaken");
            }
            ReliableEnter(obj, ref lockTaken);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static extern void Exit(object obj);
        private static int MillisecondsTimeoutFromTimeSpan(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return (int) totalMilliseconds;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ObjPulse(object obj);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ObjPulseAll(object obj);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool ObjWait(bool exitContext, int millisecondsTimeout, object obj);
        [SecuritySafeCritical]
        public static void Pulse(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ObjPulse(obj);
        }

        [SecuritySafeCritical]
        public static void PulseAll(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ObjPulseAll(obj);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ReliableEnter(object obj, ref bool lockTaken);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void ReliableEnterTimeout(object obj, int timeout, ref bool lockTaken);
        public static bool TryEnter(object obj)
        {
            bool lockTaken = false;
            TryEnter(obj, 0, ref lockTaken);
            return lockTaken;
        }

        public static bool TryEnter(object obj, int millisecondsTimeout)
        {
            bool lockTaken = false;
            TryEnter(obj, millisecondsTimeout, ref lockTaken);
            return lockTaken;
        }

        public static bool TryEnter(object obj, TimeSpan timeout)
        {
            return TryEnter(obj, MillisecondsTimeoutFromTimeSpan(timeout));
        }

        [SecuritySafeCritical]
        public static void TryEnter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeFalse"), "lockTaken");
            }
            ReliableEnterTimeout(obj, 0, ref lockTaken);
        }

        [SecuritySafeCritical]
        public static void TryEnter(object obj, int millisecondsTimeout, ref bool lockTaken)
        {
            if (lockTaken)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeFalse"), "lockTaken");
            }
            ReliableEnterTimeout(obj, millisecondsTimeout, ref lockTaken);
        }

        [SecuritySafeCritical]
        public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
        {
            if (lockTaken)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeFalse"), "lockTaken");
            }
            ReliableEnterTimeout(obj, MillisecondsTimeoutFromTimeSpan(timeout), ref lockTaken);
        }

        [SecuritySafeCritical]
        public static bool Wait(object obj)
        {
            return Wait(obj, -1, false);
        }

        [SecuritySafeCritical]
        public static bool Wait(object obj, int millisecondsTimeout)
        {
            return Wait(obj, millisecondsTimeout, false);
        }

        [SecuritySafeCritical]
        public static bool Wait(object obj, TimeSpan timeout)
        {
            return Wait(obj, MillisecondsTimeoutFromTimeSpan(timeout), false);
        }

        [SecuritySafeCritical]
        public static bool Wait(object obj, int millisecondsTimeout, bool exitContext)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return ObjWait(exitContext, millisecondsTimeout, obj);
        }

        public static bool Wait(object obj, TimeSpan timeout, bool exitContext)
        {
            return Wait(obj, MillisecondsTimeoutFromTimeSpan(timeout), exitContext);
        }
    }
}

