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
    using System.Text;
    using System.Xml;

    internal abstract class HttpInput
    {
        private BufferManager bufferManager;
        private const string defaultContentType = "application/octet-stream";
        private bool enableChannelBinding;
        private Stream inputStream;
        private bool isRequest;
        private MessageEncoder messageEncoder;
        private const string multipartRelatedMediaType = "multipart/related";
        private IHttpTransportFactorySettings settings;
        private const string startInfoHeaderParam = "start-info";
        private bool streamed;
        private System.Net.WebException webException;

        protected HttpInput(IHttpTransportFactorySettings settings, bool isRequest, bool enableChannelBinding)
        {
            this.settings = settings;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            this.webException = null;
            this.isRequest = isRequest;
            this.inputStream = null;
            this.enableChannelBinding = enableChannelBinding;
            if (isRequest)
            {
                this.streamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
            }
            else
            {
                this.streamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
            }
        }

        protected abstract void AddProperties(Message message);
        private void ApplyChannelBinding(Message message)
        {
            if (this.enableChannelBinding)
            {
                ChannelBindingUtility.TryAddToMessage(this.ChannelBinding, message, true);
            }
        }

        public IAsyncResult BeginParseIncomingMessage(AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            bool flag = true;
            try
            {
                IAsyncResult result = new ParseMessageAsyncResult(this, callback, state);
                flag = false;
                result2 = result;
            }
            finally
            {
                if (flag)
                {
                    this.Close();
                }
            }
            return result2;
        }

        protected virtual void Close()
        {
        }

        internal static HttpInput CreateHttpInput(HttpWebResponse httpWebResponse, IHttpTransportFactorySettings settings, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding)
        {
            return new WebResponseHttpInput(httpWebResponse, settings, channelBinding);
        }

        private Message DecodeBufferedMessage(ArraySegment<byte> buffer, Stream inputStream)
        {
            Message message;
            try
            {
                if ((this.ContentLength == -1L) && (buffer.Count == this.settings.MaxReceivedMessageSize))
                {
                    byte[] buffer2 = new byte[1];
                    if (inputStream.Read(buffer2, 0, 1) > 0)
                    {
                        this.ThrowMaxReceivedMessageSizeExceeded();
                    }
                }
                try
                {
                    message = this.messageEncoder.ReadMessage(buffer, this.bufferManager, this.ContentType);
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
                }
            }
            finally
            {
                inputStream.Close();
            }
            return message;
        }

        public Message EndParseIncomingMessage(IAsyncResult result, out Exception requestException)
        {
            Message message2;
            bool flag = true;
            try
            {
                Message message = ParseMessageAsyncResult.End(result, out requestException);
                flag = false;
                message2 = message;
            }
            finally
            {
                if (flag)
                {
                    this.Close();
                }
            }
            return message2;
        }

        protected abstract Stream GetInputStream();
        private ArraySegment<byte> GetMessageBuffer()
        {
            long contentLength = this.ContentLength;
            if (contentLength > this.settings.MaxReceivedMessageSize)
            {
                this.ThrowMaxReceivedMessageSizeExceeded();
            }
            int count = (int) contentLength;
            return new ArraySegment<byte>(this.bufferManager.TakeBuffer(count), 0, count);
        }

        public Message ParseIncomingMessage(out Exception requestException)
        {
            Message message = null;
            Message message2;
            requestException = null;
            bool flag = true;
            try
            {
                this.ValidateContentType();
                ServiceModelActivity activity = null;
                if (DiagnosticUtility.ShouldUseActivity && ((ServiceModelActivity.Current == null) || (ServiceModelActivity.Current.ActivityType != ActivityType.ProcessAction)))
                {
                    activity = ServiceModelActivity.CreateBoundedActivity(true);
                }
                using (activity)
                {
                    if (DiagnosticUtility.ShouldUseActivity && (activity != null))
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
                    }
                    if (!this.HasContent)
                    {
                        if (this.messageEncoder.MessageVersion != MessageVersion.None)
                        {
                            return null;
                        }
                        message = new NullMessage();
                    }
                    else if (this.streamed)
                    {
                        message = this.ReadStreamedMessage(this.InputStream);
                    }
                    else if (this.ContentLength == -1L)
                    {
                        message = this.ReadChunkedBufferedMessage(this.InputStream);
                    }
                    else
                    {
                        message = this.ReadBufferedMessage(this.InputStream);
                    }
                    requestException = this.ProcessHttpAddressing(message);
                    flag = false;
                    message2 = message;
                }
            }
            finally
            {
                if (flag)
                {
                    this.Close();
                }
            }
            return message2;
        }

        private Exception ProcessHttpAddressing(Message message)
        {
            Exception exception = null;
            this.AddProperties(message);
            if (message.Version.Addressing == AddressingVersion.None)
            {
                bool flag = false;
                try
                {
                    flag = message.Headers.Action == null;
                }
                catch (XmlException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                if (!flag)
                {
                    exception = new ProtocolException(System.ServiceModel.SR.GetString("HttpAddressingNoneHeaderOnWire", new object[] { XD.AddressingDictionary.Action.Value }));
                }
                bool flag2 = false;
                try
                {
                    flag2 = message.Headers.To == null;
                }
                catch (XmlException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception5)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                    }
                }
                if (!flag2)
                {
                    exception = new ProtocolException(System.ServiceModel.SR.GetString("HttpAddressingNoneHeaderOnWire", new object[] { XD.AddressingDictionary.To.Value }));
                }
                message.Headers.To = message.Properties.Via;
            }
            if (this.isRequest)
            {
                string soapActionHeader = null;
                if (message.Version.Envelope == EnvelopeVersion.Soap11)
                {
                    soapActionHeader = this.SoapActionHeader;
                }
                else if ((message.Version.Envelope == EnvelopeVersion.Soap12) && !string.IsNullOrEmpty(this.ContentType))
                {
                    System.Net.Mime.ContentType type = new System.Net.Mime.ContentType(this.ContentType);
                    if ((type.MediaType == "multipart/related") && type.Parameters.ContainsKey("start-info"))
                    {
                        soapActionHeader = new System.Net.Mime.ContentType(type.Parameters["start-info"]).Parameters["action"];
                    }
                    if (soapActionHeader == null)
                    {
                        soapActionHeader = type.Parameters["action"];
                    }
                }
                if (soapActionHeader != null)
                {
                    soapActionHeader = UrlUtility.UrlDecode(soapActionHeader, Encoding.UTF8);
                    if (((soapActionHeader.Length >= 2) && (soapActionHeader[0] == '"')) && (soapActionHeader[soapActionHeader.Length - 1] == '"'))
                    {
                        soapActionHeader = soapActionHeader.Substring(1, soapActionHeader.Length - 2);
                    }
                    if (message.Version.Addressing == AddressingVersion.None)
                    {
                        message.Headers.Action = soapActionHeader;
                    }
                    try
                    {
                        if ((soapActionHeader.Length > 0) && (string.Compare(message.Headers.Action, soapActionHeader, StringComparison.Ordinal) != 0))
                        {
                            exception = new ActionMismatchAddressingException(System.ServiceModel.SR.GetString("HttpSoapActionMismatchFault", new object[] { message.Headers.Action, soapActionHeader }), message.Headers.Action, soapActionHeader);
                        }
                    }
                    catch (XmlException exception6)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception6, TraceEventType.Information);
                        }
                    }
                    catch (CommunicationException exception7)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception7, TraceEventType.Information);
                        }
                    }
                }
            }
            this.ApplyChannelBinding(message);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.TransferFromTransport(message);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null, message);
            }
            if (MessageLogger.LoggingEnabled && (message.Version.Addressing == AddressingVersion.None))
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.LastChance | MessageLoggingSource.TransportReceive);
            }
            return exception;
        }

        private Message ReadBufferedMessage(Stream inputStream)
        {
            ArraySegment<byte> messageBuffer = this.GetMessageBuffer();
            byte[] array = messageBuffer.Array;
            int offset = 0;
            int count = messageBuffer.Count;
            while (count > 0)
            {
                int num3 = inputStream.Read(array, offset, count);
                if (num3 == 0)
                {
                    if (this.ContentLength != -1L)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("HttpContentLengthIncorrect")));
                    }
                    break;
                }
                count -= num3;
                offset += num3;
            }
            return this.DecodeBufferedMessage(new ArraySegment<byte>(array, 0, offset), inputStream);
        }

        private Message ReadChunkedBufferedMessage(Stream inputStream)
        {
            Message message;
            try
            {
                message = this.messageEncoder.ReadMessage(inputStream, this.bufferManager, this.settings.MaxBufferSize, this.ContentType);
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
            }
            return message;
        }

        private Message ReadStreamedMessage(Stream inputStream)
        {
            Message message;
            MaxMessageSizeStream innerStream = new MaxMessageSizeStream(inputStream, this.settings.MaxReceivedMessageSize);
            Stream stream = new DrainOnCloseStream(innerStream);
            try
            {
                message = this.messageEncoder.ReadMessage(stream, this.settings.MaxBufferSize, this.ContentType);
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("MessageXmlProtocolError"), exception));
            }
            return message;
        }

        private void ThrowHttpProtocolException(string message, HttpStatusCode statusCode)
        {
            this.ThrowHttpProtocolException(message, statusCode, null);
        }

        private void ThrowHttpProtocolException(string message, HttpStatusCode statusCode, string statusDescription)
        {
            ProtocolException exception = new ProtocolException(message, this.webException);
            exception.Data.Add("System.ServiceModel.Channels.HttpInput.HttpStatusCode", statusCode);
            if ((statusDescription != null) && (statusDescription.Length > 0))
            {
                exception.Data.Add("System.ServiceModel.Channels.HttpInput.HttpStatusDescription", statusDescription);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
        }

        private void ThrowMaxReceivedMessageSizeExceeded()
        {
            if (this.isRequest)
            {
                this.ThrowHttpProtocolException(System.ServiceModel.SR.GetString("MaxReceivedMessageSizeExceeded", new object[] { this.settings.MaxReceivedMessageSize }), HttpStatusCode.BadRequest);
            }
            else
            {
                string message = System.ServiceModel.SR.GetString("MaxReceivedMessageSizeExceeded", new object[] { this.settings.MaxReceivedMessageSize });
                Exception innerException = new QuotaExceededException(message);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(message, innerException));
            }
        }

        private void ValidateContentType()
        {
            if (this.HasContent)
            {
                if (string.IsNullOrEmpty(this.ContentType))
                {
                    if (MessageLogger.ShouldLogMalformed)
                    {
                        MessageLogger.LogMessage(this.InputStream, MessageLoggingSource.Malformed);
                    }
                    this.ThrowHttpProtocolException(System.ServiceModel.SR.GetString("HttpContentTypeHeaderRequired"), HttpStatusCode.UnsupportedMediaType, "Missing Content Type");
                }
                if (!this.messageEncoder.IsContentTypeSupported(this.ContentType))
                {
                    if (MessageLogger.ShouldLogMalformed)
                    {
                        MessageLogger.LogMessage(this.InputStream, MessageLoggingSource.Malformed);
                    }
                    string statusDescription = string.Format(CultureInfo.InvariantCulture, "Cannot process the message because the content type '{0}' was not the expected type '{1}'.", new object[] { this.ContentType, this.messageEncoder.ContentType });
                    this.ThrowHttpProtocolException(System.ServiceModel.SR.GetString("ContentTypeMismatch", new object[] { this.ContentType, this.messageEncoder.ContentType }), HttpStatusCode.UnsupportedMediaType, statusDescription);
                }
            }
        }

        protected virtual System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            get
            {
                return null;
            }
        }

        public abstract long ContentLength { get; }

        protected string ContentType
        {
            get
            {
                string contentTypeCore = this.ContentTypeCore;
                if (string.IsNullOrEmpty(contentTypeCore))
                {
                    return "application/octet-stream";
                }
                return contentTypeCore;
            }
        }

        protected abstract string ContentTypeCore { get; }

        protected abstract bool HasContent { get; }

        internal Stream InputStream
        {
            get
            {
                if (this.inputStream == null)
                {
                    this.inputStream = this.GetInputStream();
                }
                return this.inputStream;
            }
        }

        protected abstract string SoapActionHeader { get; }

        internal System.Net.WebException WebException
        {
            get
            {
                return this.webException;
            }
            set
            {
                this.webException = value;
            }
        }

        private class ParseMessageAsyncResult : TraceAsyncResult
        {
            private ArraySegment<byte> buffer;
            private int count;
            private HttpInput httpInput;
            private Stream inputStream;
            private Message message;
            private int offset;
            private static AsyncCallback onRead = Fx.ThunkCallback(new AsyncCallback(HttpInput.ParseMessageAsyncResult.OnRead));
            private Exception requestException;

            public ParseMessageAsyncResult(HttpInput httpInput, AsyncCallback callback, object state) : base(callback, state)
            {
                this.httpInput = httpInput;
                httpInput.ValidateContentType();
                this.inputStream = httpInput.InputStream;
                if (!httpInput.HasContent)
                {
                    if (httpInput.messageEncoder.MessageVersion != MessageVersion.None)
                    {
                        base.Complete(true);
                        return;
                    }
                    this.message = new NullMessage();
                }
                else if (httpInput.streamed || (httpInput.ContentLength == -1L))
                {
                    if (httpInput.streamed)
                    {
                        this.message = httpInput.ReadStreamedMessage(this.inputStream);
                    }
                    else
                    {
                        this.message = httpInput.ReadChunkedBufferedMessage(this.inputStream);
                    }
                }
                if (this.message != null)
                {
                    this.requestException = httpInput.ProcessHttpAddressing(this.message);
                    base.Complete(true);
                }
                else
                {
                    this.buffer = httpInput.GetMessageBuffer();
                    this.count = this.buffer.Count;
                    this.offset = 0;
                    IAsyncResult asyncResult = this.inputStream.BeginRead(this.buffer.Array, this.offset, this.count, onRead, this);
                    if (asyncResult.CompletedSynchronously && this.ContinueReading(this.inputStream.EndRead(asyncResult)))
                    {
                        base.Complete(true);
                    }
                }
            }

            private bool ContinueReading(int bytesRead)
            {
                while (true)
                {
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    this.offset += bytesRead;
                    this.count -= bytesRead;
                    if (this.count <= 0)
                    {
                        break;
                    }
                    IAsyncResult asyncResult = this.inputStream.BeginRead(this.buffer.Array, this.offset, this.count, onRead, this);
                    if (!asyncResult.CompletedSynchronously)
                    {
                        return false;
                    }
                    bytesRead = this.inputStream.EndRead(asyncResult);
                }
                using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
                    }
                    this.message = this.httpInput.DecodeBufferedMessage(new ArraySegment<byte>(this.buffer.Array, 0, this.offset), this.inputStream);
                    this.requestException = this.httpInput.ProcessHttpAddressing(this.message);
                }
                return true;
            }

            public static Message End(IAsyncResult result, out Exception requestException)
            {
                HttpInput.ParseMessageAsyncResult result2 = AsyncResult.End<HttpInput.ParseMessageAsyncResult>(result);
                requestException = result2.requestException;
                return result2.message;
            }

            private static void OnRead(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    HttpInput.ParseMessageAsyncResult asyncState = (HttpInput.ParseMessageAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.ContinueReading(asyncState.inputStream.EndRead(result));
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
        }

        private class WebResponseHttpInput : HttpInput
        {
            private System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding;
            private HttpWebResponse httpWebResponse;
            private byte[] preReadBuffer;

            public WebResponseHttpInput(HttpWebResponse httpWebResponse, IHttpTransportFactorySettings settings, System.Security.Authentication.ExtendedProtection.ChannelBinding channelBinding) : base(settings, false, channelBinding != null)
            {
                this.channelBinding = channelBinding;
                this.httpWebResponse = httpWebResponse;
                if (this.httpWebResponse.ContentLength == -1L)
                {
                    this.preReadBuffer = new byte[1];
                    if (this.httpWebResponse.GetResponseStream().Read(this.preReadBuffer, 0, 1) == 0)
                    {
                        this.preReadBuffer = null;
                    }
                }
            }

            protected override void AddProperties(Message message)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty(this.httpWebResponse.Headers) {
                    StatusCode = this.httpWebResponse.StatusCode,
                    StatusDescription = this.httpWebResponse.StatusDescription
                };
                message.Properties.Add(HttpResponseMessageProperty.Name, property);
                message.Properties.Via = message.Version.Addressing.AnonymousUri;
            }

            protected override void Close()
            {
                try
                {
                    this.httpWebResponse.Close();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                }
            }

            protected override Stream GetInputStream()
            {
                if (this.preReadBuffer != null)
                {
                    return new WebResponseInputStream(this.httpWebResponse, this.preReadBuffer);
                }
                return new WebResponseInputStream(this.httpWebResponse);
            }

            protected override System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
            {
                get
                {
                    return this.channelBinding;
                }
            }

            public override long ContentLength
            {
                get
                {
                    return this.httpWebResponse.ContentLength;
                }
            }

            protected override string ContentTypeCore
            {
                get
                {
                    return this.httpWebResponse.ContentType;
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
                    return this.httpWebResponse.Headers["SOAPAction"];
                }
            }

            private class WebResponseInputStream : DetectEofStream
            {
                private const int maxSocketRead = 0x10000;
                private bool responseClosed;
                private HttpWebResponse webResponse;

                public WebResponseInputStream(HttpWebResponse httpWebResponse) : base(httpWebResponse.GetResponseStream())
                {
                    this.webResponse = httpWebResponse;
                }

                public WebResponseInputStream(HttpWebResponse httpWebResponse, byte[] prereadBuffer) : base(new PreReadStream(httpWebResponse.GetResponseStream(), prereadBuffer))
                {
                    this.webResponse = httpWebResponse;
                }

                public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    IAsyncResult result;
                    try
                    {
                        result = base.BaseStream.BeginRead(buffer, offset, Math.Min(count, 0x10000), callback, state);
                    }
                    catch (IOException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(exception, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception2.Message, exception2));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(exception3, this.webResponse));
                    }
                    return result;
                }

                public override void Close()
                {
                    base.Close();
                    this.CloseResponse();
                }

                private void CloseResponse()
                {
                    if (!this.responseClosed)
                    {
                        this.responseClosed = true;
                        this.webResponse.Close();
                    }
                }

                public override int EndRead(IAsyncResult result)
                {
                    int num;
                    try
                    {
                        num = base.BaseStream.EndRead(result);
                    }
                    catch (IOException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(exception, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (ObjectDisposedException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception2.Message, exception2));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(exception3, this.webResponse));
                    }
                    return num;
                }

                protected override void OnReceivedEof()
                {
                    base.OnReceivedEof();
                    this.CloseResponse();
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    int num;
                    try
                    {
                        num = base.BaseStream.Read(buffer, offset, Math.Min(count, 0x10000));
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception.Message, exception));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(exception2, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(exception3, this.webResponse));
                    }
                    return num;
                }

                public override int ReadByte()
                {
                    int num;
                    try
                    {
                        num = base.BaseStream.ReadByte();
                    }
                    catch (ObjectDisposedException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(exception.Message, exception));
                    }
                    catch (IOException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(exception2, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (WebException exception3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(exception3, this.webResponse));
                    }
                    return num;
                }
            }
        }
    }
}

