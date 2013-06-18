namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.Hosting;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions;

    [DataContract(Name="Executor", Namespace="http://schemas.datacontract.org/2010/02/System.Activities")]
    internal class ActivityExecutor : IEnlistmentNotification
    {
        private Dictionary<System.Activities.ActivityInstance, AsyncOperationContext> activeOperations;
        [DataMember(Name="bookmarkMgr", EmitDefaultValue=false)]
        private BookmarkManager bookmarkManager;
        [DataMember(Name="bookmarkScopeManager", EmitDefaultValue=false)]
        private System.Activities.Runtime.BookmarkScopeManager bookmarkScopeManager;
        private ActivityContext cachedResolutionContext;
        private Pool<CodeActivityContext> codeActivityContextPool;
        [DataMember(Name="completionException", EmitDefaultValue=false)]
        private Exception completionException;
        private Pool<CompletionCallbackWrapper.CompletionWorkItem> completionWorkItemPool;
        private DebugController debugController;
        private static ReadOnlyCollection<BookmarkInfo> emptyBookmarkInfoCollection;
        private Pool<EmptyWorkItem> emptyWorkItemPool;
        private Pool<ExecuteActivityWorkItem> executeActivityWorkItemPool;
        private List<System.Activities.ActivityInstance> executingSecondaryRootInstances;
        [DataMember(Name="state", EmitDefaultValue=false)]
        private ActivityInstanceState executionState;
        [DataMember(EmitDefaultValue=false)]
        private List<Handle> handles;
        private bool hasRaisedWorkflowStarted;
        [DataMember(EmitDefaultValue=false)]
        private bool hasTrackedStarted;
        private WorkflowInstance host;
        private Guid instanceId;
        private bool instanceIdSet;
        private ActivityInstanceMap instanceMap;
        private bool isAbortPending;
        private bool isDisposed;
        private bool isTerminatePending;
        [DataMember(Name="lastInstanceId", EmitDefaultValue=false)]
        private long lastInstanceId;
        [DataMember(Name="mainRootCompleteBookmark", EmitDefaultValue=false)]
        private Bookmark mainRootCompleteBookmark;
        private System.Activities.Runtime.MappableObjectManager mappableObjectManager;
        private Pool<NativeActivityContext> nativeActivityContextPool;
        [DataMember(EmitDefaultValue=false)]
        private long nextTrackingRecordNumber;
        private int noPersistCount;
        private Queue<PersistenceWaiter> persistenceWaiters;
        private Pool<ResolveNextArgumentWorkItem> resolveNextArgumentWorkItemPool;
        private Activity rootElement;
        [DataMember(Name="rootEnvironment", EmitDefaultValue=false)]
        private LocationEnvironment rootEnvironment;
        [DataMember(Name="rootInstance", EmitDefaultValue=false)]
        private System.Activities.ActivityInstance rootInstance;
        private ExecutionPropertyManager rootPropertyManager;
        private RuntimeTransactionData runtimeTransaction;
        [DataMember(Name="scheduler", EmitDefaultValue=false)]
        private Scheduler scheduler;
        private bool shouldPauseOnCanPersist;
        [DataMember(Name="shouldRaiseMainBodyComplete", EmitDefaultValue=false)]
        private bool shouldRaiseMainBodyComplete;
        private System.Activities.Hosting.SymbolResolver symbolResolver;
        private Exception terminationPendingException;
        private Quack<TransactionContextWaiter> transactionContextWaiters;
        [DataMember(Name="workflowOutputs", EmitDefaultValue=false)]
        private IDictionary<string, object> workflowOutputs;

        public ActivityExecutor(WorkflowInstance host)
        {
            this.host = host;
            this.bookmarkManager = new BookmarkManager();
            this.scheduler = new Scheduler(new Scheduler.Callbacks(this));
        }

        public bool Abort(Exception reason)
        {
            Guid guid;
            bool hasBeenResumed = this.TryTraceResume(out guid);
            bool flag2 = this.Abort(reason, false);
            this.TraceSuspend(hasBeenResumed, guid);
            return flag2;
        }

        private bool Abort(Exception terminationException, bool isTerminate)
        {
            if (this.isDisposed)
            {
                return false;
            }
            if (!this.rootInstance.IsCompleted)
            {
                this.rootInstance.Abort(this, this.bookmarkManager, terminationException, isTerminate);
                if (this.rootPropertyManager != null)
                {
                    if (isTerminate)
                    {
                        HandleInitializationContext context = new HandleInitializationContext(this, null);
                        foreach (ExecutionPropertyManager.ExecutionProperty property in this.rootPropertyManager.Properties.Values)
                        {
                            Handle handle = property.Property as Handle;
                            if (handle != null)
                            {
                                handle.Uninitialize(context);
                            }
                        }
                        context.Dispose();
                    }
                    this.rootPropertyManager.UnregisterProperties(null, null, true);
                }
            }
            if (this.executingSecondaryRootInstances != null)
            {
                for (int i = this.executingSecondaryRootInstances.Count - 1; i >= 0; i--)
                {
                    this.executingSecondaryRootInstances[i].Abort(this, this.bookmarkManager, terminationException, isTerminate);
                }
            }
            this.scheduler.ClearAllWorkItems(this);
            if (isTerminate)
            {
                this.completionException = terminationException;
                this.executionState = ActivityInstanceState.Faulted;
            }
            this.Dispose();
            return true;
        }

        internal void AbortActivityInstance(System.Activities.ActivityInstance instance, Exception reason)
        {
            instance.Abort(this, this.bookmarkManager, reason, true);
            if (instance.CompletionBookmark != null)
            {
                instance.CompletionBookmark.CheckForCancelation();
            }
            else if (instance.Parent != null)
            {
                instance.CompletionBookmark = new CompletionBookmark();
            }
            this.ScheduleCompletionBookmark(instance);
        }

        public void AbortWorkflowInstance(Exception reason)
        {
            this.isAbortPending = true;
            this.host.Abort(reason);
            try
            {
                this.host.OnRequestAbort(reason);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw FxTrace.Exception.AsError(new CallbackException(System.Activities.SR.CallbackExceptionFromHostAbort(this.WorkflowInstanceId), exception));
            }
        }

        internal void AddHandle(Handle handleToAdd)
        {
            if (this.handles == null)
            {
                this.handles = new List<Handle>();
            }
            this.handles.Add(handleToAdd);
        }

        public void AddTrackingRecord(TrackingRecord record)
        {
            this.host.TrackingProvider.AddRecord(record);
        }

        internal IAsyncResult BeginAssociateKeys(ICollection<InstanceKey> keysToAssociate, AsyncCallback callback, object state)
        {
            return new AssociateKeysAsyncResult(this, keysToAssociate, callback, state);
        }

        internal IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.host.OnBeginResumeBookmark(bookmark, value, timeout, callback, state);
        }

        public IAsyncResult BeginTrackPendingRecords(AsyncCallback callback, object state)
        {
            return this.host.BeginFlushTrackingRecords(callback, state);
        }

        public void CancelActivity(System.Activities.ActivityInstance activityInstance)
        {
            if ((activityInstance.State == ActivityInstanceState.Executing) && !activityInstance.IsCancellationRequested)
            {
                activityInstance.IsCancellationRequested = true;
                if (activityInstance.HasNotExecuted)
                {
                    this.scheduler.PushWork(this.CreateEmptyWorkItem(activityInstance));
                }
                else
                {
                    this.scheduler.PushWork(new CancelActivityWorkItem(activityInstance));
                }
                if (this.ShouldTrackCancelRequestedRecords)
                {
                    this.AddTrackingRecord(new CancelRequestedRecord(this.WorkflowInstanceId, activityInstance.Parent, activityInstance));
                }
            }
        }

        internal void CancelPendingOperation(System.Activities.ActivityInstance instance)
        {
            AsyncOperationContext context;
            if (this.TryGetPendingOperation(instance, out context) && context.IsStillActive)
            {
                context.CancelOperation();
            }
        }

        public void CancelRootActivity()
        {
            if (this.rootInstance.State == ActivityInstanceState.Executing)
            {
                if (!this.rootInstance.IsCancellationRequested)
                {
                    Guid guid;
                    bool hasBeenResumed = this.TryTraceResume(out guid);
                    bool flag2 = true;
                    if ((this.runtimeTransaction != null) && (this.runtimeTransaction.IsolationScope != null))
                    {
                        if (this.runtimeTransaction.IsRootCancelPending)
                        {
                            flag2 = false;
                        }
                        this.runtimeTransaction.IsRootCancelPending = true;
                    }
                    else
                    {
                        this.rootInstance.IsCancellationRequested = true;
                        if (this.rootInstance.HasNotExecuted)
                        {
                            this.scheduler.PushWork(this.CreateEmptyWorkItem(this.rootInstance));
                        }
                        else
                        {
                            this.scheduler.PushWork(new CancelActivityWorkItem(this.rootInstance));
                        }
                    }
                    if (this.ShouldTrackCancelRequestedRecords && flag2)
                    {
                        this.AddTrackingRecord(new CancelRequestedRecord(this.WorkflowInstanceId, null, this.rootInstance));
                    }
                    this.TraceSuspend(hasBeenResumed, guid);
                }
            }
            else if (this.rootInstance.State != ActivityInstanceState.Closed)
            {
                this.executionState = ActivityInstanceState.Canceled;
                this.completionException = null;
            }
        }

        private List<BookmarkInfo> CollectExternalBookmarks()
        {
            List<BookmarkInfo> bookmarks = null;
            if ((this.bookmarkManager != null) && this.bookmarkManager.HasBookmarks)
            {
                bookmarks = new List<BookmarkInfo>();
                this.bookmarkManager.PopulateBookmarkInfo(bookmarks);
            }
            if (this.bookmarkScopeManager != null)
            {
                this.bookmarkScopeManager.PopulateBookmarkInfo(ref bookmarks);
            }
            if ((bookmarks != null) && (bookmarks.Count != 0))
            {
                return bookmarks;
            }
            return null;
        }

        internal Exception CompleteActivityInstance(System.Activities.ActivityInstance targetInstance)
        {
            Exception exception = null;
            this.HandleRootCompletion(targetInstance);
            this.ScheduleCompletionBookmark(targetInstance);
            if (!targetInstance.HasNotExecuted)
            {
                this.DebugActivityCompleted(targetInstance);
            }
            try
            {
                if (targetInstance.PropertyManager != null)
                {
                    targetInstance.PropertyManager.UnregisterProperties(targetInstance, targetInstance.Activity.MemberOf);
                }
                if (this.IsSecondaryRoot(targetInstance))
                {
                    LocationEnvironment reference = targetInstance.Environment;
                    if (targetInstance.IsEnvironmentOwner)
                    {
                        reference.RemoveReference(true);
                        if (reference.ShouldDispose)
                        {
                            reference.UninitializeHandles(targetInstance);
                            reference.Dispose();
                        }
                        reference = reference.Parent;
                    }
                    while (reference != null)
                    {
                        reference.RemoveReference(false);
                        if (reference.ShouldDispose)
                        {
                            reference.UninitializeHandles(targetInstance);
                            reference.Dispose();
                            if (this.instanceMap != null)
                            {
                                this.instanceMap.RemoveEntry(reference);
                            }
                        }
                        reference = reference.Parent;
                    }
                }
                else if (targetInstance.IsEnvironmentOwner)
                {
                    targetInstance.Environment.RemoveReference(true);
                    if (targetInstance.Environment.ShouldDispose)
                    {
                        targetInstance.Environment.UninitializeHandles(targetInstance);
                        targetInstance.Environment.Dispose();
                    }
                    else if (this.instanceMap != null)
                    {
                        this.instanceMap.AddEntry(targetInstance.Environment);
                    }
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
            targetInstance.MarkAsComplete(this.bookmarkScopeManager, this.bookmarkManager);
            targetInstance.FinalizeState(this, exception != null);
            return exception;
        }

        public void CompleteOperation(System.Activities.ActivityInstance owningInstance)
        {
            this.CompleteOperation(owningInstance, true);
        }

        public void CompleteOperation(System.Activities.Runtime.WorkItem asyncCompletionWorkItem)
        {
            this.scheduler.EnqueueWork(asyncCompletionWorkItem);
            this.CompleteOperation(asyncCompletionWorkItem.ActivityInstance, false);
        }

        private void CompleteOperation(System.Activities.ActivityInstance owningInstance, bool exitNoPersist)
        {
            this.activeOperations.Remove(owningInstance);
            owningInstance.DecrementBusyCount();
            if (exitNoPersist)
            {
                this.ExitNoPersist();
            }
        }

        public void CompleteOperation(System.Activities.ActivityInstance owningInstance, BookmarkCallback callback, object state)
        {
            CompleteAsyncOperationWorkItem asyncCompletionWorkItem = new CompleteAsyncOperationWorkItem(new BookmarkCallbackWrapper(callback, owningInstance), this.bookmarkManager.GenerateTempBookmark(), state);
            this.CompleteOperation(asyncCompletionWorkItem);
        }

        public void CompleteTransaction(RuntimeTransactionHandle handle, BookmarkCallback callback, System.Activities.ActivityInstance callbackOwner)
        {
            if (callback != null)
            {
                ActivityExecutionWorkItem item;
                Bookmark bookmark = this.bookmarkManager.CreateBookmark(callback, callbackOwner, BookmarkOptions.None);
                System.Activities.ActivityInstance isolationInstance = null;
                if (this.runtimeTransaction != null)
                {
                    isolationInstance = this.runtimeTransaction.IsolationScope;
                }
                this.bookmarkManager.TryGenerateWorkItem(this, false, ref bookmark, null, isolationInstance, out item);
                this.scheduler.EnqueueWork(item);
            }
            if ((this.runtimeTransaction != null) && (this.runtimeTransaction.TransactionHandle == handle))
            {
                this.runtimeTransaction.ShouldScheduleCompletion = true;
                if (TD.RuntimeTransactionCompletionRequestedIsEnabled())
                {
                    TD.RuntimeTransactionCompletionRequested(callbackOwner.Activity.GetType().ToString(), callbackOwner.Activity.DisplayName, callbackOwner.Id);
                }
            }
        }

        internal ActivityInstanceReference CreateActivityInstanceReference(System.Activities.ActivityInstance toReference, System.Activities.ActivityInstance referenceOwner)
        {
            ActivityInstanceReference reference = new ActivityInstanceReference(toReference);
            if (this.instanceMap != null)
            {
                this.instanceMap.AddEntry(reference);
            }
            referenceOwner.AddActivityReference(reference);
            return reference;
        }

        public EmptyWorkItem CreateEmptyWorkItem(System.Activities.ActivityInstance instance)
        {
            EmptyWorkItem item = this.EmptyWorkItemPool.Acquire();
            item.Initialize(instance);
            return item;
        }

        public NoPersistProperty CreateNoPersistProperty()
        {
            return new NoPersistProperty(this);
        }

        private System.Activities.ActivityInstance CreateUninitalizedActivityInstance(Activity activity, System.Activities.ActivityInstance parent, CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            System.Activities.ActivityInstance item = new System.Activities.ActivityInstance(activity);
            if (parent != null)
            {
                item.CompletionBookmark = completionBookmark;
                item.FaultBookmark = faultBookmark;
                parent.AddChild(item);
            }
            if (this.lastInstanceId == 0x7fffffffffffffffL)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.OutOfInstanceIds));
            }
            this.lastInstanceId += 1L;
            return item;
        }

        public void DebugActivityCompleted(System.Activities.ActivityInstance instance)
        {
            if (this.debugController != null)
            {
                this.debugController.ActivityCompleted(instance);
            }
        }

        internal void DisassociateKeys(ICollection<InstanceKey> keysToDisassociate)
        {
            this.host.OnDisassociateKeys(keysToDisassociate);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool aborting)
        {
            if (!this.isDisposed)
            {
                if (this.debugController != null)
                {
                    this.debugController.WorkflowCompleted();
                    this.debugController = null;
                }
                if ((this.activeOperations != null) && (this.activeOperations.Count > 0))
                {
                    this.Abort(new OperationCanceledException());
                }
                else
                {
                    this.scheduler.ClearAllWorkItems(this);
                    if (!aborting)
                    {
                        this.scheduler = null;
                        this.bookmarkManager = null;
                        this.lastInstanceId = 0L;
                        this.rootInstance = null;
                    }
                    this.isDisposed = true;
                }
            }
        }

        internal void EndAssociateKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        internal BookmarkResumptionResult EndResumeBookmark(IAsyncResult result)
        {
            return this.host.OnEndResumeBookmark(result);
        }

        public void EndTrackPendingRecords(IAsyncResult result)
        {
            this.host.EndFlushTrackingRecords(result);
        }

        public void EnterNoPersist()
        {
            this.noPersistCount++;
            if (TD.EnterNoPersistBlockIsEnabled())
            {
                TD.EnterNoPersistBlock();
            }
        }

        public void ExitNoPersist()
        {
            this.noPersistCount--;
            if (TD.ExitNoPersistBlockIsEnabled())
            {
                TD.ExitNoPersistBlock();
            }
            if (this.shouldPauseOnCanPersist && this.IsPersistable)
            {
                this.scheduler.Pause();
            }
        }

        internal void FinishWorkItem(System.Activities.Runtime.WorkItem workItem)
        {
            Scheduler.RequestedAction yieldSilently = Scheduler.Continue;
            try
            {
                if (workItem.WorkflowAbortException != null)
                {
                    this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                }
                else
                {
                    workItem.PostProcess(this);
                    if (workItem.ExceptionToPropagate != null)
                    {
                        this.PropagateException(workItem);
                    }
                    if (this.HasPendingTrackingRecords && !workItem.FlushTracking(this))
                    {
                        yieldSilently = Scheduler.YieldSilently;
                        return;
                    }
                    if (workItem.WorkflowAbortException != null)
                    {
                        this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                    }
                    else
                    {
                        this.ScheduleRuntimeWorkItems();
                        if (workItem.ExceptionToPropagate != null)
                        {
                            yieldSilently = Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
                        }
                    }
                }
            }
            finally
            {
                if (yieldSilently != Scheduler.YieldSilently)
                {
                    workItem.Dispose(this);
                }
            }
            this.scheduler.InternalResume(yieldSilently);
        }

        internal void FinishWorkItemAfterTracking(System.Activities.Runtime.WorkItem workItem)
        {
            Scheduler.RequestedAction action = Scheduler.Continue;
            try
            {
                if (workItem.WorkflowAbortException != null)
                {
                    this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                }
                else
                {
                    this.ScheduleRuntimeWorkItems();
                    if (workItem.ExceptionToPropagate != null)
                    {
                        action = Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
                    }
                }
            }
            finally
            {
                workItem.Dispose(this);
            }
            this.scheduler.InternalResume(action);
        }

        internal IDictionary<string, LocationInfo> GatherMappableVariables()
        {
            if (this.mappableObjectManager != null)
            {
                return this.MappableObjectManager.GatherMappableVariables();
            }
            return null;
        }

        private void GatherRootOutputs()
        {
            if (this.rootInstance.State == ActivityInstanceState.Closed)
            {
                IList<RuntimeArgument> runtimeArguments = this.rootElement.RuntimeArguments;
                for (int i = 0; i < runtimeArguments.Count; i++)
                {
                    RuntimeArgument argument = runtimeArguments[i];
                    if (ArgumentDirectionHelper.IsOut(argument.Direction))
                    {
                        if (this.workflowOutputs == null)
                        {
                            this.workflowOutputs = new Dictionary<string, object>();
                        }
                        System.Activities.Location specificLocation = this.rootEnvironment.GetSpecificLocation(argument.BoundArgument.Id);
                        if (specificLocation == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.NoOutputLocationWasFound(argument.Name)));
                        }
                        this.workflowOutputs.Add(argument.Name, specificLocation.Value);
                    }
                }
            }
            this.rootEnvironment = null;
        }

        internal ReadOnlyCollection<BookmarkInfo> GetAllBookmarks()
        {
            List<BookmarkInfo> list = this.CollectExternalBookmarks();
            if (list != null)
            {
                return new ReadOnlyCollection<BookmarkInfo>(list);
            }
            return EmptyBookmarkInfoCollection;
        }

        internal ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
        {
            if (this.bookmarkScopeManager == null)
            {
                return EmptyBookmarkInfoCollection;
            }
            ReadOnlyCollection<BookmarkInfo> bookmarks = this.bookmarkScopeManager.GetBookmarks(scope);
            if (bookmarks == null)
            {
                return EmptyBookmarkInfoCollection;
            }
            return bookmarks;
        }

        public T GetExtension<T>() where T: class
        {
            T extension = default(T);
            try
            {
                extension = this.host.GetExtension<T>();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw FxTrace.Exception.AsError(new CallbackException(System.Activities.SR.CallbackExceptionFromHostGetExtension(this.WorkflowInstanceId), exception));
            }
            return extension;
        }

        public ActivityContext GetResolutionContext(System.Activities.ActivityInstance activityInstance)
        {
            if (this.cachedResolutionContext == null)
            {
                this.cachedResolutionContext = new ActivityContext(activityInstance, this);
            }
            else
            {
                this.cachedResolutionContext.Reinitialize(activityInstance, this);
            }
            return this.cachedResolutionContext;
        }

        internal void HandleRootCompletion(System.Activities.ActivityInstance completedInstance)
        {
            if (completedInstance.Parent == null)
            {
                if (completedInstance == this.rootInstance)
                {
                    this.shouldRaiseMainBodyComplete = true;
                    this.executionState = this.rootInstance.State;
                    this.rootEnvironment = this.rootInstance.Environment;
                }
                else
                {
                    this.executingSecondaryRootInstances.Remove(completedInstance);
                }
                if (this.rootInstance.IsCompleted && ((this.executingSecondaryRootInstances == null) || (this.executingSecondaryRootInstances.Count == 0)))
                {
                    this.GatherRootOutputs();
                    if (this.rootPropertyManager != null)
                    {
                        HandleInitializationContext context = new HandleInitializationContext(this, null);
                        foreach (ExecutionPropertyManager.ExecutionProperty property in this.rootPropertyManager.Properties.Values)
                        {
                            Handle handle = property.Property as Handle;
                            if (handle != null)
                            {
                                handle.Uninitialize(context);
                            }
                        }
                        context.Dispose();
                        this.rootPropertyManager.UnregisterProperties(null, null);
                    }
                }
            }
        }

        public bool IsCompletingTransaction(System.Activities.ActivityInstance instance)
        {
            if ((this.runtimeTransaction == null) || (this.runtimeTransaction.IsolationScope != instance))
            {
                return false;
            }
            this.scheduler.PushWork(this.CreateEmptyWorkItem(instance));
            this.runtimeTransaction.ShouldScheduleCompletion = true;
            if (TD.RuntimeTransactionCompletionRequestedIsEnabled())
            {
                TD.RuntimeTransactionCompletionRequested(instance.Activity.GetType().ToString(), instance.Activity.DisplayName, instance.Id);
            }
            return true;
        }

        private bool IsDebugged()
        {
            if ((this.debugController == null) && Debugger.IsAttached)
            {
                this.debugController = new DebugController(this.host);
            }
            return (this.debugController != null);
        }

        private bool IsSecondaryRoot(System.Activities.ActivityInstance instance)
        {
            return ((instance.Parent == null) && (instance != this.rootInstance));
        }

        public void MarkSchedulerRunning()
        {
            this.scheduler.MarkRunning();
        }

        internal void NotifyUnhandledException(Exception exception, System.Activities.ActivityInstance source)
        {
            try
            {
                this.host.NotifyUnhandledException(exception, source.Activity, source.Id);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.AbortWorkflowInstance(exception2);
            }
        }

        internal void OnDeserialized(Activity workflow, WorkflowInstance workflowInstance)
        {
            this.rootElement = workflow;
            this.host = workflowInstance;
            if (!this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.EmptyGuidOnDeserializedInstance));
            }
            if (this.host.Id != this.instanceId)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.HostIdDoesNotMatchInstance(this.host.Id, this.instanceId)));
            }
            if (this.host.HasTrackingParticipant)
            {
                this.host.TrackingProvider.OnDeserialized(this.nextTrackingRecordNumber);
                this.host.OnDeserialized(this.hasTrackedStarted);
            }
            if (this.scheduler != null)
            {
                this.scheduler.OnDeserialized(new Scheduler.Callbacks(this));
            }
            if (this.rootInstance != null)
            {
                this.instanceMap.LoadActivityTree(workflow, this.rootInstance, this.executingSecondaryRootInstances, this);
                if (this.executingSecondaryRootInstances != null)
                {
                    for (int i = 0; i < this.executingSecondaryRootInstances.Count; i++)
                    {
                        System.Activities.ActivityInstance handleScope = this.executingSecondaryRootInstances[i];
                        LocationEnvironment parent = handleScope.Environment.Parent;
                        if (parent != null)
                        {
                            parent.OnDeserialized(this, handleScope);
                        }
                    }
                }
            }
            else
            {
                this.isDisposed = true;
            }
        }

        internal Scheduler.RequestedAction OnExecuteWorkItem(System.Activities.Runtime.WorkItem workItem)
        {
            workItem.Release(this);
            if (workItem.IsValid)
            {
                if (!workItem.IsEmpty)
                {
                    Exception abortException = null;
                    System.Activities.ActivityInstance propertyManagerOwner = workItem.PropertyManagerOwner;
                    try
                    {
                        if ((propertyManagerOwner != null) && (propertyManagerOwner.PropertyManager != null))
                        {
                            try
                            {
                                propertyManagerOwner.PropertyManager.SetupWorkflowThread();
                            }
                            catch (Exception exception2)
                            {
                                if (Fx.IsFatal(exception2))
                                {
                                    throw;
                                }
                                abortException = exception2;
                            }
                        }
                        if ((abortException == null) && !workItem.Execute(this, this.bookmarkManager))
                        {
                            return Scheduler.YieldSilently;
                        }
                    }
                    finally
                    {
                        if ((propertyManagerOwner != null) && (propertyManagerOwner.PropertyManager != null))
                        {
                            propertyManagerOwner.PropertyManager.CleanupWorkflowThread(ref abortException);
                        }
                        if (abortException != null)
                        {
                            this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.SetupOrCleanupWorkflowThreadThrew, abortException));
                        }
                    }
                    if (abortException != null)
                    {
                        return Scheduler.Continue;
                    }
                }
                if (workItem.WorkflowAbortException != null)
                {
                    this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                    return Scheduler.Continue;
                }
                if ((this.bookmarkScopeManager != null) && this.bookmarkScopeManager.HasKeysToUpdate)
                {
                    if (!workItem.FlushBookmarkScopeKeys(this))
                    {
                        return Scheduler.YieldSilently;
                    }
                    if (workItem.WorkflowAbortException != null)
                    {
                        this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                        return Scheduler.Continue;
                    }
                }
                workItem.PostProcess(this);
                if (workItem.ExceptionToPropagate != null)
                {
                    this.PropagateException(workItem);
                }
                if (this.HasPendingTrackingRecords)
                {
                    if (!workItem.FlushTracking(this))
                    {
                        return Scheduler.YieldSilently;
                    }
                    if (workItem.WorkflowAbortException != null)
                    {
                        this.AbortWorkflowInstance(new OperationCanceledException(System.Activities.SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                        return Scheduler.Continue;
                    }
                }
                this.ScheduleRuntimeWorkItems();
                if (workItem.ExceptionToPropagate != null)
                {
                    return Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
                }
            }
            return Scheduler.Continue;
        }

        internal void OnSchedulerIdle()
        {
            if (this.isTerminatePending)
            {
                this.Terminate(this.terminationPendingException);
                this.isTerminatePending = false;
            }
            if (this.IsIdle)
            {
                if (((this.transactionContextWaiters != null) && (this.transactionContextWaiters.Count > 0)) && (this.IsPersistable || (this.transactionContextWaiters[0].IsRequires && (this.noPersistCount == 1))))
                {
                    TransactionContextWaiter waiter = this.transactionContextWaiters.Dequeue();
                    waiter.WaitingInstance.DecrementBusyCount();
                    waiter.WaitingInstance.WaitingForTransactionContext = false;
                    this.ScheduleItem(new TransactionContextWorkItem(waiter));
                    this.MarkSchedulerRunning();
                    this.ResumeScheduler();
                    return;
                }
                if (this.shouldRaiseMainBodyComplete)
                {
                    this.shouldRaiseMainBodyComplete = false;
                    if (this.mainRootCompleteBookmark != null)
                    {
                        BookmarkResumptionResult result = this.TryResumeUserBookmark(this.mainRootCompleteBookmark, this.rootInstance.State, false);
                        this.mainRootCompleteBookmark = null;
                        if (result == BookmarkResumptionResult.Success)
                        {
                            this.MarkSchedulerRunning();
                            this.ResumeScheduler();
                            return;
                        }
                    }
                    if ((this.executingSecondaryRootInstances == null) || (this.executingSecondaryRootInstances.Count == 0))
                    {
                        this.Dispose(false);
                    }
                }
            }
            if (this.shouldPauseOnCanPersist && this.IsPersistable)
            {
                this.shouldPauseOnCanPersist = false;
            }
            try
            {
                this.host.NotifyPaused();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.AbortWorkflowInstance(exception);
            }
        }

        internal void OnSchedulerThreadAcquired()
        {
            if (this.IsDebugged() && !this.hasRaisedWorkflowStarted)
            {
                this.hasRaisedWorkflowStarted = true;
                this.debugController.WorkflowStarted();
            }
        }

        public void Open(SynchronizationContext synchronizationContext)
        {
            this.scheduler.Open(synchronizationContext);
        }

        public void PauseScheduler()
        {
            Scheduler scheduler = this.scheduler;
            if (scheduler != null)
            {
                scheduler.Pause();
            }
        }

        public void PauseWhenPersistable()
        {
            this.shouldPauseOnCanPersist = true;
        }

        public object PrepareForSerialization()
        {
            if (this.host.HasTrackingParticipant)
            {
                this.nextTrackingRecordNumber = this.host.TrackingProvider.NextTrackingRecordNumber;
                this.hasTrackedStarted = this.host.HasTrackedStarted;
            }
            return this;
        }

        private void PropagateException(System.Activities.Runtime.WorkItem workItem)
        {
            System.Activities.ActivityInstance activityInstance = workItem.ActivityInstance;
            Exception exceptionToPropagate = workItem.ExceptionToPropagate;
            System.Activities.ActivityInstance parent = activityInstance;
            FaultBookmark faultBookmark = null;
            while ((parent != null) && (faultBookmark == null))
            {
                if ((!parent.IsCompleted && (this.runtimeTransaction != null)) && (this.runtimeTransaction.IsolationScope == parent))
                {
                    this.scheduler.PushWork(new AbortActivityWorkItem(parent, exceptionToPropagate, this.CreateActivityInstanceReference(workItem.OriginalExceptionSource, parent)));
                    this.runtimeTransaction.ShouldScheduleCompletion = false;
                    workItem.ExceptionPropagated();
                    return;
                }
                if (parent.IsCancellationRequested)
                {
                    this.AbortWorkflowInstance(new InvalidOperationException(System.Activities.SR.CannotPropagateExceptionWhileCanceling(activityInstance.Activity.DisplayName, activityInstance.Id), exceptionToPropagate));
                    workItem.ExceptionPropagated();
                    return;
                }
                if (parent.FaultBookmark != null)
                {
                    faultBookmark = parent.FaultBookmark;
                }
                else
                {
                    parent = parent.Parent;
                }
            }
            if (faultBookmark != null)
            {
                if (this.ShouldTrackFaultPropagationRecords)
                {
                    this.AddTrackingRecord(new FaultPropagationRecord(this.WorkflowInstanceId, workItem.OriginalExceptionSource, parent.Parent, activityInstance == workItem.OriginalExceptionSource, exceptionToPropagate));
                }
                this.scheduler.PushWork(faultBookmark.GenerateWorkItem(exceptionToPropagate, parent, this.CreateActivityInstanceReference(workItem.OriginalExceptionSource, parent.Parent)));
                workItem.ExceptionPropagated();
            }
            else if (this.ShouldTrackFaultPropagationRecords)
            {
                this.AddTrackingRecord(new FaultPropagationRecord(this.WorkflowInstanceId, workItem.OriginalExceptionSource, null, activityInstance == workItem.OriginalExceptionSource, exceptionToPropagate));
            }
        }

        public void RegisterMainRootCompleteCallback(Bookmark bookmark)
        {
            this.mainRootCompleteBookmark = bookmark;
        }

        public void RequestPersist(Bookmark onPersistBookmark, System.Activities.ActivityInstance requestingInstance)
        {
            if (this.persistenceWaiters == null)
            {
                this.persistenceWaiters = new Queue<PersistenceWaiter>();
            }
            this.persistenceWaiters.Enqueue(new PersistenceWaiter(onPersistBookmark, requestingInstance));
        }

        public void RequestTransactionContext(System.Activities.ActivityInstance instance, bool isRequires, RuntimeTransactionHandle handle, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            if (isRequires)
            {
                this.EnterNoPersist();
            }
            if (this.transactionContextWaiters == null)
            {
                this.transactionContextWaiters = new Quack<TransactionContextWaiter>();
            }
            TransactionContextWaiter item = new TransactionContextWaiter(instance, isRequires, handle, new TransactionContextWaiterCallbackWrapper(callback, instance), state);
            if (isRequires)
            {
                this.transactionContextWaiters.PushFront(item);
            }
            else
            {
                this.transactionContextWaiters.Enqueue(item);
            }
            instance.IncrementBusyCount();
            instance.WaitingForTransactionContext = true;
        }

        private void ResumeScheduler()
        {
            this.scheduler.Resume();
        }

        internal void RethrowException(System.Activities.ActivityInstance fromInstance, FaultContext context)
        {
            this.scheduler.PushWork(new RethrowExceptionWorkItem(fromInstance, context.Exception, context.Source));
        }

        public void Run()
        {
            this.ResumeScheduler();
        }

        public System.Activities.ActivityInstance ScheduleActivity(Activity activity, System.Activities.ActivityInstance parent, CompletionBookmark completionBookmark, FaultBookmark faultBookmark, LocationEnvironment parentEnvironment)
        {
            return this.ScheduleActivity(activity, parent, completionBookmark, faultBookmark, parentEnvironment, null, null);
        }

        private System.Activities.ActivityInstance ScheduleActivity(Activity activity, System.Activities.ActivityInstance parent, CompletionBookmark completionBookmark, FaultBookmark faultBookmark, LocationEnvironment parentEnvironment, IDictionary<string, object> argumentValueOverrides, System.Activities.Location resultLocation)
        {
            System.Activities.ActivityInstance scheduledInstance = this.CreateUninitalizedActivityInstance(activity, parent, completionBookmark, faultBookmark);
            bool requiresSymbolResolution = scheduledInstance.Initialize(parent, this.instanceMap, parentEnvironment, this.lastInstanceId, this);
            if (TD.ActivityScheduledIsEnabled())
            {
                this.TraceActivityScheduled(parent, activity, scheduledInstance);
            }
            if (this.ShouldTrackActivityScheduledRecords)
            {
                this.AddTrackingRecord(new ActivityScheduledRecord(this.WorkflowInstanceId, parent, scheduledInstance));
            }
            this.ScheduleBody(scheduledInstance, requiresSymbolResolution, argumentValueOverrides, resultLocation);
            return scheduledInstance;
        }

        internal void ScheduleBody(System.Activities.ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides, System.Activities.Location resultLocation)
        {
            if (resultLocation == null)
            {
                ExecuteActivityWorkItem workItem = this.ExecuteActivityWorkItemPool.Acquire();
                workItem.Initialize(activityInstance, requiresSymbolResolution, argumentValueOverrides);
                this.scheduler.PushWork(workItem);
            }
            else
            {
                this.scheduler.PushWork(new ExecuteExpressionWorkItem(activityInstance, requiresSymbolResolution, argumentValueOverrides, resultLocation));
            }
        }

        private void ScheduleCompletionBookmark(System.Activities.ActivityInstance completedInstance)
        {
            if (completedInstance.CompletionBookmark != null)
            {
                this.scheduler.PushWork(completedInstance.CompletionBookmark.GenerateWorkItem(completedInstance, this));
            }
            else if (completedInstance.Parent != null)
            {
                if ((completedInstance.State != ActivityInstanceState.Closed) && completedInstance.Parent.HasNotExecuted)
                {
                    completedInstance.Parent.SetInitializationIncomplete();
                }
                this.scheduler.PushWork(this.CreateEmptyWorkItem(completedInstance.Parent));
            }
        }

        public System.Activities.ActivityInstance ScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, System.Activities.ActivityInstance parent, LocationEnvironment executionEnvironment, CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            System.Activities.ActivityInstance instance;
            if (activityDelegate.Handler == null)
            {
                instance = System.Activities.ActivityInstance.CreateCompletedInstance(new EmptyDelegateActivity());
                instance.CompletionBookmark = completionBookmark;
                this.ScheduleCompletionBookmark(instance);
                return instance;
            }
            instance = this.CreateUninitalizedActivityInstance(activityDelegate.Handler, parent, completionBookmark, faultBookmark);
            bool requiresSymbolResolution = instance.Initialize(parent, this.instanceMap, executionEnvironment, this.lastInstanceId, this, activityDelegate.RuntimeDelegateArguments.Count);
            IList<RuntimeDelegateArgument> runtimeDelegateArguments = activityDelegate.RuntimeDelegateArguments;
            for (int i = 0; i < runtimeDelegateArguments.Count; i++)
            {
                RuntimeDelegateArgument argument = runtimeDelegateArguments[i];
                if (argument.BoundArgument != null)
                {
                    string name = argument.Name;
                    System.Activities.Location location = argument.BoundArgument.CreateLocation();
                    instance.Environment.Declare(argument.BoundArgument, location, instance);
                    if ((ArgumentDirectionHelper.IsIn(argument.Direction) && (inputParameters != null)) && (inputParameters.Count > 0))
                    {
                        location.Value = inputParameters[name];
                    }
                }
            }
            if (TD.ActivityScheduledIsEnabled())
            {
                this.TraceActivityScheduled(parent, activityDelegate.Handler, instance);
            }
            if (this.ShouldTrackActivityScheduledRecords)
            {
                this.AddTrackingRecord(new ActivityScheduledRecord(this.WorkflowInstanceId, parent, instance));
            }
            this.ScheduleBody(instance, requiresSymbolResolution, null, null);
            return instance;
        }

        internal void ScheduleExpression(Activity activity, System.Activities.ActivityInstance parent, LocationEnvironment parentEnvironment, System.Activities.Location resultLocation)
        {
            this.ScheduleActivity(activity, parent, null, null, parentEnvironment, null, resultLocation);
        }

        internal void ScheduleItem(System.Activities.Runtime.WorkItem workItem)
        {
            this.scheduler.PushWork(workItem);
        }

        private void SchedulePendingCancelation()
        {
            if (this.runtimeTransaction.IsRootCancelPending)
            {
                if (!this.rootInstance.IsCancellationRequested && !this.rootInstance.IsCompleted)
                {
                    this.rootInstance.IsCancellationRequested = true;
                    this.scheduler.PushWork(new CancelActivityWorkItem(this.rootInstance));
                }
                this.runtimeTransaction.IsRootCancelPending = false;
            }
        }

        public void ScheduleRootActivity(Activity activity, IDictionary<string, object> argumentValueOverrides, IList<Handle> hostProperties)
        {
            Guid guid;
            if ((hostProperties != null) && (hostProperties.Count > 0))
            {
                Dictionary<string, ExecutionPropertyManager.ExecutionProperty> properties = new Dictionary<string, ExecutionPropertyManager.ExecutionProperty>(hostProperties.Count);
                HandleInitializationContext context = new HandleInitializationContext(this, null);
                for (int i = 0; i < hostProperties.Count; i++)
                {
                    Handle property = hostProperties[i];
                    property.Initialize(context);
                    properties.Add(property.ExecutionPropertyName, new ExecutionPropertyManager.ExecutionProperty(property.ExecutionPropertyName, property, null));
                }
                context.Dispose();
                this.rootPropertyManager = new ExecutionPropertyManager(null, properties);
            }
            bool hasBeenResumed = this.TryTraceStart(out guid);
            System.Activities.ActivityInstance instance = new System.Activities.ActivityInstance(activity) {
                PropertyManager = this.rootPropertyManager
            };
            this.rootInstance = instance;
            this.rootElement = activity;
            this.lastInstanceId += 1L;
            bool requiresSymbolResolution = this.rootInstance.Initialize(null, this.instanceMap, null, this.lastInstanceId, this);
            if (TD.ActivityScheduledIsEnabled())
            {
                this.TraceActivityScheduled(null, activity, this.rootInstance);
            }
            this.scheduler.PushWork(new ExecuteRootWorkItem(this.rootInstance, requiresSymbolResolution, argumentValueOverrides));
            this.TraceSuspend(hasBeenResumed, guid);
        }

        private void ScheduleRuntimeWorkItems()
        {
            if ((this.runtimeTransaction != null) && this.runtimeTransaction.ShouldScheduleCompletion)
            {
                this.scheduler.PushWork(new CompleteTransactionWorkItem(this.runtimeTransaction.IsolationScope));
            }
            else if (((this.persistenceWaiters != null) && (this.persistenceWaiters.Count > 0)) && this.IsPersistable)
            {
                PersistenceWaiter waiter = this.persistenceWaiters.Dequeue();
                while ((waiter != null) && waiter.WaitingInstance.IsCompleted)
                {
                    if (this.persistenceWaiters.Count == 0)
                    {
                        waiter = null;
                    }
                    else
                    {
                        waiter = this.persistenceWaiters.Dequeue();
                    }
                }
                if (waiter != null)
                {
                    this.scheduler.PushWork(waiter.CreateWorkItem());
                }
            }
        }

        public System.Activities.ActivityInstance ScheduleSecondaryRootActivity(Activity activity, LocationEnvironment environment)
        {
            System.Activities.ActivityInstance item = this.ScheduleActivity(activity, null, null, null, environment);
            while (environment != null)
            {
                environment.AddReference();
                environment = environment.Parent;
            }
            if (this.executingSecondaryRootInstances == null)
            {
                this.executingSecondaryRootInstances = new List<System.Activities.ActivityInstance>();
            }
            this.executingSecondaryRootInstances.Add(item);
            return item;
        }

        public void ScheduleTerminate(Exception reason)
        {
            this.isTerminatePending = true;
            this.terminationPendingException = reason;
        }

        public void SetTransaction(RuntimeTransactionHandle handle, Transaction transaction, System.Activities.ActivityInstance isolationScope, System.Activities.ActivityInstance transactionOwner)
        {
            this.runtimeTransaction = new RuntimeTransactionData(handle, transaction, isolationScope);
            this.EnterNoPersist();
            if (transactionOwner != null)
            {
                Exception reason = null;
                try
                {
                    transaction.EnlistVolatile(this, EnlistmentOptions.EnlistDuringPrepareRequired);
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
                    this.AbortWorkflowInstance(reason);
                }
                else if (TD.RuntimeTransactionSetIsEnabled())
                {
                    TD.RuntimeTransactionSet(transactionOwner.Activity.GetType().ToString(), transactionOwner.Activity.DisplayName, transactionOwner.Id, isolationScope.Activity.GetType().ToString(), isolationScope.Activity.DisplayName, isolationScope.Id);
                }
            }
        }

        public AsyncOperationContext SetupAsyncOperationBlock(System.Activities.ActivityInstance owningActivity)
        {
            if ((this.activeOperations != null) && this.activeOperations.ContainsKey(owningActivity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.OnlyOneOperationPerActivity));
            }
            this.EnterNoPersist();
            AsyncOperationContext context = new AsyncOperationContext(this, owningActivity);
            if (this.activeOperations == null)
            {
                this.activeOperations = new Dictionary<System.Activities.ActivityInstance, AsyncOperationContext>();
            }
            this.activeOperations.Add(owningActivity, context);
            return context;
        }

        public bool ShouldTrackActivity(string name)
        {
            return this.host.TrackingProvider.ShouldTrackActivity(name);
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            RuntimeTransactionData runtimeTransaction = this.runtimeTransaction;
            if (runtimeTransaction != null)
            {
                AsyncWaitHandle completionEvent = null;
                lock (runtimeTransaction)
                {
                    completionEvent = runtimeTransaction.CompletionEvent;
                    runtimeTransaction.TransactionStatus = TransactionStatus.Committed;
                }
                enlistment.Done();
                if (completionEvent != null)
                {
                    completionEvent.Set();
                }
            }
            else
            {
                enlistment.Done();
            }
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            ((IEnlistmentNotification) this).Rollback(enlistment);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            RuntimeTransactionData runtimeTransaction = this.runtimeTransaction;
            if (runtimeTransaction != null)
            {
                bool flag = false;
                lock (runtimeTransaction)
                {
                    if (runtimeTransaction.HasPrepared)
                    {
                        flag = true;
                    }
                    else
                    {
                        runtimeTransaction.PendingPreparingEnlistment = preparingEnlistment;
                    }
                }
                if (flag)
                {
                    preparingEnlistment.Prepared();
                }
            }
            else
            {
                preparingEnlistment.Prepared();
            }
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            RuntimeTransactionData runtimeTransaction = this.runtimeTransaction;
            if (runtimeTransaction != null)
            {
                AsyncWaitHandle completionEvent = null;
                lock (runtimeTransaction)
                {
                    completionEvent = runtimeTransaction.CompletionEvent;
                    runtimeTransaction.TransactionStatus = TransactionStatus.Aborted;
                }
                enlistment.Done();
                if (completionEvent != null)
                {
                    completionEvent.Set();
                }
            }
            else
            {
                enlistment.Done();
            }
        }

        public void Terminate(Exception reason)
        {
            Guid guid;
            bool hasBeenResumed = this.TryTraceResume(out guid);
            this.Abort(reason, true);
            this.TraceSuspend(hasBeenResumed, guid);
        }

        public void TerminateSpecialExecutionBlocks(System.Activities.ActivityInstance terminatedInstance, Exception terminationReason)
        {
            if ((this.runtimeTransaction != null) && (this.runtimeTransaction.IsolationScope == terminatedInstance))
            {
                Exception reason = null;
                try
                {
                    this.runtimeTransaction.Rollback(terminationReason);
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
                    this.AbortWorkflowInstance(reason);
                }
                this.SchedulePendingCancelation();
                this.ExitNoPersist();
                if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                {
                    this.AbortWorkflowInstance(terminationReason);
                }
                this.runtimeTransaction = null;
            }
        }

        private void TraceActivityScheduled(System.Activities.ActivityInstance parent, Activity activity, System.Activities.ActivityInstance scheduledInstance)
        {
            if (parent != null)
            {
                TD.ActivityScheduled(parent.Activity.GetType().ToString(), parent.Activity.DisplayName, parent.Id, activity.GetType().ToString(), activity.DisplayName, scheduledInstance.Id);
            }
            else
            {
                TD.ActivityScheduled(string.Empty, string.Empty, string.Empty, activity.GetType().ToString(), activity.DisplayName, scheduledInstance.Id);
            }
        }

        private void TraceSuspend(bool hasBeenResumed, Guid oldActivityId)
        {
            if (hasBeenResumed)
            {
                if (TD.WorkflowActivitySuspendIsEnabled())
                {
                    TD.WorkflowActivitySuspend(this.WorkflowInstanceId.ToString());
                }
                DiagnosticTrace.ActivityId = oldActivityId;
            }
        }

        internal bool TryGetPendingOperation(System.Activities.ActivityInstance instance, out AsyncOperationContext asyncContext)
        {
            if (this.activeOperations != null)
            {
                return this.activeOperations.TryGetValue(instance, out asyncContext);
            }
            asyncContext = null;
            return false;
        }

        internal BookmarkResumptionResult TryResumeBookmark(Bookmark bookmark, object value, BookmarkScope scope)
        {
            Guid guid;
            ActivityExecutionWorkItem item;
            bool hasBeenResumed = this.TryTraceResume(out guid);
            System.Activities.ActivityInstance isolationInstance = null;
            if (this.runtimeTransaction != null)
            {
                isolationInstance = this.runtimeTransaction.IsolationScope;
            }
            bool flag2 = (this.activeOperations != null) && (this.activeOperations.Count > 0);
            BookmarkResumptionResult result = this.BookmarkScopeManager.TryGenerateWorkItem(this, ref bookmark, scope, value, isolationInstance, flag2 || this.bookmarkManager.HasBookmarks, out item);
            if (result == BookmarkResumptionResult.Success)
            {
                this.scheduler.EnqueueWork(item);
                if (this.ShouldTrackBookmarkResumptionRecords)
                {
                    this.AddTrackingRecord(new BookmarkResumptionRecord(this.WorkflowInstanceId, bookmark, item.ActivityInstance, value));
                }
            }
            this.TraceSuspend(hasBeenResumed, guid);
            return result;
        }

        internal BookmarkResumptionResult TryResumeHostBookmark(Bookmark bookmark, object value)
        {
            Guid guid;
            bool hasBeenResumed = this.TryTraceResume(out guid);
            BookmarkResumptionResult result = this.TryResumeUserBookmark(bookmark, value, true);
            this.TraceSuspend(hasBeenResumed, guid);
            return result;
        }

        internal BookmarkResumptionResult TryResumeUserBookmark(Bookmark bookmark, object value, bool isExternal)
        {
            ActivityExecutionWorkItem item;
            if (this.isDisposed)
            {
                return BookmarkResumptionResult.NotFound;
            }
            System.Activities.ActivityInstance isolationInstance = null;
            if (this.runtimeTransaction != null)
            {
                isolationInstance = this.runtimeTransaction.IsolationScope;
            }
            BookmarkResumptionResult success = this.bookmarkManager.TryGenerateWorkItem(this, isExternal, ref bookmark, value, isolationInstance, out item);
            if (success == BookmarkResumptionResult.Success)
            {
                this.scheduler.EnqueueWork(item);
                if (this.ShouldTrackBookmarkResumptionRecords)
                {
                    this.AddTrackingRecord(new BookmarkResumptionRecord(this.WorkflowInstanceId, bookmark, item.ActivityInstance, value));
                }
                return success;
            }
            if ((success != BookmarkResumptionResult.NotReady) && (bookmark == Bookmark.AsyncOperationCompletionBookmark))
            {
                ((AsyncOperationContext.CompleteData) value).CompleteOperation();
                success = BookmarkResumptionResult.Success;
            }
            return success;
        }

        private bool TryTraceResume(out Guid oldActivityId)
        {
            if (FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                oldActivityId = DiagnosticTrace.ActivityId;
                FxTrace.Trace.SetAndTraceTransfer(this.WorkflowInstanceId, true);
                if (TD.WorkflowActivityResumeIsEnabled())
                {
                    TD.WorkflowActivityResume(this.WorkflowInstanceId.ToString());
                }
                return true;
            }
            oldActivityId = Guid.Empty;
            return false;
        }

        private bool TryTraceStart(out Guid oldActivityId)
        {
            if (FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                oldActivityId = DiagnosticTrace.ActivityId;
                FxTrace.Trace.SetAndTraceTransfer(this.WorkflowInstanceId, true);
                if (TD.WorkflowActivityStartIsEnabled())
                {
                    TD.WorkflowActivityStart(this.WorkflowInstanceId.ToString());
                }
                return true;
            }
            oldActivityId = Guid.Empty;
            return false;
        }

        internal System.Activities.Runtime.BookmarkScopeManager BookmarkScopeManager
        {
            get
            {
                if (this.bookmarkScopeManager == null)
                {
                    this.bookmarkScopeManager = new System.Activities.Runtime.BookmarkScopeManager();
                }
                return this.bookmarkScopeManager;
            }
        }

        public Pool<CodeActivityContext> CodeActivityContextPool
        {
            get
            {
                if (this.codeActivityContextPool == null)
                {
                    this.codeActivityContextPool = new PoolOfCodeActivityContexts();
                }
                return this.codeActivityContextPool;
            }
        }

        public Pool<CompletionCallbackWrapper.CompletionWorkItem> CompletionWorkItemPool
        {
            get
            {
                if (this.completionWorkItemPool == null)
                {
                    this.completionWorkItemPool = new PoolOfCompletionWorkItems();
                }
                return this.completionWorkItemPool;
            }
        }

        public Transaction CurrentTransaction
        {
            get
            {
                if (this.runtimeTransaction != null)
                {
                    return this.runtimeTransaction.ClonedTransaction;
                }
                return null;
            }
        }

        private static ReadOnlyCollection<BookmarkInfo> EmptyBookmarkInfoCollection
        {
            get
            {
                if (emptyBookmarkInfoCollection == null)
                {
                    emptyBookmarkInfoCollection = new ReadOnlyCollection<BookmarkInfo>(new List<BookmarkInfo>(0));
                }
                return emptyBookmarkInfoCollection;
            }
        }

        public LocationEnvironment EmptyEnvironment
        {
            get
            {
                return new LocationEnvironment(this, null);
            }
        }

        public Pool<EmptyWorkItem> EmptyWorkItemPool
        {
            get
            {
                if (this.emptyWorkItemPool == null)
                {
                    this.emptyWorkItemPool = new PoolOfEmptyWorkItems();
                }
                return this.emptyWorkItemPool;
            }
        }

        private Pool<ExecuteActivityWorkItem> ExecuteActivityWorkItemPool
        {
            get
            {
                if (this.executeActivityWorkItemPool == null)
                {
                    this.executeActivityWorkItemPool = new PoolOfExecuteActivityWorkItems();
                }
                return this.executeActivityWorkItemPool;
            }
        }

        internal List<Handle> Handles
        {
            get
            {
                return this.handles;
            }
        }

        public bool HasPendingTrackingRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.HasPendingRecords);
            }
        }

        public bool HasRuntimeTransaction
        {
            get
            {
                return (this.runtimeTransaction != null);
            }
        }

        public bool IsAbortPending
        {
            get
            {
                return this.isAbortPending;
            }
        }

        public bool IsIdle
        {
            get
            {
                if (!this.isDisposed)
                {
                    return this.scheduler.IsIdle;
                }
                return true;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return (this.host != null);
            }
        }

        public bool IsPersistable
        {
            get
            {
                return (this.noPersistCount == 0);
            }
        }

        public bool IsRunning
        {
            get
            {
                return (!this.isDisposed && this.scheduler.IsRunning);
            }
        }

        public bool IsTerminatePending
        {
            get
            {
                return this.isTerminatePending;
            }
        }

        public bool KeysAllowed
        {
            get
            {
                return this.host.SupportsInstanceKeys;
            }
        }

        internal System.Activities.Runtime.MappableObjectManager MappableObjectManager
        {
            get
            {
                if (this.mappableObjectManager == null)
                {
                    this.mappableObjectManager = new System.Activities.Runtime.MappableObjectManager();
                }
                return this.mappableObjectManager;
            }
        }

        public Pool<NativeActivityContext> NativeActivityContextPool
        {
            get
            {
                if (this.nativeActivityContextPool == null)
                {
                    this.nativeActivityContextPool = new PoolOfNativeActivityContexts();
                }
                return this.nativeActivityContextPool;
            }
        }

        internal BookmarkManager RawBookmarkManager
        {
            get
            {
                return this.bookmarkManager;
            }
        }

        internal System.Activities.Runtime.BookmarkScopeManager RawBookmarkScopeManager
        {
            get
            {
                return this.bookmarkScopeManager;
            }
        }

        public bool RequiresTransactionContextWaiterExists
        {
            get
            {
                return (((this.transactionContextWaiters != null) && (this.transactionContextWaiters.Count > 0)) && this.transactionContextWaiters[0].IsRequires);
            }
        }

        public Pool<ResolveNextArgumentWorkItem> ResolveNextArgumentWorkItemPool
        {
            get
            {
                if (this.resolveNextArgumentWorkItemPool == null)
                {
                    this.resolveNextArgumentWorkItemPool = new PoolOfResolveNextArgumentWorkItems();
                }
                return this.resolveNextArgumentWorkItemPool;
            }
        }

        public Activity RootActivity
        {
            get
            {
                return this.rootElement;
            }
        }

        internal ExecutionPropertyManager RootPropertyManager
        {
            get
            {
                return this.rootPropertyManager;
            }
        }

        [DataMember(Name="secondaryRootInstances", EmitDefaultValue=false)]
        private List<System.Activities.ActivityInstance> SerializedExecutingSecondaryRootInstances
        {
            get
            {
                if ((this.executingSecondaryRootInstances != null) && (this.executingSecondaryRootInstances.Count > 0))
                {
                    return this.executingSecondaryRootInstances;
                }
                return null;
            }
            set
            {
                this.executingSecondaryRootInstances = value;
            }
        }

        [DataMember(Name="mappableObjectManager", EmitDefaultValue=false)]
        private System.Activities.Runtime.MappableObjectManager SerializedMappableObjectManager
        {
            get
            {
                if ((this.mappableObjectManager != null) && (this.mappableObjectManager.Count != 0))
                {
                    return this.mappableObjectManager;
                }
                return null;
            }
            set
            {
                this.mappableObjectManager = value;
            }
        }

        [DataMember(Name="persistenceWaiters", EmitDefaultValue=false)]
        private Queue<PersistenceWaiter> SerializedPersistenceWaiters
        {
            get
            {
                if ((this.persistenceWaiters != null) && (this.persistenceWaiters.Count != 0))
                {
                    return this.persistenceWaiters;
                }
                return null;
            }
            set
            {
                this.persistenceWaiters = value;
            }
        }

        [DataMember(Name="activities", EmitDefaultValue=false)]
        private ActivityInstanceMap SerializedProgramMapping
        {
            get
            {
                if ((this.instanceMap == null) && !this.isDisposed)
                {
                    this.instanceMap = new ActivityInstanceMap();
                    this.rootInstance.FillInstanceMap(this.instanceMap);
                    this.scheduler.FillInstanceMap(this.instanceMap);
                    if ((this.executingSecondaryRootInstances != null) && (this.executingSecondaryRootInstances.Count > 0))
                    {
                        foreach (System.Activities.ActivityInstance instance in this.executingSecondaryRootInstances)
                        {
                            instance.FillInstanceMap(this.instanceMap);
                            LocationEnvironment reference = instance.Environment;
                            if (instance.IsEnvironmentOwner)
                            {
                                reference = reference.Parent;
                            }
                            while (reference != null)
                            {
                                if (reference.HasOwnerCompleted)
                                {
                                    this.instanceMap.AddEntry(reference, true);
                                }
                                reference = reference.Parent;
                            }
                        }
                    }
                }
                return this.instanceMap;
            }
            set
            {
                this.instanceMap = value;
            }
        }

        [DataMember(Name="propertyManager", EmitDefaultValue=false)]
        private ExecutionPropertyManager SerializedPropertyManager
        {
            get
            {
                return this.rootPropertyManager;
            }
            set
            {
                this.rootPropertyManager = value;
                this.rootPropertyManager.OnDeserialized(null, null, null, this);
            }
        }

        [DataMember(Name="transactionContextWaiters", EmitDefaultValue=false)]
        private TransactionContextWaiter[] SerializedTransactionContextWaiters
        {
            get
            {
                if ((this.transactionContextWaiters != null) && (this.transactionContextWaiters.Count > 0))
                {
                    return this.transactionContextWaiters.ToArray();
                }
                return null;
            }
            set
            {
                this.transactionContextWaiters = new Quack<TransactionContextWaiter>(value);
            }
        }

        public bool ShouldTrack
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrack);
            }
        }

        public bool ShouldTrackActivityScheduledRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityScheduledRecords);
            }
        }

        public bool ShouldTrackActivityStateRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecords);
            }
        }

        public bool ShouldTrackActivityStateRecordsClosedState
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecordsClosedState);
            }
        }

        public bool ShouldTrackActivityStateRecordsExecutingState
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecordsExecutingState);
            }
        }

        public bool ShouldTrackBookmarkResumptionRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackBookmarkResumptionRecords);
            }
        }

        public bool ShouldTrackCancelRequestedRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackCancelRequestedRecords);
            }
        }

        public bool ShouldTrackFaultPropagationRecords
        {
            get
            {
                return (this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackFaultPropagationRecords);
            }
        }

        public ActivityInstanceState State
        {
            get
            {
                if (((this.executingSecondaryRootInstances == null) || (this.executingSecondaryRootInstances.Count <= 0)) && ((this.rootInstance == null) || this.rootInstance.IsCompleted))
                {
                    return this.executionState;
                }
                return ActivityInstanceState.Executing;
            }
        }

        public System.Activities.Hosting.SymbolResolver SymbolResolver
        {
            get
            {
                if (this.symbolResolver == null)
                {
                    try
                    {
                        this.symbolResolver = this.host.GetExtension<System.Activities.Hosting.SymbolResolver>();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw FxTrace.Exception.AsError(new CallbackException(System.Activities.SR.CallbackExceptionFromHostGetExtension(this.WorkflowInstanceId), exception));
                    }
                }
                return this.symbolResolver;
            }
        }

        public Exception TerminationException
        {
            get
            {
                return this.completionException;
            }
        }

        [DataMember]
        public Guid WorkflowInstanceId
        {
            get
            {
                if (!this.instanceIdSet)
                {
                    this.WorkflowInstanceId = this.host.Id;
                    if (!this.instanceIdSet)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.EmptyIdReturnedFromHost(this.host.GetType())));
                    }
                }
                return this.instanceId;
            }
            private set
            {
                this.instanceId = value;
                this.instanceIdSet = value != Guid.Empty;
            }
        }

        public IDictionary<string, object> WorkflowOutputs
        {
            get
            {
                return this.workflowOutputs;
            }
        }

        [DataContract]
        private class AbortActivityWorkItem : System.Activities.Runtime.WorkItem
        {
            [DataMember]
            private ActivityInstanceReference originalSource;
            [DataMember]
            private Exception reason;

            public AbortActivityWorkItem(System.Activities.ActivityInstance activityInstance, Exception reason, ActivityInstanceReference originalSource) : base(activityInstance)
            {
                this.reason = reason;
                this.originalSource = originalSource;
                base.IsEmpty = true;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return true;
            }

            public override void PostProcess(ActivityExecutor executor)
            {
                executor.AbortActivityInstance(base.ActivityInstance, this.reason);
                base.ExceptionToPropagate = this.reason;
            }

            public override void TraceCompleted()
            {
                base.TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                base.TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                base.TraceRuntimeWorkItemStarting();
            }

            public override bool IsValid
            {
                get
                {
                    return (base.ActivityInstance.State == ActivityInstanceState.Executing);
                }
            }

            public override System.Activities.ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.originalSource.ActivityInstance;
                }
            }

            public override System.Activities.ActivityInstance PropertyManagerOwner
            {
                get
                {
                    return null;
                }
            }
        }

        private class AssociateKeysAsyncResult : AsyncResult
        {
            private static readonly AsyncResult.AsyncCompletion associatedCallback = new AsyncResult.AsyncCompletion(ActivityExecutor.AssociateKeysAsyncResult.OnAssociated);
            private readonly ActivityExecutor executor;

            public AssociateKeysAsyncResult(ActivityExecutor executor, ICollection<InstanceKey> keysToAssociate, AsyncCallback callback, object state) : base(callback, state)
            {
                IAsyncResult result;
                this.executor = executor;
                using (base.PrepareTransactionalCall(this.executor.CurrentTransaction))
                {
                    result = this.executor.host.OnBeginAssociateKeys(keysToAssociate, base.PrepareAsyncCompletion(associatedCallback), this);
                }
                if (base.SyncContinue(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ActivityExecutor.AssociateKeysAsyncResult>(result);
            }

            private static bool OnAssociated(IAsyncResult result)
            {
                ActivityExecutor.AssociateKeysAsyncResult asyncState = (ActivityExecutor.AssociateKeysAsyncResult) result.AsyncState;
                asyncState.executor.host.OnEndAssociateKeys(result);
                return true;
            }
        }

        [DataContract]
        private class CancelActivityWorkItem : ActivityExecutionWorkItem
        {
            public CancelActivityWorkItem(System.Activities.ActivityInstance activityInstance) : base(activityInstance)
            {
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                try
                {
                    base.ActivityInstance.Cancel(executor, bookmarkManager);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ExceptionToPropagate = exception;
                }
                return true;
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteCancelActivityWorkItemIsEnabled())
                {
                    TD.CompleteCancelActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleCancelActivityWorkItemIsEnabled())
                {
                    TD.ScheduleCancelActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartCancelActivityWorkItemIsEnabled())
                {
                    TD.StartCancelActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }
        }

        [DataContract]
        private class CompleteAsyncOperationWorkItem : BookmarkWorkItem
        {
            public CompleteAsyncOperationWorkItem(BookmarkCallbackWrapper wrapper, Bookmark bookmark, object value) : base(wrapper, bookmark, value)
            {
                base.ExitNoPersistRequired = true;
            }
        }

        private class CompleteTransactionWorkItem : System.Activities.Runtime.WorkItem
        {
            private static AsyncCallback commitCompleteCallback;
            private ActivityExecutor executor;
            private static Action<object, TimeoutException> outcomeDeterminedCallback;
            private static AsyncCallback persistCompleteCallback;
            private ActivityExecutor.RuntimeTransactionData runtimeTransaction;

            public CompleteTransactionWorkItem(System.Activities.ActivityInstance instance) : base(instance)
            {
                base.ExitNoPersistRequired = true;
            }

            private bool CheckOutcome()
            {
                AsyncWaitHandle handle = null;
                lock (this.runtimeTransaction)
                {
                    if (this.runtimeTransaction.TransactionStatus == TransactionStatus.Active)
                    {
                        handle = new AsyncWaitHandle();
                        this.runtimeTransaction.CompletionEvent = handle;
                    }
                }
                if ((handle != null) && !handle.WaitAsync(OutcomeDeterminedCallback, this, ActivityDefaults.TransactionCompletionTimeout))
                {
                    return false;
                }
                return this.FinishCheckOutcome();
            }

            private bool CheckTransactionAborted()
            {
                try
                {
                    Fx.ThrowIfTransactionAbortedOrInDoubt(this.runtimeTransaction.OriginalTransaction);
                    return false;
                }
                catch (TransactionException exception)
                {
                    if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                    {
                        base.workflowAbortException = exception;
                    }
                    else
                    {
                        base.ExceptionToPropagate = exception;
                    }
                    return true;
                }
            }

            private bool CompleteTransaction()
            {
                PreparingEnlistment pendingPreparingEnlistment = null;
                lock (this.runtimeTransaction)
                {
                    if (this.runtimeTransaction.PendingPreparingEnlistment != null)
                    {
                        pendingPreparingEnlistment = this.runtimeTransaction.PendingPreparingEnlistment;
                    }
                    this.runtimeTransaction.HasPrepared = true;
                }
                if (pendingPreparingEnlistment != null)
                {
                    pendingPreparingEnlistment.Prepared();
                }
                Transaction originalTransaction = this.runtimeTransaction.OriginalTransaction;
                DependentTransaction transaction2 = originalTransaction as DependentTransaction;
                if (transaction2 != null)
                {
                    transaction2.Complete();
                    return this.CheckOutcome();
                }
                CommittableTransaction transaction3 = originalTransaction as CommittableTransaction;
                if (transaction3 == null)
                {
                    return this.CheckOutcome();
                }
                IAsyncResult result = transaction3.BeginCommit(CommitCompleteCallback, this);
                return (result.CompletedSynchronously && this.FinishCommit(result));
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                bool flag;
                this.runtimeTransaction = executor.runtimeTransaction;
                this.executor = executor;
                this.executor.SchedulePendingCancelation();
                try
                {
                    flag = this.CheckTransactionAborted();
                    if (!flag)
                    {
                        IAsyncResult result = new TransactionalPersistAsyncResult(this.executor, PersistCompleteCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            flag = this.FinishPersist(result);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.HandleException(exception);
                    flag = true;
                }
                if (flag)
                {
                    this.executor.runtimeTransaction = null;
                    this.TraceTransactionOutcome();
                    return true;
                }
                return false;
            }

            private bool FinishCheckOutcome()
            {
                this.CheckTransactionAborted();
                return true;
            }

            private bool FinishCommit(IAsyncResult result)
            {
                ((CommittableTransaction) this.runtimeTransaction.OriginalTransaction).EndCommit(result);
                return this.CheckOutcome();
            }

            private bool FinishPersist(IAsyncResult result)
            {
                TransactionalPersistAsyncResult.End(result);
                return this.CompleteTransaction();
            }

            private void HandleException(Exception exception)
            {
                try
                {
                    this.runtimeTransaction.OriginalTransaction.Rollback(exception);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    base.workflowAbortException = exception2;
                }
                if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                {
                    base.workflowAbortException = exception;
                }
                else
                {
                    base.ExceptionToPropagate = exception;
                }
            }

            private static void OnCommitComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ActivityExecutor.CompleteTransactionWorkItem asyncState = (ActivityExecutor.CompleteTransactionWorkItem) result.AsyncState;
                    bool flag = true;
                    try
                    {
                        flag = asyncState.FinishCommit(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.HandleException(exception);
                        flag = true;
                    }
                    if (flag)
                    {
                        asyncState.executor.runtimeTransaction = null;
                        asyncState.TraceTransactionOutcome();
                        asyncState.executor.FinishWorkItem(asyncState);
                    }
                }
            }

            private static void OnOutcomeDetermined(object state, TimeoutException asyncException)
            {
                ActivityExecutor.CompleteTransactionWorkItem workItem = (ActivityExecutor.CompleteTransactionWorkItem) state;
                bool flag = true;
                if (asyncException != null)
                {
                    workItem.HandleException(asyncException);
                }
                else
                {
                    try
                    {
                        flag = workItem.FinishCheckOutcome();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        workItem.HandleException(exception);
                        flag = true;
                    }
                }
                if (flag)
                {
                    workItem.executor.runtimeTransaction = null;
                    workItem.TraceTransactionOutcome();
                    workItem.executor.FinishWorkItem(workItem);
                }
            }

            private static void OnPersistComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ActivityExecutor.CompleteTransactionWorkItem asyncState = (ActivityExecutor.CompleteTransactionWorkItem) result.AsyncState;
                    bool flag = true;
                    try
                    {
                        flag = asyncState.FinishPersist(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.HandleException(exception);
                        flag = true;
                    }
                    if (flag)
                    {
                        asyncState.executor.runtimeTransaction = null;
                        asyncState.TraceTransactionOutcome();
                        asyncState.executor.FinishWorkItem(asyncState);
                    }
                }
            }

            public override void PostProcess(ActivityExecutor executor)
            {
            }

            public override void TraceCompleted()
            {
                base.TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                base.TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                base.TraceRuntimeWorkItemStarting();
            }

            private void TraceTransactionOutcome()
            {
                if (TD.RuntimeTransactionCompleteIsEnabled())
                {
                    TD.RuntimeTransactionComplete(this.runtimeTransaction.TransactionStatus.ToString());
                }
            }

            private static AsyncCallback CommitCompleteCallback
            {
                get
                {
                    if (commitCompleteCallback == null)
                    {
                        commitCompleteCallback = Fx.ThunkCallback(new AsyncCallback(ActivityExecutor.CompleteTransactionWorkItem.OnCommitComplete));
                    }
                    return commitCompleteCallback;
                }
            }

            public override bool IsValid
            {
                get
                {
                    return true;
                }
            }

            private static Action<object, TimeoutException> OutcomeDeterminedCallback
            {
                get
                {
                    if (outcomeDeterminedCallback == null)
                    {
                        outcomeDeterminedCallback = new Action<object, TimeoutException>(ActivityExecutor.CompleteTransactionWorkItem.OnOutcomeDetermined);
                    }
                    return outcomeDeterminedCallback;
                }
            }

            private static AsyncCallback PersistCompleteCallback
            {
                get
                {
                    if (persistCompleteCallback == null)
                    {
                        persistCompleteCallback = Fx.ThunkCallback(new AsyncCallback(ActivityExecutor.CompleteTransactionWorkItem.OnPersistComplete));
                    }
                    return persistCompleteCallback;
                }
            }

            public override System.Activities.ActivityInstance PropertyManagerOwner
            {
                get
                {
                    return null;
                }
            }

            private class TransactionalPersistAsyncResult : AsyncResult
            {
                private readonly ActivityExecutor executor;
                private static readonly AsyncResult.AsyncCompletion onPersistComplete = new AsyncResult.AsyncCompletion(ActivityExecutor.CompleteTransactionWorkItem.TransactionalPersistAsyncResult.OnPersistComplete);
                private ActivityExecutor.CompleteTransactionWorkItem workItem;

                public TransactionalPersistAsyncResult(ActivityExecutor executor, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.executor = executor;
                    this.workItem = (ActivityExecutor.CompleteTransactionWorkItem) state;
                    IAsyncResult result = null;
                    IDisposable disposable = base.PrepareTransactionalCall(this.executor.CurrentTransaction);
                    try
                    {
                        result = this.executor.host.OnBeginPersist(base.PrepareAsyncCompletion(onPersistComplete), this);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.workItem.workflowAbortException = exception;
                        throw;
                    }
                    finally
                    {
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    if (base.SyncContinue(result))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ActivityExecutor.CompleteTransactionWorkItem.TransactionalPersistAsyncResult>(result);
                }

                private static bool OnPersistComplete(IAsyncResult result)
                {
                    ActivityExecutor.CompleteTransactionWorkItem.TransactionalPersistAsyncResult asyncState = (ActivityExecutor.CompleteTransactionWorkItem.TransactionalPersistAsyncResult) result.AsyncState;
                    try
                    {
                        asyncState.executor.host.OnEndPersist(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        asyncState.workItem.workflowAbortException = exception;
                        throw;
                    }
                    return true;
                }
            }
        }

        private class EmptyDelegateActivity : NativeActivity
        {
            internal EmptyDelegateActivity()
            {
            }

            protected override void Execute(NativeActivityContext context)
            {
            }
        }

        [DataContract]
        private class ExecuteActivityWorkItem : ActivityExecutionWorkItem
        {
            [DataMember(EmitDefaultValue=false)]
            private IDictionary<string, object> argumentValueOverrides;
            [DataMember(EmitDefaultValue=false)]
            private bool requiresSymbolResolution;

            public ExecuteActivityWorkItem()
            {
                base.IsPooled = true;
            }

            protected ExecuteActivityWorkItem(System.Activities.ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides) : base(activityInstance)
            {
                this.requiresSymbolResolution = requiresSymbolResolution;
                this.argumentValueOverrides = argumentValueOverrides;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return this.ExecuteBody(executor, bookmarkManager, null);
            }

            protected bool ExecuteBody(ActivityExecutor executor, BookmarkManager bookmarkManager, System.Activities.Location resultLocation)
            {
                try
                {
                    if (this.requiresSymbolResolution)
                    {
                        if (!base.ActivityInstance.ResolveArguments(executor, this.argumentValueOverrides, resultLocation, 0))
                        {
                            return true;
                        }
                        if (!base.ActivityInstance.ResolveVariables(executor))
                        {
                            return true;
                        }
                    }
                    base.ActivityInstance.SetInitializedSubstate(executor);
                    if (executor.IsDebugged())
                    {
                        executor.debugController.ActivityStarted(base.ActivityInstance);
                    }
                    base.ActivityInstance.Execute(executor, bookmarkManager);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.ExceptionToPropagate = exception;
                }
                return true;
            }

            public void Initialize(System.Activities.ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides)
            {
                base.Reinitialize(activityInstance);
                this.requiresSymbolResolution = requiresSymbolResolution;
                this.argumentValueOverrides = argumentValueOverrides;
            }

            protected override void ReleaseToPool(ActivityExecutor executor)
            {
                base.ClearForReuse();
                this.requiresSymbolResolution = false;
                this.argumentValueOverrides = null;
                executor.ExecuteActivityWorkItemPool.Release(this);
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteExecuteActivityWorkItemIsEnabled())
                {
                    TD.CompleteExecuteActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleExecuteActivityWorkItemIsEnabled())
                {
                    TD.ScheduleExecuteActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartExecuteActivityWorkItemIsEnabled())
                {
                    TD.StartExecuteActivityWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }
        }

        [DataContract]
        private class ExecuteExpressionWorkItem : ActivityExecutor.ExecuteActivityWorkItem
        {
            [DataMember]
            private System.Activities.Location resultLocation;

            public ExecuteExpressionWorkItem(System.Activities.ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides, System.Activities.Location resultLocation) : base(activityInstance, requiresSymbolResolution, argumentValueOverrides)
            {
                this.resultLocation = resultLocation;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return base.ExecuteBody(executor, bookmarkManager, this.resultLocation);
            }
        }

        [DataContract]
        private class ExecuteRootWorkItem : ActivityExecutor.ExecuteActivityWorkItem
        {
            public ExecuteRootWorkItem(System.Activities.ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides) : base(activityInstance, requiresSymbolResolution, argumentValueOverrides)
            {
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                if (executor.ShouldTrackActivityScheduledRecords)
                {
                    executor.AddTrackingRecord(new ActivityScheduledRecord(executor.WorkflowInstanceId, null, base.ActivityInstance));
                }
                return base.ExecuteBody(executor, bookmarkManager, null);
            }
        }

        [DataContract]
        private class PersistenceWaiter
        {
            public PersistenceWaiter(Bookmark onPersist, System.Activities.ActivityInstance waitingInstance)
            {
                this.OnPersistBookmark = onPersist;
                this.WaitingInstance = waitingInstance;
            }

            public System.Activities.Runtime.WorkItem CreateWorkItem()
            {
                return new PersistWorkItem(this);
            }

            [DataMember]
            public Bookmark OnPersistBookmark { get; private set; }

            [DataMember]
            public System.Activities.ActivityInstance WaitingInstance { get; private set; }

            [DataContract]
            private class PersistWorkItem : System.Activities.Runtime.WorkItem
            {
                [DataMember]
                private ActivityExecutor.PersistenceWaiter waiter;

                public PersistWorkItem(ActivityExecutor.PersistenceWaiter waiter) : base(waiter.WaitingInstance)
                {
                    this.waiter = waiter;
                }

                public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
                {
                    executor.TryResumeUserBookmark(this.waiter.OnPersistBookmark, null, false);
                    IAsyncResult result = null;
                    try
                    {
                        result = executor.host.OnBeginPersist(Fx.ThunkCallback(new AsyncCallback(this.OnPersistComplete)), executor);
                        if (result.CompletedSynchronously)
                        {
                            executor.host.OnEndPersist(result);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        base.workflowAbortException = exception;
                    }
                    if (result != null)
                    {
                        return result.CompletedSynchronously;
                    }
                    return true;
                }

                private void OnPersistComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        ActivityExecutor asyncState = (ActivityExecutor) result.AsyncState;
                        try
                        {
                            asyncState.host.OnEndPersist(result);
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            base.workflowAbortException = exception;
                        }
                        asyncState.FinishWorkItem(this);
                    }
                }

                public override void PostProcess(ActivityExecutor executor)
                {
                    if (base.ExceptionToPropagate != null)
                    {
                        executor.AbortActivityInstance(this.waiter.WaitingInstance, base.ExceptionToPropagate);
                    }
                }

                public override void TraceCompleted()
                {
                    base.TraceRuntimeWorkItemCompleted();
                }

                public override void TraceScheduled()
                {
                    base.TraceRuntimeWorkItemScheduled();
                }

                public override void TraceStarting()
                {
                    base.TraceRuntimeWorkItemStarting();
                }

                public override bool IsValid
                {
                    get
                    {
                        return true;
                    }
                }

                public override System.Activities.ActivityInstance PropertyManagerOwner
                {
                    get
                    {
                        return null;
                    }
                }
            }
        }

        private class PoolOfCodeActivityContexts : Pool<CodeActivityContext>
        {
            protected override CodeActivityContext CreateNew()
            {
                return new CodeActivityContext();
            }
        }

        private class PoolOfCompletionWorkItems : Pool<CompletionCallbackWrapper.CompletionWorkItem>
        {
            protected override CompletionCallbackWrapper.CompletionWorkItem CreateNew()
            {
                return new CompletionCallbackWrapper.CompletionWorkItem();
            }
        }

        private class PoolOfEmptyWorkItems : Pool<EmptyWorkItem>
        {
            protected override EmptyWorkItem CreateNew()
            {
                return new EmptyWorkItem();
            }
        }

        private class PoolOfExecuteActivityWorkItems : Pool<ActivityExecutor.ExecuteActivityWorkItem>
        {
            protected override ActivityExecutor.ExecuteActivityWorkItem CreateNew()
            {
                return new ActivityExecutor.ExecuteActivityWorkItem();
            }
        }

        private class PoolOfNativeActivityContexts : Pool<NativeActivityContext>
        {
            protected override NativeActivityContext CreateNew()
            {
                return new NativeActivityContext();
            }
        }

        private class PoolOfResolveNextArgumentWorkItems : Pool<ResolveNextArgumentWorkItem>
        {
            protected override ResolveNextArgumentWorkItem CreateNew()
            {
                return new ResolveNextArgumentWorkItem();
            }
        }

        [DataContract]
        private class RethrowExceptionWorkItem : System.Activities.Runtime.WorkItem
        {
            [DataMember]
            private Exception exception;
            [DataMember]
            private ActivityInstanceReference source;

            public RethrowExceptionWorkItem(System.Activities.ActivityInstance activityInstance, Exception exception, ActivityInstanceReference source) : base(activityInstance)
            {
                this.exception = exception;
                this.source = source;
                base.IsEmpty = true;
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return true;
            }

            public override void PostProcess(ActivityExecutor executor)
            {
                executor.AbortActivityInstance(base.ActivityInstance, base.ExceptionToPropagate);
                base.ExceptionToPropagate = this.exception;
            }

            public override void TraceCompleted()
            {
                base.TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                base.TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                base.TraceRuntimeWorkItemStarting();
            }

            public override bool IsValid
            {
                get
                {
                    return (base.ActivityInstance.State == ActivityInstanceState.Executing);
                }
            }

            public override System.Activities.ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.source.ActivityInstance;
                }
            }

            public override System.Activities.ActivityInstance PropertyManagerOwner
            {
                get
                {
                    return null;
                }
            }
        }

        private class RuntimeTransactionData
        {
            public RuntimeTransactionData(RuntimeTransactionHandle handle, Transaction transaction, System.Activities.ActivityInstance isolationScope)
            {
                this.TransactionHandle = handle;
                this.OriginalTransaction = transaction;
                this.ClonedTransaction = transaction.Clone();
                this.IsolationScope = isolationScope;
                this.TransactionStatus = System.Transactions.TransactionStatus.Active;
            }

            public void Rollback(Exception reason)
            {
                this.OriginalTransaction.Rollback(reason);
            }

            public Transaction ClonedTransaction { get; private set; }

            public AsyncWaitHandle CompletionEvent { get; set; }

            public bool HasPrepared { get; set; }

            public System.Activities.ActivityInstance IsolationScope { get; private set; }

            public bool IsRootCancelPending { get; set; }

            public Transaction OriginalTransaction { get; private set; }

            public PreparingEnlistment PendingPreparingEnlistment { get; set; }

            public bool ShouldScheduleCompletion { get; set; }

            public RuntimeTransactionHandle TransactionHandle { get; private set; }

            public System.Transactions.TransactionStatus TransactionStatus { get; set; }
        }

        [DataContract]
        private class TransactionContextWaiter
        {
            public TransactionContextWaiter(System.Activities.ActivityInstance instance, bool isRequires, RuntimeTransactionHandle handle, ActivityExecutor.TransactionContextWaiterCallbackWrapper callbackWrapper, object state)
            {
                this.WaitingInstance = instance;
                this.IsRequires = isRequires;
                this.Handle = handle;
                this.State = state;
                this.CallbackWrapper = callbackWrapper;
            }

            [DataMember]
            public ActivityExecutor.TransactionContextWaiterCallbackWrapper CallbackWrapper { get; private set; }

            [DataMember]
            public RuntimeTransactionHandle Handle { get; private set; }

            [DataMember(EmitDefaultValue=false)]
            public bool IsRequires { get; private set; }

            [DataMember(EmitDefaultValue=false)]
            public object State { get; private set; }

            [DataMember]
            public System.Activities.ActivityInstance WaitingInstance { get; private set; }
        }

        [DataContract]
        private class TransactionContextWaiterCallbackWrapper : CallbackWrapper
        {
            private static Type callbackType = typeof(Action<NativeActivityTransactionContext, object>);
            private static Type[] transactionCallbackParameterTypes = new Type[] { typeof(NativeActivityTransactionContext), typeof(object) };

            public TransactionContextWaiterCallbackWrapper(Action<NativeActivityTransactionContext, object> action, System.Activities.ActivityInstance owningInstance) : base(action, owningInstance)
            {
            }

            public void Invoke(NativeActivityTransactionContext context, object value)
            {
                base.EnsureCallback(callbackType, transactionCallbackParameterTypes);
                Action<NativeActivityTransactionContext, object> callback = (Action<NativeActivityTransactionContext, object>) base.Callback;
                callback(context, value);
            }
        }

        [DataContract]
        private class TransactionContextWorkItem : ActivityExecutionWorkItem
        {
            [DataMember]
            private ActivityExecutor.TransactionContextWaiter waiter;

            public TransactionContextWorkItem(ActivityExecutor.TransactionContextWaiter waiter) : base(waiter.WaitingInstance)
            {
                this.waiter = waiter;
                if (this.waiter.IsRequires)
                {
                    base.ExitNoPersistRequired = true;
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityTransactionContext context = null;
                try
                {
                    context = new NativeActivityTransactionContext(base.ActivityInstance, executor, bookmarkManager, this.waiter.Handle);
                    this.waiter.CallbackWrapper.Invoke(context, this.waiter.State);
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
                    if (context != null)
                    {
                        context.Dispose();
                    }
                }
                return true;
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteTransactionContextWorkItemIsEnabled())
                {
                    TD.CompleteTransactionContextWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleTransactionContextWorkItemIsEnabled())
                {
                    TD.ScheduleTransactionContextWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartTransactionContextWorkItemIsEnabled())
                {
                    TD.StartTransactionContextWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id);
                }
            }
        }
    }
}

