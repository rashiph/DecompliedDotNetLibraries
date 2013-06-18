namespace System.Web.Services.Protocols
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;

    [ComVisible(true)]
    public abstract class HttpSimpleClientProtocol : HttpWebClientProtocol
    {
        private HttpClientType clientType;

        protected HttpSimpleClientProtocol()
        {
            Type type = base.GetType();
            this.clientType = (HttpClientType) WebClientProtocol.GetFromCache(type);
            if (this.clientType == null)
            {
                lock (WebClientProtocol.InternalSyncObject)
                {
                    this.clientType = (HttpClientType) WebClientProtocol.GetFromCache(type);
                    if (this.clientType == null)
                    {
                        this.clientType = new HttpClientType(type);
                        WebClientProtocol.AddToCache(type, this.clientType);
                    }
                }
            }
        }

        internal override void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState)
        {
            InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
            if (state.ParamWriter != null)
            {
                state.ParamWriter.InitializeRequest(request, state.Parameters);
                if (state.ParamWriter.UsesWriteRequest && (state.Parameters.Length > 0))
                {
                    state.ParamWriter.WriteRequest(requestStream, state.Parameters);
                }
            }
        }

        protected IAsyncResult BeginInvoke(string methodName, string requestUrl, object[] parameters, AsyncCallback callback, object asyncState)
        {
            HttpClientMethod clientMethod = this.GetClientMethod(methodName);
            MimeParameterWriter parameterWriter = this.GetParameterWriter(clientMethod);
            Uri requestUri = new Uri(requestUrl);
            if (parameterWriter != null)
            {
                parameterWriter.RequestEncoding = base.RequestEncoding;
                requestUrl = parameterWriter.GetRequestUrl(requestUri.AbsoluteUri, parameters);
                requestUri = new Uri(requestUrl, true);
            }
            InvokeAsyncState internalAsyncState = new InvokeAsyncState(clientMethod, parameterWriter, parameters);
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, internalAsyncState, null, callback, asyncState);
            return base.BeginSend(requestUri, asyncResult, parameterWriter.UsesWriteRequest);
        }

        protected object EndInvoke(IAsyncResult asyncResult)
        {
            object internalAsyncState = null;
            Stream responseStream = null;
            WebResponse response = base.EndSend(asyncResult, ref internalAsyncState, ref responseStream);
            InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
            return this.ReadResponse(state.Method, response, responseStream);
        }

        private HttpClientMethod GetClientMethod(string methodName)
        {
            HttpClientMethod method = this.clientType.GetMethod(methodName);
            if (method == null)
            {
                throw new ArgumentException(Res.GetString("WebInvalidMethodName", new object[] { methodName }), "methodName");
            }
            return method;
        }

        private MimeParameterWriter GetParameterWriter(HttpClientMethod method)
        {
            if (method.writerType == null)
            {
                return null;
            }
            return (MimeParameterWriter) MimeFormatter.CreateInstance(method.writerType, method.writerInitializer);
        }

        internal override void InitializeAsyncRequest(WebRequest request, object internalAsyncState)
        {
            InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
            if (state.ParamWriter.UsesWriteRequest && (state.Parameters.Length == 0))
            {
                request.ContentLength = 0L;
            }
        }

        protected object Invoke(string methodName, string requestUrl, object[] parameters)
        {
            WebResponse webResponse = null;
            object obj2;
            HttpClientMethod clientMethod = this.GetClientMethod(methodName);
            MimeParameterWriter parameterWriter = this.GetParameterWriter(clientMethod);
            Uri uri = new Uri(requestUrl);
            if (parameterWriter != null)
            {
                parameterWriter.RequestEncoding = base.RequestEncoding;
                requestUrl = parameterWriter.GetRequestUrl(uri.AbsoluteUri, parameters);
                uri = new Uri(requestUrl, true);
            }
            WebRequest webRequest = null;
            try
            {
                webRequest = this.GetWebRequest(uri);
                base.NotifyClientCallOut(webRequest);
                base.PendingSyncRequest = webRequest;
                if (parameterWriter != null)
                {
                    parameterWriter.InitializeRequest(webRequest, parameters);
                    if (parameterWriter.UsesWriteRequest)
                    {
                        if (parameters.Length == 0)
                        {
                            webRequest.ContentLength = 0L;
                        }
                        else
                        {
                            Stream requestStream = null;
                            try
                            {
                                requestStream = webRequest.GetRequestStream();
                                parameterWriter.WriteRequest(requestStream, parameters);
                            }
                            finally
                            {
                                if (requestStream != null)
                                {
                                    requestStream.Close();
                                }
                            }
                        }
                    }
                }
                webResponse = this.GetWebResponse(webRequest);
                Stream responseStream = null;
                if (webResponse.ContentLength != 0L)
                {
                    responseStream = webResponse.GetResponseStream();
                }
                obj2 = this.ReadResponse(clientMethod, webResponse, responseStream);
            }
            finally
            {
                if (webRequest == base.PendingSyncRequest)
                {
                    base.PendingSyncRequest = null;
                }
            }
            return obj2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected void InvokeAsync(string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback)
        {
            this.InvokeAsync(methodName, requestUrl, parameters, callback, null);
        }

        protected void InvokeAsync(string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback, object userState)
        {
            if (userState == null)
            {
                userState = base.NullToken;
            }
            AsyncOperation userAsyncState = AsyncOperationManager.CreateOperation(new UserToken(callback, userState));
            WebClientAsyncResult result = new WebClientAsyncResult(this, null, null, new AsyncCallback(this.InvokeAsyncCallback), userAsyncState);
            try
            {
                base.AsyncInvokes.Add(userState, result);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsync", exception);
                }
                Exception exception2 = new ArgumentException(Res.GetString("AsyncDuplicateUserState"), exception);
                object[] results = new object[1];
                InvokeCompletedEventArgs arg = new InvokeCompletedEventArgs(results, exception2, false, userState);
                userAsyncState.PostOperationCompleted(callback, arg);
                return;
            }
            try
            {
                HttpClientMethod clientMethod = this.GetClientMethod(methodName);
                MimeParameterWriter parameterWriter = this.GetParameterWriter(clientMethod);
                Uri requestUri = new Uri(requestUrl);
                if (parameterWriter != null)
                {
                    parameterWriter.RequestEncoding = base.RequestEncoding;
                    requestUrl = parameterWriter.GetRequestUrl(requestUri.AbsoluteUri, parameters);
                    requestUri = new Uri(requestUrl, true);
                }
                result.InternalAsyncState = new InvokeAsyncState(clientMethod, parameterWriter, parameters);
                base.BeginSend(requestUri, result, parameterWriter.UsesWriteRequest);
            }
            catch (Exception exception3)
            {
                if (((exception3 is ThreadAbortException) || (exception3 is StackOverflowException)) || (exception3 is OutOfMemoryException))
                {
                    throw;
                }
                if (Tracing.On)
                {
                    Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsync", exception3);
                }
                object[] objArray2 = new object[1];
                base.OperationCompleted(userState, objArray2, exception3, false);
            }
        }

        private void InvokeAsyncCallback(IAsyncResult result)
        {
            object obj2 = null;
            Exception e = null;
            WebClientAsyncResult asyncResult = (WebClientAsyncResult) result;
            if (asyncResult.Request != null)
            {
                try
                {
                    object internalAsyncState = null;
                    Stream responseStream = null;
                    WebResponse response = base.EndSend(asyncResult, ref internalAsyncState, ref responseStream);
                    InvokeAsyncState state = (InvokeAsyncState) internalAsyncState;
                    obj2 = this.ReadResponse(state.Method, response, responseStream);
                }
                catch (Exception exception2)
                {
                    if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                    {
                        throw;
                    }
                    e = exception2;
                    if (Tracing.On)
                    {
                        Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsyncCallback", exception2);
                    }
                }
            }
            AsyncOperation asyncState = (AsyncOperation) result.AsyncState;
            UserToken userSuppliedState = (UserToken) asyncState.UserSuppliedState;
            base.OperationCompleted(userSuppliedState.UserState, new object[] { obj2 }, e, false);
        }

        private object ReadResponse(HttpClientMethod method, WebResponse response, Stream responseStream)
        {
            HttpWebResponse response2 = response as HttpWebResponse;
            if ((response2 != null) && (response2.StatusCode >= HttpStatusCode.MultipleChoices))
            {
                throw new WebException(RequestResponseUtils.CreateResponseExceptionString(response2, responseStream), null, WebExceptionStatus.ProtocolError, response2);
            }
            if ((method.readerType != null) && (responseStream != null))
            {
                MimeReturnReader reader = (MimeReturnReader) MimeFormatter.CreateInstance(method.readerType, method.readerInitializer);
                return reader.Read(response, responseStream);
            }
            return null;
        }

        private class InvokeAsyncState
        {
            internal HttpClientMethod Method;
            internal object[] Parameters;
            internal MimeParameterWriter ParamWriter;

            internal InvokeAsyncState(HttpClientMethod method, MimeParameterWriter paramWriter, object[] parameters)
            {
                this.Method = method;
                this.ParamWriter = paramWriter;
                this.Parameters = parameters;
            }
        }
    }
}

