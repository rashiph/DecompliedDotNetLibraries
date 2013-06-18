namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlSchemaProvider("GetSchema"), XmlRoot("EndpointReference", Namespace="http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public class EndpointAddressAugust2004 : IXmlSerializable
    {
        private EndpointAddress address;
        private static XmlQualifiedName eprType;
        private const string Schema = "<xs:schema targetNamespace=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" elementFormDefault=\"qualified\" blockDefault=\"#all\">\r\n  <!-- //////////////////// WS-Addressing //////////////////// -->\r\n  <!-- Endpoint reference -->\r\n  <xs:element name=\"EndpointReference\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:complexType name=\"EndpointReferenceType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"Address\" type=\"wsa:AttributedURI\"/>\r\n      <xs:element name=\"ReferenceProperties\" type=\"wsa:ReferencePropertiesType\" minOccurs=\"0\"/>\r\n      <xs:element name=\"ReferenceParameters\" type=\"wsa:ReferenceParametersType\" minOccurs=\"0\"/>\r\n      <xs:element name=\"PortType\" type=\"wsa:AttributedQName\" minOccurs=\"0\"/>\r\n      <xs:element name=\"ServiceName\" type=\"wsa:ServiceNameType\" minOccurs=\"0\"/>\r\n      <xs:any namespace=\"##other\" processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n\t\t\t\t\t If \"Policy\" elements from namespace \"http://schemas.xmlsoap.org/ws/2002/12/policy#policy\" are used, they must appear first (before any extensibility elements).\r\n\t\t\t\t\t</xs:documentation>\r\n        </xs:annotation>\r\n      </xs:any>\r\n    </xs:sequence>\r\n    <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ReferencePropertiesType\">\r\n    <xs:sequence>\r\n      <xs:any processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ReferenceParametersType\">\r\n    <xs:sequence>\r\n      <xs:any processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ServiceNameType\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:QName\">\r\n        <xs:attribute name=\"PortName\" type=\"xs:NCName\"/>\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <!-- Message information header blocks -->\r\n  <xs:element name=\"MessageID\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"RelatesTo\" type=\"wsa:Relationship\"/>\r\n  <xs:element name=\"To\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"Action\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"From\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:element name=\"ReplyTo\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:element name=\"FaultTo\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:complexType name=\"Relationship\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:anyURI\">\r\n        <xs:attribute name=\"RelationshipType\" type=\"xs:QName\" use=\"optional\"/>\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:simpleType name=\"RelationshipTypeValues\">\r\n    <xs:restriction base=\"xs:QName\">\r\n      <xs:enumeration value=\"wsa:Reply\"/>\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:element name=\"ReplyAfter\" type=\"wsa:ReplyAfterType\"/>\r\n  <xs:complexType name=\"ReplyAfterType\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:nonNegativeInteger\">\r\n        <xs:anyAttribute namespace=\"##other\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:simpleType name=\"FaultSubcodeValues\">\r\n    <xs:restriction base=\"xs:QName\">\r\n      <xs:enumeration value=\"wsa:InvalidMessageInformationHeader\"/>\r\n      <xs:enumeration value=\"wsa:MessageInformationHeaderRequired\"/>\r\n      <xs:enumeration value=\"wsa:DestinationUnreachable\"/>\r\n      <xs:enumeration value=\"wsa:ActionNotSupported\"/>\r\n      <xs:enumeration value=\"wsa:EndpointUnavailable\"/>\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:attribute name=\"Action\" type=\"xs:anyURI\"/>\r\n  <!-- Common declarations and definitions -->\r\n  <xs:complexType name=\"AttributedQName\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:QName\">\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"AttributedURI\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:anyURI\">\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n</xs:schema>";

        private EndpointAddressAugust2004()
        {
            this.address = null;
        }

        private EndpointAddressAugust2004(EndpointAddress address)
        {
            this.address = address;
        }

        public static EndpointAddressAugust2004 FromEndpointAddress(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return new EndpointAddressAugust2004(address);
        }

        private static XmlSchema GetEprSchema()
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader("<xs:schema targetNamespace=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:wsa=\"http://schemas.xmlsoap.org/ws/2004/08/addressing\" elementFormDefault=\"qualified\" blockDefault=\"#all\">\r\n  <!-- //////////////////// WS-Addressing //////////////////// -->\r\n  <!-- Endpoint reference -->\r\n  <xs:element name=\"EndpointReference\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:complexType name=\"EndpointReferenceType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"Address\" type=\"wsa:AttributedURI\"/>\r\n      <xs:element name=\"ReferenceProperties\" type=\"wsa:ReferencePropertiesType\" minOccurs=\"0\"/>\r\n      <xs:element name=\"ReferenceParameters\" type=\"wsa:ReferenceParametersType\" minOccurs=\"0\"/>\r\n      <xs:element name=\"PortType\" type=\"wsa:AttributedQName\" minOccurs=\"0\"/>\r\n      <xs:element name=\"ServiceName\" type=\"wsa:ServiceNameType\" minOccurs=\"0\"/>\r\n      <xs:any namespace=\"##other\" processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n        <xs:annotation>\r\n          <xs:documentation>\r\n\t\t\t\t\t If \"Policy\" elements from namespace \"http://schemas.xmlsoap.org/ws/2002/12/policy#policy\" are used, they must appear first (before any extensibility elements).\r\n\t\t\t\t\t</xs:documentation>\r\n        </xs:annotation>\r\n      </xs:any>\r\n    </xs:sequence>\r\n    <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ReferencePropertiesType\">\r\n    <xs:sequence>\r\n      <xs:any processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ReferenceParametersType\">\r\n    <xs:sequence>\r\n      <xs:any processContents=\"lax\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"ServiceNameType\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:QName\">\r\n        <xs:attribute name=\"PortName\" type=\"xs:NCName\"/>\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <!-- Message information header blocks -->\r\n  <xs:element name=\"MessageID\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"RelatesTo\" type=\"wsa:Relationship\"/>\r\n  <xs:element name=\"To\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"Action\" type=\"wsa:AttributedURI\"/>\r\n  <xs:element name=\"From\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:element name=\"ReplyTo\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:element name=\"FaultTo\" type=\"wsa:EndpointReferenceType\"/>\r\n  <xs:complexType name=\"Relationship\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:anyURI\">\r\n        <xs:attribute name=\"RelationshipType\" type=\"xs:QName\" use=\"optional\"/>\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:simpleType name=\"RelationshipTypeValues\">\r\n    <xs:restriction base=\"xs:QName\">\r\n      <xs:enumeration value=\"wsa:Reply\"/>\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:element name=\"ReplyAfter\" type=\"wsa:ReplyAfterType\"/>\r\n  <xs:complexType name=\"ReplyAfterType\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:nonNegativeInteger\">\r\n        <xs:anyAttribute namespace=\"##other\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:simpleType name=\"FaultSubcodeValues\">\r\n    <xs:restriction base=\"xs:QName\">\r\n      <xs:enumeration value=\"wsa:InvalidMessageInformationHeader\"/>\r\n      <xs:enumeration value=\"wsa:MessageInformationHeaderRequired\"/>\r\n      <xs:enumeration value=\"wsa:DestinationUnreachable\"/>\r\n      <xs:enumeration value=\"wsa:ActionNotSupported\"/>\r\n      <xs:enumeration value=\"wsa:EndpointUnavailable\"/>\r\n    </xs:restriction>\r\n  </xs:simpleType>\r\n  <xs:attribute name=\"Action\" type=\"xs:anyURI\"/>\r\n  <!-- Common declarations and definitions -->\r\n  <xs:complexType name=\"AttributedQName\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:QName\">\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n  <xs:complexType name=\"AttributedURI\">\r\n    <xs:simpleContent>\r\n      <xs:extension base=\"xs:anyURI\">\r\n        <xs:anyAttribute namespace=\"##other\" processContents=\"lax\"/>\r\n      </xs:extension>\r\n    </xs:simpleContent>\r\n  </xs:complexType>\r\n</xs:schema>")))
            {
                return XmlSchema.Read(reader, null);
            }
        }

        public static XmlQualifiedName GetSchema(XmlSchemaSet xmlSchemaSet)
        {
            if (xmlSchemaSet == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlSchemaSet");
            }
            XmlQualifiedName eprType = EprType;
            XmlSchema eprSchema = GetEprSchema();
            ICollection is2 = xmlSchemaSet.Schemas("http://schemas.xmlsoap.org/ws/2004/08/addressing");
            if ((is2 == null) || (is2.Count == 0))
            {
                xmlSchemaSet.Add(eprSchema);
                return eprType;
            }
            XmlSchema schema = null;
            foreach (XmlSchema schema3 in is2)
            {
                if (schema3.SchemaTypes.Contains(eprType))
                {
                    schema = null;
                    break;
                }
                schema = schema3;
            }
            if (schema != null)
            {
                foreach (XmlQualifiedName name2 in eprSchema.Namespaces.ToArray())
                {
                    schema.Namespaces.Add(name2.Name, name2.Namespace);
                }
                foreach (XmlSchemaObject obj2 in eprSchema.Items)
                {
                    schema.Items.Add(obj2);
                }
                xmlSchemaSet.Reprocess(schema);
            }
            return eprType;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressingAugust2004, XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.address.WriteContentsTo(AddressingVersion.WSAddressingAugust2004, XmlDictionaryWriter.CreateDictionaryWriter(writer));
        }

        public EndpointAddress ToEndpointAddress()
        {
            return this.address;
        }

        private static XmlQualifiedName EprType
        {
            get
            {
                if (eprType == null)
                {
                    eprType = new XmlQualifiedName("EndpointReferenceType", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                }
                return eprType;
            }
        }
    }
}

