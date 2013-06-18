namespace System.Xaml.MS.Impl
{
    using System;
    using System.Xaml;

    internal sealed class SixItemList<T> : FrugalListBase<T>
    {
        private T _entry0;
        private T _entry1;
        private T _entry2;
        private T _entry3;
        private T _entry4;
        private T _entry5;
        private const int SIZE = 6;

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

                case 3:
                    this._entry3 = value;
                    break;

                case 4:
                    this._entry4 = value;
                    break;

                case 5:
                    this._entry5 = value;
                    break;

                default:
                    return FrugalListStoreState.Array;
            }
            base._count++;
            return FrugalListStoreState.Success;
        }

        public override void Clear()
        {
            this._entry0 = default(T);
            this._entry1 = default(T);
            this._entry2 = default(T);
            this._entry3 = default(T);
            this._entry4 = default(T);
            this._entry5 = default(T);
            base._count = 0;
        }

        public override object Clone()
        {
            SixItemList<T> list = new SixItemList<T>();
            list.Promote((SixItemList<T>) this);
            return list;
        }

        public override bool Contains(T value)
        {
            return (-1 != this.IndexOf(value));
        }

        public override void CopyTo(T[] array, int index)
        {
            if (base._count >= 1)
            {
                array[index] = this._entry0;
                if (base._count >= 2)
                {
                    array[index + 1] = this._entry1;
                    if (base._count >= 3)
                    {
                        array[index + 2] = this._entry2;
                        if (base._count >= 4)
                        {
                            array[index + 3] = this._entry3;
                            if (base._count >= 5)
                            {
                                array[index + 4] = this._entry4;
                                if (base._count == 6)
                                {
                                    array[index + 5] = this._entry5;
                                }
                            }
                        }
                    }
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

                case 3:
                    return this._entry3;

                case 4:
                    return this._entry4;

                case 5:
                    return this._entry5;
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
                if (base._count > 2)
                {
                    if (this._entry2.Equals(value))
                    {
                        return 2;
                    }
                    if (base._count > 3)
                    {
                        if (this._entry3.Equals(value))
                        {
                            return 3;
                        }
                        if (base._count > 4)
                        {
                            if (this._entry4.Equals(value))
                            {
                                return 4;
                            }
                            if ((6 == base._count) && this._entry5.Equals(value))
                            {
                                return 5;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        public override void Insert(int index, T value)
        {
            if (base._count >= 6)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            switch (index)
            {
                case 0:
                    this._entry5 = this._entry4;
                    this._entry4 = this._entry3;
                    this._entry3 = this._entry2;
                    this._entry2 = this._entry1;
                    this._entry1 = this._entry0;
                    this._entry0 = value;
                    break;

                case 1:
                    this._entry5 = this._entry4;
                    this._entry4 = this._entry3;
                    this._entry3 = this._entry2;
                    this._entry2 = this._entry1;
                    this._entry1 = value;
                    break;

                case 2:
                    this._entry5 = this._entry4;
                    this._entry4 = this._entry3;
                    this._entry3 = this._entry2;
                    this._entry2 = value;
                    break;

                case 3:
                    this._entry5 = this._entry4;
                    this._entry4 = this._entry3;
                    this._entry3 = value;
                    break;

                case 4:
                    this._entry5 = this._entry4;
                    this._entry4 = value;
                    break;

                case 5:
                    this._entry5 = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
            base._count++;
        }

        public override void Promote(FrugalListBase<T> oldList)
        {
            int count = oldList.Count;
            if (6 >= count)
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
            throw new ArgumentException(System.Xaml.SR.Get("FrugalList_TargetMapCannotHoldAllData", new object[] { oldList.ToString(), this.ToString() }), "oldList");
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

        public void Promote(ThreeItemList<T> oldList)
        {
            int count = oldList.Count;
            if (6 <= count)
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
                if (base._count > 2)
                {
                    if (this._entry2.Equals(value))
                    {
                        this.RemoveAt(2);
                        return true;
                    }
                    if (base._count > 3)
                    {
                        if (this._entry3.Equals(value))
                        {
                            this.RemoveAt(3);
                            return true;
                        }
                        if (base._count > 4)
                        {
                            if (this._entry4.Equals(value))
                            {
                                this.RemoveAt(4);
                                return true;
                            }
                            if ((6 == base._count) && this._entry5.Equals(value))
                            {
                                this.RemoveAt(5);
                                return true;
                            }
                        }
                    }
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
                    this._entry2 = this._entry3;
                    this._entry3 = this._entry4;
                    this._entry4 = this._entry5;
                    break;

                case 1:
                    this._entry1 = this._entry2;
                    this._entry2 = this._entry3;
                    this._entry3 = this._entry4;
                    this._entry4 = this._entry5;
                    break;

                case 2:
                    this._entry2 = this._entry3;
                    this._entry3 = this._entry4;
                    this._entry4 = this._entry5;
                    break;

                case 3:
                    this._entry3 = this._entry4;
                    this._entry4 = this._entry5;
                    break;

                case 4:
                    this._entry4 = this._entry5;
                    break;

                case 5:
                    break;

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
            this._entry5 = default(T);
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

                case 3:
                    this._entry3 = value;
                    return;

                case 4:
                    this._entry4 = value;
                    return;

                case 5:
                    this._entry5 = value;
                    return;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        private void SetCount(int value)
        {
            if ((value < 0) || (value > 6))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            base._count = value;
        }

        public override T[] ToArray()
        {
            T[] localArray = new T[base._count];
            if (base._count >= 1)
            {
                localArray[0] = this._entry0;
                if (base._count >= 2)
                {
                    localArray[1] = this._entry1;
                    if (base._count >= 3)
                    {
                        localArray[2] = this._entry2;
                        if (base._count >= 4)
                        {
                            localArray[3] = this._entry3;
                            if (base._count >= 5)
                            {
                                localArray[4] = this._entry4;
                                if (base._count == 6)
                                {
                                    localArray[5] = this._entry5;
                                }
                            }
                        }
                    }
                }
            }
            return localArray;
        }

        public override int Capacity
        {
            get
            {
                return 6;
            }
        }
    }
}

