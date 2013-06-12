namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class SZArrayHelper
    {
        private SZArrayHelper()
        {
        }

        private void Add<T>(T value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        private void Clear<T>()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
        }

        private bool Contains<T>(T value)
        {
            return (Array.IndexOf<T>(JitHelpers.UnsafeCast<T[]>(this), value) != -1);
        }

        private void CopyTo<T>(T[] array, int index)
        {
            if ((array != null) && (array.Rank != 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
            }
            T[] sourceArray = JitHelpers.UnsafeCast<T[]>(this);
            Array.Copy(sourceArray, 0, array, index, sourceArray.Length);
        }

        internal int get_Count<T>()
        {
            return JitHelpers.UnsafeCast<T[]>(this).Length;
        }

        private bool get_IsReadOnly<T>()
        {
            return true;
        }

        internal T get_Item<T>(int index)
        {
            T[] localArray = JitHelpers.UnsafeCast<T[]>(this);
            if (index >= localArray.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return localArray[index];
        }

        internal IEnumerator<T> GetEnumerator<T>()
        {
            return new SZGenericArrayEnumerator<T>(JitHelpers.UnsafeCast<T[]>(this));
        }

        private int IndexOf<T>(T value)
        {
            return Array.IndexOf<T>(JitHelpers.UnsafeCast<T[]>(this), value);
        }

        private void Insert<T>(int index, T value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        private bool Remove<T>(T value)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        private void RemoveAt<T>(int index)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
        }

        internal void set_Item<T>(int index, T value)
        {
            T[] localArray = JitHelpers.UnsafeCast<T[]>(this);
            if (index >= localArray.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            localArray[index] = value;
        }

        [Serializable]
        private sealed class SZGenericArrayEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
        {
            private T[] _array;
            private int _endIndex;
            private int _index;

            internal SZGenericArrayEnumerator(T[] array)
            {
                this._array = array;
                this._index = -1;
                this._endIndex = array.Length;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this._index < this._endIndex)
                {
                    this._index++;
                    return (this._index < this._endIndex);
                }
                return false;
            }

            void IEnumerator.Reset()
            {
                this._index = -1;
            }

            public T Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                    }
                    if (this._index >= this._endIndex)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                    }
                    return this._array[this._index];
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
}

