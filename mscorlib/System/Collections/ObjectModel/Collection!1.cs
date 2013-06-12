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

    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(false), DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    public class Collection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        [NonSerialized]
        private object _syncRoot;
        private IList<T> items;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public Collection()
        {
            this.items = new List<T>();
        }

        public Collection(IList<T> list)
        {
            if (list == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
            }
            this.items = list;
        }

        public void Add(T item)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            int count = this.items.Count;
            this.InsertItem(count, item);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void Clear()
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            this.ClearItems();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual void ClearItems()
        {
            this.items.Clear();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool Contains(T item)
        {
            return this.items.Contains(item);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void CopyTo(T[] array, int index)
        {
            this.items.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(T item)
        {
            return this.items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            if ((index < 0) || (index > this.items.Count))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            }
            this.InsertItem(index, item);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual void InsertItem(int index, T item)
        {
            this.items.Insert(index, item);
        }

        private static bool IsCompatibleObject(object value)
        {
            return ((value is T) || ((value == null) && (default(T) == null)));
        }

        public bool Remove(T item)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            int index = this.items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            this.RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            if ((index < 0) || (index >= this.items.Count))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            this.RemoveItem(index);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual void RemoveItem(int index)
        {
            this.items.RemoveAt(index);
        }

        protected virtual void SetItem(int index, T item)
        {
            this.items[index] = item;
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
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - index) < this.Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            T[] localArray = array as T[];
            if (localArray != null)
            {
                this.items.CopyTo(localArray, index);
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
                int count = this.items.Count;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        objArray[index++] = this.items[i];
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
            return this.items.GetEnumerator();
        }

        int IList.Add(object value)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
            try
            {
                this.Add((T) value);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }
            return (this.Count - 1);
        }

        bool IList.Contains(object value)
        {
            return (Collection<T>.IsCompatibleObject(value) && this.Contains((T) value));
        }

        int IList.IndexOf(object value)
        {
            if (Collection<T>.IsCompatibleObject(value))
            {
                return this.IndexOf((T) value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
            try
            {
                this.Insert(index, (T) value);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }
        }

        void IList.Remove(object value)
        {
            if (this.items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
            }
            if (Collection<T>.IsCompatibleObject(value))
            {
                this.Remove((T) value);
            }
        }

        public int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.items.Count;
            }
        }

        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.items[index];
            }
            set
            {
                if (this.items.IsReadOnly)
                {
                    ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
                }
                if ((index < 0) || (index >= this.items.Count))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                this.SetItem(index, value);
            }
        }

        protected IList<T> Items
        {
            get
            {
                return this.items;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return this.items.IsReadOnly;
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
                    ICollection items = this.items as ICollection;
                    if (items != null)
                    {
                        this._syncRoot = items.SyncRoot;
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
                IList items = this.items as IList;
                if (items != null)
                {
                    return items.IsFixedSize;
                }
                return this.items.IsReadOnly;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.items.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
                try
                {
                    this[index] = (T) value;
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                }
            }
        }
    }
}

