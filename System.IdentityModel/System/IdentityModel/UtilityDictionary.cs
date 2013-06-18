namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class UtilityDictionary
    {
        public XmlDictionaryString CreatedElement;
        public XmlDictionaryString ExpiresElement;
        public XmlDictionaryString IdAttribute;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Timestamp;

        public UtilityDictionary(IdentityModelDictionary dictionary)
        {
            this.IdAttribute = dictionary.CreateString("Id", 3);
            this.Namespace = dictionary.CreateString("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", 0x10);
            this.Timestamp = dictionary.CreateString("Timestamp", 0x11);
            this.CreatedElement = dictionary.CreateString("Created", 0x12);
            this.ExpiresElement = dictionary.CreateString("Expires", 0x13);
            this.Prefix = dictionary.CreateString("u", 0x51);
        }

        public UtilityDictionary(IXmlDictionary dictionary)
        {
            this.IdAttribute = this.LookupDictionaryString(dictionary, "Id");
            this.Namespace = this.LookupDictionaryString(dictionary, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            this.Timestamp = this.LookupDictionaryString(dictionary, "Timestamp");
            this.CreatedElement = this.LookupDictionaryString(dictionary, "Created");
            this.ExpiresElement = this.LookupDictionaryString(dictionary, "Expires");
            this.Prefix = this.LookupDictionaryString(dictionary, "u");
        }

        private XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString str;
            if (!dictionary.TryLookup(value, out str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("XDCannotFindValueInDictionaryString", new object[] { value }));
            }
            return str;
        }
    }
}

