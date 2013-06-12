namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>)), ComVisible(false), DebuggerDisplay("Count = {Count}")]
    public class ReadOnlyCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        [NonSerialized]
        private object _syncRoot;
        private IList<T> list;

        public ReadOnlyCollection(IList<T> list)
        {
            if (list == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
            }
            this.list = list;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool Contains(T value)
        {
            return this.list.Contains(value);
        }

        public void CopyTo(T[] array, int index)
        {
            this.list.CopyTo(array, index);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(T value)
        {
            return this.list.IndexOf(value);
        }

        private static bool IsCompatibleObject(object value)
        {
            return ((value is T) || ((value == null) && (default(T) == null)));
        }

        void ICollection<T>.Add(T value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void ICollection<T>.Clear()
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        bool ICollection<T>.Remove(T value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            return false;
        }

        void IList<T>.Insert(int index, T value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList<T>.RemoveAt(int index)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }
            if (array.Rank != 1)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            if (array.GetLowerBound(0) != 0)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
            }
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - index) < this.Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            T[] localArray = array as T[];
            if (localArray != null)
            {
                this.list.CopyTo(localArray, index);
            }
            else
            {
                Type elementType = array.GetType().GetElementType();
                Type c = typeof(T);
                if (!elementType.IsAssignableFrom(c) && !c.IsAssignableFrom(elementType))
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
                object[] objArray = array as object[];
                if (objArray == null)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
                int count = this.list.Count;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        objArray[index++] = this.list[i];
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        int IList.Add(object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            return -1;
        }

        void IList.Clear()
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        bool IList.Contains(object value)
        {
            return (ReadOnlyCollection<T>.IsCompatibleObject(value) && this.Contains((T) value));
        }

        int IList.IndexOf(object value)
        {
            if (ReadOnlyCollection<T>.IsCompatibleObject(value))
            {
                return this.IndexOf((T) value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList.Remove(object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList.RemoveAt(int index)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        public int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.list.Count;
            }
        }

        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.list[index];
            }
        }

        protected IList<T> Items
        {
            get
            {
                return this.list;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this.list[index];
            }
            set
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
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
                if (this._syncRoot == null)
                {
                    ICollection list = this.list as ICollection;
                    if (list != null)
                    {
                        this._syncRoot = list.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                    }
                }
                return this._syncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.list[index];
            }
            set
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
        }
    }
}

