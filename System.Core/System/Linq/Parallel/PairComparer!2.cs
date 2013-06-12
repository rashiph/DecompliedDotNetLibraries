namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal class PairComparer<T, U> : IComparer<Pair<T, U>>
    {
        private IComparer<T> m_comparer1;
        private IComparer<U> m_comparer2;

        public PairComparer(IComparer<T> comparer1, IComparer<U> comparer2)
        {
            this.m_comparer1 = comparer1;
            this.m_comparer2 = comparer2;
        }

        public int Compare(Pair<T, U> x, Pair<T, U> y)
        {
            int num = this.m_comparer1.Compare(x.First, y.First);
            if (num != 0)
            {
                return num;
            }
            return this.m_comparer2.Compare(x.Second, y.Second);
        }
    }
}

