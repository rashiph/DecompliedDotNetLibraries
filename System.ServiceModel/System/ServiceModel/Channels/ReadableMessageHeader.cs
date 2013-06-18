namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class ReadableMessageHeader : MessageHeader
    {
        protected ReadableMessageHeader()
        {
        }

        public abstract XmlDictionaryReader GetHeaderReader();
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            XmlDictionaryReader headerReader = this.GetHeaderReader();
            headerReader.ReadStartElement();
            while (headerReader.NodeType != XmlNodeType.EndElement)
            {
                writer.WriteNode(headerReader, false);
            }
            headerReader.ReadEndElement();
            headerReader.Close();
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (!this.IsMessageVersionSupported(messageVersion))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MessageHeaderVersionNotSupported", new object[] { base.GetType().FullName, messageVersion.ToString() }), "version"));
            }
            XmlDictionaryReader headerReader = this.GetHeaderReader();
            writer.WriteStartElement(headerReader.Prefix, headerReader.LocalName, headerReader.NamespaceURI);
            writer.WriteAttributes(headerReader, false);
            headerReader.Close();
        }
    }
}

