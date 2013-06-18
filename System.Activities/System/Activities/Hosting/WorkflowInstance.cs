namespace System.Activities.Hosting
{
    using System;
    using System.Activities;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class WorkflowInstance
    {
        private Exception abortedException;
        private WorkflowInstanceControl controller;
        private static readonly IDictionary<string, LocationInfo> EmptyMappedVariablesDictionary = new ReadOnlyDictionary<string, LocationInfo>(new Dictionary<string, LocationInfo>(0), false);
        private ActivityExecutor executor;
        private WorkflowInstanceExtensionCollection extensions;
        private const int False = 0;
        private bool hasTrackedCompletion;
        private bool hasTrackedResumed;
        private LocationReferenceEnvironment hostEnvironment;
        private bool isAborted;
        private bool isInitialized;
        private int isPerformingOperation;
        private System.Threading.SynchronizationContext syncContext;
        private System.Activities.Tracking.TrackingProvider trackingProvider;
        private const int True = 1;

        protected WorkflowInstance(Activity workflowDefinition)
        {
            if (workflowDefinition == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowDefinition");
            }
            this.WorkflowDefinition = workflowDefinition;
        }

        internal void Abort(Exception reason)
        {
            if (!this.isAborted)
            {
                this.isAborted = true;
                if (reason != null)
                {
                    this.abortedException = reason;
                }
                if (this.extensions != null)
                {
                    this.extensions.Cancel();
                }
                if (this.controller.TrackingEnabled && (reason != null))
                {
                    string message = reason.Message;
                    if (reason.InnerException != null)
                    {
                        message = System.Activities.SR.WorkflowAbortedReason(reason.Message, reason.InnerException.Message);
                    }
                    this.controller.Track(new WorkflowInstanceAbortedRecord(this.Id, this.WorkflowDefinition.DisplayName, message));
                }
            }
        }

        internal IAsyncResult BeginFlushTrackingRecords(AsyncCallback callback, object state)
        {
            return this.OnBeginFlushTrackingRecords(callback, state);
        }

        protected void DisposeExtensions()
        {
            if (this.extensions != null)
            {
                this.extensions.Dispose();
                this.extensions = null;
            }
        }

        internal void EndFlushTrackingRecords(IAsyncResult result)
        {
            this.OnEndFlushTrackingRecords(result);
        }

        private void FinishOperation(ref bool resetRequired)
        {
            if (resetRequired)
            {
                this.isPerformingOperation = 0;
            }
        }

        protected internal T GetExtension<T>() where T: class
        {
            if (this.extensions != null)
            {
                return this.extensions.Find<T>();
            }
            return default(T);
        }

        protected internal IEnumerable<T> GetExtensions<T>() where T: class
        {
            if (this.extensions != null)
            {
                return this.extensions.FindAll<T>();
            }
            return new T[0];
        }

        protected void Initialize(object deserializedRuntimeState)
        {
            this.ThrowIfAborted();
            this.ThrowIfReadOnly();
            this.executor = deserializedRuntimeState as ActivityExecutor;
            if (this.executor == null)
            {
                throw FxTrace.Exception.Argument("deserializedRuntimeState", System.Activities.SR.InvalidRuntimeState);
            }
            this.InitializeCore(null, null);
        }

        protected void Initialize(IDictionary<string, object> workflowArgumentValues, IList<Handle> workflowExecutionProperties)
        {
            this.ThrowIfAborted();
            this.ThrowIfReadOnly();
            this.executor = new ActivityExecutor(this);
            this.InitializeCore(workflowArgumentValues ?? ActivityUtilities.EmptyParameters, workflowExecutionProperties);
        }

        private void InitializeCore(IDictionary<string, object> workflowArgumentValues, IList<Handle> workflowExecutionProperties)
        {
            if (this.extensions != null)
            {
                this.extensions.Initialize();
                if (this.extensions.HasTrackingParticipant)
                {
                    this.HasTrackingParticipant = true;
                    this.trackingProvider = new System.Activities.Tracking.TrackingProvider(this.WorkflowDefinition);
                    foreach (TrackingParticipant participant in this.GetExtensions<TrackingParticipant>())
                    {
                        this.trackingProvider.AddParticipant(participant);
                    }
                }
            }
            else
            {
                this.ValidateWorkflow(null);
            }
            this.WorkflowDefinition.HasBeenAssociatedWithAnInstance = true;
            if (workflowArgumentValues != null)
            {
                IDictionary<string, object> objA = workflowArgumentValues;
                if (object.ReferenceEquals(objA, ActivityUtilities.EmptyParameters))
                {
                    objA = null;
                }
                if ((this.WorkflowDefinition.RuntimeArguments.Count > 0) || ((objA != null) && (objA.Count > 0)))
                {
                    ActivityValidationServices.ValidateRootInputs(this.WorkflowDefinition, objA);
                }
                this.executor.ScheduleRootActivity(this.WorkflowDefinition, objA, workflowExecutionProperties);
            }
            else
            {
                this.executor.OnDeserialized(this.WorkflowDefinition, this);
            }
            this.executor.Open(this.SynchronizationContext);
            this.controller = new WorkflowInstanceControl(this, this.executor);
            this.isInitialized = true;
            if ((this.extensions != null) && this.extensions.HasWorkflowInstanceExtensions)
            {
                WorkflowInstanceProxy instance = new WorkflowInstanceProxy(this);
                for (int i = 0; i < this.extensions.WorkflowInstanceExtensions.Count; i++)
                {
                    this.extensions.WorkflowInstanceExtensions[i].SetInstance(instance);
                }
            }
        }

        internal void NotifyPaused()
        {
            if (this.executor.State != ActivityInstanceState.Executing)
            {
                this.TrackCompletion();
            }
            this.OnNotifyPaused();
        }

        internal void NotifyUnhandledException(Exception exception, Activity source, string sourceInstanceId)
        {
            if (this.controller.TrackingEnabled)
            {
                ActivityInfo faultSource = new ActivityInfo(source.DisplayName, source.Id, sourceInstanceId, source.GetType().FullName);
                this.controller.Track(new WorkflowInstanceUnhandledExceptionRecord(this.Id, this.WorkflowDefinition.DisplayName, faultSource, exception));
            }
            this.OnNotifyUnhandledException(exception, source, sourceInstanceId);
        }

        protected internal abstract IAsyncResult OnBeginAssociateKeys(ICollection<InstanceKey> keys, AsyncCallback callback, object state);
        protected virtual IAsyncResult OnBeginFlushTrackingRecords(AsyncCallback callback, object state)
        {
            return this.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, callback, state);
        }

        protected internal abstract IAsyncResult OnBeginPersist(AsyncCallback callback, object state);
        protected internal abstract IAsyncResult OnBeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state);
        internal void OnDeserialized(bool hasTrackedStarted)
        {
            this.HasTrackedStarted = hasTrackedStarted;
        }

        protected internal abstract void OnDisassociateKeys(ICollection<InstanceKey> keys);
        protected internal abstract void OnEndAssociateKeys(IAsyncResult result);
        protected virtual void OnEndFlushTrackingRecords(IAsyncResult result)
        {
            this.Controller.EndFlushTrackingRecords(result);
        }

        protected internal abstract void OnEndPersist(IAsyncResult result);
        protected internal abstract BookmarkResumptionResult OnEndResumeBookmark(IAsyncResult result);
        protected abstract void OnNotifyPaused();
        protected abstract void OnNotifyUnhandledException(Exception exception, Activity source, string sourceInstanceId);
        protected internal abstract void OnRequestAbort(Exception reason);
        protected void RegisterExtensionManager(WorkflowInstanceExtensionManager extensionManager)
        {
            this.ValidateWorkflow(extensionManager);
            this.extensions = WorkflowInstanceExtensionManager.CreateInstanceExtensions(this.WorkflowDefinition, extensionManager);
            if (this.extensions != null)
            {
                this.HasPersistenceModule = this.extensions.HasPersistenceModule;
            }
        }

        private void Run()
        {
            this.ThrowIfAborted();
            this.TrackResumed();
            this.executor.MarkSchedulerRunning();
        }

        private BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value)
        {
            this.ValidateScheduleResumeBookmark();
            this.TrackResumed();
            return this.executor.TryResumeHostBookmark(bookmark, value);
        }

        private BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value, BookmarkScope scope)
        {
            this.ValidateScheduleResumeBookmark();
            this.TrackResumed();
            return this.executor.TryResumeBookmark(bookmark, value, scope);
        }

        private void ScheduleCancel()
        {
            this.ThrowIfAborted();
            this.TrackResumed();
            this.executor.CancelRootActivity();
        }

        private void StartOperation(ref bool resetRequired)
        {
            this.StartReadOnlyOperation(ref resetRequired);
            if (this.executor.IsRunning)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeRunning));
            }
        }

        private void StartReadOnlyOperation(ref bool resetRequired)
        {
            bool flag = false;
            try
            {
            }
            finally
            {
                flag = Interlocked.CompareExchange(ref this.isPerformingOperation, 1, 0) == 1;
                if (!flag)
                {
                    resetRequired = true;
                }
            }
            if (flag)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RuntimeOperationInProgress));
            }
        }

        private void Terminate(Exception reason)
        {
            this.ThrowIfAborted();
            this.executor.Terminate(reason);
            this.TrackCompletion();
        }

        private void ThrowIfAborted()
        {
            if (this.isAborted || ((this.executor != null) && this.executor.IsAbortPending))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowInstanceAborted(this.Id)));
            }
        }

        private void ThrowIfNotIdle()
        {
            if (!this.executor.IsIdle)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarksOnlyResumableWhileIdle));
            }
        }

        protected void ThrowIfReadOnly()
        {
            if (this.isInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WorkflowInstanceIsReadOnly(this.Id)));
            }
        }

        private void TrackCompletion()
        {
            if (this.controller.TrackingEnabled && !this.hasTrackedCompletion)
            {
                ActivityInstanceState state = this.executor.State;
                switch (state)
                {
                    case ActivityInstanceState.Faulted:
                        this.controller.Track(new WorkflowInstanceTerminatedRecord(this.Id, this.WorkflowDefinition.DisplayName, this.executor.TerminationException.Message));
                        break;

                    case ActivityInstanceState.Closed:
                        this.controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, "Completed"));
                        break;

                    default:
                        Fx.AssertAndThrow(state == ActivityInstanceState.Canceled, "Cannot be executing a workflow instance when WorkflowState was completed.");
                        this.controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, "Canceled"));
                        break;
                }
                this.hasTrackedCompletion = true;
            }
        }

        private void TrackResumed()
        {
            if (!this.hasTrackedResumed)
            {
                if (this.Controller.TrackingEnabled)
                {
                    if (!this.HasTrackedStarted)
                    {
                        this.TrackingProvider.AddRecord(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, "Started"));
                        this.HasTrackedStarted = true;
                    }
                    else
                    {
                        this.TrackingProvider.AddRecord(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, "Resumed"));
                    }
                }
                this.hasTrackedResumed = true;
            }
        }

        private void ValidateGetBookmarks()
        {
            this.ThrowIfAborted();
        }

        private void ValidateGetMappedVariables()
        {
            this.ThrowIfAborted();
        }

        private void ValidatePauseWhenPersistable()
        {
            this.ThrowIfAborted();
            if (this.Controller.IsPersistable)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.PauseWhenPersistableInvalidIfPersistable));
            }
        }

        private void ValidatePrepareForSerialization()
        {
            this.ThrowIfAborted();
            if (!this.Controller.IsPersistable)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.PrepareForSerializationRequiresPersistability));
            }
        }

        private void ValidateScheduleResumeBookmark()
        {
            this.ThrowIfAborted();
            this.ThrowIfNotIdle();
        }

        private void ValidateWorkflow(WorkflowInstanceExtensionManager extensionManager)
        {
            if (!this.WorkflowDefinition.IsRuntimeReady)
            {
                LocationReferenceEnvironment hostEnvironment = this.hostEnvironment;
                if (hostEnvironment == null)
                {
                    LocationReferenceEnvironment parent = null;
                    if ((extensionManager != null) && (extensionManager.SymbolResolver != null))
                    {
                        parent = extensionManager.SymbolResolver.AsLocationReferenceEnvironment();
                    }
                    hostEnvironment = new ActivityLocationReferenceEnvironment(parent);
                }
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(this.WorkflowDefinition, hostEnvironment, ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }
        }

        protected WorkflowInstanceControl Controller
        {
            get
            {
                if (!this.isInitialized)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ControllerInvalidBeforeInitialize));
                }
                return this.controller;
            }
        }

        internal bool HasPersistenceModule { get; private set; }

        internal bool HasTrackedStarted { get; private set; }

        internal bool HasTrackingParticipant { get; private set; }

        public LocationReferenceEnvironment HostEnvironment
        {
            get
            {
                return this.hostEnvironment;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.hostEnvironment = value;
            }
        }

        public abstract Guid Id { get; }

        protected bool IsReadOnly
        {
            get
            {
                return this.isInitialized;
            }
        }

        protected internal abstract bool SupportsInstanceKeys { get; }

        public System.Threading.SynchronizationContext SynchronizationContext
        {
            get
            {
                return this.syncContext;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.syncContext = value;
            }
        }

        internal System.Activities.Tracking.TrackingProvider TrackingProvider
        {
            get
            {
                return this.trackingProvider;
            }
        }

        public Activity WorkflowDefinition { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        protected struct WorkflowInstanceControl
        {
            private ActivityExecutor executor;
            private WorkflowInstance instance;
            internal WorkflowInstanceControl(WorkflowInstance instance, ActivityExecutor executor)
            {
                this.instance = instance;
                this.executor = executor;
            }

            public bool IsPersistable
            {
                get
                {
                    return this.executor.IsPersistable;
                }
            }
            public bool HasPendingTrackingRecords
            {
                get
                {
                    return (this.instance.HasTrackingParticipant && this.instance.TrackingProvider.HasPendingRecords);
                }
            }
            public bool TrackingEnabled
            {
                get
                {
                    return (this.instance.HasTrackingParticipant && this.instance.TrackingProvider.ShouldTrackWorkflowInstanceRecords);
                }
            }
            public WorkflowInstanceState State
            {
                get
                {
                    if (this.instance.isAborted)
                    {
                        return WorkflowInstanceState.Aborted;
                    }
                    if (!this.executor.IsIdle)
                    {
                        return WorkflowInstanceState.Runnable;
                    }
                    if (this.executor.State == ActivityInstanceState.Executing)
                    {
                        return WorkflowInstanceState.Idle;
                    }
                    return WorkflowInstanceState.Complete;
                }
            }
            public override bool Equals(object obj)
            {
                if (!(obj is WorkflowInstance.WorkflowInstanceControl))
                {
                    return false;
                }
                WorkflowInstance.WorkflowInstanceControl control = (WorkflowInstance.WorkflowInstanceControl) obj;
                return (control.instance == this.instance);
            }

            public override int GetHashCode()
            {
                return this.instance.GetHashCode();
            }

            public static bool operator ==(WorkflowInstance.WorkflowInstanceControl left, WorkflowInstance.WorkflowInstanceControl right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(WorkflowInstance.WorkflowInstanceControl left, WorkflowInstance.WorkflowInstanceControl right)
            {
                return !left.Equals(right);
            }

            public ReadOnlyCollection<BookmarkInfo> GetBookmarks()
            {
                ReadOnlyCollection<BookmarkInfo> allBookmarks;
                bool resetRequired = false;
                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);
                    this.instance.ValidateGetBookmarks();
                    allBookmarks = this.executor.GetAllBookmarks();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return allBookmarks;
            }

            public ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
            {
                ReadOnlyCollection<BookmarkInfo> bookmarks;
                bool resetRequired = false;
                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);
                    this.instance.ValidateGetBookmarks();
                    bookmarks = this.executor.GetBookmarks(scope);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return bookmarks;
            }

            public IDictionary<string, LocationInfo> GetMappedVariables()
            {
                IDictionary<string, LocationInfo> dictionary2;
                bool resetRequired = false;
                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);
                    this.instance.ValidateGetMappedVariables();
                    IDictionary<string, LocationInfo> emptyMappedVariablesDictionary = this.instance.executor.GatherMappableVariables();
                    if (emptyMappedVariablesDictionary != null)
                    {
                        emptyMappedVariablesDictionary = new ReadOnlyDictionary<string, LocationInfo>(emptyMappedVariablesDictionary, false);
                    }
                    else
                    {
                        emptyMappedVariablesDictionary = WorkflowInstance.EmptyMappedVariablesDictionary;
                    }
                    dictionary2 = emptyMappedVariablesDictionary;
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return dictionary2;
            }

            public void Run()
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.instance.Run();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                this.executor.Run();
            }

            public void RequestPause()
            {
                this.executor.PauseScheduler();
            }

            public void PauseWhenPersistable()
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.instance.ValidatePauseWhenPersistable();
                    this.executor.PauseWhenPersistable();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void ScheduleCancel()
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.instance.ScheduleCancel();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Terminate(Exception reason)
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.instance.Terminate(reason);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value)
            {
                BookmarkResumptionResult result;
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    result = this.instance.ScheduleBookmarkResumption(bookmark, value);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return result;
            }

            public BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value, BookmarkScope scope)
            {
                BookmarkResumptionResult result;
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    result = this.instance.ScheduleBookmarkResumption(bookmark, value, scope);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return result;
            }

            public void Abort()
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.executor.Dispose();
                    this.instance.Abort(null);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Abort(Exception reason)
            {
                bool resetRequired = false;
                try
                {
                    this.instance.StartOperation(ref resetRequired);
                    this.executor.Abort(reason);
                    this.instance.Abort(reason);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Track(WorkflowInstanceRecord instanceRecord)
            {
                if (this.instance.HasTrackingParticipant)
                {
                    this.instance.TrackingProvider.AddRecord(instanceRecord);
                }
            }

            public void FlushTrackingRecords(TimeSpan timeout)
            {
                if (this.instance.HasTrackingParticipant)
                {
                    this.instance.TrackingProvider.FlushPendingRecords(timeout);
                }
            }

            public IAsyncResult BeginFlushTrackingRecords(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.instance.HasTrackingParticipant)
                {
                    return this.instance.TrackingProvider.BeginFlushPendingRecords(timeout, callback, state);
                }
                return new CompletedAsyncResult(callback, state);
            }

            public void EndFlushTrackingRecords(IAsyncResult result)
            {
                if (this.instance.HasTrackingParticipant)
                {
                    this.instance.TrackingProvider.EndFlushPendingRecords(result);
                }
                else
                {
                    CompletedAsyncResult.End(result);
                }
            }

            public object PrepareForSerialization()
            {
                object obj2;
                bool resetRequired = false;
                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);
                    this.instance.ValidatePrepareForSerialization();
                    obj2 = this.executor.PrepareForSerialization();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
                return obj2;
            }

            public ActivityInstanceState GetCompletionState()
            {
                return this.executor.State;
            }

            public ActivityInstanceState GetCompletionState(out Exception terminationException)
            {
                terminationException = this.executor.TerminationException;
                return this.executor.State;
            }

            public ActivityInstanceState GetCompletionState(out IDictionary<string, object> outputs, out Exception terminationException)
            {
                outputs = this.executor.WorkflowOutputs;
                terminationException = this.executor.TerminationException;
                return this.executor.State;
            }

            public Exception GetAbortReason()
            {
                return this.instance.abortedException;
            }
        }
    }
}

