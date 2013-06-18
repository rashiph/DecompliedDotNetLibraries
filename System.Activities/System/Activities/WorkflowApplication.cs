namespace System.Activities
{
    using System;
    using System.Activities.DurableInstancing;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    public sealed class WorkflowApplication : WorkflowInstance
    {
        private int actionCount;
        private static CompletedEventHandler completedHandler;
        private WorkflowEventData eventData;
        private static AsyncCallback eventFrameCallback;
        private WorkflowInstanceExtensionManager extensions;
        private int handlerThreadId;
        private bool hasCalledAbort;
        private bool hasCalledRun;
        private bool hasExecutionOccurredSinceLastIdle;
        private bool hasRaisedCompleted;
        private static IdleEventHandler idleHandler;
        private IDictionary<string, object> initialWorkflowArguments;
        private Guid instanceId;
        private bool instanceIdSet;
        private IDictionary<XName, InstanceValue> instanceMetadata;
        private System.Runtime.DurableInstancing.InstanceStore instanceStore;
        private Action invokeCompletedCallback;
        private bool isBusy;
        private bool isInHandler;
        private Action<WorkflowApplicationAbortedEventArgs> onAborted;
        private Action<WorkflowApplicationCompletedEventArgs> onCompleted;
        private Action<WorkflowApplicationIdleEventArgs> onIdle;
        private Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction> onPersistableIdle;
        private Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> onUnhandledException;
        private Action<WorkflowApplicationEventArgs> onUnloaded;
        private Quack<InstanceOperation> pendingOperations;
        private PersistenceManager persistenceManager;
        private PersistencePipeline persistencePipelineInUse;
        private IList<Handle> rootExecutionProperties;
        private WorkflowApplicationState state;
        private static UnhandledExceptionEventHandler unhandledExceptionHandler;
        private static Action<object, TimeoutException> waitAsyncCompleteCallback;

        public WorkflowApplication(Activity workflowDefinition) : base(workflowDefinition)
        {
            this.pendingOperations = new Quack<InstanceOperation>();
        }

        public WorkflowApplication(Activity workflowDefinition, IDictionary<string, object> inputs) : this(workflowDefinition)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }
            this.initialWorkflowArguments = inputs;
        }

        private WorkflowApplication(Activity workflowDefinition, IDictionary<string, object> inputs, IList<Handle> executionProperties) : this(workflowDefinition)
        {
            this.initialWorkflowArguments = inputs;
            this.rootExecutionProperties = executionProperties;
        }

        public void Abort()
        {
            this.Abort(System.Activities.SR.DefaultAbortReason);
        }

        public void Abort(string reason)
        {
            if (this.state != WorkflowApplicationState.Aborted)
            {
                this.AbortInstance(new WorkflowApplicationAbortedException(reason), false);
            }
        }

        private void AbortInstance(Exception reason, bool isWorkflowThread)
        {
            this.state = WorkflowApplicationState.Aborted;
            Thread.MemoryBarrier();
            this.AbortPersistence();
            if (isWorkflowThread)
            {
                if (!this.hasCalledAbort)
                {
                    this.hasCalledAbort = true;
                    base.Controller.Abort(reason);
                    this.ScheduleTrackAndRaiseAborted(reason);
                }
            }
            else
            {
                bool flag = true;
                InstanceOperation operation = null;
                try
                {
                    operation = new InstanceOperation();
                    flag = this.WaitForTurnAsync(operation, true, ActivityDefaults.AcquireLockTimeout, new Action<object, TimeoutException>(this.OnAbortWaitComplete), reason);
                    if (flag && !this.hasCalledAbort)
                    {
                        this.hasCalledAbort = true;
                        base.Controller.Abort(reason);
                        this.ScheduleTrackAndRaiseAborted(reason);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.NotifyOperationComplete(operation);
                    }
                }
            }
        }

        private void AbortPersistence()
        {
            if (this.persistenceManager != null)
            {
                this.persistenceManager.Abort();
            }
            PersistencePipeline persistencePipelineInUse = this.persistencePipelineInUse;
            if (persistencePipelineInUse != null)
            {
                persistencePipelineInUse.Abort();
            }
        }

        public void AddInitialInstanceValues(IDictionary<XName, object> writeOnlyValues)
        {
            base.ThrowIfReadOnly();
            if (writeOnlyValues != null)
            {
                if (this.instanceMetadata == null)
                {
                    this.instanceMetadata = new Dictionary<XName, InstanceValue>(writeOnlyValues.Count);
                }
                foreach (KeyValuePair<XName, object> pair in writeOnlyValues)
                {
                    this.instanceMetadata[pair.Key] = new InstanceValue(pair.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                }
            }
        }

        private void AddToPending(InstanceOperation operation, bool push)
        {
            if (push)
            {
                this.pendingOperations.PushFront(operation);
            }
            else
            {
                this.pendingOperations.Enqueue(operation);
            }
            operation.OnEnqueued();
        }

        private bool AreBookmarksInvalid(out BookmarkResumptionResult result)
        {
            if (this.hasRaisedCompleted)
            {
                result = BookmarkResumptionResult.NotFound;
                return true;
            }
            if ((this.state == WorkflowApplicationState.Unloaded) || (this.state == WorkflowApplicationState.Aborted))
            {
                result = BookmarkResumptionResult.NotReady;
                return true;
            }
            result = BookmarkResumptionResult.Success;
            return false;
        }

        public IAsyncResult BeginCancel(AsyncCallback callback, object state)
        {
            return this.BeginCancel(ActivityDefaults.AcquireLockTimeout, callback, state);
        }

        public IAsyncResult BeginCancel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return CancelAsyncResult.Create(this, timeout, callback, state);
        }

        private IAsyncResult BeginInternalPersist(PersistenceOperation operation, TimeSpan timeout, bool isInternalPersist, AsyncCallback callback, object state)
        {
            return new UnloadOrPersistAsyncResult(this, timeout, operation, true, isInternalPersist, callback, state);
        }

        private IAsyncResult BeginInternalRun(TimeSpan timeout, bool isUserRun, AsyncCallback callback, object state)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return RunAsyncResult.Create(this, isUserRun, timeout, callback, state);
        }

        internal static IAsyncResult BeginInvoke(Activity activity, IDictionary<string, object> inputs, WorkflowInstanceExtensionManager extensions, TimeSpan timeout, SynchronizationContext syncContext, AsyncInvokeContext invokeContext, AsyncCallback callback, object state)
        {
            return new InvokeAsyncResult(activity, inputs, extensions, timeout, syncContext, invokeContext, callback, state);
        }

        public IAsyncResult BeginLoad(Guid instanceId, AsyncCallback callback, object state)
        {
            return this.BeginLoad(instanceId, ActivityDefaults.LoadTimeout, callback, state);
        }

        public IAsyncResult BeginLoad(Guid instanceId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfAborted();
            base.ThrowIfReadOnly();
            if (instanceId == Guid.Empty)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("instanceId");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (this.InstanceStore == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.LoadingWorkflowApplicationRequiresInstanceStore));
            }
            if (this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
            }
            if (this.initialWorkflowArguments != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUseInputsWithLoad));
            }
            this.instanceId = instanceId;
            this.instanceIdSet = true;
            this.CreatePersistenceManager();
            return new LoadAsyncResult(this, false, timeout, callback, state);
        }

        public IAsyncResult BeginLoadRunnableInstance(AsyncCallback callback, object state)
        {
            return this.BeginLoadRunnableInstance(ActivityDefaults.LoadTimeout, callback, state);
        }

        public IAsyncResult BeginLoadRunnableInstance(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfReadOnly();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (this.InstanceStore == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.LoadingWorkflowApplicationRequiresInstanceStore));
            }
            if (this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
            }
            if (this.initialWorkflowArguments != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUseInputsWithLoad));
            }
            if (this.persistenceManager != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TryLoadRequiresOwner));
            }
            base.RegisterExtensionManager(this.extensions);
            this.persistenceManager = new PersistenceManager(this.InstanceStore, this.instanceMetadata);
            if (!this.persistenceManager.IsInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TryLoadRequiresOwner));
            }
            return new LoadAsyncResult(this, true, timeout, callback, state);
        }

        public IAsyncResult BeginPersist(AsyncCallback callback, object state)
        {
            return this.BeginPersist(ActivityDefaults.SaveTimeout, callback, state);
        }

        public IAsyncResult BeginPersist(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return new UnloadOrPersistAsyncResult(this, timeout, PersistenceOperation.Save, false, false, callback, state);
        }

        public IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, AsyncCallback callback, object state)
        {
            return this.BeginResumeBookmark(bookmark, value, ActivityDefaults.ResumeBookmarkTimeout, callback, state);
        }

        public IAsyncResult BeginResumeBookmark(string bookmarkName, object value, AsyncCallback callback, object state)
        {
            if (string.IsNullOrEmpty(bookmarkName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("bookmarkName");
            }
            return this.BeginResumeBookmark(new Bookmark(bookmarkName), value, callback, state);
        }

        public IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.ThrowIfHandlerThread();
            return new ResumeBookmarkAsyncResult(this, bookmark, value, timeout, callback, state);
        }

        public IAsyncResult BeginResumeBookmark(string bookmarkName, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (string.IsNullOrEmpty(bookmarkName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("bookmarkName");
            }
            return this.BeginResumeBookmark(new Bookmark(bookmarkName), value, timeout, callback, state);
        }

        public IAsyncResult BeginRun(AsyncCallback callback, object state)
        {
            return this.BeginRun(ActivityDefaults.AcquireLockTimeout, callback, state);
        }

        public IAsyncResult BeginRun(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginInternalRun(timeout, true, callback, state);
        }

        public IAsyncResult BeginTerminate(Exception reason, AsyncCallback callback, object state)
        {
            return this.BeginTerminate(reason, ActivityDefaults.AcquireLockTimeout, callback, state);
        }

        public IAsyncResult BeginTerminate(string reason, AsyncCallback callback, object state)
        {
            return this.BeginTerminate(reason, ActivityDefaults.AcquireLockTimeout, callback, state);
        }

        public IAsyncResult BeginTerminate(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (reason == null)
            {
                throw FxTrace.Exception.ArgumentNull("reason");
            }
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return TerminateAsyncResult.Create(this, reason, timeout, callback, state);
        }

        public IAsyncResult BeginTerminate(string reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            return this.BeginTerminate(new WorkflowApplicationTerminatedException(reason, this.Id), timeout, callback, state);
        }

        public IAsyncResult BeginUnload(AsyncCallback callback, object state)
        {
            return this.BeginUnload(ActivityDefaults.SaveTimeout, callback, state);
        }

        public IAsyncResult BeginUnload(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return new UnloadOrPersistAsyncResult(this, timeout, PersistenceOperation.Unload, false, false, callback, state);
        }

        public void Cancel()
        {
            this.Cancel(ActivityDefaults.AcquireLockTimeout);
        }

        public void Cancel(TimeSpan timeout)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            InstanceOperation operation = null;
            try
            {
                operation = new InstanceOperation();
                this.WaitForTurn(operation, helper.RemainingTime());
                this.ValidateStateForCancel();
                this.CancelCore();
                base.Controller.FlushTrackingRecords(helper.RemainingTime());
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        }

        private void CancelCore()
        {
            if (!this.hasRaisedCompleted && (this.state != WorkflowApplicationState.Unloaded))
            {
                base.Controller.ScheduleCancel();
                if (!this.hasCalledRun && !this.hasRaisedCompleted)
                {
                    this.RunCore();
                }
            }
        }

        private static System.Activities.WorkflowApplication CreateInstance(Activity activity, IDictionary<string, object> inputs, WorkflowInstanceExtensionManager extensions, SynchronizationContext syncContext, Action invokeCompletedCallback)
        {
            Transaction current = Transaction.Current;
            List<Handle> executionProperties = null;
            if (current != null)
            {
                executionProperties = new List<Handle>(1) {
                    new RuntimeTransactionHandle(current)
                };
            }
            System.Activities.WorkflowApplication application = new System.Activities.WorkflowApplication(activity, inputs, executionProperties) {
                SynchronizationContext = syncContext
            };
            bool flag = false;
            try
            {
                application.isBusy = true;
                if (extensions != null)
                {
                    application.extensions = extensions;
                }
                application.invokeCompletedCallback = invokeCompletedCallback;
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    application.isBusy = false;
                }
            }
            return application;
        }

        private void CreatePersistenceManager()
        {
            base.RegisterExtensionManager(this.extensions);
            this.persistenceManager = new PersistenceManager(this.InstanceStore, this.instanceMetadata, this.instanceId);
        }

        public void EndCancel(IAsyncResult result)
        {
            CancelAsyncResult.End(result);
        }

        private void EndInternalPersist(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        internal static IDictionary<string, object> EndInvoke(IAsyncResult result)
        {
            return InvokeAsyncResult.End(result);
        }

        public void EndLoad(IAsyncResult result)
        {
            LoadAsyncResult.End(result);
        }

        public void EndLoadRunnableInstance(IAsyncResult result)
        {
            LoadAsyncResult.End(result);
        }

        public void EndPersist(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        public BookmarkResumptionResult EndResumeBookmark(IAsyncResult result)
        {
            return ResumeBookmarkAsyncResult.End(result);
        }

        public void EndRun(IAsyncResult result)
        {
            RunAsyncResult.End(result);
        }

        public void EndTerminate(IAsyncResult result)
        {
            TerminateAsyncResult.End(result);
        }

        public void EndUnload(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        private void Enqueue(InstanceOperation operation)
        {
            this.Enqueue(operation, false);
        }

        private void Enqueue(InstanceOperation operation, bool push)
        {
            lock (this.pendingOperations)
            {
                this.EnsureInitialized();
                operation.ActionId = this.actionCount;
                if (this.isBusy)
                {
                    if (operation.InterruptsScheduler)
                    {
                        base.Controller.RequestPause();
                    }
                    this.AddToPending(operation, push);
                }
                else if (!operation.CanRun(this))
                {
                    this.AddToPending(operation, push);
                }
                else
                {
                    this.actionCount++;
                    try
                    {
                    }
                    finally
                    {
                        operation.Notified = true;
                        this.isBusy = true;
                    }
                }
            }
        }

        private void EnsureInitialized()
        {
            if (!base.IsReadOnly)
            {
                base.RegisterExtensionManager(this.extensions);
                base.Initialize(this.initialWorkflowArguments, this.rootExecutionProperties);
                if ((this.persistenceManager == null) && (this.instanceStore != null))
                {
                    this.persistenceManager = new PersistenceManager(this.instanceStore, this.instanceMetadata, this.Id);
                }
            }
        }

        private static void EventFrame(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                WorkflowEventData asyncState = (WorkflowEventData) result.AsyncState;
                System.Activities.WorkflowApplication instance = asyncState.Instance;
                bool flag = true;
                try
                {
                    Exception reason = null;
                    try
                    {
                        flag = asyncState.NextCallback(result, instance, false);
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
                        instance.AbortInstance(reason, true);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        instance.OnNotifyPaused();
                    }
                }
            }
        }

        private InstanceOperation FindOperation()
        {
            if (this.pendingOperations.Count > 0)
            {
                InstanceOperation operation = this.pendingOperations[0];
                if (operation.CanRun(this) || this.IsInTerminalState)
                {
                    this.actionCount++;
                    operation.Notified = true;
                    this.pendingOperations.Dequeue();
                    return operation;
                }
                for (int i = 0; i < this.pendingOperations.Count; i++)
                {
                    operation = this.pendingOperations[i];
                    if (operation.CanRun(this))
                    {
                        this.actionCount++;
                        operation.Notified = true;
                        this.pendingOperations.Remove(i);
                        return operation;
                    }
                }
            }
            return null;
        }

        private void ForceNotifyOperationComplete()
        {
            this.OnNotifyPaused();
        }

        public ReadOnlyCollection<BookmarkInfo> GetBookmarks()
        {
            return this.GetBookmarks(ActivityDefaults.ResumeBookmarkTimeout);
        }

        public ReadOnlyCollection<BookmarkInfo> GetBookmarks(TimeSpan timeout)
        {
            ReadOnlyCollection<BookmarkInfo> bookmarks;
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            InstanceOperation operation = new InstanceOperation();
            try
            {
                this.WaitForTurn(operation, timeout);
                this.ValidateStateForGetAllBookmarks();
                bookmarks = base.Controller.GetBookmarks();
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
            return bookmarks;
        }

        internal ReadOnlyCollection<BookmarkInfo> GetBookmarksForIdle()
        {
            return base.Controller.GetBookmarks();
        }

        internal void GetCompletionStatus(out Exception terminationException, out bool cancelled)
        {
            IDictionary<string, object> dictionary;
            ActivityInstanceState completionState = base.Controller.GetCompletionState(out dictionary, out terminationException);
            cancelled = completionState == ActivityInstanceState.Canceled;
        }

        internal IEnumerable<T> InternalGetExtensions<T>() where T: class
        {
            return base.GetExtensions<T>();
        }

        private void InternalRun(TimeSpan timeout, bool isUserRun)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            InstanceOperation operation = null;
            try
            {
                operation = new InstanceOperation();
                this.WaitForTurn(operation, helper.RemainingTime());
                this.ValidateStateForRun();
                if (isUserRun)
                {
                    this.hasExecutionOccurredSinceLastIdle = true;
                }
                this.RunCore();
                base.Controller.FlushTrackingRecords(helper.RemainingTime());
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        }

        internal static IDictionary<string, object> Invoke(Activity activity, IDictionary<string, object> inputs, WorkflowInstanceExtensionManager extensions, TimeSpan timeout)
        {
            PumpBasedSynchronizationContext syncContext = new PumpBasedSynchronizationContext(timeout);
            System.Activities.WorkflowApplication instance = CreateInstance(activity, inputs, extensions, syncContext, new Action(syncContext.OnInvokeCompleted));
            try
            {
                RunInstance(instance);
                syncContext.DoPump();
            }
            catch (TimeoutException)
            {
                instance.Abort(System.Activities.SR.AbortingDueToInstanceTimeout);
                throw;
            }
            Exception terminationException = null;
            IDictionary<string, object> outputs = null;
            if (instance.Controller.State == WorkflowInstanceState.Aborted)
            {
                terminationException = new WorkflowApplicationAbortedException(System.Activities.SR.DefaultAbortReason, instance.Controller.GetAbortReason());
            }
            else
            {
                instance.Controller.GetCompletionState(out outputs, out terminationException);
            }
            if (terminationException != null)
            {
                throw FxTrace.Exception.AsError(terminationException);
            }
            return outputs;
        }

        private bool IsLoadTransactionRequired()
        {
            return base.GetExtensions<IPersistencePipelineModule>().Any<IPersistencePipelineModule>(module => module.IsLoadTransactionRequired);
        }

        public void Load(Guid instanceId)
        {
            this.Load(instanceId, ActivityDefaults.LoadTimeout);
        }

        public void Load(Guid instanceId, TimeSpan timeout)
        {
            this.ThrowIfAborted();
            base.ThrowIfReadOnly();
            if (instanceId == Guid.Empty)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("instanceId");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (this.InstanceStore == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.LoadingWorkflowApplicationRequiresInstanceStore));
            }
            if (this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
            }
            if (this.initialWorkflowArguments != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUseInputsWithLoad));
            }
            this.instanceId = instanceId;
            this.instanceIdSet = true;
            this.CreatePersistenceManager();
            this.LoadCore(timeout, false);
        }

        private void LoadCore(TimeSpan timeout, bool loadAny)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.persistenceManager.IsInitialized)
            {
                this.persistenceManager.Initialize(helper.RemainingTime());
            }
            PersistencePipeline pipeline = null;
            WorkflowPersistenceContext context = null;
            TransactionScope scope = null;
            bool flag = false;
            try
            {
                IDictionary<XName, InstanceValue> dictionary;
                object obj2;
                context = new WorkflowPersistenceContext(this.IsLoadTransactionRequired(), helper.OriginalTimeout);
                scope = Fx.CreateTransactionScope(context.PublicTransaction);
                if (loadAny)
                {
                    if (!this.persistenceManager.TryLoad(helper.RemainingTime(), out dictionary))
                    {
                        throw FxTrace.Exception.AsError(new InstanceNotReadyException(System.Activities.SR.NoRunnableInstances));
                    }
                    if (this.instanceIdSet)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
                    }
                    this.instanceId = this.persistenceManager.InstanceId;
                    this.instanceIdSet = true;
                }
                else
                {
                    dictionary = this.persistenceManager.Load(helper.RemainingTime());
                }
                pipeline = this.ProcessInstanceValues(dictionary, out obj2);
                if (pipeline != null)
                {
                    try
                    {
                        this.persistencePipelineInUse = pipeline;
                        Thread.MemoryBarrier();
                        if (this.state == WorkflowApplicationState.Aborted)
                        {
                            throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.DefaultAbortReason));
                        }
                        pipeline.EndLoad(pipeline.BeginLoad(helper.RemainingTime(), null, null));
                    }
                    finally
                    {
                        this.persistencePipelineInUse = null;
                    }
                }
                base.Initialize(obj2);
                flag = true;
            }
            finally
            {
                Fx.CompleteTransactionScope(ref scope);
                if (context != null)
                {
                    if (flag)
                    {
                        context.Complete();
                    }
                    else
                    {
                        context.Abort();
                    }
                }
                if (!flag)
                {
                    this.Abort(System.Activities.SR.AbortingDueToLoadFailure);
                }
            }
            if (pipeline != null)
            {
                pipeline.Publish();
            }
        }

        public void LoadRunnableInstance()
        {
            this.LoadRunnableInstance(ActivityDefaults.LoadTimeout);
        }

        public void LoadRunnableInstance(TimeSpan timeout)
        {
            base.ThrowIfReadOnly();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (this.InstanceStore == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.LoadingWorkflowApplicationRequiresInstanceStore));
            }
            if (this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
            }
            if (this.initialWorkflowArguments != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUseInputsWithLoad));
            }
            if (this.persistenceManager != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TryLoadRequiresOwner));
            }
            base.RegisterExtensionManager(this.extensions);
            this.persistenceManager = new PersistenceManager(this.InstanceStore, this.instanceMetadata);
            if (!this.persistenceManager.IsInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TryLoadRequiresOwner));
            }
            this.LoadCore(timeout, true);
        }

        private void MarkUnloaded()
        {
            this.state = WorkflowApplicationState.Unloaded;
            if (base.Controller.State != WorkflowInstanceState.Complete)
            {
                base.Controller.Abort();
            }
            else
            {
                base.DisposeExtensions();
            }
            Exception reason = null;
            try
            {
                Action<WorkflowApplicationEventArgs> unloaded = this.Unloaded;
                if (unloaded != null)
                {
                    this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                    this.isInHandler = true;
                    unloaded(new WorkflowApplicationEventArgs(this));
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
            finally
            {
                this.isInHandler = false;
            }
            if (reason != null)
            {
                this.AbortInstance(reason, true);
            }
        }

        private void NotifyOperationComplete(InstanceOperation operation)
        {
            if ((operation != null) && operation.Notified)
            {
                this.OnNotifyPaused();
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
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                }
                this.RaiseAborted(asyncState);
            }
        }

        private void OnAbortWaitComplete(object state, TimeoutException exception)
        {
            if (exception == null)
            {
                bool flag = false;
                Exception reason = (Exception) state;
                try
                {
                    if (!this.hasCalledAbort)
                    {
                        flag = true;
                        this.hasCalledAbort = true;
                        base.Controller.Abort(reason);
                    }
                }
                finally
                {
                    this.ForceNotifyOperationComplete();
                }
                if (flag)
                {
                    this.TrackAndRaiseAborted(reason);
                }
            }
        }

        protected internal override IAsyncResult OnBeginAssociateKeys(ICollection<InstanceKey> keys, AsyncCallback callback, object state)
        {
            throw Fx.AssertAndThrow("WorkflowApplication is sealed with CanUseKeys as false, so WorkflowInstance should not call OnBeginAssociateKeys.");
        }

        protected internal override IAsyncResult OnBeginPersist(AsyncCallback callback, object state)
        {
            return this.BeginInternalPersist(PersistenceOperation.Save, ActivityDefaults.InternalSaveTimeout, true, callback, state);
        }

        protected internal override IAsyncResult OnBeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfHandlerThread();
            return new ResumeBookmarkAsyncResult(this, bookmark, value, true, timeout, callback, state);
        }

        protected internal override void OnDisassociateKeys(ICollection<InstanceKey> keys)
        {
            throw Fx.AssertAndThrow("WorkflowApplication is sealed with CanUseKeys as false, so WorkflowInstance should not call OnDisassociateKeys.");
        }

        protected internal override void OnEndAssociateKeys(IAsyncResult result)
        {
            throw Fx.AssertAndThrow("WorkflowApplication is sealed with CanUseKeys as false, so WorkflowInstance should not call OnEndAssociateKeys.");
        }

        protected internal override void OnEndPersist(IAsyncResult result)
        {
            this.EndInternalPersist(result);
        }

        protected internal override BookmarkResumptionResult OnEndResumeBookmark(IAsyncResult result)
        {
            return ResumeBookmarkAsyncResult.End(result);
        }

        protected override void OnNotifyPaused()
        {
            WorkflowInstanceState state = base.Controller.State;
            WorkflowApplicationState state2 = this.state;
            bool flag = true;
            while (flag)
            {
                if (this.ShouldRaiseComplete(state))
                {
                    Exception reason = null;
                    try
                    {
                        this.hasRaisedCompleted = true;
                        if (completedHandler == null)
                        {
                            completedHandler = new CompletedEventHandler();
                        }
                        flag = completedHandler.Run(this);
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
                else
                {
                    bool flag2;
                    bool flag3;
                    InstanceOperation operation = null;
                    lock (this.pendingOperations)
                    {
                        operation = this.FindOperation();
                        flag2 = (state == WorkflowInstanceState.Runnable) && (state2 == WorkflowApplicationState.Runnable);
                        flag3 = (this.hasExecutionOccurredSinceLastIdle && (state == WorkflowInstanceState.Idle)) && !this.hasRaisedCompleted;
                        if (((operation == null) && !flag2) && !flag3)
                        {
                            this.isBusy = false;
                            flag = false;
                        }
                    }
                    if (operation != null)
                    {
                        operation.NotifyTurn();
                        flag = false;
                        continue;
                    }
                    if (flag3)
                    {
                        this.hasExecutionOccurredSinceLastIdle = false;
                        Exception exception3 = null;
                        try
                        {
                            if (idleHandler == null)
                            {
                                idleHandler = new IdleEventHandler();
                            }
                            flag = idleHandler.Run(this);
                        }
                        catch (Exception exception4)
                        {
                            if (Fx.IsFatal(exception4))
                            {
                                throw;
                            }
                            exception3 = exception4;
                        }
                        if (exception3 != null)
                        {
                            this.AbortInstance(exception3, true);
                        }
                        continue;
                    }
                    if (flag2)
                    {
                        this.hasExecutionOccurredSinceLastIdle = true;
                        this.actionCount++;
                        base.Controller.Run();
                        flag = false;
                    }
                }
            }
        }

        protected override void OnNotifyUnhandledException(Exception exception, Activity exceptionSource, string exceptionSourceInstanceId)
        {
            bool flag = true;
            try
            {
                Exception reason = null;
                try
                {
                    if (unhandledExceptionHandler == null)
                    {
                        unhandledExceptionHandler = new UnhandledExceptionEventHandler();
                    }
                    flag = unhandledExceptionHandler.Run(this, exception, exceptionSource, exceptionSourceInstanceId);
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    reason = exception3;
                }
                if (reason != null)
                {
                    this.AbortInstance(reason, true);
                }
            }
            finally
            {
                if (flag)
                {
                    this.OnNotifyPaused();
                }
            }
        }

        protected internal override void OnRequestAbort(Exception reason)
        {
            this.AbortInstance(reason, false);
        }

        private static void OnWaitAsyncComplete(object state, TimeoutException exception)
        {
            WaitForTurnData data = (WaitForTurnData) state;
            if (!data.Instance.Remove(data.Operation))
            {
                exception = null;
            }
            data.Callback(data.State, exception);
        }

        public void Persist()
        {
            this.Persist(ActivityDefaults.SaveTimeout);
        }

        public void Persist(TimeSpan timeout)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            RequiresPersistenceOperation operation = new RequiresPersistenceOperation();
            try
            {
                this.WaitForTurn(operation, timeoutHelper.RemainingTime());
                this.ValidateStateForPersist();
                this.PersistCore(ref timeoutHelper, PersistenceOperation.Save);
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        }

        private void PersistCore(ref TimeoutHelper timeoutHelper, PersistenceOperation operation)
        {
            if (this.HasPersistenceProvider)
            {
                if (!this.persistenceManager.IsInitialized)
                {
                    this.persistenceManager.Initialize(timeoutHelper.RemainingTime());
                }
                if (!this.persistenceManager.IsLocked && (Transaction.Current != null))
                {
                    this.persistenceManager.EnsureReadyness(timeoutHelper.RemainingTime());
                }
                this.TrackPersistence(operation);
                base.Controller.FlushTrackingRecords(timeoutHelper.RemainingTime());
            }
            bool flag = false;
            WorkflowPersistenceContext context = null;
            TransactionScope scope = null;
            try
            {
                IDictionary<XName, InstanceValue> instance = null;
                PersistencePipeline pipeline = null;
                if (base.HasPersistenceModule)
                {
                    pipeline = new PersistencePipeline(base.GetExtensions<IPersistencePipelineModule>(), PersistenceManager.GenerateInitialData(this));
                    pipeline.Collect();
                    pipeline.Map();
                    instance = pipeline.Values;
                }
                if (this.HasPersistenceProvider)
                {
                    if (instance == null)
                    {
                        instance = PersistenceManager.GenerateInitialData(this);
                    }
                    if (context == null)
                    {
                        context = new WorkflowPersistenceContext((pipeline != null) && pipeline.IsSaveTransactionRequired, timeoutHelper.OriginalTimeout);
                        scope = Fx.CreateTransactionScope(context.PublicTransaction);
                    }
                    this.persistenceManager.Save(instance, operation, timeoutHelper.RemainingTime());
                }
                if (pipeline != null)
                {
                    if (context == null)
                    {
                        context = new WorkflowPersistenceContext(pipeline.IsSaveTransactionRequired, timeoutHelper.OriginalTimeout);
                        scope = Fx.CreateTransactionScope(context.PublicTransaction);
                    }
                    try
                    {
                        this.persistencePipelineInUse = pipeline;
                        Thread.MemoryBarrier();
                        if (this.state == WorkflowApplicationState.Aborted)
                        {
                            throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.DefaultAbortReason));
                        }
                        pipeline.EndSave(pipeline.BeginSave(timeoutHelper.RemainingTime(), null, null));
                    }
                    finally
                    {
                        this.persistencePipelineInUse = null;
                    }
                }
                flag = true;
            }
            finally
            {
                Fx.CompleteTransactionScope(ref scope);
                if (context != null)
                {
                    if (flag)
                    {
                        context.Complete();
                    }
                    else
                    {
                        context.Abort();
                    }
                }
                if (flag)
                {
                    if (operation != PersistenceOperation.Save)
                    {
                        this.state = WorkflowApplicationState.Paused;
                        if (TD.WorkflowApplicationUnloadedIsEnabled())
                        {
                            TD.WorkflowApplicationUnloaded(this.Id.ToString());
                        }
                    }
                    else if (TD.WorkflowApplicationPersistedIsEnabled())
                    {
                        TD.WorkflowApplicationPersisted(this.Id.ToString());
                    }
                    if ((operation == PersistenceOperation.Complete) || (operation == PersistenceOperation.Unload))
                    {
                        if (this.HasPersistenceProvider && this.persistenceManager.OwnerWasCreated)
                        {
                            this.persistenceManager.DeleteOwner(timeoutHelper.RemainingTime());
                        }
                        this.MarkUnloaded();
                    }
                }
            }
        }

        private PersistencePipeline ProcessInstanceValues(IDictionary<XName, InstanceValue> values, out object deserializedRuntimeState)
        {
            PersistencePipeline pipeline = null;
            InstanceValue value2;
            if (!values.TryGetValue(WorkflowNamespace.Workflow, out value2) || !(value2.Value is ActivityExecutor))
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceException(System.Activities.SR.WorkflowInstanceNotFoundInStore(this.persistenceManager.InstanceId)));
            }
            deserializedRuntimeState = value2.Value;
            if (base.HasPersistenceModule)
            {
                pipeline = new PersistencePipeline(base.GetExtensions<IPersistencePipelineModule>());
                pipeline.SetLoadedValues(values);
            }
            return pipeline;
        }

        private void RaiseAborted(Exception reason)
        {
            if (this.invokeCompletedCallback == null)
            {
                Action<WorkflowApplicationAbortedEventArgs> aborted = this.Aborted;
                if (aborted != null)
                {
                    try
                    {
                        this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        this.isInHandler = true;
                        aborted(new WorkflowApplicationAbortedEventArgs(this, reason));
                    }
                    finally
                    {
                        this.isInHandler = false;
                    }
                }
            }
            else
            {
                this.invokeCompletedCallback();
            }
            if (TD.WorkflowInstanceAbortedIsEnabled())
            {
                TD.WorkflowInstanceAborted(this.Id.ToString(), reason);
            }
        }

        private bool RaiseIdleEvent()
        {
            if (TD.WorkflowApplicationIdledIsEnabled())
            {
                TD.WorkflowApplicationIdled(this.Id.ToString());
            }
            Exception reason = null;
            try
            {
                Action<WorkflowApplicationIdleEventArgs> idle = this.Idle;
                if (idle != null)
                {
                    this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                    this.isInHandler = true;
                    idle(new WorkflowApplicationIdleEventArgs(this));
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
            finally
            {
                this.isInHandler = false;
            }
            if (reason != null)
            {
                this.AbortInstance(reason, true);
                return false;
            }
            return true;
        }

        private bool Remove(InstanceOperation operation)
        {
            lock (this.pendingOperations)
            {
                return this.pendingOperations.Remove(operation);
            }
        }

        public BookmarkResumptionResult ResumeBookmark(Bookmark bookmark, object value)
        {
            return this.ResumeBookmark(bookmark, value, ActivityDefaults.ResumeBookmarkTimeout);
        }

        public BookmarkResumptionResult ResumeBookmark(string bookmarkName, object value)
        {
            if (string.IsNullOrEmpty(bookmarkName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("bookmarkName");
            }
            return this.ResumeBookmark(new Bookmark(bookmarkName), value);
        }

        public BookmarkResumptionResult ResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout)
        {
            BookmarkResumptionResult result;
            InstanceOperation operation2;
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.ThrowIfHandlerThread();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.hasCalledRun)
            {
                this.InternalRun(helper.RemainingTime(), false);
            }
            InstanceOperation operation = new RequiresIdleOperation();
        Label_0030:
            operation2 = null;
            try
            {
                this.WaitForTurn(operation, helper.RemainingTime());
                if (this.AreBookmarksInvalid(out result))
                {
                    return result;
                }
                result = this.ResumeBookmarkCore(bookmark, value);
                switch (result)
                {
                    case BookmarkResumptionResult.Success:
                        base.Controller.FlushTrackingRecords(helper.RemainingTime());
                        goto Label_0088;

                    case BookmarkResumptionResult.NotReady:
                        operation2 = new DeferredRequiresIdleOperation();
                        goto Label_0088;
                }
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        Label_0088:
            operation = operation2;
            if (operation != null)
            {
                goto Label_0030;
            }
            return result;
        }

        public BookmarkResumptionResult ResumeBookmark(string bookmarkName, object value, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(bookmarkName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("bookmarkName");
            }
            return this.ResumeBookmark(new Bookmark(bookmarkName), value, timeout);
        }

        private BookmarkResumptionResult ResumeBookmarkCore(Bookmark bookmark, object value)
        {
            BookmarkResumptionResult result = base.Controller.ScheduleBookmarkResumption(bookmark, value);
            if (result == BookmarkResumptionResult.Success)
            {
                this.RunCore();
            }
            return result;
        }

        public void Run()
        {
            this.Run(ActivityDefaults.AcquireLockTimeout);
        }

        public void Run(TimeSpan timeout)
        {
            this.InternalRun(timeout, true);
        }

        private void RunCore()
        {
            if (!this.hasCalledRun)
            {
                this.hasCalledRun = true;
            }
            this.state = WorkflowApplicationState.Runnable;
        }

        private static void RunInstance(System.Activities.WorkflowApplication instance)
        {
            instance.EnsureInitialized();
            instance.RunCore();
            instance.hasExecutionOccurredSinceLastIdle = true;
            instance.Controller.Run();
        }

        private void ScheduleTrackAndRaiseAborted(Exception reason)
        {
            if (base.Controller.HasPendingTrackingRecords || (this.Aborted != null))
            {
                ActionItem.Schedule(new Action<object>(this.TrackAndRaiseAborted), reason);
            }
        }

        private bool ShouldRaiseComplete(WorkflowInstanceState state)
        {
            return ((state == WorkflowInstanceState.Complete) && !this.hasRaisedCompleted);
        }

        private static System.Activities.WorkflowApplication StartInvoke(Activity activity, IDictionary<string, object> inputs, WorkflowInstanceExtensionManager extensions, SynchronizationContext syncContext, Action invokeCompletedCallback, AsyncInvokeContext invokeContext)
        {
            System.Activities.WorkflowApplication instance = CreateInstance(activity, inputs, extensions, syncContext, invokeCompletedCallback);
            if (invokeContext != null)
            {
                invokeContext.WorkflowApplication = instance;
            }
            RunInstance(instance);
            return instance;
        }

        public void Terminate(Exception reason)
        {
            this.Terminate(reason, ActivityDefaults.AcquireLockTimeout);
        }

        public void Terminate(string reason)
        {
            this.Terminate(reason, ActivityDefaults.AcquireLockTimeout);
        }

        public void Terminate(Exception reason, TimeSpan timeout)
        {
            if (reason == null)
            {
                throw FxTrace.Exception.ArgumentNull("reason");
            }
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            InstanceOperation operation = null;
            try
            {
                operation = new InstanceOperation();
                this.WaitForTurn(operation, helper.RemainingTime());
                this.ValidateStateForTerminate();
                this.TerminateCore(reason);
                base.Controller.FlushTrackingRecords(helper.RemainingTime());
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        }

        public void Terminate(string reason, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("reason");
            }
            this.Terminate(new WorkflowApplicationTerminatedException(reason, this.Id), timeout);
        }

        private void TerminateCore(Exception reason)
        {
            base.Controller.Terminate(reason);
        }

        private void ThrowIfAborted()
        {
            if (this.state == WorkflowApplicationState.Aborted)
            {
                throw FxTrace.Exception.AsError(new WorkflowApplicationAbortedException(System.Activities.SR.WorkflowApplicationAborted(this.Id), this.Id));
            }
        }

        private void ThrowIfHandlerThread()
        {
            if (this.IsHandlerThread)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotPerformOperationFromHandlerThread));
            }
        }

        private void ThrowIfMulticast(Delegate value)
        {
            if ((value != null) && (value.GetInvocationList().Length > 1))
            {
                throw FxTrace.Exception.Argument("value", System.Activities.SR.OnlySingleCastDelegatesAllowed);
            }
        }

        private void ThrowIfNoInstanceStore()
        {
            if (!this.HasPersistenceProvider)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InstanceStoreRequiredToPersist));
            }
        }

        private void ThrowIfTerminatedOrCompleted()
        {
            if (this.hasRaisedCompleted)
            {
                Exception exception;
                base.Controller.GetCompletionState(out exception);
                if (exception != null)
                {
                    throw FxTrace.Exception.AsError(new WorkflowApplicationTerminatedException(System.Activities.SR.WorkflowApplicationTerminated(this.Id), this.Id, exception));
                }
                throw FxTrace.Exception.AsError(new WorkflowApplicationCompletedException(System.Activities.SR.WorkflowApplicationCompleted(this.Id), this.Id));
            }
        }

        private void ThrowIfUnloaded()
        {
            if (this.state == WorkflowApplicationState.Unloaded)
            {
                throw FxTrace.Exception.AsError(new WorkflowApplicationUnloadedException(System.Activities.SR.WorkflowApplicationUnloaded(this.Id), this.Id));
            }
        }

        private void TrackAndRaiseAborted(object state)
        {
            Exception exception = (Exception) state;
            if (base.Controller.HasPendingTrackingRecords)
            {
                try
                {
                    IAsyncResult result = base.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, Fx.ThunkCallback(new AsyncCallback(this.OnAbortTrackingComplete)), exception);
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
                }
            }
            this.RaiseAborted(exception);
        }

        private void TrackPersistence(PersistenceOperation operation)
        {
            if (base.Controller.TrackingEnabled)
            {
                if (operation == PersistenceOperation.Complete)
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

        public void Unload()
        {
            this.Unload(ActivityDefaults.SaveTimeout);
        }

        public void Unload(TimeSpan timeout)
        {
            this.ThrowIfHandlerThread();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            RequiresPersistenceOperation operation = new RequiresPersistenceOperation();
            try
            {
                this.WaitForTurn(operation, timeoutHelper.RemainingTime());
                this.ValidateStateForUnload();
                if (this.state != WorkflowApplicationState.Unloaded)
                {
                    PersistenceOperation complete;
                    if (base.Controller.State == WorkflowInstanceState.Complete)
                    {
                        complete = PersistenceOperation.Complete;
                    }
                    else
                    {
                        complete = PersistenceOperation.Unload;
                    }
                    this.PersistCore(ref timeoutHelper, complete);
                }
            }
            finally
            {
                this.NotifyOperationComplete(operation);
            }
        }

        private void ValidateStateForCancel()
        {
            this.ThrowIfAborted();
        }

        private void ValidateStateForGetAllBookmarks()
        {
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
        }

        private void ValidateStateForPersist()
        {
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
            this.ThrowIfNoInstanceStore();
        }

        private void ValidateStateForRun()
        {
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
        }

        private void ValidateStateForTerminate()
        {
            this.ThrowIfAborted();
            this.ThrowIfTerminatedOrCompleted();
            this.ThrowIfUnloaded();
        }

        private void ValidateStateForUnload()
        {
            this.ThrowIfAborted();
            if (base.Controller.State != WorkflowInstanceState.Complete)
            {
                this.ThrowIfNoInstanceStore();
            }
        }

        private bool WaitForTurn(InstanceOperation operation, TimeSpan timeout)
        {
            this.Enqueue(operation);
            if (!operation.WaitForTurn(timeout) && this.Remove(operation))
            {
                throw FxTrace.Exception.AsError(new TimeoutException(System.Activities.SR.TimeoutOnOperation(timeout)));
            }
            return true;
        }

        private bool WaitForTurnAsync(InstanceOperation operation, TimeSpan timeout, Action<object, TimeoutException> callback, object state)
        {
            return this.WaitForTurnAsync(operation, false, timeout, callback, state);
        }

        private bool WaitForTurnAsync(InstanceOperation operation, bool push, TimeSpan timeout, Action<object, TimeoutException> callback, object state)
        {
            this.Enqueue(operation, push);
            if (waitAsyncCompleteCallback == null)
            {
                waitAsyncCompleteCallback = new Action<object, TimeoutException>(System.Activities.WorkflowApplication.OnWaitAsyncComplete);
            }
            return operation.WaitForTurnAsync(timeout, waitAsyncCompleteCallback, new WaitForTurnData(callback, state, operation, this));
        }

        public Action<WorkflowApplicationAbortedEventArgs> Aborted
        {
            get
            {
                return this.onAborted;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onAborted = value;
            }
        }

        public Action<WorkflowApplicationCompletedEventArgs> Completed
        {
            get
            {
                return this.onCompleted;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onCompleted = value;
            }
        }

        private WorkflowEventData EventData
        {
            get
            {
                if (this.eventData == null)
                {
                    this.eventData = new WorkflowEventData(this);
                }
                return this.eventData;
            }
        }

        private static AsyncCallback EventFrameCallback
        {
            get
            {
                if (eventFrameCallback == null)
                {
                    eventFrameCallback = Fx.ThunkCallback(new AsyncCallback(System.Activities.WorkflowApplication.EventFrame));
                }
                return eventFrameCallback;
            }
        }

        public WorkflowInstanceExtensionManager Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new WorkflowInstanceExtensionManager();
                    if (base.IsReadOnly)
                    {
                        this.extensions.MakeReadOnly();
                    }
                }
                return this.extensions;
            }
        }

        private bool HasPersistenceProvider
        {
            get
            {
                return (this.persistenceManager != null);
            }
        }

        public override Guid Id
        {
            get
            {
                if (!this.instanceIdSet)
                {
                    lock (this.pendingOperations)
                    {
                        if (!this.instanceIdSet)
                        {
                            this.instanceId = Guid.NewGuid();
                            this.instanceIdSet = true;
                        }
                    }
                }
                return this.instanceId;
            }
        }

        public Action<WorkflowApplicationIdleEventArgs> Idle
        {
            get
            {
                return this.onIdle;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onIdle = value;
            }
        }

        public System.Runtime.DurableInstancing.InstanceStore InstanceStore
        {
            get
            {
                return this.instanceStore;
            }
            set
            {
                base.ThrowIfReadOnly();
                this.instanceStore = value;
            }
        }

        private bool IsHandlerThread
        {
            get
            {
                return (this.isInHandler && (this.handlerThreadId == Thread.CurrentThread.ManagedThreadId));
            }
        }

        private bool IsInTerminalState
        {
            get
            {
                if (this.state != WorkflowApplicationState.Unloaded)
                {
                    return (this.state == WorkflowApplicationState.Aborted);
                }
                return true;
            }
        }

        public Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> OnUnhandledException
        {
            get
            {
                return this.onUnhandledException;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onUnhandledException = value;
            }
        }

        public Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction> PersistableIdle
        {
            get
            {
                return this.onPersistableIdle;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onPersistableIdle = value;
            }
        }

        protected internal override bool SupportsInstanceKeys
        {
            get
            {
                return false;
            }
        }

        public Action<WorkflowApplicationEventArgs> Unloaded
        {
            get
            {
                return this.onUnloaded;
            }
            set
            {
                this.ThrowIfMulticast(value);
                this.onUnloaded = value;
            }
        }

        private class CancelAsyncResult : System.Activities.WorkflowApplication.SimpleOperationAsyncResult
        {
            private CancelAsyncResult(System.Activities.WorkflowApplication instance, AsyncCallback callback, object state) : base(instance, callback, state)
            {
            }

            public static System.Activities.WorkflowApplication.CancelAsyncResult Create(System.Activities.WorkflowApplication instance, TimeSpan timeout, AsyncCallback callback, object state)
            {
                System.Activities.WorkflowApplication.CancelAsyncResult result = new System.Activities.WorkflowApplication.CancelAsyncResult(instance, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.Activities.WorkflowApplication.CancelAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                base.Instance.CancelCore();
            }

            protected override void ValidateState()
            {
                base.Instance.ValidateStateForCancel();
            }
        }

        private class CompletedEventHandler
        {
            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> stage1Callback;
            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> stage2Callback;

            private bool OnStage1Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication instance, bool isStillSync)
            {
                IDictionary<string, object> dictionary;
                Exception exception;
                if (lastResult != null)
                {
                    instance.Controller.EndFlushTrackingRecords(lastResult);
                }
                ActivityInstanceState completionState = instance.Controller.GetCompletionState(out dictionary, out exception);
                if (instance.invokeCompletedCallback == null)
                {
                    Action<WorkflowApplicationCompletedEventArgs> completed = instance.Completed;
                    if (completed != null)
                    {
                        instance.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        try
                        {
                            instance.isInHandler = true;
                            completed(new WorkflowApplicationCompletedEventArgs(instance, exception, completionState, dictionary));
                        }
                        finally
                        {
                            instance.isInHandler = false;
                        }
                    }
                }
                switch (completionState)
                {
                    case ActivityInstanceState.Closed:
                        if (TD.WorkflowApplicationCompletedIsEnabled())
                        {
                            TD.WorkflowApplicationCompleted(instance.Id.ToString());
                        }
                        break;

                    case ActivityInstanceState.Canceled:
                        if (TD.WorkflowInstanceCanceledIsEnabled())
                        {
                            TD.WorkflowInstanceCanceled(instance.Id.ToString());
                        }
                        break;

                    case ActivityInstanceState.Faulted:
                        if (TD.WorkflowApplicationTerminatedIsEnabled())
                        {
                            TD.WorkflowApplicationTerminated(instance.Id.ToString(), exception);
                        }
                        break;
                }
                IAsyncResult result = null;
                if ((instance.persistenceManager != null) || instance.HasPersistenceModule)
                {
                    instance.EventData.NextCallback = this.Stage2Callback;
                    result = instance.BeginInternalPersist(System.Activities.WorkflowApplication.PersistenceOperation.Unload, ActivityDefaults.InternalSaveTimeout, true, System.Activities.WorkflowApplication.EventFrameCallback, instance.EventData);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }
                else
                {
                    instance.MarkUnloaded();
                }
                return this.OnStage2Complete(result, instance, isStillSync);
            }

            private bool OnStage2Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication instance, bool isStillSync)
            {
                if (lastResult != null)
                {
                    instance.EndInternalPersist(lastResult);
                }
                if (instance.invokeCompletedCallback != null)
                {
                    instance.invokeCompletedCallback();
                }
                return true;
            }

            public bool Run(System.Activities.WorkflowApplication instance)
            {
                IAsyncResult lastResult = null;
                if (instance.Controller.HasPendingTrackingRecords)
                {
                    instance.EventData.NextCallback = this.Stage1Callback;
                    lastResult = instance.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, System.Activities.WorkflowApplication.EventFrameCallback, instance.EventData);
                    if (!lastResult.CompletedSynchronously)
                    {
                        return false;
                    }
                }
                return this.OnStage1Complete(lastResult, instance, true);
            }

            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> Stage1Callback
            {
                get
                {
                    if (this.stage1Callback == null)
                    {
                        this.stage1Callback = new Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool>(this.OnStage1Complete);
                    }
                    return this.stage1Callback;
                }
            }

            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> Stage2Callback
            {
                get
                {
                    if (this.stage2Callback == null)
                    {
                        this.stage2Callback = new Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool>(this.OnStage2Complete);
                    }
                    return this.stage2Callback;
                }
            }
        }

        private class DeferredRequiresIdleOperation : System.Activities.WorkflowApplication.InstanceOperation
        {
            public DeferredRequiresIdleOperation()
            {
                base.InterruptsScheduler = false;
            }

            public override bool CanRun(System.Activities.WorkflowApplication instance)
            {
                return (((base.ActionId != instance.actionCount) && (instance.Controller.State == WorkflowInstanceState.Idle)) || (instance.Controller.State == WorkflowInstanceState.Complete));
            }
        }

        private class IdleEventHandler
        {
            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> stage1Callback;
            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> stage2Callback;

            private bool OnStage1Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication application, bool isStillSync)
            {
                if (lastResult != null)
                {
                    application.Controller.EndFlushTrackingRecords(lastResult);
                }
                IAsyncResult result = null;
                if ((application.RaiseIdleEvent() && application.Controller.IsPersistable) && (application.persistenceManager != null))
                {
                    Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction> persistableIdle = application.PersistableIdle;
                    if (persistableIdle != null)
                    {
                        PersistableIdleAction none = PersistableIdleAction.None;
                        application.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        try
                        {
                            application.isInHandler = true;
                            none = persistableIdle(new WorkflowApplicationIdleEventArgs(application));
                        }
                        finally
                        {
                            application.isInHandler = false;
                        }
                        if (TD.WorkflowApplicationPersistableIdleIsEnabled())
                        {
                            TD.WorkflowApplicationPersistableIdle(application.Id.ToString(), none.ToString());
                        }
                        if (none != PersistableIdleAction.None)
                        {
                            System.Activities.WorkflowApplication.PersistenceOperation unload = System.Activities.WorkflowApplication.PersistenceOperation.Unload;
                            if (none == PersistableIdleAction.Persist)
                            {
                                unload = System.Activities.WorkflowApplication.PersistenceOperation.Save;
                            }
                            else if (none != PersistableIdleAction.Unload)
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidIdleAction));
                            }
                            application.EventData.NextCallback = this.Stage2Callback;
                            result = application.BeginInternalPersist(unload, ActivityDefaults.InternalSaveTimeout, true, System.Activities.WorkflowApplication.EventFrameCallback, application.EventData);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                        }
                    }
                    else if (TD.WorkflowApplicationPersistableIdleIsEnabled())
                    {
                        TD.WorkflowApplicationPersistableIdle(application.Id.ToString(), PersistableIdleAction.None.ToString());
                    }
                }
                return this.OnStage2Complete(result, application, isStillSync);
            }

            private bool OnStage2Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication instance, bool isStillSync)
            {
                if (lastResult != null)
                {
                    instance.EndInternalPersist(lastResult);
                }
                return true;
            }

            public bool Run(System.Activities.WorkflowApplication instance)
            {
                IAsyncResult lastResult = null;
                if (instance.Controller.TrackingEnabled)
                {
                    instance.Controller.Track(new WorkflowInstanceRecord(instance.Id, instance.WorkflowDefinition.DisplayName, "Idle"));
                    instance.EventData.NextCallback = this.Stage1Callback;
                    lastResult = instance.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, System.Activities.WorkflowApplication.EventFrameCallback, instance.EventData);
                    if (!lastResult.CompletedSynchronously)
                    {
                        return false;
                    }
                }
                return this.OnStage1Complete(lastResult, instance, true);
            }

            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> Stage1Callback
            {
                get
                {
                    if (this.stage1Callback == null)
                    {
                        this.stage1Callback = new Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool>(this.OnStage1Complete);
                    }
                    return this.stage1Callback;
                }
            }

            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> Stage2Callback
            {
                get
                {
                    if (this.stage2Callback == null)
                    {
                        this.stage2Callback = new Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool>(this.OnStage2Complete);
                    }
                    return this.stage2Callback;
                }
            }
        }

        private class InstanceOperation
        {
            private AsyncWaitHandle waitHandle;

            public InstanceOperation()
            {
                this.InterruptsScheduler = true;
            }

            public virtual bool CanRun(System.Activities.WorkflowApplication instance)
            {
                return true;
            }

            public void NotifyTurn()
            {
                this.waitHandle.Set();
            }

            public void OnEnqueued()
            {
                this.waitHandle = new AsyncWaitHandle();
            }

            public bool WaitForTurn(TimeSpan timeout)
            {
                if (this.waitHandle != null)
                {
                    return this.waitHandle.Wait(timeout);
                }
                return true;
            }

            public bool WaitForTurnAsync(TimeSpan timeout, Action<object, TimeoutException> callback, object state)
            {
                if (this.waitHandle != null)
                {
                    return this.waitHandle.WaitAsync(callback, state, timeout);
                }
                return true;
            }

            public int ActionId { get; set; }

            public bool InterruptsScheduler { get; protected set; }

            public bool Notified { get; set; }
        }

        private class InvokeAsyncResult : AsyncResult
        {
            private Exception completionException;
            private AsyncWaitHandle completionWaiter;
            private System.Activities.WorkflowApplication instance;
            private IDictionary<string, object> outputs;
            private static Action<object, TimeoutException> waitCompleteCallback;

            public InvokeAsyncResult(Activity activity, IDictionary<string, object> inputs, WorkflowInstanceExtensionManager extensions, TimeSpan timeout, SynchronizationContext syncContext, AsyncInvokeContext invokeContext, AsyncCallback callback, object state) : base(callback, state)
            {
                this.completionWaiter = new AsyncWaitHandle();
                syncContext = syncContext ?? System.Activities.WorkflowApplication.SynchronousSynchronizationContext.Value;
                this.instance = System.Activities.WorkflowApplication.StartInvoke(activity, inputs, extensions, syncContext, new Action(this.OnInvokeComplete), invokeContext);
                if (this.completionWaiter.WaitAsync(WaitCompleteCallback, this, timeout) && this.OnWorkflowCompletion())
                {
                    if (this.completionException != null)
                    {
                        throw FxTrace.Exception.AsError(this.completionException);
                    }
                    base.Complete(true);
                }
            }

            public static IDictionary<string, object> End(IAsyncResult result)
            {
                return AsyncResult.End<System.Activities.WorkflowApplication.InvokeAsyncResult>(result).outputs;
            }

            private void OnInvokeComplete()
            {
                this.completionWaiter.Set();
            }

            private static void OnWaitComplete(object state, TimeoutException asyncException)
            {
                System.Activities.WorkflowApplication.InvokeAsyncResult result = (System.Activities.WorkflowApplication.InvokeAsyncResult) state;
                if (asyncException != null)
                {
                    result.instance.Abort(System.Activities.SR.AbortingDueToInstanceTimeout);
                    result.Complete(false, asyncException);
                }
                else
                {
                    bool flag = true;
                    try
                    {
                        flag = result.OnWorkflowCompletion();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        result.completionException = exception;
                    }
                    if (flag)
                    {
                        result.Complete(false, result.completionException);
                    }
                }
            }

            private bool OnWorkflowCompletion()
            {
                if (this.instance.Controller.State == WorkflowInstanceState.Aborted)
                {
                    this.completionException = new WorkflowApplicationAbortedException(System.Activities.SR.DefaultAbortReason, this.instance.Controller.GetAbortReason());
                }
                else
                {
                    this.instance.Controller.GetCompletionState(out this.outputs, out this.completionException);
                }
                return true;
            }

            private static Action<object, TimeoutException> WaitCompleteCallback
            {
                get
                {
                    if (waitCompleteCallback == null)
                    {
                        waitCompleteCallback = new Action<object, TimeoutException>(System.Activities.WorkflowApplication.InvokeAsyncResult.OnWaitComplete);
                    }
                    return waitCompleteCallback;
                }
            }
        }

        private class LoadAsyncResult : AsyncResult
        {
            private readonly System.Activities.WorkflowApplication application;
            private static Action<AsyncResult, Exception> completeCallback = new Action<AsyncResult, Exception>(System.Activities.WorkflowApplication.LoadAsyncResult.OnComplete);
            private static AsyncResult.AsyncCompletion completeContextCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.LoadAsyncResult.OnCompleteContext);
            private WorkflowPersistenceContext context;
            private DependentTransaction dependentTransaction;
            private object deserializedRuntimeState;
            private readonly bool loadAny;
            private static AsyncResult.AsyncCompletion loadCompleteCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.LoadAsyncResult.OnLoadComplete);
            private static AsyncResult.AsyncCompletion loadPipelineCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.LoadAsyncResult.OnLoadPipeline);
            private PersistencePipeline pipeline;
            private static AsyncResult.AsyncCompletion providerRegisteredCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.LoadAsyncResult.OnProviderRegistered);
            private readonly TimeoutHelper timeoutHelper;

            public LoadAsyncResult(System.Activities.WorkflowApplication application, bool loadAny, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                bool flag;
                this.application = application;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.loadAny = loadAny;
                base.OnCompleting = completeCallback;
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    this.dependentTransaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
                bool flag2 = false;
                try
                {
                    flag = this.RegisterProvider();
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        if (this.dependentTransaction != null)
                        {
                            this.dependentTransaction.Complete();
                        }
                        this.application.Abort(System.Activities.SR.AbortingDueToLoadFailure);
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool CompleteContext()
            {
                IAsyncResult result;
                this.application.Initialize(this.deserializedRuntimeState);
                if (this.context.TryBeginComplete(base.PrepareAsyncCompletion(completeContextCallback), this, out result))
                {
                    return base.SyncContinue(result);
                }
                return this.Finish();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.Activities.WorkflowApplication.LoadAsyncResult>(result);
            }

            private bool Finish()
            {
                if (this.pipeline != null)
                {
                    this.pipeline.Publish();
                }
                return true;
            }

            private bool Load()
            {
                IAsyncResult result = null;
                try
                {
                    this.context = new WorkflowPersistenceContext(this.application.IsLoadTransactionRequired(), this.dependentTransaction, this.timeoutHelper.OriginalTimeout);
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        if (this.loadAny)
                        {
                            result = this.application.persistenceManager.BeginTryLoad(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(loadCompleteCallback), this);
                        }
                        else
                        {
                            result = this.application.persistenceManager.BeginLoad(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(loadCompleteCallback), this);
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
                System.Activities.WorkflowApplication.LoadAsyncResult result2 = (System.Activities.WorkflowApplication.LoadAsyncResult) result;
                if (result2.dependentTransaction != null)
                {
                    result2.dependentTransaction.Complete();
                }
                if (exception != null)
                {
                    result2.application.Abort(System.Activities.SR.AbortingDueToLoadFailure);
                }
            }

            private static bool OnCompleteContext(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.LoadAsyncResult asyncState = (System.Activities.WorkflowApplication.LoadAsyncResult) result.AsyncState;
                asyncState.context.EndComplete(result);
                return asyncState.Finish();
            }

            private static bool OnLoadComplete(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.LoadAsyncResult asyncState = (System.Activities.WorkflowApplication.LoadAsyncResult) result.AsyncState;
                IAsyncResult result3 = null;
                bool flag = false;
                try
                {
                    IDictionary<XName, InstanceValue> dictionary;
                    if (asyncState.loadAny)
                    {
                        if (!asyncState.application.persistenceManager.EndTryLoad(result, out dictionary))
                        {
                            throw FxTrace.Exception.AsError(new InstanceNotReadyException(System.Activities.SR.NoRunnableInstances));
                        }
                        if (asyncState.application.instanceIdSet)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowApplicationAlreadyHasId));
                        }
                        asyncState.application.instanceId = asyncState.application.persistenceManager.InstanceId;
                        asyncState.application.instanceIdSet = true;
                    }
                    else
                    {
                        dictionary = asyncState.application.persistenceManager.EndLoad(result);
                    }
                    asyncState.pipeline = asyncState.application.ProcessInstanceValues(dictionary, out asyncState.deserializedRuntimeState);
                    if (asyncState.pipeline != null)
                    {
                        asyncState.pipeline.SetLoadedValues(dictionary);
                        asyncState.application.persistencePipelineInUse = asyncState.pipeline;
                        Thread.MemoryBarrier();
                        if (asyncState.application.state == System.Activities.WorkflowApplication.WorkflowApplicationState.Aborted)
                        {
                            throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.DefaultAbortReason));
                        }
                        using (asyncState.PrepareTransactionalCall(asyncState.context.PublicTransaction))
                        {
                            result3 = asyncState.pipeline.BeginLoad(asyncState.timeoutHelper.RemainingTime(), asyncState.PrepareAsyncCompletion(loadPipelineCallback), asyncState);
                        }
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        asyncState.context.Abort();
                    }
                }
                if (asyncState.pipeline != null)
                {
                    return asyncState.SyncContinue(result3);
                }
                return asyncState.CompleteContext();
            }

            private static bool OnLoadPipeline(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.LoadAsyncResult asyncState = (System.Activities.WorkflowApplication.LoadAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.pipeline.EndLoad(result);
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

            private static bool OnProviderRegistered(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.LoadAsyncResult asyncState = (System.Activities.WorkflowApplication.LoadAsyncResult) result.AsyncState;
                asyncState.application.persistenceManager.EndInitialize(result);
                return asyncState.Load();
            }

            private bool RegisterProvider()
            {
                if (!this.application.persistenceManager.IsInitialized)
                {
                    IAsyncResult result = this.application.persistenceManager.BeginInitialize(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(providerRegisteredCallback), this);
                    return base.SyncContinue(result);
                }
                return this.Load();
            }
        }

        private class PersistenceManager
        {
            private bool aborted;
            private InstanceHandle handle;
            private Guid instanceId;
            private IDictionary<XName, InstanceValue> instanceMetadata;
            private bool isLocked;
            private bool isTryLoad;
            private InstanceOwner owner;
            private bool ownerWasCreated;
            private InstanceStore store;
            private InstanceHandle temporaryHandle;

            public PersistenceManager(InstanceStore store, IDictionary<XName, InstanceValue> instanceMetadata)
            {
                this.isTryLoad = true;
                this.instanceMetadata = instanceMetadata;
                this.InitializeInstanceMetadata();
                this.owner = store.DefaultInstanceOwner;
                if (this.owner != null)
                {
                    this.handle = store.CreateInstanceHandle(this.owner);
                }
                this.store = store;
            }

            public PersistenceManager(InstanceStore store, IDictionary<XName, InstanceValue> instanceMetadata, Guid instanceId)
            {
                this.instanceId = instanceId;
                this.instanceMetadata = instanceMetadata;
                this.InitializeInstanceMetadata();
                this.owner = store.DefaultInstanceOwner;
                if (this.owner != null)
                {
                    this.handle = store.CreateInstanceHandle(this.owner, instanceId);
                }
                this.store = store;
            }

            public void Abort()
            {
                this.aborted = true;
                Thread.MemoryBarrier();
                InstanceHandle handle = this.handle;
                if (handle != null)
                {
                    handle.Free();
                }
                this.FreeTemporaryHandle();
            }

            public IAsyncResult BeginDeleteOwner(TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result = null;
                try
                {
                    this.CreateTemporaryHandle(this.owner);
                    result = this.store.BeginExecute(this.temporaryHandle, new DeleteWorkflowOwnerCommand(), timeout, callback, state);
                }
                catch (InstancePersistenceCommandException)
                {
                }
                catch (InstanceOwnerException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    if (result == null)
                    {
                        this.FreeTemporaryHandle();
                    }
                }
                return result;
            }

            public IAsyncResult BeginEnsureReadyness(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return this.store.BeginExecute(this.handle, CreateSaveCommand(null, this.instanceMetadata, System.Activities.WorkflowApplication.PersistenceOperation.Save), timeout, callback, state);
                }
            }

            public IAsyncResult BeginInitialize(TimeSpan timeout, AsyncCallback callback, object state)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    IAsyncResult result = null;
                    try
                    {
                        this.CreateTemporaryHandle(null);
                        result = this.store.BeginExecute(this.temporaryHandle, new CreateWorkflowOwnerCommand(), timeout, callback, state);
                    }
                    finally
                    {
                        if (result == null)
                        {
                            this.FreeTemporaryHandle();
                        }
                    }
                    return result;
                }
            }

            public IAsyncResult BeginLoad(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.store.BeginExecute(this.handle, new LoadWorkflowCommand(), timeout, callback, state);
            }

            public IAsyncResult BeginSave(IDictionary<XName, InstanceValue> instance, System.Activities.WorkflowApplication.PersistenceOperation operation, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.store.BeginExecute(this.handle, CreateSaveCommand(instance, this.isLocked ? null : this.instanceMetadata, operation), timeout, callback, state);
            }

            public IAsyncResult BeginTryLoad(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.store.BeginExecute(this.handle, new TryLoadRunnableWorkflowCommand(), timeout, callback, state);
            }

            private static SaveWorkflowCommand CreateSaveCommand(IDictionary<XName, InstanceValue> instance, IDictionary<XName, InstanceValue> instanceMetadata, System.Activities.WorkflowApplication.PersistenceOperation operation)
            {
                SaveWorkflowCommand command = new SaveWorkflowCommand {
                    CompleteInstance = operation == System.Activities.WorkflowApplication.PersistenceOperation.Complete,
                    UnlockInstance = operation != System.Activities.WorkflowApplication.PersistenceOperation.Save
                };
                if (instance != null)
                {
                    foreach (KeyValuePair<XName, InstanceValue> pair in instance)
                    {
                        command.InstanceData.Add(pair);
                    }
                }
                if (instanceMetadata != null)
                {
                    foreach (KeyValuePair<XName, InstanceValue> pair2 in instanceMetadata)
                    {
                        command.InstanceMetadataChanges.Add(pair2);
                    }
                }
                return command;
            }

            private void CreateTemporaryHandle(InstanceOwner owner)
            {
                this.temporaryHandle = this.store.CreateInstanceHandle(owner);
                Thread.MemoryBarrier();
                if (this.aborted)
                {
                    this.FreeTemporaryHandle();
                }
            }

            public void DeleteOwner(TimeSpan timeout)
            {
                try
                {
                    this.CreateTemporaryHandle(this.owner);
                    this.store.Execute(this.temporaryHandle, new DeleteWorkflowOwnerCommand(), timeout);
                }
                catch (InstancePersistenceCommandException)
                {
                }
                catch (InstanceOwnerException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    this.FreeTemporaryHandle();
                }
            }

            public void EndDeleteOwner(IAsyncResult result)
            {
                try
                {
                    this.store.EndExecute(result);
                }
                catch (InstancePersistenceCommandException)
                {
                }
                catch (InstanceOwnerException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    this.FreeTemporaryHandle();
                }
            }

            public void EndEnsureReadyness(IAsyncResult result)
            {
                this.store.EndExecute(result);
                this.isLocked = true;
            }

            public void EndInitialize(IAsyncResult result)
            {
                try
                {
                    this.owner = this.store.EndExecute(result).InstanceOwner;
                    this.ownerWasCreated = true;
                }
                finally
                {
                    this.FreeTemporaryHandle();
                }
                this.handle = this.isTryLoad ? this.store.CreateInstanceHandle(this.owner) : this.store.CreateInstanceHandle(this.owner, this.InstanceId);
                Thread.MemoryBarrier();
                if (this.aborted)
                {
                    this.handle.Free();
                }
            }

            public IDictionary<XName, InstanceValue> EndLoad(IAsyncResult result)
            {
                InstanceView view = this.store.EndExecute(result);
                this.isLocked = true;
                if (!this.handle.IsValid)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.WorkflowInstanceAborted(this.InstanceId)));
                }
                return view.InstanceData;
            }

            public void EndSave(IAsyncResult result)
            {
                this.store.EndExecute(result);
                this.isLocked = true;
            }

            public bool EndTryLoad(IAsyncResult result, out IDictionary<XName, InstanceValue> data)
            {
                InstanceView view = this.store.EndExecute(result);
                return this.TryLoadHelper(view, out data);
            }

            public void EnsureReadyness(TimeSpan timeout)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    this.store.Execute(this.handle, CreateSaveCommand(null, this.instanceMetadata, System.Activities.WorkflowApplication.PersistenceOperation.Save), timeout);
                    this.isLocked = true;
                }
            }

            private void FreeTemporaryHandle()
            {
                InstanceHandle temporaryHandle = this.temporaryHandle;
                if (temporaryHandle != null)
                {
                    temporaryHandle.Free();
                }
            }

            public static Dictionary<XName, InstanceValue> GenerateInitialData(System.Activities.WorkflowApplication instance)
            {
                Exception exception;
                IDictionary<string, object> dictionary2;
                Dictionary<XName, InstanceValue> dictionary = new Dictionary<XName, InstanceValue>(10);
                dictionary[WorkflowNamespace.Bookmarks] = new InstanceValue(instance.Controller.GetBookmarks(), InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                dictionary[WorkflowNamespace.LastUpdate] = new InstanceValue(DateTime.UtcNow, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                foreach (KeyValuePair<string, LocationInfo> pair in instance.Controller.GetMappedVariables())
                {
                    XName name = WorkflowNamespace.VariablesPath.GetName(pair.Key);
                    dictionary[name] = new InstanceValue(pair.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                }
                Fx.AssertAndThrow(instance.Controller.State != WorkflowInstanceState.Aborted, "Cannot generate data for an aborted instance.");
                if (instance.Controller.State != WorkflowInstanceState.Complete)
                {
                    dictionary[WorkflowNamespace.Workflow] = new InstanceValue(instance.Controller.PrepareForSerialization());
                    dictionary[WorkflowNamespace.Status] = new InstanceValue((instance.Controller.State == WorkflowInstanceState.Idle) ? "Idle" : "Executing", InstanceValueOptions.WriteOnly);
                    return dictionary;
                }
                dictionary[WorkflowNamespace.Workflow] = new InstanceValue(instance.Controller.PrepareForSerialization(), InstanceValueOptions.Optional);
                ActivityInstanceState completionState = instance.Controller.GetCompletionState(out dictionary2, out exception);
                switch (completionState)
                {
                    case ActivityInstanceState.Faulted:
                        dictionary[WorkflowNamespace.Status] = new InstanceValue("Faulted", InstanceValueOptions.WriteOnly);
                        dictionary[WorkflowNamespace.Exception] = new InstanceValue(exception, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                        return dictionary;

                    case ActivityInstanceState.Closed:
                        dictionary[WorkflowNamespace.Status] = new InstanceValue("Closed", InstanceValueOptions.WriteOnly);
                        if (dictionary2 != null)
                        {
                            foreach (KeyValuePair<string, object> pair2 in dictionary2)
                            {
                                XName introduced17 = WorkflowNamespace.OutputPath.GetName(pair2.Key);
                                dictionary[introduced17] = new InstanceValue(pair2.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                            }
                        }
                        return dictionary;
                }
                Fx.AssertAndThrow(completionState == ActivityInstanceState.Canceled, "Cannot be executing when WorkflowState was completed.");
                dictionary[WorkflowNamespace.Status] = new InstanceValue("Canceled", InstanceValueOptions.WriteOnly);
                return dictionary;
            }

            public void Initialize(TimeSpan timeout)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    try
                    {
                        this.CreateTemporaryHandle(null);
                        this.owner = this.store.Execute(this.temporaryHandle, new CreateWorkflowOwnerCommand(), timeout).InstanceOwner;
                        this.ownerWasCreated = true;
                    }
                    finally
                    {
                        this.FreeTemporaryHandle();
                    }
                    this.handle = this.isTryLoad ? this.store.CreateInstanceHandle(this.owner) : this.store.CreateInstanceHandle(this.owner, this.InstanceId);
                    Thread.MemoryBarrier();
                    if (this.aborted)
                    {
                        this.handle.Free();
                    }
                }
            }

            private void InitializeInstanceMetadata()
            {
                if (this.instanceMetadata == null)
                {
                    this.instanceMetadata = new Dictionary<XName, InstanceValue>(1);
                }
                this.instanceMetadata[PersistenceMetadataNamespace.InstanceType] = new InstanceValue(WorkflowNamespace.WorkflowHostType, InstanceValueOptions.WriteOnly);
            }

            public IDictionary<XName, InstanceValue> Load(TimeSpan timeout)
            {
                InstanceView view = this.store.Execute(this.handle, new LoadWorkflowCommand(), timeout);
                this.isLocked = true;
                if (!this.handle.IsValid)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.WorkflowInstanceAborted(this.InstanceId)));
                }
                return view.InstanceData;
            }

            public void Save(IDictionary<XName, InstanceValue> instance, System.Activities.WorkflowApplication.PersistenceOperation operation, TimeSpan timeout)
            {
                this.store.Execute(this.handle, CreateSaveCommand(instance, this.isLocked ? null : this.instanceMetadata, operation), timeout);
                this.isLocked = true;
            }

            public bool TryLoad(TimeSpan timeout, out IDictionary<XName, InstanceValue> data)
            {
                InstanceView view = this.store.Execute(this.handle, new TryLoadRunnableWorkflowCommand(), timeout);
                return this.TryLoadHelper(view, out data);
            }

            private bool TryLoadHelper(InstanceView view, out IDictionary<XName, InstanceValue> data)
            {
                if (!view.IsBoundToLock)
                {
                    data = null;
                    return false;
                }
                this.instanceId = view.InstanceId;
                this.isLocked = true;
                if (!this.handle.IsValid)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.WorkflowInstanceAborted(this.InstanceId)));
                }
                data = view.InstanceData;
                return true;
            }

            public Guid InstanceId
            {
                get
                {
                    return this.instanceId;
                }
            }

            public bool IsInitialized
            {
                get
                {
                    return (this.handle != null);
                }
            }

            public bool IsLocked
            {
                get
                {
                    return this.isLocked;
                }
            }

            public bool OwnerWasCreated
            {
                get
                {
                    return this.ownerWasCreated;
                }
            }
        }

        private enum PersistenceOperation : byte
        {
            Complete = 0,
            Save = 1,
            Unload = 2
        }

        private class PumpBasedSynchronizationContext : SynchronizationContext
        {
            private WorkItem currentWorkItem;
            private AutoResetEvent queueWaiter;
            private object thisLock;
            private TimeoutHelper timeoutHelper;
            [ThreadStatic]
            private static AutoResetEvent waitObject;

            public PumpBasedSynchronizationContext(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.thisLock = new object();
            }

            public void DoPump()
            {
                WorkItem currentWorkItem;
                lock (this.thisLock)
                {
                    if (waitObject == null)
                    {
                        waitObject = new AutoResetEvent(false);
                    }
                    this.queueWaiter = waitObject;
                    currentWorkItem = this.currentWorkItem;
                    this.currentWorkItem = null;
                    currentWorkItem.Invoke();
                    goto Label_0062;
                }
            Label_004E:
                currentWorkItem = this.currentWorkItem;
                this.currentWorkItem = null;
                currentWorkItem.Invoke();
            Label_0062:
                if (this.WaitForNextItem())
                {
                    goto Label_004E;
                }
            }

            public void OnInvokeCompleted()
            {
                Fx.AssertAndFailFast(this.currentWorkItem == null, "There can be no pending work items when complete");
                this.IsInvokeCompleted = true;
                lock (this.thisLock)
                {
                    if (this.queueWaiter != null)
                    {
                        this.queueWaiter.Set();
                    }
                }
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                this.ScheduleWorkItem(new WorkItem(d, state));
            }

            private void ScheduleWorkItem(WorkItem item)
            {
                lock (this.thisLock)
                {
                    Fx.AssertAndFailFast(this.currentWorkItem == null, "There cannot be more than 1 work item at a given time");
                    this.currentWorkItem = item;
                    if (this.queueWaiter != null)
                    {
                        this.queueWaiter.Set();
                    }
                }
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw FxTrace.Exception.AsError(FxTrace.FailFast(System.Activities.SR.SendNotSupported));
            }

            private bool WaitForNextItem()
            {
                if (!this.WaitOne(this.queueWaiter, this.timeoutHelper.RemainingTime()))
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(System.Activities.SR.TimeoutOnOperation(this.timeoutHelper.OriginalTimeout)));
                }
                if (this.IsInvokeCompleted)
                {
                    return false;
                }
                return true;
            }

            private bool WaitOne(AutoResetEvent waiter, TimeSpan timeout)
            {
                bool flag3;
                bool flag = false;
                try
                {
                    bool flag2 = TimeoutHelper.WaitOne(waiter, timeout);
                    flag = flag2;
                    flag3 = flag2;
                }
                finally
                {
                    if (!flag)
                    {
                        waitObject = null;
                    }
                }
                return flag3;
            }

            private bool IsInvokeCompleted { get; set; }

            private class WorkItem
            {
                private SendOrPostCallback callback;
                private object state;

                public WorkItem(SendOrPostCallback callback, object state)
                {
                    this.callback = callback;
                    this.state = state;
                }

                public void Invoke()
                {
                    this.callback(this.state);
                }
            }
        }

        private class RequiresIdleOperation : System.Activities.WorkflowApplication.InstanceOperation
        {
            private bool requiresRunnableInstance;

            public RequiresIdleOperation() : this(false)
            {
            }

            public RequiresIdleOperation(bool requiresRunnableInstance)
            {
                base.InterruptsScheduler = false;
                this.requiresRunnableInstance = requiresRunnableInstance;
            }

            public override bool CanRun(System.Activities.WorkflowApplication instance)
            {
                if (this.requiresRunnableInstance && (instance.state != System.Activities.WorkflowApplication.WorkflowApplicationState.Runnable))
                {
                    return false;
                }
                if (instance.Controller.State != WorkflowInstanceState.Idle)
                {
                    return (instance.Controller.State == WorkflowInstanceState.Complete);
                }
                return true;
            }
        }

        private class RequiresPersistenceOperation : System.Activities.WorkflowApplication.InstanceOperation
        {
            public override bool CanRun(System.Activities.WorkflowApplication instance)
            {
                if (!instance.Controller.IsPersistable && (instance.Controller.State != WorkflowInstanceState.Complete))
                {
                    instance.Controller.PauseWhenPersistable();
                    return false;
                }
                return true;
            }
        }

        private class ResumeBookmarkAsyncResult : AsyncResult
        {
            private Bookmark bookmark;
            private System.Activities.WorkflowApplication.InstanceOperation currentOperation;
            private System.Activities.WorkflowApplication instance;
            private bool isFromExtension;
            private static AsyncResult.AsyncCompletion resumedCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult.OnResumed);
            private BookmarkResumptionResult resumptionResult;
            private TimeoutHelper timeoutHelper;
            private static AsyncResult.AsyncCompletion trackingCompleteCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult.OnTrackingComplete);
            private object value;
            private static Action<object, TimeoutException> waitCompleteCallback = new Action<object, TimeoutException>(System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult.OnWaitComplete);

            public ResumeBookmarkAsyncResult(System.Activities.WorkflowApplication instance, Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state) : this(instance, bookmark, value, false, timeout, callback, state)
            {
            }

            public ResumeBookmarkAsyncResult(System.Activities.WorkflowApplication instance, Bookmark bookmark, object value, bool isFromExtension, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
                this.bookmark = bookmark;
                this.value = value;
                this.isFromExtension = isFromExtension;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = false;
                bool flag2 = false;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.Finally);
                try
                {
                    if (!this.instance.hasCalledRun && !this.isFromExtension)
                    {
                        IAsyncResult result = this.instance.BeginInternalRun(this.timeoutHelper.RemainingTime(), false, base.PrepareAsyncCompletion(resumedCallback), this);
                        if (result.CompletedSynchronously)
                        {
                            flag = OnResumed(result);
                        }
                    }
                    else
                    {
                        flag = this.StartResumptionLoop();
                    }
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.instance.NotifyOperationComplete(this.currentOperation);
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool CheckIfBookmarksAreInvalid()
            {
                return this.instance.AreBookmarksInvalid(out this.resumptionResult);
            }

            public static BookmarkResumptionResult End(IAsyncResult result)
            {
                return AsyncResult.End<System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult>(result).resumptionResult;
            }

            private void Finally(AsyncResult result, Exception completionException)
            {
                this.instance.NotifyOperationComplete(this.currentOperation);
            }

            private static bool OnResumed(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult asyncState = (System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult) result.AsyncState;
                asyncState.instance.EndRun(result);
                return asyncState.StartResumptionLoop();
            }

            private static bool OnTrackingComplete(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult asyncState = (System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult) result.AsyncState;
                asyncState.instance.Controller.EndFlushTrackingRecords(result);
                return true;
            }

            private static void OnWaitComplete(object state, TimeoutException asyncException)
            {
                System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult result = (System.Activities.WorkflowApplication.ResumeBookmarkAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    Exception exception = null;
                    bool flag = false;
                    try
                    {
                        if (result.CheckIfBookmarksAreInvalid())
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = result.ProcessResumption();
                            if (result.resumptionResult == BookmarkResumptionResult.NotReady)
                            {
                                flag = result.WaitOnCurrentOperation();
                            }
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private bool ProcessResumption()
            {
                bool flag = true;
                this.resumptionResult = this.instance.ResumeBookmarkCore(this.bookmark, this.value);
                if (this.resumptionResult == BookmarkResumptionResult.Success)
                {
                    if (!this.instance.Controller.HasPendingTrackingRecords)
                    {
                        return flag;
                    }
                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(trackingCompleteCallback), this);
                    return (result.CompletedSynchronously && OnTrackingComplete(result));
                }
                if (this.resumptionResult == BookmarkResumptionResult.NotFound)
                {
                    System.Activities.WorkflowApplication.InstanceOperation currentOperation = this.currentOperation;
                    this.currentOperation = null;
                    this.instance.NotifyOperationComplete(currentOperation);
                    this.currentOperation = new System.Activities.WorkflowApplication.DeferredRequiresIdleOperation();
                }
                return flag;
            }

            private bool StartResumptionLoop()
            {
                this.currentOperation = new System.Activities.WorkflowApplication.RequiresIdleOperation(this.isFromExtension);
                return this.WaitOnCurrentOperation();
            }

            private bool WaitOnCurrentOperation()
            {
                bool flag = true;
                bool flag2 = true;
                while (flag2)
                {
                    flag2 = false;
                    if (this.instance.WaitForTurnAsync(this.currentOperation, this.timeoutHelper.RemainingTime(), waitCompleteCallback, this))
                    {
                        if (this.CheckIfBookmarksAreInvalid())
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = this.ProcessResumption();
                            flag2 = this.resumptionResult == BookmarkResumptionResult.NotReady;
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
                return flag;
            }
        }

        private class RunAsyncResult : System.Activities.WorkflowApplication.SimpleOperationAsyncResult
        {
            private bool isUserRun;

            private RunAsyncResult(System.Activities.WorkflowApplication instance, bool isUserRun, AsyncCallback callback, object state) : base(instance, callback, state)
            {
                this.isUserRun = isUserRun;
            }

            public static System.Activities.WorkflowApplication.RunAsyncResult Create(System.Activities.WorkflowApplication instance, bool isUserRun, TimeSpan timeout, AsyncCallback callback, object state)
            {
                System.Activities.WorkflowApplication.RunAsyncResult result = new System.Activities.WorkflowApplication.RunAsyncResult(instance, isUserRun, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.Activities.WorkflowApplication.RunAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                if (this.isUserRun)
                {
                    base.Instance.hasExecutionOccurredSinceLastIdle = true;
                }
                base.Instance.RunCore();
            }

            protected override void ValidateState()
            {
                base.Instance.ValidateStateForRun();
            }
        }

        private abstract class SimpleOperationAsyncResult : AsyncResult
        {
            private System.Activities.WorkflowApplication instance;
            private TimeoutHelper timeoutHelper;
            private static AsyncCallback trackingCompleteCallback = Fx.ThunkCallback(new AsyncCallback(System.Activities.WorkflowApplication.SimpleOperationAsyncResult.OnTrackingComplete));
            private static Action<object, TimeoutException> waitCompleteCallback = new Action<object, TimeoutException>(System.Activities.WorkflowApplication.SimpleOperationAsyncResult.OnWaitComplete);

            protected SimpleOperationAsyncResult(System.Activities.WorkflowApplication instance, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instance = instance;
            }

            private static void OnTrackingComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    System.Activities.WorkflowApplication.SimpleOperationAsyncResult asyncState = (System.Activities.WorkflowApplication.SimpleOperationAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.instance.Controller.EndFlushTrackingRecords(result);
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
                        asyncState.instance.ForceNotifyOperationComplete();
                    }
                    asyncState.Complete(false, exception);
                }
            }

            private static void OnWaitComplete(object state, TimeoutException asyncException)
            {
                System.Activities.WorkflowApplication.SimpleOperationAsyncResult result = (System.Activities.WorkflowApplication.SimpleOperationAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    Exception exception = null;
                    bool flag = true;
                    try
                    {
                        result.ValidateState();
                        flag = result.PerformOperationAndTrack();
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
                            result.instance.ForceNotifyOperationComplete();
                        }
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            protected abstract void PerformOperation();
            private bool PerformOperationAndTrack()
            {
                this.PerformOperation();
                bool flag = true;
                if (!this.instance.Controller.HasPendingTrackingRecords)
                {
                    return flag;
                }
                IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.timeoutHelper.RemainingTime(), trackingCompleteCallback, this);
                if (result.CompletedSynchronously)
                {
                    this.instance.Controller.EndFlushTrackingRecords(result);
                    return flag;
                }
                return false;
            }

            protected void Run(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                System.Activities.WorkflowApplication.InstanceOperation operation = new System.Activities.WorkflowApplication.InstanceOperation();
                bool flag = true;
                try
                {
                    flag = this.instance.WaitForTurnAsync(operation, this.timeoutHelper.RemainingTime(), waitCompleteCallback, this);
                    if (flag)
                    {
                        this.ValidateState();
                        flag = this.PerformOperationAndTrack();
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.instance.NotifyOperationComplete(operation);
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            protected abstract void ValidateState();

            protected System.Activities.WorkflowApplication Instance
            {
                get
                {
                    return this.instance;
                }
            }
        }

        internal class SynchronousSynchronizationContext : SynchronizationContext
        {
            private static System.Activities.WorkflowApplication.SynchronousSynchronizationContext value;

            private SynchronousSynchronizationContext()
            {
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                d(state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                d(state);
            }

            public static System.Activities.WorkflowApplication.SynchronousSynchronizationContext Value
            {
                get
                {
                    if (value == null)
                    {
                        value = new System.Activities.WorkflowApplication.SynchronousSynchronizationContext();
                    }
                    return value;
                }
            }
        }

        private class TerminateAsyncResult : System.Activities.WorkflowApplication.SimpleOperationAsyncResult
        {
            private Exception reason;

            private TerminateAsyncResult(System.Activities.WorkflowApplication instance, Exception reason, AsyncCallback callback, object state) : base(instance, callback, state)
            {
                this.reason = reason;
            }

            public static System.Activities.WorkflowApplication.TerminateAsyncResult Create(System.Activities.WorkflowApplication instance, Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
            {
                System.Activities.WorkflowApplication.TerminateAsyncResult result = new System.Activities.WorkflowApplication.TerminateAsyncResult(instance, reason, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.Activities.WorkflowApplication.TerminateAsyncResult>(result);
            }

            protected override void PerformOperation()
            {
                base.Instance.TerminateCore(this.reason);
            }

            protected override void ValidateState()
            {
                base.Instance.ValidateStateForTerminate();
            }
        }

        private class UnhandledExceptionEventHandler
        {
            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> stage1Callback;

            private bool OnStage1Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication instance, bool isStillSync)
            {
                return this.OnStage1Complete(lastResult, instance, instance.EventData.UnhandledException, instance.EventData.UnhandledExceptionSource, instance.EventData.UnhandledExceptionSourceInstance);
            }

            private bool OnStage1Complete(IAsyncResult lastResult, System.Activities.WorkflowApplication instance, Exception exception, Activity source, string sourceInstanceId)
            {
                if (lastResult != null)
                {
                    instance.Controller.EndFlushTrackingRecords(lastResult);
                }
                Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> onUnhandledException = instance.OnUnhandledException;
                UnhandledExceptionAction terminate = UnhandledExceptionAction.Terminate;
                if (onUnhandledException != null)
                {
                    try
                    {
                        instance.isInHandler = true;
                        instance.handlerThreadId = Thread.CurrentThread.ManagedThreadId;
                        terminate = onUnhandledException(new WorkflowApplicationUnhandledExceptionEventArgs(instance, exception, source, sourceInstanceId));
                    }
                    finally
                    {
                        instance.isInHandler = false;
                    }
                }
                if (instance.invokeCompletedCallback != null)
                {
                    terminate = UnhandledExceptionAction.Terminate;
                }
                if (TD.WorkflowApplicationUnhandledExceptionIsEnabled())
                {
                    TD.WorkflowApplicationUnhandledException(instance.Id.ToString(), source.GetType().ToString(), source.DisplayName, terminate.ToString(), exception);
                }
                switch (terminate)
                {
                    case UnhandledExceptionAction.Abort:
                        instance.AbortInstance(exception, true);
                        break;

                    case UnhandledExceptionAction.Cancel:
                        instance.Controller.ScheduleCancel();
                        break;

                    case UnhandledExceptionAction.Terminate:
                        instance.TerminateCore(exception);
                        break;

                    default:
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidUnhandledExceptionAction));
                }
                return true;
            }

            public bool Run(System.Activities.WorkflowApplication instance, Exception exception, Activity exceptionSource, string exceptionSourceInstanceId)
            {
                IAsyncResult lastResult = null;
                if (instance.Controller.HasPendingTrackingRecords)
                {
                    instance.EventData.NextCallback = this.Stage1Callback;
                    instance.EventData.UnhandledException = exception;
                    instance.EventData.UnhandledExceptionSource = exceptionSource;
                    instance.EventData.UnhandledExceptionSourceInstance = exceptionSourceInstanceId;
                    lastResult = instance.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, System.Activities.WorkflowApplication.EventFrameCallback, instance.EventData);
                    if (!lastResult.CompletedSynchronously)
                    {
                        return false;
                    }
                }
                return this.OnStage1Complete(lastResult, instance, exception, exceptionSource, exceptionSourceInstanceId);
            }

            private Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> Stage1Callback
            {
                get
                {
                    if (this.stage1Callback == null)
                    {
                        this.stage1Callback = new Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool>(this.OnStage1Complete);
                    }
                    return this.stage1Callback;
                }
            }
        }

        private class UnloadOrPersistAsyncResult : AsyncResult
        {
            private static Action<AsyncResult, Exception> completeCallback = new Action<AsyncResult, Exception>(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnComplete);
            private static AsyncResult.AsyncCompletion completeContextCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnCompleteContext);
            private WorkflowPersistenceContext context;
            private IDictionary<XName, InstanceValue> data;
            private static AsyncResult.AsyncCompletion deleteOwnerCompleteCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnOwnerDeleted);
            private DependentTransaction dependentTransaction;
            private static AsyncResult.AsyncCompletion initializedCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnProviderInitialized);
            private System.Activities.WorkflowApplication instance;
            private System.Activities.WorkflowApplication.RequiresPersistenceOperation instanceOperation;
            private bool isInternalPersist;
            private bool isUnloaded;
            private System.Activities.WorkflowApplication.PersistenceOperation operation;
            private static AsyncResult.AsyncCompletion persistedCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnPersisted);
            private PersistencePipeline pipeline;
            private static AsyncResult.AsyncCompletion readynessEnsuredCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnProviderReadynessEnsured);
            private static AsyncResult.AsyncCompletion savedCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnSaved);
            private TimeoutHelper timeoutHelper;
            private static AsyncResult.AsyncCompletion trackingCompleteCallback = new AsyncResult.AsyncCompletion(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnTrackingComplete);
            private static Action<object, TimeoutException> waitCompleteCallback = new Action<object, TimeoutException>(System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult.OnWaitComplete);

            public UnloadOrPersistAsyncResult(System.Activities.WorkflowApplication instance, TimeSpan timeout, System.Activities.WorkflowApplication.PersistenceOperation operation, bool isWorkflowThread, bool isInternalPersist, AsyncCallback callback, object state) : base(callback, state)
            {
                bool flag;
                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.operation = operation;
                this.isInternalPersist = isInternalPersist;
                this.isUnloaded = (operation == System.Activities.WorkflowApplication.PersistenceOperation.Unload) || (operation == System.Activities.WorkflowApplication.PersistenceOperation.Complete);
                base.OnCompleting = completeCallback;
                bool flag2 = false;
                Transaction current = Transaction.Current;
                if (current != null)
                {
                    this.dependentTransaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
                try
                {
                    if (isWorkflowThread)
                    {
                        flag = this.InitializeProvider();
                        flag2 = true;
                    }
                    else
                    {
                        this.instanceOperation = new System.Activities.WorkflowApplication.RequiresPersistenceOperation();
                        try
                        {
                            if (this.instance.WaitForTurnAsync(this.instanceOperation, this.timeoutHelper.RemainingTime(), waitCompleteCallback, this))
                            {
                                flag = this.ValidateState();
                            }
                            else
                            {
                                flag = false;
                            }
                            flag2 = true;
                        }
                        finally
                        {
                            if (!flag2)
                            {
                                this.NotifyOperationComplete();
                            }
                        }
                    }
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

            private bool CloseInstance()
            {
                if (this.operation != System.Activities.WorkflowApplication.PersistenceOperation.Save)
                {
                    this.instance.state = System.Activities.WorkflowApplication.WorkflowApplicationState.Paused;
                }
                if (this.isUnloaded)
                {
                    this.instance.MarkUnloaded();
                }
                return true;
            }

            private bool CollectAndMap()
            {
                bool flag = false;
                try
                {
                    if (this.instance.HasPersistenceModule)
                    {
                        IEnumerable<IPersistencePipelineModule> extensions = this.instance.GetExtensions<IPersistencePipelineModule>();
                        this.pipeline = new PersistencePipeline(extensions, System.Activities.WorkflowApplication.PersistenceManager.GenerateInitialData(this.instance));
                        this.pipeline.Collect();
                        this.pipeline.Map();
                        this.data = this.pipeline.Values;
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
                if (this.instance.HasPersistenceProvider)
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
                    flag = this.context.TryBeginComplete(base.PrepareAsyncCompletion(completeContextCallback), this, out result);
                }
                if (flag)
                {
                    return base.SyncContinue(result);
                }
                return this.DeleteOwner();
            }

            private bool DeleteOwner()
            {
                if ((!this.instance.HasPersistenceProvider || !this.instance.persistenceManager.OwnerWasCreated) || ((this.operation != System.Activities.WorkflowApplication.PersistenceOperation.Unload) && (this.operation != System.Activities.WorkflowApplication.PersistenceOperation.Complete)))
                {
                    return this.CloseInstance();
                }
                IAsyncResult result = null;
                using (base.PrepareTransactionalCall(this.dependentTransaction))
                {
                    result = this.instance.persistenceManager.BeginDeleteOwner(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(deleteOwnerCompleteCallback), this);
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult>(result);
            }

            private bool EnsureProviderReadyness()
            {
                if ((this.instance.HasPersistenceProvider && !this.instance.persistenceManager.IsLocked) && (this.dependentTransaction != null))
                {
                    IAsyncResult result = this.instance.persistenceManager.BeginEnsureReadyness(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(readynessEnsuredCallback), this);
                    return base.SyncContinue(result);
                }
                return this.Track();
            }

            private bool InitializeProvider()
            {
                if ((this.operation == System.Activities.WorkflowApplication.PersistenceOperation.Unload) && (this.instance.Controller.State == WorkflowInstanceState.Complete))
                {
                    this.operation = System.Activities.WorkflowApplication.PersistenceOperation.Complete;
                }
                if (this.instance.HasPersistenceProvider && !this.instance.persistenceManager.IsInitialized)
                {
                    IAsyncResult result = this.instance.persistenceManager.BeginInitialize(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(initializedCallback), this);
                    return base.SyncContinue(result);
                }
                return this.EnsureProviderReadyness();
            }

            private void NotifyOperationComplete()
            {
                System.Activities.WorkflowApplication.RequiresPersistenceOperation instanceOperation = this.instanceOperation;
                this.instanceOperation = null;
                this.instance.NotifyOperationComplete(instanceOperation);
            }

            private static void OnComplete(AsyncResult result, Exception exception)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult result2 = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result;
                try
                {
                    result2.NotifyOperationComplete();
                }
                finally
                {
                    if (result2.dependentTransaction != null)
                    {
                        result2.dependentTransaction.Complete();
                    }
                }
            }

            private static bool OnCompleteContext(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.context.EndComplete(result);
                return asyncState.DeleteOwner();
            }

            private static bool OnOwnerDeleted(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.persistenceManager.EndDeleteOwner(result);
                return asyncState.CloseInstance();
            }

            private static bool OnPersisted(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.instance.persistenceManager.EndSave(result);
                    flag = true;
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

            private static bool OnProviderInitialized(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.persistenceManager.EndInitialize(result);
                return asyncState.EnsureProviderReadyness();
            }

            private static bool OnProviderReadynessEnsured(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.persistenceManager.EndEnsureReadyness(result);
                return asyncState.Track();
            }

            private static bool OnSaved(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
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
                return asyncState.CompleteContext();
            }

            private static bool OnTrackingComplete(IAsyncResult result)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult asyncState = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) result.AsyncState;
                asyncState.instance.Controller.EndFlushTrackingRecords(result);
                return asyncState.CollectAndMap();
            }

            private static void OnWaitComplete(object state, TimeoutException asyncException)
            {
                System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult result = (System.Activities.WorkflowApplication.UnloadOrPersistAsyncResult) state;
                if (asyncException != null)
                {
                    result.Complete(false, asyncException);
                }
                else
                {
                    bool flag;
                    Exception exception = null;
                    try
                    {
                        flag = result.ValidateState();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                        flag = true;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception);
                    }
                }
            }

            private bool Persist()
            {
                IAsyncResult result = null;
                try
                {
                    if (this.data == null)
                    {
                        this.data = System.Activities.WorkflowApplication.PersistenceManager.GenerateInitialData(this.instance);
                    }
                    if (this.context == null)
                    {
                        this.context = new WorkflowPersistenceContext((this.pipeline != null) && this.pipeline.IsSaveTransactionRequired, this.dependentTransaction, this.timeoutHelper.OriginalTimeout);
                    }
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.instance.persistenceManager.BeginSave(this.data, this.operation, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(persistedCallback), this);
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

            private bool Save()
            {
                if (this.pipeline == null)
                {
                    return this.CompleteContext();
                }
                IAsyncResult result = null;
                try
                {
                    if (this.context == null)
                    {
                        this.context = new WorkflowPersistenceContext(this.pipeline.IsSaveTransactionRequired, this.dependentTransaction, this.timeoutHelper.RemainingTime());
                    }
                    this.instance.persistencePipelineInUse = this.pipeline;
                    Thread.MemoryBarrier();
                    if (this.instance.state == System.Activities.WorkflowApplication.WorkflowApplicationState.Aborted)
                    {
                        throw FxTrace.Exception.AsError(new OperationCanceledException(System.Activities.SR.DefaultAbortReason));
                    }
                    using (base.PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.pipeline.BeginSave(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(savedCallback), this);
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
                TimeSpan trackingTimeout;
                if (this.instance.HasPersistenceProvider)
                {
                    this.instance.TrackPersistence(this.operation);
                }
                if (!this.instance.Controller.HasPendingTrackingRecords)
                {
                    return this.CollectAndMap();
                }
                if (this.isInternalPersist)
                {
                    trackingTimeout = ActivityDefaults.TrackingTimeout;
                }
                else
                {
                    trackingTimeout = this.timeoutHelper.RemainingTime();
                }
                IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(trackingTimeout, base.PrepareAsyncCompletion(trackingCompleteCallback), this);
                return base.SyncContinue(result);
            }

            private bool ValidateState()
            {
                bool flag = false;
                if (this.operation == System.Activities.WorkflowApplication.PersistenceOperation.Unload)
                {
                    this.instance.ValidateStateForUnload();
                    flag = this.instance.state == System.Activities.WorkflowApplication.WorkflowApplicationState.Unloaded;
                }
                else
                {
                    this.instance.ValidateStateForPersist();
                }
                return (flag || this.InitializeProvider());
            }
        }

        private class WaitForTurnData
        {
            public WaitForTurnData(Action<object, TimeoutException> callback, object state, System.Activities.WorkflowApplication.InstanceOperation operation, System.Activities.WorkflowApplication instance)
            {
                this.Callback = callback;
                this.State = state;
                this.Operation = operation;
                this.Instance = instance;
            }

            public Action<object, TimeoutException> Callback { get; private set; }

            public System.Activities.WorkflowApplication Instance { get; private set; }

            public System.Activities.WorkflowApplication.InstanceOperation Operation { get; private set; }

            public object State { get; private set; }
        }

        private enum WorkflowApplicationState : byte
        {
            Aborted = 3,
            Paused = 0,
            Runnable = 1,
            Unloaded = 2
        }

        private class WorkflowEventData
        {
            public WorkflowEventData(System.Activities.WorkflowApplication instance)
            {
                this.Instance = instance;
            }

            public System.Activities.WorkflowApplication Instance { get; private set; }

            public Func<IAsyncResult, System.Activities.WorkflowApplication, bool, bool> NextCallback { get; set; }

            public Exception UnhandledException { get; set; }

            public Activity UnhandledExceptionSource { get; set; }

            public string UnhandledExceptionSourceInstance { get; set; }
        }
    }
}

