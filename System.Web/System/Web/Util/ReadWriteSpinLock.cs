namespace System.Web.Util
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ReadWriteSpinLock
    {
        private const int BACK_OFF_FACTORS_LENGTH = 13;
        private const int WRITER_WAITING_MASK = 0x40000000;
        private const int WRITE_COUNT_MASK = 0x3fff0000;
        private const int READ_COUNT_MASK = 0xffff;
        private const int WRITER_WAITING_SHIFT = 30;
        private const int WRITE_COUNT_SHIFT = 0x10;
        private int _bits;
        private int _id;
        private static bool s_disableBusyWaiting;
        private static readonly double[] s_backOffFactors;
        private static bool WriterWaiting(int bits)
        {
            return ((bits & 0x40000000) != 0);
        }

        private static int WriteLockCount(int bits)
        {
            return ((bits & 0x3fff0000) >> 0x10);
        }

        private static int ReadLockCount(int bits)
        {
            return (bits & 0xffff);
        }

        private static bool NoWriters(int bits)
        {
            return ((bits & 0x3fff0000) == 0);
        }

        private static bool NoWritersOrWaitingWriters(int bits)
        {
            return ((bits & 0x7fff0000) == 0);
        }

        private static bool NoLocks(int bits)
        {
            return ((bits & -1073741825) == 0);
        }

        private bool WriterWaiting()
        {
            return WriterWaiting(this._bits);
        }

        private int WriteLockCount()
        {
            return WriteLockCount(this._bits);
        }

        private int ReadLockCount()
        {
            return ReadLockCount(this._bits);
        }

        private bool NoWriters()
        {
            return NoWriters(this._bits);
        }

        private bool NoWritersOrWaitingWriters()
        {
            return NoWritersOrWaitingWriters(this._bits);
        }

        private bool NoLocks()
        {
            return NoLocks(this._bits);
        }

        private int CreateNewBits(bool writerWaiting, int writeCount, int readCount)
        {
            int num = (writeCount << 0x10) | readCount;
            if (writerWaiting)
            {
                num |= 0x40000000;
            }
            return num;
        }

        internal void AcquireReaderLock()
        {
            int hashCode = Thread.CurrentThread.GetHashCode();
            if (!this._TryAcquireReaderLock(hashCode))
            {
                this._Spin(true, hashCode);
            }
        }

        internal void AcquireWriterLock()
        {
            int hashCode = Thread.CurrentThread.GetHashCode();
            if (!this._TryAcquireWriterLock(hashCode))
            {
                this._Spin(false, hashCode);
            }
        }

        internal void ReleaseReaderLock()
        {
            Interlocked.Decrement(ref this._bits);
        }

        private void AlterWriteCountHoldingWriterLock(int oldBits, int delta)
        {
            int readCount = ReadLockCount(oldBits);
            int writeCount = WriteLockCount(oldBits) + delta;
            while (true)
            {
                int num4 = this.CreateNewBits(WriterWaiting(oldBits), writeCount, readCount);
                int num5 = Interlocked.CompareExchange(ref this._bits, num4, oldBits);
                if (num5 == oldBits)
                {
                    return;
                }
                oldBits = num5;
            }
        }

        internal void ReleaseWriterLock()
        {
            int bits = this._bits;
            if (WriteLockCount(bits) == 1)
            {
                this._id = 0;
            }
            this.AlterWriteCountHoldingWriterLock(bits, -1);
        }

        private bool _TryAcquireWriterLock(int threadId)
        {
            int num3;
            int num4;
            int num = this._id;
            int oldBits = this._bits;
            if (num == threadId)
            {
                this.AlterWriteCountHoldingWriterLock(oldBits, 1);
                return true;
            }
            if ((num == 0) && NoLocks(oldBits))
            {
                num3 = this.CreateNewBits(false, 1, 0);
                num4 = Interlocked.CompareExchange(ref this._bits, num3, oldBits);
                if (num4 == oldBits)
                {
                    num = this._id;
                    this._id = threadId;
                    return true;
                }
                oldBits = num4;
            }
            if (!WriterWaiting(oldBits))
            {
                while (true)
                {
                    num3 = oldBits | 0x40000000;
                    num4 = Interlocked.CompareExchange(ref this._bits, num3, oldBits);
                    if (num4 == oldBits)
                    {
                        break;
                    }
                    oldBits = num4;
                }
            }
            return false;
        }

        private bool _TryAcquireReaderLock(int threadId)
        {
            int bits = this._bits;
            int num2 = this._id;
            if (num2 == 0)
            {
                if (!NoWriters(bits))
                {
                    return false;
                }
            }
            else if (num2 != threadId)
            {
                return false;
            }
            return (Interlocked.CompareExchange(ref this._bits, bits + 1, bits) == bits);
        }

        private void _Spin(bool isReaderLock, int threadId)
        {
            int millisecondsTimeout = 0;
            double num3 = s_backOffFactors[Math.Abs(threadId) % 13];
            int num2 = (int) (4000.0 * num3);
            num2 = Math.Max(Math.Min(0x2710, num2), 100);
            DateTime utcNow = DateTime.UtcNow;
            bool flag = s_disableBusyWaiting;
        Label_0040:
            if (isReaderLock)
            {
                if (this._TryAcquireReaderLock(threadId))
                {
                    return;
                }
            }
            else if (this._TryAcquireWriterLock(threadId))
            {
                return;
            }
            if (flag)
            {
                Thread.Sleep(millisecondsTimeout);
                millisecondsTimeout ^= 1;
                goto Label_0040;
            }
            int num4 = num2;
        Label_0069:
            if (isReaderLock)
            {
                if (!this.NoWritersOrWaitingWriters())
                {
                    goto Label_007E;
                }
                goto Label_0040;
            }
            if (this.NoLocks())
            {
                goto Label_0040;
            }
        Label_007E:
            if (--num4 < 0)
            {
                Thread.Sleep(millisecondsTimeout);
                num2 /= 2;
                num2 = Math.Max(num2, 100);
                num4 = num2;
                millisecondsTimeout ^= 1;
            }
            else
            {
                Thread.SpinWait(10);
            }
            goto Label_0069;
        }

        static ReadWriteSpinLock()
        {
            s_disableBusyWaiting = SystemInfo.GetNumProcessCPUs() == 1;
            s_backOffFactors = new double[] { 1.02, 0.965, 0.89, 1.065, 1.025, 1.115, 0.94, 0.995, 1.05, 1.08, 0.915, 0.98, 1.01 };
        }
    }
}

