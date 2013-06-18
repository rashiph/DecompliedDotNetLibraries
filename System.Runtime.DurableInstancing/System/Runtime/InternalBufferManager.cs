namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal abstract class InternalBufferManager
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InternalBufferManager()
        {
        }

        public abstract void Clear();
        public static InternalBufferManager Create(long maxBufferPoolSize, int maxBufferSize)
        {
            if (maxBufferPoolSize == 0L)
            {
                return GCBufferManager.Value;
            }
            return new PooledBufferManager(maxBufferPoolSize, maxBufferSize);
        }

        public abstract void ReturnBuffer(byte[] buffer);
        public abstract byte[] TakeBuffer(int bufferSize);

        private class GCBufferManager : InternalBufferManager
        {
            private static InternalBufferManager.GCBufferManager value = new InternalBufferManager.GCBufferManager();

            private GCBufferManager()
            {
            }

            public override void Clear()
            {
            }

            public override void ReturnBuffer(byte[] buffer)
            {
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                return Fx.AllocateByteArray(bufferSize);
            }

            public static InternalBufferManager.GCBufferManager Value
            {
                get
                {
                    return value;
                }
            }
        }

        private class PooledBufferManager : InternalBufferManager
        {
            private bool areQuotasBeingTuned;
            private BufferPool[] bufferPools;
            private int[] bufferSizes;
            private const int initialBufferCount = 1;
            private const int maxMissesBeforeTuning = 8;
            private long memoryLimit;
            private const int minBufferSize = 0x80;
            private long remainingMemory;
            private int totalMisses;
            private readonly object tuningLock;

            public PooledBufferManager(long maxMemoryToPool, int maxBufferSize)
            {
                long num2;
                this.tuningLock = new object();
                this.memoryLimit = maxMemoryToPool;
                this.remainingMemory = maxMemoryToPool;
                List<BufferPool> list = new List<BufferPool>();
                int bufferSize = 0x80;
            Label_002B:
                num2 = this.remainingMemory / ((long) bufferSize);
                int limit = (num2 > 0x7fffffffL) ? 0x7fffffff : ((int) num2);
                if (limit > 1)
                {
                    limit = 1;
                }
                list.Add(new BufferPool(bufferSize, limit));
                this.remainingMemory -= limit * bufferSize;
                if (bufferSize < maxBufferSize)
                {
                    long num4 = bufferSize * 2L;
                    if (num4 > maxBufferSize)
                    {
                        bufferSize = maxBufferSize;
                    }
                    else
                    {
                        bufferSize = (int) num4;
                    }
                    goto Label_002B;
                }
                this.bufferPools = list.ToArray();
                this.bufferSizes = new int[this.bufferPools.Length];
                for (int i = 0; i < this.bufferPools.Length; i++)
                {
                    this.bufferSizes[i] = this.bufferPools[i].BufferSize;
                }
            }

            private void ChangeQuota(ref BufferPool bufferPool, int delta)
            {
                BufferPool pool = bufferPool;
                int limit = pool.Limit + delta;
                BufferPool pool2 = new BufferPool(pool.BufferSize, limit);
                for (int i = 0; i < limit; i++)
                {
                    byte[] buffer = pool.Take();
                    if (buffer == null)
                    {
                        break;
                    }
                    pool2.Return(buffer);
                    pool2.IncrementCount();
                }
                this.remainingMemory -= pool.BufferSize * delta;
                bufferPool = pool2;
            }

            public override void Clear()
            {
                for (int i = 0; i < this.bufferPools.Length; i++)
                {
                    this.bufferPools[i].Clear();
                }
            }

            private void DecreaseQuota(ref BufferPool bufferPool)
            {
                this.ChangeQuota(ref bufferPool, -1);
            }

            private int FindMostExcessivePool()
            {
                long num = 0L;
                int num2 = -1;
                for (int i = 0; i < this.bufferPools.Length; i++)
                {
                    BufferPool pool = this.bufferPools[i];
                    if (pool.Peak < pool.Limit)
                    {
                        long num4 = (pool.Limit - pool.Peak) * pool.BufferSize;
                        if (num4 > num)
                        {
                            num2 = i;
                            num = num4;
                        }
                    }
                }
                return num2;
            }

            private int FindMostStarvedPool()
            {
                long num = 0L;
                int num2 = -1;
                for (int i = 0; i < this.bufferPools.Length; i++)
                {
                    BufferPool pool = this.bufferPools[i];
                    if (pool.Peak == pool.Limit)
                    {
                        long num4 = pool.Misses * pool.BufferSize;
                        if (num4 > num)
                        {
                            num2 = i;
                            num = num4;
                        }
                    }
                }
                return num2;
            }

            private BufferPool FindPool(int desiredBufferSize)
            {
                for (int i = 0; i < this.bufferSizes.Length; i++)
                {
                    if (desiredBufferSize <= this.bufferSizes[i])
                    {
                        return this.bufferPools[i];
                    }
                }
                return null;
            }

            private void IncreaseQuota(ref BufferPool bufferPool)
            {
                this.ChangeQuota(ref bufferPool, 1);
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                BufferPool pool = this.FindPool(buffer.Length);
                if (pool != null)
                {
                    if (buffer.Length != pool.BufferSize)
                    {
                        throw Fx.Exception.Argument("buffer", SRCore.BufferIsNotRightSizeForBufferManager);
                    }
                    if (pool.Return(buffer))
                    {
                        pool.IncrementCount();
                    }
                }
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                BufferPool pool = this.FindPool(bufferSize);
                if (pool == null)
                {
                    return Fx.AllocateByteArray(bufferSize);
                }
                byte[] buffer = pool.Take();
                if (buffer != null)
                {
                    pool.DecrementCount();
                    return buffer;
                }
                if (pool.Peak == pool.Limit)
                {
                    pool.Misses++;
                    if (++this.totalMisses >= 8)
                    {
                        this.TuneQuotas();
                    }
                }
                return Fx.AllocateByteArray(pool.BufferSize);
            }

            private void TuneQuotas()
            {
                if (!this.areQuotasBeingTuned)
                {
                    bool lockTaken = false;
                    try
                    {
                        Monitor.TryEnter(this.tuningLock, ref lockTaken);
                        if (!lockTaken || this.areQuotasBeingTuned)
                        {
                            return;
                        }
                        this.areQuotasBeingTuned = true;
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(this.tuningLock);
                        }
                    }
                    int index = this.FindMostStarvedPool();
                    if (index >= 0)
                    {
                        BufferPool pool = this.bufferPools[index];
                        if (this.remainingMemory < pool.BufferSize)
                        {
                            int num2 = this.FindMostExcessivePool();
                            if (num2 >= 0)
                            {
                                this.DecreaseQuota(ref this.bufferPools[num2]);
                            }
                        }
                        if (this.remainingMemory >= pool.BufferSize)
                        {
                            this.IncreaseQuota(ref this.bufferPools[index]);
                        }
                    }
                    for (int i = 0; i < this.bufferPools.Length; i++)
                    {
                        BufferPool pool2 = this.bufferPools[i];
                        pool2.Misses = 0;
                    }
                    this.totalMisses = 0;
                    this.areQuotasBeingTuned = false;
                }
            }

            private class BufferPool
            {
                private int bufferSize;
                private int count;
                private int limit;
                private int misses;
                private int peak;
                private SynchronizedPool<byte[]> pool;

                public BufferPool(int bufferSize, int limit)
                {
                    this.pool = new SynchronizedPool<byte[]>(limit);
                    this.bufferSize = bufferSize;
                    this.limit = limit;
                }

                public void Clear()
                {
                    this.pool.Clear();
                    this.count = 0;
                }

                public void DecrementCount()
                {
                    int num = this.count - 1;
                    if (num >= 0)
                    {
                        this.count = num;
                    }
                }

                public void IncrementCount()
                {
                    int num = this.count + 1;
                    if (num <= this.limit)
                    {
                        this.count = num;
                        if (num > this.peak)
                        {
                            this.peak = num;
                        }
                    }
                }

                public bool Return(byte[] buffer)
                {
                    return this.pool.Return(buffer);
                }

                public byte[] Take()
                {
                    return this.pool.Take();
                }

                public int BufferSize
                {
                    get
                    {
                        return this.bufferSize;
                    }
                }

                public int Limit
                {
                    get
                    {
                        return this.limit;
                    }
                }

                public int Misses
                {
                    get
                    {
                        return this.misses;
                    }
                    set
                    {
                        this.misses = value;
                    }
                }

                public int Peak
                {
                    get
                    {
                        return this.peak;
                    }
                }
            }
        }
    }
}

