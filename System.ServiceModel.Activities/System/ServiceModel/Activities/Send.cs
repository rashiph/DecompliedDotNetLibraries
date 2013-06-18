namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xml.Linq;

    [ContentProperty("Content")]
    public sealed class Send : Activity
    {
        private bool? channelCacheEnabled;
        private Collection<CorrelationInitializer> correlationInitializers;
        private InternalSendMessage internalSend;
        private bool isOneWay;
        private Collection<System.Type> knownTypes;
        private IList<CorrelationQuery> lazyCorrelationQueries;
        private IClientMessageFormatter lazyFormatter;
        private ToRequest requestFormatter;

        public Send()
        {
            Func<Activity> func = null;
            this.isOneWay = true;
            this.TokenImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            if (func == null)
            {
                func = delegate {
                    if (this.internalSend == null)
                    {
                        return null;
                    }
                    if (this.requestFormatter == null)
                    {
                        return this.internalSend;
                    }
                    Variable<Message> variable = new Variable<Message> {
                        Name = "RequestMessage"
                    };
                    this.requestFormatter.Message = new OutArgument<Message>(variable);
                    this.requestFormatter.Send = this;
                    this.internalSend.Message = new InArgument<Message>(variable);
                    return new NoPersistScope { Body = new Sequence { Variables = { variable }, Activities = { this.requestFormatter, this.internalSend } } };
                };
            }
            base.Implementation = func;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(this.OperationName))
            {
                metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingOperationName(base.DisplayName));
            }
            if (this.ServiceContractName == null)
            {
                string errorMessageOperationName = ContractValidationHelper.GetErrorMessageOperationName(this.OperationName);
                metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingServiceContractName(base.DisplayName, errorMessageOperationName));
            }
            if (this.Endpoint == null)
            {
                if (string.IsNullOrEmpty(this.EndpointConfigurationName))
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.EndpointNotSet(base.DisplayName, this.OperationName));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.EndpointConfigurationName))
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.EndpointIncorrectlySet(base.DisplayName, this.OperationName));
                }
                if (this.Endpoint.Binding == null)
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingBindingInEndpoint(this.Endpoint.Name, this.ServiceContractName));
                }
            }
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, false, base.DisplayName, this.OperationName);
            MessagingActivityHelper.AddRuntimeArgument(this.CorrelatesWith, "CorrelatesWith", System.ServiceModel.Activities.Constants.CorrelationHandleType, ArgumentDirection.In, metadata);
            MessagingActivityHelper.AddRuntimeArgument(this.EndpointAddress, "EndpointAddress", System.ServiceModel.Activities.Constants.UriType, ArgumentDirection.In, metadata);
            this.InternalContent.CacheMetadata(metadata, this, this.OperationName);
            if (this.correlationInitializers != null)
            {
                for (int i = 0; i < this.correlationInitializers.Count; i++)
                {
                    CorrelationInitializer initializer = this.correlationInitializers[i];
                    initializer.ArgumentName = "Parameter" + i;
                    RuntimeArgument argument = new RuntimeArgument(initializer.ArgumentName, System.ServiceModel.Activities.Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(initializer.CorrelationHandle, argument);
                    metadata.AddArgument(argument);
                }
            }
            if (!metadata.HasViolations)
            {
                if ((this.InternalContent is SendMessageContent) && MessageBuilder.IsMessageContract(((SendMessageContent) this.InternalContent).InternalDeclaredMessageType))
                {
                    this.OperationUsesMessageContract = true;
                }
                this.internalSend = this.CreateInternalSend();
                this.InternalContent.ConfigureInternalSend(this.internalSend, out this.requestFormatter);
                if ((this.requestFormatter != null) && (this.lazyFormatter != null))
                {
                    this.requestFormatter.Formatter = this.lazyFormatter;
                }
            }
            else
            {
                this.internalSend = null;
                this.requestFormatter = null;
            }
        }

        private InternalSendMessage CreateInternalSend()
        {
            InternalSendMessage message2 = new InternalSendMessage {
                OperationName = this.OperationName
            };
            ArgumentValue<CorrelationHandle> expression = new ArgumentValue<CorrelationHandle> {
                ArgumentName = "CorrelatesWith"
            };
            message2.CorrelatesWith = new InArgument<CorrelationHandle>(expression);
            message2.Endpoint = this.Endpoint;
            message2.EndpointConfigurationName = this.EndpointConfigurationName;
            message2.IsOneWay = this.isOneWay;
            message2.IsSendReply = false;
            message2.TokenImpersonationLevel = this.TokenImpersonationLevel;
            message2.ServiceContractName = this.ServiceContractName;
            message2.Action = this.Action;
            message2.Parent = this;
            InternalSendMessage message = message2;
            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    message.CorrelationInitializers.Add(initializer.Clone());
                }
                Collection<CorrelationQuery> collection = ContractInferenceHelper.CreateClientCorrelationQueries(null, this.correlationInitializers, this.Action, this.ServiceContractName, this.OperationName, false);
                if (collection.Count == 1)
                {
                    message.CorrelationQuery = collection[0];
                }
            }
            if (this.EndpointAddress != null)
            {
                message.EndpointAddress = new InArgument<Uri>(context => this.EndpointAddress.Get(context));
            }
            if (this.lazyCorrelationQueries != null)
            {
                foreach (CorrelationQuery query in this.lazyCorrelationQueries)
                {
                    message.ReplyCorrelationQueries.Add(query);
                }
            }
            return message;
        }

        internal MessageVersion GetMessageVersion()
        {
            return this.internalSend.GetMessageVersion();
        }

        internal void InitializeChannelCacheEnabledSetting(ActivityContext context)
        {
            if (!this.channelCacheEnabled.HasValue)
            {
                SendMessageChannelCache extension = context.GetExtension<SendMessageChannelCache>();
                this.InitializeChannelCacheEnabledSetting(extension);
            }
        }

        internal void InitializeChannelCacheEnabledSetting(SendMessageChannelCache channelCacheExtension)
        {
            bool flag;
            ChannelCacheSettings factorySettings = channelCacheExtension.FactorySettings;
            if (((factorySettings.IdleTimeout == TimeSpan.Zero) || (factorySettings.LeaseTimeout == TimeSpan.Zero)) || (factorySettings.MaxItemsInCache == 0))
            {
                flag = false;
            }
            else
            {
                flag = true;
            }
            if (!this.channelCacheEnabled.HasValue)
            {
                this.channelCacheEnabled = new bool?(flag);
            }
        }

        internal void SetFormatter(IClientMessageFormatter formatter)
        {
            if (this.requestFormatter != null)
            {
                this.requestFormatter.Formatter = formatter;
            }
            else
            {
                this.lazyFormatter = formatter;
            }
        }

        internal void SetIsOneWay(bool value)
        {
            this.isOneWay = value;
            if (this.internalSend != null)
            {
                this.internalSend.IsOneWay = this.isOneWay;
            }
        }

        internal void SetReplyCorrelationQuery(CorrelationQuery replyQuery)
        {
            if ((this.internalSend != null) && !this.internalSend.ReplyCorrelationQueries.Contains(replyQuery))
            {
                this.internalSend.ReplyCorrelationQueries.Add(replyQuery);
            }
            else
            {
                if (this.lazyCorrelationQueries == null)
                {
                    this.lazyCorrelationQueries = new List<CorrelationQuery>();
                }
                this.lazyCorrelationQueries.Add(replyQuery);
            }
        }

        [DefaultValue((string) null)]
        public string Action { get; set; }

        internal bool ChannelCacheEnabled
        {
            get
            {
                return this.channelCacheEnabled.Value;
            }
        }

        [DefaultValue((string) null)]
        public SendContent Content { get; set; }

        [DefaultValue((string) null)]
        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }

        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        [DefaultValue((string) null)]
        public System.ServiceModel.Endpoint Endpoint { get; set; }

        [DefaultValue((string) null)]
        public InArgument<Uri> EndpointAddress { get; set; }

        [DefaultValue((string) null)]
        public string EndpointConfigurationName { get; set; }

        internal SendContent InternalContent
        {
            get
            {
                return (this.Content ?? SendContent.DefaultSendContent);
            }
        }

        public Collection<System.Type> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new Collection<System.Type>();
                }
                return this.knownTypes;
            }
        }

        internal System.ServiceModel.Description.OperationDescription OperationDescription { get; set; }

        [DefaultValue((string) null)]
        public string OperationName { get; set; }

        internal bool OperationUsesMessageContract { get; set; }

        [DefaultValue((string) null)]
        public System.Net.Security.ProtectionLevel? ProtectionLevel { get; set; }

        [DefaultValue(0)]
        public System.ServiceModel.Activities.SerializerOption SerializerOption { get; set; }

        [TypeConverter(typeof(ServiceXNameTypeConverter)), DefaultValue((string) null)]
        public XName ServiceContractName { get; set; }

        [DefaultValue(2)]
        public System.Security.Principal.TokenImpersonationLevel TokenImpersonationLevel { get; set; }
    }
}

