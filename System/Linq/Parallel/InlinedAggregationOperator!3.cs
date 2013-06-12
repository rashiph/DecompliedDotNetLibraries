namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal abstract class InlinedAggregationOperator<TSource, TIntermediate, TResult> : UnaryQueryOperator<TSource, TIntermediate>
    {
        internal InlinedAggregationOperator(IEnumerable<TSource> child) : base(child)
        {
        }

        internal TResult Aggregate()
        {
            TResult local;
            Exception singularExceptionToThrow = null;
            try
            {
                local = this.InternalAggregate(ref singularExceptionToThrow);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                if (exception2 is AggregateException)
                {
                    throw;
                }
                OperationCanceledException exception3 = exception2 as OperationCanceledException;
                if (((exception3 != null) && (exception3.CancellationToken == base.SpecifiedQuerySettings.CancellationState.ExternalCancellationToken)) && base.SpecifiedQuerySettings.CancellationState.ExternalCancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                throw new AggregateException(new Exception[] { exception2 });
            }
            if (singularExceptionToThrow != null)
            {
                throw singularExceptionToThrow;
            }
            return local;
        }

        internal override IEnumerable<TIntermediate> AsSequentialQuery(CancellationToken token)
        {
            throw new NotSupportedException();
        }

        protected abstract QueryOperatorEnumerator<TIntermediate, int> CreateEnumerator<TKey>(int index, int count, QueryOperatorEnumerator<TSource, TKey> source, object sharedData, CancellationToken cancellationToken);
        protected abstract TResult InternalAggregate(ref Exception singularExceptionToThrow);
        internal override QueryResults<TIntermediate> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TSource, TIntermediate>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TSource, TKey> inputStream, IPartitionedStreamRecipient<TIntermediate> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TIntermediate, int> partitionedStream = new PartitionedStream<TIntermediate, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = this.CreateEnumerator<TKey>(i, partitionCount, inputStream[i], null, settings.CancellationState.MergedCancellationToken);
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
    }
}

