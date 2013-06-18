namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [DataContract(Name="Scheduler", Namespace="http://schemas.datacontract.org/2010/02/System.Activities")]
    internal class Scheduler
    {
        private static AbortAction abortAction = new AbortAction();
        private Callbacks callbacks;
        private static ContinueAction continueAction = new ContinueAction();
        [DataMember(EmitDefaultValue=false)]
        private System.Activities.Runtime.WorkItem firstWorkItem;
        private bool isPausing;
        private bool isRunning;
        private static SendOrPostCallback onScheduledWorkCallback = Fx.ThunkCallback(new SendOrPostCallback(Scheduler.OnScheduledWork));
        private bool resumeTraceRequired;
        private SynchronizationContext synchronizationContext;
        private Quack<System.Activities.Runtime.WorkItem> workItemQueue;
        private static YieldSilentlyAction yieldSilentlyAction = new YieldSilentlyAction();

        public Scheduler(Callbacks callbacks)
        {
            this.Initialize(callbacks);
        }

        public void ClearAllWorkItems(ActivityExecutor executor)
        {
            if (this.firstWorkItem != null)
            {
                this.firstWorkItem.Release(executor);
                this.firstWorkItem = null;
                if (this.workItemQueue != null)
                {
                    while (this.workItemQueue.Count > 0)
                    {
                        this.workItemQueue.Dequeue().Release(executor);
                    }
                }
            }
            this.workItemQueue = null;
        }

        public static RequestedAction CreateNotifyUnhandledExceptionAction(Exception exception, System.Activities.ActivityInstance sourceInstance)
        {
            return new NotifyUnhandledExceptionAction(exception, sourceInstance);
        }

        public void EnqueueWork(System.Activities.Runtime.WorkItem workItem)
        {
            if (this.firstWorkItem == null)
            {
                this.firstWorkItem = workItem;
            }
            else
            {
                if (this.workItemQueue == null)
                {
                    this.workItemQueue = new Quack<System.Activities.Runtime.WorkItem>();
                }
                this.workItemQueue.Enqueue(workItem);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                workItem.TraceScheduled();
            }
        }

        public void FillInstanceMap(ActivityInstanceMap instanceMap)
        {
            if (this.firstWorkItem != null)
            {
                ActivityInstanceMap.IActivityReference firstWorkItem = this.firstWorkItem as ActivityInstanceMap.IActivityReference;
                if (firstWorkItem != null)
                {
                    instanceMap.AddEntry(firstWorkItem, true);
                }
                if ((this.workItemQueue != null) && (this.workItemQueue.Count > 0))
                {
                    for (int i = 0; i < this.workItemQueue.Count; i++)
                    {
                        firstWorkItem = this.workItemQueue[i] as ActivityInstanceMap.IActivityReference;
                        if (firstWorkItem != null)
                        {
                            instanceMap.AddEntry(firstWorkItem, true);
                        }
                    }
                }
            }
        }

        private void Initialize(Callbacks callbacks)
        {
            this.callbacks = callbacks;
        }

        public void InternalResume(RequestedAction action)
        {
            bool shouldTraceInformation = FxTrace.ShouldTraceInformation;
            bool flag2 = false;
            bool isCompleted = false;
            if (this.callbacks.IsAbortPending)
            {
                this.isPausing = false;
                this.isRunning = false;
                this.NotifyWorkCompletion();
                flag2 = true;
                if (shouldTraceInformation)
                {
                    isCompleted = this.callbacks.IsCompleted;
                }
                this.callbacks.SchedulerIdle();
            }
            else if (object.ReferenceEquals(action, continueAction))
            {
                this.ScheduleWork(false);
            }
            else
            {
                NotifyUnhandledExceptionAction action2 = (NotifyUnhandledExceptionAction) action;
                this.isRunning = false;
                this.NotifyWorkCompletion();
                flag2 = true;
                if (shouldTraceInformation)
                {
                    isCompleted = this.callbacks.IsCompleted;
                }
                this.callbacks.NotifyUnhandledException(action2.Exception, action2.Source);
            }
            if (shouldTraceInformation && flag2)
            {
                Guid empty = Guid.Empty;
                bool flag4 = false;
                if (isCompleted)
                {
                    if (TD.WorkflowActivityStopIsEnabled())
                    {
                        empty = DiagnosticTrace.ActivityId;
                        DiagnosticTrace.ActivityId = this.callbacks.WorkflowInstanceId;
                        flag4 = true;
                        TD.WorkflowActivityStop(this.callbacks.WorkflowInstanceId.ToString());
                    }
                }
                else if (TD.WorkflowActivitySuspendIsEnabled())
                {
                    empty = DiagnosticTrace.ActivityId;
                    DiagnosticTrace.ActivityId = this.callbacks.WorkflowInstanceId;
                    flag4 = true;
                    TD.WorkflowActivitySuspend(this.callbacks.WorkflowInstanceId.ToString());
                }
                if (flag4)
                {
                    DiagnosticTrace.ActivityId = empty;
                }
            }
        }

        public void MarkRunning()
        {
            this.isRunning = true;
        }

        private void NotifyWorkCompletion()
        {
            this.synchronizationContext.OperationCompleted();
        }

        public void OnDeserialized(Callbacks callbacks)
        {
            this.Initialize(callbacks);
        }

        private static void OnScheduledWork(object state)
        {
            Scheduler scheduler = (Scheduler) state;
            bool flag = FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational);
            Guid empty = Guid.Empty;
            Guid newId = Guid.Empty;
            if (flag)
            {
                empty = DiagnosticTrace.ActivityId;
                newId = scheduler.callbacks.WorkflowInstanceId;
                FxTrace.Trace.SetAndTraceTransfer(newId, true);
                if (scheduler.resumeTraceRequired && TD.WorkflowActivityResumeIsEnabled())
                {
                    TD.WorkflowActivityResume(newId.ToString());
                }
            }
            scheduler.callbacks.ThreadAcquired();
            RequestedAction continueAction = Scheduler.continueAction;
            bool flag2 = false;
            while (object.ReferenceEquals(continueAction, Scheduler.continueAction))
            {
                if (scheduler.IsIdle || scheduler.isPausing)
                {
                    flag2 = true;
                    break;
                }
                System.Activities.Runtime.WorkItem firstWorkItem = scheduler.firstWorkItem;
                if ((scheduler.workItemQueue != null) && (scheduler.workItemQueue.Count > 0))
                {
                    scheduler.firstWorkItem = scheduler.workItemQueue.Dequeue();
                }
                else
                {
                    scheduler.firstWorkItem = null;
                }
                continueAction = scheduler.callbacks.ExecuteWorkItem(firstWorkItem);
            }
            bool flag3 = false;
            bool isCompleted = false;
            if (flag2 || object.ReferenceEquals(continueAction, abortAction))
            {
                scheduler.isPausing = false;
                scheduler.isRunning = false;
                scheduler.NotifyWorkCompletion();
                flag3 = true;
                if (flag)
                {
                    isCompleted = scheduler.callbacks.IsCompleted;
                }
                scheduler.callbacks.SchedulerIdle();
            }
            else if (!object.ReferenceEquals(continueAction, yieldSilentlyAction))
            {
                NotifyUnhandledExceptionAction action2 = (NotifyUnhandledExceptionAction) continueAction;
                scheduler.isRunning = false;
                scheduler.NotifyWorkCompletion();
                flag3 = true;
                if (flag)
                {
                    isCompleted = scheduler.callbacks.IsCompleted;
                }
                scheduler.callbacks.NotifyUnhandledException(action2.Exception, action2.Source);
            }
            if (flag)
            {
                if (flag3)
                {
                    if (isCompleted)
                    {
                        if (TD.WorkflowActivityStopIsEnabled())
                        {
                            TD.WorkflowActivityStop(newId.ToString());
                        }
                    }
                    else if (TD.WorkflowActivitySuspendIsEnabled())
                    {
                        TD.WorkflowActivitySuspend(newId.ToString());
                    }
                }
                DiagnosticTrace.ActivityId = empty;
            }
        }

        internal void Open(Scheduler oldScheduler)
        {
            this.synchronizationContext = SynchronizationContextHelper.CloneSynchronizationContext(oldScheduler.synchronizationContext);
        }

        public void Open(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext != null)
            {
                this.synchronizationContext = synchronizationContext;
            }
            else
            {
                this.synchronizationContext = SynchronizationContextHelper.GetDefaultSynchronizationContext();
            }
        }

        public void Pause()
        {
            this.isPausing = true;
        }

        public void PushWork(System.Activities.Runtime.WorkItem workItem)
        {
            if (this.firstWorkItem == null)
            {
                this.firstWorkItem = workItem;
            }
            else
            {
                if (this.workItemQueue == null)
                {
                    this.workItemQueue = new Quack<System.Activities.Runtime.WorkItem>();
                }
                this.workItemQueue.PushFront(this.firstWorkItem);
                this.firstWorkItem = workItem;
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                workItem.TraceScheduled();
            }
        }

        public void Resume()
        {
            if ((this.IsIdle || this.isPausing) || this.callbacks.IsAbortPending)
            {
                this.isPausing = false;
                this.isRunning = false;
                this.callbacks.SchedulerIdle();
            }
            else
            {
                this.ScheduleWork(true);
            }
        }

        private void ScheduleWork(bool notifyStart)
        {
            if (notifyStart)
            {
                this.synchronizationContext.OperationStarted();
                this.resumeTraceRequired = true;
            }
            else
            {
                this.resumeTraceRequired = false;
            }
            this.synchronizationContext.Post(onScheduledWorkCallback, this);
        }

        public static RequestedAction Abort
        {
            get
            {
                return abortAction;
            }
        }

        public static RequestedAction Continue
        {
            get
            {
                return continueAction;
            }
        }

        public bool IsIdle
        {
            get
            {
                return (this.firstWorkItem == null);
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        private System.Activities.Runtime.WorkItem[] SerializedWorkItemQueue
        {
            get
            {
                if ((this.workItemQueue != null) && (this.workItemQueue.Count > 0))
                {
                    return this.workItemQueue.ToArray();
                }
                return null;
            }
            set
            {
                this.workItemQueue = new Quack<System.Activities.Runtime.WorkItem>(value);
            }
        }

        public static RequestedAction YieldSilently
        {
            get
            {
                return yieldSilentlyAction;
            }
        }

        private class AbortAction : Scheduler.RequestedAction
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Callbacks
        {
            private readonly ActivityExecutor activityExecutor;
            public Callbacks(ActivityExecutor activityExecutor)
            {
                this.activityExecutor = activityExecutor;
            }

            public Guid WorkflowInstanceId
            {
                get
                {
                    return this.activityExecutor.WorkflowInstanceId;
                }
            }
            public bool IsAbortPending
            {
                get
                {
                    if (!this.activityExecutor.IsAbortPending)
                    {
                        return this.activityExecutor.IsTerminatePending;
                    }
                    return true;
                }
            }
            public bool IsCompleted
            {
                get
                {
                    return ActivityUtilities.IsCompletedState(this.activityExecutor.State);
                }
            }
            public Scheduler.RequestedAction ExecuteWorkItem(System.Activities.Runtime.WorkItem workItem)
            {
                if (FxTrace.ShouldTraceVerboseToTraceSource)
                {
                    workItem.TraceStarting();
                }
                Scheduler.RequestedAction objA = this.activityExecutor.OnExecuteWorkItem(workItem);
                if (!object.ReferenceEquals(objA, Scheduler.YieldSilently))
                {
                    if (this.activityExecutor.IsAbortPending || this.activityExecutor.IsTerminatePending)
                    {
                        objA = Scheduler.Abort;
                    }
                    workItem.Dispose(this.activityExecutor);
                }
                return objA;
            }

            public void SchedulerIdle()
            {
                this.activityExecutor.OnSchedulerIdle();
            }

            public void ThreadAcquired()
            {
                this.activityExecutor.OnSchedulerThreadAcquired();
            }

            public void NotifyUnhandledException(Exception exception, System.Activities.ActivityInstance source)
            {
                this.activityExecutor.NotifyUnhandledException(exception, source);
            }
        }

        private class ContinueAction : Scheduler.RequestedAction
        {
        }

        private class NotifyUnhandledExceptionAction : Scheduler.RequestedAction
        {
            public NotifyUnhandledExceptionAction(System.Exception exception, System.Activities.ActivityInstance source)
            {
                this.Exception = exception;
                this.Source = source;
            }

            public System.Exception Exception { get; private set; }

            public System.Activities.ActivityInstance Source { get; private set; }
        }

        internal abstract class RequestedAction
        {
            protected RequestedAction()
            {
            }
        }

        private class YieldSilentlyAction : Scheduler.RequestedAction
        {
        }
    }
}

