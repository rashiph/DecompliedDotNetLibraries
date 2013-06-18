namespace System.Web.SessionState
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    public interface IHttpSessionState
    {
        void Abandon();
        void Add(string name, object value);
        void Clear();
        void CopyTo(Array array, int index);
        IEnumerator GetEnumerator();
        void Remove(string name);
        void RemoveAll();
        void RemoveAt(int index);

        int CodePage { get; set; }

        HttpCookieMode CookieMode { get; }

        int Count { get; }

        bool IsCookieless { get; }

        bool IsNewSession { get; }

        bool IsReadOnly { get; }

        bool IsSynchronized { get; }

        object this[string name] { get; set; }

        object this[int index] { get; set; }

        NameObjectCollectionBase.KeysCollection Keys { get; }

        int LCID { get; set; }

        SessionStateMode Mode { get; }

        string SessionID { get; }

        HttpStaticObjectsCollection StaticObjects { get; }

        object SyncRoot { get; }

        int Timeout { get; set; }
    }
}

