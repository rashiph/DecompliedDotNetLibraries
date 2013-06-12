namespace System.Web.SessionState
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    public sealed class HttpSessionState : ICollection, IEnumerable
    {
        private IHttpSessionState _container;

        internal HttpSessionState(IHttpSessionState container)
        {
            this._container = container;
        }

        public void Abandon()
        {
            this._container.Abandon();
        }

        public void Add(string name, object value)
        {
            this._container[name] = value;
        }

        public void Clear()
        {
            this._container.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            this._container.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._container.GetEnumerator();
        }

        public void Remove(string name)
        {
            this._container.Remove(name);
        }

        public void RemoveAll()
        {
            this.Clear();
        }

        public void RemoveAt(int index)
        {
            this._container.RemoveAt(index);
        }

        public int CodePage
        {
            get
            {
                return this._container.CodePage;
            }
            set
            {
                this._container.CodePage = value;
            }
        }

        internal IHttpSessionState Container
        {
            get
            {
                return this._container;
            }
        }

        public HttpSessionState Contents
        {
            get
            {
                return this;
            }
        }

        public HttpCookieMode CookieMode
        {
            get
            {
                return this._container.CookieMode;
            }
        }

        public int Count
        {
            get
            {
                return this._container.Count;
            }
        }

        public bool IsCookieless
        {
            get
            {
                return this._container.IsCookieless;
            }
        }

        public bool IsNewSession
        {
            get
            {
                return this._container.IsNewSession;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._container.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this._container.IsSynchronized;
            }
        }

        public object this[string name]
        {
            get
            {
                return this._container[name];
            }
            set
            {
                this._container[name] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return this._container[index];
            }
            set
            {
                this._container[index] = value;
            }
        }

        public NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return this._container.Keys;
            }
        }

        public int LCID
        {
            get
            {
                return this._container.LCID;
            }
            set
            {
                this._container.LCID = value;
            }
        }

        public SessionStateMode Mode
        {
            get
            {
                return this._container.Mode;
            }
        }

        public string SessionID
        {
            get
            {
                return this._container.SessionID;
            }
        }

        public HttpStaticObjectsCollection StaticObjects
        {
            get
            {
                return this._container.StaticObjects;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this._container.SyncRoot;
            }
        }

        public int Timeout
        {
            get
            {
                return this._container.Timeout;
            }
            set
            {
                this._container.Timeout = value;
            }
        }
    }
}

