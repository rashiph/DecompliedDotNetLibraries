namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class TimerThread
    {
        private const int c_CacheScanPerIterations = 0x20;
        private const int c_ThreadIdleTimeoutMilliseconds = 0x7530;
        private const int c_TickCountResolution = 15;
        private static int s_CacheScanIteration;
        private static LinkedList<WeakReference> s_NewQueues = new LinkedList<WeakReference>();
        private static LinkedList<WeakReference> s_Queues = new LinkedList<WeakReference>();
        private static Hashtable s_QueuesCache = new Hashtable();
        private static WaitHandle[] s_ThreadEvents = new WaitHandle[] { s_ThreadShutdownEvent, s_ThreadReadyEvent };
        private static AutoResetEvent s_ThreadReadyEvent = new AutoResetEvent(false);
        private static ManualResetEvent s_ThreadShutdownEvent = new ManualResetEvent(false);
        private static int s_ThreadState = 0;

        static TimerThread()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(TimerThread.OnDomainUnload);
        }

        internal static Queue CreateQueue(int durationMilliseconds)
        {
            TimerQueue queue;
            if (durationMilliseconds == -1)
            {
                return new InfiniteTimerQueue();
            }
            if (durationMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("durationMilliseconds");
            }
            lock (s_NewQueues)
            {
                queue = new TimerQueue(durationMilliseconds);
                WeakReference reference = new WeakReference(queue);
                s_NewQueues.AddLast(reference);
            }
            return queue;
        }

        internal static Queue GetOrCreateQueue(int durationMilliseconds)
        {
            TimerQueue queue;
            if (durationMilliseconds == -1)
            {
                return new InfiniteTimerQueue();
            }
            if (durationMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException("durationMilliseconds");
            }
            WeakReference reference = (WeakReference) s_QueuesCache[durationMilliseconds];
            if ((reference == null) || ((queue = (TimerQueue) reference.Target) == null))
            {
                lock (s_NewQueues)
                {
                    reference = (WeakReference) s_QueuesCache[durationMilliseconds];
                    if ((reference != null) && ((queue = (TimerQueue) reference.Target) != null))
                    {
                        return queue;
                    }
                    queue = new TimerQueue(durationMilliseconds);
                    reference = new WeakReference(queue);
                    s_NewQueues.AddLast(reference);
                    s_QueuesCache[durationMilliseconds] = reference;
                    if ((++s_CacheScanIteration % 0x20) != 0)
                    {
                        return queue;
                    }
                    List<int> list = new List<int>();
                    foreach (DictionaryEntry entry in s_QueuesCache)
                    {
                        if (((WeakReference) entry.Value).Target == null)
                        {
                            list.Add((int) entry.Key);
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        s_QueuesCache.Remove(list[i]);
                    }
                }
            }
            return queue;
        }

        private static bool IsTickBetween(int start, int end, int comparand)
        {
            return (((start <= comparand) == (end <= comparand)) != (start <= end));
        }

        private static void OnDomainUnload(object sender, EventArgs e)
        {
            try
            {
                StopTimerThread();
            }
            catch
            {
            }
        }

        private static void Prod()
        {
            s_ThreadReadyEvent.Set();
            if (Interlocked.CompareExchange(ref s_ThreadState, 1, 0) == 0)
            {
                new Thread(new ThreadStart(TimerThread.ThreadProc)).Start();
            }
        }

        private static void StopTimerThread()
        {
            Interlocked.Exchange(ref s_ThreadState, 2);
            s_ThreadShutdownEvent.Set();
        }

        private static void ThreadProc()
        {
            Thread.CurrentThread.IsBackground = true;
            lock (s_Queues)
            {
                if (Interlocked.CompareExchange(ref s_ThreadState, 1, 1) == 1)
                {
                    bool flag = true;
                    while (flag)
                    {
                        try
                        {
                            s_ThreadReadyEvent.Reset();
                        Label_0043:
                            if (s_NewQueues.Count > 0)
                            {
                                lock (s_NewQueues)
                                {
                                    for (LinkedListNode<WeakReference> node = s_NewQueues.First; node != null; node = s_NewQueues.First)
                                    {
                                        s_NewQueues.Remove(node);
                                        s_Queues.AddLast(node);
                                    }
                                }
                            }
                            int tickCount = Environment.TickCount;
                            int end = 0;
                            bool flag3 = false;
                            LinkedListNode<WeakReference> first = s_Queues.First;
                            while (first != null)
                            {
                                TimerQueue target = (TimerQueue) first.Value.Target;
                                if (target == null)
                                {
                                    LinkedListNode<WeakReference> next = first.Next;
                                    s_Queues.Remove(first);
                                    first = next;
                                }
                                else
                                {
                                    int num3;
                                    if (target.Fire(out num3) && (!flag3 || IsTickBetween(tickCount, end, num3)))
                                    {
                                        end = num3;
                                        flag3 = true;
                                    }
                                    first = first.Next;
                                }
                            }
                            int comparand = Environment.TickCount;
                            int millisecondsTimeout = flag3 ? (IsTickBetween(tickCount, end, comparand) ? (((int) Math.Min((uint) (end - comparand), 0x7ffffff0)) + 15) : 0) : 0x7530;
                            int num6 = WaitHandle.WaitAny(s_ThreadEvents, millisecondsTimeout, false);
                            if (num6 == 0)
                            {
                                flag = false;
                            }
                            else
                            {
                                if ((num6 != 0x102) || flag3)
                                {
                                    goto Label_0043;
                                }
                                Interlocked.CompareExchange(ref s_ThreadState, 0, 1);
                                if (s_ThreadReadyEvent.WaitOne(0, false) && (Interlocked.CompareExchange(ref s_ThreadState, 1, 0) == 0))
                                {
                                    goto Label_0043;
                                }
                                flag = false;
                            }
                            continue;
                        }
                        catch (Exception exception)
                        {
                            if (NclUtilities.IsFatal(exception))
                            {
                                throw;
                            }
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.Web, "TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString(NumberFormatInfo.InvariantInfo) + "::ThreadProc() - Exception:" + exception.ToString());
                            }
                            Thread.Sleep(0x3e8);
                            continue;
                        }
                    }
                }
            }
        }

        internal delegate void Callback(TimerThread.Timer timer, int timeNoticed, object context);

        private class InfiniteTimer : TimerThread.Timer
        {
            private int cancelled;

            internal InfiniteTimer() : base(-1)
            {
            }

            internal override bool Cancel()
            {
                return (Interlocked.Exchange(ref this.cancelled, 1) == 0);
            }

            internal override bool HasExpired
            {
                get
                {
                    return false;
                }
            }
        }

        private class InfiniteTimerQueue : TimerThread.Queue
        {
            internal InfiniteTimerQueue() : base(-1)
            {
            }

            internal override TimerThread.Timer CreateTimer(TimerThread.Callback callback, object context)
            {
                return new TimerThread.InfiniteTimer();
            }
        }

        internal abstract class Queue
        {
            private readonly int m_DurationMilliseconds;

            internal Queue(int durationMilliseconds)
            {
                this.m_DurationMilliseconds = durationMilliseconds;
            }

            internal TimerThread.Timer CreateTimer()
            {
                return this.CreateTimer(null, null);
            }

            internal abstract TimerThread.Timer CreateTimer(TimerThread.Callback callback, object context);

            internal int Duration
            {
                get
                {
                    return this.m_DurationMilliseconds;
                }
            }
        }

        internal abstract class Timer : IDisposable
        {
            private readonly int m_DurationMilliseconds;
            private readonly int m_StartTimeMilliseconds;

            internal Timer(int durationMilliseconds)
            {
                this.m_DurationMilliseconds = durationMilliseconds;
                this.m_StartTimeMilliseconds = Environment.TickCount;
            }

            internal abstract bool Cancel();
            public void Dispose()
            {
                this.Cancel();
            }

            internal int Duration
            {
                get
                {
                    return this.m_DurationMilliseconds;
                }
            }

            internal int Expiration
            {
                get
                {
                    return (this.m_StartTimeMilliseconds + this.m_DurationMilliseconds);
                }
            }

            internal abstract bool HasExpired { get; }

            internal int StartTime
            {
                get
                {
                    return this.m_StartTimeMilliseconds;
                }
            }

            internal int TimeRemaining
            {
                get
                {
                    if (this.HasExpired)
                    {
                        return 0;
                    }
                    if (this.Duration == -1)
                    {
                        return -1;
                    }
                    int tickCount = Environment.TickCount;
                    int num2 = TimerThread.IsTickBetween(this.StartTime, this.Expiration, tickCount) ? ((int) Math.Min((uint) (this.Expiration - tickCount), 0x7fffffff)) : 0;
                    if (num2 >= 2)
                    {
                        return num2;
                    }
                    return (num2 + 1);
                }
            }
        }

        private class TimerNode : TimerThread.Timer
        {
            private TimerThread.Callback m_Callback;
            private object m_Context;
            private object m_QueueLock;
            private TimerState m_TimerState;
            private TimerThread.TimerNode next;
            private TimerThread.TimerNode prev;

            internal TimerNode() : base(0)
            {
                this.m_TimerState = TimerState.Sentinel;
            }

            internal TimerNode(TimerThread.Callback callback, object context, int durationMilliseconds, object queueLock) : base(durationMilliseconds)
            {
                if (callback != null)
                {
                    this.m_Callback = callback;
                    this.m_Context = context;
                }
                this.m_TimerState = TimerState.Ready;
                this.m_QueueLock = queueLock;
            }

            internal override bool Cancel()
            {
                if (this.m_TimerState == TimerState.Ready)
                {
                    lock (this.m_QueueLock)
                    {
                        if (this.m_TimerState == TimerState.Ready)
                        {
                            this.Next.Prev = this.Prev;
                            this.Prev.Next = this.Next;
                            this.Next = null;
                            this.Prev = null;
                            this.m_Callback = null;
                            this.m_Context = null;
                            this.m_TimerState = TimerState.Cancelled;
                            return true;
                        }
                    }
                }
                return false;
            }

            internal bool Fire()
            {
                if (this.m_TimerState == TimerState.Ready)
                {
                    int tickCount = Environment.TickCount;
                    if (TimerThread.IsTickBetween(base.StartTime, base.Expiration, tickCount))
                    {
                        return false;
                    }
                    bool flag = false;
                    lock (this.m_QueueLock)
                    {
                        if (this.m_TimerState == TimerState.Ready)
                        {
                            this.m_TimerState = TimerState.Fired;
                            this.Next.Prev = this.Prev;
                            this.Prev.Next = this.Next;
                            this.Next = null;
                            this.Prev = null;
                            flag = this.m_Callback != null;
                        }
                    }
                    if (flag)
                    {
                        try
                        {
                            TimerThread.Callback callback = this.m_Callback;
                            object context = this.m_Context;
                            this.m_Callback = null;
                            this.m_Context = null;
                            callback(this, tickCount, context);
                        }
                        catch (Exception exception)
                        {
                            if (NclUtilities.IsFatal(exception))
                            {
                                throw;
                            }
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.Web, "TimerThreadTimer#" + base.StartTime.ToString(NumberFormatInfo.InvariantInfo) + "::Fire() - " + SR.GetString("net_log_exception_in_callback", new object[] { exception }));
                            }
                        }
                    }
                }
                return true;
            }

            internal override bool HasExpired
            {
                get
                {
                    return (this.m_TimerState == TimerState.Fired);
                }
            }

            internal TimerThread.TimerNode Next
            {
                get
                {
                    return this.next;
                }
                set
                {
                    this.next = value;
                }
            }

            internal TimerThread.TimerNode Prev
            {
                get
                {
                    return this.prev;
                }
                set
                {
                    this.prev = value;
                }
            }

            private enum TimerState
            {
                Ready,
                Fired,
                Cancelled,
                Sentinel
            }
        }

        private class TimerQueue : TimerThread.Queue
        {
            private IntPtr m_ThisHandle;
            private readonly TimerThread.TimerNode m_Timers;

            internal TimerQueue(int durationMilliseconds) : base(durationMilliseconds)
            {
                this.m_Timers = new TimerThread.TimerNode();
                this.m_Timers.Next = this.m_Timers;
                this.m_Timers.Prev = this.m_Timers;
            }

            internal override TimerThread.Timer CreateTimer(TimerThread.Callback callback, object context)
            {
                TimerThread.TimerNode node = new TimerThread.TimerNode(callback, context, base.Duration, this.m_Timers);
                bool flag = false;
                lock (this.m_Timers)
                {
                    if (this.m_Timers.Next == this.m_Timers)
                    {
                        if (this.m_ThisHandle == IntPtr.Zero)
                        {
                            this.m_ThisHandle = (IntPtr) GCHandle.Alloc(this);
                        }
                        flag = true;
                    }
                    node.Next = this.m_Timers;
                    node.Prev = this.m_Timers.Prev;
                    this.m_Timers.Prev.Next = node;
                    this.m_Timers.Prev = node;
                }
                if (flag)
                {
                    TimerThread.Prod();
                }
                return node;
            }

            internal bool Fire(out int nextExpiration)
            {
                TimerThread.TimerNode next;
                do
                {
                    next = this.m_Timers.Next;
                    if (next == this.m_Timers)
                    {
                        lock (this.m_Timers)
                        {
                            next = this.m_Timers.Next;
                            if (next == this.m_Timers)
                            {
                                if (this.m_ThisHandle != IntPtr.Zero)
                                {
                                    ((GCHandle) this.m_ThisHandle).Free();
                                    this.m_ThisHandle = IntPtr.Zero;
                                }
                                nextExpiration = 0;
                                return false;
                            }
                        }
                    }
                }
                while (next.Fire());
                nextExpiration = next.Expiration;
                return true;
            }
        }

        private enum TimerThreadState
        {
            Idle,
            Running,
            Stopped
        }
    }
}

