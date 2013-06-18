namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Xml;

    internal class TokenElement : ISecurityElement
    {
        private SecurityStandardsManager standardsManager;
        private SecurityToken token;

        public TokenElement(SecurityToken token, SecurityStandardsManager standardsManager)
        {
            this.token = token;
            this.standardsManager = standardsManager;
        }

        public override bool Equals(object item)
        {
            TokenElement element = item as TokenElement;
            return (((element != null) && (this.token == element.token)) && (this.standardsManager == element.standardsManager));
        }

        public override int GetHashCode()
        {
            return (this.token.GetHashCode() ^ this.standardsManager.GetHashCode());
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.standardsManager.SecurityTokenSerializer.WriteToken(writer, this.token);
        }

        public bool HasId
        {
            get
            {
                return true;
            }
        }

        public string Id
        {
            get
            {
                return this.token.Id;
            }
        }
    }
}

