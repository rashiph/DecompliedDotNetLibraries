namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SecurityAlgorithmDec2005Dictionary
    {
        public XmlDictionaryString Psha1KeyDerivationDec2005;

        public SecurityAlgorithmDec2005Dictionary(XmlDictionary dictionary)
        {
            this.Psha1KeyDerivationDec2005 = dictionary.Add("http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1");
        }
    }
}

