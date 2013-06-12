namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        void CopyTo(T[] array, int index);
        T[] ToArray();
        bool TryAdd(T item);
        bool TryTake(out T item);
    }
}

