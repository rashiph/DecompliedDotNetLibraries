namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class DistinctQueryOperator<TInputOutput> : UnaryQueryOperator<TInputOutput, TInputOutput>
    {
        private readonly IEqualityComparer<TInputOutput> m_comparer;

        internal DistinctQueryOperator(IEnumerable<TInputOutput> source, IEqualityComparer<TInputOutput> comparer) : base(source)
        {
            this.m_comparer = comparer;
            base.SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            return CancellableEnumerable.Wrap<TInputOutput>(base.Child.AsSequentialQuery(token), token).Distinct<TInputOutput>(this.m_comparer);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInputOutput, TInputOutput>.UnaryQueryOperatorResults(base.Child.Open(settings, false), this, settings, false);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            if (base.OutputOrdered)
            {
                this.WrapPartitionedStreamHelper<TKey>(ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TKey>(inputStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), recipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                this.WrapPartitionedStreamHelper<int>(ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TKey>(inputStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), recipient, settings.CancellationState.MergedCancellationToken);
            }
        }

        private void WrapPartitionedStreamHelper<TKey>(PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> hashStream, IPartitionedStreamRecipient<TInputOutput> recipient, CancellationToken cancellationToken)
        {
            int partitionCount = hashStream.PartitionCount;
            PartitionedStream<TInputOutput, TKey> partitionedStream = new PartitionedStream<TInputOutput, TKey>(partitionCount, hashStream.KeyComparer, OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                if (base.OutputOrdered)
                {
                    partitionedStream[i] = new OrderedDistinctQueryOperatorEnumerator<TInputOutput, TKey>(hashStream[i], this.m_comparer, hashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    partitionedStream[i] = (QueryOperatorEnumerator<TInputOutput, TKey>) new DistinctQueryOperatorEnumerator<TInputOutput, TKey>(hashStream[i], this.m_comparer, cancellationToken);
                }
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

        private class DistinctQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, int>
        {
            private CancellationToken m_cancellationToken;
            private System.Linq.Parallel.Set<TInputOutput> m_hashLookup;
            private Shared<int> m_outputLoopCount;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> m_source;

            internal DistinctQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> source, IEqualityComparer<TInputOutput> comparer, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_hashLookup = new System.Linq.Parallel.Set<TInputOutput>(comparer);
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                TKey local = default(TKey);
                Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                if (this.m_outputLoopCount == null)
                {
                    this.m_outputLoopCount = new Shared<int>(0);
                }
                while (this.m_source.MoveNext(ref pair, ref local))
                {
                    if ((this.m_outputLoopCount.Value++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    if (this.m_hashLookup.Add(pair.First))
                    {
                        currentElement = pair.First;
                        return true;
                    }
                }
                return false;
            }
        }

        private class OrderedDistinctQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, TKey>
        {
            private CancellationToken m_cancellationToken;
            private Dictionary<Wrapper<TInputOutput>, TKey> m_hashLookup;
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, TKey>> m_hashLookupEnumerator;
            private IComparer<TKey> m_keyComparer;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> m_source;

            internal OrderedDistinctQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TKey> source, IEqualityComparer<TInputOutput> comparer, IComparer<TKey> keyComparer, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_keyComparer = keyComparer;
                this.m_hashLookup = new Dictionary<Wrapper<TInputOutput>, TKey>(new WrapperEqualityComparer<TInputOutput>(comparer));
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
                if (this.m_hashLookupEnumerator != null)
                {
                    this.m_hashLookupEnumerator.Dispose();
                }
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref TKey currentKey)
            {
                if (this.m_hashLookupEnumerator == null)
                {
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    TKey local = default(TKey);
                    int num = 0;
                    while (this.m_source.MoveNext(ref pair, ref local))
                    {
                        TKey local2;
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        Wrapper<TInputOutput> key = new Wrapper<TInputOutput>(pair.First);
                        if (!this.m_hashLookup.TryGetValue(key, out local2) || (this.m_keyComparer.Compare(local, local2) < 0))
                        {
                            this.m_hashLookup[key] = local;
                        }
                    }
                    this.m_hashLookupEnumerator = this.m_hashLookup.GetEnumerator();
                }
                if (this.m_hashLookupEnumerator.MoveNext())
                {
                    KeyValuePair<Wrapper<TInputOutput>, TKey> current = this.m_hashLookupEnumerator.Current;
                    currentElement = current.Key.Value;
                    currentKey = current.Value;
                    return true;
                }
                return false;
            }
        }
    }
}

