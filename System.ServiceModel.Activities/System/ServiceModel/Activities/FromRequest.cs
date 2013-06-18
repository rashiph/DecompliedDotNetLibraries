namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class FromRequest : NativeActivity
    {
        private Collection<OutArgument> parameters;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Message", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.InOut, true);
            metadata.Bind(this.Message, argument);
            metadata.AddArgument(argument);
            if (this.parameters != null)
            {
                int num = 0;
                foreach (OutArgument argument2 in this.parameters)
                {
                    RuntimeArgument argument3 = new RuntimeArgument("Parameter" + num++, argument2.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(argument2, argument3);
                    metadata.AddArgument(argument3);
                }
            }
            RuntimeArgument argument4 = new RuntimeArgument("noPersistHandle", System.ServiceModel.Activities.Constants.NoPersistHandleType, ArgumentDirection.In);
            metadata.Bind(this.NoPersistHandle, argument4);
            metadata.AddArgument(argument4);
        }

        protected override void Execute(NativeActivityContext context)
        {
            System.ServiceModel.Channels.Message message = null;
            try
            {
                object[] emptyArray;
                message = this.Message.Get(context);
                if ((this.parameters == null) || (this.parameters.Count == 0))
                {
                    emptyArray = System.ServiceModel.Activities.Constants.EmptyArray;
                }
                else
                {
                    emptyArray = new object[this.parameters.Count];
                }
                this.Formatter.DeserializeRequest(message, emptyArray);
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
            finally
            {
                if (this.CloseMessage && (message != null))
                {
                    message.Close();
                }
                this.Message.Set(context, null);
                System.Activities.NoPersistHandle handle = (this.NoPersistHandle == null) ? null : this.NoPersistHandle.Get(context);
                if (handle != null)
                {
                    handle.Exit(context);
                }
            }
        }

        internal bool CloseMessage { get; set; }

        public IDispatchMessageFormatter Formatter { get; set; }

        public InOutArgument<System.ServiceModel.Channels.Message> Message { get; set; }

        public InArgument<System.Activities.NoPersistHandle> NoPersistHandle { get; set; }

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
    }
}

