namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class SingleQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        private readonly Func<TSource, bool> m_predicate;

        internal SingleQueryOperator(IEnumerable<TSource> child, Func<TSource, bool> predicate) : base(child)
        {
            this.m_predicate = predicate;
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            throw new NotSupportedException();
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TSource, TSource>.UnaryQueryOperatorResults(base.Child.Open(settings, false), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TSource> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TSource, int> partitionedStream = new PartitionedStream<TSource, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Shuffled);
            Shared<int> totalElementCount = new Shared<int>(0);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new SingleQueryOperatorEnumerator<TSource, TKey>(inputStream[i], this.m_predicate, totalElementCount);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class SingleQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, int>
        {
            private bool m_alreadySearched;
            private Func<TSource, bool> m_predicate;
            private QueryOperatorEnumerator<TSource, TKey> m_source;
            private Shared<int> m_totalElementCount;
            private bool m_yieldExtra;

            internal SingleQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, TKey> source, Func<TSource, bool> predicate, Shared<int> totalElementCount)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_totalElementCount = totalElementCount;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref int currentKey)
            {
                if (this.m_alreadySearched)
                {
                    if (this.m_yieldExtra)
                    {
                        this.m_yieldExtra = false;
                        currentElement = default(TSource);
                        currentKey = 0;
                        return true;
                    }
                    return false;
                }
                bool flag = false;
                TSource local = default(TSource);
                TKey local2 = default(TKey);
                while (this.m_source.MoveNext(ref local, ref local2))
                {
                    if ((this.m_predicate == null) || this.m_predicate(local))
                    {
                        Interlocked.Increment(ref this.m_totalElementCount.Value);
                        currentElement = local;
                        currentKey = 0;
                        if (flag)
                        {
                            this.m_yieldExtra = true;
                            break;
                        }
                        flag = true;
                    }
                    if (this.m_totalElementCount.Value > 1)
                    {
                        break;
                    }
                }
                this.m_alreadySearched = true;
                return flag;
            }
        }
    }
}

