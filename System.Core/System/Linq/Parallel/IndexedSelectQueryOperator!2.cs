namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class IndexedSelectQueryOperator<TInput, TOutput> : UnaryQueryOperator<TInput, TOutput>
    {
        private bool m_prematureMerge;
        private Func<TInput, int, TOutput> m_selector;

        internal IndexedSelectQueryOperator(IEnumerable<TInput> child, Func<TInput, int, TOutput> selector) : base(child)
        {
            this.m_selector = selector;
            base.m_outputOrdered = true;
            this.InitOrdinalIndexState();
        }

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            return base.Child.AsSequentialQuery(token).Select<TInput, TOutput>(this.m_selector);
        }

        private void InitOrdinalIndexState()
        {
            OrdinalIndexState ordinalIndexState = base.Child.OrdinalIndexState;
            if (base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Correct))
            {
                this.m_prematureMerge = true;
                ordinalIndexState = OrdinalIndexState.Correct;
            }
            base.SetOrdinalIndexState(ordinalIndexState);
        }

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return IndexedSelectQueryOperatorResults<TInput, TOutput>.NewResults(base.Child.Open(settings, preferStriping), (IndexedSelectQueryOperator<TInput, TOutput>) this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInput, int> stream;
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMerge)
            {
                stream = QueryOperator<TInput>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TInput, int>) inputStream;
            }
            PartitionedStream<TOutput, int> partitionedStream = new PartitionedStream<TOutput, int>(partitionCount, Util.GetDefaultComparer<int>(), this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new IndexedSelectQueryOperatorEnumerator<TInput, TOutput>(stream[i], this.m_selector);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return this.m_prematureMerge;
            }
        }

        private class IndexedSelectQueryOperatorEnumerator : QueryOperatorEnumerator<TOutput, int>
        {
            private readonly Func<TInput, int, TOutput> m_selector;
            private readonly QueryOperatorEnumerator<TInput, int> m_source;

            internal IndexedSelectQueryOperatorEnumerator(QueryOperatorEnumerator<TInput, int> source, Func<TInput, int, TOutput> selector)
            {
                this.m_source = source;
                this.m_selector = selector;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TOutput currentElement, ref int currentKey)
            {
                TInput local = default(TInput);
                if (this.m_source.MoveNext(ref local, ref currentKey))
                {
                    currentElement = this.m_selector(local, currentKey);
                    return true;
                }
                return false;
            }
        }

        private class IndexedSelectQueryOperatorResults : UnaryQueryOperator<TInput, TOutput>.UnaryQueryOperatorResults
        {
            private int m_childCount;
            private IndexedSelectQueryOperator<TInput, TOutput> m_selectOp;

            private IndexedSelectQueryOperatorResults(QueryResults<TInput> childQueryResults, IndexedSelectQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping) : base(childQueryResults, op, settings, preferStriping)
            {
                this.m_selectOp = op;
                this.m_childCount = base.m_childQueryResults.ElementsCount;
            }

            internal override TOutput GetElement(int index)
            {
                return this.m_selectOp.m_selector(base.m_childQueryResults.GetElement(index), index);
            }

            public static QueryResults<TOutput> NewResults(QueryResults<TInput> childQueryResults, IndexedSelectQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new IndexedSelectQueryOperator<TInput, TOutput>.IndexedSelectQueryOperatorResults(childQueryResults, op, settings, preferStriping);
                }
                return new UnaryQueryOperator<TInput, TOutput>.UnaryQueryOperatorResults(childQueryResults, op, settings, preferStriping);
            }

            internal override int ElementsCount
            {
                get
                {
                    return base.m_childQueryResults.ElementsCount;
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

