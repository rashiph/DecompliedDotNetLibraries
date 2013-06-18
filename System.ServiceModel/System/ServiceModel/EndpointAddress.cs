namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.Xml;

    public class EndpointAddress
    {
        private static EndpointAddress anonymousAddress;
        private static System.Uri anonymousUri;
        private XmlBuffer buffer;
        internal const string DummyName = "Dummy";
        internal const string DummyNamespace = "http://Dummy";
        private int extensionSection;
        private AddressHeaderCollection headers;
        private EndpointIdentity identity;
        private bool isAnonymous;
        private bool isNone;
        private int metadataSection;
        private static System.Uri noneUri;
        private int pspSection;
        private System.Uri uri;

        public EndpointAddress(string uri)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            System.Uri uri2 = new System.Uri(uri);
            this.Init(uri2, null, null, null, -1, -1, -1);
        }

        public EndpointAddress(System.Uri uri, params AddressHeader[] addressHeaders) : this(uri, null, addressHeaders)
        {
        }

        public EndpointAddress(System.Uri uri, EndpointIdentity identity, params AddressHeader[] addressHeaders)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            this.Init(uri, identity, addressHeaders);
        }

        public EndpointAddress(System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            this.Init(uri, identity, headers, null, -1, -1, -1);
        }

        public EndpointAddress(System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers, XmlDictionaryReader metadataReader, XmlDictionaryReader extensionReader) : this(uri, identity, headers, metadataReader, extensionReader, null)
        {
        }

        internal EndpointAddress(System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers, XmlDictionaryReader metadataReader, XmlDictionaryReader extensionReader, XmlDictionaryReader pspReader)
        {
            EndpointIdentity identity2;
            int num;
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }
            XmlBuffer buffer = null;
            this.PossiblyPopulateBuffer(metadataReader, ref buffer, out this.metadataSection);
            buffer = ReadExtensions(extensionReader, null, buffer, out identity2, out num);
            if ((identity != null) && (identity2 != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("MultipleIdentities"), "extensionReader"));
            }
            this.PossiblyPopulateBuffer(pspReader, ref buffer, out this.pspSection);
            if (buffer != null)
            {
                buffer.Close();
            }
            this.Init(uri, identity ?? identity2, headers, buffer, this.metadataSection, num, this.pspSection);
        }

        private EndpointAddress(AddressingVersion version, System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers, XmlBuffer buffer, int metadataSection, int extensionSection, int pspSection)
        {
            this.Init(version, uri, identity, headers, buffer, metadataSection, extensionSection, pspSection);
        }

        public void ApplyTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            System.Uri uri = this.Uri;
            if (this.IsAnonymous)
            {
                if (message.Version.Addressing != AddressingVersion.WSAddressing10)
                {
                    if (message.Version.Addressing != AddressingVersion.WSAddressingAugust2004)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { message.Version.Addressing })));
                    }
                    message.Headers.To = message.Version.Addressing.AnonymousUri;
                }
                else
                {
                    message.Headers.To = null;
                }
            }
            else if (this.IsNone)
            {
                message.Headers.To = message.Version.Addressing.NoneUri;
            }
            else
            {
                message.Headers.To = uri;
            }
            message.Properties.Via = message.Headers.To;
            if (this.headers != null)
            {
                this.headers.AddHeadersTo(message);
            }
        }

        internal static void Copy(XmlDictionaryWriter writer, XmlDictionaryReader reader)
        {
            while (!Done(reader))
            {
                writer.WriteNode(reader, true);
            }
        }

        private static XmlException CreateXmlException(XmlDictionaryReader reader, string message)
        {
            IXmlLineInfo info = reader as IXmlLineInfo;
            if (info != null)
            {
                return new XmlException(message, null, info.LineNumber, info.LinePosition);
            }
            return new XmlException(message);
        }

        private static bool Done(XmlDictionaryReader reader)
        {
            reader.MoveToContent();
            if (reader.NodeType != XmlNodeType.EndElement)
            {
                return reader.EOF;
            }
            return true;
        }

        internal bool EndpointEquals(EndpointAddress endpointAddress)
        {
            if (endpointAddress == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(this, endpointAddress))
            {
                System.Uri uri = this.Uri;
                System.Uri uri2 = endpointAddress.Uri;
                if (!UriEquals(uri, uri2, false, true))
                {
                    return false;
                }
                if (this.Identity == null)
                {
                    if (endpointAddress.Identity != null)
                    {
                        return false;
                    }
                }
                else if (!this.Identity.Equals(endpointAddress.Identity))
                {
                    return false;
                }
                if (!this.Headers.IsEquivalent(endpointAddress.Headers))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, this))
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            EndpointAddress endpointAddress = obj as EndpointAddress;
            if (endpointAddress == null)
            {
                return false;
            }
            return this.EndpointEquals(endpointAddress);
        }

        public override int GetHashCode()
        {
            return UriGetHashCode(this.uri, true);
        }

        public XmlDictionaryReader GetReaderAtExtensions()
        {
            return GetReaderAtSection(this.buffer, this.extensionSection);
        }

        public XmlDictionaryReader GetReaderAtMetadata()
        {
            return GetReaderAtSection(this.buffer, this.metadataSection);
        }

        internal XmlDictionaryReader GetReaderAtPsp()
        {
            return GetReaderAtSection(this.buffer, this.pspSection);
        }

        private static XmlDictionaryReader GetReaderAtSection(XmlBuffer buffer, int section)
        {
            if ((buffer == null) || (section < 0))
            {
                return null;
            }
            XmlDictionaryReader reader = buffer.GetReader(section);
            reader.MoveToContent();
            reader.Read();
            return reader;
        }

        private void Init(System.Uri uri, EndpointIdentity identity, AddressHeader[] headers)
        {
            if ((headers == null) || (headers.Length == 0))
            {
                this.Init(uri, identity, null, null, -1, -1, -1);
            }
            else
            {
                this.Init(uri, identity, new AddressHeaderCollection(headers), null, -1, -1, -1);
            }
        }

        private void Init(System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers, XmlBuffer buffer, int metadataSection, int extensionSection, int pspSection)
        {
            this.Init(null, uri, identity, headers, buffer, metadataSection, extensionSection, pspSection);
        }

        private void Init(AddressingVersion version, System.Uri uri, EndpointIdentity identity, AddressHeaderCollection headers, XmlBuffer buffer, int metadataSection, int extensionSection, int pspSection)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri", System.ServiceModel.SR.GetString("UriMustBeAbsolute"));
            }
            this.uri = uri;
            this.identity = identity;
            this.headers = headers;
            this.buffer = buffer;
            this.metadataSection = metadataSection;
            this.extensionSection = extensionSection;
            this.pspSection = pspSection;
            if (version != null)
            {
                this.isAnonymous = uri == version.AnonymousUri;
                this.isNone = uri == version.NoneUri;
            }
            else
            {
                this.isAnonymous = object.ReferenceEquals(uri, AnonymousUri) || (uri == AnonymousUri);
                this.isNone = object.ReferenceEquals(uri, NoneUri) || (uri == NoneUri);
            }
            if (this.isAnonymous)
            {
                this.uri = AnonymousUri;
            }
            if (this.isNone)
            {
                this.uri = NoneUri;
            }
        }

        public static bool operator ==(EndpointAddress address1, EndpointAddress address2)
        {
            if (object.ReferenceEquals(address2, null))
            {
                return object.ReferenceEquals(address1, null);
            }
            return address2.Equals(address1);
        }

        public static bool operator !=(EndpointAddress address1, EndpointAddress address2)
        {
            if (object.ReferenceEquals(address2, null))
            {
                return !object.ReferenceEquals(address1, null);
            }
            return !address2.Equals(address1);
        }

        private void PossiblyPopulateBuffer(XmlDictionaryReader reader, ref XmlBuffer buffer, out int section)
        {
            if (reader == null)
            {
                section = -1;
            }
            else
            {
                if (buffer == null)
                {
                    buffer = new XmlBuffer(0x7fff);
                }
                section = buffer.SectionCount;
                XmlDictionaryWriter writer = buffer.OpenSection(reader.Quotas);
                writer.WriteStartElement("Dummy", "http://Dummy");
                Copy(writer, reader);
                buffer.CloseSection();
            }
        }

        private static bool ReadContentsFrom10(XmlDictionaryReader reader, out System.Uri uri, out AddressHeaderCollection headers, out EndpointIdentity identity, out XmlBuffer buffer, out int metadataSection, out int extensionSection)
        {
            buffer = null;
            extensionSection = -1;
            metadataSection = -1;
            if (!reader.IsStartElement(XD.AddressingDictionary.Address, XD.Addressing10Dictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(reader, System.ServiceModel.SR.GetString("UnexpectedElementExpectingElement", new object[] { reader.LocalName, reader.NamespaceURI, XD.AddressingDictionary.Address.Value, XD.Addressing10Dictionary.Namespace.Value })));
            }
            string uriString = reader.ReadElementContentAsString();
            if (reader.IsStartElement(XD.AddressingDictionary.ReferenceParameters, XD.Addressing10Dictionary.Namespace))
            {
                headers = AddressHeaderCollection.ReadServiceParameters(reader);
            }
            else
            {
                headers = null;
            }
            if (reader.IsStartElement(XD.Addressing10Dictionary.Metadata, XD.Addressing10Dictionary.Namespace))
            {
                reader.ReadFullStartElement();
                buffer = new XmlBuffer(0x7fff);
                metadataSection = 0;
                XmlDictionaryWriter writer = buffer.OpenSection(reader.Quotas);
                writer.WriteStartElement("Dummy", "http://Dummy");
                while ((reader.NodeType != XmlNodeType.EndElement) && !reader.EOF)
                {
                    writer.WriteNode(reader, true);
                }
                writer.Flush();
                buffer.CloseSection();
                reader.ReadEndElement();
            }
            buffer = ReadExtensions(reader, AddressingVersion.WSAddressing10, buffer, out identity, out extensionSection);
            if (buffer != null)
            {
                buffer.Close();
            }
            if (uriString == "http://www.w3.org/2005/08/addressing/anonymous")
            {
                uri = AddressingVersion.WSAddressing10.AnonymousUri;
                if ((headers == null) && (identity == null))
                {
                    return true;
                }
            }
            else
            {
                if (uriString == "http://www.w3.org/2005/08/addressing/none")
                {
                    uri = AddressingVersion.WSAddressing10.NoneUri;
                    return false;
                }
                if (!System.Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidUriValue", new object[] { uriString, XD.AddressingDictionary.Address.Value, XD.Addressing10Dictionary.Namespace.Value })));
                }
            }
            return false;
        }

        private static bool ReadContentsFrom200408(XmlDictionaryReader reader, out System.Uri uri, out AddressHeaderCollection headers, out EndpointIdentity identity, out XmlBuffer buffer, out int metadataSection, out int extensionSection, out int pspSection)
        {
            buffer = null;
            headers = null;
            extensionSection = -1;
            metadataSection = -1;
            pspSection = -1;
            reader.MoveToContent();
            if (!reader.IsStartElement(XD.AddressingDictionary.Address, AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(reader, System.ServiceModel.SR.GetString("UnexpectedElementExpectingElement", new object[] { reader.LocalName, reader.NamespaceURI, XD.AddressingDictionary.Address.Value, XD.Addressing200408Dictionary.Namespace.Value })));
            }
            string uriString = reader.ReadElementContentAsString();
            reader.MoveToContent();
            if (reader.IsStartElement(XD.AddressingDictionary.ReferenceProperties, AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                headers = AddressHeaderCollection.ReadServiceParameters(reader, true);
            }
            reader.MoveToContent();
            if (reader.IsStartElement(XD.AddressingDictionary.ReferenceParameters, AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                if (headers != null)
                {
                    List<AddressHeader> addressHeaders = new List<AddressHeader>();
                    foreach (AddressHeader header in headers)
                    {
                        addressHeaders.Add(header);
                    }
                    foreach (AddressHeader header2 in AddressHeaderCollection.ReadServiceParameters(reader))
                    {
                        addressHeaders.Add(header2);
                    }
                    headers = new AddressHeaderCollection(addressHeaders);
                }
                else
                {
                    headers = AddressHeaderCollection.ReadServiceParameters(reader);
                }
            }
            XmlDictionaryWriter writer = null;
            reader.MoveToContent();
            if (reader.IsStartElement(XD.AddressingDictionary.PortType, AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                if (writer == null)
                {
                    if (buffer == null)
                    {
                        buffer = new XmlBuffer(0x7fff);
                    }
                    writer = buffer.OpenSection(reader.Quotas);
                    writer.WriteStartElement("Dummy", "http://Dummy");
                }
                writer.WriteNode(reader, true);
            }
            reader.MoveToContent();
            if (reader.IsStartElement(XD.AddressingDictionary.ServiceName, AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                if (writer == null)
                {
                    if (buffer == null)
                    {
                        buffer = new XmlBuffer(0x7fff);
                    }
                    writer = buffer.OpenSection(reader.Quotas);
                    writer.WriteStartElement("Dummy", "http://Dummy");
                }
                writer.WriteNode(reader, true);
            }
            reader.MoveToContent();
            while (reader.IsNamespaceUri(XD.PolicyDictionary.Namespace))
            {
                if (writer == null)
                {
                    if (buffer == null)
                    {
                        buffer = new XmlBuffer(0x7fff);
                    }
                    writer = buffer.OpenSection(reader.Quotas);
                    writer.WriteStartElement("Dummy", "http://Dummy");
                }
                writer.WriteNode(reader, true);
                reader.MoveToContent();
            }
            if (writer != null)
            {
                writer.WriteEndElement();
                buffer.CloseSection();
                pspSection = buffer.SectionCount - 1;
                writer = null;
            }
            else
            {
                pspSection = -1;
            }
            if (reader.IsStartElement("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex"))
            {
                if (writer == null)
                {
                    if (buffer == null)
                    {
                        buffer = new XmlBuffer(0x7fff);
                    }
                    writer = buffer.OpenSection(reader.Quotas);
                    writer.WriteStartElement("Dummy", "http://Dummy");
                }
                writer.WriteNode(reader, true);
            }
            if (writer != null)
            {
                writer.WriteEndElement();
                buffer.CloseSection();
                metadataSection = buffer.SectionCount - 1;
                writer = null;
            }
            else
            {
                metadataSection = -1;
            }
            reader.MoveToContent();
            buffer = ReadExtensions(reader, AddressingVersion.WSAddressingAugust2004, buffer, out identity, out extensionSection);
            if (buffer != null)
            {
                buffer.Close();
            }
            if (uriString == "http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous")
            {
                uri = AddressingVersion.WSAddressingAugust2004.AnonymousUri;
                if ((headers == null) && (identity == null))
                {
                    return true;
                }
            }
            else if (!System.Uri.TryCreate(uriString, UriKind.Absolute, out uri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidUriValue", new object[] { uriString, XD.AddressingDictionary.Address.Value, AddressingVersion.WSAddressingAugust2004.Namespace })));
            }
            return false;
        }

        internal static XmlBuffer ReadExtensions(XmlDictionaryReader reader, AddressingVersion version, XmlBuffer buffer, out EndpointIdentity identity, out int section)
        {
            if (reader == null)
            {
                identity = null;
                section = -1;
                return buffer;
            }
            identity = null;
            XmlDictionaryWriter writer = null;
            reader.MoveToContent();
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(XD.AddressingDictionary.Identity, XD.AddressingDictionary.IdentityExtensionNamespace))
                {
                    if (identity != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(reader, System.ServiceModel.SR.GetString("UnexpectedDuplicateElement", new object[] { XD.AddressingDictionary.Identity.Value, XD.AddressingDictionary.IdentityExtensionNamespace.Value })));
                    }
                    identity = EndpointIdentity.ReadIdentity(reader);
                }
                else
                {
                    if ((version != null) && (reader.NamespaceURI == version.Namespace))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(reader, System.ServiceModel.SR.GetString("AddressingExtensionInBadNS", new object[] { reader.LocalName, reader.NamespaceURI })));
                    }
                    if (writer == null)
                    {
                        if (buffer == null)
                        {
                            buffer = new XmlBuffer(0x7fff);
                        }
                        writer = buffer.OpenSection(reader.Quotas);
                        writer.WriteStartElement("Dummy", "http://Dummy");
                    }
                    writer.WriteNode(reader, true);
                }
                reader.MoveToContent();
            }
            if (writer != null)
            {
                writer.WriteEndElement();
                buffer.CloseSection();
                section = buffer.SectionCount - 1;
                return buffer;
            }
            section = -1;
            return buffer;
        }

        public static EndpointAddress ReadFrom(XmlDictionaryReader reader)
        {
            AddressingVersion version;
            return ReadFrom(reader, out version);
        }

        public static EndpointAddress ReadFrom(AddressingVersion addressingVersion, XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            reader.ReadFullStartElement();
            EndpointAddress address = ReadFromDriver(addressingVersion, reader);
            reader.ReadEndElement();
            return address;
        }

        public static EndpointAddress ReadFrom(AddressingVersion addressingVersion, XmlReader reader)
        {
            return ReadFrom(addressingVersion, XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        internal static EndpointAddress ReadFrom(XmlDictionaryReader reader, out AddressingVersion version)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            reader.ReadFullStartElement();
            reader.MoveToContent();
            if (reader.IsNamespaceUri(AddressingVersion.WSAddressing10.DictionaryNamespace))
            {
                version = AddressingVersion.WSAddressing10;
            }
            else if (reader.IsNamespaceUri(AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                version = AddressingVersion.WSAddressingAugust2004;
            }
            else
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("CannotDetectAddressingVersion"));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { reader.NamespaceURI }));
            }
            EndpointAddress address = ReadFromDriver(version, reader);
            reader.ReadEndElement();
            return address;
        }

        public static EndpointAddress ReadFrom(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            AddressingVersion version;
            return ReadFrom(reader, localName, ns, out version);
        }

        public static EndpointAddress ReadFrom(AddressingVersion addressingVersion, XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            reader.ReadFullStartElement(localName, ns);
            EndpointAddress address = ReadFromDriver(addressingVersion, reader);
            reader.ReadEndElement();
            return address;
        }

        public static EndpointAddress ReadFrom(AddressingVersion addressingVersion, XmlReader reader, string localName, string ns)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
            reader2.ReadFullStartElement(localName, ns);
            EndpointAddress address = ReadFromDriver(addressingVersion, reader2);
            reader.ReadEndElement();
            return address;
        }

        internal static EndpointAddress ReadFrom(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns, out AddressingVersion version)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            reader.ReadFullStartElement(localName, ns);
            reader.MoveToContent();
            if (reader.IsNamespaceUri(AddressingVersion.WSAddressing10.DictionaryNamespace))
            {
                version = AddressingVersion.WSAddressing10;
            }
            else if (reader.IsNamespaceUri(AddressingVersion.WSAddressingAugust2004.DictionaryNamespace))
            {
                version = AddressingVersion.WSAddressingAugust2004;
            }
            else
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("CannotDetectAddressingVersion"));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("reader", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { reader.NamespaceURI }));
            }
            EndpointAddress address = ReadFromDriver(version, reader);
            reader.ReadEndElement();
            return address;
        }

        private static EndpointAddress ReadFromDriver(AddressingVersion addressingVersion, XmlDictionaryReader reader)
        {
            AddressHeaderCollection headers;
            EndpointIdentity identity;
            System.Uri uri;
            XmlBuffer buffer;
            bool flag;
            int num;
            int num2;
            int pspSection = -1;
            if (addressingVersion == AddressingVersion.WSAddressing10)
            {
                flag = ReadContentsFrom10(reader, out uri, out headers, out identity, out buffer, out num2, out num);
            }
            else
            {
                if (addressingVersion != AddressingVersion.WSAddressingAugust2004)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion }));
                }
                flag = ReadContentsFrom200408(reader, out uri, out headers, out identity, out buffer, out num2, out num, out pspSection);
            }
            if ((flag && (headers == null)) && ((identity == null) && (buffer == null)))
            {
                return AnonymousAddress;
            }
            return new EndpointAddress(addressingVersion, uri, identity, headers, buffer, num2, num, pspSection);
        }

        public override string ToString()
        {
            return this.uri.ToString();
        }

        internal static bool UriEquals(System.Uri u1, System.Uri u2, bool ignoreCase, bool includeHostInComparison)
        {
            return UriEquals(u1, u2, ignoreCase, includeHostInComparison, true);
        }

        internal static bool UriEquals(System.Uri u1, System.Uri u2, bool ignoreCase, bool includeHostInComparison, bool includePortInComparison)
        {
            if (u1.Equals(u2))
            {
                return true;
            }
            if (u1.Scheme != u2.Scheme)
            {
                return false;
            }
            if (includePortInComparison && (u1.Port != u2.Port))
            {
                return false;
            }
            if (includeHostInComparison && (string.Compare(u1.Host, u2.Host, StringComparison.OrdinalIgnoreCase) != 0))
            {
                return false;
            }
            if (string.Compare(u1.AbsolutePath, u2.AbsolutePath, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
            {
                return true;
            }
            string components = u1.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            string strB = u2.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            int length = ((components.Length > 0) && (components[components.Length - 1] == '/')) ? (components.Length - 1) : components.Length;
            int num2 = ((strB.Length > 0) && (strB[strB.Length - 1] == '/')) ? (strB.Length - 1) : strB.Length;
            if (num2 != length)
            {
                return false;
            }
            return (string.Compare(components, 0, strB, 0, length, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0);
        }

        internal static int UriGetHashCode(System.Uri uri, bool includeHostInComparison)
        {
            return UriGetHashCode(uri, includeHostInComparison, true);
        }

        internal static int UriGetHashCode(System.Uri uri, bool includeHostInComparison, bool includePortInComparison)
        {
            UriComponents components = UriComponents.Path | UriComponents.Scheme;
            if (includePortInComparison)
            {
                components |= UriComponents.Port;
            }
            if (includeHostInComparison)
            {
                components |= UriComponents.Host;
            }
            string str = uri.GetComponents(components, UriFormat.Unescaped);
            if ((str.Length > 0) && (str[str.Length - 1] != '/'))
            {
                str = str + "/";
            }
            return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
        }

        public void WriteContentsTo(AddressingVersion addressingVersion, XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            if (addressingVersion == AddressingVersion.WSAddressing10)
            {
                this.WriteContentsTo10(writer);
            }
            else if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
            {
                this.WriteContentsTo200408(writer);
            }
            else
            {
                if (addressingVersion != AddressingVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion }));
                }
                this.WriteContentsToNone(writer);
            }
        }

        public void WriteContentsTo(AddressingVersion addressingVersion, XmlWriter writer)
        {
            XmlDictionaryWriter writer2 = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            this.WriteContentsTo(addressingVersion, writer2);
        }

        private void WriteContentsTo10(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(XD.AddressingDictionary.Address, XD.Addressing10Dictionary.Namespace);
            if (this.isAnonymous)
            {
                writer.WriteString(XD.Addressing10Dictionary.Anonymous);
            }
            else if (this.isNone)
            {
                writer.WriteString(XD.Addressing10Dictionary.NoneAddress);
            }
            else
            {
                writer.WriteString(this.Uri.AbsoluteUri);
            }
            writer.WriteEndElement();
            if ((this.headers != null) && (this.headers.Count > 0))
            {
                writer.WriteStartElement(XD.AddressingDictionary.ReferenceParameters, XD.Addressing10Dictionary.Namespace);
                this.headers.WriteContentsTo(writer);
                writer.WriteEndElement();
            }
            if (this.metadataSection >= 0)
            {
                XmlDictionaryReader readerAtSection = GetReaderAtSection(this.buffer, this.metadataSection);
                writer.WriteStartElement(XD.Addressing10Dictionary.Metadata, XD.Addressing10Dictionary.Namespace);
                Copy(writer, readerAtSection);
                writer.WriteEndElement();
            }
            if (this.Identity != null)
            {
                this.Identity.WriteTo(writer);
            }
            if (this.extensionSection >= 0)
            {
                XmlDictionaryReader reader = GetReaderAtSection(this.buffer, this.extensionSection);
                while (reader.IsStartElement())
                {
                    if (reader.NamespaceURI == AddressingVersion.WSAddressing10.Namespace)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(reader, System.ServiceModel.SR.GetString("AddressingExtensionInBadNS", new object[] { reader.LocalName, reader.NamespaceURI })));
                    }
                    writer.WriteNode(reader, true);
                }
            }
        }

        private void WriteContentsTo200408(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(XD.AddressingDictionary.Address, XD.Addressing200408Dictionary.Namespace);
            if (this.isAnonymous)
            {
                writer.WriteString(XD.Addressing200408Dictionary.Anonymous);
            }
            else
            {
                if (this.isNone)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("SFxNone2004"));
                }
                writer.WriteString(this.Uri.AbsoluteUri);
            }
            writer.WriteEndElement();
            if ((this.headers != null) && this.headers.HasReferenceProperties)
            {
                writer.WriteStartElement(XD.AddressingDictionary.ReferenceProperties, XD.Addressing200408Dictionary.Namespace);
                this.headers.WriteReferencePropertyContentsTo(writer);
                writer.WriteEndElement();
            }
            if ((this.headers != null) && this.headers.HasNonReferenceProperties)
            {
                writer.WriteStartElement(XD.AddressingDictionary.ReferenceParameters, XD.Addressing200408Dictionary.Namespace);
                this.headers.WriteNonReferencePropertyContentsTo(writer);
                writer.WriteEndElement();
            }
            XmlDictionaryReader readerAtSection = null;
            if (this.pspSection >= 0)
            {
                readerAtSection = GetReaderAtSection(this.buffer, this.pspSection);
                Copy(writer, readerAtSection);
            }
            readerAtSection = null;
            if (this.metadataSection >= 0)
            {
                readerAtSection = GetReaderAtSection(this.buffer, this.metadataSection);
                Copy(writer, readerAtSection);
            }
            if (this.Identity != null)
            {
                this.Identity.WriteTo(writer);
            }
            if (this.extensionSection >= 0)
            {
                readerAtSection = GetReaderAtSection(this.buffer, this.extensionSection);
                while (readerAtSection.IsStartElement())
                {
                    if (readerAtSection.NamespaceURI == AddressingVersion.WSAddressingAugust2004.Namespace)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateXmlException(readerAtSection, System.ServiceModel.SR.GetString("AddressingExtensionInBadNS", new object[] { readerAtSection.LocalName, readerAtSection.NamespaceURI })));
                    }
                    writer.WriteNode(readerAtSection, true);
                }
            }
        }

        private void WriteContentsToNone(XmlDictionaryWriter writer)
        {
            writer.WriteString(this.Uri.AbsoluteUri);
        }

        public void WriteTo(AddressingVersion addressingVersion, XmlDictionaryWriter writer)
        {
            this.WriteTo(addressingVersion, writer, XD.AddressingDictionary.EndpointReference, addressingVersion.DictionaryNamespace);
        }

        public void WriteTo(AddressingVersion addressingVersion, XmlWriter writer)
        {
            XmlDictionaryString dictionaryNamespace = addressingVersion.DictionaryNamespace;
            if (dictionaryNamespace == null)
            {
                dictionaryNamespace = XD.AddressingDictionary.Empty;
            }
            this.WriteTo(addressingVersion, XmlDictionaryWriter.CreateDictionaryWriter(writer), XD.AddressingDictionary.EndpointReference, dictionaryNamespace);
        }

        public void WriteTo(AddressingVersion addressingVersion, XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            writer.WriteStartElement(localName, ns);
            this.WriteContentsTo(addressingVersion, writer);
            writer.WriteEndElement();
        }

        public void WriteTo(AddressingVersion addressingVersion, XmlWriter writer, string localName, string ns)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            writer.WriteStartElement(localName, ns);
            this.WriteContentsTo(addressingVersion, writer);
            writer.WriteEndElement();
        }

        internal static EndpointAddress AnonymousAddress
        {
            get
            {
                if (anonymousAddress == null)
                {
                    anonymousAddress = new EndpointAddress(AnonymousUri, new AddressHeader[0]);
                }
                return anonymousAddress;
            }
        }

        public static System.Uri AnonymousUri
        {
            get
            {
                if (anonymousUri == null)
                {
                    anonymousUri = new System.Uri("http://schemas.microsoft.com/2005/12/ServiceModel/Addressing/Anonymous");
                }
                return anonymousUri;
            }
        }

        internal XmlBuffer Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        public AddressHeaderCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new AddressHeaderCollection();
                }
                return this.headers;
            }
        }

        public EndpointIdentity Identity
        {
            get
            {
                return this.identity;
            }
        }

        public bool IsAnonymous
        {
            get
            {
                return this.isAnonymous;
            }
        }

        public bool IsNone
        {
            get
            {
                return this.isNone;
            }
        }

        public static System.Uri NoneUri
        {
            get
            {
                if (noneUri == null)
                {
                    noneUri = new System.Uri("http://schemas.microsoft.com/2005/12/ServiceModel/Addressing/None");
                }
                return noneUri;
            }
        }

        [TypeConverter(typeof(UriTypeConverter))]
        public System.Uri Uri
        {
            get
            {
                return this.uri;
            }
        }
    }
}

