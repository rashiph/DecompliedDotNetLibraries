namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class GroupByQueryOperator<TSource, TGroupKey, TElement> : UnaryQueryOperator<TSource, IGrouping<TGroupKey, TElement>>
    {
        private readonly Func<TSource, TElement> m_elementSelector;
        private readonly IEqualityComparer<TGroupKey> m_keyComparer;
        private readonly Func<TSource, TGroupKey> m_keySelector;

        internal GroupByQueryOperator(IEnumerable<TSource> child, Func<TSource, TGroupKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TGroupKey> keyComparer) : base(child)
        {
            this.m_keySelector = keySelector;
            this.m_elementSelector = elementSelector;
            this.m_keyComparer = keyComparer;
            base.SetOrdinalIndexState(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<IGrouping<TGroupKey, TElement>> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TSource> source = CancellableEnumerable.Wrap<TSource>(base.Child.AsSequentialQuery(token), token);
            if (this.m_elementSelector == null)
            {
                return (IEnumerable<IGrouping<TGroupKey, TElement>>) source.GroupBy<TSource, TGroupKey>(this.m_keySelector, this.m_keyComparer);
            }
            return source.GroupBy<TSource, TGroupKey, TElement>(this.m_keySelector, this.m_elementSelector, this.m_keyComparer);
        }

        internal override QueryResults<IGrouping<TGroupKey, TElement>> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TSource, IGrouping<TGroupKey, TElement>>.UnaryQueryOperatorResults(base.Child.Open(settings, false), this, settings, false);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<IGrouping<TGroupKey, TElement>> recipient, bool preferStriping, QuerySettings settings)
        {
            if (base.Child.OutputOrdered)
            {
                this.WrapPartitionedStreamHelperOrdered<TKey>(ExchangeUtilities.HashRepartitionOrdered<TSource, TGroupKey, TKey>(inputStream, this.m_keySelector, this.m_keyComparer, null, settings.CancellationState.MergedCancellationToken), recipient, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                this.WrapPartitionedStreamHelper<TKey, int>(ExchangeUtilities.HashRepartition<TSource, TGroupKey, TKey>(inputStream, this.m_keySelector, this.m_keyComparer, null, settings.CancellationState.MergedCancellationToken), recipient, settings.CancellationState.MergedCancellationToken);
            }
        }

        private void WrapPartitionedStreamHelper<TIgnoreKey, TKey>(PartitionedStream<Pair<TSource, TGroupKey>, TKey> hashStream, IPartitionedStreamRecipient<IGrouping<TGroupKey, TElement>> recipient, CancellationToken cancellationToken)
        {
            int partitionCount = hashStream.PartitionCount;
            PartitionedStream<IGrouping<TGroupKey, TElement>, TKey> partitionedStream = new PartitionedStream<IGrouping<TGroupKey, TElement>, TKey>(partitionCount, hashStream.KeyComparer, OrdinalIndexState.Shuffled);
            for (int i = 0; i < partitionCount; i++)
            {
                if (this.m_elementSelector == null)
                {
                    GroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TKey> enumerator = new GroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TKey>(hashStream[i], this.m_keyComparer, cancellationToken);
                    partitionedStream[i] = (QueryOperatorEnumerator<IGrouping<TGroupKey, TElement>, TKey>) enumerator;
                }
                else
                {
                    partitionedStream[i] = new GroupByElementSelectorQueryOperatorEnumerator<TSource, TGroupKey, TElement, TKey>(hashStream[i], this.m_keyComparer, this.m_elementSelector, cancellationToken);
                }
            }
            recipient.Receive<TKey>(partitionedStream);
        }

        private void WrapPartitionedStreamHelperOrdered<TKey>(PartitionedStream<Pair<TSource, TGroupKey>, TKey> hashStream, IPartitionedStreamRecipient<IGrouping<TGroupKey, TElement>> recipient, CancellationToken cancellationToken)
        {
            int partitionCount = hashStream.PartitionCount;
            PartitionedStream<IGrouping<TGroupKey, TElement>, TKey> partitionedStream = new PartitionedStream<IGrouping<TGroupKey, TElement>, TKey>(partitionCount, hashStream.KeyComparer, OrdinalIndexState.Shuffled);
            IComparer<TKey> keyComparer = hashStream.KeyComparer;
            for (int i = 0; i < partitionCount; i++)
            {
                if (this.m_elementSelector == null)
                {
                    OrderedGroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TKey> enumerator = new OrderedGroupByIdentityQueryOperatorEnumerator<TSource, TGroupKey, TKey>(hashStream[i], this.m_keySelector, this.m_keyComparer, keyComparer, cancellationToken);
                    partitionedStream[i] = (QueryOperatorEnumerator<IGrouping<TGroupKey, TElement>, TKey>) enumerator;
                }
                else
                {
                    partitionedStream[i] = new OrderedGroupByElementSelectorQueryOperatorEnumerator<TSource, TGroupKey, TElement, TKey>(hashStream[i], this.m_keySelector, this.m_elementSelector, this.m_keyComparer, keyComparer, cancellationToken);
                }
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
    }
}

