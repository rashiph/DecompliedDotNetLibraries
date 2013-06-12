namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal class SortQueryOperatorEnumerator<TInputOutput, TKey, TSortKey> : QueryOperatorEnumerator<TInputOutput, TSortKey>
    {
        private readonly IComparer<TSortKey> m_keyComparer;
        private readonly Func<TInputOutput, TSortKey> m_keySelector;
        private readonly QueryOperatorEnumerator<TInputOutput, TKey> m_source;

        internal SortQueryOperatorEnumerator(QueryOperatorEnumerator<TInputOutput, TKey> source, Func<TInputOutput, TSortKey> keySelector, IComparer<TSortKey> keyComparer)
        {
            this.m_source = source;
            this.m_keySelector = keySelector;
            this.m_keyComparer = keyComparer;
        }

        protected override void Dispose(bool disposing)
        {
            this.m_source.Dispose();
        }

        internal override bool MoveNext(ref TInputOutput currentElement, ref TSortKey currentKey)
        {
            TKey local = default(TKey);
            if (!this.m_source.MoveNext(ref currentElement, ref local))
            {
                return false;
            }
            currentKey = this.m_keySelector(currentElement);
            return true;
        }

        public IComparer<TSortKey> KeyComparer
        {
            get
            {
                return this.m_keyComparer;
            }
        }
    }
}

