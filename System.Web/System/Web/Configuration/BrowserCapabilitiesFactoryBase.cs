namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    public class BrowserCapabilitiesFactoryBase
    {
        private IDictionary _browserElements;
        private object _lock = new object();
        private IDictionary _matchedHeaders;

        internal int CompareFilters(string filter1, string filter2)
        {
            bool flag = string.IsNullOrEmpty(filter1);
            bool flag2 = string.IsNullOrEmpty(filter2);
            IDictionary browserElements = this.BrowserElements;
            bool flag3 = browserElements.Contains(filter1) || flag;
            bool flag4 = browserElements.Contains(filter2) || flag2;
            if (!flag3)
            {
                if (!flag4)
                {
                    return 0;
                }
                return -1;
            }
            if (!flag4)
            {
                return 1;
            }
            if (flag && !flag2)
            {
                return 1;
            }
            if (flag2 && !flag)
            {
                return -1;
            }
            if (flag && flag2)
            {
                return 0;
            }
            int third = (int) ((Triplet) this.BrowserElements[filter1]).Third;
            int num2 = (int) ((Triplet) this.BrowserElements[filter2]).Third;
            return (num2 - third);
        }

        public virtual void ConfigureBrowserCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        public virtual void ConfigureCustomCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps)
        {
        }

        internal static string GetBrowserCapKey(IDictionary headers, HttpRequest request)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in headers.Keys)
            {
                if (str.Length == 0)
                {
                    builder.Append(HttpCapabilitiesDefaultProvider.GetUserAgent(request));
                }
                else
                {
                    builder.Append(request.Headers[str]);
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }

        internal HttpBrowserCapabilities GetHttpBrowserCapabilities(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            NameValueCollection headers = request.Headers;
            HttpBrowserCapabilities browserCaps = new HttpBrowserCapabilities();
            Hashtable hashtable = new Hashtable(180, StringComparer.OrdinalIgnoreCase);
            hashtable[string.Empty] = HttpCapabilitiesDefaultProvider.GetUserAgent(request);
            browserCaps.Capabilities = hashtable;
            this.ConfigureBrowserCapabilities(headers, browserCaps);
            this.ConfigureCustomCapabilities(headers, browserCaps);
            return browserCaps;
        }

        internal IDictionary InternalGetBrowserElements()
        {
            return this.BrowserElements;
        }

        internal IDictionary InternalGetMatchedHeaders()
        {
            return this.MatchedHeaders;
        }

        protected bool IsBrowserUnknown(HttpCapabilitiesBase browserCaps)
        {
            if ((browserCaps.Browsers != null) && (browserCaps.Browsers.Count > 1))
            {
                return false;
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void PopulateBrowserElements(IDictionary dictionary)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void PopulateMatchedHeaders(IDictionary dictionary)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IDictionary BrowserElements
        {
            get
            {
                if (this._browserElements == null)
                {
                    lock (this._lock)
                    {
                        if (this._browserElements == null)
                        {
                            Hashtable dictionary = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
                            this.PopulateBrowserElements(dictionary);
                            this._browserElements = dictionary;
                        }
                    }
                }
                return this._browserElements;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IDictionary MatchedHeaders
        {
            get
            {
                if (this._matchedHeaders == null)
                {
                    lock (this._lock)
                    {
                        if (this._matchedHeaders == null)
                        {
                            Hashtable dictionary = Hashtable.Synchronized(new Hashtable(0x18, StringComparer.OrdinalIgnoreCase));
                            this.PopulateMatchedHeaders(dictionary);
                            this._matchedHeaders = dictionary;
                        }
                    }
                }
                return this._matchedHeaders;
            }
        }
    }
}

