namespace System.Linq.Parallel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class AsynchronousChannel<T> : IDisposable
    {
        private T[][] m_buffer;
        private CancellationToken m_cancellationToken;
        private int m_chunkSize;
        private int m_consumerBufferIndex;
        private T[] m_consumerChunk;
        private int m_consumerChunkIndex;
        private ManualResetEventSlim m_consumerEvent;
        private volatile int m_consumerIsWaiting;
        private volatile bool m_done;
        private volatile int m_producerBufferIndex;
        private T[] m_producerChunk;
        private int m_producerChunkIndex;
        private ManualResetEventSlim m_producerEvent;
        private volatile int m_producerIsWaiting;

        internal AsynchronousChannel(int chunkSize, CancellationToken cancellationToken) : this(0x200, chunkSize, cancellationToken)
        {
        }

        internal AsynchronousChannel(int capacity, int chunkSize, CancellationToken cancellationToken)
        {
            if (chunkSize == 0)
            {
                chunkSize = Scheduling.GetDefaultChunkSize<T>();
            }
            this.m_buffer = new T[capacity + 1][];
            this.m_producerBufferIndex = 0;
            this.m_consumerBufferIndex = 0;
            this.m_producerEvent = new ManualResetEventSlim();
            this.m_consumerEvent = new ManualResetEventSlim();
            this.m_chunkSize = chunkSize;
            this.m_producerChunk = new T[chunkSize];
            this.m_producerChunkIndex = 0;
            this.m_cancellationToken = cancellationToken;
        }

        public void Dispose()
        {
            lock (((AsynchronousChannel<T>) this))
            {
                this.m_producerEvent.Dispose();
                this.m_producerEvent = null;
                this.m_consumerEvent.Dispose();
                this.m_consumerEvent = null;
            }
        }

        internal void DoneWithDequeueWait()
        {
            this.m_consumerIsWaiting = 0;
        }

        internal void Enqueue(T item)
        {
            int producerChunkIndex = this.m_producerChunkIndex;
            this.m_producerChunk[producerChunkIndex] = item;
            if (producerChunkIndex == (this.m_chunkSize - 1))
            {
                this.EnqueueChunk(this.m_producerChunk);
                this.m_producerChunk = new T[this.m_chunkSize];
            }
            this.m_producerChunkIndex = (producerChunkIndex + 1) % this.m_chunkSize;
        }

        private void EnqueueChunk(T[] chunk)
        {
            if (this.IsFull)
            {
                this.WaitUntilNonFull();
            }
            int producerBufferIndex = this.m_producerBufferIndex;
            this.m_buffer[producerBufferIndex] = chunk;
            Interlocked.Exchange(ref this.m_producerBufferIndex, (producerBufferIndex + 1) % this.m_buffer.Length);
            if ((this.m_consumerIsWaiting == 1) && !this.IsChunkBufferEmpty)
            {
                this.m_consumerIsWaiting = 0;
                this.m_consumerEvent.Set();
            }
        }

        internal void FlushBuffers()
        {
            this.FlushCachedChunk();
        }

        private void FlushCachedChunk()
        {
            if ((this.m_producerChunk != null) && (this.m_producerChunkIndex != 0))
            {
                T[] destinationArray = new T[this.m_producerChunkIndex];
                Array.Copy(this.m_producerChunk, destinationArray, this.m_producerChunkIndex);
                this.EnqueueChunk(destinationArray);
                this.m_producerChunk = null;
            }
        }

        private T[] InternalDequeueChunk()
        {
            int consumerBufferIndex = this.m_consumerBufferIndex;
            T[] localArray = this.m_buffer[consumerBufferIndex];
            this.m_buffer[consumerBufferIndex] = null;
            Interlocked.Exchange(ref this.m_consumerBufferIndex, (consumerBufferIndex + 1) % this.m_buffer.Length);
            if ((this.m_producerIsWaiting == 1) && !this.IsFull)
            {
                this.m_producerIsWaiting = 0;
                this.m_producerEvent.Set();
            }
            return localArray;
        }

        internal void SetDone()
        {
            this.m_done = true;
            lock (((AsynchronousChannel<T>) this))
            {
                if (this.m_consumerEvent != null)
                {
                    this.m_consumerEvent.Set();
                }
            }
        }

        internal bool TryDequeue(ref T item)
        {
            if (this.m_consumerChunk == null)
            {
                if (!this.TryDequeueChunk(ref this.m_consumerChunk))
                {
                    return false;
                }
                this.m_consumerChunkIndex = 0;
            }
            item = this.m_consumerChunk[this.m_consumerChunkIndex];
            this.m_consumerChunkIndex++;
            if (this.m_consumerChunkIndex == this.m_consumerChunk.Length)
            {
                this.m_consumerChunk = null;
            }
            return true;
        }

        internal bool TryDequeue(ref T item, ref ManualResetEventSlim waitEvent)
        {
            waitEvent = null;
            if (this.m_consumerChunk == null)
            {
                if (!this.TryDequeueChunk(ref this.m_consumerChunk, ref waitEvent))
                {
                    return false;
                }
                this.m_consumerChunkIndex = 0;
            }
            item = this.m_consumerChunk[this.m_consumerChunkIndex];
            this.m_consumerChunkIndex++;
            if (this.m_consumerChunkIndex == this.m_consumerChunk.Length)
            {
                this.m_consumerChunk = null;
            }
            return true;
        }

        private bool TryDequeueChunk(ref T[] chunk)
        {
            if (this.IsChunkBufferEmpty)
            {
                return false;
            }
            chunk = this.InternalDequeueChunk();
            return true;
        }

        private bool TryDequeueChunk(ref T[] chunk, ref ManualResetEventSlim waitEvent)
        {
            while (this.IsChunkBufferEmpty)
            {
                if (this.IsDone && this.IsChunkBufferEmpty)
                {
                    waitEvent = null;
                    return false;
                }
                this.m_consumerEvent.Reset();
                Interlocked.Exchange(ref this.m_consumerIsWaiting, 1);
                if (this.IsChunkBufferEmpty && !this.IsDone)
                {
                    waitEvent = this.m_consumerEvent;
                    return false;
                }
                this.m_consumerIsWaiting = 0;
            }
            chunk = this.InternalDequeueChunk();
            return true;
        }

        private void WaitUntilNonFull()
        {
            do
            {
                this.m_producerEvent.Reset();
                Interlocked.Exchange(ref this.m_producerIsWaiting, 1);
                if (this.IsFull)
                {
                    this.m_producerEvent.Wait(this.m_cancellationToken);
                }
                else
                {
                    this.m_producerIsWaiting = 0;
                }
            }
            while (this.IsFull);
        }

        internal bool IsChunkBufferEmpty
        {
            get
            {
                return (this.m_producerBufferIndex == this.m_consumerBufferIndex);
            }
        }

        internal bool IsDone
        {
            get
            {
                return this.m_done;
            }
        }

        internal bool IsFull
        {
            get
            {
                int producerBufferIndex = this.m_producerBufferIndex;
                int consumerBufferIndex = this.m_consumerBufferIndex;
                return ((producerBufferIndex == (consumerBufferIndex - 1)) || ((consumerBufferIndex == 0) && (producerBufferIndex == (this.m_buffer.Length - 1))));
            }
        }
    }
}

