namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class DiscoveryClientDocumentCollection : DictionaryBase
    {
        public void Add(string url, object value)
        {
            base.Dictionary.Add(url, value);
        }

        public bool Contains(string url)
        {
            return base.Dictionary.Contains(url);
        }

        public void Remove(string url)
        {
            base.Dictionary.Remove(url);
        }

        public object this[string url]
        {
            get
            {
                return base.Dictionary[url];
            }
            set
            {
                base.Dictionary[url] = value;
            }
        }

        public ICollection Keys
        {
            get
            {
                return base.Dictionary.Keys;
            }
        }

        public ICollection Values
        {
            get
            {
                return base.Dictionary.Values;
            }
        }
    }
}

