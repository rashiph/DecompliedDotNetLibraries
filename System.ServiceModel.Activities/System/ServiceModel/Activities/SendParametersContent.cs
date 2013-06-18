namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Description;
    using System.Windows.Markup;

    [ContentProperty("Parameters")]
    public sealed class SendParametersContent : SendContent
    {
        private string[] argumentNames;
        private Type[] argumentTypes;

        public SendParametersContent()
        {
            this.Parameters = new OrderedDictionary<string, InArgument>();
        }

        public SendParametersContent(IDictionary<string, InArgument> parameters)
        {
            if (parameters == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("parameters");
            }
            this.Parameters = new OrderedDictionary<string, InArgument>(parameters);
        }

        internal override void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName)
        {
            this.ShredParameters();
            int index = 0;
            foreach (Type type in this.argumentTypes)
            {
                if ((type == null) || (type == TypeHelper.VoidType))
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.ArgumentCannotHaveNullOrVoidType(owner.DisplayName, this.argumentNames[index]));
                }
                if ((type == MessageDescription.TypeOfUntypedMessage) || MessageBuilder.IsMessageContract(type))
                {
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.SendParametersContentDoesNotSupportMessage(owner.DisplayName, this.argumentNames[index]));
                }
                index++;
            }
            if (!metadata.HasViolations)
            {
                foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
                {
                    RuntimeArgument argument = new RuntimeArgument(pair.Key, pair.Value.ArgumentType, ArgumentDirection.In);
                    metadata.Bind(pair.Value, argument);
                    metadata.AddArgument(argument);
                }
            }
        }

        internal override void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter)
        {
            requestFormatter = new ToRequest();
            foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
            {
                requestFormatter.Parameters.Add(InArgument.CreateReference(pair.Value, pair.Key));
            }
        }

        internal override void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter)
        {
            responseFormatter = new ToReply();
            foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
            {
                responseFormatter.Parameters.Add(InArgument.CreateReference(pair.Value, pair.Key));
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.ArgumentTypes);
            string overridingAction = (owner is Send) ? ((Send) owner).Action : ((SendReply) owner).Action;
            if (direction == MessageDirection.Input)
            {
                ContractInferenceHelper.AddInputMessage(operation, overridingAction, this.ArgumentNames, this.ArgumentTypes);
            }
            else
            {
                ContractInferenceHelper.AddOutputMessage(operation, overridingAction, this.ArgumentNames, this.ArgumentTypes);
            }
        }

        private void ShredParameters()
        {
            int count = this.Parameters.Count;
            this.argumentNames = new string[count];
            this.argumentTypes = new Type[count];
            int index = 0;
            foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
            {
                this.argumentNames[index] = pair.Key;
                this.argumentTypes[index] = pair.Value.ArgumentType;
                index++;
            }
        }

        internal string[] ArgumentNames
        {
            get
            {
                if (this.argumentNames == null)
                {
                    this.ShredParameters();
                }
                return this.argumentNames;
            }
        }

        internal Type[] ArgumentTypes
        {
            get
            {
                if (this.argumentTypes == null)
                {
                    this.ShredParameters();
                }
                return this.argumentTypes;
            }
        }

        internal override bool IsFault
        {
            get
            {
                return ((this.ArgumentTypes.Length == 1) && ContractInferenceHelper.ExceptionType.IsAssignableFrom(this.ArgumentTypes[0]));
            }
        }

        public IDictionary<string, InArgument> Parameters { get; private set; }
    }
}

