namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public abstract class MessageEncoder
    {
        protected MessageEncoder()
        {
        }

        internal ArraySegment<byte> BufferMessageStream(Stream stream, BufferManager bufferManager, int maxBufferSize)
        {
            byte[] buffer = bufferManager.TakeBuffer(0x2000);
            int offset = 0;
            int bufferSize = Math.Min(buffer.Length, maxBufferSize);
            while (offset < bufferSize)
            {
                int num3 = stream.Read(buffer, offset, bufferSize - offset);
                if (num3 == 0)
                {
                    stream.Close();
                    break;
                }
                offset += num3;
                if (offset == bufferSize)
                {
                    if (bufferSize >= maxBufferSize)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException((long) maxBufferSize));
                    }
                    bufferSize = Math.Min(bufferSize * 2, maxBufferSize);
                    byte[] dst = bufferManager.TakeBuffer(bufferSize);
                    Buffer.BlockCopy(buffer, 0, dst, 0, offset);
                    bufferManager.ReturnBuffer(buffer);
                    buffer = dst;
                }
            }
            return new ArraySegment<byte>(buffer, 0, offset);
        }

        public virtual T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(FaultConverter))
            {
                return (T) FaultConverter.GetDefaultFaultConverter(this.MessageVersion);
            }
            return default(T);
        }

        internal virtual bool IsCharSetSupported(string charset)
        {
            return false;
        }

        public virtual bool IsContentTypeSupported(string contentType)
        {
            if (contentType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contentType"));
            }
            return this.IsContentTypeSupported(contentType, this.ContentType, this.MediaType);
        }

        internal bool IsContentTypeSupported(string contentType, string supportedContentType, string supportedMediaType)
        {
            if (supportedContentType != contentType)
            {
                if (((contentType.Length > supportedContentType.Length) && contentType.StartsWith(supportedContentType, StringComparison.Ordinal)) && (contentType[supportedContentType.Length] == ';'))
                {
                    return true;
                }
                if (contentType.StartsWith(supportedContentType, StringComparison.OrdinalIgnoreCase))
                {
                    if (contentType.Length == supportedContentType.Length)
                    {
                        return true;
                    }
                    if (contentType.Length > supportedContentType.Length)
                    {
                        char ch = contentType[supportedContentType.Length];
                        if (ch == ';')
                        {
                            return true;
                        }
                        int length = supportedContentType.Length;
                        if (((ch == '\r') && (contentType.Length > (supportedContentType.Length + 1))) && (contentType[length + 1] == '\n'))
                        {
                            length += 2;
                            ch = contentType[length];
                        }
                        if ((ch == ' ') || (ch == '\t'))
                        {
                            length++;
                            while (length < contentType.Length)
                            {
                                ch = contentType[length];
                                if ((ch != ' ') && (ch != '\t'))
                                {
                                    break;
                                }
                                length++;
                            }
                        }
                        if ((ch == ';') || (length == contentType.Length))
                        {
                            return true;
                        }
                    }
                }
                try
                {
                    System.Net.Mime.ContentType type = new System.Net.Mime.ContentType(contentType);
                    if ((supportedMediaType.Length > 0) && !supportedMediaType.Equals(type.MediaType, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    if (!this.IsCharSetSupported(type.CharSet))
                    {
                        return false;
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return true;
        }

        public Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            return this.ReadMessage(buffer, bufferManager, null);
        }

        public Message ReadMessage(Stream stream, int maxSizeOfHeaders)
        {
            return this.ReadMessage(stream, maxSizeOfHeaders, null);
        }

        public abstract Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType);
        public abstract Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType);
        internal virtual Message ReadMessage(Stream stream, BufferManager bufferManager, int maxBufferSize, string contentType)
        {
            return this.ReadMessage(this.BufferMessageStream(stream, bufferManager, maxBufferSize), bufferManager, contentType);
        }

        internal void ThrowIfMismatchedMessageVersion(Message message)
        {
            if (message.Version != this.MessageVersion)
            {
                throw TraceUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("EncoderMessageVersionMismatch", new object[] { message.Version, this.MessageVersion })), message);
            }
        }

        public override string ToString()
        {
            return this.ContentType;
        }

        public abstract void WriteMessage(Message message, Stream stream);
        public ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager)
        {
            return this.WriteMessage(message, maxMessageSize, bufferManager, 0);
        }

        public abstract ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset);

        public abstract string ContentType { get; }

        public abstract string MediaType { get; }

        public abstract System.ServiceModel.Channels.MessageVersion MessageVersion { get; }
    }
}

