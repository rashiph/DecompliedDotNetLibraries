namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class HttpListenerPrefixCollection : ICollection<string>, IEnumerable<string>, IEnumerable
    {
        private HttpListener m_HttpListener;

        internal HttpListenerPrefixCollection(HttpListener listener)
        {
            this.m_HttpListener = listener;
        }

        public void Add(string uriPrefix)
        {
            this.m_HttpListener.AddPrefix(uriPrefix);
        }

        public void Clear()
        {
            this.m_HttpListener.RemoveAll(true);
        }

        public bool Contains(string uriPrefix)
        {
            return this.m_HttpListener.m_UriPrefixes.Contains(uriPrefix);
        }

        public void CopyTo(Array array, int offset)
        {
            this.m_HttpListener.CheckDisposed();
            if (this.Count > array.Length)
            {
                throw new ArgumentOutOfRangeException("array", SR.GetString("net_array_too_small"));
            }
            if ((offset + this.Count) > array.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int num = 0;
            foreach (string str in this.m_HttpListener.m_UriPrefixes.Keys)
            {
                array.SetValue(str, (int) (offset + num++));
            }
        }

        public void CopyTo(string[] array, int offset)
        {
            this.m_HttpListener.CheckDisposed();
            if (this.Count > array.Length)
            {
                throw new ArgumentOutOfRangeException("array", SR.GetString("net_array_too_small"));
            }
            if ((offset + this.Count) > array.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            int num = 0;
            foreach (string str in this.m_HttpListener.m_UriPrefixes.Keys)
            {
                array[offset + num++] = str;
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new ListenerPrefixEnumerator(this.m_HttpListener.m_UriPrefixes.Keys.GetEnumerator());
        }

        public bool Remove(string uriPrefix)
        {
            return this.m_HttpListener.RemovePrefix(uriPrefix);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.m_HttpListener.m_UriPrefixes.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }
    }
}

