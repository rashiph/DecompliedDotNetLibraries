namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerRequestSecurityTokenResponse : RequestSecurityTokenResponse
    {
        public const string Action = "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate";
        public const string CodeString = "Code";
        public const string InvalidString = "http://schemas.xmlsoap.org/ws/2005/02/trust/status/invalid";
        private bool isValid;
        public const string StatusString = "Status";
        private PeerHashToken token;
        public const string ValidString = "http://schemas.xmlsoap.org/ws/2005/02/trust/status/valid";

        public PeerRequestSecurityTokenResponse() : this(null)
        {
        }

        public PeerRequestSecurityTokenResponse(PeerHashToken token)
        {
            this.token = token;
            this.isValid = (token != null) && token.IsValid;
        }

        public static RequestSecurityTokenResponse CreateFrom(X509Certificate2 credential, string password)
        {
            return new PeerRequestSecurityTokenResponse(new PeerHashToken(credential, password));
        }

        public static PeerHashToken CreateHashTokenFrom(Message message)
        {
            PeerHashToken invalid = PeerHashToken.Invalid;
            RequestSecurityTokenResponse response = RequestSecurityTokenResponse.CreateFrom(message.GetReaderAtBodyContents(), MessageSecurityVersion.Default, new PeerSecurityTokenSerializer());
            if (string.Compare(response.TokenType, "http://schemas.microsoft.com/net/2006/05/peer/peerhashtoken", StringComparison.OrdinalIgnoreCase) == 0)
            {
                XmlElement requestSecurityTokenResponseXml = response.RequestSecurityTokenResponseXml;
                if (requestSecurityTokenResponseXml == null)
                {
                    return invalid;
                }
                foreach (XmlElement element2 in requestSecurityTokenResponseXml.ChildNodes)
                {
                    if (PeerRequestSecurityToken.CompareWithNS(element2.LocalName, element2.NamespaceURI, "Status", "http://schemas.xmlsoap.org/ws/2005/02/trust"))
                    {
                        if (element2.ChildNodes.Count == 1)
                        {
                            XmlElement element = element2.ChildNodes[0] as XmlElement;
                            if (PeerRequestSecurityToken.CompareWithNS(element.LocalName, element.NamespaceURI, "Code", "http://schemas.xmlsoap.org/ws/2005/02/trust") && (string.Compare(XmlHelper.ReadTextElementAsTrimmedString(element), "http://schemas.xmlsoap.org/ws/2005/02/trust/status/valid", StringComparison.OrdinalIgnoreCase) != 0))
                            {
                                return invalid;
                            }
                        }
                    }
                    else if (PeerRequestSecurityToken.CompareWithNS(element2.LocalName, element2.NamespaceURI, "RequestedSecurityToken", "http://schemas.xmlsoap.org/ws/2005/02/trust"))
                    {
                        return PeerHashToken.CreateFrom(element2);
                    }
                }
            }
            return invalid;
        }

        protected internal override void OnWriteCustomElements(XmlWriter writer)
        {
            string prefix = writer.LookupPrefix("http://schemas.xmlsoap.org/ws/2005/02/trust");
            writer.WriteStartElement(prefix, "TokenType", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            writer.WriteString("http://schemas.microsoft.com/net/2006/05/peer/peerhashtoken");
            writer.WriteEndElement();
            writer.WriteStartElement(prefix, "Status", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            writer.WriteStartElement(prefix, "Code", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            if (!this.IsValid)
            {
                writer.WriteString("http://schemas.xmlsoap.org/ws/2005/02/trust/status/invalid");
            }
            else
            {
                writer.WriteString("http://schemas.xmlsoap.org/ws/2005/02/trust/status/valid");
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            if (this.IsValid)
            {
                writer.WriteStartElement(prefix, "RequestedSecurityToken", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                this.token.Write(writer);
                writer.WriteEndElement();
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public PeerHashToken Token
        {
            get
            {
                if (!this.isValid)
                {
                    throw Fx.AssertAndThrow("should not be called when the token is invalid!");
                }
                return this.token;
            }
        }
    }
}

