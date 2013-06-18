namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Xml;

    internal sealed class Signature
    {
        private string id;
        private SecurityKeyIdentifier keyIdentifier;
        private string prefix = "";
        private readonly SignatureValueElement signatureValueElement = new SignatureValueElement();
        private readonly System.IdentityModel.SignedInfo signedInfo;
        private SignedXml signedXml;

        public Signature(SignedXml signedXml, System.IdentityModel.SignedInfo signedInfo)
        {
            this.signedXml = signedXml;
            this.signedInfo = signedInfo;
        }

        public byte[] GetSignatureBytes()
        {
            return this.signatureValueElement.Value;
        }

        public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Signature, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            this.Id = reader.GetAttribute(dictionaryManager.UtilityDictionary.IdAttribute, null);
            reader.Read();
            this.signedInfo.ReadFrom(reader, this.signedXml.TransformFactory, dictionaryManager);
            this.signatureValueElement.ReadFrom(reader, dictionaryManager);
            if (this.signedXml.SecurityTokenSerializer.CanReadKeyIdentifier(reader))
            {
                this.keyIdentifier = this.signedXml.SecurityTokenSerializer.ReadKeyIdentifier(reader);
            }
            reader.ReadEndElement();
        }

        public void SetSignatureValue(byte[] signatureValue)
        {
            this.signatureValueElement.Value = signatureValue;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Signature, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (this.id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
            }
            this.signedInfo.WriteTo(writer, dictionaryManager);
            this.signatureValueElement.WriteTo(writer, dictionaryManager);
            if (this.keyIdentifier != null)
            {
                this.signedXml.SecurityTokenSerializer.WriteKeyIdentifier(writer, this.keyIdentifier);
            }
            writer.WriteEndElement();
        }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get
            {
                return this.keyIdentifier;
            }
            set
            {
                this.keyIdentifier = value;
            }
        }

        public ISignatureValueSecurityElement SignatureValue
        {
            get
            {
                return this.signatureValueElement;
            }
        }

        public System.IdentityModel.SignedInfo SignedInfo
        {
            get
            {
                return this.signedInfo;
            }
        }

        private sealed class SignatureValueElement : ISignatureValueSecurityElement, ISecurityElement
        {
            private string id;
            private string prefix = "";
            private string signatureText;
            private byte[] signatureValue;

            public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
            {
                reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.SignatureValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                this.prefix = reader.Prefix;
                this.Id = reader.GetAttribute("Id", null);
                reader.Read();
                this.signatureText = reader.ReadString();
                this.signatureValue = Convert.FromBase64String(this.signatureText.Trim());
                reader.ReadEndElement();
            }

            byte[] ISignatureValueSecurityElement.GetSignatureValue()
            {
                return this.Value;
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.SignatureValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                if (this.id != null)
                {
                    writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
                }
                if (this.signatureText != null)
                {
                    writer.WriteString(this.signatureText);
                }
                else
                {
                    writer.WriteBase64(this.signatureValue, 0, this.signatureValue.Length);
                }
                writer.WriteEndElement();
            }

            public bool HasId
            {
                get
                {
                    return true;
                }
            }

            public string Id
            {
                get
                {
                    return this.id;
                }
                set
                {
                    this.id = value;
                }
            }

            internal byte[] Value
            {
                get
                {
                    return this.signatureValue;
                }
                set
                {
                    this.signatureValue = value;
                    this.signatureText = null;
                }
            }
        }
    }
}

