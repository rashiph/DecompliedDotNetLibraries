namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal abstract class WorkItem
    {
        [DataMember]
        private System.Activities.ActivityInstance activityInstance;
        private static AsyncCallback associateCallback;
        private Exception exceptionToPropagate;
        private static AsyncCallback trackingCallback;
        protected Exception workflowAbortException;

        protected WorkItem()
        {
        }

        protected WorkItem(System.Activities.ActivityInstance activityInstance)
        {
            this.activityInstance = activityInstance;
            this.activityInstance.IncrementBusyCount();
        }

        protected virtual void ClearForReuse()
        {
            this.exceptionToPropagate = null;
            this.workflowAbortException = null;
            this.activityInstance = null;
        }

        public void Dispose(ActivityExecutor executor)
        {
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                this.TraceCompleted();
            }
            if (this.IsPooled)
            {
                this.ReleaseToPool(executor);
            }
        }

        public void ExceptionPropagated()
        {
            this.exceptionToPropagate = null;
        }

        public abstract bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager);
        public bool FlushBookmarkScopeKeys(ActivityExecutor executor)
        {
            try
            {
                ICollection<InstanceKey> keysToDisassociate = executor.BookmarkScopeManager.GetKeysToDisassociate();
                if ((keysToDisassociate != null) && (keysToDisassociate.Count > 0))
                {
                    executor.DisassociateKeys(keysToDisassociate);
                }
                ICollection<InstanceKey> keysToAssociate = executor.BookmarkScopeManager.GetKeysToAssociate();
                if ((keysToAssociate != null) && (keysToAssociate.Count > 0))
                {
                    if (associateCallback == null)
                    {
                        associateCallback = Fx.ThunkCallback(new AsyncCallback(System.Activities.Runtime.WorkItem.OnAssociateComplete));
                    }
                    IAsyncResult result = executor.BeginAssociateKeys(keysToAssociate, associateCallback, new CallbackData(executor, this));
                    if (result.CompletedSynchronously)
                    {
                        executor.EndAssociateKeys(result);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.workflowAbortException = exception;
            }
            return true;
        }

        public bool FlushTracking(ActivityExecutor executor)
        {
            try
            {
                if (trackingCallback == null)
                {
                    trackingCallback = Fx.ThunkCallback(new AsyncCallback(System.Activities.Runtime.WorkItem.OnTrackingComplete));
                }
                IAsyncResult result = executor.BeginTrackPendingRecords(trackingCallback, new CallbackData(executor, this));
                if (result.CompletedSynchronously)
                {
                    executor.EndTrackPendingRecords(result);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.workflowAbortException = exception;
            }
            return true;
        }

        private static void OnAssociateComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackData asyncState = (CallbackData) result.AsyncState;
                try
                {
                    asyncState.Executor.EndAssociateKeys(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.WorkItem.workflowAbortException = exception;
                }
                asyncState.Executor.FinishWorkItem(asyncState.WorkItem);
            }
        }

        private static void OnTrackingComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackData asyncState = (CallbackData) result.AsyncState;
                try
                {
                    asyncState.Executor.EndTrackPendingRecords(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.WorkItem.workflowAbortException = exception;
                }
                asyncState.Executor.FinishWorkItemAfterTracking(asyncState.WorkItem);
            }
        }

        public abstract void PostProcess(ActivityExecutor executor);
        protected virtual void Reinitialize(System.Activities.ActivityInstance activityInstance)
        {
            this.activityInstance = activityInstance;
            this.activityInstance.IncrementBusyCount();
        }

        public void Release(ActivityExecutor executor)
        {
            this.activityInstance.DecrementBusyCount();
            if (this.ExitNoPersistRequired)
            {
                executor.ExitNoPersist();
            }
        }

        protected virtual void ReleaseToPool(ActivityExecutor executor)
        {
        }

        public abstract void TraceCompleted();
        protected void TraceRuntimeWorkItemCompleted()
        {
            if (TD.CompleteRuntimeWorkItemIsEnabled())
            {
                TD.CompleteRuntimeWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
            }
        }

        protected void TraceRuntimeWorkItemScheduled()
        {
            if (TD.ScheduleRuntimeWorkItemIsEnabled())
            {
                TD.ScheduleRuntimeWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
            }
        }

        protected void TraceRuntimeWorkItemStarting()
        {
            if (TD.StartRuntimeWorkItemIsEnabled())
            {
                TD.StartRuntimeWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
            }
        }

        public abstract void TraceScheduled();
        public abstract void TraceStarting();

        public System.Activities.ActivityInstance ActivityInstance
        {
            get
            {
                return this.activityInstance;
            }
        }

        public Exception ExceptionToPropagate
        {
            get
            {
                return this.exceptionToPropagate;
            }
            set
            {
                this.exceptionToPropagate = value;
            }
        }

        public bool ExitNoPersistRequired { get; protected set; }

        [DataMember(EmitDefaultValue=false)]
        public bool IsEmpty { get; protected set; }

        protected bool IsPooled { get; set; }

        public abstract bool IsValid { get; }

        public virtual System.Activities.ActivityInstance OriginalExceptionSource
        {
            get
            {
                return this.ActivityInstance;
            }
        }

        public abstract System.Activities.ActivityInstance PropertyManagerOwner { get; }

        public Exception WorkflowAbortException
        {
            get
            {
                return this.workflowAbortException;
            }
        }

        private class CallbackData
        {
            public CallbackData(ActivityExecutor executor, System.Activities.Runtime.WorkItem workItem)
            {
                this.Executor = executor;
                this.WorkItem = workItem;
            }

            public ActivityExecutor Executor { get; private set; }

            public System.Activities.Runtime.WorkItem WorkItem { get; private set; }
        }
    }
}

