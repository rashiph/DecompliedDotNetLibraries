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
    public sealed class ReceiveParametersContent : ReceiveContent
    {
        private string[] argumentNames;
        private Type[] argumentTypes;

        public ReceiveParametersContent()
        {
            this.Parameters = new OrderedDictionary<string, OutArgument>();
        }

        public ReceiveParametersContent(IDictionary<string, OutArgument> parameters)
        {
            if (parameters == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("parameters");
            }
            this.Parameters = new OrderedDictionary<string, OutArgument>(parameters);
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
                    metadata.AddValidationError(System.ServiceModel.Activities.SR.ReceiveParametersContentDoesNotSupportMessage(owner.DisplayName, this.argumentNames[index]));
                }
                index++;
            }
            if (!metadata.HasViolations)
            {
                foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
                {
                    RuntimeArgument argument = new RuntimeArgument(pair.Key, pair.Value.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(pair.Value, argument);
                    metadata.AddArgument(argument);
                }
            }
        }

        internal override void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter)
        {
            requestFormatter = new FromRequest();
            foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
            {
                requestFormatter.Parameters.Add(OutArgument.CreateReference(pair.Value, pair.Key));
            }
        }

        internal override void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter)
        {
            responseFormatter = new FromReply();
            foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
            {
                responseFormatter.Parameters.Add(OutArgument.CreateReference(pair.Value, pair.Key));
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.argumentTypes);
            string overridingAction = (owner is Receive) ? ((Receive) owner).Action : ((ReceiveReply) owner).Action;
            if (direction == MessageDirection.Input)
            {
                ContractInferenceHelper.AddInputMessage(operation, overridingAction, this.argumentNames, this.argumentTypes);
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
            foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
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

        public IDictionary<string, OutArgument> Parameters { get; private set; }
    }
}

