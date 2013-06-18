namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Xml;

    internal abstract class HttpRequestContext : System.ServiceModel.Channels.RequestContextBase
    {
        private System.ServiceModel.Channels.HttpInput httpInput;
        private HttpOutput httpOutput;
        private HttpChannelListener listener;
        private SecurityMessageProperty securityProperty;

        protected HttpRequestContext(HttpChannelListener listener, Message requestMessage) : base(requestMessage, listener.InternalCloseTimeout, listener.InternalSendTimeout)
        {
            this.listener = listener;
        }

        private Message CreateAckMessage(HttpStatusCode statusCode, string statusDescription)
        {
            Message message = new NullMessage();
            HttpResponseMessageProperty property = new HttpResponseMessageProperty {
                StatusCode = statusCode,
                SuppressEntityBody = true
            };
            if (statusDescription.Length > 0)
            {
                property.StatusDescription = statusDescription;
            }
            message.Properties.Add(HttpResponseMessageProperty.Name, property);
            return message;
        }

        internal static HttpRequestContext CreateContext(HttpChannelListener listener, HttpListenerContext listenerContext)
        {
            return new ListenerHttpContext(listener, listenerContext);
        }

        public void CreateMessage()
        {
            Exception exception;
            Message message = this.HttpInput.ParseIncomingMessage(out exception);
            if ((message == null) && (exception == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), new XmlException(System.ServiceModel.SR.GetString("MessageIsEmpty"))));
            }
            this.SetRequestMessage(message, exception);
        }

        protected abstract System.ServiceModel.Channels.HttpInput GetHttpInput();
        protected abstract HttpOutput GetHttpOutput(Message message);
        protected override void OnAbort()
        {
            if (this.httpOutput != null)
            {
                this.httpOutput.Abort(HttpAbortReason.Aborted);
            }
        }

        protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReplyAsyncResult(this, message, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.httpOutput != null)
            {
                this.httpOutput.Close();
            }
        }

        protected override void OnEndReply(IAsyncResult result)
        {
            ReplyAsyncResult.End(result);
        }

        protected abstract SecurityMessageProperty OnProcessAuthentication();
        protected override void OnReply(Message message, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            Message message2 = null;
            message2 = message;
            try
            {
                bool flag = this.PrepareReply(ref message2);
                ThreadTrace.Trace("Begin sending http reply");
                this.httpOutput.Send(helper.RemainingTime());
                if (TD.MessageSentByTransportIsEnabled())
                {
                    TD.MessageSentByTransport(this.Listener.Uri.AbsoluteUri);
                }
                ThreadTrace.Trace("End sending http reply");
                if (flag)
                {
                    this.httpOutput.Close();
                }
            }
            finally
            {
                if ((message != null) && !object.ReferenceEquals(message, message2))
                {
                    message2.Close();
                }
            }
        }

        private bool PrepareReply(ref Message message)
        {
            if (message == null)
            {
                message = this.CreateAckMessage(HttpStatusCode.Accepted, string.Empty);
            }
            if (!this.listener.ManualAddressing)
            {
                if (message.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                {
                    if ((message.Version.Addressing != AddressingVersion.WSAddressing10) && (message.Version.Addressing != AddressingVersion.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { message.Version.Addressing })));
                    }
                    if ((message.Headers.To != null) && ((this.listener.AnonymousUriPrefixMatcher == null) || !this.listener.AnonymousUriPrefixMatcher.IsAnonymousUri(message.Headers.To)))
                    {
                        message.Headers.To = null;
                    }
                }
                else if (((message.Headers.To == null) || (this.listener.AnonymousUriPrefixMatcher == null)) || !this.listener.AnonymousUriPrefixMatcher.IsAnonymousUri(message.Headers.To))
                {
                    message.Headers.To = message.Version.Addressing.AnonymousUri;
                }
            }
            message.Properties.AllowOutputBatching = false;
            this.httpOutput = this.GetHttpOutput(message);
            HttpDelayedAcceptStream inputStream = this.HttpInput.InputStream as HttpDelayedAcceptStream;
            if (((inputStream != null) && TransferModeHelper.IsRequestStreamed(this.listener.TransferMode)) && inputStream.EnableDelayedAccept(this.httpOutput))
            {
                return false;
            }
            return true;
        }

        public bool ProcessAuthentication()
        {
            HttpStatusCode statusCode = this.ValidateAuthentication();
            if (statusCode == HttpStatusCode.OK)
            {
                bool flag = false;
                statusCode = HttpStatusCode.Forbidden;
                try
                {
                    this.securityProperty = this.OnProcessAuthentication();
                    flag = true;
                    return true;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (exception.Data.Contains("HttpStatusCode") && (exception.Data["HttpStatusCode"] is HttpStatusCode))
                    {
                        statusCode = (HttpStatusCode) exception.Data["HttpStatusCode"];
                    }
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        this.SendResponseAndClose(statusCode);
                    }
                }
            }
            this.SendResponseAndClose(statusCode);
            return false;
        }

        internal void SendResponseAndClose(HttpStatusCode statusCode)
        {
            this.SendResponseAndClose(statusCode, string.Empty);
        }

        internal void SendResponseAndClose(HttpStatusCode statusCode, string statusDescription)
        {
            if (base.ReplyInitiated)
            {
                this.Close();
            }
            else
            {
                using (Message message = this.CreateAckMessage(statusCode, statusDescription))
                {
                    this.Reply(message);
                }
                this.Close();
            }
        }

        private void SetRequestMessage(Message message, Exception requestException)
        {
            if (requestException != null)
            {
                base.SetRequestMessage(requestException);
                message.Close();
            }
            else
            {
                message.Properties.Security = (this.securityProperty != null) ? ((SecurityMessageProperty) this.securityProperty.CreateCopy()) : null;
                base.SetRequestMessage(message);
            }
        }

        protected abstract HttpStatusCode ValidateAuthentication();

        protected System.ServiceModel.Channels.HttpInput HttpInput
        {
            get
            {
                if (this.httpInput == null)
                {
                    this.httpInput = this.GetHttpInput();
                }
                return this.httpInput;
            }
        }

        public abstract string HttpMethod { get; }

        public bool KeepAliveEnabled
        {
            get
            {
                return this.listener.KeepAliveEnabled;
            }
        }

        protected HttpChannelListener Listener
        {
            get
            {
                return this.listener;
            }
        }

        private class ListenerHttpContext : HttpRequestContext, HttpRequestMessageProperty.IHttpHeaderProvider
        {
            private HttpListenerContext listenerContext;

            public ListenerHttpContext(HttpChannelListener listener, HttpListenerContext listenerContext) : base(listener, null)
            {
                this.listenerContext = listenerContext;
            }

            protected override HttpInput GetHttpInput()
            {
                return new ListenerContextHttpInput(this);
            }

            protected override HttpOutput GetHttpOutput(Message message)
            {
                if ((this.listenerContext.Request.ContentLength64 == -1L) && !OSEnvironmentHelper.IsVistaOrGreater)
                {
                    this.listenerContext.Response.KeepAlive = false;
                }
                else
                {
                    this.listenerContext.Response.KeepAlive = base.listener.KeepAliveEnabled;
                }
                return HttpOutput.CreateHttpOutput(this.listenerContext.Response, base.Listener, message, this.HttpMethod);
            }

            protected override void OnAbort()
            {
                this.listenerContext.Response.Abort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                base.OnClose(new TimeoutHelper(timeout).RemainingTime());
                try
                {
                    this.listenerContext.Response.Close();
                }
                catch (HttpListenerException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                }
            }

            protected override SecurityMessageProperty OnProcessAuthentication()
            {
                return base.Listener.ProcessAuthentication(this.listenerContext);
            }

            void HttpRequestMessageProperty.IHttpHeaderProvider.CopyHeaders(WebHeaderCollection headers)
            {
                HttpListenerRequest request = this.listenerContext.Request;
                headers.Add(request.Headers);
                if ((request.UserAgent != null) && (headers[HttpRequestHeader.UserAgent] == null))
                {
                    headers.Add(HttpRequestHeader.UserAgent, request.UserAgent);
                }
            }

            protected override HttpStatusCode ValidateAuthentication()
            {
                return base.Listener.ValidateAuthentication(this.listenerContext);
            }

            public override string HttpMethod
            {
                get
                {
                    return this.listenerContext.Request.HttpMethod;
                }
            }

            private class ListenerContextHttpInput : HttpInput
            {
                private string cachedContentType;
                private HttpRequestContext.ListenerHttpContext listenerHttpContext;
                private byte[] preReadBuffer;

                public ListenerContextHttpInput(HttpRequestContext.ListenerHttpContext listenerHttpContext) : base(listenerHttpContext.Listener, true, listenerHttpContext.listener.IsChannelBindingSupportEnabled)
                {
                    this.listenerHttpContext = listenerHttpContext;
                    if (this.listenerHttpContext.listenerContext.Request.ContentLength64 == -1L)
                    {
                        this.preReadBuffer = new byte[1];
                        if (this.listenerHttpContext.listenerContext.Request.InputStream.Read(this.preReadBuffer, 0, 1) == 0)
                        {
                            this.preReadBuffer = null;
                        }
                    }
                }

                protected override void AddProperties(Message message)
                {
                    HttpRequestMessageProperty property = new HttpRequestMessageProperty(this.listenerHttpContext) {
                        Method = this.listenerHttpContext.listenerContext.Request.HttpMethod
                    };
                    if (this.listenerHttpContext.listenerContext.Request.Url.Query.Length > 1)
                    {
                        property.QueryString = this.listenerHttpContext.listenerContext.Request.Url.Query.Substring(1);
                    }
                    message.Properties.Add(HttpRequestMessageProperty.Name, property);
                    message.Properties.Via = this.listenerHttpContext.listenerContext.Request.Url;
                    RemoteEndpointMessageProperty property2 = new RemoteEndpointMessageProperty(this.listenerHttpContext.listenerContext.Request.RemoteEndPoint);
                    message.Properties.Add(RemoteEndpointMessageProperty.Name, property2);
                }

                protected override Stream GetInputStream()
                {
                    if (this.preReadBuffer != null)
                    {
                        return new ListenerContextInputStream(this.listenerHttpContext, this.preReadBuffer);
                    }
                    return new ListenerContextInputStream(this.listenerHttpContext);
                }

                protected override System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
                {
                    get
                    {
                        return ChannelBindingUtility.GetToken(this.listenerHttpContext.listenerContext.Request.TransportContext);
                    }
                }

                public override long ContentLength
                {
                    get
                    {
                        return this.listenerHttpContext.listenerContext.Request.ContentLength64;
                    }
                }

                protected override string ContentTypeCore
                {
                    get
                    {
                        if (this.cachedContentType == null)
                        {
                            this.cachedContentType = this.listenerHttpContext.listenerContext.Request.ContentType;
                        }
                        return this.cachedContentType;
                    }
                }

                protected override bool HasContent
                {
                    get
                    {
                        if (this.preReadBuffer == null)
                        {
                            return (this.ContentLength > 0L);
                        }
                        return true;
                    }
                }

                protected override string SoapActionHeader
                {
                    get
                    {
                        return this.listenerHttpContext.listenerContext.Request.Headers["SOAPAction"];
                    }
                }

                private class ListenerContextInputStream : HttpDelayedAcceptStream
                {
                    public ListenerContextInputStream(HttpRequestContext.ListenerHttpContext listenerHttpContext) : base(listenerHttpContext.listenerContext.Request.InputStream)
                    {
                    }

                    public ListenerContextInputStream(HttpRequestContext.ListenerHttpContext listenerHttpContext, byte[] preReadBuffer) : base(new PreReadStream(listenerHttpContext.listenerContext.Request.InputStream, preReadBuffer))
                    {
                    }

                    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                    {
                        IAsyncResult result;
                        try
                        {
                            result = base.BeginRead(buffer, offset, count, callback, state);
                        }
                        catch (HttpListenerException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                        }
                        return result;
                    }

                    public override int EndRead(IAsyncResult result)
                    {
                        int num;
                        try
                        {
                            num = base.EndRead(result);
                        }
                        catch (HttpListenerException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                        }
                        return num;
                    }

                    public override int Read(byte[] buffer, int offset, int count)
                    {
                        int num;
                        try
                        {
                            num = base.Read(buffer, offset, count);
                        }
                        catch (HttpListenerException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                        }
                        return num;
                    }

                    public override int ReadByte()
                    {
                        int num;
                        try
                        {
                            num = base.ReadByte();
                        }
                        catch (HttpListenerException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                        }
                        return num;
                    }
                }
            }
        }

        private class ReplyAsyncResult : AsyncResult
        {
            private bool closeOutputAfterReply;
            private HttpRequestContext context;
            private Message message;
            private static AsyncCallback onSendCompleted;
            private Message responseMessage;
            private TimeoutHelper timeoutHelper;

            public ReplyAsyncResult(HttpRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                this.message = message;
                this.responseMessage = null;
                this.timeoutHelper = new TimeoutHelper(timeout);
                ThreadTrace.Trace("Begin sending http reply");
                this.responseMessage = this.message;
                if (this.SendResponse())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<HttpRequestContext.ReplyAsyncResult>(result);
            }

            private void OnSendResponseCompleted(IAsyncResult result)
            {
                try
                {
                    this.context.httpOutput.EndSend(result);
                    ThreadTrace.Trace("End sending http reply");
                    if (this.closeOutputAfterReply)
                    {
                        this.context.httpOutput.Close();
                    }
                }
                finally
                {
                    if ((this.message != null) && !object.ReferenceEquals(this.message, this.responseMessage))
                    {
                        this.responseMessage.Close();
                    }
                }
            }

            private static void OnSendResponseCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    HttpRequestContext.ReplyAsyncResult asyncState = (HttpRequestContext.ReplyAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnSendResponseCompleted(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }

            public bool SendResponse()
            {
                if (onSendCompleted == null)
                {
                    onSendCompleted = Fx.ThunkCallback(new AsyncCallback(HttpRequestContext.ReplyAsyncResult.OnSendResponseCompletedCallback));
                }
                bool flag = false;
                try
                {
                    this.closeOutputAfterReply = this.context.PrepareReply(ref this.responseMessage);
                    IAsyncResult result = this.context.httpOutput.BeginSend(this.timeoutHelper.RemainingTime(), onSendCompleted, this);
                    flag = true;
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.OnSendResponseCompleted(result);
                }
                finally
                {
                    if ((!flag && (this.message != null)) && !object.ReferenceEquals(this.message, this.responseMessage))
                    {
                        this.responseMessage.Close();
                    }
                }
                return true;
            }
        }
    }
}

