namespace System.ServiceModel
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class WS2007HttpBinding : WSHttpBinding
    {
        private static readonly MessageSecurityVersion WS2007MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        private static readonly ReliableMessagingVersion WS2007ReliableMessagingVersion = ReliableMessagingVersion.WSReliableMessaging11;
        private static readonly TransactionProtocol WS2007TransactionProtocol = TransactionProtocol.WSAtomicTransaction11;

        public WS2007HttpBinding()
        {
            base.ReliableSessionBindingElement.ReliableMessagingVersion = WS2007ReliableMessagingVersion;
            base.TransactionFlowBindingElement.TransactionProtocol = WS2007TransactionProtocol;
            base.HttpsTransport.MessageSecurityVersion = WS2007MessageSecurityVersion;
        }

        public WS2007HttpBinding(SecurityMode securityMode) : this(securityMode, false)
        {
        }

        public WS2007HttpBinding(string configName) : this()
        {
            this.ApplyConfiguration(configName);
        }

        public WS2007HttpBinding(SecurityMode securityMode, bool reliableSessionEnabled) : base(securityMode, reliableSessionEnabled)
        {
            base.ReliableSessionBindingElement.ReliableMessagingVersion = WS2007ReliableMessagingVersion;
            base.TransactionFlowBindingElement.TransactionProtocol = WS2007TransactionProtocol;
            base.HttpsTransport.MessageSecurityVersion = WS2007MessageSecurityVersion;
        }

        internal WS2007HttpBinding(WSHttpSecurity security, bool reliableSessionEnabled) : base(security, reliableSessionEnabled)
        {
            base.ReliableSessionBindingElement.ReliableMessagingVersion = WS2007ReliableMessagingVersion;
            base.TransactionFlowBindingElement.TransactionProtocol = WS2007TransactionProtocol;
            base.HttpsTransport.MessageSecurityVersion = WS2007MessageSecurityVersion;
        }

        private void ApplyConfiguration(string configurationName)
        {
            WS2007HttpBindingElement element2 = WS2007HttpBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "ws2007HttpBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        protected override SecurityBindingElement CreateMessageSecurity()
        {
            return base.Security.CreateMessageSecurity(base.ReliableSession.Enabled, WS2007MessageSecurityVersion);
        }

        internal static bool TryCreate(SecurityBindingElement sbe, TransportBindingElement transport, ReliableSessionBindingElement rsbe, TransactionFlowBindingElement tfbe, out Binding binding)
        {
            UnifiedSecurityMode mode;
            WSHttpSecurity security2;
            bool isReliableSession = rsbe != null;
            binding = null;
            HttpTransportSecurity defaultHttpTransportSecurity = WSHttpSecurity.GetDefaultHttpTransportSecurity();
            if (!WSHttpBinding.GetSecurityModeFromTransport(transport, defaultHttpTransportSecurity, out mode))
            {
                return false;
            }
            HttpsTransportBindingElement element = transport as HttpsTransportBindingElement;
            if (((element != null) && (element.MessageSecurityVersion != null)) && (element.MessageSecurityVersion.SecurityPolicyVersion != WS2007MessageSecurityVersion.SecurityPolicyVersion))
            {
                return false;
            }
            if (TryCreateSecurity(sbe, mode, defaultHttpTransportSecurity, isReliableSession, out security2))
            {
                bool flag2;
                WS2007HttpBinding binding2 = new WS2007HttpBinding(security2, isReliableSession);
                if (!WSHttpBinding.TryGetAllowCookiesFromTransport(transport, out flag2))
                {
                    return false;
                }
                binding2.AllowCookies = flag2;
                binding = binding2;
            }
            if ((rsbe != null) && (rsbe.ReliableMessagingVersion != ReliableMessagingVersion.WSReliableMessaging11))
            {
                return false;
            }
            if ((tfbe != null) && (tfbe.TransactionProtocol != TransactionProtocol.WSAtomicTransaction11))
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
            return SecurityElementBase.AreBindingsMatching(security.CreateMessageSecurity(isReliableSession, WS2007MessageSecurityVersion), sbe);
        }
    }
}

