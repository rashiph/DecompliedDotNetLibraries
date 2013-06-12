namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class SortQueryOperator<TInputOutput, TSortKey> : UnaryQueryOperator<TInputOutput, TInputOutput>, IOrderedEnumerable<TInputOutput>, IEnumerable<TInputOutput>, IEnumerable
    {
        private readonly IComparer<TSortKey> m_comparer;
        private readonly Func<TInputOutput, TSortKey> m_keySelector;

        internal SortQueryOperator(IEnumerable<TInputOutput> source, Func<TInputOutput, TSortKey> keySelector, IComparer<TSortKey> comparer, bool descending) : base(source, true)
        {
            this.m_keySelector = keySelector;
            if (comparer == null)
            {
                this.m_comparer = Util.GetDefaultComparer<TSortKey>();
            }
            else
            {
                this.m_comparer = comparer;
            }
            if (descending)
            {
                this.m_comparer = new ReverseComparer<TSortKey>(this.m_comparer);
            }
            base.SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<TInputOutput> AsSequentialQuery(CancellationToken token)
        {
            return CancellableEnumerable.Wrap<TInputOutput>(base.Child.AsSequentialQuery(token), token).OrderBy<TInputOutput, TSortKey>(this.m_keySelector, this.m_comparer);
        }

        internal override QueryResults<TInputOutput> Open(QuerySettings settings, bool preferStriping)
        {
            return new SortQueryOperatorResults<TInputOutput, TSortKey>(base.Child.Open(settings, false), (SortQueryOperator<TInputOutput, TSortKey>) this, settings, preferStriping);
        }

        IOrderedEnumerable<TInputOutput> IOrderedEnumerable<TInputOutput>.CreateOrderedEnumerable<TKey2>(Func<TInputOutput, TKey2> key2Selector, IComparer<TKey2> key2Comparer, bool descending)
        {
            <>c__DisplayClass1<TInputOutput, TSortKey, TKey2> class2;
            key2Comparer = key2Comparer ?? Util.GetDefaultComparer<TKey2>();
            if (descending)
            {
                key2Comparer = new ReverseComparer<TKey2>(key2Comparer);
            }
            IComparer<Pair<TSortKey, TKey2>> comparer = new PairComparer<TSortKey, TKey2>(this.m_comparer, key2Comparer);
            return new SortQueryOperator<TInputOutput, Pair<TSortKey, TKey2>>(base.Child, new Func<TInputOutput, Pair<TSortKey, TKey2>>(class2.<System.Linq.IOrderedEnumerable<TInputOutput>.CreateOrderedEnumerable>b__0), comparer, false);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInputOutput, TKey> inputStream, IPartitionedStreamRecipient<TInputOutput> recipient, bool preferStriping, QuerySettings settings)
        {
            PartitionedStream<TInputOutput, TSortKey> partitionedStream = new PartitionedStream<TInputOutput, TSortKey>(inputStream.PartitionCount, this.m_comparer, this.OrdinalIndexState);
            for (int i = 0; i < partitionedStream.PartitionCount; i++)
            {
                partitionedStream[i] = new SortQueryOperatorEnumerator<TInputOutput, TKey, TSortKey>(inputStream[i], this.m_keySelector, this.m_comparer);
            }
            recipient.Receive<TSortKey>(partitionedStream);
        }

        internal IComparer<TSortKey> KeyComparer
        {
            get
            {
                return this.m_comparer;
            }
        }

        internal Func<TInputOutput, TSortKey> KeySelector
        {
            get
            {
                return this.m_keySelector;
            }
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }
    }
}

