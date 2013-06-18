namespace System.Web.Caching
{
    using System;
    using System.Web;

    internal class CachedRawResponse
    {
        internal Guid _cachedVaryId;
        internal readonly string _kernelCacheUrl;
        internal readonly HttpRawResponse _rawResponse;
        internal readonly HttpCachePolicySettings _settings;

        internal CachedRawResponse(HttpRawResponse rawResponse, HttpCachePolicySettings settings, string kernelCacheUrl, Guid cachedVaryId)
        {
            this._rawResponse = rawResponse;
            this._settings = settings;
            this._kernelCacheUrl = kernelCacheUrl;
            this._cachedVaryId = cachedVaryId;
        }
    }
}

