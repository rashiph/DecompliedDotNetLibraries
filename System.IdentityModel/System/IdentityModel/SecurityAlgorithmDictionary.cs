namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class SecurityAlgorithmDictionary
    {
        public XmlDictionaryString Aes128Encryption;
        public XmlDictionaryString Aes128KeyWrap;
        public XmlDictionaryString Aes192Encryption;
        public XmlDictionaryString Aes192KeyWrap;
        public XmlDictionaryString Aes256Encryption;
        public XmlDictionaryString Aes256KeyWrap;
        public XmlDictionaryString DesEncryption;
        public XmlDictionaryString DsaSha1Signature;
        public XmlDictionaryString ExclusiveC14n;
        public XmlDictionaryString ExclusiveC14nWithComments;
        public XmlDictionaryString HmacSha1Signature;
        public XmlDictionaryString HmacSha256Signature;
        public XmlDictionaryString Psha1KeyDerivation;
        public XmlDictionaryString Ripemd160Digest;
        public XmlDictionaryString RsaOaepKeyWrap;
        public XmlDictionaryString RsaSha1Signature;
        public XmlDictionaryString RsaSha256Signature;
        public XmlDictionaryString RsaV15KeyWrap;
        public XmlDictionaryString Sha1Digest;
        public XmlDictionaryString Sha256Digest;
        public XmlDictionaryString Sha512Digest;
        public XmlDictionaryString TlsSspiKeyWrap;
        public XmlDictionaryString TripleDesEncryption;
        public XmlDictionaryString TripleDesKeyWrap;
        public XmlDictionaryString WindowsSspiKeyWrap;

        public SecurityAlgorithmDictionary(IdentityModelDictionary dictionary)
        {
            this.Aes128Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes128-cbc", 0x5f);
            this.Aes128KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes128", 0x60);
            this.Aes192Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes192-cbc", 0x61);
            this.Aes192KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes192", 0x62);
            this.Aes256Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes256-cbc", 0x63);
            this.Aes256KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes256", 100);
            this.DesEncryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#des-cbc", 0x65);
            this.DsaSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#dsa-sha1", 0x66);
            this.ExclusiveC14n = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#", 20);
            this.ExclusiveC14nWithComments = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#WithComments", 0x67);
            this.HmacSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#hmac-sha1", 0x68);
            this.HmacSha256Signature = dictionary.CreateString("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", 0x69);
            this.Psha1KeyDerivation = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1", 0x6a);
            this.Ripemd160Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#ripemd160", 0x6b);
            this.RsaOaepKeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p", 0x6c);
            this.RsaSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#rsa-sha1", 0x6d);
            this.RsaSha256Signature = dictionary.CreateString("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", 110);
            this.RsaV15KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#rsa-1_5", 0x6f);
            this.Sha1Digest = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#sha1", 0x70);
            this.Sha256Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#sha256", 0x71);
            this.Sha512Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#sha512", 0x72);
            this.TripleDesEncryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#tripledes-cbc", 0x73);
            this.TripleDesKeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-tripledes", 0x74);
            this.TlsSspiKeyWrap = dictionary.CreateString("http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap", 0x75);
            this.WindowsSspiKeyWrap = dictionary.CreateString("http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap", 0x76);
        }

        public SecurityAlgorithmDictionary(IXmlDictionary dictionary)
        {
            this.Aes128Encryption = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#aes128-cbc");
            this.Aes128KeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#kw-aes128");
            this.Aes192Encryption = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#aes192-cbc");
            this.Aes192KeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#kw-aes192");
            this.Aes256Encryption = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#aes256-cbc");
            this.Aes256KeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#kw-aes256");
            this.DesEncryption = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#des-cbc");
            this.DsaSha1Signature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#dsa-sha1");
            this.ExclusiveC14n = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/10/xml-exc-c14n#");
            this.ExclusiveC14nWithComments = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/10/xml-exc-c14n#WithComments");
            this.HmacSha1Signature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#hmac-sha1");
            this.HmacSha256Signature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            this.Psha1KeyDerivation = this.LookupDictionaryString(dictionary, "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1");
            this.Ripemd160Digest = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#ripemd160");
            this.RsaOaepKeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p");
            this.RsaSha1Signature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
            this.RsaSha256Signature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
            this.RsaV15KeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#rsa-1_5");
            this.Sha1Digest = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#sha1");
            this.Sha256Digest = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#sha256");
            this.Sha512Digest = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#sha512");
            this.TripleDesEncryption = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#tripledes-cbc");
            this.TripleDesKeyWrap = this.LookupDictionaryString(dictionary, "http://www.w3.org/2001/04/xmlenc#kw-tripledes");
            this.TlsSspiKeyWrap = this.LookupDictionaryString(dictionary, "http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap");
            this.WindowsSspiKeyWrap = this.LookupDictionaryString(dictionary, "http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap");
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

