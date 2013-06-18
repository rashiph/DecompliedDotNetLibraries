namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class ReadOnlyEmptyCollection<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private static ReadOnlyEmptyCollection<T> instance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private ReadOnlyEmptyCollection()
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
            return new <GetEnumerator>d__0<T>(0) { <>4__this = (ReadOnlyEmptyCollection<T>) this };
        }

        public bool Remove(T item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
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

        public static ReadOnlyEmptyCollection<T> Instance
        {
            get
            {
                if (ReadOnlyEmptyCollection<T>.instance == null)
                {
                    ReadOnlyEmptyCollection<T>.instance = new ReadOnlyEmptyCollection<T>();
                }
                return ReadOnlyEmptyCollection<T>.instance;
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
            public ReadOnlyEmptyCollection<T> <>4__this;

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

