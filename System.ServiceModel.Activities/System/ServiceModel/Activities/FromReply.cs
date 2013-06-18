namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class FromReply : CodeActivity
    {
        private Collection<OutArgument> parameters;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Message", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.In, true);
            metadata.Bind(this.Message, argument);
            metadata.AddArgument(argument);
            if (this.Result != null)
            {
                RuntimeArgument argument2 = new RuntimeArgument("Result", this.Result.ArgumentType, ArgumentDirection.Out);
                metadata.Bind(this.Result, argument2);
                metadata.AddArgument(argument2);
            }
            if (this.parameters != null)
            {
                int num = 0;
                foreach (OutArgument argument3 in this.parameters)
                {
                    RuntimeArgument argument4 = new RuntimeArgument("Message" + num++, argument3.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(argument3, argument4);
                    metadata.AddArgument(argument4);
                }
            }
        }

        private Exception DeserializeFault(System.ServiceModel.Channels.Message inMessage, FaultConverter faultConverter)
        {
            MessageFault fault = MessageFault.CreateFault(inMessage, 0x10000);
            string action = inMessage.Headers.Action;
            if (action == inMessage.Version.Addressing.DefaultFaultAction)
            {
                action = null;
            }
            Exception exception = null;
            if (faultConverter != null)
            {
                faultConverter.TryCreateException(inMessage, fault, out exception);
            }
            if (exception == null)
            {
                exception = this.FaultFormatter.Deserialize(fault, action);
            }
            if (inMessage.State != MessageState.Created)
            {
                inMessage.Close();
            }
            return exception;
        }

        protected override void Execute(CodeActivityContext context)
        {
            object[] emptyArray;
            System.ServiceModel.Channels.Message inMessage = this.Message.Get(context);
            if (inMessage == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.NullReplyMessageContractMismatch));
            }
            if (inMessage.IsFault)
            {
                FaultConverter defaultFaultConverter = FaultConverter.GetDefaultFaultConverter(inMessage.Version);
                Exception exception = this.DeserializeFault(inMessage, defaultFaultConverter);
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(exception);
            }
            if (this.parameters != null)
            {
                emptyArray = new object[this.parameters.Count];
            }
            else
            {
                emptyArray = System.ServiceModel.Activities.Constants.EmptyArray;
            }
            object obj2 = this.Formatter.DeserializeReply(inMessage, emptyArray);
            if (this.Result != null)
            {
                this.Result.Set(context, obj2);
            }
            if (this.parameters != null)
            {
                for (int i = 0; i < this.parameters.Count; i++)
                {
                    OutArgument argument = this.parameters[i];
                    object defaultParameterValue = emptyArray[i];
                    if (defaultParameterValue == null)
                    {
                        defaultParameterValue = ProxyOperationRuntime.GetDefaultParameterValue(argument.ArgumentType);
                    }
                    argument.Set(context, defaultParameterValue);
                }
            }
        }

        public IClientFaultFormatter FaultFormatter { get; set; }

        public IClientMessageFormatter Formatter { get; set; }

        public InArgument<System.ServiceModel.Channels.Message> Message { get; set; }

        public Collection<OutArgument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Collection<OutArgument>();
                }
                return this.parameters;
            }
        }

        public OutArgument Result { get; set; }
    }
}

