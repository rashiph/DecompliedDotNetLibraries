namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class QueryOperator<TOutput> : ParallelQuery<TOutput>
    {
        protected bool m_outputOrdered;

        internal QueryOperator(QuerySettings settings) : this(false, settings)
        {
        }

        internal QueryOperator(bool isOrdered, QuerySettings settings) : base(settings)
        {
            this.m_outputOrdered = isOrdered;
        }

        internal static QueryOperator<TOutput> AsQueryOperator(IEnumerable<TOutput> source)
        {
            QueryOperator<TOutput> @operator = source as QueryOperator<TOutput>;
            if (@operator != null)
            {
                return @operator;
            }
            OrderedParallelQuery<TOutput> query = source as OrderedParallelQuery<TOutput>;
            if (query != null)
            {
                return query.SortOperator;
            }
            return new ScanQueryOperator<TOutput>(source);
        }

        internal abstract IEnumerable<TOutput> AsSequentialQuery(CancellationToken token);
        internal static ListQueryResults<TOutput> ExecuteAndCollectResults<TKey>(PartitionedStream<TOutput, TKey> openedChild, int partitionCount, bool outputOrdered, bool useStriping, QuerySettings settings)
        {
            TaskScheduler taskScheduler = settings.TaskScheduler;
            return new ListQueryResults<TOutput>(MergeExecutor<TOutput>.Execute<TKey>(openedChild, false, ParallelMergeOptions.FullyBuffered, taskScheduler, outputOrdered, settings.CancellationState, settings.QueryId).GetResultsAsArray(), partitionCount, useStriping);
        }

        internal TOutput[] ExecuteAndGetResultsAsArray()
        {
            TOutput[] localArray3;
            QuerySettings querySettings = base.SpecifiedQuerySettings.WithPerExecutionSettings().WithDefaults();
            QueryLifecycle.LogicalQueryExecutionBegin(querySettings.QueryId);
            try
            {
                if ((((ParallelExecutionMode) querySettings.ExecutionMode.Value) == ParallelExecutionMode.Default) && this.LimitsParallelism)
                {
                    IEnumerable<TOutput> source = this.AsSequentialQuery(querySettings.CancellationState.ExternalCancellationToken);
                    IEnumerable<TOutput> introduced13 = CancellableEnumerable.Wrap<TOutput>(source, querySettings.CancellationState.ExternalCancellationToken);
                    return ExceptionAggregator.WrapEnumerable<TOutput>(introduced13, querySettings.CancellationState).ToArray<TOutput>();
                }
                QueryResults<TOutput> queryResults = this.GetQueryResults(querySettings);
                if (queryResults.IsIndexible && this.OutputOrdered)
                {
                    ArrayMergeHelper<TOutput> helper = new ArrayMergeHelper<TOutput>(base.SpecifiedQuerySettings, queryResults);
                    helper.Execute();
                    TOutput[] localArray = helper.GetResultsAsArray();
                    querySettings.CleanStateAtQueryEnd();
                    return localArray;
                }
                PartitionedStreamMerger<TOutput> recipient = new PartitionedStreamMerger<TOutput>(false, ParallelMergeOptions.FullyBuffered, querySettings.TaskScheduler, this.OutputOrdered, querySettings.CancellationState, querySettings.QueryId);
                queryResults.GivePartitionedStream(recipient);
                TOutput[] resultsAsArray = recipient.MergeExecutor.GetResultsAsArray();
                querySettings.CleanStateAtQueryEnd();
                localArray3 = resultsAsArray;
            }
            finally
            {
                QueryLifecycle.LogicalQueryExecutionEnd(querySettings.QueryId);
            }
            return localArray3;
        }

        public override IEnumerator<TOutput> GetEnumerator()
        {
            return this.GetEnumerator(null, false);
        }

        public IEnumerator<TOutput> GetEnumerator(ParallelMergeOptions? mergeOptions)
        {
            return this.GetEnumerator(mergeOptions, false);
        }

        internal virtual IEnumerator<TOutput> GetEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            return new QueryOpeningEnumerator<TOutput>((QueryOperator<TOutput>) this, mergeOptions, suppressOrderPreservation);
        }

        internal IEnumerator<TOutput> GetOpenedEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrder, bool forEffect, QuerySettings querySettings)
        {
            if ((((ParallelExecutionMode) querySettings.ExecutionMode.Value) == ParallelExecutionMode.Default) && this.LimitsParallelism)
            {
                return ExceptionAggregator.WrapEnumerable<TOutput>(this.AsSequentialQuery(querySettings.CancellationState.ExternalCancellationToken), querySettings.CancellationState).GetEnumerator();
            }
            QueryResults<TOutput> queryResults = this.GetQueryResults(querySettings);
            if (!mergeOptions.HasValue)
            {
                mergeOptions = querySettings.MergeOptions;
            }
            if (querySettings.CancellationState.MergedCancellationToken.IsCancellationRequested)
            {
                if (querySettings.CancellationState.ExternalCancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(querySettings.CancellationState.ExternalCancellationToken);
                }
                throw new OperationCanceledException();
            }
            bool outputOrdered = this.OutputOrdered && !suppressOrder;
            PartitionedStreamMerger<TOutput> recipient = new PartitionedStreamMerger<TOutput>(forEffect, mergeOptions.GetValueOrDefault(), querySettings.TaskScheduler, outputOrdered, querySettings.CancellationState, querySettings.QueryId);
            queryResults.GivePartitionedStream(recipient);
            if (forEffect)
            {
                return null;
            }
            return recipient.MergeExecutor.GetEnumerator();
        }

        private QueryResults<TOutput> GetQueryResults(QuerySettings querySettings)
        {
            return this.Open(querySettings, false);
        }

        internal abstract QueryResults<TOutput> Open(QuerySettings settings, bool preferStriping);

        internal abstract bool LimitsParallelism { get; }

        internal abstract System.Linq.Parallel.OrdinalIndexState OrdinalIndexState { get; }

        internal bool OutputOrdered
        {
            get
            {
                return this.m_outputOrdered;
            }
        }
    }
}

