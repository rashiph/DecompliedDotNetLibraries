namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;
    using System.Xml.Linq;

    [ContentProperty("Content")]
    public sealed class SendReply : Activity
    {
        private Collection<CorrelationInitializer> correlationInitializers;
        private InternalSendMessage internalSend;
        private ToReply responseFormatter;

        public SendReply()
        {
            Func<Activity> func = null;
            if (func == null)
            {
                func = delegate {
                    if (this.internalSend == null)
                    {
                        return null;
                    }
                    if (this.responseFormatter == null)
                    {
                        return this.internalSend;
                    }
                    Variable<Message> variable = new Variable<Message> {
                        Name = "ResponseMessage"
                    };
                    this.responseFormatter.Message = new OutArgument<Message>(variable);
                    this.internalSend.Message = new InArgument<Message>(variable);
                    this.internalSend.MessageOut = new OutArgument<Message>(variable);
                    return new Sequence { Variables = { variable }, Activities = { this.responseFormatter, this.internalSend } };
                };
            }
            base.Implementation = func;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(System.ServiceModel.Activities.SR.SendReplyRequestCannotBeNull(base.DisplayName));
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
                this.internalSend = this.CreateInternalSend();
                this.InternalContent.ConfigureInternalSendReply(this.internalSend, out this.responseFormatter);
                InArgument<CorrelationHandle> replyHandleFromReceive = this.GetReplyHandleFromReceive();
                if (replyHandleFromReceive != null)
                {
                    InArgument<CorrelationHandle> binding = MessagingActivityHelper.CreateReplyCorrelatesWith(replyHandleFromReceive);
                    RuntimeArgument argument4 = new RuntimeArgument("InternalSendCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(binding, argument4);
                    metadata.AddArgument(argument4);
                    this.internalSend.CorrelatesWith = (InArgument<CorrelationHandle>) InArgument.CreateReference(binding, "InternalSendCorrelatesWith");
                    if (this.responseFormatter != null)
                    {
                        InArgument<CorrelationHandle> argument5 = MessagingActivityHelper.CreateReplyCorrelatesWith(replyHandleFromReceive);
                        RuntimeArgument argument6 = new RuntimeArgument("ResponseFormatterCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                        metadata.Bind(argument5, argument6);
                        metadata.AddArgument(argument6);
                        this.responseFormatter.CorrelatesWith = (InArgument<CorrelationHandle>) InArgument.CreateReference(argument5, "ResponseFormatterCorrelatesWith");
                    }
                }
            }
            else
            {
                this.internalSend = null;
                this.responseFormatter = null;
            }
            metadata.SetImportedChildrenCollection(new Collection<Activity>());
        }

        private InternalSendMessage CreateInternalSend()
        {
            InternalSendMessage message = new InternalSendMessage {
                IsSendReply = true,
                ShouldPersistBeforeSend = this.PersistBeforeSend,
                OperationName = this.Request.OperationName
            };
            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    message.CorrelationInitializers.Add(initializer.Clone());
                }
            }
            return message;
        }

        private InArgument<CorrelationHandle> GetReplyHandleFromReceive()
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

        internal void SetContractName(XName contractName)
        {
            this.internalSend.ServiceContractName = contractName;
        }

        internal void SetFaultFormatter(IDispatchFaultFormatter faultFormatter, bool includeExceptionDetailInFaults)
        {
            this.responseFormatter.FaultFormatter = faultFormatter;
            this.responseFormatter.IncludeExceptionDetailInFaults = includeExceptionDetailInFaults;
        }

        internal void SetFormatter(IDispatchMessageFormatter formatter)
        {
            this.responseFormatter.Formatter = formatter;
        }

        [DefaultValue((string) null)]
        public string Action { get; set; }

        [DefaultValue((string) null)]
        public SendContent Content { get; set; }

        [DefaultValue((string) null)]
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

        internal SendContent InternalContent
        {
            get
            {
                return (this.Content ?? SendContent.DefaultSendContent);
            }
        }

        [DefaultValue(false)]
        public bool PersistBeforeSend { get; set; }

        [DefaultValue((string) null)]
        public Receive Request { get; set; }
    }
}

