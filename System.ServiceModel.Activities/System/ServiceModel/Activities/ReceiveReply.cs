namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    [ContentProperty("Content")]
    public sealed class ReceiveReply : Activity
    {
        private Collection<CorrelationInitializer> correlationInitializers;
        private InternalReceiveMessage internalReceive;
        private FromReply responseFormatter;

        public ReceiveReply()
        {
            Func<Activity> func = null;
            if (func == null)
            {
                func = delegate {
                    if (this.internalReceive == null)
                    {
                        return null;
                    }
                    if (this.responseFormatter == null)
                    {
                        return this.internalReceive;
                    }
                    Variable<Message> variable = new Variable<Message> {
                        Name = "ResponseMessage"
                    };
                    this.internalReceive.Message = new OutArgument<Message>(variable);
                    this.responseFormatter.Message = new InArgument<Message>(variable);
                    return new NoPersistScope { Body = new Sequence { Variables = { variable }, Activities = { this.internalReceive, this.responseFormatter } } };
                };
            }
            base.Implementation = func;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(System.ServiceModel.Activities.SR.ReceiveReplyRequestCannotBeNull(base.DisplayName));
            }
            else
            {
                if (this.Request.ServiceContractName == null)
                {
                    string errorMessageOperationName = ContractValidationHelper.GetErrorMessageOperationName(this.Request.OperationName);
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingServiceContractName(this.Request.DisplayName, errorMessageOperationName));
                }
                if (string.IsNullOrEmpty(this.Request.OperationName))
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.MissingOperationName(this.Request.DisplayName));
                }
            }
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, true, base.DisplayName, (this.Request != null) ? this.Request.OperationName : string.Empty);
            string operationName = (this.Request != null) ? this.Request.OperationName : null;
            this.InternalContent.CacheMetadata(metadata, this, operationName);
            if (this.correlationInitializers != null)
            {
                for (int i = 0; i < this.correlationInitializers.Count; i++)
                {
                    CorrelationInitializer initializer = this.correlationInitializers[i];
                    initializer.ArgumentName = "Parameter" + i;
                    RuntimeArgument argument = new RuntimeArgument(initializer.ArgumentName, Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(initializer.CorrelationHandle, argument);
                    metadata.AddArgument(argument);
                }
            }
            if (!metadata.HasViolations)
            {
                this.internalReceive = this.CreateInternalReceive();
                InArgument<CorrelationHandle> replyHandleFromSend = this.GetReplyHandleFromSend();
                if (replyHandleFromSend != null)
                {
                    InArgument<CorrelationHandle> binding = MessagingActivityHelper.CreateReplyCorrelatesWith(replyHandleFromSend);
                    RuntimeArgument argument4 = new RuntimeArgument("ResultCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(binding, argument4);
                    metadata.AddArgument(argument4);
                    this.internalReceive.CorrelatesWith = (InArgument<CorrelationHandle>) InArgument.CreateReference(binding, "ResultCorrelatesWith");
                }
                this.InternalContent.ConfigureInternalReceiveReply(this.internalReceive, out this.responseFormatter);
                if ((this.InternalContent is ReceiveMessageContent) && MessageBuilder.IsMessageContract(((ReceiveMessageContent) this.InternalContent).InternalDeclaredMessageType))
                {
                    this.Request.OperationUsesMessageContract = true;
                }
                OperationDescription operationDescription = ContractInferenceHelper.CreateTwoWayOperationDescription(this.Request, this);
                this.Request.OperationDescription = operationDescription;
                if (this.responseFormatter != null)
                {
                    IClientMessageFormatter formatterFromRuntime = ClientOperationFormatterProvider.GetFormatterFromRuntime(operationDescription);
                    this.Request.SetFormatter(formatterFromRuntime);
                    this.responseFormatter.Formatter = formatterFromRuntime;
                    int index = 0;
                    Type[] detailTypes = new Type[operationDescription.KnownTypes.Count];
                    foreach (Type type in operationDescription.KnownTypes)
                    {
                        detailTypes[index] = type;
                        index++;
                    }
                    this.responseFormatter.FaultFormatter = new FaultFormatter(detailTypes);
                }
                if ((this.correlationInitializers != null) && (this.correlationInitializers.Count > 0))
                {
                    foreach (CorrelationQuery query in ContractInferenceHelper.CreateClientCorrelationQueries(null, this.correlationInitializers, this.Action, this.Request.ServiceContractName, this.Request.OperationName, true))
                    {
                        this.Request.SetReplyCorrelationQuery(query);
                    }
                }
            }
            else
            {
                this.internalReceive = null;
                this.responseFormatter = null;
            }
            metadata.SetImportedChildrenCollection(new Collection<Activity>());
        }

        private InternalReceiveMessage CreateInternalReceive()
        {
            InternalReceiveMessage message = new InternalReceiveMessage {
                IsOneWay = false
            };
            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    message.CorrelationInitializers.Add(initializer.Clone());
                }
            }
            if (this.Request != null)
            {
                this.Request.SetIsOneWay(false);
            }
            return message;
        }

        private InArgument<CorrelationHandle> GetReplyHandleFromSend()
        {
            if (this.Request != null)
            {
                foreach (CorrelationInitializer initializer in this.Request.CorrelationInitializers)
                {
                    RequestReplyCorrelationInitializer initializer2 = initializer as RequestReplyCorrelationInitializer;
                    if ((initializer2 != null) && (initializer2.CorrelationHandle != null))
                    {
                        return initializer2.CorrelationHandle;
                    }
                }
            }
            return null;
        }

        [DefaultValue((string) null)]
        public string Action { get; set; }

        [DefaultValue((string) null)]
        public ReceiveContent Content { get; set; }

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

        internal ReceiveContent InternalContent
        {
            get
            {
                return (this.Content ?? ReceiveContent.DefaultReceiveContent);
            }
        }

        [DefaultValue((string) null)]
        public Send Request { get; set; }
    }
}

