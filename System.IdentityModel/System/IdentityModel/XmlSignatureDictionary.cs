namespace System.IdentityModel
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

        public XmlSignatureDictionary(IdentityModelDictionary dictionary)
        {
            this.Algorithm = dictionary.CreateString("Algorithm", 0);
            this.URI = dictionary.CreateString("URI", 1);
            this.Reference = dictionary.CreateString("Reference", 2);
            this.Transforms = dictionary.CreateString("Transforms", 4);
            this.Transform = dictionary.CreateString("Transform", 5);
            this.DigestMethod = dictionary.CreateString("DigestMethod", 6);
            this.DigestValue = dictionary.CreateString("DigestValue", 7);
            this.Namespace = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#", 8);
            this.EnvelopedSignature = dictionary.CreateString("http://www.w3.org/2000/09/xmldsig#enveloped-signature", 9);
            this.KeyInfo = dictionary.CreateString("KeyInfo", 10);
            this.Signature = dictionary.CreateString("Signature", 11);
            this.SignedInfo = dictionary.CreateString("SignedInfo", 12);
            this.CanonicalizationMethod = dictionary.CreateString("CanonicalizationMethod", 13);
            this.SignatureMethod = dictionary.CreateString("SignatureMethod", 14);
            this.SignatureValue = dictionary.CreateString("SignatureValue", 15);
            this.KeyName = dictionary.CreateString("KeyName", 0x52);
            this.Type = dictionary.CreateString("Type", 0x53);
            this.MgmtData = dictionary.CreateString("MgmtData", 0x54);
            this.Prefix = dictionary.CreateString("", 0x55);
            this.KeyValue = dictionary.CreateString("KeyValue", 0x56);
            this.RsaKeyValue = dictionary.CreateString("RSAKeyValue", 0x57);
            this.Modulus = dictionary.CreateString("Modulus", 0x58);
            this.Exponent = dictionary.CreateString("Exponent", 0x59);
            this.X509Data = dictionary.CreateString("X509Data", 90);
            this.X509IssuerSerial = dictionary.CreateString("X509IssuerSerial", 0x5b);
            this.X509IssuerName = dictionary.CreateString("X509IssuerName", 0x5c);
            this.X509SerialNumber = dictionary.CreateString("X509SerialNumber", 0x5d);
            this.X509Certificate = dictionary.CreateString("X509Certificate", 0x5e);
        }

        public XmlSignatureDictionary(IXmlDictionary dictionary)
        {
            this.Algorithm = this.LookupDictionaryString(dictionary, "Algorithm");
            this.URI = this.LookupDictionaryString(dictionary, "URI");
            this.Reference = this.LookupDictionaryString(dictionary, "Reference");
            this.Transforms = this.LookupDictionaryString(dictionary, "Transforms");
            this.Transform = this.LookupDictionaryString(dictionary, "Transform");
            this.DigestMethod = this.LookupDictionaryString(dictionary, "DigestMethod");
            this.DigestValue = this.LookupDictionaryString(dictionary, "DigestValue");
            this.Namespace = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#");
            this.EnvelopedSignature = this.LookupDictionaryString(dictionary, "http://www.w3.org/2000/09/xmldsig#enveloped-signature");
            this.KeyInfo = this.LookupDictionaryString(dictionary, "KeyInfo");
            this.Signature = this.LookupDictionaryString(dictionary, "Signature");
            this.SignedInfo = this.LookupDictionaryString(dictionary, "SignedInfo");
            this.CanonicalizationMethod = this.LookupDictionaryString(dictionary, "CanonicalizationMethod");
            this.SignatureMethod = this.LookupDictionaryString(dictionary, "SignatureMethod");
            this.SignatureValue = this.LookupDictionaryString(dictionary, "SignatureValue");
            this.KeyName = this.LookupDictionaryString(dictionary, "KeyName");
            this.Type = this.LookupDictionaryString(dictionary, "Type");
            this.MgmtData = this.LookupDictionaryString(dictionary, "MgmtData");
            this.Prefix = this.LookupDictionaryString(dictionary, "");
            this.KeyValue = this.LookupDictionaryString(dictionary, "KeyValue");
            this.RsaKeyValue = this.LookupDictionaryString(dictionary, "RSAKeyValue");
            this.Modulus = this.LookupDictionaryString(dictionary, "Modulus");
            this.Exponent = this.LookupDictionaryString(dictionary, "Exponent");
            this.X509Data = this.LookupDictionaryString(dictionary, "X509Data");
            this.X509IssuerSerial = this.LookupDictionaryString(dictionary, "X509IssuerSerial");
            this.X509IssuerName = this.LookupDictionaryString(dictionary, "X509IssuerName");
            this.X509SerialNumber = this.LookupDictionaryString(dictionary, "X509SerialNumber");
            this.X509Certificate = this.LookupDictionaryString(dictionary, "X509Certificate");
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

