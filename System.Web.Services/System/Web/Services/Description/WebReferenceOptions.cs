namespace System.Web.Services.Description
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Web.Services;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlType("webReferenceOptions", Namespace="http://microsoft.com/webReference/"), XmlRoot("webReferenceOptions", Namespace="http://microsoft.com/webReference/")]
    public class WebReferenceOptions
    {
        private System.Xml.Serialization.CodeGenerationOptions codeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync;
        private static XmlSchema schema;
        private StringCollection schemaImporterExtensions;
        private ServiceDescriptionImportStyle style;
        public const string TargetNamespace = "http://microsoft.com/webReference/";
        private bool verbose;

        public static WebReferenceOptions Read(Stream stream, ValidationEventHandler validationEventHandler)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream) {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(xmlReader, validationEventHandler);
        }

        public static WebReferenceOptions Read(TextReader reader, ValidationEventHandler validationEventHandler)
        {
            XmlTextReader xmlReader = new XmlTextReader(reader) {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            return Read(xmlReader, validationEventHandler);
        }

        public static WebReferenceOptions Read(XmlReader xmlReader, ValidationEventHandler validationEventHandler)
        {
            WebReferenceOptions options;
            XmlValidatingReader reader = new XmlValidatingReader(xmlReader) {
                ValidationType = ValidationType.Schema
            };
            if (validationEventHandler != null)
            {
                reader.ValidationEventHandler += validationEventHandler;
            }
            else
            {
                reader.ValidationEventHandler += new ValidationEventHandler(WebReferenceOptions.SchemaValidationHandler);
            }
            reader.Schemas.Add(Schema);
            webReferenceOptionsSerializer serializer = new webReferenceOptionsSerializer();
            try
            {
                options = (WebReferenceOptions) serializer.Deserialize(reader);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                reader.Close();
            }
            return options;
        }

        private static void SchemaValidationHandler(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Error)
            {
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WsdlInstanceValidationDetails", new object[] { args.Message, args.Exception.LineNumber.ToString(CultureInfo.InvariantCulture), args.Exception.LinePosition.ToString(CultureInfo.InvariantCulture) }));
            }
        }

        [XmlElement("codeGenerationOptions"), DefaultValue(4)]
        public System.Xml.Serialization.CodeGenerationOptions CodeGenerationOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeGenerationOptions;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.codeGenerationOptions = value;
            }
        }

        public static XmlSchema Schema
        {
            get
            {
                if (schema == null)
                {
                    schema = XmlSchema.Read(new StringReader("<?xml version='1.0' encoding='UTF-8' ?>\r\n<xs:schema xmlns:tns='http://microsoft.com/webReference/' elementFormDefault='qualified' targetNamespace='http://microsoft.com/webReference/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\r\n  <xs:simpleType name='options'>\r\n    <xs:list>\r\n      <xs:simpleType>\r\n        <xs:restriction base='xs:string'>\r\n          <xs:enumeration value='properties' />\r\n          <xs:enumeration value='newAsync' />\r\n          <xs:enumeration value='oldAsync' />\r\n          <xs:enumeration value='order' />\r\n          <xs:enumeration value='enableDataBinding' />\r\n        </xs:restriction>\r\n      </xs:simpleType>\r\n    </xs:list>\r\n  </xs:simpleType>\r\n  <xs:simpleType name='style'>\r\n    <xs:restriction base='xs:string'>\r\n      <xs:enumeration value='client' />\r\n      <xs:enumeration value='server' />\r\n      <xs:enumeration value='serverInterface' />\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:complexType name='webReferenceOptions'>\r\n    <xs:all>\r\n      <xs:element minOccurs='0' default='oldAsync' name='codeGenerationOptions' type='tns:options' />\r\n      <xs:element minOccurs='0' default='client' name='style' type='tns:style' />\r\n      <xs:element minOccurs='0' default='false' name='verbose' type='xs:boolean' />\r\n      <xs:element minOccurs='0' name='schemaImporterExtensions'>\r\n        <xs:complexType>\r\n          <xs:sequence>\r\n            <xs:element minOccurs='0' maxOccurs='unbounded' name='type' type='xs:string' />\r\n          </xs:sequence>\r\n        </xs:complexType>\r\n      </xs:element>\r\n    </xs:all>\r\n  </xs:complexType>\r\n  <xs:element name='webReferenceOptions' type='tns:webReferenceOptions' />\r\n  <xs:complexType name='wsdlParameters'>\r\n    <xs:all>\r\n      <xs:element minOccurs='0' name='appSettingBaseUrl' type='xs:string' />\r\n      <xs:element minOccurs='0' name='appSettingUrlKey' type='xs:string' />\r\n      <xs:element minOccurs='0' name='domain' type='xs:string' />\r\n      <xs:element minOccurs='0' name='out' type='xs:string' />\r\n      <xs:element minOccurs='0' name='password' type='xs:string' />\r\n      <xs:element minOccurs='0' name='proxy' type='xs:string' />\r\n      <xs:element minOccurs='0' name='proxydomain' type='xs:string' />\r\n      <xs:element minOccurs='0' name='proxypassword' type='xs:string' />\r\n      <xs:element minOccurs='0' name='proxyusername' type='xs:string' />\r\n      <xs:element minOccurs='0' name='username' type='xs:string' />\r\n      <xs:element minOccurs='0' name='namespace' type='xs:string' />\r\n      <xs:element minOccurs='0' name='language' type='xs:string' />\r\n      <xs:element minOccurs='0' name='protocol' type='xs:string' />\r\n      <xs:element minOccurs='0' name='nologo' type='xs:boolean' />\r\n      <xs:element minOccurs='0' name='parsableerrors' type='xs:boolean' />\r\n      <xs:element minOccurs='0' name='sharetypes' type='xs:boolean' />\r\n      <xs:element minOccurs='0' name='webReferenceOptions' type='tns:webReferenceOptions' />\r\n      <xs:element minOccurs='0' name='documents'>\r\n        <xs:complexType>\r\n          <xs:sequence>\r\n            <xs:element minOccurs='0' maxOccurs='unbounded' name='document' type='xs:string' />\r\n          </xs:sequence>\r\n        </xs:complexType>\r\n      </xs:element>\r\n    </xs:all>\r\n  </xs:complexType>\r\n  <xs:element name='wsdlParameters' type='tns:wsdlParameters' />\r\n</xs:schema>"), null);
                }
                return schema;
            }
        }

        [XmlArray("schemaImporterExtensions"), XmlArrayItem("type")]
        public StringCollection SchemaImporterExtensions
        {
            get
            {
                if (this.schemaImporterExtensions == null)
                {
                    this.schemaImporterExtensions = new StringCollection();
                }
                return this.schemaImporterExtensions;
            }
        }

        [XmlElement("style"), DefaultValue(0)]
        public ServiceDescriptionImportStyle Style
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.style;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.style = value;
            }
        }

        [XmlElement("verbose")]
        public bool Verbose
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.verbose;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.verbose = value;
            }
        }
    }
}

