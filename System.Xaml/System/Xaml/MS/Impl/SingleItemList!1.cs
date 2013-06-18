namespace System.Xaml.MS.Impl
{
    using System;
    using System.Xaml;

    internal sealed class SingleItemList<T> : FrugalListBase<T>
    {
        private T _loneEntry;
        private const int SIZE = 1;

        public override FrugalListStoreState Add(T value)
        {
            if (base._count == 0)
            {
                this._loneEntry = value;
                base._count++;
                return FrugalListStoreState.Success;
            }
            return FrugalListStoreState.ThreeItemList;
        }

        public override void Clear()
        {
            this._loneEntry = default(T);
            base._count = 0;
        }

        public override object Clone()
        {
            SingleItemList<T> list = new SingleItemList<T>();
            list.Promote((SingleItemList<T>) this);
            return list;
        }

        public override bool Contains(T value)
        {
            return this._loneEntry.Equals(value);
        }

        public override void CopyTo(T[] array, int index)
        {
            array[index] = this._loneEntry;
        }

        public override T EntryAt(int index)
        {
            return this._loneEntry;
        }

        public override int IndexOf(T value)
        {
            if (this._loneEntry.Equals(value))
            {
                return 0;
            }
            return -1;
        }

        public override void Insert(int index, T value)
        {
            if ((base._count >= 1) || (index >= 1))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._loneEntry = value;
            base._count++;
        }

        public override void Promote(FrugalListBase<T> oldList)
        {
            if (1 != oldList.Count)
            {
                throw new ArgumentException(System.Xaml.SR.Get("FrugalList_TargetMapCannotHoldAllData", new object[] { oldList.ToString(), this.ToString() }), "oldList");
            }
            this.SetCount(1);
            this.SetAt(0, oldList.EntryAt(0));
        }

        public void Promote(SingleItemList<T> oldList)
        {
            this.SetCount(oldList.Count);
            this.SetAt(0, oldList.EntryAt(0));
        }

        public override bool Remove(T value)
        {
            if (this._loneEntry.Equals(value))
            {
                this._loneEntry = default(T);
                base._count--;
                return true;
            }
            return false;
        }

        public override void RemoveAt(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this._loneEntry = default(T);
            base._count--;
        }

        public override void SetAt(int index, T value)
        {
            this._loneEntry = value;
        }

        private void SetCount(int value)
        {
            if ((value < 0) || (value > 1))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            base._count = value;
        }

        public override T[] ToArray()
        {
            return new T[] { this._loneEntry };
        }

        public override int Capacity
        {
            get
            {
                return 1;
            }
        }
    }
}

