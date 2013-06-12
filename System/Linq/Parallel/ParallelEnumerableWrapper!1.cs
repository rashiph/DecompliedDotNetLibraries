namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ParallelEnumerableWrapper<T> : ParallelQuery<T>
    {
        private readonly IEnumerable<T> m_wrappedEnumerable;

        internal ParallelEnumerableWrapper(IEnumerable<T> wrappedEnumerable) : base(QuerySettings.Empty)
        {
            this.m_wrappedEnumerable = wrappedEnumerable;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return this.m_wrappedEnumerable.GetEnumerator();
        }

        internal IEnumerable<T> WrappedEnumerable
        {
            get
            {
                return this.m_wrappedEnumerable;
            }
        }
    }
}

