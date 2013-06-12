namespace System.Net
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization;

    [Serializable]
    public class CookieCollection : ICollection, IEnumerable
    {
        private bool m_has_other_versions;
        [OptionalField]
        private bool m_IsReadOnly;
        private ArrayList m_list;
        private DateTime m_TimeStamp;
        internal int m_version;

        public CookieCollection()
        {
            this.m_list = new ArrayList();
            this.m_TimeStamp = DateTime.MinValue;
            this.m_IsReadOnly = true;
        }

        internal CookieCollection(bool IsReadOnly)
        {
            this.m_list = new ArrayList();
            this.m_TimeStamp = DateTime.MinValue;
            this.m_IsReadOnly = IsReadOnly;
        }

        public void Add(Cookie cookie)
        {
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            this.m_version++;
            int index = this.IndexOf(cookie);
            if (index == -1)
            {
                this.m_list.Add(cookie);
            }
            else
            {
                this.m_list[index] = cookie;
            }
        }

        public void Add(CookieCollection cookies)
        {
            if (cookies == null)
            {
                throw new ArgumentNullException("cookies");
            }
            foreach (Cookie cookie in cookies)
            {
                this.Add(cookie);
            }
        }

        public void CopyTo(Array array, int index)
        {
            this.m_list.CopyTo(array, index);
        }

        public void CopyTo(Cookie[] array, int index)
        {
            this.m_list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return new CookieCollectionEnumerator(this);
        }

        internal int IndexOf(Cookie cookie)
        {
            IComparer comparer = Cookie.GetComparer();
            int num = 0;
            foreach (Cookie cookie2 in this.m_list)
            {
                if (comparer.Compare(cookie, cookie2) == 0)
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        internal int InternalAdd(Cookie cookie, bool isStrict)
        {
            int num = 1;
            if (!isStrict)
            {
                this.m_list.Add(cookie);
            }
            else
            {
                IComparer comparer = Cookie.GetComparer();
                int num2 = 0;
                foreach (Cookie cookie2 in this.m_list)
                {
                    if (comparer.Compare(cookie, cookie2) == 0)
                    {
                        num = 0;
                        if (cookie2.Variant <= cookie.Variant)
                        {
                            this.m_list[num2] = cookie;
                        }
                        break;
                    }
                    num2++;
                }
                if (num2 == this.m_list.Count)
                {
                    this.m_list.Add(cookie);
                }
            }
            if (cookie.Version != 1)
            {
                this.m_has_other_versions = true;
            }
            return num;
        }

        internal void RemoveAt(int idx)
        {
            this.m_list.RemoveAt(idx);
        }

        internal DateTime TimeStamp(Stamp how)
        {
            switch (how)
            {
                case Stamp.Set:
                    this.m_TimeStamp = DateTime.Now;
                    break;

                case Stamp.SetToUnused:
                    this.m_TimeStamp = DateTime.MinValue;
                    break;

                case Stamp.SetToMaxUsed:
                    this.m_TimeStamp = DateTime.MaxValue;
                    break;
            }
            return this.m_TimeStamp;
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        internal bool IsOtherVersionSeen
        {
            get
            {
                return this.m_has_other_versions;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.m_IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public Cookie this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_list.Count))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return (Cookie) this.m_list[index];
            }
        }

        public Cookie this[string name]
        {
            get
            {
                foreach (Cookie cookie in this.m_list)
                {
                    if (string.Compare(cookie.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return cookie;
                    }
                }
                return null;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private class CookieCollectionEnumerator : IEnumerator
        {
            private CookieCollection m_cookies;
            private int m_count;
            private int m_index = -1;
            private int m_version;

            internal CookieCollectionEnumerator(CookieCollection cookies)
            {
                this.m_cookies = cookies;
                this.m_count = cookies.Count;
                this.m_version = cookies.m_version;
            }

            bool IEnumerator.MoveNext()
            {
                if (this.m_version != this.m_cookies.m_version)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                }
                if (++this.m_index < this.m_count)
                {
                    return true;
                }
                this.m_index = this.m_count;
                return false;
            }

            void IEnumerator.Reset()
            {
                this.m_index = -1;
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this.m_index < 0) || (this.m_index >= this.m_count))
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumOpCantHappen"));
                    }
                    if (this.m_version != this.m_cookies.m_version)
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
                    }
                    return this.m_cookies[this.m_index];
                }
            }
        }

        internal enum Stamp
        {
            Check,
            Set,
            SetToUnused,
            SetToMaxUsed
        }
    }
}

