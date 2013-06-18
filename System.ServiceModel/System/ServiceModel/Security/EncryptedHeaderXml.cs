namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal sealed class EncryptedHeaderXml
    {
        private string actor;
        internal static readonly XmlDictionaryString ElementName = XD.SecurityXXX2005Dictionary.EncryptedHeader;
        private EncryptedData encryptedData;
        private string id;
        private bool mustUnderstand;
        internal static readonly XmlDictionaryString NamespaceUri = XD.SecurityXXX2005Dictionary.Namespace;
        private const string Prefix = "k";
        private bool relay;
        private MessageVersion version;

        public EncryptedHeaderXml(MessageVersion version)
        {
            this.version = version;
            this.encryptedData = new EncryptedData();
        }

        public byte[] GetDecryptedBuffer()
        {
            return this.encryptedData.GetDecryptedBuffer();
        }

        public void ReadFrom(XmlDictionaryReader reader, long maxBufferSize)
        {
            bool flag;
            reader.MoveToStartElement(ElementName, NamespaceUri);
            MessageHeader.GetHeaderAttributes(reader, this.version, out this.actor, out this.mustUnderstand, out this.relay, out flag);
            this.id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
            reader.ReadStartElement();
            this.encryptedData.ReadFrom(reader, maxBufferSize);
            reader.ReadEndElement();
        }

        public void SetUpDecryption(SymmetricAlgorithm algorithm)
        {
            this.encryptedData.SetUpDecryption(algorithm);
        }

        public void SetUpEncryption(SymmetricAlgorithm algorithm, MemoryStream source)
        {
            this.encryptedData.SetUpEncryption(algorithm, new ArraySegment<byte>(source.GetBuffer(), 0, (int) source.Length));
        }

        public void WriteHeaderContents(XmlDictionaryWriter writer)
        {
            this.encryptedData.WriteTo(writer, ServiceModelDictionaryManager.Instance);
        }

        public void WriteHeaderElement(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("k", ElementName, NamespaceUri);
        }

        public void WriteHeaderId(XmlDictionaryWriter writer)
        {
            writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, this.id);
        }

        public string Actor
        {
            get
            {
                return this.actor;
            }
            set
            {
                this.actor = value;
            }
        }

        public string EncryptionMethod
        {
            get
            {
                return this.encryptedData.EncryptionMethod;
            }
            set
            {
                this.encryptedData.EncryptionMethod = value;
            }
        }

        public XmlDictionaryString EncryptionMethodDictionaryString
        {
            get
            {
                return this.encryptedData.EncryptionMethodDictionaryString;
            }
            set
            {
                this.encryptedData.EncryptionMethodDictionaryString = value;
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
                return this.encryptedData.KeyIdentifier;
            }
            set
            {
                this.encryptedData.KeyIdentifier = value;
            }
        }

        public bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
            set
            {
                this.mustUnderstand = value;
            }
        }

        public bool Relay
        {
            get
            {
                return this.relay;
            }
            set
            {
                this.relay = value;
            }
        }

        public System.IdentityModel.Selectors.SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.encryptedData.SecurityTokenSerializer;
            }
            set
            {
                this.encryptedData.SecurityTokenSerializer = value;
            }
        }
    }
}

