namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    internal class FaultCallbackWrapper : CallbackWrapper
    {
        private static Type[] faultCallbackParameters = new Type[] { typeof(NativeActivityFaultContext), typeof(Exception), typeof(System.Activities.ActivityInstance) };
        private static Type faultCallbackType = typeof(FaultCallback);

        public FaultCallbackWrapper(FaultCallback callback, System.Activities.ActivityInstance owningInstance) : base(callback, owningInstance)
        {
        }

        public System.Activities.Runtime.WorkItem CreateWorkItem(Exception propagatedException, System.Activities.ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource)
        {
            return new FaultWorkItem(this, propagatedException, propagatedFrom, originalExceptionSource);
        }

        public void Invoke(NativeActivityFaultContext faultContext, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            base.EnsureCallback(faultCallbackType, faultCallbackParameters);
            FaultCallback callback = (FaultCallback) base.Callback;
            callback(faultContext, propagatedException, propagatedFrom);
        }

        [DataContract]
        private class FaultWorkItem : ActivityExecutionWorkItem
        {
            [DataMember]
            private FaultCallbackWrapper callbackWrapper;
            [DataMember]
            private ActivityInstanceReference originalExceptionSource;
            [DataMember]
            private Exception propagatedException;
            [DataMember]
            private System.Activities.ActivityInstance propagatedFrom;

            public FaultWorkItem(FaultCallbackWrapper callbackWrapper, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource) : base(callbackWrapper.ActivityInstance)
            {
                this.callbackWrapper = callbackWrapper;
                this.propagatedException = propagatedException;
                this.propagatedFrom = propagatedFrom;
                this.originalExceptionSource = originalExceptionSource;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityFaultContext faultContext = null;
                try
                {
                    faultContext = new NativeActivityFaultContext(base.ActivityInstance, executor, bookmarkManager, this.propagatedException, this.originalExceptionSource);
                    this.callbackWrapper.Invoke(faultContext, this.propagatedException, this.propagatedFrom);
                    if (!faultContext.IsFaultHandled)
                    {
                        base.SetExceptionToPropagateWithoutAbort(this.propagatedException);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ExceptionToPropagate = exception;
                }
                finally
                {
                    if (faultContext != null)
                    {
                        faultContext.Dispose();
                    }
                }
                return true;
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteFaultWorkItemIsEnabled())
                {
                    TD.CompleteFaultWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleFaultWorkItemIsEnabled())
                {
                    TD.ScheduleFaultWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartFaultWorkItemIsEnabled())
                {
                    TD.StartFaultWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override System.Activities.ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.originalExceptionSource.ActivityInstance;
                }
            }
        }
    }
}

