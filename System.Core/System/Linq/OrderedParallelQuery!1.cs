namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Parallel;

    public class OrderedParallelQuery<TSource> : ParallelQuery<TSource>
    {
        private QueryOperator<TSource> m_sortOp;

        internal OrderedParallelQuery(QueryOperator<TSource> sortOp) : base(sortOp.SpecifiedQuerySettings)
        {
            this.m_sortOp = sortOp;
        }

        public override IEnumerator<TSource> GetEnumerator()
        {
            return this.m_sortOp.GetEnumerator();
        }

        internal IOrderedEnumerable<TSource> OrderedEnumerable
        {
            get
            {
                return (IOrderedEnumerable<TSource>) this.m_sortOp;
            }
        }

        internal QueryOperator<TSource> SortOperator
        {
            get
            {
                return this.m_sortOp;
            }
        }
    }
}

