namespace System.Net.Cache
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;

    internal class FtpRequestCacheValidator : HttpRequestCacheValidator
    {
        private bool m_HttpProxyMode;
        private DateTime m_LastModified;

        internal FtpRequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge) : base(strictCacheErrors, unspecifiedMaxAge)
        {
        }

        internal override RequestCacheValidator CreateValidator()
        {
            return new FtpRequestCacheValidator(base.StrictCacheErrors, base.UnspecifiedMaxAge);
        }

        protected internal override CacheValidationStatus RevalidateCache()
        {
            if (this.HttpProxyMode)
            {
                return base.RevalidateCache();
            }
            if (this.Policy.Level >= RequestCacheLevel.Reload)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_validator_invalid_for_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (base.CacheStream != Stream.Null)
            {
                CacheValidationStatus doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
                FtpWebResponse response = base.Response as FtpWebResponse;
                if (response == null)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (response.StatusCode != FtpStatusCode.FileStatus)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_response_last_modified", new object[] { response.LastModified.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture), response.ContentLength }));
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_cache_last_modified", new object[] { base.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture), base.CacheEntry.StreamSize }));
                }
                if ((base.CacheStreamOffset != 0L) && base.CacheEntry.IsPartialEntry)
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_partial_and_non_zero_content_offset", new object[] { base.CacheStreamOffset.ToString(CultureInfo.InvariantCulture) }));
                    }
                    doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
                }
                if (!(response.LastModified.ToUniversalTime() == base.CacheEntry.LastModifiedUtc))
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (base.CacheEntry.IsPartialEntry)
                {
                    if (response.ContentLength > 0L)
                    {
                        base.CacheStreamLength = response.ContentLength;
                    }
                    else
                    {
                        base.CacheStreamLength = -1L;
                    }
                    return CacheValidationStatus.CombineCachedAndServerResponse;
                }
                if (response.ContentLength == base.CacheEntry.StreamSize)
                {
                    return CacheValidationStatus.ReturnCachedResponse;
                }
            }
            return CacheValidationStatus.DoNotTakeFromCache;
        }

        private CacheValidationStatus TryConditionalRequest()
        {
            FtpWebRequest request = base.Request as FtpWebRequest;
            if ((request == null) || !request.UseBinary)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (request.ContentOffset != 0L)
            {
                if (base.CacheEntry.IsPartialEntry || (request.ContentOffset >= base.CacheStreamLength))
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                base.CacheStreamOffset = request.ContentOffset;
            }
            return CacheValidationStatus.Continue;
        }

        protected internal override CacheValidationStatus UpdateCache()
        {
            if (this.HttpProxyMode)
            {
                return base.UpdateCache();
            }
            base.CacheStreamOffset = 0L;
            if (base.RequestMethod == HttpMethod.Other)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_based_on_policy", new object[] { base.Request.Method }));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if (base.ValidationStatus == CacheValidationStatus.RemoveFromCache)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_removed_existing_invalid_entry"));
                }
                return CacheValidationStatus.RemoveFromCache;
            }
            if (this.Policy.Level == RequestCacheLevel.CacheOnly)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            FtpWebResponse response = base.Response as FtpWebResponse;
            if (response == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_because_no_response"));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if ((base.RequestMethod == HttpMethod.Delete) || (base.RequestMethod == HttpMethod.Put))
            {
                if (((base.RequestMethod == HttpMethod.Delete) || (response.StatusCode == FtpStatusCode.OpeningData)) || (((response.StatusCode == FtpStatusCode.DataAlreadyOpen) || (response.StatusCode == FtpStatusCode.FileActionOK)) || (response.StatusCode == FtpStatusCode.ClosingData)))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_removed_existing_based_on_method", new object[] { base.Request.Method }));
                    }
                    return CacheValidationStatus.RemoveFromCache;
                }
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_existing_not_removed_because_unexpected_response_status", new object[] { (int) response.StatusCode, response.StatusCode.ToString() }));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if (this.Policy.Level == RequestCacheLevel.NoCacheNoStore)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_removed_existing_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.RemoveFromCache;
            }
            if (base.ValidationStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                return this.UpdateCacheEntryOnRevalidate();
            }
            if (((response.StatusCode != FtpStatusCode.OpeningData) && (response.StatusCode != FtpStatusCode.DataAlreadyOpen)) && (response.StatusCode != FtpStatusCode.ClosingData))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_based_on_ftp_response_status", new object[] { FtpStatusCode.OpeningData.ToString() + "|" + FtpStatusCode.DataAlreadyOpen.ToString() + "|" + FtpStatusCode.ClosingData.ToString(), response.StatusCode.ToString() }));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if (((FtpWebRequest) base.Request).ContentOffset == 0L)
            {
                return this.UpdateCacheEntryOnStore();
            }
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_update_not_supported_for_ftp_restart", new object[] { ((FtpWebRequest) base.Request).ContentOffset.ToString(CultureInfo.InvariantCulture) }));
            }
            if (!(base.CacheEntry.LastModifiedUtc != DateTime.MinValue) || !(response.LastModified.ToUniversalTime() != base.CacheEntry.LastModifiedUtc))
            {
                return CacheValidationStatus.DoNotUpdateCache;
            }
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_removed_entry_because_ftp_restart_response_changed", new object[] { base.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture), response.LastModified.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture) }));
            }
            return CacheValidationStatus.RemoveFromCache;
        }

        private CacheValidationStatus UpdateCacheEntryOnRevalidate()
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_last_synchronized", new object[] { base.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture) }));
            }
            DateTime utcNow = DateTime.UtcNow;
            if ((base.CacheEntry.LastSynchronizedUtc + TimeSpan.FromMinutes(1.0)) >= utcNow)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_suppress_update_because_synched_last_minute"));
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }
            base.CacheEntry.EntryMetadata = null;
            base.CacheEntry.SystemMetadata = null;
            base.CacheEntry.LastSynchronizedUtc = utcNow;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_updating_last_synchronized", new object[] { base.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture) }));
            }
            return CacheValidationStatus.UpdateResponseInformation;
        }

        private CacheValidationStatus UpdateCacheEntryOnStore()
        {
            base.CacheEntry.EntryMetadata = null;
            base.CacheEntry.SystemMetadata = null;
            FtpWebResponse response = base.Response as FtpWebResponse;
            if (response.LastModified != DateTime.MinValue)
            {
                base.CacheEntry.LastModifiedUtc = response.LastModified.ToUniversalTime();
            }
            base.ResponseEntityLength = base.Response.ContentLength;
            base.CacheEntry.StreamSize = base.ResponseEntityLength;
            base.CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
            return CacheValidationStatus.CacheResponse;
        }

        protected internal override CacheValidationStatus ValidateCache()
        {
            if (this.HttpProxyMode)
            {
                return base.ValidateCache();
            }
            if (this.Policy.Level >= RequestCacheLevel.Reload)
            {
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_validator_invalid_for_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if ((base.CacheStream == Stream.Null) || base.CacheEntry.IsPartialEntry)
            {
                if (this.Policy.Level == RequestCacheLevel.CacheOnly)
                {
                    this.FailRequest(WebExceptionStatus.CacheEntryNotFound);
                }
                if (base.CacheStream == Stream.Null)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
            }
            base.CacheStreamOffset = 0L;
            base.CacheStreamLength = base.CacheEntry.StreamSize;
            if ((this.Policy.Level == RequestCacheLevel.Revalidate) || base.CacheEntry.IsPartialEntry)
            {
                return this.TryConditionalRequest();
            }
            long num = (base.Request is FtpWebRequest) ? ((FtpWebRequest) base.Request).ContentOffset : 0L;
            if (((base.CacheFreshnessStatus != CacheFreshnessStatus.Fresh) && (this.Policy.Level != RequestCacheLevel.CacheOnly)) && (this.Policy.Level != RequestCacheLevel.CacheIfAvailable))
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (num != 0L)
            {
                if (num >= base.CacheStreamLength)
                {
                    if (this.Policy.Level == RequestCacheLevel.CacheOnly)
                    {
                        this.FailRequest(WebExceptionStatus.CacheEntryNotFound);
                    }
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                base.CacheStreamOffset = num;
            }
            return CacheValidationStatus.ReturnCachedResponse;
        }

        protected internal override CacheFreshnessStatus ValidateFreshness()
        {
            if (this.HttpProxyMode)
            {
                if (base.CacheStream != Stream.Null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_replacing_entry_with_HTTP_200"));
                    }
                    if (base.CacheEntry.EntryMetadata == null)
                    {
                        base.CacheEntry.EntryMetadata = new StringCollection();
                    }
                    base.CacheEntry.EntryMetadata.Clear();
                    base.CacheEntry.EntryMetadata.Add("HTTP/1.1 200 OK");
                }
                return base.ValidateFreshness();
            }
            DateTime utcNow = DateTime.UtcNow;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_now_time", new object[] { utcNow.ToString("r", CultureInfo.InvariantCulture) }));
            }
            if (base.CacheEntry.ExpiresUtc != DateTime.MinValue)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_max_age_absolute", new object[] { base.CacheEntry.ExpiresUtc.ToString("r", CultureInfo.InvariantCulture) }));
                }
                if (base.CacheEntry.ExpiresUtc < utcNow)
                {
                    return CacheFreshnessStatus.Stale;
                }
                return CacheFreshnessStatus.Fresh;
            }
            TimeSpan maxValue = TimeSpan.MaxValue;
            if (base.CacheEntry.LastSynchronizedUtc != DateTime.MinValue)
            {
                maxValue = (TimeSpan) (utcNow - base.CacheEntry.LastSynchronizedUtc);
                if (Logging.On)
                {
                    object[] args = new object[] { ((int) maxValue.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), base.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture) };
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age1", args));
                }
            }
            if (base.CacheEntry.LastModifiedUtc != DateTime.MinValue)
            {
                TimeSpan span2 = (TimeSpan) (utcNow - base.CacheEntry.LastModifiedUtc);
                int num = (int) (span2.TotalSeconds / 10.0);
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_no_max_age_use_10_percent", new object[] { num.ToString(NumberFormatInfo.InvariantInfo), base.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture) }));
                }
                if (maxValue.TotalSeconds < num)
                {
                    return CacheFreshnessStatus.Fresh;
                }
                return CacheFreshnessStatus.Stale;
            }
            if (Logging.On)
            {
                object[] objArray5 = new object[] { ((int) base.UnspecifiedMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_no_max_age_use_default", objArray5));
            }
            if (base.UnspecifiedMaxAge >= maxValue)
            {
                return CacheFreshnessStatus.Fresh;
            }
            return CacheFreshnessStatus.Stale;
        }

        protected internal override CacheValidationStatus ValidateRequest()
        {
            this.ZeroPrivateVars();
            if (base.Request is HttpWebRequest)
            {
                this.m_HttpProxyMode = true;
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_ftp_proxy_doesnt_support_partial"));
                }
                return base.ValidateRequest();
            }
            if (this.Policy.Level == RequestCacheLevel.BypassCache)
            {
                return CacheValidationStatus.DoNotUseCache;
            }
            string str = base.Request.Method.ToUpper(CultureInfo.InvariantCulture);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_ftp_method", new object[] { str }));
            }
            string str2 = str;
            if (str2 != null)
            {
                if (!(str2 == "RETR"))
                {
                    if (str2 == "STOR")
                    {
                        base.RequestMethod = HttpMethod.Put;
                        goto Label_0105;
                    }
                    if (str2 == "APPE")
                    {
                        base.RequestMethod = HttpMethod.Put;
                        goto Label_0105;
                    }
                    if (str2 == "RENAME")
                    {
                        base.RequestMethod = HttpMethod.Put;
                        goto Label_0105;
                    }
                    if (str2 == "DELE")
                    {
                        base.RequestMethod = HttpMethod.Delete;
                        goto Label_0105;
                    }
                }
                else
                {
                    base.RequestMethod = HttpMethod.Get;
                    goto Label_0105;
                }
            }
            base.RequestMethod = HttpMethod.Other;
        Label_0105:
            if (((base.RequestMethod != HttpMethod.Get) || !((FtpWebRequest) base.Request).UseBinary) && (this.Policy.Level == RequestCacheLevel.CacheOnly))
            {
                this.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
            }
            if (str != "RETR")
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (!((FtpWebRequest) base.Request).UseBinary)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_ftp_supports_bin_only"));
                }
                return CacheValidationStatus.DoNotUseCache;
            }
            if (this.Policy.Level >= RequestCacheLevel.Reload)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            return CacheValidationStatus.Continue;
        }

        protected internal override CacheValidationStatus ValidateResponse()
        {
            if (this.HttpProxyMode)
            {
                return base.ValidateResponse();
            }
            if ((this.Policy.Level != RequestCacheLevel.Default) && (this.Policy.Level != RequestCacheLevel.Revalidate))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_response_valid_based_on_policy", new object[] { this.Policy.ToString() }));
                }
                return CacheValidationStatus.Continue;
            }
            FtpWebResponse response = base.Response as FtpWebResponse;
            if (response == null)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_null_response_failure"));
                }
                return CacheValidationStatus.Continue;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_ftp_response_status", new object[] { ((int) response.StatusCode).ToString(CultureInfo.InvariantCulture), response.StatusCode.ToString() }));
            }
            if (base.ResponseCount > 1)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_valid_based_on_retry", new object[] { base.ResponseCount }));
                }
                return CacheValidationStatus.Continue;
            }
            if ((response.StatusCode != FtpStatusCode.OpeningData) && (response.StatusCode != FtpStatusCode.FileStatus))
            {
                return CacheValidationStatus.RetryResponseFromServer;
            }
            return CacheValidationStatus.Continue;
        }

        private void ZeroPrivateVars()
        {
            this.m_LastModified = DateTime.MinValue;
            this.m_HttpProxyMode = false;
        }

        private bool HttpProxyMode
        {
            get
            {
                return this.m_HttpProxyMode;
            }
        }

        internal RequestCachePolicy Policy
        {
            get
            {
                return base.Policy;
            }
        }
    }
}

