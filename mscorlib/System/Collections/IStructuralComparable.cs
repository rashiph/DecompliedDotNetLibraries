namespace System.Collections
{
    using System;

    public interface IStructuralComparable
    {
        int CompareTo(object other, IComparer comparer);
    }
}

