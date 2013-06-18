namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class StreamFormatter
    {
        private bool isRequest;
        private string operationName;
        private string partName;
        private string partNS;
        private const int returnValueIndex = -1;
        private int streamIndex;
        private string wrapperName;
        private string wrapperNS;

        private StreamFormatter(MessageDescription messageDescription, MessagePartDescription streamPart, string operationName, bool isRequest)
        {
            if (streamPart == messageDescription.Body.ReturnValue)
            {
                this.streamIndex = -1;
            }
            else
            {
                this.streamIndex = streamPart.Index;
            }
            this.wrapperName = messageDescription.Body.WrapperName;
            this.wrapperNS = messageDescription.Body.WrapperNamespace;
            this.partName = streamPart.Name;
            this.partNS = streamPart.Namespace;
            this.isRequest = isRequest;
            this.operationName = operationName;
        }

        internal static StreamFormatter Create(MessageDescription messageDescription, string operationName, bool isRequest)
        {
            MessagePartDescription streamPart = ValidateAndGetStreamPart(messageDescription, isRequest, operationName);
            if (streamPart == null)
            {
                return null;
            }
            return new StreamFormatter(messageDescription, streamPart, operationName, isRequest);
        }

        internal void Deserialize(object[] parameters, ref object retVal, Message message)
        {
            this.SetStreamValue(parameters, ref retVal, new MessageBodyStream(message, this.WrapperName, this.WrapperNamespace, this.PartName, this.PartNamespace, this.isRequest));
        }

        private static MessagePartDescription GetStreamPart(MessageDescription messageDescription)
        {
            if (OperationFormatter.IsValidReturnValue(messageDescription.Body.ReturnValue))
            {
                if ((messageDescription.Body.Parts.Count == 0) && (messageDescription.Body.ReturnValue.Type == typeof(Stream)))
                {
                    return messageDescription.Body.ReturnValue;
                }
            }
            else if ((messageDescription.Body.Parts.Count == 1) && (messageDescription.Body.Parts[0].Type == typeof(Stream)))
            {
                return messageDescription.Body.Parts[0];
            }
            return null;
        }

        private Stream GetStreamValue(object[] parameters, object returnValue)
        {
            if (this.streamIndex == -1)
            {
                return (Stream) returnValue;
            }
            return (Stream) parameters[this.streamIndex];
        }

        private static bool HasStream(MessageDescription messageDescription)
        {
            if ((messageDescription.Body.ReturnValue != null) && (messageDescription.Body.ReturnValue.Type == typeof(Stream)))
            {
                return true;
            }
            foreach (MessagePartDescription description in messageDescription.Body.Parts)
            {
                if (description.Type == typeof(Stream))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsStream(MessageDescription messageDescription)
        {
            return (GetStreamPart(messageDescription) != null);
        }

        internal void Serialize(XmlDictionaryWriter writer, object[] parameters, object returnValue)
        {
            Stream streamValue = this.GetStreamValue(parameters, returnValue);
            if (streamValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(this.partName);
            }
            if (this.WrapperName != null)
            {
                writer.WriteStartElement(this.WrapperName, this.WrapperNamespace);
            }
            writer.WriteStartElement(this.PartName, this.PartNamespace);
            writer.WriteValue(new OperationStreamProvider(streamValue));
            writer.WriteEndElement();
            if (this.wrapperName != null)
            {
                writer.WriteEndElement();
            }
        }

        private void SetStreamValue(object[] parameters, ref object returnValue, Stream streamValue)
        {
            if (this.streamIndex == -1)
            {
                returnValue = streamValue;
            }
            else
            {
                parameters[this.streamIndex] = streamValue;
            }
        }

        private static MessagePartDescription ValidateAndGetStreamPart(MessageDescription messageDescription, bool isRequest, string operationName)
        {
            MessagePartDescription streamPart = GetStreamPart(messageDescription);
            if (streamPart != null)
            {
                return streamPart;
            }
            if (!HasStream(messageDescription))
            {
                return null;
            }
            if (messageDescription.IsTypedMessage)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStreamInTypedMessage", new object[] { messageDescription.MessageName })));
            }
            if (isRequest)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStreamInRequest", new object[] { operationName })));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStreamInResponse", new object[] { operationName })));
        }

        internal string PartName
        {
            get
            {
                return this.partName;
            }
        }

        internal string PartNamespace
        {
            get
            {
                return this.partNS;
            }
        }

        internal string WrapperName
        {
            get
            {
                return this.wrapperName;
            }
            set
            {
                this.wrapperName = value;
            }
        }

        internal string WrapperNamespace
        {
            get
            {
                return this.wrapperNS;
            }
            set
            {
                this.wrapperNS = value;
            }
        }

        internal class MessageBodyStream : Stream
        {
            private string elementName;
            private string elementNs;
            private bool isRequest;
            private Message message;
            private long position;
            private XmlDictionaryReader reader;
            private string wrapperName;
            private string wrapperNs;

            internal MessageBodyStream(Message message, string wrapperName, string wrapperNs, string elementName, string elementNs, bool isRequest)
            {
                this.message = message;
                this.position = 0L;
                this.wrapperName = wrapperName;
                this.wrapperNs = wrapperNs;
                this.elementName = elementName;
                this.elementNs = elementNs;
                this.isRequest = isRequest;
            }

            public override void Close()
            {
                this.message.Close();
                if (this.reader != null)
                {
                    this.reader.Close();
                    this.reader = null;
                }
                base.Close();
            }

            private void EnsureStreamIsOpen()
            {
                if (this.message.State == MessageState.Closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(System.ServiceModel.SR.GetString(this.isRequest ? "SFxStreamRequestMessageClosed" : "SFxStreamResponseMessageClosed")));
                }
            }

            private static void Exhaust(XmlDictionaryReader reader)
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                    }
                }
            }

            public override void Flush()
            {
                throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int num2;
                this.EnsureStreamIsOpen();
                if (buffer == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("buffer"), this.message);
                }
                if (offset < 0)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")), this.message);
                }
                if (count < 0)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", count, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")), this.message);
                }
                if ((buffer.Length - offset) < count)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxInvalidStreamOffsetLength", new object[] { offset + count })), this.message);
                }
                try
                {
                    if (this.reader == null)
                    {
                        this.reader = this.message.GetReaderAtBodyContents();
                        if (this.wrapperName != null)
                        {
                            this.reader.MoveToContent();
                            this.reader.ReadStartElement(this.wrapperName, this.wrapperNs);
                        }
                        this.reader.MoveToContent();
                        if (this.reader.NodeType == XmlNodeType.EndElement)
                        {
                            return 0;
                        }
                        this.reader.ReadStartElement(this.elementName, this.elementNs);
                    }
                    if (this.reader.MoveToContent() != XmlNodeType.Text)
                    {
                        Exhaust(this.reader);
                        return 0;
                    }
                    int num = this.reader.ReadContentAsBase64(buffer, offset, count);
                    this.position += num;
                    if (num == 0)
                    {
                        Exhaust(this.reader);
                    }
                    num2 = num;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IOException(System.ServiceModel.SR.GetString("SFxStreamIOException"), exception));
                }
                return num2;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
            }

            public override void SetLength(long value)
            {
                throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
            }

            public override bool CanRead
            {
                get
                {
                    return (this.message.State != MessageState.Closed);
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public override long Length
            {
                get
                {
                    throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
                }
            }

            public override long Position
            {
                get
                {
                    this.EnsureStreamIsOpen();
                    return this.position;
                }
                set
                {
                    throw TraceUtility.ThrowHelperError(new NotSupportedException(), this.message);
                }
            }
        }

        private class OperationStreamProvider : IStreamProvider
        {
            private Stream stream;

            internal OperationStreamProvider(Stream stream)
            {
                this.stream = stream;
            }

            public Stream GetStream()
            {
                return this.stream;
            }

            public void ReleaseStream(Stream stream)
            {
            }
        }
    }
}

