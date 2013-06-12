namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Configuration;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable]
    public class HttpWebRequest : WebRequest, ISerializable
    {
        private HttpAbortDelegate _AbortDelegate;
        private ICredentials _AuthInfo;
        private int _AutoRedirects;
        private Booleans _Booleans;
        private X509CertificateCollection _ClientCertificates;
        private LazyAsyncResult _ConnectionAResult;
        private string _ConnectionGroupName;
        private LazyAsyncResult _ConnectionReaderAResult;
        private long _ContentLength;
        private HttpContinueDelegate _ContinueDelegate;
        private System.Net.CookieContainer _CookieContainer;
        private object _CoreResponse;
        private bool _HostHasPort;
        private Uri _HostUri;
        private WebHeaderCollection _HttpRequestHeaders;
        internal HttpWebResponse _HttpResponse;
        private System.Net.HttpWriteMode _HttpWriteMode;
        private int _MaximumAllowedRedirections;
        private int _MaximumResponseHeadersLength;
        private string _MediaType;
        private int _NestedWriteSideCheck;
        private ConnectStream _OldSubmitWriteStream;
        private Uri _OriginUri;
        private KnownHttpVerb _OriginVerb;
        private IWebProxy _Proxy;
        private AuthenticationState _ProxyAuthenticationState;
        private ProxyChain _ProxyChain;
        private LazyAsyncResult _ReadAResult;
        private int _ReadWriteTimeout;
        private bool _RedirectedToDifferentHost;
        private int _RequestContinueCount;
        private System.Net.TriState _RequestIsAsync;
        private int _RerequestCount;
        private AuthenticationState _ServerAuthenticationState;
        internal System.Net.ServicePoint _ServicePoint;
        private ConnectStream _SubmitWriteStream;
        private int _Timeout;
        private TimerThread.Timer _Timer;
        private TimerThread.Queue _TimerQueue;
        private System.Net.UnlockConnectionDelegate _UnlockDelegate;
        private Uri _Uri;
        private KnownHttpVerb _Verb;
        private LazyAsyncResult _WriteAResult;
        private byte[] _WriteBuffer;
        internal const string ChunkedHeader = "chunked";
        private const string ContinueHeader = "100-continue";
        internal const int DefaultContinueTimeout = 350;
        private const int DefaultReadWriteTimeout = 0x493e0;
        internal const string DeflateHeader = "deflate";
        internal const string GZipHeader = "gzip";
        private static readonly byte[] HttpBytes = new byte[] { 0x48, 0x54, 0x54, 80, 0x2f };
        private int m_Aborted;
        private DecompressionMethods m_AutomaticDecompression;
        private bool m_BodyStarted;
        private InterlockedGate m_ContinueGate;
        private TimerThread.Timer m_ContinueTimer;
        private bool m_Extra401Retry;
        private bool m_HeadersCompleted;
        private bool m_InternalConnectionGroup;
        private bool m_IsCurrentAuthenticationStateProxy;
        private bool m_KeepAlive;
        private bool m_LockConnection;
        private bool m_NtlmKeepAlive;
        private bool m_OnceFailed;
        private bool m_OriginallyBuffered;
        private object m_PendingReturnResult;
        private bool m_Pipelined;
        private bool m_PreAuthenticate;
        private bool m_RequestSubmitted;
        private bool m_Retry;
        private bool m_Saw100Continue;
        private bool m_SawInitialResponse;
        private long m_StartTimestamp;
        internal const HttpStatusCode MaxOkStatus = ((HttpStatusCode) 0x12b);
        private const HttpStatusCode MaxRedirectionStatus = ((HttpStatusCode) 0x18f);
        private const int RequestLineConstantSize = 12;
        private static readonly WaitCallback s_AbortWrapper = new WaitCallback(HttpWebRequest.AbortWrapper);
        private static readonly TimerThread.Callback s_ContinueTimeoutCallback = new TimerThread.Callback(HttpWebRequest.ContinueTimeoutCallback);
        private static readonly TimerThread.Queue s_ContinueTimerQueue = TimerThread.GetOrCreateQueue(350);
        private static readonly WaitCallback s_EndWriteHeaders_Part2Callback = new WaitCallback(HttpWebRequest.EndWriteHeaders_Part2Wrapper);
        private static readonly TimerThread.Callback s_TimeoutCallback = new TimerThread.Callback(HttpWebRequest.TimeoutCallback);
        private static int s_UniqueGroupId;

        [Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202"), SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected HttpWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            this.m_KeepAlive = true;
            this.m_Pipelined = true;
            this.m_Retry = true;
            this._Booleans = Booleans.AllowAutoRedirect | Booleans.AllowWriteStreamBuffering | Booleans.ExpectContinue;
            ExceptionHelper.WebPermissionUnrestricted.Demand();
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "HttpWebRequest", serializationInfo);
            }
            this._HttpRequestHeaders = (WebHeaderCollection) serializationInfo.GetValue("_HttpRequestHeaders", typeof(WebHeaderCollection));
            this._Proxy = (IWebProxy) serializationInfo.GetValue("_Proxy", typeof(IWebProxy));
            this.KeepAlive = serializationInfo.GetBoolean("_KeepAlive");
            this.Pipelined = serializationInfo.GetBoolean("_Pipelined");
            this.AllowAutoRedirect = serializationInfo.GetBoolean("_AllowAutoRedirect");
            this.AllowWriteStreamBuffering = serializationInfo.GetBoolean("_AllowWriteStreamBuffering");
            this.HttpWriteMode = (System.Net.HttpWriteMode) serializationInfo.GetInt32("_HttpWriteMode");
            this._MaximumAllowedRedirections = serializationInfo.GetInt32("_MaximumAllowedRedirections");
            this._AutoRedirects = serializationInfo.GetInt32("_AutoRedirects");
            this._Timeout = serializationInfo.GetInt32("_Timeout");
            try
            {
                this._ReadWriteTimeout = serializationInfo.GetInt32("_ReadWriteTimeout");
            }
            catch
            {
                this._ReadWriteTimeout = 0x493e0;
            }
            try
            {
                this._MaximumResponseHeadersLength = serializationInfo.GetInt32("_MaximumResponseHeadersLength");
            }
            catch
            {
                this._MaximumResponseHeadersLength = DefaultMaximumResponseHeadersLength;
            }
            this._ContentLength = serializationInfo.GetInt64("_ContentLength");
            this._MediaType = serializationInfo.GetString("_MediaType");
            this._OriginVerb = KnownHttpVerb.Parse(serializationInfo.GetString("_OriginVerb"));
            this._ConnectionGroupName = serializationInfo.GetString("_ConnectionGroupName");
            this.ProtocolVersion = (Version) serializationInfo.GetValue("_Version", typeof(Version));
            this._OriginUri = (Uri) serializationInfo.GetValue("_OriginUri", typeof(Uri));
            base.SetupCacheProtocol(this._OriginUri);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "HttpWebRequest", (string) null);
            }
        }

        internal HttpWebRequest(Uri uri, System.Net.ServicePoint servicePoint)
        {
            this.m_KeepAlive = true;
            this.m_Pipelined = true;
            this.m_Retry = true;
            this._Booleans = Booleans.AllowAutoRedirect | Booleans.AllowWriteStreamBuffering | Booleans.ExpectContinue;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "HttpWebRequest", uri);
            }
            this.CheckConnectPermission(uri, false);
            this.m_StartTimestamp = NetworkingPerfCounters.GetTimestamp();
            NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestCreated);
            this._HttpRequestHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
            this._Proxy = WebRequest.InternalDefaultWebProxy;
            this._HttpWriteMode = System.Net.HttpWriteMode.Unknown;
            this._MaximumAllowedRedirections = 50;
            this._Timeout = 0x186a0;
            this._TimerQueue = WebRequest.DefaultTimerQueue;
            this._ReadWriteTimeout = 0x493e0;
            this._MaximumResponseHeadersLength = DefaultMaximumResponseHeadersLength;
            this._ContentLength = -1L;
            this._OriginVerb = KnownHttpVerb.Get;
            this._OriginUri = uri;
            this._Uri = this._OriginUri;
            this._ServicePoint = servicePoint;
            this._RequestIsAsync = System.Net.TriState.Unspecified;
            base.SetupCacheProtocol(this._OriginUri);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "HttpWebRequest", (string) null);
            }
        }

        internal HttpWebRequest(Uri proxyUri, Uri requestUri, HttpWebRequest orginalRequest) : this(proxyUri, null)
        {
            this._OriginVerb = KnownHttpVerb.Parse("CONNECT");
            this.Pipelined = false;
            this._OriginUri = requestUri;
            this.IsTunnelRequest = true;
            this._ConnectionGroupName = ServicePointManager.SpecialConnectGroupName + "(" + UniqueGroupId + ")";
            this.m_InternalConnectionGroup = true;
            this.ServerAuthenticationState = new AuthenticationState(true);
            base.CacheProtocol = null;
        }

        public override void Abort()
        {
            this.Abort(null, 1);
        }

        private void Abort(Exception exception, int abortState)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "Abort", (exception == null) ? "" : exception.Message);
            }
            if (Interlocked.CompareExchange(ref this.m_Aborted, abortState, 0) == 0)
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestAborted);
                this.m_OnceFailed = true;
                this.CancelTimer();
                WebException webException = exception as WebException;
                if (exception == null)
                {
                    webException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                }
                else if (webException == null)
                {
                    webException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), exception, WebExceptionStatus.RequestCanceled, this._HttpResponse);
                }
                try
                {
                    Thread.MemoryBarrier();
                    HttpAbortDelegate delegate2 = this._AbortDelegate;
                    if ((delegate2 == null) || delegate2(this, webException))
                    {
                        LazyAsyncResult result = this.Async ? null : this.ConnectionAsyncResult;
                        LazyAsyncResult result2 = this.Async ? null : this.ConnectionReaderAsyncResult;
                        this.SetResponse(webException);
                        if (result != null)
                        {
                            result.InvokeCallback(webException);
                        }
                        if (result2 != null)
                        {
                            result2.InvokeCallback(webException);
                        }
                    }
                }
                catch (InternalException)
                {
                }
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "Abort", "");
            }
        }

        private static void AbortWrapper(object context)
        {
            ((HttpWebRequest) context).Abort(new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout), 1);
        }

        public void AddRange(int range)
        {
            this.AddRange("bytes", (long) range);
        }

        public void AddRange(long range)
        {
            this.AddRange("bytes", range);
        }

        public void AddRange(int from, int to)
        {
            this.AddRange("bytes", (long) from, (long) to);
        }

        public void AddRange(long from, long to)
        {
            this.AddRange("bytes", from, to);
        }

        public void AddRange(string rangeSpecifier, int range)
        {
            this.AddRange(rangeSpecifier, (long) range);
        }

        public void AddRange(string rangeSpecifier, long range)
        {
            if (rangeSpecifier == null)
            {
                throw new ArgumentNullException("rangeSpecifier");
            }
            if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
            {
                throw new ArgumentException(SR.GetString("net_nottoken"), "rangeSpecifier");
            }
            if (!this.AddRange(rangeSpecifier, range.ToString(NumberFormatInfo.InvariantInfo), (range >= 0L) ? "" : null))
            {
                throw new InvalidOperationException(SR.GetString("net_rangetype"));
            }
        }

        public void AddRange(string rangeSpecifier, int from, int to)
        {
            this.AddRange(rangeSpecifier, (long) from, (long) to);
        }

        public void AddRange(string rangeSpecifier, long from, long to)
        {
            if (rangeSpecifier == null)
            {
                throw new ArgumentNullException("rangeSpecifier");
            }
            if ((from < 0L) || (to < 0L))
            {
                throw new ArgumentOutOfRangeException("from, to", SR.GetString("net_rangetoosmall"));
            }
            if (from > to)
            {
                throw new ArgumentOutOfRangeException("from", SR.GetString("net_fromto"));
            }
            if (!WebHeaderCollection.IsValidToken(rangeSpecifier))
            {
                throw new ArgumentException(SR.GetString("net_nottoken"), "rangeSpecifier");
            }
            if (!this.AddRange(rangeSpecifier, from.ToString(NumberFormatInfo.InvariantInfo), to.ToString(NumberFormatInfo.InvariantInfo)))
            {
                throw new InvalidOperationException(SR.GetString("net_rangetype"));
            }
        }

        private bool AddRange(string rangeSpecifier, string from, string to)
        {
            string str = this._HttpRequestHeaders["Range"];
            if ((str == null) || (str.Length == 0))
            {
                str = rangeSpecifier + "=";
            }
            else
            {
                if (string.Compare(str.Substring(0, str.IndexOf('=')), rangeSpecifier, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                str = string.Empty;
            }
            str = str + from.ToString();
            if (to != null)
            {
                str = str + "-" + to;
            }
            this._HttpRequestHeaders.SetAddVerified("Range", str);
            return true;
        }

        internal string AuthHeader(HttpResponseHeader header)
        {
            if (this._HttpResponse == null)
            {
                return null;
            }
            return this._HttpResponse.Headers[header];
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginGetRequestStream", "");
            }
            this.CheckProtocol(true);
            ContextAwareResult retObject = new ContextAwareResult(this.IdentityRequired, true, this, state, callback);
            lock (retObject.StartPostingAsyncOp())
            {
                if ((this._WriteAResult != null) && this._WriteAResult.InternalPeekCompleted)
                {
                    if (this._WriteAResult.Result is Exception)
                    {
                        throw ((Exception) this._WriteAResult.Result);
                    }
                    try
                    {
                        retObject.InvokeCallback(this._WriteAResult.Result);
                        goto Label_014B;
                    }
                    catch (Exception exception)
                    {
                        this.Abort(exception, 1);
                        throw;
                    }
                }
                if (!this.RequestSubmitted && NclUtilities.IsThreadPoolLow())
                {
                    Exception exception2 = new InvalidOperationException(SR.GetString("net_needmorethreads"));
                    this.Abort(exception2, 1);
                    throw exception2;
                }
                lock (this)
                {
                    if (this._WriteAResult != null)
                    {
                        throw new InvalidOperationException(SR.GetString("net_repcall"));
                    }
                    if (this.SetRequestSubmitted())
                    {
                        throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                    }
                    if (this._ReadAResult != null)
                    {
                        throw ((Exception) this._ReadAResult.Result);
                    }
                    this._WriteAResult = retObject;
                    this.Async = true;
                }
                this.CurrentMethod = this._OriginVerb;
                this.BeginSubmitRequest();
            Label_014B:
                retObject.FinishPostingAsyncOp();
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "BeginGetRequestStream", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginGetResponse", "");
            }
            this.CheckProtocol(false);
            ConnectStream stream = (this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream;
            if ((stream != null) && !stream.IsClosed)
            {
                if (stream.BytesLeftToWrite > 0L)
                {
                    throw new ProtocolViolationException(SR.GetString("net_entire_body_not_written"));
                }
                stream.Close();
            }
            else if ((stream == null) && this.HasEntityBody)
            {
                throw new ProtocolViolationException(SR.GetString("net_must_provide_request_body"));
            }
            ContextAwareResult retObject = new ContextAwareResult(this.IdentityRequired, true, this, state, callback);
            if (!this.RequestSubmitted && NclUtilities.IsThreadPoolLow())
            {
                Exception exception = new InvalidOperationException(SR.GetString("net_needmorethreads"));
                this.Abort(exception, 1);
                throw exception;
            }
            lock (retObject.StartPostingAsyncOp())
            {
                bool flag2;
                bool flag = false;
                lock (this)
                {
                    flag2 = this.SetRequestSubmitted();
                    if (this.HaveResponse)
                    {
                        flag = true;
                    }
                    else
                    {
                        if (this._ReadAResult != null)
                        {
                            throw new InvalidOperationException(SR.GetString("net_repcall"));
                        }
                        this._ReadAResult = retObject;
                        this.Async = true;
                    }
                }
                this.CheckDeferredCallDone(stream);
                if (flag)
                {
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.Web, this, "BeginGetResponse", this._ReadAResult.Result);
                    }
                    Exception result = this._ReadAResult.Result as Exception;
                    if (result != null)
                    {
                        throw result;
                    }
                    try
                    {
                        retObject.InvokeCallback(this._ReadAResult.Result);
                        goto Label_01B8;
                    }
                    catch (Exception exception3)
                    {
                        this.Abort(exception3, 1);
                        throw;
                    }
                }
                if (!flag2)
                {
                    this.CurrentMethod = this._OriginVerb;
                }
                if ((this._RerequestCount > 0) || !flag2)
                {
                    while (this.m_Retry)
                    {
                        this.BeginSubmitRequest();
                    }
                }
            Label_01B8:
                retObject.FinishPostingAsyncOp();
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "BeginGetResponse", retObject);
            }
            return retObject;
        }

        private void BeginSubmitRequest()
        {
            this.SubmitRequest(this.FindServicePoint(false));
        }

        internal void CallContinueDelegateCallback(object state)
        {
            CoreResponseData data = (CoreResponseData) state;
            this.ContinueDelegate((int) data.m_StatusCode, data.m_ResponseHeaders);
        }

        private void CancelTimer()
        {
            TimerThread.Timer timer = this._Timer;
            if (timer != null)
            {
                timer.Cancel();
            }
        }

        private bool CheckCacheRetrieveBeforeSubmit()
        {
            bool flag;
            if (base.CacheProtocol == null)
            {
                return false;
            }
            try
            {
                Uri remoteResourceUri = this.GetRemoteResourceUri();
                if (remoteResourceUri.Fragment.Length != 0)
                {
                    remoteResourceUri = new Uri(remoteResourceUri.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped));
                }
                base.CacheProtocol.GetRetrieveStatus(remoteResourceUri, this);
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                {
                    throw base.CacheProtocol.ProtocolException;
                }
                if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
                {
                    return false;
                }
                if (this.HttpWriteMode != System.Net.HttpWriteMode.None)
                {
                    throw new NotSupportedException(SR.GetString("net_cache_not_supported_body"));
                }
                HttpRequestCacheValidator validator = (HttpRequestCacheValidator) base.CacheProtocol.Validator;
                CoreResponseData coreData = new CoreResponseData {
                    m_IsVersionHttp11 = validator.CacheHttpVersion.Equals(HttpVersion.Version11),
                    m_StatusCode = validator.CacheStatusCode,
                    m_StatusDescription = validator.CacheStatusDescription,
                    m_ResponseHeaders = validator.CacheHeaders,
                    m_ContentLength = base.CacheProtocol.ResponseStreamLength,
                    m_ConnectStream = base.CacheProtocol.ResponseStream
                };
                this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression);
                this._HttpResponse.InternalSetFromCache = true;
                this._HttpResponse.InternalSetIsCacheFresh = validator.CacheFreshnessStatus != CacheFreshnessStatus.Stale;
                this.ProcessResponse();
                flag = true;
            }
            catch (Exception exception)
            {
                this.Abort(exception, 1);
                throw;
            }
            return flag;
        }

        private bool CheckCacheRetrieveOnResponse()
        {
            if (base.CacheProtocol != null)
            {
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                {
                    throw base.CacheProtocol.ProtocolException;
                }
                Stream responseStream = this._HttpResponse.ResponseStream;
                base.CacheProtocol.GetRevalidateStatus(this._HttpResponse, this._HttpResponse.ResponseStream);
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
                {
                    return false;
                }
                if ((base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse) && (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.CombineCachedAndServerResponse))
                {
                    return true;
                }
                if (this.HttpWriteMode != System.Net.HttpWriteMode.None)
                {
                    throw new NotSupportedException(SR.GetString("net_cache_not_supported_body"));
                }
                CoreResponseData coreData = new CoreResponseData();
                HttpRequestCacheValidator validator = (HttpRequestCacheValidator) base.CacheProtocol.Validator;
                coreData.m_IsVersionHttp11 = validator.CacheHttpVersion.Equals(HttpVersion.Version11);
                coreData.m_StatusCode = validator.CacheStatusCode;
                coreData.m_StatusDescription = validator.CacheStatusDescription;
                coreData.m_ResponseHeaders = (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse) ? new WebHeaderCollection(validator.CacheHeaders) : validator.CacheHeaders;
                coreData.m_ContentLength = base.CacheProtocol.ResponseStreamLength;
                coreData.m_ConnectStream = base.CacheProtocol.ResponseStream;
                this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression);
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
                {
                    this._HttpResponse.InternalSetFromCache = true;
                    this._HttpResponse.InternalSetIsCacheFresh = base.CacheProtocol.IsCacheFresh;
                    if (responseStream != null)
                    {
                        try
                        {
                            responseStream.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return true;
        }

        private void CheckCacheUpdateOnResponse()
        {
            if (base.CacheProtocol != null)
            {
                if (base.CacheProtocol.GetUpdateStatus(this._HttpResponse, this._HttpResponse.ResponseStream) == CacheValidationStatus.UpdateResponseInformation)
                {
                    this._HttpResponse.ResponseStream = base.CacheProtocol.ResponseStream;
                }
                else if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                {
                    throw base.CacheProtocol.ProtocolException;
                }
            }
        }

        private void CheckConnectPermission(Uri uri, bool needExecutionContext)
        {
            ExecutionContext executionContext = needExecutionContext ? this.GetReadingContext().ContextCopy : null;
            CodeAccessPermission state = new WebPermission(NetworkAccess.Connect, uri);
            if (executionContext == null)
            {
                state.Demand();
            }
            else
            {
                ExecutionContext.Run(executionContext, NclUtilities.ContextRelativeDemandCallback, state);
            }
        }

        private void CheckDeferredCallDone(ConnectStream stream)
        {
            object obj2 = Interlocked.Exchange(ref this.m_PendingReturnResult, DBNull.Value);
            if (obj2 == NclConstants.Sentinel)
            {
                this.EndSubmitRequest();
            }
            else if ((obj2 != null) && (obj2 != DBNull.Value))
            {
                stream.ProcessWriteCallDone(obj2 as ConnectionReturnResult);
            }
        }

        private void CheckProtocol(bool onRequestStream)
        {
            if (!this.CanGetRequestStream)
            {
                if (onRequestStream)
                {
                    throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
                }
                if (((this.HttpWriteMode != System.Net.HttpWriteMode.Unknown) && (this.HttpWriteMode != System.Net.HttpWriteMode.None)) || ((this.ContentLength > 0L) || this.SendChunked))
                {
                    throw new ProtocolViolationException(SR.GetString("net_nocontentlengthonget"));
                }
                this.HttpWriteMode = System.Net.HttpWriteMode.None;
            }
            else if (this.HttpWriteMode == System.Net.HttpWriteMode.Unknown)
            {
                if (this.SendChunked)
                {
                    if ((this.ServicePoint.HttpBehaviour != HttpBehaviour.HTTP11) && (this.ServicePoint.HttpBehaviour != HttpBehaviour.Unknown))
                    {
                        if (!this.AllowWriteStreamBuffering)
                        {
                            throw new ProtocolViolationException(SR.GetString("net_nochunkuploadonhttp10"));
                        }
                        this.HttpWriteMode = System.Net.HttpWriteMode.Buffer;
                    }
                    else
                    {
                        this.HttpWriteMode = System.Net.HttpWriteMode.Chunked;
                    }
                }
                else
                {
                    this.HttpWriteMode = (this.ContentLength >= 0L) ? System.Net.HttpWriteMode.ContentLength : (onRequestStream ? System.Net.HttpWriteMode.Buffer : System.Net.HttpWriteMode.None);
                }
            }
            if (this.HttpWriteMode != System.Net.HttpWriteMode.Chunked)
            {
                if (((onRequestStream || this._OriginVerb.Equals(KnownHttpVerb.Post)) || this._OriginVerb.Equals(KnownHttpVerb.Put)) && (((this.ContentLength == -1L) && !this.AllowWriteStreamBuffering) && this.KeepAlive))
                {
                    throw new ProtocolViolationException(SR.GetString("net_contentlengthmissing"));
                }
                if (!ValidationHelper.IsBlankString(this.TransferEncoding))
                {
                    throw new InvalidOperationException(SR.GetString("net_needchunked"));
                }
            }
        }

        private bool CheckResubmit(ref Exception e)
        {
            Uri uri;
            bool flag = false;
            if ((this.ResponseStatusCode == HttpStatusCode.Unauthorized) || (this.ResponseStatusCode == HttpStatusCode.ProxyAuthenticationRequired))
            {
                try
                {
                    if (!(flag = this.CheckResubmitForAuth()))
                    {
                        e = new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                        return false;
                    }
                    goto Label_04E2;
                }
                catch (Win32Exception exception)
                {
                    throw new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), exception, WebExceptionStatus.ProtocolError, this._HttpResponse);
                }
            }
            if ((this.ServerAuthenticationState != null) && (this.ServerAuthenticationState.Authorization != null))
            {
                HttpWebResponse response = this._HttpResponse;
                if (response != null)
                {
                    response.InternalSetIsMutuallyAuthenticated = this.ServerAuthenticationState.Authorization.MutuallyAuthenticated;
                    if ((base.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired) && !response.IsMutuallyAuthenticated)
                    {
                        throw new WebException(SR.GetString("net_webstatus_RequestCanceled"), new ProtocolViolationException(SR.GetString("net_mutualauthfailed")), WebExceptionStatus.RequestCanceled, response);
                    }
                }
            }
            if (((this.ResponseStatusCode == HttpStatusCode.BadRequest) && this.SendChunked) && this.ServicePoint.InternalProxyServicePoint)
            {
                this.ClearAuthenticatedConnectionResources();
                return true;
            }
            if (!this.AllowAutoRedirect || (((this.ResponseStatusCode != HttpStatusCode.MultipleChoices) && (this.ResponseStatusCode != HttpStatusCode.MovedPermanently)) && (((this.ResponseStatusCode != HttpStatusCode.Found) && (this.ResponseStatusCode != HttpStatusCode.SeeOther)) && (this.ResponseStatusCode != HttpStatusCode.TemporaryRedirect))))
            {
                if (this.ResponseStatusCode > ((HttpStatusCode) 0x18f))
                {
                    e = new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                    return false;
                }
                if (this.AllowAutoRedirect && (this.ResponseStatusCode > ((HttpStatusCode) 0x12b)))
                {
                    e = new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                    return false;
                }
                return false;
            }
            this._AutoRedirects++;
            if (this._AutoRedirects > this._MaximumAllowedRedirections)
            {
                e = new WebException(SR.GetString("net_tooManyRedirections"), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                return false;
            }
            string location = this._HttpResponse.Headers.Location;
            if (location == null)
            {
                e = new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                return false;
            }
            try
            {
                uri = new Uri(this._Uri, location);
            }
            catch (UriFormatException exception2)
            {
                e = new WebException(SR.GetString("net_resubmitprotofailed"), exception2, WebExceptionStatus.ProtocolError, this._HttpResponse);
                return false;
            }
            if ((uri.Scheme != Uri.UriSchemeHttp) && (uri.Scheme != Uri.UriSchemeHttps))
            {
                e = new WebException(SR.GetString("net_resubmitprotofailed"), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                return false;
            }
            if (!this.HasRedirectPermission(uri, ref e))
            {
                return false;
            }
            Uri uri2 = this._Uri;
            this._Uri = uri;
            this._RedirectedToDifferentHost = Uri.Compare(this._OriginUri, this._Uri, UriComponents.HostAndPort, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) != 0;
            if (this.UseCustomHost)
            {
                Uri uri3;
                string hostName = GetHostAndPortString(this._HostUri.Host, this._HostUri.Port, true);
                this.TryGetHostUri(hostName, out uri3);
                if (!this.HasRedirectPermission(uri3, ref e))
                {
                    this._Uri = uri2;
                    return false;
                }
                this._HostUri = uri3;
            }
            bool flag2 = false;
            if ((this.ResponseStatusCode > ((HttpStatusCode) 0x12b)) && Logging.On)
            {
                Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_server_response_error_code", new object[] { ((int) this.ResponseStatusCode).ToString(NumberFormatInfo.InvariantInfo) }));
            }
            switch (this.ResponseStatusCode)
            {
                case HttpStatusCode.MovedPermanently:
                case HttpStatusCode.Found:
                    if (this.CurrentMethod.Equals(KnownHttpVerb.Post))
                    {
                        flag2 = true;
                    }
                    break;

                case HttpStatusCode.TemporaryRedirect:
                    break;

                default:
                    flag2 = true;
                    break;
            }
            if (flag2)
            {
                this.CurrentMethod = KnownHttpVerb.Get;
                this.ExpectContinue = false;
                this.HttpWriteMode = System.Net.HttpWriteMode.None;
            }
            ICredentials credentials = this.Credentials as CredentialCache;
            if (credentials == null)
            {
                credentials = this.Credentials as SystemNetworkCredential;
            }
            if (credentials == null)
            {
                this.Credentials = null;
            }
            this.ProxyAuthenticationState.ClearAuthReq(this);
            this.ServerAuthenticationState.ClearAuthReq(this);
            if (this._OriginUri.Scheme == Uri.UriSchemeHttps)
            {
                this._HttpRequestHeaders.RemoveInternal("Referer");
            }
        Label_04E2:
            if (((this.HttpWriteMode != System.Net.HttpWriteMode.None) && !this.AllowWriteStreamBuffering) && ((this.HttpWriteMode != System.Net.HttpWriteMode.ContentLength) || (this.ContentLength != 0L)))
            {
                e = new WebException(SR.GetString("net_need_writebuffering"), null, WebExceptionStatus.ProtocolError, this._HttpResponse);
                return false;
            }
            if (!flag)
            {
                this.ClearAuthenticatedConnectionResources();
            }
            if (Logging.On)
            {
                Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_resubmitting_request"));
            }
            return true;
        }

        private bool CheckResubmitForAuth()
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            if ((this.UsesProxySemantics && (this._Proxy != null)) && (this._Proxy.Credentials != null))
            {
                try
                {
                    flag |= this.ProxyAuthenticationState.AttemptAuthenticate(this, this._Proxy.Credentials);
                }
                catch (Win32Exception)
                {
                    if (!this.m_Extra401Retry)
                    {
                        throw;
                    }
                    flag3 = true;
                }
                flag2 = true;
            }
            if ((this.Credentials != null) && !flag3)
            {
                try
                {
                    flag |= this.ServerAuthenticationState.AttemptAuthenticate(this, this.Credentials);
                }
                catch (Win32Exception)
                {
                    if (!this.m_Extra401Retry)
                    {
                        throw;
                    }
                    flag = false;
                }
                flag2 = true;
            }
            if ((!flag && flag2) && this.m_Extra401Retry)
            {
                this.ClearAuthenticatedConnectionResources();
                this.m_Extra401Retry = false;
                flag = true;
            }
            return flag;
        }

        private bool CheckResubmitForCache(ref Exception e)
        {
            if (!this.CheckCacheRetrieveOnResponse())
            {
                if (this.AllowAutoRedirect)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.Web, this, "", SR.GetString("net_log_cache_validation_failed_resubmit"));
                    }
                    return true;
                }
                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, this, "", SR.GetString("net_log_cache_refused_server_response"));
                }
                e = new InvalidOperationException(SR.GetString("net_cache_not_accept_response"));
                return false;
            }
            this.CheckCacheUpdateOnResponse();
            return false;
        }

        internal void CheckWriteSideResponseProcessing()
        {
            object obj2 = this.Async ? Interlocked.CompareExchange(ref this._CoreResponse, null, DBNull.Value) : this._CoreResponse;
            if (obj2 != DBNull.Value)
            {
                if (obj2 == null)
                {
                    throw new InternalException();
                }
                if (this.Async || (++this._NestedWriteSideCheck == 1))
                {
                    Exception e = obj2 as Exception;
                    if (e != null)
                    {
                        this.SetResponse(e);
                    }
                    else
                    {
                        this.SetResponse(obj2 as CoreResponseData);
                    }
                }
            }
        }

        private void ClearAuthenticatedConnectionResources()
        {
            if ((this.ProxyAuthenticationState.UniqueGroupId != null) || (this.ServerAuthenticationState.UniqueGroupId != null))
            {
                this.ServicePoint.ReleaseConnectionGroup(this.GetConnectionGroupLine());
            }
            System.Net.UnlockConnectionDelegate unlockConnectionDelegate = this.UnlockConnectionDelegate;
            try
            {
                if (unlockConnectionDelegate != null)
                {
                    unlockConnectionDelegate();
                }
                this.UnlockConnectionDelegate = null;
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
            }
            this.ProxyAuthenticationState.ClearSession(this);
            this.ServerAuthenticationState.ClearSession(this);
        }

        private void ClearRequestForResubmit()
        {
            this._HttpRequestHeaders.RemoveInternal("Host");
            this._HttpRequestHeaders.RemoveInternal("Connection");
            this._HttpRequestHeaders.RemoveInternal("Proxy-Connection");
            this._HttpRequestHeaders.RemoveInternal("Content-Length");
            this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
            this._HttpRequestHeaders.RemoveInternal("Expect");
            if ((this._HttpResponse != null) && (this._HttpResponse.ResponseStream != null))
            {
                if (!this._HttpResponse.KeepAlive)
                {
                    ConnectStream stream = this._HttpResponse.ResponseStream as ConnectStream;
                    if (stream != null)
                    {
                        stream.ErrorResponseNotify(false);
                    }
                }
                ICloseEx responseStream = this._HttpResponse.ResponseStream as ICloseEx;
                if (responseStream != null)
                {
                    responseStream.CloseEx(CloseExState.Silent);
                }
                else
                {
                    this._HttpResponse.ResponseStream.Close();
                }
            }
            this._AbortDelegate = null;
            if (this._SubmitWriteStream != null)
            {
                if ((((this._HttpResponse != null) && this._HttpResponse.KeepAlive) || this._SubmitWriteStream.IgnoreSocketErrors) && this.HasEntityBody)
                {
                    this.SetRequestContinue();
                    if (!this.Async && this.UserRetrievedWriteStream)
                    {
                        this._SubmitWriteStream.CallDone();
                    }
                }
                if ((this.Async || this.UserRetrievedWriteStream) && ((this._OldSubmitWriteStream != null) && (this._OldSubmitWriteStream != this._SubmitWriteStream)))
                {
                    this._SubmitWriteStream.CloseInternal(true);
                }
            }
            this.m_ContinueGate.Reset();
            this._RerequestCount++;
            this.m_BodyStarted = false;
            this.HeadersCompleted = false;
            this._WriteBuffer = null;
            this.m_Extra401Retry = false;
            this._HttpResponse = null;
            if (!this.Aborted && this.Async)
            {
                this._CoreResponse = null;
            }
        }

        private bool CompleteContinueGate()
        {
            return this.m_ContinueGate.Complete();
        }

        private static void ContinueTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            HttpWebRequest state = (HttpWebRequest) context;
            if (state.HttpWriteMode != System.Net.HttpWriteMode.None)
            {
                if (state.CompleteContinueGate())
                {
                    state.m_ContinueTimer = null;
                }
                ThreadPool.UnsafeQueueUserWorkItem(s_EndWriteHeaders_Part2Callback, state);
            }
        }

        private HttpProcessingResult DoSubmitRequestProcessing(ref Exception exception)
        {
            HttpProcessingResult writeWait = HttpProcessingResult.Continue;
            this.m_Retry = false;
            try
            {
                if (this._HttpResponse != null)
                {
                    if (this._CookieContainer != null)
                    {
                        CookieModule.OnReceivedHeaders(this);
                    }
                    this.ProxyAuthenticationState.Update(this);
                    this.ServerAuthenticationState.Update(this);
                }
                bool flag = false;
                bool flag2 = true;
                if (this._HttpResponse == null)
                {
                    flag = true;
                }
                else if (this.CheckResubmitForCache(ref exception) || this.CheckResubmit(ref exception))
                {
                    flag = true;
                    flag2 = false;
                }
                System.Net.ServicePoint servicePoint = null;
                if (flag2)
                {
                    WebException exception2 = exception as WebException;
                    if ((exception2 != null) && (exception2.InternalStatus == WebExceptionInternalStatus.ServicePointFatal))
                    {
                        ProxyChain chain = this._ProxyChain;
                        if (chain != null)
                        {
                            servicePoint = ServicePointManager.FindServicePoint(chain);
                        }
                        flag = servicePoint != null;
                    }
                }
                if (!flag)
                {
                    return writeWait;
                }
                if ((base.CacheProtocol != null) && (this._HttpResponse != null))
                {
                    base.CacheProtocol.Reset();
                }
                this.ClearRequestForResubmit();
                WebException exception3 = exception as WebException;
                if ((exception3 != null) && ((exception3.Status == WebExceptionStatus.PipelineFailure) || (exception3.Status == WebExceptionStatus.KeepAliveFailure)))
                {
                    this.m_Extra401Retry = true;
                }
                if (servicePoint == null)
                {
                    servicePoint = this.FindServicePoint(true);
                }
                else
                {
                    this._ServicePoint = servicePoint;
                }
                if (this.Async)
                {
                    this.SubmitRequest(servicePoint);
                }
                else
                {
                    this.m_Retry = true;
                }
                writeWait = HttpProcessingResult.WriteWait;
            }
            finally
            {
                if (writeWait == HttpProcessingResult.Continue)
                {
                    this.ClearAuthenticatedConnectionResources();
                }
            }
            return writeWait;
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            TransportContext context;
            return this.EndGetRequestStream(asyncResult, out context);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult, out TransportContext context)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndGetRequestStream", "");
            }
            context = null;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndGetRequestStream" }));
            }
            ConnectStream connectStream = result.InternalWaitForCompletion() as ConnectStream;
            result.EndCalled = true;
            if (connectStream == null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetRequestStream", result.Result as Exception);
                }
                throw ((Exception) result.Result);
            }
            context = new ConnectStreamContext(connectStream);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "EndGetRequestStream", connectStream);
            }
            return connectStream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndGetResponse", "");
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            if ((result == null) || (result.AsyncObject != this))
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndGetResponse" }));
            }
            HttpWebResponse retObject = result.InternalWaitForCompletion() as HttpWebResponse;
            result.EndCalled = true;
            if (retObject == null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetResponse", result.Result as Exception);
                }
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestFailed);
                throw ((Exception) result.Result);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "EndGetResponse", retObject);
            }
            this.InitLifetimeTracking(retObject);
            return retObject;
        }

        private void EndSubmitRequest()
        {
            try
            {
                if (this.HttpWriteMode == System.Net.HttpWriteMode.Buffer)
                {
                    this.InvokeGetRequestStreamCallback();
                }
                else
                {
                    if (this.WriteBuffer == null)
                    {
                        long num = this.SwitchToContentLength();
                        this.SerializeHeaders();
                        this.PostSwitchToContentLength(num);
                    }
                    this._SubmitWriteStream.WriteHeaders(this.Async);
                }
            }
            catch
            {
                ConnectStream stream = this._SubmitWriteStream;
                if (stream != null)
                {
                    stream.CallDone();
                }
                throw;
            }
            finally
            {
                if (!this.Async)
                {
                    this.CheckWriteSideResponseProcessing();
                }
            }
        }

        internal bool EndWriteHeaders(bool async)
        {
            try
            {
                if ((((this.ContentLength > 0L) || (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)) && (this.ExpectContinue && this._ServicePoint.Understands100Continue)) && (async ? this.m_ContinueGate.StartTrigger(true) : this.m_ContinueGate.Trigger(true)))
                {
                    if (async)
                    {
                        try
                        {
                            this.m_ContinueTimer = s_ContinueTimerQueue.CreateTimer(s_ContinueTimeoutCallback, this);
                        }
                        finally
                        {
                            this.m_ContinueGate.FinishTrigger();
                        }
                        return false;
                    }
                    this._SubmitWriteStream.PollAndRead(this.UserRetrievedWriteStream);
                    return true;
                }
                this.EndWriteHeaders_Part2();
            }
            catch
            {
                ConnectStream stream = this._SubmitWriteStream;
                if (stream != null)
                {
                    stream.CallDone();
                }
                throw;
            }
            return true;
        }

        internal void EndWriteHeaders_Part2()
        {
            try
            {
                ConnectStream stream = this._SubmitWriteStream;
                if (this.HttpWriteMode != System.Net.HttpWriteMode.None)
                {
                    this.m_BodyStarted = true;
                    if (this.AllowWriteStreamBuffering)
                    {
                        if (stream.BufferOnly)
                        {
                            this._OldSubmitWriteStream = stream;
                        }
                        if (this._OldSubmitWriteStream != null)
                        {
                            stream.ResubmitWrite(this._OldSubmitWriteStream, this.NtlmKeepAlive && (this.ContentLength == 0L));
                            stream.CloseInternal(true);
                        }
                    }
                }
                else
                {
                    if (stream != null)
                    {
                        stream.CloseInternal(true);
                        stream = null;
                    }
                    this._OldSubmitWriteStream = null;
                }
                this.InvokeGetRequestStreamCallback();
            }
            catch
            {
                ConnectStream stream2 = this._SubmitWriteStream;
                if (stream2 != null)
                {
                    stream2.CallDone();
                }
                throw;
            }
        }

        private static void EndWriteHeaders_Part2Wrapper(object state)
        {
            ((HttpWebRequest) state).EndWriteHeaders_Part2();
        }

        internal void ErrorStatusCodeNotify(System.Net.Connection connection, bool isKeepAlive, bool fatal)
        {
            ConnectStream stream = this._SubmitWriteStream;
            if ((stream != null) && (stream.Connection == connection))
            {
                if (!fatal)
                {
                    stream.ErrorResponseNotify(isKeepAlive);
                }
                else if (!this.Aborted)
                {
                    stream.FatalResponseNotify();
                }
            }
        }

        private System.Net.ServicePoint FindServicePoint(bool forceFind)
        {
            System.Net.ServicePoint point = this._ServicePoint;
            if ((point != null) && !forceFind)
            {
                return point;
            }
            lock (this)
            {
                if ((this._ServicePoint == null) || forceFind)
                {
                    if (!this.ProxySet)
                    {
                        this._Proxy = WebRequest.InternalDefaultWebProxy;
                    }
                    if (this._ProxyChain != null)
                    {
                        this._ProxyChain.Dispose();
                    }
                    this._ServicePoint = ServicePointManager.FindServicePoint(this._Uri, this._Proxy, out this._ProxyChain, ref this._AbortDelegate, ref this.m_Aborted);
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, this._ServicePoint);
                    }
                }
            }
            return this._ServicePoint;
        }

        private void FinishRequest(HttpWebResponse response, Exception errorException)
        {
            if ((!this._ReadAResult.InternalPeekCompleted && (this.m_Aborted != 1)) && ((response != null) && (errorException != null)))
            {
                response.ResponseStream = this.MakeMemoryStream(response.ResponseStream);
            }
            if (((errorException != null) && (this._SubmitWriteStream != null)) && !this._SubmitWriteStream.IsClosed)
            {
                this._SubmitWriteStream.ErrorResponseNotify(this._SubmitWriteStream.Connection.KeepAlive);
            }
            if ((((errorException == null) && (this._HttpResponse != null)) && ((this._HttpWriteMode == System.Net.HttpWriteMode.Chunked) || (this._ContentLength > 0L))) && (((this.ExpectContinue && !this.Saw100Continue) && (this._ServicePoint.Understands100Continue && !this.IsTunnelRequest)) && (this.ResponseStatusCode <= ((HttpStatusCode) 0x12b))))
            {
                this._ServicePoint.Understands100Continue = false;
            }
        }

        internal static StringBuilder GenerateConnectionGroup(string connectionGroupName, bool unsafeConnectionGroup, bool isInternalGroup)
        {
            StringBuilder builder = new StringBuilder(connectionGroupName);
            builder.Append(unsafeConnectionGroup ? "U>" : "S>");
            if (isInternalGroup)
            {
                builder.Append("I>");
            }
            return builder;
        }

        private int GenerateConnectRequestLine(int headersSize)
        {
            int destByteIndex = 0;
            HostHeaderString str = new HostHeaderString(this.GetSafeHostAndPort(true));
            int num2 = ((this.CurrentMethod.Name.Length + str.ByteCount) + 12) + headersSize;
            this._WriteBuffer = new byte[num2];
            destByteIndex = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
            this.WriteBuffer[destByteIndex++] = 0x20;
            str.Copy(this.WriteBuffer, destByteIndex);
            destByteIndex += str.ByteCount;
            this.WriteBuffer[destByteIndex++] = 0x20;
            return destByteIndex;
        }

        private int GenerateFtpProxyRequestLine(int headersSize)
        {
            int byteIndex = 0;
            string components = this._Uri.GetComponents(UriComponents.KeepDelimiter | UriComponents.Scheme, UriFormat.UriEscaped);
            string s = this._Uri.GetComponents(UriComponents.KeepDelimiter | UriComponents.UserInfo, UriFormat.UriEscaped);
            HostHeaderString str3 = new HostHeaderString(this.GetSafeHostAndPort(false));
            string str4 = this._Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
            if (s == "")
            {
                string domainUserName = null;
                string password = null;
                NetworkCredential credential = this.Credentials.GetCredential(this._Uri, "basic");
                if ((credential != null) && (credential != FtpWebRequest.DefaultNetworkCredential))
                {
                    domainUserName = credential.InternalGetDomainUserName();
                    password = credential.InternalGetPassword();
                    password = (password == null) ? string.Empty : password;
                }
                if (domainUserName != null)
                {
                    domainUserName = domainUserName.Replace(":", "%3A");
                    password = password.Replace(":", "%3A");
                    domainUserName = domainUserName.Replace(@"\", "%5C");
                    password = password.Replace(@"\", "%5C");
                    domainUserName = domainUserName.Replace("/", "%2F");
                    password = password.Replace("/", "%2F");
                    domainUserName = domainUserName.Replace("?", "%3F");
                    password = password.Replace("?", "%3F");
                    domainUserName = domainUserName.Replace("#", "%23");
                    password = password.Replace("#", "%23");
                    domainUserName = domainUserName.Replace("%", "%25");
                    password = password.Replace("%", "%25");
                    domainUserName = domainUserName.Replace("@", "%40");
                    password = password.Replace("@", "%40");
                    s = domainUserName + ":" + password + "@";
                }
            }
            int num2 = (((((this.CurrentMethod.Name.Length + components.Length) + s.Length) + str3.ByteCount) + str4.Length) + 12) + headersSize;
            this._WriteBuffer = new byte[num2];
            byteIndex = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
            this.WriteBuffer[byteIndex++] = 0x20;
            byteIndex += Encoding.ASCII.GetBytes(components, 0, components.Length, this.WriteBuffer, byteIndex);
            byteIndex += Encoding.ASCII.GetBytes(s, 0, s.Length, this.WriteBuffer, byteIndex);
            str3.Copy(this.WriteBuffer, byteIndex);
            byteIndex += str3.ByteCount;
            byteIndex += Encoding.ASCII.GetBytes(str4, 0, str4.Length, this.WriteBuffer, byteIndex);
            this.WriteBuffer[byteIndex++] = 0x20;
            return byteIndex;
        }

        private int GenerateProxyRequestLine(int headersSize)
        {
            if (this._Uri.Scheme == Uri.UriSchemeFtp)
            {
                return this.GenerateFtpProxyRequestLine(headersSize);
            }
            int byteIndex = 0;
            string components = this._Uri.GetComponents(UriComponents.KeepDelimiter | UriComponents.Scheme, UriFormat.UriEscaped);
            HostHeaderString str2 = new HostHeaderString(this.GetSafeHostAndPort(false));
            string s = this._Uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
            int num2 = ((((this.CurrentMethod.Name.Length + components.Length) + str2.ByteCount) + s.Length) + 12) + headersSize;
            this._WriteBuffer = new byte[num2];
            byteIndex = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
            this.WriteBuffer[byteIndex++] = 0x20;
            byteIndex += Encoding.ASCII.GetBytes(components, 0, components.Length, this.WriteBuffer, byteIndex);
            str2.Copy(this.WriteBuffer, byteIndex);
            byteIndex += str2.ByteCount;
            byteIndex += Encoding.ASCII.GetBytes(s, 0, s.Length, this.WriteBuffer, byteIndex);
            this.WriteBuffer[byteIndex++] = 0x20;
            return byteIndex;
        }

        private int GenerateRequestLine(int headersSize)
        {
            int byteIndex = 0;
            string pathAndQuery = this._Uri.PathAndQuery;
            int num2 = ((this.CurrentMethod.Name.Length + pathAndQuery.Length) + 12) + headersSize;
            this._WriteBuffer = new byte[num2];
            byteIndex = Encoding.ASCII.GetBytes(this.CurrentMethod.Name, 0, this.CurrentMethod.Name.Length, this.WriteBuffer, 0);
            this.WriteBuffer[byteIndex++] = 0x20;
            byteIndex += Encoding.ASCII.GetBytes(pathAndQuery, 0, pathAndQuery.Length, this.WriteBuffer, byteIndex);
            this.WriteBuffer[byteIndex++] = 0x20;
            return byteIndex;
        }

        internal override ContextAwareResult GetConnectingContext()
        {
            if (!this.Async)
            {
                return null;
            }
            ContextAwareResult result = ((((this.HttpWriteMode == System.Net.HttpWriteMode.None) || (this._OldSubmitWriteStream != null)) || (this._WriteAResult == null)) ? ((ContextAwareResult) this._ReadAResult) : ((ContextAwareResult) this._WriteAResult)) as ContextAwareResult;
            if (result == null)
            {
                throw new InternalException();
            }
            return result;
        }

        internal string GetConnectionGroupLine()
        {
            StringBuilder builder = GenerateConnectionGroup(this._ConnectionGroupName, this.UnsafeAuthenticatedConnectionSharing, this.m_InternalConnectionGroup);
            if ((this._Uri.Scheme == Uri.UriSchemeHttps) || this.IsTunnelRequest)
            {
                if (this.UsesProxy)
                {
                    builder.Append(this.GetSafeHostAndPort(true));
                    builder.Append("$");
                }
                if ((this._ClientCertificates != null) && (this.ClientCertificates.Count > 0))
                {
                    builder.Append(this.ClientCertificates.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            if (this.ProxyAuthenticationState.UniqueGroupId != null)
            {
                builder.Append(this.ProxyAuthenticationState.UniqueGroupId);
            }
            else if (this.ServerAuthenticationState.UniqueGroupId != null)
            {
                builder.Append(this.ServerAuthenticationState.UniqueGroupId);
            }
            return builder.ToString();
        }

        private DateTime GetDateHeaderHelper(string headerName)
        {
            string s = this._HttpRequestHeaders[headerName];
            if (s == null)
            {
                return DateTime.MinValue;
            }
            return HttpProtocolUtils.string2date(s);
        }

        private static string GetHostAndPortString(string hostName, int port, bool addPort)
        {
            if (addPort)
            {
                return (hostName + ":" + port);
            }
            return hostName;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            serializationInfo.AddValue("_HttpRequestHeaders", this._HttpRequestHeaders, typeof(WebHeaderCollection));
            serializationInfo.AddValue("_Proxy", this._Proxy, typeof(IWebProxy));
            serializationInfo.AddValue("_KeepAlive", this.KeepAlive);
            serializationInfo.AddValue("_Pipelined", this.Pipelined);
            serializationInfo.AddValue("_AllowAutoRedirect", this.AllowAutoRedirect);
            serializationInfo.AddValue("_AllowWriteStreamBuffering", this.AllowWriteStreamBuffering);
            serializationInfo.AddValue("_HttpWriteMode", this.HttpWriteMode);
            serializationInfo.AddValue("_MaximumAllowedRedirections", this._MaximumAllowedRedirections);
            serializationInfo.AddValue("_AutoRedirects", this._AutoRedirects);
            serializationInfo.AddValue("_Timeout", this._Timeout);
            serializationInfo.AddValue("_ReadWriteTimeout", this._ReadWriteTimeout);
            serializationInfo.AddValue("_MaximumResponseHeadersLength", this._MaximumResponseHeadersLength);
            serializationInfo.AddValue("_ContentLength", this.ContentLength);
            serializationInfo.AddValue("_MediaType", this._MediaType);
            serializationInfo.AddValue("_OriginVerb", this._OriginVerb);
            serializationInfo.AddValue("_ConnectionGroupName", this._ConnectionGroupName);
            serializationInfo.AddValue("_Version", this.ProtocolVersion, typeof(Version));
            serializationInfo.AddValue("_OriginUri", this._OriginUri, typeof(Uri));
            base.GetObjectData(serializationInfo, streamingContext);
        }

        internal override ContextAwareResult GetReadingContext()
        {
            if (!this.Async)
            {
                return null;
            }
            ContextAwareResult result = this._ReadAResult as ContextAwareResult;
            if (result == null)
            {
                throw new InternalException();
            }
            return result;
        }

        internal Uri GetRemoteResourceUri()
        {
            if (this.UseCustomHost)
            {
                return this._HostUri;
            }
            return this._Uri;
        }

        public override Stream GetRequestStream()
        {
            TransportContext context;
            return this.GetRequestStream(out context);
        }

        public Stream GetRequestStream(out TransportContext context)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "GetRequestStream", "");
            }
            context = null;
            this.CheckProtocol(true);
            if ((this._WriteAResult == null) || !this._WriteAResult.InternalPeekCompleted)
            {
                lock (this)
                {
                    if (this._WriteAResult != null)
                    {
                        throw new InvalidOperationException(SR.GetString("net_repcall"));
                    }
                    if (this.SetRequestSubmitted())
                    {
                        throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                    }
                    if (this._ReadAResult != null)
                    {
                        throw ((Exception) this._ReadAResult.Result);
                    }
                    this._WriteAResult = new LazyAsyncResult(this, null, null);
                    this.Async = false;
                }
                this.CurrentMethod = this._OriginVerb;
                while (this.m_Retry && !this._WriteAResult.InternalPeekCompleted)
                {
                    this._OldSubmitWriteStream = null;
                    this._SubmitWriteStream = null;
                    this.BeginSubmitRequest();
                }
                while (this.Aborted && !this._WriteAResult.InternalPeekCompleted)
                {
                    if (!(this._CoreResponse is Exception))
                    {
                        Thread.SpinWait(1);
                    }
                    else
                    {
                        this.CheckWriteSideResponseProcessing();
                    }
                }
            }
            ConnectStream connectStream = this._WriteAResult.InternalWaitForCompletion() as ConnectStream;
            this._WriteAResult.EndCalled = true;
            if (connectStream == null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetRequestStream", this._WriteAResult.Result as Exception);
                }
                throw ((Exception) this._WriteAResult.Result);
            }
            context = new ConnectStreamContext(connectStream);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "GetRequestStream", connectStream);
            }
            return connectStream;
        }

        public override WebResponse GetResponse()
        {
            bool flag2;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "GetResponse", "");
            }
            this.CheckProtocol(false);
            ConnectStream stream = (this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream;
            if ((stream != null) && !stream.IsClosed)
            {
                if (stream.BytesLeftToWrite > 0L)
                {
                    throw new ProtocolViolationException(SR.GetString("net_entire_body_not_written"));
                }
                stream.Close();
            }
            else if ((stream == null) && this.HasEntityBody)
            {
                throw new ProtocolViolationException(SR.GetString("net_must_provide_request_body"));
            }
            bool flag = false;
            HttpWebResponse retObject = null;
            lock (this)
            {
                flag2 = this.SetRequestSubmitted();
                if (this.HaveResponse)
                {
                    flag = true;
                    retObject = this._ReadAResult.Result as HttpWebResponse;
                }
                else
                {
                    if (this._ReadAResult != null)
                    {
                        throw new InvalidOperationException(SR.GetString("net_repcall"));
                    }
                    this.Async = false;
                    if (this.Async)
                    {
                        ContextAwareResult result = new ContextAwareResult(this.IdentityRequired, true, this, null, null);
                        result.StartPostingAsyncOp(false);
                        result.FinishPostingAsyncOp();
                        this._ReadAResult = result;
                    }
                    else
                    {
                        this._ReadAResult = new LazyAsyncResult(this, null, null);
                    }
                }
            }
            this.CheckDeferredCallDone(stream);
            if (!flag)
            {
                if (this._Timer == null)
                {
                    this._Timer = this.TimerQueue.CreateTimer(s_TimeoutCallback, this);
                }
                if (!flag2)
                {
                    this.CurrentMethod = this._OriginVerb;
                }
                while (this.m_Retry)
                {
                    this.BeginSubmitRequest();
                }
                while ((!this.Async && this.Aborted) && !this._ReadAResult.InternalPeekCompleted)
                {
                    if (!(this._CoreResponse is Exception))
                    {
                        Thread.SpinWait(1);
                    }
                    else
                    {
                        this.CheckWriteSideResponseProcessing();
                    }
                }
                retObject = this._ReadAResult.InternalWaitForCompletion() as HttpWebResponse;
                this._ReadAResult.EndCalled = true;
            }
            if (retObject == null)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "GetResponse", this._ReadAResult.Result as Exception);
                }
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestFailed);
                throw ((Exception) this._ReadAResult.Result);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, this, "GetResponse", retObject);
            }
            if (!flag)
            {
                this.InitLifetimeTracking(retObject);
            }
            return retObject;
        }

        private string GetSafeHostAndPort(bool addDefaultPort)
        {
            if (this.IsTunnelRequest)
            {
                return GetSafeHostAndPort(this._OriginUri, addDefaultPort);
            }
            return GetSafeHostAndPort(this._Uri, addDefaultPort);
        }

        private static string GetSafeHostAndPort(Uri sourceUri, bool addDefaultPort)
        {
            string dnsSafeHost;
            if (sourceUri.HostNameType == UriHostNameType.IPv6)
            {
                dnsSafeHost = "[" + TrimScopeID(sourceUri.DnsSafeHost) + "]";
            }
            else
            {
                dnsSafeHost = sourceUri.DnsSafeHost;
            }
            return GetHostAndPortString(dnsSafeHost, sourceUri.Port, addDefaultPort || !sourceUri.IsDefaultPort);
        }

        internal override ContextAwareResult GetWritingContext()
        {
            if (!this.Async)
            {
                return null;
            }
            ContextAwareResult result = (((((this.HttpWriteMode == System.Net.HttpWriteMode.None) || (this.HttpWriteMode == System.Net.HttpWriteMode.Buffer)) || ((this.m_PendingReturnResult == DBNull.Value) || this.m_OriginallyBuffered)) || (this._WriteAResult == null)) ? ((ContextAwareResult) this._ReadAResult) : ((ContextAwareResult) this._WriteAResult)) as ContextAwareResult;
            if (result == null)
            {
                throw new InternalException();
            }
            return result;
        }

        private bool HasRedirectPermission(Uri uri, ref Exception resultException)
        {
            try
            {
                this.CheckConnectPermission(uri, this.Async);
            }
            catch (SecurityException exception)
            {
                resultException = new SecurityException(SR.GetString("net_redirect_perm"), new WebException(SR.GetString("net_resubmitcanceled"), exception, WebExceptionStatus.ProtocolError, this._HttpResponse));
                return false;
            }
            return true;
        }

        private void InitLifetimeTracking(HttpWebResponse httpWebResponse)
        {
            (httpWebResponse.ResponseStream as IRequestLifetimeTracker).TrackRequestLifetime(this.m_StartTimestamp);
        }

        private void InvokeGetRequestStreamCallback()
        {
            LazyAsyncResult result = this._WriteAResult;
            if (result != null)
            {
                try
                {
                    result.InvokeCallback(this._SubmitWriteStream);
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    this.Abort(exception, 1);
                    throw;
                }
            }
        }

        private Stream MakeMemoryStream(Stream stream)
        {
            if ((stream == null) || (stream is SyncMemoryStream))
            {
                return stream;
            }
            SyncMemoryStream stream2 = new SyncMemoryStream(0);
            try
            {
                if (stream.CanRead)
                {
                    byte[] buffer = new byte[0x400];
                    int count = 0;
                    int num2 = (DefaultMaximumErrorResponseLength == -1) ? buffer.Length : (DefaultMaximumErrorResponseLength * 0x400);
                    while ((count = stream.Read(buffer, 0, Math.Min(buffer.Length, num2))) > 0)
                    {
                        stream2.Write(buffer, 0, count);
                        if (DefaultMaximumErrorResponseLength != -1)
                        {
                            num2 -= count;
                        }
                    }
                }
                stream2.Position = 0L;
            }
            catch
            {
            }
            finally
            {
                try
                {
                    ICloseEx ex = stream as ICloseEx;
                    if (ex != null)
                    {
                        ex.CloseEx(CloseExState.Silent);
                    }
                    else
                    {
                        stream.Close();
                    }
                }
                catch
                {
                }
            }
            return stream2;
        }

        internal void NeedEndSubmitRequest()
        {
            if (Interlocked.CompareExchange(ref this.m_PendingReturnResult, NclConstants.Sentinel, null) == DBNull.Value)
            {
                this.EndSubmitRequest();
            }
        }

        internal void OpenWriteSideResponseWindow()
        {
            this._CoreResponse = DBNull.Value;
            this._NestedWriteSideCheck = 0;
        }

        private void PostSwitchToContentLength(long value)
        {
            if (value > -1L)
            {
                this._ContentLength = value;
            }
            if (value == -2L)
            {
                this._ContentLength = -1L;
                this.HttpWriteMode = System.Net.HttpWriteMode.Chunked;
            }
        }

        private void ProcessResponse()
        {
            Exception exception = null;
            if (this.DoSubmitRequestProcessing(ref exception) == HttpProcessingResult.Continue)
            {
                this.CancelTimer();
                object result = (exception != null) ? ((object) exception) : ((object) this._HttpResponse);
                if (this._ReadAResult == null)
                {
                    lock (this)
                    {
                        if (this._ReadAResult == null)
                        {
                            this._ReadAResult = new LazyAsyncResult(null, null, null);
                        }
                    }
                }
                try
                {
                    this.FinishRequest(this._HttpResponse, exception);
                    this._ReadAResult.InvokeCallback(result);
                    try
                    {
                        this.SetRequestContinue();
                    }
                    catch
                    {
                    }
                }
                catch (Exception exception2)
                {
                    this.Abort(exception2, 1);
                    throw;
                }
                finally
                {
                    if ((exception == null) && (this._ReadAResult.Result != this._HttpResponse))
                    {
                        WebException exception3 = this._ReadAResult.Result as WebException;
                        if ((exception3 != null) && (exception3.Response != null))
                        {
                            this._HttpResponse.Abort();
                        }
                    }
                }
            }
        }

        internal void SerializeHeaders()
        {
            int num;
            if (this.HttpWriteMode != System.Net.HttpWriteMode.None)
            {
                if (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)
                {
                    this._HttpRequestHeaders.AddInternal("Transfer-Encoding", "chunked");
                }
                else if (this.ContentLength >= 0L)
                {
                    this._HttpRequestHeaders.ChangeInternal("Content-Length", this._ContentLength.ToString(NumberFormatInfo.InvariantInfo));
                }
                this.ExpectContinue = (this.ExpectContinue && !this.IsVersionHttp10) && this.ServicePoint.Expect100Continue;
                if (((this.ContentLength > 0L) || (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)) && this.ExpectContinue)
                {
                    this._HttpRequestHeaders.AddInternal("Expect", "100-continue");
                }
            }
            if ((this.AutomaticDecompression & DecompressionMethods.GZip) != DecompressionMethods.None)
            {
                if ((this.AutomaticDecompression & DecompressionMethods.Deflate) != DecompressionMethods.None)
                {
                    this._HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip, deflate");
                }
                else
                {
                    this._HttpRequestHeaders.AddInternal("Accept-Encoding", "gzip");
                }
            }
            else if ((this.AutomaticDecompression & DecompressionMethods.Deflate) != DecompressionMethods.None)
            {
                this._HttpRequestHeaders.AddInternal("Accept-Encoding", "deflate");
            }
            string name = "Connection";
            if (this.UsesProxySemantics || this.IsTunnelRequest)
            {
                this._HttpRequestHeaders.RemoveInternal("Connection");
                name = "Proxy-Connection";
                if (!ValidationHelper.IsBlankString(this.Connection))
                {
                    this._HttpRequestHeaders.AddInternal("Proxy-Connection", this._HttpRequestHeaders["Connection"]);
                }
            }
            else
            {
                this._HttpRequestHeaders.RemoveInternal("Proxy-Connection");
            }
            if (this.KeepAlive || this.NtlmKeepAlive)
            {
                if (this.IsVersionHttp10 || (this.ServicePoint.HttpBehaviour <= HttpBehaviour.HTTP10))
                {
                    this._HttpRequestHeaders.AddInternal((this.UsesProxySemantics || this.IsTunnelRequest) ? "Proxy-Connection" : "Connection", "Keep-Alive");
                }
            }
            else if (!this.IsVersionHttp10)
            {
                this._HttpRequestHeaders.AddInternal(name, "Close");
            }
            string myString = this._HttpRequestHeaders.ToString();
            int byteCount = WebHeaderCollection.HeaderEncoding.GetByteCount(myString);
            if (this.CurrentMethod.ConnectRequest)
            {
                num = this.GenerateConnectRequestLine(byteCount);
            }
            else if (this.UsesProxySemantics)
            {
                num = this.GenerateProxyRequestLine(byteCount);
            }
            else
            {
                num = this.GenerateRequestLine(byteCount);
            }
            Buffer.BlockCopy(HttpBytes, 0, this.WriteBuffer, num, HttpBytes.Length);
            num += HttpBytes.Length;
            this.WriteBuffer[num++] = 0x31;
            this.WriteBuffer[num++] = 0x2e;
            this.WriteBuffer[num++] = this.IsVersionHttp10 ? ((byte) 0x30) : ((byte) 0x31);
            this.WriteBuffer[num++] = 13;
            this.WriteBuffer[num++] = 10;
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "Request: " + Encoding.ASCII.GetString(this.WriteBuffer, 0, num));
            }
            WebHeaderCollection.HeaderEncoding.GetBytes(myString, 0, myString.Length, this.WriteBuffer, num);
        }

        internal void SetAndOrProcessResponse(object responseOrException)
        {
            if (responseOrException == null)
            {
                throw new InternalException();
            }
            CoreResponseData coreResponseData = responseOrException as CoreResponseData;
            WebException exception = responseOrException as WebException;
            object obj2 = Interlocked.CompareExchange(ref this._CoreResponse, responseOrException, DBNull.Value);
            if (obj2 != null)
            {
                if (obj2.GetType() == typeof(CoreResponseData))
                {
                    if (coreResponseData != null)
                    {
                        throw new InternalException();
                    }
                    if (((exception != null) && (exception.InternalStatus != WebExceptionInternalStatus.ServicePointFatal)) && (exception.InternalStatus != WebExceptionInternalStatus.RequestFatal))
                    {
                        return;
                    }
                }
                else if (obj2.GetType() != typeof(DBNull))
                {
                    if (coreResponseData == null)
                    {
                        throw new InternalException();
                    }
                    ICloseEx connectStream = coreResponseData.m_ConnectStream as ICloseEx;
                    if (connectStream != null)
                    {
                        connectStream.CloseEx(CloseExState.Silent);
                        return;
                    }
                    coreResponseData.m_ConnectStream.Close();
                    return;
                }
            }
            if (obj2 == DBNull.Value)
            {
                if (!this.Async)
                {
                    LazyAsyncResult connectionAsyncResult = this.ConnectionAsyncResult;
                    LazyAsyncResult connectionReaderAsyncResult = this.ConnectionReaderAsyncResult;
                    connectionAsyncResult.InvokeCallback(responseOrException);
                    connectionReaderAsyncResult.InvokeCallback(responseOrException);
                }
            }
            else if (obj2 != null)
            {
                Exception e = responseOrException as Exception;
                if (e == null)
                {
                    throw new InternalException();
                }
                this.SetResponse(e);
            }
            else if ((Interlocked.CompareExchange(ref this._CoreResponse, responseOrException, null) != null) && (coreResponseData != null))
            {
                ICloseEx ex2 = coreResponseData.m_ConnectStream as ICloseEx;
                if (ex2 != null)
                {
                    ex2.CloseEx(CloseExState.Silent);
                }
                else
                {
                    coreResponseData.m_ConnectStream.Close();
                }
            }
            else
            {
                if (!this.Async)
                {
                    throw new InternalException();
                }
                if (coreResponseData != null)
                {
                    this.SetResponse(coreResponseData);
                }
                else
                {
                    this.SetResponse(responseOrException as Exception);
                }
            }
        }

        private void SetDateHeaderHelper(string headerName, DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                this.SetSpecialHeaders(headerName, null);
            }
            else
            {
                this.SetSpecialHeaders(headerName, HttpProtocolUtils.date2string(dateTime));
            }
        }

        internal void SetRequestContinue()
        {
            this.SetRequestContinue(null);
        }

        internal void SetRequestContinue(CoreResponseData continueResponse)
        {
            this._RequestContinueCount++;
            if ((this.HttpWriteMode != System.Net.HttpWriteMode.None) && this.m_ContinueGate.Complete())
            {
                TimerThread.Timer continueTimer = this.m_ContinueTimer;
                this.m_ContinueTimer = null;
                if ((continueTimer == null) || continueTimer.Cancel())
                {
                    if ((continueResponse != null) && (this.ContinueDelegate != null))
                    {
                        ExecutionContext executionContext = this.Async ? this.GetWritingContext().ContextCopy : null;
                        if (executionContext == null)
                        {
                            this.ContinueDelegate((int) continueResponse.m_StatusCode, continueResponse.m_ResponseHeaders);
                        }
                        else
                        {
                            ExecutionContext.Run(executionContext, new ContextCallback(this.CallContinueDelegateCallback), continueResponse);
                        }
                    }
                    this.EndWriteHeaders_Part2();
                }
            }
        }

        internal void SetRequestSubmitDone(ConnectStream submitStream)
        {
            if (!this.Async)
            {
                this.ConnectionAsyncResult.InvokeCallback();
            }
            if (this.AllowWriteStreamBuffering)
            {
                submitStream.EnableWriteBuffering();
            }
            if (submitStream.CanTimeout)
            {
                submitStream.ReadTimeout = this.ReadWriteTimeout;
                submitStream.WriteTimeout = this.ReadWriteTimeout;
            }
            if (Logging.On)
            {
                Logging.Associate(Logging.Web, this, submitStream);
            }
            TransportContext context = new ConnectStreamContext(submitStream);
            this.ServerAuthenticationState.TransportContext = context;
            this.ProxyAuthenticationState.TransportContext = context;
            this._SubmitWriteStream = submitStream;
            if ((this.Async && (this._CoreResponse != null)) && (this._CoreResponse != DBNull.Value))
            {
                submitStream.CallDone();
            }
            else
            {
                this.EndSubmitRequest();
            }
        }

        private bool SetRequestSubmitted()
        {
            bool requestSubmitted = this.RequestSubmitted;
            this.m_RequestSubmitted = true;
            return requestSubmitted;
        }

        private void SetResponse(Exception E)
        {
            HttpProcessingResult result = HttpProcessingResult.Continue;
            WebException e = this.HaveResponse ? (this._ReadAResult.Result as WebException) : null;
            WebException exception2 = E as WebException;
            if (((e != null) && ((e.InternalStatus == WebExceptionInternalStatus.RequestFatal) || (e.InternalStatus == WebExceptionInternalStatus.ServicePointFatal))) && ((exception2 == null) || (exception2.InternalStatus != WebExceptionInternalStatus.RequestFatal)))
            {
                E = e;
            }
            else
            {
                e = exception2;
            }
            if ((E != null) && Logging.On)
            {
                Logging.Exception(Logging.Web, this, "", e);
            }
            try
            {
                if ((e != null) && (((e.InternalStatus == WebExceptionInternalStatus.Isolated) || (e.InternalStatus == WebExceptionInternalStatus.ServicePointFatal)) || ((e.InternalStatus == WebExceptionInternalStatus.Recoverable) && !this.m_OnceFailed)))
                {
                    if (e.InternalStatus == WebExceptionInternalStatus.Recoverable)
                    {
                        this.m_OnceFailed = true;
                    }
                    this.Pipelined = false;
                    if (((this._SubmitWriteStream != null) && (this._OldSubmitWriteStream == null)) && this._SubmitWriteStream.BufferOnly)
                    {
                        this._OldSubmitWriteStream = this._SubmitWriteStream;
                    }
                    result = this.DoSubmitRequestProcessing(ref E);
                }
            }
            catch (Exception exception3)
            {
                if (NclUtilities.IsFatal(exception3))
                {
                    throw;
                }
                result = HttpProcessingResult.Continue;
                E = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), exception3, WebExceptionStatus.RequestCanceled, this._HttpResponse);
            }
            finally
            {
                if (result == HttpProcessingResult.Continue)
                {
                    LazyAsyncResult result2;
                    this.CancelTimer();
                    if (!(E is WebException) && !(E is SecurityException))
                    {
                        if (this._HttpResponse == null)
                        {
                            E = new WebException(E.Message, E);
                        }
                        else
                        {
                            E = new WebException(SR.GetString("net_servererror", new object[] { NetRes.GetWebStatusCodeString(this.ResponseStatusCode, this._HttpResponse.StatusDescription) }), E, WebExceptionStatus.ProtocolError, this._HttpResponse);
                        }
                    }
                    LazyAsyncResult result3 = null;
                    HttpWebResponse response = this._HttpResponse;
                    lock (this)
                    {
                        result2 = this._WriteAResult;
                        if (this._ReadAResult == null)
                        {
                            this._ReadAResult = new LazyAsyncResult(null, null, null, E);
                        }
                        else
                        {
                            result3 = this._ReadAResult;
                        }
                    }
                    try
                    {
                        this.FinishRequest(response, E);
                        try
                        {
                            if (result2 != null)
                            {
                                result2.InvokeCallback(E);
                            }
                        }
                        finally
                        {
                            if (result3 != null)
                            {
                                result3.InvokeCallback(E);
                            }
                        }
                    }
                    finally
                    {
                        response = this._ReadAResult.Result as HttpWebResponse;
                        if (response != null)
                        {
                            response.Abort();
                        }
                        if (base.CacheProtocol != null)
                        {
                            base.CacheProtocol.Abort();
                        }
                    }
                }
            }
        }

        private void SetResponse(CoreResponseData coreResponseData)
        {
            try
            {
                if (!this.Async)
                {
                    LazyAsyncResult connectionAsyncResult = this.ConnectionAsyncResult;
                    LazyAsyncResult connectionReaderAsyncResult = this.ConnectionReaderAsyncResult;
                    connectionAsyncResult.InvokeCallback(coreResponseData);
                    connectionReaderAsyncResult.InvokeCallback(coreResponseData);
                }
                if (coreResponseData != null)
                {
                    if (coreResponseData.m_ConnectStream.CanTimeout)
                    {
                        coreResponseData.m_ConnectStream.WriteTimeout = this.ReadWriteTimeout;
                        coreResponseData.m_ConnectStream.ReadTimeout = this.ReadWriteTimeout;
                    }
                    this._HttpResponse = new HttpWebResponse(this.GetRemoteResourceUri(), this.CurrentMethod, coreResponseData, this._MediaType, this.UsesProxySemantics, this.AutomaticDecompression);
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, coreResponseData.m_ConnectStream);
                    }
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, this._HttpResponse);
                    }
                    this.ProcessResponse();
                }
                else
                {
                    this.Abort(null, 1);
                }
            }
            catch (Exception exception)
            {
                this.Abort(exception, 2);
            }
        }

        private void SetSpecialHeaders(string HeaderName, string value)
        {
            value = WebHeaderCollection.CheckBadChars(value, true);
            this._HttpRequestHeaders.RemoveInternal(HeaderName);
            if (value.Length != 0)
            {
                this._HttpRequestHeaders.AddInternal(HeaderName, value);
            }
        }

        private void SubmitRequest(System.Net.ServicePoint servicePoint)
        {
            if (!this.Async)
            {
                this._ConnectionAResult = new LazyAsyncResult(this, null, null);
                this._ConnectionReaderAResult = new LazyAsyncResult(this, null, null);
                this.OpenWriteSideResponseWindow();
            }
            if ((this._Timer == null) && !this.Async)
            {
                this._Timer = this.TimerQueue.CreateTimer(s_TimeoutCallback, this);
            }
            try
            {
                if ((this._SubmitWriteStream != null) && this._SubmitWriteStream.IsPostStream)
                {
                    if ((this._OldSubmitWriteStream == null) && !this._SubmitWriteStream.ErrorInStream)
                    {
                        this._OldSubmitWriteStream = this._SubmitWriteStream;
                    }
                    this._WriteBuffer = null;
                }
                this.m_Retry = false;
                if (this.PreAuthenticate)
                {
                    if ((this.UsesProxySemantics && (this._Proxy != null)) && (this._Proxy.Credentials != null))
                    {
                        this.ProxyAuthenticationState.PreAuthIfNeeded(this, this._Proxy.Credentials);
                    }
                    if (this.Credentials != null)
                    {
                        this.ServerAuthenticationState.PreAuthIfNeeded(this, this.Credentials);
                    }
                }
                if (this.WriteBuffer == null)
                {
                    this.UpdateHeaders();
                }
                if (!this.CheckCacheRetrieveBeforeSubmit())
                {
                    servicePoint.SubmitRequest(this, this.GetConnectionGroupLine());
                }
            }
            finally
            {
                if (!this.Async)
                {
                    this.CheckWriteSideResponseProcessing();
                }
            }
        }

        internal long SwitchToContentLength()
        {
            if (this.HaveResponse)
            {
                return -1L;
            }
            if (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)
            {
                ConnectStream stream = this._OldSubmitWriteStream;
                if (stream == null)
                {
                    stream = this._SubmitWriteStream;
                }
                if ((stream.Connection != null) && (stream.Connection.IISVersion >= 6))
                {
                    return -1L;
                }
            }
            long num = -1L;
            long num2 = this._ContentLength;
            if (this.HttpWriteMode != System.Net.HttpWriteMode.None)
            {
                if (this.HttpWriteMode == System.Net.HttpWriteMode.Buffer)
                {
                    this._ContentLength = this._SubmitWriteStream.BufferedData.Length;
                    this.m_OriginallyBuffered = true;
                    this.HttpWriteMode = System.Net.HttpWriteMode.ContentLength;
                    return -1L;
                }
                if (this.NtlmKeepAlive && (this._OldSubmitWriteStream == null))
                {
                    this._ContentLength = 0L;
                    this._SubmitWriteStream.SuppressWrite = true;
                    if (!this._SubmitWriteStream.BufferOnly)
                    {
                        num = num2;
                    }
                    if (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)
                    {
                        this.HttpWriteMode = System.Net.HttpWriteMode.ContentLength;
                        this._SubmitWriteStream.SwitchToContentLength();
                        num = -2L;
                        this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
                    }
                }
                if (this._OldSubmitWriteStream == null)
                {
                    return num;
                }
                if (this.NtlmKeepAlive)
                {
                    this._ContentLength = 0L;
                }
                else if ((this._ContentLength == 0L) || (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked))
                {
                    this._ContentLength = this._OldSubmitWriteStream.BufferedData.Length;
                }
                if (this.HttpWriteMode == System.Net.HttpWriteMode.Chunked)
                {
                    this.HttpWriteMode = System.Net.HttpWriteMode.ContentLength;
                    this._SubmitWriteStream.SwitchToContentLength();
                    this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
                }
            }
            return num;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            this.GetObjectData(serializationInfo, streamingContext);
        }

        private static void TimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ThreadPool.UnsafeQueueUserWorkItem(s_AbortWrapper, context);
        }

        private static string TrimScopeID(string s)
        {
            int length = s.LastIndexOf('%');
            if (length > 0)
            {
                return s.Substring(0, length);
            }
            return s;
        }

        private bool TryGetHostUri(string hostName, out Uri hostUri)
        {
            StringBuilder builder = new StringBuilder(this._Uri.Scheme);
            builder.Append("://");
            builder.Append(hostName);
            builder.Append(this._Uri.PathAndQuery);
            return Uri.TryCreate(builder.ToString(), UriKind.Absolute, out hostUri);
        }

        internal void UpdateHeaders()
        {
            string safeHostAndPort;
            if (this.UseCustomHost)
            {
                safeHostAndPort = GetSafeHostAndPort(this._HostUri, this._HostHasPort);
            }
            else
            {
                safeHostAndPort = this.GetSafeHostAndPort(false);
            }
            HostHeaderString str2 = new HostHeaderString(safeHostAndPort);
            string str3 = WebHeaderCollection.HeaderEncoding.GetString(str2.Bytes, 0, str2.ByteCount);
            this._HttpRequestHeaders.ChangeInternal("Host", str3);
            if (this._CookieContainer != null)
            {
                CookieModule.OnSendingHeaders(this);
            }
        }

        internal void WriteCallDone(ConnectStream stream, ConnectionReturnResult returnResult)
        {
            if (!object.ReferenceEquals(stream, (this._OldSubmitWriteStream != null) ? this._OldSubmitWriteStream : this._SubmitWriteStream))
            {
                stream.ProcessWriteCallDone(returnResult);
            }
            else if (!this.UserRetrievedWriteStream)
            {
                stream.ProcessWriteCallDone(returnResult);
            }
            else
            {
                object obj2 = (returnResult == null) ? ((object) Missing.Value) : ((object) returnResult);
                if (Interlocked.CompareExchange(ref this.m_PendingReturnResult, obj2, null) == DBNull.Value)
                {
                    stream.ProcessWriteCallDone(returnResult);
                }
            }
        }

        internal void WriteHeadersCallback(WebExceptionStatus errorStatus, ConnectStream stream, bool async)
        {
            if (errorStatus == WebExceptionStatus.Success)
            {
                if (!this.EndWriteHeaders(async))
                {
                    errorStatus = WebExceptionStatus.Pending;
                }
                else if (stream.BytesLeftToWrite == 0L)
                {
                    stream.CallDone();
                }
            }
        }

        internal HttpAbortDelegate AbortDelegate
        {
            set
            {
                this._AbortDelegate = value;
            }
        }

        internal bool Aborted
        {
            get
            {
                return (this.m_Aborted != 0);
            }
        }

        public string Accept
        {
            get
            {
                return this._HttpRequestHeaders["Accept"];
            }
            set
            {
                this.SetSpecialHeaders("Accept", value);
            }
        }

        public Uri Address
        {
            get
            {
                return this._Uri;
            }
        }

        public bool AllowAutoRedirect
        {
            get
            {
                return ((this._Booleans & Booleans.AllowAutoRedirect) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.AllowAutoRedirect;
                }
                else
                {
                    this._Booleans &= ~Booleans.AllowAutoRedirect;
                }
            }
        }

        public bool AllowReadStreamBuffering
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw new InvalidOperationException(SR.GetString("NotSupported"));
                }
            }
        }

        public bool AllowWriteStreamBuffering
        {
            get
            {
                return ((this._Booleans & Booleans.AllowWriteStreamBuffering) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.AllowWriteStreamBuffering;
                }
                else
                {
                    this._Booleans &= ~Booleans.AllowWriteStreamBuffering;
                }
            }
        }

        internal bool Async
        {
            get
            {
                return (this._RequestIsAsync != System.Net.TriState.False);
            }
            set
            {
                if (this._RequestIsAsync == System.Net.TriState.Unspecified)
                {
                    this._RequestIsAsync = value ? System.Net.TriState.True : System.Net.TriState.False;
                }
            }
        }

        public DecompressionMethods AutomaticDecompression
        {
            get
            {
                return this.m_AutomaticDecompression;
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_writestarted"));
                }
                this.m_AutomaticDecompression = value;
            }
        }

        internal bool BodyStarted
        {
            get
            {
                return this.m_BodyStarted;
            }
        }

        private bool CanGetRequestStream
        {
            get
            {
                return !this.CurrentMethod.ContentBodyNotAllowed;
            }
        }

        internal bool CanGetResponseStream
        {
            get
            {
                return !this.CurrentMethod.ExpectNoContentResponse;
            }
        }

        internal Uri ChallengedUri
        {
            get
            {
                return this.CurrentAuthenticationState.ChallengedUri;
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (this._ClientCertificates == null)
                {
                    this._ClientCertificates = new X509CertificateCollection();
                }
                return this._ClientCertificates;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._ClientCertificates = value;
            }
        }

        public string Connection
        {
            get
            {
                return this._HttpRequestHeaders["Connection"];
            }
            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    this._HttpRequestHeaders.RemoveInternal("Connection");
                }
                else
                {
                    string str = value.ToLower(CultureInfo.InvariantCulture);
                    bool flag = str.IndexOf("keep-alive") != -1;
                    bool flag2 = str.IndexOf("close") != -1;
                    if (flag || flag2)
                    {
                        throw new ArgumentException(SR.GetString("net_connarg"), "value");
                    }
                    this._HttpRequestHeaders.CheckUpdate("Connection", value);
                }
            }
        }

        internal LazyAsyncResult ConnectionAsyncResult
        {
            get
            {
                return this._ConnectionAResult;
            }
        }

        public override string ConnectionGroupName
        {
            get
            {
                return this._ConnectionGroupName;
            }
            set
            {
                this._ConnectionGroupName = value;
            }
        }

        internal LazyAsyncResult ConnectionReaderAsyncResult
        {
            get
            {
                return this._ConnectionReaderAResult;
            }
        }

        public override long ContentLength
        {
            get
            {
                return this._ContentLength;
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_writestarted"));
                }
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_clsmall"));
                }
                this._ContentLength = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return this._HttpRequestHeaders["Content-Type"];
            }
            set
            {
                this.SetSpecialHeaders("Content-Type", value);
            }
        }

        public HttpContinueDelegate ContinueDelegate
        {
            get
            {
                return this._ContinueDelegate;
            }
            set
            {
                this._ContinueDelegate = value;
            }
        }

        public System.Net.CookieContainer CookieContainer
        {
            get
            {
                return this._CookieContainer;
            }
            set
            {
                this._CookieContainer = value;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return this._AuthInfo;
            }
            set
            {
                this._AuthInfo = value;
            }
        }

        internal AuthenticationState CurrentAuthenticationState
        {
            get
            {
                if (!this.m_IsCurrentAuthenticationStateProxy)
                {
                    return this._ServerAuthenticationState;
                }
                return this._ProxyAuthenticationState;
            }
            set
            {
                this.m_IsCurrentAuthenticationStateProxy = this._ProxyAuthenticationState == value;
            }
        }

        internal KnownHttpVerb CurrentMethod
        {
            get
            {
                if (this._Verb == null)
                {
                    return this._OriginVerb;
                }
                return this._Verb;
            }
            set
            {
                this._Verb = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return this.GetDateHeaderHelper("Date");
            }
            set
            {
                this.SetDateHeaderHelper("Date", value);
            }
        }

        public static RequestCachePolicy DefaultCachePolicy
        {
            get
            {
                RequestCachePolicy policy = RequestCacheManager.GetBinding(Uri.UriSchemeHttp).Policy;
                if (policy == null)
                {
                    return WebRequest.DefaultCachePolicy;
                }
                return policy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                RequestCacheBinding binding = RequestCacheManager.GetBinding(Uri.UriSchemeHttp);
                RequestCacheManager.SetBinding(Uri.UriSchemeHttp, new RequestCacheBinding(binding.Cache, binding.Validator, value));
            }
        }

        public static int DefaultMaximumErrorResponseLength
        {
            get
            {
                return SettingsSectionInternal.Section.MaximumErrorResponseLength;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_toosmall"));
                }
                SettingsSectionInternal.Section.MaximumErrorResponseLength = value;
            }
        }

        public static int DefaultMaximumResponseHeadersLength
        {
            get
            {
                return SettingsSectionInternal.Section.MaximumResponseHeadersLength;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_toosmall"));
                }
                SettingsSectionInternal.Section.MaximumResponseHeadersLength = value;
            }
        }

        public string Expect
        {
            get
            {
                return this._HttpRequestHeaders["Expect"];
            }
            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    this._HttpRequestHeaders.RemoveInternal("Expect");
                }
                else
                {
                    if (value.ToLower(CultureInfo.InvariantCulture).IndexOf("100-continue") != -1)
                    {
                        throw new ArgumentException(SR.GetString("net_no100"), "value");
                    }
                    this._HttpRequestHeaders.CheckUpdate("Expect", value);
                }
            }
        }

        private bool ExpectContinue
        {
            get
            {
                return ((this._Booleans & Booleans.ExpectContinue) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.ExpectContinue;
                }
                else
                {
                    this._Booleans &= ~Booleans.ExpectContinue;
                }
            }
        }

        internal bool HasEntityBody
        {
            get
            {
                return (((this.HttpWriteMode == System.Net.HttpWriteMode.Chunked) || (this.HttpWriteMode == System.Net.HttpWriteMode.Buffer)) || ((this.HttpWriteMode == System.Net.HttpWriteMode.ContentLength) && (this.ContentLength > 0L)));
            }
        }

        public bool HaveResponse
        {
            get
            {
                return ((this._ReadAResult != null) && this._ReadAResult.InternalPeekCompleted);
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this._HttpRequestHeaders;
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                WebHeaderCollection headers = value;
                WebHeaderCollection headers2 = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
                foreach (string str in headers.AllKeys)
                {
                    headers2.Add(str, headers[str]);
                }
                this._HttpRequestHeaders = headers2;
            }
        }

        internal bool HeadersCompleted
        {
            get
            {
                return this.m_HeadersCompleted;
            }
            set
            {
                this.m_HeadersCompleted = value;
            }
        }

        public string Host
        {
            get
            {
                if (this.UseCustomHost)
                {
                    return GetHostAndPortString(this._HostUri.Host, this._HostUri.Port, this._HostHasPort);
                }
                return GetHostAndPortString(this._Uri.Host, this._Uri.Port, !this._Uri.IsDefaultPort);
            }
            set
            {
                Uri uri;
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_writestarted"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if ((value.IndexOf('/') != -1) || !this.TryGetHostUri(value, out uri))
                {
                    throw new ArgumentException(SR.GetString("net_invalid_host"));
                }
                this.CheckConnectPermission(uri, false);
                this._HostUri = uri;
                if (!this._HostUri.IsDefaultPort)
                {
                    this._HostHasPort = true;
                }
                else if (value.IndexOf(':') == -1)
                {
                    this._HostHasPort = false;
                }
                else
                {
                    int index = value.IndexOf(']');
                    if (index == -1)
                    {
                        this._HostHasPort = true;
                    }
                    else
                    {
                        this._HostHasPort = value.LastIndexOf(':') > index;
                    }
                }
            }
        }

        internal System.Net.HttpWriteMode HttpWriteMode
        {
            get
            {
                return this._HttpWriteMode;
            }
            set
            {
                this._HttpWriteMode = value;
            }
        }

        private bool IdentityRequired
        {
            get
            {
                if ((this.Credentials == null) || !ComNetOS.IsWinNt)
                {
                    return false;
                }
                if (!(this.Credentials is SystemNetworkCredential))
                {
                    if (this.Credentials is NetworkCredential)
                    {
                        return false;
                    }
                    CredentialCache credentials = this.Credentials as CredentialCache;
                    if (credentials != null)
                    {
                        return credentials.IsDefaultInCache;
                    }
                }
                return true;
            }
        }

        public DateTime IfModifiedSince
        {
            get
            {
                return this.GetDateHeaderHelper("If-Modified-Since");
            }
            set
            {
                this.SetDateHeaderHelper("If-Modified-Since", value);
            }
        }

        internal bool InternalConnectionGroup
        {
            set
            {
                this.m_InternalConnectionGroup = value;
            }
        }

        internal IWebProxy InternalProxy
        {
            get
            {
                return this._Proxy;
            }
            set
            {
                this.ProxySet = true;
                this._Proxy = value;
                if (this._ProxyChain != null)
                {
                    this._ProxyChain.Dispose();
                }
                this._ProxyChain = null;
                this.FindServicePoint(true);
            }
        }

        internal bool IsTunnelRequest
        {
            get
            {
                return ((this._Booleans & Booleans.IsTunnelRequest) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.IsTunnelRequest;
                }
                else
                {
                    this._Booleans &= ~Booleans.IsTunnelRequest;
                }
            }
        }

        private bool IsVersionHttp10
        {
            get
            {
                return ((this._Booleans & Booleans.IsVersionHttp10) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.IsVersionHttp10;
                }
                else
                {
                    this._Booleans &= ~Booleans.IsVersionHttp10;
                }
            }
        }

        public bool KeepAlive
        {
            get
            {
                return this.m_KeepAlive;
            }
            set
            {
                this.m_KeepAlive = value;
            }
        }

        internal bool LockConnection
        {
            get
            {
                return this.m_LockConnection;
            }
            set
            {
                this.m_LockConnection = value;
            }
        }

        public int MaximumAutomaticRedirections
        {
            get
            {
                return this._MaximumAllowedRedirections;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(SR.GetString("net_toosmall"), "value");
                }
                this._MaximumAllowedRedirections = value;
            }
        }

        public int MaximumResponseHeadersLength
        {
            get
            {
                return this._MaximumResponseHeadersLength;
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_toosmall"));
                }
                this._MaximumResponseHeadersLength = value;
            }
        }

        public string MediaType
        {
            get
            {
                return this._MediaType;
            }
            set
            {
                this._MediaType = value;
            }
        }

        public override string Method
        {
            get
            {
                return this._OriginVerb.Name;
            }
            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    throw new ArgumentException(SR.GetString("net_badmethod"), "value");
                }
                if (ValidationHelper.IsInvalidHttpString(value))
                {
                    throw new ArgumentException(SR.GetString("net_badmethod"), "value");
                }
                this._OriginVerb = KnownHttpVerb.Parse(value);
            }
        }

        internal bool NtlmKeepAlive
        {
            get
            {
                return this.m_NtlmKeepAlive;
            }
            set
            {
                this.m_NtlmKeepAlive = value;
            }
        }

        public bool Pipelined
        {
            get
            {
                return this.m_Pipelined;
            }
            set
            {
                this.m_Pipelined = value;
            }
        }

        public override bool PreAuthenticate
        {
            get
            {
                return this.m_PreAuthenticate;
            }
            set
            {
                this.m_PreAuthenticate = value;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                if (!this.IsVersionHttp10)
                {
                    return HttpVersion.Version11;
                }
                return HttpVersion.Version10;
            }
            set
            {
                if (value.Equals(HttpVersion.Version11))
                {
                    this.IsVersionHttp10 = false;
                }
                else
                {
                    if (!value.Equals(HttpVersion.Version10))
                    {
                        throw new ArgumentException(SR.GetString("net_wrongversion"), "value");
                    }
                    this.IsVersionHttp10 = true;
                }
            }
        }

        public override IWebProxy Proxy
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this._Proxy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.InternalProxy = value;
            }
        }

        internal AuthenticationState ProxyAuthenticationState
        {
            get
            {
                if (this._ProxyAuthenticationState == null)
                {
                    this._ProxyAuthenticationState = new AuthenticationState(true);
                }
                return this._ProxyAuthenticationState;
            }
        }

        private bool ProxySet
        {
            get
            {
                return ((this._Booleans & Booleans.ProxySet) != ((Booleans) 0));
            }
            set
            {
                if (value)
                {
                    this._Booleans |= Booleans.ProxySet;
                }
                else
                {
                    this._Booleans &= ~Booleans.ProxySet;
                }
            }
        }

        public int ReadWriteTimeout
        {
            get
            {
                return this._ReadWriteTimeout;
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this._ReadWriteTimeout = value;
            }
        }

        public string Referer
        {
            get
            {
                return this._HttpRequestHeaders["Referer"];
            }
            set
            {
                this.SetSpecialHeaders("Referer", value);
            }
        }

        internal int RequestContinueCount
        {
            get
            {
                return this._RequestContinueCount;
            }
        }

        private bool RequestSubmitted
        {
            get
            {
                return this.m_RequestSubmitted;
            }
        }

        internal TimerThread.Timer RequestTimer
        {
            get
            {
                return this._Timer;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this._OriginUri;
            }
        }

        internal bool RequireBody
        {
            get
            {
                return this.CurrentMethod.RequireContentBody;
            }
        }

        internal HttpStatusCode ResponseStatusCode
        {
            get
            {
                return this._HttpResponse.StatusCode;
            }
        }

        internal bool Saw100Continue
        {
            get
            {
                return this.m_Saw100Continue;
            }
            set
            {
                this.m_Saw100Continue = value;
            }
        }

        internal bool SawInitialResponse
        {
            get
            {
                return this.m_SawInitialResponse;
            }
            set
            {
                this.m_SawInitialResponse = value;
            }
        }

        public bool SendChunked
        {
            get
            {
                return ((this._Booleans & Booleans.SendChunked) != ((Booleans) 0));
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_writestarted"));
                }
                if (value)
                {
                    this._Booleans |= Booleans.SendChunked;
                }
                else
                {
                    this._Booleans &= ~Booleans.SendChunked;
                }
            }
        }

        internal AuthenticationState ServerAuthenticationState
        {
            get
            {
                if (this._ServerAuthenticationState == null)
                {
                    this._ServerAuthenticationState = new AuthenticationState(false);
                }
                return this._ServerAuthenticationState;
            }
            set
            {
                this._ServerAuthenticationState = value;
            }
        }

        public System.Net.ServicePoint ServicePoint
        {
            get
            {
                return this.FindServicePoint(false);
            }
        }

        public bool SupportsCookieContainer
        {
            get
            {
                return true;
            }
        }

        public override int Timeout
        {
            get
            {
                return this._Timeout;
            }
            set
            {
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_ge_zero"));
                }
                if (this._Timeout != value)
                {
                    this._Timeout = value;
                    this._TimerQueue = null;
                }
            }
        }

        private TimerThread.Queue TimerQueue
        {
            get
            {
                TimerThread.Queue orCreateQueue = this._TimerQueue;
                if (orCreateQueue == null)
                {
                    orCreateQueue = TimerThread.GetOrCreateQueue((this._Timeout == 0) ? 1 : this._Timeout);
                    this._TimerQueue = orCreateQueue;
                }
                return orCreateQueue;
            }
        }

        public string TransferEncoding
        {
            get
            {
                return this._HttpRequestHeaders["Transfer-Encoding"];
            }
            set
            {
                if (ValidationHelper.IsBlankString(value))
                {
                    this._HttpRequestHeaders.RemoveInternal("Transfer-Encoding");
                }
                else
                {
                    if (value.ToLower(CultureInfo.InvariantCulture).IndexOf("chunked") != -1)
                    {
                        throw new ArgumentException(SR.GetString("net_nochunked"), "value");
                    }
                    if (!this.SendChunked)
                    {
                        throw new InvalidOperationException(SR.GetString("net_needchunked"));
                    }
                    this._HttpRequestHeaders.CheckUpdate("Transfer-Encoding", value);
                }
            }
        }

        private static string UniqueGroupId
        {
            get
            {
                return Interlocked.Increment(ref s_UniqueGroupId).ToString(NumberFormatInfo.InvariantInfo);
            }
        }

        internal System.Net.UnlockConnectionDelegate UnlockConnectionDelegate
        {
            get
            {
                return this._UnlockDelegate;
            }
            set
            {
                this._UnlockDelegate = value;
            }
        }

        public bool UnsafeAuthenticatedConnectionSharing
        {
            get
            {
                return ((this._Booleans & Booleans.UnsafeAuthenticatedConnectionSharing) != ((Booleans) 0));
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (value)
                {
                    this._Booleans |= Booleans.UnsafeAuthenticatedConnectionSharing;
                }
                else
                {
                    this._Booleans &= ~Booleans.UnsafeAuthenticatedConnectionSharing;
                }
            }
        }

        internal bool UnsafeOrProxyAuthenticatedConnectionSharing
        {
            get
            {
                if (!this.m_IsCurrentAuthenticationStateProxy)
                {
                    return this.UnsafeAuthenticatedConnectionSharing;
                }
                return true;
            }
        }

        internal bool UseCustomHost
        {
            get
            {
                return ((this._HostUri != null) && !this._RedirectedToDifferentHost);
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                return (this.Credentials is SystemNetworkCredential);
            }
            set
            {
                if (this.RequestSubmitted)
                {
                    throw new InvalidOperationException(SR.GetString("net_writestarted"));
                }
                this._AuthInfo = value ? CredentialCache.DefaultCredentials : null;
            }
        }

        public string UserAgent
        {
            get
            {
                return this._HttpRequestHeaders["User-Agent"];
            }
            set
            {
                this.SetSpecialHeaders("User-Agent", value);
            }
        }

        private bool UserRetrievedWriteStream
        {
            get
            {
                return ((this._WriteAResult != null) && this._WriteAResult.InternalPeekCompleted);
            }
        }

        private bool UsesProxy
        {
            get
            {
                return this.ServicePoint.InternalProxyServicePoint;
            }
        }

        internal bool UsesProxySemantics
        {
            get
            {
                if (!this.ServicePoint.InternalProxyServicePoint)
                {
                    return false;
                }
                if (this._Uri.Scheme == Uri.UriSchemeHttps)
                {
                    return this.IsTunnelRequest;
                }
                return true;
            }
        }

        internal byte[] WriteBuffer
        {
            get
            {
                return this._WriteBuffer;
            }
        }

        private static class AbortState
        {
            public const int Internal = 2;
            public const int Public = 1;
        }

        [Flags]
        private enum Booleans : uint
        {
            AllowAutoRedirect = 1,
            AllowWriteStreamBuffering = 2,
            Default = 7,
            EnableDecompression = 0x200,
            ExpectContinue = 4,
            IsTunnelRequest = 0x400,
            IsVersionHttp10 = 0x80,
            ProxySet = 0x10,
            SendChunked = 0x100,
            UnsafeAuthenticatedConnectionSharing = 0x40
        }
    }
}

