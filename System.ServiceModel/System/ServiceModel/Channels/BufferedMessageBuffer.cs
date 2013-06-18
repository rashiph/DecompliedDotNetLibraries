namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;

    internal class BufferedMessageBuffer : MessageBuffer
    {
        private bool closed;
        private IBufferedMessageData messageData;
        private KeyValuePair<string, object>[] properties;
        private object thisLock = new object();
        private bool[] understoodHeaders;
        private bool understoodHeadersModified;

        public BufferedMessageBuffer(IBufferedMessageData messageData, KeyValuePair<string, object>[] properties, bool[] understoodHeaders, bool understoodHeadersModified)
        {
            this.messageData = messageData;
            this.properties = properties;
            this.understoodHeaders = understoodHeaders;
            this.understoodHeadersModified = understoodHeadersModified;
            messageData.Open();
        }

        public override void Close()
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    this.messageData.Close();
                    this.messageData = null;
                }
            }
        }

        public override Message CreateMessage()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                }
                RecycledMessageState recycledMessageState = this.messageData.TakeMessageState();
                if (recycledMessageState == null)
                {
                    recycledMessageState = new RecycledMessageState();
                }
                BufferedMessage message = new BufferedMessage(this.messageData, recycledMessageState, this.understoodHeaders, this.understoodHeadersModified);
                message.Properties.CopyProperties(this.properties);
                this.messageData.Open();
                return message;
            }
        }

        public override void WriteMessage(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                }
                ArraySegment<byte> buffer = this.messageData.Buffer;
                stream.Write(buffer.Array, buffer.Offset, buffer.Count);
            }
        }

        public override int BufferSize
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.closed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                    }
                    return this.messageData.Buffer.Count;
                }
            }
        }

        public override string MessageContentType
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.closed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
                    }
                    return this.messageData.MessageEncoder.ContentType;
                }
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
}

