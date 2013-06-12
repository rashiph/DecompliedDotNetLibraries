namespace System.Web.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    [Serializable]
    internal class OutputCacheEntry : IOutputCacheEntry
    {
        private Guid _cachedVaryId;
        private string[] _dependencies;
        private string _dependenciesKey;
        private List<HeaderElement> _headerElements;
        private string _kernelCacheUrl;
        private List<ResponseElement> _responseElements;
        private HttpCachePolicySettings _settings;
        private int _statusCode;
        private string _statusDescription;

        private OutputCacheEntry()
        {
        }

        internal OutputCacheEntry(Guid cachedVaryId, HttpCachePolicySettings settings, string kernelCacheUrl, string dependenciesKey, string[] dependencies, int statusCode, string statusDescription, List<HeaderElement> headerElements, List<ResponseElement> responseElements)
        {
            this._cachedVaryId = cachedVaryId;
            this._settings = settings;
            this._kernelCacheUrl = kernelCacheUrl;
            this._dependenciesKey = dependenciesKey;
            this._dependencies = dependencies;
            this._statusCode = statusCode;
            this._statusDescription = statusDescription;
            this._headerElements = headerElements;
            this._responseElements = responseElements;
        }

        internal Guid CachedVaryId
        {
            get
            {
                return this._cachedVaryId;
            }
        }

        internal string[] Dependencies
        {
            get
            {
                return this._dependencies;
            }
        }

        internal string DependenciesKey
        {
            get
            {
                return this._dependenciesKey;
            }
        }

        public List<HeaderElement> HeaderElements
        {
            get
            {
                return this._headerElements;
            }
            set
            {
                this._headerElements = value;
            }
        }

        internal string KernelCacheUrl
        {
            get
            {
                return this._kernelCacheUrl;
            }
        }

        public List<ResponseElement> ResponseElements
        {
            get
            {
                return this._responseElements;
            }
            set
            {
                this._responseElements = value;
            }
        }

        internal HttpCachePolicySettings Settings
        {
            get
            {
                return this._settings;
            }
        }

        internal int StatusCode
        {
            get
            {
                return this._statusCode;
            }
        }

        internal string StatusDescription
        {
            get
            {
                return this._statusDescription;
            }
        }
    }
}

