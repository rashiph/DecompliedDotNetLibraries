namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public sealed class MsmqTransportBindingElement : MsmqBindingElementBase
    {
        private int maxPoolSize;
        private System.ServiceModel.QueueTransferProtocol queueTransferProtocol;
        private bool useActiveDirectory;

        public MsmqTransportBindingElement()
        {
            this.maxPoolSize = 8;
        }

        private MsmqTransportBindingElement(MsmqTransportBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.maxPoolSize = 8;
            this.useActiveDirectory = elementToBeCloned.useActiveDirectory;
            this.maxPoolSize = elementToBeCloned.maxPoolSize;
            this.queueTransferProtocol = elementToBeCloned.queueTransferProtocol;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                MsmqChannelFactoryBase<IOutputChannel> base2 = new MsmqOutputChannelFactory(this, context);
                MsmqVerifier.VerifySender<IOutputChannel>(base2);
                return (IChannelFactory<TChannel>) base2;
            }
            if (typeof(TChannel) != typeof(IOutputSessionChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            MsmqChannelFactoryBase<IOutputSessionChannel> factory = new MsmqOutputSessionChannelFactory(this, context);
            MsmqVerifier.VerifySender<IOutputSessionChannel>(factory);
            return (IChannelFactory<TChannel>) factory;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            TransportChannelListener listener;
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            MsmqTransportReceiveParameters receiveParameters = new MsmqTransportReceiveParameters(this, MsmqUri.NetMsmqAddressTranslator);
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                listener = new MsmqInputChannelListener(this, context, receiveParameters);
            }
            else
            {
                if (typeof(TChannel) != typeof(IInputSessionChannel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
                }
                listener = new MsmqInputSessionChannelListener(this, context, receiveParameters);
            }
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            MsmqVerifier.VerifyReceiver(receiveParameters, listener.Uri);
            return (IChannelListener<TChannel>) listener;
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (!(typeof(TChannel) == typeof(IOutputChannel)))
            {
                return (typeof(TChannel) == typeof(IOutputSessionChannel));
            }
            return true;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (!(typeof(TChannel) == typeof(IInputChannel)))
            {
                return (typeof(TChannel) == typeof(IInputSessionChannel));
            }
            return true;
        }

        public override BindingElement Clone()
        {
            return new MsmqTransportBindingElement(this);
        }

        internal override MsmqUri.IAddressTranslator AddressTranslator
        {
            get
            {
                switch (this.queueTransferProtocol)
                {
                    case System.ServiceModel.QueueTransferProtocol.Srmp:
                        return MsmqUri.SrmpAddressTranslator;

                    case System.ServiceModel.QueueTransferProtocol.SrmpSecure:
                        return MsmqUri.SrmpsAddressTranslator;
                }
                if (!this.useActiveDirectory)
                {
                    return MsmqUri.NetMsmqAddressTranslator;
                }
                return MsmqUri.ActiveDirectoryAddressTranslator;
            }
        }

        public int MaxPoolSize
        {
            get
            {
                return this.maxPoolSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("MsmqNonNegativeArgumentExpected")));
                }
                this.maxPoolSize = value;
            }
        }

        public System.ServiceModel.QueueTransferProtocol QueueTransferProtocol
        {
            get
            {
                return this.queueTransferProtocol;
            }
            set
            {
                if (!QueueTransferProtocolHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.queueTransferProtocol = value;
            }
        }

        public override string Scheme
        {
            get
            {
                return "net.msmq";
            }
        }

        public bool UseActiveDirectory
        {
            get
            {
                return this.useActiveDirectory;
            }
            set
            {
                this.useActiveDirectory = value;
            }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return "http://schemas.microsoft.com/soap/msmq";
            }
        }
    }
}

