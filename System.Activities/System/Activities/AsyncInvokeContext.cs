namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class AsyncInvokeContext
    {
        public AsyncInvokeContext(object userState, WorkflowInvoker invoker)
        {
            this.UserState = userState;
            SynchronizationContext syncContext = SynchronizationContext.Current ?? System.Activities.WorkflowApplication.SynchronousSynchronizationContext.Value;
            this.Operation = new AsyncInvokeOperation(syncContext);
            this.Invoker = invoker;
        }

        public WorkflowInvoker Invoker { get; private set; }

        public AsyncInvokeOperation Operation { get; private set; }

        public IDictionary<string, object> Outputs { get; set; }

        public object UserState { get; private set; }

        public System.Activities.WorkflowApplication WorkflowApplication { get; set; }
    }
}

