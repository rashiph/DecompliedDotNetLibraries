namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSSecurityXXX2005 : WSSecurityJan2004
    {
        public WSSecurityXXX2005(WSSecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer) : base(tokenSerializer, samlSerializer)
        {
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<WSSecurityTokenSerializer.KeyIdentifierClauseEntry> clauseEntries)
        {
            List<WSSecurityTokenSerializer.StrEntry> strEntries = new List<WSSecurityTokenSerializer.StrEntry>();
            base.WSSecurityTokenSerializer.PopulateStrEntries(strEntries);
            SecurityTokenReferenceXXX2005ClauseEntry item = new SecurityTokenReferenceXXX2005ClauseEntry(base.WSSecurityTokenSerializer, strEntries);
            clauseEntries.Add(item);
        }

        public override void PopulateStrEntries(IList<WSSecurityTokenSerializer.StrEntry> strEntries)
        {
            base.PopulateJan2004StrEntries(strEntries);
            strEntries.Add(new SamlDirectStrEntry());
            strEntries.Add(new X509ThumbprintStrEntry(base.WSSecurityTokenSerializer));
            strEntries.Add(new EncryptedKeyHashStrEntry(base.WSSecurityTokenSerializer));
        }

        public override void PopulateTokenEntries(IList<WSSecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            base.PopulateJan2004TokenEntries(tokenEntryList);
            tokenEntryList.Add(new WrappedKeyTokenEntry(base.WSSecurityTokenSerializer));
            tokenEntryList.Add(new SamlTokenEntry(base.WSSecurityTokenSerializer, base.SamlSerializer));
        }

        private class EncryptedKeyHashStrEntry : WSSecurityJan2004.KeyIdentifierStrEntry
        {
            public EncryptedKeyHashStrEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
            {
            }

            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if ((tokenType != null) && (tokenType != "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey"))
                {
                    return false;
                }
                return base.CanReadClause(reader, tokenType);
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new EncryptedKeyHashIdentifierClause(bytes, true, derivationNonce, derivationLength);
            }

            public override string GetTokenTypeUri()
            {
                return "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey";
            }

            protected override Type ClauseType
            {
                get
                {
                    return typeof(EncryptedKeyHashIdentifierClause);
                }
            }

            public override Type TokenType
            {
                get
                {
                    return typeof(WrappedKeySecurityToken);
                }
            }

            protected override string ValueTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1";
                }
            }
        }

        private class SamlDirectStrEntry : WSSecurityTokenSerializer.StrEntry
        {
            public override bool CanReadClause(XmlDictionaryReader reader, string tokenType)
            {
                if (tokenType != XD.SecurityXXX2005Dictionary.Saml20TokenType.Value)
                {
                    return false;
                }
                return reader.IsStartElement(XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
            }

            public override Type GetTokenType(SecurityKeyIdentifierClause clause)
            {
                return null;
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.Saml20TokenType.Value;
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
                return new SamlAssertionDirectKeyIdentifierClause(attribute, derivationNone, derivationLength);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause clause)
            {
                return typeof(SamlAssertionDirectKeyIdentifierClause).IsAssignableFrom(clause.GetType());
            }

            public override void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause)
            {
                SamlAssertionDirectKeyIdentifierClause clause2 = clause as SamlAssertionDirectKeyIdentifierClause;
                writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.Reference, XD.SecurityJan2004Dictionary.Namespace);
                writer.WriteAttributeString(XD.SecurityJan2004Dictionary.URI, null, clause2.SamlUri);
                writer.WriteEndElement();
            }
        }

        private class SamlTokenEntry : WSSecurityJan2004.SamlTokenEntry
        {
            public SamlTokenEntry(WSSecurityTokenSerializer tokenSerializer, SamlSerializer samlSerializer) : base(tokenSerializer, samlSerializer)
            {
            }

            public override string TokenTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";
                }
            }
        }

        private class SecurityTokenReferenceXXX2005ClauseEntry : WSSecurityJan2004.SecurityTokenReferenceJan2004ClauseEntry
        {
            public SecurityTokenReferenceXXX2005ClauseEntry(WSSecurityTokenSerializer tokenSerializer, IList<WSSecurityTokenSerializer.StrEntry> strEntries) : base(tokenSerializer, strEntries)
            {
            }

            private bool EmitTokenType(WSSecurityTokenSerializer.StrEntry str)
            {
                bool flag = false;
                if (((!(str is WSSecurityJan2004.SamlJan2004KeyIdentifierStrEntry) && !(str is WSSecurityXXX2005.EncryptedKeyHashStrEntry)) && !(str is WSSecurityXXX2005.SamlDirectStrEntry)) && (!base.TokenSerializer.EmitBspRequiredAttributes || (!(str is WSSecurityJan2004.KerberosHashStrEntry) && !(str is WSSecurityJan2004.LocalReferenceStrEntry))))
                {
                    return flag;
                }
                return true;
            }

            private string GetTokenTypeUri(WSSecurityTokenSerializer.StrEntry str, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                if (!this.EmitTokenType(str))
                {
                    return null;
                }
                if (str is WSSecurityJan2004.LocalReferenceStrEntry)
                {
                    string str3;
                    string localTokenTypeUri = (str as WSSecurityJan2004.LocalReferenceStrEntry).GetLocalTokenTypeUri(keyIdentifierClause);
                    if (((str3 = localTokenTypeUri) != null) && (((str3 == "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0") || (str3 == "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1")) || ((str3 == "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey") || (str3 == "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ"))))
                    {
                        return localTokenTypeUri;
                    }
                    return null;
                }
                return str.GetTokenTypeUri();
            }

            protected override string ReadTokenType(XmlDictionaryReader reader)
            {
                return reader.GetAttribute(XD.SecurityXXX2005Dictionary.TokenTypeAttribute, XD.SecurityXXX2005Dictionary.Namespace);
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                for (int i = 0; i < base.StrEntries.Count; i++)
                {
                    if (base.StrEntries[i].SupportsCore(keyIdentifierClause))
                    {
                        writer.WriteStartElement(XD.SecurityJan2004Dictionary.Prefix.Value, XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace);
                        string tokenTypeUri = this.GetTokenTypeUri(base.StrEntries[i], keyIdentifierClause);
                        if (tokenTypeUri != null)
                        {
                            writer.WriteAttributeString(XD.SecurityXXX2005Dictionary.Prefix.Value, XD.SecurityXXX2005Dictionary.TokenTypeAttribute, XD.SecurityXXX2005Dictionary.Namespace, tokenTypeUri);
                        }
                        base.StrEntries[i].WriteContent(writer, keyIdentifierClause);
                        writer.WriteEndElement();
                        return;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("StandardsManagerCannotWriteObject", new object[] { keyIdentifierClause.GetType() })));
            }
        }

        private class WrappedKeyTokenEntry : WSSecurityJan2004.WrappedKeyTokenEntry
        {
            public WrappedKeyTokenEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
            {
            }

            public override string TokenTypeUri
            {
                get
                {
                    return "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey";
                }
            }
        }

        private class X509ThumbprintStrEntry : WSSecurityJan2004.KeyIdentifierStrEntry
        {
            public X509ThumbprintStrEntry(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
            {
            }

            protected override SecurityKeyIdentifierClause CreateClause(byte[] bytes, byte[] derivationNonce, int derivationLength)
            {
                return new X509ThumbprintKeyIdentifierClause(bytes);
            }

            public override string GetTokenTypeUri()
            {
                return XD.SecurityXXX2005Dictionary.ThumbprintSha1ValueType.Value;
            }

            protected override Type ClauseType
            {
                get
                {
                    return typeof(X509ThumbprintKeyIdentifierClause);
                }
            }

            public override Type TokenType
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
                    return "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1";
                }
            }
        }
    }
}

