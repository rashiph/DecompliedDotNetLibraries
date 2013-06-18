namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlRoot(ElementName="MetadataSection", Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex")]
    public class MetadataSection
    {
        private Collection<System.Xml.XmlAttribute> attributes;
        private string dialect;
        private string identifier;
        private object metadata;
        private string sourceUrl;
        private static XmlDocument xmlDocument = new XmlDocument();

        public MetadataSection() : this(null, null, null)
        {
        }

        public MetadataSection(string dialect, string identifier, object metadata)
        {
            this.attributes = new Collection<System.Xml.XmlAttribute>();
            this.dialect = dialect;
            this.identifier = identifier;
            this.metadata = metadata;
        }

        public static MetadataSection CreateFromPolicy(XmlElement policy, string identifier)
        {
            if (policy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policy");
            }
            if (!IsPolicyElement(policy))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("policy", System.ServiceModel.SR.GetString("SFxBadMetadataMustBePolicy", new object[] { "http://schemas.xmlsoap.org/ws/2004/09/policy", "Policy", policy.NamespaceURI, policy.LocalName }));
            }
            return new MetadataSection { Dialect = policy.NamespaceURI, Identifier = identifier, Metadata = policy };
        }

        public static MetadataSection CreateFromSchema(System.Xml.Schema.XmlSchema schema)
        {
            if (schema == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("schema");
            }
            return new MetadataSection { Dialect = XmlSchemaDialect, Identifier = schema.TargetNamespace, Metadata = schema };
        }

        public static MetadataSection CreateFromServiceDescription(System.Web.Services.Description.ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }
            return new MetadataSection { Dialect = ServiceDescriptionDialect, Identifier = serviceDescription.TargetNamespace, Metadata = serviceDescription };
        }

        internal static bool IsPolicyElement(XmlElement policy)
        {
            if (!(policy.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy") && !(policy.NamespaceURI == "http://www.w3.org/ns/ws-policy"))
            {
                return false;
            }
            return (policy.LocalName == "Policy");
        }

        [XmlAnyAttribute]
        public Collection<System.Xml.XmlAttribute> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        [XmlAttribute]
        public string Dialect
        {
            get
            {
                return this.dialect;
            }
            set
            {
                this.dialect = value;
            }
        }

        [XmlAttribute]
        public string Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        [XmlElement("Location", typeof(MetadataLocation), Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex"), XmlElement("schema", typeof(System.Xml.Schema.XmlSchema), Namespace="http://www.w3.org/2001/XMLSchema"), XmlElement("definitions", typeof(System.Web.Services.Description.ServiceDescription), Namespace="http://schemas.xmlsoap.org/wsdl/"), XmlElement("Metadata", typeof(MetadataSet), Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex"), XmlElement("MetadataReference", typeof(MetadataReference), Namespace="http://schemas.xmlsoap.org/ws/2004/09/mex"), XmlAnyElement]
        public object Metadata
        {
            get
            {
                return this.metadata;
            }
            set
            {
                this.metadata = value;
            }
        }

        public static string MetadataExchangeDialect
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/09/mex";
            }
        }

        public static string PolicyDialect
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/09/policy";
            }
        }

        public static string ServiceDescriptionDialect
        {
            get
            {
                return "http://schemas.xmlsoap.org/wsdl/";
            }
        }

        internal string SourceUrl
        {
            get
            {
                return this.sourceUrl;
            }
            set
            {
                this.sourceUrl = value;
            }
        }

        public static string XmlSchemaDialect
        {
            get
            {
                return "http://www.w3.org/2001/XMLSchema";
            }
        }
    }
}

