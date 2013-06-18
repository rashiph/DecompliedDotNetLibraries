namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CompletionBookmark
    {
        [DataMember(EmitDefaultValue=false)]
        private CompletionCallbackWrapper callbackWrapper;

        public CompletionBookmark()
        {
        }

        public CompletionBookmark(CompletionCallbackWrapper callbackWrapper)
        {
            this.callbackWrapper = callbackWrapper;
        }

        public void CheckForCancelation()
        {
            this.callbackWrapper.CheckForCancelation();
        }

        public System.Activities.Runtime.WorkItem GenerateWorkItem(System.Activities.ActivityInstance completedInstance, ActivityExecutor executor)
        {
            if (this.callbackWrapper != null)
            {
                return this.callbackWrapper.CreateWorkItem(completedInstance, executor);
            }
            if ((completedInstance.State != ActivityInstanceState.Closed) && completedInstance.Parent.HasNotExecuted)
            {
                completedInstance.Parent.SetInitializationIncomplete();
            }
            return new EmptyWithCancelationCheckWorkItem(completedInstance.Parent, completedInstance);
        }
    }
}

