namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class WSHttpBinding : WSHttpBindingBase
    {
        private WSHttpSecurity security;
        private static readonly MessageSecurityVersion WSMessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

        public WSHttpBinding()
        {
            this.security = new WSHttpSecurity();
        }

        public WSHttpBinding(SecurityMode securityMode) : this(securityMode, false)
        {
        }

        public WSHttpBinding(string configName) : this()
        {
            this.ApplyConfiguration(configName);
        }

        public WSHttpBinding(SecurityMode securityMode, bool reliableSessionEnabled) : base(reliableSessionEnabled)
        {
            this.security = new WSHttpSecurity();
            this.security.Mode = securityMode;
        }

        internal WSHttpBinding(WSHttpSecurity security, bool reliableSessionEnabled) : base(reliableSessionEnabled)
        {
            this.security = new WSHttpSecurity();
            this.security = (security == null) ? new WSHttpSecurity() : security;
        }

        private void ApplyConfiguration(string configurationName)
        {
            WSHttpBindingElement element2 = WSHttpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "wsHttpBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            if (base.ReliableSession.Enabled && (this.security.Mode == SecurityMode.Transport))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WSHttpDoesNotSupportRMWithHttps")));
            }
            return base.CreateBindingElements();
        }

        protected override SecurityBindingElement CreateMessageSecurity()
        {
            return this.security.CreateMessageSecurity(base.ReliableSession.Enabled, WSMessageSecurityVersion);
        }

        internal static bool GetSecurityModeFromTransport(TransportBindingElement transport, HttpTransportSecurity transportSecurity, out UnifiedSecurityMode mode)
        {
            mode = UnifiedSecurityMode.None;
            if (transport is HttpsTransportBindingElement)
            {
                mode = UnifiedSecurityMode.TransportWithMessageCredential | UnifiedSecurityMode.Transport;
                WSHttpSecurity.ApplyTransportSecurity((HttpsTransportBindingElement) transport, transportSecurity);
            }
            else if (transport is HttpTransportBindingElement)
            {
                mode = UnifiedSecurityMode.Message | UnifiedSecurityMode.None;
            }
            else
            {
                return false;
            }
            return true;
        }

        protected override TransportBindingElement GetTransport()
        {
            if ((this.security.Mode == SecurityMode.None) || (this.security.Mode == SecurityMode.Message))
            {
                base.HttpTransport.ExtendedProtectionPolicy = this.security.Transport.ExtendedProtectionPolicy;
                return base.HttpTransport;
            }
            this.security.ApplyTransportSecurity(base.HttpsTransport);
            return base.HttpsTransport;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }

        internal static bool TryCreate(SecurityBindingElement sbe, TransportBindingElement transport, ReliableSessionBindingElement rsbe, TransactionFlowBindingElement tfbe, out Binding binding)
        {
            UnifiedSecurityMode mode;
            WSHttpSecurity security2;
            bool isReliableSession = rsbe != null;
            binding = null;
            HttpTransportSecurity defaultHttpTransportSecurity = WSHttpSecurity.GetDefaultHttpTransportSecurity();
            if (!GetSecurityModeFromTransport(transport, defaultHttpTransportSecurity, out mode))
            {
                return false;
            }
            HttpsTransportBindingElement element = transport as HttpsTransportBindingElement;
            if (((element != null) && (element.MessageSecurityVersion != null)) && (element.MessageSecurityVersion.SecurityPolicyVersion != WSMessageSecurityVersion.SecurityPolicyVersion))
            {
                return false;
            }
            if (TryCreateSecurity(sbe, mode, defaultHttpTransportSecurity, isReliableSession, out security2))
            {
                bool flag2;
                WSHttpBinding binding2 = new WSHttpBinding(security2, isReliableSession);
                if (!TryGetAllowCookiesFromTransport(transport, out flag2))
                {
                    return false;
                }
                binding2.AllowCookies = flag2;
                binding = binding2;
            }
            if ((rsbe != null) && (rsbe.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessagingFebruary2005))
            {
                return false;
            }
            if ((tfbe != null) && (tfbe.TransactionProtocol != TransactionProtocol.WSAtomicTransactionOctober2004))
            {
                return false;
            }
            return (binding != null);
        }

        private static bool TryCreateSecurity(SecurityBindingElement sbe, UnifiedSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSession, out WSHttpSecurity security)
        {
            if (!WSHttpSecurity.TryCreate(sbe, mode, transportSecurity, isReliableSession, out security))
            {
                return false;
            }
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(isReliableSession, WSMessageSecurityVersion), sbe);
        }

        internal static bool TryGetAllowCookiesFromTransport(TransportBindingElement transport, out bool allowCookies)
        {
            HttpTransportBindingElement element = transport as HttpTransportBindingElement;
            if (element == null)
            {
                allowCookies = false;
                return false;
            }
            allowCookies = element.AllowCookies;
            return true;
        }

        [DefaultValue(false)]
        public bool AllowCookies
        {
            get
            {
                return base.HttpTransport.AllowCookies;
            }
            set
            {
                base.HttpTransport.AllowCookies = value;
                base.HttpsTransport.AllowCookies = value;
            }
        }

        public WSHttpSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.security = value;
            }
        }
    }
}

