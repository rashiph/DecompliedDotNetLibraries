namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class ToReply : NativeActivity
    {
        private IDispatchFaultFormatter faultFormatter;
        private IDispatchMessageFormatter formatter;
        private Collection<InArgument> parameters;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.Result != null)
            {
                RuntimeArgument argument = new RuntimeArgument("Result", this.Result.ArgumentType, ArgumentDirection.In);
                metadata.Bind(this.Result, argument);
                metadata.AddArgument(argument);
            }
            if (this.parameters != null)
            {
                int num = 0;
                foreach (InArgument argument2 in this.parameters)
                {
                    RuntimeArgument argument3 = new RuntimeArgument("Parameter" + num++, argument2.ArgumentType, ArgumentDirection.In);
                    metadata.Bind(argument2, argument3);
                    metadata.AddArgument(argument3);
                }
            }
            RuntimeArgument argument4 = new RuntimeArgument("Message", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.Out, true);
            metadata.Bind(this.Message, argument4);
            metadata.AddArgument(argument4);
            RuntimeArgument argument5 = new RuntimeArgument("CorrelatesWith", System.ServiceModel.Activities.Constants.CorrelationHandleType, ArgumentDirection.In);
            metadata.Bind(this.CorrelatesWith, argument5);
            metadata.AddArgument(argument5);
        }

        protected override void Execute(NativeActivityContext context)
        {
            CorrelationResponseContext context2;
            CorrelationHandle handle = (this.CorrelatesWith == null) ? null : this.CorrelatesWith.Get(context);
            if (handle == null)
            {
                handle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
            }
            if ((handle == null) || !handle.TryAcquireResponseContext(context, out context2))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CorrelationResponseContextShouldNotBeNull));
            }
            if (!handle.TryRegisterResponseContext(context, context2))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ResponseContextIsNotNull));
            }
            MessageVersion messageVersion = context2.MessageVersion;
            if (this.FaultFormatter != null)
            {
                MessageFault fault;
                string defaultFaultAction;
                Exception exception = this.parameters[0].Get(context) as Exception;
                FaultException faultException = exception as FaultException;
                if (faultException != null)
                {
                    fault = this.FaultFormatter.Serialize(faultException, out defaultFaultAction);
                    if (defaultFaultAction == null)
                    {
                        defaultFaultAction = messageVersion.Addressing.DefaultFaultAction;
                    }
                }
                else
                {
                    FaultCode subCode = new FaultCode("InternalServiceFault", "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher");
                    subCode = FaultCode.CreateReceiverFaultCode(subCode);
                    defaultFaultAction = "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault";
                    if (this.IncludeExceptionDetailInFaults)
                    {
                        fault = MessageFault.CreateFault(subCode, new FaultReason(new FaultReasonText(exception.Message, CultureInfo.CurrentCulture)), new ExceptionDetail(exception));
                    }
                    else
                    {
                        fault = MessageFault.CreateFault(subCode, new FaultReason(new FaultReasonText(System.ServiceModel.Activities.SR.InternalServerError, CultureInfo.CurrentCulture)));
                    }
                }
                if (fault == null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CannotCreateMessageFault));
                }
                System.ServiceModel.Channels.Message message = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, fault, defaultFaultAction);
                this.Message.Set(context, message);
            }
            else
            {
                object[] emptyArray;
                if (this.parameters != null)
                {
                    emptyArray = new object[this.parameters.Count];
                    for (int i = 0; i < this.parameters.Count; i++)
                    {
                        emptyArray[i] = this.parameters[i].Get(context);
                    }
                }
                else
                {
                    emptyArray = System.ServiceModel.Activities.Constants.EmptyArray;
                }
                object result = null;
                if (this.Result != null)
                {
                    result = this.Result.Get(context);
                }
                System.ServiceModel.Channels.Message message2 = this.Formatter.SerializeReply(messageVersion, emptyArray, result);
                this.Message.Set(context, message2);
            }
        }

        private void ValidateFormatters()
        {
            if ((this.Formatter == null) && (this.FaultFormatter == null))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.OperationFormatterAndFaultFormatterNotSet));
            }
            if ((this.Formatter != null) && (this.FaultFormatter != null))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.OperationFormatterAndFaultFormatterIncorrectlySet));
            }
        }

        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }

        public IDispatchFaultFormatter FaultFormatter
        {
            get
            {
                return this.faultFormatter;
            }
            set
            {
                this.faultFormatter = value;
                this.ValidateFormatters();
            }
        }

        public IDispatchMessageFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
            set
            {
                this.formatter = value;
                this.ValidateFormatters();
            }
        }

        public bool IncludeExceptionDetailInFaults { get; set; }

        public OutArgument<System.ServiceModel.Channels.Message> Message { get; set; }

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

        public InArgument Result { get; set; }
    }
}

