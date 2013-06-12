namespace System.Threading
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ReaderWriterLockSlim : IDisposable
    {
        private bool fDisposed;
        private bool fIsReentrant;
        private bool fNoWaiters;
        private bool fUpgradeThreadHoldingRead;
        private const int hashTableSize = 0xff;
        private const int LockSleep0Count = 5;
        private const int LockSpinCount = 10;
        private const int LockSpinCycles = 20;
        private const uint MAX_READER = 0xffffffe;
        private const int MaxSpinCount = 20;
        private int myLock;
        private uint numReadWaiters;
        private uint numUpgradeWaiters;
        private uint numWriteUpgradeWaiters;
        private uint numWriteWaiters;
        private uint owners;
        private const uint READER_MASK = 0xfffffff;
        private EventWaitHandle readEvent;
        private ReaderWriterCount[] rwc;
        private EventWaitHandle upgradeEvent;
        private int upgradeLockOwnerId;
        private const uint WAITING_UPGRADER = 0x20000000;
        private const uint WAITING_WRITERS = 0x40000000;
        private EventWaitHandle waitUpgradeEvent;
        private EventWaitHandle writeEvent;
        private int writeLockOwnerId;
        private const uint WRITER_HELD = 0x80000000;

        public ReaderWriterLockSlim() : this(LockRecursionPolicy.NoRecursion)
        {
        }

        public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
        {
            if (recursionPolicy == LockRecursionPolicy.SupportsRecursion)
            {
                this.fIsReentrant = true;
            }
            this.InitializeThreadCounts();
        }

        private void ClearUpgraderWaiting()
        {
            this.owners &= 0xdfffffff;
        }

        private void ClearWriterAcquired()
        {
            this.owners &= 0x7fffffff;
        }

        private void ClearWritersWaiting()
        {
            this.owners &= 0xbfffffff;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.fDisposed)
                {
                    throw new ObjectDisposedException(null);
                }
                if (((this.WaitingReadCount > 0) || (this.WaitingUpgradeCount > 0)) || (this.WaitingWriteCount > 0))
                {
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_IncorrectDispose"));
                }
                if ((this.IsReadLockHeld || this.IsUpgradeableReadLockHeld) || this.IsWriteLockHeld)
                {
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_IncorrectDispose"));
                }
                if (this.writeEvent != null)
                {
                    this.writeEvent.Close();
                    this.writeEvent = null;
                }
                if (this.readEvent != null)
                {
                    this.readEvent.Close();
                    this.readEvent = null;
                }
                if (this.upgradeEvent != null)
                {
                    this.upgradeEvent.Close();
                    this.upgradeEvent = null;
                }
                if (this.waitUpgradeEvent != null)
                {
                    this.waitUpgradeEvent.Close();
                    this.waitUpgradeEvent = null;
                }
                this.fDisposed = true;
            }
        }

        private void EnterMyLock()
        {
            if (Interlocked.CompareExchange(ref this.myLock, 1, 0) != 0)
            {
                this.EnterMyLockSpin();
            }
        }

        private void EnterMyLockSpin()
        {
            int processorCount = Environment.ProcessorCount;
            int num2 = 0;
            while (true)
            {
                if ((num2 < 10) && (processorCount > 1))
                {
                    Thread.SpinWait(20 * (num2 + 1));
                }
                else if (num2 < 15)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep(1);
                }
                if ((this.myLock == 0) && (Interlocked.CompareExchange(ref this.myLock, 1, 0) == 0))
                {
                    return;
                }
                num2++;
            }
        }

        public void EnterReadLock()
        {
            this.TryEnterReadLock(-1);
        }

        public void EnterUpgradeableReadLock()
        {
            this.TryEnterUpgradeableReadLock(-1);
        }

        public void EnterWriteLock()
        {
            this.TryEnterWriteLock(-1);
        }

        private void ExitAndWakeUpAppropriateWaiters()
        {
            if (this.fNoWaiters)
            {
                this.ExitMyLock();
            }
            else
            {
                this.ExitAndWakeUpAppropriateWaitersPreferringWriters();
            }
        }

        private void ExitAndWakeUpAppropriateWaitersPreferringWriters()
        {
            bool flag = false;
            bool flag2 = false;
            uint numReaders = this.GetNumReaders();
            if ((this.fIsReentrant && (this.numWriteUpgradeWaiters > 0)) && (this.fUpgradeThreadHoldingRead && (numReaders == 2)))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 1) && (this.numWriteUpgradeWaiters > 0))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 0) && (this.numWriteWaiters > 0))
            {
                this.ExitMyLock();
                this.writeEvent.Set();
            }
            else if (numReaders >= 0)
            {
                if ((this.numReadWaiters == 0) && (this.numUpgradeWaiters == 0))
                {
                    this.ExitMyLock();
                }
                else
                {
                    if (this.numReadWaiters != 0)
                    {
                        flag2 = true;
                    }
                    if ((this.numUpgradeWaiters != 0) && (this.upgradeLockOwnerId == -1))
                    {
                        flag = true;
                    }
                    this.ExitMyLock();
                    if (flag2)
                    {
                        this.readEvent.Set();
                    }
                    if (flag)
                    {
                        this.upgradeEvent.Set();
                    }
                }
            }
            else
            {
                this.ExitMyLock();
            }
        }

        private void ExitMyLock()
        {
            this.myLock = 0;
        }

        public void ExitReadLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            ReaderWriterCount threadRWCount = null;
            this.EnterMyLock();
            threadRWCount = this.GetThreadRWCount(managedThreadId, true);
            if (!this.fIsReentrant)
            {
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedRead"));
                }
            }
            else
            {
                if ((threadRWCount == null) || (threadRWCount.readercount < 1))
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedRead"));
                }
                if (threadRWCount.readercount > 1)
                {
                    threadRWCount.readercount--;
                    this.ExitMyLock();
                    Thread.EndCriticalRegion();
                    return;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    this.fUpgradeThreadHoldingRead = false;
                }
            }
            this.owners--;
            threadRWCount.readercount--;
            this.ExitAndWakeUpAppropriateWaiters();
            Thread.EndCriticalRegion();
        }

        public void ExitUpgradeableReadLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId != this.upgradeLockOwnerId)
                {
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedUpgrade"));
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedUpgrade"));
                }
                RecursiveCounts rc = threadRWCount.rc;
                if (rc.upgradecount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedUpgrade"));
                }
                rc.upgradecount--;
                if (rc.upgradecount > 0)
                {
                    this.ExitMyLock();
                    Thread.EndCriticalRegion();
                    return;
                }
                this.fUpgradeThreadHoldingRead = false;
            }
            this.owners--;
            this.upgradeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();
            Thread.EndCriticalRegion();
        }

        public void ExitWriteLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId != this.writeLockOwnerId)
                {
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedWrite"));
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedWrite"));
                }
                RecursiveCounts rc = threadRWCount.rc;
                if (rc.writercount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException(System.SR.GetString("SynchronizationLockException_MisMatchedWrite"));
                }
                rc.writercount--;
                if (rc.writercount > 0)
                {
                    this.ExitMyLock();
                    Thread.EndCriticalRegion();
                    return;
                }
            }
            this.ClearWriterAcquired();
            this.writeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();
            Thread.EndCriticalRegion();
        }

        private uint GetNumReaders()
        {
            return (this.owners & 0xfffffff);
        }

        private ReaderWriterCount GetThreadRWCount(int id, bool DontAllocate)
        {
            int index = id & 0xff;
            ReaderWriterCount count = null;
            if (this.rwc[index] == null)
            {
                if (DontAllocate)
                {
                    return null;
                }
                this.rwc[index] = new ReaderWriterCount(this.fIsReentrant);
            }
            if (this.rwc[index].threadid == id)
            {
                return this.rwc[index];
            }
            if (IsRWEntryEmpty(this.rwc[index]) && !DontAllocate)
            {
                if (this.rwc[index].next == null)
                {
                    this.rwc[index].threadid = id;
                    return this.rwc[index];
                }
                count = this.rwc[index];
            }
            ReaderWriterCount next = this.rwc[index].next;
            while (next != null)
            {
                if (next.threadid == id)
                {
                    return next;
                }
                if ((count == null) && IsRWEntryEmpty(next))
                {
                    count = next;
                }
                next = next.next;
            }
            if (DontAllocate)
            {
                return null;
            }
            if (count == null)
            {
                next = new ReaderWriterCount(this.fIsReentrant) {
                    threadid = id,
                    next = this.rwc[index].next
                };
                this.rwc[index].next = next;
                return next;
            }
            count.threadid = id;
            return count;
        }

        private void InitializeThreadCounts()
        {
            this.rwc = new ReaderWriterCount[0x100];
            this.upgradeLockOwnerId = -1;
            this.writeLockOwnerId = -1;
        }

        private static bool IsRWEntryEmpty(ReaderWriterCount rwc)
        {
            return ((rwc.threadid == -1) || (((rwc.readercount == 0) && (rwc.rc == null)) || (((rwc.readercount == 0) && (rwc.rc.writercount == 0)) && (rwc.rc.upgradecount == 0))));
        }

        private static bool IsRwHashEntryChanged(ReaderWriterCount lrwc, int id)
        {
            return (lrwc.threadid != id);
        }

        private bool IsWriterAcquired()
        {
            return ((this.owners & 0xbfffffff) == 0);
        }

        private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
        {
            EventWaitHandle handle;
            this.ExitMyLock();
            if (makeAutoResetEvent)
            {
                handle = new AutoResetEvent(false);
            }
            else
            {
                handle = new ManualResetEvent(false);
            }
            this.EnterMyLock();
            if (waitEvent == null)
            {
                waitEvent = handle;
            }
            else
            {
                handle.Close();
            }
        }

        private void SetUpgraderWaiting()
        {
            this.owners |= 0x20000000;
        }

        private void SetWriterAcquired()
        {
            this.owners |= 0x80000000;
        }

        private void SetWritersWaiting()
        {
            this.owners |= 0x40000000;
        }

        private static void SpinWait(int SpinCount)
        {
            if ((SpinCount < 5) && (Environment.ProcessorCount > 1))
            {
                Thread.SpinWait(20 * SpinCount);
            }
            else if (SpinCount < 0x11)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        public bool TryEnterReadLock(int millisecondsTimeout)
        {
            Thread.BeginCriticalRegion();
            bool flag = false;
            try
            {
                flag = this.TryEnterReadLockCore(millisecondsTimeout);
            }
            finally
            {
                if (!flag)
                {
                    Thread.EndCriticalRegion();
                }
            }
            return flag;
        }

        public bool TryEnterReadLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int) timeout.TotalMilliseconds;
            return this.TryEnterReadLock(millisecondsTimeout);
        }

        private bool TryEnterReadLockCore(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            ReaderWriterCount lrwc = null;
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_ReadAfterWriteNotAllowed"));
                }
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(managedThreadId, false);
                if (lrwc.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_RecursiveReadNotAllowed"));
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            else
            {
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(managedThreadId, false);
                if (lrwc.readercount > 0)
                {
                    lrwc.readercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    this.fUpgradeThreadHoldingRead = true;
                    return true;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            bool flag = true;
            int spinCount = 0;
        Label_013D:
            if (this.owners < 0xffffffe)
            {
                this.owners++;
                lrwc.readercount++;
            }
            else
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
                else if (this.readEvent == null)
                {
                    this.LazyCreateEvent(ref this.readEvent, false);
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
                else
                {
                    flag = this.WaitOnEvent(this.readEvent, ref this.numReadWaiters, millisecondsTimeout);
                    if (!flag)
                    {
                        return false;
                    }
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
                goto Label_013D;
            }
            this.ExitMyLock();
            return flag;
        }

        public bool TryEnterUpgradeableReadLock(int millisecondsTimeout)
        {
            Thread.BeginCriticalRegion();
            bool flag = false;
            try
            {
                flag = this.TryEnterUpgradeableReadLockCore(millisecondsTimeout);
            }
            finally
            {
                if (!flag)
                {
                    Thread.EndCriticalRegion();
                }
            }
            return flag;
        }

        public bool TryEnterUpgradeableReadLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int) timeout.TotalMilliseconds;
            return this.TryEnterUpgradeableReadLock(millisecondsTimeout);
        }

        private bool TryEnterUpgradeableReadLockCore(int millisecondsTimeout)
        {
            ReaderWriterCount threadRWCount;
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_RecursiveUpgradeNotAllowed"));
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_UpgradeAfterWriteNotAllowed"));
                }
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if ((threadRWCount != null) && (threadRWCount.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_UpgradeAfterReadNotAllowed"));
                }
            }
            else
            {
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    threadRWCount.rc.upgradecount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    this.owners++;
                    this.upgradeLockOwnerId = managedThreadId;
                    threadRWCount.rc.upgradecount++;
                    if (threadRWCount.readercount > 0)
                    {
                        this.fUpgradeThreadHoldingRead = true;
                    }
                    this.ExitMyLock();
                    return true;
                }
                if (threadRWCount.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_UpgradeAfterReadNotAllowed"));
                }
            }
            int spinCount = 0;
        Label_0139:
            if ((this.upgradeLockOwnerId == -1) && (this.owners < 0xffffffe))
            {
                this.owners++;
                this.upgradeLockOwnerId = managedThreadId;
            }
            else
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    goto Label_0139;
                }
                if (this.upgradeEvent == null)
                {
                    this.LazyCreateEvent(ref this.upgradeEvent, true);
                    goto Label_0139;
                }
                if (this.WaitOnEvent(this.upgradeEvent, ref this.numUpgradeWaiters, millisecondsTimeout))
                {
                    goto Label_0139;
                }
                return false;
            }
            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                {
                    threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                }
                threadRWCount.rc.upgradecount++;
            }
            this.ExitMyLock();
            return true;
        }

        public bool TryEnterWriteLock(int millisecondsTimeout)
        {
            Thread.BeginCriticalRegion();
            bool flag = false;
            try
            {
                flag = this.TryEnterWriteLockCore(millisecondsTimeout);
            }
            finally
            {
                if (!flag)
                {
                    Thread.EndCriticalRegion();
                }
            }
            return flag;
        }

        public bool TryEnterWriteLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int) timeout.TotalMilliseconds;
            return this.TryEnterWriteLock(millisecondsTimeout);
        }

        private bool TryEnterWriteLockCore(int millisecondsTimeout)
        {
            ReaderWriterCount threadRWCount;
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            bool flag = false;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_RecursiveWriteNotAllowed"));
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if ((threadRWCount != null) && (threadRWCount.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_WriteAfterReadNotAllowed"));
                }
            }
            else
            {
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (managedThreadId == this.writeLockOwnerId)
                {
                    threadRWCount.rc.writercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                else if (threadRWCount.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new LockRecursionException(System.SR.GetString("LockRecursionException_WriteAfterReadNotAllowed"));
                }
            }
            int spinCount = 0;
        Label_00EC:
            if (this.IsWriterAcquired())
            {
                this.SetWriterAcquired();
            }
            else
            {
                if (flag)
                {
                    uint numReaders = this.GetNumReaders();
                    if (numReaders == 1)
                    {
                        this.SetWriterAcquired();
                        goto Label_01DD;
                    }
                    if ((numReaders == 2) && (threadRWCount != null))
                    {
                        if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                        {
                            threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                        }
                        if (threadRWCount.readercount > 0)
                        {
                            this.SetWriterAcquired();
                            goto Label_01DD;
                        }
                    }
                }
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    goto Label_00EC;
                }
                if (flag)
                {
                    if (this.waitUpgradeEvent != null)
                    {
                        if (!this.WaitOnEvent(this.waitUpgradeEvent, ref this.numWriteUpgradeWaiters, millisecondsTimeout))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        this.LazyCreateEvent(ref this.waitUpgradeEvent, true);
                    }
                    goto Label_00EC;
                }
                if (this.writeEvent == null)
                {
                    this.LazyCreateEvent(ref this.writeEvent, true);
                    goto Label_00EC;
                }
                if (this.WaitOnEvent(this.writeEvent, ref this.numWriteWaiters, millisecondsTimeout))
                {
                    goto Label_00EC;
                }
                return false;
            }
        Label_01DD:
            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                {
                    threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                }
                threadRWCount.rc.writercount++;
            }
            this.ExitMyLock();
            this.writeLockOwnerId = managedThreadId;
            return true;
        }

        private bool WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
        {
            waitEvent.Reset();
            numWaiters++;
            this.fNoWaiters = false;
            if (this.numWriteWaiters == 1)
            {
                this.SetWritersWaiting();
            }
            if (this.numWriteUpgradeWaiters == 1)
            {
                this.SetUpgraderWaiting();
            }
            bool flag = false;
            this.ExitMyLock();
            try
            {
                flag = waitEvent.WaitOne(millisecondsTimeout, false);
            }
            finally
            {
                this.EnterMyLock();
                numWaiters--;
                if (((this.numWriteWaiters == 0) && (this.numWriteUpgradeWaiters == 0)) && ((this.numUpgradeWaiters == 0) && (this.numReadWaiters == 0)))
                {
                    this.fNoWaiters = true;
                }
                if (this.numWriteWaiters == 0)
                {
                    this.ClearWritersWaiting();
                }
                if (this.numWriteUpgradeWaiters == 0)
                {
                    this.ClearUpgraderWaiting();
                }
                if (!flag)
                {
                    this.ExitMyLock();
                }
            }
            return flag;
        }

        public int CurrentReadCount
        {
            get
            {
                int numReaders = (int) this.GetNumReaders();
                if (this.upgradeLockOwnerId != -1)
                {
                    return (numReaders - 1);
                }
                return numReaders;
            }
        }

        public bool IsReadLockHeld
        {
            get
            {
                return (this.RecursiveReadCount > 0);
            }
        }

        public bool IsUpgradeableReadLockHeld
        {
            get
            {
                return (this.RecursiveUpgradeCount > 0);
            }
        }

        public bool IsWriteLockHeld
        {
            get
            {
                return (this.RecursiveWriteCount > 0);
            }
        }

        public LockRecursionPolicy RecursionPolicy
        {
            get
            {
                if (this.fIsReentrant)
                {
                    return LockRecursionPolicy.SupportsRecursion;
                }
                return LockRecursionPolicy.NoRecursion;
            }
        }

        public int RecursiveReadCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                int readercount = 0;
                Thread.BeginCriticalRegion();
                this.EnterMyLock();
                ReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if (threadRWCount != null)
                {
                    readercount = threadRWCount.readercount;
                }
                this.ExitMyLock();
                Thread.EndCriticalRegion();
                return readercount;
            }
        }

        public int RecursiveUpgradeCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                if (this.fIsReentrant)
                {
                    int upgradecount = 0;
                    Thread.BeginCriticalRegion();
                    this.EnterMyLock();
                    ReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                    if (threadRWCount != null)
                    {
                        upgradecount = threadRWCount.rc.upgradecount;
                    }
                    this.ExitMyLock();
                    Thread.EndCriticalRegion();
                    return upgradecount;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int RecursiveWriteCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                int writercount = 0;
                if (this.fIsReentrant)
                {
                    Thread.BeginCriticalRegion();
                    this.EnterMyLock();
                    ReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                    if (threadRWCount != null)
                    {
                        writercount = threadRWCount.rc.writercount;
                    }
                    this.ExitMyLock();
                    Thread.EndCriticalRegion();
                    return writercount;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int WaitingReadCount
        {
            get
            {
                return (int) this.numReadWaiters;
            }
        }

        public int WaitingUpgradeCount
        {
            get
            {
                return (int) this.numUpgradeWaiters;
            }
        }

        public int WaitingWriteCount
        {
            get
            {
                return (int) this.numWriteWaiters;
            }
        }
    }
}

