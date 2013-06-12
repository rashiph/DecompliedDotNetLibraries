namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Reflection;

    public interface IOrderedDictionary : IDictionary, ICollection, IEnumerable
    {
        IDictionaryEnumerator GetEnumerator();
        void Insert(int index, object key, object value);
        void RemoveAt(int index);

        object this[int index] { get; set; }
    }
}

