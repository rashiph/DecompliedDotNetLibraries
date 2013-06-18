namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class WSHttpContextBinding : WSHttpBinding
    {
        private bool contextManagementEnabled;
        private ProtectionLevel contextProtectionLevel;

        public WSHttpContextBinding()
        {
            this.contextProtectionLevel = ProtectionLevel.Sign;
            this.contextManagementEnabled = true;
        }

        public WSHttpContextBinding(SecurityMode securityMode) : base(securityMode)
        {
            this.contextProtectionLevel = ProtectionLevel.Sign;
            this.contextManagementEnabled = true;
        }

        private WSHttpContextBinding(WSHttpBinding wsHttpBinding)
        {
            this.contextProtectionLevel = ProtectionLevel.Sign;
            this.contextManagementEnabled = true;
            WSHttpContextBindingPropertyTransferHelper helper = new WSHttpContextBindingPropertyTransferHelper();
            helper.InitializeFrom(wsHttpBinding);
            helper.SetBindingElementType(typeof(WSHttpContextBinding));
            helper.ApplyConfiguration(this);
        }

        public WSHttpContextBinding(string configName)
        {
            this.contextProtectionLevel = ProtectionLevel.Sign;
            this.contextManagementEnabled = true;
            if (configName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configName");
            }
            this.ApplyConfiguration(configName);
        }

        public WSHttpContextBinding(SecurityMode securityMode, bool reliableSessionEnabled) : base(securityMode, reliableSessionEnabled)
        {
            this.contextProtectionLevel = ProtectionLevel.Sign;
            this.contextManagementEnabled = true;
        }

        private void ApplyConfiguration(string configurationName)
        {
            WSHttpContextBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName].ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements;
            if (base.AllowCookies)
            {
                try
                {
                    base.AllowCookies = false;
                    elements = base.CreateBindingElements();
                }
                finally
                {
                    base.AllowCookies = true;
                }
                elements.Insert(0, new ContextBindingElement(this.ContextProtectionLevel, ContextExchangeMechanism.HttpCookie, this.ClientCallbackAddress, this.ContextManagementEnabled));
                return elements;
            }
            elements = base.CreateBindingElements();
            elements.Insert(0, new ContextBindingElement(this.ContextProtectionLevel, ContextExchangeMechanism.ContextSoapHeader, this.ClientCallbackAddress, this.ContextManagementEnabled));
            return elements;
        }

        internal static bool TryCreate(BindingElementCollection bindingElements, out Binding binding)
        {
            if (bindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            binding = null;
            ContextBindingElement element = bindingElements.Find<ContextBindingElement>();
            if (element != null)
            {
                Binding binding2;
                BindingElementCollection elements = new BindingElementCollection(bindingElements);
                elements.Remove<ContextBindingElement>();
                if (WSHttpBindingBase.TryCreate(elements, out binding2))
                {
                    bool allowCookies = ((WSHttpBinding) binding2).AllowCookies;
                    if ((allowCookies && (element.ContextExchangeMechanism == ContextExchangeMechanism.HttpCookie)) || (!allowCookies && (element.ContextExchangeMechanism == ContextExchangeMechanism.ContextSoapHeader)))
                    {
                        WSHttpContextBinding binding3 = new WSHttpContextBinding((WSHttpBinding) binding2) {
                            ContextProtectionLevel = element.ProtectionLevel,
                            ContextManagementEnabled = element.ContextManagementEnabled
                        };
                        binding = binding3;
                    }
                }
            }
            return (binding != null);
        }

        [DefaultValue((string) null)]
        public Uri ClientCallbackAddress { get; set; }

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

        [DefaultValue(1)]
        public ProtectionLevel ContextProtectionLevel
        {
            get
            {
                return this.contextProtectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.contextProtectionLevel = value;
            }
        }

        private class WSHttpContextBindingPropertyTransferHelper : WSHttpBindingElement
        {
            private System.Type bindingElementType = typeof(WSHttpBinding);

            public void SetBindingElementType(System.Type bindingElementType)
            {
                this.bindingElementType = bindingElementType;
            }

            protected override System.Type BindingElementType
            {
                get
                {
                    return this.bindingElementType;
                }
            }
        }
    }
}

