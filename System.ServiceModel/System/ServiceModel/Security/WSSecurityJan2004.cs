namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal class WSSecurityJan2004 : System.ServiceModel.Security.WSSecurityTokenSerializer.SerializerEntries
    {
        private System.IdentityModel.Tokens.SamlSerializer samlSerializer;
        private System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer;

        public WSSecurityJan2004(System.ServiceModel.Security.WSSecurityTokenSerializer tokenSerializer, System.IdentityModel.Tokens.SamlSerializer samlSerializer)
        {
            this.tokenSerializer = tokenSerializer;
            this.samlSerializer = samlSerializer;
        }

        protected void PopulateJan2004StrEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.StrEntry> strEntries)
        {
            strEntries.Add(new LocalReferenceStrEntry(this.tokenSerializer));
            strEntries.Add(new KerberosHashStrEntry(this.tokenSerializer));
            strEntries.Add(new X509SkiStrEntry(this.tokenSerializer));
            strEntries.Add(new X509IssuerSerialStrEntry());
            strEntries.Add(new RelDirectStrEntry());
            strEntries.Add(new SamlJan2004KeyIdentifierStrEntry());
            strEntries.Add(new Saml2Jan2004KeyIdentifierStrEntry());
        }

        protected void PopulateJan2004TokenEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new GenericXmlTokenEntry());
            tokenEntryList.Add(new UserNamePasswordTokenEntry(this.tokenSerializer));
            tokenEntryList.Add(new KerberosTokenEntry(this.tokenSerializer));
            tokenEntryList.Add(new X509TokenEntry(this.tokenSerializer));
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.KeyIdentifierClauseEntry> clauseEntries)
        {
            List<System.ServiceModel.Security.WSSecurityTokenSerializer.StrEntry> strEntries = new List<System.ServiceModel.Security.WSSecurityTokenSerializer.StrEntry>();
            this.tokenSerializer.PopulateStrEntries(strEntries);
            SecurityTokenReferenceJan2004ClauseEntry item = new SecurityTokenReferenceJan2004ClauseEntry(this.tokenSerializer, strEntries);
            clauseEntries.Add(item);
        }

        public override void PopulateStrEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.StrEntry> strEntries)
        {
            this.PopulateJan2004StrEntries(strEntries);
        }

        public override void PopulateTokenEntries(IList<System.ServiceModel.Security.WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            this.PopulateJan2004TokenEntries(tokenEntryList);
            tokenEntryList.Add(new SamlTokenEntry(this.tokenSerializer, this.samlSerializer));
            tokenEntryList.Add(new WrappedKeyTokenEntry(this.tokenSerializer));
        }

        public System.IdentityModel.Tokens.SamlSerializer SamlSerializer
        {
            get
            {
                return this.samlSerializer;
            }
        }

        public System.ServiceModel.Security.WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
        }

        internal abstract class BinaryTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            internal static readonly XmlDictionaryString ElementName = XD.SecurityJan2004Dictionary.BinarySecurityToken;
            internal static readonly XmlDictionaryString EncodingTypeAttribute = XD.SecurityJan2004Dictionary.EncodingType;
            internal const string EncodingTypeAttributeString = "EncodingType";
            internal const string EncodingTypeValueBase64Binary = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            internal const string EncodingTypeValueHexBinary = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary";
            private WSSecurityTokenSerializer tokenSerializer;
            internal static readonly XmlDictionaryString ValueTypeAttribute = XD.SecurityJan2004Dictionary.ValueType;
            private string[] valueTypeUris;

            protected BinaryTokenEntry(WSSecurityTokenSerializer tokenSerializer, string valueTypeUri)
            {
                this.tokenSerializer = tokenSerializer;
                this.valueTypeUris = new string[] { valueTypeUri };
            }

            protected BinaryTokenEntry(WSSecurityTokenSerializer tokenSerializer, string[] valueTypeUris)
            {
                if (valueTypeUris == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("valueTypeUris");
                }
                this.tokenSerializer = tokenSerializer;
                this.valueTypeUris = new string[valueTypeUris.GetLength(0)];
                for (int i = 0; i < this.valueTypeUris.GetLength(0); i++)
                {
                    this.valueTypeUris[i] = valueTypeUris[i];
                }
            }

            public abstract SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData);
            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                byte[] buffer;
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", base.TokenType);

                    case SecurityTokenReferenceStyle.External:
                    {
                        string attribute = issuedTokenXml.GetAttribute("EncodingType", null);
                        string innerText = issuedTokenXml.InnerText;
                        if ((attribute != null) && !(attribute == "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"))
                        {
                            if (attribute != "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary")
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnknownEncodingInBinarySecurityToken")));
                            }
                            buffer = SoapHexBinary.Parse(innerText).Value;
                            break;
                        }
                        buffer = Convert.FromBase64String(innerText);
                        break;
                    }
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
                }
                return this.CreateKeyIdentifierClauseFromBinaryCore(buffer);
            }

            public abstract SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData);
            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                byte[] buffer;
                string attribute = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                string valueTypeUri = reader.GetAttribute(ValueTypeAttribute, null);
                string str3 = reader.GetAttribute(EncodingTypeAttribute, null);
                switch (str3)
                {
                    case null:
                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary":
                        buffer = reader.ReadElementContentAsBase64();
                        break;

                    default:
                        if (str3 != "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary")
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnknownEncodingInBinarySecurityToken")));
                        }
                        buffer = SoapHexBinary.Parse(reader.ReadElementContentAsString()).Value;
                        break;
                }
                return this.ReadBinaryCore(attribute, valueTypeUri, buffer);
            }

            public override bool SupportsTokenTypeUri(string tokenTypeUri)
            {
                for (int i = 0; i < this.valueTypeUris.GetLength(0); i++)
                {
                    if (this.valueTypeUris[i] == tokenTypeUri)
                    {
                        return true;
                    }
                }
                return false;
            }

            public abstract void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData);
            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                string str;
                byte[] buffer;
                this.WriteBinaryCore(token, out str, out buffer);
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rawData");
                }
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, ElementName, XD.SecurityJan2004Dictionary.Namespace);
                if (str != null)
                {
                    writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, str);
                }
                if (this.valueTypeUris != null)
                {
                    writer.WriteAttributeString(ValueTypeAttribute, null, this.valueTypeUris[0]);
                }
                if (this.tokenSerializer.EmitBspRequiredAttributes)
                {
                    writer.WriteAttributeString(EncodingTypeAttribute, null, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
                }
                writer.WriteBase64(buffer, 0, buffer.Length);
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return ElementName;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.Namespace;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return this.valueTypeUris[0];
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return this.valueTypeUris[0];
                }
            }
        }

        private class GenericXmlTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            public override bool CanReadTokenCore(XmlDictionaryReader reader)
            {
                return false;
            }

            public override bool CanReadTokenCore(XmlElement element)
            {
                return false;
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(GenericXmlSecurityToken) };
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                BufferedGenericXmlSecurityToken token2 = token as BufferedGenericXmlSecurityToken;
                if ((token2 != null) && (token2.TokenXmlBuffer != null))
                {
                    using (XmlDictionaryReader reader = token2.TokenXmlBuffer.GetReader(0))
                    {
                        writer.WriteNode(reader, false);
                        return;
                    }
                }
                GenericXmlSecurityToken token3 = (GenericXmlSecurityToken) token;
                token3.TokenXml.WriteTo(writer);
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return null;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return null;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return null;
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return null;
                }
            }
        }

        public class IdManager : SignatureTargetIdManager
        {
            private static readonly WSSecurityJan2004.IdManager instance = new WSSecurityJan2004.IdManager();

            private IdManager()
            {
            }

            public override string ExtractId(XmlDictionaryReader reader)
            {
                if (reader == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
                }
                if (reader.IsStartElement(EncryptedData.ElementName, XD.XmlEncryptionDictionary.Namespace))
                {
                    return reader.GetAttribute(XD.XmlEncryptionDictionary.Id, null);
                }
                return reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
            }

            public override void WriteIdAttribute(XmlDictionaryWriter writer, string id)
            {
                if (writer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
                }
                writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
            }

            public override string DefaultIdNamespacePrefix
            {
                get
                {
                    return "u";
                }
            }

            public override string DefaultIdNamespaceUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
                }
            }

            internal static WSSecurityJan2004.IdManager Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        protected class KerberosHashStrEntry : WSSecurityJan2004.KeyIdentifierStrEntry
        {
            public KerberosHashStrEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
            {
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new KerberosTicketHashKeyIdentifierClause(bytes, derivationNonce, derivationLength);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityJan2004Dictionary.KerberosTokenTypeGSS.Value;
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, this.ValueTypeUri);
                KerberosTicketHashKeyIdentifierClause clause2 = clause as KerberosTicketHashKeyIdentifierClause;
                if (base.TokenSerializer.EmitBspRequiredAttributes)
                {
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.EncodingType, null, this.DefaultEncodingType);
                }
                string defaultEncodingType = this.DefaultEncodingType;
                byte[] buffer = clause2.GetBuffer();
                switch (defaultEncodingType)
                {
                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary":
                        writer.WriteBase64(buffer, 0, buffer.Length);
                        break;

                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary":
                        writer.WriteBinHex(buffer, 0, buffer.Length);
                        break;

                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text":
                        writer.WriteString(new UTF8Encoding().GetString(buffer, 0, buffer.Length));
                        break;
                }
                writer.WriteEndElement();
            }

            protected override System.Type ClauseType
            {
                get
                {
                    return typeof(KerberosTicketHashKeyIdentifierClause);
                }
            }

            public override System.Type TokenType
            {
                get
                {
                    return typeof(KerberosRequestorSecurityToken);
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#Kerberosv5APREQSHA1";
                }
            }
        }

        private class KerberosTokenEntry : WSSecurityJan2004.BinaryTokenEntry
        {
            public KerberosTokenEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer, new string[] { "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ", "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ1510" })
            {
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData)
            {
                byte[] buffer;
                using (HashAlgorithm algorithm = CryptoHelper.NewSha1HashAlgorithm())
                {
                    buffer = algorithm.ComputeHash(rawData, 0, rawData.Length);
                }
                return new KerberosTicketHashKeyIdentifierClause(buffer);
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(KerberosReceiverSecurityToken), typeof(KerberosRequestorSecurityToken) };
            }

            public override SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData)
            {
                return new KerberosReceiverSecurityToken(rawData, id, false, valueTypeUri);
            }

            public override void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData)
            {
                KerberosRequestorSecurityToken token2 = (KerberosRequestorSecurityToken) token;
                id = token.Id;
                rawData = token2.GetRequest();
            }
        }

        protected abstract class KeyIdentifierStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            protected const string EncodingTypeValueBase64Binary = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            protected const string EncodingTypeValueHexBinary = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary";
            protected const string EncodingTypeValueText = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text";
            private WSSecurityTokenSerializer tokenSerializer;

            protected KeyIdentifierStrEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    return (this.ValueTypeUri == attribute);
                }
                return false;
            }

            protected abstract SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength);
            public override System.Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return this.TokenType;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                byte[] bytes;
                string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.EncodingType, null);
                if (attribute == null)
                {
                    attribute = this.DefaultEncodingType;
                }
                reader.ReadStartElement();
                if (attribute == "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary")
                {
                    bytes = reader.ReadContentAsBase64();
                }
                else if (attribute == "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary")
                {
                    bytes = SoapHexBinary.Parse(reader.ReadContentAsString()).Value;
                }
                else
                {
                    if (attribute != "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text")
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnknownEncodingInKeyIdentifier")));
                    }
                    bytes = new UTF8Encoding().GetBytes(reader.ReadContentAsString());
                }
                reader.ReadEndElement();
                return this.CreateClause(bytes, derivationNonce, derivationLength);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return this.ClauseType.IsAssignableFrom(clause.GetType());
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, this.ValueTypeUri);
                if (this.tokenSerializer.EmitBspRequiredAttributes)
                {
                    writer.WriteAttributeString(XD.SecurityJan2004Dictionary.EncodingType, null, this.DefaultEncodingType);
                }
                string defaultEncodingType = this.DefaultEncodingType;
                byte[] buffer = (clause as BinaryKeyIdentifierClause).GetBuffer();
                switch (defaultEncodingType)
                {
                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary":
                        writer.WriteBase64(buffer, 0, buffer.Length);
                        break;

                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary":
                        writer.WriteBinHex(buffer, 0, buffer.Length);
                        break;

                    case "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text":
                        writer.WriteString(new UTF8Encoding().GetString(buffer, 0, buffer.Length));
                        break;
                }
                writer.WriteEndElement();
            }

            protected abstract System.Type ClauseType { get; }

            protected virtual string DefaultEncodingType
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
                }
            }

            protected WSSecurityTokenSerializer TokenSerializer
            {
                get
                {
                    return this.tokenSerializer;
                }
            }

            public abstract System.Type TokenType { get; }

            protected abstract string ValueTypeUri { get; }
        }

        protected class LocalReferenceStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            private WSSecurityTokenSerializer tokenSerializer;

            public LocalReferenceStrEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                    if (((attribute != null) && (attribute.Length > 0)) && (attribute[0] == '#'))
                    {
                        return true;
                    }
                }
                return false;
            }

            public string GetLocalTokenTypeUri(SecurityKeyIdentifierClause clause)
            {
                System.Type tokenType = this.GetTokenType(clause);
                return this.tokenSerializer.GetTokenTypeUri(tokenType);
            }

            public override System.Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                LocalIdKeyIdentifierClause clause2 = clause as LocalIdKeyIdentifierClause;
                return clause2.OwnerType;
            }

            public override string GetTokenTypeUri()
            {
                return null;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                string tokenTypeUri = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                System.Type[] ownerTypes = null;
                if (tokenTypeUri != null)
                {
                    ownerTypes = this.tokenSerializer.GetTokenTypes(tokenTypeUri);
                }
                SecurityKeyIdentifierClause clause = new LocalIdKeyIdentifierClause(attribute.Substring(1), derivationNonce, derivationLength, ownerTypes);
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    return clause;
                }
                reader.ReadStartElement();
                reader.ReadEndElement();
                return clause;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return (clause is LocalIdKeyIdentifierClause);
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                LocalIdKeyIdentifierClause clause2 = clause as LocalIdKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                if (this.tokenSerializer.EmitBspRequiredAttributes)
                {
                    string localTokenTypeUri = this.GetLocalTokenTypeUri(clause2);
                    if (localTokenTypeUri != null)
                    {
                        writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, localTokenTypeUri);
                    }
                }
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.URI, null, "#" + clause2.LocalId);
                writer.WriteEndElement();
            }
        }

        protected class RelDirectStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                return (reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace) && (reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null) == "http://docs.oasis-open.org/wss/oasis-wss-rel-token-profile-1.0.pdf#license"));
            }

            public override System.Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return null;
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityJan2004Dictionary.RelAssertionValueType.Value;
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNone, int derivationLength, string tokenType)
            {
                string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.URI, null);
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                }
                else
                {
                    reader.ReadStartElement();
                    reader.ReadEndElement();
                }
                return new RelAssertionDirectKeyIdentifierClause(attribute, derivationNone, derivationLength);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return typeof(RelAssertionDirectKeyIdentifierClause).IsAssignableFrom(clause.GetType());
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                RelAssertionDirectKeyIdentifierClause clause2 = clause as RelAssertionDirectKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, "http://docs.oasis-open.org/wss/oasis-wss-rel-token-profile-1.0.pdf#license");
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.URI, null, clause2.AssertionId);
                writer.WriteEndElement();
            }
        }

        private class Saml2Jan2004KeyIdentifierStrEntry : WSSecurityJan2004.SamlJan2004KeyIdentifierStrEntry
        {
            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.Saml20TokenType.Value;
            }

            protected override bool IsMatchingValueType(string valueType)
            {
                return (valueType == XD.SecurityXXX2005Dictionary.Saml11AssertionValueType.Value);
            }
        }

        protected class SamlJan2004KeyIdentifierStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (reader.IsStartElement(XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri))
                {
                    return true;
                }
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                {
                    string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                    return this.IsMatchingValueType(attribute);
                }
                return false;
            }

            public override System.Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return typeof(SamlSecurityToken);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.SamlTokenType.Value;
            }

            protected virtual bool IsMatchingValueType(string valueType)
            {
                return (valueType == "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID");
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNone, int derivationLength, string tokenType)
            {
                bool flag = false;
                bool flag2 = false;
                string assertionId = null;
                string valueType = null;
                string attribute = null;
                string str4 = null;
                string str5 = null;
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri))
                    {
                        if (flag)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("MultipleSamlAuthorityBindingsInReference")));
                        }
                        flag = true;
                        attribute = reader.GetAttribute(XD.SamlDictionary.Binding, null);
                        if (string.IsNullOrEmpty(attribute))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { XD.SamlDictionary.Binding.Value, XD.SamlDictionary.AuthorityBinding.Value })));
                        }
                        str4 = reader.GetAttribute(XD.SamlDictionary.Location, null);
                        if (string.IsNullOrEmpty(str4))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { XD.SamlDictionary.Location.Value, XD.SamlDictionary.AuthorityBinding.Value })));
                        }
                        str5 = reader.GetAttribute(XD.SamlDictionary.AuthorityKind, null);
                        if (string.IsNullOrEmpty(str5))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { XD.SamlDictionary.AuthorityKind.Value, XD.SamlDictionary.AuthorityBinding.Value })));
                        }
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                        }
                        else
                        {
                            reader.ReadStartElement();
                            reader.ReadEndElement();
                        }
                    }
                    else
                    {
                        if (!reader.IsStartElement(XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace))
                        {
                            break;
                        }
                        if (flag2)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("MultipleKeyIdentifiersInReference")));
                        }
                        flag2 = true;
                        valueType = reader.GetAttribute(XD.SecurityJan2004Dictionary.ValueType, null);
                        assertionId = reader.ReadElementContentAsString();
                    }
                }
                if (!flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("DidNotFindKeyIdentifierInReference")));
                }
                return new SamlAssertionKeyIdentifierClause(assertionId, derivationNone, derivationLength, valueType, tokenType, attribute, str4, str5);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                if (typeof(SamlAssertionKeyIdentifierClause).IsAssignableFrom(clause.GetType()))
                {
                    SamlAssertionKeyIdentifierClause clause2 = clause as SamlAssertionKeyIdentifierClause;
                    if ((clause2.TokenTypeUri == null) || (clause2.TokenTypeUri == this.GetTokenTypeUri()))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                SamlAssertionKeyIdentifierClause clause2 = clause as SamlAssertionKeyIdentifierClause;
                if ((!string.IsNullOrEmpty(clause2.Binding) || !string.IsNullOrEmpty(clause2.Location)) || !string.IsNullOrEmpty(clause2.AuthorityKind))
                {
                    writer.WriteStartElement(XD.SamlDictionary.PreferredPrefix.Value, XD.SamlDictionary.AuthorityBinding, XD.SecurityJan2004Dictionary.SamlUri);
                    if (!string.IsNullOrEmpty(clause2.Binding))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.Binding, null, clause2.Binding);
                    }
                    if (!string.IsNullOrEmpty(clause2.Location))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.Location, null, clause2.Location);
                    }
                    if (!string.IsNullOrEmpty(clause2.AuthorityKind))
                    {
                        writer.WriteAttributeString(XD.SamlDictionary.AuthorityKind, null, clause2.AuthorityKind);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.KeyIdentifier, XD.SecurityJan2004Dictionary.Namespace);
                string str = string.IsNullOrEmpty(clause2.ValueType) ? XD.SecurityJan2004Dictionary.SamlAssertionIdValueType.Value : clause2.ValueType;
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.ValueType, null, str);
                writer.WriteString(clause2.AssertionId);
                writer.WriteEndElement();
            }
        }

        protected class SamlTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            private const string samlAssertionId = "AssertionID";
            private SamlSerializer samlSerializer;
            private SecurityTokenSerializer tokenSerializer;

            public SamlTokenEntry(SecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer)
            {
                this.tokenSerializer = tokenSerializer;
                if (samlSerializer != null)
                {
                    this.samlSerializer = samlSerializer;
                }
                else
                {
                    this.samlSerializer = new SamlSerializer();
                }
                this.samlSerializer.PopulateDictionary(BinaryMessageEncoderFactory.XmlDictionary);
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                    case SecurityTokenReferenceStyle.External:
                        return new SamlAssertionKeyIdentifierClause(issuedTokenXml.GetAttribute("AssertionID"));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(SamlSecurityToken) };
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                return this.samlSerializer.ReadToken(reader, this.tokenSerializer, tokenResolver);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                SamlSecurityToken token2 = token as SamlSecurityToken;
                this.samlSerializer.WriteToken(token2, writer, this.tokenSerializer);
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.SamlAssertion;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.SamlUri;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return null;
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return null;
                }
            }
        }

        protected class SecurityTokenReferenceJan2004ClauseEntry : WSSecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            private IList<WSSecurityTokenSerializer.StrEntry> strEntries;
            private WSSecurityTokenSerializer tokenSerializer;

            public SecurityTokenReferenceJan2004ClauseEntry(WSSecurityTokenSerializer tokenSerializer, IList<WSSecurityTokenSerializer.StrEntry> strEntries)
            {
                this.tokenSerializer = tokenSerializer;
                this.strEntries = strEntries;
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                byte[] derivationNonce = null;
                int derivationLength = 0;
                if (reader.IsStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, this.NamespaceUri))
                {
                    string attribute = reader.GetAttribute(XD.SecureConversationFeb2005Dictionary.Nonce, XD.SecureConversationFeb2005Dictionary.Namespace);
                    if (attribute != null)
                    {
                        derivationNonce = Convert.FromBase64String(attribute);
                    }
                    string str2 = reader.GetAttribute(XD.SecureConversationFeb2005Dictionary.Length, XD.SecureConversationFeb2005Dictionary.Namespace);
                    if (str2 != null)
                    {
                        derivationLength = Convert.ToInt32(str2, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        derivationLength = 0x20;
                    }
                }
                string tokenType = this.ReadTokenType(reader);
                reader.ReadStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, this.NamespaceUri);
                SecurityKeyIdentifierClause clause = null;
                for (int i = 0; i < this.strEntries.Count; i++)
                {
                    if (this.strEntries[i].CanReadClause(reader, tokenType))
                    {
                        clause = this.strEntries[i].ReadClause(reader, derivationNonce, derivationLength, tokenType);
                        break;
                    }
                }
                if (clause == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CannotReadKeyIdentifierClause", new object[] { reader.LocalName, reader.NamespaceURI })));
                }
                reader.ReadEndElement();
                return clause;
            }

            protected virtual string ReadTokenType(XmlDictionaryReader reader)
            {
                return null;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                for (int i = 0; i < this.strEntries.Count; i++)
                {
                    if (this.strEntries[i].SupportsCore(keyIdentifierClause))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                for (int i = 0; i < this.strEntries.Count; i++)
                {
                    if (this.strEntries[i].SupportsCore(keyIdentifierClause))
                    {
                        writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace);
                        this.strEntries[i].WriteContent(writer, keyIdentifierClause);
                        writer.WriteEndElement();
                        return;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StandardsManagerCannotWriteObject", new object[] { keyIdentifierClause.GetType() })));
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.SecurityTokenReference;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.Namespace;
                }
            }

            protected IList<WSSecurityTokenSerializer.StrEntry> StrEntries
            {
                get
                {
                    return this.strEntries;
                }
            }

            protected WSSecurityTokenSerializer TokenSerializer
            {
                get
                {
                    return this.tokenSerializer;
                }
            }
        }

        private class UserNamePasswordTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            private WSSecurityTokenSerializer tokenSerializer;

            public UserNamePasswordTokenEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override IAsyncResult BeginReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver, AsyncCallback callback, object state)
            {
                string str;
                string str2;
                string str3;
                ParseToken(reader, out str, out str2, out str3);
                return new CompletedAsyncResult<SecurityToken>(new UserNameSecurityToken(str2, str3, str), callback, state);
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", typeof(UserNameSecurityToken));

                    case SecurityTokenReferenceStyle.External:
                        return null;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            public override SecurityToken EndReadTokenCore(IAsyncResult result)
            {
                return CompletedAsyncResult<SecurityToken>.End(result);
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(UserNameSecurityToken) };
            }

            private static string ParsePassword(XmlDictionaryReader reader)
            {
                string attribute = reader.GetAttribute(XD.SecurityJan2004Dictionary.TypeAttribute, null);
                if (((attribute != null) && (attribute.Length > 0)) && (attribute != "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedPasswordType", new object[] { attribute })));
                }
                return reader.ReadElementString();
            }

            private static void ParseToken(XmlDictionaryReader reader, out string id, out string userName, out string password)
            {
                id = null;
                userName = null;
                password = null;
                reader.MoveToContent();
                id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                reader.ReadStartElement(XD.SecurityJan2004Dictionary.UserNameTokenElement, XD.SecurityJan2004Dictionary.Namespace);
                while (reader.IsStartElement())
                {
                    if (reader.IsStartElement(XD.SecurityJan2004Dictionary.UserNameElement, XD.SecurityJan2004Dictionary.Namespace))
                    {
                        userName = reader.ReadElementString();
                    }
                    else
                    {
                        if (reader.IsStartElement(XD.SecurityJan2004Dictionary.PasswordElement, XD.SecurityJan2004Dictionary.Namespace))
                        {
                            password = ParsePassword(reader);
                            continue;
                        }
                        if (reader.IsStartElement(XD.SecurityJan2004Dictionary.NonceElement, XD.SecurityJan2004Dictionary.Namespace))
                        {
                            reader.Skip();
                            continue;
                        }
                        if (reader.IsStartElement(XD.UtilityDictionary.CreatedElement, XD.UtilityDictionary.Namespace))
                        {
                            reader.Skip();
                            continue;
                        }
                        XmlHelper.OnUnexpectedChildNodeError("UsernameToken", reader);
                    }
                }
                reader.ReadEndElement();
                if (userName == null)
                {
                    XmlHelper.OnRequiredElementMissing("Username", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                }
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                string str;
                string str2;
                string str3;
                ParseToken(reader, out str, out str2, out str3);
                if (str == null)
                {
                    str = SecurityUniqueId.Create().Value;
                }
                return new UserNameSecurityToken(str2, str3, str);
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                UserNameSecurityToken token2 = (UserNameSecurityToken) token;
                this.WriteUserNamePassword(writer, token2.Id, token2.UserName, token2.Password);
            }

            private void WriteUserNamePassword(XmlDictionaryWriter writer, string id, string userName, string password)
            {
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.UserNameTokenElement, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
                writer.WriteElementString(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.UserNameElement, XD.SecurityJan2004Dictionary.Namespace, userName);
                if (password != null)
                {
                    writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.PasswordElement, XD.SecurityJan2004Dictionary.Namespace);
                    if (this.tokenSerializer.EmitBspRequiredAttributes)
                    {
                        writer.WriteAttributeString(XD.SecurityJan2004Dictionary.TypeAttribute, null, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText");
                    }
                    writer.WriteString(password);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.UserNameTokenElement;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.SecurityJan2004Dictionary.Namespace;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#UsernameToken";
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return null;
                }
            }
        }

        protected class WrappedKeyTokenEntry : WSSecurityTokenSerializer.TokenEntry
        {
            private WSSecurityTokenSerializer tokenSerializer;

            public WrappedKeyTokenEntry(WSSecurityTokenSerializer tokenSerializer)
            {
                this.tokenSerializer = tokenSerializer;
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXmlCore(XmlElement issuedTokenXml, SecurityTokenReferenceStyle tokenReferenceStyle)
            {
                TokenReferenceStyleHelper.Validate(tokenReferenceStyle);
                switch (tokenReferenceStyle)
                {
                    case SecurityTokenReferenceStyle.Internal:
                        return WSSecurityTokenSerializer.TokenEntry.CreateDirectReference(issuedTokenXml, "Id", null, null);

                    case SecurityTokenReferenceStyle.External:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CantInferReferenceForToken", new object[] { EncryptedKey.ElementName.Value })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenReferenceStyle"));
            }

            private WrappedKeySecurityToken CreateWrappedKeyToken(string id, string encryptionMethod, string carriedKeyName, SecurityKeyIdentifier unwrappingTokenIdentifier, byte[] wrappedKey, SecurityTokenResolver tokenResolver)
            {
                SecurityToken expectedWrapper;
                SecurityKey key;
                ISspiNegotiationInfo info = tokenResolver as ISspiNegotiationInfo;
                if (info != null)
                {
                    ISspiNegotiation sspiNegotiation = info.SspiNegotiation;
                    if (encryptionMethod != sspiNegotiation.KeyEncryptionAlgorithm)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("BadKeyEncryptionAlgorithm", new object[] { encryptionMethod })));
                    }
                    byte[] keyToWrap = sspiNegotiation.Decrypt(wrappedKey);
                    return new WrappedKeySecurityToken(id, keyToWrap, encryptionMethod, sspiNegotiation, keyToWrap);
                }
                if (tokenResolver == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("tokenResolver"));
                }
                if ((unwrappingTokenIdentifier == null) || (unwrappingTokenIdentifier.Count == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MissingKeyInfoInEncryptedKey")));
                }
                SecurityHeaderTokenResolver resolver = tokenResolver as SecurityHeaderTokenResolver;
                if (resolver != null)
                {
                    expectedWrapper = resolver.ExpectedWrapper;
                    if (expectedWrapper == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToResolveKeyInfoForUnwrappingToken", new object[] { unwrappingTokenIdentifier, resolver })));
                    }
                    if (!resolver.CheckExternalWrapperMatch(unwrappingTokenIdentifier))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken", new object[] { expectedWrapper })));
                    }
                }
                else
                {
                    try
                    {
                        expectedWrapper = tokenResolver.ResolveToken(unwrappingTokenIdentifier);
                    }
                    catch (Exception exception)
                    {
                        if (exception is MessageSecurityException)
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToResolveKeyInfoForUnwrappingToken", new object[] { unwrappingTokenIdentifier, tokenResolver }), exception));
                    }
                }
                return new WrappedKeySecurityToken(id, System.ServiceModel.Security.SecurityUtils.DecryptKey(expectedWrapper, encryptionMethod, wrappedKey, out key), encryptionMethod, expectedWrapper, unwrappingTokenIdentifier, wrappedKey, key);
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(WrappedKeySecurityToken) };
            }

            public override SecurityToken ReadTokenCore(XmlDictionaryReader reader, SecurityTokenResolver tokenResolver)
            {
                EncryptedKey key = new EncryptedKey {
                    SecurityTokenSerializer = this.tokenSerializer
                };
                key.ReadFrom(reader);
                SecurityKeyIdentifier keyIdentifier = key.KeyIdentifier;
                byte[] wrappedKey = key.GetWrappedKey();
                WrappedKeySecurityToken token = this.CreateWrappedKeyToken(key.Id, key.EncryptionMethod, key.CarriedKeyName, keyIdentifier, wrappedKey, tokenResolver);
                token.EncryptedKey = key;
                return token;
            }

            public override void WriteTokenCore(XmlDictionaryWriter writer, SecurityToken token)
            {
                WrappedKeySecurityToken token2 = token as WrappedKeySecurityToken;
                token2.EnsureEncryptedKeySetUp();
                token2.EncryptedKey.SecurityTokenSerializer = this.tokenSerializer;
                token2.EncryptedKey.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }

            protected override XmlDictionaryString LocalName
            {
                get
                {
                    return EncryptedKey.ElementName;
                }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get
                {
                    return XD.XmlEncryptionDictionary.Namespace;
                }
            }

            public override string TokenTypeUri
            {
                get
                {
                    return null;
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return null;
                }
            }
        }

        protected class X509IssuerSerialStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                return reader.IsStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
            }

            public override System.Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return typeof(X509SecurityToken);
            }

            public override string GetTokenTypeUri()
            {
                return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
            }

            public override SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType)
            {
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace);
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace);
                string issuerName = reader.ReadContentAsString();
                reader.ReadEndElement();
                reader.ReadStartElement(XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace);
                string issuerSerialNumber = reader.ReadContentAsString();
                reader.ReadEndElement();
                reader.ReadEndElement();
                reader.ReadEndElement();
                return new X509IssuerSerialKeyIdentifierClause(issuerName, issuerSerialNumber);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return (clause is X509IssuerSerialKeyIdentifierClause);
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                X509IssuerSerialKeyIdentifierClause clause2 = clause as X509IssuerSerialKeyIdentifierClause;
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
                writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerSerial, XD.XmlSignatureDictionary.Namespace);
                writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509IssuerName, XD.XmlSignatureDictionary.Namespace, clause2.IssuerName);
                writer.WriteElementString(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509SerialNumber, XD.XmlSignatureDictionary.Namespace, clause2.IssuerSerialNumber);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        protected class X509SkiStrEntry : WSSecurityJan2004.KeyIdentifierStrEntry
        {
            public X509SkiStrEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
            {
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new X509SubjectKeyIdentifierClause(bytes);
            }

            public override string GetTokenTypeUri()
            {
                return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
            }

            protected override System.Type ClauseType
            {
                get
                {
                    return typeof(X509SubjectKeyIdentifierClause);
                }
            }

            public override System.Type TokenType
            {
                get
                {
                    return typeof(X509SecurityToken);
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier";
                }
            }
        }

        protected class X509TokenEntry : WSSecurityJan2004.BinaryTokenEntry
        {
            internal const string ValueTypeAbsoluteUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";

            public X509TokenEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer, "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3")
            {
            }

            public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromBinaryCore(byte[] rawData)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CantInferReferenceForToken", new object[] { "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3" })));
            }

            protected override System.Type[] GetTokenTypesCore()
            {
                return new System.Type[] { typeof(X509SecurityToken), typeof(X509WindowsSecurityToken) };
            }

            public override SecurityToken ReadBinaryCore(string id, string valueTypeUri, byte[] rawData)
            {
                X509Certificate2 certificate;
                if (!System.ServiceModel.Security.SecurityUtils.TryCreateX509CertificateFromRawData(rawData, out certificate))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("InvalidX509RawData")));
                }
                return new X509SecurityToken(certificate, id, false);
            }

            public override void WriteBinaryCore(SecurityToken token, out string id, out byte[] rawData)
            {
                id = token.Id;
                X509SecurityToken token2 = token as X509SecurityToken;
                if (token2 != null)
                {
                    rawData = token2.Certificate.GetRawCertData();
                }
                else
                {
                    rawData = ((X509WindowsSecurityToken) token).Certificate.GetRawCertData();
                }
            }
        }
    }
}

