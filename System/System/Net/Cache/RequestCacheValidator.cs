namespace System.Net.Cache
{
    using System;
    using System.IO;
    using System.Net;

    internal abstract class RequestCacheValidator
    {
        private RequestCacheEntry _CacheEntry;
        private System.Net.Cache.CacheFreshnessStatus _CacheFreshnessStatus;
        private string _CacheKey;
        internal Stream _CacheStream;
        private long _CacheStreamLength;
        private long _CacheStreamOffset;
        private RequestCachePolicy _Policy;
        internal WebRequest _Request;
        internal WebResponse _Response;
        private int _ResponseCount;
        private bool _StrictCacheErrors;
        private TimeSpan _UnspecifiedMaxAge;
        private System.Uri _Uri;
        private CacheValidationStatus _ValidationStatus;

        protected RequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge)
        {
            this._StrictCacheErrors = strictCacheErrors;
            this._UnspecifiedMaxAge = unspecifiedMaxAge;
            this._ValidationStatus = CacheValidationStatus.DoNotUseCache;
            this._CacheFreshnessStatus = System.Net.Cache.CacheFreshnessStatus.Undefined;
        }

        internal abstract RequestCacheValidator CreateValidator();
        protected internal virtual void FailRequest(WebExceptionStatus webStatus)
        {
            if (Logging.On)
            {
                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_failing_request_with_exception", new object[] { webStatus.ToString() }));
            }
            if (webStatus == WebExceptionStatus.CacheEntryNotFound)
            {
                throw ExceptionHelper.CacheEntryNotFoundException;
            }
            if (webStatus == WebExceptionStatus.RequestProhibitedByCachePolicy)
            {
                throw ExceptionHelper.RequestProhibitedByCachePolicyException;
            }
            throw new WebException(NetRes.GetWebStatusString("net_requestaborted", webStatus), webStatus);
        }

        internal void FetchCacheEntry(RequestCacheEntry fetchEntry)
        {
            this._CacheEntry = fetchEntry;
        }

        internal void FetchRequest(System.Uri uri, WebRequest request)
        {
            this._Request = request;
            this._Policy = request.CachePolicy;
            this._Response = null;
            this._ResponseCount = 0;
            this._ValidationStatus = CacheValidationStatus.DoNotUseCache;
            this._CacheFreshnessStatus = System.Net.Cache.CacheFreshnessStatus.Undefined;
            this._CacheStream = null;
            this._CacheStreamOffset = 0L;
            this._CacheStreamLength = 0L;
            if (!uri.Equals(this._Uri))
            {
                this._CacheKey = uri.GetParts(UriComponents.AbsoluteUri, UriFormat.Unescaped);
            }
            this._Uri = uri;
        }

        internal void FetchResponse(WebResponse fetchResponse)
        {
            this._ResponseCount++;
            this._Response = fetchResponse;
        }

        protected internal abstract CacheValidationStatus RevalidateCache();
        internal void SetFreshnessStatus(System.Net.Cache.CacheFreshnessStatus status)
        {
            this._CacheFreshnessStatus = status;
        }

        internal void SetValidationStatus(CacheValidationStatus status)
        {
            this._ValidationStatus = status;
        }

        protected internal abstract CacheValidationStatus UpdateCache();
        protected internal abstract CacheValidationStatus ValidateCache();
        protected internal abstract System.Net.Cache.CacheFreshnessStatus ValidateFreshness();
        protected internal abstract CacheValidationStatus ValidateRequest();
        protected internal abstract CacheValidationStatus ValidateResponse();

        protected internal RequestCacheEntry CacheEntry
        {
            get
            {
                return this._CacheEntry;
            }
        }

        protected internal System.Net.Cache.CacheFreshnessStatus CacheFreshnessStatus
        {
            get
            {
                return this._CacheFreshnessStatus;
            }
        }

        protected internal string CacheKey
        {
            get
            {
                return this._CacheKey;
            }
        }

        protected internal Stream CacheStream
        {
            get
            {
                return this._CacheStream;
            }
            set
            {
                this._CacheStream = value;
            }
        }

        protected internal long CacheStreamLength
        {
            get
            {
                return this._CacheStreamLength;
            }
            set
            {
                this._CacheStreamLength = value;
            }
        }

        protected internal long CacheStreamOffset
        {
            get
            {
                return this._CacheStreamOffset;
            }
            set
            {
                this._CacheStreamOffset = value;
            }
        }

        protected internal RequestCachePolicy Policy
        {
            get
            {
                return this._Policy;
            }
        }

        protected internal WebRequest Request
        {
            get
            {
                return this._Request;
            }
        }

        protected internal WebResponse Response
        {
            get
            {
                return this._Response;
            }
        }

        protected internal int ResponseCount
        {
            get
            {
                return this._ResponseCount;
            }
        }

        internal bool StrictCacheErrors
        {
            get
            {
                return this._StrictCacheErrors;
            }
        }

        internal TimeSpan UnspecifiedMaxAge
        {
            get
            {
                return this._UnspecifiedMaxAge;
            }
        }

        protected internal System.Uri Uri
        {
            get
            {
                return this._Uri;
            }
        }

        protected internal CacheValidationStatus ValidationStatus
        {
            get
            {
                return this._ValidationStatus;
            }
        }
    }
}

