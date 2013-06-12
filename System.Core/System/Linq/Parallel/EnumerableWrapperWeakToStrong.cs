namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class EnumerableWrapperWeakToStrong : IEnumerable<object>, IEnumerable
    {
        private readonly IEnumerable m_wrappedEnumerable;

        internal EnumerableWrapperWeakToStrong(IEnumerable wrappedEnumerable)
        {
            this.m_wrappedEnumerable = wrappedEnumerable;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return new WrapperEnumeratorWeakToStrong(this.m_wrappedEnumerable.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class WrapperEnumeratorWeakToStrong : IEnumerator<object>, IDisposable, IEnumerator
        {
            private IEnumerator m_wrappedEnumerator;

            internal WrapperEnumeratorWeakToStrong(IEnumerator wrappedEnumerator)
            {
                this.m_wrappedEnumerator = wrappedEnumerator;
            }

            bool IEnumerator.MoveNext()
            {
                return this.m_wrappedEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                this.m_wrappedEnumerator.Reset();
            }

            void IDisposable.Dispose()
            {
                IDisposable wrappedEnumerator = this.m_wrappedEnumerator as IDisposable;
                if (wrappedEnumerator != null)
                {
                    wrappedEnumerator.Dispose();
                }
            }

            object IEnumerator<object>.Current
            {
                get
                {
                    return this.m_wrappedEnumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.m_wrappedEnumerator.Current;
                }
            }
        }
    }
}

