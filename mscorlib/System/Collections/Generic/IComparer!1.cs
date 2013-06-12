namespace System.Collections.Generic
{
    using System;

    public interface IComparer<in T>
    {
        int Compare(T x, T y);
    }
}

