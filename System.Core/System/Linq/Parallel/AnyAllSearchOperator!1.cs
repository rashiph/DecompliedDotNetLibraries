namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class AnyAllSearchOperator<TInput> : UnaryQueryOperator<TInput, bool>
    {
        private readonly Func<TInput, bool> m_predicate;
        private readonly bool m_qualification;

        internal AnyAllSearchOperator(IEnumerable<TInput> child, bool qualification, Func<TInput, bool> predicate) : base(child)
        {
            this.m_qualification = qualification;
            this.m_predicate = predicate;
        }

        internal bool Aggregate()
        {
            using (IEnumerator<bool> enumerator = this.GetEnumerator(3, true))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == this.m_qualification)
                    {
                        return this.m_qualification;
                    }
                }
            }
            return !this.m_qualification;
        }

        internal override IEnumerable<bool> AsSequentialQuery(CancellationToken token)
        {
            throw new NotSupportedException();
        }

        internal override QueryResults<bool> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInput, bool>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<bool> recipient, bool preferStriping, QuerySettings settings)
        {
            Shared<bool> resultFoundFlag = new Shared<bool>(false);
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<bool, int> partitionedStream = new PartitionedStream<bool, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new AnyAllSearchOperatorEnumerator<TInput, TKey>(inputStream[i], this.m_qualification, this.m_predicate, i, resultFoundFlag, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class AnyAllSearchOperatorEnumerator<TKey> : QueryOperatorEnumerator<bool, int>
        {
            private readonly CancellationToken m_cancellationToken;
            private readonly int m_partitionIndex;
            private readonly Func<TInput, bool> m_predicate;
            private readonly bool m_qualification;
            private readonly Shared<bool> m_resultFoundFlag;
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source;

            internal AnyAllSearchOperatorEnumerator(QueryOperatorEnumerator<TInput, TKey> source, bool qualification, Func<TInput, bool> predicate, int partitionIndex, Shared<bool> resultFoundFlag, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_qualification = qualification;
                this.m_predicate = predicate;
                this.m_partitionIndex = partitionIndex;
                this.m_resultFoundFlag = resultFoundFlag;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref bool currentElement, ref int currentKey)
            {
                if (this.m_resultFoundFlag.Value)
                {
                    return false;
                }
                TInput local = default(TInput);
                TKey local2 = default(TKey);
                if (!this.m_source.MoveNext(ref local, ref local2))
                {
                    return false;
                }
                currentElement = !this.m_qualification;
                currentKey = this.m_partitionIndex;
                int num = 0;
                do
                {
                    if ((num++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    if (this.m_resultFoundFlag.Value)
                    {
                        return false;
                    }
                    if (this.m_predicate(local) == this.m_qualification)
                    {
                        this.m_resultFoundFlag.Value = true;
                        currentElement = this.m_qualification;
                        break;
                    }
                }
                while (this.m_source.MoveNext(ref local, ref local2));
                return true;
            }
        }
    }
}

