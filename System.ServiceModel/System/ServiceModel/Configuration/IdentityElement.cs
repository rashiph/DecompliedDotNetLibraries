namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public sealed class IdentityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void Copy(IdentityElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.UserPrincipalName.Value = source.UserPrincipalName.Value;
            }
            if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ServicePrincipalName.Value = source.ServicePrincipalName.Value;
            }
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.EncodedValue = source.Certificate.EncodedValue;
            }
            if (properties["certificateReference"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.CertificateReference.StoreName = source.CertificateReference.StoreName;
                this.CertificateReference.StoreLocation = source.CertificateReference.StoreLocation;
                this.CertificateReference.X509FindType = source.CertificateReference.X509FindType;
                this.CertificateReference.FindValue = source.CertificateReference.FindValue;
            }
        }

        public void InitializeFrom(EndpointIdentity identity)
        {
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            Claim identityClaim = identity.IdentityClaim;
            if (ClaimTypes.Dns.Equals(identityClaim.ClaimType))
            {
                this.Dns.Value = (string) identityClaim.Resource;
            }
            else if (ClaimTypes.Spn.Equals(identityClaim.ClaimType))
            {
                this.ServicePrincipalName.Value = (string) identityClaim.Resource;
            }
            else if (ClaimTypes.Upn.Equals(identityClaim.ClaimType))
            {
                this.UserPrincipalName.Value = (string) identityClaim.Resource;
            }
            else if (ClaimTypes.Rsa.Equals(identityClaim.ClaimType))
            {
                this.Rsa.Value = ((RSA) identityClaim.Resource).ToXmlString(false);
            }
            else if (identity is X509CertificateEndpointIdentity)
            {
                X509Certificate2Collection certificates = ((X509CertificateEndpointIdentity) identity).Certificates;
                this.Certificate.EncodedValue = Convert.ToBase64String(certificates.Export((certificates.Count == 1) ? X509ContentType.SerializedCert : X509ContentType.SerializedStore));
            }
        }

        [ConfigurationProperty("certificate")]
        public CertificateElement Certificate
        {
            get
            {
                return (CertificateElement) base["certificate"];
            }
        }

        [ConfigurationProperty("certificateReference")]
        public CertificateReferenceElement CertificateReference
        {
            get
            {
                return (CertificateReferenceElement) base["certificateReference"];
            }
        }

        [ConfigurationProperty("dns")]
        public DnsElement Dns
        {
            get
            {
                return (DnsElement) base["dns"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("userPrincipalName", typeof(UserPrincipalNameElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("servicePrincipalName", typeof(ServicePrincipalNameElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("dns", typeof(DnsElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("rsa", typeof(RsaElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("certificate", typeof(CertificateElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("certificateReference", typeof(CertificateReferenceElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("rsa")]
        public RsaElement Rsa
        {
            get
            {
                return (RsaElement) base["rsa"];
            }
        }

        [ConfigurationProperty("servicePrincipalName")]
        public ServicePrincipalNameElement ServicePrincipalName
        {
            get
            {
                return (ServicePrincipalNameElement) base["servicePrincipalName"];
            }
        }

        [ConfigurationProperty("userPrincipalName")]
        public UserPrincipalNameElement UserPrincipalName
        {
            get
            {
                return (UserPrincipalNameElement) base["userPrincipalName"];
            }
        }
    }
}

