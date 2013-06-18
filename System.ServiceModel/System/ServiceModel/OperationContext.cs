namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public sealed class OperationContext : IExtensibleObject<OperationContext>
    {
        private ServiceChannel channel;
        private Message clientReply;
        private bool closeClientReply;
        [ThreadStatic]
        private static Holder currentContext;
        private System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher;
        private ExtensionCollection<OperationContext> extensions;
        private ServiceHostBase host;
        private System.ServiceModel.InstanceContext instanceContext;
        private bool isServiceReentrant;
        private MessageHeaders outgoingMessageHeaders;
        private MessageProperties outgoingMessageProperties;
        private MessageVersion outgoingMessageVersion;
        private Message request;
        private System.ServiceModel.Channels.RequestContext requestContext;
        internal IPrincipal threadPrincipal;
        private TransactionRpcFacet txFacet;

        public event EventHandler OperationCompleted;

        public OperationContext(IContextChannel channel)
        {
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("channel"));
            }
            ServiceChannel serviceChannel = channel as ServiceChannel;
            if (serviceChannel == null)
            {
                serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            }
            if (serviceChannel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidChannelToOperationContext")));
            }
            this.outgoingMessageVersion = serviceChannel.MessageVersion;
            this.channel = serviceChannel;
        }

        internal OperationContext(ServiceHostBase host) : this(host, MessageVersion.Soap12WSAddressing10)
        {
        }

        internal OperationContext(ServiceHostBase host, MessageVersion outgoingMessageVersion)
        {
            if (outgoingMessageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("outgoingMessageVersion"));
            }
            this.host = host;
            this.outgoingMessageVersion = outgoingMessageVersion;
        }

        internal OperationContext(System.ServiceModel.Channels.RequestContext requestContext, Message request, ServiceChannel channel, ServiceHostBase host)
        {
            this.channel = channel;
            this.host = host;
            this.requestContext = requestContext;
            this.request = request;
            this.outgoingMessageVersion = channel.MessageVersion;
        }

        internal void ClearClientReplyNoThrow()
        {
            this.clientReply = null;
        }

        internal void FireOperationCompleted()
        {
            try
            {
                EventHandler operationCompleted = this.OperationCompleted;
                if (operationCompleted != null)
                {
                    operationCompleted(this, EventArgs.Empty);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        public T GetCallbackChannel<T>()
        {
            if ((this.channel != null) && !this.IsUserContext)
            {
                return (T) this.channel.Proxy;
            }
            return default(T);
        }

        internal void Recycle()
        {
            this.requestContext = null;
            this.request = null;
            this.extensions = null;
            this.instanceContext = null;
            this.threadPrincipal = null;
            this.txFacet = null;
            this.SetClientReply(null, false);
        }

        internal void ReInit(System.ServiceModel.Channels.RequestContext requestContext, Message request, ServiceChannel channel)
        {
            this.requestContext = requestContext;
            this.request = request;
            this.channel = channel;
        }

        internal void SetClientReply(Message message, bool closeMessage)
        {
            Message clientReply = null;
            if (!object.Equals(message, this.clientReply))
            {
                if (this.closeClientReply && (this.clientReply != null))
                {
                    clientReply = this.clientReply;
                }
                this.clientReply = message;
            }
            this.closeClientReply = closeMessage;
            if (clientReply != null)
            {
                clientReply.Close();
            }
        }

        internal void SetInstanceContext(System.ServiceModel.InstanceContext instanceContext)
        {
            this.instanceContext = instanceContext;
        }

        public void SetTransactionComplete()
        {
            if (this.txFacet == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoTransactionInContext")));
            }
            this.txFacet.Completed();
        }

        public IContextChannel Channel
        {
            get
            {
                return this.GetCallbackChannel<IContextChannel>();
            }
        }

        public static OperationContext Current
        {
            get
            {
                return CurrentHolder.Context;
            }
            set
            {
                CurrentHolder.Context = value;
            }
        }

        internal static Holder CurrentHolder
        {
            get
            {
                Holder currentContext = OperationContext.currentContext;
                if (currentContext == null)
                {
                    currentContext = new Holder();
                    OperationContext.currentContext = currentContext;
                }
                return currentContext;
            }
        }

        public System.ServiceModel.Dispatcher.EndpointDispatcher EndpointDispatcher
        {
            get
            {
                return this.endpointDispatcher;
            }
            set
            {
                this.endpointDispatcher = value;
            }
        }

        public IExtensionCollection<OperationContext> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ExtensionCollection<OperationContext>(this);
                }
                return this.extensions;
            }
        }

        internal bool HasOutgoingMessageHeaders
        {
            get
            {
                return (this.outgoingMessageHeaders != null);
            }
        }

        internal bool HasOutgoingMessageProperties
        {
            get
            {
                return (this.outgoingMessageProperties != null);
            }
        }

        public bool HasSupportingTokens
        {
            get
            {
                MessageProperties incomingMessageProperties = this.IncomingMessageProperties;
                return (((incomingMessageProperties != null) && (incomingMessageProperties.Security != null)) && incomingMessageProperties.Security.HasIncomingSupportingTokens);
            }
        }

        public ServiceHostBase Host
        {
            get
            {
                return this.host;
            }
        }

        internal Message IncomingMessage
        {
            get
            {
                return (this.clientReply ?? this.request);
            }
        }

        public MessageHeaders IncomingMessageHeaders
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                {
                    return message.Headers;
                }
                return null;
            }
        }

        public MessageProperties IncomingMessageProperties
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                {
                    return message.Properties;
                }
                return null;
            }
        }

        public MessageVersion IncomingMessageVersion
        {
            get
            {
                Message message = this.clientReply ?? this.request;
                if (message != null)
                {
                    return message.Version;
                }
                return null;
            }
        }

        public System.ServiceModel.InstanceContext InstanceContext
        {
            get
            {
                return this.instanceContext;
            }
        }

        internal ServiceChannel InternalServiceChannel
        {
            get
            {
                return this.channel;
            }
            set
            {
                this.channel = value;
            }
        }

        internal bool IsServiceReentrant
        {
            get
            {
                return this.isServiceReentrant;
            }
            set
            {
                this.isServiceReentrant = value;
            }
        }

        public bool IsUserContext
        {
            get
            {
                return (this.request == null);
            }
        }

        public MessageHeaders OutgoingMessageHeaders
        {
            get
            {
                if (this.outgoingMessageHeaders == null)
                {
                    this.outgoingMessageHeaders = new MessageHeaders(this.OutgoingMessageVersion);
                }
                return this.outgoingMessageHeaders;
            }
        }

        public MessageProperties OutgoingMessageProperties
        {
            get
            {
                if (this.outgoingMessageProperties == null)
                {
                    this.outgoingMessageProperties = new MessageProperties();
                }
                return this.outgoingMessageProperties;
            }
        }

        internal MessageVersion OutgoingMessageVersion
        {
            get
            {
                return this.outgoingMessageVersion;
            }
        }

        public System.ServiceModel.Channels.RequestContext RequestContext
        {
            get
            {
                return this.requestContext;
            }
            set
            {
                this.requestContext = value;
            }
        }

        public System.ServiceModel.ServiceSecurityContext ServiceSecurityContext
        {
            get
            {
                MessageProperties incomingMessageProperties = this.IncomingMessageProperties;
                if ((incomingMessageProperties != null) && (incomingMessageProperties.Security != null))
                {
                    return incomingMessageProperties.Security.ServiceSecurityContext;
                }
                return null;
            }
        }

        public string SessionId
        {
            get
            {
                if (this.channel != null)
                {
                    IChannel innerChannel = this.channel.InnerChannel;
                    if (innerChannel != null)
                    {
                        ISessionChannel<IDuplexSession> channel2 = innerChannel as ISessionChannel<IDuplexSession>;
                        if ((channel2 != null) && (channel2.Session != null))
                        {
                            return channel2.Session.Id;
                        }
                        ISessionChannel<IInputSession> channel3 = innerChannel as ISessionChannel<IInputSession>;
                        if ((channel3 != null) && (channel3.Session != null))
                        {
                            return channel3.Session.Id;
                        }
                        ISessionChannel<IOutputSession> channel4 = innerChannel as ISessionChannel<IOutputSession>;
                        if ((channel4 != null) && (channel4.Session != null))
                        {
                            return channel4.Session.Id;
                        }
                    }
                }
                return null;
            }
        }

        public ICollection<SupportingTokenSpecification> SupportingTokens
        {
            get
            {
                MessageProperties incomingMessageProperties = this.IncomingMessageProperties;
                if ((incomingMessageProperties != null) && (incomingMessageProperties.Security != null))
                {
                    return new ReadOnlyCollection<SupportingTokenSpecification>(incomingMessageProperties.Security.IncomingSupportingTokens);
                }
                return null;
            }
        }

        internal IPrincipal ThreadPrincipal
        {
            get
            {
                return this.threadPrincipal;
            }
            set
            {
                this.threadPrincipal = value;
            }
        }

        internal TransactionRpcFacet TransactionFacet
        {
            get
            {
                return this.txFacet;
            }
            set
            {
                this.txFacet = value;
            }
        }

        internal class Holder
        {
            private OperationContext context;

            public OperationContext Context
            {
                get
                {
                    return this.context;
                }
                set
                {
                    this.context = value;
                }
            }
        }
    }
}

