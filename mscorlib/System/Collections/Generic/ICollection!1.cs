namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    [TypeDependency("System.SZArrayHelper")]
    public interface ICollection<T> : IEnumerable<T>, IEnumerable
    {
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Remove(T item);

        int Count { get; }

        bool IsReadOnly { get; }
    }
}

