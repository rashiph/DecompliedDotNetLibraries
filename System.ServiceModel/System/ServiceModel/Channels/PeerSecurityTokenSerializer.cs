namespace System.ServiceModel.Channels
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class PeerSecurityTokenSerializer : WSSecurityTokenSerializer
    {
        public override SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            return null;
        }
    }
}

