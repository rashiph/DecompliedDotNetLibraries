namespace System.Web
{
    using System;
    using System.Runtime.Serialization;
    using System.Web.Compilation;
    using System.Web.UI;

    [Serializable]
    internal sealed class HttpCachePolicySettings
    {
        internal readonly int _allowInHistory;
        internal readonly HttpCacheability _cacheability;
        internal readonly string _cacheExtension;
        internal readonly string _etag;
        internal readonly bool _generateEtagFromFiles;
        internal readonly bool _generateLastModifiedFromFiles;
        internal readonly bool _hasSetCookieHeader;
        internal readonly bool _hasUserProvidedDependencies;
        internal readonly HttpResponseHeader _headerCacheControl;
        internal readonly HttpResponseHeader _headerEtag;
        internal readonly HttpResponseHeader _headerExpires;
        internal readonly HttpResponseHeader _headerLastModified;
        internal readonly HttpResponseHeader _headerPragma;
        internal readonly HttpResponseHeader _headerVaryBy;
        internal readonly bool _ignoreRangeRequests;
        internal readonly bool _isExpiresSet;
        internal readonly bool _isLastModifiedSet;
        internal readonly bool _isMaxAgeSet;
        internal readonly bool _isModified;
        internal readonly bool _isProxyMaxAgeSet;
        internal readonly TimeSpan _maxAge;
        internal readonly string[] _noCacheFields;
        internal readonly bool _noServerCaching;
        internal readonly bool _noStore;
        internal readonly bool _noTransforms;
        internal readonly int _omitVaryStar;
        internal readonly string[] _privateFields;
        internal readonly TimeSpan _proxyMaxAge;
        internal readonly HttpCacheRevalidation _revalidation;
        internal readonly TimeSpan _slidingDelta;
        internal readonly int _slidingExpiration;
        internal readonly DateTime _utcExpires;
        internal readonly DateTime _utcLastModified;
        internal readonly DateTime _utcTimestampCreated;
        [NonSerialized]
        internal System.Web.ValidationCallbackInfo[] _validationCallbackInfo;
        private string[] _validationCallbackInfoForSerialization;
        internal readonly int _validUntilExpires;
        internal readonly string[] _varyByContentEncodings;
        internal readonly string _varyByCustom;
        internal readonly string[] _varyByHeaderValues;
        internal readonly string[] _varyByParamValues;

        internal HttpCachePolicySettings(bool isModified, System.Web.ValidationCallbackInfo[] validationCallbackInfo, bool hasSetCookieHeader, bool noServerCaching, string cacheExtension, bool noTransforms, bool ignoreRangeRequests, string[] varyByContentEncodings, string[] varyByHeaderValues, string[] varyByParamValues, string varyByCustom, HttpCacheability cacheability, bool noStore, string[] privateFields, string[] noCacheFields, DateTime utcExpires, bool isExpiresSet, TimeSpan maxAge, bool isMaxAgeSet, TimeSpan proxyMaxAge, bool isProxyMaxAgeSet, int slidingExpiration, TimeSpan slidingDelta, DateTime utcTimestampCreated, int validUntilExpires, int allowInHistory, HttpCacheRevalidation revalidation, DateTime utcLastModified, bool isLastModifiedSet, string etag, bool generateLastModifiedFromFiles, bool generateEtagFromFiles, int omitVaryStar, HttpResponseHeader headerCacheControl, HttpResponseHeader headerPragma, HttpResponseHeader headerExpires, HttpResponseHeader headerLastModified, HttpResponseHeader headerEtag, HttpResponseHeader headerVaryBy, bool hasUserProvidedDependencies)
        {
            this._isModified = isModified;
            this._validationCallbackInfo = validationCallbackInfo;
            this._hasSetCookieHeader = hasSetCookieHeader;
            this._noServerCaching = noServerCaching;
            this._cacheExtension = cacheExtension;
            this._noTransforms = noTransforms;
            this._ignoreRangeRequests = ignoreRangeRequests;
            this._varyByContentEncodings = varyByContentEncodings;
            this._varyByHeaderValues = varyByHeaderValues;
            this._varyByParamValues = varyByParamValues;
            this._varyByCustom = varyByCustom;
            this._cacheability = cacheability;
            this._noStore = noStore;
            this._privateFields = privateFields;
            this._noCacheFields = noCacheFields;
            this._utcExpires = utcExpires;
            this._isExpiresSet = isExpiresSet;
            this._maxAge = maxAge;
            this._isMaxAgeSet = isMaxAgeSet;
            this._proxyMaxAge = proxyMaxAge;
            this._isProxyMaxAgeSet = isProxyMaxAgeSet;
            this._slidingExpiration = slidingExpiration;
            this._slidingDelta = slidingDelta;
            this._utcTimestampCreated = utcTimestampCreated;
            this._validUntilExpires = validUntilExpires;
            this._allowInHistory = allowInHistory;
            this._revalidation = revalidation;
            this._utcLastModified = utcLastModified;
            this._isLastModifiedSet = isLastModifiedSet;
            this._etag = etag;
            this._generateLastModifiedFromFiles = generateLastModifiedFromFiles;
            this._generateEtagFromFiles = generateEtagFromFiles;
            this._omitVaryStar = omitVaryStar;
            this._headerCacheControl = headerCacheControl;
            this._headerPragma = headerPragma;
            this._headerExpires = headerExpires;
            this._headerLastModified = headerLastModified;
            this._headerEtag = headerEtag;
            this._headerVaryBy = headerVaryBy;
            this._hasUserProvidedDependencies = hasUserProvidedDependencies;
        }

        internal bool HasValidationPolicy()
        {
            if ((!this.ValidUntilExpires && !this.GenerateLastModifiedFromFiles) && !this.GenerateEtagFromFiles)
            {
                return (this.ValidationCallbackInfo != null);
            }
            return true;
        }

        internal bool IsValidationCallbackSerializable()
        {
            if (this._validationCallbackInfo != null)
            {
                foreach (System.Web.ValidationCallbackInfo info in this._validationCallbackInfo)
                {
                    if ((info.data != null) || !info.handler.Method.IsStatic)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            if (this._validationCallbackInfoForSerialization != null)
            {
                System.Web.ValidationCallbackInfo[] infoArray = new System.Web.ValidationCallbackInfo[this._validationCallbackInfoForSerialization.Length / 2];
                for (int i = 0; i < this._validationCallbackInfoForSerialization.Length; i += 2)
                {
                    string str = this._validationCallbackInfoForSerialization[i];
                    string method = this._validationCallbackInfoForSerialization[i + 1];
                    Type target = null;
                    if (!string.IsNullOrEmpty(str))
                    {
                        target = BuildManager.GetType(str, true, false);
                    }
                    if (target == null)
                    {
                        throw new SerializationException(System.Web.SR.GetString("Type_cannot_be_resolved", new object[] { str }));
                    }
                    HttpCacheValidateHandler handler = (HttpCacheValidateHandler) Delegate.CreateDelegate(typeof(HttpCacheValidateHandler), target, method);
                    infoArray[i] = new System.Web.ValidationCallbackInfo(handler, null);
                }
                this._validationCallbackInfo = infoArray;
            }
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            if (this._validationCallbackInfo != null)
            {
                string[] strArray = new string[this._validationCallbackInfo.Length * 2];
                for (int i = 0; i < this._validationCallbackInfo.Length; i++)
                {
                    HttpCacheValidateHandler handler = this._validationCallbackInfo[i].handler;
                    string assemblyQualifiedTypeName = Util.GetAssemblyQualifiedTypeName(handler.Method.ReflectedType);
                    string name = handler.Method.Name;
                    strArray[i] = assemblyQualifiedTypeName;
                    strArray[i + 1] = name;
                }
                this._validationCallbackInfoForSerialization = strArray;
            }
        }

        internal int AllowInHistoryInternal
        {
            get
            {
                return this._allowInHistory;
            }
        }

        internal HttpCacheability CacheabilityInternal
        {
            get
            {
                return this._cacheability;
            }
        }

        internal string CacheExtension
        {
            get
            {
                return this._cacheExtension;
            }
        }

        internal string ETag
        {
            get
            {
                return this._etag;
            }
        }

        internal bool GenerateEtagFromFiles
        {
            get
            {
                return this._generateEtagFromFiles;
            }
        }

        internal bool GenerateLastModifiedFromFiles
        {
            get
            {
                return this._generateLastModifiedFromFiles;
            }
        }

        internal bool hasSetCookieHeader
        {
            get
            {
                return this._hasSetCookieHeader;
            }
        }

        internal bool HasUserProvidedDependencies
        {
            get
            {
                return this._hasUserProvidedDependencies;
            }
        }

        internal HttpResponseHeader HeaderCacheControl
        {
            get
            {
                return this._headerCacheControl;
            }
        }

        internal HttpResponseHeader HeaderEtag
        {
            get
            {
                return this._headerEtag;
            }
        }

        internal HttpResponseHeader HeaderExpires
        {
            get
            {
                return this._headerExpires;
            }
        }

        internal HttpResponseHeader HeaderLastModified
        {
            get
            {
                return this._headerLastModified;
            }
        }

        internal HttpResponseHeader HeaderPragma
        {
            get
            {
                return this._headerPragma;
            }
        }

        internal HttpResponseHeader HeaderVaryBy
        {
            get
            {
                return this._headerVaryBy;
            }
        }

        internal bool IgnoreParams
        {
            get
            {
                return ((this._varyByParamValues != null) && (this._varyByParamValues[0].Length == 0));
            }
        }

        internal bool IgnoreRangeRequests
        {
            get
            {
                return this._ignoreRangeRequests;
            }
        }

        internal bool IsExpiresSet
        {
            get
            {
                return this._isExpiresSet;
            }
        }

        internal bool IsLastModifiedSet
        {
            get
            {
                return this._isLastModifiedSet;
            }
        }

        internal bool IsMaxAgeSet
        {
            get
            {
                return this._isMaxAgeSet;
            }
        }

        internal bool IsModified
        {
            get
            {
                return this._isModified;
            }
        }

        internal bool IsProxyMaxAgeSet
        {
            get
            {
                return this._isProxyMaxAgeSet;
            }
        }

        internal TimeSpan MaxAge
        {
            get
            {
                return this._maxAge;
            }
        }

        internal string[] NoCacheFields
        {
            get
            {
                if (this._noCacheFields != null)
                {
                    return (string[]) this._noCacheFields.Clone();
                }
                return null;
            }
        }

        internal bool NoServerCaching
        {
            get
            {
                return this._noServerCaching;
            }
        }

        internal bool NoStore
        {
            get
            {
                return this._noStore;
            }
        }

        internal bool NoTransforms
        {
            get
            {
                return this._noTransforms;
            }
        }

        internal int OmitVaryStarInternal
        {
            get
            {
                return this._omitVaryStar;
            }
        }

        internal string[] PrivateFields
        {
            get
            {
                if (this._privateFields != null)
                {
                    return (string[]) this._privateFields.Clone();
                }
                return null;
            }
        }

        internal TimeSpan ProxyMaxAge
        {
            get
            {
                return this._proxyMaxAge;
            }
        }

        internal HttpCacheRevalidation Revalidation
        {
            get
            {
                return this._revalidation;
            }
        }

        internal TimeSpan SlidingDelta
        {
            get
            {
                return this._slidingDelta;
            }
        }

        internal bool SlidingExpiration
        {
            get
            {
                return (this._slidingExpiration == 1);
            }
        }

        internal int SlidingExpirationInternal
        {
            get
            {
                return this._slidingExpiration;
            }
        }

        internal DateTime UtcExpires
        {
            get
            {
                return this._utcExpires;
            }
        }

        internal DateTime UtcLastModified
        {
            get
            {
                return this._utcLastModified;
            }
        }

        internal DateTime UtcTimestampCreated
        {
            get
            {
                return this._utcTimestampCreated;
            }
        }

        internal System.Web.ValidationCallbackInfo[] ValidationCallbackInfo
        {
            get
            {
                return this._validationCallbackInfo;
            }
        }

        internal bool ValidUntilExpires
        {
            get
            {
                return ((((this._validUntilExpires == 1) && !this.SlidingExpiration) && (!this.GenerateLastModifiedFromFiles && !this.GenerateEtagFromFiles)) && (this.ValidationCallbackInfo == null));
            }
        }

        internal int ValidUntilExpiresInternal
        {
            get
            {
                return this._validUntilExpires;
            }
        }

        internal string[] VaryByContentEncodings
        {
            get
            {
                if (this._varyByContentEncodings != null)
                {
                    return (string[]) this._varyByContentEncodings.Clone();
                }
                return null;
            }
        }

        internal string VaryByCustom
        {
            get
            {
                return this._varyByCustom;
            }
        }

        internal string[] VaryByHeaders
        {
            get
            {
                if (this._varyByHeaderValues != null)
                {
                    return (string[]) this._varyByHeaderValues.Clone();
                }
                return null;
            }
        }

        internal string[] VaryByParams
        {
            get
            {
                if (this._varyByParamValues != null)
                {
                    return (string[]) this._varyByParamValues.Clone();
                }
                return null;
            }
        }
    }
}

