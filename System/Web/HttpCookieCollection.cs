namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web.Util;

    public sealed class HttpCookieCollection : NameObjectCollectionBase
    {
        private HttpCookie[] _all;
        private string[] _allKeys;
        private bool _changed;
        private HttpResponse _response;

        public HttpCookieCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        internal HttpCookieCollection(HttpResponse response, bool readOnly) : base(StringComparer.OrdinalIgnoreCase)
        {
            this._response = response;
            base.IsReadOnly = readOnly;
        }

        public void Add(HttpCookie cookie)
        {
            if (this._response != null)
            {
                this._response.BeforeCookieCollectionChange();
            }
            this.AddCookie(cookie, true);
            if (this._response != null)
            {
                this._response.OnCookieAdd(cookie);
            }
        }

        internal void AddCookie(HttpCookie cookie, bool append)
        {
            this.ThrowIfMaxHttpCollectionKeysExceeded();
            this._all = null;
            this._allKeys = null;
            if (append)
            {
                cookie.Added = true;
                base.BaseAdd(cookie.Name, cookie);
            }
            else
            {
                if (base.BaseGet(cookie.Name) != null)
                {
                    cookie.Changed = true;
                }
                base.BaseSet(cookie.Name, cookie);
            }
        }

        public void Clear()
        {
            this.Reset();
        }

        public void CopyTo(Array dest, int index)
        {
            if (this._all == null)
            {
                int count = this.Count;
                this._all = new HttpCookie[count];
                for (int i = 0; i < count; i++)
                {
                    this._all[i] = this.Get(i);
                }
            }
            this._all.CopyTo(dest, index);
        }

        public HttpCookie Get(int index)
        {
            return (HttpCookie) base.BaseGet(index);
        }

        public HttpCookie Get(string name)
        {
            HttpCookie cookie = (HttpCookie) base.BaseGet(name);
            if ((cookie == null) && (this._response != null))
            {
                cookie = new HttpCookie(name);
                this.AddCookie(cookie, true);
                this._response.OnCookieAdd(cookie);
            }
            return cookie;
        }

        public string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        internal HttpCookie GetNoCreate(string name)
        {
            return (HttpCookie) base.BaseGet(name);
        }

        public void Remove(string name)
        {
            if (this._response != null)
            {
                this._response.BeforeCookieCollectionChange();
            }
            this.RemoveCookie(name);
            if (this._response != null)
            {
                this._response.OnCookieCollectionChange();
            }
        }

        internal void RemoveCookie(string name)
        {
            this._all = null;
            this._allKeys = null;
            base.BaseRemove(name);
            this._changed = true;
        }

        internal void Reset()
        {
            this._all = null;
            this._allKeys = null;
            base.BaseClear();
            this._changed = true;
        }

        public void Set(HttpCookie cookie)
        {
            if (this._response != null)
            {
                this._response.BeforeCookieCollectionChange();
            }
            this.AddCookie(cookie, false);
            if (this._response != null)
            {
                this._response.OnCookieCollectionChange();
            }
        }

        private void ThrowIfMaxHttpCollectionKeysExceeded()
        {
            if (this.Count >= AppSettings.MaxHttpCollectionKeys)
            {
                throw new InvalidOperationException();
            }
        }

        public string[] AllKeys
        {
            get
            {
                if (this._allKeys == null)
                {
                    this._allKeys = base.BaseGetAllKeys();
                }
                return this._allKeys;
            }
        }

        internal bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        public HttpCookie this[string name]
        {
            get
            {
                return this.Get(name);
            }
        }

        public HttpCookie this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }
    }
}

