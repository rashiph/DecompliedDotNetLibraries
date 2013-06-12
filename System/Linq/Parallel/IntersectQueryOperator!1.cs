namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class IntersectQueryOperator<TInputOutput> : BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {
        private readonly IEqualityComparer<TInputOutput> m_comparer;

        internal IntersectQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer) : base(left, right)
        {
            this.m_comparer = comparer;
            base.m_outputOrdered = base.LeftChild.OutputOrdered;
            base.SetOrdinalIndex(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> first = CancellableEnumerable.Wrap<TInputOutput>(base.LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> second = CancellableEnumerable.Wrap<TInputOutput>(base.RightChild.AsSequentialQuery(token), token);
            return first.Intersect<TInputOutput>(second, this.m_comparer);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TInputOutput> leftChildQueryResults = base.LeftChild.Open(settings, false);
            return new BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>.BinaryQueryOperatorResults(leftChildQueryResults, base.RightChild.Open(settings, false), this, settings, false);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TInputOutput, TLeftKey> leftPartitionedStream, PartitionedStream<TInputOutput, TRightKey> rightPartitionedStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            if (base.OutputOrdered)
            {
                this.WrapPartitionedStreamHelper<TLeftKey, TRightKey>(ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftPartitionedStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), rightPartitionedStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                this.WrapPartitionedStreamHelper<int, TRightKey>(ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftPartitionedStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), rightPartitionedStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
        }

        private void WrapPartitionedStreamHelper<TLeftKey, TRightKey>(PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream, PartitionedStream<TInputOutput, TRightKey> rightPartitionedStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, CancellationToken cancellationToken)
        {
            int partitionCount = leftHashStream.PartitionCount;
            PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> stream = ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TRightKey>(rightPartitionedStream, null, null, this.m_comparer, cancellationToken);
            PartitionedStream<TInputOutput, TLeftKey> partitionedStream = new PartitionedStream<TInputOutput, TLeftKey>(partitionCount, leftHashStream.KeyComparer, OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                if (base.OutputOrdered)
                {
                    partitionedStream[i] = new OrderedIntersectQueryOperatorEnumerator<TInputOutput, TLeftKey>(leftHashStream[i], stream[i], this.m_comparer, leftHashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    partitionedStream[i] = (QueryOperatorEnumerator<TInputOutput, TLeftKey>) new IntersectQueryOperatorEnumerator<TInputOutput, TLeftKey>(leftHashStream[i], stream[i], this.m_comparer, cancellationToken);
                }
            }
            outputRecipient.Receive<TLeftKey>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class IntersectQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, int>
        {
            private CancellationToken m_cancellationToken;
            private IEqualityComparer<TInputOutput> m_comparer;
            private System.Linq.Parallel.Set<TInputOutput> m_hashLookup;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private Shared<int> m_outputLoopCount;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource;

            internal IntersectQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource, IEqualityComparer<TInputOutput> comparer, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_rightSource = rightSource;
                this.m_comparer = comparer;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_leftSource.Dispose();
                this.m_rightSource.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                if (this.m_hashLookup == null)
                {
                    this.m_outputLoopCount = new Shared<int>(0);
                    this.m_hashLookup = new System.Linq.Parallel.Set<TInputOutput>(this.m_comparer);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    int num = 0;
                    int num2 = 0;
                    while (this.m_rightSource.MoveNext(ref pair, ref num))
                    {
                        if ((num2++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        this.m_hashLookup.Add(pair.First);
                    }
                }
                Pair<TInputOutput, NoKeyMemoizationRequired> pair2 = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                TLeftKey local = default(TLeftKey);
                while (this.m_leftSource.MoveNext(ref pair2, ref local))
                {
                    if ((this.m_outputLoopCount.Value++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    if (this.m_hashLookup.Contains(pair2.First))
                    {
                        this.m_hashLookup.Remove(pair2.First);
                        currentElement = pair2.First;
                        return true;
                    }
                }
                return false;
            }
        }

        private class OrderedIntersectQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, TLeftKey>
        {
            private CancellationToken m_cancellationToken;
            private IEqualityComparer<Wrapper<TInputOutput>> m_comparer;
            private Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>> m_hashLookup;
            private IComparer<TLeftKey> m_leftKeyComparer;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource;

            internal OrderedIntersectQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource, IEqualityComparer<TInputOutput> comparer, IComparer<TLeftKey> leftKeyComparer, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_rightSource = rightSource;
                this.m_comparer = new WrapperEqualityComparer<TInputOutput>(comparer);
                this.m_leftKeyComparer = leftKeyComparer;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_leftSource.Dispose();
                this.m_rightSource.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref TLeftKey currentKey)
            {
                int num = 0;
                if (this.m_hashLookup == null)
                {
                    this.m_hashLookup = new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>(this.m_comparer);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    TLeftKey local = default(TLeftKey);
                    while (this.m_leftSource.MoveNext(ref pair, ref local))
                    {
                        Pair<TInputOutput, TLeftKey> pair2;
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        Wrapper<TInputOutput> key = new Wrapper<TInputOutput>(pair.First);
                        if (!this.m_hashLookup.TryGetValue(key, out pair2) || (this.m_leftKeyComparer.Compare(local, pair2.Second) < 0))
                        {
                            this.m_hashLookup[key] = new Pair<TInputOutput, TLeftKey>(pair.First, local);
                        }
                    }
                }
                Pair<TInputOutput, NoKeyMemoizationRequired> pair3 = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                int num2 = 0;
                while (this.m_rightSource.MoveNext(ref pair3, ref num2))
                {
                    Pair<TInputOutput, TLeftKey> pair4;
                    if ((num++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    Wrapper<TInputOutput> wrapper2 = new Wrapper<TInputOutput>(pair3.First);
                    if (this.m_hashLookup.TryGetValue(wrapper2, out pair4))
                    {
                        currentElement = pair4.First;
                        currentKey = pair4.Second;
                        this.m_hashLookup.Remove(new Wrapper<TInputOutput>(pair4.First));
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

