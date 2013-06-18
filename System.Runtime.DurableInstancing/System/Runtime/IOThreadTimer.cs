namespace System.Runtime
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime.Interop;
    using System.Security;
    using System.Threading;

    internal class IOThreadTimer
    {
        private Action<object> callback;
        private object callbackState;
        private long dueTime;
        private int index;
        private long maxSkew;
        private const int maxSkewInMillisecondsDefault = 100;
        private static long systemTimeResolutionTicks = -1L;
        private TimerGroup timerGroup;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IOThreadTimer(Action<object> callback, object callbackState, bool isTypicallyCanceledShortlyAfterBeingSet) : this(callback, callbackState, isTypicallyCanceledShortlyAfterBeingSet, 100)
        {
        }

        public IOThreadTimer(Action<object> callback, object callbackState, bool isTypicallyCanceledShortlyAfterBeingSet, int maxSkewInMilliseconds)
        {
            this.callback = callback;
            this.callbackState = callbackState;
            this.maxSkew = Ticks.FromMilliseconds(maxSkewInMilliseconds);
            this.timerGroup = isTypicallyCanceledShortlyAfterBeingSet ? TimerManager.Value.VolatileTimerGroup : TimerManager.Value.StableTimerGroup;
        }

        public bool Cancel()
        {
            return TimerManager.Value.Cancel(this);
        }

        [SecuritySafeCritical]
        private static long GetSystemTimeResolution()
        {
            int num;
            uint num2;
            uint num3;
            if (UnsafeNativeMethods.GetSystemTimeAdjustment(out num, out num2, out num3) != 0)
            {
                return (long) num2;
            }
            return 0x249f0L;
        }

        public void Set(int millisecondsFromNow)
        {
            this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromMilliseconds(millisecondsFromNow)));
        }

        public void Set(TimeSpan timeFromNow)
        {
            if (timeFromNow != TimeSpan.MaxValue)
            {
                this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromTimeSpan(timeFromNow)));
            }
        }

        public void SetAt(long dueTime)
        {
            TimerManager.Value.Set(this, dueTime);
        }

        public static long SystemTimeResolutionTicks
        {
            get
            {
                if (systemTimeResolutionTicks == -1L)
                {
                    systemTimeResolutionTicks = GetSystemTimeResolution();
                }
                return systemTimeResolutionTicks;
            }
        }

        private class TimerGroup
        {
            private System.Runtime.IOThreadTimer.TimerQueue timerQueue;
            private System.Runtime.IOThreadTimer.WaitableTimer waitableTimer = new System.Runtime.IOThreadTimer.WaitableTimer();

            public TimerGroup()
            {
                this.waitableTimer.Set(0x7fffffffffffffffL);
                this.timerQueue = new System.Runtime.IOThreadTimer.TimerQueue();
            }

            public System.Runtime.IOThreadTimer.TimerQueue TimerQueue
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.timerQueue;
                }
            }

            public System.Runtime.IOThreadTimer.WaitableTimer WaitableTimer
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.waitableTimer;
                }
            }
        }

        private class TimerManager
        {
            private const long maxTimeToWaitForMoreTimers = 0x989680L;
            private Action<object> onWaitCallback;
            private IOThreadTimer.TimerGroup stableTimerGroup;
            private static IOThreadTimer.TimerManager value = new IOThreadTimer.TimerManager();
            private IOThreadTimer.TimerGroup volatileTimerGroup;
            private IOThreadTimer.WaitableTimer[] waitableTimers;
            private bool waitScheduled;

            public TimerManager()
            {
                this.onWaitCallback = new Action<object>(this.OnWaitCallback);
                this.stableTimerGroup = new IOThreadTimer.TimerGroup();
                this.volatileTimerGroup = new IOThreadTimer.TimerGroup();
                this.waitableTimers = new IOThreadTimer.WaitableTimer[] { this.stableTimerGroup.WaitableTimer, this.volatileTimerGroup.WaitableTimer };
            }

            public bool Cancel(IOThreadTimer timer)
            {
                lock (this.ThisLock)
                {
                    if (timer.index > 0)
                    {
                        IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
                        IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                        timerQueue.DeleteTimer(timer);
                        if (timerQueue.Count > 0)
                        {
                            this.UpdateWaitableTimer(timerGroup);
                        }
                        else
                        {
                            IOThreadTimer.TimerGroup otherTimerGroup = this.GetOtherTimerGroup(timerGroup);
                            if (otherTimerGroup.TimerQueue.Count == 0)
                            {
                                long now = Ticks.Now;
                                long num2 = timerGroup.WaitableTimer.DueTime - now;
                                long num3 = otherTimerGroup.WaitableTimer.DueTime - now;
                                if ((num2 > 0x989680L) && (num3 > 0x989680L))
                                {
                                    timerGroup.WaitableTimer.Set(Ticks.Add(now, 0x989680L));
                                }
                            }
                        }
                        return true;
                    }
                    return false;
                }
            }

            private void EnsureWaitScheduled()
            {
                if (!this.waitScheduled)
                {
                    this.ScheduleWait();
                }
            }

            private IOThreadTimer.TimerGroup GetOtherTimerGroup(IOThreadTimer.TimerGroup timerGroup)
            {
                if (object.ReferenceEquals(timerGroup, this.volatileTimerGroup))
                {
                    return this.stableTimerGroup;
                }
                return this.volatileTimerGroup;
            }

            private void OnWaitCallback(object state)
            {
                WaitHandle.WaitAny(this.waitableTimers);
                long now = Ticks.Now;
                lock (this.ThisLock)
                {
                    this.waitScheduled = false;
                    this.ScheduleElapsedTimers(now);
                    this.ReactivateWaitableTimers();
                    this.ScheduleWaitIfAnyTimersLeft();
                }
            }

            private void ReactivateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
            {
                IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                if (timerQueue.Count > 0)
                {
                    timerGroup.WaitableTimer.Set(timerQueue.MinTimer.dueTime);
                }
                else
                {
                    timerGroup.WaitableTimer.Set(0x7fffffffffffffffL);
                }
            }

            private void ReactivateWaitableTimers()
            {
                this.ReactivateWaitableTimer(this.stableTimerGroup);
                this.ReactivateWaitableTimer(this.volatileTimerGroup);
            }

            private void ScheduleElapsedTimers(long now)
            {
                this.ScheduleElapsedTimers(this.stableTimerGroup, now);
                this.ScheduleElapsedTimers(this.volatileTimerGroup, now);
            }

            private void ScheduleElapsedTimers(IOThreadTimer.TimerGroup timerGroup, long now)
            {
                IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                while (timerQueue.Count > 0)
                {
                    IOThreadTimer minTimer = timerQueue.MinTimer;
                    long num = minTimer.dueTime - now;
                    if (num > minTimer.maxSkew)
                    {
                        break;
                    }
                    timerQueue.DeleteMinTimer();
                    ActionItem.Schedule(minTimer.callback, minTimer.callbackState);
                }
            }

            private void ScheduleWait()
            {
                ActionItem.Schedule(this.onWaitCallback, null);
                this.waitScheduled = true;
            }

            private void ScheduleWaitIfAnyTimersLeft()
            {
                if ((this.stableTimerGroup.TimerQueue.Count > 0) || (this.volatileTimerGroup.TimerQueue.Count > 0))
                {
                    this.ScheduleWait();
                }
            }

            public void Set(IOThreadTimer timer, long dueTime)
            {
                long num = dueTime - timer.dueTime;
                if (num < 0L)
                {
                    num = -num;
                }
                if (num > timer.maxSkew)
                {
                    lock (this.ThisLock)
                    {
                        IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
                        IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                        if (timer.index > 0)
                        {
                            if (timerQueue.UpdateTimer(timer, dueTime))
                            {
                                this.UpdateWaitableTimer(timerGroup);
                            }
                        }
                        else if (timerQueue.InsertTimer(timer, dueTime))
                        {
                            this.UpdateWaitableTimer(timerGroup);
                            if (timerQueue.Count == 1)
                            {
                                this.EnsureWaitScheduled();
                            }
                        }
                    }
                }
            }

            private void UpdateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
            {
                IOThreadTimer.WaitableTimer waitableTimer = timerGroup.WaitableTimer;
                IOThreadTimer minTimer = timerGroup.TimerQueue.MinTimer;
                long num = waitableTimer.DueTime - minTimer.dueTime;
                if (num < 0L)
                {
                    num = -num;
                }
                if (num > minTimer.maxSkew)
                {
                    waitableTimer.Set(minTimer.dueTime);
                }
            }

            public IOThreadTimer.TimerGroup StableTimerGroup
            {
                get
                {
                    return this.stableTimerGroup;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this;
                }
            }

            public static IOThreadTimer.TimerManager Value
            {
                get
                {
                    return value;
                }
            }

            public IOThreadTimer.TimerGroup VolatileTimerGroup
            {
                get
                {
                    return this.volatileTimerGroup;
                }
            }
        }

        private class TimerQueue
        {
            private int count;
            private IOThreadTimer[] timers = new IOThreadTimer[4];

            public void DeleteMinTimer()
            {
                IOThreadTimer minTimer = this.MinTimer;
                this.DeleteMinTimerCore();
                minTimer.index = 0;
                minTimer.dueTime = 0L;
            }

            private void DeleteMinTimerCore()
            {
                int num3;
                int count = this.count;
                if (count == 1)
                {
                    this.count = 0;
                    this.timers[1] = null;
                    return;
                }
                IOThreadTimer[] timers = this.timers;
                IOThreadTimer timer = timers[count];
                this.count = --count;
                int index = 1;
            Label_0034:
                num3 = index * 2;
                if (num3 <= count)
                {
                    int num4;
                    IOThreadTimer timer2;
                    if (num3 < count)
                    {
                        IOThreadTimer timer3 = timers[num3];
                        int num5 = num3 + 1;
                        IOThreadTimer timer4 = timers[num5];
                        if (timer4.dueTime < timer3.dueTime)
                        {
                            timer2 = timer4;
                            num4 = num5;
                        }
                        else
                        {
                            timer2 = timer3;
                            num4 = num3;
                        }
                    }
                    else
                    {
                        num4 = num3;
                        timer2 = timers[num4];
                    }
                    if (timer.dueTime > timer2.dueTime)
                    {
                        timers[index] = timer2;
                        timer2.index = index;
                        index = num4;
                        if (num3 < count)
                        {
                            goto Label_0034;
                        }
                    }
                }
                timers[index] = timer;
                timer.index = index;
                timers[count + 1] = null;
            }

            public void DeleteTimer(IOThreadTimer timer)
            {
                int index = timer.index;
                IOThreadTimer[] timers = this.timers;
                while (true)
                {
                    int num2 = index / 2;
                    if (num2 < 1)
                    {
                        break;
                    }
                    IOThreadTimer timer2 = timers[num2];
                    timers[index] = timer2;
                    timer2.index = index;
                    index = num2;
                }
                timer.index = 0;
                timer.dueTime = 0L;
                timers[1] = null;
                this.DeleteMinTimerCore();
            }

            public bool InsertTimer(IOThreadTimer timer, long dueTime)
            {
                IOThreadTimer[] timers = this.timers;
                int index = this.count + 1;
                if (index == timers.Length)
                {
                    timers = new IOThreadTimer[timers.Length * 2];
                    Array.Copy(this.timers, timers, this.timers.Length);
                    this.timers = timers;
                }
                this.count = index;
                if (index > 1)
                {
                    while (true)
                    {
                        int num2 = index / 2;
                        if (num2 == 0)
                        {
                            break;
                        }
                        IOThreadTimer timer2 = timers[num2];
                        if (timer2.dueTime <= dueTime)
                        {
                            break;
                        }
                        timers[index] = timer2;
                        timer2.index = index;
                        index = num2;
                    }
                }
                timers[index] = timer;
                timer.index = index;
                timer.dueTime = dueTime;
                return (index == 1);
            }

            public bool UpdateTimer(IOThreadTimer timer, long dueTime)
            {
                int index = timer.index;
                IOThreadTimer[] timers = this.timers;
                int count = this.count;
                int num3 = index / 2;
                if ((num3 == 0) || (timers[num3].dueTime <= dueTime))
                {
                    int num4 = index * 2;
                    if ((num4 > count) || (timers[num4].dueTime >= dueTime))
                    {
                        int num5 = num4 + 1;
                        if ((num5 > count) || (timers[num5].dueTime >= dueTime))
                        {
                            timer.dueTime = dueTime;
                            return (index == 1);
                        }
                    }
                }
                this.DeleteTimer(timer);
                this.InsertTimer(timer, dueTime);
                return true;
            }

            public int Count
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.count;
                }
            }

            public IOThreadTimer MinTimer
            {
                get
                {
                    return this.timers[1];
                }
            }
        }

        private class WaitableTimer : WaitHandle
        {
            private long dueTime;

            [SecuritySafeCritical]
            public WaitableTimer()
            {
                base.SafeWaitHandle = TimerHelper.CreateWaitableTimer();
            }

            [SecuritySafeCritical]
            public void Set(long dueTime)
            {
                this.dueTime = TimerHelper.Set(base.SafeWaitHandle, dueTime);
            }

            public long DueTime
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.dueTime;
                }
            }

            [SecurityCritical]
            private static class TimerHelper
            {
                public static SafeWaitHandle CreateWaitableTimer()
                {
                    SafeWaitHandle handle = UnsafeNativeMethods.CreateWaitableTimer(IntPtr.Zero, false, null);
                    if (handle.IsInvalid)
                    {
                        Exception exception = new Win32Exception();
                        handle.SetHandleAsInvalid();
                        throw Fx.Exception.AsError(exception);
                    }
                    return handle;
                }

                public static long Set(SafeWaitHandle timer, long dueTime)
                {
                    if (!UnsafeNativeMethods.SetWaitableTimer(timer, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false))
                    {
                        throw Fx.Exception.AsError(new Win32Exception());
                    }
                    return dueTime;
                }
            }
        }
    }
}

