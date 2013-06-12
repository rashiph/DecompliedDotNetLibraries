namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DefaultIfEmptyQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        private readonly TSource m_defaultValue;

        internal DefaultIfEmptyQueryOperator(IEnumerable<TSource> child, TSource defaultValue) : base(child)
        {
            this.m_defaultValue = defaultValue;
            base.SetOrdinalIndexState(base.Child.OrdinalIndexState.Worse(OrdinalIndexState.Correct));
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return base.Child.AsSequentialQuery(token).DefaultIfEmpty<TSource>(this.m_defaultValue);
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TSource, TSource>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            Shared<int> sharedEmptyCount = new Shared<int>(0);
            CountdownEvent sharedLatch = new CountdownEvent(partitionCount - 1);
            PartitionedStream<TSource, TKey> partitionedStream = new PartitionedStream<TSource, TKey>(partitionCount, inputStream.KeyComparer, this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new DefaultIfEmptyQueryOperatorEnumerator<TSource, TKey>(inputStream[i], this.m_defaultValue, i, partitionCount, sharedEmptyCount, sharedLatch, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<TKey>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class DefaultIfEmptyQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, TKey>
        {
            private CancellationToken m_cancelToken;
            private TSource m_defaultValue;
            private bool m_lookedForEmpty;
            private int m_partitionCount;
            private int m_partitionIndex;
            private Shared<int> m_sharedEmptyCount;
            private CountdownEvent m_sharedLatch;
            private QueryOperatorEnumerator<TSource, TKey> m_source;

            internal DefaultIfEmptyQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source, TSource defaultValue, int partitionIndex, int partitionCount, Shared<int> sharedEmptyCount, CountdownEvent sharedLatch, CancellationToken cancelToken)
            {
                this.m_source = source;
                this.m_defaultValue = defaultValue;
                this.m_partitionIndex = partitionIndex;
                this.m_partitionCount = partitionCount;
                this.m_sharedEmptyCount = sharedEmptyCount;
                this.m_sharedLatch = sharedLatch;
                this.m_cancelToken = cancelToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref TKey currentKey)
            {
                bool flag = this.m_source.MoveNext(ref currentElement, ref currentKey);
                if (!this.m_lookedForEmpty)
                {
                    this.m_lookedForEmpty = true;
                    if (!flag)
                    {
                        if (this.m_partitionIndex == 0)
                        {
                            this.m_sharedLatch.Wait(this.m_cancelToken);
                            this.m_sharedLatch.Dispose();
                            if (this.m_sharedEmptyCount.Value == (this.m_partitionCount - 1))
                            {
                                currentElement = this.m_defaultValue;
                                currentKey = default(TKey);
                                return true;
                            }
                            return false;
                        }
                        Interlocked.Increment(ref this.m_sharedEmptyCount.Value);
                    }
                    if (this.m_partitionIndex != 0)
                    {
                        this.m_sharedLatch.Signal();
                    }
                }
                return flag;
            }
        }
    }
}

