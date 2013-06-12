namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class SelectQueryOperator<TInput, TOutput> : UnaryQueryOperator<TInput, TOutput>
    {
        private Func<TInput, TOutput> m_selector;

        internal SelectQueryOperator(IEnumerable<TInput> child, Func<TInput, TOutput> selector) : base(child)
        {
            this.m_selector = selector;
            base.SetOrdinalIndexState(base.Child.OrdinalIndexState);
        }

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            return base.Child.AsSequentialQuery(token).Select<TInput, TOutput>(this.m_selector);
        }

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return SelectQueryOperatorResults<TInput, TOutput>.NewResults(base.Child.Open(settings, preferStriping), (SelectQueryOperator<TInput, TOutput>) this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TOutput, TKey> partitionedStream = new PartitionedStream<TOutput, TKey>(inputStream.PartitionCount, inputStream.KeyComparer, this.OrdinalIndexState);
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                partitionedStream[i] = new SelectQueryOperatorEnumerator<TInput, TOutput, TKey>(inputStream[i], this.m_selector);
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

        private class SelectQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TOutput, TKey>
        {
            private readonly Func<TInput, TOutput> m_selector;
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source;

            internal SelectQueryOperatorEnumerator(QueryOperatorEnumerator<TInput, TKey> source, Func<TInput, TOutput> selector)
            {
                this.m_source = source;
                this.m_selector = selector;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TOutput currentElement, ref TKey currentKey)
            {
                TInput local = default(TInput);
                if (this.m_source.MoveNext(ref local, ref currentKey))
                {
                    currentElement = this.m_selector(local);
                    return true;
                }
                return false;
            }
        }

        private class SelectQueryOperatorResults : UnaryQueryOperator<TInput, TOutput>.UnaryQueryOperatorResults
        {
            private int m_childCount;
            private Func<TInput, TOutput> m_selector;

            private SelectQueryOperatorResults(QueryResults<TInput> childQueryResults, SelectQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping) : base(childQueryResults, op, settings, preferStriping)
            {
                this.m_selector = op.m_selector;
                this.m_childCount = base.m_childQueryResults.ElementsCount;
            }

            internal override TOutput GetElement(int index)
            {
                return this.m_selector(base.m_childQueryResults.GetElement(index));
            }

            public static QueryResults<TOutput> NewResults(QueryResults<TInput> childQueryResults, SelectQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping)
            {
                if (childQueryResults.IsIndexible)
                {
                    return new SelectQueryOperator<TInput, TOutput>.SelectQueryOperatorResults(childQueryResults, op, settings, preferStriping);
                }
                return new UnaryQueryOperator<TInput, TOutput>.UnaryQueryOperatorResults(childQueryResults, op, settings, preferStriping);
            }

            internal override int ElementsCount
            {
                get
                {
                    return this.m_childCount;
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

