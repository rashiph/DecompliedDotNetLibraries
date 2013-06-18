namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class ReadOnlyEmptyCollection<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private static Microsoft.Build.Collections.ReadOnlyEmptyCollection<T> instance;

        private ReadOnlyEmptyCollection()
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
            return new <GetEnumerator>d__0<T>(0) { <>4__this = (Microsoft.Build.Collections.ReadOnlyEmptyCollection<T>) this };
        }

        public bool Remove(T item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
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

        public static Microsoft.Build.Collections.ReadOnlyEmptyCollection<T> Instance
        {
            get
            {
                if (Microsoft.Build.Collections.ReadOnlyEmptyCollection<T>.instance == null)
                {
                    Microsoft.Build.Collections.ReadOnlyEmptyCollection<T>.instance = new Microsoft.Build.Collections.ReadOnlyEmptyCollection<T>();
                }
                return Microsoft.Build.Collections.ReadOnlyEmptyCollection<T>.instance;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
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
                return this;
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__0 : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private T <>2__current;
            public Microsoft.Build.Collections.ReadOnlyEmptyCollection<T> <>4__this;

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

