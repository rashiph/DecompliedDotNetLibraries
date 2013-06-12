namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal sealed class GroupJoinQueryOperator<TLeftInput, TRightInput, TKey, TOutput> : BinaryQueryOperator<TLeftInput, TRightInput, TOutput>
    {
        private readonly IEqualityComparer<TKey> m_keyComparer;
        private readonly Func<TLeftInput, TKey> m_leftKeySelector;
        private readonly Func<TLeftInput, IEnumerable<TRightInput>, TOutput> m_resultSelector;
        private readonly Func<TRightInput, TKey> m_rightKeySelector;

        internal GroupJoinQueryOperator(ParallelQuery<TLeftInput> left, ParallelQuery<TRightInput> right, Func<TLeftInput, TKey> leftKeySelector, Func<TRightInput, TKey> rightKeySelector, Func<TLeftInput, IEnumerable<TRightInput>, TOutput> resultSelector, IEqualityComparer<TKey> keyComparer) : base(left, right)
        {
            this.m_leftKeySelector = leftKeySelector;
            this.m_rightKeySelector = rightKeySelector;
            this.m_resultSelector = resultSelector;
            this.m_keyComparer = keyComparer;
            base.m_outputOrdered = base.LeftChild.OutputOrdered;
            base.SetOrdinalIndex(OrdinalIndexState.Shuffled);
        }

        internal override IEnumerable<TOutput> AsSequentialQuery(CancellationToken token)
        {
            IEnumerable<TLeftInput> outer = CancellableEnumerable.Wrap<TLeftInput>(base.LeftChild.AsSequentialQuery(token), token);
            IEnumerable<TRightInput> inner = CancellableEnumerable.Wrap<TRightInput>(base.RightChild.AsSequentialQuery(token), token);
            return outer.GroupJoin<TLeftInput, TRightInput, TKey, TOutput>(inner, this.m_leftKeySelector, this.m_rightKeySelector, this.m_resultSelector, this.m_keyComparer);
        }

        internal override QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping)
        {
            QueryResults<TLeftInput> leftChildQueryResults = base.LeftChild.Open(settings, false);
            return new BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults(leftChildQueryResults, base.RightChild.Open(settings, false), this, settings, false);
        }

        public override void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TLeftInput, TLeftKey> leftStream, PartitionedStream<TRightInput, TRightKey> rightStream, IPartitionedStreamRecipient<TOutput> outputRecipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = leftStream.PartitionCount;
            if (base.LeftChild.OutputOrdered)
            {
                this.WrapPartitionedStreamHelper<TLeftKey, TRightKey>(ExchangeUtilities.HashRepartitionOrdered<TLeftInput, TKey, TLeftKey>(leftStream, this.m_leftKeySelector, this.m_keyComparer, null, settings.CancellationState.MergedCancellationToken), rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
            else
            {
                this.WrapPartitionedStreamHelper<int, TRightKey>(ExchangeUtilities.HashRepartition<TLeftInput, TKey, TLeftKey>(leftStream, this.m_leftKeySelector, this.m_keyComparer, null, settings.CancellationState.MergedCancellationToken), rightStream, outputRecipient, partitionCount, settings.CancellationState.MergedCancellationToken);
            }
        }

        private void WrapPartitionedStreamHelper<TLeftKey, TRightKey>(PartitionedStream<Pair<TLeftInput, TKey>, TLeftKey> leftHashStream, PartitionedStream<TRightInput, TRightKey> rightPartitionedStream, IPartitionedStreamRecipient<TOutput> outputRecipient, int partitionCount, CancellationToken cancellationToken)
        {
            PartitionedStream<Pair<TRightInput, TKey>, int> stream = ExchangeUtilities.HashRepartition<TRightInput, TKey, TRightKey>(rightPartitionedStream, this.m_rightKeySelector, this.m_keyComparer, null, cancellationToken);
            PartitionedStream<TOutput, TLeftKey> partitionedStream = new PartitionedStream<TOutput, TLeftKey>(partitionCount, leftHashStream.KeyComparer, this.OrdinalIndexState);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new HashJoinQueryOperatorEnumerator<TLeftInput, TLeftKey, TRightInput, TKey, TOutput>(leftHashStream[i], stream[i], null, this.m_resultSelector, this.m_keyComparer, cancellationToken);
            }
            outputRecipient.Receive<TLeftKey>(partitionedStream);
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

