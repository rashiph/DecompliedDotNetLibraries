namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics.Contracts;

    internal abstract class IArraySortHelperContract<TKey> : IArraySortHelper<TKey>
    {
        protected IArraySortHelperContract()
        {
        }

        int IArraySortHelper<TKey>.BinarySearch(TKey[] keys, int index, int length, TKey value, IComparer<TKey> comparer)
        {
            return Contract.Result<int>();
        }

        void IArraySortHelper<TKey>.Sort(TKey[] keys, int index, int length, IComparer<TKey> comparer)
        {
        }
    }
}

