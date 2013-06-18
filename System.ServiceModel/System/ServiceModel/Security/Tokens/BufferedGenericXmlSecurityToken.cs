namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.Xml;

    internal class BufferedGenericXmlSecurityToken : GenericXmlSecurityToken
    {
        private XmlBuffer tokenXmlBuffer;

        public BufferedGenericXmlSecurityToken(XmlElement tokenXml, SecurityToken proofToken, DateTime effectiveTime, DateTime expirationTime, SecurityKeyIdentifierClause internalTokenReference, SecurityKeyIdentifierClause externalTokenReference, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies, XmlBuffer tokenXmlBuffer) : base(tokenXml, proofToken, effectiveTime, expirationTime, internalTokenReference, externalTokenReference, authorizationPolicies)
        {
            this.tokenXmlBuffer = tokenXmlBuffer;
        }

        public XmlBuffer TokenXmlBuffer
        {
            get
            {
                return this.tokenXmlBuffer;
            }
        }
    }
}

