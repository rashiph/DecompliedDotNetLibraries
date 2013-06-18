namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class XmlSignatureDictionary
    {
        public XmlDictionaryString Algorithm;
        public XmlDictionaryString CanonicalizationMethod;
        public XmlDictionaryString DigestMethod;
        public XmlDictionaryString DigestValue;
        public XmlDictionaryString EnvelopedSignature;
        public XmlDictionaryString Exponent;
        public XmlDictionaryString KeyInfo;
        public XmlDictionaryString KeyName;
        public XmlDictionaryString KeyValue;
        public XmlDictionaryString MgmtData;
        public XmlDictionaryString Modulus;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Reference;
        public XmlDictionaryString RsaKeyValue;
        public XmlDictionaryString Signature;
        public XmlDictionaryString SignatureMethod;
        public XmlDictionaryString SignatureValue;
        public XmlDictionaryString SignedInfo;
        public XmlDictionaryString Transform;
        public XmlDictionaryString Transforms;
        public XmlDictionaryString Type;
        public XmlDictionaryString URI;
        public XmlDictionaryString X509Certificate;
        public XmlDictionaryString X509Data;
        public XmlDictionaryString X509IssuerName;
        public XmlDictionaryString X509IssuerSerial;
        public XmlDictionaryString X509SerialNumber;

        public XmlSignatureDictionary(ServiceModelDictionary dictionary)
        {
            this.Algorithm = dictionary.CreateString("Algorithm", 8);
            this.URI = dictionary.CreateString("URI", 11);
            this.Reference = dictionary.CreateString("Reference", 12);
            this.Transforms = dictionary.CreateString("Transforms", 0x11);
            this.Transform = dictionary.CreateString("Transform", 0x12);
            this.DigestMethod = dictionary.CreateString("DigestMethod", 0x13);
            this.DigestValue = dictionary.CreateString("DigestValue", 20);
            this.Namespace = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#", 0x21);
            this.EnvelopedSignature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#enveloped-signature", 0x22);
            this.KeyInfo = dictionary.CreateString("KeyInfo", 0x23);
            this.Signature = dictionary.CreateString("Signature", 0x29);
            this.SignedInfo = dictionary.CreateString("SignedInfo", 0x2a);
            this.CanonicalizationMethod = dictionary.CreateString("CanonicalizationMethod", 0x2b);
            this.SignatureMethod = dictionary.CreateString("SignatureMethod", 0x2c);
            this.SignatureValue = dictionary.CreateString("SignatureValue", 0x2d);
            this.KeyName = dictionary.CreateString("KeyName", 0x13d);
            this.Type = dictionary.CreateString("Type", 0x3b);
            this.MgmtData = dictionary.CreateString("MgmtData", 0x13e);
            this.Prefix = dictionary.CreateString("", 0x51);
            this.KeyValue = dictionary.CreateString("KeyValue", 0x13f);
            this.RsaKeyValue = dictionary.CreateString("RSAKeyValue", 320);
            this.Modulus = dictionary.CreateString("Modulus", 0x141);
            this.Exponent = dictionary.CreateString("Exponent", 0x142);
            this.X509Data = dictionary.CreateString("X509Data", 0x143);
            this.X509IssuerSerial = dictionary.CreateString("X509IssuerSerial", 0x144);
            this.X509IssuerName = dictionary.CreateString("X509IssuerName", 0x145);
            this.X509SerialNumber = dictionary.CreateString("X509SerialNumber", 0x146);
            this.X509Certificate = dictionary.CreateString("X509Certificate", 0x147);
        }
    }
}

