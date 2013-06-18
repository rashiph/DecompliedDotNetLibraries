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
    public sealed class ReceiveMessageContent : ReceiveContent
    {
        public ReceiveMessageContent()
        {
        }

        public ReceiveMessageContent(OutArgument message) : this()
        {
            this.Message = message;
        }

        public ReceiveMessageContent(OutArgument message, Type declaredMessageType) : this(message)
        {
            this.DeclaredMessageType = declaredMessageType;
        }

        internal override void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName)
        {
            MessagingActivityHelper.FixMessageArgument(this.Message, ArgumentDirection.Out, metadata);
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

        internal override void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalReceiveMessage.Message = new OutArgument<System.ServiceModel.Channels.Message>(context => ((OutArgument<System.ServiceModel.Channels.Message>) this.Message).Get(context));
                requestFormatter = null;
            }
            else
            {
                requestFormatter = new FromRequest();
                if (this.Message != null)
                {
                    requestFormatter.Parameters.Add(OutArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalReceiveMessage.Message = new OutArgument<System.ServiceModel.Channels.Message>(context => ((OutArgument<System.ServiceModel.Channels.Message>) this.Message).Get(context));
                responseFormatter = null;
            }
            else
            {
                responseFormatter = new FromReply();
                if (MessageBuilder.IsMessageContract(this.InternalDeclaredMessageType))
                {
                    responseFormatter.Result = OutArgument.CreateReference(this.Message, "Message");
                }
                else if (this.Message != null)
                {
                    responseFormatter.Parameters.Add(OutArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.InternalDeclaredMessageType);
            string overridingAction = null;
            SerializerOption dataContractSerializer = SerializerOption.DataContractSerializer;
            Receive receive = owner as Receive;
            if (receive != null)
            {
                overridingAction = receive.Action;
                dataContractSerializer = receive.SerializerOption;
            }
            else
            {
                ReceiveReply reply = owner as ReceiveReply;
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

        [DefaultValue((string) null)]
        public OutArgument Message { get; set; }
    }
}

