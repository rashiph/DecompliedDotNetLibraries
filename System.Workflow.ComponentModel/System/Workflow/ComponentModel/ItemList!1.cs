namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;

    internal class ItemList<T> : List<T>, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private object owner;

        public event ItemListChangeEventHandler<T> ListChanged;

        internal event ItemListChangeEventHandler<T> ListChanging;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ItemList(object owner)
        {
            this.owner = owner;
        }

        public void Add(T item)
        {
            ((ICollection<T>) this).Add(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(-1, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
            base.AddRange(collection);
            this.FireListChanged(new ItemListChangeEventArgs<T>(base.Count, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
        }

        public void Clear()
        {
            ((ICollection<T>) this).Clear();
        }

        protected virtual void FireListChanged(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanged != null)
            {
                this.ListChanged(this, eventArgs);
            }
        }

        protected virtual void FireListChanging(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanging != null)
            {
                this.ListChanging(this, eventArgs);
            }
        }

        public void Insert(int index, T item)
        {
            ((IList<T>) this).Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
            base.InsertRange(index, collection);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>) this).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>) this).RemoveAt(index);
        }

        void ICollection<T>.Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(base.Count, default(T), item, this.owner, ItemListChangeAction.Add));
            base.Add(item);
            this.FireListChanged(new ItemListChangeEventArgs<T>(base.Count, default(T), item, this.owner, ItemListChangeAction.Add));
        }

        void ICollection<T>.Clear()
        {
            ICollection<T> range = base.GetRange(0, base.Count);
            this.FireListChanging(new ItemListChangeEventArgs<T>(-1, range, null, this.owner, ItemListChangeAction.Remove));
            base.Clear();
            this.FireListChanged(new ItemListChangeEventArgs<T>(-1, range, null, this.owner, ItemListChangeAction.Remove));
        }

        bool ICollection<T>.Contains(T item)
        {
            return base.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            if (base.Contains(item))
            {
                int index = base.IndexOf(item);
                if (index >= 0)
                {
                    this.FireListChanging(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
                    base.Remove(item);
                    this.FireListChanged(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
                    return true;
                }
            }
            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        int IList<T>.IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, default(T), item, this.owner, ItemListChangeAction.Add));
            base.Insert(index, item);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, default(T), item, this.owner, ItemListChangeAction.Add));
        }

        void IList<T>.RemoveAt(int index)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            T removedActivity = base[index];
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, removedActivity, default(T), this.owner, ItemListChangeAction.Remove));
            base.RemoveAt(index);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, removedActivity, default(T), this.owner, ItemListChangeAction.Remove));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < base.Count; i++)
            {
                array.SetValue(this[i], (int) (i + index));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        int IList.Add(object value)
        {
            if (!(value is T))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((ICollection<T>) this).Add((T) value);
            return (base.Count - 1);
        }

        void IList.Clear()
        {
            ((ICollection<T>) this).Clear();
        }

        bool IList.Contains(object value)
        {
            if (!(value is T))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            return ((ICollection<T>) this).Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is T))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            return ((IList<T>) this).IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is T))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((IList<T>) this).Insert(index, (T) value);
        }

        void IList.Remove(object value)
        {
            if (!(value is T))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((ICollection<T>) this).Remove((T) value);
        }

        private bool IsFixedSize
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
                return this[index];
            }
            set
            {
                this[index] = value;
            }
        }

        protected object Owner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.owner;
            }
        }

        int ICollection<T>.Count
        {
            get
            {
                return base.Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("item");
                }
                T removedActivity = base[index];
                this.FireListChanging(new ItemListChangeEventArgs<T>(index, removedActivity, value, this.owner, ItemListChangeAction.Replace));
                base[index] = value;
                this.FireListChanged(new ItemListChangeEventArgs<T>(index, removedActivity, value, this.owner, ItemListChangeAction.Replace));
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

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (!(value is T))
                {
                    throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
                }
                this[index] = (T) value;
            }
        }
    }
}

