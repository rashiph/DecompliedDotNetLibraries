namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class UnionQueryOperator<TInputOutput> : BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>
    {
        private readonly IEqualityComparer<TInputOutput> m_comparer;

        internal UnionQueryOperator(ParallelQuery<TInputOutput> left, ParallelQuery<TInputOutput> right, IEqualityComparer<TInputOutput> comparer) : base(left, right)
        {
            this.m_comparer = comparer;
            base.m_outputOrdered = base.LeftChild.OutputOrdered || base.RightChild.OutputOrdered;
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TInputOutput> first = CancellableEnumerable.Wrap<TInputOutput>(base.LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TInputOutput> second = CancellableEnumerable.Wrap<TInputOutput>(base.RightChild.AsSequentialQuery(token), token);
            return first.Union<TInputOutput>(second, this.m_comparer);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TInputOutput> leftChildQueryResults = base.LeftChild.Open(settings, false);
            return new BinaryQueryOperator<TInputOutput, TInputOutput, TInputOutput>.BinaryQueryOperatorResults(leftChildQueryResults, base.RightChild.Open(settings, false), this, settings, false);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TInputOutput, TLeftKey> leftStream, PartitionedStream<TInputOutput, TRightKey> rightStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = leftStream.PartitionCount;
            if (base.LeftChild.OutputOrdered)
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream = ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken);
                this.WrapPartitionedStreamFixedLeftType<TLeftKey, TRightKey>(leftHashStream, rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> stream2 = ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TLeftKey>(leftStream, null, null, this.m_comparer, settings.CancellationState.MergedCancellationToken);
                this.WrapPartitionedStreamFixedLeftType<int, TRightKey>(stream2, rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
        }

        private void WrapPartitionedStreamFixedBothTypes<TLeftKey, TRightKey>(PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream, PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightHashStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, int partitionCount, CancellationToken cancellationToken)
        {
            if (base.LeftChild.OutputOrdered || base.RightChild.OutputOrdered)
            {
                IComparer<ConcatKey<TLeftKey, TRightKey>> keyComparer = ConcatKey<TLeftKey, TRightKey>.MakeComparer(leftHashStream.KeyComparer, rightHashStream.KeyComparer);
                PartitionedStream<TInputOutput, ConcatKey<TLeftKey, TRightKey>> partitionedStream = new PartitionedStream<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(partitionCount, keyComparer, OrdinalIndexState.Shuffled);
                for (int i = 0; i < partitionCount; i++)
                {
                    partitionedStream[i] = new OrderedUnionQueryOperatorEnumerator<TInputOutput, TLeftKey, TRightKey>(leftHashStream[i], rightHashStream[i], base.LeftChild.OutputOrdered, base.RightChild.OutputOrdered, this.m_comparer, keyComparer, cancellationToken);
                }
                outputRecipient.Receive<ConcatKey<TLeftKey, TRightKey>>(partitionedStream);
            }
            else
            {
                PartitionedStream<TInputOutput, int> stream2 = new PartitionedStream<TInputOutput, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Shuffled);
                for (int j = 0; j < partitionCount; j++)
                {
                    stream2[j] = new UnionQueryOperatorEnumerator<TInputOutput, TLeftKey, TRightKey>(leftHashStream[j], rightHashStream[j], j, this.m_comparer, cancellationToken);
                }
                outputRecipient.Receive<int>(stream2);
            }
        }

        private void WrapPartitionedStreamFixedLeftType<TLeftKey, TRightKey>(PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftHashStream, PartitionedStream<TInputOutput, TRightKey> rightStream, IPartitionedStreamRecipient<TInputOutput> outputRecipient, int partitionCount, CancellationToken cancellationToken)
        {
            if (base.RightChild.OutputOrdered)
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightHashStream = ExchangeUtilities.HashRepartitionOrdered<TInputOutput, NoKeyMemoizationRequired, TRightKey>(rightStream, null, null, this.m_comparer, cancellationToken);
                this.WrapPartitionedStreamFixedBothTypes<TLeftKey, TRightKey>(leftHashStream, rightHashStream, outputRecipient, partitionCount, cancellationToken);
            }
            else
            {
                PartitionedStream<Pair<TInputOutput, NoKeyMemoizationRequired>, int> stream2 = ExchangeUtilities.HashRepartition<TInputOutput, NoKeyMemoizationRequired, TRightKey>(rightStream, null, null, this.m_comparer, cancellationToken);
                this.WrapPartitionedStreamFixedBothTypes<TLeftKey, int>(leftHashStream, stream2, outputRecipient, partitionCount, cancellationToken);
            }
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class OrderedUnionQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TInputOutput, ConcatKey<TLeftKey, TRightKey>>
        {
            private CancellationToken m_cancellationToken;
            private IEqualityComparer<TInputOutput> m_comparer;
            private IComparer<ConcatKey<TLeftKey, TRightKey>> m_keyComparer;
            private bool m_leftOrdered;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private IEnumerator<KeyValuePair<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>>> m_outputEnumerator;
            private bool m_rightOrdered;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> m_rightSource;

            internal OrderedUnionQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightSource, bool leftOrdered, bool rightOrdered, IEqualityComparer<TInputOutput> comparer, IComparer<ConcatKey<TLeftKey, TRightKey>> keyComparer, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_rightSource = rightSource;
                this.m_keyComparer = keyComparer;
                this.m_leftOrdered = leftOrdered;
                this.m_rightOrdered = rightOrdered;
                this.m_comparer = comparer;
                if (this.m_comparer == null)
                {
                    this.m_comparer = EqualityComparer<TInputOutput>.Default;
                }
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_leftSource.Dispose();
                this.m_rightSource.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref ConcatKey<TLeftKey, TRightKey> currentKey)
            {
                if (this.m_outputEnumerator == null)
                {
                    IEqualityComparer<Wrapper<TInputOutput>> comparer = new WrapperEqualityComparer<TInputOutput>(this.m_comparer);
                    Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>> dictionary = new Dictionary<Wrapper<TInputOutput>, Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>>(comparer);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    TLeftKey local = default(TLeftKey);
                    int num = 0;
                    while (this.m_leftSource.MoveNext(ref pair, ref local))
                    {
                        Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> pair2;
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        ConcatKey<TLeftKey, TRightKey> x = ConcatKey<TLeftKey, TRightKey>.MakeLeft(this.m_leftOrdered ? local : default(TLeftKey));
                        Wrapper<TInputOutput> key = new Wrapper<TInputOutput>(pair.First);
                        if (!dictionary.TryGetValue(key, out pair2) || (this.m_keyComparer.Compare(x, pair2.Second) < 0))
                        {
                            dictionary[key] = new Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(pair.First, x);
                        }
                    }
                    TRightKey local2 = default(TRightKey);
                    while (this.m_rightSource.MoveNext(ref pair, ref local2))
                    {
                        Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> pair3;
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        ConcatKey<TLeftKey, TRightKey> key2 = ConcatKey<TLeftKey, TRightKey>.MakeRight(this.m_rightOrdered ? local2 : default(TRightKey));
                        Wrapper<TInputOutput> wrapper2 = new Wrapper<TInputOutput>(pair.First);
                        if (!dictionary.TryGetValue(wrapper2, out pair3) || (this.m_keyComparer.Compare(key2, pair3.Second) < 0))
                        {
                            dictionary[wrapper2] = new Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>>(pair.First, key2);
                        }
                    }
                    this.m_outputEnumerator = dictionary.GetEnumerator();
                }
                if (this.m_outputEnumerator.MoveNext())
                {
                    Pair<TInputOutput, ConcatKey<TLeftKey, TRightKey>> pair4 = this.m_outputEnumerator.Current.Value;
                    currentElement = pair4.First;
                    currentKey = pair4.Second;
                    return true;
                }
                return false;
            }
        }

        private class UnionQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TInputOutput, int>
        {
            private CancellationToken m_cancellationToken;
            private readonly IEqualityComparer<TInputOutput> m_comparer;
            private System.Linq.Parallel.Set<TInputOutput> m_hashLookup;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> m_leftSource;
            private Shared<int> m_outputLoopCount;
            private readonly int m_partitionIndex;
            private QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> m_rightSource;

            internal UnionQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TInputOutput, NoKeyMemoizationRequired>, TRightKey> rightSource, int partitionIndex, IEqualityComparer<TInputOutput> comparer, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_rightSource = rightSource;
                this.m_partitionIndex = partitionIndex;
                this.m_comparer = comparer;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                if (this.m_leftSource != null)
                {
                    this.m_leftSource.Dispose();
                }
                if (this.m_rightSource != null)
                {
                    this.m_rightSource.Dispose();
                }
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                if (this.m_hashLookup == null)
                {
                    this.m_hashLookup = new System.Linq.Parallel.Set<TInputOutput>(this.m_comparer);
                    this.m_outputLoopCount = new Shared<int>(0);
                }
                if (this.m_leftSource != null)
                {
                    TLeftKey local = default(TLeftKey);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    int num = 0;
                    while (this.m_leftSource.MoveNext(ref pair, ref local))
                    {
                        if ((num++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if (this.m_hashLookup.Add(pair.First))
                        {
                            currentElement = pair.First;
                            return true;
                        }
                    }
                    this.m_leftSource.Dispose();
                    this.m_leftSource = null;
                }
                if (this.m_rightSource != null)
                {
                    TRightKey local2 = default(TRightKey);
                    Pair<TInputOutput, NoKeyMemoizationRequired> pair2 = new Pair<TInputOutput, NoKeyMemoizationRequired>();
                    while (this.m_rightSource.MoveNext(ref pair2, ref local2))
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
                    this.m_rightSource.Dispose();
                    this.m_rightSource = null;
                }
                return false;
            }
        }
    }
}

