namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Transactions;
    using System.Transactions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class CoordinationContext : IXmlSerializable
    {
        private AtomicTransactionXmlDictionaryStrings atomicTransactionXmlDictionaryStrings;
        private string contextId;
        private CoordinationStrings coordinationStrings;
        private CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings;
        private string description;
        private uint expiration;
        private bool expiresPresent;
        private System.ServiceModel.Transactions.IsolationFlags isoFlags;
        private System.Transactions.IsolationLevel isoLevel = System.Transactions.IsolationLevel.Unspecified;
        private Guid localTxId = Guid.Empty;
        public const int MaxIdentifierLength = 0x100;
        private byte[] propToken;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        private EndpointAddress registrationRef;
        private List<System.Xml.XmlNode> unknownData;
        private List<System.Xml.XmlNode> unknownExpiresAttributes;
        private List<System.Xml.XmlNode> unknownIdentifierAttributes;
        public const string UuidScheme = "urn:uuid:";

        public CoordinationContext(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            this.atomicTransactionXmlDictionaryStrings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            this.protocolVersion = protocolVersion;
        }

        public static string CreateNativeIdentifier(Guid transactionId)
        {
            return ("urn:uuid:" + transactionId.ToString("D"));
        }

        public static bool IsNativeIdentifier(string identifier, Guid transactionId)
        {
            return (string.Compare(identifier, CreateNativeIdentifier(transactionId), StringComparison.Ordinal) == 0);
        }

        public static CoordinationContext ReadFrom(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            CoordinationContext that = new CoordinationContext(protocolVersion);
            ReadFrom(that, reader, localName, ns, protocolVersion);
            return that;
        }

        private static void ReadFrom(CoordinationContext that, XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            try
            {
                Uri uri;
                CoordinationXmlDictionaryStrings strings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
                AtomicTransactionStrings strings2 = AtomicTransactionStrings.Version(protocolVersion);
                reader.ReadFullStartElement(localName, strings.Namespace);
                reader.MoveToStartElement(strings.Identifier, strings.Namespace);
                that.unknownIdentifierAttributes = ReadOtherAttributes(reader, strings.Namespace);
                that.contextId = reader.ReadElementContentAsString().Trim();
                if ((that.contextId.Length == 0) || (that.contextId.Length > 0x100))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                }
                if (!Uri.TryCreate(that.contextId, UriKind.Absolute, out uri))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                }
                if (reader.IsStartElement(strings.Expires, strings.Namespace))
                {
                    that.unknownExpiresAttributes = ReadOtherAttributes(reader, strings.Namespace);
                    int num = reader.ReadElementContentAsInt();
                    if (num < 0)
                    {
                        throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                    }
                    that.expiration = (uint) num;
                    that.expiresPresent = true;
                }
                reader.MoveToStartElement(strings.CoordinationType, strings.Namespace);
                if (reader.ReadElementContentAsString().Trim() != strings2.Namespace)
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                }
                that.registrationRef = EndpointAddress.ReadFrom(MessagingVersionHelper.AddressingVersion(protocolVersion), reader, strings.RegistrationService, strings.Namespace);
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    that.isoLevel = (System.Transactions.IsolationLevel) reader.ReadElementContentAsInt();
                    if (((that.IsolationLevel < System.Transactions.IsolationLevel.Serializable) || (that.IsolationLevel > System.Transactions.IsolationLevel.Unspecified)) || (that.IsolationLevel == System.Transactions.IsolationLevel.Snapshot))
                    {
                        throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                    }
                }
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationFlags, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    that.isoFlags = (System.ServiceModel.Transactions.IsolationFlags) reader.ReadElementContentAsInt();
                }
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.Description, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    that.description = reader.ReadElementContentAsString().Trim();
                }
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    that.localTxId = reader.ReadElementContentAsGuid();
                }
                if (OleTxTransactionHeader.IsStartPropagationTokenElement(reader))
                {
                    that.propToken = OleTxTransactionHeader.ReadPropagationTokenElement(reader);
                }
                if (reader.IsStartElement())
                {
                    XmlDocument document = new XmlDocument();
                    that.unknownData = new List<System.Xml.XmlNode>(5);
                    while (reader.IsStartElement())
                    {
                        System.Xml.XmlNode item = document.ReadNode(reader);
                        that.unknownData.Add(item);
                    }
                }
                reader.ReadEndElement();
            }
            catch (XmlException exception)
            {
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext"), exception));
            }
        }

        private static List<System.Xml.XmlNode> ReadOtherAttributes(XmlDictionaryReader reader, XmlDictionaryString ns)
        {
            int attributeCount = reader.AttributeCount;
            if (attributeCount == 0)
            {
                return null;
            }
            XmlDocument document = new XmlDocument();
            List<System.Xml.XmlNode> list = new List<System.Xml.XmlNode>(attributeCount);
            reader.MoveToFirstAttribute();
            do
            {
                System.Xml.XmlNode item = document.ReadNode(reader);
                if ((item == null) || (item.NamespaceURI == ns.Value))
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                }
                list.Add(item);
            }
            while (reader.MoveToNextAttribute());
            reader.MoveToElement();
            return list;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            ReadFrom(this, reader2, this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace, this.protocolVersion);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            this.WriteTo(writer2, this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace);
        }

        public void WriteContent(XmlDictionaryWriter writer)
        {
            if ((this.isoLevel != System.Transactions.IsolationLevel.Unspecified) || (this.localTxId != Guid.Empty))
            {
                writer.WriteXmlnsAttribute("mstx", XD.DotNetAtomicTransactionExternalDictionary.Namespace);
            }
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.Identifier, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.unknownIdentifierAttributes != null)
            {
                foreach (System.Xml.XmlNode node in this.unknownIdentifierAttributes)
                {
                    node.WriteTo(writer);
                }
            }
            writer.WriteString(this.contextId);
            writer.WriteEndElement();
            if (this.expiresPresent)
            {
                writer.WriteStartElement(this.coordinationXmlDictionaryStrings.Expires, this.coordinationXmlDictionaryStrings.Namespace);
                if (this.unknownExpiresAttributes != null)
                {
                    foreach (System.Xml.XmlNode node2 in this.unknownExpiresAttributes)
                    {
                        node2.WriteTo(writer);
                    }
                }
                writer.WriteValue((long) this.expiration);
                writer.WriteEndElement();
            }
            writer.WriteStartElement(this.coordinationXmlDictionaryStrings.CoordinationType, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteString(this.atomicTransactionXmlDictionaryStrings.Namespace);
            writer.WriteEndElement();
            this.registrationRef.WriteTo(MessagingVersionHelper.AddressingVersion(this.protocolVersion), writer, this.coordinationXmlDictionaryStrings.RegistrationService, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.isoLevel != System.Transactions.IsolationLevel.Unspecified)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int) this.isoLevel);
                writer.WriteEndElement();
            }
            if (this.isoFlags != 0)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationFlags, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int) this.isoFlags);
                writer.WriteEndElement();
            }
            if (!string.IsNullOrEmpty(this.description))
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.Description, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.description);
                writer.WriteEndElement();
            }
            if (this.localTxId != Guid.Empty)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.localTxId);
                writer.WriteEndElement();
            }
            if (this.propToken != null)
            {
                OleTxTransactionHeader.WritePropagationTokenElement(writer, this.propToken);
            }
            if (this.unknownData != null)
            {
                int count = this.unknownData.Count;
                for (int i = 0; i < count; i++)
                {
                    this.unknownData[i].WriteTo(writer);
                }
            }
        }

        public void WriteTo(XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, localName, ns);
            this.WriteContent(writer);
            writer.WriteEndElement();
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.description = value;
            }
        }

        public uint Expires
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expiration;
            }
            set
            {
                this.expiration = value;
                this.expiresPresent = true;
            }
        }

        public bool ExpiresPresent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expiresPresent;
            }
        }

        public string Identifier
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.contextId = value;
            }
        }

        public System.ServiceModel.Transactions.IsolationFlags IsolationFlags
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isoFlags;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isoFlags = value;
            }
        }

        public System.Transactions.IsolationLevel IsolationLevel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isoLevel;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isoLevel = value;
            }
        }

        public Guid LocalTransactionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localTxId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.localTxId = value;
            }
        }

        public byte[] PropagationToken
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propToken;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.propToken = value;
            }
        }

        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }

        public EndpointAddress RegistrationService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.registrationRef;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.registrationRef = value;
            }
        }
    }
}

