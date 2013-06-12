namespace System.Net.Cache
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;

    internal class RequestCacheProtocol
    {
        private bool _CanTakeNewRequest;
        private bool _IsCacheFresh;
        private Exception _ProtocolException;
        private CacheValidationStatus _ProtocolStatus;
        private RequestCache _RequestCache;
        private Stream _ResponseStream;
        private long _ResponseStreamLength;
        private RequestCacheValidator _Validator;

        internal RequestCacheProtocol(RequestCache cache, RequestCacheValidator defaultValidator)
        {
            this._RequestCache = cache;
            this._Validator = defaultValidator;
            this._CanTakeNewRequest = true;
        }

        internal void Abort()
        {
            if (!this._CanTakeNewRequest)
            {
                Stream stream = this._ResponseStream;
                if (stream != null)
                {
                    try
                    {
                        if (Logging.On)
                        {
                            Logging.PrintWarning(Logging.RequestCache, SR.GetString("net_log_cache_closing_cache_stream", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), "Abort()", stream.GetType().FullName, this._Validator.CacheKey }));
                        }
                        ICloseEx ex = stream as ICloseEx;
                        if (ex != null)
                        {
                            ex.CloseEx(CloseExState.Silent | CloseExState.Abort);
                        }
                        else
                        {
                            stream.Close();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception))
                        {
                            throw;
                        }
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_exception_ignored", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), "stream.Close()", exception.ToString() }));
                        }
                    }
                }
                this.Reset();
            }
        }

        private void CheckRetrieveBeforeSubmit()
        {
            try
            {
                RequestCacheEntry entry;
            Label_0000:
                if ((this._Validator.CacheStream != null) && (this._Validator.CacheStream != Stream.Null))
                {
                    this._Validator.CacheStream.Close();
                    this._Validator.CacheStream = Stream.Null;
                }
                if (this._Validator.StrictCacheErrors)
                {
                    this._Validator.CacheStream = this._RequestCache.Retrieve(this._Validator.CacheKey, out entry);
                }
                else
                {
                    Stream stream;
                    this._RequestCache.TryRetrieve(this._Validator.CacheKey, out entry, out stream);
                    this._Validator.CacheStream = stream;
                }
                if (entry == null)
                {
                    entry = new RequestCacheEntry {
                        IsPrivateEntry = this._RequestCache.IsPrivateCache
                    };
                    this._Validator.FetchCacheEntry(entry);
                }
                if (this._Validator.CacheStream == null)
                {
                    this._Validator.CacheStream = Stream.Null;
                }
                this.ValidateFreshness(entry);
                this._ProtocolStatus = this.ValidateCache();
                switch (this._ProtocolStatus)
                {
                    case CacheValidationStatus.DoNotUseCache:
                    case CacheValidationStatus.DoNotTakeFromCache:
                        return;

                    case CacheValidationStatus.Fail:
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_fail", new object[] { "ValidateCache" }));
                        return;

                    case CacheValidationStatus.RetryResponseFromCache:
                        goto Label_0000;

                    case CacheValidationStatus.ReturnCachedResponse:
                        if ((this._Validator.CacheStream != null) && (this._Validator.CacheStream != Stream.Null))
                        {
                            break;
                        }
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_no_cache_entry", new object[] { "ValidateCache()" }));
                        }
                        this._ProtocolStatus = CacheValidationStatus.Fail;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_no_stream", new object[] { this._Validator.CacheKey }));
                        return;

                    case CacheValidationStatus.Continue:
                        this._ResponseStream = this._Validator.CacheStream;
                        return;

                    default:
                        goto Label_02CB;
                }
                Stream cacheStream = this._Validator.CacheStream;
                this._RequestCache.UnlockEntry(this._Validator.CacheStream);
                if ((this._Validator.CacheStreamOffset != 0L) || (this._Validator.CacheStreamLength != this._Validator.CacheEntry.StreamSize))
                {
                    cacheStream = new RangeStream(cacheStream, this._Validator.CacheStreamOffset, this._Validator.CacheStreamLength);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_returned_range_cache", new object[] { "ValidateCache()", this._Validator.CacheStreamOffset, this._Validator.CacheStreamLength }));
                    }
                }
                this._ResponseStream = cacheStream;
                this._ResponseStreamLength = this._Validator.CacheStreamLength;
                return;
            Label_02CB:
                this._ProtocolStatus = CacheValidationStatus.Fail;
                this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_result", new object[] { "ValidateCache", this._Validator.ValidationStatus.ToString() }));
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_unexpected_status", new object[] { "ValidateCache()", this._Validator.ValidationStatus.ToString() }));
                }
            }
            catch (Exception exception)
            {
                this._ProtocolStatus = CacheValidationStatus.Fail;
                this._ProtocolException = exception;
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_object_and_exception", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (exception is WebException) ? exception.Message : exception.ToString() }));
                }
            }
            finally
            {
                if (((this._ResponseStream == null) && (this._Validator.CacheStream != null)) && (this._Validator.CacheStream != Stream.Null))
                {
                    this._Validator.CacheStream.Close();
                    this._Validator.CacheStream = Stream.Null;
                }
            }
        }

        private void CheckRetrieveOnResponse(Stream responseStream)
        {
            bool flag = true;
            try
            {
                switch ((this._ProtocolStatus = this.ValidateResponse()))
                {
                    case CacheValidationStatus.DoNotUseCache:
                        goto Label_01CA;

                    case CacheValidationStatus.Fail:
                        this._ProtocolStatus = CacheValidationStatus.Fail;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_fail", new object[] { "ValidateResponse" }));
                        goto Label_01CA;

                    case CacheValidationStatus.RetryResponseFromServer:
                        flag = false;
                        goto Label_01CA;

                    case CacheValidationStatus.Continue:
                        flag = false;
                        goto Label_01CA;
                }
                this._ProtocolStatus = CacheValidationStatus.Fail;
                this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_result", new object[] { "ValidateResponse", this._Validator.ValidationStatus.ToString() }));
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_unexpected_status", new object[] { "ValidateResponse()", this._Validator.ValidationStatus.ToString() }));
                }
            }
            catch (Exception exception)
            {
                flag = true;
                this._ProtocolException = exception;
                this._ProtocolStatus = CacheValidationStatus.Fail;
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_object_and_exception", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (exception is WebException) ? exception.Message : exception.ToString() }));
                }
            }
            finally
            {
                if (flag && (this._ResponseStream != null))
                {
                    this._ResponseStream.Close();
                    this._ResponseStream = null;
                    this._Validator.CacheStream = Stream.Null;
                }
            }
        Label_01CA:
            if (this._ProtocolStatus != CacheValidationStatus.Continue)
            {
                return;
            }
            try
            {
                switch ((this._ProtocolStatus = this.RevalidateCache()))
                {
                    case CacheValidationStatus.DoNotUseCache:
                    case CacheValidationStatus.DoNotTakeFromCache:
                    case CacheValidationStatus.RemoveFromCache:
                        flag = true;
                        return;

                    case CacheValidationStatus.Fail:
                        flag = true;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_fail", new object[] { "RevalidateCache" }));
                        return;

                    case CacheValidationStatus.ReturnCachedResponse:
                        if ((this._Validator.CacheStream != null) && (this._Validator.CacheStream != Stream.Null))
                        {
                            break;
                        }
                        this._ProtocolStatus = CacheValidationStatus.Fail;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_no_stream", new object[] { this._Validator.CacheKey }));
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_null_cached_stream", new object[] { "RevalidateCache()" }));
                        }
                        return;

                    case CacheValidationStatus.CombineCachedAndServerResponse:
                        if ((this._Validator.CacheStream != null) && (this._Validator.CacheStream != Stream.Null))
                        {
                            goto Label_03FF;
                        }
                        this._ProtocolStatus = CacheValidationStatus.Fail;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_no_stream", new object[] { this._Validator.CacheKey }));
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_requested_combined_but_null_cached_stream", new object[] { "RevalidateCache()" }));
                        }
                        return;

                    default:
                        goto Label_046E;
                }
                Stream cacheStream = this._Validator.CacheStream;
                if ((this._Validator.CacheStreamOffset != 0L) || (this._Validator.CacheStreamLength != this._Validator.CacheEntry.StreamSize))
                {
                    cacheStream = new RangeStream(cacheStream, this._Validator.CacheStreamOffset, this._Validator.CacheStreamLength);
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_returned_range_cache", new object[] { "RevalidateCache()", this._Validator.CacheStreamOffset, this._Validator.CacheStreamLength }));
                    }
                }
                this._ResponseStream = cacheStream;
                this._ResponseStreamLength = this._Validator.CacheStreamLength;
                return;
            Label_03FF:
                if (responseStream != null)
                {
                    cacheStream = new CombinedReadStream(this._Validator.CacheStream, responseStream);
                }
                else
                {
                    cacheStream = this._Validator.CacheStream;
                }
                this._ResponseStream = cacheStream;
                this._ResponseStreamLength = this._Validator.CacheStreamLength;
                return;
            Label_046E:
                flag = true;
                this._ProtocolStatus = CacheValidationStatus.Fail;
                this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_result", new object[] { "RevalidateCache", this._Validator.ValidationStatus.ToString() }));
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_unexpected_status", new object[] { "RevalidateCache()", this._Validator.ValidationStatus.ToString() }));
                }
            }
            catch (Exception exception2)
            {
                flag = true;
                this._ProtocolException = exception2;
                this._ProtocolStatus = CacheValidationStatus.Fail;
                if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_object_and_exception", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (exception2 is WebException) ? exception2.Message : exception2.ToString() }));
                }
            }
            finally
            {
                if (flag && (this._ResponseStream != null))
                {
                    this._ResponseStream.Close();
                    this._ResponseStream = null;
                    this._Validator.CacheStream = Stream.Null;
                }
            }
        }

        private void CheckUpdateOnResponse(Stream responseStream)
        {
            if (this._Validator.CacheEntry == null)
            {
                RequestCacheEntry fetchEntry = new RequestCacheEntry {
                    IsPrivateEntry = this._RequestCache.IsPrivateCache
                };
                this._Validator.FetchCacheEntry(fetchEntry);
            }
            string cacheKey = this._Validator.CacheKey;
            bool flag = true;
            try
            {
                Stream stream;
                switch ((this._ProtocolStatus = this.UpdateCache()))
                {
                    case CacheValidationStatus.DoNotUseCache:
                    case CacheValidationStatus.DoNotUpdateCache:
                        return;

                    case CacheValidationStatus.Fail:
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_fail", new object[] { "UpdateCache" }));
                        return;

                    case CacheValidationStatus.CacheResponse:
                        if (!this._Validator.StrictCacheErrors)
                        {
                            break;
                        }
                        stream = this._RequestCache.Store(this._Validator.CacheKey, this._Validator.CacheEntry.StreamSize, this._Validator.CacheEntry.ExpiresUtc, this._Validator.CacheEntry.LastModifiedUtc, this._Validator.CacheEntry.MaxStale, this._Validator.CacheEntry.EntryMetadata, this._Validator.CacheEntry.SystemMetadata);
                        goto Label_022C;

                    case CacheValidationStatus.UpdateResponseInformation:
                        this._ResponseStream = new MetadataUpdateStream(responseStream, this._RequestCache, this._Validator.CacheKey, this._Validator.CacheEntry.ExpiresUtc, this._Validator.CacheEntry.LastModifiedUtc, this._Validator.CacheEntry.LastSynchronizedUtc, this._Validator.CacheEntry.MaxStale, this._Validator.CacheEntry.EntryMetadata, this._Validator.CacheEntry.SystemMetadata, this._Validator.StrictCacheErrors);
                        flag = false;
                        this._ProtocolStatus = CacheValidationStatus.UpdateResponseInformation;
                        return;

                    case CacheValidationStatus.RemoveFromCache:
                        this.EnsureCacheRemoval(cacheKey);
                        flag = false;
                        return;

                    default:
                        goto Label_0298;
                }
                this._RequestCache.TryStore(this._Validator.CacheKey, this._Validator.CacheEntry.StreamSize, this._Validator.CacheEntry.ExpiresUtc, this._Validator.CacheEntry.LastModifiedUtc, this._Validator.CacheEntry.MaxStale, this._Validator.CacheEntry.EntryMetadata, this._Validator.CacheEntry.SystemMetadata, out stream);
            Label_022C:
                if (stream == null)
                {
                    this._ProtocolStatus = CacheValidationStatus.DoNotUpdateCache;
                }
                else
                {
                    this._ResponseStream = new ForwardingReadStream(responseStream, stream, this._Validator.CacheStreamOffset, this._Validator.StrictCacheErrors);
                    this._ProtocolStatus = CacheValidationStatus.UpdateResponseInformation;
                }
                return;
            Label_0298:
                this._ProtocolStatus = CacheValidationStatus.Fail;
                this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_result", new object[] { "UpdateCache", this._Validator.ValidationStatus.ToString() }));
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_unexpected_status", new object[] { "UpdateCache()", this._Validator.ValidationStatus.ToString() }));
                }
            }
            finally
            {
                if (flag)
                {
                    this._RequestCache.UnlockEntry(this._Validator.CacheStream);
                }
            }
        }

        private void EnsureCacheRemoval(string retrieveKey)
        {
            this._RequestCache.UnlockEntry(this._Validator.CacheStream);
            if (this._Validator.StrictCacheErrors)
            {
                this._RequestCache.Remove(retrieveKey);
            }
            else
            {
                this._RequestCache.TryRemove(retrieveKey);
            }
            if (retrieveKey != this._Validator.CacheKey)
            {
                if (this._Validator.StrictCacheErrors)
                {
                    this._RequestCache.Remove(this._Validator.CacheKey);
                }
                else
                {
                    this._RequestCache.TryRemove(this._Validator.CacheKey);
                }
            }
        }

        internal CacheValidationStatus GetRetrieveStatus(Uri cacheUri, WebRequest request)
        {
            if (cacheUri == null)
            {
                throw new ArgumentNullException("cacheUri");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (!this._CanTakeNewRequest || (this._ProtocolStatus == CacheValidationStatus.RetryResponseFromServer))
            {
                return CacheValidationStatus.Continue;
            }
            this._CanTakeNewRequest = false;
            this._ResponseStream = null;
            this._ResponseStreamLength = 0L;
            this._ProtocolStatus = CacheValidationStatus.Continue;
            this._ProtocolException = null;
            if (Logging.On)
            {
                Logging.Enter(Logging.RequestCache, this, "GetRetrieveStatus", request);
            }
            try
            {
                if ((request.CachePolicy == null) || (request.CachePolicy.Level == RequestCacheLevel.BypassCache))
                {
                    this._ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                    return this._ProtocolStatus;
                }
                if ((this._RequestCache == null) || (this._Validator == null))
                {
                    this._ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                    return this._ProtocolStatus;
                }
                this._Validator.FetchRequest(cacheUri, request);
                switch ((this._ProtocolStatus = this.ValidateRequest()))
                {
                    case CacheValidationStatus.DoNotUseCache:
                    case CacheValidationStatus.DoNotTakeFromCache:
                    case CacheValidationStatus.Continue:
                        break;

                    case CacheValidationStatus.Fail:
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_fail", new object[] { "ValidateRequest" }));
                        break;

                    default:
                        this._ProtocolStatus = CacheValidationStatus.Fail;
                        this._ProtocolException = new InvalidOperationException(SR.GetString("net_cache_validator_result", new object[] { "ValidateRequest", this._Validator.ValidationStatus.ToString() }));
                        if (Logging.On)
                        {
                            Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_unexpected_status", new object[] { "ValidateRequest()", this._Validator.ValidationStatus.ToString() }));
                        }
                        break;
                }
                if (this._ProtocolStatus != CacheValidationStatus.Continue)
                {
                    return this._ProtocolStatus;
                }
                this.CheckRetrieveBeforeSubmit();
            }
            catch (Exception exception)
            {
                this._ProtocolException = exception;
                this._ProtocolStatus = CacheValidationStatus.Fail;
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_object_and_exception", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (exception is WebException) ? exception.Message : exception.ToString() }));
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, this, "GetRetrieveStatus", "result = " + this._ProtocolStatus.ToString());
                }
            }
            return this._ProtocolStatus;
        }

        internal CacheValidationStatus GetRevalidateStatus(WebResponse response, Stream responseStream)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (this._ProtocolStatus == CacheValidationStatus.DoNotUseCache)
            {
                return CacheValidationStatus.DoNotUseCache;
            }
            if (this._ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                this._ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                return this._ProtocolStatus;
            }
            try
            {
                if (Logging.On)
                {
                    Logging.Enter(Logging.RequestCache, this, "GetRevalidateStatus", (this._Validator == null) ? null : this._Validator.Request);
                }
                this._Validator.FetchResponse(response);
                if ((this._ProtocolStatus != CacheValidationStatus.Continue) && (this._ProtocolStatus != CacheValidationStatus.RetryResponseFromServer))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_revalidation_not_needed", new object[] { "GetRevalidateStatus()" }));
                    }
                    return this._ProtocolStatus;
                }
                this.CheckRetrieveOnResponse(responseStream);
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, this, "GetRevalidateStatus", "result = " + this._ProtocolStatus.ToString());
                }
            }
            return this._ProtocolStatus;
        }

        internal CacheValidationStatus GetUpdateStatus(WebResponse response, Stream responseStream)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (this._ProtocolStatus == CacheValidationStatus.DoNotUseCache)
            {
                return CacheValidationStatus.DoNotUseCache;
            }
            try
            {
                if (Logging.On)
                {
                    Logging.Enter(Logging.RequestCache, this, "GetUpdateStatus", (string) null);
                }
                if (this._Validator.Response == null)
                {
                    this._Validator.FetchResponse(response);
                }
                if (this._ProtocolStatus == CacheValidationStatus.RemoveFromCache)
                {
                    this.EnsureCacheRemoval(this._Validator.CacheKey);
                    return this._ProtocolStatus;
                }
                if (((this._ProtocolStatus != CacheValidationStatus.DoNotTakeFromCache) && (this._ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)) && (this._ProtocolStatus != CacheValidationStatus.CombineCachedAndServerResponse))
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_not_updated_based_on_cache_protocol_status", new object[] { "GetUpdateStatus()", this._ProtocolStatus.ToString() }));
                    }
                    return this._ProtocolStatus;
                }
                this.CheckUpdateOnResponse(responseStream);
            }
            catch (Exception exception)
            {
                this._ProtocolException = exception;
                this._ProtocolStatus = CacheValidationStatus.Fail;
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.RequestCache, SR.GetString("net_log_cache_object_and_exception", new object[] { "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (exception is WebException) ? exception.Message : exception.ToString() }));
                }
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.RequestCache, this, "GetUpdateStatus", "result = " + this._ProtocolStatus.ToString());
                }
            }
            return this._ProtocolStatus;
        }

        internal void Reset()
        {
            this._CanTakeNewRequest = true;
        }

        private CacheValidationStatus RevalidateCache()
        {
            CacheValidationStatus status = this._Validator.RevalidateCache();
            this._Validator.SetValidationStatus(status);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_result", new object[] { "RevalidateCache()", status.ToString() }));
            }
            return status;
        }

        private CacheValidationStatus UpdateCache()
        {
            CacheValidationStatus status = this._Validator.UpdateCache();
            this._Validator.SetValidationStatus(status);
            return status;
        }

        private CacheValidationStatus ValidateCache()
        {
            CacheValidationStatus status = this._Validator.ValidateCache();
            this._Validator.SetValidationStatus(status);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_result", new object[] { "ValidateCache()", status.ToString() }));
            }
            return status;
        }

        private void ValidateFreshness(RequestCacheEntry fetchEntry)
        {
            this._Validator.FetchCacheEntry(fetchEntry);
            if ((this._Validator.CacheStream == null) || (this._Validator.CacheStream == Stream.Null))
            {
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_entry_not_found_freshness_undefined", new object[] { "ValidateFreshness()" }));
                }
                this._Validator.SetFreshnessStatus(CacheFreshnessStatus.Undefined);
            }
            else
            {
                if (Logging.On && Logging.IsVerbose(Logging.RequestCache))
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_dumping_cache_context"));
                    if (fetchEntry == null)
                    {
                        Logging.PrintInfo(Logging.RequestCache, "<null>");
                    }
                    else
                    {
                        string[] strArray = fetchEntry.ToString(Logging.IsVerbose(Logging.RequestCache)).Split(RequestCache.LineSplits);
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if (strArray[i].Length != 0)
                            {
                                Logging.PrintInfo(Logging.RequestCache, strArray[i]);
                            }
                        }
                    }
                }
                CacheFreshnessStatus status = this._Validator.ValidateFreshness();
                this._Validator.SetFreshnessStatus(status);
                this._IsCacheFresh = status == CacheFreshnessStatus.Fresh;
                if (Logging.On)
                {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_result", new object[] { "ValidateFreshness()", status.ToString() }));
                }
            }
        }

        private CacheValidationStatus ValidateRequest()
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, string.Concat(new object[] { "Request#", this._Validator.Request.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), ", Policy = ", this._Validator.Request.CachePolicy.ToString(), ", Cache Uri = ", this._Validator.Uri }));
            }
            CacheValidationStatus status = this._Validator.ValidateRequest();
            this._Validator.SetValidationStatus(status);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, "Selected cache Key = " + this._Validator.CacheKey);
            }
            return status;
        }

        private CacheValidationStatus ValidateResponse()
        {
            CacheValidationStatus status = this._Validator.ValidateResponse();
            this._Validator.SetValidationStatus(status);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.RequestCache, SR.GetString("net_log_cache_result", new object[] { "ValidateResponse()", status.ToString() }));
            }
            return status;
        }

        internal bool IsCacheFresh
        {
            get
            {
                return ((this._Validator != null) && (this._Validator.CacheFreshnessStatus == CacheFreshnessStatus.Fresh));
            }
        }

        internal Exception ProtocolException
        {
            get
            {
                return this._ProtocolException;
            }
        }

        internal CacheValidationStatus ProtocolStatus
        {
            get
            {
                return this._ProtocolStatus;
            }
        }

        internal Stream ResponseStream
        {
            get
            {
                return this._ResponseStream;
            }
        }

        internal long ResponseStreamLength
        {
            get
            {
                return this._ResponseStreamLength;
            }
        }

        internal RequestCacheValidator Validator
        {
            get
            {
                return this._Validator;
            }
        }
    }
}

