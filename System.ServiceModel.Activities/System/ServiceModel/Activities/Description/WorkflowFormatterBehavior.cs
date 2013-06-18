namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class WorkflowFormatterBehavior : IOperationBehavior
    {
        private IDispatchFaultFormatter faultFormatter;
        private IDispatchMessageFormatter formatter;
        private Collection<Receive> receives;

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (dispatchOperation.Formatter != null)
            {
                this.formatter = dispatchOperation.Formatter;
                this.faultFormatter = dispatchOperation.FaultFormatter;
                if (this.receives != null)
                {
                    foreach (Receive receive in this.receives)
                    {
                        receive.SetFormatter(this.formatter, this.faultFormatter, dispatchOperation.IncludeExceptionDetailInFaults);
                    }
                }
                dispatchOperation.Formatter = null;
                dispatchOperation.DeserializeRequest = false;
                dispatchOperation.SerializeReply = false;
            }
        }

        public void Validate(OperationDescription operationDescription)
        {
        }

        public Collection<Receive> Receives
        {
            get
            {
                if (this.receives == null)
                {
                    this.receives = new Collection<Receive>();
                }
                return this.receives;
            }
        }
    }
}

