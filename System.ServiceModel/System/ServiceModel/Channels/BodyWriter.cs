namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    public abstract class BodyWriter
    {
        private bool canWrite;
        private bool isBuffered;
        private object thisLock;

        protected BodyWriter(bool isBuffered)
        {
            this.isBuffered = isBuffered;
            this.canWrite = true;
            if (!this.isBuffered)
            {
                this.thisLock = new object();
            }
        }

        public BodyWriter CreateBufferedCopy(int maxBufferSize)
        {
            if (maxBufferSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (this.isBuffered)
            {
                return this;
            }
            lock (this.thisLock)
            {
                if (!this.canWrite)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BodyWriterCanOnlyBeWrittenOnce")));
                }
                this.canWrite = false;
            }
            BodyWriter writer = this.OnCreateBufferedCopy(maxBufferSize);
            if (!writer.IsBuffered)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BodyWriterReturnedIsNotBuffered")));
            }
            return writer;
        }

        protected virtual BodyWriter OnCreateBufferedCopy(int maxBufferSize)
        {
            return this.OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
        }

        internal BodyWriter OnCreateBufferedCopy(int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            XmlBuffer buffer = new XmlBuffer(maxBufferSize);
            using (XmlDictionaryWriter writer = buffer.OpenSection(quotas))
            {
                writer.WriteStartElement("a");
                this.OnWriteBodyContents(writer);
                writer.WriteEndElement();
            }
            buffer.CloseSection();
            buffer.Close();
            return new BufferedBodyWriter(buffer);
        }

        protected abstract void OnWriteBodyContents(XmlDictionaryWriter writer);
        public void WriteBodyContents(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (!this.isBuffered)
            {
                lock (this.thisLock)
                {
                    if (!this.canWrite)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BodyWriterCanOnlyBeWrittenOnce")));
                    }
                    this.canWrite = false;
                }
            }
            this.OnWriteBodyContents(writer);
        }

        public bool IsBuffered
        {
            get
            {
                return this.isBuffered;
            }
        }

        internal virtual bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        internal virtual bool IsFault
        {
            get
            {
                return false;
            }
        }

        private class BufferedBodyWriter : BodyWriter
        {
            private XmlBuffer buffer;

            public BufferedBodyWriter(XmlBuffer buffer) : base(true)
            {
                this.buffer = buffer;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                XmlDictionaryReader reader = this.buffer.GetReader(0);
                using (reader)
                {
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                    reader.ReadEndElement();
                }
            }
        }
    }
}

