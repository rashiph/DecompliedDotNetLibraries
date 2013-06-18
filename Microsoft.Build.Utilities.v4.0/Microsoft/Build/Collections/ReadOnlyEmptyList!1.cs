namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Count = 0")]
    internal class ReadOnlyEmptyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private static ReadOnlyEmptyList<T> instance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private ReadOnlyEmptyList()
        {
        }

        public void Add(T item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Clear()
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
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
            return new <GetEnumerator>d__0<T>(0) { <>4__this = (ReadOnlyEmptyList<T>) this };
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public bool Remove(T item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        public void RemoveAt(int index)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        void ICollection.CopyTo(Array array, int index)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

        public static ReadOnlyEmptyList<T> Instance
        {
            get
            {
                if (ReadOnlyEmptyList<T>.instance == null)
                {
                    ReadOnlyEmptyList<T>.instance = new ReadOnlyEmptyList<T>();
                }
                return ReadOnlyEmptyList<T>.instance;
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
                ErrorUtilities.ThrowArgumentOutOfRange("index");
                return default(T);
            }
            set
            {
                ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
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
            public ReadOnlyEmptyList<T> <>4__this;

            [DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

