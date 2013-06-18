namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class PeerTransportSecuritySettings
    {
        private PeerTransportCredentialType credentialType;
        internal const PeerTransportCredentialType DefaultCredentialType = PeerTransportCredentialType.Password;

        public PeerTransportSecuritySettings()
        {
            this.credentialType = PeerTransportCredentialType.Password;
        }

        internal PeerTransportSecuritySettings(PeerTransportSecurityElement element)
        {
            this.credentialType = element.CredentialType;
        }

        internal PeerTransportSecuritySettings(PeerTransportSecuritySettings other)
        {
            this.credentialType = other.credentialType;
        }

        internal void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            string str = "";
            switch (this.CredentialType)
            {
                case PeerTransportCredentialType.Password:
                    str = "PeerTransportCredentialTypePassword";
                    break;

                case PeerTransportCredentialType.Certificate:
                    str = "PeerTransportCredentialTypeCertificate";
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            XmlElement item = new XmlDocument().CreateElement("pc", "PeerTransportCredentialType", "http://schemas.microsoft.com/soap/peer");
            item.InnerText = str;
            context.GetBindingAssertions().Add(item);
        }

        internal void OnImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            string str;
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "PeerTransportCredentialType", "http://schemas.microsoft.com/soap/peer", true);
            PeerTransportCredentialType password = PeerTransportCredentialType.Password;
            if ((element != null) && ((str = element.InnerText) != null))
            {
                if (!(str == "PeerTransportCredentialTypePassword"))
                {
                    if (str == "PeerTransportCredentialTypeCertificate")
                    {
                        password = PeerTransportCredentialType.Certificate;
                    }
                }
                else
                {
                    password = PeerTransportCredentialType.Password;
                }
            }
            this.CredentialType = password;
        }

        public PeerTransportCredentialType CredentialType
        {
            get
            {
                return this.credentialType;
            }
            set
            {
                if (!PeerTransportCredentialTypeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(PeerTransportCredentialType)));
                }
                this.credentialType = value;
            }
        }
    }
}

