namespace System.Xaml.MS.Impl
{
    using System;
    using System.Xaml;

    internal sealed class ThreeItemList<T> : FrugalListBase<T>
    {
        private T _entry0;
        private T _entry1;
        private T _entry2;
        private const int SIZE = 3;

        public override FrugalListStoreState Add(T value)
        {
            switch (base._count)
            {
                case 0:
                    this._entry0 = value;
                    break;

                case 1:
                    this._entry1 = value;
                    break;

                case 2:
                    this._entry2 = value;
                    break;

                default:
                    return FrugalListStoreState.SixItemList;
            }
            base._count++;
            return FrugalListStoreState.Success;
        }

        public override void Clear()
        {
            this._entry0 = default(T);
            this._entry1 = default(T);
            this._entry2 = default(T);
            base._count = 0;
        }

        public override object Clone()
        {
            ThreeItemList<T> list = new ThreeItemList<T>();
            list.Promote((ThreeItemList<T>) this);
            return list;
        }

        public override bool Contains(T value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override void CopyTo(T[] array, int index)
        {
            array[index] = this._entry0;
            if (base._count >= 2)
            {
                array[index + 1] = this._entry1;
                if (base._count == 3)
                {
                    array[index + 2] = this._entry2;
                }
            }
        }

        public override T EntryAt(int index)
        {
            switch (index)
            {
                case 0:
                    return this._entry0;

                case 1:
                    return this._entry1;

                case 2:
                    return this._entry2;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public override int IndexOf(T value)
        {
            if (this._entry0.Equals(value))
            {
                return 0;
            }
            if (base._count > 1)
            {
                if (this._entry1.Equals(value))
                {
                    return 1;
                }
                if ((3 == base._count) && this._entry2.Equals(value))
                {
                    return 2;
                }
            }
            return -1;
        }

        public override void Insert(int index, T value)
        {
            if (base._count >= 3)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            switch (index)
            {
                case 0:
                    this._entry2 = this._entry1;
                    this._entry1 = this._entry0;
                    this._entry0 = value;
                    break;

                case 1:
                    this._entry2 = this._entry1;
                    this._entry1 = value;
                    break;

                case 2:
                    this._entry2 = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
            base._count++;
        }

        public override void Promote(FrugalListBase<T> oldList)
        {
            int count = oldList.Count;
            if (3 >= count)
            {
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
                }
                throw new ArgumentOutOfRangeException("oldList");
            }
            throw new ArgumentException(System.Xaml.SR.Get("FrugalList_TargetMapCannotHoldAllData", new object[] { oldList.ToString(), this.ToString() }), "oldList");
        }

        public void Promote(SingleItemList<T> oldList)
        {
            this.SetCount(oldList.Count);
            this.SetAt(0, oldList.EntryAt(0));
        }

        public void Promote(ThreeItemList<T> oldList)
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
            }
            throw new ArgumentOutOfRangeException("oldList");
        }

        public override bool Remove(T value)
        {
            if (this._entry0.Equals(value))
            {
                this.RemoveAt(0);
                return true;
            }
            if (base._count > 1)
            {
                if (this._entry1.Equals(value))
                {
                    this.RemoveAt(1);
                    return true;
                }
                if ((3 == base._count) && this._entry2.Equals(value))
                {
                    this.RemoveAt(2);
                    return true;
                }
            }
            return false;
        }

        public override void RemoveAt(int index)
        {
            switch (index)
            {
                case 0:
                    this._entry0 = this._entry1;
                    this._entry1 = this._entry2;
                    break;

                case 1:
                    this._entry1 = this._entry2;
                    break;

                case 2:
                    break;

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
            this._entry2 = default(T);
            base._count--;
        }

        public override void SetAt(int index, T value)
        {
            switch (index)
            {
                case 0:
                    this._entry0 = value;
                    return;

                case 1:
                    this._entry1 = value;
                    return;

                case 2:
                    this._entry2 = value;
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        private void SetCount(int value)
        {
            if ((value < 0) || (value > 3))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            base._count = value;
        }

        public override T[] ToArray()
        {
            T[] localArray = new T[base._count];
            localArray[0] = this._entry0;
            if (base._count >= 2)
            {
                localArray[1] = this._entry1;
                if (base._count == 3)
                {
                    localArray[2] = this._entry2;
                }
            }
            return localArray;
        }

        public override int Capacity
        {
            get
            {
                return 3;
            }
        }
    }
}

