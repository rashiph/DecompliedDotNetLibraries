namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal abstract class QueryResults<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        protected QueryResults()
        {
        }

        internal virtual T GetElement(int index)
        {
            throw new NotSupportedException();
        }

        internal abstract void GivePartitionedStream(IPartitionedStreamRecipient<T> recipient);
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            int iteratorVariable0 = 0;
            while (true)
            {
                if (iteratorVariable0 >= this.Count)
                {
                    yield break;
                }
                yield return this[iteratorVariable0];
                iteratorVariable0++;
            }
        }

        int IList<T>.IndexOf(T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.ElementsCount;
            }
        }

        internal virtual int ElementsCount
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        internal virtual bool IsIndexible
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.GetElement(index);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        [CompilerGenerated]
        private sealed class GetEnumerator>d__0 : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public QueryResults<T> <>4__this;
            public int <index>5__1;

            [DebuggerHidden]
            public GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<index>5__1 = 0;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        this.<index>5__1++;
                        break;

                    default:
                        goto Label_006F;
                }
                if (this.<index>5__1 < this.<>4__this.Count)
                {
                    this.<>2__current = this.<>4__this[this.<index>5__1];
                    this.<>1__state = 1;
                    return true;
                }
            Label_006F:
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

