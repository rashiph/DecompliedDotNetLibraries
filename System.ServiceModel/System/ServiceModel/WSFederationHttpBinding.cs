namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class WSFederationHttpBinding : WSHttpBindingBase
    {
        private Uri privacyNoticeAt;
        private int privacyNoticeVersion;
        private WSFederationHttpSecurity security;
        private static readonly MessageSecurityVersion WSMessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

        public WSFederationHttpBinding()
        {
            this.security = new WSFederationHttpSecurity();
        }

        public WSFederationHttpBinding(WSFederationHttpSecurityMode securityMode) : this(securityMode, false)
        {
        }

        public WSFederationHttpBinding(string configName) : this()
        {
            this.ApplyConfiguration(configName);
        }

        public WSFederationHttpBinding(WSFederationHttpSecurityMode securityMode, bool reliableSessionEnabled) : base(reliableSessionEnabled)
        {
            this.security = new WSFederationHttpSecurity();
            this.security.Mode = securityMode;
        }

        internal WSFederationHttpBinding(WSFederationHttpSecurity security, PrivacyNoticeBindingElement privacy, bool reliableSessionEnabled) : base(reliableSessionEnabled)
        {
            this.security = new WSFederationHttpSecurity();
            this.security = security;
            if (privacy != null)
            {
                this.privacyNoticeAt = privacy.Url;
                this.privacyNoticeVersion = privacy.Version;
            }
        }

        private void ApplyConfiguration(string configurationName)
        {
            WSFederationHttpBindingElement element2 = WSFederationHttpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "wsFederationHttpBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = base.CreateBindingElements();
            PrivacyNoticeBindingElement item = this.CreatePrivacyPolicy();
            if (item != null)
            {
                elements.Insert(0, item);
            }
            return elements;
        }

        protected override SecurityBindingElement CreateMessageSecurity()
        {
            return this.security.CreateMessageSecurity(base.ReliableSession.Enabled, WSMessageSecurityVersion);
        }

        private PrivacyNoticeBindingElement CreatePrivacyPolicy()
        {
            PrivacyNoticeBindingElement element = null;
            if (this.PrivacyNoticeAt != null)
            {
                element = new PrivacyNoticeBindingElement {
                    Url = this.PrivacyNoticeAt,
                    Version = this.privacyNoticeVersion
                };
            }
            return element;
        }

        internal static bool GetSecurityModeFromTransport(TransportBindingElement transport, HttpTransportSecurity transportSecurity, out WSFederationHttpSecurityMode mode)
        {
            mode = WSFederationHttpSecurityMode.TransportWithMessageCredential | WSFederationHttpSecurityMode.Message;
            if (transport is HttpsTransportBindingElement)
            {
                mode = WSFederationHttpSecurityMode.TransportWithMessageCredential;
            }
            else if (transport is HttpTransportBindingElement)
            {
                mode = WSFederationHttpSecurityMode.Message;
            }
            else
            {
                return false;
            }
            return true;
        }

        protected override TransportBindingElement GetTransport()
        {
            if ((this.security.Mode != WSFederationHttpSecurityMode.None) && (this.security.Mode != WSFederationHttpSecurityMode.Message))
            {
                return base.HttpsTransport;
            }
            return base.HttpTransport;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.Security.InternalShouldSerialize();
        }

        internal static bool TryCreate(SecurityBindingElement sbe, TransportBindingElement transport, PrivacyNoticeBindingElement privacy, ReliableSessionBindingElement rsbe, TransactionFlowBindingElement tfbe, out Binding binding)
        {
            WSFederationHttpSecurityMode mode;
            WSFederationHttpSecurity security2;
            bool isReliableSession = rsbe != null;
            binding = null;
            HttpTransportSecurity transportSecurity = new HttpTransportSecurity();
            if (!GetSecurityModeFromTransport(transport, transportSecurity, out mode))
            {
                return false;
            }
            HttpsTransportBindingElement element = transport as HttpsTransportBindingElement;
            if (((element != null) && (element.MessageSecurityVersion != null)) && (element.MessageSecurityVersion.SecurityPolicyVersion != WSMessageSecurityVersion.SecurityPolicyVersion))
            {
                return false;
            }
            if (TryCreateSecurity(sbe, mode, transportSecurity, isReliableSession, out security2))
            {
                binding = new WSFederationHttpBinding(security2, privacy, isReliableSession);
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

        private static bool TryCreateSecurity(SecurityBindingElement sbe, WSFederationHttpSecurityMode mode, HttpTransportSecurity transportSecurity, bool isReliableSession, out WSFederationHttpSecurity security)
        {
            if (!WSFederationHttpSecurity.TryCreate(sbe, mode, transportSecurity, isReliableSession, WSMessageSecurityVersion, out security))
            {
                return false;
            }
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(isReliableSession, WSMessageSecurityVersion), sbe);
        }

        [DefaultValue((string) null)]
        public Uri PrivacyNoticeAt
        {
            get
            {
                return this.privacyNoticeAt;
            }
            set
            {
                this.privacyNoticeAt = value;
            }
        }

        [DefaultValue(0)]
        public int PrivacyNoticeVersion
        {
            get
            {
                return this.privacyNoticeVersion;
            }
            set
            {
                this.privacyNoticeVersion = value;
            }
        }

        public WSFederationHttpSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.security = value;
            }
        }
    }
}

