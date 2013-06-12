namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class OrderingQueryOperator<TSource> : QueryOperator<TSource>
    {
        private QueryOperator<TSource> m_child;
        private bool m_orderOn;
        private System.Linq.Parallel.OrdinalIndexState m_ordinalIndexState;

        public OrderingQueryOperator(QueryOperator<TSource> child, bool orderOn) : base(orderOn, child.SpecifiedQuerySettings)
        {
            this.m_child = child;
            this.m_ordinalIndexState = this.m_child.OrdinalIndexState;
            this.m_orderOn = orderOn;
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return this.m_child.AsSequentialQuery(token);
        }

        internal override IEnumerator<TSource> GetEnumerator(ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            ScanQueryOperator<TSource> child = this.m_child as ScanQueryOperator<TSource>;
            if (child != null)
            {
                return child.Data.GetEnumerator();
            }
            return base.GetEnumerator(mergeOptions, suppressOrderPreservation);
        }

        internal override QueryResults<TSource> Open(QuerySettings settings, bool preferStriping)
        {
            return this.m_child.Open(settings, preferStriping);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return this.m_child.LimitsParallelism;
            }
        }

        internal override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return this.m_ordinalIndexState;
            }
        }
    }
}

