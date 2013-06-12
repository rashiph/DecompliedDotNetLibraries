namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal class OrderPreservingPipeliningMergeHelper<TOutput> : IMergeHelper<TOutput>
    {
        internal const int INITIAL_BUFFER_SIZE = 0x80;
        private readonly bool m_autoBuffered;
        private readonly object[] m_bufferLocks;
        private readonly Queue<Pair<int, TOutput>>[] m_buffers;
        private readonly bool[] m_consumerWaiting;
        private readonly PartitionedStream<TOutput, int> m_partitions;
        private readonly bool[] m_producerDone;
        private readonly bool[] m_producerWaiting;
        private readonly QueryTaskGroupState m_taskGroupState;
        private readonly TaskScheduler m_taskScheduler;
        internal const int MAX_BUFFER_SIZE = 0x2000;
        private static ProducerComparer<TOutput> s_producerComparer;
        internal const int STEAL_BUFFER_SIZE = 0x400;

        static OrderPreservingPipeliningMergeHelper()
        {
            OrderPreservingPipeliningMergeHelper<TOutput>.s_producerComparer = new ProducerComparer<TOutput>();
        }

        internal OrderPreservingPipeliningMergeHelper(PartitionedStream<TOutput, int> partitions, TaskScheduler taskScheduler, CancellationState cancellationState, bool autoBuffered, int queryId)
        {
            this.m_taskGroupState = new QueryTaskGroupState(cancellationState, queryId);
            this.m_partitions = partitions;
            this.m_taskScheduler = taskScheduler;
            this.m_autoBuffered = autoBuffered;
            int partitionCount = this.m_partitions.PartitionCount;
            this.m_buffers = new Queue<Pair<int, TOutput>>[partitionCount];
            this.m_producerDone = new bool[partitionCount];
            this.m_consumerWaiting = new bool[partitionCount];
            this.m_producerWaiting = new bool[partitionCount];
            this.m_bufferLocks = new object[partitionCount];
        }

        public TOutput[] GetResultsAsArray()
        {
            throw new InvalidOperationException();
        }

        void IMergeHelper<TOutput>.Execute()
        {
            OrderPreservingPipeliningSpoolingTask<TOutput>.Spool(this.m_taskGroupState, this.m_partitions, this.m_consumerWaiting, this.m_producerWaiting, this.m_producerDone, this.m_buffers, this.m_bufferLocks, this.m_taskScheduler, this.m_autoBuffered);
        }

        IEnumerator<TOutput> IMergeHelper<TOutput>.GetEnumerator()
        {
            return new OrderedPipeliningMergeEnumerator<TOutput>((OrderPreservingPipeliningMergeHelper<TOutput>) this);
        }

        private class OrderedPipeliningMergeEnumerator : MergeEnumerator<TOutput>
        {
            private bool m_initialized;
            private OrderPreservingPipeliningMergeHelper<TOutput> m_mergeHelper;
            private readonly Queue<Pair<int, TOutput>>[] m_privateBuffer;
            private readonly FixedMaxHeap<OrderPreservingPipeliningMergeHelper<TOutput>.Producer> m_producerHeap;
            private readonly TOutput[] m_producerNextElement;

            internal OrderedPipeliningMergeEnumerator(OrderPreservingPipeliningMergeHelper<TOutput> mergeHelper) : base(mergeHelper.m_taskGroupState)
            {
                int partitionCount = mergeHelper.m_partitions.PartitionCount;
                this.m_mergeHelper = mergeHelper;
                this.m_producerHeap = new FixedMaxHeap<OrderPreservingPipeliningMergeHelper<TOutput>.Producer>(partitionCount, OrderPreservingPipeliningMergeHelper<TOutput>.s_producerComparer);
                this.m_privateBuffer = new Queue<Pair<int, TOutput>>[partitionCount];
                this.m_producerNextElement = new TOutput[partitionCount];
            }

            public override void Dispose()
            {
                int length = this.m_mergeHelper.m_buffers.Length;
                for (int i = 0; i < length; i++)
                {
                    object obj2 = this.m_mergeHelper.m_bufferLocks[i];
                    lock (obj2)
                    {
                        if (this.m_mergeHelper.m_producerWaiting[i])
                        {
                            Monitor.Pulse(obj2);
                        }
                    }
                }
                base.Dispose();
            }

            public override bool MoveNext()
            {
                if (!this.m_initialized)
                {
                    this.m_initialized = true;
                    for (int i = 0; i < this.m_mergeHelper.m_partitions.PartitionCount; i++)
                    {
                        Pair<int, TOutput> element = new Pair<int, TOutput>();
                        if (this.TryWaitForElement(i, ref element))
                        {
                            this.m_producerHeap.Insert(new OrderPreservingPipeliningMergeHelper<TOutput>.Producer(element.First, i));
                            this.m_producerNextElement[i] = element.Second;
                        }
                        else
                        {
                            this.ThrowIfInTearDown();
                        }
                    }
                }
                else
                {
                    if (this.m_producerHeap.Count == 0)
                    {
                        return false;
                    }
                    int producerIndex = this.m_producerHeap.MaxValue.ProducerIndex;
                    Pair<int, TOutput> pair2 = new Pair<int, TOutput>();
                    if (this.TryGetPrivateElement(producerIndex, ref pair2) || this.TryWaitForElement(producerIndex, ref pair2))
                    {
                        this.m_producerHeap.ReplaceMax(new OrderPreservingPipeliningMergeHelper<TOutput>.Producer(pair2.First, producerIndex));
                        this.m_producerNextElement[producerIndex] = pair2.Second;
                    }
                    else
                    {
                        this.ThrowIfInTearDown();
                        this.m_producerHeap.RemoveMax();
                    }
                }
                return (this.m_producerHeap.Count > 0);
            }

            private void ThrowIfInTearDown()
            {
                if (this.m_mergeHelper.m_taskGroupState.CancellationState.MergedCancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        object[] bufferLocks = this.m_mergeHelper.m_bufferLocks;
                        for (int i = 0; i < bufferLocks.Length; i++)
                        {
                            lock (bufferLocks[i])
                            {
                                Monitor.Pulse(bufferLocks[i]);
                            }
                        }
                        base.m_taskGroupState.QueryEnd(false);
                    }
                    finally
                    {
                        this.m_producerHeap.Clear();
                    }
                }
            }

            private bool TryGetPrivateElement(int producer, ref Pair<int, TOutput> element)
            {
                Queue<Pair<int, TOutput>> queue = this.m_privateBuffer[producer];
                if (queue != null)
                {
                    if (queue.Count > 0)
                    {
                        element = queue.Dequeue();
                        return true;
                    }
                    this.m_privateBuffer[producer] = null;
                }
                return false;
            }

            private bool TryWaitForElement(int producer, ref Pair<int, TOutput> element)
            {
                Queue<Pair<int, TOutput>> queue = this.m_mergeHelper.m_buffers[producer];
                object obj2 = this.m_mergeHelper.m_bufferLocks[producer];
                lock (obj2)
                {
                    if (queue.Count == 0)
                    {
                        if (this.m_mergeHelper.m_producerDone[producer])
                        {
                            element = new Pair<int, TOutput>();
                            return false;
                        }
                        this.m_mergeHelper.m_consumerWaiting[producer] = true;
                        Monitor.Wait(obj2);
                        if (queue.Count == 0)
                        {
                            element = new Pair<int, TOutput>();
                            return false;
                        }
                    }
                    if (this.m_mergeHelper.m_producerWaiting[producer])
                    {
                        Monitor.Pulse(obj2);
                        this.m_mergeHelper.m_producerWaiting[producer] = false;
                    }
                    if (queue.Count < 0x400)
                    {
                        element = queue.Dequeue();
                        return true;
                    }
                    this.m_privateBuffer[producer] = this.m_mergeHelper.m_buffers[producer];
                    this.m_mergeHelper.m_buffers[producer] = new Queue<Pair<int, TOutput>>(0x80);
                }
                this.TryGetPrivateElement(producer, ref element);
                return true;
            }

            public override TOutput Current
            {
                get
                {
                    int producerIndex = this.m_producerHeap.MaxValue.ProducerIndex;
                    return this.m_producerNextElement[producerIndex];
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Producer
        {
            internal readonly int MaxKey;
            internal readonly int ProducerIndex;
            internal Producer(int maxKey, int producerIndex)
            {
                this.MaxKey = maxKey;
                this.ProducerIndex = producerIndex;
            }
        }

        private class ProducerComparer : IComparer<OrderPreservingPipeliningMergeHelper<TOutput>.Producer>
        {
            public int Compare(OrderPreservingPipeliningMergeHelper<TOutput>.Producer x, OrderPreservingPipeliningMergeHelper<TOutput>.Producer y)
            {
                return (y.MaxKey - x.MaxKey);
            }
        }
    }
}

