namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class ToRequest : CodeActivity
    {
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private Collection<InArgument> parameters;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (this.parameters != null)
            {
                int num = 0;
                foreach (InArgument argument in this.parameters)
                {
                    RuntimeArgument argument2 = new RuntimeArgument("Parameter" + num++, argument.ArgumentType, ArgumentDirection.In);
                    metadata.Bind(argument, argument2);
                    metadata.AddArgument(argument2);
                }
            }
            RuntimeArgument argument3 = new RuntimeArgument("Message", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.Out, true);
            metadata.Bind(this.Message, argument3);
            metadata.AddArgument(argument3);
        }

        protected override void Execute(CodeActivityContext context)
        {
            object[] emptyArray;
            if ((this.parameters == null) || (this.parameters.Count == 0))
            {
                emptyArray = System.ServiceModel.Activities.Constants.EmptyArray;
            }
            else
            {
                emptyArray = new object[this.parameters.Count];
                for (int i = 0; i < this.parameters.Count; i++)
                {
                    emptyArray[i] = this.parameters[i].Get(context);
                }
            }
            if (this.Formatter == null)
            {
                OperationDescription operationDescription = ContractInferenceHelper.CreateOneWayOperationDescription(this.Send);
                this.Formatter = ClientOperationFormatterProvider.GetFormatterFromRuntime(operationDescription);
                this.Send.OperationDescription = operationDescription;
            }
            this.Send.InitializeChannelCacheEnabledSetting(context);
            System.ServiceModel.Channels.Message message = this.Formatter.SerializeRequest(this.MessageVersion, emptyArray);
            this.Message.Set(context, message);
        }

        public IClientMessageFormatter Formatter { get; set; }

        public OutArgument<System.ServiceModel.Channels.Message> Message { get; set; }

        internal System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                if (this.messageVersion == null)
                {
                    this.messageVersion = this.Send.GetMessageVersion();
                }
                return this.messageVersion;
            }
        }

        public Collection<InArgument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Collection<InArgument>();
                }
                return this.parameters;
            }
        }

        public System.ServiceModel.Activities.Send Send { get; set; }
    }
}

