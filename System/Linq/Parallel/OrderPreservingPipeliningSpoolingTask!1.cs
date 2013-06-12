namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class OrderPreservingPipeliningSpoolingTask<TOutput> : SpoolingTaskBase
    {
        private readonly bool m_autoBuffered;
        private readonly object m_bufferLock;
        private readonly Queue<Pair<int, TOutput>>[] m_buffers;
        private readonly bool[] m_consumerWaiting;
        private readonly QueryOperatorEnumerator<TOutput, int> m_partition;
        private readonly int m_partitionIndex;
        private readonly bool[] m_producerDone;
        private readonly bool[] m_producerWaiting;
        private readonly QueryTaskGroupState m_taskGroupState;
        private readonly TaskScheduler m_taskScheduler;
        private const int PRODUCER_BUFFER_AUTO_SIZE = 0x10;

        internal OrderPreservingPipeliningSpoolingTask(QueryOperatorEnumerator<TOutput, int> partition, QueryTaskGroupState taskGroupState, bool[] consumerWaiting, bool[] producerWaiting, bool[] producerDone, int partitionIndex, Queue<Pair<int, TOutput>>[] buffers, object bufferLock, TaskScheduler taskScheduler, bool autoBuffered) : base(partitionIndex, taskGroupState)
        {
            this.m_partition = partition;
            this.m_taskGroupState = taskGroupState;
            this.m_producerDone = producerDone;
            this.m_consumerWaiting = consumerWaiting;
            this.m_producerWaiting = producerWaiting;
            this.m_partitionIndex = partitionIndex;
            this.m_buffers = buffers;
            this.m_bufferLock = bufferLock;
            this.m_taskScheduler = taskScheduler;
            this.m_autoBuffered = autoBuffered;
        }

        public static void Spool(QueryTaskGroupState groupState, PartitionedStream<TOutput, int> partitions, bool[] consumerWaiting, bool[] producerWaiting, bool[] producerDone, Queue<Pair<int, TOutput>>[] buffers, object[] bufferLocks, TaskScheduler taskScheduler, bool autoBuffered)
        {
            int degreeOfParallelism = partitions.PartitionCount;
            for (int j = 0; j < degreeOfParallelism; j++)
            {
                buffers[j] = new Queue<Pair<int, TOutput>>(0x80);
                bufferLocks[j] = new object();
            }
            Task rootTask = new Task(delegate {
                for (int k = 0; k < degreeOfParallelism; k++)
                {
                    new OrderPreservingPipeliningSpoolingTask<TOutput>(partitions[k], groupState, consumerWaiting, producerWaiting, producerDone, k, buffers, bufferLocks[k], taskScheduler, autoBuffered).RunAsynchronously(taskScheduler);
                }
            });
            groupState.QueryBegin(rootTask);
            rootTask.Start(taskScheduler);
        }

        protected override void SpoolingFinally()
        {
            lock (this.m_bufferLock)
            {
                this.m_producerDone[this.m_partitionIndex] = true;
                if (this.m_consumerWaiting[this.m_partitionIndex])
                {
                    Monitor.Pulse(this.m_bufferLock);
                    this.m_consumerWaiting[this.m_partitionIndex] = false;
                }
            }
            base.SpoolingFinally();
            this.m_partition.Dispose();
        }

        protected override void SpoolingWork()
        {
            int num3;
            TOutput currentElement = default(TOutput);
            int currentKey = 0;
            int num2 = this.m_autoBuffered ? 0x10 : 1;
            Pair<int, TOutput>[] pairArray = new Pair<int, TOutput>[num2];
            QueryOperatorEnumerator<TOutput, int> partition = this.m_partition;
            CancellationToken mergedCancellationToken = this.m_taskGroupState.CancellationState.MergedCancellationToken;
            do
            {
                num3 = 0;
                while ((num3 < num2) && partition.MoveNext(ref currentElement, ref currentKey))
                {
                    pairArray[num3] = new Pair<int, TOutput>(currentKey, currentElement);
                    num3++;
                }
                if (num3 == 0)
                {
                    return;
                }
                lock (this.m_bufferLock)
                {
                    if (mergedCancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    for (int i = 0; i < num3; i++)
                    {
                        this.m_buffers[this.m_partitionIndex].Enqueue(pairArray[i]);
                    }
                    if (this.m_consumerWaiting[this.m_partitionIndex])
                    {
                        Monitor.Pulse(this.m_bufferLock);
                        this.m_consumerWaiting[this.m_partitionIndex] = false;
                    }
                    if (this.m_buffers[this.m_partitionIndex].Count >= 0x2000)
                    {
                        this.m_producerWaiting[this.m_partitionIndex] = true;
                        Monitor.Wait(this.m_bufferLock);
                    }
                }
            }
            while (num3 == num2);
        }
    }
}

