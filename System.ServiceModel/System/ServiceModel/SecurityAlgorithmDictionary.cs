namespace System.ServiceModel
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

        public SecurityAlgorithmDictionary(ServiceModelDictionary dictionary)
        {
            this.Aes128Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes128-cbc", 0x8a);
            this.Aes128KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes128", 0x8b);
            this.Aes192Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes192-cbc", 140);
            this.Aes192KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes192", 0x8d);
            this.Aes256Encryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#aes256-cbc", 0x8e);
            this.Aes256KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-aes256", 0x8f);
            this.DesEncryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#des-cbc", 0x90);
            this.DsaSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#dsa-sha1", 0x91);
            this.ExclusiveC14n = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#", 0x6f);
            this.ExclusiveC14nWithComments = dictionary.CreateString("http://www.w3.org/2001/10/xml-exc-c14n#WithComments", 0x92);
            this.HmacSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#hmac-sha1", 0x93);
            this.HmacSha256Signature = dictionary.CreateString("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", 0x94);
            this.Psha1KeyDerivation = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1", 0x95);
            this.Ripemd160Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#ripemd160", 150);
            this.RsaOaepKeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p", 0x97);
            this.RsaSha1Signature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#rsa-sha1", 0x98);
            this.RsaSha256Signature = dictionary.CreateString("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", 0x99);
            this.RsaV15KeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#rsa-1_5", 0x9a);
            this.Sha1Digest = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#sha1", 0x9b);
            this.Sha256Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#sha256", 0x9c);
            this.Sha512Digest = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#sha512", 0x9d);
            this.TripleDesEncryption = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#tripledes-cbc", 0x9e);
            this.TripleDesKeyWrap = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#kw-tripledes", 0x9f);
            this.TlsSspiKeyWrap = dictionary.CreateString("http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap", 160);
            this.WindowsSspiKeyWrap = dictionary.CreateString("http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap", 0xa1);
        }
    }
}

