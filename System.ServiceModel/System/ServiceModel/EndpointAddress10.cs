namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlSchemaProvider("GetSchema"), XmlRoot("EndpointReference", Namespace="http://www.w3.org/2005/08/addressing")]
    public class EndpointAddress10 : IXmlSerializable
    {
        private EndpointAddress address;
        private static XmlQualifiedName eprType;
        private const string Schema = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:wsa='http://www.w3.org/2005/08/addressing' targetNamespace='http://www.w3.org/2005/08/addressing' blockDefault='#all' elementFormDefault='qualified' finalDefault='' attributeFormDefault='unqualified'>\r\n    \r\n    <!-- Constructs from the WS-Addressing Core -->\r\n\r\n    <xs:element name='EndpointReference' type='wsa:EndpointReferenceType'/>\r\n    <xs:complexType name='EndpointReferenceType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:element name='Address' type='wsa:AttributedURIType'/>\r\n            <xs:element name='ReferenceParameters' type='wsa:ReferenceParametersType' minOccurs='0'/>\r\n            <xs:element ref='wsa:Metadata' minOccurs='0'/>\r\n            <xs:any namespace='##other' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:complexType name='ReferenceParametersType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='Metadata' type='wsa:MetadataType'/>\r\n    <xs:complexType name='MetadataType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='MessageID' type='wsa:AttributedURIType'/>\r\n    <xs:element name='RelatesTo' type='wsa:RelatesToType'/>\r\n    <xs:complexType name='RelatesToType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:anyURI'>\r\n                <xs:attribute name='RelationshipType' type='wsa:RelationshipTypeOpenEnum' use='optional' default='http://www.w3.org/2005/08/addressing/reply'/>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:simpleType name='RelationshipTypeOpenEnum'>\r\n        <xs:union memberTypes='wsa:RelationshipType xs:anyURI'/>\r\n    </xs:simpleType>\r\n    \r\n    <xs:simpleType name='RelationshipType'>\r\n        <xs:restriction base='xs:anyURI'>\r\n            <xs:enumeration value='http://www.w3.org/2005/08/addressing/reply'/>\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n    \r\n    <xs:element name='ReplyTo' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='From' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='FaultTo' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='To' type='wsa:AttributedURIType'/>\r\n    <xs:element name='Action' type='wsa:AttributedURIType'/>\r\n\r\n    <xs:complexType name='AttributedURIType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:anyURI'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <!-- Constructs from the WS-Addressing SOAP binding -->\r\n\r\n    <xs:attribute name='IsReferenceParameter' type='xs:boolean'/>\r\n    \r\n    <xs:simpleType name='FaultCodesOpenEnumType'>\r\n        <xs:union memberTypes='wsa:FaultCodesType xs:QName'/>\r\n    </xs:simpleType>\r\n    \r\n    <xs:simpleType name='FaultCodesType'>\r\n        <xs:restriction base='xs:QName'>\r\n            <xs:enumeration value='wsa:InvalidAddressingHeader'/>\r\n            <xs:enumeration value='wsa:InvalidAddress'/>\r\n            <xs:enumeration value='wsa:InvalidEPR'/>\r\n            <xs:enumeration value='wsa:InvalidCardinality'/>\r\n            <xs:enumeration value='wsa:MissingAddressInEPR'/>\r\n            <xs:enumeration value='wsa:DuplicateMessageID'/>\r\n            <xs:enumeration value='wsa:ActionMismatch'/>\r\n            <xs:enumeration value='wsa:MessageAddressingHeaderRequired'/>\r\n            <xs:enumeration value='wsa:DestinationUnreachable'/>\r\n            <xs:enumeration value='wsa:ActionNotSupported'/>\r\n            <xs:enumeration value='wsa:EndpointUnavailable'/>\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n    \r\n    <xs:element name='RetryAfter' type='wsa:AttributedUnsignedLongType'/>\r\n    <xs:complexType name='AttributedUnsignedLongType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:unsignedLong'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemHeaderQName' type='wsa:AttributedQNameType'/>\r\n    <xs:complexType name='AttributedQNameType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:QName'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemHeader' type='wsa:AttributedAnyType'/>\r\n    <xs:complexType name='AttributedAnyType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='1' maxOccurs='1'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemIRI' type='wsa:AttributedURIType'/>\r\n    \r\n    <xs:element name='ProblemAction' type='wsa:ProblemActionType'/>\r\n    <xs:complexType name='ProblemActionType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:element ref='wsa:Action' minOccurs='0'/>\r\n            <xs:element name='SoapAction' minOccurs='0' type='xs:anyURI'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n</xs:schema>";

        private EndpointAddress10()
        {
            this.address = null;
        }

        private EndpointAddress10(EndpointAddress address)
        {
            this.address = address;
        }

        public static EndpointAddress10 FromEndpointAddress(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return new EndpointAddress10(address);
        }

        private static XmlSchema GetEprSchema()
        {
            using (XmlTextReader reader = new XmlTextReader(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:wsa='http://www.w3.org/2005/08/addressing' targetNamespace='http://www.w3.org/2005/08/addressing' blockDefault='#all' elementFormDefault='qualified' finalDefault='' attributeFormDefault='unqualified'>\r\n    \r\n    <!-- Constructs from the WS-Addressing Core -->\r\n\r\n    <xs:element name='EndpointReference' type='wsa:EndpointReferenceType'/>\r\n    <xs:complexType name='EndpointReferenceType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:element name='Address' type='wsa:AttributedURIType'/>\r\n            <xs:element name='ReferenceParameters' type='wsa:ReferenceParametersType' minOccurs='0'/>\r\n            <xs:element ref='wsa:Metadata' minOccurs='0'/>\r\n            <xs:any namespace='##other' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:complexType name='ReferenceParametersType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='Metadata' type='wsa:MetadataType'/>\r\n    <xs:complexType name='MetadataType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='MessageID' type='wsa:AttributedURIType'/>\r\n    <xs:element name='RelatesTo' type='wsa:RelatesToType'/>\r\n    <xs:complexType name='RelatesToType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:anyURI'>\r\n                <xs:attribute name='RelationshipType' type='wsa:RelationshipTypeOpenEnum' use='optional' default='http://www.w3.org/2005/08/addressing/reply'/>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:simpleType name='RelationshipTypeOpenEnum'>\r\n        <xs:union memberTypes='wsa:RelationshipType xs:anyURI'/>\r\n    </xs:simpleType>\r\n    \r\n    <xs:simpleType name='RelationshipType'>\r\n        <xs:restriction base='xs:anyURI'>\r\n            <xs:enumeration value='http://www.w3.org/2005/08/addressing/reply'/>\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n    \r\n    <xs:element name='ReplyTo' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='From' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='FaultTo' type='wsa:EndpointReferenceType'/>\r\n    <xs:element name='To' type='wsa:AttributedURIType'/>\r\n    <xs:element name='Action' type='wsa:AttributedURIType'/>\r\n\r\n    <xs:complexType name='AttributedURIType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:anyURI'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <!-- Constructs from the WS-Addressing SOAP binding -->\r\n\r\n    <xs:attribute name='IsReferenceParameter' type='xs:boolean'/>\r\n    \r\n    <xs:simpleType name='FaultCodesOpenEnumType'>\r\n        <xs:union memberTypes='wsa:FaultCodesType xs:QName'/>\r\n    </xs:simpleType>\r\n    \r\n    <xs:simpleType name='FaultCodesType'>\r\n        <xs:restriction base='xs:QName'>\r\n            <xs:enumeration value='wsa:InvalidAddressingHeader'/>\r\n            <xs:enumeration value='wsa:InvalidAddress'/>\r\n            <xs:enumeration value='wsa:InvalidEPR'/>\r\n            <xs:enumeration value='wsa:InvalidCardinality'/>\r\n            <xs:enumeration value='wsa:MissingAddressInEPR'/>\r\n            <xs:enumeration value='wsa:DuplicateMessageID'/>\r\n            <xs:enumeration value='wsa:ActionMismatch'/>\r\n            <xs:enumeration value='wsa:MessageAddressingHeaderRequired'/>\r\n            <xs:enumeration value='wsa:DestinationUnreachable'/>\r\n            <xs:enumeration value='wsa:ActionNotSupported'/>\r\n            <xs:enumeration value='wsa:EndpointUnavailable'/>\r\n        </xs:restriction>\r\n    </xs:simpleType>\r\n    \r\n    <xs:element name='RetryAfter' type='wsa:AttributedUnsignedLongType'/>\r\n    <xs:complexType name='AttributedUnsignedLongType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:unsignedLong'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemHeaderQName' type='wsa:AttributedQNameType'/>\r\n    <xs:complexType name='AttributedQNameType' mixed='false'>\r\n        <xs:simpleContent>\r\n            <xs:extension base='xs:QName'>\r\n                <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n            </xs:extension>\r\n        </xs:simpleContent>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemHeader' type='wsa:AttributedAnyType'/>\r\n    <xs:complexType name='AttributedAnyType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:any namespace='##any' processContents='lax' minOccurs='1' maxOccurs='1'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n    <xs:element name='ProblemIRI' type='wsa:AttributedURIType'/>\r\n    \r\n    <xs:element name='ProblemAction' type='wsa:ProblemActionType'/>\r\n    <xs:complexType name='ProblemActionType' mixed='false'>\r\n        <xs:sequence>\r\n            <xs:element ref='wsa:Action' minOccurs='0'/>\r\n            <xs:element name='SoapAction' minOccurs='0' type='xs:anyURI'/>\r\n        </xs:sequence>\r\n        <xs:anyAttribute namespace='##other' processContents='lax'/>\r\n    </xs:complexType>\r\n    \r\n</xs:schema>")))
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
            ICollection is2 = xmlSchemaSet.Schemas("http://www.w3.org/2005/08/addressing");
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
            this.address = EndpointAddress.ReadFrom(AddressingVersion.WSAddressing10, XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.address.WriteContentsTo(AddressingVersion.WSAddressing10, XmlDictionaryWriter.CreateDictionaryWriter(writer));
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
                    eprType = new XmlQualifiedName("EndpointReferenceType", "http://www.w3.org/2005/08/addressing");
                }
                return eprType;
            }
        }
    }
}

