namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public abstract class SamlStatement
    {
        protected SamlStatement()
        {
        }

        public abstract IAuthorizationPolicy CreatePolicy(ClaimSet issuer, SamlSecurityTokenAuthenticator samlAuthenticator);
        public abstract void MakeReadOnly();
        public abstract void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver);
        public abstract void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer);

        public abstract bool IsReadOnly { get; }
    }
}

