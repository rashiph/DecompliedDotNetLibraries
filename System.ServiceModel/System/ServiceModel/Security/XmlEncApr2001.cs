namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.Xml;

    internal class XmlEncApr2001 : WSSecurityTokenSerializer.SerializerEntries
    {
        private WSSecurityTokenSerializer tokenSerializer;

        public XmlEncApr2001(WSSecurityTokenSerializer tokenSerializer)
        {
            this.tokenSerializer = tokenSerializer;
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<WSSecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
        {
            keyIdentifierClauseEntries.Add(new EncryptedKeyClauseEntry(this.tokenSerializer));
        }

        internal class EncryptedKeyClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            private WSSecurityTokenSerializer tokenSerializer;

            public EncryptedKeyClauseEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                string encryptionMethod = null;
                string carriedKeyName = null;
                SecurityKeyIdentifier encryptingKeyIdentifier = null;
                byte[] encryptedKey = null;
                reader.ReadStartElement(XD.XmlEncryptionDictionary.EncryptedKey, this.NamespaceUri);
                if (reader.IsStartElement(XD.XmlEncryptionDictionary.EncryptionMethod, this.NamespaceUri))
                {
                    encryptionMethod = reader.GetAttribute(XD.XmlEncryptionDictionary.AlgorithmAttribute, null);
                    bool isEmptyElement = reader.IsEmptyElement;
                    reader.ReadStartElement();
                    if (!isEmptyElement)
                    {
                        while (reader.IsStartElement())
                        {
                            reader.Skip();
                        }
                        reader.ReadEndElement();
                    }
                }
                if (this.tokenSerializer.CanReadKeyIdentifier(reader))
                {
                    encryptingKeyIdentifier = this.tokenSerializer.ReadKeyIdentifier(reader);
                }
                reader.ReadStartElement(XD.XmlEncryptionDictionary.CipherData, this.NamespaceUri);
                reader.ReadStartElement(XD.XmlEncryptionDictionary.CipherValue, this.NamespaceUri);
                encryptedKey = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                reader.ReadEndElement();
                if (reader.IsStartElement(XD.XmlEncryptionDictionary.CarriedKeyName, this.NamespaceUri))
                {
                    reader.ReadStartElement();
                    carriedKeyName = reader.ReadString();
                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
                return new EncryptedKeyIdentifierClause(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return (keyIdentifierClause is EncryptedKeyIdentifierClause);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                EncryptedKeyIdentifierClause clause = keyIdentifierClause as EncryptedKeyIdentifierClause;
                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.EncryptedKey, this.NamespaceUri);
                if (clause.EncryptionMethod != null)
                {
                    writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.EncryptionMethod, this.NamespaceUri);
                    writer.WriteAttributeString(XD.XmlEncryptionDictionary.AlgorithmAttribute, null, clause.EncryptionMethod);
                    if (clause.EncryptionMethod == XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap.Value)
                    {
                        writer.WriteStartElement("", XD.XmlSignatureDictionary.DigestMethod, XD.XmlSignatureDictionary.Namespace);
                        writer.WriteAttributeString(XD.XmlSignatureDictionary.Algorithm, null, "http://www.w3.org/2000/09/xmldsig#sha1");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                if (clause.EncryptingKeyIdentifier != null)
                {
                    this.tokenSerializer.WriteKeyIdentifier(writer, clause.EncryptingKeyIdentifier);
                }
                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CipherData, this.NamespaceUri);
                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CipherValue, this.NamespaceUri);
                byte[] encryptedKey = clause.GetEncryptedKey();
                writer.WriteBase64(encryptedKey, 0, encryptedKey.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
                if (clause.CarriedKeyName != null)
                {
                    writer.WriteElementString(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CarriedKeyName, this.NamespaceUri, clause.CarriedKeyName);
                }
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlEncryptionDictionary.EncryptedKey;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlEncryptionDictionary.Namespace;
                }
            }
        }
    }
}

