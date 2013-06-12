namespace System.Net.Cache
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class Rfc2616
    {
        private Rfc2616()
        {
        }

        public static CacheValidationStatus OnUpdateCache(HttpRequestCacheValidator ctx)
        {
            if (ctx.CacheStatusCode == HttpStatusCode.NotModified)
            {
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_need_to_remove_invalid_cache_entry_304"));
                }
                return CacheValidationStatus.RemoveFromCache;
            }
            HttpWebResponse resp = ctx.Response as HttpWebResponse;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_status", new object[] { resp.StatusCode }));
            }
            if (ctx.ValidationStatus == CacheValidationStatus.RemoveFromCache)
            {
                return CacheValidationStatus.RemoveFromCache;
            }
            CacheValidationStatus status = (((ctx.RequestMethod >= HttpMethod.Post) && (ctx.RequestMethod <= HttpMethod.Delete)) || (ctx.RequestMethod == HttpMethod.Other)) ? CacheValidationStatus.RemoveFromCache : CacheValidationStatus.DoNotUpdateCache;
            if (Common.OnUpdateCache(ctx, resp) != TriState.Valid)
            {
                return status;
            }
            CacheValidationStatus cacheResponse = CacheValidationStatus.CacheResponse;
            ctx.CacheEntry.IsPartialEntry = false;
            if ((resp.StatusCode == HttpStatusCode.NotModified) || (ctx.RequestMethod == HttpMethod.Head))
            {
                cacheResponse = CacheValidationStatus.UpdateResponseInformation;
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_304_or_request_head"));
                }
                if (ctx.CacheDontUpdateHeaders)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_dont_update_cached_headers"));
                    }
                    ctx.CacheHeaders = null;
                    ctx.CacheEntry.ExpiresUtc = ctx.ResponseExpires;
                    ctx.CacheEntry.LastModifiedUtc = ctx.ResponseLastModified;
                    if (ctx.Policy.Level == HttpRequestCacheLevel.Default)
                    {
                        ctx.CacheEntry.MaxStale = ctx.Policy.MaxStale;
                    }
                    else
                    {
                        ctx.CacheEntry.MaxStale = TimeSpan.MinValue;
                    }
                    ctx.CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
                    return cacheResponse;
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_update_cached_headers"));
                }
                return cacheResponse;
            }
            if (resp.StatusCode == HttpStatusCode.PartialContent)
            {
                if ((ctx.CacheEntry.StreamSize != ctx.ResponseRangeStart) && (ctx.ResponseRangeStart != 0L))
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_partial_resp_not_combined_with_existing_entry", new object[] { ctx.CacheEntry.StreamSize, ctx.ResponseRangeStart }));
                    }
                    return status;
                }
                if (!ctx.RequestRangeUser)
                {
                    ctx.CacheStreamOffset = 0L;
                }
                Common.ReplaceOrUpdateCacheHeaders(ctx, resp);
                ctx.CacheHttpVersion = resp.ProtocolVersion;
                ctx.CacheEntityLength = ctx.ResponseEntityLength;
                ctx.CacheStreamLength = ctx.CacheEntry.StreamSize = ctx.ResponseRangeEnd + 1L;
                if ((ctx.CacheEntityLength > 0L) && (ctx.CacheEntityLength == ctx.CacheEntry.StreamSize))
                {
                    Common.Construct200ok(ctx);
                    return cacheResponse;
                }
                Common.Construct206PartialContent(ctx, 0);
                return cacheResponse;
            }
            Common.ReplaceOrUpdateCacheHeaders(ctx, resp);
            ctx.CacheHttpVersion = resp.ProtocolVersion;
            ctx.CacheStatusCode = resp.StatusCode;
            ctx.CacheStatusDescription = resp.StatusDescription;
            ctx.CacheEntry.StreamSize = resp.ContentLength;
            return cacheResponse;
        }

        public static CacheValidationStatus OnValidateCache(HttpRequestCacheValidator ctx)
        {
            if (Common.ValidateCacheByVaryHeader(ctx) == TriState.Invalid)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }
            if (ctx.Policy.Level == HttpRequestCacheLevel.Revalidate)
            {
                return Common.TryConditionalRequest(ctx);
            }
            if (Common.ValidateCacheBySpecialCases(ctx) == TriState.Invalid)
            {
                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                return Common.TryConditionalRequest(ctx);
            }
            if ((!Common.ValidateCacheByClientPolicy(ctx) && (ctx.Policy.Level != HttpRequestCacheLevel.CacheOnly)) && ((ctx.Policy.Level != HttpRequestCacheLevel.CacheIfAvailable) && (ctx.Policy.Level != HttpRequestCacheLevel.CacheOrNextCacheOnly)))
            {
                return Common.TryConditionalRequest(ctx);
            }
            CacheValidationStatus status = Common.TryResponseFromCache(ctx);
            if (status != CacheValidationStatus.ReturnCachedResponse)
            {
                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                return status;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_valid_as_fresh_or_because_policy", new object[] { ctx.Policy.ToString() }));
            }
            return CacheValidationStatus.ReturnCachedResponse;
        }

        public static CacheFreshnessStatus OnValidateFreshness(HttpRequestCacheValidator ctx)
        {
            CacheFreshnessStatus status = Common.ComputeFreshness(ctx);
            if (ctx.Uri.Query.Length == 0)
            {
                return status;
            }
            if ((ctx.CacheHeaders.Expires == null) && (ctx.CacheEntry.IsPrivateEntry ? (ctx.CacheCacheControl.MaxAge == -1) : (ctx.CacheCacheControl.SMaxAge == -1)))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_uri_with_query_has_no_expiration"));
                }
                return CacheFreshnessStatus.Stale;
            }
            if ((ctx.CacheHttpVersion.Major > 1) || (ctx.CacheHttpVersion.Minor >= 1))
            {
                return status;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_uri_with_query_and_cached_resp_from_http_10"));
            }
            return CacheFreshnessStatus.Stale;
        }

        public static CacheValidationStatus OnValidateRequest(HttpRequestCacheValidator ctx)
        {
            CacheValidationStatus status = Common.OnValidateRequest(ctx);
            if (status != CacheValidationStatus.DoNotUseCache)
            {
                ctx.Request.Headers.RemoveInternal("Pragma");
                ctx.Request.Headers.RemoveInternal("Cache-Control");
                if (ctx.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore)
                {
                    ctx.Request.Headers.AddInternal("Cache-Control", "no-store");
                    ctx.Request.Headers.AddInternal("Cache-Control", "no-cache");
                    ctx.Request.Headers.AddInternal("Pragma", "no-cache");
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (status != CacheValidationStatus.Continue)
                {
                    return status;
                }
                if ((ctx.Policy.Level == HttpRequestCacheLevel.Reload) || (ctx.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore))
                {
                    ctx.Request.Headers.AddInternal("Cache-Control", "no-cache");
                    ctx.Request.Headers.AddInternal("Pragma", "no-cache");
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (ctx.Policy.Level == HttpRequestCacheLevel.Refresh)
                {
                    ctx.Request.Headers.AddInternal("Cache-Control", "max-age=0");
                    ctx.Request.Headers.AddInternal("Pragma", "no-cache");
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (ctx.Policy.Level == HttpRequestCacheLevel.Default)
                {
                    if (ctx.Policy.MinFresh > TimeSpan.Zero)
                    {
                        ctx.Request.Headers.AddInternal("Cache-Control", "min-fresh=" + ((int) ctx.Policy.MinFresh.TotalSeconds));
                    }
                    if (ctx.Policy.MaxAge != TimeSpan.MaxValue)
                    {
                        ctx.Request.Headers.AddInternal("Cache-Control", "max-age=" + ((int) ctx.Policy.MaxAge.TotalSeconds));
                    }
                    if (ctx.Policy.MaxStale > TimeSpan.Zero)
                    {
                        ctx.Request.Headers.AddInternal("Cache-Control", "max-stale=" + ((int) ctx.Policy.MaxStale.TotalSeconds));
                    }
                    return status;
                }
                if ((ctx.Policy.Level != HttpRequestCacheLevel.CacheOnly) && (ctx.Policy.Level != HttpRequestCacheLevel.CacheOrNextCacheOnly))
                {
                    return status;
                }
                ctx.Request.Headers.AddInternal("Cache-Control", "only-if-cached");
            }
            return status;
        }

        public static CacheValidationStatus OnValidateResponse(HttpRequestCacheValidator ctx)
        {
            if (ctx.ResponseCount > 1)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_accept_based_on_retry_count", new object[] { ctx.ResponseCount }));
                }
                return CacheValidationStatus.Continue;
            }
            if (!ctx.RequestRangeUser)
            {
                if (((ctx.CacheDate != DateTime.MinValue) && (ctx.ResponseDate != DateTime.MinValue)) && (ctx.CacheDate > ctx.ResponseDate))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_date_header_older_than_cache_entry"));
                    }
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
                HttpWebResponse response = ctx.Response as HttpWebResponse;
                if (ctx.RequestRangeCache && (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_server_didnt_satisfy_range", new object[] { ctx.Request.Headers["Range"] }));
                    }
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    if (ctx.RequestIfHeader1 == null)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_304_received_on_unconditional_request"));
                        }
                        Common.ConstructUnconditionalRefreshRequest(ctx);
                        return CacheValidationStatus.RetryResponseFromServer;
                    }
                    if (ctx.RequestRangeCache)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_304_received_on_unconditional_request_expected_200_206"));
                        }
                        Common.ConstructUnconditionalRefreshRequest(ctx);
                        return CacheValidationStatus.RetryResponseFromServer;
                    }
                }
                if ((((ctx.CacheHttpVersion.Major <= 1) && (response.ProtocolVersion.Major <= 1)) && ((ctx.CacheHttpVersion.Minor < 1) && (response.ProtocolVersion.Minor < 1))) && (ctx.CacheLastModified > ctx.ResponseLastModified))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_last_modified_header_older_than_cache_entry"));
                    }
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
                if (((ctx.Policy.Level == HttpRequestCacheLevel.Default) && (ctx.ResponseAge != TimeSpan.MinValue)) && (((ctx.ResponseAge > ctx.Policy.MaxAge) || (((ctx.ResponseExpires != DateTime.MinValue) && (ctx.Policy.MinFresh > TimeSpan.Zero)) && ((ctx.ResponseExpires - DateTime.UtcNow) < ctx.Policy.MinFresh))) || ((ctx.Policy.MaxStale > TimeSpan.Zero) && ((DateTime.UtcNow - ctx.ResponseExpires) > ctx.Policy.MaxStale))))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_freshness_outside_policy_limits"));
                    }
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
                if (ctx.RequestIfHeader1 != null)
                {
                    ctx.Request.Headers.RemoveInternal(ctx.RequestIfHeader1);
                    ctx.RequestIfHeader1 = null;
                }
                if (ctx.RequestIfHeader2 != null)
                {
                    ctx.Request.Headers.RemoveInternal(ctx.RequestIfHeader2);
                    ctx.RequestIfHeader2 = null;
                }
                if (ctx.RequestRangeCache)
                {
                    ctx.Request.Headers.RemoveInternal("Range");
                    ctx.RequestRangeCache = false;
                }
            }
            return CacheValidationStatus.Continue;
        }

        internal static class Common
        {
            public const string OkDescription = "OK";
            public const string PartialContentDescription = "Partial Content";

            private static bool AsciiLettersNoCaseEqual(string s1, string s2)
            {
                if (s1.Length != s2.Length)
                {
                    return false;
                }
                for (int i = 0; i < s1.Length; i++)
                {
                    if ((s1[i] | ' ') != (s2[i] | ' '))
                    {
                        return false;
                    }
                }
                return true;
            }

            private static Rfc2616.TriState CheckForRangeRequest(HttpRequestCacheValidator ctx, out string ranges)
            {
                if ((ranges = ctx.Request.Headers["Range"]) != null)
                {
                    ctx.RequestRangeUser = true;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_range_request_range", new object[] { ctx.Request.Headers["Range"] }));
                    }
                    return Rfc2616.TriState.Invalid;
                }
                if ((ctx.CacheStatusCode == HttpStatusCode.PartialContent) && (ctx.CacheEntityLength == ctx.CacheEntry.StreamSize))
                {
                    ctx.CacheStatusCode = HttpStatusCode.OK;
                    ctx.CacheStatusDescription = "OK";
                    return Rfc2616.TriState.Unknown;
                }
                if ((!ctx.CacheEntry.IsPartialEntry && ((ctx.CacheEntityLength == -1L) || (ctx.CacheEntityLength == ctx.CacheEntry.StreamSize))) && (ctx.CacheStatusCode != HttpStatusCode.PartialContent))
                {
                    return Rfc2616.TriState.Unknown;
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_could_be_partial", new object[] { ctx.CacheEntry.StreamSize, ctx.CacheEntityLength }));
                }
                return Rfc2616.TriState.Valid;
            }

            public static CacheFreshnessStatus ComputeFreshness(HttpRequestCacheValidator ctx)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_now_time", new object[] { DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture) }));
                }
                DateTime utcNow = DateTime.UtcNow;
                TimeSpan maxValue = TimeSpan.MaxValue;
                DateTime cacheDate = ctx.CacheDate;
                if (cacheDate != DateTime.MinValue)
                {
                    maxValue = (TimeSpan) (utcNow - cacheDate);
                    if (Logging.On)
                    {
                        object[] args = new object[] { ((int) maxValue.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheDate.ToString("r", CultureInfo.InvariantCulture) };
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age1_date_header", args));
                    }
                }
                else if (ctx.CacheEntry.LastSynchronizedUtc != DateTime.MinValue)
                {
                    maxValue = (TimeSpan) (utcNow - ctx.CacheEntry.LastSynchronizedUtc);
                    if (ctx.CacheAge != TimeSpan.MinValue)
                    {
                        maxValue += ctx.CacheAge;
                    }
                    if (Logging.On)
                    {
                        if (ctx.CacheAge != TimeSpan.MinValue)
                        {
                            object[] objArray3 = new object[] { ((int) maxValue.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture), ((int) ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age1_last_synchronized_age_header", objArray3));
                        }
                        else
                        {
                            object[] objArray4 = new object[] { ((int) maxValue.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age1_last_synchronized", objArray4));
                        }
                    }
                }
                if (ctx.CacheAge != TimeSpan.MinValue)
                {
                    if (Logging.On)
                    {
                        object[] objArray5 = new object[] { ((int) ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age2", objArray5));
                    }
                    if ((ctx.CacheAge > maxValue) || (maxValue == TimeSpan.MaxValue))
                    {
                        maxValue = ctx.CacheAge;
                    }
                }
                ctx.CacheAge = (maxValue < TimeSpan.Zero) ? TimeSpan.Zero : maxValue;
                if (ctx.CacheAge != TimeSpan.MinValue)
                {
                    if (!ctx.CacheEntry.IsPrivateEntry && (ctx.CacheCacheControl.SMaxAge != -1))
                    {
                        ctx.CacheMaxAge = TimeSpan.FromSeconds((double) ctx.CacheCacheControl.SMaxAge);
                        if (Logging.On)
                        {
                            object[] objArray6 = new object[] { ((int) ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_max_age_cache_s_max_age", objArray6));
                        }
                        if (ctx.CacheAge < ctx.CacheMaxAge)
                        {
                            return CacheFreshnessStatus.Fresh;
                        }
                        return CacheFreshnessStatus.Stale;
                    }
                    if (ctx.CacheCacheControl.MaxAge != -1)
                    {
                        ctx.CacheMaxAge = TimeSpan.FromSeconds((double) ctx.CacheCacheControl.MaxAge);
                        if (Logging.On)
                        {
                            object[] objArray7 = new object[] { ((int) ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_max_age_cache_max_age", objArray7));
                        }
                        if (ctx.CacheAge < ctx.CacheMaxAge)
                        {
                            return CacheFreshnessStatus.Fresh;
                        }
                        return CacheFreshnessStatus.Stale;
                    }
                }
                if (cacheDate == DateTime.MinValue)
                {
                    cacheDate = ctx.CacheEntry.LastSynchronizedUtc;
                }
                DateTime expiresUtc = ctx.CacheEntry.ExpiresUtc;
                if ((ctx.CacheExpires != DateTime.MinValue) && (ctx.CacheExpires < expiresUtc))
                {
                    expiresUtc = ctx.CacheExpires;
                }
                if (((expiresUtc != DateTime.MinValue) && (cacheDate != DateTime.MinValue)) && (ctx.CacheAge != TimeSpan.MinValue))
                {
                    ctx.CacheMaxAge = (TimeSpan) (expiresUtc - cacheDate);
                    if (Logging.On)
                    {
                        object[] objArray8 = new object[2];
                        TimeSpan span7 = (TimeSpan) (expiresUtc - cacheDate);
                        objArray8[0] = ((int) span7.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                        objArray8[1] = expiresUtc.ToString("r", CultureInfo.InvariantCulture);
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_max_age_expires_date", objArray8));
                    }
                    if (ctx.CacheAge < ctx.CacheMaxAge)
                    {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }
                if (expiresUtc != DateTime.MinValue)
                {
                    ctx.CacheMaxAge = (TimeSpan) (expiresUtc - DateTime.UtcNow);
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_max_age_absolute", new object[] { expiresUtc.ToString("r", CultureInfo.InvariantCulture) }));
                    }
                    if (expiresUtc < DateTime.UtcNow)
                    {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }
                ctx.HeuristicExpiration = true;
                DateTime lastModifiedUtc = ctx.CacheEntry.LastModifiedUtc;
                if (ctx.CacheLastModified > lastModifiedUtc)
                {
                    lastModifiedUtc = ctx.CacheLastModified;
                }
                ctx.CacheMaxAge = ctx.UnspecifiedMaxAge;
                if (lastModifiedUtc != DateTime.MinValue)
                {
                    TimeSpan span2 = (TimeSpan) (utcNow - lastModifiedUtc);
                    int num = (int) (span2.TotalSeconds / 10.0);
                    ctx.CacheMaxAge = TimeSpan.FromSeconds((double) num);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_no_max_age_use_10_percent", new object[] { num.ToString(NumberFormatInfo.InvariantInfo), lastModifiedUtc.ToString("r", CultureInfo.InvariantCulture) }));
                    }
                    if (ctx.CacheAge.TotalSeconds < num)
                    {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }
                ctx.CacheMaxAge = ctx.UnspecifiedMaxAge;
                if (Logging.On)
                {
                    object[] objArray11 = new object[] { ((int) ctx.UnspecifiedMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_no_max_age_use_default", objArray11));
                }
                if (ctx.CacheMaxAge >= ctx.CacheAge)
                {
                    return CacheFreshnessStatus.Fresh;
                }
                return CacheFreshnessStatus.Stale;
            }

            public static void Construct200ok(HttpRequestCacheValidator ctx)
            {
                ctx.CacheStatusCode = HttpStatusCode.OK;
                ctx.CacheStatusDescription = "OK";
                if (ctx.CacheHttpVersion == null)
                {
                    ctx.CacheHttpVersion = new Version(1, 1);
                }
                ctx.CacheHeaders.Remove("Content-Range");
                if (ctx.CacheEntityLength == -1L)
                {
                    ctx.CacheHeaders.Remove("Content-Length");
                }
                else
                {
                    ctx.CacheHeaders["Content-Length"] = ctx.CacheEntityLength.ToString(NumberFormatInfo.InvariantInfo);
                }
                ctx.CacheEntry.IsPartialEntry = false;
            }

            public static void Construct206PartialContent(HttpRequestCacheValidator ctx, int rangeStart)
            {
                ctx.CacheStatusCode = HttpStatusCode.PartialContent;
                ctx.CacheStatusDescription = "Partial Content";
                if (ctx.CacheHttpVersion == null)
                {
                    ctx.CacheHttpVersion = new Version(1, 1);
                }
                string str = string.Concat(new object[] { "bytes ", rangeStart, '-', (rangeStart + ctx.CacheStreamLength) - 1L, '/', (ctx.CacheEntityLength <= 0L) ? "*" : ctx.CacheEntityLength.ToString(NumberFormatInfo.InvariantInfo) });
                ctx.CacheHeaders["Content-Range"] = str;
                ctx.CacheHeaders["Content-Length"] = ctx.CacheStreamLength.ToString(NumberFormatInfo.InvariantInfo);
                ctx.CacheEntry.IsPartialEntry = true;
            }

            public static CacheValidationStatus ConstructConditionalRequest(HttpRequestCacheValidator ctx)
            {
                CacheValidationStatus doNotTakeFromCache = CacheValidationStatus.DoNotTakeFromCache;
                bool flag = false;
                string eTag = ctx.CacheHeaders.ETag;
                if (eTag != null)
                {
                    doNotTakeFromCache = CacheValidationStatus.Continue;
                    ctx.Request.Headers["If-None-Match"] = eTag;
                    ctx.RequestIfHeader1 = "If-None-Match";
                    ctx.RequestValidator1 = eTag;
                    flag = true;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_condition_if_none_match", new object[] { ctx.Request.Headers["If-None-Match"] }));
                    }
                }
                if (ctx.CacheEntry.LastModifiedUtc != DateTime.MinValue)
                {
                    doNotTakeFromCache = CacheValidationStatus.Continue;
                    eTag = ctx.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture);
                    ctx.Request.Headers.ChangeInternal("If-Modified-Since", eTag);
                    if (flag)
                    {
                        ctx.RequestIfHeader2 = "If-Modified-Since";
                        ctx.RequestValidator2 = eTag;
                    }
                    else
                    {
                        ctx.RequestIfHeader1 = "If-Modified-Since";
                        ctx.RequestValidator1 = eTag;
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_condition_if_modified_since", new object[] { ctx.Request.Headers["If-Modified-Since"] }));
                    }
                }
                if (Logging.On && (doNotTakeFromCache == CacheValidationStatus.DoNotTakeFromCache))
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_cannot_construct_conditional_request"));
                }
                return doNotTakeFromCache;
            }

            public static void ConstructUnconditionalRefreshRequest(HttpRequestCacheValidator ctx)
            {
                WebHeaderCollection headers = ctx.Request.Headers;
                headers["Cache-Control"] = "max-age=0";
                headers["Pragma"] = "no-cache";
                if (ctx.RequestIfHeader1 != null)
                {
                    headers.RemoveInternal(ctx.RequestIfHeader1);
                    ctx.RequestIfHeader1 = null;
                }
                if (ctx.RequestIfHeader2 != null)
                {
                    headers.RemoveInternal(ctx.RequestIfHeader2);
                    ctx.RequestIfHeader2 = null;
                }
                if (ctx.RequestRangeCache)
                {
                    headers.RemoveInternal("Range");
                    ctx.RequestRangeCache = false;
                }
            }

            public static bool GetBytesRange(string ranges, ref long start, ref long end, ref long total, bool isRequest)
            {
                ranges = ranges.ToLower(CultureInfo.InvariantCulture);
                int num = 0;
                while ((num < ranges.Length) && (ranges[num] == ' '))
                {
                    num++;
                }
                num += 5;
                if ((((num >= ranges.Length) || (ranges[num - 5] != 'b')) || ((ranges[num - 4] != 'y') || (ranges[num - 3] != 't'))) || ((ranges[num - 2] != 'e') || (ranges[num - 1] != 's')))
                {
                    if (Logging.On)
                    {
                        Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_only_byte_range_implemented"));
                    }
                    return false;
                }
                if (isRequest)
                {
                    while ((num < ranges.Length) && (ranges[num] == ' '))
                    {
                        num++;
                    }
                    if (ranges[num] != '=')
                    {
                        return false;
                    }
                }
                else if (ranges[num] != ' ')
                {
                    return false;
                }
                char ch = '\0';
                while ((++num < ranges.Length) && ((ch = ranges[num]) == ' '))
                {
                }
                start = -1L;
                if (ch != '-')
                {
                    if (((num < ranges.Length) && (ch >= '0')) && (ch <= '9'))
                    {
                        start = ch - '0';
                        while (((++num < ranges.Length) && ((ch = ranges[num]) >= '0')) && (ch <= '9'))
                        {
                            start = (start * 10L) + (ch - '0');
                        }
                    }
                    while ((num < ranges.Length) && (ch == ' '))
                    {
                        ch = ranges[++num];
                    }
                    if (ch != '-')
                    {
                        return false;
                    }
                }
                while ((num < ranges.Length) && ((ch = ranges[++num]) == ' '))
                {
                }
                end = -1L;
                if (((num < ranges.Length) && (ch >= '0')) && (ch <= '9'))
                {
                    end = ch - '0';
                    while (((++num < ranges.Length) && ((ch = ranges[num]) >= '0')) && (ch <= '9'))
                    {
                        end = (end * 10L) + (ch - '0');
                    }
                }
                if (!isRequest)
                {
                    while ((num < ranges.Length) && ((ch = ranges[num]) == ' '))
                    {
                        num++;
                    }
                    if (ch != '/')
                    {
                        return false;
                    }
                    while ((++num < ranges.Length) && ((ch = ranges[num]) == ' '))
                    {
                    }
                    total = -1L;
                    if (((ch != '*') && (num < ranges.Length)) && ((ch >= '0') && (ch <= '9')))
                    {
                        total = ch - '0';
                        while (((++num < ranges.Length) && ((ch = ranges[num]) >= '0')) && (ch <= '9'))
                        {
                            total = (total * 10L) + (ch - '0');
                        }
                    }
                }
                else
                {
                    while (num < ranges.Length)
                    {
                        if (ranges[num++] != ' ')
                        {
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_multiple_complex_range_not_implemented"));
                            }
                            return false;
                        }
                    }
                }
                return (isRequest || ((start != -1L) && (end != -1L)));
            }

            internal static Rfc2616.TriState OnUpdateCache(HttpRequestCacheValidator ctx, HttpWebResponse resp)
            {
                if (((ctx.RequestMethod != HttpMethod.Head) && (ctx.RequestMethod != HttpMethod.Get)) && (ctx.RequestMethod != HttpMethod.Post))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_a_get_head_post"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if ((ctx.CacheStream == Stream.Null) || (ctx.CacheStatusCode == ((HttpStatusCode) 0)))
                {
                    if (resp.StatusCode == HttpStatusCode.NotModified)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_cannot_update_cache_if_304"));
                        }
                        return Rfc2616.TriState.Unknown;
                    }
                    if (ctx.RequestMethod == HttpMethod.Head)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_cannot_update_cache_with_head_resp"));
                        }
                        return Rfc2616.TriState.Unknown;
                    }
                }
                if (resp == null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_http_resp_is_null"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if (ctx.ResponseCacheControl.NoStore)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_cache_control_is_no_store"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if (((ctx.ResponseDate != DateTime.MinValue) && (ctx.CacheDate != DateTime.MinValue)) && (ctx.ResponseDate < ctx.CacheDate))
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_resp_older_than_cache"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if (ctx.ResponseCacheControl.Public)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_cache_control_is_public"));
                    }
                    return Rfc2616.TriState.Valid;
                }
                Rfc2616.TriState unknown = Rfc2616.TriState.Unknown;
                if (ctx.ResponseCacheControl.Private)
                {
                    if (!ctx.CacheEntry.IsPrivateEntry)
                    {
                        if (ctx.ResponseCacheControl.PrivateHeaders == null)
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_cache_control_is_private"));
                            }
                            return Rfc2616.TriState.Unknown;
                        }
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_cache_control_is_private_plus_headers"));
                        }
                        for (int i = 0; i < ctx.ResponseCacheControl.PrivateHeaders.Length; i++)
                        {
                            ctx.CacheHeaders.Remove(ctx.ResponseCacheControl.PrivateHeaders[i]);
                            unknown = Rfc2616.TriState.Valid;
                        }
                    }
                    else
                    {
                        unknown = Rfc2616.TriState.Valid;
                    }
                }
                if (ctx.ResponseCacheControl.NoCache)
                {
                    if ((ctx.ResponseLastModified == DateTime.MinValue) && (ctx.Response.Headers.ETag == null))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_revalidation_required"));
                        }
                        return Rfc2616.TriState.Unknown;
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_needs_revalidation"));
                    }
                    return Rfc2616.TriState.Valid;
                }
                if ((ctx.ResponseCacheControl.SMaxAge != -1) || (ctx.ResponseCacheControl.MaxAge != -1))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_allows_caching", new object[] { ctx.ResponseCacheControl.ToString() }));
                    }
                    return Rfc2616.TriState.Valid;
                }
                if (!ctx.CacheEntry.IsPrivateEntry && (ctx.Request.Headers["Authorization"] != null))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_auth_header_and_no_s_max_age"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if ((ctx.RequestMethod == HttpMethod.Post) && (resp.Headers.Expires == null))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_post_resp_without_cache_control_or_expires"));
                    }
                    return Rfc2616.TriState.Unknown;
                }
                if ((((resp.StatusCode == HttpStatusCode.NotModified) || (resp.StatusCode == HttpStatusCode.OK)) || ((resp.StatusCode == HttpStatusCode.NonAuthoritativeInformation) || (resp.StatusCode == HttpStatusCode.PartialContent))) || (((resp.StatusCode == HttpStatusCode.MultipleChoices) || (resp.StatusCode == HttpStatusCode.MovedPermanently)) || (resp.StatusCode == HttpStatusCode.Gone)))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_valid_based_on_status_code", new object[] { (int) resp.StatusCode }));
                    }
                    return Rfc2616.TriState.Valid;
                }
                if ((unknown != Rfc2616.TriState.Valid) && Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_resp_no_cache_control", new object[] { (int) resp.StatusCode }));
                }
                return unknown;
            }

            public static CacheValidationStatus OnValidateRequest(HttpRequestCacheValidator ctx)
            {
                if ((ctx.RequestMethod >= HttpMethod.Post) && (ctx.RequestMethod <= HttpMethod.Delete))
                {
                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                    {
                        ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                    }
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if ((ctx.RequestMethod < HttpMethod.Head) || (ctx.RequestMethod > HttpMethod.Get))
                {
                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                    {
                        ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                    }
                    return CacheValidationStatus.DoNotUseCache;
                }
                if (((ctx.Request.Headers["If-Modified-Since"] == null) && (ctx.Request.Headers["If-None-Match"] == null)) && (((ctx.Request.Headers["If-Range"] == null) && (ctx.Request.Headers["If-Match"] == null)) && (ctx.Request.Headers["If-Unmodified-Since"] == null)))
                {
                    return CacheValidationStatus.Continue;
                }
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_request_contains_conditional_header"));
                }
                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                {
                    ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            public static void ReplaceOrUpdateCacheHeaders(HttpRequestCacheValidator ctx, HttpWebResponse resp)
            {
                if ((ctx.CacheHeaders == null) || ((resp.StatusCode != HttpStatusCode.NotModified) && (resp.StatusCode != HttpStatusCode.PartialContent)))
                {
                    ctx.CacheHeaders = new WebHeaderCollection();
                }
                string[] values = resp.Headers.GetValues("Vary");
                if (values != null)
                {
                    ArrayList list = new ArrayList();
                    HttpRequestCacheValidator.ParseHeaderValues(values, HttpRequestCacheValidator.ParseValuesCallback, list);
                    if ((list.Count != 0) && (((string) list[0])[0] != '*'))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_saving_request_headers", new object[] { resp.Headers["Vary"] }));
                        }
                        if (ctx.SystemMeta == null)
                        {
                            ctx.SystemMeta = new NameValueCollection(list.Count + 1, CaseInsensitiveAscii.StaticInstance);
                        }
                        for (int j = 0; j < list.Count; j++)
                        {
                            string str = ctx.Request.Headers[(string) list[j]];
                            ctx.SystemMeta[(string) list[j]] = str;
                        }
                    }
                }
                for (int i = 0; i < ctx.Response.Headers.Count; i++)
                {
                    string key = ctx.Response.Headers.GetKey(i);
                    if ((((!AsciiLettersNoCaseEqual(key, "Connection") && !AsciiLettersNoCaseEqual(key, "Keep-Alive")) && (!AsciiLettersNoCaseEqual(key, "Proxy-Authenticate") && !AsciiLettersNoCaseEqual(key, "Proxy-Authorization"))) && ((!AsciiLettersNoCaseEqual(key, "TE") && !AsciiLettersNoCaseEqual(key, "Transfer-Encoding")) && (!AsciiLettersNoCaseEqual(key, "Trailer") && !AsciiLettersNoCaseEqual(key, "Upgrade")))) && ((resp.StatusCode != HttpStatusCode.NotModified) || !AsciiLettersNoCaseEqual(key, "Content-Length")))
                    {
                        ctx.CacheHeaders.ChangeInternal(key, ctx.Response.Headers[i]);
                    }
                }
            }

            private static bool TryConditionalRangeRequest(HttpRequestCacheValidator ctx)
            {
                if (ctx.CacheEntry.StreamSize >= 0x7fffffffL)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_entry_size_too_big", new object[] { ctx.CacheEntry.StreamSize }));
                    }
                    return false;
                }
                string eTag = ctx.CacheHeaders.ETag;
                if (eTag != null)
                {
                    ctx.Request.Headers["If-Range"] = eTag;
                    ctx.RequestIfHeader1 = "If-Range";
                    ctx.RequestValidator1 = eTag;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_condition_if_range", new object[] { ctx.Request.Headers["If-Range"] }));
                    }
                    return true;
                }
                if (ctx.CacheEntry.LastModifiedUtc != DateTime.MinValue)
                {
                    eTag = ctx.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture);
                    if ((ctx.CacheHttpVersion.Major == 1) && (ctx.CacheHttpVersion.Minor == 0))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_conditional_range_not_implemented_on_http_10"));
                        }
                        return false;
                    }
                    ctx.Request.Headers["If-Range"] = eTag;
                    ctx.RequestIfHeader1 = "If-Range";
                    ctx.RequestValidator1 = eTag;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_condition_if_range", new object[] { ctx.Request.Headers["If-Range"] }));
                    }
                    return true;
                }
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_cannot_construct_conditional_range_request"));
                }
                return false;
            }

            public static CacheValidationStatus TryConditionalRequest(HttpRequestCacheValidator ctx)
            {
                string str;
                Rfc2616.TriState state = CheckForRangeRequest(ctx, out str);
                if (state != Rfc2616.TriState.Invalid)
                {
                    if (state != Rfc2616.TriState.Valid)
                    {
                        return ConstructConditionalRequest(ctx);
                    }
                    if (ctx is FtpRequestCacheValidator)
                    {
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    if (!TryConditionalRangeRequest(ctx))
                    {
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    ctx.RequestRangeCache = true;
                    ((HttpWebRequest) ctx.Request).AddRange((int) ctx.CacheEntry.StreamSize);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_range", new object[] { ctx.Request.Headers["Range"] }));
                    }
                }
                return CacheValidationStatus.Continue;
            }

            public static CacheValidationStatus TryResponseFromCache(HttpRequestCacheValidator ctx)
            {
                string str;
                switch (CheckForRangeRequest(ctx, out str))
                {
                    case Rfc2616.TriState.Unknown:
                        return CacheValidationStatus.ReturnCachedResponse;

                    case Rfc2616.TriState.Invalid:
                    {
                        long start = 0L;
                        long end = 0L;
                        long total = 0L;
                        if (!GetBytesRange(str, ref start, ref end, ref total, true))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_range_invalid_format", new object[] { str }));
                            }
                            return CacheValidationStatus.DoNotTakeFromCache;
                        }
                        if (((((start >= ctx.CacheEntry.StreamSize) || (end > ctx.CacheEntry.StreamSize)) || ((end == -1L) && (ctx.CacheEntityLength == -1L))) || ((end == -1L) && (ctx.CacheEntityLength > ctx.CacheEntry.StreamSize))) || ((start == -1L) && (((end == -1L) || (ctx.CacheEntityLength == -1L)) || ((ctx.CacheEntityLength - end) >= ctx.CacheEntry.StreamSize))))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_range_not_in_cache", new object[] { str }));
                            }
                            return CacheValidationStatus.Continue;
                        }
                        if (start == -1L)
                        {
                            start = ctx.CacheEntityLength - end;
                        }
                        if (end <= 0L)
                        {
                            end = ctx.CacheEntry.StreamSize - 1L;
                        }
                        ctx.CacheStreamOffset = start;
                        ctx.CacheStreamLength = (end - start) + 1L;
                        Construct206PartialContent(ctx, (int) start);
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_range_in_cache", new object[] { ctx.CacheHeaders["Content-Range"] }));
                        }
                        return CacheValidationStatus.ReturnCachedResponse;
                    }
                }
                if ((ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly) && ((ctx.Uri.Scheme == Uri.UriSchemeHttp) || (ctx.Uri.Scheme == Uri.UriSchemeHttps)))
                {
                    ctx.CacheStreamOffset = 0L;
                    ctx.CacheStreamLength = ctx.CacheEntry.StreamSize;
                    Construct206PartialContent(ctx, 0);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_partial_resp", new object[] { ctx.CacheHeaders["Content-Range"] }));
                    }
                    return CacheValidationStatus.ReturnCachedResponse;
                }
                if (ctx.CacheEntry.StreamSize >= 0x7fffffffL)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_entry_size_too_big", new object[] { ctx.CacheEntry.StreamSize }));
                    }
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (TryConditionalRangeRequest(ctx))
                {
                    ctx.RequestRangeCache = true;
                    ((HttpWebRequest) ctx.Request).AddRange((int) ctx.CacheEntry.StreamSize);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_range", new object[] { ctx.Request.Headers["Range"] }));
                    }
                }
                return CacheValidationStatus.Continue;
            }

            internal static unsafe bool UnsafeAsciiLettersNoCaseEqual(char* s1, int start, int length, string s2)
            {
                if ((length - start) < s2.Length)
                {
                    return false;
                }
                for (int i = 0; i < s2.Length; i++)
                {
                    if ((s1[start + i] | ' ') != (s2[i] | ' '))
                    {
                        return false;
                    }
                }
                return true;
            }

            public static CacheValidationStatus ValidateCacheAfterResponse(HttpRequestCacheValidator ctx, HttpWebResponse resp)
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_after_validation"));
                }
                if (((ctx.CacheStream == Stream.Null) || (ctx.CacheStatusCode == ((HttpStatusCode) 0))) && (resp.StatusCode == HttpStatusCode.NotModified))
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_resp_status_304"));
                    }
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                if (ctx.RequestMethod == HttpMethod.Head)
                {
                    bool flag = false;
                    if ((ctx.ResponseEntityLength != -1L) && (ctx.ResponseEntityLength != ctx.CacheEntityLength))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_head_resp_has_different_content_length"));
                        }
                        flag = true;
                    }
                    if (resp.Headers["Content-MD5"] != ctx.CacheHeaders["Content-MD5"])
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_head_resp_has_different_content_md5"));
                        }
                        flag = true;
                    }
                    if (resp.Headers.ETag != ctx.CacheHeaders.ETag)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_head_resp_has_different_etag"));
                        }
                        flag = true;
                    }
                    if ((resp.StatusCode != HttpStatusCode.NotModified) && (resp.Headers.LastModified != ctx.CacheHeaders.LastModified))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_304_head_resp_has_different_last_modified"));
                        }
                        flag = true;
                    }
                    if (flag)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_existing_entry_has_to_be_discarded"));
                        }
                        return CacheValidationStatus.RemoveFromCache;
                    }
                }
                if (resp.StatusCode == HttpStatusCode.PartialContent)
                {
                    if ((ctx.CacheHeaders.ETag != ctx.Response.Headers.ETag) || ((ctx.CacheHeaders.LastModified != ctx.Response.Headers.LastModified) && ((ctx.Response.Headers.LastModified != null) || (ctx.Response.Headers.ETag == null))))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_206_resp_non_matching_entry"));
                        }
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_existing_entry_should_be_discarded"));
                        }
                        return CacheValidationStatus.RemoveFromCache;
                    }
                    if (ctx.CacheEntry.StreamSize != ctx.ResponseRangeStart)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_206_resp_starting_position_not_adjusted"));
                        }
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    ReplaceOrUpdateCacheHeaders(ctx, resp);
                    if (ctx.RequestRangeUser)
                    {
                        ctx.CacheStreamOffset = ctx.CacheEntry.StreamSize;
                        ctx.CacheStreamLength = (ctx.ResponseRangeEnd - ctx.ResponseRangeStart) + 1L;
                        ctx.CacheEntityLength = ctx.ResponseEntityLength;
                        ctx.CacheStatusCode = resp.StatusCode;
                        ctx.CacheStatusDescription = resp.StatusDescription;
                        ctx.CacheHttpVersion = resp.ProtocolVersion;
                    }
                    else
                    {
                        ctx.CacheStreamOffset = 0L;
                        ctx.CacheStreamLength = ctx.ResponseEntityLength;
                        ctx.CacheEntityLength = ctx.ResponseEntityLength;
                        ctx.CacheStatusCode = HttpStatusCode.OK;
                        ctx.CacheStatusDescription = "OK";
                        ctx.CacheHttpVersion = resp.ProtocolVersion;
                        ctx.CacheHeaders.Remove("Content-Range");
                        if (ctx.CacheStreamLength == -1L)
                        {
                            ctx.CacheHeaders.Remove("Content-Length");
                        }
                        else
                        {
                            ctx.CacheHeaders["Content-Length"] = ctx.CacheStreamLength.ToString(NumberFormatInfo.InvariantInfo);
                        }
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_combined_resp_requested"));
                    }
                    return CacheValidationStatus.CombineCachedAndServerResponse;
                }
                if (resp.StatusCode == HttpStatusCode.NotModified)
                {
                    WebHeaderCollection headers = resp.Headers;
                    string str = null;
                    string str2 = null;
                    if (((((ctx.CacheExpires != ctx.ResponseExpires) || (ctx.CacheLastModified != ctx.ResponseLastModified)) || ((ctx.CacheDate != ctx.ResponseDate) || ctx.ResponseCacheControl.IsNotEmpty)) || (((str = headers["Content-Location"]) != null) && (str != ctx.CacheHeaders["Content-Location"]))) || (((str2 = headers.ETag) != null) && (str2 != ctx.CacheHeaders.ETag)))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_updating_headers_on_304"));
                        }
                        ReplaceOrUpdateCacheHeaders(ctx, resp);
                        return CacheValidationStatus.ReturnCachedResponse;
                    }
                    int num = 0;
                    if (str2 != null)
                    {
                        num++;
                    }
                    if (str != null)
                    {
                        num++;
                    }
                    if (ctx.ResponseAge != TimeSpan.MinValue)
                    {
                        num++;
                    }
                    if (ctx.ResponseLastModified != DateTime.MinValue)
                    {
                        num++;
                    }
                    if (ctx.ResponseExpires != DateTime.MinValue)
                    {
                        num++;
                    }
                    if (ctx.ResponseDate != DateTime.MinValue)
                    {
                        num++;
                    }
                    if (headers.Via != null)
                    {
                        num++;
                    }
                    if (headers["Connection"] != null)
                    {
                        num++;
                    }
                    if (headers["Keep-Alive"] != null)
                    {
                        num++;
                    }
                    if (headers.ProxyAuthenticate != null)
                    {
                        num++;
                    }
                    if (headers["Proxy-Authorization"] != null)
                    {
                        num++;
                    }
                    if (headers["TE"] != null)
                    {
                        num++;
                    }
                    if (headers["Transfer-Encoding"] != null)
                    {
                        num++;
                    }
                    if (headers["Trailer"] != null)
                    {
                        num++;
                    }
                    if (headers["Upgrade"] != null)
                    {
                        num++;
                    }
                    if (resp.Headers.Count <= num)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_suppressing_headers_update_on_304"));
                        }
                        ctx.CacheDontUpdateHeaders = true;
                    }
                    else
                    {
                        ReplaceOrUpdateCacheHeaders(ctx, resp);
                    }
                    return CacheValidationStatus.ReturnCachedResponse;
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_status_code_not_304_206"));
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            public static bool ValidateCacheByClientPolicy(HttpRequestCacheValidator ctx)
            {
                if (ctx.Policy.Level == HttpRequestCacheLevel.Default)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_age", new object[] { (ctx.CacheAge != TimeSpan.MinValue) ? ((int) ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) : SR.GetString("net_log_unknown"), (ctx.CacheMaxAge != TimeSpan.MinValue) ? ((int) ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) : SR.GetString("net_log_unknown") }));
                    }
                    if (ctx.Policy.MinFresh > TimeSpan.Zero)
                    {
                        if (Logging.On)
                        {
                            object[] args = new object[] { ((int) ctx.Policy.MinFresh.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_policy_min_fresh", args));
                        }
                        if ((ctx.CacheAge + ctx.Policy.MinFresh) >= ctx.CacheMaxAge)
                        {
                            return false;
                        }
                    }
                    if (ctx.Policy.MaxAge != TimeSpan.MaxValue)
                    {
                        if (Logging.On)
                        {
                            object[] objArray3 = new object[] { ((int) ctx.Policy.MaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_policy_max_age", objArray3));
                        }
                        if (ctx.CacheAge >= ctx.Policy.MaxAge)
                        {
                            return false;
                        }
                    }
                    if (ctx.Policy.InternalCacheSyncDateUtc != DateTime.MinValue)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_policy_cache_sync_date", new object[] { ctx.Policy.InternalCacheSyncDateUtc.ToString("r", CultureInfo.CurrentCulture), ctx.CacheEntry.LastSynchronizedUtc.ToString(CultureInfo.CurrentCulture) }));
                        }
                        if (ctx.CacheEntry.LastSynchronizedUtc < ctx.Policy.InternalCacheSyncDateUtc)
                        {
                            return false;
                        }
                    }
                    TimeSpan cacheMaxAge = ctx.CacheMaxAge;
                    if (ctx.Policy.MaxStale > TimeSpan.Zero)
                    {
                        if (Logging.On)
                        {
                            object[] objArray5 = new object[] { ((int) ctx.Policy.MaxStale.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) };
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_policy_max_stale", objArray5));
                        }
                        if (cacheMaxAge < (TimeSpan.MaxValue - ctx.Policy.MaxStale))
                        {
                            cacheMaxAge += ctx.Policy.MaxStale;
                        }
                        else
                        {
                            cacheMaxAge = TimeSpan.MaxValue;
                        }
                        if (ctx.CacheAge >= cacheMaxAge)
                        {
                            return false;
                        }
                        return true;
                    }
                }
                return (ctx.CacheFreshnessStatus == CacheFreshnessStatus.Fresh);
            }

            internal static Rfc2616.TriState ValidateCacheBySpecialCases(HttpRequestCacheValidator ctx)
            {
                if (ctx.CacheCacheControl.NoCache)
                {
                    if (ctx.CacheCacheControl.NoCacheHeaders == null)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_control_no_cache"));
                        }
                        return Rfc2616.TriState.Invalid;
                    }
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_control_no_cache_removing_some_headers"));
                    }
                    for (int i = 0; i < ctx.CacheCacheControl.NoCacheHeaders.Length; i++)
                    {
                        ctx.CacheHeaders.Remove(ctx.CacheCacheControl.NoCacheHeaders[i]);
                    }
                }
                if ((ctx.CacheCacheControl.MustRevalidate || (!ctx.CacheEntry.IsPrivateEntry && ctx.CacheCacheControl.ProxyRevalidate)) && (ctx.CacheFreshnessStatus != CacheFreshnessStatus.Fresh))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_control_must_revalidate"));
                    }
                    return Rfc2616.TriState.Invalid;
                }
                if (ctx.Request.Headers["Authorization"] != null)
                {
                    if (ctx.CacheFreshnessStatus != CacheFreshnessStatus.Fresh)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_cached_auth_header"));
                        }
                        return Rfc2616.TriState.Invalid;
                    }
                    if ((!ctx.CacheEntry.IsPrivateEntry && (ctx.CacheCacheControl.SMaxAge == -1)) && (!ctx.CacheCacheControl.MustRevalidate && !ctx.CacheCacheControl.Public))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_cached_auth_header_no_control_directive"));
                        }
                        return Rfc2616.TriState.Invalid;
                    }
                }
                return Rfc2616.TriState.Valid;
            }

            internal static Rfc2616.TriState ValidateCacheByVaryHeader(HttpRequestCacheValidator ctx)
            {
                string[] values = ctx.CacheHeaders.GetValues("Vary");
                if (values == null)
                {
                    return Rfc2616.TriState.Unknown;
                }
                ArrayList list = new ArrayList();
                HttpRequestCacheValidator.ParseHeaderValues(values, HttpRequestCacheValidator.ParseValuesCallback, list);
                if (list.Count == 0)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_vary_header_empty"));
                    }
                    return Rfc2616.TriState.Invalid;
                }
                if (((string) list[0])[0] == '*')
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_vary_header_contains_asterisks"));
                    }
                    return Rfc2616.TriState.Invalid;
                }
                if ((ctx.SystemMeta == null) || (ctx.SystemMeta.Count == 0))
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_no_headers_in_metadata"));
                    }
                    return Rfc2616.TriState.Invalid;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    string[] strArray2 = ctx.Request.Headers.GetValues((string) list[i]);
                    ArrayList list2 = new ArrayList();
                    if (strArray2 != null)
                    {
                        HttpRequestCacheValidator.ParseHeaderValues(strArray2, HttpRequestCacheValidator.ParseValuesCallback, list2);
                    }
                    string[] strArray3 = ctx.SystemMeta.GetValues((string) list[i]);
                    ArrayList list3 = new ArrayList();
                    if (strArray3 != null)
                    {
                        HttpRequestCacheValidator.ParseHeaderValues(strArray3, HttpRequestCacheValidator.ParseValuesCallback, list3);
                    }
                    if (list2.Count != list3.Count)
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_vary_header_mismatched_count", new object[] { (string) list[i] }));
                        }
                        return Rfc2616.TriState.Invalid;
                    }
                    for (int j = 0; j < list3.Count; j++)
                    {
                        if (!AsciiLettersNoCaseEqual((string) list3[j], (string) list2[j]))
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_vary_header_mismatched_field", new object[] { (string) list[i], (string) list3[j], (string) list2[j] }));
                            }
                            return Rfc2616.TriState.Invalid;
                        }
                    }
                }
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_vary_header_match"));
                }
                return Rfc2616.TriState.Valid;
            }

            public static CacheValidationStatus ValidateCacheOn5XXResponse(HttpRequestCacheValidator ctx)
            {
                if ((ctx.CacheStream != Stream.Null) && (ctx.CacheStatusCode != ((HttpStatusCode) 0)))
                {
                    if ((ctx.CacheEntityLength != ctx.CacheEntry.StreamSize) || (ctx.CacheStatusCode == HttpStatusCode.PartialContent))
                    {
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    if (ValidateCacheBySpecialCases(ctx) != Rfc2616.TriState.Valid)
                    {
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    if (((ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly) || (ctx.Policy.Level == HttpRequestCacheLevel.CacheIfAvailable)) || (ctx.Policy.Level == HttpRequestCacheLevel.CacheOrNextCacheOnly))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_sxx_resp_cache_only"));
                        }
                        return CacheValidationStatus.ReturnCachedResponse;
                    }
                    if (((ctx.Policy.Level == HttpRequestCacheLevel.Default) || (ctx.Policy.Level == HttpRequestCacheLevel.Revalidate)) && ValidateCacheByClientPolicy(ctx))
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_sxx_resp_can_be_replaced"));
                        }
                        ctx.CacheHeaders.Add("Warning", "111 Revalidation failed");
                        return CacheValidationStatus.ReturnCachedResponse;
                    }
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }
        }

        internal enum TriState
        {
            Unknown,
            Valid,
            Invalid
        }
    }
}

