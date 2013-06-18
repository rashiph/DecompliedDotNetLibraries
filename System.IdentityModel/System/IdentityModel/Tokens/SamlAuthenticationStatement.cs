namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAuthenticationStatement : SamlSubjectStatement
    {
        private DateTime authenticationInstant;
        private string authenticationMethod;
        private readonly ImmutableCollection<SamlAuthorityBinding> authorityBindings;
        private string dnsAddress;
        private string ipAddress;
        private bool isReadOnly;

        public SamlAuthenticationStatement()
        {
            this.authenticationInstant = DateTime.UtcNow.ToUniversalTime();
            this.authenticationMethod = XD.SamlDictionary.UnspecifiedAuthenticationMethod.Value;
            this.authorityBindings = new ImmutableCollection<SamlAuthorityBinding>();
        }

        public SamlAuthenticationStatement(SamlSubject samlSubject, string authenticationMethod, DateTime authenticationInstant, string dnsAddress, string ipAddress, IEnumerable<SamlAuthorityBinding> authorityBindings) : base(samlSubject)
        {
            this.authenticationInstant = DateTime.UtcNow.ToUniversalTime();
            this.authenticationMethod = XD.SamlDictionary.UnspecifiedAuthenticationMethod.Value;
            this.authorityBindings = new ImmutableCollection<SamlAuthorityBinding>();
            if (string.IsNullOrEmpty(authenticationMethod))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("authenticationMethod", System.IdentityModel.SR.GetString("SAMLAuthenticationStatementMissingAuthenticationMethod"));
            }
            this.authenticationMethod = authenticationMethod;
            this.authenticationInstant = authenticationInstant.ToUniversalTime();
            this.dnsAddress = dnsAddress;
            this.ipAddress = ipAddress;
            if (authorityBindings != null)
            {
                foreach (SamlAuthorityBinding binding in authorityBindings)
                {
                    if (binding == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Assertion.Value }));
                    }
                    this.authorityBindings.Add(binding);
                }
            }
            this.CheckObjectValidity();
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");
            }
            claims.Add(new Claim(ClaimTypes.Authentication, new SamlAuthenticationClaimResource(this.authenticationInstant, this.authenticationMethod, this.dnsAddress, this.ipAddress, this.authorityBindings), Rights.PossessProperty));
        }

        private void CheckObjectValidity()
        {
            if (base.SamlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectStatementRequiresSubject")));
            }
            if (string.IsNullOrEmpty(this.authenticationMethod))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthenticationStatementMissingAuthenticationMethod")));
            }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAuthorityBinding binding in this.authorityBindings)
                {
                    binding.MakeReadOnly();
                }
                this.authorityBindings.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            string attribute = reader.GetAttribute(samlDictionary.AuthenticationInstant, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthenticationStatementMissingAuthenticationInstanceOnRead")));
            }
            this.authenticationInstant = DateTime.ParseExact(attribute, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();
            this.authenticationMethod = reader.GetAttribute(samlDictionary.AuthenticationMethod, null);
            if (string.IsNullOrEmpty(this.authenticationMethod))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthenticationStatementMissingAuthenticationMethodOnRead")));
            }
            reader.MoveToContent();
            reader.Read();
            if (!reader.IsStartElement(samlDictionary.Subject, samlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthenticationStatementMissingSubject")));
            }
            SamlSubject subject = new SamlSubject();
            subject.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
            base.SamlSubject = subject;
            if (reader.IsStartElement(samlDictionary.SubjectLocality, samlDictionary.Namespace))
            {
                this.dnsAddress = reader.GetAttribute(samlDictionary.SubjectLocalityDNSAddress, null);
                this.ipAddress = reader.GetAttribute(samlDictionary.SubjectLocalityIPAddress, null);
                if (reader.IsEmptyElement)
                {
                    reader.MoveToContent();
                    reader.Read();
                }
                else
                {
                    reader.MoveToContent();
                    reader.Read();
                    reader.ReadEndElement();
                }
            }
            while (reader.IsStartElement())
            {
                if (!reader.IsStartElement(samlDictionary.AuthorityBinding, samlDictionary.Namespace))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.AuthenticationStatement })));
                }
                SamlAuthorityBinding item = new SamlAuthorityBinding();
                item.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                this.authorityBindings.Add(item);
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
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
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AuthenticationStatement, samlDictionary.Namespace);
            writer.WriteStartAttribute(samlDictionary.AuthenticationMethod, null);
            writer.WriteString(this.authenticationMethod);
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(samlDictionary.AuthenticationInstant, null);
            writer.WriteString(this.authenticationInstant.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();
            base.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);
            if ((this.ipAddress != null) || (this.dnsAddress != null))
            {
                writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.SubjectLocality, samlDictionary.Namespace);
                if (this.ipAddress != null)
                {
                    writer.WriteStartAttribute(samlDictionary.SubjectLocalityIPAddress, null);
                    writer.WriteString(this.ipAddress);
                    writer.WriteEndAttribute();
                }
                if (this.dnsAddress != null)
                {
                    writer.WriteStartAttribute(samlDictionary.SubjectLocalityDNSAddress, null);
                    writer.WriteString(this.dnsAddress);
                    writer.WriteEndAttribute();
                }
                writer.WriteEndElement();
            }
            for (int i = 0; i < this.authorityBindings.Count; i++)
            {
                this.authorityBindings[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            writer.WriteEndElement();
        }

        public DateTime AuthenticationInstant
        {
            get
            {
                return this.authenticationInstant;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.authenticationInstant = value;
            }
        }

        public string AuthenticationMethod
        {
            get
            {
                return this.authenticationMethod;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    this.authenticationMethod = XD.SamlDictionary.UnspecifiedAuthenticationMethod.Value;
                }
                else
                {
                    this.authenticationMethod = value;
                }
            }
        }

        public IList<SamlAuthorityBinding> AuthorityBindings
        {
            get
            {
                return this.authorityBindings;
            }
        }

        public static string ClaimType
        {
            get
            {
                return ClaimTypes.Authentication;
            }
        }

        public string DnsAddress
        {
            get
            {
                return this.dnsAddress;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.dnsAddress = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.ipAddress = value;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }
    }
}

