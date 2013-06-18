namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Xml;

    internal class XmlDsigSep2000 : WSSecurityTokenSerializer.SerializerEntries
    {
        private WSSecurityTokenSerializer tokenSerializer;

        public XmlDsigSep2000(WSSecurityTokenSerializer tokenSerializer)
        {
            this.tokenSerializer = tokenSerializer;
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<WSSecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
        {
            keyIdentifierClauseEntries.Add(new KeyNameClauseEntry());
            keyIdentifierClauseEntries.Add(new KeyValueClauseEntry());
            keyIdentifierClauseEntries.Add(new X509CertificateClauseEntry());
        }

        public override void PopulateKeyIdentifierEntries(IList<WSSecurityTokenSerializer.KeyIdentifierEntry> keyIdentifierEntries)
        {
            keyIdentifierEntries.Add(new KeyInfoEntry(this.tokenSerializer));
        }

        internal class KeyInfoEntry : WSSecurityTokenSerializer.KeyIdentifierEntry
        {
            private WSSecurityTokenSerializer tokenSerializer;

            public KeyInfoEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlDictionaryReader reader)
            {
                reader.ReadStartElement(this.LocalName, this.NamespaceUri);
                SecurityKeyIdentifier identifier = new SecurityKeyIdentifier();
                while (reader.IsStartElement())
                {
                    SecurityKeyIdentifierClause clause = this.tokenSerializer.ReadKeyIdentifierClause(reader);
                    if (clause == null)
                    {
                        reader.Skip();
                    }
                    else
                    {
                        identifier.Add(clause);
                    }
                }
                if (identifier.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ErrorDeserializingKeyIdentifierClause")));
                }
                reader.ReadEndElement();
                return identifier;
            }

            public override bool SupportsCore(SecurityKeyIdentifier keyIdentifier)
            {
                return true;
            }

            public override void WriteKeyIdentifierCore(XmlDictionaryWriter writer, SecurityKeyIdentifier keyIdentifier)
            {
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, this.LocalName, this.NamespaceUri);
                bool flag = false;
                foreach (SecurityKeyIdentifierClause clause in keyIdentifier)
                {
                    this.tokenSerializer.WriteKeyIdentifierClause(writer, clause);
                    flag = true;
                }
                writer.WriteEndElement();
                if (!flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoKeyInfoClausesToWrite")));
                }
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyInfo;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }
        }

        internal class KeyNameClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                reader.ReadStartElement(XD.XmlSignatureDictionary.KeyName, this.NamespaceUri);
                string keyName = reader.ReadString();
                reader.ReadEndElement();
                return new KeyNameIdentifierClause(keyName);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return (keyIdentifierClause is KeyNameIdentifierClause);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                KeyNameIdentifierClause clause = keyIdentifierClause as KeyNameIdentifierClause;
                writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyName, this.NamespaceUri, clause.KeyName);
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyName;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }
        }

        internal class KeyValueClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                reader.ReadStartElement(XD.XmlSignatureDictionary.KeyValue, this.NamespaceUri);
                reader.ReadStartElement(XD.XmlSignatureDictionary.RsaKeyValue, this.NamespaceUri);
                reader.ReadStartElement(XD.XmlSignatureDictionary.Modulus, this.NamespaceUri);
                byte[] buffer = Convert.FromBase64String(reader.ReadString());
                reader.ReadEndElement();
                reader.ReadStartElement(XD.XmlSignatureDictionary.Exponent, this.NamespaceUri);
                byte[] buffer2 = Convert.FromBase64String(reader.ReadString());
                reader.ReadEndElement();
                reader.ReadEndElement();
                reader.ReadEndElement();
                RSA rsa = new RSACryptoServiceProvider();
                RSAParameters parameters = new RSAParameters {
                    Modulus = buffer,
                    Exponent = buffer2
                };
                rsa.ImportParameters(parameters);
                return new RsaKeyIdentifierClause(rsa);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return (keyIdentifierClause is RsaKeyIdentifierClause);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                RsaKeyIdentifierClause clause = keyIdentifierClause as RsaKeyIdentifierClause;
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyValue, this.NamespaceUri);
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.RsaKeyValue, this.NamespaceUri);
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Modulus, this.NamespaceUri);
                clause.WriteModulusAsBase64(writer);
                writer.WriteEndElement();
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.Exponent, this.NamespaceUri);
                clause.WriteExponentAsBase64(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.KeyValue;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }
        }

        internal class X509CertificateClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                SecurityKeyIdentifierClause clause = null;
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509Data, this.NamespaceUri);
                while (reader.IsStartElement())
                {
                    if ((clause == null) && reader.IsStartElement(XD.XmlSignatureDictionary.X509Certificate, this.NamespaceUri))
                    {
                        X509Certificate2 certificate = null;
                        if (!System.ServiceModel.Security.SecurityUtils.TryCreateX509CertificateFromRawData(reader.ReadElementContentAsBase64(), out certificate))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidX509RawData")));
                        }
                        clause = new X509RawDataKeyIdentifierClause(certificate);
                    }
                    else if ((clause == null) && reader.IsStartElement("X509SKI", this.NamespaceUri.ToString()))
                    {
                        clause = new X509SubjectKeyIdentifierClause(reader.ReadElementContentAsBase64());
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
                reader.ReadEndElement();
                return clause;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return (keyIdentifierClause is X509RawDataKeyIdentifierClause);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                X509RawDataKeyIdentifierClause clause = keyIdentifierClause as X509RawDataKeyIdentifierClause;
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, this.NamespaceUri);
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Certificate, this.NamespaceUri);
                byte[] buffer = clause.GetX509RawData();
                writer.WriteBase64(buffer, 0, buffer.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.XmlSignatureDictionary.X509Data;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlSignatureDictionary.Namespace;
                }
            }
        }
    }
}

