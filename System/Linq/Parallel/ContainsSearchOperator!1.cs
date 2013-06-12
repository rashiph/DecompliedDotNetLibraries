namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ContainsSearchOperator<TInput> : UnaryQueryOperator<TInput, bool>
    {
        private readonly IEqualityComparer<TInput> m_comparer;
        private readonly TInput m_searchValue;

        internal ContainsSearchOperator(IEnumerable<TInput> child, TInput searchValue, IEqualityComparer<TInput> comparer) : base(child)
        {
            this.m_searchValue = searchValue;
            if (comparer == null)
            {
                this.m_comparer = EqualityComparer<TInput>.Default;
            }
            else
            {
                this.m_comparer = comparer;
            }
        }

        internal bool Aggregate()
        {
            using (IEnumerator<bool> enumerator = this.GetEnumerator(3, true))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current)
                    {
                        return true;
                    }
                }
            }
            return false;
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
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<bool, int> partitionedStream = new PartitionedStream<bool, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            Shared<bool> resultFoundFlag = new Shared<bool>(false);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new ContainsSearchOperatorEnumerator<TInput, TKey>(inputStream[i], this.m_searchValue, this.m_comparer, i, resultFoundFlag, settings.CancellationState.MergedCancellationToken);
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

        private class ContainsSearchOperatorEnumerator<TKey> : QueryOperatorEnumerator<bool, int>
        {
            private CancellationToken m_cancellationToken;
            private readonly IEqualityComparer<TInput> m_comparer;
            private readonly int m_partitionIndex;
            private readonly Shared<bool> m_resultFoundFlag;
            private readonly TInput m_searchValue;
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source;

            internal ContainsSearchOperatorEnumerator(QueryOperatorEnumerator<TInput, TKey> source, TInput searchValue, IEqualityComparer<TInput> comparer, int partitionIndex, Shared<bool> resultFoundFlag, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_searchValue = searchValue;
                this.m_comparer = comparer;
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
                currentElement = false;
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
                    if (this.m_comparer.Equals(local, this.m_searchValue))
                    {
                        this.m_resultFoundFlag.Value = true;
                        currentElement = true;
                        break;
                    }
                }
                while (this.m_source.MoveNext(ref local, ref local2));
                return true;
            }
        }
    }
}

