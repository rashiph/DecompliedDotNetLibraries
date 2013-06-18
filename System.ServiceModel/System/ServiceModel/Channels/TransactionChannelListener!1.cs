namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal sealed class TransactionChannelListener<TChannel> : DelegatingChannelListener<TChannel>, ITransactionChannelManager where TChannel: class, IChannel
    {
        private Dictionary<DirectionalAction, TransactionFlowOption> dictionary;
        private TransactionFlowOption flowIssuedTokens;
        private SecurityStandardsManager standardsManager;
        private System.ServiceModel.TransactionProtocol transactionProtocol;

        public TransactionChannelListener(System.ServiceModel.TransactionProtocol transactionProtocol, IDefaultCommunicationTimeouts timeouts, Dictionary<DirectionalAction, TransactionFlowOption> dictionary, IChannelListener<TChannel> innerListener) : base(timeouts, innerListener)
        {
            this.dictionary = dictionary;
            this.TransactionProtocol = transactionProtocol;
            base.Acceptor = new TransactionChannelAcceptor<TChannel>((TransactionChannelListener<TChannel>) this, innerListener);
            this.standardsManager = SecurityStandardsHelper.CreateStandardsManager(this.TransactionProtocol);
        }

        public TransactionFlowOption GetTransaction(MessageDirection direction, string action)
        {
            TransactionFlowOption option;
            if (this.dictionary.TryGetValue(new DirectionalAction(direction, action), out option))
            {
                return option;
            }
            if (this.dictionary.TryGetValue(new DirectionalAction(direction, "*"), out option))
            {
                return option;
            }
            return TransactionFlowOption.NotAllowed;
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

        private class TransactionChannelAcceptor : LayeredChannelAcceptor<TChannel, TChannel>
        {
            private TransactionChannelListener<TChannel> listener;

            public TransactionChannelAcceptor(TransactionChannelListener<TChannel> listener, IChannelListener<TChannel> innerListener) : base(listener, innerListener)
            {
                this.listener = listener;
            }

            protected override TChannel OnAcceptChannel(TChannel innerChannel)
            {
                if (typeof(TChannel) == typeof(IInputSessionChannel))
                {
                    return (TChannel) new TransactionChannelListener<TChannel>.TransactionInputSessionChannel(this.listener, (IInputSessionChannel) innerChannel);
                }
                if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                {
                    return (TChannel) new TransactionChannelListener<TChannel>.TransactionDuplexSessionChannel(this.listener, (IDuplexSessionChannel) innerChannel);
                }
                if (typeof(TChannel) == typeof(IInputChannel))
                {
                    return (TChannel) new TransactionChannelListener<TChannel>.TransactionInputChannel(this.listener, (IInputChannel) innerChannel);
                }
                if (typeof(TChannel) == typeof(IReplyChannel))
                {
                    return (TChannel) new TransactionChannelListener<TChannel>.TransactionReplyChannel(this.listener, (IReplyChannel) innerChannel);
                }
                if (typeof(TChannel) == typeof(IReplySessionChannel))
                {
                    return (TChannel) new TransactionChannelListener<TChannel>.TransactionReplySessionChannel(this.listener, (IReplySessionChannel) innerChannel);
                }
                if (typeof(TChannel) != typeof(IDuplexChannel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.listener.CreateChannelTypeNotSupportedException(typeof(TChannel)));
                }
                return (TChannel) new TransactionChannelListener<TChannel>.TransactionDuplexChannel(this.listener, (IDuplexChannel) innerChannel);
            }
        }

        private sealed class TransactionDuplexChannel : TransactionInputDuplexChannelGeneric<IDuplexChannel>
        {
            public TransactionDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel) : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionDuplexSessionChannel : TransactionInputDuplexChannelGeneric<IDuplexSessionChannel>, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
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

        private sealed class TransactionInputChannel : TransactionReceiveChannelGeneric<IInputChannel>
        {
            public TransactionInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel) : base(channelManager, innerChannel, MessageDirection.Input)
            {
            }
        }

        private sealed class TransactionInputSessionChannel : TransactionReceiveChannelGeneric<IInputSessionChannel>, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
        {
            public TransactionInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel) : base(channelManager, innerChannel, MessageDirection.Input)
            {
            }

            public IInputSession Session
            {
                get
                {
                    return base.InnerChannel.Session;
                }
            }
        }

        private sealed class TransactionReplyChannel : TransactionReplyChannelGeneric<IReplyChannel>
        {
            public TransactionReplyChannel(ChannelManagerBase channelManager, IReplyChannel innerChannel) : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionReplySessionChannel : TransactionReplyChannelGeneric<IReplySessionChannel>, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
        {
            public TransactionReplySessionChannel(ChannelManagerBase channelManager, IReplySessionChannel innerChannel) : base(channelManager, innerChannel)
            {
            }

            public IInputSession Session
            {
                get
                {
                    return base.InnerChannel.Session;
                }
            }
        }
    }
}

