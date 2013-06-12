namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal class ReverseComparer<T> : IComparer<T>
    {
        private IComparer<T> m_comparer;

        internal ReverseComparer(IComparer<T> comparer)
        {
            this.m_comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return -this.m_comparer.Compare(x, y);
        }
    }
}

