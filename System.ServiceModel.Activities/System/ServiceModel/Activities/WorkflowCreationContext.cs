namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class WorkflowCreationContext
    {
        private Dictionary<string, object> workflowArguments;

        protected internal virtual void OnAbort()
        {
        }

        protected internal virtual IAsyncResult OnBeginWorkflowCompleted(ActivityInstanceState completionState, IDictionary<string, object> workflowOutputs, Exception terminationException, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected internal virtual void OnEndWorkflowCompleted(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        [DataMember]
        public bool CreateOnly { get; set; }

        [DataMember]
        public bool IsCompletionTransactionRequired { get; set; }

        internal IDictionary<string, object> RawWorkflowArguments
        {
            get
            {
                return this.workflowArguments;
            }
        }

        public IDictionary<string, object> WorkflowArguments
        {
            get
            {
                if (this.workflowArguments == null)
                {
                    this.workflowArguments = new Dictionary<string, object>();
                }
                return this.workflowArguments;
            }
        }
    }
}

