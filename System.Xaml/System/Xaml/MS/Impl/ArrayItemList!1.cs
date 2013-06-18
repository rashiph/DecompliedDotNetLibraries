namespace System.Xaml.MS.Impl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xaml;

    internal sealed class ArrayItemList<T> : FrugalListBase<T>
    {
        private T[] _entries;
        private const int GROWTH = 3;
        private const int LARGEGROWTH = 0x12;
        private const int MINSIZE = 9;

        public ArrayItemList()
        {
        }

        public ArrayItemList(ICollection<T> collection)
        {
            if (collection != null)
            {
                base._count = collection.Count;
                this._entries = new T[base._count];
                collection.CopyTo(this._entries, 0);
            }
        }

        public ArrayItemList(ICollection collection)
        {
            if (collection != null)
            {
                base._count = collection.Count;
                this._entries = new T[base._count];
                collection.CopyTo(this._entries, 0);
            }
        }

        public ArrayItemList(int size)
        {
            size += 2;
            size -= size % 3;
            this._entries = new T[size];
        }

        public override FrugalListStoreState Add(T value)
        {
            if ((this._entries != null) && (base._count < this._entries.Length))
            {
                this._entries[base._count] = value;
                base._count++;
            }
            else
            {
                if (this._entries != null)
                {
                    int length = this._entries.Length;
                    if (length < 0x12)
                    {
                        length += 3;
                    }
                    else
                    {
                        length += length >> 2;
                    }
                    T[] destinationArray = new T[length];
                    Array.Copy(this._entries, 0, destinationArray, 0, this._entries.Length);
                    this._entries = destinationArray;
                }
                else
                {
                    this._entries = new T[9];
                }
                this._entries[base._count] = value;
                base._count++;
            }
            return FrugalListStoreState.Success;
        }

        public override void Clear()
        {
            for (int i = 0; i < base._count; i++)
            {
                this._entries[i] = default(T);
            }
            base._count = 0;
        }

        public override object Clone()
        {
            ArrayItemList<T> list = new ArrayItemList<T>(this.Capacity);
            list.Promote((ArrayItemList<T>) this);
            return list;
        }

        public override bool Contains(T value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override void CopyTo(T[] array, int index)
        {
            for (int i = 0; i < base._count; i++)
            {
                array[index + i] = this._entries[i];
            }
        }

        public override T EntryAt(int index)
        {
            return this._entries[index];
        }

        public override int IndexOf(T value)
        {
            for (int i = 0; i < base._count; i++)
            {
                if (this._entries[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public override void Insert(int index, T value)
        {
            if ((this._entries == null) || (base._count >= this._entries.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            Array.Copy(this._entries, index, this._entries, index + 1, base._count - index);
            this._entries[index] = value;
            base._count++;
        }

        public void Promote(ArrayItemList<T> oldList)
        {
            int count = oldList.Count;
            if (this._entries.Length < count)
            {
                throw new ArgumentException(System.Xaml.SR.Get("FrugalList_TargetMapCannotHoldAllData", new object[] { oldList.ToString(), this.ToString() }), "oldList");
            }
            this.SetCount(oldList.Count);
            for (int i = 0; i < count; i++)
            {
                this.SetAt(i, oldList.EntryAt(i));
            }
        }

        public override void Promote(FrugalListBase<T> oldList)
        {
            for (int i = 0; i < oldList.Count; i++)
            {
                if (this.Add(oldList.EntryAt(i)) != FrugalListStoreState.Success)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("FrugalList_TargetMapCannotHoldAllData", new object[] { oldList.ToString(), this.ToString() }), "oldList");
                }
            }
        }

        public void Promote(SixItemList<T> oldList)
        {
            int count = oldList.Count;
            this.SetCount(oldList.Count);
            switch (count)
            {
                case 0:
                    return;

                case 1:
                    this.SetAt(0, oldList.EntryAt(0));
                    return;

                case 2:
                    this.SetAt(0, oldList.EntryAt(0));
                    this.SetAt(1, oldList.EntryAt(1));
                    return;

                case 3:
                    this.SetAt(0, oldList.EntryAt(0));
                    this.SetAt(1, oldList.EntryAt(1));
                    this.SetAt(2, oldList.EntryAt(2));
                    return;

                case 4:
                    this.SetAt(0, oldList.EntryAt(0));
                    this.SetAt(1, oldList.EntryAt(1));
                    this.SetAt(2, oldList.EntryAt(2));
                    this.SetAt(3, oldList.EntryAt(3));
                    return;

                case 5:
                    this.SetAt(0, oldList.EntryAt(0));
                    this.SetAt(1, oldList.EntryAt(1));
                    this.SetAt(2, oldList.EntryAt(2));
                    this.SetAt(3, oldList.EntryAt(3));
                    this.SetAt(4, oldList.EntryAt(4));
                    return;

                case 6:
                    this.SetAt(0, oldList.EntryAt(0));
                    this.SetAt(1, oldList.EntryAt(1));
                    this.SetAt(2, oldList.EntryAt(2));
                    this.SetAt(3, oldList.EntryAt(3));
                    this.SetAt(4, oldList.EntryAt(4));
                    this.SetAt(5, oldList.EntryAt(5));
                    return;
            }
            throw new ArgumentOutOfRangeException("oldList");
        }

        public override bool Remove(T value)
        {
            for (int i = 0; i < base._count; i++)
            {
                if (this._entries[i].Equals(value))
                {
                    this.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public override void RemoveAt(int index)
        {
            int length = (base._count - index) - 1;
            if (length > 0)
            {
                Array.Copy(this._entries, index + 1, this._entries, index, length);
            }
            this._entries[base._count - 1] = default(T);
            base._count--;
        }

        public override void SetAt(int index, T value)
        {
            this._entries[index] = value;
        }

        private void SetCount(int value)
        {
            if ((value < 0) || (value > this._entries.Length))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            base._count = value;
        }

        public override T[] ToArray()
        {
            T[] localArray = new T[base._count];
            for (int i = 0; i < base._count; i++)
            {
                localArray[i] = this._entries[i];
            }
            return localArray;
        }

        public override int Capacity
        {
            get
            {
                if (this._entries != null)
                {
                    return this._entries.Length;
                }
                return 0;
            }
        }
    }
}

