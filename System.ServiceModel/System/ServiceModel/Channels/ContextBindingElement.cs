namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ContextBindingElement : BindingElement, IPolicyExportExtension, IContextSessionProvider, IWmiInstanceProvider, IContextBindingElement
    {
        private System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism;
        private bool contextManagementEnabled;
        internal const System.ServiceModel.Channels.ContextExchangeMechanism DefaultContextExchangeMechanism = System.ServiceModel.Channels.ContextExchangeMechanism.ContextSoapHeader;
        internal const bool DefaultContextManagementEnabled = true;
        internal const System.Net.Security.ProtectionLevel DefaultProtectionLevel = System.Net.Security.ProtectionLevel.Sign;
        private ICorrelationDataSource instanceCorrelationData;
        private System.Net.Security.ProtectionLevel protectionLevel;

        public ContextBindingElement() : this(System.Net.Security.ProtectionLevel.Sign, System.ServiceModel.Channels.ContextExchangeMechanism.ContextSoapHeader, null, true)
        {
        }

        public ContextBindingElement(System.Net.Security.ProtectionLevel protectionLevel) : this(protectionLevel, System.ServiceModel.Channels.ContextExchangeMechanism.ContextSoapHeader, null, true)
        {
        }

        private ContextBindingElement(ContextBindingElement other) : base(other)
        {
            this.ProtectionLevel = other.ProtectionLevel;
            this.ContextExchangeMechanism = other.ContextExchangeMechanism;
            this.ClientCallbackAddress = other.ClientCallbackAddress;
            this.ContextManagementEnabled = other.ContextManagementEnabled;
        }

        public ContextBindingElement(System.Net.Security.ProtectionLevel protectionLevel, System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism) : this(protectionLevel, contextExchangeMechanism, null, true)
        {
        }

        public ContextBindingElement(System.Net.Security.ProtectionLevel protectionLevel, System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism, Uri clientCallbackAddress) : this(protectionLevel, contextExchangeMechanism, clientCallbackAddress, true)
        {
        }

        public ContextBindingElement(System.Net.Security.ProtectionLevel protectionLevel, System.ServiceModel.Channels.ContextExchangeMechanism contextExchangeMechanism, Uri clientCallbackAddress, bool contextManagementEnabled)
        {
            this.ProtectionLevel = protectionLevel;
            this.ContextExchangeMechanism = contextExchangeMechanism;
            this.ClientCallbackAddress = clientCallbackAddress;
            this.ContextManagementEnabled = contextManagementEnabled;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextBindingElementCannotProvideChannelFactory", new object[] { typeof(TChannel).ToString() })));
            }
            this.EnsureContextExchangeMechanismCompatibleWithScheme(context);
            this.EnsureContextExchangeMechanismCompatibleWithTransportCookieSetting(context);
            return new ContextChannelFactory<TChannel>(context, this.ContextExchangeMechanism, this.ClientCallbackAddress, this.ContextManagementEnabled);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ContextBindingElementCannotProvideChannelListener", new object[] { typeof(TChannel).ToString() })));
            }
            this.EnsureContextExchangeMechanismCompatibleWithScheme(context);
            return new ContextChannelListener<TChannel>(context, this.ContextExchangeMechanism);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if ((!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IOutputSessionChannel))) && ((!(typeof(TChannel) == typeof(IRequestChannel)) && !(typeof(TChannel) == typeof(IRequestSessionChannel))) && (!(typeof(TChannel) == typeof(IDuplexSessionChannel)) || (this.ContextExchangeMechanism == System.ServiceModel.Channels.ContextExchangeMechanism.HttpCookie))))
            {
                return false;
            }
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if ((!(typeof(TChannel) == typeof(IInputChannel)) && !(typeof(TChannel) == typeof(IInputSessionChannel))) && ((!(typeof(TChannel) == typeof(IReplyChannel)) && !(typeof(TChannel) == typeof(IReplySessionChannel))) && (!(typeof(TChannel) == typeof(IDuplexSessionChannel)) || (this.ContextExchangeMechanism == System.ServiceModel.Channels.ContextExchangeMechanism.HttpCookie))))
            {
                return false;
            }
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override BindingElement Clone()
        {
            return new ContextBindingElement(this);
        }

        private void EnsureContextExchangeMechanismCompatibleWithScheme(BindingContext context)
        {
            if (((context.Binding != null) && (this.contextExchangeMechanism == System.ServiceModel.Channels.ContextExchangeMechanism.HttpCookie)) && (!"http".Equals(context.Binding.Scheme, StringComparison.OrdinalIgnoreCase) && !"https".Equals(context.Binding.Scheme, StringComparison.OrdinalIgnoreCase)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpCookieContextExchangeMechanismNotCompatibleWithTransportType", new object[] { context.Binding.Scheme, context.Binding.Namespace, context.Binding.Name })));
            }
        }

        private void EnsureContextExchangeMechanismCompatibleWithTransportCookieSetting(BindingContext context)
        {
            if ((context.Binding != null) && (this.contextExchangeMechanism == System.ServiceModel.Channels.ContextExchangeMechanism.HttpCookie))
            {
                foreach (BindingElement element in context.Binding.Elements)
                {
                    HttpTransportBindingElement element2 = element as HttpTransportBindingElement;
                    if ((element2 != null) && element2.AllowCookies)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("HttpCookieContextExchangeMechanismNotCompatibleWithTransportCookieSetting", new object[] { context.Binding.Namespace, context.Binding.Name })));
                    }
                }
            }
        }

        public virtual void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            ContextBindingElementPolicy.ExportRequireContextAssertion(this, context.GetBindingAssertions());
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if ((typeof(T) == typeof(ChannelProtectionRequirements)) && (this.ProtectionLevel != System.Net.Security.ProtectionLevel.None))
            {
                ChannelProtectionRequirements innerProperty = context.GetInnerProperty<ChannelProtectionRequirements>();
                if (innerProperty == null)
                {
                    return (T) ContextMessageHeader.GetChannelProtectionRequirements(this.ProtectionLevel);
                }
                ChannelProtectionRequirements requirements2 = new ChannelProtectionRequirements(innerProperty);
                requirements2.Add(ContextMessageHeader.GetChannelProtectionRequirements(this.ProtectionLevel));
                return (T) requirements2;
            }
            if (typeof(T) == typeof(IContextSessionProvider))
            {
                return (T) this;
            }
            if (typeof(T) == typeof(IContextBindingElement))
            {
                return (T) this;
            }
            if (!(typeof(T) == typeof(ICorrelationDataSource)))
            {
                return context.GetInnerProperty<T>();
            }
            ICorrelationDataSource instanceCorrelationData = this.instanceCorrelationData;
            if (instanceCorrelationData == null)
            {
                instanceCorrelationData = CorrelationDataSourceHelper.Combine(context.GetInnerProperty<ICorrelationDataSource>(), ContextExchangeCorrelationDataDescription.DataSource);
                this.instanceCorrelationData = instanceCorrelationData;
            }
            return (T) instanceCorrelationData;
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            ContextBindingElement element = b as ContextBindingElement;
            if (element == null)
            {
                return false;
            }
            if (this.ClientCallbackAddress != element.ClientCallbackAddress)
            {
                return false;
            }
            if (this.ContextExchangeMechanism != element.ContextExchangeMechanism)
            {
                return false;
            }
            if (this.ContextManagementEnabled != element.ContextManagementEnabled)
            {
                return false;
            }
            if (this.ProtectionLevel != element.protectionLevel)
            {
                return false;
            }
            return true;
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("ProtectionLevel", this.protectionLevel.ToString());
            wmiInstance.SetProperty("ContextExchangeMechanism", this.contextExchangeMechanism.ToString());
            wmiInstance.SetProperty("ContextManagementEnabled", this.contextManagementEnabled);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "ContextBindingElement";
        }

        internal static void ValidateContextBindingElementOnAllEndpointsWithSessionfulContract(System.ServiceModel.Description.ServiceDescription description, IServiceBehavior callingBehavior)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (callingBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callingBehavior");
            }
            BindingParameterCollection parameters = new BindingParameterCollection();
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if ((((endpoint.Binding != null) && (endpoint.Contract != null)) && (!endpoint.InternalIsSystemEndpoint(description) && (endpoint.Contract.SessionMode != SessionMode.NotAllowed))) && (endpoint.Binding.GetProperty<IContextBindingElement>(parameters) == null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BehaviorRequiresContextProtocolSupportInBinding", new object[] { callingBehavior.GetType().Name, endpoint.Name, endpoint.ListenUri.ToString() })));
                }
            }
        }

        [DefaultValue((string) null)]
        public Uri ClientCallbackAddress { get; set; }

        [DefaultValue(0)]
        public System.ServiceModel.Channels.ContextExchangeMechanism ContextExchangeMechanism
        {
            get
            {
                return this.contextExchangeMechanism;
            }
            set
            {
                if (!ContextExchangeMechanismHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.contextExchangeMechanism = value;
            }
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

        [DefaultValue(1)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
            }
        }

        private class ContextExchangeCorrelationDataDescription : CorrelationDataDescription
        {
            private static CorrelationDataSourceHelper cachedCorrelationDataSource;

            private ContextExchangeCorrelationDataDescription()
            {
            }

            public static ICorrelationDataSource DataSource
            {
                get
                {
                    if (cachedCorrelationDataSource == null)
                    {
                        cachedCorrelationDataSource = new CorrelationDataSourceHelper(new CorrelationDataDescription[] { new ContextBindingElement.ContextExchangeCorrelationDataDescription() });
                    }
                    return cachedCorrelationDataSource;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return true;
                }
            }

            public override bool IsOptional
            {
                get
                {
                    return true;
                }
            }

            public override bool KnownBeforeSend
            {
                get
                {
                    return true;
                }
            }

            public override string Name
            {
                get
                {
                    return ContextExchangeCorrelationHelper.CorrelationName;
                }
            }

            public override bool ReceiveValue
            {
                get
                {
                    return true;
                }
            }

            public override bool SendValue
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

