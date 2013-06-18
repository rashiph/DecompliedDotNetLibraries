namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal abstract class ActivityExecutionWorkItem : System.Activities.Runtime.WorkItem
    {
        private bool skipActivityInstanceAbort;

        protected ActivityExecutionWorkItem()
        {
        }

        public ActivityExecutionWorkItem(System.Activities.ActivityInstance activityInstance) : base(activityInstance)
        {
        }

        protected override void ClearForReuse()
        {
            base.ClearForReuse();
            this.skipActivityInstanceAbort = false;
        }

        public override void PostProcess(ActivityExecutor executor)
        {
            if ((base.ExceptionToPropagate != null) && !this.skipActivityInstanceAbort)
            {
                executor.AbortActivityInstance(base.ActivityInstance, base.ExceptionToPropagate);
            }
            else if (base.ActivityInstance.UpdateState(executor))
            {
                Exception exception = executor.CompleteActivityInstance(base.ActivityInstance);
                if (exception != null)
                {
                    base.ExceptionToPropagate = exception;
                }
            }
        }

        protected void SetExceptionToPropagateWithoutAbort(Exception exception)
        {
            base.ExceptionToPropagate = exception;
            this.skipActivityInstanceAbort = true;
        }

        public override bool IsValid
        {
            get
            {
                return (base.ActivityInstance.State == ActivityInstanceState.Executing);
            }
        }

        public override System.Activities.ActivityInstance PropertyManagerOwner
        {
            get
            {
                return base.ActivityInstance;
            }
        }
    }
}

