namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ICollection : IEnumerable
    {
        void CopyTo(Array array, int index);

        int Count { get; }

        bool IsSynchronized { get; }

        object SyncRoot { get; }
    }
}

