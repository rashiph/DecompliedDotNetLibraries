namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class LastQueryOperator<TSource> : UnaryQueryOperator<TSource, TSource>
    {
        private readonly Func<TSource, bool> m_predicate;
        private readonly bool m_prematureMergeNeeded;

        internal LastQueryOperator(IEnumerable<TSource> child, Func<TSource, bool> predicate) : base(child)
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
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMergeNeeded)
            {
                stream = QueryOperator<TSource>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TSource, int>) inputStream;
            }
            Shared<int> sharedLastCandidate = new Shared<int>(-1);
            CountdownEvent sharedBarrier = new CountdownEvent(partitionCount);
            PartitionedStream<TSource, int> partitionedStream = new PartitionedStream<TSource, int>(partitionCount, stream.KeyComparer, OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new LastQueryOperatorEnumerator<TSource, TKey>(stream[i], this.m_predicate, sharedLastCandidate, sharedBarrier, settings.CancellationState.MergedCancellationToken);
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

        private class LastQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TSource, int>
        {
            private bool m_alreadySearched;
            private CancellationToken m_cancellationToken;
            private Func<TSource, bool> m_predicate;
            private CountdownEvent m_sharedBarrier;
            private Shared<int> m_sharedLastCandidate;
            private QueryOperatorEnumerator<TSource, int> m_source;

            internal LastQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, int> source, Func<TSource, bool> predicate, Shared<int> sharedLastCandidate, CountdownEvent sharedBarrier, CancellationToken cancelToken)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_sharedLastCandidate = sharedLastCandidate;
                this.m_sharedBarrier = sharedBarrier;
                this.m_cancellationToken = cancelToken;
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
                    TSource local2 = default(TSource);
                    int num2 = 0;
                    for (int i = 0; this.m_source.MoveNext(ref local2, ref num2); i++)
                    {
                        if ((i & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if ((this.m_predicate == null) || this.m_predicate(local2))
                        {
                            local = local2;
                            num = num2;
                        }
                    }
                    if (num != -1)
                    {
                        int num4;
                        do
                        {
                            num4 = this.m_sharedLastCandidate.Value;
                            if ((num4 != -1) && (num <= num4))
                            {
                                goto Label_00A7;
                            }
                        }
                        while (Interlocked.CompareExchange(ref this.m_sharedLastCandidate.Value, num, num4) != num4);
                    }
                }
                finally
                {
                    this.m_sharedBarrier.Signal();
                }
            Label_00A7:
                this.m_alreadySearched = true;
                if (num != -1)
                {
                    this.m_sharedBarrier.Wait(this.m_cancellationToken);
                    if (this.m_sharedLastCandidate.Value == num)
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

