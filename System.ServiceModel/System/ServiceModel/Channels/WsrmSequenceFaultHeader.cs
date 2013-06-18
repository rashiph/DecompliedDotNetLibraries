namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class WsrmSequenceFaultHeader : WsrmMessageHeader
    {
        private WsrmFault fault;

        public WsrmSequenceFaultHeader(ReliableMessagingVersion reliableMessagingVersion, WsrmFault fault) : base(reliableMessagingVersion)
        {
            this.fault = fault;
        }

        public static XmlDictionaryReader GetReaderAtDetailContents(string detailName, string detailNamespace, XmlDictionaryReader headerReader, ReliableMessagingVersion reliableMessagingVersion)
        {
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                return GetReaderAtDetailContentsFeb2005(detailName, detailNamespace, headerReader);
            }
            return GetReaderAtDetailContents11(detailName, detailNamespace, headerReader);
        }

        public static XmlDictionaryReader GetReaderAtDetailContents11(string detailName, string detailNamespace, XmlDictionaryReader headerReader)
        {
            XmlDictionaryString namespaceUri = DXD.Wsrm11Dictionary.Namespace;
            headerReader.ReadFullStartElement(XD.WsrmFeb2005Dictionary.SequenceFault, namespaceUri);
            headerReader.Skip();
            headerReader.ReadFullStartElement(XD.Message12Dictionary.FaultDetail, namespaceUri);
            if (((headerReader.NodeType == XmlNodeType.Element) && !(headerReader.NamespaceURI != detailNamespace)) && !(headerReader.LocalName != detailName))
            {
                return headerReader;
            }
            headerReader.Close();
            return null;
        }

        public static XmlDictionaryReader GetReaderAtDetailContentsFeb2005(string detailName, string detailNamespace, XmlDictionaryReader headerReader)
        {
            XmlDictionaryReader reader;
            try
            {
                WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
                XmlDictionaryString namespaceUri = dictionary.Namespace;
                XmlBuffer buffer = null;
                int sectionIndex = 0;
                int depth = headerReader.Depth;
                headerReader.ReadFullStartElement(dictionary.SequenceFault, namespaceUri);
                while (headerReader.Depth > depth)
                {
                    if (((headerReader.NodeType == XmlNodeType.Element) && (headerReader.NamespaceURI == detailNamespace)) && (headerReader.LocalName == detailName))
                    {
                        if (buffer != null)
                        {
                            return null;
                        }
                        buffer = new XmlBuffer(0x7fffffff);
                        try
                        {
                            sectionIndex = buffer.SectionCount;
                            buffer.OpenSection(headerReader.Quotas).WriteNode(headerReader, false);
                            continue;
                        }
                        finally
                        {
                            buffer.CloseSection();
                        }
                    }
                    if (headerReader.Depth == depth)
                    {
                        break;
                    }
                    headerReader.Read();
                }
                if (buffer == null)
                {
                    return null;
                }
                buffer.Close();
                reader = buffer.GetReader(sectionIndex);
            }
            finally
            {
                headerReader.Close();
            }
            return reader;
        }

        public static string GetSubcode(XmlDictionaryReader headerReader, ReliableMessagingVersion reliableMessagingVersion)
        {
            string localName = null;
            try
            {
                string str3;
                WsrmFeb2005Dictionary dictionary = XD.WsrmFeb2005Dictionary;
                XmlDictionaryString namespaceUri = WsrmIndex.GetNamespace(reliableMessagingVersion);
                headerReader.ReadStartElement(dictionary.SequenceFault, namespaceUri);
                headerReader.ReadStartElement(dictionary.FaultCode, namespaceUri);
                XmlUtil.ReadContentAsQName(headerReader, out localName, out str3);
                if (str3 != WsrmIndex.GetNamespaceString(reliableMessagingVersion))
                {
                    localName = null;
                }
                headerReader.ReadEndElement();
                while (headerReader.IsStartElement())
                {
                    headerReader.Skip();
                }
                headerReader.ReadEndElement();
            }
            finally
            {
                headerReader.Close();
            }
            return localName;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteStartElement("r", "FaultCode", this.Namespace);
            writer.WriteXmlnsAttribute(null, this.Namespace);
            writer.WriteQualifiedName(this.Subcode, this.Namespace);
            writer.WriteEndElement();
            bool flag = base.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
            if (flag)
            {
                writer.WriteStartElement("r", XD.Message12Dictionary.FaultDetail, this.DictionaryNamespace);
            }
            this.fault.WriteDetail(writer);
            if (flag)
            {
                writer.WriteEndElement();
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get
            {
                return XD.WsrmFeb2005Dictionary.SequenceFault;
            }
        }

        public WsrmFault Fault
        {
            get
            {
                return this.fault;
            }
        }

        public string Subcode
        {
            get
            {
                return this.fault.Subcode;
            }
        }
    }
}

