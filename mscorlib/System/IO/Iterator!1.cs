namespace System.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading;

    internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
    {
        internal TSource current;
        internal int state;
        private int threadId;

        public Iterator()
        {
            this.threadId = Thread.CurrentThread.ManagedThreadId;
        }

        [SecuritySafeCritical]
        protected abstract Iterator<TSource> Clone();
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.current = default(TSource);
            this.state = -1;
        }

        [SecuritySafeCritical]
        public IEnumerator<TSource> GetEnumerator()
        {
            if ((this.threadId == Thread.CurrentThread.ManagedThreadId) && (this.state == 0))
            {
                this.state = 1;
                return this;
            }
            Iterator<TSource> iterator = this.Clone();
            iterator.state = 1;
            return iterator;
        }

        [SecuritySafeCritical]
        public abstract bool MoveNext();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        public TSource Current
        {
            get
            {
                return this.current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}

