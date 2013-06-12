namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class EmptyEnumerator<T> : QueryOperatorEnumerator<T, int>, IEnumerator<T>, IDisposable, IEnumerator
    {
        public bool MoveNext()
        {
            return false;
        }

        internal override bool MoveNext(ref T currentElement, ref int currentKey)
        {
            return false;
        }

        void IEnumerator.Reset()
        {
        }

        public T Current
        {
            get
            {
                return default(T);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }
    }
}

