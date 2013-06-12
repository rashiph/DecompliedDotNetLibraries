namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    internal sealed class ThreadPoolRequestQueue
    {
        private int tpCount;
        private _ThreadPoolWaitCallback tpHead;
        private object tpSync = new object();
        private _ThreadPoolWaitCallback tpTail;

        [SecurityCritical]
        public int DeQueue(ref _ThreadPoolWaitCallback callback)
        {
            bool lockTaken = false;
            _ThreadPoolWaitCallback callback2 = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this.tpSync, ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    _ThreadPoolWaitCallback tpHead = this.tpHead;
                    if (tpHead != null)
                    {
                        callback2 = tpHead;
                        this.tpHead = tpHead._next;
                        this.tpCount--;
                        if (this.tpCount == 0)
                        {
                            this.tpTail = null;
                            ThreadPool.ClearAppDomainRequestActive();
                        }
                    }
                    Monitor.Exit(this.tpSync);
                }
            }
            callback = callback2;
            return this.tpCount;
        }

        [SecuritySafeCritical]
        public int EnQueue(_ThreadPoolWaitCallback tpcallBack)
        {
            int tpCount = 0;
            bool lockTaken = false;
            bool flag2 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this.tpSync, ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    if (this.tpCount == 0)
                    {
                        flag2 = ThreadPool.SetAppDomainRequestActive();
                    }
                    this.tpCount++;
                    tpCount = this.tpCount;
                    if (this.tpHead == null)
                    {
                        this.tpHead = tpcallBack;
                        this.tpTail = tpcallBack;
                    }
                    else
                    {
                        this.tpTail._next = tpcallBack;
                        this.tpTail = tpcallBack;
                    }
                    Monitor.Exit(this.tpSync);
                    if (flag2)
                    {
                        ThreadPool.SetNativeTpEvent();
                    }
                }
            }
            return tpCount;
        }

        public int GetQueueCount()
        {
            return this.tpCount;
        }
    }
}

