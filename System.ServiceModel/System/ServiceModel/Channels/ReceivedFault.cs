namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Xml;

    internal class ReceivedFault : MessageFault
    {
        private string actor;
        private FaultCode code;
        private XmlBuffer detail;
        private bool hasDetail;
        private string node;
        private FaultReason reason;
        private EnvelopeVersion receivedVersion;

        private ReceivedFault(FaultCode code, FaultReason reason, string actor, string node, XmlBuffer detail, EnvelopeVersion version)
        {
            this.code = code;
            this.reason = reason;
            this.actor = actor;
            this.node = node;
            this.receivedVersion = version;
            this.hasDetail = this.InferHasDetail(detail);
            this.detail = this.hasDetail ? detail : null;
        }

        public static ReceivedFault CreateFault11(XmlDictionaryReader reader, int maxBufferSize)
        {
            string str;
            string str2;
            reader.ReadStartElement(XD.MessageDictionary.Fault, XD.Message11Dictionary.Namespace);
            reader.ReadStartElement(XD.Message11Dictionary.FaultCode, XD.Message11Dictionary.FaultNamespace);
            XmlUtil.ReadContentAsQName(reader, out str2, out str);
            FaultCode code = new FaultCode(str2, str);
            reader.ReadEndElement();
            string xmlLang = reader.XmlLang;
            reader.MoveToContent();
            FaultReasonText translation = new FaultReasonText(reader.ReadElementContentAsString(XD.Message11Dictionary.FaultString.Value, XD.Message11Dictionary.FaultNamespace.Value), xmlLang);
            string actor = "";
            if (reader.IsStartElement(XD.Message11Dictionary.FaultActor, XD.Message11Dictionary.FaultNamespace))
            {
                actor = reader.ReadElementContentAsString();
            }
            XmlBuffer detail = null;
            if (reader.IsStartElement(XD.Message11Dictionary.FaultDetail, XD.Message11Dictionary.FaultNamespace))
            {
                detail = new XmlBuffer(maxBufferSize);
                detail.OpenSection(reader.Quotas).WriteNode(reader, false);
                detail.CloseSection();
                detail.Close();
            }
            reader.ReadEndElement();
            return new ReceivedFault(code, new FaultReason(translation), actor, actor, detail, EnvelopeVersion.Soap11);
        }

        public static ReceivedFault CreateFault12(XmlDictionaryReader reader, int maxBufferSize)
        {
            return CreateFault12Driver(reader, maxBufferSize, EnvelopeVersion.Soap12);
        }

        private static ReceivedFault CreateFault12Driver(XmlDictionaryReader reader, int maxBufferSize, EnvelopeVersion version)
        {
            reader.ReadStartElement(XD.MessageDictionary.Fault, version.DictionaryNamespace);
            reader.ReadStartElement(XD.Message12Dictionary.FaultCode, version.DictionaryNamespace);
            FaultCode code = ReadFaultCode12Driver(reader, version);
            reader.ReadEndElement();
            List<FaultReasonText> translations = new List<FaultReasonText>();
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.ServiceModel.SR.GetString("AtLeastOneFaultReasonMustBeSpecified")));
            }
            reader.ReadStartElement(XD.Message12Dictionary.FaultReason, version.DictionaryNamespace);
            while (reader.IsStartElement(XD.Message12Dictionary.FaultText, version.DictionaryNamespace))
            {
                translations.Add(ReadTranslation12(reader));
            }
            reader.ReadEndElement();
            string actor = "";
            string node = "";
            if (reader.IsStartElement(XD.Message12Dictionary.FaultNode, version.DictionaryNamespace))
            {
                node = reader.ReadElementContentAsString();
            }
            if (reader.IsStartElement(XD.Message12Dictionary.FaultRole, version.DictionaryNamespace))
            {
                actor = reader.ReadElementContentAsString();
            }
            XmlBuffer detail = null;
            if (reader.IsStartElement(XD.Message12Dictionary.FaultDetail, version.DictionaryNamespace))
            {
                detail = new XmlBuffer(maxBufferSize);
                detail.OpenSection(reader.Quotas).WriteNode(reader, false);
                detail.CloseSection();
                detail.Close();
            }
            reader.ReadEndElement();
            return new ReceivedFault(code, new FaultReason(translations), actor, node, detail, version);
        }

        public static ReceivedFault CreateFaultNone(XmlDictionaryReader reader, int maxBufferSize)
        {
            return CreateFault12Driver(reader, maxBufferSize, EnvelopeVersion.None);
        }

        private bool InferHasDetail(XmlBuffer detail)
        {
            bool flag = false;
            if (detail != null)
            {
                XmlDictionaryReader reader = detail.GetReader(0);
                if (!reader.IsEmptyElement && reader.Read())
                {
                    flag = reader.MoveToContent() != XmlNodeType.EndElement;
                }
                reader.Close();
            }
            return flag;
        }

        protected override XmlDictionaryReader OnGetReaderAtDetailContents()
        {
            XmlDictionaryReader reader = this.detail.GetReader(0);
            reader.Read();
            return reader;
        }

        protected override void OnWriteDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            using (XmlReader reader = this.detail.GetReader(0))
            {
                base.OnWriteStartDetail(writer, version);
                while (reader.MoveToNextAttribute())
                {
                    if (this.ShouldWriteDetailAttribute(version, reader.Prefix, reader.LocalName, reader.Value))
                    {
                        writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                    }
                }
                reader.MoveToElement();
                reader.Read();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    writer.WriteNode(reader, false);
                }
                writer.WriteEndElement();
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            using (XmlReader reader = this.detail.GetReader(0))
            {
                reader.Read();
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    writer.WriteNode(reader, false);
                }
            }
        }

        protected override void OnWriteStartDetail(XmlDictionaryWriter writer, EnvelopeVersion version)
        {
            using (XmlReader reader = this.detail.GetReader(0))
            {
                base.OnWriteStartDetail(writer, version);
                while (reader.MoveToNextAttribute())
                {
                    if (this.ShouldWriteDetailAttribute(version, reader.Prefix, reader.LocalName, reader.Value))
                    {
                        writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                    }
                }
            }
        }

        private static FaultCode ReadFaultCode12Driver(XmlDictionaryReader reader, EnvelopeVersion version)
        {
            string str;
            string str2;
            FaultCode subCode = null;
            reader.ReadStartElement(XD.Message12Dictionary.FaultValue, version.DictionaryNamespace);
            XmlUtil.ReadContentAsQName(reader, out str, out str2);
            reader.ReadEndElement();
            if (reader.IsStartElement(XD.Message12Dictionary.FaultSubcode, version.DictionaryNamespace))
            {
                reader.ReadStartElement();
                subCode = ReadFaultCode12Driver(reader, version);
                reader.ReadEndElement();
                return new FaultCode(str, str2, subCode);
            }
            return new FaultCode(str, str2);
        }

        private static FaultReasonText ReadTranslation12(XmlDictionaryReader reader)
        {
            return new FaultReasonText(reader.ReadElementContentAsString(), XmlUtil.GetXmlLangAttribute(reader));
        }

        private bool ShouldWriteDetailAttribute(EnvelopeVersion targetVersion, string prefix, string localName, string attributeValue)
        {
            bool flag = (((this.receivedVersion == EnvelopeVersion.Soap12) && (targetVersion == EnvelopeVersion.Soap11)) && (string.IsNullOrEmpty(prefix) && (localName == "xmlns"))) && (attributeValue == XD.Message12Dictionary.Namespace.Value);
            return !flag;
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override FaultCode Code
        {
            get
            {
                return this.code;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return this.hasDetail;
            }
        }

        public override string Node
        {
            get
            {
                return this.node;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.reason;
            }
        }
    }
}

