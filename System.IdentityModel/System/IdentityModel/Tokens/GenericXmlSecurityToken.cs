namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IO;
    using System.Xml;

    public class GenericXmlSecurityToken : SecurityToken
    {
        private ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        private DateTime effectiveTime;
        private DateTime expirationTime;
        private SecurityKeyIdentifierClause externalTokenReference;
        private string id;
        private SecurityKeyIdentifierClause internalTokenReference;
        private SecurityToken proofToken;
        private const int SupportedPersistanceVersion = 1;
        private XmlElement tokenXml;

        public GenericXmlSecurityToken(XmlElement tokenXml, SecurityToken proofToken, DateTime effectiveTime, DateTime expirationTime, SecurityKeyIdentifierClause internalTokenReference, SecurityKeyIdentifierClause externalTokenReference, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (tokenXml == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenXml");
            }
            this.id = GetId(tokenXml);
            this.tokenXml = tokenXml;
            this.proofToken = proofToken;
            this.effectiveTime = effectiveTime.ToUniversalTime();
            this.expirationTime = expirationTime.ToUniversalTime();
            this.internalTokenReference = internalTokenReference;
            this.externalTokenReference = externalTokenReference;
            this.authorizationPolicies = authorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
        }

        public override bool CanCreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            return (((this.internalTokenReference != null) && (typeof(T) == this.internalTokenReference.GetType())) || ((this.externalTokenReference != null) && (typeof(T) == this.externalTokenReference.GetType())));
        }

        public override T CreateKeyIdentifierClause<T>() where T: SecurityKeyIdentifierClause
        {
            if ((this.internalTokenReference != null) && (typeof(T) == this.internalTokenReference.GetType()))
            {
                return (T) this.internalTokenReference;
            }
            if ((this.externalTokenReference == null) || (typeof(T) != this.externalTokenReference.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("UnableToCreateTokenReference")));
            }
            return (T) this.externalTokenReference;
        }

        private static string GetId(XmlElement tokenXml)
        {
            if (tokenXml != null)
            {
                string attribute = tokenXml.GetAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                if (string.IsNullOrEmpty(attribute))
                {
                    attribute = tokenXml.GetAttribute("AssertionID");
                    if (string.IsNullOrEmpty(attribute))
                    {
                        attribute = tokenXml.GetAttribute("Id");
                    }
                }
                if (!string.IsNullOrEmpty(attribute))
                {
                    return attribute;
                }
            }
            return null;
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return (((this.internalTokenReference != null) && this.internalTokenReference.Matches(keyIdentifierClause)) || ((this.externalTokenReference != null) && this.externalTokenReference.Matches(keyIdentifierClause)));
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            writer.WriteLine("Generic XML token:");
            writer.WriteLine("   validFrom: {0}", this.ValidFrom);
            writer.WriteLine("   validTo: {0}", this.ValidTo);
            if (this.internalTokenReference != null)
            {
                writer.WriteLine("   InternalTokenReference: {0}", this.internalTokenReference);
            }
            if (this.externalTokenReference != null)
            {
                writer.WriteLine("   ExternalTokenReference: {0}", this.externalTokenReference);
            }
            writer.WriteLine("   Token Element: ({0}, {1})", this.tokenXml.LocalName, this.tokenXml.NamespaceURI);
            return writer.ToString();
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get
            {
                return this.authorizationPolicies;
            }
        }

        public SecurityKeyIdentifierClause ExternalTokenReference
        {
            get
            {
                return this.externalTokenReference;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public SecurityKeyIdentifierClause InternalTokenReference
        {
            get
            {
                return this.internalTokenReference;
            }
        }

        public SecurityToken ProofToken
        {
            get
            {
                return this.proofToken;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.proofToken != null)
                {
                    return this.proofToken.SecurityKeys;
                }
                return EmptyReadOnlyCollection<SecurityKey>.Instance;
            }
        }

        public XmlElement TokenXml
        {
            get
            {
                return this.tokenXml;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return this.expirationTime;
            }
        }
    }
}

