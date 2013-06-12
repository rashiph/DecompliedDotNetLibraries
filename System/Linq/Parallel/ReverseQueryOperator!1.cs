namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class ReverseQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        internal ReverseQueryOperator(IEnumerable<TSource> child) : base(child)
        {
            if (base.Child.OrdinalIndexState == OrdinalIndexState.Indexible)
            {
                base.SetOrdinalIndexState(OrdinalIndexState.Indexible);
            }
            else
            {
                base.SetOrdinalIndexState(OrdinalIndexState.Shuffled);
            }
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return CancellableEnumerable.Wrap<TSource>(base.Child.AsSequentialQuery(token), token).Reverse<TSource>();
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return ReverseQueryOperatorResults<TSource>.NewResults(base.Child.Open(settings, false), (ReverseQueryOperator<TSource>) this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TSource, TKey> partitionedStream = new PartitionedStream<TSource, TKey>(partitionCount, new ReverseComparer<TKey>(inputStream.KeyComparer), OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new ReverseQueryOperatorEnumerator<TSource, TKey>(inputStream[i], settings.CancellationState.MergedCancellationToken);
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

        private class ReverseQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, TKey>
        {
            private List<Pair<TSource, TKey>> m_buffer;
            private Shared<int> m_bufferIndex;
            private readonly CancellationToken m_cancellationToken;
            private readonly QueryOperatorEnumerator<TSource, TKey> m_source;

            internal ReverseQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref TKey currentKey)
            {
                if (this.m_buffer == null)
                {
                    this.m_bufferIndex = new Shared<int>(0);
                    this.m_buffer = new List<Pair<TSource, TKey>>();
                    TSource local = default(TSource);
                    TKey local2 = default(TKey);
                    int num = 0;
                    while (this.m_source.MoveNext(ref local, ref local2))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        this.m_buffer.Add(new Pair<TSource, TKey>(local, local2));
                        this.m_bufferIndex.Value += 1;
                    }
                }
                this.m_bufferIndex.Value -= 1;
                if (this.m_bufferIndex.Value >= 0)
                {
                    Pair<TSource, TKey> pair = this.m_buffer[this.m_bufferIndex.Value];
                    currentElement = pair.First;
                    Pair<TSource, TKey> pair2 = this.m_buffer[this.m_bufferIndex.Value];
                    currentKey = pair2.Second;
                    return true;
                }
                return false;
            }
        }

        private class ReverseQueryOperatorResults : UnaryQueryOperator<TSource, TSource>.UnaryQueryOperatorResults
        {
            private int m_count;

            private ReverseQueryOperatorResults(QueryResults<TSource> childQueryResults, ReverseQueryOperator<TSource> op, QuerySettings settings, bool preferStriping) : base(childQueryResults, op, settings, preferStriping)
            {
                this.m_count = base.m_childQueryResults.ElementsCount;
            }

            internal override TSource GetElement(int index)
            {
                return base.m_childQueryResults.GetElement((this.m_count - index) - 1);
            }

            public static QueryResults<TSource> NewResults(QueryResults<TSource> childQueryResults, ReverseQueryOperator<TSource> op, QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new ReverseQueryOperator<TSource>.ReverseQueryOperatorResults(childQueryResults, op, settings, preferStriping);
                }
                return new UnaryQueryOperator<TSource, TSource>.UnaryQueryOperatorResults(childQueryResults, op, settings, preferStriping);
            }

            internal override int ElementsCount
            {
                get
                {
                    return this.m_count;
                }
            }

            internal override bool IsIndexible
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

