namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class DefaultMessageBuffer : MessageBuffer
    {
        private string action;
        private bool closed;
        private bool isNullMessage;
        private XmlBuffer msgBuffer;
        private KeyValuePair<string, object>[] properties;
        private Uri to;
        private bool[] understoodHeaders;
        private MessageVersion version;

        public DefaultMessageBuffer(Message message, XmlBuffer msgBuffer)
        {
            this.msgBuffer = msgBuffer;
            this.version = message.Version;
            this.isNullMessage = message is NullMessage;
            this.properties = new KeyValuePair<string, object>[message.Properties.Count];
            ((ICollection<KeyValuePair<string, object>>) message.Properties).CopyTo(this.properties, 0);
            this.understoodHeaders = new bool[message.Headers.Count];
            for (int i = 0; i < this.understoodHeaders.Length; i++)
            {
                this.understoodHeaders[i] = message.Headers.IsUnderstood(i);
            }
            if (this.version == MessageVersion.None)
            {
                this.to = message.Headers.To;
                this.action = message.Headers.Action;
            }
        }

        public override void Close()
        {
            lock (this.ThisLock)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    for (int i = 0; i < this.properties.Length; i++)
                    {
                        IDisposable disposable = this.properties[i].Value as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        public override Message CreateMessage()
        {
            Message message;
            if (this.closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateBufferDisposedException());
            }
            if (this.isNullMessage)
            {
                message = new NullMessage();
            }
            else
            {
                message = Message.CreateMessage(this.msgBuffer.GetReader(0), 0x7fffffff, this.version);
            }
            lock (this.ThisLock)
            {
                message.Properties.CopyProperties(this.properties);
            }
            for (int i = 0; i < this.understoodHeaders.Length; i++)
            {
                if (this.understoodHeaders[i])
                {
                    message.Headers.AddUnderstood(i);
                }
            }
            if (this.to != null)
            {
                message.Headers.To = this.to;
            }
            if (this.action != null)
            {
                message.Headers.Action = this.action;
            }
            return message;
        }

        public override int BufferSize
        {
            get
            {
                return this.msgBuffer.BufferSize;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.msgBuffer;
            }
        }
    }
}

