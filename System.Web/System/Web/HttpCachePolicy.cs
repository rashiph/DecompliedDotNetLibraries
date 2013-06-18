namespace System.Web
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Util;

    public sealed class HttpCachePolicy
    {
        private int _allowInHistory;
        private HttpCacheability _cacheability;
        private string _cacheExtension;
        private string _etag;
        private bool _generateEtagFromFiles;
        private bool _generateLastModifiedFromFiles;
        private bool _hasSetCookieHeader;
        private bool _hasUserProvidedDependencies;
        private HttpResponseHeader _headerCacheControl;
        private HttpResponseHeader _headerEtag;
        private HttpResponseHeader _headerExpires;
        private HttpResponseHeader _headerLastModified;
        private HttpResponseHeader _headerPragma;
        private HttpResponseHeader _headerVaryBy;
        private bool _ignoreRangeRequests;
        private bool _isExpiresSet;
        private bool _isLastModifiedSet;
        private bool _isMaxAgeSet;
        private bool _isModified;
        private bool _isProxyMaxAgeSet;
        private TimeSpan _maxAge;
        private HttpDictionary _noCacheFields;
        private bool _noMaxAgeInCacheControl;
        private bool _noServerCaching;
        private bool _noStore;
        private bool _noTransforms;
        private int _omitVaryStar;
        private HttpDictionary _privateFields;
        private TimeSpan _proxyMaxAge;
        private HttpCacheRevalidation _revalidation;
        private TimeSpan _slidingDelta;
        private int _slidingExpiration;
        private bool _useCachedHeaders;
        private DateTime _utcExpires;
        private DateTime _utcLastModified;
        private DateTime _utcTimestampCreated;
        private DateTime _utcTimestampRequest;
        private ArrayList _validationCallbackInfo;
        private int _validUntilExpires;
        private HttpCacheVaryByContentEncodings _varyByContentEncodings = new HttpCacheVaryByContentEncodings();
        private string _varyByCustom;
        private HttpCacheVaryByHeaders _varyByHeaders = new HttpCacheVaryByHeaders();
        private HttpCacheVaryByParams _varyByParams = new HttpCacheVaryByParams();
        private static readonly string[] s_cacheabilityTokens;
        private static readonly int[] s_cacheabilityValues;
        private static HttpResponseHeader s_headerExpiresMinus1;
        private static HttpResponseHeader s_headerPragmaNoCache;
        private static TimeSpan s_oneYear = new TimeSpan(0x11ed178c6c000L);
        private static readonly string[] s_revalidationTokens;

        static HttpCachePolicy()
        {
            string[] strArray = new string[7];
            strArray[1] = "no-cache";
            strArray[2] = "private";
            strArray[3] = "no-cache";
            strArray[4] = "public";
            strArray[5] = "private";
            s_cacheabilityTokens = strArray;
            string[] strArray2 = new string[4];
            strArray2[1] = "must-revalidate";
            strArray2[2] = "proxy-revalidate";
            s_revalidationTokens = strArray2;
            s_cacheabilityValues = new int[] { -1, 0, 2, 1, 4, 3, 100 };
        }

        internal HttpCachePolicy()
        {
            this.Reset();
        }

        public void AddValidationCallback(HttpCacheValidateHandler handler, object data)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this.Dirtied();
            if (this._validationCallbackInfo == null)
            {
                this._validationCallbackInfo = new ArrayList();
            }
            this._validationCallbackInfo.Add(new ValidationCallbackInfo(handler, data));
        }

        public void AppendCacheExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }
            this.Dirtied();
            if (this._cacheExtension == null)
            {
                this._cacheExtension = extension;
            }
            else
            {
                this._cacheExtension = this._cacheExtension + ", " + extension;
            }
        }

        internal static void AppendValueToHeader(StringBuilder s, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (s.Length > 0)
                {
                    s.Append(", ");
                }
                s.Append(value);
            }
        }

        private void Dirtied()
        {
            this._isModified = true;
            this._useCachedHeaders = false;
        }

        internal HttpCacheability GetCacheability()
        {
            return this._cacheability;
        }

        internal HttpCachePolicySettings GetCurrentSettings(HttpResponse response)
        {
            string[] allKeys;
            string[] strArray5;
            ValidationCallbackInfo[] infoArray;
            this.UpdateCachedHeaders(response);
            string[] contentEncodings = this._varyByContentEncodings.GetContentEncodings();
            string[] headers = this._varyByHeaders.GetHeaders();
            string[] @params = this._varyByParams.GetParams();
            if (this._privateFields != null)
            {
                allKeys = this._privateFields.GetAllKeys();
            }
            else
            {
                allKeys = null;
            }
            if (this._noCacheFields != null)
            {
                strArray5 = this._noCacheFields.GetAllKeys();
            }
            else
            {
                strArray5 = null;
            }
            if (this._validationCallbackInfo != null)
            {
                infoArray = new ValidationCallbackInfo[this._validationCallbackInfo.Count];
                this._validationCallbackInfo.CopyTo(0, infoArray, 0, this._validationCallbackInfo.Count);
            }
            else
            {
                infoArray = null;
            }
            return new HttpCachePolicySettings(this._isModified, infoArray, this._hasSetCookieHeader, this._noServerCaching, this._cacheExtension, this._noTransforms, this._ignoreRangeRequests, contentEncodings, headers, @params, this._varyByCustom, this._cacheability, this._noStore, allKeys, strArray5, this._utcExpires, this._isExpiresSet, this._maxAge, this._isMaxAgeSet, this._proxyMaxAge, this._isProxyMaxAgeSet, this._slidingExpiration, this._slidingDelta, this._utcTimestampCreated, this._validUntilExpires, this._allowInHistory, this._revalidation, this._utcLastModified, this._isLastModifiedSet, this._etag, this._generateLastModifiedFromFiles, this._generateEtagFromFiles, this._omitVaryStar, this._headerCacheControl, this._headerPragma, this._headerExpires, this._headerLastModified, this._headerEtag, this._headerVaryBy, this._hasUserProvidedDependencies);
        }

        internal void GetHeaders(ArrayList headers, HttpResponse response)
        {
            this.UpdateCachedHeaders(response);
            HttpResponseHeader header = this._headerExpires;
            HttpResponseHeader header2 = this._headerCacheControl;
            if ((this._cacheability != HttpCacheability.NoCache) && (this._cacheability != HttpCacheability.Server))
            {
                if (this._slidingExpiration == 1)
                {
                    if (this._isExpiresSet)
                    {
                        DateTime dt = this._utcTimestampRequest + this._slidingDelta;
                        string str = HttpUtility.FormatHttpDateTimeUtc(dt);
                        header = new HttpResponseHeader(0x12, str);
                    }
                }
                else if (this._isMaxAgeSet || this._isProxyMaxAgeSet)
                {
                    StringBuilder builder;
                    if (header2 != null)
                    {
                        builder = new StringBuilder(header2.Value);
                    }
                    else
                    {
                        builder = new StringBuilder();
                    }
                    TimeSpan span = (TimeSpan) (this._utcTimestampRequest - this._utcTimestampCreated);
                    if (this._isMaxAgeSet)
                    {
                        TimeSpan zero = this._maxAge - span;
                        if (zero < TimeSpan.Zero)
                        {
                            zero = TimeSpan.Zero;
                        }
                        if (!this._noMaxAgeInCacheControl)
                        {
                            AppendValueToHeader(builder, "max-age=" + ((long) zero.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    if (this._isProxyMaxAgeSet)
                    {
                        TimeSpan span3 = this._proxyMaxAge - span;
                        if (span3 < TimeSpan.Zero)
                        {
                            span3 = TimeSpan.Zero;
                        }
                        if (!this._noMaxAgeInCacheControl)
                        {
                            AppendValueToHeader(builder, "s-maxage=" + ((long) span3.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    header2 = new HttpResponseHeader(0, builder.ToString());
                }
            }
            if (header2 != null)
            {
                headers.Add(header2);
            }
            if (this._headerPragma != null)
            {
                headers.Add(this._headerPragma);
            }
            if (header != null)
            {
                headers.Add(header);
            }
            if (this._headerLastModified != null)
            {
                headers.Add(this._headerLastModified);
            }
            if (this._headerEtag != null)
            {
                headers.Add(this._headerEtag);
            }
            if (this._headerVaryBy != null)
            {
                headers.Add(this._headerVaryBy);
            }
        }

        internal bool GetNoServerCaching()
        {
            return this._noServerCaching;
        }

        internal bool HasExpirationPolicy()
        {
            if (this._slidingExpiration == 1)
            {
                return false;
            }
            if (!this._isExpiresSet)
            {
                return this._isMaxAgeSet;
            }
            return true;
        }

        internal bool HasValidationPolicy()
        {
            return (((this._generateLastModifiedFromFiles || this._generateEtagFromFiles) || (this._validationCallbackInfo != null)) || ((this._validUntilExpires == 1) && (this._slidingExpiration != 1)));
        }

        internal bool IsKernelCacheable(HttpRequest request, bool enableKernelCacheForVaryByStar)
        {
            return (((((((this._cacheability == HttpCacheability.Public) && !this._hasUserProvidedDependencies) && (!this._hasSetCookieHeader && !this._noServerCaching)) && ((this.HasExpirationPolicy() && (this._cacheExtension == null)) && (!this._varyByContentEncodings.IsModified() && !this._varyByHeaders.IsModified()))) && ((!this._varyByParams.IsModified() || this._varyByParams.IgnoreParams) || (this._varyByParams.IsVaryByStar && enableKernelCacheForVaryByStar))) && (((!this._noStore && (this._varyByCustom == null)) && ((this._privateFields == null) && (this._noCacheFields == null))) && (this._validationCallbackInfo == null))) && ((request != null) && (request.HttpVerb == HttpVerb.GET)));
        }

        internal bool IsModified()
        {
            if ((!this._isModified && !this._varyByContentEncodings.IsModified()) && !this._varyByHeaders.IsModified())
            {
                return this._varyByParams.IsModified();
            }
            return true;
        }

        internal void Reset()
        {
            this._varyByContentEncodings.Reset();
            this._varyByHeaders.Reset();
            this._varyByParams.Reset();
            this._isModified = false;
            this._hasSetCookieHeader = false;
            this._noServerCaching = false;
            this._cacheExtension = null;
            this._noTransforms = false;
            this._ignoreRangeRequests = false;
            this._varyByCustom = null;
            this._cacheability = HttpCacheability.Public | HttpCacheability.Private;
            this._noStore = false;
            this._privateFields = null;
            this._noCacheFields = null;
            this._utcExpires = DateTime.MinValue;
            this._isExpiresSet = false;
            this._maxAge = TimeSpan.Zero;
            this._isMaxAgeSet = false;
            this._proxyMaxAge = TimeSpan.Zero;
            this._isProxyMaxAgeSet = false;
            this._slidingExpiration = -1;
            this._slidingDelta = TimeSpan.Zero;
            this._utcTimestampCreated = DateTime.MinValue;
            this._utcTimestampRequest = DateTime.MinValue;
            this._validUntilExpires = -1;
            this._allowInHistory = -1;
            this._revalidation = HttpCacheRevalidation.None;
            this._utcLastModified = DateTime.MinValue;
            this._isLastModifiedSet = false;
            this._etag = null;
            this._generateLastModifiedFromFiles = false;
            this._generateEtagFromFiles = false;
            this._validationCallbackInfo = null;
            this._useCachedHeaders = false;
            this._headerCacheControl = null;
            this._headerPragma = null;
            this._headerExpires = null;
            this._headerLastModified = null;
            this._headerEtag = null;
            this._headerVaryBy = null;
            this._noMaxAgeInCacheControl = false;
            this._hasUserProvidedDependencies = false;
            this._omitVaryStar = -1;
        }

        internal void ResetFromHttpCachePolicySettings(HttpCachePolicySettings settings, DateTime utcTimestampRequest)
        {
            int num;
            int length;
            this._utcTimestampRequest = utcTimestampRequest;
            this._varyByContentEncodings.ResetFromContentEncodings(settings.VaryByContentEncodings);
            this._varyByHeaders.ResetFromHeaders(settings.VaryByHeaders);
            this._varyByParams.ResetFromParams(settings.VaryByParams);
            this._isModified = settings.IsModified;
            this._hasSetCookieHeader = settings.hasSetCookieHeader;
            this._noServerCaching = settings.NoServerCaching;
            this._cacheExtension = settings.CacheExtension;
            this._noTransforms = settings.NoTransforms;
            this._ignoreRangeRequests = settings.IgnoreRangeRequests;
            this._varyByCustom = settings.VaryByCustom;
            this._cacheability = settings.CacheabilityInternal;
            this._noStore = settings.NoStore;
            this._utcExpires = settings.UtcExpires;
            this._isExpiresSet = settings.IsExpiresSet;
            this._maxAge = settings.MaxAge;
            this._isMaxAgeSet = settings.IsMaxAgeSet;
            this._proxyMaxAge = settings.ProxyMaxAge;
            this._isProxyMaxAgeSet = settings.IsProxyMaxAgeSet;
            this._slidingExpiration = settings.SlidingExpirationInternal;
            this._slidingDelta = settings.SlidingDelta;
            this._utcTimestampCreated = settings.UtcTimestampCreated;
            this._validUntilExpires = settings.ValidUntilExpiresInternal;
            this._allowInHistory = settings.AllowInHistoryInternal;
            this._revalidation = settings.Revalidation;
            this._utcLastModified = settings.UtcLastModified;
            this._isLastModifiedSet = settings.IsLastModifiedSet;
            this._etag = settings.ETag;
            this._generateLastModifiedFromFiles = settings.GenerateLastModifiedFromFiles;
            this._generateEtagFromFiles = settings.GenerateEtagFromFiles;
            this._omitVaryStar = settings.OmitVaryStarInternal;
            this._hasUserProvidedDependencies = settings.HasUserProvidedDependencies;
            this._useCachedHeaders = true;
            this._headerCacheControl = settings.HeaderCacheControl;
            this._headerPragma = settings.HeaderPragma;
            this._headerExpires = settings.HeaderExpires;
            this._headerLastModified = settings.HeaderLastModified;
            this._headerEtag = settings.HeaderEtag;
            this._headerVaryBy = settings.HeaderVaryBy;
            this._noMaxAgeInCacheControl = false;
            string[] privateFields = settings.PrivateFields;
            if (privateFields != null)
            {
                this._privateFields = new HttpDictionary();
                num = 0;
                length = privateFields.Length;
                while (num < length)
                {
                    this._privateFields.SetValue(privateFields[num], privateFields[num]);
                    num++;
                }
            }
            privateFields = settings.NoCacheFields;
            if (privateFields != null)
            {
                this._noCacheFields = new HttpDictionary();
                num = 0;
                length = privateFields.Length;
                while (num < length)
                {
                    this._noCacheFields.SetValue(privateFields[num], privateFields[num]);
                    num++;
                }
            }
            if (settings.ValidationCallbackInfo != null)
            {
                this._validationCallbackInfo = new ArrayList();
                num = 0;
                length = settings.ValidationCallbackInfo.Length;
                while (num < length)
                {
                    this._validationCallbackInfo.Add(new ValidationCallbackInfo(settings.ValidationCallbackInfo[num].handler, settings.ValidationCallbackInfo[num].data));
                    num++;
                }
            }
        }

        public void SetAllowResponseInBrowserHistory(bool allow)
        {
            if ((this._allowInHistory == -1) || (this._allowInHistory == 1))
            {
                this.Dirtied();
                this._allowInHistory = allow ? 1 : 0;
            }
        }

        public void SetCacheability(HttpCacheability cacheability)
        {
            if ((cacheability < HttpCacheability.NoCache) || (HttpCacheability.ServerAndPrivate < cacheability))
            {
                throw new ArgumentOutOfRangeException("cacheability");
            }
            if (s_cacheabilityValues[(int) cacheability] < s_cacheabilityValues[(int) this._cacheability])
            {
                this.Dirtied();
                this._cacheability = cacheability;
            }
        }

        public void SetCacheability(HttpCacheability cacheability, string field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            switch (cacheability)
            {
                case HttpCacheability.NoCache:
                    if (this._noCacheFields == null)
                    {
                        this._noCacheFields = new HttpDictionary();
                    }
                    this._noCacheFields.SetValue(field, field);
                    break;

                case HttpCacheability.Private:
                    if (this._privateFields == null)
                    {
                        this._privateFields = new HttpDictionary();
                    }
                    this._privateFields.SetValue(field, field);
                    break;

                default:
                    throw new ArgumentException(System.Web.SR.GetString("Cacheability_for_field_must_be_private_or_nocache"), "cacheability");
            }
            this.Dirtied();
        }

        internal void SetDependencies(bool hasUserProvidedDependencies)
        {
            this.Dirtied();
            this._hasUserProvidedDependencies = hasUserProvidedDependencies;
        }

        public void SetETag(string etag)
        {
            if (etag == null)
            {
                throw new ArgumentNullException("etag");
            }
            if (this._etag != null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Etag_already_set"));
            }
            if (this._generateEtagFromFiles)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Cant_both_set_and_generate_Etag"));
            }
            this.Dirtied();
            this._etag = etag;
        }

        public void SetETagFromFileDependencies()
        {
            if (this._etag != null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Cant_both_set_and_generate_Etag"));
            }
            this.Dirtied();
            this._generateEtagFromFiles = true;
        }

        public void SetExpires(DateTime date)
        {
            DateTime time = DateTimeUtil.ConvertToUniversalTime(date);
            DateTime utcNow = DateTime.UtcNow;
            if ((time - utcNow) > s_oneYear)
            {
                time = utcNow + s_oneYear;
            }
            if (!this._isExpiresSet || (time < this._utcExpires))
            {
                this.Dirtied();
                this._utcExpires = time;
                this._isExpiresSet = true;
            }
        }

        internal void SetHasSetCookieHeader()
        {
            this.Dirtied();
            this._hasSetCookieHeader = true;
        }

        internal void SetIgnoreRangeRequests()
        {
            this.Dirtied();
            this._ignoreRangeRequests = true;
        }

        public void SetLastModified(DateTime date)
        {
            DateTime utcDate = DateTimeUtil.ConvertToUniversalTime(date);
            this.UtcSetLastModified(utcDate);
        }

        public void SetLastModifiedFromFileDependencies()
        {
            this.Dirtied();
            this._generateLastModifiedFromFiles = true;
        }

        public void SetMaxAge(TimeSpan delta)
        {
            if (delta < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("delta");
            }
            if (s_oneYear < delta)
            {
                delta = s_oneYear;
            }
            if (!this._isMaxAgeSet || (delta < this._maxAge))
            {
                this.Dirtied();
                this._maxAge = delta;
                this._isMaxAgeSet = true;
            }
        }

        internal void SetNoMaxAgeInCacheControl()
        {
            this._noMaxAgeInCacheControl = true;
        }

        public void SetNoServerCaching()
        {
            this.Dirtied();
            this._noServerCaching = true;
        }

        public void SetNoStore()
        {
            this.Dirtied();
            this._noStore = true;
        }

        public void SetNoTransforms()
        {
            this.Dirtied();
            this._noTransforms = true;
        }

        public void SetOmitVaryStar(bool omit)
        {
            this.Dirtied();
            if ((this._omitVaryStar == -1) || (this._omitVaryStar == 1))
            {
                this.Dirtied();
                this._omitVaryStar = omit ? 1 : 0;
            }
        }

        public void SetProxyMaxAge(TimeSpan delta)
        {
            if (delta < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("delta");
            }
            if (!this._isProxyMaxAgeSet || (delta < this._proxyMaxAge))
            {
                this.Dirtied();
                this._proxyMaxAge = delta;
                this._isProxyMaxAgeSet = true;
            }
        }

        public void SetRevalidation(HttpCacheRevalidation revalidation)
        {
            if ((revalidation < HttpCacheRevalidation.AllCaches) || (HttpCacheRevalidation.None < revalidation))
            {
                throw new ArgumentOutOfRangeException("revalidation");
            }
            if (revalidation < this._revalidation)
            {
                this.Dirtied();
                this._revalidation = revalidation;
            }
        }

        public void SetSlidingExpiration(bool slide)
        {
            if ((this._slidingExpiration == -1) || (this._slidingExpiration == 1))
            {
                this.Dirtied();
                this._slidingExpiration = slide ? 1 : 0;
            }
        }

        public void SetValidUntilExpires(bool validUntilExpires)
        {
            if ((this._validUntilExpires == -1) || (this._validUntilExpires == 1))
            {
                this.Dirtied();
                this._validUntilExpires = validUntilExpires ? 1 : 0;
            }
        }

        public void SetVaryByCustom(string custom)
        {
            if (custom == null)
            {
                throw new ArgumentNullException("custom");
            }
            if (this._varyByCustom != null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("VaryByCustom_already_set"));
            }
            this.Dirtied();
            this._varyByCustom = custom;
        }

        private void UpdateCachedHeaders(HttpResponse response)
        {
            if (!this._useCachedHeaders)
            {
                HttpCacheability @private;
                int num;
                int size;
                if (this._utcTimestampCreated == DateTime.MinValue)
                {
                    this._utcTimestampCreated = this._utcTimestampRequest = response.Context.UtcTimestamp;
                }
                if (this._slidingExpiration != 1)
                {
                    this._slidingDelta = TimeSpan.Zero;
                }
                else if (this._isMaxAgeSet)
                {
                    this._slidingDelta = this._maxAge;
                }
                else if (this._isExpiresSet)
                {
                    this._slidingDelta = (TimeSpan) (this._utcExpires - this._utcTimestampCreated);
                }
                else
                {
                    this._slidingDelta = TimeSpan.Zero;
                }
                this._headerCacheControl = null;
                this._headerPragma = null;
                this._headerExpires = null;
                this._headerLastModified = null;
                this._headerEtag = null;
                this._headerVaryBy = null;
                this.UpdateFromDependencies(response);
                StringBuilder s = new StringBuilder();
                if (this._cacheability == (HttpCacheability.Public | HttpCacheability.Private))
                {
                    @private = HttpCacheability.Private;
                }
                else
                {
                    @private = this._cacheability;
                }
                AppendValueToHeader(s, s_cacheabilityTokens[(int) @private]);
                if ((@private == HttpCacheability.Public) && (this._privateFields != null))
                {
                    AppendValueToHeader(s, "private=\"");
                    s.Append(this._privateFields.GetKey(0));
                    num = 1;
                    size = this._privateFields.Size;
                    while (num < size)
                    {
                        AppendValueToHeader(s, this._privateFields.GetKey(num));
                        num++;
                    }
                    s.Append('"');
                }
                if (((@private != HttpCacheability.NoCache) && (@private != HttpCacheability.Server)) && (this._noCacheFields != null))
                {
                    AppendValueToHeader(s, "no-cache=\"");
                    s.Append(this._noCacheFields.GetKey(0));
                    num = 1;
                    size = this._noCacheFields.Size;
                    while (num < size)
                    {
                        AppendValueToHeader(s, this._noCacheFields.GetKey(num));
                        num++;
                    }
                    s.Append('"');
                }
                if (this._noStore)
                {
                    AppendValueToHeader(s, "no-store");
                }
                AppendValueToHeader(s, s_revalidationTokens[(int) this._revalidation]);
                if (this._noTransforms)
                {
                    AppendValueToHeader(s, "no-transform");
                }
                if (this._cacheExtension != null)
                {
                    AppendValueToHeader(s, this._cacheExtension);
                }
                if (((this._slidingExpiration == 1) && (@private != HttpCacheability.NoCache)) && (@private != HttpCacheability.Server))
                {
                    if (this._isMaxAgeSet && !this._noMaxAgeInCacheControl)
                    {
                        AppendValueToHeader(s, "max-age=" + ((long) this._maxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                    }
                    if (this._isProxyMaxAgeSet && !this._noMaxAgeInCacheControl)
                    {
                        AppendValueToHeader(s, "s-maxage=" + ((long) this._proxyMaxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                    }
                }
                if (s.Length > 0)
                {
                    this._headerCacheControl = new HttpResponseHeader(0, s.ToString());
                }
                switch (@private)
                {
                    case HttpCacheability.NoCache:
                    case HttpCacheability.Server:
                        if (s_headerPragmaNoCache == null)
                        {
                            s_headerPragmaNoCache = new HttpResponseHeader(4, "no-cache");
                        }
                        this._headerPragma = s_headerPragmaNoCache;
                        if (this._allowInHistory != 1)
                        {
                            if (s_headerExpiresMinus1 == null)
                            {
                                s_headerExpiresMinus1 = new HttpResponseHeader(0x12, "-1");
                            }
                            this._headerExpires = s_headerExpiresMinus1;
                        }
                        break;

                    default:
                        if (this._isExpiresSet && (this._slidingExpiration != 1))
                        {
                            string str = HttpUtility.FormatHttpDateTimeUtc(this._utcExpires);
                            this._headerExpires = new HttpResponseHeader(0x12, str);
                        }
                        if (this._isLastModifiedSet)
                        {
                            string str2 = HttpUtility.FormatHttpDateTimeUtc(this._utcLastModified);
                            this._headerLastModified = new HttpResponseHeader(0x13, str2);
                        }
                        if (@private != HttpCacheability.Private)
                        {
                            bool omitVaryStar;
                            if (this._etag != null)
                            {
                                this._headerEtag = new HttpResponseHeader(0x16, this._etag);
                            }
                            string str3 = null;
                            if (this._omitVaryStar != -1)
                            {
                                omitVaryStar = this._omitVaryStar == 1;
                            }
                            else
                            {
                                OutputCacheSection outputCache = RuntimeConfig.GetLKGConfig(response.Context).OutputCache;
                                if (outputCache != null)
                                {
                                    omitVaryStar = outputCache.OmitVaryStar;
                                }
                                else
                                {
                                    omitVaryStar = false;
                                }
                            }
                            if (!omitVaryStar && ((this._varyByCustom != null) || (this._varyByParams.IsModified() && !this._varyByParams.IgnoreParams)))
                            {
                                str3 = "*";
                            }
                            if (str3 == null)
                            {
                                str3 = this._varyByHeaders.ToHeaderString();
                            }
                            if (str3 != null)
                            {
                                this._headerVaryBy = new HttpResponseHeader(0x1c, str3);
                            }
                        }
                        break;
                }
                this._useCachedHeaders = true;
            }
        }

        private void UpdateFromDependencies(HttpResponse response)
        {
            CacheDependency dep = null;
            if ((this._etag == null) && this._generateEtagFromFiles)
            {
                dep = response.CreateCacheDependencyForResponse();
                if (dep == null)
                {
                    return;
                }
                string uniqueID = dep.GetUniqueID();
                if (uniqueID == null)
                {
                    throw new HttpException(System.Web.SR.GetString("No_UniqueId_Cache_Dependency"));
                }
                DateTime time = this.UpdateLastModifiedTimeFromDependency(dep);
                StringBuilder builder = new StringBuilder(0x100);
                builder.Append(HttpRuntime.AppDomainIdInternal);
                builder.Append(uniqueID);
                builder.Append("+LM");
                builder.Append(time.Ticks.ToString(CultureInfo.InvariantCulture));
                this._etag = MachineKeySection.HashAndBase64EncodeString(builder.ToString());
                this._etag = "\"" + this._etag + "\"";
            }
            if (this._generateLastModifiedFromFiles)
            {
                if (dep == null)
                {
                    dep = response.CreateCacheDependencyForResponse();
                    if (dep == null)
                    {
                        return;
                    }
                }
                DateTime utcDate = this.UpdateLastModifiedTimeFromDependency(dep);
                this.UtcSetLastModified(utcDate);
            }
        }

        private DateTime UpdateLastModifiedTimeFromDependency(CacheDependency dep)
        {
            DateTime utcLastModified = dep.UtcLastModified;
            if (utcLastModified < this._utcLastModified)
            {
                utcLastModified = this._utcLastModified;
            }
            DateTime utcNow = DateTime.UtcNow;
            if (utcLastModified > utcNow)
            {
                utcLastModified = utcNow;
            }
            return utcLastModified;
        }

        internal DateTime UtcGetAbsoluteExpiration()
        {
            DateTime noAbsoluteExpiration = Cache.NoAbsoluteExpiration;
            if (this._slidingExpiration != 1)
            {
                if (this._isMaxAgeSet)
                {
                    return (this._utcTimestampCreated + this._maxAge);
                }
                if (this._isExpiresSet)
                {
                    noAbsoluteExpiration = this._utcExpires;
                }
            }
            return noAbsoluteExpiration;
        }

        private void UtcSetLastModified(DateTime utcDate)
        {
            utcDate = new DateTime(utcDate.Ticks - (utcDate.Ticks % 0x989680L));
            if (utcDate > DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException("utcDate");
            }
            if (!this._isLastModifiedSet || (utcDate > this._utcLastModified))
            {
                this.Dirtied();
                this._utcLastModified = utcDate;
                this._isLastModifiedSet = true;
            }
        }

        internal bool IsVaryByStar
        {
            get
            {
                return this._varyByParams.IsVaryByStar;
            }
        }

        public HttpCacheVaryByContentEncodings VaryByContentEncodings
        {
            get
            {
                return this._varyByContentEncodings;
            }
        }

        public HttpCacheVaryByHeaders VaryByHeaders
        {
            get
            {
                return this._varyByHeaders;
            }
        }

        public HttpCacheVaryByParams VaryByParams
        {
            get
            {
                return this._varyByParams;
            }
        }
    }
}

