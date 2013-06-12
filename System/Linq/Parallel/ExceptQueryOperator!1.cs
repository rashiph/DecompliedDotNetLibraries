namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class ExceptQueryOperator<TInputOutput> : BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {
        private readonly IEqualityComparer<TInputOutput> m_comparer;

        internal ExceptQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer) : base(left, right)
        {
            this.m_comparer = comparer;
            base.m_outputOrdered = base.LeftChild.OutputOrdered;
            base.SetOrdinalIndex(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> first = CancellableEnumerable.Wrap<TInputOutput>(base.LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> second = CancellableEnumerable.Wrap<TInputOutput>(base.RightChild.AsSequentialQuery(token), token);
            return first.Except<TInputOutput>(second, this.m_comparer);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TInputOutput> leftChildQueryResults = base.LeftChild.Open(settings, false);
            return new BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>.BinaryQueryOperatorResults(leftChildQueryResults, base.RightChild.Open(settings, false), this, settings, false);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TInputOutput, TLeftKey> leftStream, PartitionedStream<TInputOutput, TRightKey> rightStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            if (base.OutputOrdered)
            {
                this.WrapPartitionedStreamHelper<TLeftKey, TRightKey>(ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), rightStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                this.WrapPartitionedStreamHelper<int, TRightKey>(ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken), rightStream, outputRecipient, settings.CancellationState.MergedCancellationToken);
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
                    partitionedStream[i] = new OrderedExceptQueryOperatorEnumerator<TInputOutput, TLeftKey>(leftHashStream[i], stream[i], this.m_comparer, leftHashStream.KeyComparer, cancellationToken);
                }
                else
                {
                    partitionedStream[i] = (QueryOperatorEnumerator<TInputOutput, TLeftKey>) new ExceptQueryOperatorEnumerator<TInputOutput, TLeftKey>(leftHashStream[i], stream[i], this.m_comparer, cancellationToken);
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

        private class ExceptQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, int>
        {
            private CancellationToken m_cancellationToken;
            private IEqualityComparer<TInputOutput> m_comparer;
            private System.Linq.Parallel.Set<TInputOutput> m_hashLookup;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private Shared<int> m_outputLoopCount;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource;

            internal ExceptQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource, IEqualityComparer<TInputOutput> comparer, CancellationToken cancellationToken)
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
                    if (this.m_hashLookup.Add(pair2.First))
                    {
                        currentElement = pair2.First;
                        return true;
                    }
                }
                return false;
            }
        }

        private class OrderedExceptQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TInputOutput, TLeftKey>
        {
            private CancellationToken m_cancellationToken;
            private IEqualityComparer<TInputOutput> m_comparer;
            private IComparer<TLeftKey> m_leftKeyComparer;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>> m_outputEnumerator;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> m_rightSource;

            internal OrderedExceptQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, int> rightSource, IEqualityComparer<TInputOutput> comparer, IComparer<TLeftKey> leftKeyComparer, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_rightSource = rightSource;
                this.m_comparer = comparer;
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
                if (this.m_outputEnumerator == null)
                {
                    System.Linq.Parallel.Set<TInputOutput> set = new System.Linq.Parallel.Set<TInputOutput>(this.m_comparer);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    int num = 0;
                    int num2 = 0;
                    while (this.m_rightSource.MoveNext(ref pair, ref num))
                    {
                        if ((num2++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        set.Add(pair.First);
                    }
                    Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>> dictionary = new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, TLeftKey>>(new WrapperEqualityComparer<TInputOutput>(this.m_comparer));
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair2 = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    TLeftKey local = default(TLeftKey);
                    while (this.m_leftSource.MoveNext(ref pair2, ref local))
                    {
                        if ((num2++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if (!set.Contains(pair2.First))
                        {
                            Pair<TInputOutput, TLeftKey> pair3;
                            Wrapper<TInputOutput> key = new Wrapper<TInputOutput>(pair2.First);
                            if (!dictionary.TryGetValue(key, out pair3) || (this.m_leftKeyComparer.Compare(local, pair3.Second) < 0))
                            {
                                dictionary[key] = new Pair<TInputOutput, TLeftKey>(pair2.First, local);
                            }
                        }
                    }
                    this.m_outputEnumerator = dictionary.GetEnumerator();
                }
                if (this.m_outputEnumerator.MoveNext())
                {
                    Pair<TInputOutput, TLeftKey> pair4 = this.m_outputEnumerator.Current.Value;
                    currentElement = pair4.First;
                    currentKey = pair4.Second;
                    return true;
                }
                return false;
            }
        }
    }
}

