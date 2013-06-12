namespace System.Web.SessionState
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web;

    public class HttpSessionStateContainer : IHttpSessionState
    {
        private bool _abandon;
        private HttpCookieMode _cookieMode;
        private string _id;
        private bool _isReadonly;
        private SessionStateMode _mode;
        private bool _newSession;
        private ISessionStateItemCollection _sessionItems;
        private SessionStateModule _stateModule;
        private HttpStaticObjectsCollection _staticObjects;
        private int _timeout;

        internal HttpSessionStateContainer()
        {
        }

        public HttpSessionStateContainer(string id, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout, bool newSession, HttpCookieMode cookieMode, SessionStateMode mode, bool isReadonly) : this(null, id, sessionItems, staticObjects, timeout, newSession, cookieMode, mode, isReadonly)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
        }

        internal HttpSessionStateContainer(SessionStateModule stateModule, string id, ISessionStateItemCollection sessionItems, HttpStaticObjectsCollection staticObjects, int timeout, bool newSession, HttpCookieMode cookieMode, SessionStateMode mode, bool isReadonly)
        {
            this._stateModule = stateModule;
            this._id = id;
            this._sessionItems = sessionItems;
            this._staticObjects = staticObjects;
            this._timeout = timeout;
            this._newSession = newSession;
            this._cookieMode = cookieMode;
            this._mode = mode;
            this._isReadonly = isReadonly;
        }

        public void Abandon()
        {
            this._abandon = true;
        }

        public void Add(string name, object value)
        {
            this._sessionItems[name] = value;
        }

        public void Clear()
        {
            this._sessionItems.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this._sessionItems.GetEnumerator();
        }

        public void Remove(string name)
        {
            this._sessionItems.Remove(name);
        }

        public void RemoveAll()
        {
            this.Clear();
        }

        public void RemoveAt(int index)
        {
            this._sessionItems.RemoveAt(index);
        }

        public int CodePage
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Response.ContentEncoding.CodePage;
                }
                return Encoding.Default.CodePage;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.ContentEncoding = Encoding.GetEncoding(value);
                }
            }
        }

        public HttpCookieMode CookieMode
        {
            get
            {
                return this._cookieMode;
            }
        }

        public int Count
        {
            get
            {
                return this._sessionItems.Count;
            }
        }

        public bool IsAbandoned
        {
            get
            {
                return this._abandon;
            }
        }

        public bool IsCookieless
        {
            get
            {
                if (this._stateModule != null)
                {
                    return this._stateModule.SessionIDManagerUseCookieless;
                }
                return (this.CookieMode == HttpCookieMode.UseUri);
            }
        }

        public bool IsNewSession
        {
            get
            {
                return this._newSession;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._isReadonly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return this._sessionItems[name];
            }
            set
            {
                this._sessionItems[name] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return this._sessionItems[index];
            }
            set
            {
                this._sessionItems[index] = value;
            }
        }

        public NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return this._sessionItems.Keys;
            }
        }

        public int LCID
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.LCID;
            }
            set
            {
                Thread.CurrentThread.CurrentCulture = HttpServerUtility.CreateReadOnlyCultureInfo(value);
            }
        }

        public SessionStateMode Mode
        {
            get
            {
                return this._mode;
            }
        }

        public string SessionID
        {
            get
            {
                if (this._id == null)
                {
                    this._id = this._stateModule.DelayedGetSessionId();
                }
                return this._id;
            }
        }

        public HttpStaticObjectsCollection StaticObjects
        {
            get
            {
                return this._staticObjects;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Timeout_must_be_positive"));
                }
                if ((value > 0x80520) && ((this.Mode == SessionStateMode.InProc) || (this.Mode == SessionStateMode.StateServer)))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_cache_based_session_timeout"));
                }
                this._timeout = value;
            }
        }
    }
}

