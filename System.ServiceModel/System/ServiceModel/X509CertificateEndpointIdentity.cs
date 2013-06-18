namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    public class X509CertificateEndpointIdentity : EndpointIdentity
    {
        private X509Certificate2Collection certificateCollection;

        public X509CertificateEndpointIdentity(X509Certificate2 certificate)
        {
            this.certificateCollection = new X509Certificate2Collection();
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            base.Initialize(new Claim(ClaimTypes.Thumbprint, certificate.GetCertHash(), Rights.PossessProperty));
            this.certificateCollection.Add(certificate);
        }

        internal X509CertificateEndpointIdentity(XmlDictionaryReader reader)
        {
            this.certificateCollection = new X509Certificate2Collection();
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedEmptyElementExpectingClaim", new object[] { XD.AddressingDictionary.X509v3Certificate.Value, XD.AddressingDictionary.IdentityExtensionNamespace.Value })));
            }
            reader.ReadStartElement(XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
            while (reader.IsStartElement(XD.XmlSignatureDictionary.X509Certificate, XD.XmlSignatureDictionary.Namespace))
            {
                X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(reader.ReadElementString()));
                if (this.certificateCollection.Count == 0)
                {
                    base.Initialize(new Claim(ClaimTypes.Thumbprint, certificate.GetCertHash(), Rights.PossessProperty));
                }
                this.certificateCollection.Add(certificate);
            }
            reader.ReadEndElement();
            if (this.certificateCollection.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedEmptyElementExpectingClaim", new object[] { XD.AddressingDictionary.X509v3Certificate.Value, XD.AddressingDictionary.IdentityExtensionNamespace.Value })));
            }
        }

        public X509CertificateEndpointIdentity(X509Certificate2 primaryCertificate, X509Certificate2Collection supportingCertificates)
        {
            this.certificateCollection = new X509Certificate2Collection();
            if (primaryCertificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("primaryCertificate");
            }
            if (supportingCertificates == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("supportingCertificates");
            }
            base.Initialize(new Claim(ClaimTypes.Thumbprint, primaryCertificate.GetCertHash(), Rights.PossessProperty));
            this.certificateCollection.Add(primaryCertificate);
            for (int i = 0; i < supportingCertificates.Count; i++)
            {
                this.certificateCollection.Add(supportingCertificates[i]);
            }
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace);
            writer.WriteStartElement(XD.XmlSignatureDictionary.Prefix.Value, XD.XmlSignatureDictionary.X509Data, XD.XmlSignatureDictionary.Namespace);
            for (int i = 0; i < this.certificateCollection.Count; i++)
            {
                writer.WriteElementString(XD.XmlSignatureDictionary.X509Certificate, XD.XmlSignatureDictionary.Namespace, Convert.ToBase64String(this.certificateCollection[i].RawData));
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public X509Certificate2Collection Certificates
        {
            get
            {
                return this.certificateCollection;
            }
        }
    }
}

