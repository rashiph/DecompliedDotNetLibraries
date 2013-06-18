namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.DebugEngine;
    using System.Workflow.Runtime.Hosting;

    internal sealed class WorkflowExecutor : IWorkflowCoreRuntime, IServiceProvider, ISupportInterop
    {
        private InstanceLock _executorLock;
        private bool _isInstanceValid = false;
        private Activity _lastExecutingActivity;
        private InstanceLock _msgDeliveryLock;
        private VolatileResourceManager _resourceManager = new VolatileResourceManager();
        private System.Workflow.Runtime.WorkflowRuntime _runtime;
        private InstanceLock _schedulerLock;
        private TimerEventSubscriptionCollection _timerQueue;
        private WorkflowDebuggerService _workflowDebuggerService;
        private volatile Activity _workflowDefinition;
        private System.Workflow.Runtime.WorkflowInstance _workflowInstance;
        private string activityThrowingException;
        private ManualResetEvent atomicActivityEvent;
        private bool attemptedRootAECUnload;
        private bool attemptedRootDispose;
        private Hashtable completedContextActivities = new Hashtable();
        private static DependencyProperty ContextIdProperty = DependencyProperty.RegisterAttached("ContextId", typeof(int), typeof(WorkflowExecutor), new PropertyMetadata(0));
        internal Activity currentAtomicActivity;
        private static BooleanSwitch disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");
        private List<SchedulerLockGuardInfo> eventsToFireList = new List<SchedulerLockGuardInfo>();
        internal static readonly DependencyProperty IsBlockedProperty = DependencyProperty.RegisterAttached("IsBlocked", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));
        private static DependencyProperty IsIdleProperty = DependencyProperty.RegisterAttached("IsIdle", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));
        private bool isInstanceIdle;
        private static DependencyProperty IsSuspensionRequestedProperty = DependencyProperty.RegisterAttached("IsSuspensionRequested", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));
        private WorkflowQueuingService qService;
        private Activity rootActivity;
        private System.Workflow.Runtime.Scheduler schedulingContext;
        internal bool stateChangedSincePersistence;
        [NonSerialized]
        private Dictionary<int, Activity> subStateMap = new Dictionary<int, Activity>();
        internal static readonly DependencyProperty SuspendOrTerminateInfoProperty = DependencyProperty.RegisterAttached("SuspendOrTerminateInfo", typeof(string), typeof(WorkflowExecutor));
        private Exception thrownException;
        private static DependencyProperty TrackingCallingStateProperty = DependencyProperty.RegisterAttached("TrackingCallingState", typeof(System.Workflow.Runtime.TrackingCallingState), typeof(WorkflowExecutor));
        internal static DependencyProperty TrackingListenerBrokerProperty = DependencyProperty.RegisterAttached("TrackingListenerBroker", typeof(TrackingListenerBroker), typeof(WorkflowExecutor));
        internal static readonly DependencyProperty TransactionalPropertiesProperty = DependencyProperty.RegisterAttached("TransactionalProperties", typeof(TransactionalProperties), typeof(WorkflowExecutor), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static readonly DependencyProperty TransientBatchProperty = DependencyProperty.RegisterAttached("TransientBatch", typeof(IWorkBatch), typeof(WorkflowExecutor), new PropertyMetadata(null, DependencyPropertyOptions.NonSerialized, new GetValueOverride(WorkflowExecutor.GetTransientBatch), null));
        private static bool workflowDebuggingDisabled;
        internal static readonly DependencyProperty WorkflowExecutorProperty = DependencyProperty.RegisterAttached("WorkflowExecutor", typeof(IWorkflowCoreRuntime), typeof(WorkflowExecutor), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        private string workflowIdString;
        private Guid workflowInstanceId;
        internal static readonly DependencyProperty WorkflowInstanceIdProperty = DependencyProperty.RegisterAttached("WorkflowInstanceId", typeof(Guid), typeof(WorkflowExecutor), new PropertyMetadata(Guid.NewGuid()));
        private System.Workflow.Runtime.WorkflowStateRollbackService workflowStateRollbackService;
        internal static readonly DependencyProperty WorkflowStatusProperty = DependencyProperty.RegisterAttached("WorkflowStatus", typeof(System.Workflow.Runtime.WorkflowStatus), typeof(WorkflowExecutor), new PropertyMetadata(System.Workflow.Runtime.WorkflowStatus.Created));

        private event EventHandler<WorkflowExecutionEventArgs> _workflowExecutionEvent;

        internal event EventHandler<WorkflowExecutionEventArgs> WorkflowExecutionEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] add
            {
                this._workflowExecutionEvent += value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] remove
            {
                this._workflowExecutionEvent -= value;
            }
        }

        static WorkflowExecutor()
        {
            DependencyProperty.RegisterAsKnown(ContextIdProperty, 0x33, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(IsSuspensionRequestedProperty, 0x34, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(TrackingCallingStateProperty, 0x35, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(TrackingListenerBrokerProperty, 0x36, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(IsIdleProperty, 0x38, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(System.Workflow.Runtime.Scheduler.NormalPriorityEntriesQueueProperty, 0x3d, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(System.Workflow.Runtime.Scheduler.HighPriorityEntriesQueueProperty, 0x3e, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(WorkflowQueuingService.LocalPersistedQueueStatesProperty, 0x3f, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowQueuingService.RootPersistedQueueStatesProperty, 0x40, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(CorrelationTokenCollection.CorrelationTokenCollectionProperty, 0x41, DependencyProperty.PropertyValidity.Always);
            DependencyProperty.RegisterAsKnown(CorrelationToken.NameProperty, 0x43, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.OwnerActivityNameProperty, 0x44, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.PropertiesProperty, 0x45, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.SubscriptionsProperty, 70, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.InitializedProperty, 0x47, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty, 80, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowInstanceIdProperty, 0x66, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(IsBlockedProperty, 0x67, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowStatusProperty, 0x68, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(SuspendOrTerminateInfoProperty, 0x69, DependencyProperty.PropertyValidity.Reexecute);
            workflowDebuggingDisabled = disableWorkflowDebugging.Enabled;
        }

        internal WorkflowExecutor(Guid instanceId)
        {
            this._executorLock = LockFactory.CreateWorkflowExecutorLock(instanceId);
            this._msgDeliveryLock = LockFactory.CreateWorkflowMessageDeliveryLock(instanceId);
            this.stateChangedSincePersistence = true;
            if (!workflowDebuggingDisabled)
            {
                this._workflowDebuggerService = new WorkflowDebuggerService(this);
            }
        }

        private void _setInArgsOnCompanion(IDictionary<string, object> namedInArguments)
        {
            if (namedInArguments != null)
            {
                foreach (string str in namedInArguments.Keys)
                {
                    PropertyInfo property = this.WorkflowDefinition.GetType().GetProperty(str);
                    if ((property != null) && property.CanWrite)
                    {
                        try
                        {
                            property.SetValue(this.rootActivity, namedInArguments[str], null);
                            continue;
                        }
                        catch (ArgumentException exception)
                        {
                            throw new ArgumentException(ExecutionStringManager.InvalidWorkflowParameterValue, str, exception);
                        }
                    }
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.SemanticErrorInvalidNamedParameter, new object[] { this.WorkflowDefinition.Name, str }));
                }
            }
        }

        internal void Abort()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor::Abort : Got a abort request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                using (this._executorLock.Enter())
                {
                    this.Scheduler.AbortOrTerminateRequested = true;
                    this.Scheduler.CanRun = false;
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (this._msgDeliveryLock.Enter())
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                {
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                                }
                                this.AbortOnIdle();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Abort attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
        }

        internal void AbortOnIdle()
        {
            if (this.IsInstanceValid)
            {
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Aborting);
                this.TimerQueue.SuspendDelivery();
                this.Scheduler.CanRun = false;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Aborting instance {0}", new object[] { this.InstanceIdString });
                try
                {
                    if (this.currentAtomicActivity != null)
                    {
                        this.RollbackTransaction(null, this.currentAtomicActivity);
                        this.currentAtomicActivity = null;
                    }
                    this.ResourceManager.ClearAllBatchedWork();
                    WorkflowPersistenceService workflowPersistenceService = this.WorkflowRuntime.WorkflowPersistenceService;
                    if (workflowPersistenceService != null)
                    {
                        workflowPersistenceService.UnlockWorkflowInstanceState(this.attemptedRootDispose ? null : this.rootActivity);
                        if (this.HasNonEmptyWorkBatch())
                        {
                            this.CommitTransaction(this.rootActivity);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (IsIrrecoverableException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    this.IsInstanceValid = false;
                    this.DisposeRootActivity(true);
                    if (this.currentAtomicActivity != null)
                    {
                        this.atomicActivityEvent.Set();
                        this.atomicActivityEvent.Close();
                    }
                    this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Aborted);
                }
            }
        }

        private void AddItemToBeScheduledLater(Activity atomicActivity, SchedulableItem item)
        {
            if ((atomicActivity != null) && atomicActivity.SupportsTransaction)
            {
                TransactionalProperties properties = (TransactionalProperties) atomicActivity.GetValue(TransactionalPropertiesProperty);
                if (properties != null)
                {
                    lock (properties)
                    {
                        List<SchedulableItem> itemsToBeScheduledAtCompletion = null;
                        itemsToBeScheduledAtCompletion = properties.ItemsToBeScheduledAtCompletion;
                        if (itemsToBeScheduledAtCompletion == null)
                        {
                            itemsToBeScheduledAtCompletion = new List<SchedulableItem>();
                            properties.ItemsToBeScheduledAtCompletion = itemsToBeScheduledAtCompletion;
                        }
                        itemsToBeScheduledAtCompletion.Add(item);
                    }
                }
            }
        }

        internal void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            if (workflowChanges == null)
            {
                throw new ArgumentNullException("workflowChanges");
            }
            if (!this.IsInstanceValid)
            {
                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
            }
            if (this.currentAtomicActivity != null)
            {
                throw new InvalidOperationException(ExecutionStringManager.Error_InsideAtomicScope);
            }
            try
            {
                using (new ScheduleWork(this))
                {
                    using (this._executorLock.Enter())
                    {
                        if (!this.IsInstanceValid)
                        {
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                        }
                        this.Scheduler.CanRun = false;
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                bool flag = false;
                                if (!this.IsInstanceValid)
                                {
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                                }
                                try
                                {
                                    switch (this.WorkflowStatus)
                                    {
                                        case System.Workflow.Runtime.WorkflowStatus.Completed:
                                        case System.Workflow.Runtime.WorkflowStatus.Terminated:
                                            throw new InvalidOperationException(ExecutionStringManager.InvalidOperationRequest);

                                        case System.Workflow.Runtime.WorkflowStatus.Suspended:
                                            flag = false;
                                            break;

                                        default:
                                            this.SuspendOnIdle(null);
                                            flag = true;
                                            break;
                                    }
                                    workflowChanges.ApplyTo(this.rootActivity);
                                }
                                finally
                                {
                                    if (flag)
                                    {
                                        this.ResumeOnIdle(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: dynamic update attempt from outside on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
        }

        internal static bool CheckAndProcessTransactionAborted(TransactionalProperties transactionalProperties)
        {
            if ((transactionalProperties.Transaction == null) || (transactionalProperties.Transaction.TransactionInformation.Status == TransactionStatus.Aborted))
            {
                switch (transactionalProperties.TransactionState)
                {
                    case TransactionProcessState.Ok:
                    case TransactionProcessState.Aborted:
                        transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                        throw new TransactionAbortedException();

                    case TransactionProcessState.AbortProcessed:
                        return true;
                }
            }
            return false;
        }

        private void CommitTransaction(Activity activityContext)
        {
            if (null == Transaction.Current)
            {
                try
                {
                    this.WorkflowRuntime.TransactionService.CommitWorkBatch(new WorkflowCommitWorkBatchService.CommitWorkBatchCallback(this.DoResourceManagerCommit));
                    this.ResourceManager.Complete();
                    return;
                }
                catch
                {
                    this.ResourceManager.HandleFault();
                    throw;
                }
            }
            TransactionalProperties transactionalProperties = null;
            bool flag = activityContext == this.currentAtomicActivity;
            if (flag)
            {
                transactionalProperties = (TransactionalProperties) activityContext.GetValue(TransactionalPropertiesProperty);
                if (CheckAndProcessTransactionAborted(transactionalProperties))
                {
                    return;
                }
            }
            try
            {
                this.WorkflowRuntime.TransactionService.CommitWorkBatch(new WorkflowCommitWorkBatchService.CommitWorkBatchCallback(this.DoResourceManagerCommit));
            }
            catch
            {
                this.ResourceManager.HandleFault();
                throw;
            }
            finally
            {
                if (flag)
                {
                    this.DisposeTransactionScope(transactionalProperties);
                }
            }
            if (flag)
            {
                try
                {
                    CommittableTransaction transaction = transactionalProperties.Transaction as CommittableTransaction;
                    if (null != transaction)
                    {
                        try
                        {
                            transaction.Commit();
                        }
                        catch
                        {
                            this.qService.PostPersist(false);
                            throw;
                        }
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Committed CommittableTransaction ", transactionalProperties.Transaction.GetHashCode() }));
                    }
                    DependentTransaction transaction2 = transactionalProperties.Transaction as DependentTransaction;
                    if (null != transaction2)
                    {
                        try
                        {
                            transaction2.Complete();
                        }
                        catch
                        {
                            this.qService.PostPersist(false);
                            throw;
                        }
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Completed DependentTransaction ", transactionalProperties.Transaction.GetHashCode() }));
                    }
                }
                catch
                {
                    this.ResourceManager.HandleFault();
                    throw;
                }
                this.DisposeTransaction(activityContext);
                this.currentAtomicActivity = null;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString + "Reset CurrentAtomicActivity to null");
            }
            this.ResourceManager.Complete();
        }

        private void CreateTransaction(Activity atomicActivity)
        {
            TransactionalProperties properties = new TransactionalProperties();
            TransactionOptions options = new TransactionOptions();
            WorkflowTransactionOptions transactionOptions = TransactedContextFilter.GetTransactionOptions(atomicActivity);
            options.IsolationLevel = transactionOptions.IsolationLevel;
            if (options.IsolationLevel == IsolationLevel.Unspecified)
            {
                options.IsolationLevel = IsolationLevel.Serializable;
            }
            options.Timeout = transactionOptions.TimeoutDuration;
            CommittableTransaction transaction = new CommittableTransaction(options);
            properties.Transaction = transaction;
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Created enlistable transaction ", transaction.GetHashCode(), " with timeout ", options.Timeout, ", isolation ", options.IsolationLevel }));
            properties.LocalQueuingService = new WorkflowQueuingService(this.qService);
            atomicActivity.SetValue(TransactionalPropertiesProperty, properties);
            this.currentAtomicActivity = atomicActivity;
            this.atomicActivityEvent = new ManualResetEvent(false);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString + " .Set CurrentAtomicActivity to " + atomicActivity.Name);
        }

        internal void DeliverTimerSubscriptions()
        {
            using (new ScheduleWork(this))
            {
                using (this._executorLock.Enter())
                {
                    if (this.IsInstanceValid)
                    {
                        using (this.MessageDeliveryLock.Enter())
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                if (this.IsInstanceValid)
                                {
                                    TimerEventSubscriptionCollection timerQueue = this.TimerQueue;
                                    bool flag = false;
                                    while (!flag)
                                    {
                                        lock (timerQueue.SyncRoot)
                                        {
                                            TimerEventSubscription subscription = timerQueue.Peek();
                                            if ((subscription == null) || (subscription.ExpiresAt > DateTime.UtcNow))
                                            {
                                                flag = true;
                                            }
                                            else
                                            {
                                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Delivering timer subscription for instance {0}", new object[] { this.InstanceIdString });
                                                this.stateChangedSincePersistence = true;
                                                lock (this.qService.SyncRoot)
                                                {
                                                    if (this.qService.Exists(subscription.QueueName))
                                                    {
                                                        this.qService.EnqueueEvent(subscription.QueueName, subscription.SubscriptionId);
                                                    }
                                                }
                                                timerQueue.Dequeue();
                                            }
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void DiagnosticStackTrace(string reason)
        {
            StackTrace trace = new StackTrace(true);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: InstanceId: {0} : {1} stack trace: {2}", new object[] { this.InstanceIdString, reason, trace.ToString() });
        }

        private void DisposeRootActivity(bool aborting)
        {
            try
            {
                if (!this.attemptedRootAECUnload)
                {
                    this.attemptedRootAECUnload = true;
                    this.RootActivity.OnActivityExecutionContextUnload(this);
                }
                if (!this.attemptedRootDispose)
                {
                    this.attemptedRootDispose = true;
                    this.RootActivity.Dispose();
                }
            }
            catch (Exception)
            {
                if (!aborting)
                {
                    using (this._msgDeliveryLock.Enter())
                    {
                        this.AbortOnIdle();
                        throw;
                    }
                }
            }
        }

        private void DisposeTransaction(Activity atomicActivity)
        {
            TransactionalProperties properties = (TransactionalProperties) atomicActivity.GetValue(TransactionalPropertiesProperty);
            properties.Transaction.Dispose();
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Disposed enlistable transaction ", properties.Transaction.GetHashCode() }));
            properties.Transaction = null;
            properties.LocalQueuingService = null;
            properties.Transaction = null;
            this.atomicActivityEvent.Set();
            this.atomicActivityEvent.Close();
        }

        private void DisposeTransactionScope(TransactionalProperties transactionalProperties)
        {
            if (transactionalProperties.TransactionScope != null)
            {
                transactionalProperties.TransactionScope.Complete();
                transactionalProperties.TransactionScope.Dispose();
                transactionalProperties.TransactionScope = null;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString + "Left TransactionScope, Current atomic acitivity was " + ((this.currentAtomicActivity == null) ? null : this.currentAtomicActivity.Name));
            }
        }

        private void DoResourceManagerCommit()
        {
            if (null == Transaction.Current)
            {
                throw new Exception(ExecutionStringManager.NullAmbientTransaction);
            }
            this.ResourceManager.Commit();
        }

        internal void EnqueueItem(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            using (new ScheduleWork(this))
            {
                bool flag = false;
                if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                {
                    flag = this._schedulerLock.TryEnter();
                }
                try
                {
                    using (this.MessageDeliveryLock.Enter())
                    {
                        if (!this.IsInstanceValid)
                        {
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                        }
                        if (flag || ServiceEnvironment.IsInServiceThread(this.InstanceId))
                        {
                            using (new ServiceEnvironment(this.RootActivity))
                            {
                                this.qService.EnqueueEvent(queueName, item);
                                goto Label_008E;
                            }
                        }
                        if (this.qService.SafeEnqueueEvent(queueName, item))
                        {
                            ScheduleWork.NeedsService = true;
                        }
                    Label_008E:
                        if (pendingWork != null)
                        {
                            this._resourceManager.BatchCollection.GetBatch(this.rootActivity).Add(pendingWork, workItem);
                        }
                        this.stateChangedSincePersistence = true;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this._schedulerLock.Exit();
                    }
                }
            }
        }

        internal void EnqueueItemOnIdle(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            using (new ScheduleWork(this))
            {
                using (this._executorLock.Enter())
                {
                    if (!this.IsInstanceValid)
                    {
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                    }
                    using (InstanceLock.InstanceLockGuard guard = this.MessageDeliveryLock.Enter())
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {
                            if (this.IsInstanceValid)
                            {
                                goto Label_006C;
                            }
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                        Label_0052:
                            guard.Wait();
                            if (!this.IsInstanceValid)
                            {
                                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                            }
                        Label_006C:
                            if (!this.IsIdle)
                            {
                                goto Label_0052;
                            }
                            if ((this.WorkflowStatus == System.Workflow.Runtime.WorkflowStatus.Suspended) || !this.Scheduler.CanRun)
                            {
                                throw new InvalidOperationException(ExecutionStringManager.InvalidWaitForIdleOnSuspendedWorkflow);
                            }
                            try
                            {
                                if (pendingWork != null)
                                {
                                    ((IWorkBatch) this.rootActivity.GetValue(TransientBatchProperty)).Add(pendingWork, workItem);
                                }
                                this.stateChangedSincePersistence = true;
                                this.qService.EnqueueEvent(queueName, item);
                            }
                            finally
                            {
                                if (this.IsIdle)
                                {
                                    guard.Pulse();
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void ExceptionOccured(Exception exp, Activity currentActivity, string originalActivityId)
        {
            if (this.ThrownException != exp)
            {
                this.ThrownException = exp;
                this.activityThrowingException = currentActivity.QualifiedName;
                originalActivityId = currentActivity.QualifiedName;
            }
            else
            {
                originalActivityId = this.activityThrowingException;
            }
            Guid contextGuid = ((ActivityExecutionContextInfo) ContextActivityUtils.ContextActivity(currentActivity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
            Guid empty = Guid.Empty;
            if (currentActivity.Parent != null)
            {
                empty = ((ActivityExecutionContextInfo) ContextActivityUtils.ContextActivity(currentActivity.Parent).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
            }
            this.FireExceptionOccured(exp, currentActivity.QualifiedName, originalActivityId, contextGuid, empty);
            using (new ServiceEnvironment(currentActivity))
            {
                using (this.SetCurrentActivity(currentActivity))
                {
                    using (ActivityExecutionContext context = new ActivityExecutionContext(currentActivity, true))
                    {
                        context.FaultActivity(exp);
                    }
                }
            }
            this.RollbackTransaction(exp, currentActivity);
            if ((currentActivity is TransactionScopeActivity) || (exp is PersistenceException))
            {
                this.BatchCollection.RollbackBatch(currentActivity);
            }
        }

        private Activity FindExecutorToHandleException()
        {
            Activity currentActivity = this.CurrentActivity;
            if (currentActivity == null)
            {
                currentActivity = this.rootActivity;
            }
            return currentActivity;
        }

        private void FireActivityExecuting(object sender, Activity activity)
        {
            ActivityExecutingEventArgs e = new ActivityExecutingEventArgs(activity);
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void FireActivityStatusChange(object sender, Activity activity)
        {
            ActivityStatusChangeEventArgs e = new ActivityStatusChangeEventArgs(activity);
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void FireDynamicUpdateBegin(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeBegin));
            }
        }

        private void FireDynamicUpdateCommit(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeCommit));
            }
        }

        private void FireDynamicUpdateRollback(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeRollback));
            }
        }

        private void FireEventAfterSchedulerLockDrop(WorkflowEventInternal workflowEventInternal)
        {
            this.eventsToFireList.Add(new SchedulerLockGuardInfo(this, workflowEventInternal));
        }

        private void FireEventAfterSchedulerLockDrop(WorkflowEventInternal workflowEventInternal, object eventInfo)
        {
            this.eventsToFireList.Add(new SchedulerLockGuardInfo(this, workflowEventInternal, eventInfo));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void FireExceptionOccured(Exception e, string currentActivityPath, string originalActivityPath, Guid contextGuid, Guid parentContextGuid)
        {
            this.FireWorkflowException(e, currentActivityPath, originalActivityPath, contextGuid, parentContextGuid);
        }

        private void FireUserTrackPoint(Activity activity, string key, object args)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new UserTrackPointEventArgs(activity, key, args));
            }
        }

        private void FireWorkflowException(Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionExceptionEventArgs(exception, currentPath, originalPath, contextGuid, parentContextGuid));
            }
        }

        internal void FireWorkflowExecutionEvent(object sender, WorkflowEventInternal eventType)
        {
            if (sender == null)
            {
                sender = this;
            }
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(sender, new WorkflowExecutionEventArgs(eventType));
            }
        }

        internal void FireWorkflowHandlerInvokingEvent(object sender, WorkflowEventInternal eventType, Delegate delegateHandler)
        {
            if (sender == null)
            {
                sender = this;
            }
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(sender, new WorkflowHandlerInvokingEventArgs(eventType, delegateHandler));
            }
        }

        internal void FireWorkflowSuspended(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionSuspendedEventArgs(error));
            }
        }

        private void FireWorkflowSuspending(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionSuspendingEventArgs(error));
            }
        }

        internal void FireWorkflowTerminated(Exception exception)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionTerminatedEventArgs(exception));
            }
        }

        internal void FireWorkflowTerminated(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionTerminatedEventArgs(error));
            }
        }

        private void FireWorkflowTerminating(Exception exception)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionTerminatingEventArgs(exception));
            }
        }

        private void FireWorkflowTerminating(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> handler = this._workflowExecutionEvent;
            if (handler != null)
            {
                handler(this, new WorkflowExecutionTerminatingEventArgs(error));
            }
        }

        internal Activity GetContextActivityForId(int stateId)
        {
            if (this.subStateMap.ContainsKey(stateId))
            {
                return this.subStateMap[stateId];
            }
            return null;
        }

        private static string GetNestedExceptionMessage(Exception exp)
        {
            string message = "";
            while (exp != null)
            {
                if (message == "")
                {
                    message = exp.Message;
                }
                else
                {
                    message = message + " " + exp.Message;
                }
                exp = exp.InnerException;
            }
            return message;
        }

        private int GetNewContextId()
        {
            int num = ((int) this.rootActivity.GetValue(ContextIdProperty)) + 1;
            this.rootActivity.SetValue(ContextIdProperty, num);
            return num;
        }

        private static object GetTransientBatch(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException("dependencyObject");
            }
            if (!(dependencyObject is Activity))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, new object[] { "dependencyObject", typeof(Activity).ToString() }));
            }
            Activity parent = (Activity) dependencyObject;
            IWorkflowCoreRuntime runtime = null;
            ISupportInterop interop = null;
            if (parent != null)
            {
                runtime = ContextActivityUtils.RetrieveWorkflowExecutor(parent);
                interop = runtime as ISupportInterop;
            }
            while (parent != null)
            {
                IWorkBatch valueBase = parent.GetValueBase(TransientBatchProperty) as IWorkBatch;
                if (valueBase != null)
                {
                    return valueBase;
                }
                if ((TransactedContextFilter.GetTransactionOptions(parent) != null) && (parent.ExecutionStatus == ActivityExecutionStatus.Executing))
                {
                    return interop.BatchCollection.GetBatch(parent);
                }
                if (parent is CompositeActivity)
                {
                    foreach (Activity activity2 in ((ISupportAlternateFlow) parent).AlternateFlowActivities)
                    {
                        if (activity2 is FaultHandlerActivity)
                        {
                            return interop.BatchCollection.GetBatch(parent);
                        }
                    }
                }
                if (parent == runtime.RootActivity)
                {
                    return interop.BatchCollection.GetBatch(parent);
                }
                parent = parent.Parent;
            }
            return null;
        }

        internal Activity GetWorkflowDefinition(string workflowContext)
        {
            if (workflowContext == null)
            {
                throw new ArgumentNullException("workflowContext");
            }
            return this.WorkflowDefinition;
        }

        internal Activity GetWorkflowDefinitionClone(string workflowContext)
        {
            if (workflowContext == null)
            {
                throw new ArgumentNullException("workflowContext");
            }
            Activity workflowDefinition = this.WorkflowDefinition;
            using (new WorkflowDefinitionLock(workflowDefinition))
            {
                return workflowDefinition.Clone();
            }
        }

        internal DateTime GetWorkflowNextTimerExpiration()
        {
            DateTime time;
            using (this._executorLock.Enter())
            {
                using (this.MessageDeliveryLock.Enter())
                {
                    TimerEventSubscription subscription = this.TimerQueue.Peek();
                    time = (subscription == null) ? DateTime.MaxValue : subscription.ExpiresAt;
                }
            }
            return time;
        }

        internal ReadOnlyCollection<WorkflowQueueInfo> GetWorkflowQueueInfos()
        {
            List<WorkflowQueueInfo> list = new List<WorkflowQueueInfo>();
            using (this.MessageDeliveryLock.Enter())
            {
                using (new ServiceEnvironment(this.rootActivity))
                {
                    lock (this.qService.SyncRoot)
                    {
                        foreach (IComparable comparable in this.qService.QueueNames)
                        {
                            try
                            {
                                if (this.qService.GetWorkflowQueue(comparable).Enabled)
                                {
                                    Queue messages = this.qService.GetQueue(comparable).Messages;
                                    List<ActivityExecutorDelegateInfo<QueueEventArgs>> asynchronousListeners = this.qService.GetQueue(comparable).AsynchronousListeners;
                                    List<string> list3 = new List<string>();
                                    foreach (ActivityExecutorDelegateInfo<QueueEventArgs> info in asynchronousListeners)
                                    {
                                        string item = (info.SubscribedActivityQualifiedName == null) ? info.ActivityQualifiedName : info.SubscribedActivityQualifiedName;
                                        list3.Add(item);
                                    }
                                    list.Add(new WorkflowQueueInfo(comparable, messages, list3.AsReadOnly()));
                                }
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }
                }
            }
            return list.AsReadOnly();
        }

        private bool HasNonEmptyWorkBatch()
        {
            foreach (WorkBatch batch in this.ResourceManager.BatchCollection.Values)
            {
                if (batch.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal void Initialize(Activity rootActivity, System.Workflow.Runtime.WorkflowRuntime runtime, WorkflowExecutor previousWorkflowExecutor)
        {
            this._workflowInstance = previousWorkflowExecutor.WorkflowInstance;
            this.ReloadHelper(rootActivity);
            this.IsInstanceValid = true;
            this._runtime = runtime;
            this._runtime.WorkflowExecutorCreated(this, true);
            this.TimerQueue.Executor = this;
            this.TimerQueue.ResumeDelivery();
            this._executorLock = previousWorkflowExecutor._executorLock;
            this._msgDeliveryLock = previousWorkflowExecutor._msgDeliveryLock;
            this._schedulerLock = previousWorkflowExecutor._schedulerLock;
            ScheduleWork.Executor = this;
        }

        internal void Initialize(Activity rootActivity, WorkflowExecutor invokerExec, string invokeActivityID, Guid instanceId, IDictionary<string, object> namedArguments, System.Workflow.Runtime.WorkflowInstance workflowInstance)
        {
            this.rootActivity = rootActivity;
            this.InstanceId = instanceId;
            this.rootActivity.SetValue(ContextIdProperty, 0);
            this.rootActivity.SetValue(WorkflowInstanceIdProperty, instanceId);
            this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Created;
            this.rootActivity.SetValue(Activity.ActivityExecutionContextInfoProperty, new ActivityExecutionContextInfo(this.rootActivity.QualifiedName, this.GetNewContextId(), instanceId, -1));
            this.rootActivity.SetValue(Activity.ActivityContextGuidProperty, instanceId);
            this.rootActivity.SetValue(IsIdleProperty, true);
            this.isInstanceIdle = true;
            this.rootActivity.SetValue(WorkflowExecutorProperty, this);
            this.RefreshWorkflowDefinition();
            Activity workflowDefinition = this.WorkflowDefinition;
            if (workflowDefinition == null)
            {
                throw new InvalidOperationException("workflowDefinition");
            }
            ((IDependencyObjectAccessor) this.rootActivity).InitializeActivatingInstanceForRuntime(null, this);
            this.rootActivity.FixUpMetaProperties(workflowDefinition);
            this._runtime = workflowInstance.WorkflowRuntime;
            if (invokerExec != null)
            {
                List<string> list = new List<string>();
                System.Workflow.Runtime.TrackingCallingState state = (System.Workflow.Runtime.TrackingCallingState) invokerExec.rootActivity.GetValue(TrackingCallingStateProperty);
                if ((state != null) && (state.CallerActivityPathProxy != null))
                {
                    foreach (string str in state.CallerActivityPathProxy)
                    {
                        list.Add(str);
                    }
                }
                list.Add(invokeActivityID);
                System.Workflow.Runtime.TrackingCallingState state2 = new System.Workflow.Runtime.TrackingCallingState {
                    CallerActivityPathProxy = list,
                    CallerWorkflowInstanceId = invokerExec.InstanceId,
                    CallerContextGuid = ((ActivityExecutionContextInfo) ContextActivityUtils.ContextActivity(invokerExec.CurrentActivity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid
                };
                if (invokerExec.CurrentActivity.Parent == null)
                {
                    state2.CallerParentContextGuid = state2.CallerContextGuid;
                }
                else
                {
                    state2.CallerParentContextGuid = ((ActivityExecutionContextInfo) ContextActivityUtils.ContextActivity(invokerExec.CurrentActivity.Parent).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
                }
                this.rootActivity.SetValue(TrackingCallingStateProperty, state2);
            }
            this._setInArgsOnCompanion(namedArguments);
            this.schedulingContext = new System.Workflow.Runtime.Scheduler(this, true);
            this._schedulerLock = LockFactory.CreateWorkflowSchedulerLock(this.InstanceId);
            this.qService = new WorkflowQueuingService(this);
            this._workflowInstance = workflowInstance;
            this.TimerQueue = new TimerEventSubscriptionCollection(this, this.InstanceId);
            using (new ServiceEnvironment(this.rootActivity))
            {
                using (this.SetCurrentActivity(this.rootActivity))
                {
                    this.RegisterDynamicActivity(this.rootActivity, false);
                }
            }
        }

        internal bool IsActivityInAtomicContext(Activity activity, out Activity atomicActivity)
        {
            atomicActivity = null;
            while (activity != null)
            {
                if (activity == this.currentAtomicActivity)
                {
                    atomicActivity = activity;
                    return true;
                }
                activity = activity.Parent;
            }
            return false;
        }

        internal static bool IsIrrecoverableException(Exception e)
        {
            return ((((e is OutOfMemoryException) || (e is StackOverflowException)) || (e is ThreadInterruptedException)) || (e is ThreadAbortException));
        }

        internal void OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes)
        {
            if (updateSucceeded)
            {
                this.RefreshWorkflowDefinition();
                this.FireDynamicUpdateCommit(changes);
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Changed);
            }
            else
            {
                this.FireDynamicUpdateRollback(changes);
            }
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Done updating a schedule in instance {0}", new object[] { this.InstanceIdString });
        }

        internal bool OnBeforeDynamicChange(IList<WorkflowChangeAction> changes)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a dynamic update request for instance {0}", new object[] { this.InstanceIdString });
            if (!this.IsInstanceValid)
            {
                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
            }
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Found a match for the schedule in updating instance {0}", new object[] { this.InstanceIdString });
            this.FireDynamicUpdateBegin(changes);
            return true;
        }

        private bool PerformUnloading(bool handleExceptions)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Unloading instance {0}", new object[] { this.InstanceIdString });
            this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Unloading);
            using (this._msgDeliveryLock.Enter())
            {
                bool flag;
                this.TimerQueue.SuspendDelivery();
                if (handleExceptions)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Calling PerformUnloading(false): InstanceId {0}, hc: {1}", new object[] { this.InstanceIdString, this.GetHashCode() });
                    flag = this.ProtectedPersist(true);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Returning from ProtectedPersist: InstanceId {0}, hc: {1}, ret={2}", new object[] { this.InstanceIdString, this.GetHashCode(), flag });
                }
                else
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Calling Persist");
                    this.Persist(this.rootActivity, true, false);
                    flag = true;
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Returning from Persist: InstanceId {0}, hc: {1}, IsInstanceValid={2}", new object[] { this.InstanceIdString, this.GetHashCode(), this.IsInstanceValid });
                }
                if (flag)
                {
                    this.IsInstanceValid = false;
                    this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Unloaded);
                    return true;
                }
                return false;
            }
        }

        internal void Persist(Activity dynamicActivity, bool unlock, bool needsCompensation)
        {
            Activity currentActivity = (this.CurrentActivity == null) ? dynamicActivity : this.CurrentActivity;
            System.Workflow.Runtime.WorkflowStatus workflowStatus = this.WorkflowStatus;
            using (new ServiceEnvironment(currentActivity))
            {
                try
                {
                    using (this.MessageDeliveryLock.Enter())
                    {
                        this.ProcessQueuedEvents();
                        if (this.ResourceManager.IsBatchDirty)
                        {
                            this.stateChangedSincePersistence = true;
                        }
                        else if (!this.stateChangedSincePersistence && !unlock)
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: NOT Persisting Instance '{0}' since the batch is NOT dirty and the instance state is NOT dirty", new object[] { this.InstanceIdString });
                            return;
                        }
                        this.PrePersist();
                        if (System.Workflow.Runtime.WorkflowStatus.Completed == this.WorkflowStatus)
                        {
                            this.qService.MoveAllMessagesToPendingQueue();
                        }
                        WorkflowPersistenceService workflowPersistenceService = this.WorkflowRuntime.WorkflowPersistenceService;
                        currentActivity.SetValue(TransientBatchProperty, this._resourceManager.BatchCollection.GetTransientBatch());
                        bool flag = false;
                        if (workflowPersistenceService != null)
                        {
                            foreach (Activity activity2 in this.completedContextActivities.Values)
                            {
                                activity2.SetValue(WorkflowInstanceIdProperty, this.InstanceId);
                                if (!flag)
                                {
                                    this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Persisting);
                                    flag = true;
                                }
                                workflowPersistenceService.SaveCompletedContextActivity(activity2);
                                activity2.Dispose();
                            }
                            if (this.stateChangedSincePersistence)
                            {
                                if (!flag)
                                {
                                    this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Persisting);
                                    flag = true;
                                }
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Calling SaveWorkflowInstanceState for instance {0} hc {1}", new object[] { this.InstanceIdString, this.GetHashCode() });
                                workflowPersistenceService.SaveWorkflowInstanceState(this.rootActivity, unlock);
                            }
                            else if (unlock)
                            {
                                workflowPersistenceService.UnlockWorkflowInstanceState(this.rootActivity);
                            }
                        }
                        if (unlock)
                        {
                            this.DisposeRootActivity(false);
                        }
                        if (((this.currentAtomicActivity != null) || this.ResourceManager.IsBatchDirty) || (unlock && this.HasNonEmptyWorkBatch()))
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Calling CommitTransaction for instance {0} hc {1}", new object[] { this.InstanceIdString, this.GetHashCode() });
                            this.CommitTransaction(currentActivity);
                        }
                        if (flag)
                        {
                            this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Persisted);
                        }
                        this.stateChangedSincePersistence = false;
                        this.PostPersist();
                        if (System.Workflow.Runtime.WorkflowStatus.Completed == this.WorkflowStatus)
                        {
                            this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Completed);
                            this.IsInstanceValid = false;
                        }
                    }
                }
                catch (PersistenceException exception)
                {
                    this.Rollback(workflowStatus);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persist attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                    throw;
                }
                catch (Exception exception2)
                {
                    if (IsIrrecoverableException(exception2))
                    {
                        throw;
                    }
                    this.Rollback(workflowStatus);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persist attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception2.Message, exception2.StackTrace });
                    throw new PersistenceException(exception2.Message, exception2);
                }
                finally
                {
                    currentActivity.SetValue(TransientBatchProperty, null);
                }
            }
        }

        private void PostPersist()
        {
            this.qService.PostPersist(true);
            if (this.Scheduler != null)
            {
                this.Scheduler.PostPersist();
            }
            this.completedContextActivities.Clear();
        }

        private void PrePersist()
        {
            System.Workflow.Runtime.WorkflowStatus workflowStatus = this.WorkflowStatus;
            if ((ActivityExecutionStatus.Closed == this.rootActivity.ExecutionStatus) && (System.Workflow.Runtime.WorkflowStatus.Terminated != workflowStatus))
            {
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Completing);
                this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Completed;
            }
            switch (this.WorkflowStatus)
            {
                case System.Workflow.Runtime.WorkflowStatus.Running:
                    this.rootActivity.SetValue(IsBlockedProperty, this.Scheduler.IsStalledNow);
                    break;

                case System.Workflow.Runtime.WorkflowStatus.Completed:
                case System.Workflow.Runtime.WorkflowStatus.Suspended:
                case System.Workflow.Runtime.WorkflowStatus.Terminated:
                case System.Workflow.Runtime.WorkflowStatus.Created:
                    this.rootActivity.SetValue(IsBlockedProperty, false);
                    break;
            }
            this.qService.PrePersist();
        }

        internal void ProcessQueuedEvents()
        {
            using (this.MessageDeliveryLock.Enter())
            {
                this.qService.ProcessesQueuedAsynchronousEvents();
            }
        }

        private bool ProtectedPersist(bool unlock)
        {
            try
            {
                this.Persist(this.rootActivity, unlock, false);
            }
            catch (Exception exception)
            {
                if (IsIrrecoverableException(exception))
                {
                    throw;
                }
                if ((this.WorkflowStatus != System.Workflow.Runtime.WorkflowStatus.Suspended) && this.IsInstanceValid)
                {
                    Activity currentActivity = this.FindExecutorToHandleException();
                    this.Scheduler.CanRun = true;
                    this.ExceptionOccured(exception, currentActivity, null);
                }
                else if (this.TerminateOnIdle(GetNestedExceptionMessage(exception)))
                {
                    this.stateChangedSincePersistence = true;
                    this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Terminated;
                }
                return false;
            }
            return true;
        }

        private void RefreshWorkflowDefinition()
        {
            Activity dependencyObject = (Activity) this.rootActivity.GetValue(Activity.WorkflowDefinitionProperty);
            WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(dependencyObject, new object());
            this._workflowDefinition = dependencyObject;
        }

        internal void RegisterDynamicActivity(Activity dynamicActivity, bool load)
        {
            int key = ContextActivityUtils.ContextId(dynamicActivity);
            this.subStateMap.Add(key, dynamicActivity);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Adding context {0}:{1}", new object[] { key, dynamicActivity.QualifiedName + (load ? " for load" : "") });
            dynamicActivity.OnActivityExecutionContextLoad(this);
        }

        internal void Registered(bool isActivation)
        {
            using (new ScheduleWork(this))
            {
                this.Scheduler.ResumeIfRunnable();
            }
            if (isActivation)
            {
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Created);
            }
            else
            {
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Loaded);
            }
        }

        internal void RegisterWithRuntime(System.Workflow.Runtime.WorkflowRuntime workflowRuntime)
        {
            this._isInstanceValid = true;
            this._runtime = workflowRuntime;
            using (new ServiceEnvironment(this.rootActivity))
            {
                using (this.SetCurrentActivity(this.rootActivity))
                {
                    using (ActivityExecutionContext context = new ActivityExecutionContext(this.rootActivity, true))
                    {
                        context.InitializeActivity(this.rootActivity);
                    }
                }
                this._runtime.WorkflowExecutorCreated(this, false);
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Creating);
            }
        }

        internal void Reload(Activity rootActivity, System.Workflow.Runtime.WorkflowInstance workflowInstance)
        {
            this._workflowInstance = workflowInstance;
            this.ReloadHelper(rootActivity);
        }

        private void ReloadHelper(Activity rootActivity)
        {
            this.rootActivity = rootActivity;
            this.InstanceId = (Guid) rootActivity.GetValue(WorkflowInstanceIdProperty);
            this.rootActivity.SetValue(WorkflowExecutorProperty, this);
            this._schedulerLock = LockFactory.CreateWorkflowSchedulerLock(this.InstanceId);
            this.schedulingContext = new System.Workflow.Runtime.Scheduler(this, false);
            this.qService = new WorkflowQueuingService(this);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Loading instance {0}", new object[] { this.InstanceIdString });
            using (new ServiceEnvironment(this.rootActivity))
            {
                switch (this.WorkflowStatus)
                {
                    case System.Workflow.Runtime.WorkflowStatus.Completed:
                    case System.Workflow.Runtime.WorkflowStatus.Terminated:
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: attempt to load a completed/terminated instance: {0}", new object[] { this.InstanceIdString });
                        throw new InvalidOperationException(ExecutionStringManager.InvalidAttemptToLoad);
                }
                this._resourceManager = new VolatileResourceManager();
                this._runtime = this._workflowInstance.WorkflowRuntime;
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(this.rootActivity);
                while (queue.Count > 0)
                {
                    Activity dynamicActivity = queue.Dequeue();
                    ((IDependencyObjectAccessor) dynamicActivity).InitializeInstanceForRuntime(this);
                    this.RegisterDynamicActivity(dynamicActivity, true);
                    IList<Activity> list = (IList<Activity>) dynamicActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                    if (list != null)
                    {
                        foreach (Activity activity2 in list)
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                }
            }
            this.isInstanceIdle = (bool) this.rootActivity.GetValue(IsIdleProperty);
            this.RefreshWorkflowDefinition();
        }

        internal void RequestHostingService()
        {
            this.WorkflowRuntime.SchedulerService.Schedule(new WaitCallback(this.RunSome), this.InstanceId);
        }

        internal void ReRegisterWithRuntime(System.Workflow.Runtime.WorkflowRuntime workflowRuntime)
        {
            using (new SchedulerLockGuard(this._schedulerLock, this))
            {
                this._isInstanceValid = true;
                this._runtime = workflowRuntime;
                using (new ServiceEnvironment(this.rootActivity))
                {
                    this._runtime.WorkflowExecutorCreated(this, true);
                    this.TimerQueue.Executor = this;
                    this.TimerQueue.ResumeDelivery();
                    if (this.WorkflowStatus == System.Workflow.Runtime.WorkflowStatus.Running)
                    {
                        this.Scheduler.CanRun = true;
                    }
                    this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Loading);
                }
            }
        }

        internal void Resume()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a resume request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                if (!this.IsInstanceValid)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                }
                using (new ScheduleWork(this))
                {
                    using (this._executorLock.Enter())
                    {
                        if (!this.IsInstanceValid)
                        {
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                        }
                        if (this.WorkflowStatus == System.Workflow.Runtime.WorkflowStatus.Suspended)
                        {
                            using (new SchedulerLockGuard(this._schedulerLock, this))
                            {
                                using (new ServiceEnvironment(this.rootActivity))
                                {
                                    this.ResumeOnIdle(true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Resume attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
        }

        internal bool ResumeOnIdle(bool outsideThread)
        {
            if (!this.IsInstanceValid)
            {
                return false;
            }
            if ((this.WorkflowStatus != System.Workflow.Runtime.WorkflowStatus.Suspended) && !this.Scheduler.CanRun)
            {
                return false;
            }
            this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Resuming);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Resuming instance {0}", new object[] { this.InstanceIdString });
            this.stateChangedSincePersistence = true;
            this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Running;
            this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, string.Empty);
            this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Resumed, this.ThrownException);
            using (this._msgDeliveryLock.Enter())
            {
                this.TimerQueue.ResumeDelivery();
            }
            if (outsideThread)
            {
                this.Scheduler.Resume();
            }
            else
            {
                this.Scheduler.CanRun = true;
            }
            return true;
        }

        private void Rollback(System.Workflow.Runtime.WorkflowStatus oldStatus)
        {
            this.WorkflowStatus = oldStatus;
            if (this.Scheduler != null)
            {
                this.Scheduler.Rollback();
            }
        }

        private void RollbackTransaction(Exception exp, Activity activityContext)
        {
            if (activityContext == this.currentAtomicActivity)
            {
                TransactionalProperties transactionalProperties = (TransactionalProperties) activityContext.GetValue(TransactionalPropertiesProperty);
                if (transactionalProperties.TransactionState != TransactionProcessState.AbortProcessed)
                {
                    transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                }
                try
                {
                    this.DisposeTransactionScope(transactionalProperties);
                    Transaction transaction = transactionalProperties.Transaction;
                    if (TransactionStatus.Aborted != transaction.TransactionInformation.Status)
                    {
                        transaction.Rollback();
                    }
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Aborted enlistable transaction ", transaction.GetHashCode() }));
                }
                finally
                {
                    transactionalProperties.LocalQueuingService.Complete(false);
                    this.DisposeTransaction(this.currentAtomicActivity);
                }
            }
        }

        private void RunScheduler()
        {
            try
            {
                this.Scheduler.Run();
            }
            finally
            {
                this.IsIdle = true;
            }
            if (!this.IsInstanceValid)
            {
                return;
            }
            if (this.WorkflowStateRollbackService.IsInstanceStateRevertRequested)
            {
                using (this.MessageDeliveryLock.Enter())
                {
                    this.WorkflowStateRollbackService.RevertToCheckpointState();
                    return;
                }
            }
            if (this.Scheduler.IsStalledNow)
            {
                using (this.MessageDeliveryLock.Enter())
                {
                    if (this.rootActivity.ExecutionStatus != ActivityExecutionStatus.Closed)
                    {
                        this.ProcessQueuedEvents();
                        if (this.Scheduler.IsStalledNow)
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: workflow instance '{0}' has no work.", new object[] { this.InstanceIdString });
                            this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.SchedulerEmpty);
                            this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Idle);
                            WorkflowPersistenceService workflowPersistenceService = this.WorkflowRuntime.WorkflowPersistenceService;
                            if ((workflowPersistenceService != null) && workflowPersistenceService.UnloadOnIdle(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                {
                                    return;
                                }
                                if (this.IsUnloadableNow && !(this.ThrownException is PersistenceException))
                                {
                                    this.PerformUnloading(true);
                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowExecutor: unloaded workflow instance '{0}'.  IsInstanceValid={1}", new object[] { this.InstanceIdString, this.IsInstanceValid });
                                }
                            }
                            else if (this.ResourceManager.IsBatchDirty && (this.currentAtomicActivity == null))
                            {
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: workflow instance '{0}' has no work and the batch is dirty. Persisting state and commiting batch.", new object[] { this.InstanceIdString });
                                this.Persist(this.rootActivity, false, false);
                            }
                        }
                    }
                    goto Label_01E1;
                }
            }
            if ((bool) this.rootActivity.GetValue(IsSuspensionRequestedProperty))
            {
                this.SuspendOnIdle(this.AdditionalInformation);
                this.rootActivity.SetValue(IsSuspensionRequestedProperty, false);
            }
        Label_01E1:
            if (this.currentAtomicActivity != null)
            {
                TransactionalProperties transactionalProperties = (TransactionalProperties) this.currentAtomicActivity.GetValue(TransactionalPropertiesProperty);
                this.DisposeTransactionScope(transactionalProperties);
            }
        }

        internal void RunSome(object ignored)
        {
            using (new ScheduleWork(this))
            {
                using (new WorkflowTraceTransfer(this.InstanceId))
                {
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {
                            if (this.IsInstanceValid && (((this.rootActivity.ExecutionStatus != ActivityExecutionStatus.Closed) && (System.Workflow.Runtime.WorkflowStatus.Completed != this.WorkflowStatus)) && (System.Workflow.Runtime.WorkflowStatus.Terminated != this.WorkflowStatus)))
                            {
                                bool flag = false;
                                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                                {
                                    try
                                    {
                                        this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Executing);
                                        this.RunScheduler();
                                    }
                                    catch (Exception exception)
                                    {
                                        if (IsIrrecoverableException(exception))
                                        {
                                            flag = true;
                                            throw;
                                        }
                                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Fatal exception thrown in the scheduler. Terminating the workflow instance '{0}'. Exception:{1}\n{2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                                        this.TerminateOnIdle(GetNestedExceptionMessage(exception));
                                        this.ThrownException = exception;
                                    }
                                    finally
                                    {
                                        if (!flag)
                                        {
                                            this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.NotExecuting);
                                        }
                                    }
                                    scope.Complete();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ScheduleDelayedItems(Activity atomicActivity)
        {
            List<SchedulableItem> itemsToBeScheduledAtCompletion = null;
            TransactionalProperties properties = (TransactionalProperties) atomicActivity.GetValue(TransactionalPropertiesProperty);
            if (properties != null)
            {
                lock (properties)
                {
                    itemsToBeScheduledAtCompletion = properties.ItemsToBeScheduledAtCompletion;
                    if (itemsToBeScheduledAtCompletion != null)
                    {
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "Workflow Runtime: WorkflowExecutor: instanceId: ", this.InstanceIdString, " .Scheduling delayed ", itemsToBeScheduledAtCompletion.Count, " number of items" }));
                        foreach (SchedulableItem item in itemsToBeScheduledAtCompletion)
                        {
                            this.Scheduler.ScheduleItem(item, false, true);
                        }
                        itemsToBeScheduledAtCompletion.Clear();
                        properties.ItemsToBeScheduledAtCompletion = null;
                    }
                }
            }
        }

        internal void ScheduleForWork()
        {
            this.IsIdle = false;
            if (this.IsInstanceValid)
            {
                this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Runnable);
            }
            ScheduleWork.NeedsService = true;
        }

        public IDisposable SetCurrentActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            Activity currentActivity = this.CurrentActivity;
            this.CurrentActivity = activity;
            return new ResetCurrentActivity(this, currentActivity);
        }

        internal void Start()
        {
            using (new ScheduleWork(this))
            {
                using (this.ExecutorLock.Enter())
                {
                    if (this.WorkflowStatus != System.Workflow.Runtime.WorkflowStatus.Created)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CannotStartInstanceTwice, new object[] { this.InstanceId }));
                    }
                    this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Running;
                    using (new ServiceEnvironment(this.rootActivity))
                    {
                        this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Starting);
                        try
                        {
                            using (ActivityExecutionContext context = new ActivityExecutionContext(this.rootActivity, true))
                            {
                                this.schedulingContext.CanRun = true;
                                using (new SchedulerLockGuard(this._schedulerLock, this))
                                {
                                    context.ExecuteActivity(this.rootActivity);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            this.Terminate(exception.Message);
                            throw;
                        }
                        this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.Started);
                    }
                }
            }
        }

        internal bool Suspend(string error)
        {
            bool flag;
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a suspend request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                if (!this.IsInstanceValid)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                }
                using (this._executorLock.Enter())
                {
                    if (!this.IsInstanceValid)
                    {
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                    }
                    this.Scheduler.CanRun = false;
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {
                            if (!this.IsInstanceValid)
                            {
                                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                            }
                            flag = this.SuspendOnIdle(error);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Suspend attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
            return flag;
        }

        internal bool SuspendOnIdle(string error)
        {
            if (this.IsInstanceValid)
            {
                if (this.currentAtomicActivity != null)
                {
                    this.Scheduler.CanRun = true;
                    throw new ExecutorLocksHeldException(this.atomicActivityEvent);
                }
                switch (this.WorkflowStatus)
                {
                    case System.Workflow.Runtime.WorkflowStatus.Suspended:
                    case System.Workflow.Runtime.WorkflowStatus.Created:
                        return false;
                }
                this.FireWorkflowSuspending(error);
                this.Scheduler.CanRun = false;
                switch (this.rootActivity.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Initialized:
                    case ActivityExecutionStatus.Executing:
                    case ActivityExecutionStatus.Canceling:
                    case ActivityExecutionStatus.Compensating:
                    case ActivityExecutionStatus.Faulting:
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Suspending instance {0}", new object[] { this.InstanceIdString });
                        this.stateChangedSincePersistence = true;
                        this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Suspended;
                        this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, error);
                        this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Suspended, error);
                        return true;

                    case ActivityExecutionStatus.Closed:
                        return false;
                }
            }
            return false;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IWorkflowCoreRuntime) this).GetService(this.rootActivity, serviceType);
        }

        void IWorkflowCoreRuntime.ActivityStatusChanged(Activity activity, bool transacted, bool committed)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            if (!committed)
            {
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    if ((TransactedContextFilter.GetTransactionOptions(activity) != null) && (this.WorkflowRuntime.WorkflowPersistenceService == null))
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, new object[] { this.InstanceId });
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, message);
                        throw new InvalidOperationException(message);
                    }
                }
                else if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    this.ScheduleDelayedItems(activity);
                }
                else if (((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)) && (TransactedContextFilter.GetTransactionOptions(activity) != null))
                {
                    this.BatchCollection.RollbackBatch(activity);
                }
            }
            if (!committed)
            {
                this.FireActivityStatusChange(this, activity);
            }
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Closed) && (!(activity is ICompensatableActivity) || ((activity is ICompensatableActivity) && activity.CanUninitializeNow)))
            {
                CorrelationTokenCollection.UninitializeCorrelationTokens(activity);
            }
        }

        void IWorkflowCoreRuntime.CheckpointInstanceState(Activity currentActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            using (this.MessageDeliveryLock.Enter())
            {
                this.WorkflowStateRollbackService.CheckpointInstanceState();
            }
            this.CreateTransaction(currentActivity);
        }

        void IWorkflowCoreRuntime.DisposeCheckpointState()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.WorkflowStateRollbackService.DisposeCheckpointState();
        }

        Activity IWorkflowCoreRuntime.GetContextActivityForId(int stateId)
        {
            if (this.subStateMap.ContainsKey(stateId))
            {
                return this.subStateMap[stateId];
            }
            return null;
        }

        int IWorkflowCoreRuntime.GetNewContextActivityId()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            return this.GetNewContextId();
        }

        object IWorkflowCoreRuntime.GetService(Activity activity, Type serviceType)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            if (serviceType == typeof(IWorkflowCoreRuntime))
            {
                return this;
            }
            if (serviceType == typeof(System.Workflow.Runtime.WorkflowRuntime))
            {
                return null;
            }
            if (serviceType == typeof(WorkflowQueuingService))
            {
                WorkflowQueuingService queuingService = ServiceEnvironment.QueuingService;
                if (queuingService == null)
                {
                    queuingService = this.qService;
                }
                queuingService.CallingActivity = ContextActivityUtils.ContextActivity(activity);
                return queuingService;
            }
            if (serviceType == typeof(IWorkflowDebuggerService))
            {
                return this._workflowDebuggerService;
            }
            return this.WorkflowRuntime.GetService(serviceType);
        }

        Activity IWorkflowCoreRuntime.LoadContextActivity(ActivityExecutionContextInfo contextInfo, Activity outerActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            Activity activity = null;
            if (this.completedContextActivities.Contains(contextInfo))
            {
                activity = (Activity) this.completedContextActivities[contextInfo];
                this.completedContextActivities.Remove(contextInfo);
                if (activity.Parent != outerActivity.Parent)
                {
                    activity.parent = outerActivity.Parent;
                }
                return activity;
            }
            using (new System.Workflow.Runtime.RuntimeEnvironment(this.WorkflowRuntime))
            {
                activity = this.WorkflowRuntime.WorkflowPersistenceService.LoadCompletedContextActivity(contextInfo.ContextGuid, outerActivity);
                if (activity == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.LoadContextActivityFailed);
                }
            }
            return activity;
        }

        void IWorkflowCoreRuntime.OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.OnAfterDynamicChange(updateSucceeded, changes);
        }

        bool IWorkflowCoreRuntime.OnBeforeDynamicChange(IList<WorkflowChangeAction> changes)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            return this.OnBeforeDynamicChange(changes);
        }

        void IWorkflowCoreRuntime.PersistInstanceState(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            bool flag = false;
            if (activity.UserData.Contains(typeof(PersistOnCloseAttribute)))
            {
                flag = (bool) activity.UserData[typeof(PersistOnCloseAttribute)];
            }
            else
            {
                object[] customAttributes = activity.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    flag = true;
                }
            }
            if (flag && (this.WorkflowRuntime.GetService<WorkflowPersistenceService>() == null))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceServiceWithPersistOnClose, new object[] { activity.Name }));
            }
            this.ScheduleDelayedItems(activity);
            bool unlock = activity.Parent == null;
            bool needsCompensation = false;
            this.Persist(activity, unlock, needsCompensation);
        }

        void IWorkflowCoreRuntime.RaiseActivityExecuting(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.FireActivityExecuting(this, activity);
        }

        void IWorkflowCoreRuntime.RaiseException(Exception e, Activity activity, string responsibleActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.ExceptionOccured(e, activity, responsibleActivity);
        }

        void IWorkflowCoreRuntime.RaiseHandlerInvoked()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.FireWorkflowExecutionEvent(this, WorkflowEventInternal.HandlerInvoked);
        }

        void IWorkflowCoreRuntime.RaiseHandlerInvoking(Delegate handlerDelegate)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.FireWorkflowHandlerInvokingEvent(this, WorkflowEventInternal.HandlerInvoking, handlerDelegate);
        }

        void IWorkflowCoreRuntime.RegisterContextActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.RegisterDynamicActivity(activity, false);
        }

        void IWorkflowCoreRuntime.RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendInfo)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.WorkflowStateRollbackService.RequestRevertToCheckpointState(currentActivity, callbackHandler, callbackData, suspendOnRevert, suspendInfo);
        }

        bool IWorkflowCoreRuntime.Resume()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            return this.ResumeOnIdle(false);
        }

        void IWorkflowCoreRuntime.SaveContextActivity(Activity contextActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.completedContextActivities.Add((ActivityExecutionContextInfo) contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty), contextActivity);
        }

        void IWorkflowCoreRuntime.ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            if (!queueInTransaction)
            {
                this.Scheduler.ScheduleItem(item, isInAtomicTransaction, transacted);
            }
            else
            {
                this.AddItemToBeScheduledLater(this.CurrentActivity, item);
            }
        }

        Guid IWorkflowCoreRuntime.StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            Guid empty = Guid.Empty;
            System.Workflow.Runtime.WorkflowInstance instance = this.WorkflowRuntime.InternalCreateWorkflow(new CreationContext(workflowType, this, this.CurrentActivity.QualifiedName, namedArgumentValues), Guid.NewGuid());
            if (instance != null)
            {
                empty = instance.InstanceId;
                instance.Start();
            }
            return empty;
        }

        bool IWorkflowCoreRuntime.SuspendInstance(string suspendDescription)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            return this.SuspendOnIdle(suspendDescription);
        }

        void IWorkflowCoreRuntime.TerminateInstance(Exception e)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.ThrownException = e;
            this.TerminateOnIdle(GetNestedExceptionMessage(e));
        }

        void IWorkflowCoreRuntime.Track(string key, object args)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.Track(this.CurrentActivity, key, args);
        }

        void IWorkflowCoreRuntime.UnregisterContextActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
            {
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            }
            this.UnregisterDynamicActivity(activity);
        }

        internal void Terminate(string error)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor::Terminate : Got a terminate request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                using (new ScheduleWork(this, true))
                {
                    using (this._executorLock.Enter())
                    {
                        this.Scheduler.AbortOrTerminateRequested = true;
                        this.Scheduler.CanRun = false;
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                {
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                                }
                                this.TerminateOnIdle(error);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Terminate attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
        }

        internal bool TerminateOnIdle(string error)
        {
            if (!this.IsInstanceValid)
            {
                return false;
            }
            this.Scheduler.CanRun = false;
            try
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Terminating instance {0}", new object[] { this.InstanceIdString });
                if (this.ThrownException != null)
                {
                    this.FireWorkflowTerminating(this.ThrownException);
                }
                else
                {
                    this.FireWorkflowTerminating(error);
                }
                this.stateChangedSincePersistence = true;
                System.Workflow.Runtime.WorkflowStatus workflowStatus = this.WorkflowStatus;
                this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, error);
                this.WorkflowStatus = System.Workflow.Runtime.WorkflowStatus.Terminated;
                using (this._msgDeliveryLock.Enter())
                {
                    this.TimerQueue.SuspendDelivery();
                    this.rootActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Canceled);
                    try
                    {
                        this.Persist(this.rootActivity, true, false);
                    }
                    catch (Exception exception)
                    {
                        this.WorkflowStatus = workflowStatus;
                        this.rootActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.None);
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persistence attempt at instance '{0}' termination threw an exception. Aborting the instance. The termination event would be raised. The instance would execute from the last persisted point whenever started by the host explicitly. Exception:{1}\n{2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                        this.AbortOnIdle();
                        return false;
                    }
                    this.qService.MoveAllMessagesToPendingQueue();
                    if (this.ThrownException != null)
                    {
                        this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Terminated, this.ThrownException);
                    }
                    else
                    {
                        this.FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Terminated, error);
                    }
                    this.IsInstanceValid = false;
                }
                if (this.currentAtomicActivity != null)
                {
                    this.atomicActivityEvent.Set();
                    this.atomicActivityEvent.Close();
                }
            }
            catch (Exception)
            {
                if ((this.rootActivity == this.CurrentActivity) && (this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
                {
                    using (this._msgDeliveryLock.Enter())
                    {
                        this.AbortOnIdle();
                        return false;
                    }
                }
                this.Scheduler.CanRun = true;
                throw;
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void Track(Activity activity, string key, object args)
        {
            this.FireUserTrackPoint(activity, key, args);
        }

        internal bool TryUnload()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a TryUnload request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                if (!this.IsInstanceValid)
                {
                    return false;
                }
                if (this.WorkflowRuntime.WorkflowPersistenceService == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, new object[] { this.InstanceId });
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, message);
                    throw new InvalidOperationException(message);
                }
                using (new ScheduleWork(this, true))
                {
                    if (this._executorLock.TryEnter())
                    {
                        try
                        {
                            if (this._schedulerLock.TryEnter())
                            {
                                try
                                {
                                    if (this._msgDeliveryLock.TryEnter())
                                    {
                                        using (new ServiceEnvironment(this.rootActivity))
                                        {
                                            try
                                            {
                                                if (this.IsInstanceValid)
                                                {
                                                    this.ProcessQueuedEvents();
                                                    if (this.IsUnloadableNow)
                                                    {
                                                        return this.PerformUnloading(false);
                                                    }
                                                }
                                                return false;
                                            }
                                            finally
                                            {
                                                this._msgDeliveryLock.Exit();
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    SchedulerLockGuard.Exit(this._schedulerLock, this);
                                }
                            }
                        }
                        finally
                        {
                            this._executorLock.Exit();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: TryUnloading attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
            return false;
        }

        internal void Unload()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got an unload request for instance {0}", new object[] { this.InstanceIdString });
            try
            {
                using (new ScheduleWork(this, true))
                {
                    using (this._executorLock.Enter())
                    {
                        if (this.WorkflowRuntime.WorkflowPersistenceService == null)
                        {
                            string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, new object[] { this.InstanceId });
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, message);
                            throw new InvalidOperationException(message);
                        }
                        this.Scheduler.CanRun = false;
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                {
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                                }
                                if (this.currentAtomicActivity != null)
                                {
                                    this.Scheduler.CanRun = true;
                                    throw new ExecutorLocksHeldException(this.atomicActivityEvent);
                                }
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Calling PerformUnloading(false) on instance {0} hc {1}", new object[] { this.InstanceIdString, this.GetHashCode() });
                                this.PerformUnloading(false);
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, this.InstanceId + ": Returning from PerformUnloading(false): IsInstanceValue: " + this.IsInstanceValid);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Unload attempt on instance '{0}' threw an exception '{1}' at {2}", new object[] { this.InstanceIdString, exception.Message, exception.StackTrace });
                throw;
            }
        }

        internal void UnregisterDynamicActivity(Activity dynamicActivity)
        {
            int key = ContextActivityUtils.ContextId(dynamicActivity);
            this.subStateMap.Remove(key);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Removing context {0}:{1}", new object[] { key, dynamicActivity.QualifiedName });
            dynamicActivity.OnActivityExecutionContextUnload(this);
        }

        internal string AdditionalInformation
        {
            get
            {
                return (string) this.rootActivity.GetValue(SuspendOrTerminateInfoProperty);
            }
        }

        public WorkBatchCollection BatchCollection
        {
            get
            {
                return this._resourceManager.BatchCollection;
            }
        }

        internal Hashtable CompletedContextActivities
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completedContextActivities;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.completedContextActivities = value;
            }
        }

        internal Activity CurrentActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._lastExecutingActivity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._lastExecutingActivity = value;
            }
        }

        internal List<SchedulerLockGuardInfo> EventsToFireList
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventsToFireList;
            }
        }

        internal InstanceLock ExecutorLock
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._executorLock;
            }
        }

        internal Guid ID
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.InstanceId;
            }
        }

        internal Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowInstanceId;
            }
            private set
            {
                this.workflowInstanceId = value;
            }
        }

        internal string InstanceIdString
        {
            get
            {
                if (this.workflowIdString == null)
                {
                    this.workflowIdString = this.InstanceId.ToString();
                }
                return this.workflowIdString;
            }
        }

        internal bool IsIdle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isInstanceIdle;
            }
            set
            {
                using (InstanceLock.InstanceLockGuard guard = this.MessageDeliveryLock.Enter())
                {
                    try
                    {
                        this.isInstanceIdle = value;
                        this.RootActivity.SetValue(IsIdleProperty, value);
                    }
                    finally
                    {
                        if (this.IsIdle)
                        {
                            guard.Pulse();
                        }
                    }
                }
            }
        }

        internal bool IsInstanceValid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._isInstanceValid;
            }
            set
            {
                if (!value)
                {
                    this.ResourceManager.ClearAllBatchedWork();
                }
                this._isInstanceValid = value;
            }
        }

        internal bool IsUnloadableNow
        {
            get
            {
                if (this.currentAtomicActivity != null)
                {
                    return false;
                }
                if (!this.Scheduler.IsStalledNow)
                {
                    return (this.WorkflowStatus == System.Workflow.Runtime.WorkflowStatus.Suspended);
                }
                return true;
            }
        }

        internal InstanceLock MessageDeliveryLock
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._msgDeliveryLock;
            }
        }

        internal VolatileResourceManager ResourceManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._resourceManager;
            }
        }

        internal Activity RootActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootActivity;
            }
        }

        internal System.Workflow.Runtime.Scheduler Scheduler
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.schedulingContext;
            }
        }

        Activity IWorkflowCoreRuntime.CurrentActivity
        {
            get
            {
                if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                {
                    throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
                }
                return this.CurrentActivity;
            }
        }

        Activity IWorkflowCoreRuntime.CurrentAtomicActivity
        {
            get
            {
                return this.currentAtomicActivity;
            }
        }

        Guid IWorkflowCoreRuntime.InstanceID
        {
            get
            {
                if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                {
                    throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
                }
                return this.InstanceId;
            }
        }

        bool IWorkflowCoreRuntime.IsDynamicallyUpdated
        {
            get
            {
                return (this.WorkflowDefinition.GetValue(WorkflowChanges.WorkflowChangeActionsProperty) != null);
            }
        }

        WaitCallback IWorkflowCoreRuntime.ProcessTimersCallback
        {
            get
            {
                return new WaitCallback(this.WorkflowInstance.ProcessTimers);
            }
        }

        Activity IWorkflowCoreRuntime.RootActivity
        {
            get
            {
                return this.rootActivity;
            }
        }

        internal Exception ThrownException
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.thrownException;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.thrownException = value;
            }
        }

        internal TimerEventSubscriptionCollection TimerQueue
        {
            get
            {
                if (this._timerQueue == null)
                {
                    this._timerQueue = (TimerEventSubscriptionCollection) this.rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
                }
                return this._timerQueue;
            }
            private set
            {
                this._timerQueue = value;
                this.rootActivity.SetValue(TimerEventSubscriptionCollection.TimerCollectionProperty, this._timerQueue);
            }
        }

        internal System.Workflow.Runtime.TrackingCallingState TrackingCallingState
        {
            get
            {
                return (System.Workflow.Runtime.TrackingCallingState) this.rootActivity.GetValue(TrackingCallingStateProperty);
            }
        }

        internal Activity WorkflowDefinition
        {
            get
            {
                return this._workflowDefinition;
            }
        }

        internal System.Workflow.Runtime.WorkflowInstance WorkflowInstance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowInstance;
            }
        }

        internal System.Workflow.Runtime.WorkflowRuntime WorkflowRuntime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._runtime;
            }
        }

        internal System.Workflow.Runtime.WorkflowStateRollbackService WorkflowStateRollbackService
        {
            get
            {
                if (this.workflowStateRollbackService == null)
                {
                    this.workflowStateRollbackService = new System.Workflow.Runtime.WorkflowStateRollbackService(this);
                }
                return this.workflowStateRollbackService;
            }
        }

        internal System.Workflow.Runtime.WorkflowStatus WorkflowStatus
        {
            get
            {
                return (System.Workflow.Runtime.WorkflowStatus) this.rootActivity.GetValue(WorkflowStatusProperty);
            }
            private set
            {
                this.rootActivity.SetValue(WorkflowStatusProperty, value);
            }
        }

        internal class ActivityExecutingEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private System.Workflow.ComponentModel.Activity _activity;

            internal ActivityExecutingEventArgs(System.Workflow.ComponentModel.Activity activity)
            {
                this._activity = activity;
                base._eventType = WorkflowEventInternal.ActivityExecuting;
            }

            internal System.Workflow.ComponentModel.Activity Activity
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._activity;
                }
            }
        }

        internal class ActivityStatusChangeEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private System.Workflow.ComponentModel.Activity _activity;

            internal ActivityStatusChangeEventArgs(System.Workflow.ComponentModel.Activity activity)
            {
                this._activity = activity;
                base._eventType = WorkflowEventInternal.ActivityStatusChange;
            }

            internal System.Workflow.ComponentModel.Activity Activity
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._activity;
                }
            }
        }

        internal class DynamicUpdateEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private IList<WorkflowChangeAction> _changeActions = new List<WorkflowChangeAction>();

            internal DynamicUpdateEventArgs(IList<WorkflowChangeAction> changeActions, WorkflowEventInternal eventType)
            {
                this._changeActions = changeActions;
                base._eventType = eventType;
            }

            internal IList<WorkflowChangeAction> ChangeActions
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._changeActions;
                }
            }
        }

        private class ResetCurrentActivity : IDisposable
        {
            private Activity oldCurrentActivity;
            private WorkflowExecutor workflowExecutor;

            internal ResetCurrentActivity(WorkflowExecutor workflowExecutor, Activity oldCurrentActivity)
            {
                this.workflowExecutor = workflowExecutor;
                this.oldCurrentActivity = oldCurrentActivity;
            }

            void IDisposable.Dispose()
            {
                this.workflowExecutor.CurrentActivity = this.oldCurrentActivity;
            }
        }

        internal class UserTrackPointEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private System.Workflow.ComponentModel.Activity _activity;
            private object _args;
            private string _key;

            internal UserTrackPointEventArgs(System.Workflow.ComponentModel.Activity activity, string key, object args)
            {
                if (activity == null)
                {
                    throw new ArgumentNullException("activity");
                }
                this._activity = activity;
                this._args = args;
                base._eventType = WorkflowEventInternal.UserTrackPoint;
                this._key = key;
            }

            internal System.Workflow.ComponentModel.Activity Activity
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._activity;
                }
            }

            internal object Args
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._args;
                }
            }

            internal string Key
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._key;
                }
            }
        }

        internal class WorkflowExecutionEventArgs : EventArgs
        {
            protected WorkflowEventInternal _eventType;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected WorkflowExecutionEventArgs()
            {
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal WorkflowExecutionEventArgs(WorkflowEventInternal eventType)
            {
                this._eventType = eventType;
            }

            internal WorkflowEventInternal EventType
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._eventType;
                }
            }
        }

        internal class WorkflowExecutionExceptionEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private Guid _contextGuid;
            private string _currentPath;
            private System.Exception _exception;
            private string _originalPath;
            private Guid _parentContextGuid;

            internal WorkflowExecutionExceptionEventArgs(System.Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception");
                }
                this._exception = exception;
                this._currentPath = currentPath;
                this._originalPath = originalPath;
                base._eventType = WorkflowEventInternal.Exception;
                this._contextGuid = contextGuid;
                this._parentContextGuid = parentContextGuid;
            }

            internal Guid ContextGuid
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._contextGuid;
                }
            }

            internal string CurrentPath
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._currentPath;
                }
            }

            internal System.Exception Exception
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._exception;
                }
            }

            internal string OriginalPath
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._originalPath;
                }
            }

            internal Guid ParentContextGuid
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._parentContextGuid;
                }
            }
        }

        internal sealed class WorkflowExecutionSuspendedEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private string _error;

            internal WorkflowExecutionSuspendedEventArgs(string error)
            {
                base._eventType = WorkflowEventInternal.Suspended;
                this._error = error;
            }

            internal string Error
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._error;
                }
            }
        }

        internal sealed class WorkflowExecutionSuspendingEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private string _error;

            internal WorkflowExecutionSuspendingEventArgs(string error)
            {
                base._eventType = WorkflowEventInternal.Suspending;
                this._error = error;
            }

            internal string Error
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._error;
                }
            }
        }

        internal sealed class WorkflowExecutionTerminatedEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private string _error;
            private System.Exception _exception;

            internal WorkflowExecutionTerminatedEventArgs(System.Exception exception)
            {
                this._exception = exception;
                base._eventType = WorkflowEventInternal.Terminated;
            }

            internal WorkflowExecutionTerminatedEventArgs(string error)
            {
                this._error = error;
                base._eventType = WorkflowEventInternal.Terminated;
            }

            internal string Error
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._error;
                }
            }

            internal System.Exception Exception
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._exception;
                }
            }
        }

        internal sealed class WorkflowExecutionTerminatingEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private string _error;
            private System.Exception _exception;

            internal WorkflowExecutionTerminatingEventArgs(System.Exception exception)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception");
                }
                this._exception = exception;
                base._eventType = WorkflowEventInternal.Terminating;
            }

            internal WorkflowExecutionTerminatingEventArgs(string error)
            {
                this._error = error;
                base._eventType = WorkflowEventInternal.Terminating;
            }

            internal string Error
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._error;
                }
            }

            internal System.Exception Exception
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._exception;
                }
            }
        }

        internal class WorkflowHandlerInvokingEventArgs : WorkflowExecutor.WorkflowExecutionEventArgs
        {
            private Delegate _delegateHandler;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal WorkflowHandlerInvokingEventArgs(WorkflowEventInternal eventType, Delegate delegateHandler) : base(eventType)
            {
                this._delegateHandler = delegateHandler;
            }

            internal Delegate DelegateMethod
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this._delegateHandler;
                }
            }
        }
    }
}

