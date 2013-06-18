namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class PeerHashToken : SecurityToken
    {
        internal const string Action = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate";
        private byte[] authenticator;
        private DateTime effectiveTime;
        private DateTime expirationTime;
        private string id;
        private static PeerHashToken invalid = new PeerHashToken();
        private bool isValid;
        private ReadOnlyCollection<SecurityKey> keys;
        public const string PeerAuthenticatorElementName = "Authenticator";
        public const string PeerNamespace = "http://schemas.microsoft.com/net/2006/05/peer";
        public const string PeerPrefix = "peer";
        public const string PeerTokenElementName = "PeerHashToken";
        internal const string RequestTypeString = "http://schemas.xmlsoap.org/ws/2005/02/trust/Validate";
        private Uri status;
        internal const string TokenTypeString = "http://schemas.microsoft.com/net/2006/05/peer/peerhashtoken";

        private PeerHashToken()
        {
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = DateTime.UtcNow.AddHours(10.0);
            this.CheckValidity();
        }

        public PeerHashToken(byte[] authenticator)
        {
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = DateTime.UtcNow.AddHours(10.0);
            this.authenticator = authenticator;
            this.CheckValidity();
        }

        public PeerHashToken(Claim claim, string password)
        {
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = DateTime.UtcNow.AddHours(10.0);
            this.authenticator = PeerSecurityHelpers.ComputeHash(claim, password);
            this.CheckValidity();
        }

        public PeerHashToken(X509Certificate2 certificate, string password)
        {
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            this.expirationTime = DateTime.UtcNow.AddHours(10.0);
            this.authenticator = PeerSecurityHelpers.ComputeHash(certificate, password);
            this.CheckValidity();
        }

        private void CheckValidity()
        {
            this.isValid = this.authenticator != null;
            this.status = new Uri(this.isValid ? "http://schemas.xmlsoap.org/ws/2005/02/trust/status/valid" : "http://schemas.xmlsoap.org/ws/2005/02/trust/status/invalid");
        }

        internal static PeerHashToken CreateFrom(XmlElement child)
        {
            byte[] authenticator = null;
            foreach (System.Xml.XmlNode node in child.ChildNodes)
            {
                XmlElement element = (XmlElement) node;
                if ((element != null) && PeerRequestSecurityToken.CompareWithNS(element.LocalName, element.NamespaceURI, "PeerHashToken", "http://schemas.microsoft.com/net/2006/05/peer"))
                {
                    if (element.ChildNodes.Count != 1)
                    {
                        break;
                    }
                    XmlElement element2 = element.ChildNodes[0] as XmlElement;
                    if ((element2 == null) || !PeerRequestSecurityToken.CompareWithNS(element2.LocalName, element2.NamespaceURI, "Authenticator", "http://schemas.microsoft.com/net/2006/05/peer"))
                    {
                        break;
                    }
                    try
                    {
                        authenticator = Convert.FromBase64String(XmlHelper.ReadTextElementAsTrimmedString(element2));
                        break;
                    }
                    catch (ArgumentNullException exception)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    catch (FormatException exception2)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
            }
            return new PeerHashToken(authenticator);
        }

        public override bool Equals(object token)
        {
            PeerHashToken objA = token as PeerHashToken;
            if (objA == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(objA, this))
            {
                if (((this.authenticator == null) || (objA.authenticator == null)) || (this.authenticator.Length != objA.authenticator.Length))
                {
                    return false;
                }
                for (int i = 0; i < this.authenticator.Length; i++)
                {
                    if (this.authenticator[i] != objA.authenticator[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (!this.isValid)
            {
                return 0;
            }
            return this.authenticator.GetHashCode();
        }

        public bool Validate(Claim claim, string password)
        {
            if (this.authenticator == null)
            {
                throw Fx.AssertAndThrow("Incorrect initialization");
            }
            return PeerSecurityHelpers.Authenticate(claim, password, this.authenticator);
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("peer", "PeerHashToken", "http://schemas.microsoft.com/net/2006/05/peer");
            writer.WriteStartElement("peer", "Authenticator", "http://schemas.microsoft.com/net/2006/05/peer");
            writer.WriteString(Convert.ToBase64String(this.authenticator));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public static PeerHashToken Invalid
        {
            get
            {
                return invalid;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new ReadOnlyCollection<SecurityKey>(new List<SecurityKey>());
                }
                return this.keys;
            }
        }

        public Uri Status
        {
            get
            {
                return this.status;
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

