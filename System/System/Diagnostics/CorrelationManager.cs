namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Messaging;

    public class CorrelationManager
    {
        private const string activityIdSlotName = "E2ETrace.ActivityID";
        private const string transactionSlotName = "System.Diagnostics.Trace.CorrelationManagerSlot";

        internal CorrelationManager()
        {
        }

        private Stack GetLogicalOperationStack()
        {
            Stack data = CallContext.LogicalGetData("System.Diagnostics.Trace.CorrelationManagerSlot") as Stack;
            if (data == null)
            {
                data = new Stack();
                CallContext.LogicalSetData("System.Diagnostics.Trace.CorrelationManagerSlot", data);
            }
            return data;
        }

        public void StartLogicalOperation()
        {
            this.StartLogicalOperation(Guid.NewGuid());
        }

        public void StartLogicalOperation(object operationId)
        {
            if (operationId == null)
            {
                throw new ArgumentNullException("operationId");
            }
            this.GetLogicalOperationStack().Push(operationId);
        }

        public void StopLogicalOperation()
        {
            this.GetLogicalOperationStack().Pop();
        }

        public Guid ActivityId
        {
            get
            {
                object obj2 = CallContext.LogicalGetData("E2ETrace.ActivityID");
                if (obj2 != null)
                {
                    return (Guid) obj2;
                }
                return Guid.Empty;
            }
            set
            {
                CallContext.LogicalSetData("E2ETrace.ActivityID", value);
            }
        }

        public Stack LogicalOperationStack
        {
            get
            {
                return this.GetLogicalOperationStack();
            }
        }
    }
}

