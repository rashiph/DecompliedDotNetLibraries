namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    internal class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement>
    {
        internal IComparer<TKey> comparer;
        internal bool descending;
        internal TKey[] keys;
        internal Func<TElement, TKey> keySelector;
        internal EnumerableSorter<TElement> next;

        internal EnumerableSorter(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement> next)
        {
            this.keySelector = keySelector;
            this.comparer = comparer;
            this.descending = descending;
            this.next = next;
        }

        internal override int CompareKeys(int index1, int index2)
        {
            int num = this.comparer.Compare(this.keys[index1], this.keys[index2]);
            if (num == 0)
            {
                if (this.next == null)
                {
                    return (index1 - index2);
                }
                return this.next.CompareKeys(index1, index2);
            }
            if (!this.descending)
            {
                return num;
            }
            return -num;
        }

        internal override void ComputeKeys(TElement[] elements, int count)
        {
            this.keys = new TKey[count];
            for (int i = 0; i < count; i++)
            {
                this.keys[i] = this.keySelector(elements[i]);
            }
            if (this.next != null)
            {
                this.next.ComputeKeys(elements, count);
            }
        }
    }
}

