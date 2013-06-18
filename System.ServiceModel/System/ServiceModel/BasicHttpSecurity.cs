namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public sealed class BasicHttpSecurity
    {
        internal const BasicHttpSecurityMode DefaultMode = BasicHttpSecurityMode.None;
        private BasicHttpMessageSecurity messageSecurity;
        private BasicHttpSecurityMode mode;
        private HttpTransportSecurity transportSecurity;

        public BasicHttpSecurity() : this(BasicHttpSecurityMode.None, new HttpTransportSecurity(), new BasicHttpMessageSecurity())
        {
        }

        private BasicHttpSecurity(BasicHttpSecurityMode mode, HttpTransportSecurity transportSecurity, BasicHttpMessageSecurity messageSecurity)
        {
            this.Mode = mode;
            this.transportSecurity = (transportSecurity == null) ? new HttpTransportSecurity() : transportSecurity;
            this.messageSecurity = (messageSecurity == null) ? new BasicHttpMessageSecurity() : messageSecurity;
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            if ((this.mode != BasicHttpSecurityMode.Message) && (this.mode != BasicHttpSecurityMode.TransportWithMessageCredential))
            {
                return null;
            }
            return this.messageSecurity.CreateMessageSecurity(this.Mode == BasicHttpSecurityMode.TransportWithMessageCredential);
        }

        internal void DisableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.DisableTransportAuthentication(http);
        }

        internal void EnableTransportAuthentication(HttpTransportBindingElement http)
        {
            this.transportSecurity.ConfigureTransportAuthentication(http);
        }

        internal void EnableTransportSecurity(HttpsTransportBindingElement https)
        {
            if (this.mode == BasicHttpSecurityMode.TransportWithMessageCredential)
            {
                this.transportSecurity.ConfigureTransportProtectionOnly(https);
            }
            else
            {
                this.transportSecurity.ConfigureTransportProtectionAndAuthentication(https);
            }
        }

        internal static void EnableTransportSecurity(HttpsTransportBindingElement https, HttpTransportSecurity transportSecurity)
        {
            HttpTransportSecurity.ConfigureTransportProtectionAndAuthentication(https, transportSecurity);
        }

        internal bool InternalShouldSerialize()
        {
            if ((this.Mode == BasicHttpSecurityMode.None) && !this.ShouldSerializeMessage())
            {
                return this.ShouldSerializeTransport();
            }
            return true;
        }

        internal static bool IsEnabledTransportAuthentication(HttpTransportBindingElement http, HttpTransportSecurity transportSecurity)
        {
            return HttpTransportSecurity.IsConfiguredTransportAuthentication(http, transportSecurity);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.messageSecurity.InternalShouldSerialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.transportSecurity.InternalShouldSerialize();
        }

        internal static bool TryCreate(SecurityBindingElement sbe, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, out BasicHttpSecurity security)
        {
            security = null;
            BasicHttpMessageSecurity security2 = null;
            if (sbe != null)
            {
                bool flag;
                mode &= UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message;
                if (!BasicHttpMessageSecurity.TryCreate(sbe, out security2, out flag))
                {
                    return false;
                }
            }
            else
            {
                mode &= ~(UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Message);
            }
            BasicHttpSecurityMode mode2 = BasicHttpSecurityModeHelper.ToSecurityMode(mode);
            security = new BasicHttpSecurity(mode2, transportSecurity, security2);
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(), sbe);
        }

        public BasicHttpMessageSecurity Message
        {
            get
            {
                return this.messageSecurity;
            }
            set
            {
                this.messageSecurity = (value == null) ? new BasicHttpMessageSecurity() : value;
            }
        }

        public BasicHttpSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!BasicHttpSecurityModeHelper.IsDefined(value))
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
                this.transportSecurity = (value == null) ? new HttpTransportSecurity() : value;
            }
        }
    }
}

