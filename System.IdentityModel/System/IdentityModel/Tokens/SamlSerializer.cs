namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    public class SamlSerializer
    {
        private System.IdentityModel.DictionaryManager dictionaryManager;

        public virtual SamlAdvice LoadAdvice(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SamlAdvice advice = new SamlAdvice();
            advice.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return advice;
        }

        public virtual SamlAssertion LoadAssertion(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SamlAssertion assertion = new SamlAssertion();
            assertion.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return assertion;
        }

        public virtual SamlAttribute LoadAttribute(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            SamlAttribute attribute = new SamlAttribute();
            attribute.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return attribute;
        }

        public virtual SamlCondition LoadCondition(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (reader.IsStartElement(this.DictionaryManager.SamlDictionary.AudienceRestrictionCondition, this.DictionaryManager.SamlDictionary.Namespace))
            {
                SamlAudienceRestrictionCondition condition = new SamlAudienceRestrictionCondition();
                condition.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
                return condition;
            }
            if (!reader.IsStartElement(this.DictionaryManager.SamlDictionary.DoNotCacheCondition, this.DictionaryManager.SamlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.IdentityModel.SR.GetString("SAMLUnableToLoadUnknownElement", new object[] { reader.LocalName })));
            }
            SamlDoNotCacheCondition condition2 = new SamlDoNotCacheCondition();
            condition2.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return condition2;
        }

        public virtual SamlConditions LoadConditions(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            SamlConditions conditions = new SamlConditions();
            conditions.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return conditions;
        }

        public virtual SamlStatement LoadStatement(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (reader.IsStartElement(this.DictionaryManager.SamlDictionary.AuthenticationStatement, this.DictionaryManager.SamlDictionary.Namespace))
            {
                SamlAuthenticationStatement statement = new SamlAuthenticationStatement();
                statement.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
                return statement;
            }
            if (reader.IsStartElement(this.DictionaryManager.SamlDictionary.AttributeStatement, this.DictionaryManager.SamlDictionary.Namespace))
            {
                SamlAttributeStatement statement2 = new SamlAttributeStatement();
                statement2.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
                return statement2;
            }
            if (!reader.IsStartElement(this.DictionaryManager.SamlDictionary.AuthorizationDecisionStatement, this.DictionaryManager.SamlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.IdentityModel.SR.GetString("SAMLUnableToLoadUnknownElement", new object[] { reader.LocalName })));
            }
            SamlAuthorizationDecisionStatement statement3 = new SamlAuthorizationDecisionStatement();
            statement3.ReadXml(reader, this, keyInfoSerializer, outOfBandTokenResolver);
            return statement3;
        }

        public void PopulateDictionary(IXmlDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");
            }
            this.dictionaryManager = new System.IdentityModel.DictionaryManager(dictionary);
        }

        internal static SecurityKeyIdentifier ReadSecurityKeyIdentifier(XmlReader reader, SecurityTokenSerializer tokenSerializer)
        {
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer", System.IdentityModel.SR.GetString("SamlSerializerRequiresExternalSerializers"));
            }
            if (!tokenSerializer.CanReadKeyIdentifier(reader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SamlSerializerUnableToReadSecurityKeyIdentifier")));
            }
            return tokenSerializer.ReadKeyIdentifier(reader);
        }

        public virtual SamlSecurityToken ReadToken(XmlReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            WrappedReader reader3 = new WrappedReader(XmlDictionaryReader.CreateDictionaryReader(reader));
            SamlAssertion assertion = this.LoadAssertion(reader3, keyInfoSerializer, outOfBandTokenResolver);
            if (assertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadAssertion")));
            }
            return new SamlSecurityToken(assertion);
        }

        internal static SecurityKey ResolveSecurityKey(SecurityKeyIdentifier ski, SecurityTokenResolver tokenResolver)
        {
            if (ski == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ski");
            }
            if (tokenResolver != null)
            {
                for (int i = 0; i < ski.Count; i++)
                {
                    SecurityKey key = null;
                    if (tokenResolver.TryResolveSecurityKey(ski[i], out key))
                    {
                        return key;
                    }
                }
            }
            if (ski.CanCreateKey)
            {
                return ski.CreateKey();
            }
            return null;
        }

        internal static SecurityToken ResolveSecurityToken(SecurityKeyIdentifier ski, SecurityTokenResolver tokenResolver)
        {
            SecurityToken token = null;
            RsaKeyIdentifierClause clause;
            X509RawDataKeyIdentifierClause clause2;
            if (tokenResolver != null)
            {
                tokenResolver.TryResolveToken(ski, out token);
            }
            if ((token == null) && ski.TryFind<RsaKeyIdentifierClause>(out clause))
            {
                token = new RsaSecurityToken(clause.Rsa);
            }
            if ((token == null) && ski.TryFind<X509RawDataKeyIdentifierClause>(out clause2))
            {
                token = new X509SecurityToken(new X509Certificate2(clause2.GetX509RawData()));
            }
            return token;
        }

        internal static void WriteSecurityKeyIdentifier(XmlWriter writer, SecurityKeyIdentifier ski, SecurityTokenSerializer tokenSerializer)
        {
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer", System.IdentityModel.SR.GetString("SamlSerializerRequiresExternalSerializers"));
            }
            bool flag = false;
            if (tokenSerializer.CanWriteKeyIdentifier(ski))
            {
                tokenSerializer.WriteKeyIdentifier(writer, ski);
                flag = true;
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SamlSerializerUnableToWriteSecurityKeyIdentifier", new object[] { ski.ToString() })));
            }
        }

        public virtual void WriteToken(SamlSecurityToken token, XmlWriter writer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            token.Assertion.WriteTo(writer, this, keyInfoSerializer);
        }

        internal System.IdentityModel.DictionaryManager DictionaryManager
        {
            get
            {
                if (this.dictionaryManager == null)
                {
                    this.dictionaryManager = new System.IdentityModel.DictionaryManager();
                }
                return this.dictionaryManager;
            }
        }
    }
}

