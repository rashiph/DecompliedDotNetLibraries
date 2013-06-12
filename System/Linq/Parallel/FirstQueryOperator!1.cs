namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class FirstQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        private readonly Func<TSource, bool> m_predicate;
        private readonly bool m_prematureMergeNeeded;

        internal FirstQueryOperator(IEnumerable<TSource> child, Func<TSource, bool> predicate) : base(child)
        {
            this.m_predicate = predicate;
            this.m_prematureMergeNeeded = base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
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
            PartitionedStream<TSource, int> stream;
            OrdinalIndexState ordinalIndexState = inputStream.OrdinalIndexState;
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMergeNeeded)
            {
                stream = QueryOperator<TSource>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TSource, int>) inputStream;
            }
            Shared<int> sharedFirstCandidate = new Shared<int>(-1);
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);
            PartitionedStream<TSource, int> partitionedStream = new PartitionedStream<TSource, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new FirstQueryOperatorEnumerator<TSource>(stream[i], this.m_predicate, sharedFirstCandidate, sharedBarrier, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return this.m_prematureMergeNeeded;
            }
        }

        private class FirstQueryOperatorEnumerator : QueryOperatorEnumerator<TSource, int>
        {
            private bool m_alreadySearched;
            private CancellationToken m_cancellationToken;
            private Func<TSource, bool> m_predicate;
            private CountdownEvent m_sharedBarrier;
            private Shared<int> m_sharedFirstCandidate;
            private QueryOperatorEnumerator<TSource, int> m_source;

            internal FirstQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, int> source, Func<TSource, bool> predicate, Shared<int> sharedFirstCandidate, CountdownEvent sharedBarrier, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_sharedFirstCandidate = sharedFirstCandidate;
                this.m_sharedBarrier = sharedBarrier;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref int currentKey)
            {
                if (this.m_alreadySearched)
                {
                    return false;
                }
                TSource local = default(TSource);
                int num = -1;
                try
                {
                    int num2 = 0;
                    int num3 = 0;
                    while (this.m_source.MoveNext(ref local, ref num2))
                    {
                        if ((num3++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if ((this.m_predicate == null) || this.m_predicate(local))
                        {
                            int num4;
                            num = num2;
                            do
                            {
                                num4 = this.m_sharedFirstCandidate.Value;
                                if ((num4 != -1) && (num >= num4))
                                {
                                    break;
                                }
                            }
                            while (Interlocked.CompareExchange(ref this.m_sharedFirstCandidate.Value, num, num4) != num4);
                            goto Label_00B6;
                        }
                        if ((this.m_sharedFirstCandidate.Value != -1) && (num2 > this.m_sharedFirstCandidate.Value))
                        {
                            goto Label_00B6;
                        }
                    }
                }
                finally
                {
                    this.m_sharedBarrier.Signal();
                }
            Label_00B6:
                this.m_alreadySearched = true;
                if (num != -1)
                {
                    this.m_sharedBarrier.Wait(this.m_cancellationToken);
                    if (this.m_sharedFirstCandidate.Value == num)
                    {
                        currentElement = local;
                        currentKey = 0;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

