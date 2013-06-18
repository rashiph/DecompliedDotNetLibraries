namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Web;

    internal class HostedHttpContext : HttpRequestContext
    {
        private HostedRequestContainer requestContainer;
        private HostedHttpRequestAsyncResult result;

        public HostedHttpContext(HttpChannelListener listener, HostedHttpRequestAsyncResult result) : base(listener, null)
        {
            this.result = result;
        }

        private void CloseHostedRequestContainer()
        {
            if (this.requestContainer != null)
            {
                this.requestContainer.Close();
                this.requestContainer = null;
            }
        }

        protected override HttpInput GetHttpInput()
        {
            return new HostedHttpInput(this);
        }

        protected override HttpOutput GetHttpOutput(Message message)
        {
            if (((base.HttpInput.ContentLength == -1L) && !OSEnvironmentHelper.IsVistaOrGreater) || !base.KeepAliveEnabled)
            {
                this.result.SetConnectionClose();
            }
            return new HostedRequestHttpOutput(this.result, base.Listener, message, this);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.result.Abort();
        }

        protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CloseHostedRequestContainer();
            return base.OnBeginReply(message, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            this.result.OnReplySent();
        }

        protected override SecurityMessageProperty OnProcessAuthentication()
        {
            return base.Listener.ProcessAuthentication(this.result);
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            this.CloseHostedRequestContainer();
            base.OnReply(message, timeout);
        }

        private void SetRequestContainer(HostedRequestContainer requestContainer)
        {
            this.requestContainer = requestContainer;
        }

        protected override HttpStatusCode ValidateAuthentication()
        {
            return base.Listener.ValidateAuthentication(this.result);
        }

        public override string HttpMethod
        {
            get
            {
                return this.result.GetHttpMethod();
            }
        }

        private class HostedHttpInput : HttpInput
        {
            private int contentLength;
            private string contentType;
            private HostedHttpContext hostedHttpContext;
            private byte[] preReadBuffer;

            public HostedHttpInput(HostedHttpContext hostedHttpContext) : base(hostedHttpContext.Listener, true, hostedHttpContext.Listener.IsChannelBindingSupportEnabled)
            {
                this.hostedHttpContext = hostedHttpContext;
                if (hostedHttpContext.Listener.MessageEncoderFactory.Encoder.MessageVersion.Envelope == EnvelopeVersion.Soap11)
                {
                    this.contentType = hostedHttpContext.result.GetContentType();
                }
                else
                {
                    this.contentType = hostedHttpContext.result.GetContentTypeFast();
                }
                this.contentLength = hostedHttpContext.result.GetContentLength();
                if (this.contentLength == 0)
                {
                    this.preReadBuffer = hostedHttpContext.result.GetPrereadBuffer(ref this.contentLength);
                }
            }

            protected override void AddProperties(Message message)
            {
                HostedHttpContext.HostedRequestContainer httpHeaderProvider = new HostedHttpContext.HostedRequestContainer(this.hostedHttpContext.result);
                HttpRequestMessageProperty property = new HttpRequestMessageProperty(httpHeaderProvider) {
                    Method = this.hostedHttpContext.HttpMethod
                };
                if (this.hostedHttpContext.result.RequestUri.Query.Length > 1)
                {
                    property.QueryString = this.hostedHttpContext.result.RequestUri.Query.Substring(1);
                }
                message.Properties.Add(HttpRequestMessageProperty.Name, property);
                message.Properties.Add(HostingMessageProperty.Name, CreateMessagePropertyFromHostedResult(this.hostedHttpContext.result));
                message.Properties.Via = this.hostedHttpContext.result.RequestUri;
                RemoteEndpointMessageProperty property2 = new RemoteEndpointMessageProperty(httpHeaderProvider);
                message.Properties.Add(RemoteEndpointMessageProperty.Name, property2);
                this.hostedHttpContext.SetRequestContainer(httpHeaderProvider);
            }

            [SecuritySafeCritical]
            private static HostingMessageProperty CreateMessagePropertyFromHostedResult(HostedHttpRequestAsyncResult result)
            {
                return new HostingMessageProperty(result);
            }

            protected override Stream GetInputStream()
            {
                if (this.preReadBuffer != null)
                {
                    return new HostedInputStream(this.hostedHttpContext, this.preReadBuffer);
                }
                return new HostedInputStream(this.hostedHttpContext);
            }

            protected override System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
            {
                get
                {
                    return ChannelBindingUtility.DuplicateToken(this.hostedHttpContext.result.GetChannelBinding());
                }
            }

            public override long ContentLength
            {
                get
                {
                    return (long) this.contentLength;
                }
            }

            protected override string ContentTypeCore
            {
                get
                {
                    return this.contentType;
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
                    return this.hostedHttpContext.result.GetSoapAction();
                }
            }

            private class HostedInputStream : HttpDelayedAcceptStream
            {
                public HostedInputStream(HostedHttpContext hostedContext) : base(hostedContext.result.GetInputStream())
                {
                }

                public HostedInputStream(HostedHttpContext hostedContext, byte[] preReadBuffer) : base(new PreReadStream(hostedContext.result.GetInputStream(), preReadBuffer))
                {
                }

                public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    return base.BeginRead(buffer, offset, count, callback, state);
                }

                public override int EndRead(IAsyncResult result)
                {
                    return base.EndRead(result);
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    return base.Read(buffer, offset, count);
                }
            }
        }

        private class HostedRequestContainer : RemoteEndpointMessageProperty.IRemoteEndpointProvider, HttpRequestMessageProperty.IHttpHeaderProvider
        {
            private bool isClosed;
            private HostedHttpRequestAsyncResult result;
            private object thisLock;

            public HostedRequestContainer(HostedHttpRequestAsyncResult result)
            {
                this.result = result;
                this.thisLock = new object();
            }

            public void Close()
            {
                lock (this.ThisLock)
                {
                    this.isClosed = true;
                }
            }

            [SecuritySafeCritical]
            void HttpRequestMessageProperty.IHttpHeaderProvider.CopyHeaders(WebHeaderCollection headers)
            {
                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            headers.Add(this.result.Application.Request.Headers);
                        }
                    }
                }
            }

            [SecuritySafeCritical]
            string RemoteEndpointMessageProperty.IRemoteEndpointProvider.GetAddress()
            {
                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            return this.result.Application.Request.UserHostAddress;
                        }
                    }
                }
                return string.Empty;
            }

            [SecuritySafeCritical]
            int RemoteEndpointMessageProperty.IRemoteEndpointProvider.GetPort()
            {
                int result = 0;
                if (this.isClosed)
                {
                    return result;
                }
                lock (this.ThisLock)
                {
                    if (this.isClosed)
                    {
                        return result;
                    }
                    string str = this.result.Application.Request.ServerVariables["REMOTE_PORT"];
                    if (!string.IsNullOrEmpty(str) && int.TryParse(str, out result))
                    {
                        return result;
                    }
                    return 0;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }

        private class HostedRequestHttpOutput : HttpOutput
        {
            private string contentType;
            private HostedHttpContext context;
            private string mimeVersion;
            private HostedHttpRequestAsyncResult result;
            private int statusCode;

            public HostedRequestHttpOutput(HostedHttpRequestAsyncResult result, IHttpTransportFactorySettings settings, Message message, HostedHttpContext context) : base(settings, message, false, false)
            {
                this.result = result;
                this.context = context;
                if (TransferModeHelper.IsResponseStreamed(settings.TransferMode))
                {
                    result.SetTransferModeToStreaming();
                }
                if (message.IsFault)
                {
                    this.statusCode = 500;
                }
                else
                {
                    this.statusCode = 200;
                }
            }

            protected override void AddMimeVersion(string version)
            {
                this.mimeVersion = version;
            }

            protected override Stream GetOutputStream()
            {
                return new HostedResponseOutputStream(this.result, this.context);
            }

            protected override bool PrepareHttpSend(Message message)
            {
                object obj2;
                bool flag = base.PrepareHttpSend(message);
                bool flag2 = string.Compare(this.context.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) == 0;
                if (flag2)
                {
                    flag = true;
                }
                if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2))
                {
                    HttpResponseMessageProperty property = (HttpResponseMessageProperty) obj2;
                    if (property.SuppressPreamble)
                    {
                        if (!flag)
                        {
                            return property.SuppressEntityBody;
                        }
                        return true;
                    }
                    this.result.SetStatusCode((int) property.StatusCode);
                    if (property.StatusDescription != null)
                    {
                        this.result.SetStatusDescription(property.StatusDescription);
                    }
                    WebHeaderCollection headers = property.Headers;
                    for (int i = 0; i < headers.Count; i++)
                    {
                        string strA = headers.Keys[i];
                        string contentType = headers[i];
                        if (string.Compare(strA, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.SetContentType(contentType);
                        }
                        else if (string.Compare(strA, "MIME-Version", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.mimeVersion = contentType;
                        }
                        else if (string.Compare(strA, "content-length", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            int result = -1;
                            if (flag2 && int.TryParse(contentType, out result))
                            {
                                this.SetContentLength(result);
                            }
                        }
                        else
                        {
                            this.result.AppendHeader(strA, contentType);
                        }
                    }
                    if (property.SuppressEntityBody)
                    {
                        this.contentType = null;
                        flag = true;
                    }
                }
                else
                {
                    this.result.SetStatusCode(this.statusCode);
                }
                if ((this.contentType != null) && (this.contentType.Length != 0))
                {
                    this.result.SetContentType(this.contentType);
                }
                if (this.mimeVersion != null)
                {
                    this.result.AppendHeader("MIME-Version", this.mimeVersion);
                }
                return flag;
            }

            protected override void SetContentLength(int contentLength)
            {
                this.result.AppendHeader("content-length", contentLength.ToString(CultureInfo.InvariantCulture));
            }

            protected override void SetContentType(string contentType)
            {
                this.contentType = contentType;
            }

            private class HostedResponseOutputStream : BytesReadPositionStream
            {
                private HostedHttpContext context;
                private HostedHttpRequestAsyncResult result;

                public HostedResponseOutputStream(HostedHttpRequestAsyncResult result, HostedHttpContext context) : base(result.GetOutputStream())
                {
                    this.context = context;
                    this.result = result;
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    IAsyncResult result;
                    try
                    {
                        result = base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (Exception exception)
                    {
                        this.CheckWrapThrow(exception);
                        throw;
                    }
                    return result;
                }

                private void CheckWrapThrow(Exception e)
                {
                    if (!Fx.IsFatal(e))
                    {
                        if (e is HttpException)
                        {
                            if (this.context.Aborted)
                            {
                                throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activation.SR.RequestContextAborted, e));
                            }
                            throw FxTrace.Exception.AsError(new CommunicationException(e.Message, e));
                        }
                        if (this.context.Aborted)
                        {
                            if (DiagnosticUtility.ShouldTraceError)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Error, 0x4001e, System.ServiceModel.Activation.SR.TraceCodeRequestContextAbort, this, e);
                            }
                            throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activation.SR.RequestContextAborted));
                        }
                    }
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (Exception exception)
                    {
                        this.CheckWrapThrow(exception);
                        throw;
                    }
                    finally
                    {
                        this.result.OnReplySent();
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (Exception exception)
                    {
                        this.CheckWrapThrow(exception);
                        throw;
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (Exception exception)
                    {
                        this.CheckWrapThrow(exception);
                        throw;
                    }
                }
            }
        }
    }
}

