namespace System.Collections
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IDictionary : ICollection, IEnumerable
    {
        void Add(object key, object value);
        void Clear();
        bool Contains(object key);
        IDictionaryEnumerator GetEnumerator();
        void Remove(object key);

        bool IsFixedSize { get; }

        bool IsReadOnly { get; }

        object this[object key] { get; set; }

        ICollection Keys { get; }

        ICollection Values { get; }
    }
}

