namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class ConcatQueryOperator<TSource> : BinaryQueryOperator<TSource, TSource, TSource>
    {
        private readonly bool m_prematureMergeLeft;
        private readonly bool m_prematureMergeRight;

        internal ConcatQueryOperator(ParallelQuery<TSource> firstChild, ParallelQuery<TSource> secondChild) : base(firstChild, secondChild)
        {
            base.m_outputOrdered = base.LeftChild.OutputOrdered || base.RightChild.OutputOrdered;
            this.m_prematureMergeLeft = base.LeftChild.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
            this.m_prematureMergeRight = base.RightChild.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Increasing);
            if ((base.LeftChild.OrdinalIndexState == OrdinalIndexState.Indexible) && (base.RightChild.OrdinalIndexState == OrdinalIndexState.Indexible))
            {
                base.SetOrdinalIndex(OrdinalIndexState.Indexible);
            }
            else
            {
                base.SetOrdinalIndex(OrdinalIndexState.Shuffled);
            }
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return base.LeftChild.AsSequentialQuery(token).Concat<TSource>(base.RightChild.AsSequentialQuery(token));
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TSource> leftChildQueryResults = base.LeftChild.Open(settings, preferStriping);
            QueryResults<TSource> rightChildQueryResults = base.RightChild.Open(settings, preferStriping);
            return ConcatQueryOperatorResults<TSource>.NewResults(leftChildQueryResults, rightChildQueryResults, (ConcatQueryOperator<TSource>) this, settings, preferStriping);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TSource, TLeftKey> leftStream, PartitionedStream<TSource, TRightKey> rightStream, IPartitionedStreamRecipient<TSource> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TSource, int> stream;
            PartitionedStream<TSource, int> stream2;
            OrdinalIndexState ordinalIndexState = leftStream.OrdinalIndexState;
            int partitionCount = leftStream.PartitionCount;
            if (this.m_prematureMergeLeft)
            {
                stream = QueryOperator<TSource>.ExecuteAndCollectResults<TLeftKey>(leftStream, partitionCount, base.LeftChild.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TSource, int>) leftStream;
            }
            if (this.m_prematureMergeRight)
            {
                stream2 = QueryOperator<TSource>.ExecuteAndCollectResults<TRightKey>(rightStream, partitionCount, base.LeftChild.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream2 = (PartitionedStream<TSource, int>) rightStream;
            }
            IComparer<ConcatKey<int, int>> keyComparer = ConcatKey<int, int>.MakeComparer(stream.KeyComparer, stream2.KeyComparer);
            PartitionedStream<TSource, ConcatKey<int, int>> partitionedStream = new PartitionedStream<TSource, ConcatKey<int, int>>(partitionCount, keyComparer, this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new ConcatQueryOperatorEnumerator<TSource, int, int>(stream[i], stream2[i]);
            }
            outputRecipient.Receive<ConcatKey<int, int>>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                if (!this.m_prematureMergeLeft)
                {
                    return this.m_prematureMergeLeft;
                }
                return true;
            }
        }

        private class ConcatQueryOperatorEnumerator<TLeftKey, TRightKey> : QueryOperatorEnumerator<TSource, ConcatKey<TLeftKey, TRightKey>>
        {
            private bool m_begunSecond;
            private QueryOperatorEnumerator<TSource, TLeftKey> m_firstSource;
            private QueryOperatorEnumerator<TSource, TRightKey> m_secondSource;

            internal ConcatQueryOperatorEnumerator(QueryOperatorEnumerator<TSource, TLeftKey> firstSource, QueryOperatorEnumerator<TSource, TRightKey> secondSource)
            {
                this.m_firstSource = firstSource;
                this.m_secondSource = secondSource;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_firstSource.Dispose();
                this.m_secondSource.Dispose();
            }

            internal override bool MoveNext(ref TSource currentElement, ref ConcatKey<TLeftKey, TRightKey> currentKey)
            {
                if (!this.m_begunSecond)
                {
                    TLeftKey local = default(TLeftKey);
                    if (this.m_firstSource.MoveNext(ref currentElement, ref local))
                    {
                        currentKey = ConcatKey<TLeftKey, TRightKey>.MakeLeft(local);
                        return true;
                    }
                    this.m_begunSecond = true;
                }
                TRightKey local2 = default(TRightKey);
                if (this.m_secondSource.MoveNext(ref currentElement, ref local2))
                {
                    currentKey = ConcatKey<TLeftKey, TRightKey>.MakeRight(local2);
                    return true;
                }
                return false;
            }
        }

        private class ConcatQueryOperatorResults : BinaryQueryOperator<TSource, TSource, TSource>.BinaryQueryOperatorResults
        {
            private ConcatQueryOperator<TSource> m_concatOp;
            private int m_leftChildCount;
            private int m_rightChildCount;

            private ConcatQueryOperatorResults(QueryResults<TSource> leftChildQueryResults, QueryResults<TSource> rightChildQueryResults, ConcatQueryOperator<TSource> concatOp, QuerySettings settings, bool preferStriping) : base(leftChildQueryResults, rightChildQueryResults, concatOp, settings, preferStriping)
            {
                this.m_concatOp = concatOp;
                this.m_leftChildCount = leftChildQueryResults.ElementsCount;
                this.m_rightChildCount = rightChildQueryResults.ElementsCount;
            }

            internal override TSource GetElement(int index)
            {
                if (index < this.m_leftChildCount)
                {
                    return base.m_leftChildQueryResults.GetElement(index);
                }
                return base.m_rightChildQueryResults.GetElement(index - this.m_leftChildCount);
            }

            public static QueryResults<TSource> NewResults(QueryResults<TSource> leftChildQueryResults, QueryResults<TSource> rightChildQueryResults, ConcatQueryOperator<TSource> op, QuerySettings settings, bool preferStriping)
            {
                if (leftChildQueryResults.IsIndexible && rightChildQueryResults.IsIndexible)
                {
                    return new ConcatQueryOperator<TSource>.ConcatQueryOperatorResults(leftChildQueryResults, rightChildQueryResults, op, settings, preferStriping);
                }
                return new BinaryQueryOperator<TSource, TSource, TSource>.BinaryQueryOperatorResults(leftChildQueryResults, rightChildQueryResults, op, settings, preferStriping);
            }

            internal override int ElementsCount
            {
                get
                {
                    return (this.m_leftChildCount + this.m_rightChildCount);
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

