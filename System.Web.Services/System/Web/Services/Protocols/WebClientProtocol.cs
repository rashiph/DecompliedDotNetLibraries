namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;

    [ComVisible(true)]
    public abstract class WebClientProtocol : Component
    {
        private Hashtable asyncInvokes;
        private static RequestCachePolicy bypassCache;
        private static ClientTypeCache cache = new ClientTypeCache();
        private string connectionGroupName;
        private ICredentials credentials;
        private RemoteDebugger debugger;
        private static AsyncCallback getRequestStreamAsyncCallback;
        private static AsyncCallback getResponseAsyncCallback;
        private object nullToken;
        private WebRequest pendingSyncRequest;
        private bool preAuthenticate;
        private static AsyncCallback readResponseAsyncCallback;
        private Encoding requestEncoding;
        private static object s_InternalSyncObject;
        private int timeout;
        private System.Uri uri;

        protected WebClientProtocol()
        {
            this.nullToken = new object();
            this.asyncInvokes = Hashtable.Synchronized(new Hashtable());
            this.timeout = 0x186a0;
        }

        internal WebClientProtocol(WebClientProtocol protocol)
        {
            this.nullToken = new object();
            this.asyncInvokes = Hashtable.Synchronized(new Hashtable());
            this.credentials = protocol.credentials;
            this.uri = protocol.uri;
            this.timeout = protocol.timeout;
            this.connectionGroupName = protocol.connectionGroupName;
            this.requestEncoding = protocol.requestEncoding;
        }

        public virtual void Abort()
        {
            WebRequest pendingSyncRequest = this.PendingSyncRequest;
            if (pendingSyncRequest != null)
            {
                pendingSyncRequest.Abort();
            }
        }

        protected static void AddToCache(Type type, object value)
        {
            cache.Add(type, value);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        internal virtual void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState)
        {
            throw new NotSupportedException(Res.GetString("ProtocolDoesNotAsyncSerialize"));
        }

        internal IAsyncResult BeginSend(System.Uri requestUri, WebClientAsyncResult asyncResult, bool callWriteAsyncRequest)
        {
            if (readResponseAsyncCallback == null)
            {
                lock (InternalSyncObject)
                {
                    if (readResponseAsyncCallback == null)
                    {
                        getRequestStreamAsyncCallback = new AsyncCallback(WebClientProtocol.GetRequestStreamAsyncCallback);
                        getResponseAsyncCallback = new AsyncCallback(WebClientProtocol.GetResponseAsyncCallback);
                        readResponseAsyncCallback = new AsyncCallback(WebClientProtocol.ReadResponseAsyncCallback);
                    }
                }
            }
            WebRequest webRequest = this.GetWebRequest(requestUri);
            asyncResult.Request = webRequest;
            this.InitializeAsyncRequest(webRequest, asyncResult.InternalAsyncState);
            if (callWriteAsyncRequest)
            {
                webRequest.BeginGetRequestStream(getRequestStreamAsyncCallback, asyncResult);
            }
            else
            {
                webRequest.BeginGetResponse(getResponseAsyncCallback, asyncResult);
            }
            if (!asyncResult.IsCompleted)
            {
                asyncResult.CombineCompletedSynchronously(false);
            }
            return asyncResult;
        }

        internal WebResponse EndSend(IAsyncResult asyncResult, ref object internalAsyncState, ref Stream responseStream)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(Res.GetString("WebNullAsyncResultInEnd"));
            }
            WebClientAsyncResult result = (WebClientAsyncResult) asyncResult;
            if (result.EndSendCalled)
            {
                throw new InvalidOperationException(Res.GetString("CanTCallTheEndMethodOfAnAsyncCallMoreThan"));
            }
            result.EndSendCalled = true;
            WebResponse response = result.WaitForResponse();
            internalAsyncState = result.InternalAsyncState;
            responseStream = result.ResponseBufferedStream;
            return response;
        }

        protected static object GetFromCache(Type type)
        {
            return cache[type];
        }

        private static void GetRequestStreamAsyncCallback(IAsyncResult asyncResult)
        {
            WebClientAsyncResult asyncState = (WebClientAsyncResult) asyncResult.AsyncState;
            asyncState.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            bool flag = true;
            try
            {
                Stream requestStream = asyncState.Request.EndGetRequestStream(asyncResult);
                flag = false;
                try
                {
                    asyncState.ClientProtocol.AsyncBufferedSerialize(asyncState.Request, requestStream, asyncState.InternalAsyncState);
                }
                finally
                {
                    requestStream.Close();
                }
                asyncState.Request.BeginGetResponse(getResponseAsyncCallback, asyncState);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                ProcessAsyncException(asyncState, exception, "GetRequestStreamAsyncCallback");
                if (flag)
                {
                    WebException exception2 = exception as WebException;
                    if ((exception2 != null) && (exception2.Response != null))
                    {
                        asyncState.Complete(exception);
                    }
                }
            }
        }

        private static void GetResponseAsyncCallback(IAsyncResult asyncResult)
        {
            WebClientAsyncResult asyncState = (WebClientAsyncResult) asyncResult.AsyncState;
            asyncState.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            try
            {
                asyncState.Response = asyncState.ClientProtocol.GetWebResponse(asyncState.Request, asyncResult);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                ProcessAsyncException(asyncState, exception, "GetResponseAsyncCallback");
                if (asyncState.Response == null)
                {
                    return;
                }
            }
            ReadAsyncResponse(asyncState);
        }

        protected virtual WebRequest GetWebRequest(System.Uri uri)
        {
            if (uri == null)
            {
                throw new InvalidOperationException(Res.GetString("WebMissingPath"));
            }
            WebRequest request = WebRequest.Create(uri);
            this.PendingSyncRequest = request;
            request.Timeout = this.timeout;
            request.ConnectionGroupName = this.connectionGroupName;
            request.Credentials = this.Credentials;
            request.PreAuthenticate = this.PreAuthenticate;
            request.CachePolicy = BypassCache;
            return request;
        }

        protected virtual WebResponse GetWebResponse(WebRequest request)
        {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "GetWebResponse", new object[0]) : null;
            WebResponse response = null;
            try
            {
                if (Tracing.On)
                {
                    Tracing.Enter("WebRequest.GetResponse", caller, new TraceMethod(request, "GetResponse", new object[0]));
                }
                response = request.GetResponse();
                if (Tracing.On)
                {
                    Tracing.Exit("WebRequest.GetResponse", caller);
                }
            }
            catch (WebException exception)
            {
                if (exception.Response == null)
                {
                    throw exception;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "GetWebResponse", exception);
                }
                response = exception.Response;
            }
            finally
            {
                if (this.debugger != null)
                {
                    this.debugger.NotifyClientCallReturn(response);
                }
            }
            return response;
        }

        protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = request.EndGetResponse(result);
            if ((response != null) && (this.debugger != null))
            {
                this.debugger.NotifyClientCallReturn(response);
            }
            return response;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        internal virtual void InitializeAsyncRequest(WebRequest request, object internalAsyncState)
        {
        }

        internal void NotifyClientCallOut(WebRequest request)
        {
            if (RemoteDebugger.IsClientCallOutEnabled())
            {
                this.debugger = new RemoteDebugger();
                this.debugger.NotifyClientCallOut(request);
            }
            else
            {
                this.debugger = null;
            }
        }

        private static void ProcessAsyncException(WebClientAsyncResult client, Exception e, string method)
        {
            if (Tracing.On)
            {
                Tracing.ExceptionCatch(TraceEventType.Error, typeof(WebClientProtocol), method, e);
            }
            WebException exception = e as WebException;
            if ((exception != null) && (exception.Response != null))
            {
                client.Response = exception.Response;
            }
            else
            {
                if (client.IsCompleted)
                {
                    throw new InvalidOperationException(Res.GetString("ThereWasAnErrorDuringAsyncProcessing"), e);
                }
                client.Complete(e);
            }
        }

        private static bool ProcessAsyncResponseStreamResult(WebClientAsyncResult client, IAsyncResult asyncResult)
        {
            bool flag;
            int count = client.ResponseStream.EndRead(asyncResult);
            long contentLength = client.Response.ContentLength;
            if ((contentLength > 0L) && (count == contentLength))
            {
                client.ResponseBufferedStream = new MemoryStream(client.Buffer);
                flag = true;
            }
            else if (count > 0)
            {
                if (client.ResponseBufferedStream == null)
                {
                    int capacity = (contentLength == -1L) ? client.Buffer.Length : ((int) contentLength);
                    client.ResponseBufferedStream = new MemoryStream(capacity);
                }
                client.ResponseBufferedStream.Write(client.Buffer, 0, count);
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                client.Complete();
            }
            return flag;
        }

        private static void ReadAsyncResponse(WebClientAsyncResult client)
        {
            if (client.Response.ContentLength == 0L)
            {
                client.Complete();
            }
            else
            {
                try
                {
                    client.ResponseStream = client.Response.GetResponseStream();
                    ReadAsyncResponseStream(client);
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    ProcessAsyncException(client, exception, "ReadAsyncResponse");
                }
            }
        }

        private static void ReadAsyncResponseStream(WebClientAsyncResult client)
        {
            IAsyncResult result;
            do
            {
                byte[] buffer = client.Buffer;
                long contentLength = client.Response.ContentLength;
                if (buffer == null)
                {
                    buffer = client.Buffer = new byte[(contentLength == -1L) ? ((int) 0x400L) : ((int) contentLength)];
                }
                else if ((contentLength != -1L) && (contentLength > buffer.Length))
                {
                    buffer = client.Buffer = new byte[contentLength];
                }
                result = client.ResponseStream.BeginRead(buffer, 0, buffer.Length, readResponseAsyncCallback, client);
            }
            while (result.CompletedSynchronously && !ProcessAsyncResponseStreamResult(client, result));
        }

        private static void ReadResponseAsyncCallback(IAsyncResult asyncResult)
        {
            WebClientAsyncResult asyncState = (WebClientAsyncResult) asyncResult.AsyncState;
            asyncState.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            if (!asyncResult.CompletedSynchronously)
            {
                try
                {
                    if (!ProcessAsyncResponseStreamResult(asyncState, asyncResult))
                    {
                        ReadAsyncResponseStream(asyncState);
                    }
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    ProcessAsyncException(asyncState, exception, "ReadResponseAsyncCallback");
                }
            }
        }

        internal Hashtable AsyncInvokes
        {
            get
            {
                return this.asyncInvokes;
            }
        }

        internal static RequestCachePolicy BypassCache
        {
            get
            {
                if (bypassCache == null)
                {
                    bypassCache = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                }
                return bypassCache;
            }
        }

        [DefaultValue("")]
        public string ConnectionGroupName
        {
            get
            {
                if (this.connectionGroupName != null)
                {
                    return this.connectionGroupName;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.connectionGroupName = value;
            }
        }

        public ICredentials Credentials
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.credentials;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.credentials = value;
            }
        }

        internal static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal object NullToken
        {
            get
            {
                return this.nullToken;
            }
        }

        internal WebRequest PendingSyncRequest
        {
            get
            {
                return this.pendingSyncRequest;
            }
            set
            {
                this.pendingSyncRequest = value;
            }
        }

        [DefaultValue(false), WebServicesDescription("ClientProtocolPreAuthenticate")]
        public bool PreAuthenticate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.preAuthenticate;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.preAuthenticate = value;
            }
        }

        [WebServicesDescription("ClientProtocolEncoding"), DefaultValue((string) null), SettingsBindable(true)]
        public Encoding RequestEncoding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.requestEncoding;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.requestEncoding = value;
            }
        }

        [DefaultValue(0x186a0), SettingsBindable(true), WebServicesDescription("ClientProtocolTimeout")]
        public int Timeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = (value < -1) ? -1 : value;
            }
        }

        internal System.Uri Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }

        [SettingsBindable(true), WebServicesDescription("ClientProtocolUrl"), DefaultValue("")]
        public string Url
        {
            get
            {
                if (this.uri != null)
                {
                    return this.uri.ToString();
                }
                return string.Empty;
            }
            set
            {
                this.uri = new System.Uri(value);
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                if (this.credentials != CredentialCache.DefaultCredentials)
                {
                    return false;
                }
                return true;
            }
            set
            {
                this.credentials = value ? CredentialCache.DefaultCredentials : null;
            }
        }
    }
}

