namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities;
    using System.Activities.DurableInstancing;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.Transactions;

    internal sealed class PersistenceProviderDirectory
    {
        private bool aborted;
        private Dictionary<Guid, PersistenceContext> instanceCache;
        private readonly Dictionary<Guid, PersistenceContext> keyMap;
        private readonly InstanceOwner owner;
        private HashSet<PersistencePipeline> pipelinesInUse;
        private readonly WorkflowServiceHost serviceHost;
        private readonly InstanceStore store;
        private readonly InstanceThrottle throttle;
        private readonly Activity workflowDefinition;

        internal PersistenceProviderDirectory(Activity workflowDefinition, WorkflowServiceHost serviceHost, int maxInstances) : this(workflowDefinition, serviceHost, DurableConsistencyScope.Local, maxInstances)
        {
        }

        private PersistenceProviderDirectory(Activity workflowDefinition, WorkflowServiceHost serviceHost, DurableConsistencyScope consistencyScope, int maxInstances)
        {
            Fx.AssertAndThrow(maxInstances > 0, "MaxInstance must be greater than zero on PPD.");
            this.workflowDefinition = workflowDefinition;
            this.serviceHost = serviceHost;
            this.ConsistencyScope = consistencyScope;
            this.MaxInstances = maxInstances;
            this.throttle = new InstanceThrottle(this.MaxInstances);
            this.pipelinesInUse = new HashSet<PersistencePipeline>();
            this.keyMap = new Dictionary<Guid, PersistenceContext>();
            this.instanceCache = new Dictionary<Guid, PersistenceContext>();
        }

        internal PersistenceProviderDirectory(InstanceStore store, InstanceOwner owner, IDictionary<XName, InstanceValue> instanceMetadataChanges, Activity workflowDefinition, WorkflowServiceHost serviceHost, DurableConsistencyScope consistencyScope, int maxInstances) : this(workflowDefinition, serviceHost, consistencyScope, maxInstances)
        {
            this.store = store;
            this.owner = owner;
            this.InstanceMetadataChanges = instanceMetadataChanges;
        }

        public void Abort()
        {
            List<PersistenceContext> contextsToAbort = null;
            HashSet<PersistencePipeline> pipelinesInUse = null;
            lock (this.ThisLock)
            {
                this.aborted = true;
                if (this.instanceCache != null)
                {
                    foreach (PersistenceContext context in this.instanceCache.Values.ToArray<PersistenceContext>())
                    {
                        this.DetachContext(context, ref contextsToAbort);
                    }
                    this.instanceCache = null;
                }
                if (this.pipelinesInUse != null)
                {
                    pipelinesInUse = this.pipelinesInUse;
                    this.pipelinesInUse = null;
                }
            }
            this.AbortContexts(contextsToAbort);
            if (pipelinesInUse != null)
            {
                foreach (PersistencePipeline pipeline in pipelinesInUse)
                {
                    pipeline.Abort();
                }
            }
            this.throttle.Abort();
        }

        private void AbortContexts(List<PersistenceContext> contextsToAbort)
        {
            if (contextsToAbort != null)
            {
                foreach (PersistenceContext context in contextsToAbort)
                {
                    context.Abort();
                }
            }
        }

        public IAsyncResult BeginLoad(InstanceKey key, ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (key == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("key");
            }
            if (key.Value == Guid.Empty)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.Argument("key", System.ServiceModel.Activities.SR.InvalidKey);
            }
            return new LoadOrCreateAsyncResult(this, key, Guid.Empty, false, associatedKeys, transaction, false, timeout, callback, state);
        }

        public IAsyncResult BeginLoad(Guid instanceId, ICollection<InstanceKey> associatedKeys, Transaction transaction, bool loadAny, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if ((instanceId == Guid.Empty) && !loadAny)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.Argument("instanceId", System.ServiceModel.Activities.SR.InvalidInstanceId);
            }
            return new LoadOrCreateAsyncResult(this, null, instanceId, false, associatedKeys, transaction, loadAny, timeout, callback, state);
        }

        public IAsyncResult BeginLoadOrCreate(Guid instanceId, ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new LoadOrCreateAsyncResult(this, null, instanceId, true, associatedKeys, transaction, false, timeout, callback, state);
        }

        public IAsyncResult BeginLoadOrCreate(InstanceKey key, Guid suggestedId, ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (key == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("key");
            }
            if (key.Value == Guid.Empty)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.Argument("key", System.ServiceModel.Activities.SR.InvalidKey);
            }
            return new LoadOrCreateAsyncResult(this, key, suggestedId, true, associatedKeys, transaction, false, timeout, callback, state);
        }

        internal IAsyncResult BeginReserveThrottle(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReserveThrottleAsyncResult(this, timeout, callback, state);
        }

        public void Close()
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    this.ThrowIfClosedOrAborted();
                }
                if (this.instanceCache != null)
                {
                    if (this.instanceCache.Count > 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        this.instanceCache = null;
                    }
                }
            }
            if (flag)
            {
                this.Abort();
                this.ThrowIfClosedOrAborted();
                throw Fx.AssertAndThrow("Should have thrown due to abort.");
            }
        }

        private InstancePersistenceCommand CreateLoadCommandHelper(InstanceKey key, out InstanceHandle handle, bool canCreateInstance, Guid suggestedIdOrId, ICollection<InstanceKey> associatedKeys, bool loadAny)
        {
            if (loadAny)
            {
                handle = this.store.CreateInstanceHandle(this.owner);
                return new TryLoadRunnableWorkflowCommand();
            }
            if (key != null)
            {
                LoadWorkflowByInstanceKeyCommand command;
                handle = this.store.CreateInstanceHandle(this.owner);
                if (canCreateInstance)
                {
                    command = new LoadWorkflowByInstanceKeyCommand {
                        LookupInstanceKey = key.Value,
                        AssociateInstanceKeyToInstanceId = (suggestedIdOrId == Guid.Empty) ? Guid.NewGuid() : suggestedIdOrId,
                        AcceptUninitializedInstance = true
                    };
                }
                else
                {
                    command = new LoadWorkflowByInstanceKeyCommand {
                        LookupInstanceKey = key.Value
                    };
                }
                InstanceKey keyToAdd = ((canCreateInstance && (key.Metadata != null)) && (key.Metadata.Count > 0)) ? key : null;
                if (associatedKeys != null)
                {
                    foreach (InstanceKey key3 in associatedKeys)
                    {
                        if (key3 == key)
                        {
                            if (!canCreateInstance)
                            {
                                continue;
                            }
                            keyToAdd = null;
                        }
                        TryAddKeyToInstanceKeysCollection(command.InstanceKeysToAssociate, key3);
                    }
                }
                if (keyToAdd != null)
                {
                    TryAddKeyToInstanceKeysCollection(command.InstanceKeysToAssociate, keyToAdd);
                }
                return command;
            }
            if (associatedKeys != null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.NoAdditionalKeysOnInstanceIdLoad));
            }
            handle = this.store.CreateInstanceHandle(this.owner, (suggestedIdOrId == Guid.Empty) ? Guid.NewGuid() : suggestedIdOrId);
            return new LoadWorkflowCommand { AcceptUninitializedInstance = canCreateInstance };
        }

        private void DetachContext(PersistenceContext contextToAbort)
        {
            if (contextToAbort.IsVisible)
            {
                foreach (InstanceKey key in contextToAbort.AssociatedKeys)
                {
                    this.keyMap.Remove(key.Value);
                }
                try
                {
                }
                finally
                {
                    if (this.instanceCache.Remove(contextToAbort.InstanceId))
                    {
                        contextToAbort.IsVisible = false;
                        this.throttle.Exit();
                    }
                }
            }
        }

        private void DetachContext(PersistenceContext contextToAbort, ref List<PersistenceContext> contextsToAbort)
        {
            if (contextsToAbort == null)
            {
                contextsToAbort = new List<PersistenceContext>();
            }
            contextsToAbort.Add(contextToAbort);
            this.DetachContext(contextToAbort);
        }

        public PersistenceContext EndLoad(IAsyncResult result, out bool fromCache)
        {
            return LoadOrCreateAsyncResult.End(result, out fromCache);
        }

        public PersistenceContext EndLoadOrCreate(IAsyncResult result, out bool fromCache)
        {
            return LoadOrCreateAsyncResult.End(result, out fromCache);
        }

        internal void EndReserveThrottle(out bool ownsThrottle, IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                ownsThrottle = true;
            }
            else
            {
                ownsThrottle = ReserveThrottleAsyncResult.End(result);
            }
        }

        internal ReadOnlyCollection<BookmarkInfo> GetBookmarksForInstance(InstanceKey instanceKey)
        {
            ReadOnlyCollection<BookmarkInfo> bookmarks = null;
            lock (this.ThisLock)
            {
                PersistenceContext context;
                if (this.keyMap.TryGetValue(instanceKey.Value, out context))
                {
                    bookmarks = context.Bookmarks;
                }
            }
            return bookmarks;
        }

        public IEnumerable<PersistenceContext> GetContexts()
        {
            lock (this.ThisLock)
            {
                this.ThrowIfClosedOrAborted();
                return this.instanceCache.Values.ToList<PersistenceContext>();
            }
        }

        public Transaction GetTransactionForInstance(InstanceKey instanceKey)
        {
            Transaction lockingTransaction = null;
            lock (this.ThisLock)
            {
                PersistenceContext context;
                if (this.keyMap.TryGetValue(instanceKey.Value, out context))
                {
                    lockingTransaction = context.LockingTransaction;
                    if (lockingTransaction != null)
                    {
                        lockingTransaction = lockingTransaction.Clone();
                    }
                }
            }
            return lockingTransaction;
        }

        public WorkflowServiceInstance InitializeInstance(Guid instanceId, PersistenceContext context, IDictionary<XName, InstanceValue> instance, WorkflowCreationContext creationContext)
        {
            return WorkflowServiceInstance.InitializeInstance(context, instanceId, this.workflowDefinition, instance, creationContext, WorkflowSynchronizationContext.Instance, this.serviceHost);
        }

        private PersistenceContext LoadFromCache(InstanceKey key, Guid suggestedIdOrId, bool canCreateInstance)
        {
            PersistenceContext context = null;
            if ((key != null) || (suggestedIdOrId != Guid.Empty))
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfClosedOrAborted();
                    if (key == null)
                    {
                        this.instanceCache.TryGetValue(suggestedIdOrId, out context);
                        return context;
                    }
                    this.keyMap.TryGetValue(key.Value, out context);
                }
            }
            return context;
        }

        private void RegisterPipelineInUse(PersistencePipeline pipeline)
        {
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.DirectoryAborted));
                }
                this.pipelinesInUse.Add(pipeline);
            }
        }

        internal void ReleaseThrottle()
        {
            this.throttle.Exit();
        }

        internal void RemoveAssociations(PersistenceContext context, IEnumerable<InstanceKey> keys)
        {
            lock (this.ThisLock)
            {
                if (!context.IsPermanentlyRemoved)
                {
                    foreach (InstanceKey key in keys)
                    {
                        if (context.AssociatedKeys.Remove(key))
                        {
                            Fx.AssertAndThrow(this.instanceCache != null, "Since the context must be visible, it must still be in the cache.");
                            this.keyMap.Remove(key.Value);
                        }
                    }
                }
            }
        }

        internal void RemoveInstance(PersistenceContext context)
        {
            this.RemoveInstance(context, false);
        }

        internal void RemoveInstance(PersistenceContext context, bool permanent)
        {
            lock (this.ThisLock)
            {
                if (permanent)
                {
                    context.IsPermanentlyRemoved = true;
                }
                this.DetachContext(context);
            }
        }

        private void ThrowIfClosedOrAborted()
        {
            if (this.instanceCache == null)
            {
                if (this.aborted)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.DirectoryAborted));
                }
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ObjectDisposedException(base.GetType().Name));
            }
        }

        internal bool TryAddAssociations(PersistenceContext context, IEnumerable<InstanceKey> keys, HashSet<InstanceKey> keysToAssociate, HashSet<InstanceKey> keysToDisassociate)
        {
            List<PersistenceContext> contextsToAbort = null;
            bool flag2;
            try
            {
                lock (this.ThisLock)
                {
                    if (context.IsPermanentlyRemoved)
                    {
                        return false;
                    }
                    if (this.store == null)
                    {
                        foreach (InstanceKey key in keys)
                        {
                            PersistenceContext context2;
                            if (!context.AssociatedKeys.Contains(key) && this.keyMap.TryGetValue(key.Value, out context2))
                            {
                                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstanceKeyCollisionException(null, context.InstanceId, key, context2.InstanceId));
                            }
                        }
                    }
                    foreach (InstanceKey key2 in keys)
                    {
                        if (context.AssociatedKeys.Contains(key2))
                        {
                            if (keysToDisassociate != null)
                            {
                                keysToDisassociate.Remove(key2);
                            }
                        }
                        else
                        {
                            PersistenceContext context3;
                            Fx.AssertAndThrow(this.instanceCache != null, "Since the context must be visible, it must still be in the cache.");
                            if (this.keyMap.TryGetValue(key2.Value, out context3))
                            {
                                this.DetachContext(context3, ref contextsToAbort);
                            }
                            this.keyMap.Add(key2.Value, context);
                            context.AssociatedKeys.Add(key2);
                            keysToAssociate.Add(key2);
                        }
                    }
                    flag2 = true;
                }
            }
            finally
            {
                this.AbortContexts(contextsToAbort);
            }
            return flag2;
        }

        private static void TryAddKeyToInstanceKeysCollection(IDictionary<Guid, IDictionary<XName, InstanceValue>> instanceKeysToAssociate, InstanceKey keyToAdd)
        {
            if (instanceKeysToAssociate.ContainsKey(keyToAdd.Value))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.DuplicateInstanceKeyExists(keyToAdd.Value)));
            }
            instanceKeysToAssociate.Add(keyToAdd.Value, keyToAdd.Metadata);
        }

        private void UnregisterPipelineInUse(PersistencePipeline pipeline)
        {
            lock (this.ThisLock)
            {
                if (!this.aborted)
                {
                    this.pipelinesInUse.Remove(pipeline);
                }
            }
        }

        public DurableConsistencyScope ConsistencyScope { get; private set; }

        public IDictionary<XName, InstanceValue> InstanceMetadataChanges { get; private set; }

        public int MaxInstances { get; private set; }

        private object ThisLock
        {
            get
            {
                return this.keyMap;
            }
        }

        private class InstanceThrottle
        {
            private int maxCount;
            private readonly ThreadNeutralSemaphore throttle;
            private bool warningIssued;
            private int warningRestoreLimit;

            public InstanceThrottle(int maxCount)
            {
                this.throttle = new ThreadNeutralSemaphore(maxCount);
                this.maxCount = maxCount;
                this.warningRestoreLimit = (int) Math.Floor((double) (0.7 * maxCount));
            }

            public void Abort()
            {
                this.throttle.Abort();
            }

            public bool EnterAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
            {
                bool flag = this.throttle.EnterAsync(timeout, callback, state);
                if (!flag)
                {
                    this.TraceWarning();
                }
                return flag;
            }

            public void Exit()
            {
                if (this.throttle.Exit() < this.warningRestoreLimit)
                {
                    this.warningIssued = false;
                }
            }

            private void TraceWarning()
            {
                if (TraceCore.MaxInstancesExceededIsEnabled(Fx.Trace) && !this.warningIssued)
                {
                    TraceCore.MaxInstancesExceeded(Fx.Trace, this.maxCount);
                    this.warningIssued = true;
                }
            }
        }

        private class LoadOrCreateAsyncResult : AsyncResult
        {
            private bool addedToCacheResult;
            private readonly ICollection<InstanceKey> associatedKeys;
            private readonly bool canCreateInstance;
            private PersistenceContext context;
            private List<PersistenceContext> contextsToAbort;
            private InstanceHandle handle;
            private static AsyncResult.AsyncCompletion handleContextEnlist = new AsyncResult.AsyncCompletion(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleContextEnlist);
            private static AsyncResult.AsyncCompletion handleExecute = new AsyncResult.AsyncCompletion(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleExecute);
            private static AsyncResult.AsyncCompletion handleLoadPipeline = new AsyncResult.AsyncCompletion(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleLoadPipeline);
            private static Action<object> handleLoadRetry = new Action<object>(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleLoadRetry);
            private static AsyncResult.AsyncCompletion handleReserveThrottle = new AsyncResult.AsyncCompletion(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleReserveThrottle);
            private bool isInstanceInitialized;
            private readonly InstanceKey key;
            private readonly bool loadAny;
            private bool loadPending;
            private bool lockInstance;
            private static Action<AsyncResult, Exception> onComplete = new Action<AsyncResult, Exception>(PersistenceProviderDirectory.LoadOrCreateAsyncResult.OnComplete);
            private PersistencePipeline pipeline;
            private readonly PersistenceProviderDirectory ppd;
            private PersistenceContext result;
            private Guid suggestedIdOrId;
            private readonly TimeoutHelper timeoutHelper;
            private readonly Transaction transaction;
            private InstanceView view;

            public LoadOrCreateAsyncResult(PersistenceProviderDirectory ppd, InstanceKey key, Guid suggestedIdOrId, bool canCreateInstance, ICollection<InstanceKey> associatedKeys, Transaction transaction, bool loadAny, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                bool flag;
                this.ppd = ppd;
                this.key = key;
                this.suggestedIdOrId = suggestedIdOrId;
                this.canCreateInstance = canCreateInstance;
                this.associatedKeys = associatedKeys;
                this.transaction = transaction;
                this.loadAny = loadAny;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if ((this.associatedKeys != null) && (this.associatedKeys.Count == 0))
                {
                    this.associatedKeys = null;
                }
                base.OnCompleting = onComplete;
                if (this.transaction != null)
                {
                    TransactionInterop.GetDtcTransaction(this.transaction);
                }
                Exception exception = null;
                try
                {
                    this.result = this.loadAny ? null : this.ppd.LoadFromCache(this.key, this.suggestedIdOrId, this.canCreateInstance);
                    if (this.result != null)
                    {
                        flag = true;
                    }
                    else
                    {
                        if ((this.ppd.store == null) && !this.canCreateInstance)
                        {
                            if (this.key != null)
                            {
                                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstanceKeyNotReadyException(null, this.key));
                            }
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstanceNotReadyException(null, this.suggestedIdOrId));
                        }
                        IAsyncResult result = this.ppd.BeginReserveThrottle(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleReserveThrottle), this);
                        flag = base.SyncContinue(result);
                    }
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
                    base.Complete(true, exception);
                }
            }

            private bool AddToCache()
            {
                lock (this.ppd.ThisLock)
                {
                    PersistenceContext context2;
                    this.ppd.ThrowIfClosedOrAborted();
                    if (this.ppd.store == null)
                    {
                        if (this.key == null)
                        {
                            this.ppd.instanceCache.TryGetValue(this.suggestedIdOrId, out this.result);
                        }
                        else
                        {
                            this.ppd.keyMap.TryGetValue(this.key.Value, out this.result);
                        }
                        if (this.result != null)
                        {
                            return true;
                        }
                        foreach (InstanceKey key in this.context.AssociatedKeys)
                        {
                            PersistenceContext context;
                            if (this.ppd.keyMap.TryGetValue(key.Value, out context))
                            {
                                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstanceKeyCollisionException(null, this.context.InstanceId, key, context.InstanceId));
                            }
                        }
                    }
                    if (!this.context.IsHandleValid)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.HandleFreedInDirectory));
                    }
                    this.context.IsVisible = true;
                    if (this.ppd.instanceCache.TryGetValue(this.context.InstanceId, out context2))
                    {
                        this.ppd.DetachContext(context2, ref this.contextsToAbort);
                    }
                    foreach (InstanceKey key2 in this.context.AssociatedKeys)
                    {
                        if (this.ppd.keyMap.TryGetValue(key2.Value, out context2))
                        {
                            this.ppd.DetachContext(context2, ref this.contextsToAbort);
                        }
                        this.ppd.keyMap.Add(key2.Value, this.context);
                    }
                    try
                    {
                    }
                    finally
                    {
                        this.ppd.instanceCache.Add(this.context.InstanceId, this.context);
                        this.loadPending = false;
                    }
                }
                this.addedToCacheResult = true;
                this.result = this.context;
                this.context = null;
                return true;
            }

            private bool AfterLoad()
            {
                if (!this.isInstanceInitialized)
                {
                    if (!this.canCreateInstance)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.PersistenceViolationNoCreate));
                    }
                    if (this.view == null)
                    {
                        this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.suggestedIdOrId, null, true, false, null);
                    }
                    else
                    {
                        this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.view.InstanceId, from keyView in this.view.InstanceKeys.Values select new InstanceKey(keyView.InstanceKey, keyView.InstanceKeyMetadata), true, true, this.view);
                    }
                    this.handle = null;
                }
                else
                {
                    this.EnsureWorkflowHostType();
                    this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.view.InstanceId, from keyView in this.view.InstanceKeys.Values select new InstanceKey(keyView.InstanceKey, keyView.InstanceKeyMetadata), false, true, this.view);
                    this.handle = null;
                    IEnumerable<IPersistencePipelineModule> pipelineModules = this.context.GetInstance(null).PipelineModules;
                    if (pipelineModules != null)
                    {
                        IAsyncResult result;
                        this.pipeline = new PersistencePipeline(pipelineModules);
                        this.pipeline.SetLoadedValues(this.view.InstanceData);
                        this.ppd.RegisterPipelineInUse(this.pipeline);
                        using (base.PrepareTransactionalCall(this.transaction))
                        {
                            result = this.pipeline.BeginLoad(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleLoadPipeline), this);
                        }
                        return base.SyncContinue(result);
                    }
                }
                return this.Finish();
            }

            public static PersistenceContext End(IAsyncResult result, out bool fromCache)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult result2 = AsyncResult.End<PersistenceProviderDirectory.LoadOrCreateAsyncResult>(result);
                fromCache = !result2.addedToCacheResult;
                return result2.result;
            }

            private void EnsureWorkflowHostType()
            {
                InstanceValue value2;
                if (!this.view.InstanceMetadata.TryGetValue(WorkflowNamespace.WorkflowHostType, out value2))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceCommandException(SRCore.NullAssignedToValueType(this.ppd.serviceHost.DurableInstancingOptions.ScopeName)));
                }
                if (!this.ppd.serviceHost.DurableInstancingOptions.ScopeName.Equals(value2.Value))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceCommandException(SRCore.IncorrectValueType(this.ppd.serviceHost.DurableInstancingOptions.ScopeName, value2.Value)));
                }
            }

            private bool Finish()
            {
                IAsyncResult result;
                if (this.pipeline != null)
                {
                    this.pipeline.Publish();
                }
                this.context.Open(TimeSpan.Zero);
                using (base.PrepareTransactionalCall(this.transaction))
                {
                    result = this.context.BeginEnlist(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleContextEnlist), this);
                }
                return base.SyncContinue(result);
            }

            private static bool HandleContextEnlist(IAsyncResult result)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult asyncState = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) result.AsyncState;
                asyncState.context.EndEnlist(result);
                return asyncState.AddToCache();
            }

            private static bool HandleExecute(IAsyncResult result)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult asyncState = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) result.AsyncState;
                try
                {
                    asyncState.view = asyncState.ppd.store.EndExecute(result);
                }
                catch (InstanceHandleConflictException)
                {
                    asyncState.view = null;
                }
                catch (InstanceLockLostException)
                {
                    asyncState.view = null;
                }
                if (asyncState.view == null)
                {
                    return asyncState.ResolveHandleConflict();
                }
                if (asyncState.view.InstanceState == InstanceState.Unknown)
                {
                    if (asyncState.loadAny)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstanceNotReadyException(System.ServiceModel.Activities.SR.NoRunnableInstances));
                    }
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.StoreViolationNoInstanceBound));
                }
                asyncState.isInstanceInitialized = asyncState.view.InstanceState != InstanceState.Uninitialized;
                return asyncState.AfterLoad();
            }

            private static bool HandleLoadPipeline(IAsyncResult result)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult asyncState = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) result.AsyncState;
                asyncState.pipeline.EndLoad(result);
                return asyncState.Finish();
            }

            private static void HandleLoadRetry(object state)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult result = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) state;
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = result.Load();
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

            private static bool HandleReserveThrottle(IAsyncResult result)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult asyncState = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) result.AsyncState;
                asyncState.ppd.EndReserveThrottle(out asyncState.loadPending, result);
                asyncState.lockInstance = (asyncState.ppd.ConsistencyScope != DurableConsistencyScope.Local) || !asyncState.canCreateInstance;
                return asyncState.Load();
            }

            private bool Load()
            {
                IAsyncResult result;
                if (this.ppd.store == null)
                {
                    this.isInstanceInitialized = false;
                    this.context = new PersistenceContext(this.ppd, (this.suggestedIdOrId == Guid.Empty) ? Guid.NewGuid() : this.suggestedIdOrId, this.key, this.associatedKeys);
                    return this.Finish();
                }
                if (this.canCreateInstance && !this.lockInstance)
                {
                    if (this.suggestedIdOrId == Guid.Empty)
                    {
                        this.suggestedIdOrId = Guid.NewGuid();
                    }
                    this.handle = this.ppd.store.CreateInstanceHandle(this.ppd.owner, this.suggestedIdOrId);
                    this.isInstanceInitialized = false;
                    return this.AfterLoad();
                }
                InstancePersistenceCommand command = this.ppd.CreateLoadCommandHelper(this.key, out this.handle, this.canCreateInstance, this.suggestedIdOrId, this.associatedKeys, this.loadAny);
                try
                {
                    using (base.PrepareTransactionalCall(this.transaction))
                    {
                        result = this.ppd.store.BeginExecute(this.handle, command, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(new AsyncResult.AsyncCompletion(PersistenceProviderDirectory.LoadOrCreateAsyncResult.HandleExecute)), this);
                    }
                }
                catch (InstanceHandleConflictException)
                {
                    result = null;
                }
                catch (InstanceLockLostException)
                {
                    result = null;
                }
                if (result == null)
                {
                    return this.ResolveHandleConflict();
                }
                return base.SyncContinue(result);
            }

            private static void OnComplete(AsyncResult result, Exception exception)
            {
                PersistenceProviderDirectory.LoadOrCreateAsyncResult result2 = (PersistenceProviderDirectory.LoadOrCreateAsyncResult) result;
                if (result2.pipeline != null)
                {
                    result2.ppd.UnregisterPipelineInUse(result2.pipeline);
                }
                if (result2.loadPending)
                {
                    result2.ppd.throttle.Exit();
                }
                if (result2.context != null)
                {
                    lock (result2.ppd.ThisLock)
                    {
                        result2.ppd.DetachContext(result2.context, ref result2.contextsToAbort);
                        goto Label_008C;
                    }
                }
                if (result2.handle != null)
                {
                    result2.handle.Free();
                }
            Label_008C:
                result2.ppd.AbortContexts(result2.contextsToAbort);
                if (exception is OperationCanceledException)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.LoadingAborted, exception));
                }
            }

            private bool ResolveHandleConflict()
            {
                this.result = this.loadAny ? null : this.ppd.LoadFromCache(this.key, this.suggestedIdOrId, this.canCreateInstance);
                if (this.result != null)
                {
                    return true;
                }
                ActionItem.Schedule(handleLoadRetry, this);
                return false;
            }
        }

        private class ReserveThrottleAsyncResult : AsyncResult
        {
            private static readonly FastAsyncCallback onThrottleAcquired = new FastAsyncCallback(PersistenceProviderDirectory.ReserveThrottleAsyncResult.OnThrottleAcquired);
            private bool ownsThrottle;

            public ReserveThrottleAsyncResult(PersistenceProviderDirectory directory, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                if (directory.throttle.EnterAsync(timeout, onThrottleAcquired, this))
                {
                    this.ownsThrottle = true;
                    base.Complete(true);
                }
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<PersistenceProviderDirectory.ReserveThrottleAsyncResult>(result).ownsThrottle;
            }

            private static void OnThrottleAcquired(object state, Exception asyncException)
            {
                PersistenceProviderDirectory.ReserveThrottleAsyncResult result = (PersistenceProviderDirectory.ReserveThrottleAsyncResult) state;
                result.ownsThrottle = asyncException == null;
                result.Complete(false, asyncException);
            }
        }
    }
}

