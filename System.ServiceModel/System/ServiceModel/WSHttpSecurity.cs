namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    public sealed class WSHttpSecurity
    {
        internal const SecurityMode DefaultMode = SecurityMode.Message;
        private NonDualMessageSecurityOverHttp messageSecurity;
        private SecurityMode mode;
        private HttpTransportSecurity transportSecurity;

        public WSHttpSecurity() : this(SecurityMode.Message, GetDefaultHttpTransportSecurity(), new NonDualMessageSecurityOverHttp())
        {
        }

        internal WSHttpSecurity(SecurityMode mode, HttpTransportSecurity transportSecurity, NonDualMessageSecurityOverHttp messageSecurity)
        {
            this.mode = mode;
            this.transportSecurity = (transportSecurity == null) ? GetDefaultHttpTransportSecurity() : transportSecurity;
            this.messageSecurity = (messageSecurity == null) ? new NonDualMessageSecurityOverHttp() : messageSecurity;
        }

        internal void ApplyTransportSecurity(HttpsTransportBindingElement https)
        {
            if (this.mode == SecurityMode.TransportWithMessageCredential)
            {
                this.transportSecurity.ConfigureTransportProtectionOnly(https);
            }
            else
            {
                this.transportSecurity.ConfigureTransportProtectionAndAuthentication(https);
            }
        }

        internal static void ApplyTransportSecurity(HttpsTransportBindingElement transport, HttpTransportSecurity transportSecurity)
        {
            HttpTransportSecurity.ConfigureTransportProtectionAndAuthentication(transport, transportSecurity);
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled, MessageSecurityVersion version)
        {
            if ((this.mode != SecurityMode.Message) && (this.mode != SecurityMode.TransportWithMessageCredential))
            {
                return null;
            }
            return this.messageSecurity.CreateSecurityBindingElement(this.Mode == SecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version);
        }

        internal static HttpTransportSecurity GetDefaultHttpTransportSecurity()
        {
            return new HttpTransportSecurity { ClientCredentialType = HttpClientCredentialType.Windows };
        }

        internal bool InternalShouldSerialize()
        {
            if (!this.ShouldSerializeMode() && !this.ShouldSerializeMessage())
            {
                return this.ShouldSerializeTransport();
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.Message.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return (this.Mode != SecurityMode.Message);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            if ((this.Transport.ClientCredentialType == HttpClientCredentialType.Windows) && !this.Transport.ShouldSerializeProxyCredentialType())
            {
                return this.Transport.ShouldSerializeRealm();
            }
            return true;
        }

        internal static bool TryCreate(SecurityBindingElement sbe, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSessionEnabled, out WSHttpSecurity security)
        {
            security = null;
            NonDualMessageSecurityOverHttp messageSecurity = null;
            SecurityMode none = SecurityMode.None;
            if (sbe != null)
            {
                mode &= UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message;
                none = SecurityModeHelper.ToSecurityMode(mode);
                if (!MessageSecurityOverHttp.TryCreate<NonDualMessageSecurityOverHttp>(sbe, none == SecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, out messageSecurity))
                {
                    return false;
                }
            }
            else
            {
                mode &= ~(UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message);
                none = SecurityModeHelper.ToSecurityMode(mode);
            }
            security = new WSHttpSecurity(none, transportSecurity, messageSecurity);
            return true;
        }

        public NonDualMessageSecurityOverHttp Message
        {
            get
            {
                return this.messageSecurity;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurity = value;
            }
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public HttpTransportSecurity Transport
        {
            get
            {
                return this.transportSecurity;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.transportSecurity = value;
            }
        }
    }
}

