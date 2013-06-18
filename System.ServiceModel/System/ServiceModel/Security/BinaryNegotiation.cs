namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class BinaryNegotiation
    {
        private byte[] negotiationData;
        private string valueTypeUri;
        private XmlDictionaryString valueTypeUriDictionaryString;

        public BinaryNegotiation(string valueTypeUri, byte[] negotiationData)
        {
            if (valueTypeUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("valueTypeUri");
            }
            if (negotiationData == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiationData");
            }
            this.valueTypeUriDictionaryString = null;
            this.valueTypeUri = valueTypeUri;
            this.negotiationData = negotiationData;
        }

        public BinaryNegotiation(XmlDictionaryString valueTypeDictionaryString, byte[] negotiationData)
        {
            if (valueTypeDictionaryString == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("valueTypeDictionaryString");
            }
            if (negotiationData == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiationData");
            }
            this.valueTypeUriDictionaryString = valueTypeDictionaryString;
            this.valueTypeUri = valueTypeDictionaryString.Value;
            this.negotiationData = negotiationData;
        }

        public byte[] GetNegotiationData()
        {
            return this.negotiationData;
        }

        public void Validate(XmlDictionaryString valueTypeUriDictionaryString)
        {
            if (this.valueTypeUri != valueTypeUriDictionaryString.Value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(System.ServiceModel.SR.GetString("IncorrectBinaryNegotiationValueType", new object[] { this.valueTypeUri })));
            }
            this.valueTypeUriDictionaryString = valueTypeUriDictionaryString;
        }

        public void WriteTo(XmlDictionaryWriter writer, string prefix, XmlDictionaryString localName, XmlDictionaryString ns, XmlDictionaryString valueTypeLocalName, XmlDictionaryString valueTypeNs)
        {
            writer.WriteStartElement(prefix, localName, ns);
            writer.WriteStartAttribute(valueTypeLocalName, valueTypeNs);
            if (this.valueTypeUriDictionaryString != null)
            {
                writer.WriteString(this.valueTypeUriDictionaryString);
            }
            else
            {
                writer.WriteString(this.valueTypeUri);
            }
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(XD.SecurityJan2004Dictionary.EncodingType, null);
            writer.WriteString(XD.SecurityJan2004Dictionary.EncodingTypeValueBase64Binary);
            writer.WriteEndAttribute();
            writer.WriteBase64(this.negotiationData, 0, this.negotiationData.Length);
            writer.WriteEndElement();
        }

        public string ValueTypeUri
        {
            get
            {
                return this.valueTypeUri;
            }
        }
    }
}

