namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal abstract class HttpOutput
    {
        private HttpAbortReason abortReason;
        private BufferManager bufferManager;
        private byte[] bufferToRecycle;
        private bool isDisposed;
        private bool isRequest;
        private Message message;
        private MessageEncoder messageEncoder;
        private string mtomBoundary;
        private static Action<object> onStreamSendTimeout;
        private Stream outputStream;
        private IHttpTransportFactorySettings settings;
        private bool streamed;
        private bool supportsConcurrentIO;

        protected HttpOutput(IHttpTransportFactorySettings settings, Message message, bool isRequest, bool supportsConcurrentIO)
        {
            this.settings = settings;
            this.message = message;
            this.isRequest = isRequest;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            if (isRequest)
            {
                this.streamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
            }
            else
            {
                this.streamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
            }
            this.supportsConcurrentIO = supportsConcurrentIO;
        }

        protected void Abort()
        {
            this.Abort(HttpAbortReason.Aborted);
        }

        public virtual void Abort(HttpAbortReason reason)
        {
            if (!this.isDisposed)
            {
                this.abortReason = reason;
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, this.isRequest ? 0x4000d : 0x4000e, this.isRequest ? System.ServiceModel.SR.GetString("TraceCodeHttpChannelRequestAborted") : System.ServiceModel.SR.GetString("TraceCodeHttpChannelResponseAborted"), this.message);
                }
                this.CleanupBuffer();
            }
        }

        protected abstract void AddMimeVersion(string version);
        private void ApplyChannelBinding()
        {
            if (this.IsChannelBindingSupportEnabled)
            {
                ChannelBindingUtility.TryAddToMessage(this.ChannelBinding, this.message, this.CleanupChannelBinding);
            }
        }

        protected virtual IAsyncResult BeginGetOutputStream(AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual IAsyncResult BeginSend(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            bool flag = true;
            try
            {
                bool suppressEntityBody = this.PrepareHttpSend(this.message);
                IAsyncResult result = new SendAsyncResult(this, suppressEntityBody, timeout, callback, state);
                flag = false;
                result2 = result;
            }
            finally
            {
                if (flag)
                {
                    this.Abort();
                }
            }
            return result2;
        }

        private void CleanupBuffer()
        {
            if (this.bufferToRecycle != null)
            {
                this.bufferManager.ReturnBuffer(this.bufferToRecycle);
                this.bufferToRecycle = null;
            }
            this.isDisposed = true;
        }

        public void Close()
        {
            if (!this.isDisposed)
            {
                if (this.outputStream != null)
                {
                    this.outputStream.Close();
                }
                this.CleanupBuffer();
            }
        }

        internal static HttpOutput CreateHttpOutput(HttpListenerResponse httpListenerResponse, IHttpTransportFactorySettings settings, Message message, string httpMethod)
        {
            return new ListenerResponseHttpOutput(httpListenerResponse, settings, message, httpMethod);
        }

        internal static HttpOutput CreateHttpOutput(HttpWebRequest httpWebRequest, IHttpTransportFactorySettings settings, Message message, bool enableChannelBindingSupport)
        {
            return new WebRequestHttpOutput(httpWebRequest, settings, message, enableChannelBindingSupport);
        }

        protected virtual Stream EndGetOutputStream(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void EndSend(IAsyncResult result)
        {
            bool flag = true;
            try
            {
                SendAsyncResult.End(result);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.Abort();
                }
            }
        }

        protected abstract Stream GetOutputStream();
        private void LogMessage()
        {
            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref this.message, MessageLoggingSource.TransportSend);
            }
        }

        private static void OnStreamSendTimeout(object state)
        {
            ((HttpOutput) state).Abort(HttpAbortReason.TimedOut);
        }

        protected virtual bool PrepareHttpSend(Message message)
        {
            string action = message.Headers.Action;
            if (message.Version.Addressing == AddressingVersion.None)
            {
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    message.Properties.Add(AddressingProperty.Name, new AddressingProperty(message.Headers));
                }
                message.Headers.Action = null;
                message.Headers.To = null;
            }
            string contentType = null;
            if (message.Version == MessageVersion.None)
            {
                object obj2 = null;
                if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2))
                {
                    HttpResponseMessageProperty property = (HttpResponseMessageProperty) obj2;
                    if (!string.IsNullOrEmpty(property.Headers[HttpResponseHeader.ContentType]))
                    {
                        contentType = property.Headers[HttpResponseHeader.ContentType];
                        if (!this.messageEncoder.IsContentTypeSupported(contentType))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("ResponseContentTypeNotSupported", new object[] { contentType })));
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(contentType))
            {
                MtomMessageEncoder messageEncoder = this.messageEncoder as MtomMessageEncoder;
                if (messageEncoder == null)
                {
                    contentType = this.messageEncoder.ContentType;
                }
                else
                {
                    contentType = messageEncoder.GetContentType(out this.mtomBoundary);
                    this.AddMimeVersion("1.0");
                }
            }
            this.SetContentType(contentType);
            return (message is NullMessage);
        }

        public virtual void Send(TimeSpan timeout)
        {
            if (this.PrepareHttpSend(this.message))
            {
                if (!this.isRequest)
                {
                    this.outputStream = this.GetOutputStream();
                }
                else
                {
                    this.SetContentLength(0);
                    this.LogMessage();
                }
            }
            else if (this.streamed)
            {
                this.outputStream = this.GetOutputStream();
                this.ApplyChannelBinding();
                this.WriteStreamedMessage(timeout);
            }
            else if (this.IsChannelBindingSupportEnabled)
            {
                this.outputStream = this.GetOutputStream();
                this.ApplyChannelBinding();
                ArraySegment<byte> segment = this.SerializeBufferedMessage(this.message);
                this.outputStream.Write(segment.Array, segment.Offset, segment.Count);
            }
            else
            {
                ArraySegment<byte> segment2 = this.SerializeBufferedMessage(this.message);
                this.SetContentLength(segment2.Count);
                if (!this.isRequest || (segment2.Count > 0))
                {
                    this.outputStream = this.GetOutputStream();
                    this.outputStream.Write(segment2.Array, segment2.Offset, segment2.Count);
                }
            }
            this.TraceSend();
        }

        private ArraySegment<byte> SerializeBufferedMessage(Message message)
        {
            ArraySegment<byte> segment;
            MtomMessageEncoder messageEncoder = this.messageEncoder as MtomMessageEncoder;
            if (messageEncoder == null)
            {
                segment = this.messageEncoder.WriteMessage(message, 0x7fffffff, this.bufferManager);
            }
            else
            {
                segment = messageEncoder.WriteMessage(message, 0x7fffffff, this.bufferManager, 0, this.mtomBoundary);
            }
            this.bufferToRecycle = segment.Array;
            return segment;
        }

        protected virtual void SetContentLength(int contentLength)
        {
        }

        protected abstract void SetContentType(string contentType);
        public virtual System.Security.Authentication.ExtendedProtection.ChannelBinding TakeChannelBinding()
        {
            return null;
        }

        private void TraceSend()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40014, System.ServiceModel.SR.GetString("TraceCodeMessageSent"), new MessageTraceRecord(this.message), this, null);
            }
        }

        private void WriteStreamedMessage(TimeSpan timeout)
        {
            this.outputStream = this.supportsConcurrentIO ? ((Stream) new BufferedOutputAsyncStream(this.outputStream, 0x4000, 4)) : ((Stream) new BufferedStream(this.outputStream, 0x8000));
            if (onStreamSendTimeout == null)
            {
                onStreamSendTimeout = new Action<object>(HttpOutput.OnStreamSendTimeout);
            }
            IOThreadTimer timer = new IOThreadTimer(onStreamSendTimeout, this, true);
            timer.Set(timeout);
            try
            {
                MtomMessageEncoder messageEncoder = this.messageEncoder as MtomMessageEncoder;
                if (messageEncoder == null)
                {
                    this.messageEncoder.WriteMessage(this.message, this.outputStream);
                }
                else
                {
                    messageEncoder.WriteMessage(this.message, this.outputStream, this.mtomBoundary);
                }
            }
            finally
            {
                timer.Cancel();
            }
        }

        protected virtual System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return null;
            }
        }

        protected virtual bool CleanupChannelBinding
        {
            get
            {
                return true;
            }
        }

        protected virtual bool IsChannelBindingSupportEnabled
        {
            get
            {
                return false;
            }
        }

        protected virtual bool WillGetOutputStreamCompleteSynchronously
        {
            get
            {
                return true;
            }
        }

        private class ListenerResponseHttpOutput : HttpOutput
        {
            private string httpMethod;
            private HttpListenerResponse listenerResponse;

            public ListenerResponseHttpOutput(HttpListenerResponse listenerResponse, IHttpTransportFactorySettings settings, Message message, string httpMethod) : base(settings, message, false, true)
            {
                this.listenerResponse = listenerResponse;
                this.httpMethod = httpMethod;
                if (message.IsFault)
                {
                    this.listenerResponse.StatusCode = 500;
                }
                else
                {
                    this.listenerResponse.StatusCode = 200;
                }
            }

            public override void Abort(HttpAbortReason abortReason)
            {
                this.listenerResponse.Abort();
                base.Abort(abortReason);
            }

            protected override void AddMimeVersion(string version)
            {
                this.listenerResponse.Headers["MIME-Version"] = version;
            }

            protected override Stream GetOutputStream()
            {
                return new ListenerResponseOutputStream(this.listenerResponse);
            }

            protected override bool PrepareHttpSend(Message message)
            {
                object obj2;
                bool flag = base.PrepareHttpSend(message);
                bool flag2 = message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out obj2);
                HttpResponseMessageProperty property = (HttpResponseMessageProperty) obj2;
                bool flag3 = string.Compare(this.httpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) == 0;
                if (flag3 || (flag2 && property.SuppressEntityBody))
                {
                    flag = true;
                    this.SetContentLength(0);
                    this.SetContentType(null);
                    this.listenerResponse.SendChunked = false;
                }
                if (flag2)
                {
                    this.listenerResponse.StatusCode = (int) property.StatusCode;
                    if (property.StatusDescription != null)
                    {
                        this.listenerResponse.StatusDescription = property.StatusDescription;
                    }
                    WebHeaderCollection headers = property.Headers;
                    for (int i = 0; i < headers.Count; i++)
                    {
                        string strA = headers.Keys[i];
                        string s = headers[i];
                        if (string.Compare(strA, "content-length", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            int result = -1;
                            if (flag3 && int.TryParse(s, out result))
                            {
                                this.SetContentLength(result);
                            }
                        }
                        else if (string.Compare(strA, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (flag3 || !property.SuppressEntityBody)
                            {
                                this.SetContentType(s);
                            }
                        }
                        else if (string.Compare(strA, "WWW-Authenticate", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.listenerResponse.AddHeader(strA, s);
                        }
                        else
                        {
                            this.listenerResponse.AppendHeader(strA, s);
                        }
                    }
                }
                return flag;
            }

            protected override void SetContentLength(int contentLength)
            {
                this.listenerResponse.ContentLength64 = contentLength;
            }

            protected override void SetContentType(string contentType)
            {
                this.listenerResponse.ContentType = contentType;
            }

            private class ListenerResponseOutputStream : BytesReadPositionStream
            {
                public ListenerResponseOutputStream(HttpListenerResponse listenerResponse) : base(listenerResponse.OutputStream)
                {
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    IAsyncResult result;
                    try
                    {
                        result = base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (HttpListenerException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                    }
                    catch (ApplicationException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpResponseAborted"), exception2));
                    }
                    return result;
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (HttpListenerException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (HttpListenerException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                    }
                    catch (ApplicationException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpResponseAborted"), exception2));
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (HttpListenerException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateCommunicationException(exception));
                    }
                    catch (ApplicationException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("HttpResponseAborted"), exception2));
                    }
                }
            }
        }

        private class SendAsyncResult : AsyncResult
        {
            private ArraySegment<byte> buffer;
            private HttpOutput httpOutput;
            private static AsyncCallback onGetOutputStream;
            private static AsyncCallback onWriteBody;
            private static Action<object> onWriteStreamedMessage;
            private bool suppressEntityBody;
            private TimeoutHelper timeoutHelper;

            public SendAsyncResult(HttpOutput httpOutput, bool suppressEntityBody, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.httpOutput = httpOutput;
                this.suppressEntityBody = suppressEntityBody;
                if (suppressEntityBody && httpOutput.isRequest)
                {
                    httpOutput.SetContentLength(0);
                    this.httpOutput.TraceSend();
                    this.httpOutput.LogMessage();
                    base.Complete(true);
                }
                else
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.Send();
                }
            }

            private void CompleteWriteBody(IAsyncResult result)
            {
                this.httpOutput.outputStream.EndWrite(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<HttpOutput.SendAsyncResult>(result);
            }

            private static void OnGetOutputStream(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    HttpOutput.SendAsyncResult asyncState = (HttpOutput.SendAsyncResult) result.AsyncState;
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        asyncState.httpOutput.outputStream = asyncState.httpOutput.EndGetOutputStream(result);
                        asyncState.httpOutput.ApplyChannelBinding();
                        if (!asyncState.httpOutput.streamed && asyncState.httpOutput.IsChannelBindingSupportEnabled)
                        {
                            asyncState.buffer = asyncState.httpOutput.SerializeBufferedMessage(asyncState.httpOutput.message);
                            asyncState.httpOutput.SetContentLength(asyncState.buffer.Count);
                        }
                        if (asyncState.WriteMessage(false))
                        {
                            asyncState.httpOutput.TraceSend();
                            flag = true;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private static void OnWriteBody(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    HttpOutput.SendAsyncResult asyncState = (HttpOutput.SendAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.CompleteWriteBody(result);
                        asyncState.httpOutput.TraceSend();
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

            private static void OnWriteStreamedMessage(object state)
            {
                HttpOutput.SendAsyncResult result = (HttpOutput.SendAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.WriteStreamedMessage();
                    result.httpOutput.TraceSend();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }

            private void Send()
            {
                if (this.httpOutput.IsChannelBindingSupportEnabled)
                {
                    this.SendWithChannelBindingToken();
                }
                else
                {
                    this.SendWithoutChannelBindingToken();
                }
            }

            private void SendWithChannelBindingToken()
            {
                if (this.httpOutput.WillGetOutputStreamCompleteSynchronously)
                {
                    this.httpOutput.outputStream = this.httpOutput.GetOutputStream();
                    this.httpOutput.ApplyChannelBinding();
                }
                else
                {
                    if (onGetOutputStream == null)
                    {
                        onGetOutputStream = Fx.ThunkCallback(new AsyncCallback(HttpOutput.SendAsyncResult.OnGetOutputStream));
                    }
                    IAsyncResult result = this.httpOutput.BeginGetOutputStream(onGetOutputStream, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.httpOutput.outputStream = this.httpOutput.EndGetOutputStream(result);
                    this.httpOutput.ApplyChannelBinding();
                }
                if (!this.httpOutput.streamed)
                {
                    this.buffer = this.httpOutput.SerializeBufferedMessage(this.httpOutput.message);
                    this.httpOutput.SetContentLength(this.buffer.Count);
                }
                if (this.WriteMessage(true))
                {
                    this.httpOutput.TraceSend();
                    base.Complete(true);
                }
            }

            private void SendWithoutChannelBindingToken()
            {
                if (!this.suppressEntityBody && !this.httpOutput.streamed)
                {
                    this.buffer = this.httpOutput.SerializeBufferedMessage(this.httpOutput.message);
                    this.httpOutput.SetContentLength(this.buffer.Count);
                }
                if (this.httpOutput.WillGetOutputStreamCompleteSynchronously)
                {
                    this.httpOutput.outputStream = this.httpOutput.GetOutputStream();
                }
                else
                {
                    if (onGetOutputStream == null)
                    {
                        onGetOutputStream = Fx.ThunkCallback(new AsyncCallback(HttpOutput.SendAsyncResult.OnGetOutputStream));
                    }
                    IAsyncResult result = this.httpOutput.BeginGetOutputStream(onGetOutputStream, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.httpOutput.outputStream = this.httpOutput.EndGetOutputStream(result);
                }
                if (this.WriteMessage(true))
                {
                    this.httpOutput.TraceSend();
                    base.Complete(true);
                }
            }

            private bool WriteMessage(bool isStillSynchronous)
            {
                if (!this.suppressEntityBody)
                {
                    if (this.httpOutput.streamed)
                    {
                        if (isStillSynchronous)
                        {
                            if (onWriteStreamedMessage == null)
                            {
                                onWriteStreamedMessage = new Action<object>(HttpOutput.SendAsyncResult.OnWriteStreamedMessage);
                            }
                            ActionItem.Schedule(onWriteStreamedMessage, this);
                            return false;
                        }
                        this.WriteStreamedMessage();
                    }
                    else
                    {
                        if (onWriteBody == null)
                        {
                            onWriteBody = Fx.ThunkCallback(new AsyncCallback(HttpOutput.SendAsyncResult.OnWriteBody));
                        }
                        IAsyncResult result = this.httpOutput.outputStream.BeginWrite(this.buffer.Array, this.buffer.Offset, this.buffer.Count, onWriteBody, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        this.CompleteWriteBody(result);
                    }
                }
                return true;
            }

            private void WriteStreamedMessage()
            {
                this.httpOutput.WriteStreamedMessage(this.timeoutHelper.RemainingTime());
            }
        }

        private class WebRequestHttpOutput : HttpOutput
        {
            private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken;
            private bool enableChannelBindingSupport;
            private HttpWebRequest httpWebRequest;

            public WebRequestHttpOutput(HttpWebRequest httpWebRequest, IHttpTransportFactorySettings settings, Message message, bool enableChannelBindingSupport) : base(settings, message, true, false)
            {
                this.httpWebRequest = httpWebRequest;
                this.enableChannelBindingSupport = enableChannelBindingSupport;
            }

            public override void Abort(HttpAbortReason abortReason)
            {
                this.httpWebRequest.Abort();
                base.Abort(abortReason);
            }

            protected override void AddMimeVersion(string version)
            {
                this.httpWebRequest.Headers["MIME-Version"] = version;
            }

            protected override IAsyncResult BeginGetOutputStream(AsyncCallback callback, object state)
            {
                return new GetOutputStreamAsyncResult(this.httpWebRequest, this, callback, state);
            }

            protected override Stream EndGetOutputStream(IAsyncResult result)
            {
                return GetOutputStreamAsyncResult.End(result, out this.channelBindingToken);
            }

            protected override Stream GetOutputStream()
            {
                Stream stream2;
                try
                {
                    Stream requestStream;
                    if (this.IsChannelBindingSupportEnabled)
                    {
                        TransportContext context;
                        requestStream = this.httpWebRequest.GetRequestStream(out context);
                        this.channelBindingToken = ChannelBindingUtility.GetToken(context);
                    }
                    else
                    {
                        requestStream = this.httpWebRequest.GetRequestStream();
                    }
                    requestStream = new WebRequestOutputStream(requestStream, this.httpWebRequest, this);
                    stream2 = requestStream;
                }
                catch (WebException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception, this.httpWebRequest, base.abortReason));
                }
                return stream2;
            }

            protected override bool PrepareHttpSend(Message message)
            {
                object obj2;
                bool flag = false;
                string action = message.Headers.Action;
                if (action != null)
                {
                    action = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { UrlUtility.UrlPathEncode(action) });
                }
                bool flag2 = base.PrepareHttpSend(message);
                if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out obj2))
                {
                    HttpRequestMessageProperty property = (HttpRequestMessageProperty) obj2;
                    this.httpWebRequest.Method = property.Method;
                    WebHeaderCollection headers = property.Headers;
                    flag2 = flag2 || property.SuppressEntityBody;
                    for (int i = 0; i < headers.Count; i++)
                    {
                        string strA = headers.Keys[i];
                        string str3 = headers[i];
                        if (string.Compare(strA, "accept", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.httpWebRequest.Accept = str3;
                        }
                        else if (string.Compare(strA, "connection", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (str3.IndexOf("keep-alive", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                this.httpWebRequest.KeepAlive = true;
                            }
                            else
                            {
                                this.httpWebRequest.Connection = str3;
                            }
                        }
                        else if (string.Compare(strA, "SOAPAction", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (action != null)
                            {
                                if ((str3.Length > 0) && (string.Compare(str3, action, StringComparison.Ordinal) != 0))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("HttpSoapActionMismatch", new object[] { action, str3 })));
                                }
                            }
                            else
                            {
                                action = str3;
                            }
                        }
                        else if (string.Compare(strA, "content-length", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (string.Compare(strA, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this.httpWebRequest.ContentType = str3;
                                flag = true;
                            }
                            else if (string.Compare(strA, "expect", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (str3.ToUpperInvariant().IndexOf("100-CONTINUE", StringComparison.OrdinalIgnoreCase) != -1)
                                {
                                    this.httpWebRequest.ServicePoint.Expect100Continue = true;
                                }
                                else
                                {
                                    this.httpWebRequest.Expect = str3;
                                }
                            }
                            else if (string.Compare(strA, "host", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                if (string.Compare(strA, "referer", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this.httpWebRequest.Referer = str3;
                                }
                                else if (string.Compare(strA, "transfer-encoding", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    if (str3.ToUpperInvariant().IndexOf("CHUNKED", StringComparison.OrdinalIgnoreCase) != -1)
                                    {
                                        this.httpWebRequest.SendChunked = true;
                                    }
                                    else
                                    {
                                        this.httpWebRequest.TransferEncoding = str3;
                                    }
                                }
                                else if (string.Compare(strA, "user-agent", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    this.httpWebRequest.UserAgent = str3;
                                }
                                else if (string.Compare(strA, "if-modified-since", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    DateTime time;
                                    if (!DateTime.TryParse(str3, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out time))
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("HttpIfModifiedSinceParseError", new object[] { str3 })));
                                    }
                                    this.httpWebRequest.IfModifiedSince = time;
                                }
                                else if (((string.Compare(strA, "date", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(strA, "proxy-connection", StringComparison.OrdinalIgnoreCase) != 0)) && (string.Compare(strA, "range", StringComparison.OrdinalIgnoreCase) != 0))
                                {
                                    this.httpWebRequest.Headers.Add(strA, str3);
                                }
                            }
                        }
                    }
                }
                if (action != null)
                {
                    if (message.Version.Envelope == EnvelopeVersion.Soap11)
                    {
                        this.httpWebRequest.Headers["SOAPAction"] = action;
                    }
                    else if (message.Version.Envelope == EnvelopeVersion.Soap12)
                    {
                        if (message.Version.Addressing == AddressingVersion.None)
                        {
                            bool flag3 = true;
                            if (flag && (this.httpWebRequest.ContentType.Contains("action") || (this.httpWebRequest.ContentType.ToUpperInvariant().IndexOf("ACTION", StringComparison.OrdinalIgnoreCase) != -1)))
                            {
                                try
                                {
                                    ContentType type = new ContentType(this.httpWebRequest.ContentType);
                                    if (type.Parameters.ContainsKey("action"))
                                    {
                                        string str4 = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { type.Parameters["action"] });
                                        if (string.Compare(str4, action, StringComparison.Ordinal) != 0)
                                        {
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("HttpSoapActionMismatchContentType", new object[] { action, str4 })));
                                        }
                                        flag3 = false;
                                    }
                                }
                                catch (FormatException exception)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("HttpContentTypeFormatException", new object[] { exception.Message, this.httpWebRequest.ContentType }), exception));
                                }
                            }
                            if (flag3)
                            {
                                this.httpWebRequest.ContentType = string.Format(CultureInfo.InvariantCulture, "{0}; action={1}", new object[] { this.httpWebRequest.ContentType, action });
                            }
                        }
                    }
                    else if (message.Version.Envelope != EnvelopeVersion.None)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EnvelopeVersionUnknown", new object[] { message.Version.Envelope.ToString() })));
                    }
                }
                if (flag2)
                {
                    this.httpWebRequest.SendChunked = false;
                    return flag2;
                }
                if (this.IsChannelBindingSupportEnabled)
                {
                    this.httpWebRequest.SendChunked = true;
                }
                return flag2;
            }

            protected override void SetContentLength(int contentLength)
            {
                if ((contentLength == 0) && !this.enableChannelBindingSupport)
                {
                    this.httpWebRequest.ContentLength = contentLength;
                }
            }

            protected override void SetContentType(string contentType)
            {
                this.httpWebRequest.ContentType = contentType;
            }

            public override System.Security.Authentication.ExtendedProtection.ChannelBinding TakeChannelBinding()
            {
                System.Security.Authentication.ExtendedProtection.ChannelBinding channelBindingToken = this.channelBindingToken;
                this.channelBindingToken = null;
                return channelBindingToken;
            }

            protected override System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
            {
                get
                {
                    return this.channelBindingToken;
                }
            }

            protected override bool CleanupChannelBinding
            {
                get
                {
                    return false;
                }
            }

            protected override bool IsChannelBindingSupportEnabled
            {
                get
                {
                    return this.enableChannelBindingSupport;
                }
            }

            protected override bool WillGetOutputStreamCompleteSynchronously
            {
                get
                {
                    return false;
                }
            }

            private class GetOutputStreamAsyncResult : AsyncResult
            {
                private ChannelBinding channelBindingToken;
                private HttpOutput httpOutput;
                private HttpWebRequest httpWebRequest;
                private static AsyncCallback onGetRequestStream = Fx.ThunkCallback(new AsyncCallback(HttpOutput.WebRequestHttpOutput.GetOutputStreamAsyncResult.OnGetRequestStream));
                private Stream outputStream;

                public GetOutputStreamAsyncResult(HttpWebRequest httpWebRequest, HttpOutput httpOutput, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.httpWebRequest = httpWebRequest;
                    this.httpOutput = httpOutput;
                    IAsyncResult result = null;
                    try
                    {
                        result = httpWebRequest.BeginGetRequestStream(onGetRequestStream, this);
                    }
                    catch (WebException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception, httpWebRequest, httpOutput.abortReason));
                    }
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteGetRequestStream(result);
                        base.Complete(true);
                    }
                }

                private void CompleteGetRequestStream(IAsyncResult result)
                {
                    try
                    {
                        TransportContext context;
                        this.outputStream = new HttpOutput.WebRequestHttpOutput.WebRequestOutputStream(this.httpWebRequest.EndGetRequestStream(result, out context), this.httpWebRequest, this.httpOutput);
                        this.channelBindingToken = ChannelBindingUtility.GetToken(context);
                    }
                    catch (WebException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                }

                public static Stream End(IAsyncResult result, out ChannelBinding channelBindingToken)
                {
                    HttpOutput.WebRequestHttpOutput.GetOutputStreamAsyncResult result2 = AsyncResult.End<HttpOutput.WebRequestHttpOutput.GetOutputStreamAsyncResult>(result);
                    channelBindingToken = result2.channelBindingToken;
                    return result2.outputStream;
                }

                private static void OnGetRequestStream(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        HttpOutput.WebRequestHttpOutput.GetOutputStreamAsyncResult asyncState = (HttpOutput.WebRequestHttpOutput.GetOutputStreamAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.CompleteGetRequestStream(result);
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
            }

            private class WebRequestOutputStream : BytesReadPositionStream
            {
                private int bytesSent;
                private HttpOutput httpOutput;
                private HttpWebRequest httpWebRequest;

                public WebRequestOutputStream(Stream requestStream, HttpWebRequest httpWebRequest, HttpOutput httpOutput) : base(requestStream)
                {
                    this.httpWebRequest = httpWebRequest;
                    this.httpOutput = httpOutput;
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    IAsyncResult result;
                    this.bytesSent += count;
                    try
                    {
                        result = base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(exception, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(exception2, this.httpWebRequest));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception3, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    return result;
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(exception, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(exception2, this.httpWebRequest));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception3, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(exception, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(exception2, this.httpWebRequest));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception3, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(exception, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(exception2, this.httpWebRequest));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(exception3, this.httpWebRequest, this.httpOutput.abortReason));
                    }
                    this.bytesSent += count;
                }

                public override long Position
                {
                    get
                    {
                        return (long) this.bytesSent;
                    }
                    set
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SeekNotSupported")));
                    }
                }
            }
        }
    }
}

