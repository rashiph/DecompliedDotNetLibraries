namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class XmlEncryptionDictionary
    {
        public XmlDictionaryString AlgorithmAttribute;
        public XmlDictionaryString CarriedKeyName;
        public XmlDictionaryString CipherData;
        public XmlDictionaryString CipherValue;
        public XmlDictionaryString ContentType;
        public XmlDictionaryString DataReference;
        public XmlDictionaryString ElementType;
        public XmlDictionaryString Encoding;
        public XmlDictionaryString EncryptedData;
        public XmlDictionaryString EncryptedKey;
        public XmlDictionaryString EncryptionMethod;
        public XmlDictionaryString Id;
        public XmlDictionaryString KeyReference;
        public XmlDictionaryString MimeType;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Recipient;
        public XmlDictionaryString ReferenceList;
        public XmlDictionaryString Type;
        public XmlDictionaryString URI;

        public XmlEncryptionDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#", 0x25);
            this.DataReference = dictionary.CreateString("DataReference", 0x2e);
            this.EncryptedData = dictionary.CreateString("EncryptedData", 0x2f);
            this.EncryptionMethod = dictionary.CreateString("EncryptionMethod", 0x30);
            this.CipherData = dictionary.CreateString("CipherData", 0x31);
            this.CipherValue = dictionary.CreateString("CipherValue", 50);
            this.ReferenceList = dictionary.CreateString("ReferenceList", 0x39);
            this.Encoding = dictionary.CreateString("Encoding", 0x134);
            this.MimeType = dictionary.CreateString("MimeType", 0x135);
            this.Type = dictionary.CreateString("Type", 0x3b);
            this.Id = dictionary.CreateString("Id", 14);
            this.CarriedKeyName = dictionary.CreateString("CarriedKeyName", 310);
            this.Recipient = dictionary.CreateString("Recipient", 0x137);
            this.EncryptedKey = dictionary.CreateString("EncryptedKey", 0x138);
            this.URI = dictionary.CreateString("URI", 11);
            this.KeyReference = dictionary.CreateString("KeyReference", 0x139);
            this.Prefix = dictionary.CreateString("e", 0x13a);
            this.ElementType = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#Element", 0x13b);
            this.ContentType = dictionary.CreateString("http://www.w3.org/2001/04/xmlenc#Content", 0x13c);
            this.AlgorithmAttribute = dictionary.CreateString("Algorithm", 8);
        }
    }
}

