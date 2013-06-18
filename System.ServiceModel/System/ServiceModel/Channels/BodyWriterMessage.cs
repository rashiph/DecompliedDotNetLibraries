namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class BodyWriterMessage : Message
    {
        private System.ServiceModel.Channels.BodyWriter bodyWriter;
        private MessageHeaders headers;
        private MessageProperties properties;

        private BodyWriterMessage(System.ServiceModel.Channels.BodyWriter bodyWriter)
        {
            this.bodyWriter = bodyWriter;
        }

        public BodyWriterMessage(MessageHeaders headers, KeyValuePair<string, object>[] properties, System.ServiceModel.Channels.BodyWriter bodyWriter) : this(bodyWriter)
        {
            this.headers = new MessageHeaders(headers);
            this.properties = new MessageProperties(properties);
        }

        public BodyWriterMessage(MessageVersion version, ActionHeader actionHeader, System.ServiceModel.Channels.BodyWriter bodyWriter) : this(bodyWriter)
        {
            this.headers = new MessageHeaders(version);
            this.headers.SetActionHeader(actionHeader);
        }

        public BodyWriterMessage(MessageVersion version, string action, System.ServiceModel.Channels.BodyWriter bodyWriter) : this(bodyWriter)
        {
            this.headers = new MessageHeaders(version);
            this.headers.Action = action;
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (this.bodyWriter.IsBuffered)
            {
                this.bodyWriter.WriteBodyContents(writer);
            }
            else
            {
                writer.WriteString(System.ServiceModel.SR.GetString("MessageBodyIsStream"));
            }
        }

        protected override void OnClose()
        {
            Exception exception = null;
            try
            {
                base.OnClose();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
            }
            try
            {
                if (this.properties != null)
                {
                    this.properties.Dispose();
                }
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception3;
                }
            }
            if (exception != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            this.bodyWriter = null;
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            System.ServiceModel.Channels.BodyWriter bodyWriter;
            if (this.bodyWriter.IsBuffered)
            {
                bodyWriter = this.bodyWriter;
            }
            else
            {
                bodyWriter = this.bodyWriter.CreateBufferedCopy(maxBufferSize);
            }
            KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[this.Properties.Count];
            ((ICollection<KeyValuePair<string, object>>) this.Properties).CopyTo(array, 0);
            return new BodyWriterMessageBuffer(this.headers, array, bodyWriter);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.bodyWriter.WriteBodyContents(writer);
        }

        protected internal System.ServiceModel.Channels.BodyWriter BodyWriter
        {
            get
            {
                return this.bodyWriter;
            }
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.headers;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.bodyWriter.IsEmpty;
            }
        }

        public override bool IsFault
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.bodyWriter.IsFault;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                if (this.properties == null)
                {
                    this.properties = new MessageProperties();
                }
                return this.properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                if (base.IsDisposed)
                {
                    throw TraceUtility.ThrowHelperError(base.CreateMessageDisposedException(), this);
                }
                return this.headers.MessageVersion;
            }
        }
    }
}

