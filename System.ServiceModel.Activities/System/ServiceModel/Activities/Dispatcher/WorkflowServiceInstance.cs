namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    internal class WorkflowServiceInstance : WorkflowInstance
    {
        private bool abortingExtensions;
        private TimeSpan acquireLockTimeout;
        private ThreadNeutralSemaphore acquireReferenceSemaphore;
        private int activeOperations;
        private object activeOperationsLock;
        private System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager bufferedReceiveManager;
        private List<WaitForCanPersistAsyncResult> checkCanPersistWaiters;
        private ActivityInstanceState completionState;
        private WorkflowCreationContext creationContext;
        private bool creationContextAborted;
        private static ReadOnlyCollection<BookmarkInfo> emptyBookmarkInfoCollection = new ReadOnlyCollection<BookmarkInfo>(new List<BookmarkInfo>());
        private WorkflowExecutionLock executorLock;
        private static AsyncCallback handleEndReleaseInstance;
        private int handlerThreadId;
        private bool hasDataToPersist;
        private bool hasIncrementedBusyCount;
        private bool hasPersistedDeleted;
        private bool hasRaisedCompleted;
        private List<AsyncWaitHandle> idleWaiters;
        private Guid instanceId;
        private bool isInHandler;
        private bool isInTransaction;
        private bool isRunnable;
        private bool isTransactedCancelled;
        private bool isWorkflowServiceInstanceReady;
        private static FastAsyncCallback lockAcquiredAsyncCallback = new FastAsyncCallback(WorkflowServiceInstance.OnLockAcquiredAsync);
        private List<AsyncWaitHandle> nextIdleWaiters;
        private int pendingOperationCount;
        private Dictionary<string, List<PendingOperationAsyncResult>> pendingOperations;
        private List<WorkflowOperationContext> pendingRequests;
        private PersistenceContext persistenceContext;
        private PersistencePipeline persistencePipelineInUse;
        private TimeSpan persistTimeout;
        private int referenceCount;
        private WorkflowServiceHost serviceHost;
        private State state;
        private Exception terminationException;
        private object thisLock;
        private static AsyncCallback trackCompleteDoneCallback;
        private static AsyncCallback trackIdleDoneCallback;
        private TimeSpan trackTimeout;
        private static AsyncCallback trackUnhandledExceptionDoneCallback;
        private TransactionContext transactionContext;
        private UnhandledExceptionPolicyHelper unhandledExceptionPolicy;
        private UnloadInstancePolicyHelper unloadInstancePolicy;
        private IDictionary<string, object> workflowOutputs;
        private AsyncWaitHandle workflowServiceInstanceReadyWaitHandle;

        private WorkflowServiceInstance(WorkflowServiceHost serviceHost) : base(serviceHost.Activity)
        {
        }

        private WorkflowServiceInstance(Activity workflowDefinition, Guid instanceId, WorkflowServiceHost serviceHost, PersistenceContext persistenceContext) : base(workflowDefinition)
        {
            this.serviceHost = serviceHost;
            this.instanceId = instanceId;
            this.persistTimeout = serviceHost.PersistTimeout;
            this.trackTimeout = serviceHost.TrackTimeout;
            this.bufferedReceiveManager = serviceHost.Extensions.Find<System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager>();
            if (persistenceContext != null)
            {
                this.persistenceContext = persistenceContext;
                this.persistenceContext.Closed += new EventHandler(this.OnPersistenceContextClosed);
            }
            this.thisLock = new object();
            this.pendingRequests = new List<WorkflowOperationContext>();
            this.executorLock = new WorkflowExecutionLock(this);
            this.activeOperationsLock = new object();
            this.acquireReferenceSemaphore = new ThreadNeutralSemaphore(1);
            this.acquireLockTimeout = TimeSpan.MaxValue;
            this.referenceCount = 1;
            this.TryAddReference();
        }

        private void AbortExtensions()
        {
            this.abortingExtensions = true;
            Thread.MemoryBarrier();
            if (this.persistenceContext != null)
            {
                this.persistenceContext.Abort();
            }
            PersistencePipeline persistencePipelineInUse = this.persistencePipelineInUse;
            if (persistencePipelineInUse != null)
            {
                persistencePipelineInUse.Abort();
            }
            if (this.hasRaisedCompleted && (this.bufferedReceiveManager != null))
            {
                this.bufferedReceiveManager.AbandonBufferedReceives(this.persistenceContext.AssociatedKeys);
            }
        }

        private void AbortInstance(Exception reason, bool isWorkflowThread)
        {
            this.AbortInstance(reason, isWorkflowThread, true);
        }

        private void AbortInstance(Exception reason, bool isWorkflowThread, bool shouldTrackAbort)
        {
            bool flag = false;
            if (shouldTrackAbort)
            {
                System.ServiceModel.Activities.FxTrace.Exception.AsWarning(reason);
            }
            this.FaultPendingRequests(reason);
            this.AbortExtensions();
            try
            {
                if ((this.creationContext != null) && !this.creationContextAborted)
                {
                    this.creationContextAborted = true;
                    this.creationContext.OnAbort();
                }
                if (isWorkflowThread)
                {
                    flag = true;
                    if (this.ValidateStateForAbort())
                    {
                        this.state = State.Aborted;
                        if (shouldTrackAbort)
                        {
                            base.Controller.Abort(reason);
                        }
                        else
                        {
                            base.Controller.Abort();
                        }
                        this.DecrementBusyCount();
                        this.ScheduleTrackAndRaiseAborted(reason);
                    }
                }
                else
                {
                    bool ownsLock = false;
                    try
                    {
                        if (this.AcquireLockAsync(this.acquireLockTimeout, true, false, ref ownsLock, new FastAsyncCallback(this.OnAbortLockAcquired), new AbortInstanceState(reason, shouldTrackAbort)))
                        {
                            flag = true;
                            if (this.ValidateStateForAbort())
                            {
                                this.state = State.Aborted;
                                if (shouldTrackAbort)
                                {
                                    base.Controller.Abort(reason);
                                }
                                else
                                {
                                    base.Controller.Abort();
                                }
                                this.DecrementBusyCount();
                                this.ScheduleTrackAndRaiseAborted(reason);
                            }
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.ReleaseLock(ref ownsLock);
                        }
                    }
                }
            }
            finally
            {
                this.serviceHost.FaultServiceHostIfNecessary(reason);
            }
        }

        private void AcquireLock(TimeSpan timeout, ref bool ownsLock)
        {
            if (!this.IsHandlerThread && !this.executorLock.TryEnter(ref ownsLock))
            {
                bool flag = false;
                object token = null;
                try
                {
                    lock (this.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.activeOperations++;
                            flag = true;
                        }
                        base.Controller.RequestPause();
                        this.executorLock.SetupWaiter(ref token);
                    }
                    this.executorLock.Enter(timeout, ref token, ref ownsLock);
                }
                finally
                {
                    if (flag)
                    {
                        lock (this.activeOperationsLock)
                        {
                            this.activeOperations--;
                        }
                    }
                    this.executorLock.CleanupWaiter(token, ref ownsLock);
                }
            }
        }

        private bool AcquireLockAsync(TimeSpan timeout, ref bool ownsLock, FastAsyncCallback callback, object state)
        {
            return this.AcquireLockAsync(timeout, false, false, ref ownsLock, callback, state);
        }

        private bool AcquireLockAsync(TimeSpan timeout, bool isAbortPriority, bool skipPause, ref bool ownsLock, FastAsyncCallback callback, object state)
        {
            if (!this.executorLock.TryEnter(ref ownsLock))
            {
                bool flag = false;
                bool flag2 = true;
                object token = null;
                try
                {
                    lock (this.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.activeOperations++;
                            flag = true;
                        }
                        if (!skipPause)
                        {
                            base.Controller.RequestPause();
                        }
                        this.executorLock.SetupWaiter(isAbortPriority, ref token);
                    }
                    return this.executorLock.EnterAsync(timeout, ref token, ref ownsLock, lockAcquiredAsyncCallback, new AcquireLockAsyncData(this, callback, state));
                }
                finally
                {
                    if (flag && flag2)
                    {
                        lock (this.activeOperationsLock)
                        {
                            this.activeOperations--;
                        }
                    }
                    this.executorLock.CleanupWaiter(token, ref ownsLock);
                }
            }
            return true;
        }

        private void AddCheckCanPersistWaiter(WaitForCanPersistAsyncResult result)
        {
            if (this.checkCanPersistWaiters == null)
            {
                this.checkCanPersistWaiters = new List<WaitForCanPersistAsyncResult>();
            }
            this.checkCanPersistWaiters.Add(result);
        }

        private bool AreBookmarksInvalid(out BookmarkResumptionResult result)
        {
            if (this.hasRaisedCompleted)
            {
                result = BookmarkResumptionResult.NotFound;
                return true;
            }
            if (((this.state == State.Unloaded) || (this.state == State.Aborted)) || (this.state == State.Suspended))
            {
                result = BookmarkResumptionResult.NotReady;
                return true;
            }
            result = BookmarkResumptionResult.Success;
            return false;
        }

        public IAsyncResult BeginAbandon(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginAbandon(reason, true, timeout, callback, state);
        }

        private IAsyncResult BeginAbandon(Exception reason, bool shouldTrackAbort, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return AbandonAsyncResult.Create(this, reason, shouldTrackAbort, timeout, callback, state);
        }

        private IAsyncResult BeginAbandonAndSuspend(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return AbandonAndSuspendAsyncResult.Create(this, reason, timeout, callback, state);
        }

        private IAsyncResult BeginAcquireLockOnIdle(TimeSpan timeout, ref bool ownsLock, AsyncCallback callback, object state)
        {
            return new AcquireLockOnIdleAsyncResult(this, timeout, ref ownsLock, callback, state);
        }

        public IAsyncResult BeginAssociateInfrastructureKeys(ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new AssociateKeysAsyncResult(this, associatedKeys, transaction, timeout, callback, state);
        }

        public IAsyncResult BeginCancel(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return CancelAsyncResult.Create(this, transaction, timeout, callback, state);
        }

        public IAsyncResult BeginPersist(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginPersist(false, timeout, callback, state);
        }

        private IAsyncResult BeginPersist(bool isTry, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new UnloadOrPersistAsyncResult(this, (base.Controller.State == WorkflowInstanceState.Complete) ? PersistenceOperation.Delete : PersistenceOperation.Save, false, isTry, timeout, callback, state);
        }

        public IAsyncResult BeginReleaseInstance(bool isTryUnload, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReleaseInstanceAsyncResult(this, isTryUnload, timeout, callback, state);
        }

        public IAsyncResult BeginResumeProtocolBookmark(Bookmark bookmark, BookmarkScope bookmarkScope, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            object bookmarkValue = value;
            WorkflowOperationContext item = value as WorkflowOperationContext;
            if (item != null)
            {
                if (!item.HasResponse)
                {
                    lock (this.thisLock)
                    {
                        this.pendingRequests.Add(item);
                    }
                }
                bookmarkValue = item.BookmarkValue;
            }
            return new ResumeProtocolBookmarkAsyncResult(this, bookmark, bookmarkValue, bookmarkScope, true, timeout, callback, state);
        }

        public IAsyncResult BeginRun(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return RunAsyncResult.Create(this, transaction, timeout, callback, state);
        }

        public IAsyncResult BeginSuspend(bool isUnlocked, string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return SuspendAsyncResult.Create(this, isUnlocked, reason, transaction, timeout, callback, state);
        }

        private IAsyncResult BeginTerminate(Exception reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return TerminateAsyncResult.Create(this, reason, transaction, timeout, callback, state);
        }

        public IAsyncResult BeginTerminate(string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationExecutionFault fault = OperationExecutionFault.CreateTerminatedFault(reason);
            return this.BeginTerminate(new FaultException(fault.Reason, fault.Code), transaction, timeout, callback, state);
        }

        public IAsyncResult BeginTryAcquireReference(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryAcquireReferenceAsyncResult(this, timeout, callback, state);
        }

        public IAsyncResult BeginUnsuspend(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return UnsuspendAsyncResult.Create(this, transaction, timeout, callback, state);
        }

        private IAsyncResult BeginWaitForCanPersist(ref bool ownsLock, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForCanPersistAsyncResult(this, ref ownsLock, timeout, callback, state);
        }

        public IAsyncResult BeginWaitForPendingOperations(string sessionId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            PendingOperationAsyncResult item = null;
            lock (this.thisLock)
            {
                List<PendingOperationAsyncResult> list;
                if (this.pendingOperations == null)
                {
                    this.pendingOperations = new Dictionary<string, List<PendingOperationAsyncResult>>();
                }
                if (!this.pendingOperations.TryGetValue(sessionId, out list))
                {
                    list = new List<PendingOperationAsyncResult>();
                    this.pendingOperations.Add(sessionId, list);
                }
                bool isFirstRequest = list.Count == 0;
                item = new PendingOperationAsyncResult(isFirstRequest, timeout, callback, state);
                list.Add(item);
                this.pendingOperationCount++;
            }
            item.Start();
            return item;
        }

        private bool CleanupIdleWaiter(AsyncWaitHandle idleEvent, Exception waitException, ref bool ownsLock)
        {
            lock (this.activeOperationsLock)
            {
                if (!this.idleWaiters.Remove(idleEvent) && (waitException is TimeoutException))
                {
                    ownsLock = true;
                    return false;
                }
            }
            return true;
        }

        private void CompletePendingOperations()
        {
            lock (this.thisLock)
            {
                if (this.pendingOperations != null)
                {
                    foreach (List<PendingOperationAsyncResult> list in this.pendingOperations.Values)
                    {
                        foreach (PendingOperationAsyncResult result in list)
                        {
                            result.Unblock();
                        }
                    }
                }
                this.pendingOperations = null;
                this.pendingOperationCount = 0;
            }
        }

        private void DecrementBusyCount()
        {
            if (this.hasIncrementedBusyCount)
            {
                AspNetEnvironment.Current.DecrementBusyCount();
                if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceDecrementBusyCount(System.ServiceModel.Activities.SR.BusyCountTraceFormatString(this.Id));
                }
                this.hasIncrementedBusyCount = false;
            }
        }

        private void Dispose()
        {
            base.DisposeExtensions();
            if (this.hasRaisedCompleted && (this.bufferedReceiveManager != null))
            {
                this.bufferedReceiveManager.AbandonBufferedReceives(this.persistenceContext.AssociatedKeys);
            }
        }

        public void EndAbandon(IAsyncResult result)
        {
            AbandonAsyncResult.End(result);
        }

        private void EndAbandonAndSuspend(IAsyncResult result)
        {
            AbandonAndSuspendAsyncResult.End(result);
        }

        private void EndAcquireLockOnIdle(IAsyncResult result)
        {
            AcquireLockOnIdleAsyncResult.End(result);
        }

        private void EndAcquireLockOnIdle(IAsyncResult result, ref bool ownsLock)
        {
            AcquireLockOnIdleAsyncResult.End(result, ref ownsLock);
        }

        public void EndAssociateInfrastructureKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        public void EndCancel(IAsyncResult result)
        {
            CancelAsyncResult.End(result);
        }

        public void EndPersist(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        public void EndReleaseInstance(IAsyncResult result)
        {
            ReleaseInstanceAsyncResult.End(result);
        }

        public BookmarkResumptionResult EndResumeProtocolBookmark(IAsyncResult result)
        {
            return ResumeProtocolBookmarkAsyncResult.End(result);
        }

        public void EndRun(IAsyncResult result)
        {
            RunAsyncResult.End(result);
        }

        public void EndSuspend(IAsyncResult result)
        {
            SuspendAsyncResult.End(result);
        }

        public void EndTerminate(IAsyncResult result)
        {
            TerminateAsyncResult.End(result);
        }

        public bool EndTryAcquireReference(IAsyncResult result)
        {
            return TryAcquireReferenceAsyncResult.End(result);
        }

        public void EndUnsuspend(IAsyncResult result)
        {
            UnsuspendAsyncResult.End(result);
        }

        private void EndWaitForCanPersist(IAsyncResult result, ref bool ownsLock)
        {
            WaitForCanPersistAsyncResult.End(result, ref ownsLock);
        }

        public void EndWaitForPendingOperations(IAsyncResult result)
        {
            PendingOperationAsyncResult.End(result);
        }

        private void FaultPendingRequests(Exception e)
        {
            WorkflowOperationContext[] contextArray = null;
            lock (this.thisLock)
            {
                if (this.pendingRequests.Count == 0)
                {
                    return;
                }
                contextArray = this.pendingRequests.ToArray();
                this.pendingRequests.Clear();
            }
            for (int i = 0; i < contextArray.Length; i++)
            {
                contextArray[i].SendFault(e);
            }
        }

        private Dictionary<XName, InstanceValue> GeneratePersistenceData()
        {
            Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>(10);
            dictionary[WorkflowNamespace.Bookmarks] = new InstanceValue(base.Controller.GetBookmarks(), InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            dictionary[WorkflowNamespace.LastUpdate] = new InstanceValue(DateTime.UtcNow, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            foreach (KeyValuePair<string, LocationInfo> pair in base.Controller.GetMappedVariables())
            {
                XName name = WorkflowNamespace.VariablesPath.GetName(pair.Key);
                dictionary[name] = new InstanceValue(pair.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }
            Fx.AssertAndThrow(base.Controller.State != WorkflowInstanceState.Aborted, "Cannot generate data for an aborted service instance.");
            if (base.Controller.State != WorkflowInstanceState.Complete)
            {
                dictionary[WorkflowNamespace.Workflow] = new InstanceValue(base.Controller.PrepareForSerialization());
                if (this.creationContext != null)
                {
                    dictionary[WorkflowServiceNamespace.CreationContext] = new InstanceValue(this.creationContext);
                }
                dictionary[WorkflowNamespace.Status] = new InstanceValue((base.Controller.State == WorkflowInstanceState.Idle) ? "Idle" : "Executing", InstanceValueOptions.WriteOnly);
                return dictionary;
            }
            dictionary[WorkflowNamespace.Workflow] = new InstanceValue(base.Controller.PrepareForSerialization(), InstanceValueOptions.Optional);
            this.GetCompletionState();
            if (this.completionState == ActivityInstanceState.Faulted)
            {
                dictionary[WorkflowNamespace.Status] = new InstanceValue("Faulted", InstanceValueOptions.WriteOnly);
                dictionary[WorkflowNamespace.Exception] = new InstanceValue(this.terminationException, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                return dictionary;
            }
            if (this.completionState == ActivityInstanceState.Closed)
            {
                dictionary[WorkflowNamespace.Status] = new InstanceValue("Closed", InstanceValueOptions.WriteOnly);
                if (this.workflowOutputs != null)
                {
                    foreach (KeyValuePair<string, object> pair2 in this.workflowOutputs)
                    {
                        XName introduced13 = WorkflowNamespace.OutputPath.GetName(pair2.Key);
                        dictionary[introduced13] = new InstanceValue(pair2.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                    }
                }
                return dictionary;
            }
            Fx.AssertAndThrow(this.completionState == ActivityInstanceState.Canceled, "Cannot be executing a service instance when WorkflowState was completed.");
            dictionary[WorkflowNamespace.Status] = new InstanceValue("Canceled", InstanceValueOptions.WriteOnly);
            return dictionary;
        }

        private void GetCompletionState()
        {
            this.completionState = base.Controller.GetCompletionState(out this.workflowOutputs, out this.terminationException);
        }

        private static void HandleEndReleaseInstance(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((WorkflowServiceInstance) result.AsyncState).OnReleaseInstance(result);
            }
        }

        private void IncrementBusyCount()
        {
            if (!this.hasIncrementedBusyCount)
            {
                AspNetEnvironment.Current.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(System.ServiceModel.Activities.SR.BusyCountTraceFormatString(this.Id));
                }
                this.hasIncrementedBusyCount = true;
            }
        }

        public static WorkflowServiceInstance InitializeInstance(PersistenceContext persistenceContext, Guid instanceId, Activity workflowDefinition, IDictionary<XName, InstanceValue> loadedObject, WorkflowCreationContext creationContext, SynchronizationContext synchronizationContext, WorkflowServiceHost serviceHost)
        {
            WorkflowServiceInstance instance = new WorkflowServiceInstance(workflowDefinition, instanceId, serviceHost, persistenceContext) {
                SynchronizationContext = synchronizationContext
            };
            instance.SetupExtensions(serviceHost.WorkflowExtensions);
            if (loadedObject != null)
            {
                InstanceValue value2;
                if (!loadedObject.TryGetValue(WorkflowNamespace.Workflow, out value2) || (value2.Value == null))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceException(System.ServiceModel.Activities.SR.WorkflowInstanceNotFoundInStore(instanceId)));
                }
                object deserializedRuntimeState = value2.Value;
                if (loadedObject.TryGetValue(WorkflowServiceNamespace.CreationContext, out value2))
                {
                    instance.creationContext = (WorkflowCreationContext) value2.Value;
                }
                if (persistenceContext.IsSuspended)
                {
                    instance.state = State.Suspended;
                }
                instance.Initialize(deserializedRuntimeState);
                return instance;
            }
            IList<Handle> workflowExecutionProperties = null;
            IDictionary<string, object> workflowArgumentValues = null;
            if (!(workflowDefinition is CorrelationScope))
            {
                workflowExecutionProperties = new List<Handle>(1) {
                    new CorrelationHandle()
                };
            }
            if (creationContext != null)
            {
                workflowArgumentValues = creationContext.RawWorkflowArguments;
                instance.creationContext = creationContext;
            }
            instance.Initialize(workflowArgumentValues, workflowExecutionProperties);
            return instance;
        }

        public static bool IsLoadTransactionRequired(WorkflowServiceHost host)
        {
            WorkflowServiceInstance instance = new WorkflowServiceInstance(host);
            instance.RegisterExtensionManager(host.WorkflowExtensions);
            return instance.GetExtensions<IPersistencePipelineModule>().Any<IPersistencePipelineModule>(module => module.IsLoadTransactionRequired);
        }

        private void MarkUnloaded()
        {
            this.state = State.Unloaded;
            if (base.Controller.State != WorkflowInstanceState.Complete)
            {
                base.Controller.Abort();
            }
            this.DecrementBusyCount();
        }

        private void NotifyCheckCanPersistWaiters(ref bool ownsLock)
        {
            if (((this.checkCanPersistWaiters != null) && (this.checkCanPersistWaiters.Count > 0)) && base.Controller.IsPersistable)
            {
                List<WaitForCanPersistAsyncResult> checkCanPersistWaiters = this.checkCanPersistWaiters;
                this.checkCanPersistWaiters = null;
                foreach (WaitForCanPersistAsyncResult result in checkCanPersistWaiters)
                {
                    result.SetEvent(ref ownsLock);
                }
            }
        }

        private bool NotifyNextIdleWaiter(ref bool ownsLock)
        {
            if (this.state != State.Active)
            {
                this.PrepareNextIdleWaiter();
            }
            if ((this.idleWaiters != null) && (this.idleWaiters.Count > 0))
            {
                AsyncWaitHandle handle = null;
                lock (this.activeOperationsLock)
                {
                    if (this.idleWaiters.Count > 0)
                    {
                        handle = this.idleWaiters[0];
                        this.idleWaiters.RemoveAt(0);
                    }
                }
                if (handle != null)
                {
                    handle.Set();
                    ownsLock = false;
                    return true;
                }
            }
            return false;
        }

        private void OnAbortLockAcquired(object state, Exception exception)
        {
            if (exception != null)
            {
                System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
            }
            else
            {
                bool ownsLock = true;
                bool flag2 = false;
                AbortInstanceState state2 = (AbortInstanceState) state;
                try
                {
                    if (this.ValidateStateForAbort())
                    {
                        flag2 = true;
                        this.state = State.Aborted;
                        if (state2.ShouldTrackAbort)
                        {
                            base.Controller.Abort(state2.Reason);
                        }
                        else
                        {
                            base.Controller.Abort();
                        }
                        this.DecrementBusyCount();
                    }
                }
                finally
                {
                    this.ReleaseLock(ref ownsLock);
                }
                if (flag2)
                {
                    this.TrackAndRaiseAborted(state2.Reason);
                }
            }
        }

        private void OnAbortTrackingComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                Exception asyncState = (Exception) result.AsyncState;
                try
                {
                    base.Controller.EndFlushTrackingRecords(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                }
                this.RaiseAborted();
            }
        }

        protected override IAsyncResult OnBeginAssociateKeys(ICollection<InstanceKey> keys, AsyncCallback callback, object state)
        {
            if (this.persistenceContext == null)
            {
                return new CompletedAsyncResult(callback, state);
            }
            return this.persistenceContext.BeginAssociateKeys(keys, this.persistTimeout, callback, state);
        }

        protected override IAsyncResult OnBeginFlushTrackingRecords(AsyncCallback callback, object state)
        {
            return base.Controller.BeginFlushTrackingRecords(this.trackTimeout, callback, state);
        }

        protected override IAsyncResult OnBeginPersist(AsyncCallback callback, object state)
        {
            return new UnloadOrPersistAsyncResult(this, PersistenceOperation.Save, true, false, TimeSpan.MaxValue, callback, state);
        }

        protected override IAsyncResult OnBeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ResumeProtocolBookmarkAsyncResult(this, bookmark, value, null, false, timeout, callback, state);
        }

        private void OnCompleted()
        {
            if (this.terminationException != null)
            {
                this.FaultPendingRequests(new FaultException(OperationExecutionFault.CreateTerminatedFault(System.ServiceModel.Activities.SR.WorkflowInstanceTerminated(this.Id))));
            }
            else
            {
                this.FaultPendingRequests(new FaultException(OperationExecutionFault.CreateCompletedFault(System.ServiceModel.Activities.SR.WorkflowInstanceCompleted(this.Id))));
            }
            if (handleEndReleaseInstance == null)
            {
                handleEndReleaseInstance = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.HandleEndReleaseInstance));
            }
            IAsyncResult result = this.BeginReleaseInstance(false, TimeSpan.MaxValue, handleEndReleaseInstance, this);
            if (result.CompletedSynchronously)
            {
                this.OnReleaseInstance(result);
            }
            this.CompletePendingOperations();
        }

        protected override void OnDisassociateKeys(ICollection<InstanceKey> keys)
        {
            if (this.persistenceContext != null)
            {
                this.persistenceContext.DisassociateKeys(keys);
            }
        }

        protected override void OnEndAssociateKeys(IAsyncResult result)
        {
            if (this.persistenceContext == null)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.persistenceContext.EndAssociateKeys(result);
            }
        }

        protected override void OnEndFlushTrackingRecords(IAsyncResult result)
        {
            base.Controller.EndFlushTrackingRecords(result);
        }

        protected override void OnEndPersist(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        protected override BookmarkResumptionResult OnEndResumeBookmark(IAsyncResult result)
        {
            return ResumeProtocolBookmarkAsyncResult.End(result);
        }

        private void OnIdle()
        {
            if (this.BufferedReceiveManager != null)
            {
                this.persistenceContext.Bookmarks = base.Controller.GetBookmarks();
                this.BufferedReceiveManager.Retry(this.persistenceContext.AssociatedKeys, this.persistenceContext.Bookmarks);
            }
        }

        private static void OnLockAcquiredAsync(object state, Exception asyncException)
        {
            AcquireLockAsyncData data = (AcquireLockAsyncData) state;
            lock (data.Instance.activeOperationsLock)
            {
                WorkflowServiceInstance instance = data.Instance;
                instance.activeOperations--;
            }
            data.Callback(data.State, asyncException);
        }

        protected override void OnNotifyPaused()
        {
            bool ownsLock = true;
            bool flag2 = false;
            try
            {
                if (this.ShouldRaiseComplete)
                {
                    this.PrepareNextIdleWaiter();
                    Exception reason = null;
                    try
                    {
                        this.hasRaisedCompleted = true;
                        this.state = State.Completed;
                        this.GetCompletionState();
                        if (base.Controller.HasPendingTrackingRecords)
                        {
                            IAsyncResult result = base.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackCompleteDoneCallback, this);
                            if (result.CompletedSynchronously)
                            {
                                base.Controller.EndFlushTrackingRecords(result);
                            }
                            else
                            {
                                flag2 = true;
                                return;
                            }
                        }
                        this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        try
                        {
                            this.isInHandler = true;
                            this.OnCompleted();
                        }
                        finally
                        {
                            this.isInHandler = false;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        reason = exception2;
                    }
                    if (reason != null)
                    {
                        this.AbortInstance(reason, true);
                    }
                }
                else if (base.Controller.State == WorkflowInstanceState.Aborted)
                {
                    Exception abortReason = base.Controller.GetAbortReason();
                    this.AbortInstance(abortReason, true);
                }
                else
                {
                    if (this.ShouldRaiseIdle)
                    {
                        this.PrepareNextIdleWaiter();
                        if (base.Controller.TrackingEnabled)
                        {
                            base.Controller.Track(new WorkflowInstanceRecord(this.Id, base.WorkflowDefinition.DisplayName, "Idle"));
                            IAsyncResult result2 = base.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackIdleDoneCallback, this);
                            if (result2.CompletedSynchronously)
                            {
                                base.Controller.EndFlushTrackingRecords(result2);
                            }
                            else
                            {
                                flag2 = true;
                                return;
                            }
                        }
                        this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        try
                        {
                            this.isInHandler = true;
                            this.OnIdle();
                            return;
                        }
                        finally
                        {
                            this.isInHandler = false;
                        }
                    }
                    this.NotifyCheckCanPersistWaiters(ref ownsLock);
                }
            }
            finally
            {
                if (!flag2)
                {
                    this.ReleaseLock(ref ownsLock);
                }
            }
        }

        protected override void OnNotifyUnhandledException(Exception exception, Activity exceptionSource, string exceptionSourceInstanceId)
        {
            bool ownsLock = true;
            bool flag2 = false;
            UnhandledExceptionAsyncData state = new UnhandledExceptionAsyncData(this, exception, exceptionSource);
            try
            {
                if (base.Controller.HasPendingTrackingRecords)
                {
                    IAsyncResult result = base.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackUnhandledExceptionDoneCallback, state);
                    if (result.CompletedSynchronously)
                    {
                        base.Controller.EndFlushTrackingRecords(result);
                    }
                    else
                    {
                        flag2 = true;
                        return;
                    }
                }
                this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                try
                {
                    this.isInHandler = true;
                    this.OnUnhandledException(state);
                }
                finally
                {
                    this.isInHandler = false;
                }
            }
            finally
            {
                if (!flag2)
                {
                    this.ReleaseLock(ref ownsLock);
                }
            }
        }

        private void OnPersistenceContextClosed(object sender, EventArgs e)
        {
            if (this.persistenceContext.Aborted && !this.abortingExtensions)
            {
                this.AbortInstance(new FaultException(OperationExecutionFault.CreateAbortedFault(System.ServiceModel.Activities.SR.DefaultAbortReason)), false);
            }
        }

        private void OnReleaseInstance(IAsyncResult result)
        {
            try
            {
                this.EndReleaseInstance(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.AbortInstance(exception, false);
            }
        }

        protected override void OnRequestAbort(Exception reason)
        {
            this.AbortInstance(reason, false);
        }

        private static void OnTrackCompleteDone(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                WorkflowServiceInstance asyncState = (WorkflowServiceInstance) result.AsyncState;
                bool ownsLock = true;
                try
                {
                    asyncState.Controller.EndFlushTrackingRecords(result);
                    asyncState.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                    try
                    {
                        asyncState.isInHandler = true;
                        asyncState.OnCompleted();
                    }
                    finally
                    {
                        asyncState.isInHandler = false;
                    }
                }
                finally
                {
                    asyncState.ReleaseLock(ref ownsLock);
                }
            }
        }

        private static void OnTrackIdleDone(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                WorkflowServiceInstance asyncState = (WorkflowServiceInstance) result.AsyncState;
                bool ownsLock = true;
                try
                {
                    asyncState.Controller.EndFlushTrackingRecords(result);
                    asyncState.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                    try
                    {
                        asyncState.isInHandler = true;
                        asyncState.OnIdle();
                    }
                    finally
                    {
                        asyncState.isInHandler = false;
                    }
                }
                finally
                {
                    asyncState.ReleaseLock(ref ownsLock);
                }
            }
        }

        private static void OnTrackUnhandledExceptionDone(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                UnhandledExceptionAsyncData asyncState = (UnhandledExceptionAsyncData) result.AsyncState;
                WorkflowServiceInstance instance = asyncState.Instance;
                bool ownsLock = true;
                try
                {
                    instance.Controller.EndFlushTrackingRecords(result);
                    instance.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                    try
                    {
                        instance.isInHandler = true;
                        instance.OnUnhandledException(asyncState);
                    }
                    finally
                    {
                        instance.isInHandler = false;
                    }
                }
                finally
                {
                    instance.ReleaseLock(ref ownsLock);
                }
            }
        }

        public void OnTransactionAbortOrInDoubt(TransactionException exception)
        {
            this.AbortInstance(exception, false);
        }

        public void OnTransactionPrepared()
        {
            this.transactionContext = null;
            this.isInTransaction = false;
        }

        private void OnUnhandledException(UnhandledExceptionAsyncData data)
        {
            this.FaultPendingRequests(data.Exception);
            this.UnhandledExceptionPolicy.OnUnhandledException(data);
        }

        private void PrepareNextIdleWaiter()
        {
            if ((this.nextIdleWaiters != null) && (this.nextIdleWaiters.Count > 0))
            {
                lock (this.activeOperationsLock)
                {
                    if (this.idleWaiters == null)
                    {
                        this.idleWaiters = new List<AsyncWaitHandle>();
                    }
                    for (int i = 0; i < this.nextIdleWaiters.Count; i++)
                    {
                        this.idleWaiters.Add(this.nextIdleWaiters[i]);
                    }
                }
                this.nextIdleWaiters.Clear();
            }
        }

        private void RaiseAborted()
        {
            this.UnloadInstancePolicy.Cancel();
            this.CompletePendingOperations();
        }

        private void RecoverLastReference()
        {
            lock (this.thisLock)
            {
                this.referenceCount = 1;
            }
        }

        public void ReleaseContext(WorkflowOperationContext context)
        {
            lock (this.thisLock)
            {
                this.pendingRequests.Remove(context);
            }
        }

        private void ReleaseLock(ref bool ownsLock)
        {
            this.ReleaseLock(ref ownsLock, false);
        }

        private void ReleaseLock(ref bool ownsLock, bool hasBeenPersistedByIdlePolicy)
        {
            if (ownsLock)
            {
                bool flag = false;
                bool flag2 = false;
                lock (this.thisLock)
                {
                    this.isWorkflowServiceInstanceReady = true;
                    if (this.workflowServiceInstanceReadyWaitHandle != null)
                    {
                        flag2 = true;
                    }
                    this.hasDataToPersist = !hasBeenPersistedByIdlePolicy;
                }
                if (flag2)
                {
                    this.workflowServiceInstanceReadyWaitHandle.Set();
                }
                lock (this.activeOperationsLock)
                {
                    bool keepLockIfNoWaiters = ((this.state == State.Active) && this.isRunnable) && !this.IsIdle;
                    if (keepLockIfNoWaiters && (this.activeOperations == 0))
                    {
                        ownsLock = false;
                        flag = true;
                    }
                    else if (((!this.IsIdle && (this.state == State.Active)) || !this.NotifyNextIdleWaiter(ref ownsLock)) && !this.executorLock.Exit(keepLockIfNoWaiters, ref ownsLock))
                    {
                        ownsLock = false;
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.IncrementBusyCount();
                    this.persistenceContext.Bookmarks = null;
                    if (base.Controller.State == WorkflowInstanceState.Complete)
                    {
                        this.OnNotifyPaused();
                    }
                    else
                    {
                        base.Controller.Run();
                    }
                }
            }
        }

        public int ReleaseReference()
        {
            int num;
            lock (this.thisLock)
            {
                Fx.AssertAndThrow(this.referenceCount > 1, "referenceCount must be greater than 1");
                num = --this.referenceCount;
            }
            this.StartUnloadInstancePolicyIfNecessary();
            return num;
        }

        public void RemovePendingOperation(string sessionId, IAsyncResult result)
        {
            lock (this.thisLock)
            {
                List<PendingOperationAsyncResult> list;
                if (((this.pendingOperations != null) && this.pendingOperations.TryGetValue(sessionId, out list)) && (list.Count > 0))
                {
                    bool flag = list[0] == result;
                    if (list.Remove((PendingOperationAsyncResult) result))
                    {
                        this.pendingOperationCount--;
                    }
                    if (list.Count == 0)
                    {
                        this.pendingOperations.Remove(sessionId);
                    }
                    else if (flag)
                    {
                        list[0].Unblock();
                    }
                }
            }
        }

        private BookmarkResumptionResult ResumeProtocolBookmarkCore(Bookmark bookmark, object value, BookmarkScope bookmarkScope, bool bufferedReceiveEnabled, ref AsyncWaitHandle waitHandle, ref bool ownsLock)
        {
            BookmarkResumptionResult result;
            if (bookmarkScope == null)
            {
                result = base.Controller.ScheduleBookmarkResumption(bookmark, value);
            }
            else
            {
                result = base.Controller.ScheduleBookmarkResumption(bookmark, value, bookmarkScope);
            }
            if ((result == BookmarkResumptionResult.NotReady) && !bufferedReceiveEnabled)
            {
                if (waitHandle == null)
                {
                    waitHandle = new AsyncWaitHandle();
                }
                else
                {
                    waitHandle.Reset();
                }
                if (this.nextIdleWaiters == null)
                {
                    this.nextIdleWaiters = new List<AsyncWaitHandle>();
                }
                lock (this.activeOperationsLock)
                {
                    this.nextIdleWaiters.Add(waitHandle);
                }
                this.ReleaseLock(ref ownsLock);
            }
            return result;
        }

        private void RunCore()
        {
            this.isRunnable = true;
            this.state = State.Active;
        }

        private void ScheduleTrackAndRaiseAborted(Exception reason)
        {
            ActionItem.Schedule(new Action<object>(this.TrackAndRaiseAborted), reason);
        }

        private void SetupExtensions(WorkflowInstanceExtensionManager extensionManager)
        {
            base.RegisterExtensionManager(extensionManager);
            IEnumerable<IPersistencePipelineModule> extensions = base.GetExtensions<IPersistencePipelineModule>();
            int capacity = extensions.Count<IPersistencePipelineModule>();
            if (capacity > 0)
            {
                this.PipelineModules = new List<IPersistencePipelineModule>(capacity);
                this.PipelineModules.AddRange(extensions);
            }
        }

        private AsyncWaitHandle SetupIdleWaiter(ref bool ownsLock)
        {
            AsyncWaitHandle item = new AsyncWaitHandle(EventResetMode.ManualReset);
            lock (this.activeOperationsLock)
            {
                if (this.idleWaiters == null)
                {
                    this.idleWaiters = new List<AsyncWaitHandle>();
                }
                this.idleWaiters.Add(item);
            }
            this.ReleaseLock(ref ownsLock);
            return item;
        }

        private void StartUnloadInstancePolicyIfNecessary()
        {
            if ((((this.referenceCount == 1) && !this.executorLock.IsLocked) && (!this.isInTransaction && (this.state != State.Completed))) && ((this.state != State.Unloaded) && (this.state != State.Aborted)))
            {
                this.UnloadInstancePolicy.Begin();
            }
        }

        private void ThrowIfAborted()
        {
            if (this.state == State.Aborted)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateAbortedFault(System.ServiceModel.Activities.SR.WorkflowInstanceAborted(this.Id))));
            }
        }

        private void ThrowIfNoPersistenceProvider()
        {
            if (this.persistenceContext == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.PersistenceProviderRequiredToPersist));
            }
        }

        private void ThrowIfSuspended()
        {
            if (this.state == State.Suspended)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.InstanceMustNotBeSuspended));
            }
        }

        private void ThrowIfTerminatedOrCompleted()
        {
            if (this.hasRaisedCompleted)
            {
                if (this.terminationException != null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateTerminatedFault(System.ServiceModel.Activities.SR.WorkflowInstanceTerminated(this.Id))));
                }
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateCompletedFault(System.ServiceModel.Activities.SR.WorkflowInstanceCompleted(this.Id))));
            }
        }

        private void ThrowIfUnloaded()
        {
            if (this.state == State.Unloaded)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateInstanceUnloadedFault(System.ServiceModel.Activities.SR.WorkflowInstanceUnloaded(this.Id))));
            }
        }

        private void TrackAndRaiseAborted(object state)
        {
            Exception exception = (Exception) state;
            if (base.Controller.HasPendingTrackingRecords)
            {
                try
                {
                    IAsyncResult result = base.Controller.BeginFlushTrackingRecords(this.trackTimeout, Fx.ThunkCallback(new AsyncCallback(this.OnAbortTrackingComplete)), exception);
                    if (result.CompletedSynchronously)
                    {
                        base.Controller.EndFlushTrackingRecords(result);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception2);
                }
            }
            this.RaiseAborted();
        }

        private void TrackPersistence(PersistenceOperation operation)
        {
            if (base.Controller.TrackingEnabled)
            {
                if (operation == PersistenceOperation.Delete)
                {
                    base.Controller.Track(new WorkflowInstanceRecord(this.Id, base.WorkflowDefinition.DisplayName, "Deleted"));
                }
                else if (operation == PersistenceOperation.Unload)
                {
                    base.Controller.Track(new WorkflowInstanceRecord(this.Id, base.WorkflowDefinition.DisplayName, "Unloaded"));
                }
                else
                {
                    base.Controller.Track(new WorkflowInstanceRecord(this.Id, base.WorkflowDefinition.DisplayName, "Persisted"));
                }
            }
        }

        public void TransactionCommitted()
        {
            if (this.TryAddReference())
            {
                try
                {
                    if (((this.state == State.Suspended) && this.isTransactedCancelled) || (this.state == State.Active))
                    {
                        bool ownsLock = false;
                        try
                        {
                            try
                            {
                                this.AcquireLock(this.acquireLockTimeout, ref ownsLock);
                                if (((this.state == State.Suspended) && this.isTransactedCancelled) || this.ValidateStateForRun(null))
                                {
                                    this.isRunnable = true;
                                    this.state = State.Active;
                                }
                            }
                            catch (Exception exception)
                            {
                                if (Fx.IsFatal(exception))
                                {
                                    throw;
                                }
                                System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                            }
                            return;
                        }
                        finally
                        {
                            this.ReleaseLock(ref ownsLock);
                        }
                    }
                    if ((this.state == State.Unloaded) && (this.completionState == ActivityInstanceState.Faulted))
                    {
                        try
                        {
                            this.OnCompleted();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            this.AbortInstance(exception2, false);
                        }
                    }
                }
                finally
                {
                    this.ReleaseReference();
                }
            }
        }

        private bool TryAddReference()
        {
            bool flag = false;
            lock (this.thisLock)
            {
                if (this.referenceCount > 0)
                {
                    this.referenceCount++;
                    flag = true;
                }
            }
            if (flag)
            {
                this.UnloadInstancePolicy.Cancel();
            }
            return flag;
        }

        private bool TryReleaseLastReference()
        {
            lock (this.thisLock)
            {
                if (this.referenceCount == 1)
                {
                    this.referenceCount = 0;
                    return true;
                }
            }
            return false;
        }

        private void Validate(string operationName, Transaction ambientTransaction, bool controlEndpoint)
        {
            this.ValidateHelper(operationName, ambientTransaction, false, controlEndpoint);
        }

        private void ValidateHelper(string operationName, Transaction ambientTransaction, bool useThreadTransaction, bool controlEndpoint)
        {
            TransactionContext transactionContext = this.transactionContext;
            if ((transactionContext != null) && (transactionContext.CurrentTransaction != (useThreadTransaction ? Transaction.Current : ambientTransaction)))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateTransactedLockException(this.persistenceContext.InstanceId, operationName)));
            }
            if (controlEndpoint)
            {
                Fx.AssertAndThrow(this.state != State.Unloaded, "Cannot be unloaded");
            }
            if (this.state == State.Unloaded)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateInstanceUnloadedFault(System.ServiceModel.Activities.SR.ServiceInstanceUnloaded(this.persistenceContext.InstanceId))));
            }
            if ((this.state == State.Completed) || (this.state == State.Aborted))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateInstanceNotFoundFault(System.ServiceModel.Activities.SR.ServiceInstanceTerminated(this.persistenceContext.InstanceId))));
            }
            if (((((this.state == State.Suspended) && !(operationName == "Suspend")) && (!(operationName == "TransactedSuspend") && !(operationName == "Unsuspend"))) && ((!(operationName == "TransactedUnsuspend") && !(operationName == "Terminate")) && (!(operationName == "TransactedTerminate") && !(operationName == "Cancel")))) && !(operationName == "TransactedCancel"))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateSuspendedFault(this.Id, operationName)));
            }
        }

        private bool ValidateStateForAbort()
        {
            if (this.state == State.Aborted)
            {
                return false;
            }
            return true;
        }

        private void ValidateStateForAssociateKeys()
        {
            this.ThrowIfSuspended();
        }

        private bool ValidateStateForCancel(Transaction transaction)
        {
            if (this.hasRaisedCompleted)
            {
                return false;
            }
            this.Validate((transaction == null) ? "Cancel" : "TransactedCancel", transaction, true);
            this.ThrowIfAborted();
            this.ThrowIfUnloaded();
            return true;
        }

        private void ValidateStateForPersist()
        {
            this.ThrowIfAborted();
            this.ThrowIfUnloaded();
            this.ThrowIfNoPersistenceProvider();
        }

        private void ValidateStateForResumeProtocolBookmark()
        {
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
            this.ThrowIfSuspended();
        }

        private bool ValidateStateForRun(Transaction transaction)
        {
            if ((this.hasRaisedCompleted || ((this.state == State.Active) && this.isRunnable)) || this.isInTransaction)
            {
                return false;
            }
            this.Validate((transaction == null) ? "Run" : "TransactedRun", transaction, true);
            this.ThrowIfAborted();
            this.ThrowIfUnloaded();
            this.ThrowIfSuspended();
            return true;
        }

        private bool ValidateStateForSuspend(Transaction transaction)
        {
            this.Validate((transaction == null) ? "Suspend" : "TransactedSuspend", transaction, true);
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
            return true;
        }

        private bool ValidateStateForTerminate(Transaction transaction)
        {
            this.Validate((transaction == null) ? "Terminate" : "TransactedTerminate", transaction, true);
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
            return true;
        }

        private bool ValidateStateForUnload()
        {
            if (this.state == State.Unloaded)
            {
                return false;
            }
            this.ThrowIfAborted();
            if (base.Controller.State != WorkflowInstanceState.Complete)
            {
                this.ThrowIfNoPersistenceProvider();
            }
            return true;
        }

        private bool ValidateStateForUnsuspend(Transaction transaction)
        {
            if (this.state == State.Active)
            {
                return false;
            }
            this.Validate((transaction == null) ? "Unsuspend" : "TransactedUnsuspend", transaction, true);
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
            return true;
        }

        public System.ServiceModel.Activities.Dispatcher.BufferedReceiveManager BufferedReceiveManager
        {
            get
            {
                return this.bufferedReceiveManager;
            }
        }

        public override Guid Id
        {
            get
            {
                return this.instanceId;
            }
        }

        private bool IsHandlerThread
        {
            get
            {
                return (this.isInHandler && (this.handlerThreadId == Thread.CurrentThread.ManagedThreadId));
            }
        }

        private bool IsIdle
        {
            get
            {
                return (base.Controller.State == WorkflowInstanceState.Idle);
            }
        }

        internal List<IPersistencePipelineModule> PipelineModules { get; private set; }

        private bool ShouldRaiseComplete
        {
            get
            {
                return ((base.Controller.State == WorkflowInstanceState.Complete) && !this.hasRaisedCompleted);
            }
        }

        private bool ShouldRaiseIdle
        {
            get
            {
                return ((this.IsIdle && !this.hasRaisedCompleted) && (this.state != State.Aborted));
            }
        }

        protected override bool SupportsInstanceKeys
        {
            get
            {
                return true;
            }
        }

        private static AsyncCallback TrackCompleteDoneCallback
        {
            get
            {
                if (trackCompleteDoneCallback == null)
                {
                    trackCompleteDoneCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.OnTrackCompleteDone));
                }
                return trackCompleteDoneCallback;
            }
        }

        private static AsyncCallback TrackIdleDoneCallback
        {
            get
            {
                if (trackIdleDoneCallback == null)
                {
                    trackIdleDoneCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.OnTrackIdleDone));
                }
                return trackIdleDoneCallback;
            }
        }

        private static AsyncCallback TrackUnhandledExceptionDoneCallback
        {
            get
            {
                if (trackUnhandledExceptionDoneCallback == null)
                {
                    trackUnhandledExceptionDoneCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.OnTrackUnhandledExceptionDone));
                }
                return trackUnhandledExceptionDoneCallback;
            }
        }

        private UnhandledExceptionPolicyHelper UnhandledExceptionPolicy
        {
            get
            {
                if (this.unhandledExceptionPolicy == null)
                {
                    this.unhandledExceptionPolicy = new UnhandledExceptionPolicyHelper(this, this.serviceHost.UnhandledExceptionAction);
                }
                return this.unhandledExceptionPolicy;
            }
        }

        private UnloadInstancePolicyHelper UnloadInstancePolicy
        {
            get
            {
                if (this.unloadInstancePolicy == null)
                {
                    this.unloadInstancePolicy = new UnloadInstancePolicyHelper(this, this.serviceHost.IdleTimeToPersist, this.serviceHost.IdleTimeToUnload);
                }
                return this.unloadInstancePolicy;
            }
        }

        private class AbandonAndSuspendAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private Exception reason;

            private AbandonAndSuspendAsyncResult(WorkflowServiceInstance instance, Exception reason, AsyncCallback callback, object state) : base(instance, null, callback, state)
            {
                this.reason = reason;
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                IAsyncResult result;
                try
                {
                    result = base.Instance.persistenceContext.BeginUpdateSuspendMetadata(this.reason, base.Instance.persistTimeout, callback, state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.Instance.AbortInstance(this.reason, true);
                    throw;
                }
                return result;
            }

            public static WorkflowServiceInstance.AbandonAndSuspendAsyncResult Create(WorkflowServiceInstance instance, Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.AbandonAndSuspendAsyncResult result = new WorkflowServiceInstance.AbandonAndSuspendAsyncResult(instance, reason, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.AbandonAndSuspendAsyncResult>(result);
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                try
                {
                    base.Instance.persistenceContext.EndUpdateSuspendMetadata(result);
                    WorkflowServiceInstance.AbandonAndSuspendAsyncResult asyncState = (WorkflowServiceInstance.AbandonAndSuspendAsyncResult) result.AsyncState;
                    if (base.Instance.Controller.TrackingEnabled)
                    {
                        base.Instance.Controller.Track(new WorkflowInstanceSuspendedRecord(base.Instance.Id, base.Instance.WorkflowDefinition.DisplayName, asyncState.reason.Message));
                    }
                }
                finally
                {
                    base.Instance.AbortInstance(this.reason, true);
                }
            }

            protected override void PerformOperation()
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            protected override void PostOperation()
            {
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForAbort();
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    return false;
                }
            }
        }

        private class AbandonAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private Exception reason;
            private bool shouldTrackAbort;

            private AbandonAsyncResult(WorkflowServiceInstance instance, Exception reason, bool shouldTrackAbort, AsyncCallback callback, object state) : base(instance, null, callback, state)
            {
                this.reason = reason;
                this.shouldTrackAbort = shouldTrackAbort;
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                IAsyncResult result;
                try
                {
                    result = base.Instance.persistenceContext.BeginRelease(base.Instance.persistTimeout, callback, state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.Instance.AbortInstance(this.reason, true);
                    throw;
                }
                return result;
            }

            public static WorkflowServiceInstance.AbandonAsyncResult Create(WorkflowServiceInstance instance, Exception reason, bool shouldTrackAbort, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.AbandonAsyncResult result = new WorkflowServiceInstance.AbandonAsyncResult(instance, reason, shouldTrackAbort, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.AbandonAsyncResult>(result);
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                try
                {
                    base.Instance.persistenceContext.EndRelease(result);
                    if (!this.shouldTrackAbort && base.Instance.Controller.TrackingEnabled)
                    {
                        base.Instance.Controller.Track(new WorkflowInstanceRecord(base.Instance.Id, base.Instance.WorkflowDefinition.DisplayName, "Unloaded"));
                    }
                    base.Instance.AbortInstance(this.reason, true, this.shouldTrackAbort);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.Instance.AbortInstance(this.reason, true);
                    throw;
                }
            }

            protected override void PerformOperation()
            {
                base.Instance.RecoverLastReference();
            }

            protected override void PostOperation()
            {
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForAbort();
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    return (!this.shouldTrackAbort && base.Instance.hasDataToPersist);
                }
            }
        }

        private class AbortInstanceState
        {
            public AbortInstanceState(Exception reason, bool shouldTrackAbort)
            {
                this.Reason = reason;
                this.ShouldTrackAbort = shouldTrackAbort;
            }

            public Exception Reason { get; private set; }

            public bool ShouldTrackAbort { get; private set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AcquireLockAsyncData
        {
            private WorkflowServiceInstance instance;
            private FastAsyncCallback callback;
            private object state;
            public AcquireLockAsyncData(WorkflowServiceInstance instance, FastAsyncCallback callback, object state)
            {
                this.instance = instance;
                this.callback = callback;
                this.state = state;
            }

            public WorkflowServiceInstance Instance
            {
                get
                {
                    return this.instance;
                }
            }
            public FastAsyncCallback Callback
            {
                get
                {
                    return this.callback;
                }
            }
            public object State
            {
                get
                {
                    return this.state;
                }
            }
        }

        private class AcquireLockOnIdleAsyncResult : AsyncResult
        {
            private bool acquiredLockAsynchronously;
            private AsyncWaitHandle idleEvent;
            private static Action<object, TimeoutException> idleReceivedCallback = new Action<object, TimeoutException>(WorkflowServiceInstance.AcquireLockOnIdleAsyncResult.OnIdleReceived);
            private WorkflowServiceInstance instance;
            private static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(WorkflowServiceInstance.AcquireLockOnIdleAsyncResult.OnLockAcquired);
            private TimeoutHelper timeoutHelper;

            public AcquireLockOnIdleAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, ref bool ownsLock, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = false;
                bool flag2 = true;
                bool flag3 = true;
                object token = null;
                try
                {
                    lock (this.instance.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.instance.activeOperations++;
                            flag = true;
                        }
                        this.instance.executorLock.SetupWaiter(ref token);
                    }
                    flag3 = this.instance.executorLock.EnterAsync(this.timeoutHelper.RemainingTime(), ref token, ref ownsLock, lockAcquiredCallback, this);
                    flag2 = flag3;
                }
                finally
                {
                    if (flag && flag2)
                    {
                        lock (this.instance.activeOperationsLock)
                        {
                            this.instance.activeOperations--;
                        }
                    }
                    this.instance.executorLock.CleanupWaiter(token, ref ownsLock);
                }
                if (flag3 && this.CheckState(ref ownsLock))
                {
                    base.Complete(true);
                }
            }

            private bool CheckState(ref bool ownsLock)
            {
                if ((this.instance.state == WorkflowServiceInstance.State.Active) && !this.instance.isRunnable)
                {
                    this.instance.RunCore();
                }
                if ((this.instance.state == WorkflowServiceInstance.State.Active) && (this.instance.Controller.State == WorkflowInstanceState.Runnable))
                {
                    this.idleEvent = this.instance.SetupIdleWaiter(ref ownsLock);
                    try
                    {
                        if (this.idleEvent.WaitAsync(idleReceivedCallback, this, this.timeoutHelper.RemainingTime()))
                        {
                            ownsLock = true;
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
                        if (this.instance.CleanupIdleWaiter(this.idleEvent, exception, ref ownsLock))
                        {
                            throw;
                        }
                    }
                }
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.AcquireLockOnIdleAsyncResult>(result);
            }

            public static void End(IAsyncResult result, ref bool ownsLock)
            {
                WorkflowServiceInstance.AcquireLockOnIdleAsyncResult result2 = result as WorkflowServiceInstance.AcquireLockOnIdleAsyncResult;
                if (result2 != null)
                {
                    ownsLock = result2.acquiredLockAsynchronously;
                }
                AsyncResult.End<WorkflowServiceInstance.AcquireLockOnIdleAsyncResult>(result);
            }

            private static void OnIdleReceived(object state, TimeoutException asyncException)
            {
                WorkflowServiceInstance.AcquireLockOnIdleAsyncResult result = (WorkflowServiceInstance.AcquireLockOnIdleAsyncResult) state;
                if ((asyncException != null) && result.instance.CleanupIdleWaiter(result.idleEvent, asyncException, ref result.acquiredLockAsynchronously))
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    result.acquiredLockAsynchronously = true;
                    result.Complete(false, null);
                }
            }

            private static void OnLockAcquired(object state, Exception asyncException)
            {
                WorkflowServiceInstance.AcquireLockOnIdleAsyncResult result = (WorkflowServiceInstance.AcquireLockOnIdleAsyncResult) state;
                lock (result.instance.activeOperationsLock)
                {
                    result.instance.activeOperations--;
                }
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    bool flag = true;
                    Exception exception = null;
                    try
                    {
                        result.acquiredLockAsynchronously = true;
                        flag = result.CheckState(ref result.acquiredLockAsynchronously);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }
        }

        private class AssociateKeysAsyncResult : AsyncResult
        {
            private readonly ICollection<InstanceKey> associatedKeys;
            private static AsyncResult.AsyncCompletion handleAssociateInfrastructureKeys = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.AssociateKeysAsyncResult.HandleAssociateInfrastructureKeys);
            private static AsyncResult.AsyncCompletion handleLockAcquired = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.AssociateKeysAsyncResult.HandleLockAcquired);
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowServiceInstance.AssociateKeysAsyncResult.Finally);
            private bool ownsLock;
            private readonly TimeoutHelper timeoutHelper;
            private readonly Transaction transaction;
            private readonly WorkflowServiceInstance workflow;

            public AssociateKeysAsyncResult(WorkflowServiceInstance workflow, ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.workflow = workflow;
                this.associatedKeys = associatedKeys;
                this.transaction = transaction;
                this.timeoutHelper = new TimeoutHelper(timeout);
                base.OnCompleting = onCompleting;
                IAsyncResult result = this.workflow.BeginAcquireLockOnIdle(this.timeoutHelper.RemainingTime(), ref this.ownsLock, base.PrepareAsyncCompletion(handleLockAcquired), this);
                if (base.SyncContinue(result))
                {
                    base.Complete(true);
                }
            }

            private bool AssociateKeys()
            {
                IAsyncResult result;
                using (base.PrepareTransactionalCall(this.transaction))
                {
                    result = this.workflow.persistenceContext.BeginAssociateInfrastructureKeys(this.associatedKeys, this.workflow.persistTimeout, base.PrepareAsyncCompletion(handleAssociateInfrastructureKeys), this);
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.AssociateKeysAsyncResult>(result);
            }

            private static void Finally(AsyncResult result, Exception completionException)
            {
                WorkflowServiceInstance.AssociateKeysAsyncResult result2 = (WorkflowServiceInstance.AssociateKeysAsyncResult) result;
                if (result2.ownsLock)
                {
                    result2.workflow.ReleaseLock(ref result2.ownsLock);
                }
            }

            private static bool HandleAssociateInfrastructureKeys(IAsyncResult result)
            {
                WorkflowServiceInstance.AssociateKeysAsyncResult asyncState = (WorkflowServiceInstance.AssociateKeysAsyncResult) result.AsyncState;
                asyncState.workflow.persistenceContext.EndAssociateInfrastructureKeys(result);
                asyncState.workflow.ReleaseLock(ref asyncState.ownsLock);
                return true;
            }

            private static bool HandleLockAcquired(IAsyncResult result)
            {
                WorkflowServiceInstance.AssociateKeysAsyncResult asyncState = (WorkflowServiceInstance.AssociateKeysAsyncResult) result.AsyncState;
                if (result.CompletedSynchronously)
                {
                    asyncState.workflow.EndAcquireLockOnIdle(result);
                }
                else
                {
                    asyncState.workflow.EndAcquireLockOnIdle(result, ref asyncState.ownsLock);
                }
                asyncState.workflow.ValidateStateForAssociateKeys();
                return asyncState.AssociateKeys();
            }
        }

        private class CancelAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private CancelAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state) : base(instance, transaction, callback, state)
            {
            }

            public static WorkflowServiceInstance.CancelAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.CancelAsyncResult result = new WorkflowServiceInstance.CancelAsyncResult(instance, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.CancelAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                base.Instance.Controller.ScheduleCancel();
                if (base.Instance.persistenceContext.IsSuspended)
                {
                    base.Instance.persistenceContext.IsSuspended = false;
                    base.Instance.persistenceContext.SuspendedReason = null;
                }
                if (!base.Instance.isInTransaction)
                {
                    base.Instance.isRunnable = true;
                    base.Instance.state = WorkflowServiceInstance.State.Active;
                }
                else
                {
                    base.Instance.isTransactedCancelled = true;
                }
            }

            protected override void PostOperation()
            {
                base.Instance.CompletePendingOperations();
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForCancel(base.OperationTransaction);
            }
        }

        private delegate void InvokeCompletedCallback();

        private class PendingOperationAsyncResult : AsyncResult
        {
            private static Action<object, TimeoutException> handleEndWait = new Action<object, TimeoutException>(WorkflowServiceInstance.PendingOperationAsyncResult.HandleEndWait);
            private bool isFirstRequest;
            private TimeSpan timeout;
            private AsyncWaitHandle waitHandle;

            public PendingOperationAsyncResult(bool isFirstRequest, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.isFirstRequest = isFirstRequest;
                this.timeout = timeout;
                if (!this.isFirstRequest)
                {
                    this.waitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.PendingOperationAsyncResult>(result);
            }

            private static void HandleEndWait(object state, TimeoutException e)
            {
                ((WorkflowServiceInstance.PendingOperationAsyncResult) state).Complete(false, e);
            }

            public void Start()
            {
                if (this.isFirstRequest)
                {
                    base.Complete(true);
                }
                else if (this.waitHandle.WaitAsync(handleEndWait, this, this.timeout))
                {
                    base.Complete(true);
                }
            }

            public void Unblock()
            {
                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }
        }

        private enum PersistenceOperation : byte
        {
            Delete = 0,
            Save = 1,
            Unload = 2
        }

        private class ReleaseInstanceAsyncResult : AsyncResult
        {
            private static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(WorkflowServiceInstance.ReleaseInstanceAsyncResult.AcquireCompletedCallback);
            private static AsyncResult.AsyncCompletion handleEndUnload;
            private bool isTryUnload;
            private static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(WorkflowServiceInstance.ReleaseInstanceAsyncResult.OnLockAcquired);
            private static AsyncResult.AsyncCompletion onClosePersistenceContext;
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowServiceInstance.ReleaseInstanceAsyncResult.Finally);
            private static AsyncResult.AsyncCompletion onReleasePersistenceContext;
            private bool ownsLock;
            private bool referenceAcquired;
            private TimeoutHelper timeoutHelper;
            private WorkflowServiceInstance workflowInstance;

            public ReleaseInstanceAsyncResult(WorkflowServiceInstance workflowServiceInstance, bool isTryUnload, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.workflowInstance = workflowServiceInstance;
                this.isTryUnload = isTryUnload;
                this.timeoutHelper = new TimeoutHelper(timeout);
                base.OnCompleting = onCompleting;
                bool flag = false;
                Exception completionException = null;
                try
                {
                    flag = this.TryAcquire();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    completionException = exception2;
                    throw;
                }
                finally
                {
                    if (completionException != null)
                    {
                        Finally(this, completionException);
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private static void AcquireCompletedCallback(object state, Exception completionException)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult result = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) state;
                bool flag = true;
                if (completionException == null)
                {
                    try
                    {
                        flag = result.HandleEndAcquireReference();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }
                }
                if (flag)
                {
                    result.Complete(false, completionException);
                }
            }

            private IAsyncResult BeginTryUnload(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new WorkflowServiceInstance.UnloadOrPersistAsyncResult(this.workflowInstance, WorkflowServiceInstance.PersistenceOperation.Unload, false, true, timeout, callback, state);
            }

            private IAsyncResult BeginUnload(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new WorkflowServiceInstance.UnloadOrPersistAsyncResult(this.workflowInstance, WorkflowServiceInstance.PersistenceOperation.Unload, false, false, timeout, callback, state);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.ReleaseInstanceAsyncResult>(result);
            }

            private bool EndTryUnload(IAsyncResult result)
            {
                return WorkflowServiceInstance.UnloadOrPersistAsyncResult.End(result);
            }

            private void EndUnload(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult.End(result);
            }

            private static void Finally(AsyncResult result, Exception completionException)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult result2 = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) result;
                try
                {
                    try
                    {
                        if ((completionException != null) && !Fx.IsFatal(completionException))
                        {
                            result2.workflowInstance.AbortInstance(completionException, result2.ownsLock);
                        }
                    }
                    finally
                    {
                        if (result2.ownsLock)
                        {
                            result2.workflowInstance.ReleaseLock(ref result2.ownsLock);
                        }
                    }
                }
                finally
                {
                    if (result2.referenceAcquired)
                    {
                        result2.workflowInstance.acquireReferenceSemaphore.Exit();
                        result2.referenceAcquired = false;
                    }
                }
            }

            private bool HandleEndAcquireReference()
            {
                this.referenceAcquired = true;
                if (this.workflowInstance.hasPersistedDeleted)
                {
                    return this.LockAndReleasePersistenceContext();
                }
                return this.ReleaseInstance();
            }

            private static bool HandleEndUnload(IAsyncResult result)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult asyncState = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    if (asyncState.isTryUnload)
                    {
                        flag = asyncState.EndTryUnload(result);
                    }
                    else
                    {
                        asyncState.EndUnload(result);
                        flag = true;
                    }
                }
                catch (FaultException exception)
                {
                    if (!OperationExecutionFault.IsAbortedFaultException(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                }
                if (flag)
                {
                    return asyncState.LockAndReleasePersistenceContext();
                }
                return true;
            }

            private bool LockAndReleasePersistenceContext()
            {
                if (!this.workflowInstance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this))
                {
                    return false;
                }
                bool flag = true;
                try
                {
                    flag = this.ReleasePersistenceContext();
                }
                finally
                {
                    if (flag)
                    {
                        this.workflowInstance.ReleaseLock(ref this.ownsLock);
                    }
                }
                return flag;
            }

            private static bool OnClosePersistenceContext(IAsyncResult result)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult asyncState = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) result.AsyncState;
                asyncState.workflowInstance.persistenceContext.EndClose(result);
                asyncState.workflowInstance.Dispose();
                return true;
            }

            private static void OnLockAcquired(object state, Exception asyncException)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult result = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    result.ownsLock = true;
                    bool flag = true;
                    Exception exception = null;
                    try
                    {
                        flag = result.ReleasePersistenceContext();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    finally
                    {
                        if (flag)
                        {
                            result.workflowInstance.ReleaseLock(ref result.ownsLock);
                        }
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private static bool OnReleasePersistenceContext(IAsyncResult result)
            {
                WorkflowServiceInstance.ReleaseInstanceAsyncResult asyncState = (WorkflowServiceInstance.ReleaseInstanceAsyncResult) result.AsyncState;
                asyncState.workflowInstance.persistenceContext.EndRelease(result);
                if (onClosePersistenceContext == null)
                {
                    onClosePersistenceContext = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ReleaseInstanceAsyncResult.OnClosePersistenceContext);
                }
                IAsyncResult result3 = asyncState.workflowInstance.persistenceContext.BeginClose(asyncState.timeoutHelper.RemainingTime(), asyncState.PrepareAsyncCompletion(onClosePersistenceContext), asyncState);
                return asyncState.SyncContinue(result3);
            }

            private bool ReleaseInstance()
            {
                if (handleEndUnload == null)
                {
                    handleEndUnload = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ReleaseInstanceAsyncResult.HandleEndUnload);
                }
                IAsyncResult result = null;
                try
                {
                    if (this.isTryUnload)
                    {
                        result = this.BeginTryUnload(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndUnload), this);
                    }
                    else
                    {
                        result = this.BeginUnload(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndUnload), this);
                    }
                }
                catch (FaultException exception)
                {
                    if (!OperationExecutionFault.IsAbortedFaultException(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                    return true;
                }
                return (result.CompletedSynchronously && HandleEndUnload(result));
            }

            private bool ReleasePersistenceContext()
            {
                if (this.workflowInstance.persistenceContext.State != CommunicationState.Opened)
                {
                    return true;
                }
                if (onReleasePersistenceContext == null)
                {
                    onReleasePersistenceContext = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ReleaseInstanceAsyncResult.OnReleasePersistenceContext);
                }
                IAsyncResult result = this.workflowInstance.persistenceContext.BeginRelease(this.workflowInstance.persistTimeout, base.PrepareAsyncCompletion(onReleasePersistenceContext), this);
                return base.SyncContinue(result);
            }

            private bool TryAcquire()
            {
                return (this.workflowInstance.acquireReferenceSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), acquireCompletedCallback, this) && this.HandleEndAcquireReference());
            }
        }

        private class ResumeProtocolBookmarkAsyncResult : AsyncResult
        {
            private Bookmark bookmark;
            private BookmarkScope bookmarkScope;
            private static AsyncResult.AsyncCompletion handleEndLockAcquired = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.HandleEndLockAcquired);
            private static AsyncResult.AsyncCompletion handleEndReferenceAcquired = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.HandleEndReferenceAcquired);
            private static AsyncResult.AsyncCompletion handleEndTrack = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.HandleEndTrack);
            private WorkflowServiceInstance instance;
            private bool isResumeProtocolBookmark;
            private static Action<object, TimeoutException> nextIdleCallback;
            private TimeoutHelper nextIdleTimeoutHelper;
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.Finally);
            private bool ownsLock;
            private bool referenceAcquired;
            private BookmarkResumptionResult resumptionResult;
            private TimeoutHelper timeoutHelper;
            private object value;
            private AsyncWaitHandle waitHandle;
            private static Action<object, TimeoutException> workflowServiceInstanceReadyCallback;

            public ResumeProtocolBookmarkAsyncResult(WorkflowServiceInstance instance, Bookmark bookmark, object value, BookmarkScope bookmarkScope, bool isResumeProtocolBookmark, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.bookmark = bookmark;
                this.value = value;
                this.bookmarkScope = bookmarkScope;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.nextIdleTimeoutHelper = new TimeoutHelper(instance.serviceHost.FilterResumeTimeout);
                this.isResumeProtocolBookmark = isResumeProtocolBookmark;
                base.OnCompleting = onCompleting;
                Exception exception = null;
                bool flag = true;
                try
                {
                    if (this.isResumeProtocolBookmark)
                    {
                        flag = this.DoResumeBookmark();
                    }
                    else
                    {
                        flag = this.WaitForInstanceToBeReady();
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (flag)
                {
                    base.Complete(true, exception);
                }
            }

            private bool DoResumeBookmark()
            {
                IAsyncResult result = this.instance.BeginAcquireLockOnIdle(this.timeoutHelper.RemainingTime(), ref this.ownsLock, base.PrepareAsyncCompletion(handleEndLockAcquired), this);
                return base.SyncContinue(result);
            }

            public static BookmarkResumptionResult End(IAsyncResult result)
            {
                return AsyncResult.End<WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult>(result).resumptionResult;
            }

            private static void Finally(AsyncResult result, Exception completionException)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult result2 = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) result;
                try
                {
                    if (result2.ownsLock)
                    {
                        result2.instance.ReleaseLock(ref result2.ownsLock);
                    }
                }
                finally
                {
                    if (result2.referenceAcquired)
                    {
                        result2.instance.ReleaseReference();
                        result2.referenceAcquired = false;
                    }
                }
            }

            private static bool HandleEndLockAcquired(IAsyncResult result)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult asyncState = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) result.AsyncState;
                if (result.CompletedSynchronously)
                {
                    asyncState.instance.EndAcquireLockOnIdle(result);
                }
                else
                {
                    asyncState.instance.EndAcquireLockOnIdle(result, ref asyncState.ownsLock);
                }
                return asyncState.PerformResumption();
            }

            private static bool HandleEndReferenceAcquired(IAsyncResult result)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult asyncState = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) result.AsyncState;
                asyncState.referenceAcquired = asyncState.instance.EndTryAcquireReference(result);
                if (asyncState.referenceAcquired)
                {
                    return asyncState.WaitToBeSignaled();
                }
                asyncState.resumptionResult = BookmarkResumptionResult.NotReady;
                return true;
            }

            private static bool HandleEndTrack(IAsyncResult result)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult asyncState = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) result.AsyncState;
                asyncState.instance.Controller.EndFlushTrackingRecords(result);
                if (asyncState.ownsLock)
                {
                    asyncState.instance.ReleaseLock(ref asyncState.ownsLock);
                }
                if (asyncState.referenceAcquired)
                {
                    asyncState.instance.ReleaseReference();
                    asyncState.referenceAcquired = false;
                }
                return true;
            }

            private static void OnNextIdle(object state, TimeoutException asyncException)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult result = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) state;
                if (asyncException != null)
                {
                    lock (result.instance.activeOperationsLock)
                    {
                        if (result.instance.nextIdleWaiters.Remove(result.waitHandle) || result.instance.idleWaiters.Remove(result.waitHandle))
                        {
                            result.Complete(false, asyncException);
                            return;
                        }
                    }
                }
                result.ownsLock = true;
                bool flag2 = true;
                Exception exception = null;
                try
                {
                    flag2 = result.PerformResumption();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                finally
                {
                    if (flag2)
                    {
                        result.instance.ReleaseLock(ref result.ownsLock);
                    }
                }
                if (flag2)
                {
                    result.Complete(false, exception);
                }
            }

            private static void OnSignaled(object state, TimeoutException exception)
            {
                WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult result = (WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult) state;
                if (exception != null)
                {
                    result.Complete(false, exception);
                }
                else
                {
                    bool flag = false;
                    Exception exception2 = null;
                    try
                    {
                        flag = result.DoResumeBookmark();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        exception2 = exception3;
                    }
                    finally
                    {
                        if (exception2 != null)
                        {
                            result.Complete(false, exception2);
                        }
                    }
                    if (flag)
                    {
                        result.Complete(false);
                    }
                }
            }

            private bool PerformResumption()
            {
                bool flag;
                bool completeSelf = false;
                if (this.isResumeProtocolBookmark && (this.instance.BufferedReceiveManager == null))
                {
                    this.instance.ValidateStateForResumeProtocolBookmark();
                }
                else if (this.instance.AreBookmarksInvalid(out this.resumptionResult))
                {
                    return this.TrackPerformResumption(true);
                }
                do
                {
                    flag = false;
                    bool bufferedReceiveEnabled = this.isResumeProtocolBookmark && (this.instance.BufferedReceiveManager != null);
                    this.resumptionResult = this.instance.ResumeProtocolBookmarkCore(this.bookmark, this.value, this.bookmarkScope, bufferedReceiveEnabled, ref this.waitHandle, ref this.ownsLock);
                    if ((this.resumptionResult == BookmarkResumptionResult.NotReady) && !bufferedReceiveEnabled)
                    {
                        if (nextIdleCallback == null)
                        {
                            nextIdleCallback = new Action<object, TimeoutException>(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.OnNextIdle);
                        }
                        if (!this.waitHandle.WaitAsync(nextIdleCallback, this, !this.isResumeProtocolBookmark ? this.timeoutHelper.RemainingTime() : this.nextIdleTimeoutHelper.RemainingTime()))
                        {
                            return false;
                        }
                        this.ownsLock = true;
                        flag = true;
                    }
                    else
                    {
                        completeSelf = true;
                        break;
                    }
                }
                while (flag);
                return this.TrackPerformResumption(completeSelf);
            }

            private bool TrackPerformResumption(bool completeSelf)
            {
                if (this.instance.Controller.HasPendingTrackingRecords)
                {
                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, base.PrepareAsyncCompletion(handleEndTrack), this);
                    completeSelf = base.SyncContinue(result);
                }
                return completeSelf;
            }

            private bool WaitForInstanceToBeReady()
            {
                IAsyncResult result = this.instance.BeginTryAcquireReference(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndReferenceAcquired), this);
                return base.SyncContinue(result);
            }

            private bool WaitToBeSignaled()
            {
                bool flag = false;
                lock (this.instance.thisLock)
                {
                    if (!this.instance.isWorkflowServiceInstanceReady)
                    {
                        flag = true;
                        if (this.instance.workflowServiceInstanceReadyWaitHandle == null)
                        {
                            this.instance.workflowServiceInstanceReadyWaitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                        }
                    }
                }
                if (!flag)
                {
                    return this.DoResumeBookmark();
                }
                if (workflowServiceInstanceReadyCallback == null)
                {
                    workflowServiceInstanceReadyCallback = new Action<object, TimeoutException>(WorkflowServiceInstance.ResumeProtocolBookmarkAsyncResult.OnSignaled);
                }
                return (this.instance.workflowServiceInstanceReadyWaitHandle.WaitAsync(workflowServiceInstanceReadyCallback, this, this.timeoutHelper.RemainingTime()) && this.DoResumeBookmark());
            }
        }

        private class RunAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private RunAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state) : base(instance, transaction, callback, state)
            {
            }

            public static WorkflowServiceInstance.RunAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.RunAsyncResult result = new WorkflowServiceInstance.RunAsyncResult(instance, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.RunAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                if (!base.Instance.isInTransaction)
                {
                    base.Instance.RunCore();
                }
            }

            protected override void PostOperation()
            {
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForRun(base.OperationTransaction);
            }
        }

        private abstract class SimpleOperationAsyncResult : AsyncResult
        {
            private static AsyncResult.AsyncCompletion handleEndPerformOperation;
            private static AsyncResult.AsyncCompletion handleEndTrack;
            private WorkflowServiceInstance instance;
            private static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(WorkflowServiceInstance.SimpleOperationAsyncResult.OnLockAcquired);
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowServiceInstance.SimpleOperationAsyncResult.Finally);
            protected bool ownsLock;
            protected TimeoutHelper timeoutHelper;

            protected SimpleOperationAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.OperationTransaction = transaction;
                base.OnCompleting = onCompleting;
            }

            private bool AttachTransaction()
            {
                if ((this.OperationTransaction != null) && (this.Instance.transactionContext == null))
                {
                    this.Instance.transactionContext = new TransactionContext(this.Instance, this.OperationTransaction);
                    this.Instance.isInTransaction = true;
                    this.Instance.isRunnable = false;
                }
                if (this.IsSynchronousOperation)
                {
                    this.PerformOperation();
                    return this.Track();
                }
                if (handleEndPerformOperation == null)
                {
                    handleEndPerformOperation = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.SimpleOperationAsyncResult.HandleEndPerformOperation);
                }
                IAsyncResult result = this.BeginPerformOperation(base.PrepareAsyncCompletion(handleEndPerformOperation), this);
                return (result.CompletedSynchronously && HandleEndPerformOperation(result));
            }

            protected virtual IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            protected virtual void EndPerformOperation(IAsyncResult result)
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            private static void Finally(AsyncResult result, Exception completionException)
            {
                WorkflowServiceInstance.SimpleOperationAsyncResult result2 = (WorkflowServiceInstance.SimpleOperationAsyncResult) result;
                if (result2.ownsLock)
                {
                    result2.instance.ReleaseLock(ref result2.ownsLock);
                }
            }

            private static bool HandleEndPerformOperation(IAsyncResult result)
            {
                WorkflowServiceInstance.SimpleOperationAsyncResult asyncState = (WorkflowServiceInstance.SimpleOperationAsyncResult) result.AsyncState;
                asyncState.EndPerformOperation(result);
                return asyncState.Track();
            }

            private static bool HandleEndTrack(IAsyncResult result)
            {
                WorkflowServiceInstance.SimpleOperationAsyncResult asyncState = (WorkflowServiceInstance.SimpleOperationAsyncResult) result.AsyncState;
                asyncState.instance.Controller.EndFlushTrackingRecords(result);
                return asyncState.ReleaseLock();
            }

            private bool HandleLockAcquired()
            {
                if (this.ValidateState())
                {
                    return this.AttachTransaction();
                }
                return true;
            }

            private static void OnLockAcquired(object state, Exception asyncException)
            {
                WorkflowServiceInstance.SimpleOperationAsyncResult result = (WorkflowServiceInstance.SimpleOperationAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    result.ownsLock = true;
                    Exception exception = null;
                    bool flag = true;
                    try
                    {
                        flag = result.HandleLockAcquired();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            protected abstract void PerformOperation();
            protected abstract void PostOperation();
            private bool ReleaseLock()
            {
                this.instance.ReleaseLock(ref this.ownsLock);
                this.PostOperation();
                return true;
            }

            protected void Run(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                Exception exception = null;
                bool flag = true;
                if (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this))
                {
                    try
                    {
                        flag = this.HandleLockAcquired();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
                else
                {
                    flag = false;
                }
                if (flag)
                {
                    base.Complete(true, exception);
                }
            }

            private bool Track()
            {
                if ((this.instance.state == WorkflowServiceInstance.State.Aborted) || !this.instance.Controller.HasPendingTrackingRecords)
                {
                    return this.ReleaseLock();
                }
                if (handleEndTrack == null)
                {
                    handleEndTrack = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.SimpleOperationAsyncResult.HandleEndTrack);
                }
                IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, base.PrepareAsyncCompletion(handleEndTrack), this);
                return (result.CompletedSynchronously && HandleEndTrack(result));
            }

            protected abstract bool ValidateState();

            protected WorkflowServiceInstance Instance
            {
                get
                {
                    return this.instance;
                }
            }

            protected virtual bool IsSynchronousOperation
            {
                get
                {
                    return true;
                }
            }

            protected Transaction OperationTransaction { get; private set; }
        }

        private enum State
        {
            Active,
            Aborted,
            Suspended,
            Completed,
            Unloaded
        }

        private class SuspendAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private bool isUnlocked;
            private string reason;

            private SuspendAsyncResult(WorkflowServiceInstance instance, bool isUnlocked, string reason, Transaction transaction, AsyncCallback callback, object state) : base(instance, transaction, callback, state)
            {
                this.isUnlocked = isUnlocked;
                this.reason = reason;
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                return new SuspendCoreAsyncResult(this, callback, state);
            }

            public static WorkflowServiceInstance.SuspendAsyncResult Create(WorkflowServiceInstance instance, bool isUnlocked, string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.SuspendAsyncResult result = new WorkflowServiceInstance.SuspendAsyncResult(instance, isUnlocked, reason, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.SuspendAsyncResult>(result);
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                SuspendCoreAsyncResult.End(result);
            }

            protected override void PerformOperation()
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            protected override void PostOperation()
            {
                base.Instance.CompletePendingOperations();
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForSuspend(base.OperationTransaction);
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    return false;
                }
            }

            private class SuspendCoreAsyncResult : AsyncResult
            {
                private static AsyncResult.AsyncCompletion handleEndWaitForCanPersist = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.SuspendAsyncResult.SuspendCoreAsyncResult.HandleEndWaitForCanPersist);
                private WorkflowServiceInstance.SuspendAsyncResult parent;

                public SuspendCoreAsyncResult(WorkflowServiceInstance.SuspendAsyncResult parent, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.parent = parent;
                    IAsyncResult result = this.parent.Instance.BeginWaitForCanPersist(ref this.parent.ownsLock, this.parent.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndWaitForCanPersist), this);
                    if (base.SyncContinue(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WorkflowServiceInstance.SuspendAsyncResult.SuspendCoreAsyncResult>(result);
                }

                private static bool HandleEndWaitForCanPersist(IAsyncResult result)
                {
                    WorkflowServiceInstance.SuspendAsyncResult.SuspendCoreAsyncResult asyncState = (WorkflowServiceInstance.SuspendAsyncResult.SuspendCoreAsyncResult) result.AsyncState;
                    asyncState.parent.Instance.EndWaitForCanPersist(result, ref asyncState.parent.ownsLock);
                    asyncState.parent.Instance.persistenceContext.IsSuspended = true;
                    asyncState.parent.Instance.persistenceContext.SuspendedReason = asyncState.parent.reason;
                    asyncState.parent.Instance.state = WorkflowServiceInstance.State.Suspended;
                    if (asyncState.parent.Instance.Controller.TrackingEnabled)
                    {
                        asyncState.parent.Instance.Controller.Track(new WorkflowInstanceSuspendedRecord(asyncState.parent.Instance.Id, asyncState.parent.Instance.WorkflowDefinition.DisplayName, asyncState.parent.reason));
                    }
                    asyncState.parent.Instance.Controller.RequestPause();
                    return true;
                }
            }
        }

        private class TerminateAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private Exception reason;

            private TerminateAsyncResult(WorkflowServiceInstance instance, Exception reason, Transaction transaction, AsyncCallback callback, object state) : base(instance, transaction, callback, state)
            {
                this.reason = reason;
            }

            public static WorkflowServiceInstance.TerminateAsyncResult Create(WorkflowServiceInstance instance, Exception reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.TerminateAsyncResult result = new WorkflowServiceInstance.TerminateAsyncResult(instance, reason, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.TerminateAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                base.Instance.Controller.Terminate(this.reason);
                if (base.Instance.persistenceContext.IsSuspended)
                {
                    base.Instance.persistenceContext.IsSuspended = false;
                    base.Instance.persistenceContext.SuspendedReason = null;
                }
                if (!base.Instance.isInTransaction)
                {
                    base.Instance.isRunnable = true;
                    base.Instance.state = WorkflowServiceInstance.State.Active;
                }
                else
                {
                    base.Instance.GetCompletionState();
                }
            }

            protected override void PostOperation()
            {
                base.Instance.CompletePendingOperations();
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForTerminate(base.OperationTransaction);
            }
        }

        private class TryAcquireReferenceAsyncResult : AsyncResult
        {
            private static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(WorkflowServiceInstance.TryAcquireReferenceAsyncResult.AcquireCompletedCallback);
            private WorkflowServiceInstance instance;
            private bool result;
            private TimeoutHelper timeoutHelper;

            public TryAcquireReferenceAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.TryAcquire())
                {
                    base.Complete(true);
                }
            }

            private static void AcquireCompletedCallback(object state, Exception completionException)
            {
                WorkflowServiceInstance.TryAcquireReferenceAsyncResult result = (WorkflowServiceInstance.TryAcquireReferenceAsyncResult) state;
                if (completionException == null)
                {
                    try
                    {
                        result.HandleEndAcquireReference();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }
                }
                result.Complete(false, completionException);
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<WorkflowServiceInstance.TryAcquireReferenceAsyncResult>(result).result;
            }

            private void HandleEndAcquireReference()
            {
                try
                {
                    this.result = this.instance.TryAddReference();
                }
                finally
                {
                    this.instance.acquireReferenceSemaphore.Exit();
                }
            }

            private bool TryAcquire()
            {
                if (this.instance.acquireReferenceSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), acquireCompletedCallback, this))
                {
                    this.HandleEndAcquireReference();
                    return true;
                }
                return false;
            }
        }

        private class UnhandledExceptionAsyncData
        {
            public UnhandledExceptionAsyncData(WorkflowServiceInstance instance, System.Exception exception, Activity exceptionSource)
            {
                this.Instance = instance;
                this.Exception = exception;
                this.ExceptionSource = exceptionSource;
            }

            public System.Exception Exception { get; private set; }

            public Activity ExceptionSource { get; private set; }

            public WorkflowServiceInstance Instance { get; private set; }
        }

        private class UnhandledExceptionPolicyHelper
        {
            private WorkflowUnhandledExceptionAction action;
            private WorkflowServiceInstance instance;
            private static AsyncCallback operationCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.UnhandledExceptionPolicyHelper.OperationCallback));

            public UnhandledExceptionPolicyHelper(WorkflowServiceInstance instance, WorkflowUnhandledExceptionAction action)
            {
                this.instance = instance;
                this.action = action;
            }

            private void HandleEndOperation(IAsyncResult result)
            {
                if (this.action == WorkflowUnhandledExceptionAction.Cancel)
                {
                    this.instance.EndCancel(result);
                }
                else if (this.action == WorkflowUnhandledExceptionAction.Terminate)
                {
                    this.instance.EndTerminate(result);
                }
                else if (this.action == WorkflowUnhandledExceptionAction.AbandonAndSuspend)
                {
                    if (this.instance.persistenceContext.CanPersist)
                    {
                        this.instance.EndAbandonAndSuspend(result);
                    }
                    else
                    {
                        this.instance.EndAbandon(result);
                    }
                }
                else
                {
                    this.instance.EndAbandon(result);
                }
            }

            public void OnUnhandledException(WorkflowServiceInstance.UnhandledExceptionAsyncData data)
            {
                System.ServiceModel.Activities.FxTrace.Exception.AsWarning(data.Exception);
                try
                {
                    IAsyncResult result;
                    if (this.action == WorkflowUnhandledExceptionAction.Cancel)
                    {
                        result = this.instance.BeginCancel(null, TimeSpan.MaxValue, operationCallback, data);
                    }
                    else if (this.action == WorkflowUnhandledExceptionAction.Terminate)
                    {
                        result = this.instance.BeginTerminate(data.Exception, null, TimeSpan.MaxValue, operationCallback, data);
                    }
                    else if (this.action == WorkflowUnhandledExceptionAction.AbandonAndSuspend)
                    {
                        this.instance.isRunnable = false;
                        if (this.instance.persistenceContext.CanPersist)
                        {
                            result = this.instance.BeginAbandonAndSuspend(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                        }
                        else
                        {
                            result = this.instance.BeginAbandon(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                        }
                    }
                    else
                    {
                        this.instance.isRunnable = false;
                        result = this.instance.BeginAbandon(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                    }
                    if (result.CompletedSynchronously)
                    {
                        this.HandleEndOperation(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(exception, true);
                }
            }

            private static void OperationCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    WorkflowServiceInstance.UnhandledExceptionAsyncData asyncState = (WorkflowServiceInstance.UnhandledExceptionAsyncData) result.AsyncState;
                    WorkflowServiceInstance.UnhandledExceptionPolicyHelper unhandledExceptionPolicy = asyncState.Instance.UnhandledExceptionPolicy;
                    try
                    {
                        unhandledExceptionPolicy.HandleEndOperation(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        unhandledExceptionPolicy.instance.AbortInstance(exception, false);
                    }
                }
            }
        }

        private class UnloadInstancePolicyHelper
        {
            private bool cancelled;
            private WorkflowServiceInstance instance;
            private static AsyncCallback onPersistCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.UnloadInstancePolicyHelper.PersistCallback));
            private static Action<object> onTimerCallback = new Action<object>(WorkflowServiceInstance.UnloadInstancePolicyHelper.OnTimerCallback);
            private static AsyncCallback onUnloadCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.UnloadInstancePolicyHelper.UnloadCallback));
            private static AsyncCallback onUnlockAndAbortCallback = Fx.ThunkCallback(new AsyncCallback(WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortCallback));
            private bool persistEnabled;
            private IOThreadTimer persistTimer;
            private TimeSpan timeToPersist;
            private TimeSpan timeToUnload;
            private bool unloadEnabled;
            private IOThreadTimer unloadTimer;

            public UnloadInstancePolicyHelper(WorkflowServiceInstance instance, TimeSpan timeToPersist, TimeSpan timeToUnload)
            {
                this.instance = instance;
                this.timeToPersist = timeToPersist;
                this.timeToUnload = timeToUnload;
                this.persistEnabled = this.instance.persistenceContext.CanPersist && (this.timeToPersist < this.timeToUnload);
                this.unloadEnabled = this.instance.persistenceContext.CanPersist && (this.timeToUnload < TimeSpan.MaxValue);
                if (this.persistEnabled)
                {
                    this.persistTimer = new IOThreadTimer(onTimerCallback, new Action(this.Persist), true);
                }
                if (this.unloadEnabled)
                {
                    this.unloadTimer = new IOThreadTimer(onTimerCallback, new Action(this.Unload), true);
                }
            }

            public void Begin()
            {
                if (this.cancelled)
                {
                    this.cancelled = false;
                    if (this.persistEnabled)
                    {
                        this.persistTimer.Set(this.timeToPersist);
                    }
                    else if (this.instance.persistenceContext.CanPersist)
                    {
                        this.instance.DecrementBusyCount();
                        if (this.unloadEnabled)
                        {
                            this.unloadTimer.Set(this.timeToUnload);
                        }
                    }
                }
            }

            private IAsyncResult BeginUnlockAndAbort(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new UnlockAndAbortAsyncResult(this.instance, timeout, callback, state);
            }

            public void Cancel()
            {
                this.cancelled = true;
                if (this.persistTimer != null)
                {
                    this.persistTimer.Cancel();
                }
                if (this.unloadTimer != null)
                {
                    this.unloadTimer.Cancel();
                }
            }

            private void EndUnlockAndAbort(IAsyncResult result)
            {
                UnlockAndAbortAsyncResult.End(result);
            }

            private void HandleEndPersist(IAsyncResult result)
            {
                this.instance.EndPersist(result);
                if (!this.cancelled && this.instance.persistenceContext.CanPersist)
                {
                    this.instance.DecrementBusyCount();
                    if (this.unloadEnabled)
                    {
                        if (this.unloadTimer != null)
                        {
                            this.unloadTimer.Set(this.timeToUnload - this.timeToPersist);
                        }
                        else
                        {
                            this.Unload();
                        }
                    }
                }
            }

            private void HandleEndUnload(IAsyncResult result)
            {
                this.instance.EndReleaseInstance(result);
            }

            private static void OnTimerCallback(object state)
            {
                try
                {
                    ((Action) state)();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                }
            }

            private void Persist()
            {
                try
                {
                    IAsyncResult result = this.instance.BeginPersist(true, TimeSpan.MaxValue, onPersistCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        this.HandleEndPersist(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(exception, false);
                }
            }

            private static void PersistCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper asyncState = (WorkflowServiceInstance.UnloadInstancePolicyHelper) result.AsyncState;
                    try
                    {
                        asyncState.HandleEndPersist(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.instance.AbortInstance(exception, false);
                    }
                }
            }

            private void Unload()
            {
                try
                {
                    if (this.persistEnabled)
                    {
                        IAsyncResult result = this.BeginUnlockAndAbort(TimeSpan.MaxValue, onUnlockAndAbortCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            this.EndUnlockAndAbort(result);
                        }
                    }
                    else
                    {
                        IAsyncResult result2 = this.instance.BeginReleaseInstance(true, TimeSpan.MaxValue, onUnloadCallback, this);
                        if (result2.CompletedSynchronously)
                        {
                            this.HandleEndUnload(result2);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(exception, false);
                }
            }

            private static void UnloadCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper asyncState = (WorkflowServiceInstance.UnloadInstancePolicyHelper) result.AsyncState;
                    try
                    {
                        asyncState.HandleEndUnload(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.instance.AbortInstance(exception, false);
                    }
                }
            }

            private static void UnlockAndAbortCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper asyncState = (WorkflowServiceInstance.UnloadInstancePolicyHelper) result.AsyncState;
                    try
                    {
                        asyncState.EndUnlockAndAbort(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.instance.AbortInstance(exception, false);
                    }
                }
            }

            private class UnlockAndAbortAsyncResult : AsyncResult
            {
                private static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult.AcquireCompletedCallback);
                private static AsyncResult.AsyncCompletion handleEndAbandon;
                private WorkflowServiceInstance instance;
                private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult.Finally);
                private bool referenceAcquired;
                private TimeoutHelper timeoutHelper;

                public UnlockAndAbortAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.instance = instance;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    base.OnCompleting = onCompleting;
                    Exception exception = null;
                    bool flag = true;
                    if (this.instance.acquireReferenceSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), acquireCompletedCallback, this))
                    {
                        try
                        {
                            flag = this.HandleEndAcquireReference();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        base.Complete(true, exception);
                    }
                }

                private static void AcquireCompletedCallback(object state, Exception completionException)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult result = (WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult) state;
                    bool flag = true;
                    if (completionException == null)
                    {
                        try
                        {
                            flag = result.HandleEndAcquireReference();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            completionException = exception;
                        }
                    }
                    if (flag)
                    {
                        result.Complete(false, completionException);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult>(result);
                }

                private static void Finally(AsyncResult result, Exception completionException)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult result2 = (WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult) result;
                    if (result2.referenceAcquired)
                    {
                        result2.ReleaseAcquiredReference();
                    }
                }

                private static bool HandleEndAbandon(IAsyncResult result)
                {
                    WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult asyncState = (WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult) result.AsyncState;
                    asyncState.instance.EndAbandon(result);
                    return asyncState.ReleaseAcquiredReference();
                }

                private bool HandleEndAcquireReference()
                {
                    this.referenceAcquired = true;
                    if (!this.instance.TryReleaseLastReference())
                    {
                        return true;
                    }
                    if (handleEndAbandon == null)
                    {
                        handleEndAbandon = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadInstancePolicyHelper.UnlockAndAbortAsyncResult.HandleEndAbandon);
                    }
                    IAsyncResult result = this.instance.BeginAbandon(new FaultException(OperationExecutionFault.CreateAbortedFault(System.ServiceModel.Activities.SR.DefaultAbortReason)), false, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndAbandon), this);
                    return base.SyncContinue(result);
                }

                private bool ReleaseAcquiredReference()
                {
                    this.instance.acquireReferenceSemaphore.Exit();
                    this.referenceAcquired = false;
                    return true;
                }
            }
        }

        private class UnloadOrPersistAsyncResult : AsyncResult
        {
            private static Action<AsyncResult, Exception> completeCallback = new Action<AsyncResult, Exception>(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnComplete);
            private static AsyncResult.AsyncCompletion completeContextCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnCompleteContext);
            private WorkflowServiceInstance.WorkflowPersistenceContext context;
            private IDictionary<XName, InstanceValue> data;
            private DependentTransaction dependentTransaction;
            private WorkflowServiceInstance instance;
            private bool isCompletionTransactionRequired;
            private bool isIdlePolicyPersist;
            private bool isTry;
            private bool isUnloaded;
            private bool isWorkflowThread;
            private static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnLockAcquired);
            private AsyncResult.AsyncCompletion nextInnerAsyncCompletion;
            private static AsyncResult.AsyncCompletion notifyCompletionCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnNotifyCompletion);
            private WorkflowServiceInstance.PersistenceOperation operation;
            private static AsyncResult.AsyncCompletion outermostCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OutermostCallback);
            private bool ownsLock;
            private static AsyncResult.AsyncCompletion persistedCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnPersisted);
            private PersistencePipeline pipeline;
            private static AsyncResult.AsyncCompletion providerOpenedCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnProviderOpened);
            private static AsyncResult.AsyncCompletion savedCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnSaved);
            private SaveStatus saveStatus;
            private TimeoutHelper timeoutHelper;
            private static AsyncResult.AsyncCompletion trackingCompleteCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnTrackingComplete);
            private bool tryResult;
            private bool updateState;
            private static AsyncResult.AsyncCompletion waitForCanPersistCallback = new AsyncResult.AsyncCompletion(WorkflowServiceInstance.UnloadOrPersistAsyncResult.OnWaitForCanPersist);

            public UnloadOrPersistAsyncResult(WorkflowServiceInstance instance, WorkflowServiceInstance.PersistenceOperation operation, bool isWorkflowThread, bool isTry, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.operation = operation;
                this.isWorkflowThread = isWorkflowThread;
                this.isTry = isTry;
                this.tryResult = true;
                this.isUnloaded = (operation == WorkflowServiceInstance.PersistenceOperation.Unload) || (operation == WorkflowServiceInstance.PersistenceOperation.Delete);
                this.saveStatus = SaveStatus.Locked;
                this.isCompletionTransactionRequired = ((this.isUnloaded && (instance.Controller.State == WorkflowInstanceState.Complete)) && (instance.creationContext != null)) && instance.creationContext.IsCompletionTransactionRequired;
                this.isIdlePolicyPersist = isTry && (operation == WorkflowServiceInstance.PersistenceOperation.Save);
                if (operation == WorkflowServiceInstance.PersistenceOperation.Unload)
                {
                    this.saveStatus = SaveStatus.Unlocked;
                }
                else if (operation == WorkflowServiceInstance.PersistenceOperation.Delete)
                {
                    this.saveStatus = SaveStatus.Completed;
                }
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    base.OnCompleting = completeCallback;
                    this.dependentTransaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
                bool flag = true;
                bool flag2 = false;
                try
                {
                    if (this.isWorkflowThread)
                    {
                        flag = this.OpenProvider();
                    }
                    else
                    {
                        try
                        {
                            flag = this.LockAndPassGuard();
                        }
                        finally
                        {
                            if (flag)
                            {
                                this.instance.ReleaseLock(ref this.ownsLock, this.isIdlePolicyPersist);
                            }
                        }
                    }
                    flag2 = true;
                }
                finally
                {
                    if (!flag2 && (this.dependentTransaction != null))
                    {
                        this.dependentTransaction.Complete();
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool CollectAndMap()
            {
                this.updateState = true;
                Dictionary<XName, InstanceValue> initialValues = this.instance.GeneratePersistenceData();
                bool flag = false;
                try
                {
                    List<IPersistencePipelineModule> pipelineModules = this.instance.PipelineModules;
                    if (pipelineModules != null)
                    {
                        this.pipeline = new PersistencePipeline(pipelineModules, initialValues);
                        this.pipeline.Collect();
                        this.pipeline.Map();
                        this.data = this.pipeline.Values;
                    }
                    else
                    {
                        this.data = initialValues;
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag && (this.context != null))
                    {
                        this.context.Abort();
                    }
                }
                if (this.instance.persistenceContext != null)
                {
                    return this.Persist();
                }
                return this.Save();
            }

            private bool CompleteContext()
            {
                bool flag = false;
                IAsyncResult result = null;
                if (this.context != null)
                {
                    flag = this.context.TryBeginComplete(this.PrepareInnerAsyncCompletion(completeContextCallback), this, out result);
                }
                this.instance.hasPersistedDeleted = this.operation == WorkflowServiceInstance.PersistenceOperation.Delete;
                if (flag)
                {
                    return base.SyncContinue(result);
                }
                return true;
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<WorkflowServiceInstance.UnloadOrPersistAsyncResult>(result).tryResult;
            }

            private AsyncResult.AsyncCompletion GetNextInnerAsyncCompletion()
            {
                AsyncResult.AsyncCompletion nextInnerAsyncCompletion = this.nextInnerAsyncCompletion;
                this.nextInnerAsyncCompletion = null;
                return nextInnerAsyncCompletion;
            }

            private bool LockAndPassGuard()
            {
                return (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this) && this.PassGuard());
            }

            private bool NotifyCompletion()
            {
                if ((!this.isUnloaded || (this.instance.Controller.State != WorkflowInstanceState.Complete)) || (this.instance.creationContext == null))
                {
                    return this.CompleteContext();
                }
                IAsyncResult result = null;
                try
                {
                    if (this.context == null)
                    {
                        this.context = new WorkflowServiceInstance.WorkflowPersistenceContext(this.instance, this.isCompletionTransactionRequired, this.dependentTransaction, this.instance.persistTimeout);
                    }
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.instance.creationContext.OnBeginWorkflowCompleted(this.instance.completionState, this.instance.workflowOutputs, this.instance.terminationException, this.timeoutHelper.RemainingTime(), this.PrepareInnerAsyncCompletion(notifyCompletionCallback), this);
                        if (result == null)
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.WorkflowCompletionAsyncResultCannotBeNull));
                        }
                    }
                }
                finally
                {
                    if ((result == null) && (this.context != null))
                    {
                        this.context.Abort();
                    }
                }
                return base.SyncContinue(result);
            }

            private static void OnComplete(AsyncResult result, Exception exception)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult result2 = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result;
                if (result2.dependentTransaction != null)
                {
                    result2.dependentTransaction.Complete();
                }
            }

            private static bool OnCompleteContext(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.context.EndComplete(result);
                return true;
            }

            private static void OnLockAcquired(object state, Exception asyncException)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult result = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    result.ownsLock = true;
                    bool flag = true;
                    Exception exception = null;
                    try
                    {
                        flag = result.PassGuard();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    finally
                    {
                        if (flag)
                        {
                            result.instance.ReleaseLock(ref result.ownsLock, result.isIdlePolicyPersist);
                        }
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private static bool OnNotifyCompletion(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.instance.creationContext.OnEndWorkflowCompleted(result);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        asyncState.context.Abort();
                    }
                }
                return asyncState.CompleteContext();
            }

            private static bool OnPersisted(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.instance.persistenceContext.EndSave(result);
                    flag = true;
                }
                catch (InstancePersistenceException)
                {
                    asyncState.updateState = false;
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        asyncState.context.Abort();
                    }
                }
                return asyncState.Save();
            }

            private static bool OnProviderOpened(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.persistenceContext.EndOpen(result);
                return asyncState.Track();
            }

            private static bool OnSaved(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.pipeline.EndSave(result);
                    flag = true;
                }
                finally
                {
                    asyncState.instance.persistencePipelineInUse = null;
                    if (!flag)
                    {
                        asyncState.context.Abort();
                    }
                }
                return asyncState.NotifyCompletion();
            }

            private static bool OnTrackingComplete(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.Controller.EndFlushTrackingRecords(result);
                return asyncState.CollectAndMap();
            }

            private static bool OnWaitForCanPersist(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.EndWaitForCanPersist(result, ref asyncState.ownsLock);
                return asyncState.OpenProvider();
            }

            private bool OpenProvider()
            {
                if (this.operation == WorkflowServiceInstance.PersistenceOperation.Unload)
                {
                    if (((this.instance.state != WorkflowServiceInstance.State.Suspended) && !this.instance.IsIdle) && this.isTry)
                    {
                        this.tryResult = false;
                        return true;
                    }
                    if (!this.instance.TryReleaseLastReference() && this.isTry)
                    {
                        this.tryResult = false;
                        return true;
                    }
                }
                if ((this.operation == WorkflowServiceInstance.PersistenceOperation.Unload) && (this.instance.Controller.State == WorkflowInstanceState.Complete))
                {
                    this.operation = WorkflowServiceInstance.PersistenceOperation.Delete;
                }
                bool flag = false;
                if ((this.instance.persistenceContext != null) && (this.instance.persistenceContext.State == CommunicationState.Created))
                {
                    IAsyncResult result = this.instance.persistenceContext.BeginOpen(this.timeoutHelper.RemainingTime(), this.PrepareInnerAsyncCompletion(providerOpenedCallback), this);
                    if (result.CompletedSynchronously)
                    {
                        flag = OnProviderOpened(result);
                    }
                    return flag;
                }
                return this.Track();
            }

            private static bool OutermostCallback(IAsyncResult result)
            {
                WorkflowServiceInstance.UnloadOrPersistAsyncResult asyncState = (WorkflowServiceInstance.UnloadOrPersistAsyncResult) result.AsyncState;
                bool flag = true;
                AsyncResult.AsyncCompletion nextInnerAsyncCompletion = asyncState.GetNextInnerAsyncCompletion();
                try
                {
                    flag = nextInnerAsyncCompletion(result);
                }
                finally
                {
                    if (flag)
                    {
                        if (asyncState.updateState)
                        {
                            if (asyncState.saveStatus != SaveStatus.Locked)
                            {
                                asyncState.instance.isRunnable = false;
                            }
                            if (asyncState.isUnloaded)
                            {
                                asyncState.instance.MarkUnloaded();
                            }
                        }
                        if (!asyncState.isWorkflowThread)
                        {
                            asyncState.instance.ReleaseLock(ref asyncState.ownsLock, asyncState.isIdlePolicyPersist);
                        }
                    }
                }
                return flag;
            }

            private bool PassGuard()
            {
                if (this.operation == WorkflowServiceInstance.PersistenceOperation.Unload)
                {
                    if (!this.instance.ValidateStateForUnload())
                    {
                        return true;
                    }
                }
                else
                {
                    this.instance.ValidateStateForPersist();
                }
                if (this.instance.Controller.IsPersistable)
                {
                    return this.OpenProvider();
                }
                if (this.isTry)
                {
                    this.tryResult = false;
                    return true;
                }
                IAsyncResult result = this.instance.BeginWaitForCanPersist(ref this.ownsLock, this.timeoutHelper.RemainingTime(), this.PrepareInnerAsyncCompletion(waitForCanPersistCallback), this);
                return (result.CompletedSynchronously && OnWaitForCanPersist(result));
            }

            private bool Persist()
            {
                IAsyncResult result = null;
                try
                {
                    if (this.operation == WorkflowServiceInstance.PersistenceOperation.Delete)
                    {
                        this.saveStatus = SaveStatus.Completed;
                    }
                    if (this.context == null)
                    {
                        this.context = new WorkflowServiceInstance.WorkflowPersistenceContext(this.instance, ((this.pipeline != null) && this.pipeline.IsSaveTransactionRequired) || this.isCompletionTransactionRequired, this.dependentTransaction, this.instance.persistTimeout);
                    }
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.instance.persistenceContext.BeginSave(this.data, this.saveStatus, this.instance.persistTimeout, this.PrepareInnerAsyncCompletion(persistedCallback), this);
                    }
                }
                catch (InstancePersistenceException)
                {
                    this.updateState = false;
                    throw;
                }
                finally
                {
                    if ((result == null) && (this.context != null))
                    {
                        this.context.Abort();
                    }
                }
                return base.SyncContinue(result);
            }

            private AsyncCallback PrepareInnerAsyncCompletion(AsyncResult.AsyncCompletion innerCallback)
            {
                this.nextInnerAsyncCompletion = innerCallback;
                return base.PrepareAsyncCompletion(outermostCallback);
            }

            private bool Save()
            {
                if (this.pipeline == null)
                {
                    return this.NotifyCompletion();
                }
                IAsyncResult result = null;
                try
                {
                    if (this.context == null)
                    {
                        this.context = new WorkflowServiceInstance.WorkflowPersistenceContext(this.instance, this.pipeline.IsSaveTransactionRequired || this.isCompletionTransactionRequired, this.dependentTransaction, this.instance.persistTimeout);
                    }
                    this.instance.persistencePipelineInUse = this.pipeline;
                    Thread.MemoryBarrier();
                    if (this.instance.abortingExtensions)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.DefaultAbortReason));
                    }
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.pipeline.BeginSave(this.timeoutHelper.RemainingTime(), this.PrepareInnerAsyncCompletion(savedCallback), this);
                    }
                }
                finally
                {
                    if (result == null)
                    {
                        this.instance.persistencePipelineInUse = null;
                        if (this.context != null)
                        {
                            this.context.Abort();
                        }
                    }
                }
                return base.SyncContinue(result);
            }

            private bool Track()
            {
                if (this.instance.persistenceContext != null)
                {
                    this.instance.TrackPersistence(this.operation);
                }
                if (this.instance.Controller.HasPendingTrackingRecords)
                {
                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, this.PrepareInnerAsyncCompletion(trackingCompleteCallback), this);
                    return base.SyncContinue(result);
                }
                return this.CollectAndMap();
            }
        }

        private class UnsuspendAsyncResult : WorkflowServiceInstance.SimpleOperationAsyncResult
        {
            private UnsuspendAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state) : base(instance, transaction, callback, state)
            {
            }

            public static WorkflowServiceInstance.UnsuspendAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                WorkflowServiceInstance.UnsuspendAsyncResult result = new WorkflowServiceInstance.UnsuspendAsyncResult(instance, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceInstance.UnsuspendAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                if (!base.Instance.isInTransaction)
                {
                    base.Instance.isRunnable = true;
                }
                base.Instance.persistenceContext.IsSuspended = false;
                base.Instance.persistenceContext.SuspendedReason = null;
                base.Instance.state = WorkflowServiceInstance.State.Active;
                if (base.Instance.Controller.TrackingEnabled)
                {
                    base.Instance.Controller.Track(new WorkflowInstanceRecord(base.Instance.Id, base.Instance.WorkflowDefinition.DisplayName, "Unsuspended"));
                }
            }

            protected override void PostOperation()
            {
            }

            protected override bool ValidateState()
            {
                return base.Instance.ValidateStateForUnsuspend(base.OperationTransaction);
            }
        }

        private class WaitForCanPersistAsyncResult : AsyncResult
        {
            private AsyncWaitHandle checkCanPersistEvent;
            private WorkflowServiceInstance instance;
            private bool mustWait;
            private static FastAsyncCallback onLockAcquired;
            private static Action<object, TimeoutException> onWaitEvent;
            private bool ownsLock;
            private TimeoutHelper timeoutHelper;

            public WaitForCanPersistAsyncResult(WorkflowServiceInstance instance, ref bool ownsLock, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.ownsLock = ownsLock;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.WaitForCanPersist())
                {
                    base.Complete(true);
                }
            }

            private bool AcquireLockWithoutPause()
            {
                if (this.instance.IsHandlerThread || this.ownsLock)
                {
                    return this.HandleLockAcquired();
                }
                if (onLockAcquired == null)
                {
                    onLockAcquired = new FastAsyncCallback(WorkflowServiceInstance.WaitForCanPersistAsyncResult.OnLockAcquired);
                }
                return (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), false, true, ref this.ownsLock, onLockAcquired, this) && this.HandleLockAcquired());
            }

            public static void End(IAsyncResult result, ref bool ownsLock)
            {
                WorkflowServiceInstance.WaitForCanPersistAsyncResult result2 = result as WorkflowServiceInstance.WaitForCanPersistAsyncResult;
                if (result2 != null)
                {
                    ownsLock = result2.ownsLock;
                }
                AsyncResult.End<WorkflowServiceInstance.WaitForCanPersistAsyncResult>(result);
            }

            private bool HandleLockAcquired()
            {
                this.instance.ValidateStateForPersist();
                return this.WaitForCanPersist();
            }

            private bool HandleWaitEvent()
            {
                return this.AcquireLockWithoutPause();
            }

            private static void OnLockAcquired(object state, Exception asyncException)
            {
                WorkflowServiceInstance.WaitForCanPersistAsyncResult result = (WorkflowServiceInstance.WaitForCanPersistAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    result.ownsLock = true;
                    bool flag = true;
                    Exception exception = null;
                    try
                    {
                        flag = result.HandleLockAcquired();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private static void OnWaitEvent(object state, TimeoutException asyncException)
            {
                WorkflowServiceInstance.WaitForCanPersistAsyncResult result = (WorkflowServiceInstance.WaitForCanPersistAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    bool flag = true;
                    Exception exception = null;
                    try
                    {
                        flag = result.HandleWaitEvent();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            public void SetEvent(ref bool ownsLock)
            {
                this.ownsLock = ownsLock;
                ownsLock = false;
                this.checkCanPersistEvent.Set();
            }

            private bool WaitForCanPersist()
            {
                if (this.instance.Controller.IsPersistable)
                {
                    return true;
                }
                this.instance.Controller.PauseWhenPersistable();
                this.mustWait = false;
                if (this.instance.IsIdle)
                {
                    if (this.checkCanPersistEvent == null)
                    {
                        this.checkCanPersistEvent = new AsyncWaitHandle(EventResetMode.AutoReset);
                    }
                    this.instance.AddCheckCanPersistWaiter(this);
                    this.mustWait = true;
                }
                this.instance.ReleaseLock(ref this.ownsLock);
                if (!this.mustWait)
                {
                    return this.HandleWaitEvent();
                }
                if (onWaitEvent == null)
                {
                    onWaitEvent = new Action<object, TimeoutException>(WorkflowServiceInstance.WaitForCanPersistAsyncResult.OnWaitEvent);
                }
                return (this.checkCanPersistEvent.WaitAsync(onWaitEvent, this, this.timeoutHelper.RemainingTime()) && this.HandleWaitEvent());
            }
        }

        private class WorkflowExecutionLock
        {
            private static Action<object, TimeoutException> asyncWaiterSignaledCallback = new Action<object, TimeoutException>(WorkflowServiceInstance.WorkflowExecutionLock.OnAsyncWaiterSignaled);
            private WorkflowServiceInstance instance;
            private bool owned;
            private object ThisLock = new object();
            private List<object> waiters;

            public WorkflowExecutionLock(WorkflowServiceInstance instance)
            {
                this.instance = instance;
            }

            public void CleanupWaiter(object token, ref bool ownsLock)
            {
                if (token != null)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.waiters.Remove(token))
                        {
                            ownsLock = true;
                        }
                    }
                }
            }

            public void Enter(TimeSpan timeout, ref object token, ref bool ownsLock)
            {
                if (!this.TryEnter(timeout, ref token, ref ownsLock))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new TimeoutException(System.ServiceModel.Activities.SR.TimeoutOnOperation(timeout)));
                }
            }

            public bool EnterAsync(TimeSpan timeout, ref object token, ref bool ownsLock, FastAsyncCallback callback, object state)
            {
                AsyncWaitHandle handle = null;
                lock (this.ThisLock)
                {
                    if (!this.owned)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.owned = true;
                            ownsLock = true;
                        }
                        return true;
                    }
                    handle = (AsyncWaitHandle) token;
                }
                bool flag = false;
                if (handle.WaitAsync(asyncWaiterSignaledCallback, new AsyncWaiterData(this, callback, state, handle), timeout))
                {
                    ownsLock = true;
                    flag = true;
                }
                token = null;
                return flag;
            }

            private AsyncWaitHandle EnterCore(ref object token, ref bool ownsLock)
            {
                AsyncWaitHandle item = null;
                lock (this.ThisLock)
                {
                    if (this.owned)
                    {
                        if (token == null)
                        {
                            item = new AsyncWaitHandle();
                            this.Waiters.Add(item);
                            return item;
                        }
                        return (AsyncWaitHandle) token;
                    }
                    try
                    {
                    }
                    finally
                    {
                        this.owned = true;
                        ownsLock = true;
                    }
                }
                return item;
            }

            public bool Exit(bool keepLockIfNoWaiters, ref bool ownsLock)
            {
                AsyncWaitHandle handle = null;
                lock (this.ThisLock)
                {
                    if (!this.owned)
                    {
                        string invalidSemaphoreExit = SRCore.InvalidSemaphoreExit;
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new SynchronizationLockException(invalidSemaphoreExit));
                    }
                    if ((this.waiters == null) || (this.waiters.Count == 0))
                    {
                        if (keepLockIfNoWaiters)
                        {
                            return false;
                        }
                        try
                        {
                        }
                        finally
                        {
                            this.owned = false;
                            ownsLock = false;
                            this.instance.StartUnloadInstancePolicyIfNecessary();
                        }
                        return true;
                    }
                    handle = (AsyncWaitHandle) this.waiters[0];
                    this.waiters.RemoveAt(0);
                }
                handle.Set();
                ownsLock = false;
                return true;
            }

            private static void OnAsyncWaiterSignaled(object state, TimeoutException asyncException)
            {
                AsyncWaiterData data = (AsyncWaiterData) state;
                Exception exception = asyncException;
                if (asyncException != null)
                {
                    lock (data.Owner.ThisLock)
                    {
                        if (!data.Owner.waiters.Remove(data.Token))
                        {
                            exception = null;
                        }
                    }
                }
                data.Callback(data.State, exception);
            }

            public void SetupWaiter(ref object token)
            {
                this.SetupWaiter(false, ref token);
            }

            public void SetupWaiter(bool isAbortPriority, ref object token)
            {
                lock (this.ThisLock)
                {
                    try
                    {
                    }
                    finally
                    {
                        token = new AsyncWaitHandle();
                        if (isAbortPriority)
                        {
                            this.Waiters.Insert(0, token);
                        }
                        else
                        {
                            this.Waiters.Add(token);
                        }
                    }
                }
            }

            public bool TryEnter(ref bool ownsLock)
            {
                lock (this.ThisLock)
                {
                    if (!this.owned)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.owned = true;
                            ownsLock = true;
                        }
                        return true;
                    }
                    return false;
                }
            }

            public bool TryEnter(TimeSpan timeout, ref object token, ref bool ownsLock)
            {
                AsyncWaitHandle handle = this.EnterCore(ref token, ref ownsLock);
                if (handle == null)
                {
                    return true;
                }
                if (handle.Wait(timeout))
                {
                    ownsLock = true;
                    token = null;
                    return true;
                }
                return false;
            }

            public bool IsLocked
            {
                get
                {
                    return this.owned;
                }
            }

            private List<object> Waiters
            {
                get
                {
                    if (this.waiters == null)
                    {
                        this.waiters = new List<object>();
                    }
                    return this.waiters;
                }
            }

            private class AsyncWaiterData
            {
                public AsyncWaiterData(WorkflowServiceInstance.WorkflowExecutionLock owner, FastAsyncCallback callback, object state, object token)
                {
                    this.Owner = owner;
                    this.Callback = callback;
                    this.State = state;
                    this.Token = token;
                }

                public FastAsyncCallback Callback { get; private set; }

                public WorkflowServiceInstance.WorkflowExecutionLock Owner { get; private set; }

                public object State { get; private set; }

                public object Token { get; private set; }
            }
        }

        private class WorkflowPersistenceContext
        {
            private Transaction clonedTransaction;
            private CommittableTransaction contextOwnedTransaction;
            private WorkflowServiceInstance instance;

            public WorkflowPersistenceContext(WorkflowServiceInstance instance, bool transactionRequired, Transaction transactionToUse, TimeSpan transactionTimeout)
            {
                this.instance = instance;
                if (transactionToUse != null)
                {
                    this.clonedTransaction = transactionToUse;
                }
                else if (transactionRequired)
                {
                    this.contextOwnedTransaction = new CommittableTransaction(transactionTimeout);
                    this.clonedTransaction = this.contextOwnedTransaction.Clone();
                }
            }

            public void Abort()
            {
                if (this.contextOwnedTransaction != null)
                {
                    try
                    {
                        this.contextOwnedTransaction.Rollback();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                    }
                }
            }

            public void EndComplete(IAsyncResult result)
            {
                this.contextOwnedTransaction.EndCommit(result);
            }

            public bool TryBeginComplete(AsyncCallback callback, object state, out IAsyncResult result)
            {
                if (this.contextOwnedTransaction != null)
                {
                    result = this.contextOwnedTransaction.BeginCommit(callback, state);
                    return true;
                }
                result = null;
                return false;
            }

            public Transaction PublicTransaction
            {
                get
                {
                    return this.clonedTransaction;
                }
            }
        }
    }
}

