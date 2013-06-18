namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Xml;

    public class SamlAssertion : ICanonicalWriterEndRootElementCallback
    {
        private SamlAdvice advice;
        private string assertionId;
        private SamlConditions conditions;
        private ReadOnlyCollection<SecurityKey> cryptoList;
        private DictionaryManager dictionaryManager;
        private HashStream hashStream;
        private bool isReadOnly;
        private DateTime issueInstant;
        private string issuer;
        private SecurityTokenSerializer keyInfoSerializer;
        private SignedXml signature;
        private System.IdentityModel.Tokens.SigningCredentials signingCredentials;
        private SecurityToken signingToken;
        private readonly ImmutableCollection<SamlStatement> statements;
        private XmlTokenStream tokenStream;
        private SecurityKey verificationKey;

        public SamlAssertion()
        {
            this.assertionId = "SamlSecurityToken-" + Guid.NewGuid().ToString();
            this.issueInstant = DateTime.UtcNow.ToUniversalTime();
            this.statements = new ImmutableCollection<SamlStatement>();
        }

        public SamlAssertion(string assertionId, string issuer, DateTime issueInstant, SamlConditions samlConditions, SamlAdvice samlAdvice, IEnumerable<SamlStatement> samlStatements)
        {
            this.assertionId = "SamlSecurityToken-" + Guid.NewGuid().ToString();
            this.issueInstant = DateTime.UtcNow.ToUniversalTime();
            this.statements = new ImmutableCollection<SamlStatement>();
            if (string.IsNullOrEmpty(assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionIdRequired"));
            }
            if (!this.IsAssertionIdValid(assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionIDIsInvalid", new object[] { assertionId }));
            }
            if (string.IsNullOrEmpty(issuer))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionIssuerRequired"));
            }
            if (samlStatements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlStatements");
            }
            this.assertionId = assertionId;
            this.issuer = issuer;
            this.issueInstant = issueInstant.ToUniversalTime();
            this.conditions = samlConditions;
            this.advice = samlAdvice;
            foreach (SamlStatement statement in samlStatements)
            {
                if (statement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Statement.Value }));
                }
                this.statements.Add(statement);
            }
            if (this.statements.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionRequireOneStatement"));
            }
        }

        internal static void AddSamlClaimTypes(ICollection<Type> knownClaimTypes)
        {
            if (knownClaimTypes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("knownClaimTypes");
            }
            knownClaimTypes.Add(typeof(SamlAuthorizationDecisionClaimResource));
            knownClaimTypes.Add(typeof(SamlAuthenticationClaimResource));
            knownClaimTypes.Add(typeof(SamlAccessDecision));
            knownClaimTypes.Add(typeof(SamlAuthorityBinding));
            knownClaimTypes.Add(typeof(SamlNameIdentifierClaimResource));
        }

        private ReadOnlyCollection<SecurityKey> BuildCryptoList()
        {
            List<SecurityKey> list = new List<SecurityKey>();
            for (int i = 0; i < this.statements.Count; i++)
            {
                SamlSubjectStatement statement = this.statements[i] as SamlSubjectStatement;
                if (statement != null)
                {
                    bool flag = false;
                    SecurityKey item = null;
                    if (statement.SamlSubject != null)
                    {
                        item = statement.SamlSubject.Crypto;
                    }
                    InMemorySymmetricSecurityKey key2 = item as InMemorySymmetricSecurityKey;
                    if (key2 != null)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            if ((list[j] is InMemorySymmetricSecurityKey) && (list[j].KeySize == key2.KeySize))
                            {
                                byte[] symmetricKey = ((InMemorySymmetricSecurityKey) list[j]).GetSymmetricKey();
                                byte[] buffer2 = key2.GetSymmetricKey();
                                int index = 0;
                                index = 0;
                                while (index < symmetricKey.Length)
                                {
                                    if (symmetricKey[index] != buffer2[index])
                                    {
                                        break;
                                    }
                                    index++;
                                }
                                flag = index == symmetricKey.Length;
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                    if (!flag && (item != null))
                    {
                        list.Add(item);
                    }
                }
            }
            return list.AsReadOnly();
        }

        private void CheckObjectValidity()
        {
            if (string.IsNullOrEmpty(this.assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionIdRequired")));
            }
            if (!this.IsAssertionIdValid(this.assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionIDIsInvalid")));
            }
            if (string.IsNullOrEmpty(this.issuer))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionIssuerRequired")));
            }
            if (this.statements.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionRequireOneStatement")));
            }
        }

        private bool IsAssertionIdValid(string assertionId)
        {
            if (string.IsNullOrEmpty(assertionId))
            {
                return false;
            }
            if (((assertionId[0] < 'A') || (assertionId[0] > 'Z')) && ((assertionId[0] < 'a') || (assertionId[0] > 'z')))
            {
                return (assertionId[0] == '_');
            }
            return true;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (this.conditions != null)
                {
                    this.conditions.MakeReadOnly();
                }
                if (this.advice != null)
                {
                    this.advice.MakeReadOnly();
                }
                foreach (SamlStatement statement in this.statements)
                {
                    statement.MakeReadOnly();
                }
                this.statements.MakeReadOnly();
                if (this.cryptoList == null)
                {
                    this.cryptoList = this.BuildCryptoList();
                }
                this.isReadOnly = true;
            }
        }

        protected void ReadSignature(XmlDictionaryReader reader, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver, SamlSerializer samlSerializer)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSerializer");
            }
            if (this.signature != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SAMLSignatureAlreadyRead")));
            }
            XmlDictionaryReader reader2 = reader;
            if (!reader2.CanCanonicalize)
            {
                MemoryStream stream = new MemoryStream();
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, samlSerializer.DictionaryManager.ParentDictionary);
                writer.WriteNode(reader2, false);
                writer.Flush();
                stream.Position = 0L;
                reader2 = XmlDictionaryReader.CreateBinaryReader(stream.GetBuffer(), 0, (int) stream.Length, samlSerializer.DictionaryManager.ParentDictionary, reader.Quotas);
                reader2.MoveToContent();
                writer.Close();
            }
            SignedXml xml = new SignedXml(new StandardSignedInfo(samlSerializer.DictionaryManager), samlSerializer.DictionaryManager, keyInfoSerializer) {
                TransformFactory = ExtendedTransformFactory.Instance
            };
            xml.ReadFrom(reader2);
            SecurityKeyIdentifier keyIdentifier = xml.Signature.KeyIdentifier;
            this.verificationKey = SamlSerializer.ResolveSecurityKey(keyIdentifier, outOfBandTokenResolver);
            if (this.verificationKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToResolveSignatureKey", new object[] { this.issuer })));
            }
            this.signature = xml;
            this.signingToken = SamlSerializer.ResolveSecurityToken(keyIdentifier, outOfBandTokenResolver);
            if (this.signingToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlSigningTokenNotFound")));
            }
            if (!object.ReferenceEquals(reader, reader2))
            {
                reader2.Close();
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("ReadXml"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            WrappedReader reader3 = new WrappedReader(XmlDictionaryReader.CreateDictionaryReader(reader));
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            if (!reader3.IsStartElement(samlDictionary.Assertion, samlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLElementNotRecognized", new object[] { reader3.LocalName })));
            }
            string attribute = reader3.GetAttribute(samlDictionary.MajorVersion, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionMissingMajorVersionAttributeOnRead")));
            }
            int num = int.Parse(attribute, CultureInfo.InvariantCulture);
            attribute = reader3.GetAttribute(samlDictionary.MinorVersion, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionMissingMinorVersionAttributeOnRead")));
            }
            int num2 = int.Parse(attribute, CultureInfo.InvariantCulture);
            if ((num != SamlConstants.MajorVersionValue) || (num2 != SamlConstants.MinorVersionValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLTokenVersionNotSupported", new object[] { num, num2, SamlConstants.MajorVersionValue, SamlConstants.MinorVersionValue })));
            }
            attribute = reader3.GetAttribute(samlDictionary.AssertionId, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionIdRequired")));
            }
            if (!this.IsAssertionIdValid(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionIDIsInvalid", new object[] { attribute })));
            }
            this.assertionId = attribute;
            attribute = reader3.GetAttribute(samlDictionary.Issuer, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionMissingIssuerAttributeOnRead")));
            }
            this.issuer = attribute;
            attribute = reader3.GetAttribute(samlDictionary.IssueInstant, null);
            if (!string.IsNullOrEmpty(attribute))
            {
                this.issueInstant = DateTime.ParseExact(attribute, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            }
            reader3.MoveToContent();
            reader3.Read();
            if (reader3.IsStartElement(samlDictionary.Conditions, samlDictionary.Namespace))
            {
                this.conditions = samlSerializer.LoadConditions(reader3, keyInfoSerializer, outOfBandTokenResolver);
                if (this.conditions == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadCondtions")));
                }
            }
            if (reader3.IsStartElement(samlDictionary.Advice, samlDictionary.Namespace))
            {
                this.advice = samlSerializer.LoadAdvice(reader3, keyInfoSerializer, outOfBandTokenResolver);
                if (this.advice == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadAdvice")));
                }
            }
            while (reader3.IsStartElement())
            {
                if (reader3.IsStartElement(samlSerializer.DictionaryManager.XmlSignatureDictionary.Signature, samlSerializer.DictionaryManager.XmlSignatureDictionary.Namespace))
                {
                    break;
                }
                SamlStatement item = samlSerializer.LoadStatement(reader3, keyInfoSerializer, outOfBandTokenResolver);
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLUnableToLoadStatement")));
                }
                this.statements.Add(item);
            }
            if (this.statements.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAssertionRequireOneStatementOnRead")));
            }
            if (reader3.IsStartElement(samlSerializer.DictionaryManager.XmlSignatureDictionary.Signature, samlSerializer.DictionaryManager.XmlSignatureDictionary.Namespace))
            {
                this.ReadSignature(reader3, keyInfoSerializer, outOfBandTokenResolver, samlSerializer);
            }
            reader3.MoveToContent();
            reader3.ReadEndElement();
            this.tokenStream = reader3.XmlTokens;
            if (this.signature != null)
            {
                this.VerifySignature(this.signature, this.verificationKey);
            }
            this.BuildCryptoList();
        }

        void ICanonicalWriterEndRootElementCallback.OnEndOfRootElement(XmlDictionaryWriter dictionaryWriter)
        {
            byte[] digest = this.hashStream.FlushHashAndGetValue();
            PreDigestedSignedInfo signedInfo = new PreDigestedSignedInfo(this.dictionaryManager) {
                AddEnvelopedSignatureTransform = true,
                CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#",
                SignatureMethod = this.signingCredentials.SignatureAlgorithm,
                DigestMethod = this.signingCredentials.DigestAlgorithm
            };
            signedInfo.AddReference(this.assertionId, digest);
            SignedXml xml = new SignedXml(signedInfo, this.dictionaryManager, this.keyInfoSerializer);
            xml.ComputeSignature(this.signingCredentials.SigningKey);
            xml.Signature.KeyIdentifier = this.signingCredentials.SigningKeyIdentifier;
            xml.WriteTo(dictionaryWriter);
        }

        private void VerifySignature(SignedXml signature, SecurityKey signatureVerificationKey)
        {
            if (signature == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signature");
            }
            if (signatureVerificationKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signatureVerificatonKey");
            }
            signature.StartSignatureVerification(signatureVerificationKey);
            signature.EnsureDigestValidity(this.assertionId, this.tokenStream);
            signature.CompleteSignatureVerification();
        }

        internal void WriteTo(XmlWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if ((this.signingCredentials == null) && (this.signature == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SamlAssertionMissingSigningCredentials")));
            }
            XmlDictionaryWriter innerWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            if (this.signingCredentials != null)
            {
                using (HashAlgorithm algorithm = CryptoHelper.CreateHashAlgorithm(this.signingCredentials.DigestAlgorithm))
                {
                    this.hashStream = new HashStream(algorithm);
                    this.keyInfoSerializer = keyInfoSerializer;
                    this.dictionaryManager = samlSerializer.DictionaryManager;
                    SamlDelegatingWriter writer3 = new SamlDelegatingWriter(innerWriter, this.hashStream, this, samlSerializer.DictionaryManager.ParentDictionary);
                    this.WriteXml(writer3, samlSerializer, keyInfoSerializer);
                    return;
                }
            }
            this.tokenStream.SetElementExclusion(null, null);
            this.tokenStream.WriteTo(innerWriter, samlSerializer.DictionaryManager);
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            this.CheckObjectValidity();
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            try
            {
                writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Assertion, samlDictionary.Namespace);
                writer.WriteStartAttribute(samlDictionary.MajorVersion, null);
                writer.WriteValue(SamlConstants.MajorVersionValue);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(samlDictionary.MinorVersion, null);
                writer.WriteValue(SamlConstants.MinorVersionValue);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(samlDictionary.AssertionId, null);
                writer.WriteString(this.assertionId);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(samlDictionary.Issuer, null);
                writer.WriteString(this.issuer);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(samlDictionary.IssueInstant, null);
                writer.WriteString(this.issueInstant.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
                writer.WriteEndAttribute();
                if (this.conditions != null)
                {
                    this.conditions.WriteXml(writer, samlSerializer, keyInfoSerializer);
                }
                if (this.advice != null)
                {
                    this.advice.WriteXml(writer, samlSerializer, keyInfoSerializer);
                }
                for (int i = 0; i < this.statements.Count; i++)
                {
                    this.statements[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
                }
                writer.WriteEndElement();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SAMLTokenNotSerialized"), exception));
            }
        }

        public SamlAdvice Advice
        {
            get
            {
                return this.advice;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.advice = value;
            }
        }

        public string AssertionId
        {
            get
            {
                return this.assertionId;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionIdRequired"));
                }
                this.assertionId = value;
            }
        }

        public SamlConditions Conditions
        {
            get
            {
                return this.conditions;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.conditions = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public DateTime IssueInstant
        {
            get
            {
                return this.issueInstant;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.issueInstant = value;
            }
        }

        public string Issuer
        {
            get
            {
                return this.issuer;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAssertionIssuerRequired"));
                }
                this.issuer = value;
            }
        }

        public int MajorVersion
        {
            get
            {
                return SamlConstants.MajorVersionValue;
            }
        }

        public int MinorVersion
        {
            get
            {
                return SamlConstants.MinorVersionValue;
            }
        }

        internal ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.cryptoList;
            }
        }

        internal SignedXml Signature
        {
            get
            {
                return this.signature;
            }
        }

        internal SecurityKey SignatureVerificationKey
        {
            get
            {
                return this.verificationKey;
            }
        }

        public System.IdentityModel.Tokens.SigningCredentials SigningCredentials
        {
            get
            {
                return this.signingCredentials;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.signingCredentials = value;
            }
        }

        public SecurityToken SigningToken
        {
            get
            {
                return this.signingToken;
            }
        }

        public IList<SamlStatement> Statements
        {
            get
            {
                return this.statements;
            }
        }
    }
}

