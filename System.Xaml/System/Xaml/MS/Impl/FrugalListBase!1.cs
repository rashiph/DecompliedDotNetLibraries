namespace System.Xaml.MS.Impl
{
    using System;

    internal abstract class FrugalListBase<T>
    {
        protected int _count;

        protected FrugalListBase()
        {
        }

        public abstract FrugalListStoreState Add(T value);
        public abstract void Clear();
        public abstract object Clone();
        public abstract bool Contains(T value);
        public abstract void CopyTo(T[] array, int index);
        public abstract T EntryAt(int index);
        public abstract int IndexOf(T value);
        public abstract void Insert(int index, T value);
        public abstract void Promote(FrugalListBase<T> newList);
        public abstract bool Remove(T value);
        public abstract void RemoveAt(int index);
        public abstract void SetAt(int index, T value);
        public abstract T[] ToArray();

        public abstract int Capacity { get; }

        public int Count
        {
            get
            {
                return this._count;
            }
        }
    }
}

