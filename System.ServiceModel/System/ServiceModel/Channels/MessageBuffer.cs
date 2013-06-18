namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.XPath;

    public abstract class MessageBuffer : IXPathNavigable, IDisposable
    {
        protected MessageBuffer()
        {
        }

        public abstract void Close();
        internal Exception CreateBufferDisposedException()
        {
            return new ObjectDisposedException("", System.ServiceModel.SR.GetString("MessageBufferIsClosed"));
        }

        public abstract Message CreateMessage();
        public XPathNavigator CreateNavigator()
        {
            return this.CreateNavigator(0x7fffffff, XmlSpace.None);
        }

        public XPathNavigator CreateNavigator(int nodeQuota)
        {
            return this.CreateNavigator(nodeQuota, XmlSpace.None);
        }

        public XPathNavigator CreateNavigator(XmlSpace space)
        {
            return this.CreateNavigator(0x7fffffff, space);
        }

        public XPathNavigator CreateNavigator(int nodeQuota, XmlSpace space)
        {
            if (nodeQuota <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nodeQuota", System.ServiceModel.SR.GetString("FilterQuotaRange")));
            }
            return new SeekableMessageNavigator(this.CreateMessage(), nodeQuota, space, true, true);
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        public virtual void WriteMessage(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("stream"));
            }
            Message message = this.CreateMessage();
            using (message)
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, XD.Dictionary, null, false);
                using (writer)
                {
                    message.WriteMessage(writer);
                }
            }
        }

        public abstract int BufferSize { get; }

        public virtual string MessageContentType
        {
            get
            {
                return "application/soap+msbin1";
            }
        }
    }
}

