namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal abstract class QueryOperatorEnumerator<TElement, TKey>
    {
        protected QueryOperatorEnumerator()
        {
        }

        internal IEnumerator<TElement> AsClassicEnumerator()
        {
            return new QueryOperatorClassicEnumerator<TElement, TKey>((QueryOperatorEnumerator<TElement, TKey>) this);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        internal abstract bool MoveNext(ref TElement currentElement, ref TKey currentKey);
        internal virtual void Reset()
        {
        }

        private class QueryOperatorClassicEnumerator : IEnumerator<TElement>, IDisposable, IEnumerator
        {
            private TElement m_current;
            private QueryOperatorEnumerator<TElement, TKey> m_operatorEnumerator;

            internal QueryOperatorClassicEnumerator(QueryOperatorEnumerator<TElement, TKey> operatorEnumerator)
            {
                this.m_operatorEnumerator = operatorEnumerator;
            }

            public void Dispose()
            {
                this.m_operatorEnumerator.Dispose();
                this.m_operatorEnumerator = null;
            }

            public bool MoveNext()
            {
                TKey currentKey = default(TKey);
                return this.m_operatorEnumerator.MoveNext(ref this.m_current, ref currentKey);
            }

            public void Reset()
            {
                this.m_operatorEnumerator.Reset();
            }

            public TElement Current
            {
                get
                {
                    return this.m_current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.m_current;
                }
            }
        }
    }
}

