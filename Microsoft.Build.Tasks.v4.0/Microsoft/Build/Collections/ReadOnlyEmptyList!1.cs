namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Count = 0")]
    internal class ReadOnlyEmptyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private static Microsoft.Build.Collections.ReadOnlyEmptyList<T> instance;

        private ReadOnlyEmptyList()
        {
        }

        public void Add(T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Clear()
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new <GetEnumerator>d__0<T>(0) { <>4__this = (Microsoft.Build.Collections.ReadOnlyEmptyList<T>) this };
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public bool Remove(T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        public void RemoveAt(int index)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        void ICollection.CopyTo(Array array, int index)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public static Microsoft.Build.Collections.ReadOnlyEmptyList<T> Instance
        {
            get
            {
                if (Microsoft.Build.Collections.ReadOnlyEmptyList<T>.instance == null)
                {
                    Microsoft.Build.Collections.ReadOnlyEmptyList<T>.instance = new Microsoft.Build.Collections.ReadOnlyEmptyList<T>();
                }
                return Microsoft.Build.Collections.ReadOnlyEmptyList<T>.instance;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public T this[int index]
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.ThrowArgumentOutOfRange("index");
                return default(T);
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            }
        }

        int ICollection.Count
        {
            get
            {
                return 0;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public Microsoft.Build.Collections.ReadOnlyEmptyList<T> <>4__this;

            [DebuggerHidden]
            public <GetEnumerator>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                if (this.<>1__state == 0)
                {
                    this.<>1__state = -1;
                }
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

