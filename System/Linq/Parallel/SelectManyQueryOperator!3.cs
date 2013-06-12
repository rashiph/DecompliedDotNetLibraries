namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class SelectManyQueryOperator<TLeftInput, TRightInput, TOutput> : UnaryQueryOperator<TLeftInput, TOutput>
    {
        private readonly Func<TLeftInput, int, IEnumerable<TRightInput>> m_indexedRightChildSelector;
        private bool m_prematureMerge;
        private readonly Func<TLeftInput, TRightInput, TOutput> m_resultSelector;
        private readonly Func<TLeftInput, IEnumerable<TRightInput>> m_rightChildSelector;

        internal SelectManyQueryOperator(IEnumerable<TLeftInput> leftChild, Func<TLeftInput, IEnumerable<TRightInput>> rightChildSelector, Func<TLeftInput, int, IEnumerable<TRightInput>> indexedRightChildSelector, Func<TLeftInput, TRightInput, TOutput> resultSelector) : base(leftChild)
        {
            this.m_rightChildSelector = rightChildSelector;
            this.m_indexedRightChildSelector = indexedRightChildSelector;
            this.m_resultSelector = resultSelector;
            base.m_outputOrdered = base.Child.OutputOrdered || (indexedRightChildSelector != null);
            this.InitOrderIndex();
        }

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            if (this.m_rightChildSelector != null)
            {
                if (this.m_resultSelector != null)
                {
                    return CancellableEnumerable.Wrap<TLeftInput>(base.Child.AsSequentialQuery(token), token).SelectMany<TLeftInput, TRightInput, TOutput>(this.m_rightChildSelector, this.m_resultSelector);
                }
                return (IEnumerable<TOutput>) CancellableEnumerable.Wrap<TLeftInput>(base.Child.AsSequentialQuery(token), token).SelectMany<TLeftInput, TRightInput>(this.m_rightChildSelector);
            }
            if (this.m_resultSelector != null)
            {
                return CancellableEnumerable.Wrap<TLeftInput>(base.Child.AsSequentialQuery(token), token).SelectMany<TLeftInput, TRightInput, TOutput>(this.m_indexedRightChildSelector, this.m_resultSelector);
            }
            return (IEnumerable<TOutput>) CancellableEnumerable.Wrap<TLeftInput>(base.Child.AsSequentialQuery(token), token).SelectMany<TLeftInput, TRightInput>(this.m_indexedRightChildSelector);
        }

        private void InitOrderIndex()
        {
            if (this.m_indexedRightChildSelector != null)
            {
                this.m_prematureMerge = base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Correct);
            }
            else if (base.OutputOrdered)
            {
                this.m_prematureMerge = base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
            }
            base.SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TLeftInput, TOutput>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TLeftKey>(PartitionedStream<TLeftInput, TLeftKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            if (this.m_indexedRightChildSelector != null)
            {
                PartitionedStream<TLeftInput, int> partitionedStream;
                if (this.m_prematureMerge)
                {
                    partitionedStream = QueryOperator<TLeftInput>.ExecuteAndCollectResults<TLeftKey>(inputStream, partitionCount, base.OutputOrdered, preferStriping, settings).GetPartitionedStream();
                }
                else
                {
                    partitionedStream = (PartitionedStream<TLeftInput, int>) inputStream;
                }
                this.WrapPartitionedStreamIndexed(partitionedStream, recipient, settings);
            }
            else if (this.m_prematureMerge)
            {
                PartitionedStream<TLeftInput, int> stream2 = QueryOperator<TLeftInput>.ExecuteAndCollectResults<TLeftKey>(inputStream, partitionCount, base.OutputOrdered, preferStriping, settings).GetPartitionedStream();
                this.WrapPartitionedStreamNotIndexed<int>(stream2, recipient, settings);
            }
            else
            {
                this.WrapPartitionedStreamNotIndexed<TLeftKey>(inputStream, recipient, settings);
            }
        }

        private void WrapPartitionedStreamIndexed(PartitionedStream<TLeftInput, int> inputStream, IPartitionedStreamRecipient<TOutput> recipient, QuerySettings settings)
        {
            PairComparer<int, int> keyComparer = new PairComparer<int, int>(inputStream.KeyComparer, Util.GetDefaultComparer<int>());
            PartitionedStream<TOutput, Pair<int, int>> partitionedStream = new PartitionedStream<TOutput, Pair<int, int>>(inputStream.PartitionCount, keyComparer, this.OrdinalIndexState);
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                partitionedStream[i] = new IndexedSelectManyQueryOperatorEnumerator<TLeftInput, TRightInput, TOutput>(inputStream[i], (SelectManyQueryOperator<TLeftInput, TRightInput, TOutput>) this, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<Pair<int, int>>(partitionedStream);
        }

        private void WrapPartitionedStreamNotIndexed<TLeftKey>(PartitionedStream<TLeftInput, TLeftKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PairComparer<TLeftKey, int> keyComparer = new PairComparer<TLeftKey, int>(inputStream.KeyComparer, Util.GetDefaultComparer<int>());
            PartitionedStream<TOutput, Pair<TLeftKey, int>> partitionedStream = new PartitionedStream<TOutput, Pair<TLeftKey, int>>(partitionCount, keyComparer, this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new SelectManyQueryOperatorEnumerator<TLeftInput, TRightInput, TOutput, TLeftKey>(inputStream[i], (SelectManyQueryOperator<TLeftInput, TRightInput, TOutput>) this, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<Pair<TLeftKey, int>>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return this.m_prematureMerge;
            }
        }

        private class IndexedSelectManyQueryOperatorEnumerator : QueryOperatorEnumerator<TOutput, Pair<int, int>>
        {
            private readonly CancellationToken m_cancellationToken;
            private IEnumerator<TRightInput> m_currentRightSource;
            private IEnumerator<TOutput> m_currentRightSourceAsOutput;
            private readonly QueryOperatorEnumerator<TLeftInput, int> m_leftSource;
            private Mutables<TLeftInput, TRightInput, TOutput> m_mutables;
            private readonly SelectManyQueryOperator<TLeftInput, TRightInput, TOutput> m_selectManyOperator;

            internal IndexedSelectManyQueryOperatorEnumerator(QueryOperatorEnumerator<TLeftInput, int> leftSource, SelectManyQueryOperator<TLeftInput, TRightInput, TOutput> selectManyOperator, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_selectManyOperator = selectManyOperator;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_leftSource.Dispose();
                if (this.m_currentRightSource != null)
                {
                    this.m_currentRightSource.Dispose();
                }
            }

            internal override bool MoveNext(ref TOutput currentElement, ref Pair<int, int> currentKey)
            {
                while (true)
                {
                    if (this.m_currentRightSource == null)
                    {
                        this.m_mutables = new Mutables<TLeftInput, TRightInput, TOutput>();
                        if ((this.m_mutables.m_lhsCount++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if (!this.m_leftSource.MoveNext(ref this.m_mutables.m_currentLeftElement, ref this.m_mutables.m_currentLeftSourceIndex))
                        {
                            return false;
                        }
                        this.m_currentRightSource = this.m_selectManyOperator.m_indexedRightChildSelector(this.m_mutables.m_currentLeftElement, this.m_mutables.m_currentLeftSourceIndex).GetEnumerator();
                        if (this.m_selectManyOperator.m_resultSelector == null)
                        {
                            this.m_currentRightSourceAsOutput = (IEnumerator<TOutput>) this.m_currentRightSource;
                        }
                    }
                    if (this.m_currentRightSource.MoveNext())
                    {
                        this.m_mutables.m_currentRightSourceIndex++;
                        if (this.m_selectManyOperator.m_resultSelector != null)
                        {
                            currentElement = this.m_selectManyOperator.m_resultSelector(this.m_mutables.m_currentLeftElement, this.m_currentRightSource.Current);
                        }
                        else
                        {
                            currentElement = this.m_currentRightSourceAsOutput.Current;
                        }
                        currentKey = new Pair<int, int>(this.m_mutables.m_currentLeftSourceIndex, this.m_mutables.m_currentRightSourceIndex);
                        return true;
                    }
                    this.m_currentRightSource.Dispose();
                    this.m_currentRightSource = null;
                    this.m_currentRightSourceAsOutput = null;
                }
            }

            private class Mutables
            {
                internal TLeftInput m_currentLeftElement;
                internal int m_currentLeftSourceIndex;
                internal int m_currentRightSourceIndex;
                internal int m_lhsCount;

                public Mutables()
                {
                    this.m_currentRightSourceIndex = -1;
                }
            }
        }

        private class SelectManyQueryOperatorEnumerator<TLeftKey> : QueryOperatorEnumerator<TOutput, Pair<TLeftKey, int>>
        {
            private readonly CancellationToken m_cancellationToken;
            private IEnumerator<TRightInput> m_currentRightSource;
            private IEnumerator<TOutput> m_currentRightSourceAsOutput;
            private readonly QueryOperatorEnumerator<TLeftInput, TLeftKey> m_leftSource;
            private Mutables<TLeftInput, TRightInput, TOutput, TLeftKey> m_mutables;
            private readonly SelectManyQueryOperator<TLeftInput, TRightInput, TOutput> m_selectManyOperator;

            internal SelectManyQueryOperatorEnumerator(QueryOperatorEnumerator<TLeftInput, TLeftKey> leftSource, SelectManyQueryOperator<TLeftInput, TRightInput, TOutput> selectManyOperator, CancellationToken cancellationToken)
            {
                this.m_leftSource = leftSource;
                this.m_selectManyOperator = selectManyOperator;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_leftSource.Dispose();
                if (this.m_currentRightSource != null)
                {
                    this.m_currentRightSource.Dispose();
                }
            }

            internal override bool MoveNext(ref TOutput currentElement, ref Pair<TLeftKey, int> currentKey)
            {
                while (true)
                {
                    if (this.m_currentRightSource == null)
                    {
                        this.m_mutables = new Mutables<TLeftInput, TRightInput, TOutput, TLeftKey>();
                        if ((this.m_mutables.m_lhsCount++ & 0x3f) == 0)
                        {
                            CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                        }
                        if (!this.m_leftSource.MoveNext(ref this.m_mutables.m_currentLeftElement, ref this.m_mutables.m_currentLeftKey))
                        {
                            return false;
                        }
                        this.m_currentRightSource = this.m_selectManyOperator.m_rightChildSelector(this.m_mutables.m_currentLeftElement).GetEnumerator();
                        if (this.m_selectManyOperator.m_resultSelector == null)
                        {
                            this.m_currentRightSourceAsOutput = (IEnumerator<TOutput>) this.m_currentRightSource;
                        }
                    }
                    if (this.m_currentRightSource.MoveNext())
                    {
                        this.m_mutables.m_currentRightSourceIndex++;
                        if (this.m_selectManyOperator.m_resultSelector != null)
                        {
                            currentElement = this.m_selectManyOperator.m_resultSelector(this.m_mutables.m_currentLeftElement, this.m_currentRightSource.Current);
                        }
                        else
                        {
                            currentElement = this.m_currentRightSourceAsOutput.Current;
                        }
                        currentKey = new Pair<TLeftKey, int>(this.m_mutables.m_currentLeftKey, this.m_mutables.m_currentRightSourceIndex);
                        return true;
                    }
                    this.m_currentRightSource.Dispose();
                    this.m_currentRightSource = null;
                    this.m_currentRightSourceAsOutput = null;
                }
            }

            private class Mutables
            {
                internal TLeftInput m_currentLeftElement;
                internal TLeftKey m_currentLeftKey;
                internal int m_currentRightSourceIndex;
                internal int m_lhsCount;

                public Mutables()
                {
                    this.m_currentRightSourceIndex = -1;
                }
            }
        }
    }
}

