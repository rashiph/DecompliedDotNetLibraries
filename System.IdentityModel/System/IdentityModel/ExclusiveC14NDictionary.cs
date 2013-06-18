namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class ExclusiveC14NDictionary
    {
        public XmlDictionaryString InclusiveNamespaces;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString PrefixList;

        public ExclusiveC14NDictionary(IdentityModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#", 20);
            this.PrefixList = dictionary.CreateString("PrefixList", 0x15);
            this.InclusiveNamespaces = dictionary.CreateString("InclusiveNamespaces", 0x16);
            this.Prefix = dictionary.CreateString("ec", 0x17);
        }

        public ExclusiveC14NDictionary(IXmlDictionary dictionary)
        {
            this.Namespace = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/10/xml-exc-c14n#");
            this.PrefixList = this.LookupDictionaryString(dictionary, "PrefixList");
            this.InclusiveNamespaces = this.LookupDictionaryString(dictionary, "InclusiveNamespaces");
            this.Prefix = this.LookupDictionaryString(dictionary, "ec");
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

