namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class BasicHttpContextBinding : BasicHttpBinding
    {
        private bool contextManagementEnabled;

        public BasicHttpContextBinding()
        {
            this.contextManagementEnabled = true;
            base.AllowCookies = true;
        }

        public BasicHttpContextBinding(BasicHttpSecurityMode securityMode) : base(securityMode)
        {
            this.contextManagementEnabled = true;
            base.AllowCookies = true;
        }

        public BasicHttpContextBinding(string configName)
        {
            this.contextManagementEnabled = true;
            if (configName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configName");
            }
            BasicHttpContextBindingElement element2 = BasicHttpContextBindingCollectionElement.GetBindingCollectionElement().Bindings[configName];
            element2.ApplyConfiguration(this);
            if (element2.ElementInformation.Properties["allowCookies"].ValueOrigin == PropertyValueOrigin.Default)
            {
                base.AllowCookies = true;
            }
            else if (!base.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("BasicHttpContextBindingRequiresAllowCookie", new object[] { base.Namespace, base.Name }));
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements;
            if (!base.AllowCookies)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BasicHttpContextBindingRequiresAllowCookie", new object[] { base.Namespace, base.Name })));
            }
            try
            {
                base.AllowCookies = false;
                elements = base.CreateBindingElements();
            }
            finally
            {
                base.AllowCookies = true;
            }
            elements.Insert(0, new ContextBindingElement(ProtectionLevel.None, ContextExchangeMechanism.HttpCookie, null, this.ContextManagementEnabled));
            return elements;
        }

        [DefaultValue(true)]
        public bool ContextManagementEnabled
        {
            get
            {
                return this.contextManagementEnabled;
            }
            set
            {
                this.contextManagementEnabled = value;
            }
        }
    }
}

