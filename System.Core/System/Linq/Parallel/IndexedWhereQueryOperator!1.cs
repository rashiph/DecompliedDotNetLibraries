namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class IndexedWhereQueryOperator<TInputOutput> : UnaryQueryOperator<TInputOutput, TInputOutput>
    {
        private Func<TInputOutput, int, bool> m_predicate;
        private bool m_prematureMerge;

        internal IndexedWhereQueryOperator(IEnumerable<TInputOutput> child, Func<TInputOutput, int, bool> predicate) : base(child)
        {
            this.m_predicate = predicate;
            base.m_outputOrdered = true;
            this.InitOrdinalIndexState();
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            return CancellableEnumerable.Wrap<TInputOutput>(base.Child.AsSequentialQuery(token), token).Where<TInputOutput>(this.m_predicate);
        }

        private void InitOrdinalIndexState()
        {
            if (base.Child.OrdinalIndexState.IsWorseThan(OrdinalIndexState.Correct))
            {
                this.m_prematureMerge = true;
            }
            base.SetOrdinalIndexState(OrdinalIndexState.Increasing);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInputOutput, TInputOutput>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInputOutput, int> stream;
            int partitionCount = inputStream.PartitionCount;
            if (this.m_prematureMerge)
            {
                stream = QueryOperator<TInputOutput>.ExecuteAndCollectResults<TKey>(inputStream, partitionCount, base.Child.OutputOrdered, preferStriping, settings).GetPartitionedStream();
            }
            else
            {
                stream = (PartitionedStream<TInputOutput, int>) inputStream;
            }
            PartitionedStream<TInputOutput, int> partitionedStream = new PartitionedStream<TInputOutput, int>(partitionCount, Util.GetDefaultComparer<int>(), this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new IndexedWhereQueryOperatorEnumerator<TInputOutput>(stream[i], this.m_predicate, settings.CancellationState.MergedCancellationToken);
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

        private class IndexedWhereQueryOperatorEnumerator : QueryOperatorEnumerator<TInputOutput, int>
        {
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount;
            private readonly Func<TInputOutput, int, bool> m_predicate;
            private readonly QueryOperatorEnumerator<TInputOutput, int> m_source;

            internal IndexedWhereQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, int> source, Func<TInputOutput, int, bool> predicate, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref int currentKey)
            {
                if (this.m_outputLoopCount == null)
                {
                    this.m_outputLoopCount = new Shared<int>(0);
                }
                while (this.m_source.MoveNext(ref currentElement, ref currentKey))
                {
                    if ((this.m_outputLoopCount.Value++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    if (this.m_predicate(currentElement, currentKey))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

