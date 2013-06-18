namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class PeerSecuritySettings
    {
        internal const SecurityMode DefaultMode = SecurityMode.Transport;
        private SecurityMode mode;
        private PeerTransportSecuritySettings transportSecurity;

        public PeerSecuritySettings()
        {
            this.mode = SecurityMode.Transport;
            this.transportSecurity = new PeerTransportSecuritySettings();
        }

        internal PeerSecuritySettings(PeerSecurityElement element)
        {
            this.mode = element.Mode;
            this.transportSecurity = new PeerTransportSecuritySettings(element.Transport);
        }

        internal PeerSecuritySettings(PeerSecuritySettings other)
        {
            this.mode = other.mode;
            this.transportSecurity = new PeerTransportSecuritySettings(other.transportSecurity);
        }

        internal bool InternalShouldSerialize()
        {
            if (!this.ShouldSerializeMode())
            {
                return this.ShouldSerializeTransport();
            }
            return true;
        }

        internal void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            string str = "";
            switch (this.Mode)
            {
                case SecurityMode.None:
                    str = "PeerTransportSecurityModeNone";
                    break;

                case SecurityMode.Transport:
                    str = "PeerTransportSecurityModeTransport";
                    break;

                case SecurityMode.Message:
                    str = "PeerTransportSecurityModeMessage";
                    break;

                case SecurityMode.TransportWithMessageCredential:
                    str = "PeerTransportSecurityModeTransportWithMessageCredential";
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            XmlElement item = new XmlDocument().CreateElement("pc", "PeerTransportSecurityMode", "http://schemas.microsoft.com/soap/peer");
            item.InnerText = str;
            context.GetBindingAssertions().Add(item);
            this.transportSecurity.OnExportPolicy(exporter, context);
        }

        internal void OnImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            string str;
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "PeerTransportSecurityMode", "http://schemas.microsoft.com/soap/peer", true);
            this.Mode = SecurityMode.Transport;
            if ((element != null) && ((str = element.InnerText) != null))
            {
                if (!(str == "PeerTransportSecurityModeNone"))
                {
                    if (str == "PeerTransportSecurityModeTransport")
                    {
                        this.Mode = SecurityMode.Transport;
                    }
                    else if (str == "PeerTransportSecurityModeMessage")
                    {
                        this.Mode = SecurityMode.Message;
                    }
                    else if (str == "PeerTransportSecurityModeTransportWithMessageCredential")
                    {
                        this.Mode = SecurityMode.TransportWithMessageCredential;
                    }
                }
                else
                {
                    this.Mode = SecurityMode.None;
                }
            }
            this.transportSecurity.OnImportPolicy(importer, context);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return (this.Mode != SecurityMode.Transport);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return (this.Transport.CredentialType != PeerTransportCredentialType.Password);
        }

        public SecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!SecurityModeHelper.IsDefined(value))
                {
                    PeerExceptionHelper.ThrowArgumentOutOfRange_InvalidSecurityMode((int) value);
                }
                this.mode = value;
            }
        }

        internal ProtectionLevel SupportedProtectionLevel
        {
            get
            {
                ProtectionLevel none = ProtectionLevel.None;
                if ((this.Mode != SecurityMode.Message) && (this.Mode != SecurityMode.TransportWithMessageCredential))
                {
                    return none;
                }
                return ProtectionLevel.Sign;
            }
        }

        internal bool SupportsAuthentication
        {
            get
            {
                if (this.Mode != SecurityMode.Transport)
                {
                    return (this.Mode == SecurityMode.TransportWithMessageCredential);
                }
                return true;
            }
        }

        public PeerTransportSecuritySettings Transport
        {
            get
            {
                return this.transportSecurity;
            }
            set
            {
                this.transportSecurity = value;
            }
        }
    }
}

