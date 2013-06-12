namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class WhereQueryOperator<TInputOutput> : UnaryQueryOperator<TInputOutput, TInputOutput>
    {
        private Func<TInputOutput, bool> m_predicate;

        internal WhereQueryOperator(IEnumerable<TInputOutput> child, Func<TInputOutput, bool> predicate) : base(child)
        {
            base.SetOrdinalIndexState(base.Child.OrdinalIndexState.Worse(OrdinalIndexState.Increasing));
            this.m_predicate = predicate;
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            return CancellableEnumerable.Wrap<TInputOutput>(base.Child.AsSequentialQuery(token), token).Where<TInputOutput>(this.m_predicate);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInputOutput, TInputOutput>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInputOutput, TKey> partitionedStream = new PartitionedStream<TInputOutput, TKey>(inputStream.PartitionCount, inputStream.KeyComparer, this.OrdinalIndexState);
            for (int i = 0; i < inputStream.PartitionCount; i++)
            {
                partitionedStream[i] = new WhereQueryOperatorEnumerator<TInputOutput, TKey>(inputStream[i], this.m_predicate, settings.CancellationState.MergedCancellationToken);
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

        private class WhereQueryOperatorEnumerator<TKey> : QueryOperatorEnumerator<TInputOutput, TKey>
        {
            private CancellationToken m_cancellationToken;
            private Shared<int> m_outputLoopCount;
            private readonly Func<TInputOutput, bool> m_predicate;
            private readonly QueryOperatorEnumerator<TInputOutput, TKey> m_source;

            internal WhereQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, TKey> source, Func<TInputOutput, bool> predicate, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_predicate = predicate;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TInputOutput currentElement, ref TKey currentKey)
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
                    if (this.m_predicate(currentElement))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

