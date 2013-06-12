namespace System.Threading
{
    using System;
    using System.Security;

    internal sealed class ThreadPoolWorkQueueThreadLocals
    {
        public readonly Random random = new Random(Thread.CurrentThread.ManagedThreadId);
        [ThreadStatic, SecurityCritical]
        public static ThreadPoolWorkQueueThreadLocals threadLocals;
        public readonly ThreadPoolWorkQueue workQueue;
        public readonly ThreadPoolWorkQueue.WorkStealingQueue workStealingQueue;

        public ThreadPoolWorkQueueThreadLocals(ThreadPoolWorkQueue tpq)
        {
            this.workQueue = tpq;
            this.workStealingQueue = new ThreadPoolWorkQueue.WorkStealingQueue();
            ThreadPoolWorkQueue.allThreadQueues.Add(this.workStealingQueue);
        }

        [SecurityCritical]
        private void CleanUp()
        {
            if (this.workStealingQueue != null)
            {
                if (this.workQueue != null)
                {
                    bool flag = false;
                    while (!flag)
                    {
                        try
                        {
                            continue;
                        }
                        finally
                        {
                            IThreadPoolWorkItem item = null;
                            if (this.workStealingQueue.LocalPop(out item))
                            {
                                this.workQueue.Enqueue(item, true);
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                    }
                }
                ThreadPoolWorkQueue.allThreadQueues.Remove(this.workStealingQueue);
            }
        }

        [SecuritySafeCritical]
        ~ThreadPoolWorkQueueThreadLocals()
        {
            if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                this.CleanUp();
            }
        }
    }
}

