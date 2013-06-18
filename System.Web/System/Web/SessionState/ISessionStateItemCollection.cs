namespace System.Web.SessionState
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;

    public interface ISessionStateItemCollection : ICollection, IEnumerable
    {
        void Clear();
        void Remove(string name);
        void RemoveAt(int index);

        bool Dirty { get; set; }

        object this[string name] { get; set; }

        object this[int index] { get; set; }

        NameObjectCollectionBase.KeysCollection Keys { get; }
    }
}

