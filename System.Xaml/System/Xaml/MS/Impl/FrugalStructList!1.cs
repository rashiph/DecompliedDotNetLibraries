namespace System.Xaml.MS.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xaml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FrugalStructList<T>
    {
        internal FrugalListBase<T> _listStore;
        public FrugalStructList(int size)
        {
            this._listStore = null;
            this.Capacity = size;
        }

        public FrugalStructList(ICollection collection)
        {
            if (collection.Count > 6)
            {
                this._listStore = new ArrayItemList<T>(collection);
            }
            else
            {
                this._listStore = null;
                this.Capacity = collection.Count;
                foreach (T local in collection)
                {
                    this.Add(local);
                }
            }
        }

        public FrugalStructList(ICollection<T> collection)
        {
            if (collection.Count > 6)
            {
                this._listStore = new ArrayItemList<T>(collection);
            }
            else
            {
                this._listStore = null;
                this.Capacity = collection.Count;
                foreach (T local in collection)
                {
                    this.Add(local);
                }
            }
        }

        public int Capacity
        {
            get
            {
                if (this._listStore != null)
                {
                    return this._listStore.Capacity;
                }
                return 0;
            }
            set
            {
                int capacity = 0;
                if (this._listStore != null)
                {
                    capacity = this._listStore.Capacity;
                }
                if (capacity < value)
                {
                    FrugalListBase<T> base2;
                    if (value == 1)
                    {
                        base2 = new SingleItemList<T>();
                    }
                    else if (value <= 3)
                    {
                        base2 = new ThreeItemList<T>();
                    }
                    else if (value <= 6)
                    {
                        base2 = new SixItemList<T>();
                    }
                    else
                    {
                        base2 = new ArrayItemList<T>(value);
                    }
                    if (this._listStore != null)
                    {
                        base2.Promote(this._listStore);
                    }
                    this._listStore = base2;
                }
            }
        }
        public int Count
        {
            get
            {
                if (this._listStore != null)
                {
                    return this._listStore.Count;
                }
                return 0;
            }
        }
        public T this[int index]
        {
            get
            {
                if (((this._listStore == null) || (index >= this._listStore.Count)) || (index < 0))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return this._listStore.EntryAt(index);
            }
            set
            {
                if (((this._listStore == null) || (index >= this._listStore.Count)) || (index < 0))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                this._listStore.SetAt(index, value);
            }
        }
        public int Add(T value)
        {
            if (this._listStore == null)
            {
                this._listStore = new SingleItemList<T>();
            }
            FrugalListStoreState state = this._listStore.Add(value);
            if (state != FrugalListStoreState.Success)
            {
                if (FrugalListStoreState.ThreeItemList != state)
                {
                    if (FrugalListStoreState.SixItemList != state)
                    {
                        if (FrugalListStoreState.Array != state)
                        {
                            throw new InvalidOperationException(System.Xaml.SR.Get("FrugalList_CannotPromoteBeyondArray"));
                        }
                        ArrayItemList<T> list3 = new ArrayItemList<T>(this._listStore.Count + 1);
                        list3.Promote(this._listStore);
                        this._listStore = list3;
                        list3.Add(value);
                        this._listStore = list3;
                    }
                    else
                    {
                        SixItemList<T> list2 = new SixItemList<T>();
                        list2.Promote(this._listStore);
                        this._listStore = list2;
                        list2.Add(value);
                        this._listStore = list2;
                    }
                }
                else
                {
                    ThreeItemList<T> list = new ThreeItemList<T>();
                    list.Promote(this._listStore);
                    list.Add(value);
                    this._listStore = list;
                }
            }
            return (this._listStore.Count - 1);
        }

        public void Clear()
        {
            if (this._listStore != null)
            {
                this._listStore.Clear();
            }
        }

        public bool Contains(T value)
        {
            return (((this._listStore != null) && (this._listStore.Count > 0)) && this._listStore.Contains(value));
        }

        public int IndexOf(T value)
        {
            if ((this._listStore != null) && (this._listStore.Count > 0))
            {
                return this._listStore.IndexOf(value);
            }
            return -1;
        }

        public void Insert(int index, T value)
        {
            if ((index != 0) && (((this._listStore == null) || (index > this._listStore.Count)) || (index < 0)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = 1;
            if ((this._listStore != null) && (this._listStore.Count == this._listStore.Capacity))
            {
                num = this.Capacity + 1;
            }
            this.Capacity = num;
            this._listStore.Insert(index, value);
        }

        public bool Remove(T value)
        {
            return (((this._listStore != null) && (this._listStore.Count > 0)) && this._listStore.Remove(value));
        }

        public void RemoveAt(int index)
        {
            if (((this._listStore == null) || (index >= this._listStore.Count)) || (index < 0))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._listStore.RemoveAt(index);
        }

        public void EnsureIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = (index + 1) - this.Count;
            if (num > 0)
            {
                this.Capacity = index + 1;
                T local = default(T);
                for (int i = 0; i < num; i++)
                {
                    this._listStore.Add(local);
                }
            }
        }

        public T[] ToArray()
        {
            if ((this._listStore != null) && (this._listStore.Count > 0))
            {
                return this._listStore.ToArray();
            }
            return null;
        }

        public void CopyTo(T[] array, int index)
        {
            if ((this._listStore != null) && (this._listStore.Count > 0))
            {
                this._listStore.CopyTo(array, index);
            }
        }

        public FrugalStructList<T> Clone()
        {
            FrugalStructList<T> list = new FrugalStructList<T>();
            if (this._listStore != null)
            {
                list._listStore = (FrugalListBase<T>) this._listStore.Clone();
            }
            return list;
        }
    }
}

