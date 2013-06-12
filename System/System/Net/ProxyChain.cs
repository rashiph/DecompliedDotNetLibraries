namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal abstract class ProxyChain : IEnumerable<Uri>, IEnumerable, IDisposable
    {
        private List<Uri> m_Cache = new List<Uri>();
        private bool m_CacheComplete;
        private Uri m_Destination;
        private System.Net.HttpAbortDelegate m_HttpAbortDelegate;
        private ProxyEnumerator m_MainEnumerator;

        protected ProxyChain(Uri destination)
        {
            this.m_Destination = destination;
        }

        internal virtual void Abort()
        {
        }

        public virtual void Dispose()
        {
        }

        public IEnumerator<Uri> GetEnumerator()
        {
            ProxyEnumerator enumerator = new ProxyEnumerator(this);
            if (this.m_MainEnumerator == null)
            {
                this.m_MainEnumerator = enumerator;
            }
            return enumerator;
        }

        protected abstract bool GetNextProxy(out Uri proxy);
        internal bool HttpAbort(HttpWebRequest request, WebException webException)
        {
            this.Abort();
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal Uri Destination
        {
            get
            {
                return this.m_Destination;
            }
        }

        internal IEnumerator<Uri> Enumerator
        {
            get
            {
                if (this.m_MainEnumerator != null)
                {
                    return this.m_MainEnumerator;
                }
                return this.GetEnumerator();
            }
        }

        internal System.Net.HttpAbortDelegate HttpAbortDelegate
        {
            get
            {
                if (this.m_HttpAbortDelegate == null)
                {
                    this.m_HttpAbortDelegate = new System.Net.HttpAbortDelegate(this.HttpAbort);
                }
                return this.m_HttpAbortDelegate;
            }
        }

        private class ProxyEnumerator : IEnumerator<Uri>, IDisposable, IEnumerator
        {
            private ProxyChain m_Chain;
            private int m_CurrentIndex = -1;
            private bool m_Finished;
            private bool m_TriedDirect;

            internal ProxyEnumerator(ProxyChain chain)
            {
                this.m_Chain = chain;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.m_Finished)
                {
                    return false;
                }
                this.m_CurrentIndex++;
                if (this.m_Chain.m_Cache.Count > this.m_CurrentIndex)
                {
                    return true;
                }
                if (this.m_Chain.m_CacheComplete)
                {
                    this.m_Finished = true;
                    return false;
                }
                lock (this.m_Chain.m_Cache)
                {
                    Uri uri;
                    if (this.m_Chain.m_Cache.Count > this.m_CurrentIndex)
                    {
                        return true;
                    }
                    if (this.m_Chain.m_CacheComplete)
                    {
                        this.m_Finished = true;
                        return false;
                    }
                Label_0092:
                    if (!this.m_Chain.GetNextProxy(out uri))
                    {
                        this.m_Finished = true;
                        this.m_Chain.m_CacheComplete = true;
                        return false;
                    }
                    if (uri == null)
                    {
                        if (this.m_TriedDirect)
                        {
                            goto Label_0092;
                        }
                        this.m_TriedDirect = true;
                    }
                    this.m_Chain.m_Cache.Add(uri);
                    return true;
                }
            }

            public void Reset()
            {
                this.m_Finished = false;
                this.m_CurrentIndex = -1;
            }

            public Uri Current
            {
                get
                {
                    if (this.m_Finished || (this.m_CurrentIndex < 0))
                    {
                        throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.m_Chain.m_Cache[this.m_CurrentIndex];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

