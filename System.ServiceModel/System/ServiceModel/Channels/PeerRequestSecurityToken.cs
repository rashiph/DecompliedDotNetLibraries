namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerRequestSecurityToken : RequestSecurityToken
    {
        public const string PeerHashTokenElementName = "PeerHashToken";
        public const string PeerNamespace = "http://schemas.microsoft.com/net/2006/05/peer";
        public const string RequestedSecurityTokenElementName = "RequestedSecurityToken";
        public const string RequestElementName = "RequestSecurityToken";
        private PeerHashToken token;
        public const string TrustNamespace = "http://schemas.xmlsoap.org/ws/2005/02/trust";

        public PeerRequestSecurityToken(PeerHashToken token)
        {
            this.token = token;
            base.TokenType = "http://schemas.microsoft.com/net/2006/05/peer/peerhashtoken";
            base.RequestType = "http://schemas.xmlsoap.org/ws/2005/02/trust/Validate";
        }

        internal static bool CompareWithNS(string first, string firstNS, string second, string secondNS)
        {
            return ((string.Compare(first, second, StringComparison.Ordinal) == 0) && (string.Compare(firstNS, secondNS, StringComparison.OrdinalIgnoreCase) == 0));
        }

        public PeerRequestSecurityToken CreateFrom(X509Certificate2 credential, string password)
        {
            return new PeerRequestSecurityToken(new PeerHashToken(credential, password));
        }

        public static PeerHashToken CreateHashTokenFrom(Message message)
        {
            PeerHashToken invalid = PeerHashToken.Invalid;
            RequestSecurityToken token2 = RequestSecurityToken.CreateFrom(message.GetReaderAtBodyContents());
            if (token2.RequestSecurityTokenXml != null)
            {
                foreach (System.Xml.XmlNode node in token2.RequestSecurityTokenXml.ChildNodes)
                {
                    XmlElement child = (XmlElement) node;
                    if ((child != null) && CompareWithNS(child.LocalName, child.NamespaceURI, "RequestedSecurityToken", "http://schemas.xmlsoap.org/ws/2005/02/trust"))
                    {
                        invalid = PeerHashToken.CreateFrom(child);
                    }
                }
            }
            return invalid;
        }

        protected internal override void OnMakeReadOnly()
        {
        }

        protected internal override void OnWriteCustomElements(XmlWriter writer)
        {
            if ((this.token == null) || !this.token.IsValid)
            {
                throw Fx.AssertAndThrow("Could not construct a valid RST without token!");
            }
            string prefix = writer.LookupPrefix("http://schemas.xmlsoap.org/ws/2005/02/trust");
            writer.WriteStartElement(prefix, "RequestedSecurityToken", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            this.token.Write(writer);
            writer.WriteEndElement();
        }

        public PeerHashToken Token
        {
            get
            {
                return this.token;
            }
        }
    }
}

