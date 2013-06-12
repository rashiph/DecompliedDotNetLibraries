namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Security.Permissions;

    internal class SpnDictionary : StringDictionary
    {
        private Hashtable m_SyncTable = Hashtable.Synchronized(new Hashtable());

        internal SpnDictionary()
        {
        }

        public override void Add(string key, string value)
        {
            key = GetCanonicalKey(key);
            this.m_SyncTable.Add(key, value);
        }

        public override void Clear()
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            this.m_SyncTable.Clear();
        }

        public override bool ContainsKey(string key)
        {
            key = GetCanonicalKey(key);
            return this.m_SyncTable.ContainsKey(key);
        }

        public override bool ContainsValue(string value)
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            return this.m_SyncTable.ContainsValue(value);
        }

        public override void CopyTo(Array array, int index)
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            this.m_SyncTable.CopyTo(array, index);
        }

        private static string GetCanonicalKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            try
            {
                key = new Uri(key).GetParts(UriComponents.Path | UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);
                new WebPermission(NetworkAccess.Connect, new Uri(key)).Demand();
            }
            catch (UriFormatException exception)
            {
                throw new ArgumentException(SR.GetString("net_mustbeuri", new object[] { "key" }), "key", exception);
            }
            return key;
        }

        public override IEnumerator GetEnumerator()
        {
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            return this.m_SyncTable.GetEnumerator();
        }

        internal string InternalGet(string canonicalKey)
        {
            int length = 0;
            string str = null;
            lock (this.m_SyncTable.SyncRoot)
            {
                foreach (object obj2 in this.m_SyncTable.Keys)
                {
                    string strA = (string) obj2;
                    if (((strA != null) && (strA.Length > length)) && (string.Compare(strA, 0, canonicalKey, 0, strA.Length, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        length = strA.Length;
                        str = strA;
                    }
                }
            }
            if (str == null)
            {
                return null;
            }
            return (string) this.m_SyncTable[str];
        }

        internal void InternalSet(string canonicalKey, string spn)
        {
            this.m_SyncTable[canonicalKey] = spn;
        }

        public override void Remove(string key)
        {
            key = GetCanonicalKey(key);
            this.m_SyncTable.Remove(key);
        }

        public override int Count
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this.m_SyncTable.Count;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public override string this[string key]
        {
            get
            {
                key = GetCanonicalKey(key);
                return this.InternalGet(key);
            }
            set
            {
                key = GetCanonicalKey(key);
                this.InternalSet(key, value);
            }
        }

        public override ICollection Keys
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this.m_SyncTable.Keys;
            }
        }

        public override object SyncRoot
        {
            [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this.m_SyncTable;
            }
        }

        public override ICollection Values
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this.m_SyncTable.Values;
            }
        }
    }
}

