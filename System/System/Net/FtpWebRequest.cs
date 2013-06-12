namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Sockets;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class FtpWebRequest : WebRequest
    {
        private static readonly NetworkCredential DefaultFtpNetworkCredential = new NetworkCredential("anonymous", "anonymous@", string.Empty);
        private bool m_Aborted;
        private bool m_Async;
        private static readonly GeneralAsyncDelegate m_AsyncCallback = new GeneralAsyncDelegate(FtpWebRequest.AsyncCallbackWrapper);
        private ICredentials m_AuthInfo;
        private bool m_Binary = true;
        private bool m_CacheDone;
        private X509CertificateCollection m_ClientCertificates;
        private FtpControlStream m_Connection;
        private string m_ConnectionGroupName;
        private ConnectionPool m_ConnectionPool;
        private long m_ContentLength;
        private long m_ContentOffset;
        private static readonly CreateConnectionDelegate m_CreateConnectionCallback = new CreateConnectionDelegate(FtpWebRequest.CreateFtpConnection);
        private bool m_EnableSsl;
        private Exception m_Exception;
        private WebHeaderCollection m_FtpRequestHeaders;
        private FtpWebResponse m_FtpWebResponse;
        private bool m_GetRequestStreamStarted;
        private bool m_GetResponseStarted;
        private HttpWebRequest m_HttpWebRequest;
        private bool m_KeepAlive = true;
        private FtpMethodInfo m_MethodInfo;
        private bool m_OnceFailed;
        private bool m_Passive = true;
        private IWebProxy m_Proxy;
        private bool m_ProxyUserSet;
        private LazyAsyncResult m_ReadAsyncResult;
        private int m_ReadWriteTimeout = 0x493e0;
        private int m_RemainingTimeout;
        private string m_RenameTo;
        private LazyAsyncResult m_RequestCompleteAsyncResult;
        private RequestStage m_RequestStage;
        private System.Net.ServicePoint m_ServicePoint;
        private DateTime m_StartTime;
        private Stream m_Stream;
        private object m_SyncObject;
        private bool m_TimedOut;
        private int m_Timeout = s_DefaultTimeout;
        private TimerThread.Callback m_TimerCallback;
        private TimerThread.Queue m_TimerQueue = s_DefaultTimerQueue;
        private readonly Uri m_Uri;
        private ContextAwareResult m_WriteAsyncResult;
        private static readonly int s_DefaultTimeout = 0x186a0;
        private static readonly TimerThread.Queue s_DefaultTimerQueue = TimerThread.GetOrCreateQueue(s_DefaultTimeout);

        internal FtpWebRequest(Uri uri)
        {
            new WebPermission(NetworkAccess.Connect, uri).Demand();
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, ".ctor", uri.ToString());
            }
            if (uri.Scheme != Uri.UriSchemeFtp)
            {
                throw new ArgumentOutOfRangeException("uri");
            }
            this.m_TimerCallback = new TimerThread.Callback(this.TimerCallback);
            this.m_SyncObject = new object();
            NetworkCredential defaultFtpNetworkCredential = null;
            this.m_Uri = uri;
            this.m_MethodInfo = FtpMethodInfo.GetMethodInfo("RETR");
            if ((this.m_Uri.UserInfo != null) && (this.m_Uri.UserInfo.Length != 0))
            {
                string userInfo = this.m_Uri.UserInfo;
                string userName = userInfo;
                string password = "";
                int index = userInfo.IndexOf(':');
                if (index != -1)
                {
                    userName = Uri.UnescapeDataString(userInfo.Substring(0, index));
                    index++;
                    password = Uri.UnescapeDataString(userInfo.Substring(index, userInfo.Length - index));
                }
                defaultFtpNetworkCredential = new NetworkCredential(userName, password);
            }
            if (defaultFtpNetworkCredential == null)
            {
                defaultFtpNetworkCredential = DefaultFtpNetworkCredential;
            }
            this.m_AuthInfo = defaultFtpNetworkCredential;
            base.SetupCacheProtocol(this.m_Uri);
        }

        public override void Abort()
        {
            if (!this.m_Aborted)
            {
                if (Logging.On)
                {
                    Logging.Enter(Logging.Web, this, "Abort", "");
                }
                try
                {
                    if (this.HttpProxyMode)
                    {
                        this.GetHttpWebRequest().Abort();
                    }
                    else
                    {
                        Stream stream;
                        FtpControlStream connection;
                        if (base.CacheProtocol != null)
                        {
                            base.CacheProtocol.Abort();
                        }
                        lock (this.m_SyncObject)
                        {
                            if (this.m_RequestStage >= RequestStage.ReleaseConnection)
                            {
                                return;
                            }
                            this.m_Aborted = true;
                            stream = this.m_Stream;
                            connection = this.m_Connection;
                            this.m_Exception = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                        }
                        if (stream != null)
                        {
                            ((ICloseEx) stream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                        }
                        if (connection != null)
                        {
                            connection.Abort(ExceptionHelper.RequestAbortedException);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, this, "Abort", exception);
                    }
                    throw;
                }
                finally
                {
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.Web, this, "Abort", "");
                    }
                }
            }
        }

        private static void AsyncCallbackWrapper(object request, object state)
        {
            ((FtpWebRequest) request).RequestCallback(state);
        }

        private void AsyncRequestCallback(object obj)
        {
            RequestStage checkForError = RequestStage.CheckForError;
            try
            {
                FtpControlStream objectValue = obj as FtpControlStream;
                FtpDataStream stream2 = obj as FtpDataStream;
                Exception e = obj as Exception;
                bool flag = obj == null;
            Label_001D:
                if (e != null)
                {
                    if (this.AttemptedRecovery(e))
                    {
                        objectValue = this.QueueOrCreateConnection();
                        if (objectValue == null)
                        {
                            return;
                        }
                        e = null;
                    }
                    if (e != null)
                    {
                        this.SetException(e);
                        return;
                    }
                }
                if (objectValue != null)
                {
                    lock (this.m_SyncObject)
                    {
                        if (this.m_Aborted)
                        {
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.Web, this, "", SR.GetString("net_log_releasing_connection", new object[] { ValidationHelper.HashString(objectValue) }));
                            }
                            this.m_ConnectionPool.PutConnection(objectValue, this, this.Timeout);
                            return;
                        }
                        this.m_Connection = objectValue;
                        if (Logging.On)
                        {
                            Logging.Associate(Logging.Web, this, this.m_Connection);
                        }
                    }
                    try
                    {
                        stream2 = (FtpDataStream) this.TimedSubmitRequestHelper(true);
                    }
                    catch (Exception exception2)
                    {
                        e = exception2;
                        goto Label_001D;
                    }
                }
                else if (stream2 != null)
                {
                    lock (this.m_SyncObject)
                    {
                        if (this.m_Aborted)
                        {
                            ((ICloseEx) stream2).CloseEx(CloseExState.Silent | CloseExState.Abort);
                            return;
                        }
                        this.m_Stream = stream2;
                    }
                    stream2.SetSocketTimeoutOption(SocketShutdown.Both, this.Timeout, true);
                    this.EnsureFtpWebResponse(null);
                    this.CheckCacheRetrieveOnResponse();
                    this.CheckCacheUpdateOnResponse();
                    checkForError = stream2.CanRead ? RequestStage.ReadReady : RequestStage.WriteReady;
                }
                else
                {
                    if (!flag)
                    {
                        throw new InternalException();
                    }
                    objectValue = this.m_Connection;
                    bool flag4 = false;
                    if (objectValue != null)
                    {
                        this.EnsureFtpWebResponse(null);
                        this.m_FtpWebResponse.UpdateStatus(objectValue.StatusCode, objectValue.StatusLine, objectValue.ExitMessage);
                        flag4 = !this.m_CacheDone && ((base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Continue) || (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer));
                        lock (this.m_SyncObject)
                        {
                            if (!this.CheckCacheRetrieveOnResponse())
                            {
                                goto Label_001D;
                            }
                            if (this.m_FtpWebResponse.IsFromCache)
                            {
                                flag4 = false;
                            }
                            this.CheckCacheUpdateOnResponse();
                        }
                    }
                    if (!flag4)
                    {
                        checkForError = RequestStage.ReleaseConnection;
                    }
                }
            }
            catch (Exception exception3)
            {
                this.SetException(exception3);
            }
            finally
            {
                this.FinishRequestStage(checkForError);
            }
        }

        private bool AttemptedRecovery(Exception e)
        {
            if (!(e is WebException) || (((WebException) e).InternalStatus != WebExceptionInternalStatus.Isolated))
            {
                if ((((e is ThreadAbortException) || (e is StackOverflowException)) || ((e is OutOfMemoryException) || this.m_OnceFailed)) || ((this.m_Aborted || this.m_TimedOut) || ((this.m_Connection == null) || !this.m_Connection.RecoverableFailure)))
                {
                    return false;
                }
                this.m_OnceFailed = true;
            }
            lock (this.m_SyncObject)
            {
                if ((this.m_ConnectionPool != null) && (this.m_Connection != null))
                {
                    this.m_Connection.CloseSocket();
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, "", SR.GetString("net_log_releasing_connection", new object[] { ValidationHelper.HashString(this.m_Connection) }));
                    }
                    this.m_ConnectionPool.PutConnection(this.m_Connection, this, this.RemainingTimeout);
                    this.m_Connection = null;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginGetRequestStream", "");
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "BeginGetRequestStream", SR.GetString("net_log_method_equal", new object[] { this.m_MethodInfo.Method }));
            }
            ContextAwareResult result = null;
            try
            {
                if (this.m_GetRequestStreamStarted)
                {
                    throw new InvalidOperationException(SR.GetString("net_repcall"));
                }
                this.m_GetRequestStreamStarted = true;
                if (!this.m_MethodInfo.IsUpload)
                {
                    throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
                }
                this.CheckError();
                if (this.ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = this.GetHttpWebRequest();
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, httpWebRequest);
                    }
                    return (ContextAwareResult) httpWebRequest.BeginGetRequestStream(callback, state);
                }
                this.FinishRequestStage(RequestStage.RequestStarted);
                result = new ContextAwareResult(true, true, this, state, callback);
                lock (result.StartPostingAsyncOp())
                {
                    this.m_WriteAsyncResult = result;
                    this.SubmitRequest(true);
                    result.FinishPostingAsyncOp();
                    this.FinishRequestStage(RequestStage.CheckForError);
                }
                return result;
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "BeginGetRequestStream", exception);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "BeginGetRequestStream", "");
                }
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            ContextAwareResult readAsyncResult;
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "BeginGetResponse", "");
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "BeginGetResponse", SR.GetString("net_log_method_equal", new object[] { this.m_MethodInfo.Method }));
            }
            try
            {
                if (this.m_FtpWebResponse != null)
                {
                    readAsyncResult = new ContextAwareResult(this, state, callback);
                    readAsyncResult.InvokeCallback(this.m_FtpWebResponse);
                    return readAsyncResult;
                }
                if (this.m_GetResponseStarted)
                {
                    throw new InvalidOperationException(SR.GetString("net_repcall"));
                }
                this.m_GetResponseStarted = true;
                this.CheckError();
                if (this.ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = this.GetHttpWebRequest();
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, httpWebRequest);
                    }
                    return (ContextAwareResult) httpWebRequest.BeginGetResponse(callback, state);
                }
                RequestStage stage = this.FinishRequestStage(RequestStage.RequestStarted);
                readAsyncResult = new ContextAwareResult(true, true, this, state, callback);
                this.m_ReadAsyncResult = readAsyncResult;
                if (stage >= RequestStage.RequestStarted)
                {
                    readAsyncResult.StartPostingAsyncOp();
                    readAsyncResult.FinishPostingAsyncOp();
                    if (stage >= RequestStage.ReadReady)
                    {
                        readAsyncResult = null;
                    }
                    else
                    {
                        lock (this.m_SyncObject)
                        {
                            if (this.m_RequestStage >= RequestStage.ReadReady)
                            {
                                readAsyncResult = null;
                            }
                        }
                    }
                    if (readAsyncResult == null)
                    {
                        readAsyncResult = (ContextAwareResult) this.m_ReadAsyncResult;
                        if (!readAsyncResult.InternalPeekCompleted)
                        {
                            readAsyncResult.InvokeCallback();
                        }
                    }
                    return readAsyncResult;
                }
                lock (readAsyncResult.StartPostingAsyncOp())
                {
                    this.SubmitRequest(true);
                    readAsyncResult.FinishPostingAsyncOp();
                }
                this.FinishRequestStage(RequestStage.CheckForError);
                return readAsyncResult;
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "BeginGetResponse", exception);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "BeginGetResponse", "");
                }
            }
            return readAsyncResult;
        }

        private bool CheckCacheRetrieveBeforeSubmit()
        {
            if ((base.CacheProtocol == null) || this.m_CacheDone)
            {
                this.m_CacheDone = true;
                return false;
            }
            if ((base.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse) || (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.DoNotTakeFromCache))
            {
                return false;
            }
            Uri requestUri = this.RequestUri;
            string userString = this.GetUserString();
            if (userString != null)
            {
                userString = Uri.EscapeDataString(userString);
            }
            if ((requestUri.Fragment.Length != 0) || (userString != null))
            {
                if (userString == null)
                {
                    requestUri = new Uri(requestUri.GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped));
                }
                else
                {
                    requestUri = new Uri((requestUri.GetParts(UriComponents.KeepDelimiter | UriComponents.Scheme, UriFormat.SafeUnescaped) + userString + '@') + requestUri.GetParts(UriComponents.PathAndQuery | UriComponents.Port | UriComponents.Host, UriFormat.SafeUnescaped));
                }
            }
            base.CacheProtocol.GetRetrieveStatus(requestUri, this);
            if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
            {
                throw base.CacheProtocol.ProtocolException;
            }
            if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
            {
                return false;
            }
            if (this.m_MethodInfo.Operation != FtpOperation.DownloadFile)
            {
                throw new NotSupportedException(SR.GetString("net_cache_not_supported_command"));
            }
            if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                FtpRequestCacheValidator validator = (FtpRequestCacheValidator) base.CacheProtocol.Validator;
                this.m_FtpWebResponse = new FtpWebResponse(base.CacheProtocol.ResponseStream, base.CacheProtocol.ResponseStreamLength, this.RequestUri, this.UsePassive ? FtpStatusCode.DataAlreadyOpen : FtpStatusCode.OpeningData, (this.UsePassive ? FtpStatusCode.DataAlreadyOpen : FtpStatusCode.OpeningData).ToString(), (validator.CacheEntry.LastModifiedUtc == DateTime.MinValue) ? DateTime.Now : validator.CacheEntry.LastModifiedUtc.ToLocalTime(), string.Empty, string.Empty, string.Empty);
                this.m_FtpWebResponse.InternalSetFromCache = true;
                this.m_FtpWebResponse.InternalSetIsCacheFresh = validator.CacheFreshnessStatus != CacheFreshnessStatus.Stale;
            }
            return true;
        }

        private bool CheckCacheRetrieveOnResponse()
        {
            if ((base.CacheProtocol != null) && !this.m_CacheDone)
            {
                if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue)
                {
                    return true;
                }
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                {
                    if (Logging.On)
                    {
                        Logging.Exception(Logging.Web, this, "CheckCacheRetrieveOnResponse", base.CacheProtocol.ProtocolException);
                    }
                    throw base.CacheProtocol.ProtocolException;
                }
                base.CacheProtocol.GetRevalidateStatus(this.m_FtpWebResponse, null);
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
                {
                    if (this.m_FtpWebResponse != null)
                    {
                        this.m_FtpWebResponse.SetResponseStream(null);
                    }
                    return false;
                }
                if (base.CacheProtocol.ProtocolStatus != CacheValidationStatus.ReturnCachedResponse)
                {
                    return false;
                }
                if (this.m_MethodInfo.Operation != FtpOperation.DownloadFile)
                {
                    throw new NotSupportedException(SR.GetString("net_cache_not_supported_command"));
                }
                FtpRequestCacheValidator validator = (FtpRequestCacheValidator) base.CacheProtocol.Validator;
                FtpWebResponse ftpWebResponse = this.m_FtpWebResponse;
                this.m_Stream = base.CacheProtocol.ResponseStream;
                this.m_FtpWebResponse = new FtpWebResponse(base.CacheProtocol.ResponseStream, base.CacheProtocol.ResponseStreamLength, this.RequestUri, this.UsePassive ? FtpStatusCode.DataAlreadyOpen : FtpStatusCode.OpeningData, (this.UsePassive ? FtpStatusCode.DataAlreadyOpen : FtpStatusCode.OpeningData).ToString(), (validator.CacheEntry.LastModifiedUtc == DateTime.MinValue) ? DateTime.Now : validator.CacheEntry.LastModifiedUtc.ToLocalTime(), string.Empty, string.Empty, string.Empty);
                this.m_FtpWebResponse.InternalSetFromCache = true;
                this.m_FtpWebResponse.InternalSetIsCacheFresh = base.CacheProtocol.IsCacheFresh;
                ftpWebResponse.Close();
            }
            return true;
        }

        private void CheckCacheUpdateOnResponse()
        {
            if ((base.CacheProtocol != null) && !this.m_CacheDone)
            {
                this.m_CacheDone = true;
                if (this.m_Connection != null)
                {
                    this.m_FtpWebResponse.UpdateStatus(this.m_Connection.StatusCode, this.m_Connection.StatusLine, this.m_Connection.ExitMessage);
                    if ((this.m_Connection.StatusCode == FtpStatusCode.OpeningData) && (this.m_FtpWebResponse.ContentLength == 0L))
                    {
                        this.m_FtpWebResponse.SetContentLength(this.m_Connection.ContentLength);
                    }
                }
                if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse)
                {
                    this.m_Stream = new CombinedReadStream(base.CacheProtocol.Validator.CacheStream, this.m_FtpWebResponse.GetResponseStream());
                    FtpStatusCode statusCode = this.UsePassive ? FtpStatusCode.DataAlreadyOpen : FtpStatusCode.OpeningData;
                    this.m_FtpWebResponse.UpdateStatus(statusCode, statusCode.ToString(), string.Empty);
                    this.m_FtpWebResponse.SetResponseStream(this.m_Stream);
                }
                if (base.CacheProtocol.GetUpdateStatus(this.m_FtpWebResponse, this.m_FtpWebResponse.GetResponseStream()) == CacheValidationStatus.UpdateResponseInformation)
                {
                    this.m_Stream = base.CacheProtocol.ResponseStream;
                    this.m_FtpWebResponse.SetResponseStream(this.m_Stream);
                }
                else if (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Fail)
                {
                    throw base.CacheProtocol.ProtocolException;
                }
            }
        }

        private void CheckError()
        {
            if (this.m_Exception != null)
            {
                throw this.m_Exception;
            }
        }

        private static PooledStream CreateFtpConnection(ConnectionPool pool)
        {
            return new FtpControlStream(pool, TimeSpan.MaxValue, false);
        }

        internal void DataStreamClosed(CloseExState closeState)
        {
            if ((closeState & CloseExState.Abort) == CloseExState.Normal)
            {
                if (this.m_Async)
                {
                    this.m_RequestCompleteAsyncResult.InternalWaitForCompletion();
                    this.CheckError();
                }
                else if (this.m_Connection != null)
                {
                    this.m_Connection.CheckContinuePipeline();
                }
            }
            else
            {
                FtpControlStream connection = this.m_Connection;
                if (connection != null)
                {
                    connection.Abort(ExceptionHelper.RequestAbortedException);
                }
            }
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndGetRequestStream", "");
            }
            Stream stream = null;
            try
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                if ((result == null) || (this.HttpProxyMode ? (result.AsyncObject != this.GetHttpWebRequest()) : (result.AsyncObject != this)))
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
                }
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndGetResponse" }));
                }
                if (this.HttpProxyMode)
                {
                    stream = this.GetHttpWebRequest().EndGetRequestStream(asyncResult);
                }
                else
                {
                    result.InternalWaitForCompletion();
                    result.EndCalled = true;
                    this.CheckError();
                    stream = this.m_Stream;
                    result.EndCalled = true;
                }
                if (stream.CanTimeout)
                {
                    stream.WriteTimeout = this.ReadWriteTimeout;
                    stream.ReadTimeout = this.ReadWriteTimeout;
                }
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetRequestStream", exception);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "EndGetRequestStream", "");
                }
            }
            return stream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "EndGetResponse", "");
            }
            try
            {
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                if (result == null)
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
                }
                if (this.HttpProxyMode ? (result.AsyncObject != this.GetHttpWebRequest()) : (result.AsyncObject != this))
                {
                    throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
                }
                if (result.EndCalled)
                {
                    throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndGetResponse" }));
                }
                if (this.HttpProxyMode)
                {
                    try
                    {
                        this.CheckError();
                        if (this.m_FtpWebResponse == null)
                        {
                            this.m_FtpWebResponse = new FtpWebResponse((HttpWebResponse) this.GetHttpWebRequest().EndGetResponse(asyncResult));
                        }
                        goto Label_0174;
                    }
                    catch (WebException exception)
                    {
                        if ((exception.Response != null) && (exception.Response is HttpWebResponse))
                        {
                            throw new WebException(exception.Message, null, exception.Status, new FtpWebResponse((HttpWebResponse) exception.Response), exception.InternalStatus);
                        }
                        throw;
                    }
                }
                result.InternalWaitForCompletion();
                result.EndCalled = true;
                this.CheckError();
            }
            catch (Exception exception2)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "EndGetResponse", exception2);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "EndGetResponse", "");
                }
            }
        Label_0174:
            return this.m_FtpWebResponse;
        }

        private void EnsureFtpWebResponse(Exception exception)
        {
            if ((this.m_FtpWebResponse == null) || ((this.m_FtpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream) && (this.m_Stream != null)))
            {
                lock (this.m_SyncObject)
                {
                    if ((this.m_FtpWebResponse == null) || ((this.m_FtpWebResponse.GetResponseStream() is FtpWebResponse.EmptyStream) && (this.m_Stream != null)))
                    {
                        Stream stream = this.m_Stream;
                        if (this.m_MethodInfo.IsUpload)
                        {
                            stream = null;
                        }
                        if (((this.m_Stream != null) && this.m_Stream.CanRead) && this.m_Stream.CanTimeout)
                        {
                            this.m_Stream.ReadTimeout = this.ReadWriteTimeout;
                            this.m_Stream.WriteTimeout = this.ReadWriteTimeout;
                        }
                        FtpControlStream connection = this.m_Connection;
                        long contentLength = (connection != null) ? connection.ContentLength : -1L;
                        if ((stream == null) && (contentLength < 0L))
                        {
                            contentLength = 0L;
                        }
                        if (this.m_FtpWebResponse != null)
                        {
                            this.m_FtpWebResponse.SetResponseStream(stream);
                        }
                        else if (connection != null)
                        {
                            this.m_FtpWebResponse = new FtpWebResponse(stream, contentLength, connection.ResponseUri, connection.StatusCode, connection.StatusLine, connection.LastModified, connection.BannerMessage, connection.WelcomeMessage, connection.ExitMessage);
                        }
                        else
                        {
                            this.m_FtpWebResponse = new FtpWebResponse(stream, -1L, this.m_Uri, FtpStatusCode.Undefined, null, DateTime.Now, null, null, null);
                        }
                    }
                }
            }
        }

        private RequestStage FinishRequestStage(RequestStage stage)
        {
            RequestStage requestStage;
            LazyAsyncResult writeAsyncResult;
            LazyAsyncResult readAsyncResult;
            FtpControlStream connection;
            RequestStage stage3;
            if (this.m_Exception != null)
            {
                stage = RequestStage.ReleaseConnection;
            }
            lock (this.m_SyncObject)
            {
                requestStage = this.m_RequestStage;
                if (stage == RequestStage.CheckForError)
                {
                    return requestStage;
                }
                if ((requestStage == RequestStage.ReleaseConnection) && (stage == RequestStage.ReleaseConnection))
                {
                    return RequestStage.ReleaseConnection;
                }
                if (stage > requestStage)
                {
                    this.m_RequestStage = stage;
                }
                if (stage <= RequestStage.RequestStarted)
                {
                    return requestStage;
                }
                writeAsyncResult = this.m_WriteAsyncResult;
                readAsyncResult = this.m_ReadAsyncResult;
                connection = this.m_Connection;
                if (stage == RequestStage.ReleaseConnection)
                {
                    if ((((this.m_Exception == null) && !this.m_Aborted) && ((requestStage != RequestStage.ReadReady) && this.m_MethodInfo.IsDownload)) && !this.m_FtpWebResponse.IsFromCache)
                    {
                        return requestStage;
                    }
                    if (((this.m_Exception != null) || !this.m_FtpWebResponse.IsFromCache) || this.KeepAlive)
                    {
                        this.m_Connection = null;
                    }
                }
            }
            try
            {
                if (((stage == RequestStage.ReleaseConnection) || (requestStage == RequestStage.ReleaseConnection)) && (connection != null))
                {
                    try
                    {
                        if (this.m_Exception != null)
                        {
                            connection.Abort(this.m_Exception);
                        }
                        else if (this.m_FtpWebResponse.IsFromCache && !this.KeepAlive)
                        {
                            connection.Quit();
                        }
                    }
                    finally
                    {
                        if (Logging.On)
                        {
                            Logging.PrintInfo(Logging.Web, this, "", SR.GetString("net_log_releasing_connection", new object[] { ValidationHelper.HashString(connection) }));
                        }
                        this.m_ConnectionPool.PutConnection(connection, this, this.RemainingTimeout);
                        if (this.m_Async && (this.m_RequestCompleteAsyncResult != null))
                        {
                            this.m_RequestCompleteAsyncResult.InvokeCallback();
                        }
                    }
                }
                stage3 = requestStage;
            }
            finally
            {
                try
                {
                    if (stage >= RequestStage.WriteReady)
                    {
                        if (this.m_MethodInfo.IsUpload && !this.m_GetRequestStreamStarted)
                        {
                            if (this.m_Stream != null)
                            {
                                this.m_Stream.Close();
                            }
                        }
                        else if ((writeAsyncResult != null) && !writeAsyncResult.InternalPeekCompleted)
                        {
                            writeAsyncResult.InvokeCallback();
                        }
                    }
                }
                finally
                {
                    if (((stage >= RequestStage.ReadReady) && (readAsyncResult != null)) && !readAsyncResult.InternalPeekCompleted)
                    {
                        readAsyncResult.InvokeCallback();
                    }
                }
            }
            return stage3;
        }

        private string GetConnectionGroupLine()
        {
            return (this.ConnectionGroupName + "_" + this.GetUserString());
        }

        private HttpWebRequest GetHttpWebRequest()
        {
            lock (this.m_SyncObject)
            {
                if (this.m_HttpWebRequest == null)
                {
                    RequestCacheLevel bypassCache;
                    if (this.m_ContentOffset > 0L)
                    {
                        throw new InvalidOperationException(SR.GetString("net_ftp_no_offsetforhttp"));
                    }
                    if (!this.m_MethodInfo.HasHttpCommand)
                    {
                        throw new InvalidOperationException(SR.GetString("net_ftp_no_http_cmd"));
                    }
                    this.m_HttpWebRequest = new HttpWebRequest(this.m_Uri, this.ServicePoint);
                    this.m_HttpWebRequest.Credentials = this.Credentials;
                    this.m_HttpWebRequest.InternalProxy = this.m_Proxy;
                    this.m_HttpWebRequest.KeepAlive = this.KeepAlive;
                    this.m_HttpWebRequest.Timeout = this.Timeout;
                    this.m_HttpWebRequest.Method = this.m_MethodInfo.HttpCommand;
                    this.m_HttpWebRequest.CacheProtocol = base.CacheProtocol;
                    if (this.CachePolicy == null)
                    {
                        bypassCache = RequestCacheLevel.BypassCache;
                    }
                    else
                    {
                        bypassCache = this.CachePolicy.Level;
                    }
                    if (bypassCache == RequestCacheLevel.Revalidate)
                    {
                        bypassCache = RequestCacheLevel.Reload;
                    }
                    this.m_HttpWebRequest.CachePolicy = new HttpRequestCachePolicy((HttpRequestCacheLevel) bypassCache);
                    base.CacheProtocol = null;
                }
            }
            return this.m_HttpWebRequest;
        }

        public override Stream GetRequestStream()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "GetRequestStream", "");
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "GetRequestStream", SR.GetString("net_log_method_equal", new object[] { this.m_MethodInfo.Method }));
            }
            try
            {
                if (this.m_GetRequestStreamStarted)
                {
                    throw new InvalidOperationException(SR.GetString("net_repcall"));
                }
                this.m_GetRequestStreamStarted = true;
                if (!this.m_MethodInfo.IsUpload)
                {
                    throw new ProtocolViolationException(SR.GetString("net_nouploadonget"));
                }
                this.CheckError();
                this.m_StartTime = DateTime.UtcNow;
                this.m_RemainingTimeout = this.Timeout;
                System.Net.ServicePoint servicePoint = this.ServicePoint;
                if (this.Timeout != -1)
                {
                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.m_StartTime);
                    this.m_RemainingTimeout = this.Timeout - ((int) span.TotalMilliseconds);
                    if (this.m_RemainingTimeout <= 0)
                    {
                        throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    }
                }
                if (this.ServicePoint.InternalProxyServicePoint)
                {
                    HttpWebRequest httpWebRequest = this.GetHttpWebRequest();
                    if (Logging.On)
                    {
                        Logging.Associate(Logging.Web, this, httpWebRequest);
                    }
                    this.m_Stream = httpWebRequest.GetRequestStream();
                }
                else
                {
                    this.FinishRequestStage(RequestStage.RequestStarted);
                    this.SubmitRequest(false);
                    this.FinishRequestStage(RequestStage.WriteReady);
                    this.CheckError();
                }
                if (this.m_Stream.CanTimeout)
                {
                    this.m_Stream.WriteTimeout = this.ReadWriteTimeout;
                    this.m_Stream.ReadTimeout = this.ReadWriteTimeout;
                }
            }
            catch (Exception exception)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "GetRequestStream", exception);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "GetRequestStream", "");
                }
            }
            return this.m_Stream;
        }

        public override WebResponse GetResponse()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, this, "GetResponse", "");
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, this, "GetResponse", SR.GetString("net_log_method_equal", new object[] { this.m_MethodInfo.Method }));
            }
            try
            {
                this.CheckError();
                if (this.m_FtpWebResponse != null)
                {
                    return this.m_FtpWebResponse;
                }
                if (this.m_GetResponseStarted)
                {
                    throw new InvalidOperationException(SR.GetString("net_repcall"));
                }
                this.m_GetResponseStarted = true;
                this.m_StartTime = DateTime.UtcNow;
                this.m_RemainingTimeout = this.Timeout;
                System.Net.ServicePoint servicePoint = this.ServicePoint;
                if (this.Timeout != -1)
                {
                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.m_StartTime);
                    this.m_RemainingTimeout = this.Timeout - ((int) span.TotalMilliseconds);
                    if (this.m_RemainingTimeout <= 0)
                    {
                        throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    }
                }
                if (this.ServicePoint.InternalProxyServicePoint)
                {
                    if (this.EnableSsl)
                    {
                        this.m_GetResponseStarted = false;
                        throw new WebException(SR.GetString("net_ftp_proxy_does_not_support_ssl"));
                    }
                    try
                    {
                        HttpWebRequest httpWebRequest = this.GetHttpWebRequest();
                        if (Logging.On)
                        {
                            Logging.Associate(Logging.Web, this, httpWebRequest);
                        }
                        this.m_FtpWebResponse = new FtpWebResponse((HttpWebResponse) httpWebRequest.GetResponse());
                        goto Label_02FD;
                    }
                    catch (WebException exception)
                    {
                        if ((exception.Response != null) && (exception.Response is HttpWebResponse))
                        {
                            exception = new WebException(exception.Message, null, exception.Status, new FtpWebResponse((HttpWebResponse) exception.Response), exception.InternalStatus);
                        }
                        this.SetException(exception);
                        throw exception;
                    }
                    catch (InvalidOperationException exception2)
                    {
                        this.SetException(exception2);
                        this.FinishRequestStage(RequestStage.CheckForError);
                        throw;
                    }
                }
                RequestStage stage = this.FinishRequestStage(RequestStage.RequestStarted);
                if (stage >= RequestStage.RequestStarted)
                {
                    if (stage < RequestStage.ReadReady)
                    {
                        lock (this.m_SyncObject)
                        {
                            if (this.m_RequestStage < RequestStage.ReadReady)
                            {
                                this.m_ReadAsyncResult = new LazyAsyncResult(null, null, null);
                            }
                        }
                        if (this.m_ReadAsyncResult != null)
                        {
                            this.m_ReadAsyncResult.InternalWaitForCompletion();
                        }
                        this.CheckError();
                    }
                }
                else
                {
                    do
                    {
                        this.SubmitRequest(false);
                        if (this.m_MethodInfo.IsUpload)
                        {
                            this.FinishRequestStage(RequestStage.WriteReady);
                        }
                        else
                        {
                            this.FinishRequestStage(RequestStage.ReadReady);
                        }
                        this.CheckError();
                    }
                    while (!this.CheckCacheRetrieveOnResponse());
                    this.EnsureFtpWebResponse(null);
                    this.CheckCacheUpdateOnResponse();
                    if (this.m_FtpWebResponse.IsFromCache)
                    {
                        this.FinishRequestStage(RequestStage.ReleaseConnection);
                    }
                }
            }
            catch (Exception exception3)
            {
                if (Logging.On)
                {
                    Logging.Exception(Logging.Web, this, "GetResponse", exception3);
                }
                if (this.m_Exception == null)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.Web, SR.GetString("net_log_unexpected_exception", new object[] { "GetResponse()" }));
                    }
                    NclUtilities.IsFatal(exception3);
                    this.SetException(exception3);
                    this.FinishRequestStage(RequestStage.CheckForError);
                }
                throw;
            }
            finally
            {
                if (Logging.On)
                {
                    Logging.Exit(Logging.Web, this, "GetResponse", "");
                }
            }
        Label_02FD:
            return this.m_FtpWebResponse;
        }

        internal string GetUserString()
        {
            string strA = null;
            if (this.Credentials != null)
            {
                NetworkCredential credential = this.Credentials.GetCredential(this.m_Uri, "basic");
                if (credential != null)
                {
                    strA = credential.InternalGetUserName();
                    string domain = credential.InternalGetDomain();
                    if (!ValidationHelper.IsBlankString(domain))
                    {
                        strA = domain + @"\" + strA;
                    }
                }
            }
            if ((strA != null) && (string.Compare(strA, "anonymous", StringComparison.InvariantCultureIgnoreCase) != 0))
            {
                return strA;
            }
            return null;
        }

        internal override ContextAwareResult GetWritingContext()
        {
            if ((this.m_ReadAsyncResult != null) && (this.m_ReadAsyncResult is ContextAwareResult))
            {
                return (ContextAwareResult) this.m_ReadAsyncResult;
            }
            if (this.m_WriteAsyncResult != null)
            {
                return this.m_WriteAsyncResult;
            }
            return null;
        }

        private FtpControlStream QueueOrCreateConnection()
        {
            FtpControlStream objectValue = (FtpControlStream) this.m_ConnectionPool.GetConnection(this, this.m_Async ? m_AsyncCallback : null, this.m_Async ? -1 : this.RemainingTimeout);
            if (objectValue == null)
            {
                return null;
            }
            lock (this.m_SyncObject)
            {
                if (this.m_Aborted)
                {
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.Web, this, "", SR.GetString("net_log_releasing_connection", new object[] { ValidationHelper.HashString(objectValue) }));
                    }
                    this.m_ConnectionPool.PutConnection(objectValue, this, this.RemainingTimeout);
                    this.CheckError();
                    throw new InternalException();
                }
                this.m_Connection = objectValue;
                if (Logging.On)
                {
                    Logging.Associate(Logging.Web, this, this.m_Connection);
                }
            }
            return objectValue;
        }

        internal override void RequestCallback(object obj)
        {
            if (this.m_Async)
            {
                this.AsyncRequestCallback(obj);
            }
            else
            {
                this.SyncRequestCallback(obj);
            }
        }

        private void SetException(Exception exception)
        {
            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
            {
                this.m_Exception = exception;
                throw exception;
            }
            FtpControlStream connection = this.m_Connection;
            if (this.m_Exception == null)
            {
                if (exception is WebException)
                {
                    this.EnsureFtpWebResponse(exception);
                    this.m_Exception = new WebException(exception.Message, null, ((WebException) exception).Status, this.m_FtpWebResponse);
                }
                else if ((exception is AuthenticationException) || (exception is SecurityException))
                {
                    this.m_Exception = exception;
                }
                else if ((connection != null) && (connection.StatusCode != FtpStatusCode.Undefined))
                {
                    this.EnsureFtpWebResponse(exception);
                    this.m_Exception = new WebException(SR.GetString("net_servererror", new object[] { connection.StatusLine }), exception, WebExceptionStatus.ProtocolError, this.m_FtpWebResponse);
                }
                else
                {
                    this.m_Exception = new WebException(exception.Message, exception);
                }
                if ((connection != null) && (this.m_FtpWebResponse != null))
                {
                    this.m_FtpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);
                }
            }
        }

        private void SubmitRequest(bool async)
        {
            try
            {
                FtpControlStream stream;
                this.m_Async = async;
                if (this.CheckCacheRetrieveBeforeSubmit())
                {
                    this.RequestCallback(null);
                    return;
                }
                if (this.m_ConnectionPool == null)
                {
                    this.m_ConnectionPool = ConnectionPoolManager.GetConnectionPool(this.ServicePoint, this.GetConnectionGroupLine(), m_CreateConnectionCallback);
                }
            Label_003F:
                stream = this.m_Connection;
                if (stream == null)
                {
                    stream = this.QueueOrCreateConnection();
                    if (stream == null)
                    {
                        return;
                    }
                }
                if (!async && (this.Timeout != -1))
                {
                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - this.m_StartTime);
                    this.m_RemainingTimeout = this.Timeout - ((int) span.TotalMilliseconds);
                    if (this.m_RemainingTimeout <= 0)
                    {
                        throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
                    }
                }
                stream.SetSocketTimeoutOption(SocketShutdown.Both, this.RemainingTimeout, false);
                try
                {
                    this.TimedSubmitRequestHelper(async);
                }
                catch (Exception exception)
                {
                    if (!this.AttemptedRecovery(exception))
                    {
                        throw;
                    }
                    if (!async && (this.Timeout != -1))
                    {
                        TimeSpan span2 = (TimeSpan) (DateTime.UtcNow - this.m_StartTime);
                        this.m_RemainingTimeout = this.Timeout - ((int) span2.TotalMilliseconds);
                        if (this.m_RemainingTimeout <= 0)
                        {
                            throw;
                        }
                    }
                    goto Label_003F;
                }
            }
            catch (WebException exception2)
            {
                IOException innerException = exception2.InnerException as IOException;
                if (innerException != null)
                {
                    SocketException exception4 = innerException.InnerException as SocketException;
                    if ((exception4 != null) && (exception4.ErrorCode == 0x274c))
                    {
                        this.SetException(new WebException(SR.GetString("net_timeout"), WebExceptionStatus.Timeout));
                    }
                }
                this.SetException(exception2);
            }
            catch (Exception exception5)
            {
                this.SetException(exception5);
            }
        }

        private void SyncRequestCallback(object obj)
        {
            RequestStage checkForError = RequestStage.CheckForError;
            try
            {
                bool flag = obj == null;
                Exception exception = obj as Exception;
                if (exception != null)
                {
                    this.SetException(exception);
                }
                else
                {
                    if (!flag)
                    {
                        throw new InternalException();
                    }
                    FtpControlStream connection = this.m_Connection;
                    bool flag2 = false;
                    if (connection != null)
                    {
                        this.EnsureFtpWebResponse(null);
                        this.m_FtpWebResponse.UpdateStatus(connection.StatusCode, connection.StatusLine, connection.ExitMessage);
                        flag2 = !this.m_CacheDone && ((base.CacheProtocol.ProtocolStatus == CacheValidationStatus.Continue) || (base.CacheProtocol.ProtocolStatus == CacheValidationStatus.RetryResponseFromServer));
                        if (this.m_MethodInfo.IsUpload)
                        {
                            this.CheckCacheRetrieveOnResponse();
                            this.CheckCacheUpdateOnResponse();
                        }
                    }
                    if (!flag2)
                    {
                        checkForError = RequestStage.ReleaseConnection;
                    }
                }
            }
            catch (Exception exception2)
            {
                this.SetException(exception2);
            }
            finally
            {
                this.FinishRequestStage(checkForError);
                this.CheckError();
            }
        }

        private Stream TimedSubmitRequestHelper(bool async)
        {
            if (async)
            {
                if (this.m_RequestCompleteAsyncResult == null)
                {
                    this.m_RequestCompleteAsyncResult = new LazyAsyncResult(null, null, null);
                }
                return this.m_Connection.SubmitRequest(this, true, true);
            }
            Stream stream = null;
            bool flag = false;
            TimerThread.Timer timer = this.TimerQueue.CreateTimer(this.m_TimerCallback, null);
            try
            {
                stream = this.m_Connection.SubmitRequest(this, false, true);
            }
            catch (Exception exception)
            {
                if ((!(exception is SocketException) && !(exception is ObjectDisposedException)) || !timer.HasExpired)
                {
                    timer.Cancel();
                    throw;
                }
                flag = true;
            }
            if (flag || !timer.Cancel())
            {
                this.m_TimedOut = true;
                throw new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
            }
            if (stream != null)
            {
                lock (this.m_SyncObject)
                {
                    if (this.m_Aborted)
                    {
                        ((ICloseEx) stream).CloseEx(CloseExState.Silent | CloseExState.Abort);
                        this.CheckError();
                        throw new InternalException();
                    }
                    this.m_Stream = stream;
                }
            }
            return stream;
        }

        private void TimerCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            FtpControlStream connection = this.m_Connection;
            if (connection != null)
            {
                connection.AbortConnect();
            }
        }

        internal bool Aborted
        {
            get
            {
                return this.m_Aborted;
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (this.m_ClientCertificates == null)
                {
                    lock (this.m_SyncObject)
                    {
                        if (this.m_ClientCertificates == null)
                        {
                            this.m_ClientCertificates = new X509CertificateCollection();
                        }
                    }
                }
                return this.m_ClientCertificates;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_ClientCertificates = value;
            }
        }

        public override string ConnectionGroupName
        {
            get
            {
                return this.m_ConnectionGroupName;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_ConnectionGroupName = value;
            }
        }

        public override long ContentLength
        {
            get
            {
                return this.m_ContentLength;
            }
            set
            {
                this.m_ContentLength = value;
            }
        }

        public long ContentOffset
        {
            get
            {
                return this.m_ContentOffset;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_ContentOffset = value;
            }
        }

        public override string ContentType
        {
            get
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return this.m_AuthInfo;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value is SystemNetworkCredential)
                {
                    throw new ArgumentException(SR.GetString("net_ftp_no_defaultcreds"), "value");
                }
                this.m_AuthInfo = value;
            }
        }

        public static RequestCachePolicy DefaultCachePolicy
        {
            get
            {
                RequestCachePolicy policy = RequestCacheManager.GetBinding(Uri.UriSchemeFtp).Policy;
                if (policy == null)
                {
                    return WebRequest.DefaultCachePolicy;
                }
                return policy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                RequestCacheBinding binding = RequestCacheManager.GetBinding(Uri.UriSchemeFtp);
                RequestCacheManager.SetBinding(Uri.UriSchemeFtp, new RequestCacheBinding(binding.Cache, binding.Validator, value));
            }
        }

        internal static NetworkCredential DefaultNetworkCredential
        {
            get
            {
                return DefaultFtpNetworkCredential;
            }
        }

        public bool EnableSsl
        {
            get
            {
                return this.m_EnableSsl;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_EnableSsl = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                if (this.HttpProxyMode)
                {
                    return this.GetHttpWebRequest().Headers;
                }
                if (this.m_FtpRequestHeaders == null)
                {
                    this.m_FtpRequestHeaders = new WebHeaderCollection(WebHeaderCollectionType.FtpWebRequest);
                }
                return this.m_FtpRequestHeaders;
            }
            set
            {
                if (this.HttpProxyMode)
                {
                    this.GetHttpWebRequest().Headers = value;
                }
                this.m_FtpRequestHeaders = value;
            }
        }

        private bool HttpProxyMode
        {
            get
            {
                return (this.m_HttpWebRequest != null);
            }
        }

        private bool InUse
        {
            get
            {
                if (!this.m_GetRequestStreamStarted && !this.m_GetResponseStarted)
                {
                    return false;
                }
                return true;
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
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_KeepAlive = value;
            }
        }

        public override string Method
        {
            get
            {
                return this.m_MethodInfo.Method;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(SR.GetString("net_ftp_invalid_method_name"), "value");
                }
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                try
                {
                    this.m_MethodInfo = FtpMethodInfo.GetMethodInfo(value);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException(SR.GetString("net_ftp_unsupported_method"), "value");
                }
            }
        }

        internal FtpMethodInfo MethodInfo
        {
            get
            {
                return this.m_MethodInfo;
            }
        }

        public override bool PreAuthenticate
        {
            get
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }

        public override IWebProxy Proxy
        {
            get
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                return this.m_Proxy;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_ProxyUserSet = true;
                this.m_Proxy = value;
                this.m_ServicePoint = null;
                System.Net.ServicePoint servicePoint = this.ServicePoint;
            }
        }

        public int ReadWriteTimeout
        {
            get
            {
                return this.m_ReadWriteTimeout;
            }
            set
            {
                if (this.m_GetResponseStarted)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_gt_zero"));
                }
                this.m_ReadWriteTimeout = value;
            }
        }

        internal int RemainingTimeout
        {
            get
            {
                return this.m_RemainingTimeout;
            }
        }

        public string RenameTo
        {
            get
            {
                return this.m_RenameTo;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(SR.GetString("net_ftp_invalid_renameto"), "value");
                }
                this.m_RenameTo = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.m_Uri;
            }
        }

        public System.Net.ServicePoint ServicePoint
        {
            get
            {
                if (this.m_ServicePoint == null)
                {
                    IWebProxy internalDefaultWebProxy = this.m_Proxy;
                    if (!this.m_ProxyUserSet)
                    {
                        internalDefaultWebProxy = WebRequest.InternalDefaultWebProxy;
                    }
                    System.Net.ServicePoint point = ServicePointManager.FindServicePoint(this.m_Uri, internalDefaultWebProxy);
                    lock (this.m_SyncObject)
                    {
                        if (this.m_ServicePoint == null)
                        {
                            this.m_ServicePoint = point;
                            this.m_Proxy = internalDefaultWebProxy;
                        }
                    }
                }
                return this.m_ServicePoint;
            }
        }

        public override int Timeout
        {
            get
            {
                return this.m_Timeout;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_io_timeout_use_ge_zero"));
                }
                if (this.m_Timeout != value)
                {
                    this.m_Timeout = value;
                    this.m_TimerQueue = null;
                }
            }
        }

        private TimerThread.Queue TimerQueue
        {
            get
            {
                if (this.m_TimerQueue == null)
                {
                    this.m_TimerQueue = TimerThread.GetOrCreateQueue(this.RemainingTimeout);
                }
                return this.m_TimerQueue;
            }
        }

        public bool UseBinary
        {
            get
            {
                return this.m_Binary;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_Binary = value;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
            set
            {
                throw ExceptionHelper.PropertyNotSupportedException;
            }
        }

        public bool UsePassive
        {
            get
            {
                return this.m_Passive;
            }
            set
            {
                if (this.InUse)
                {
                    throw new InvalidOperationException(SR.GetString("net_reqsubmitted"));
                }
                this.m_Passive = value;
            }
        }

        private enum RequestStage
        {
            CheckForError,
            RequestStarted,
            WriteReady,
            ReadReady,
            ReleaseConnection
        }
    }
}

