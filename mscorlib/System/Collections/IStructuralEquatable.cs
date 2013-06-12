namespace System.Collections
{
    using System;

    public interface IStructuralEquatable
    {
        bool Equals(object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}

