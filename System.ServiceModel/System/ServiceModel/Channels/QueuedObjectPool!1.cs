namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal abstract class QueuedObjectPool<T>
    {
        private int batchAllocCount;
        private int maxFreeCount;
        private Queue<T> objectQueue;

        protected QueuedObjectPool()
        {
        }

        private void AllocObjects()
        {
            for (int i = 0; i < this.batchAllocCount; i++)
            {
                this.objectQueue.Enqueue(this.Create());
            }
        }

        public void Clear()
        {
            lock (this.ThisLock)
            {
                this.objectQueue.Clear();
            }
        }

        protected abstract T Create();
        protected void Initialize(int batchAllocCount, int maxFreeCount)
        {
            if (batchAllocCount <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("batchAllocCount"));
            }
            this.batchAllocCount = batchAllocCount;
            this.maxFreeCount = maxFreeCount;
            this.objectQueue = new Queue<T>(batchAllocCount);
        }

        public virtual bool Return(T value)
        {
            lock (this.ThisLock)
            {
                if (this.objectQueue.Count < this.maxFreeCount)
                {
                    this.objectQueue.Enqueue(value);
                    return true;
                }
                return false;
            }
        }

        public T Take()
        {
            lock (this.ThisLock)
            {
                if (this.objectQueue.Count == 0)
                {
                    this.AllocObjects();
                }
                return this.objectQueue.Dequeue();
            }
        }

        private object ThisLock
        {
            get
            {
                return this.objectQueue;
            }
        }
    }
}

