namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Parallel;

    public class ParallelQuery<TSource> : ParallelQuery, IEnumerable<TSource>, IEnumerable
    {
        internal ParallelQuery(QuerySettings settings) : base(settings)
        {
        }

        internal sealed override ParallelQuery<TCastTo> Cast<TCastTo>()
        {
            return ((ParallelQuery<TSource>) this).Select<TSource, TCastTo>(((Func<TSource, TCastTo>) (elem => elem)));
        }

        public virtual IEnumerator<TSource> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        internal override IEnumerator GetEnumeratorUntyped()
        {
            return this.GetEnumerator();
        }

        internal sealed override ParallelQuery<TCastTo> OfType<TCastTo>()
        {
            return ((ParallelQuery<TSource>) this).Where<TSource>(((Func<TSource, bool>) (elem => (elem is TCastTo)))).Select<TSource, TCastTo>(((Func<TSource, TCastTo>) (elem => elem)));
        }
    }
}

