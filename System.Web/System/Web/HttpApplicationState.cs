namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;

    public sealed class HttpApplicationState : NameObjectCollectionBase
    {
        private HttpStaticObjectsCollection _applicationStaticObjects;
        private HttpApplicationStateLock _lock;
        private HttpStaticObjectsCollection _sessionStaticObjects;

        internal HttpApplicationState() : this(null, null)
        {
        }

        internal HttpApplicationState(HttpStaticObjectsCollection applicationStaticObjects, HttpStaticObjectsCollection sessionStaticObjects) : base(Misc.CaseInsensitiveInvariantKeyComparer)
        {
            this._lock = new HttpApplicationStateLock();
            this._applicationStaticObjects = applicationStaticObjects;
            if (this._applicationStaticObjects == null)
            {
                this._applicationStaticObjects = new HttpStaticObjectsCollection();
            }
            this._sessionStaticObjects = sessionStaticObjects;
            if (this._sessionStaticObjects == null)
            {
                this._sessionStaticObjects = new HttpStaticObjectsCollection();
            }
        }

        public void Add(string name, object value)
        {
            this._lock.AcquireWrite();
            try
            {
                base.BaseAdd(name, value);
            }
            finally
            {
                this._lock.ReleaseWrite();
            }
        }

        public void Clear()
        {
            this._lock.AcquireWrite();
            try
            {
                base.BaseClear();
            }
            finally
            {
                this._lock.ReleaseWrite();
            }
        }

        internal void EnsureUnLock()
        {
            this._lock.EnsureReleaseWrite();
        }

        public object Get(int index)
        {
            object obj2 = null;
            this._lock.AcquireRead();
            try
            {
                obj2 = base.BaseGet(index);
            }
            finally
            {
                this._lock.ReleaseRead();
            }
            return obj2;
        }

        public object Get(string name)
        {
            object obj2 = null;
            this._lock.AcquireRead();
            try
            {
                obj2 = base.BaseGet(name);
            }
            finally
            {
                this._lock.ReleaseRead();
            }
            return obj2;
        }

        public string GetKey(int index)
        {
            string str = null;
            this._lock.AcquireRead();
            try
            {
                str = base.BaseGetKey(index);
            }
            finally
            {
                this._lock.ReleaseRead();
            }
            return str;
        }

        public void Lock()
        {
            this._lock.AcquireWrite();
        }

        public void Remove(string name)
        {
            this._lock.AcquireWrite();
            try
            {
                base.BaseRemove(name);
            }
            finally
            {
                this._lock.ReleaseWrite();
            }
        }

        public void RemoveAll()
        {
            this.Clear();
        }

        public void RemoveAt(int index)
        {
            this._lock.AcquireWrite();
            try
            {
                base.BaseRemoveAt(index);
            }
            finally
            {
                this._lock.ReleaseWrite();
            }
        }

        public void Set(string name, object value)
        {
            this._lock.AcquireWrite();
            try
            {
                base.BaseSet(name, value);
            }
            finally
            {
                this._lock.ReleaseWrite();
            }
        }

        public void UnLock()
        {
            this._lock.ReleaseWrite();
        }

        public string[] AllKeys
        {
            get
            {
                string[] strArray = null;
                this._lock.AcquireRead();
                try
                {
                    strArray = base.BaseGetAllKeys();
                }
                finally
                {
                    this._lock.ReleaseRead();
                }
                return strArray;
            }
        }

        public HttpApplicationState Contents
        {
            get
            {
                return this;
            }
        }

        public override int Count
        {
            get
            {
                int count = 0;
                this._lock.AcquireRead();
                try
                {
                    count = base.Count;
                }
                finally
                {
                    this._lock.ReleaseRead();
                }
                return count;
            }
        }

        public object this[string name]
        {
            get
            {
                return this.Get(name);
            }
            set
            {
                this.Set(name, value);
            }
        }

        public object this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }

        internal HttpStaticObjectsCollection SessionStaticObjects
        {
            get
            {
                return this._sessionStaticObjects;
            }
        }

        public HttpStaticObjectsCollection StaticObjects
        {
            get
            {
                return this._applicationStaticObjects;
            }
        }
    }
}

