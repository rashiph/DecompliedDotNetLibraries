namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class EncryptedType : ISecurityElement
    {
        internal static readonly XmlDictionaryString CipherDataElementName = System.ServiceModel.XD.XmlEncryptionDictionary.CipherData;
        internal static readonly XmlDictionaryString CipherValueElementName = System.ServiceModel.XD.XmlEncryptionDictionary.CipherValue;
        private string encoding;
        internal static readonly XmlDictionaryString EncodingAttribute = System.ServiceModel.XD.XmlEncryptionDictionary.Encoding;
        private EncryptionMethodElement encryptionMethod;
        private string id;
        private SecurityKeyIdentifier keyIdentifier;
        private string mimeType;
        internal static readonly XmlDictionaryString MimeTypeAttribute = System.ServiceModel.XD.XmlEncryptionDictionary.MimeType;
        internal static readonly XmlDictionaryString NamespaceUri = System.ServiceModel.XD.XmlEncryptionDictionary.Namespace;
        private EncryptionState state;
        private System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer;
        private string type;
        internal static readonly XmlDictionaryString TypeAttribute = System.ServiceModel.XD.XmlEncryptionDictionary.Type;
        private string wsuId;

        protected EncryptedType()
        {
            this.encryptionMethod.Init();
            this.state = EncryptionState.New;
            this.tokenSerializer = SecurityStandardsManager.DefaultInstance.SecurityTokenSerializer;
        }

        protected abstract void ForceEncryption();
        protected virtual void ReadAdditionalAttributes(XmlDictionaryReader reader)
        {
        }

        protected virtual void ReadAdditionalElements(XmlDictionaryReader reader)
        {
        }

        protected abstract void ReadCipherData(XmlDictionaryReader reader);
        protected abstract void ReadCipherData(XmlDictionaryReader reader, long maxBufferSize);
        public void ReadFrom(XmlDictionaryReader reader)
        {
            this.ReadFrom(reader, 0L);
        }

        public void ReadFrom(XmlReader reader)
        {
            this.ReadFrom(reader, 0L);
        }

        public void ReadFrom(XmlDictionaryReader reader, long maxBufferSize)
        {
            this.ValidateReadState();
            reader.MoveToStartElement(this.OpeningElementName, NamespaceUri);
            this.encoding = reader.GetAttribute(EncodingAttribute, null);
            this.id = reader.GetAttribute(System.ServiceModel.XD.XmlEncryptionDictionary.Id, null) ?? System.ServiceModel.Security.SecurityUniqueId.Create().Value;
            this.wsuId = reader.GetAttribute(System.ServiceModel.XD.XmlEncryptionDictionary.Id, System.ServiceModel.XD.UtilityDictionary.Namespace) ?? System.ServiceModel.Security.SecurityUniqueId.Create().Value;
            this.mimeType = reader.GetAttribute(MimeTypeAttribute, null);
            this.type = reader.GetAttribute(TypeAttribute, null);
            this.ReadAdditionalAttributes(reader);
            reader.Read();
            if (reader.IsStartElement(EncryptionMethodElement.ElementName, NamespaceUri))
            {
                this.encryptionMethod.ReadFrom(reader);
            }
            if (this.tokenSerializer.CanReadKeyIdentifier(reader))
            {
                this.KeyIdentifier = this.tokenSerializer.ReadKeyIdentifier(reader);
            }
            reader.ReadStartElement(CipherDataElementName, NamespaceUri);
            reader.ReadStartElement(CipherValueElementName, NamespaceUri);
            if (maxBufferSize == 0L)
            {
                this.ReadCipherData(reader);
            }
            else
            {
                this.ReadCipherData(reader, maxBufferSize);
            }
            reader.ReadEndElement();
            reader.ReadEndElement();
            this.ReadAdditionalElements(reader);
            reader.ReadEndElement();
            this.State = EncryptionState.Read;
        }

        public void ReadFrom(XmlReader reader, long maxBufferSize)
        {
            this.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader), maxBufferSize);
        }

        private void ValidateReadState()
        {
            if (this.State != EncryptionState.New)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadEncryptionState")));
            }
        }

        private void ValidateWriteState()
        {
            if (this.State == EncryptionState.EncryptionSetup)
            {
                this.ForceEncryption();
            }
            else if (this.State == EncryptionState.New)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadEncryptionState")));
            }
        }

        protected virtual void WriteAdditionalAttributes(XmlDictionaryWriter writer)
        {
        }

        protected virtual void WriteAdditionalElements(XmlDictionaryWriter writer)
        {
        }

        protected abstract void WriteCipherData(XmlDictionaryWriter writer);
        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            if (writer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            this.ValidateWriteState();
            writer.WriteStartElement("e", this.OpeningElementName, NamespaceUri);
            if ((this.id != null) && (this.id.Length != 0))
            {
                writer.WriteAttributeString(System.ServiceModel.XD.XmlEncryptionDictionary.Id, null, this.Id);
            }
            if (this.type != null)
            {
                writer.WriteAttributeString(TypeAttribute, null, this.Type);
            }
            if (this.mimeType != null)
            {
                writer.WriteAttributeString(MimeTypeAttribute, null, this.MimeType);
            }
            if (this.encoding != null)
            {
                writer.WriteAttributeString(EncodingAttribute, null, this.Encoding);
            }
            this.WriteAdditionalAttributes(writer);
            if (this.encryptionMethod.algorithm != null)
            {
                this.encryptionMethod.WriteTo(writer);
            }
            if (this.KeyIdentifier != null)
            {
                this.tokenSerializer.WriteKeyIdentifier(writer, this.KeyIdentifier);
            }
            writer.WriteStartElement(CipherDataElementName, NamespaceUri);
            writer.WriteStartElement(CipherValueElementName, NamespaceUri);
            this.WriteCipherData(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
            this.WriteAdditionalElements(writer);
            writer.WriteEndElement();
        }

        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        public string EncryptionMethod
        {
            get
            {
                return this.encryptionMethod.algorithm;
            }
            set
            {
                this.encryptionMethod.algorithm = value;
            }
        }

        public XmlDictionaryString EncryptionMethodDictionaryString
        {
            get
            {
                return this.encryptionMethod.algorithmDictionaryString;
            }
            set
            {
                this.encryptionMethod.algorithmDictionaryString = value;
            }
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

        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
            set
            {
                this.mimeType = value;
            }
        }

        protected abstract XmlDictionaryString OpeningElementName { get; }

        public System.IdentityModel.Selectors.SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
            set
            {
                this.tokenSerializer = value ?? WSSecurityTokenSerializer.DefaultInstance;
            }
        }

        protected EncryptionState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string WsuId
        {
            get
            {
                return this.wsuId;
            }
            set
            {
                this.wsuId = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EncryptionMethodElement
        {
            internal string algorithm;
            internal XmlDictionaryString algorithmDictionaryString;
            internal static readonly XmlDictionaryString ElementName;
            public void Init()
            {
                this.algorithm = null;
            }

            public void ReadFrom(XmlDictionaryReader reader)
            {
                reader.MoveToStartElement(ElementName, System.ServiceModel.XD.XmlEncryptionDictionary.Namespace);
                bool isEmptyElement = reader.IsEmptyElement;
                this.algorithm = reader.GetAttribute(System.ServiceModel.XD.XmlSignatureDictionary.Algorithm, null);
                if (this.algorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { System.ServiceModel.XD.XmlSignatureDictionary.Algorithm.Value, ElementName.Value })));
                }
                reader.Read();
                if (!isEmptyElement)
                {
                    while (reader.IsStartElement())
                    {
                        reader.Skip();
                    }
                    reader.ReadEndElement();
                }
            }

            public void WriteTo(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("e", ElementName, System.ServiceModel.XD.XmlEncryptionDictionary.Namespace);
                if (this.algorithmDictionaryString != null)
                {
                    writer.WriteStartAttribute(System.ServiceModel.XD.XmlSignatureDictionary.Algorithm, null);
                    writer.WriteString(this.algorithmDictionaryString);
                    writer.WriteEndAttribute();
                }
                else
                {
                    writer.WriteAttributeString(System.ServiceModel.XD.XmlSignatureDictionary.Algorithm, null, this.algorithm);
                }
                if (this.algorithm == System.ServiceModel.XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap.Value)
                {
                    writer.WriteStartElement("", System.ServiceModel.XD.XmlSignatureDictionary.DigestMethod, System.ServiceModel.XD.XmlSignatureDictionary.Namespace);
                    writer.WriteStartAttribute(System.ServiceModel.XD.XmlSignatureDictionary.Algorithm, null);
                    writer.WriteString(System.ServiceModel.XD.SecurityAlgorithmDictionary.Sha1Digest);
                    writer.WriteEndAttribute();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            static EncryptionMethodElement()
            {
                ElementName = System.ServiceModel.XD.XmlEncryptionDictionary.EncryptionMethod;
            }
        }

        protected enum EncryptionState
        {
            New,
            Read,
            DecryptionSetup,
            Decrypted,
            EncryptionSetup,
            Encrypted
        }
    }
}

