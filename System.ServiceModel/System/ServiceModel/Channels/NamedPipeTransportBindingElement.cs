namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public class NamedPipeTransportBindingElement : ConnectionOrientedTransportBindingElement
    {
        private List<SecurityIdentifier> allowedUsers;
        private NamedPipeConnectionPoolSettings connectionPoolSettings;

        public NamedPipeTransportBindingElement()
        {
            this.connectionPoolSettings = new NamedPipeConnectionPoolSettings();
        }

        protected NamedPipeTransportBindingElement(NamedPipeTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.connectionPoolSettings = new NamedPipeConnectionPoolSettings();
            if (elementToBeCloned.allowedUsers != null)
            {
                this.allowedUsers = new List<SecurityIdentifier>(elementToBeCloned.AllowedUsers.Count);
                foreach (SecurityIdentifier identifier in elementToBeCloned.allowedUsers)
                {
                    this.allowedUsers.Add(identifier);
                }
            }
            this.connectionPoolSettings = elementToBeCloned.connectionPoolSettings.Clone();
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            return (IChannelFactory<TChannel>) new NamedPipeChannelFactory<TChannel>(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            NamedPipeChannelListener listener;
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                listener = new NamedPipeReplyChannelListener(this, context);
            }
            else
            {
                if (typeof(TChannel) != typeof(IDuplexSessionChannel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
                }
                listener = new NamedPipeDuplexChannelListener(this, context);
            }
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>) listener;
        }

        public override BindingElement Clone()
        {
            return new NamedPipeTransportBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T) new BindingDeliveryCapabilitiesHelper();
            }
            return base.GetProperty<T>(context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }
            NamedPipeTransportBindingElement element = b as NamedPipeTransportBindingElement;
            if (element == null)
            {
                return false;
            }
            if (!this.ConnectionPoolSettings.IsMatch(element.ConnectionPoolSettings))
            {
                return false;
            }
            return true;
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
            set
            {
                this.allowedUsers = value;
            }
        }

        public NamedPipeConnectionPoolSettings ConnectionPoolSettings
        {
            get
            {
                return this.connectionPoolSettings;
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.pipe";
            }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return "http://schemas.microsoft.com/soap/named-pipe";
            }
        }

        private class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }

            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get
                {
                    return true;
                }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

