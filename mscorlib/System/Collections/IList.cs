namespace System.Collections
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IList : ICollection, IEnumerable
    {
        int Add(object value);
        void Clear();
        bool Contains(object value);
        int IndexOf(object value);
        void Insert(int index, object value);
        void Remove(object value);
        void RemoveAt(int index);

        bool IsFixedSize { get; }

        bool IsReadOnly { get; }

        object this[int index] { get; set; }
    }
}

