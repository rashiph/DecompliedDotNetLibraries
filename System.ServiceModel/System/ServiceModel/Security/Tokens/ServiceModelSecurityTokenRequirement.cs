namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;

    public abstract class ServiceModelSecurityTokenRequirement : SecurityTokenRequirement
    {
        private const string auditLogLocationProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/AuditLogLocation";
        private const string channelParametersCollectionProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ChannelParametersCollection";
        private const string defaultMessageSecurityVersionProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DefaultMessageSecurityVersion";
        private const bool defaultSupportSecurityContextCancellation = false;
        private const string duplexClientLocalAddressProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DuplexClientLocalAddress";
        private const string endpointFilterTableProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/EndpointFilterTable";
        private const string extendedProtectionPolicy = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ExtendedProtectionPolicy";
        private const string httpAuthenticationSchemeProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/HttpAuthenticationScheme";
        private const string isInitiatorProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsInitiator";
        private const string isOutOfBandTokenProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsOutOfBandToken";
        private const string issuedSecurityTokenParametersProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuedSecurityTokenParameters";
        private const string issuerAddressProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerAddress";
        private const string issuerBindingContextProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBindingContext";
        private const string issuerBindingProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBinding";
        private const string listenUriProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ListenUri";
        private const string messageAuthenticationAuditLevelProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageAuthenticationAuditLevel";
        private const string messageDirectionProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageDirection";
        private const string messageSecurityVersionProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageSecurityVersion";
        protected const string Namespace = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement";
        private const string privacyNoticeUriProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeUri";
        private const string privacyNoticeVersionProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeVersion";
        private const string secureConversationSecurityBindingElementProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecureConversationSecurityBindingElement";
        private const string securityAlgorithmSuiteProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityAlgorithmSuite";
        private const string securityBindingElementProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityBindingElement";
        private const string supportingTokenAttachmentModeProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportingTokenAttachmentMode";
        private const string supportSecurityContextCancellationProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportSecurityContextCancellation";
        private const string suppressAuditFailureProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SuppressAuditFailure";
        private const string targetAddressProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TargetAddress";
        private const string transportSchemeProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TransportScheme";
        private const string viaProperty = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/Via";

        protected ServiceModelSecurityTokenRequirement()
        {
            base.Properties[SupportSecurityContextCancellationProperty] = false;
        }

        internal TValue GetPropertyOrDefault<TValue>(string propertyName, TValue defaultValue)
        {
            TValue local;
            if (!base.TryGetProperty<TValue>(propertyName, out local))
            {
                local = defaultValue;
            }
            return local;
        }

        internal string InternalToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}:", new object[] { base.GetType().ToString() }));
            foreach (string str in base.Properties.Keys)
            {
                object obj2 = base.Properties[str];
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "PropertyName: {0}", new object[] { str }));
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "PropertyValue: {0}", new object[] { obj2 }));
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "---", new object[0]));
            }
            return builder.ToString().Trim();
        }

        public static string AuditLogLocationProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/AuditLogLocation";
            }
        }

        public static string ChannelParametersCollectionProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ChannelParametersCollection";
            }
        }

        internal System.ServiceModel.MessageSecurityVersion DefaultMessageSecurityVersion
        {
            get
            {
                System.ServiceModel.MessageSecurityVersion version;
                if (!base.TryGetProperty<System.ServiceModel.MessageSecurityVersion>(DefaultMessageSecurityVersionProperty, out version))
                {
                    return null;
                }
                return version;
            }
            set
            {
                base.Properties[DefaultMessageSecurityVersionProperty] = value;
            }
        }

        internal static string DefaultMessageSecurityVersionProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DefaultMessageSecurityVersion";
            }
        }

        internal EndpointAddress DuplexClientLocalAddress
        {
            get
            {
                return this.GetPropertyOrDefault<EndpointAddress>("http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DuplexClientLocalAddress", null);
            }
            set
            {
                base.Properties["http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DuplexClientLocalAddress"] = value;
            }
        }

        public static string DuplexClientLocalAddressProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DuplexClientLocalAddress";
            }
        }

        public static string EndpointFilterTableProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/EndpointFilterTable";
            }
        }

        public static string ExtendedProtectionPolicy
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ExtendedProtectionPolicy";
            }
        }

        public static string HttpAuthenticationSchemeProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/HttpAuthenticationScheme";
            }
        }

        public bool IsInitiator
        {
            get
            {
                return this.GetPropertyOrDefault<bool>(IsInitiatorProperty, false);
            }
        }

        public static string IsInitiatorProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsInitiator";
            }
        }

        public static string IsOutOfBandTokenProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsOutOfBandToken";
            }
        }

        public static string IssuedSecurityTokenParametersProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuedSecurityTokenParameters";
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.GetPropertyOrDefault<EndpointAddress>(IssuerAddressProperty, null);
            }
            set
            {
                base.Properties[IssuerAddressProperty] = value;
            }
        }

        public static string IssuerAddressProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerAddress";
            }
        }

        public Binding IssuerBinding
        {
            get
            {
                return this.GetPropertyOrDefault<Binding>(IssuerBindingProperty, null);
            }
            set
            {
                base.Properties[IssuerBindingProperty] = value;
            }
        }

        public static string IssuerBindingContextProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBindingContext";
            }
        }

        public static string IssuerBindingProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBinding";
            }
        }

        public static string ListenUriProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ListenUri";
            }
        }

        public static string MessageAuthenticationAuditLevelProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageAuthenticationAuditLevel";
            }
        }

        public static string MessageDirectionProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageDirection";
            }
        }

        public SecurityTokenVersion MessageSecurityVersion
        {
            get
            {
                return this.GetPropertyOrDefault<SecurityTokenVersion>(MessageSecurityVersionProperty, null);
            }
            set
            {
                base.Properties[MessageSecurityVersionProperty] = value;
            }
        }

        public static string MessageSecurityVersionProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageSecurityVersion";
            }
        }

        public static string PrivacyNoticeUriProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeUri";
            }
        }

        public static string PrivacyNoticeVersionProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeVersion";
            }
        }

        public System.ServiceModel.Channels.SecurityBindingElement SecureConversationSecurityBindingElement
        {
            get
            {
                return this.GetPropertyOrDefault<System.ServiceModel.Channels.SecurityBindingElement>(SecureConversationSecurityBindingElementProperty, null);
            }
            set
            {
                base.Properties[SecureConversationSecurityBindingElementProperty] = value;
            }
        }

        public static string SecureConversationSecurityBindingElementProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecureConversationSecurityBindingElement";
            }
        }

        public System.ServiceModel.Security.SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.GetPropertyOrDefault<System.ServiceModel.Security.SecurityAlgorithmSuite>(SecurityAlgorithmSuiteProperty, null);
            }
            set
            {
                base.Properties[SecurityAlgorithmSuiteProperty] = value;
            }
        }

        public static string SecurityAlgorithmSuiteProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityAlgorithmSuite";
            }
        }

        public System.ServiceModel.Channels.SecurityBindingElement SecurityBindingElement
        {
            get
            {
                return this.GetPropertyOrDefault<System.ServiceModel.Channels.SecurityBindingElement>(SecurityBindingElementProperty, null);
            }
            set
            {
                base.Properties[SecurityBindingElementProperty] = value;
            }
        }

        public static string SecurityBindingElementProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityBindingElement";
            }
        }

        public static string SupportingTokenAttachmentModeProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportingTokenAttachmentMode";
            }
        }

        internal bool SupportSecurityContextCancellation
        {
            get
            {
                return this.GetPropertyOrDefault<bool>(SupportSecurityContextCancellationProperty, false);
            }
            set
            {
                base.Properties[SupportSecurityContextCancellationProperty] = value;
            }
        }

        public static string SupportSecurityContextCancellationProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportSecurityContextCancellation";
            }
        }

        public static string SuppressAuditFailureProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SuppressAuditFailure";
            }
        }

        public static string TargetAddressProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TargetAddress";
            }
        }

        public string TransportScheme
        {
            get
            {
                return this.GetPropertyOrDefault<string>(TransportSchemeProperty, null);
            }
            set
            {
                base.Properties[TransportSchemeProperty] = value;
            }
        }

        public static string TransportSchemeProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TransportScheme";
            }
        }

        public static string ViaProperty
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/Via";
            }
        }
    }
}

