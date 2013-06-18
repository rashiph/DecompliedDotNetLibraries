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
    public class NetTcpContextBinding : NetTcpBinding
    {
        private bool contextManagementEnabled;
        private ProtectionLevel contextProtectionLevel;

        public NetTcpContextBinding()
        {
            this.contextManagementEnabled = true;
            this.contextProtectionLevel = ProtectionLevel.Sign;
        }

        private NetTcpContextBinding(NetTcpBinding netTcpBinding)
        {
            this.contextManagementEnabled = true;
            this.contextProtectionLevel = ProtectionLevel.Sign;
            NetTcpContextBindingPropertyTransferHelper helper = new NetTcpContextBindingPropertyTransferHelper();
            helper.InitializeFrom(netTcpBinding);
            helper.SetBindingElementType(typeof(NetTcpContextBinding));
            helper.ApplyConfiguration(this);
        }

        public NetTcpContextBinding(SecurityMode securityMode) : base(securityMode)
        {
            this.contextManagementEnabled = true;
            this.contextProtectionLevel = ProtectionLevel.Sign;
        }

        public NetTcpContextBinding(string configName)
        {
            this.contextManagementEnabled = true;
            this.contextProtectionLevel = ProtectionLevel.Sign;
            if (configName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configName");
            }
            this.ApplyConfiguration(configName);
        }

        public NetTcpContextBinding(SecurityMode securityMode, bool reliableSessionEnabled) : base(securityMode, reliableSessionEnabled)
        {
            this.contextManagementEnabled = true;
            this.contextProtectionLevel = ProtectionLevel.Sign;
        }

        private void ApplyConfiguration(string configurationName)
        {
            NetTcpContextBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName].ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = base.CreateBindingElements();
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
            if ((element != null) && (element.ContextExchangeMechanism != ContextExchangeMechanism.HttpCookie))
            {
                Binding binding2;
                BindingElementCollection elements = new BindingElementCollection(bindingElements);
                elements.Remove<ContextBindingElement>();
                if (NetTcpBinding.TryCreate(elements, out binding2))
                {
                    NetTcpContextBinding binding3 = new NetTcpContextBinding((NetTcpBinding) binding2) {
                        ContextProtectionLevel = element.ProtectionLevel,
                        ContextManagementEnabled = element.ContextManagementEnabled
                    };
                    binding = binding3;
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

        private class NetTcpContextBindingPropertyTransferHelper : NetTcpBindingElement
        {
            private System.Type bindingElementType = typeof(NetTcpBinding);

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

