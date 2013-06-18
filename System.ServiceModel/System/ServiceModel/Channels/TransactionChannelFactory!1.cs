namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal sealed class TransactionChannelFactory<TChannel> : LayeredChannelFactory<TChannel>, ITransactionChannelManager
    {
        private bool allowWildcardAction;
        private Dictionary<DirectionalAction, TransactionFlowOption> dictionary;
        private TransactionFlowOption flowIssuedTokens;
        private SecurityStandardsManager standardsManager;
        private System.ServiceModel.TransactionProtocol transactionProtocol;

        public TransactionChannelFactory(System.ServiceModel.TransactionProtocol transactionProtocol, BindingContext context, Dictionary<DirectionalAction, TransactionFlowOption> dictionary, bool allowWildcardAction) : base(context.Binding, context.BuildInnerChannelFactory<TChannel>())
        {
            this.dictionary = dictionary;
            this.TransactionProtocol = transactionProtocol;
            this.allowWildcardAction = allowWildcardAction;
            this.standardsManager = SecurityStandardsHelper.CreateStandardsManager(this.TransactionProtocol);
        }

        private TChannel CreateTransactionChannel(TChannel innerChannel)
        {
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel) new TransactionDuplexSessionChannel<TChannel>(this, (IDuplexSessionChannel) innerChannel);
            }
            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel) new TransactionRequestSessionChannel<TChannel>(this, (IRequestSessionChannel) innerChannel);
            }
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel) new TransactionOutputSessionChannel<TChannel>(this, (IOutputSessionChannel) innerChannel);
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel) new TransactionOutputChannel<TChannel>(this, (IOutputChannel) innerChannel);
            }
            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel) new TransactionRequestChannel<TChannel>(this, (IRequestChannel) innerChannel);
            }
            if (typeof(TChannel) != typeof(IDuplexChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(base.CreateChannelTypeNotSupportedException(typeof(TChannel)));
            }
            return (TChannel) new TransactionDuplexChannel<TChannel>(this, (IDuplexChannel) innerChannel);
        }

        public TransactionFlowOption GetTransaction(MessageDirection direction, string action)
        {
            TransactionFlowOption option;
            if (this.dictionary.TryGetValue(new DirectionalAction(direction, action), out option))
            {
                return option;
            }
            if (this.allowWildcardAction && this.dictionary.TryGetValue(new DirectionalAction(direction, "*"), out option))
            {
                return option;
            }
            return TransactionFlowOption.NotAllowed;
        }

        protected override TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            TChannel innerChannel = ((IChannelFactory<TChannel>) base.InnerChannelFactory).CreateChannel(remoteAddress, via);
            return this.CreateTransactionChannel(innerChannel);
        }

        public IDictionary<DirectionalAction, TransactionFlowOption> Dictionary
        {
            get
            {
                return this.dictionary;
            }
        }

        public TransactionFlowOption FlowIssuedTokens
        {
            get
            {
                return this.flowIssuedTokens;
            }
            set
            {
                this.flowIssuedTokens = value;
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                this.standardsManager = (value != null) ? value : SecurityStandardsHelper.CreateStandardsManager(this.transactionProtocol);
            }
        }

        public System.ServiceModel.TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!System.ServiceModel.TransactionProtocol.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SFxBadTransactionProtocols")));
                }
                this.transactionProtocol = value;
            }
        }

        private sealed class TransactionDuplexChannel : TransactionOutputDuplexChannelGeneric<IDuplexChannel>
        {
            public TransactionDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel) : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionDuplexSessionChannel : TransactionOutputDuplexChannelGeneric<IDuplexSessionChannel>, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            public TransactionDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel) : base(channelManager, innerChannel)
            {
            }

            public IDuplexSession Session
            {
                get
                {
                    return base.InnerChannel.Session;
                }
            }
        }

        private sealed class TransactionOutputChannel : TransactionOutputChannelGeneric<IOutputChannel>
        {
            public TransactionOutputChannel(ChannelManagerBase channelManager, IOutputChannel innerChannel) : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionOutputSessionChannel : TransactionOutputChannelGeneric<IOutputSessionChannel>, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public TransactionOutputSessionChannel(ChannelManagerBase channelManager, IOutputSessionChannel innerChannel) : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return base.InnerChannel.Session;
                }
            }
        }

        private sealed class TransactionRequestChannel : TransactionRequestChannelGeneric<IRequestChannel>
        {
            public TransactionRequestChannel(ChannelManagerBase channelManager, IRequestChannel innerChannel) : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionRequestSessionChannel : TransactionRequestChannelGeneric<IRequestSessionChannel>, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public TransactionRequestSessionChannel(ChannelManagerBase channelManager, IRequestSessionChannel innerChannel) : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return base.InnerChannel.Session;
                }
            }
        }
    }
}

