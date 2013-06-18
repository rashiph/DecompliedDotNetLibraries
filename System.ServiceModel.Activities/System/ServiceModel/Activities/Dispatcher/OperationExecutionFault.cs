namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class OperationExecutionFault : MessageFault
    {
        private FaultCode faultCode;
        private FaultReason faultReason;
        private static FaultCode instanceAbortedCode;
        private static FaultCode instanceCompletedCode;
        private static FaultCode instanceLockedFaultCode;
        private static FaultCode instanceNotFoundCode;
        private static FaultCode instanceSuspendedFaultCode;
        private static FaultCode instanceTerminatedCode;
        private static FaultCode instanceUnloadedFaultCode;
        private static FaultCode operationNotAvailableFaultCode;

        private OperationExecutionFault(string description, FaultCode subcode)
        {
            this.faultCode = FaultCode.CreateSenderFaultCode(subcode);
            this.faultReason = new FaultReason(new FaultReasonText(description, CultureInfo.CurrentCulture));
        }

        public static OperationExecutionFault CreateAbortedFault(string description)
        {
            if (instanceAbortedCode == null)
            {
                instanceAbortedCode = new FaultCode("InstanceAborted", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(description, instanceAbortedCode);
        }

        public static OperationExecutionFault CreateCompletedFault(string description)
        {
            if (instanceCompletedCode == null)
            {
                instanceCompletedCode = new FaultCode("InstanceCompleted", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(description, instanceCompletedCode);
        }

        public static OperationExecutionFault CreateInstanceNotFoundFault(string description)
        {
            if (instanceNotFoundCode == null)
            {
                instanceNotFoundCode = new FaultCode("InstanceNotFound", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(description, instanceNotFoundCode);
        }

        public static OperationExecutionFault CreateInstanceUnloadedFault(string description)
        {
            if (instanceUnloadedFaultCode == null)
            {
                instanceUnloadedFaultCode = new FaultCode("InstanceUnloaded", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(description, instanceUnloadedFaultCode);
        }

        public static OperationExecutionFault CreateOperationNotAvailableFault(Guid instanceId, string operationName)
        {
            if (operationNotAvailableFaultCode == null)
            {
                operationNotAvailableFaultCode = new FaultCode("OperationNotAvailable", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(System.ServiceModel.Activities.SR.OperationNotAvailable(operationName, instanceId), operationNotAvailableFaultCode);
        }

        public static OperationExecutionFault CreateSuspendedFault(Guid instanceId, string operationName)
        {
            if (instanceSuspendedFaultCode == null)
            {
                instanceSuspendedFaultCode = new FaultCode("InstanceSuspended", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(System.ServiceModel.Activities.SR.InstanceSuspended(operationName, instanceId), instanceSuspendedFaultCode);
        }

        public static OperationExecutionFault CreateTerminatedFault(string description)
        {
            if (instanceTerminatedCode == null)
            {
                instanceTerminatedCode = new FaultCode("InstanceTerminated", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(description, instanceTerminatedCode);
        }

        public static OperationExecutionFault CreateTransactedLockException(Guid instanceId, string operationName)
        {
            if (instanceLockedFaultCode == null)
            {
                instanceLockedFaultCode = new FaultCode("InstanceLockedUnderTransaction", "http://schemas.datacontract.org/2008/10/WorkflowServices");
            }
            return new OperationExecutionFault(System.ServiceModel.Activities.SR.InstanceLockedUnderTransaction(operationName, instanceId), instanceLockedFaultCode);
        }

        public static bool IsAbortedFaultException(FaultException exception)
        {
            return (((exception.Code != null) && (exception.Code.SubCode != null)) && ((exception.Code.SubCode.Name == instanceAbortedCode.Name) && (exception.Code.SubCode.Namespace == instanceAbortedCode.Namespace)));
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public override FaultCode Code
        {
            get
            {
                return this.faultCode;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return false;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.faultReason;
            }
        }
    }
}

