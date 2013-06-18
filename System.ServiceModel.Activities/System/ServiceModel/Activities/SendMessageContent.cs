namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Description;
    using System.Windows.Markup;

    [ContentProperty("Message")]
    public sealed class SendMessageContent : SendContent
    {
        public SendMessageContent()
        {
        }

        public SendMessageContent(InArgument message) : this()
        {
            this.Message = message;
        }

        public SendMessageContent(InArgument message, Type declaredMessageType) : this(message)
        {
            this.DeclaredMessageType = declaredMessageType;
        }

        internal override void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName)
        {
            MessagingActivityHelper.FixMessageArgument(this.Message, ArgumentDirection.In, metadata);
            if (this.DeclaredMessageType != null)
            {
                if ((this.Message == null) && (this.DeclaredMessageType != TypeHelper.VoidType))
                {
                    string errorMessageOperationName = ContractValidationHelper.GetErrorMessageOperationName(operationName);
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.ValueCannotBeNull(owner.DisplayName, errorMessageOperationName));
                }
                else if ((this.Message != null) && !this.DeclaredMessageType.IsAssignableFrom(this.Message.ArgumentType))
                {
                    string str2 = ContractValidationHelper.GetErrorMessageOperationName(operationName);
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.ValueArgumentTypeNotDerivedFromValueType(owner.DisplayName, str2));
                }
            }
        }

        internal override void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalSendMessage.Message = new InArgument<System.ServiceModel.Channels.Message>(context => ((InArgument<System.ServiceModel.Channels.Message>) this.Message).Get(context));
                requestFormatter = null;
            }
            else
            {
                requestFormatter = new ToRequest();
                if (this.Message != null)
                {
                    requestFormatter.Parameters.Add(InArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalSendMessage.Message = new InArgument<System.ServiceModel.Channels.Message>(context => ((InArgument<System.ServiceModel.Channels.Message>) this.Message).Get(context));
                responseFormatter = null;
            }
            else
            {
                responseFormatter = new ToReply();
                if (MessageBuilder.IsMessageContract(this.InternalDeclaredMessageType))
                {
                    responseFormatter.Result = InArgument.CreateReference(this.Message, "Message");
                }
                else if (this.Message != null)
                {
                    responseFormatter.Parameters.Add(InArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.InternalDeclaredMessageType);
            string overridingAction = null;
            SerializerOption dataContractSerializer = SerializerOption.DataContractSerializer;
            Send send = owner as Send;
            if (send != null)
            {
                overridingAction = send.Action;
                dataContractSerializer = send.SerializerOption;
            }
            else
            {
                SendReply reply = owner as SendReply;
                overridingAction = reply.Action;
                dataContractSerializer = reply.Request.SerializerOption;
            }
            if (direction == MessageDirection.Input)
            {
                ContractInferenceHelper.AddInputMessage(operation, overridingAction, this.InternalDeclaredMessageType, dataContractSerializer);
            }
            else
            {
                ContractInferenceHelper.AddOutputMessage(operation, overridingAction, this.InternalDeclaredMessageType, dataContractSerializer);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeDeclaredMessageType()
        {
            if (this.DeclaredMessageType == null)
            {
                return false;
            }
            if ((this.Message != null) && (this.DeclaredMessageType == this.Message.ArgumentType))
            {
                return false;
            }
            return true;
        }

        [DefaultValue((string) null)]
        public Type DeclaredMessageType { get; set; }

        internal Type InternalDeclaredMessageType
        {
            get
            {
                if ((this.DeclaredMessageType == null) && (this.Message != null))
                {
                    return this.Message.ArgumentType;
                }
                return this.DeclaredMessageType;
            }
        }

        internal override bool IsFault
        {
            get
            {
                return ((this.InternalDeclaredMessageType != null) && ContractInferenceHelper.ExceptionType.IsAssignableFrom(this.InternalDeclaredMessageType));
            }
        }

        [DefaultValue((string) null)]
        public InArgument Message { get; set; }
    }
}

