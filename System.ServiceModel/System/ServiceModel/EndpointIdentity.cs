namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    public abstract class EndpointIdentity
    {
        private IEqualityComparer<Claim> claimComparer;
        internal const StoreLocation defaultStoreLocation = StoreLocation.LocalMachine;
        internal const StoreName defaultStoreName = StoreName.My;
        internal const X509FindType defaultX509FindType = X509FindType.FindBySubjectDistinguishedName;
        private Claim identityClaim;

        protected EndpointIdentity()
        {
        }

        public static EndpointIdentity CreateDnsIdentity(string dnsName)
        {
            return new DnsEndpointIdentity(dnsName);
        }

        public static EndpointIdentity CreateIdentity(Claim identity)
        {
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            if (identity.ClaimType.Equals(ClaimTypes.Dns))
            {
                return new DnsEndpointIdentity(identity);
            }
            if (identity.ClaimType.Equals(ClaimTypes.Spn))
            {
                return new SpnEndpointIdentity(identity);
            }
            if (identity.ClaimType.Equals(ClaimTypes.Upn))
            {
                return new UpnEndpointIdentity(identity);
            }
            if (identity.ClaimType.Equals(ClaimTypes.Rsa))
            {
                return new RsaEndpointIdentity(identity);
            }
            return new GeneralEndpointIdentity(identity);
        }

        public static EndpointIdentity CreateRsaIdentity(X509Certificate2 certificate)
        {
            return new RsaEndpointIdentity(certificate);
        }

        public static EndpointIdentity CreateRsaIdentity(string publicKey)
        {
            return new RsaEndpointIdentity(publicKey);
        }

        public static EndpointIdentity CreateSpnIdentity(string spnName)
        {
            return new SpnEndpointIdentity(spnName);
        }

        public static EndpointIdentity CreateUpnIdentity(string upnName)
        {
            return new UpnEndpointIdentity(upnName);
        }

        public static EndpointIdentity CreateX509CertificateIdentity(X509Certificate2 certificate)
        {
            return new X509CertificateEndpointIdentity(certificate);
        }

        internal static EndpointIdentity CreateX509CertificateIdentity(X509Chain certificateChain)
        {
            if (certificateChain == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificateChain");
            }
            if (certificateChain.ChainElements.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("X509ChainIsEmpty"));
            }
            X509Certificate2 primaryCertificate = certificateChain.ChainElements[0].Certificate;
            X509Certificate2Collection supportingCertificates = new X509Certificate2Collection();
            for (int i = 1; i < certificateChain.ChainElements.Count; i++)
            {
                supportingCertificates.Add(certificateChain.ChainElements[i].Certificate);
            }
            return new X509CertificateEndpointIdentity(primaryCertificate, supportingCertificates);
        }

        public static EndpointIdentity CreateX509CertificateIdentity(X509Certificate2 primaryCertificate, X509Certificate2Collection supportingCertificates)
        {
            return new X509CertificateEndpointIdentity(primaryCertificate, supportingCertificates);
        }

        internal virtual void EnsureIdentityClaim()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            EndpointIdentity identity = obj as EndpointIdentity;
            if (identity == null)
            {
                return false;
            }
            return this.Matches(identity.IdentityClaim);
        }

        private IEqualityComparer<Claim> GetClaimComparer()
        {
            if (this.claimComparer == null)
            {
                this.claimComparer = Claim.DefaultComparer;
            }
            return this.claimComparer;
        }

        public override int GetHashCode()
        {
            return this.GetClaimComparer().GetHashCode(this.IdentityClaim);
        }

        protected void Initialize(Claim identityClaim)
        {
            if (identityClaim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identityClaim");
            }
            this.Initialize(identityClaim, null);
        }

        protected void Initialize(Claim identityClaim, IEqualityComparer<Claim> claimComparer)
        {
            if (identityClaim == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identityClaim");
            }
            this.identityClaim = identityClaim;
            this.claimComparer = claimComparer;
        }

        internal bool Matches(Claim claim)
        {
            return this.GetClaimComparer().Equals(this.IdentityClaim, claim);
        }

        internal static EndpointIdentity ReadIdentity(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            EndpointIdentity identity = null;
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedEmptyElementExpectingClaim", new object[] { XD.AddressingDictionary.Identity.Value, XD.AddressingDictionary.IdentityExtensionNamespace.Value })));
            }
            reader.ReadStartElement(XD.AddressingDictionary.Identity, XD.AddressingDictionary.IdentityExtensionNamespace);
            if (reader.IsStartElement(XD.AddressingDictionary.Spn, XD.AddressingDictionary.IdentityExtensionNamespace))
            {
                identity = new SpnEndpointIdentity(reader.ReadElementString());
            }
            else if (reader.IsStartElement(XD.AddressingDictionary.Upn, XD.AddressingDictionary.IdentityExtensionNamespace))
            {
                identity = new UpnEndpointIdentity(reader.ReadElementString());
            }
            else if (reader.IsStartElement(XD.AddressingDictionary.Dns, XD.AddressingDictionary.IdentityExtensionNamespace))
            {
                identity = new DnsEndpointIdentity(reader.ReadElementString());
            }
            else if (reader.IsStartElement(XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace))
            {
                reader.ReadStartElement();
                if (reader.IsStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace))
                {
                    identity = new X509CertificateEndpointIdentity(reader);
                }
                else
                {
                    if (!reader.IsStartElement(XD.XmlSignatureDictionary.RsaKeyValue, XD.XmlSignatureDictionary.Namespace))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnrecognizedIdentityType", new object[] { reader.Name, reader.NamespaceURI })));
                    }
                    identity = new RsaEndpointIdentity(reader);
                }
                reader.ReadEndElement();
            }
            else
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnrecognizedIdentityType", new object[] { reader.Name, reader.NamespaceURI })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidIdentityElement")));
            }
            reader.ReadEndElement();
            return identity;
        }

        public override string ToString()
        {
            return ("identity(" + this.IdentityClaim + ")");
        }

        internal virtual void WriteContentsTo(XmlDictionaryWriter writer)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnrecognizedIdentityPropertyType", new object[] { this.IdentityClaim.GetType().ToString() })));
        }

        internal void WriteTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteStartElement(XD.AddressingDictionary.Identity, XD.AddressingDictionary.IdentityExtensionNamespace);
            this.WriteContentsTo(writer);
            writer.WriteEndElement();
        }

        public Claim IdentityClaim
        {
            get
            {
                if (this.identityClaim == null)
                {
                    this.EnsureIdentityClaim();
                }
                return this.identityClaim;
            }
        }
    }
}

