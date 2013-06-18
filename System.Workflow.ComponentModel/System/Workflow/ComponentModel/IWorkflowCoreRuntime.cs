namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal interface IWorkflowCoreRuntime : IServiceProvider
    {
        void ActivityStatusChanged(Activity activity, bool transacted, bool committed);
        void CheckpointInstanceState(Activity currentActivity);
        void DisposeCheckpointState();
        Activity GetContextActivityForId(int id);
        int GetNewContextActivityId();
        object GetService(Activity currentActivity, Type serviceType);
        Activity LoadContextActivity(ActivityExecutionContextInfo contextInfo, Activity outerContextActivity);
        void OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes);
        bool OnBeforeDynamicChange(IList<WorkflowChangeAction> changes);
        void PersistInstanceState(Activity activity);
        void RaiseActivityExecuting(Activity activity);
        void RaiseException(Exception e, Activity activity, string responsibleActivity);
        void RaiseHandlerInvoked();
        void RaiseHandlerInvoking(Delegate delegateHandler);
        void RegisterContextActivity(Activity activity);
        void RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendReason);
        bool Resume();
        void SaveContextActivity(Activity contextActivity);
        void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction);
        IDisposable SetCurrentActivity(Activity activity);
        Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues);
        bool SuspendInstance(string suspendDescription);
        void TerminateInstance(Exception e);
        void Track(string key, object data);
        void UnregisterContextActivity(Activity activity);

        Activity CurrentActivity { get; }

        Activity CurrentAtomicActivity { get; }

        Guid InstanceID { get; }

        bool IsDynamicallyUpdated { get; }

        WaitCallback ProcessTimersCallback { get; }

        Activity RootActivity { get; }
    }
}

