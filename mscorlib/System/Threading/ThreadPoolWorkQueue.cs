namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class ThreadPoolWorkQueue
    {
        internal static SparseArray<WorkStealingQueue> allThreadQueues = new SparseArray<WorkStealingQueue>(0x10);
        private volatile int numOutstandingThreadRequests;
        internal volatile QueueSegment queueHead;
        internal volatile QueueSegment queueTail;

        public ThreadPoolWorkQueue()
        {
            this.queueTail = this.queueHead = new QueueSegment();
        }

        [SecurityCritical]
        public void Dequeue(ThreadPoolWorkQueueThreadLocals tl, out IThreadPoolWorkItem callback, out bool missedSteal)
        {
            callback = null;
            missedSteal = false;
            WorkStealingQueue workStealingQueue = tl.workStealingQueue;
            workStealingQueue.LocalPop(out callback);
            if (callback == null)
            {
                for (QueueSegment segment = this.queueTail; (!segment.TryDequeue(out callback) && (segment.Next != null)) && segment.IsUsedUp(); segment = this.queueTail)
                {
                    Interlocked.CompareExchange<QueueSegment>(ref this.queueTail, segment.Next, segment);
                }
            }
            if (callback == null)
            {
                WorkStealingQueue[] current = allThreadQueues.Current;
                int num = tl.random.Next(current.Length);
                for (int i = current.Length; i > 0; i--)
                {
                    WorkStealingQueue queue2 = current[num % current.Length];
                    if (((queue2 != null) && (queue2 != workStealingQueue)) && queue2.TrySteal(out callback, ref missedSteal))
                    {
                        return;
                    }
                    num++;
                }
            }
        }

        [SecurityCritical]
        internal static bool Dispatch()
        {
            bool flag4;
            int tickCount = Environment.TickCount;
            ThreadPoolGlobals.workQueue.MarkThreadRequestSatisfied();
            bool flag = true;
            IThreadPoolWorkItem callback = null;
            try
            {
                ThreadPoolWorkQueueThreadLocals tl = ThreadPoolGlobals.workQueue.EnsureCurrentThreadHasQueue();
                while ((Environment.TickCount - tickCount) < ThreadPoolGlobals.tpQuantum)
                {
                    try
                    {
                    }
                    finally
                    {
                        bool missedSteal = false;
                        ThreadPoolGlobals.workQueue.Dequeue(tl, out callback, out missedSteal);
                        if (callback == null)
                        {
                            flag = missedSteal;
                        }
                        else
                        {
                            ThreadPoolGlobals.workQueue.EnsureThreadRequested();
                        }
                    }
                    if (callback == null)
                    {
                        return true;
                    }
                    if (ThreadPoolGlobals.enableWorkerTracking)
                    {
                        bool flag3 = false;
                        try
                        {
                            try
                            {
                            }
                            finally
                            {
                                ThreadPool.ReportThreadStatus(true);
                                flag3 = true;
                            }
                            callback.ExecuteWorkItem();
                            callback = null;
                        }
                        finally
                        {
                            if (flag3)
                            {
                                ThreadPool.ReportThreadStatus(false);
                            }
                        }
                    }
                    else
                    {
                        callback.ExecuteWorkItem();
                        callback = null;
                    }
                    if (!ThreadPool.NotifyWorkItemComplete())
                    {
                        return false;
                    }
                }
                flag4 = true;
            }
            catch (ThreadAbortException exception)
            {
                if (callback != null)
                {
                    callback.MarkAborted(exception);
                }
                flag = false;
                throw;
            }
            finally
            {
                if (flag)
                {
                    ThreadPoolGlobals.workQueue.EnsureThreadRequested();
                }
            }
            return flag4;
        }

        [SecurityCritical]
        public void Enqueue(IThreadPoolWorkItem callback, bool forceGlobal)
        {
            ThreadPoolWorkQueueThreadLocals threadLocals = null;
            if (!forceGlobal)
            {
                threadLocals = ThreadPoolWorkQueueThreadLocals.threadLocals;
            }
            if (threadLocals != null)
            {
                threadLocals.workStealingQueue.LocalPush(callback);
            }
            else
            {
                QueueSegment queueHead = this.queueHead;
                while (!queueHead.TryEnqueue(callback))
                {
                    Interlocked.CompareExchange<QueueSegment>(ref queueHead.Next, new QueueSegment(), null);
                    while (queueHead.Next != null)
                    {
                        Interlocked.CompareExchange<QueueSegment>(ref this.queueHead, queueHead.Next, queueHead);
                        queueHead = this.queueHead;
                    }
                }
            }
            this.EnsureThreadRequested();
        }

        [SecurityCritical]
        public ThreadPoolWorkQueueThreadLocals EnsureCurrentThreadHasQueue()
        {
            if (ThreadPoolWorkQueueThreadLocals.threadLocals == null)
            {
                ThreadPoolWorkQueueThreadLocals.threadLocals = new ThreadPoolWorkQueueThreadLocals(this);
            }
            return ThreadPoolWorkQueueThreadLocals.threadLocals;
        }

        [SecurityCritical]
        internal void EnsureThreadRequested()
        {
            int num2;
            for (int i = this.numOutstandingThreadRequests; i < ThreadPoolGlobals.processorCount; i = num2)
            {
                num2 = Interlocked.CompareExchange(ref this.numOutstandingThreadRequests, i + 1, i);
                if (num2 == i)
                {
                    ThreadPool.AdjustThreadsInPool(1);
                    return;
                }
            }
        }

        [SecurityCritical]
        internal bool LocalFindAndPop(IThreadPoolWorkItem callback)
        {
            ThreadPoolWorkQueueThreadLocals threadLocals = ThreadPoolWorkQueueThreadLocals.threadLocals;
            if (threadLocals == null)
            {
                return false;
            }
            return threadLocals.workStealingQueue.LocalFindAndPop(callback);
        }

        [SecurityCritical]
        internal void MarkThreadRequestSatisfied()
        {
            int num2;
            for (int i = this.numOutstandingThreadRequests; i > 0; i = num2)
            {
                num2 = Interlocked.CompareExchange(ref this.numOutstandingThreadRequests, i - 1, i);
                if (num2 == i)
                {
                    return;
                }
            }
        }

        internal class QueueSegment
        {
            private volatile int indexes;
            public volatile ThreadPoolWorkQueue.QueueSegment Next;
            internal IThreadPoolWorkItem[] nodes = new IThreadPoolWorkItem[0x100];
            private const int QueueSegmentLength = 0x100;
            private const int SixteenBits = 0xffff;

            private bool CompareExchangeIndexes(ref int prevUpper, int newUpper, ref int prevLower, int newLower)
            {
                int comparand = (prevUpper << 0x10) | (prevLower & 0xffff);
                int num2 = (newUpper << 0x10) | (newLower & 0xffff);
                int num3 = Interlocked.CompareExchange(ref this.indexes, num2, comparand);
                prevUpper = (num3 >> 0x10) & 0xffff;
                prevLower = num3 & 0xffff;
                return (num3 == comparand);
            }

            private void GetIndexes(out int upper, out int lower)
            {
                int indexes = this.indexes;
                upper = (indexes >> 0x10) & 0xffff;
                lower = indexes & 0xffff;
            }

            public bool IsUsedUp()
            {
                int num;
                int num2;
                this.GetIndexes(out num, out num2);
                return ((num == this.nodes.Length) && (num2 == this.nodes.Length));
            }

            public bool TryDequeue(out IThreadPoolWorkItem node)
            {
                int num;
                int num2;
                this.GetIndexes(out num, out num2);
                do
                {
                    if (num2 == num)
                    {
                        node = null;
                        return false;
                    }
                }
                while (!this.CompareExchangeIndexes(ref num, num, ref num2, num2 + 1));
                SpinWait wait = new SpinWait();
                while (this.nodes[num2] == null)
                {
                    wait.SpinOnce();
                }
                node = this.nodes[num2];
                this.nodes[num2] = null;
                return true;
            }

            public bool TryEnqueue(IThreadPoolWorkItem node)
            {
                int num;
                int num2;
                this.GetIndexes(out num, out num2);
                do
                {
                    if (num == this.nodes.Length)
                    {
                        return false;
                    }
                }
                while (!this.CompareExchangeIndexes(ref num, num + 1, ref num2, num2));
                this.nodes[num] = node;
                return true;
            }
        }

        internal class SparseArray<T> where T: class
        {
            private T[] m_array;

            internal SparseArray(int initialSize)
            {
                this.m_array = new T[initialSize];
            }

            internal int Add(T e)
            {
                T[] localArray;
                int num2;
            Label_0000:
                localArray = this.m_array;
                lock (localArray)
                {
                    for (int i = 0; i < localArray.Length; i++)
                    {
                        if (localArray[i] == null)
                        {
                            localArray[i] = e;
                            return i;
                        }
                        if ((i == (localArray.Length - 1)) && (localArray == this.m_array))
                        {
                            T[] destinationArray = new T[localArray.Length * 2];
                            Array.Copy(localArray, destinationArray, (int) (i + 1));
                            destinationArray[i + 1] = e;
                            this.m_array = destinationArray;
                            return (i + 1);
                        }
                    }
                    goto Label_0000;
                }
                return num2;
            }

            internal void Remove(T e)
            {
                T[] array = this.m_array;
                lock (array)
                {
                    for (int i = 0; i < this.m_array.Length; i++)
                    {
                        if (this.m_array[i] == e)
                        {
                            this.m_array[i] = default(T);
                            break;
                        }
                    }
                }
            }

            internal T[] Current
            {
                get
                {
                    return this.m_array;
                }
            }
        }

        internal class WorkStealingQueue
        {
            private const int INITIAL_SIZE = 0x20;
            internal IThreadPoolWorkItem[] m_array = new IThreadPoolWorkItem[0x20];
            private SpinLock m_foreignLock = new SpinLock(false);
            private volatile int m_headIndex;
            private int m_mask = 0x1f;
            private volatile int m_tailIndex;
            private const int START_INDEX = 0;

            public bool LocalFindAndPop(IThreadPoolWorkItem obj)
            {
                if (this.m_array[(this.m_tailIndex - 1) & this.m_mask] == obj)
                {
                    IThreadPoolWorkItem item;
                    return this.LocalPop(out item);
                }
                for (int i = this.m_tailIndex - 2; i >= this.m_headIndex; i--)
                {
                    if (this.m_array[i & this.m_mask] == obj)
                    {
                        bool lockTaken = false;
                        try
                        {
                            this.m_foreignLock.Enter(ref lockTaken);
                            if (this.m_array[i & this.m_mask] == null)
                            {
                                return false;
                            }
                            this.m_array[i & this.m_mask] = null;
                            if (i == this.m_tailIndex)
                            {
                                this.m_tailIndex--;
                            }
                            else if (i == this.m_headIndex)
                            {
                                this.m_headIndex++;
                            }
                            return true;
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                this.m_foreignLock.Exit(false);
                            }
                        }
                    }
                }
                return false;
            }

            public bool LocalPop(out IThreadPoolWorkItem obj)
            {
                int num;
                bool flag2;
            Label_0000:
                num = this.m_tailIndex;
                if (this.m_headIndex >= num)
                {
                    obj = null;
                    return false;
                }
                num--;
                Interlocked.Exchange(ref this.m_tailIndex, num);
                if (this.m_headIndex <= num)
                {
                    int index = num & this.m_mask;
                    obj = this.m_array[index];
                    if (obj != null)
                    {
                        this.m_array[index] = null;
                        return true;
                    }
                    goto Label_0000;
                }
                bool lockTaken = false;
                try
                {
                    this.m_foreignLock.Enter(ref lockTaken);
                    if (this.m_headIndex <= num)
                    {
                        int num3 = num & this.m_mask;
                        obj = this.m_array[num3];
                        if (obj != null)
                        {
                            this.m_array[num3] = null;
                            return true;
                        }
                        goto Label_0000;
                    }
                    this.m_tailIndex = num + 1;
                    obj = null;
                    flag2 = false;
                }
                finally
                {
                    if (lockTaken)
                    {
                        this.m_foreignLock.Exit(false);
                    }
                }
                return flag2;
            }

            public void LocalPush(IThreadPoolWorkItem obj)
            {
                int tailIndex = this.m_tailIndex;
                if (tailIndex == 0x7fffffff)
                {
                    bool lockTaken = false;
                    try
                    {
                        this.m_foreignLock.Enter(ref lockTaken);
                        if (this.m_tailIndex == 0x7fffffff)
                        {
                            this.m_headIndex &= this.m_mask;
                            this.m_tailIndex = tailIndex = this.m_tailIndex & this.m_mask;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            this.m_foreignLock.Exit(true);
                        }
                    }
                }
                if (tailIndex < (this.m_headIndex + this.m_mask))
                {
                    this.m_array[tailIndex & this.m_mask] = obj;
                    this.m_tailIndex = tailIndex + 1;
                }
                else
                {
                    bool flag2 = false;
                    try
                    {
                        this.m_foreignLock.Enter(ref flag2);
                        int headIndex = this.m_headIndex;
                        int num3 = this.m_tailIndex - this.m_headIndex;
                        if (num3 >= this.m_mask)
                        {
                            IThreadPoolWorkItem[] itemArray = new IThreadPoolWorkItem[this.m_array.Length << 1];
                            for (int i = 0; i < this.m_array.Length; i++)
                            {
                                itemArray[i] = this.m_array[(i + headIndex) & this.m_mask];
                            }
                            this.m_array = itemArray;
                            this.m_headIndex = 0;
                            this.m_tailIndex = tailIndex = num3;
                            this.m_mask = (this.m_mask << 1) | 1;
                        }
                        this.m_array[tailIndex & this.m_mask] = obj;
                        this.m_tailIndex = tailIndex + 1;
                    }
                    finally
                    {
                        if (flag2)
                        {
                            this.m_foreignLock.Exit(false);
                        }
                    }
                }
            }

            public bool TrySteal(out IThreadPoolWorkItem obj, ref bool missedSteal)
            {
                return this.TrySteal(out obj, ref missedSteal, 0);
            }

            private bool TrySteal(out IThreadPoolWorkItem obj, ref bool missedSteal, int millisecondsTimeout)
            {
                obj = null;
            Label_0003:
                if (this.m_headIndex < this.m_tailIndex)
                {
                    bool lockTaken = false;
                    try
                    {
                        this.m_foreignLock.TryEnter(millisecondsTimeout, ref lockTaken);
                        if (lockTaken)
                        {
                            int headIndex = this.m_headIndex;
                            Interlocked.Exchange(ref this.m_headIndex, headIndex + 1);
                            if (headIndex < this.m_tailIndex)
                            {
                                int index = headIndex & this.m_mask;
                                obj = this.m_array[index];
                                if (obj != null)
                                {
                                    this.m_array[index] = null;
                                    return true;
                                }
                                goto Label_0003;
                            }
                            this.m_headIndex = headIndex;
                            obj = null;
                            missedSteal = true;
                        }
                        else
                        {
                            missedSteal = true;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            this.m_foreignLock.Exit(false);
                        }
                    }
                }
                return false;
            }
        }
    }
}

