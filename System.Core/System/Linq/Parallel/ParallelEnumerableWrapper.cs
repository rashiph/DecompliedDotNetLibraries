namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class ParallelEnumerableWrapper : ParallelQuery<object>
    {
        private readonly IEnumerable m_source;

        internal ParallelEnumerableWrapper(IEnumerable source) : base(QuerySettings.Empty)
        {
            this.m_source = source;
        }

        public override IEnumerator<object> GetEnumerator()
        {
            return new EnumerableWrapperWeakToStrong(this.m_source).GetEnumerator();
        }

        internal override IEnumerator GetEnumeratorUntyped()
        {
            return this.m_source.GetEnumerator();
        }
    }
}

