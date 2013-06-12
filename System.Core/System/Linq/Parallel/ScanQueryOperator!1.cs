namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ScanQueryOperator<TElement> : QueryOperator<TElement>
    {
        private readonly IEnumerable<TElement> m_data;

        internal ScanQueryOperator(IEnumerable<TElement> data) : base(false, QuerySettings.Empty)
        {
            ParallelEnumerableWrapper<TElement> wrapper = data as ParallelEnumerableWrapper<TElement>;
            if (wrapper != null)
            {
                data = wrapper.WrappedEnumerable;
            }
            this.m_data = data;
        }

        internal override IEnumerable<TElement> AsSequentialQuery(CancellationToken token)
        {
            return this.m_data;
        }

        internal override IEnumerator<TElement> GetEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            return this.m_data.GetEnumerator();
        }

        internal override QueryResults<TElement> Open(QuerySettings settings, bool preferStriping)
        {
            IList<TElement> data = this.m_data as IList<TElement>;
            if (data != null)
            {
                return new ListQueryResults<TElement>(data, settings.DegreeOfParallelism.GetValueOrDefault(), preferStriping);
            }
            return new ScanEnumerableQueryOperatorResults<TElement>(this.m_data, settings);
        }

        public IEnumerable<TElement> Data
        {
            get
            {
                return this.m_data;
            }
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        internal override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                if (this.m_data is IList<TElement>)
                {
                    return System.Linq.Parallel.OrdinalIndexState.Indexible;
                }
                return System.Linq.Parallel.OrdinalIndexState.Correct;
            }
        }

        private class ScanEnumerableQueryOperatorResults : QueryResults<TElement>
        {
            private IEnumerable<TElement> m_data;
            private QuerySettings m_settings;

            internal ScanEnumerableQueryOperatorResults(IEnumerable<TElement> data, QuerySettings settings)
            {
                this.m_data = data;
                this.m_settings = settings;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TElement> recipient)
            {
                PartitionedStream<TElement, int> partitionedStream = ExchangeUtilities.PartitionDataSource<TElement>(this.m_data, this.m_settings.DegreeOfParallelism.Value, false);
                recipient.Receive<int>(partitionedStream);
            }
        }
    }
}

