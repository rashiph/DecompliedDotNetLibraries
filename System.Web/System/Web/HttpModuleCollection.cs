namespace System.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;

    public sealed class HttpModuleCollection : NameObjectCollectionBase
    {
        private IHttpModule[] _all;
        private string[] _allKeys;

        internal HttpModuleCollection() : base(Misc.CaseInsensitiveInvariantKeyComparer)
        {
        }

        internal void AddModule(string name, IHttpModule m)
        {
            this._all = null;
            this._allKeys = null;
            base.BaseAdd(name, m);
        }

        public void CopyTo(Array dest, int index)
        {
            if (this._all == null)
            {
                int count = this.Count;
                this._all = new IHttpModule[count];
                for (int i = 0; i < count; i++)
                {
                    this._all[i] = this.Get(i);
                }
            }
            if (this._all != null)
            {
                this._all.CopyTo(dest, index);
            }
        }

        public IHttpModule Get(int index)
        {
            return (IHttpModule) base.BaseGet(index);
        }

        public IHttpModule Get(string name)
        {
            return (IHttpModule) base.BaseGet(name);
        }

        public string GetKey(int index)
        {
            return base.BaseGetKey(index);
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

        public IHttpModule this[string name]
        {
            get
            {
                return this.Get(name);
            }
        }

        public IHttpModule this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }
    }
}

