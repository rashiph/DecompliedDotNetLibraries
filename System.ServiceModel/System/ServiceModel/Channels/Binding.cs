namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public abstract class Binding : IDefaultCommunicationTimeouts
    {
        private TimeSpan closeTimeout;
        internal const string DefaultNamespace = "http://tempuri.org/";
        private string name;
        private string namespaceIdentifier;
        private TimeSpan openTimeout;
        private TimeSpan receiveTimeout;
        private TimeSpan sendTimeout;

        protected Binding()
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
            this.name = null;
            this.namespaceIdentifier = "http://tempuri.org/";
        }

        protected Binding(string name, string ns)
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name", System.ServiceModel.SR.GetString("SFXBindingNameCannotBeNullOrEmpty"));
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (ns.Length > 0)
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }
            this.name = name;
            this.namespaceIdentifier = ns;
        }

        public IChannelFactory<TChannel> BuildChannelFactory<TChannel>(params object[] parameters)
        {
            return this.BuildChannelFactory<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            this.EnsureInvariants();
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            IChannelFactory<TChannel> factory = context.BuildInnerChannelFactory<TChannel>();
            context.ValidateBindingElementsConsumed();
            this.ValidateSecurityCapabilities(factory.GetProperty<ISecurityCapabilities>(), parameters);
            return factory;
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(params object[] parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingParameterCollection parameters) where TChannel: class, IChannel
        {
            UriBuilder builder = new UriBuilder(this.Scheme, DnsCache.MachineName);
            return this.BuildChannelListener<TChannel>(builder.Uri, string.Empty, ListenUriMode.Unique, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, params object[] parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, BindingParameterCollection parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, string.Empty, ListenUriMode.Explicit, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, params object[] parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, BindingParameterCollection parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, ListenUriMode.Explicit, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, params object[] parameters) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, BindingParameterCollection parameters) where TChannel: class, IChannel
        {
            this.EnsureInvariants();
            BindingContext context = new BindingContext(new CustomBinding(this), parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
            IChannelListener<TChannel> listener = context.BuildInnerChannelListener<TChannel>();
            context.ValidateBindingElementsConsumed();
            this.ValidateSecurityCapabilities(listener.GetProperty<ISecurityCapabilities>(), parameters);
            return listener;
        }

        public bool CanBuildChannelFactory<TChannel>(params object[] parameters)
        {
            return this.CanBuildChannelFactory<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual bool CanBuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public bool CanBuildChannelListener<TChannel>(params object[] parameters) where TChannel: class, IChannel
        {
            return this.CanBuildChannelListener<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual bool CanBuildChannelListener<TChannel>(BindingParameterCollection parameters) where TChannel: class, IChannel
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        internal void CopyTimeouts(IDefaultCommunicationTimeouts source)
        {
            this.CloseTimeout = source.CloseTimeout;
            this.OpenTimeout = source.OpenTimeout;
            this.ReceiveTimeout = source.ReceiveTimeout;
            this.SendTimeout = source.SendTimeout;
        }

        public abstract BindingElementCollection CreateBindingElements();
        private void EnsureInvariants()
        {
            this.EnsureInvariants(null);
        }

        internal void EnsureInvariants(string contractName)
        {
            BindingElementCollection elements = this.CreateBindingElements();
            TransportBindingElement element = null;
            int num = 0;
            while (num < elements.Count)
            {
                element = elements[num] as TransportBindingElement;
                if (element != null)
                {
                    break;
                }
                num++;
            }
            if (element == null)
            {
                if (contractName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CustomBindingRequiresTransport", new object[] { this.Name })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCustomBindingNeedsTransport1", new object[] { contractName })));
            }
            if (num != (elements.Count - 1))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportBindingElementMustBeLast", new object[] { this.Name, element.GetType().Name })));
            }
            if (string.IsNullOrEmpty(element.Scheme))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidBindingScheme", new object[] { element.GetType().Name })));
            }
            if (this.MessageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageVersionMissingFromBinding", new object[] { this.Name })));
            }
        }

        public T GetProperty<T>(BindingParameterCollection parameters) where T: class
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.GetInnerProperty<T>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeName()
        {
            return (this.Name != base.GetType().Name);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNamespace()
        {
            return (this.Namespace != "http://tempuri.org/");
        }

        private void ValidateSecurityCapabilities(ISecurityCapabilities runtimeSecurityCapabilities, BindingParameterCollection parameters)
        {
            if (!SecurityCapabilities.IsEqual(this.GetProperty<ISecurityCapabilities>(parameters), runtimeSecurityCapabilities))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityCapabilitiesMismatched", new object[] { this })));
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:01:00")]
        public TimeSpan CloseTimeout
        {
            get
            {
                return this.closeTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.closeTimeout = value;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.GetProperty<System.ServiceModel.Channels.MessageVersion>(new BindingParameterCollection());
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                {
                    this.name = base.GetType().Name;
                }
                return this.name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("SFXBindingNameCannotBeNullOrEmpty"));
                }
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.namespaceIdentifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value.Length > 0)
                {
                    NamingHelper.CheckUriProperty(value, "Namespace");
                }
                this.namespaceIdentifier = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:01:00")]
        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.openTimeout = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), "00:10:00")]
        public TimeSpan ReceiveTimeout
        {
            get
            {
                return this.receiveTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.receiveTimeout = value;
            }
        }

        public abstract string Scheme { get; }

        [DefaultValue(typeof(TimeSpan), "00:01:00")]
        public TimeSpan SendTimeout
        {
            get
            {
                return this.sendTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.sendTimeout = value;
            }
        }
    }
}

