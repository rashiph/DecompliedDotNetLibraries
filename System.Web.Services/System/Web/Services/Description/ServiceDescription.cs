namespace System.Web.Services.Description
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions"), XmlRoot("definitions", Namespace="http://schemas.xmlsoap.org/wsdl/")]
    public sealed class ServiceDescription : NamedItem
    {
        private string appSettingBaseUrl;
        private string appSettingUrlKey;
        private BindingCollection bindings;
        private ServiceDescriptionFormatExtensionCollection extensions;
        private ImportCollection imports;
        private MessageCollection messages;
        public const string Namespace = "http://schemas.xmlsoap.org/wsdl/";
        private static XmlSerializerNamespaces namespaces;
        private ServiceDescription next;
        private ServiceDescriptionCollection parent;
        private PortTypeCollection portTypes;
        internal const string Prefix = "wsdl";
        private string retrievalUrl;
        private static XmlSchema schema = null;
        private static XmlSerializer serializer;
        private ServiceCollection services;
        private static XmlSchema soapEncodingSchema = null;
        private const WsiProfiles SupportedClaims = WsiProfiles.BasicProfile1_1;
        private string targetNamespace;
        private System.Web.Services.Description.Types types;
        private StringCollection validationWarnings;
        private static StringCollection warnings = new StringCollection();

        internal static void AddConformanceClaims(XmlElement documentation, WsiProfiles claims)
        {
            claims &= WsiProfiles.BasicProfile1_1;
            if (claims != WsiProfiles.None)
            {
                WsiProfiles conformanceClaims = GetConformanceClaims(documentation);
                claims &= ~conformanceClaims;
                if (claims != WsiProfiles.None)
                {
                    XmlDocument ownerDocument = documentation.OwnerDocument;
                    if ((claims & WsiProfiles.BasicProfile1_1) != WsiProfiles.None)
                    {
                        XmlElement newChild = ownerDocument.CreateElement("wsi", "Claim", "http://ws-i.org/schemas/conformanceClaim/");
                        newChild.SetAttribute("conformsTo", "http://ws-i.org/profiles/basic/1.1");
                        documentation.InsertBefore(newChild, null);
                    }
                }
            }
        }

        public static bool CanRead(XmlReader reader)
        {
            return Serializer.CanDeserialize(reader);
        }

        internal static WsiProfiles GetConformanceClaims(XmlElement documentation)
        {
            XmlNode nextSibling;
            if (documentation == null)
            {
                return WsiProfiles.None;
            }
            WsiProfiles none = WsiProfiles.None;
            for (XmlNode node = documentation.FirstChild; node != null; node = nextSibling)
            {
                nextSibling = node.NextSibling;
                if (node is XmlElement)
                {
                    XmlElement element = (XmlElement) node;
                    if (((element.LocalName == "Claim") && (element.NamespaceURI == "http://ws-i.org/schemas/conformanceClaim/")) && ("http://ws-i.org/profiles/basic/1.1" == element.GetAttribute("conformsTo")))
                    {
                        none |= WsiProfiles.BasicProfile1_1;
                    }
                }
            }
            return none;
        }

        private static void InstanceValidation(object sender, ValidationEventArgs args)
        {
            warnings.Add(System.Web.Services.Res.GetString("WsdlInstanceValidationDetails", new object[] { args.Message, args.Exception.LineNumber.ToString(CultureInfo.InvariantCulture), args.Exception.LinePosition.ToString(CultureInfo.InvariantCulture) }));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceDescription Read(Stream stream)
        {
            return Read(stream, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceDescription Read(TextReader textReader)
        {
            return Read(textReader, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceDescription Read(string fileName)
        {
            return Read(fileName, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ServiceDescription Read(XmlReader reader)
        {
            return Read(reader, false);
        }

        public static ServiceDescription Read(Stream stream, bool validate)
        {
            XmlTextReader reader = new XmlTextReader(stream) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(reader, validate);
        }

        public static ServiceDescription Read(TextReader textReader, bool validate)
        {
            XmlTextReader reader = new XmlTextReader(textReader) {
                WhitespaceHandling = WhitespaceHandling.Significant,
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(reader, validate);
        }

        public static ServiceDescription Read(string fileName, bool validate)
        {
            ServiceDescription description;
            StreamReader textReader = new StreamReader(fileName, Encoding.Default, true);
            try
            {
                description = Read(textReader, validate);
            }
            finally
            {
                textReader.Close();
            }
            return description;
        }

        public static ServiceDescription Read(XmlReader reader, bool validate)
        {
            if (!validate)
            {
                return (ServiceDescription) Serializer.Deserialize(reader);
            }
            XmlReaderSettings settings = new XmlReaderSettings {
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints
            };
            settings.Schemas.Add(Schema);
            settings.Schemas.Add(SoapBinding.Schema);
            settings.ValidationEventHandler += new ValidationEventHandler(ServiceDescription.InstanceValidation);
            warnings.Clear();
            XmlReader xmlReader = XmlReader.Create(reader, settings);
            if (reader.ReadState != ReadState.Initial)
            {
                xmlReader.Read();
            }
            ServiceDescription description = (ServiceDescription) Serializer.Deserialize(xmlReader);
            description.SetWarnings(warnings);
            return description;
        }

        internal void SetParent(ServiceDescriptionCollection parent)
        {
            this.parent = parent;
        }

        internal void SetWarnings(StringCollection warnings)
        {
            this.validationWarnings = warnings;
        }

        private bool ShouldSerializeTypes()
        {
            return this.Types.HasItems();
        }

        public void Write(Stream stream)
        {
            TextWriter writer = new StreamWriter(stream);
            this.Write(writer);
            writer.Flush();
        }

        public void Write(TextWriter writer)
        {
            XmlTextWriter writer2 = new XmlTextWriter(writer) {
                Formatting = Formatting.Indented,
                Indentation = 2
            };
            this.Write(writer2);
        }

        public void Write(string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                this.Write(writer);
            }
            finally
            {
                writer.Close();
            }
        }

        public void Write(XmlWriter writer)
        {
            XmlSerializerNamespaces namespaces;
            XmlSerializer serializer = Serializer;
            if ((base.Namespaces == null) || (base.Namespaces.Count == 0))
            {
                namespaces = new XmlSerializerNamespaces(ServiceDescription.namespaces);
                namespaces.Add("wsdl", "http://schemas.xmlsoap.org/wsdl/");
                if ((this.TargetNamespace != null) && (this.TargetNamespace.Length != 0))
                {
                    namespaces.Add("tns", this.TargetNamespace);
                }
                for (int i = 0; i < this.Types.Schemas.Count; i++)
                {
                    string targetNamespace = this.Types.Schemas[i].TargetNamespace;
                    if (((targetNamespace != null) && (targetNamespace.Length > 0)) && ((targetNamespace != this.TargetNamespace) && (targetNamespace != "http://schemas.xmlsoap.org/wsdl/")))
                    {
                        namespaces.Add("s" + i.ToString(CultureInfo.InvariantCulture), targetNamespace);
                    }
                }
                for (int j = 0; j < this.Imports.Count; j++)
                {
                    Import import = this.Imports[j];
                    if (import.Namespace.Length > 0)
                    {
                        namespaces.Add("i" + j.ToString(CultureInfo.InvariantCulture), import.Namespace);
                    }
                }
            }
            else
            {
                namespaces = base.Namespaces;
            }
            serializer.Serialize(writer, this, namespaces);
        }

        internal string AppSettingBaseUrl
        {
            get
            {
                return this.appSettingBaseUrl;
            }
            set
            {
                this.appSettingBaseUrl = value;
            }
        }

        internal string AppSettingUrlKey
        {
            get
            {
                return this.appSettingUrlKey;
            }
            set
            {
                this.appSettingUrlKey = value;
            }
        }

        [XmlElement("binding")]
        public BindingCollection Bindings
        {
            get
            {
                if (this.bindings == null)
                {
                    this.bindings = new BindingCollection(this);
                }
                return this.bindings;
            }
        }

        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }

        [XmlElement("import")]
        public ImportCollection Imports
        {
            get
            {
                if (this.imports == null)
                {
                    this.imports = new ImportCollection(this);
                }
                return this.imports;
            }
        }

        [XmlElement("message")]
        public MessageCollection Messages
        {
            get
            {
                if (this.messages == null)
                {
                    this.messages = new MessageCollection(this);
                }
                return this.messages;
            }
        }

        internal ServiceDescription Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        [XmlElement("portType")]
        public PortTypeCollection PortTypes
        {
            get
            {
                if (this.portTypes == null)
                {
                    this.portTypes = new PortTypeCollection(this);
                }
                return this.portTypes;
            }
        }

        [XmlIgnore]
        public string RetrievalUrl
        {
            get
            {
                if (this.retrievalUrl != null)
                {
                    return this.retrievalUrl;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.retrievalUrl = value;
            }
        }

        public static XmlSchema Schema
        {
            get
            {
                if (schema == null)
                {
                    schema = XmlSchema.Read(new StringReader("<?xml version='1.0' encoding='UTF-8' ?> \r\n<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'\r\n           xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/'\r\n           targetNamespace='http://schemas.xmlsoap.org/wsdl/'\r\n           elementFormDefault='qualified' >\r\n   \r\n  <xs:complexType mixed='true' name='tDocumentation' >\r\n    <xs:sequence>\r\n      <xs:any minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tDocumented' >\r\n    <xs:annotation>\r\n      <xs:documentation>\r\n      This type is extended by  component types to allow them to be documented\r\n      </xs:documentation>\r\n    </xs:annotation>\r\n    <xs:sequence>\r\n      <xs:element name='documentation' type='wsdl:tDocumentation' minOccurs='0' />\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n <!-- allow extensibility via elements and attributes on all elements swa124 -->\r\n <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tDocumented' >\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n          This type is extended by component types to allow attributes from other namespaces to be added.\r\n          </xs:documentation>\r\n        </xs:annotation>\r\n        <xs:sequence>\r\n          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax' />   \r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n  <xs:complexType name='tExtensibleDocumented' abstract='true' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tDocumented' >\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n          This type is extended by component types to allow elements from other namespaces to be added.\r\n          </xs:documentation>\r\n        </xs:annotation>\r\n        <xs:sequence>\r\n          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax' />   \r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n  <!-- original wsdl removed as part of swa124 resolution\r\n  <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tDocumented' >\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n          This type is extended by component types to allow attributes from other namespaces to be added.\r\n          </xs:documentation>\r\n        </xs:annotation>\r\n        <xs:anyAttribute namespace='##other' processContents='lax' />    \r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tExtensibleDocumented' abstract='true' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tDocumented' >\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n          This type is extended by component types to allow elements from other namespaces to be added.\r\n          </xs:documentation>\r\n        </xs:annotation>\r\n        <xs:sequence>\r\n          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n        </xs:sequence>\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n -->\r\n  <xs:element name='definitions' type='wsdl:tDefinitions' >\r\n    <xs:key name='message' >\r\n      <xs:selector xpath='wsdl:message' />\r\n      <xs:field xpath='@name' />\r\n    </xs:key>\r\n    <xs:key name='portType' >\r\n      <xs:selector xpath='wsdl:portType' />\r\n      <xs:field xpath='@name' />\r\n    </xs:key>\r\n    <xs:key name='binding' >\r\n      <xs:selector xpath='wsdl:binding' />\r\n      <xs:field xpath='@name' />\r\n    </xs:key>\r\n    <xs:key name='service' >\r\n      <xs:selector xpath='wsdl:service' />\r\n      <xs:field xpath='@name' />\r\n    </xs:key>\r\n    <xs:key name='import' >\r\n      <xs:selector xpath='wsdl:import' />\r\n      <xs:field xpath='@namespace' />\r\n    </xs:key>\r\n  </xs:element>\r\n\r\n  <xs:group name='anyTopLevelOptionalElement' >\r\n    <xs:annotation>\r\n      <xs:documentation>\r\n      Any top level optional element allowed to appear more then once - any child of definitions element except wsdl:types. Any extensibility element is allowed in any place.\r\n      </xs:documentation>\r\n    </xs:annotation>\r\n    <xs:choice>\r\n      <xs:element name='import' type='wsdl:tImport' />\r\n      <xs:element name='types' type='wsdl:tTypes' />                     \r\n      <xs:element name='message'  type='wsdl:tMessage' >\r\n        <xs:unique name='part' >\r\n          <xs:selector xpath='wsdl:part' />\r\n          <xs:field xpath='@name' />\r\n        </xs:unique>\r\n      </xs:element>\r\n      <xs:element name='portType' type='wsdl:tPortType' />\r\n      <xs:element name='binding'  type='wsdl:tBinding' />\r\n      <xs:element name='service'  type='wsdl:tService' >\r\n        <xs:unique name='port' >\r\n          <xs:selector xpath='wsdl:port' />\r\n          <xs:field xpath='@name' />\r\n        </xs:unique>\r\n\t  </xs:element>\r\n    </xs:choice>\r\n  </xs:group>\r\n\r\n  <xs:complexType name='tDefinitions' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:sequence>\r\n          <xs:group ref='wsdl:anyTopLevelOptionalElement'  minOccurs='0'   maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='targetNamespace' type='xs:anyURI' use='optional' />\r\n        <xs:attribute name='name' type='xs:NCName' use='optional' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n   \r\n  <xs:complexType name='tImport' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >\r\n        <xs:attribute name='namespace' type='xs:anyURI' use='required' />\r\n        <xs:attribute name='location' type='xs:anyURI' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n   \r\n  <xs:complexType name='tTypes' >\r\n    <xs:complexContent>   \r\n      <xs:extension base='wsdl:tExtensibleDocumented' />\r\n    </xs:complexContent>   \r\n  </xs:complexType>\r\n     \r\n  <xs:complexType name='tMessage' >\r\n    <xs:complexContent>   \r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:sequence>\r\n          <xs:element name='part' type='wsdl:tPart' minOccurs='0' maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>   \r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tPart' >\r\n    <xs:complexContent>   \r\n      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n        <xs:attribute name='element' type='xs:QName' use='optional' />\r\n        <xs:attribute name='type' type='xs:QName' use='optional' />    \r\n      </xs:extension>\r\n    </xs:complexContent>   \r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tPortType' >\r\n    <xs:complexContent>   \r\n      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >\r\n        <xs:sequence>\r\n          <xs:element name='operation' type='wsdl:tOperation' minOccurs='0' maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>   \r\n  </xs:complexType>\r\n   \r\n  <xs:complexType name='tOperation' >\r\n    <xs:complexContent>   \r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n\t    <xs:sequence>\r\n          <xs:choice>\r\n            <xs:group ref='wsdl:request-response-or-one-way-operation' />\r\n            <xs:group ref='wsdl:solicit-response-or-notification-operation' />\r\n          </xs:choice>\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n        <xs:attribute name='parameterOrder' type='xs:NMTOKENS' use='optional' />\r\n      </xs:extension>\r\n    </xs:complexContent>   \r\n  </xs:complexType>\r\n    \r\n  <xs:group name='request-response-or-one-way-operation' >\r\n    <xs:sequence>\r\n      <xs:element name='input' type='wsdl:tParam' />\r\n\t  <xs:sequence minOccurs='0' >\r\n\t    <xs:element name='output' type='wsdl:tParam' />\r\n\t\t<xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />\r\n      </xs:sequence>\r\n    </xs:sequence>\r\n  </xs:group>\r\n\r\n  <xs:group name='solicit-response-or-notification-operation' >\r\n    <xs:sequence>\r\n      <xs:element name='output' type='wsdl:tParam' />\r\n\t  <xs:sequence minOccurs='0' >\r\n\t    <xs:element name='input' type='wsdl:tParam' />\r\n\t\t<xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />\r\n\t  </xs:sequence>\r\n    </xs:sequence>\r\n  </xs:group>\r\n        \r\n  <xs:complexType name='tParam' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName' use='optional' />\r\n        <xs:attribute name='message' type='xs:QName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tFault' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName'  use='required' />\r\n        <xs:attribute name='message' type='xs:QName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n     \r\n  <xs:complexType name='tBinding' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:sequence>\r\n          <xs:element name='operation' type='wsdl:tBindingOperation' minOccurs='0' maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n        <xs:attribute name='type' type='xs:QName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n    \r\n  <xs:complexType name='tBindingOperationMessage' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName' use='optional' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n  \r\n  <xs:complexType name='tBindingOperationFault' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name='tBindingOperation' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:sequence>\r\n          <xs:element name='input' type='wsdl:tBindingOperationMessage' minOccurs='0' />\r\n          <xs:element name='output' type='wsdl:tBindingOperationMessage' minOccurs='0' />\r\n          <xs:element name='fault' type='wsdl:tBindingOperationFault' minOccurs='0' maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n     \r\n  <xs:complexType name='tService' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:sequence>\r\n          <xs:element name='port' type='wsdl:tPort' minOccurs='0' maxOccurs='unbounded' />\r\n        </xs:sequence>\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n     \r\n  <xs:complexType name='tPort' >\r\n    <xs:complexContent>\r\n      <xs:extension base='wsdl:tExtensibleDocumented' >\r\n        <xs:attribute name='name' type='xs:NCName' use='required' />\r\n        <xs:attribute name='binding' type='xs:QName' use='required' />\r\n      </xs:extension>\r\n    </xs:complexContent>\r\n  </xs:complexType>\r\n\r\n  <xs:attribute name='arrayType' type='xs:string' />\r\n  <xs:attribute name='required' type='xs:boolean' />\r\n  <xs:complexType name='tExtensibilityElement' abstract='true' >\r\n    <xs:attribute ref='wsdl:required' use='optional' />\r\n  </xs:complexType>\r\n\r\n</xs:schema>"), null);
                }
                return schema;
            }
        }

        [XmlIgnore]
        public static XmlSerializer Serializer
        {
            get
            {
                if (serializer == null)
                {
                    WebServicesSection current = WebServicesSection.Current;
                    XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("s", "http://www.w3.org/2001/XMLSchema");
                    WebServicesSection.LoadXmlFormatExtensions(current.GetAllFormatExtensionTypes(), overrides, namespaces);
                    ServiceDescription.namespaces = namespaces;
                    if (current.ServiceDescriptionExtended)
                    {
                        serializer = new XmlSerializer(typeof(ServiceDescription), overrides);
                    }
                    else
                    {
                        serializer = new ServiceDescriptionSerializer();
                    }
                    serializer.UnknownElement += new XmlElementEventHandler(RuntimeUtils.OnUnknownElement);
                }
                return serializer;
            }
        }

        [XmlIgnore]
        public ServiceDescriptionCollection ServiceDescriptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }

        [XmlElement("service")]
        public ServiceCollection Services
        {
            get
            {
                if (this.services == null)
                {
                    this.services = new ServiceCollection(this);
                }
                return this.services;
            }
        }

        internal static XmlSchema SoapEncodingSchema
        {
            get
            {
                if (soapEncodingSchema == null)
                {
                    soapEncodingSchema = XmlSchema.Read(new StringReader("<?xml version='1.0' encoding='UTF-8' ?>\r\n<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'\r\n           xmlns:tns='http://schemas.xmlsoap.org/soap/encoding/'\r\n           targetNamespace='http://schemas.xmlsoap.org/soap/encoding/' >\r\n        \r\n <xs:attribute name='root' >\r\n   <xs:simpleType>\r\n     <xs:restriction base='xs:boolean'>\r\n\t   <xs:pattern value='0|1' />\r\n\t </xs:restriction>\r\n   </xs:simpleType>\r\n </xs:attribute>\r\n\r\n  <xs:attributeGroup name='commonAttributes' >\r\n    <xs:attribute name='id' type='xs:ID' />\r\n    <xs:attribute name='href' type='xs:anyURI' />\r\n    <xs:anyAttribute namespace='##other' processContents='lax' />\r\n  </xs:attributeGroup>\r\n   \r\n  <xs:simpleType name='arrayCoordinate' >\r\n    <xs:restriction base='xs:string' />\r\n  </xs:simpleType>\r\n          \r\n  <xs:attribute name='arrayType' type='xs:string' />\r\n  <xs:attribute name='offset' type='tns:arrayCoordinate' />\r\n  \r\n  <xs:attributeGroup name='arrayAttributes' >\r\n    <xs:attribute ref='tns:arrayType' />\r\n    <xs:attribute ref='tns:offset' />\r\n  </xs:attributeGroup>    \r\n  \r\n  <xs:attribute name='position' type='tns:arrayCoordinate' /> \r\n  \r\n  <xs:attributeGroup name='arrayMemberAttributes' >\r\n    <xs:attribute ref='tns:position' />\r\n  </xs:attributeGroup>    \r\n\r\n  <xs:group name='Array' >\r\n    <xs:sequence>\r\n      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n\t</xs:sequence>\r\n  </xs:group>\r\n\r\n  <xs:element name='Array' type='tns:Array' />\r\n  <xs:complexType name='Array' >\r\n    <xs:group ref='tns:Array' minOccurs='0' />\r\n    <xs:attributeGroup ref='tns:arrayAttributes' />\r\n    <xs:attributeGroup ref='tns:commonAttributes' />\r\n  </xs:complexType> \r\n  <xs:element name='Struct' type='tns:Struct' />\r\n  <xs:group name='Struct' >\r\n    <xs:sequence>\r\n      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />\r\n\t</xs:sequence>\r\n  </xs:group>\r\n\r\n  <xs:complexType name='Struct' >\r\n    <xs:group ref='tns:Struct' minOccurs='0' />\r\n    <xs:attributeGroup ref='tns:commonAttributes'/>\r\n  </xs:complexType> \r\n  \r\n  <xs:simpleType name='base64' >\r\n    <xs:restriction base='xs:base64Binary' />\r\n  </xs:simpleType>\r\n\r\n  <xs:element name='duration' type='tns:duration' />\r\n  <xs:complexType name='duration' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:duration' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='dateTime' type='tns:dateTime' />\r\n  <xs:complexType name='dateTime' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:dateTime' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n\r\n\r\n  <xs:element name='NOTATION' type='tns:NOTATION' />\r\n  <xs:complexType name='NOTATION' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:QName' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  \r\n\r\n  <xs:element name='time' type='tns:time' />\r\n  <xs:complexType name='time' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:time' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='date' type='tns:date' />\r\n  <xs:complexType name='date' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:date' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='gYearMonth' type='tns:gYearMonth' />\r\n  <xs:complexType name='gYearMonth' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:gYearMonth' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='gYear' type='tns:gYear' />\r\n  <xs:complexType name='gYear' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:gYear' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='gMonthDay' type='tns:gMonthDay' />\r\n  <xs:complexType name='gMonthDay' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:gMonthDay' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='gDay' type='tns:gDay' />\r\n  <xs:complexType name='gDay' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:gDay' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='gMonth' type='tns:gMonth' />\r\n  <xs:complexType name='gMonth' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:gMonth' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  \r\n  <xs:element name='boolean' type='tns:boolean' />\r\n  <xs:complexType name='boolean' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:boolean' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='base64Binary' type='tns:base64Binary' />\r\n  <xs:complexType name='base64Binary' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:base64Binary' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='hexBinary' type='tns:hexBinary' />\r\n  <xs:complexType name='hexBinary' >\r\n    <xs:simpleContent>\r\n     <xs:extension base='xs:hexBinary' >\r\n       <xs:attributeGroup ref='tns:commonAttributes' />\r\n     </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='float' type='tns:float' />\r\n  <xs:complexType name='float' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:float' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='double' type='tns:double' />\r\n  <xs:complexType name='double' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:double' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='anyURI' type='tns:anyURI' />\r\n  <xs:complexType name='anyURI' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:anyURI' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='QName' type='tns:QName' />\r\n  <xs:complexType name='QName' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:QName' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  \r\n  <xs:element name='string' type='tns:string' />\r\n  <xs:complexType name='string' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:string' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='normalizedString' type='tns:normalizedString' />\r\n  <xs:complexType name='normalizedString' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:normalizedString' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='token' type='tns:token' />\r\n  <xs:complexType name='token' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:token' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='language' type='tns:language' />\r\n  <xs:complexType name='language' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:language' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='Name' type='tns:Name' />\r\n  <xs:complexType name='Name' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:Name' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='NMTOKEN' type='tns:NMTOKEN' />\r\n  <xs:complexType name='NMTOKEN' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:NMTOKEN' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='NCName' type='tns:NCName' />\r\n  <xs:complexType name='NCName' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:NCName' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='NMTOKENS' type='tns:NMTOKENS' />\r\n  <xs:complexType name='NMTOKENS' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:NMTOKENS' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='ID' type='tns:ID' />\r\n  <xs:complexType name='ID' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:ID' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='IDREF' type='tns:IDREF' />\r\n  <xs:complexType name='IDREF' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:IDREF' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='ENTITY' type='tns:ENTITY' />\r\n  <xs:complexType name='ENTITY' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:ENTITY' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='IDREFS' type='tns:IDREFS' />\r\n  <xs:complexType name='IDREFS' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:IDREFS' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='ENTITIES' type='tns:ENTITIES' />\r\n  <xs:complexType name='ENTITIES' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:ENTITIES' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='decimal' type='tns:decimal' />\r\n  <xs:complexType name='decimal' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:decimal' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='integer' type='tns:integer' />\r\n  <xs:complexType name='integer' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:integer' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='nonPositiveInteger' type='tns:nonPositiveInteger' />\r\n  <xs:complexType name='nonPositiveInteger' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:nonPositiveInteger' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='negativeInteger' type='tns:negativeInteger' />\r\n  <xs:complexType name='negativeInteger' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:negativeInteger' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='long' type='tns:long' />\r\n  <xs:complexType name='long' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:long' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='int' type='tns:int' />\r\n  <xs:complexType name='int' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:int' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='short' type='tns:short' />\r\n  <xs:complexType name='short' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:short' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='byte' type='tns:byte' />\r\n  <xs:complexType name='byte' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:byte' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='nonNegativeInteger' type='tns:nonNegativeInteger' />\r\n  <xs:complexType name='nonNegativeInteger' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:nonNegativeInteger' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='unsignedLong' type='tns:unsignedLong' />\r\n  <xs:complexType name='unsignedLong' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:unsignedLong' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='unsignedInt' type='tns:unsignedInt' />\r\n  <xs:complexType name='unsignedInt' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:unsignedInt' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='unsignedShort' type='tns:unsignedShort' />\r\n  <xs:complexType name='unsignedShort' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:unsignedShort' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='unsignedByte' type='tns:unsignedByte' />\r\n  <xs:complexType name='unsignedByte' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:unsignedByte' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='positiveInteger' type='tns:positiveInteger' />\r\n  <xs:complexType name='positiveInteger' >\r\n    <xs:simpleContent>\r\n      <xs:extension base='xs:positiveInteger' >\r\n        <xs:attributeGroup ref='tns:commonAttributes' />\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n\r\n  <xs:element name='anyType' />\r\n</xs:schema>"), null);
                }
                return soapEncodingSchema;
            }
        }

        [XmlAttribute("targetNamespace")]
        public string TargetNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.targetNamespace = value;
            }
        }

        [XmlElement("types")]
        public System.Web.Services.Description.Types Types
        {
            get
            {
                if (this.types == null)
                {
                    this.types = new System.Web.Services.Description.Types();
                }
                return this.types;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.types = value;
            }
        }

        [XmlIgnore]
        public StringCollection ValidationWarnings
        {
            get
            {
                if (this.validationWarnings == null)
                {
                    this.validationWarnings = new StringCollection();
                }
                return this.validationWarnings;
            }
        }

        internal class ServiceDescriptionSerializer : XmlSerializer
        {
            public override bool CanDeserialize(XmlReader xmlReader)
            {
                return xmlReader.IsStartElement("definitions", "http://schemas.xmlsoap.org/wsdl/");
            }

            protected override XmlSerializationReader CreateReader()
            {
                return new ServiceDescriptionSerializationReader();
            }

            protected override XmlSerializationWriter CreateWriter()
            {
                return new ServiceDescriptionSerializationWriter();
            }

            protected override object Deserialize(XmlSerializationReader reader)
            {
                return ((ServiceDescriptionSerializationReader) reader).Read125_definitions();
            }

            protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
            {
                ((ServiceDescriptionSerializationWriter) writer).Write125_definitions(objectToSerialize);
            }
        }
    }
}

