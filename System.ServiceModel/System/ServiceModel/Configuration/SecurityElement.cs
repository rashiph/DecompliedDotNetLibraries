namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    public sealed class SecurityElement : SecurityElementBase
    {
        private ConfigurationPropertyCollection properties;

        protected override void AddBindingTemplates(Dictionary<AuthenticationMode, SecurityBindingElement> bindingTemplates)
        {
            base.AddBindingTemplates(bindingTemplates);
            base.AddBindingTemplate(bindingTemplates, AuthenticationMode.SecureConversation);
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            SecurityElement element = (SecurityElement) from;
            if (element.ElementInformation.Properties["secureConversationBootstrap"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.SecureConversationBootstrap.CopyFrom(element.SecureConversationBootstrap);
            }
        }

        protected internal override BindingElement CreateBindingElement(bool createTemplateOnly)
        {
            SecurityBindingElement element;
            if (base.AuthenticationMode == AuthenticationMode.SecureConversation)
            {
                if (this.SecureConversationBootstrap == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationNeedsBootstrapSecurity")));
                }
                if (this.SecureConversationBootstrap.AuthenticationMode == AuthenticationMode.SecureConversation)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationBootstrapCannotUseSecureConversation")));
                }
                SecurityBindingElement bootstrapSecurity = (SecurityBindingElement) this.SecureConversationBootstrap.CreateBindingElement(createTemplateOnly);
                element = SecurityBindingElement.CreateSecureConversationBindingElement(bootstrapSecurity, base.RequireSecurityContextCancellation);
            }
            else
            {
                element = (SecurityBindingElement) base.CreateBindingElement(createTemplateOnly);
            }
            this.ApplyConfiguration(element);
            return element;
        }

        protected override void InitializeNestedTokenParameterSettings(SecurityTokenParameters sp, bool initializeNestedBindings)
        {
            if (sp is SecureConversationSecurityTokenParameters)
            {
                this.InitializeSecureConversationParameters((SecureConversationSecurityTokenParameters) sp, initializeNestedBindings);
            }
            else
            {
                base.InitializeNestedTokenParameterSettings(sp, initializeNestedBindings);
            }
        }

        private void InitializeSecureConversationParameters(SecureConversationSecurityTokenParameters sc, bool initializeNestedBindings)
        {
            base.RequireSecurityContextCancellation = sc.RequireCancellation;
            if (!sc.CanRenewSession)
            {
                base.CanRenewSecurityContextToken = sc.CanRenewSession;
            }
            if (sc.BootstrapSecurityBindingElement != null)
            {
                this.SecureConversationBootstrap.InitializeFrom(sc.BootstrapSecurityBindingElement, initializeNestedBindings);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("secureConversationBootstrap", typeof(SecurityElementBase), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("secureConversationBootstrap")]
        public SecurityElementBase SecureConversationBootstrap
        {
            get
            {
                return (SecurityElementBase) base["secureConversationBootstrap"];
            }
        }
    }
}

