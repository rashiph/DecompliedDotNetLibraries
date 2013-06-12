namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class ReaderWriterLock : CriticalFinalizerObject
    {
        private int _dwLLockID;
        private int _dwState;
        private int _dwULockID;
        private int _dwWriterID;
        private int _dwWriterSeqNum;
        private IntPtr _hObjectHandle;
        private IntPtr _hReaderEvent;
        private IntPtr _hWriterEvent;
        private short _wWriterLevel;

        [SecuritySafeCritical]
        public ReaderWriterLock()
        {
            this.PrivateInitialize();
        }

        [SecuritySafeCritical]
        public void AcquireReaderLock(int millisecondsTimeout)
        {
            this.AcquireReaderLockInternal(millisecondsTimeout);
        }

        [SecuritySafeCritical]
        public void AcquireReaderLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            this.AcquireReaderLockInternal((int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void AcquireReaderLockInternal(int millisecondsTimeout);
        [SecuritySafeCritical]
        public void AcquireWriterLock(int millisecondsTimeout)
        {
            this.AcquireWriterLockInternal(millisecondsTimeout);
        }

        [SecuritySafeCritical]
        public void AcquireWriterLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            this.AcquireWriterLockInternal((int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void AcquireWriterLockInternal(int millisecondsTimeout);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern bool AnyWritersSince(int seqNum);
        [SecuritySafeCritical]
        public void DowngradeFromWriterLock(ref LockCookie lockCookie)
        {
            this.DowngradeFromWriterLockInternal(ref lockCookie);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void DowngradeFromWriterLockInternal(ref LockCookie lockCookie);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void FCallReleaseLock(ref LockCookie result);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void FCallUpgradeToWriterLock(ref LockCookie result, int millisecondsTimeout);
        [SecuritySafeCritical]
        ~ReaderWriterLock()
        {
            this.PrivateDestruct();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void PrivateDestruct();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private extern bool PrivateGetIsReaderLockHeld();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private extern bool PrivateGetIsWriterLockHeld();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int PrivateGetWriterSeqNum();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void PrivateInitialize();
        [SecuritySafeCritical]
        public LockCookie ReleaseLock()
        {
            LockCookie result = new LockCookie();
            this.FCallReleaseLock(ref result);
            return result;
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void ReleaseReaderLock()
        {
            this.ReleaseReaderLockInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private extern void ReleaseReaderLockInternal();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public void ReleaseWriterLock()
        {
            this.ReleaseWriterLockInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private extern void ReleaseWriterLockInternal();
        [SecuritySafeCritical]
        public void RestoreLock(ref LockCookie lockCookie)
        {
            this.RestoreLockInternal(ref lockCookie);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void RestoreLockInternal(ref LockCookie lockCookie);
        [SecuritySafeCritical]
        public LockCookie UpgradeToWriterLock(int millisecondsTimeout)
        {
            LockCookie result = new LockCookie();
            this.FCallUpgradeToWriterLock(ref result, millisecondsTimeout);
            return result;
        }

        public LockCookie UpgradeToWriterLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return this.UpgradeToWriterLock((int) totalMilliseconds);
        }

        public bool IsReaderLockHeld
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            get
            {
                return this.PrivateGetIsReaderLockHeld();
            }
        }

        public bool IsWriterLockHeld
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            get
            {
                return this.PrivateGetIsWriterLockHeld();
            }
        }

        public int WriterSeqNum
        {
            [SecuritySafeCritical]
            get
            {
                return this.PrivateGetWriterSeqNum();
            }
        }
    }
}

