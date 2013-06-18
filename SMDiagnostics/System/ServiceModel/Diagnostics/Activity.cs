namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime;

    internal class Activity : IDisposable
    {
        private Guid currentId;
        private bool mustDispose;
        protected Guid parentId;

        protected Activity(Guid activityId, Guid parentId)
        {
            this.currentId = activityId;
            this.parentId = parentId;
            this.mustDispose = true;
            DiagnosticTrace.ActivityId = this.currentId;
        }

        internal static Activity CreateActivity(Guid activityId)
        {
            Activity activity = null;
            if (activityId != Guid.Empty)
            {
                Guid parentId = DiagnosticTrace.ActivityId;
                if (activityId != parentId)
                {
                    activity = new Activity(activityId, parentId);
                }
            }
            return activity;
        }

        public virtual void Dispose()
        {
            if (this.mustDispose)
            {
                this.mustDispose = false;
                DiagnosticTrace.ActivityId = this.parentId;
            }
            GC.SuppressFinalize(this);
        }

        protected Guid Id
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentId;
            }
        }
    }
}

