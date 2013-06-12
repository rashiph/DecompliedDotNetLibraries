namespace System.Collections.Generic
{
    using System;

    internal interface IArraySortHelper<TKey>
    {
        int BinarySearch(TKey[] keys, int index, int length, TKey value, IComparer<TKey> comparer);
        void Sort(TKey[] keys, int index, int length, IComparer<TKey> comparer);
    }
}

