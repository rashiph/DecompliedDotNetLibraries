namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class QueryExecutionOption<TSource> : QueryOperator<TSource>
    {
        private QueryOperator<TSource> m_child;
        private System.Linq.Parallel.OrdinalIndexState m_indexState;

        internal QueryExecutionOption(QueryOperator<TSource> source, QuerySettings settings) : base(source.OutputOrdered, settings.Merge(source.SpecifiedQuerySettings))
        {
            this.m_child = source;
            this.m_indexState = this.m_child.OrdinalIndexState;
        }

        internal override IEnumerable<TSource> AsSequentialQuery(CancellationToken token)
        {
            return this.m_child.AsSequentialQuery(token);
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
                return this.m_indexState;
            }
        }
    }
}

