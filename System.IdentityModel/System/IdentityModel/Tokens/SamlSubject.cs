namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Security.Principal;
    using System.Xml;

    public class SamlSubject
    {
        private List<Claim> claims;
        private string confirmationData;
        private readonly ImmutableCollection<string> confirmationMethods;
        private SecurityKey crypto;
        private IIdentity identity;
        private bool isReadOnly;
        private string name;
        private string nameFormat;
        private string nameQualifier;
        private SecurityKeyIdentifier securityKeyIdentifier;
        private ClaimSet subjectKeyClaimset;
        private SecurityToken subjectToken;

        public SamlSubject()
        {
            this.confirmationMethods = new ImmutableCollection<string>();
        }

        public SamlSubject(string nameFormat, string nameQualifier, string name) : this(nameFormat, nameQualifier, name, null, null, null)
        {
        }

        public SamlSubject(string nameFormat, string nameQualifier, string name, IEnumerable<string> confirmations, string confirmationData, SecurityKeyIdentifier securityKeyIdentifier)
        {
            this.confirmationMethods = new ImmutableCollection<string>();
            if (confirmations != null)
            {
                foreach (string str in confirmations)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.SubjectConfirmationMethod.Value }));
                    }
                    this.confirmationMethods.Add(str);
                }
            }
            if ((this.confirmationMethods.Count == 0) && string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLSubjectRequiresNameIdentifierOrConfirmationMethod"));
            }
            if ((this.confirmationMethods.Count == 0) && ((confirmationData != null) || (securityKeyIdentifier != null)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified"));
            }
            this.name = name;
            this.nameFormat = nameFormat;
            this.nameQualifier = nameQualifier;
            this.confirmationData = confirmationData;
            this.securityKeyIdentifier = securityKeyIdentifier;
        }

        private void CheckObjectValidity()
        {
            if ((this.confirmationMethods.Count == 0) && string.IsNullOrEmpty(this.name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectRequiresNameIdentifierOrConfirmationMethod")));
            }
            if ((this.confirmationMethods.Count == 0) && ((this.confirmationData != null) || (this.securityKeyIdentifier != null)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectRequiresConfirmationMethodWhenConfirmationDataOrKeyInfoIsSpecified")));
            }
        }

        public virtual ReadOnlyCollection<Claim> ExtractClaims()
        {
            if (this.claims == null)
            {
                this.claims = new List<Claim>();
                if (!string.IsNullOrEmpty(this.name))
                {
                    this.claims.Add(new Claim(ClaimTypes.NameIdentifier, new SamlNameIdentifierClaimResource(this.name, this.nameQualifier, this.nameFormat), Rights.Identity));
                    this.claims.Add(new Claim(ClaimTypes.NameIdentifier, new SamlNameIdentifierClaimResource(this.name, this.nameQualifier, this.nameFormat), Rights.PossessProperty));
                }
            }
            return this.claims.AsReadOnly();
        }

        public virtual ClaimSet ExtractSubjectKeyClaimSet(SamlSecurityTokenAuthenticator samlAuthenticator)
        {
            if ((this.subjectKeyClaimset == null) && (this.securityKeyIdentifier != null))
            {
                if (samlAuthenticator == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlAuthenticator");
                }
                if (this.subjectToken != null)
                {
                    this.subjectKeyClaimset = samlAuthenticator.ResolveClaimSet(this.subjectToken);
                    this.identity = samlAuthenticator.ResolveIdentity(this.subjectToken);
                    if ((this.identity == null) && (this.subjectKeyClaimset != null))
                    {
                        Claim claim = null;
                        foreach (Claim claim2 in this.subjectKeyClaimset.FindClaims(null, Rights.Identity))
                        {
                            claim = claim2;
                            break;
                        }
                        if (claim != null)
                        {
                            this.identity = System.IdentityModel.SecurityUtils.CreateIdentity(claim.Resource.ToString(), base.GetType().Name);
                        }
                    }
                }
                if (this.subjectKeyClaimset == null)
                {
                    this.subjectKeyClaimset = samlAuthenticator.ResolveClaimSet(this.securityKeyIdentifier);
                    this.identity = samlAuthenticator.ResolveIdentity(this.securityKeyIdentifier);
                }
            }
            return this.subjectKeyClaimset;
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (this.securityKeyIdentifier != null)
                {
                    this.securityKeyIdentifier.MakeReadOnly();
                }
                this.confirmationMethods.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("samlSerializer");
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            reader.MoveToContent();
            reader.Read();
            if (reader.IsStartElement(samlDictionary.NameIdentifier, samlDictionary.Namespace))
            {
                this.nameFormat = reader.GetAttribute(samlDictionary.NameIdentifierFormat, null);
                this.nameQualifier = reader.GetAttribute(samlDictionary.NameIdentifierNameQualifier, null);
                reader.MoveToContent();
                this.name = reader.ReadString();
                if (this.name == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLNameIdentifierMissingIdentifierValueOnRead")));
                }
                reader.MoveToContent();
                reader.ReadEndElement();
            }
            if (reader.IsStartElement(samlDictionary.SubjectConfirmation, samlDictionary.Namespace))
            {
                reader.MoveToContent();
                reader.Read();
                while (reader.IsStartElement(samlDictionary.SubjectConfirmationMethod, samlDictionary.Namespace))
                {
                    string str = reader.ReadString();
                    if (string.IsNullOrEmpty(str))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.SubjectConfirmationMethod.Value })));
                    }
                    this.confirmationMethods.Add(str);
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                if (this.confirmationMethods.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectConfirmationClauseMissingConfirmationMethodOnRead")));
                }
                if (reader.IsStartElement(samlDictionary.SubjectConfirmationData, samlDictionary.Namespace))
                {
                    reader.MoveToContent();
                    this.confirmationData = reader.ReadString();
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                if (reader.IsStartElement(samlSerializer.DictionaryManager.XmlSignatureDictionary.KeyInfo, samlSerializer.DictionaryManager.XmlSignatureDictionary.Namespace))
                {
                    XmlDictionaryReader reader2 = XmlDictionaryReader.CreateDictionaryReader(reader);
                    this.securityKeyIdentifier = SamlSerializer.ReadSecurityKeyIdentifier(reader2, keyInfoSerializer);
                    this.crypto = SamlSerializer.ResolveSecurityKey(this.securityKeyIdentifier, outOfBandTokenResolver);
                    if (this.crypto == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SamlUnableToExtractSubjectKey")));
                    }
                    this.subjectToken = SamlSerializer.ResolveSecurityToken(this.securityKeyIdentifier, outOfBandTokenResolver);
                }
                if ((this.confirmationMethods.Count == 0) && string.IsNullOrEmpty(this.name))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectRequiresNameIdentifierOrConfirmationMethodOnRead")));
                }
                reader.MoveToContent();
                reader.ReadEndElement();
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            this.CheckObjectValidity();
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.Subject, samlDictionary.Namespace);
            if (this.name != null)
            {
                writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.NameIdentifier, samlDictionary.Namespace);
                if (this.nameFormat != null)
                {
                    writer.WriteStartAttribute(samlDictionary.NameIdentifierFormat, null);
                    writer.WriteString(this.nameFormat);
                    writer.WriteEndAttribute();
                }
                if (this.nameQualifier != null)
                {
                    writer.WriteStartAttribute(samlDictionary.NameIdentifierNameQualifier, null);
                    writer.WriteString(this.nameQualifier);
                    writer.WriteEndAttribute();
                }
                writer.WriteString(this.name);
                writer.WriteEndElement();
            }
            if (this.confirmationMethods.Count > 0)
            {
                writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.SubjectConfirmation, samlDictionary.Namespace);
                foreach (string str in this.confirmationMethods)
                {
                    writer.WriteElementString(samlDictionary.SubjectConfirmationMethod, samlDictionary.Namespace, str);
                }
                if (!string.IsNullOrEmpty(this.confirmationData))
                {
                    writer.WriteElementString(samlDictionary.SubjectConfirmationData, samlDictionary.Namespace, this.confirmationData);
                }
                if (this.securityKeyIdentifier != null)
                {
                    SamlSerializer.WriteSecurityKeyIdentifier(XmlDictionaryWriter.CreateDictionaryWriter(writer), this.securityKeyIdentifier, keyInfoSerializer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public IList<string> ConfirmationMethods
        {
            get
            {
                return this.confirmationMethods;
            }
        }

        public SecurityKey Crypto
        {
            get
            {
                return this.crypto;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.crypto = value;
            }
        }

        internal IIdentity Identity
        {
            get
            {
                return this.identity;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get
            {
                return this.securityKeyIdentifier;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.securityKeyIdentifier = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLSubjectNameIdentifierRequiresNameValue"));
                }
                this.name = value;
            }
        }

        public static string NameClaimType
        {
            get
            {
                return ClaimTypes.NameIdentifier;
            }
        }

        public string NameFormat
        {
            get
            {
                return this.nameFormat;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.nameFormat = value;
            }
        }

        public string NameQualifier
        {
            get
            {
                return this.nameQualifier;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.nameQualifier = value;
            }
        }

        public string SubjectConfirmationData
        {
            get
            {
                return this.confirmationData;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.confirmationData = value;
            }
        }
    }
}

