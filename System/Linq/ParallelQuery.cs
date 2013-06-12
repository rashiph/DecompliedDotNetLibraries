namespace System.Linq
{
    using System;
    using System.Collections;
    using System.Linq.Parallel;

    public class ParallelQuery : IEnumerable
    {
        private QuerySettings m_specifiedSettings;

        internal ParallelQuery(QuerySettings specifiedSettings)
        {
            this.m_specifiedSettings = specifiedSettings;
        }

        internal virtual ParallelQuery<TCastTo> Cast<TCastTo>()
        {
            throw new NotSupportedException();
        }

        internal virtual IEnumerator GetEnumeratorUntyped()
        {
            throw new NotSupportedException();
        }

        internal virtual ParallelQuery<TCastTo> OfType<TCastTo>()
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorUntyped();
        }

        internal QuerySettings SpecifiedQuerySettings
        {
            get
            {
                return this.m_specifiedSettings;
            }
        }
    }
}

