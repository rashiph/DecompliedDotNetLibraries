namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class ContextMessageHeader : MessageHeader
    {
        private IDictionary<string, string> context;
        public const string ContextHeaderName = "Context";
        public const string ContextHeaderNamespace = "http://schemas.microsoft.com/ws/2006/05/context";
        public const string ContextPropertyElement = "Property";
        public const string ContextPropertyNameAttribute = "name";
        private static ChannelProtectionRequirements encryptAndSignChannelProtectionRequirements;
        private static ChannelProtectionRequirements signChannelProtectionRequirements;

        public ContextMessageHeader(IDictionary<string, string> context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            this.context = context;
        }

        internal static ChannelProtectionRequirements GetChannelProtectionRequirements(ProtectionLevel protectionLevel)
        {
            if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                if (encryptAndSignChannelProtectionRequirements == null)
                {
                    MessagePartSpecification parts = new MessagePartSpecification {
                        HeaderTypes = { new XmlQualifiedName("Context", "http://schemas.microsoft.com/ws/2006/05/context") }
                    };
                    ChannelProtectionRequirements requirements2 = new ChannelProtectionRequirements();
                    requirements2.IncomingSignatureParts.AddParts(parts);
                    requirements2.IncomingEncryptionParts.AddParts(parts);
                    requirements2.OutgoingSignatureParts.AddParts(parts);
                    requirements2.OutgoingEncryptionParts.AddParts(parts);
                    requirements2.MakeReadOnly();
                    encryptAndSignChannelProtectionRequirements = requirements2;
                }
                return encryptAndSignChannelProtectionRequirements;
            }
            if (protectionLevel != ProtectionLevel.Sign)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("protectionLevel"));
            }
            if (signChannelProtectionRequirements == null)
            {
                MessagePartSpecification specification2 = new MessagePartSpecification {
                    HeaderTypes = { new XmlQualifiedName("Context", "http://schemas.microsoft.com/ws/2006/05/context") }
                };
                ChannelProtectionRequirements requirements3 = new ChannelProtectionRequirements();
                requirements3.IncomingSignatureParts.AddParts(specification2);
                requirements3.OutgoingSignatureParts.AddParts(specification2);
                requirements3.MakeReadOnly();
                signChannelProtectionRequirements = requirements3;
            }
            return signChannelProtectionRequirements;
        }

        public static ContextMessageProperty GetContextFromHeaderIfExists(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            int headerIndex = message.Headers.FindHeader("Context", "http://schemas.microsoft.com/ws/2006/05/context");
            if (headerIndex >= 0)
            {
                MessageHeaders headers = message.Headers;
                ContextMessageProperty property = ParseContextHeader(headers.GetReaderAtHeader(headerIndex));
                headers.AddUnderstood(headerIndex);
                return property;
            }
            return null;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            WriteHeaderContents(writer, this.context);
        }

        internal static ContextMessageProperty ParseContextHeader(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            ContextMessageProperty property = new ContextMessageProperty();
            try
            {
                if (reader.IsEmptyElement)
                {
                    return property;
                }
                reader.ReadStartElement("Context", "http://schemas.microsoft.com/ws/2006/05/context");
                while (reader.MoveToContent() == XmlNodeType.Element)
                {
                    if ((reader.LocalName != "Property") || (reader.NamespaceURI != "http://schemas.microsoft.com/ws/2006/05/context"))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("SchemaViolationInsideContextHeader")));
                    }
                    string attribute = reader.GetAttribute("name");
                    if (string.IsNullOrEmpty(attribute) || !ContextDictionary.TryValidateKeyValueSpace(attribute))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("InvalidCookieContent", new object[] { attribute })));
                    }
                    property.Context[attribute] = reader.ReadElementString();
                }
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("SchemaViolationInsideContextHeader")));
                }
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("XmlFormatViolationInContextHeader"), exception));
            }
            return property;
        }

        internal static void WriteHeaderContents(XmlDictionaryWriter writer, IDictionary<string, string> context)
        {
            foreach (KeyValuePair<string, string> pair in context)
            {
                writer.WriteStartElement("Property", "http://schemas.microsoft.com/ws/2006/05/context");
                writer.WriteAttributeString("name", null, pair.Key);
                writer.WriteValue(pair.Value);
                writer.WriteEndElement();
            }
        }

        public override string Name
        {
            get
            {
                return "Context";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/context";
            }
        }
    }
}

