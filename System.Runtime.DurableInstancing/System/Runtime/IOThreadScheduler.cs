namespace System.Runtime
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class IOThreadScheduler
    {
        private static IOThreadScheduler current = new IOThreadScheduler(0x20, 0x20);
        private int headTail = -131072;
        private int headTailLowPri = -65536;
        private const int MaximumCapacity = 0x8000;
        private readonly ScheduledOverlapped overlapped;
        [SecurityCritical]
        private readonly Slot[] slots;
        [SecurityCritical]
        private readonly Slot[] slotsLowPri;

        [SecuritySafeCritical]
        private IOThreadScheduler(int capacity, int capacityLowPri)
        {
            this.slots = new Slot[capacity];
            this.slotsLowPri = new Slot[capacityLowPri];
            this.overlapped = new ScheduledOverlapped();
        }

        [SecuritySafeCritical]
        private void Cleanup()
        {
            if (this.overlapped != null)
            {
                this.overlapped.Cleanup();
            }
        }

        [SecurityCritical]
        private void CompletionCallback(out Action<object> callback, out object state)
        {
            bool flag;
            int headTail = this.headTail;
        Label_0007:
            flag = Bits.Count(headTail) == 0;
            if (flag)
            {
                int headTailLowPri = this.headTailLowPri;
                while (Bits.CountNoIdle(headTailLowPri) != 0)
                {
                    if (headTailLowPri == (headTailLowPri = Interlocked.CompareExchange(ref this.headTailLowPri, Bits.IncrementLo(headTailLowPri), headTailLowPri)))
                    {
                        this.overlapped.Post(this);
                        this.slotsLowPri[headTailLowPri & this.SlotMaskLowPri].DequeueWorkItem(out callback, out state);
                        return;
                    }
                }
            }
            if (headTail != (headTail = Interlocked.CompareExchange(ref this.headTail, Bits.IncrementLo(headTail), headTail)))
            {
                goto Label_0007;
            }
            if (!flag)
            {
                this.overlapped.Post(this);
                this.slots[headTail & this.SlotMask].DequeueWorkItem(out callback, out state);
            }
            else
            {
                if (Bits.CountNoIdle(this.headTailLowPri) != 0)
                {
                    headTail = Bits.IncrementLo(headTail);
                    if (headTail == Interlocked.CompareExchange(ref this.headTail, headTail + 0x10000, headTail))
                    {
                        headTail += 0x10000;
                        goto Label_0007;
                    }
                }
                callback = null;
                state = null;
            }
        }

        ~IOThreadScheduler()
        {
            if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                this.Cleanup();
            }
        }

        [SecurityCritical]
        private bool ScheduleCallbackHelper(Action<object> callback, object state)
        {
            bool flag2;
            int slot = Interlocked.Add(ref this.headTail, 0x10000);
            bool flag = Bits.Count(slot) == 0;
            if (flag)
            {
                slot = Interlocked.Add(ref this.headTail, 0x10000);
            }
            if (Bits.Count(slot) == -1)
            {
                throw Fx.AssertAndThrowFatal("Head/Tail overflow!");
            }
            bool flag3 = this.slots[(slot >> 0x10) & this.SlotMask].TryEnqueueWorkItem(callback, state, out flag2);
            if (flag2)
            {
                IOThreadScheduler scheduler = new IOThreadScheduler(Math.Min(this.slots.Length * 2, 0x8000), this.slotsLowPri.Length);
                Interlocked.CompareExchange<IOThreadScheduler>(ref current, scheduler, this);
            }
            if (flag)
            {
                this.overlapped.Post(this);
            }
            return flag3;
        }

        [SecurityCritical]
        private bool ScheduleCallbackLowPriHelper(Action<object> callback, object state)
        {
            bool flag2;
            int slot = Interlocked.Add(ref this.headTailLowPri, 0x10000);
            bool flag = false;
            if (Bits.CountNoIdle(slot) == 1)
            {
                int headTail = this.headTail;
                if (Bits.Count(headTail) == -1)
                {
                    int num3 = Interlocked.CompareExchange(ref this.headTail, headTail + 0x10000, headTail);
                    if (headTail == num3)
                    {
                        flag = true;
                    }
                }
            }
            if (Bits.CountNoIdle(slot) == 0)
            {
                throw Fx.AssertAndThrowFatal("Low-priority Head/Tail overflow!");
            }
            bool flag3 = this.slotsLowPri[(slot >> 0x10) & this.SlotMaskLowPri].TryEnqueueWorkItem(callback, state, out flag2);
            if (flag2)
            {
                IOThreadScheduler scheduler = new IOThreadScheduler(this.slots.Length, Math.Min(this.slotsLowPri.Length * 2, 0x8000));
                Interlocked.CompareExchange<IOThreadScheduler>(ref current, scheduler, this);
            }
            if (flag)
            {
                this.overlapped.Post(this);
            }
            return flag3;
        }

        [SecurityCritical]
        public static void ScheduleCallbackLowPriNoFlow(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw Fx.Exception.ArgumentNull("callback");
            }
            bool flag = false;
            while (!flag)
            {
                try
                {
                    continue;
                }
                finally
                {
                    flag = current.ScheduleCallbackLowPriHelper(callback, state);
                }
            }
        }

        [SecurityCritical]
        public static void ScheduleCallbackNoFlow(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw Fx.Exception.ArgumentNull("callback");
            }
            bool flag = false;
            while (!flag)
            {
                try
                {
                    continue;
                }
                finally
                {
                    flag = current.ScheduleCallbackHelper(callback, state);
                }
            }
        }

        [SecurityCritical]
        private bool TryCoalesce(out Action<object> callback, out object state)
        {
            int headTail = this.headTail;
        Label_0007:
            while (Bits.Count(headTail) > 0)
            {
                if (headTail == (headTail = Interlocked.CompareExchange(ref this.headTail, Bits.IncrementLo(headTail), headTail)))
                {
                    this.slots[headTail & this.SlotMask].DequeueWorkItem(out callback, out state);
                    return true;
                }
            }
            int headTailLowPri = this.headTailLowPri;
            if (Bits.CountNoIdle(headTailLowPri) > 0)
            {
                if (headTailLowPri == (headTailLowPri = Interlocked.CompareExchange(ref this.headTailLowPri, Bits.IncrementLo(headTailLowPri), headTailLowPri)))
                {
                    this.slotsLowPri[headTailLowPri & this.SlotMaskLowPri].DequeueWorkItem(out callback, out state);
                    return true;
                }
                headTail = this.headTail;
                goto Label_0007;
            }
            callback = null;
            state = null;
            return false;
        }

        private int SlotMask
        {
            [SecurityCritical]
            get
            {
                return (this.slots.Length - 1);
            }
        }

        private int SlotMaskLowPri
        {
            [SecurityCritical]
            get
            {
                return (this.slotsLowPri.Length - 1);
            }
        }

        private static class Bits
        {
            public const int HiBits = -2147450880;
            public const int HiCountMask = 0x7fff0000;
            public const int HiHiBit = -2147483648;
            public const int HiMask = -65536;
            public const int HiOne = 0x10000;
            public const int HiShift = 0x10;
            public const int LoCountMask = 0x7fff;
            public const int LoHiBit = 0x8000;
            public const int LoMask = 0xffff;

            public static int Count(int slot)
            {
                return (((((slot >> 0x10) - slot) + 2) & 0xffff) - 1);
            }

            public static int CountNoIdle(int slot)
            {
                return ((((slot >> 0x10) - slot) + 1) & 0xffff);
            }

            public static int IncrementLo(int slot)
            {
                return (((slot + 1) & 0xffff) | (slot & -65536));
            }

            public static bool IsComplete(int gate)
            {
                return ((gate & -65536) == (gate << 0x10));
            }
        }

        [SecurityCritical]
        private class ScheduledOverlapped
        {
            private unsafe readonly NativeOverlapped* nativeOverlapped;
            private IOThreadScheduler scheduler;

            public unsafe ScheduledOverlapped()
            {
                this.nativeOverlapped = new Overlapped().UnsafePack(Fx.ThunkCallback(new IOCompletionCallback(this.IOCallback)), null);
            }

            public unsafe void Cleanup()
            {
                if (this.scheduler != null)
                {
                    throw Fx.AssertAndThrowFatal("Cleanup called on an overlapped that is in-flight.");
                }
                Overlapped.Free(this.nativeOverlapped);
            }

            private unsafe void IOCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                Action<object> action;
                object obj2;
                IOThreadScheduler scheduler = this.scheduler;
                this.scheduler = null;
                try
                {
                }
                finally
                {
                    scheduler.CompletionCallback(out action, out obj2);
                }
                bool flag = true;
                while (flag)
                {
                    if (action != null)
                    {
                        action(obj2);
                    }
                    try
                    {
                        continue;
                    }
                    finally
                    {
                        flag = scheduler.TryCoalesce(out action, out obj2);
                    }
                }
            }

            public unsafe void Post(IOThreadScheduler iots)
            {
                this.scheduler = iots;
                ThreadPool.UnsafeQueueNativeOverlapped(this.nativeOverlapped);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Slot
        {
            private int gate;
            private Action<object> callback;
            private object state;
            public bool TryEnqueueWorkItem(Action<object> callback, object state, out bool wrapped)
            {
                int gate = Interlocked.Increment(ref this.gate);
                wrapped = (gate & 0x7fff) != 1;
                if (wrapped)
                {
                    if (((gate & 0x8000) != 0) && IOThreadScheduler.Bits.IsComplete(gate))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, gate);
                    }
                    return false;
                }
                this.state = state;
                this.callback = callback;
                gate = Interlocked.Add(ref this.gate, 0x8000);
                if ((gate & 0x7fff0000) == 0)
                {
                    return true;
                }
                this.state = null;
                this.callback = null;
                if (((gate >> 0x10) != (gate & 0x7fff)) || (Interlocked.CompareExchange(ref this.gate, 0, gate) != gate))
                {
                    gate = Interlocked.Add(ref this.gate, -2147483648);
                    if (IOThreadScheduler.Bits.IsComplete(gate))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, gate);
                    }
                }
                return false;
            }

            public void DequeueWorkItem(out Action<object> callback, out object state)
            {
                int comparand = Interlocked.Add(ref this.gate, 0x10000);
                if ((comparand & 0x8000) == 0)
                {
                    callback = null;
                    state = null;
                }
                else if ((comparand & 0x7fff0000) == 0x10000)
                {
                    callback = this.callback;
                    state = this.state;
                    this.state = null;
                    this.callback = null;
                    if (((comparand & 0x7fff) != 1) || (Interlocked.CompareExchange(ref this.gate, 0, comparand) != comparand))
                    {
                        comparand = Interlocked.Add(ref this.gate, -2147483648);
                        if (IOThreadScheduler.Bits.IsComplete(comparand))
                        {
                            Interlocked.CompareExchange(ref this.gate, 0, comparand);
                        }
                    }
                }
                else
                {
                    callback = null;
                    state = null;
                    if (IOThreadScheduler.Bits.IsComplete(comparand))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, comparand);
                    }
                }
            }
        }
    }
}

