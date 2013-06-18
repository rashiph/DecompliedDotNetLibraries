namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    public sealed class InstancePersistenceContext
    {
        private int cancellationHandlerCalled;
        private bool freezeTransaction;
        private CommittableTransaction myTransaction;
        private readonly TimeSpan timeout;
        private System.Transactions.Transaction transaction;

        private InstancePersistenceContext(System.Runtime.DurableInstancing.InstanceHandle handle)
        {
            this.InstanceHandle = handle;
            System.Runtime.DurableInstancing.InstanceView view = handle.View.Clone();
            view.InstanceStoreQueryResults = null;
            this.InstanceView = view;
            this.cancellationHandlerCalled = 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal InstancePersistenceContext(System.Runtime.DurableInstancing.InstanceHandle handle, TimeSpan timeout) : this(handle)
        {
            this.timeout = timeout;
        }

        internal InstancePersistenceContext(System.Runtime.DurableInstancing.InstanceHandle handle, System.Transactions.Transaction transaction) : this(handle)
        {
            this.transaction = transaction.Clone();
            this.IsHostTransaction = true;
        }

        public void AssociatedInstanceKey(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("AssociatedInstanceKey");
            Dictionary<Guid, InstanceKeyView> dictionary = new Dictionary<Guid, InstanceKeyView>(this.InstanceView.InstanceKeys);
            if (((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == InstanceValueConsistency.None) && dictionary.ContainsKey(key))
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyAssociated));
            }
            InstanceKeyView view = new InstanceKeyView(key) {
                InstanceKeyState = InstanceKeyState.Associated,
                InstanceKeyMetadataConsistency = InstanceValueConsistency.None
            };
            dictionary[view.InstanceKey] = view;
            this.InstanceView.InstanceKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
        }

        public IAsyncResult BeginBindReclaimedLock(long instanceVersion, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new BindReclaimedLockAsyncResult(this, this.InitiateBindReclaimedLockHelper("BeginBindReclaimedLock", instanceVersion, timeout), timeout, callback, state);
        }

        public IAsyncResult BeginExecute(InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            this.ThrowIfNotActive("BeginExecute");
            try
            {
                this.ReconcileTransaction();
                result = new ExecuteAsyncResult(this, command, timeout, callback, state);
            }
            catch (TimeoutException)
            {
                this.InstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                this.InstanceHandle.Free();
                throw;
            }
            return result;
        }

        internal static IAsyncResult BeginOuterExecute(System.Runtime.DurableInstancing.InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, System.Transactions.Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            try
            {
                result = new ExecuteAsyncResult(initialInstanceHandle, command, transaction, timeout, callback, state);
            }
            catch (TimeoutException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            return result;
        }

        public void BindAcquiredLock(long instanceVersion)
        {
            if (instanceVersion < 0L)
            {
                throw Fx.Exception.ArgumentOutOfRange("instanceVersion", instanceVersion, SRCore.InvalidLockToken);
            }
            this.ThrowIfNotActive("BindAcquiredLock");
            this.InstanceView.BindLock(instanceVersion);
            this.IsHandleDoomedByRollback = true;
            this.InstanceHandle.Bind(instanceVersion);
        }

        public void BindEvent(InstancePersistenceEvent persistenceEvent)
        {
            if (persistenceEvent == null)
            {
                throw Fx.Exception.ArgumentNull("persistenceEvent");
            }
            this.ThrowIfNotActive("BindEvent");
            if (!this.InstanceView.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            this.IsHandleDoomedByRollback = true;
            this.InstanceHandle.BindOwnerEvent(persistenceEvent);
        }

        public void BindInstance(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceId", SRCore.GuidCannotBeEmpty);
            }
            this.ThrowIfNotActive("BindInstance");
            this.InstanceView.BindInstance(instanceId);
            this.IsHandleDoomedByRollback = true;
            this.InstanceHandle.BindInstance(instanceId);
        }

        public void BindInstanceOwner(Guid instanceOwnerId, Guid lockToken)
        {
            if (instanceOwnerId == Guid.Empty)
            {
                throw Fx.Exception.Argument("instanceOwnerId", SRCore.GuidCannotBeEmpty);
            }
            if (lockToken == Guid.Empty)
            {
                throw Fx.Exception.Argument("lockToken", SRCore.GuidCannotBeEmpty);
            }
            this.ThrowIfNotActive("BindInstanceOwner");
            InstanceOwner orCreateOwner = this.InstanceHandle.Store.GetOrCreateOwner(instanceOwnerId, lockToken);
            this.InstanceView.BindOwner(orCreateOwner);
            this.IsHandleDoomedByRollback = true;
            this.InstanceHandle.BindOwner(orCreateOwner);
        }

        public void BindReclaimedLock(long instanceVersion, TimeSpan timeout)
        {
            if (!this.InitiateBindReclaimedLockHelper("BindReclaimedLock", instanceVersion, timeout).Wait(timeout))
            {
                this.InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
            }
            this.ConcludeBindReclaimedLockHelper();
        }

        public void CompletedInstance()
        {
            this.ThrowIfNotLocked();
            this.ThrowIfUninitialized();
            this.ThrowIfCompleted();
            if ((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == InstanceValueConsistency.None)
            {
                foreach (KeyValuePair<Guid, InstanceKeyView> pair in this.InstanceView.InstanceKeys)
                {
                    if (pair.Value.InstanceKeyState == InstanceKeyState.Associated)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotCompleteWithKeys));
                    }
                }
            }
            this.ThrowIfNotTransactional("CompletedInstance");
            this.InstanceView.InstanceState = InstanceState.Completed;
        }

        public void CompletedInstanceKey(Guid key)
        {
            InstanceKeyView view;
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("CompletedInstanceKey");
            this.InstanceView.InstanceKeys.TryGetValue(key, out view);
            if ((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == InstanceValueConsistency.None)
            {
                if (view != null)
                {
                    if (view.InstanceKeyState == InstanceKeyState.Completed)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyCompleted));
                    }
                }
                else if ((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.Partial) == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }
            }
            if (view != null)
            {
                view.InstanceKeyState = InstanceKeyState.Completed;
            }
            else
            {
                Dictionary<Guid, InstanceKeyView> dictionary = new Dictionary<Guid, InstanceKeyView>(this.InstanceView.InstanceKeys);
                InstanceKeyView view2 = new InstanceKeyView(key) {
                    InstanceKeyState = InstanceKeyState.Completed,
                    InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial
                };
                dictionary[view2.InstanceKey] = view2;
                this.InstanceView.InstanceKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
            }
        }

        private void ConcludeBindReclaimedLockHelper()
        {
            long instanceVersion = -1L;
            try
            {
                if (!this.InstanceHandle.FinishReclaim(ref instanceVersion))
                {
                    this.InstanceHandle.Free();
                    throw Fx.Exception.AsError(new InstanceHandleConflictException(this.LastAsyncResult.CurrentCommand.Name, this.InstanceView.InstanceId));
                }
            }
            finally
            {
                if (instanceVersion >= 0L)
                {
                    this.InstanceView.FinishBindLock(instanceVersion);
                }
            }
        }

        public Exception CreateBindReclaimedLockException(long instanceVersion)
        {
            return new BindReclaimedLockException(this.InitiateBindReclaimedLockHelper("CreateBindReclaimedLockException", instanceVersion, TimeSpan.MaxValue));
        }

        public void EndBindReclaimedLock(IAsyncResult result)
        {
            BindReclaimedLockAsyncResult.End(result);
        }

        public void EndExecute(IAsyncResult result)
        {
            ExecuteAsyncResult.End(result);
        }

        internal static System.Runtime.DurableInstancing.InstanceView EndOuterExecute(IAsyncResult result)
        {
            System.Runtime.DurableInstancing.InstanceView view = ExecuteAsyncResult.End(result);
            if (view == null)
            {
                throw Fx.Exception.Argument("result", SRCore.InvalidAsyncResult);
            }
            return view;
        }

        public void Execute(InstancePersistenceCommand command, TimeSpan timeout)
        {
            if (command == null)
            {
                throw Fx.Exception.ArgumentNull("command");
            }
            this.ThrowIfNotActive("Execute");
            try
            {
                this.ReconcileTransaction();
                ExecuteAsyncResult.End(new ExecuteAsyncResult(this, command, timeout));
            }
            catch (TimeoutException)
            {
                this.InstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                this.InstanceHandle.Free();
                throw;
            }
        }

        private AsyncWaitHandle InitiateBindReclaimedLockHelper(string methodName, long instanceVersion, TimeSpan timeout)
        {
            if (instanceVersion < 0L)
            {
                throw Fx.Exception.ArgumentOutOfRange("instanceVersion", instanceVersion, SRCore.InvalidLockToken);
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.ThrowIfNotActive(methodName);
            this.InstanceView.StartBindLock(instanceVersion);
            this.IsHandleDoomedByRollback = true;
            AsyncWaitHandle handle = this.InstanceHandle.StartReclaim(instanceVersion);
            if (handle == null)
            {
                this.InstanceHandle.Free();
                throw Fx.Exception.AsError(new InstanceHandleConflictException(this.LastAsyncResult.CurrentCommand.Name, this.InstanceView.InstanceId));
            }
            return handle;
        }

        public void LoadedInstance(InstanceState state, IDictionary<XName, InstanceValue> instanceData, IDictionary<XName, InstanceValue> instanceMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeyMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeyMetadata)
        {
            if (state == InstanceState.Uninitialized)
            {
                if ((instanceData != null) && (instanceData.Count > 0))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.UninitializedCannotHaveData));
                }
            }
            else if (state == InstanceState.Completed)
            {
                if ((associatedInstanceKeyMetadata != null) && (associatedInstanceKeyMetadata.Count > 0))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CompletedMustNotHaveAssociatedKeys));
                }
            }
            else if (state != InstanceState.Initialized)
            {
                throw Fx.Exception.Argument("state", SRCore.InvalidInstanceState);
            }
            this.ThrowIfNoInstance();
            this.ThrowIfNotActive("PersistedInstance");
            InstanceValueConsistency consistency = (this.InstanceView.IsBoundToLock || (state == InstanceState.Completed)) ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
            ReadOnlyDictionary<XName, InstanceValue> dictionary = instanceData.ReadOnlyCopy(false);
            ReadOnlyDictionary<XName, InstanceValue> dictionary2 = instanceMetadata.ReadOnlyCopy(false);
            Dictionary<Guid, InstanceKeyView> dictionary3 = null;
            int capacity = ((associatedInstanceKeyMetadata != null) ? associatedInstanceKeyMetadata.Count : 0) + ((completedInstanceKeyMetadata != null) ? completedInstanceKeyMetadata.Count : 0);
            if (capacity > 0)
            {
                dictionary3 = new Dictionary<Guid, InstanceKeyView>(capacity);
            }
            if ((associatedInstanceKeyMetadata != null) && (associatedInstanceKeyMetadata.Count > 0))
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair in associatedInstanceKeyMetadata)
                {
                    InstanceKeyView view = new InstanceKeyView(pair.Key) {
                        InstanceKeyState = InstanceKeyState.Associated,
                        InstanceKeyMetadata = pair.Value.ReadOnlyCopy(false),
                        InstanceKeyMetadataConsistency = this.InstanceView.IsBoundToLock ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt
                    };
                    dictionary3.Add(view.InstanceKey, view);
                }
            }
            if ((completedInstanceKeyMetadata != null) && (completedInstanceKeyMetadata.Count > 0))
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> pair2 in completedInstanceKeyMetadata)
                {
                    InstanceKeyView view2 = new InstanceKeyView(pair2.Key) {
                        InstanceKeyState = InstanceKeyState.Completed,
                        InstanceKeyMetadata = pair2.Value.ReadOnlyCopy(false),
                        InstanceKeyMetadataConsistency = consistency
                    };
                    dictionary3.Add(view2.InstanceKey, view2);
                }
            }
            this.InstanceView.InstanceState = state;
            this.InstanceView.InstanceData = dictionary;
            this.InstanceView.InstanceDataConsistency = consistency;
            this.InstanceView.InstanceMetadata = dictionary2;
            this.InstanceView.InstanceMetadataConsistency = consistency;
            this.InstanceView.InstanceKeys = (dictionary3 == null) ? null : new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary3, false);
            this.InstanceView.InstanceKeysConsistency = consistency;
        }

        internal void NotifyHandleFree()
        {
            this.CancelRequested = true;
            ExecuteAsyncResult lastAsyncResult = this.LastAsyncResult;
            Action<InstancePersistenceContext> action = (lastAsyncResult == null) ? null : lastAsyncResult.CancellationHandler;
            if (action != null)
            {
                try
                {
                    if (Interlocked.CompareExchange(ref this.cancellationHandlerCalled, 0, 1) == 0)
                    {
                        action(this);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.OnCancelRequestedThrew, exception));
                }
            }
        }

        internal static System.Runtime.DurableInstancing.InstanceView OuterExecute(System.Runtime.DurableInstancing.InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, System.Transactions.Transaction transaction, TimeSpan timeout)
        {
            System.Runtime.DurableInstancing.InstanceView view;
            try
            {
                view = ExecuteAsyncResult.End(new ExecuteAsyncResult(initialInstanceHandle, command, transaction, timeout));
            }
            catch (TimeoutException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            catch (OperationCanceledException)
            {
                initialInstanceHandle.Free();
                throw;
            }
            return view;
        }

        public void PersistedInstance(IDictionary<XName, InstanceValue> data)
        {
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("PersistedInstance");
            this.InstanceView.InstanceData = data.ReadOnlyCopy(true);
            this.InstanceView.InstanceDataConsistency = InstanceValueConsistency.None;
            this.InstanceView.InstanceState = InstanceState.Initialized;
        }

        internal void PrepareForReuse()
        {
            Fx.AssertAndThrow(!this.Active, "Prior use not yet complete!");
            Fx.AssertAndThrow(this.IsHostTransaction, "Can only reuse contexts with host transactions.");
        }

        public void QueriedInstanceStore(InstanceStoreQueryResult queryResult)
        {
            if (queryResult == null)
            {
                throw Fx.Exception.ArgumentNull("queryResult");
            }
            this.ThrowIfNotActive("QueriedInstanceStore");
            this.InstanceView.QueryResultsBacking.Add(queryResult);
        }

        public void ReadInstanceKeyMetadata(Guid key, IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            InstanceKeyView view;
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            this.ThrowIfNoInstance();
            this.ThrowIfNotActive("ReadInstanceKeyMetadata");
            if (!this.InstanceView.InstanceKeys.TryGetValue(key, out view))
            {
                if (this.InstanceView.InstanceKeysConsistency == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }
                Dictionary<Guid, InstanceKeyView> dictionary = new Dictionary<Guid, InstanceKeyView>(this.InstanceView.InstanceKeys);
                view = new InstanceKeyView(key);
                if (complete)
                {
                    view.InstanceKeyMetadata = metadata.ReadOnlyCopy(false);
                    view.InstanceKeyMetadataConsistency = InstanceValueConsistency.None;
                }
                else
                {
                    view.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(null, false);
                    view.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                }
                if (!this.InstanceView.IsBoundToLock && (this.InstanceView.InstanceState != InstanceState.Completed))
                {
                    view.InstanceKeyMetadataConsistency |= InstanceValueConsistency.InDoubt;
                }
                dictionary[view.InstanceKey] = view;
                this.InstanceView.InstanceKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
            }
            else if (view.InstanceKeyMetadataConsistency != InstanceValueConsistency.None)
            {
                if (complete)
                {
                    view.InstanceKeyMetadata = metadata.ReadOnlyCopy(false);
                    view.InstanceKeyMetadataConsistency = (this.InstanceView.IsBoundToLock || (this.InstanceView.InstanceState == InstanceState.Completed)) ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
                }
                else if ((this.InstanceView.IsBoundToLock || (this.InstanceView.InstanceState == InstanceState.Completed)) && ((view.InstanceKeyMetadataConsistency & InstanceValueConsistency.InDoubt) != InstanceValueConsistency.None))
                {
                    view.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(null, false);
                    view.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                }
                else
                {
                    view.InstanceKeyMetadata = metadata.ReadOnlyMergeInto(view.InstanceKeyMetadata, false);
                    view.InstanceKeyMetadataConsistency |= InstanceValueConsistency.Partial;
                }
            }
        }

        public void ReadInstanceMetadata(IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            this.ThrowIfNoInstance();
            this.ThrowIfNotActive("ReadInstanceMetadata");
            if (this.InstanceView.InstanceMetadataConsistency != InstanceValueConsistency.None)
            {
                if (complete)
                {
                    this.InstanceView.InstanceMetadata = metadata.ReadOnlyCopy(false);
                    this.InstanceView.InstanceMetadataConsistency = (this.InstanceView.IsBoundToLock || (this.InstanceView.InstanceState == InstanceState.Completed)) ? InstanceValueConsistency.None : InstanceValueConsistency.InDoubt;
                }
                else if ((this.InstanceView.IsBoundToLock || (this.InstanceView.InstanceState == InstanceState.Completed)) && ((this.InstanceView.InstanceMetadataConsistency & InstanceValueConsistency.InDoubt) != InstanceValueConsistency.None))
                {
                    this.InstanceView.InstanceMetadata = metadata.ReadOnlyMergeInto(null, false);
                    this.InstanceView.InstanceMetadataConsistency = InstanceValueConsistency.Partial;
                }
                else
                {
                    this.InstanceView.InstanceMetadata = metadata.ReadOnlyMergeInto(this.InstanceView.InstanceMetadata, false);
                    System.Runtime.DurableInstancing.InstanceView instanceView = this.InstanceView;
                    instanceView.InstanceMetadataConsistency |= InstanceValueConsistency.Partial;
                }
            }
        }

        public void ReadInstanceOwnerMetadata(IDictionary<XName, InstanceValue> metadata, bool complete)
        {
            this.ThrowIfNoOwner();
            this.ThrowIfNotActive("ReadInstanceOwnerMetadata");
            if (this.InstanceView.InstanceOwnerMetadataConsistency != InstanceValueConsistency.None)
            {
                if (complete)
                {
                    this.InstanceView.InstanceOwnerMetadata = metadata.ReadOnlyCopy(false);
                    this.InstanceView.InstanceOwnerMetadataConsistency = InstanceValueConsistency.InDoubt;
                }
                else
                {
                    this.InstanceView.InstanceOwnerMetadata = metadata.ReadOnlyMergeInto(this.InstanceView.InstanceOwnerMetadata, false);
                    System.Runtime.DurableInstancing.InstanceView instanceView = this.InstanceView;
                    instanceView.InstanceOwnerMetadataConsistency |= InstanceValueConsistency.Partial;
                }
            }
        }

        private void ReconcileTransaction()
        {
            System.Transactions.Transaction current = System.Transactions.Transaction.Current;
            if (current != null)
            {
                if (this.transaction == null)
                {
                    if (this.freezeTransaction)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MustSetTransactionOnFirstCall));
                    }
                    this.RootAsyncResult.SetInteriorTransaction(current, false);
                    this.transaction = current;
                }
                else if (!current.Equals(this.transaction))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotReplaceTransaction));
                }
            }
            this.freezeTransaction = true;
        }

        internal void RequireTransaction()
        {
            if (this.transaction == null)
            {
                Fx.AssertAndThrow(!this.freezeTransaction, "RequireTransaction called when transaction is frozen.");
                Fx.AssertAndThrow(this.Active, "RequireTransaction called when no command is active.");
                TransactionOptions options = new TransactionOptions {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = this.timeout
                };
                this.myTransaction = new CommittableTransaction(options);
                System.Transactions.Transaction transaction = this.myTransaction.Clone();
                this.RootAsyncResult.SetInteriorTransaction(this.myTransaction, true);
                this.transaction = transaction;
            }
        }

        public void SetCancellationHandler(Action<InstancePersistenceContext> cancellationHandler)
        {
            this.ThrowIfNotActive("SetCancellationHandler");
            this.LastAsyncResult.CancellationHandler = cancellationHandler;
            if (this.CancelRequested && (cancellationHandler != null))
            {
                try
                {
                    if (Interlocked.CompareExchange(ref this.cancellationHandlerCalled, 0, 1) == 0)
                    {
                        cancellationHandler(this);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.OnCancelRequestedThrew, exception));
                }
            }
        }

        private void ThrowIfCompleted()
        {
            if (this.InstanceView.IsBoundToLock && (this.InstanceView.InstanceState == InstanceState.Completed))
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresNotCompleted));
            }
        }

        private void ThrowIfNoInstance()
        {
            if (!this.InstanceView.IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresInstance));
            }
        }

        private void ThrowIfNoOwner()
        {
            if (!this.InstanceView.IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresOwner));
            }
        }

        private void ThrowIfNotActive(string methodName)
        {
            if (!this.Active)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.OutsideInstanceExecutionScope(methodName)));
            }
        }

        private void ThrowIfNotLocked()
        {
            if (!this.InstanceView.IsBoundToLock)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresLock));
            }
        }

        private void ThrowIfNotTransactional(string methodName)
        {
            this.ThrowIfNotActive(methodName);
            if (this.RootAsyncResult.CurrentCommand.IsTransactionEnlistmentOptional)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.OutsideTransactionalCommand(methodName)));
            }
        }

        private void ThrowIfUninitialized()
        {
            if (this.InstanceView.IsBoundToLock && (this.InstanceView.InstanceState == InstanceState.Uninitialized))
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InstanceOperationRequiresNotUninitialized));
            }
        }

        public void UnassociatedInstanceKey(Guid key)
        {
            InstanceKeyView view;
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("UnassociatedInstanceKey");
            this.InstanceView.InstanceKeys.TryGetValue(key, out view);
            if ((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.InDoubt) == InstanceValueConsistency.None)
            {
                if (view != null)
                {
                    if (view.InstanceKeyState == InstanceKeyState.Associated)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotCompleted));
                    }
                }
                else if ((this.InstanceView.InstanceKeysConsistency & InstanceValueConsistency.Partial) == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyAlreadyUnassociated));
                }
            }
            if (view != null)
            {
                Dictionary<Guid, InstanceKeyView> dictionary = new Dictionary<Guid, InstanceKeyView>(this.InstanceView.InstanceKeys);
                dictionary.Remove(key);
                this.InstanceView.InstanceKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
            }
        }

        public void WroteInstanceKeyMetadataValue(Guid key, XName name, InstanceValue value)
        {
            InstanceKeyView view;
            if (key == Guid.Empty)
            {
                throw Fx.Exception.Argument("key", SRCore.InvalidKeyArgument);
            }
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("WroteInstanceKeyMetadataValue");
            if (!this.InstanceView.InstanceKeys.TryGetValue(key, out view))
            {
                if (this.InstanceView.InstanceKeysConsistency == InstanceValueConsistency.None)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.KeyNotAssociated));
                }
                if (!value.IsWriteOnly() && !value.IsDeletedValue)
                {
                    Dictionary<Guid, InstanceKeyView> dictionary = new Dictionary<Guid, InstanceKeyView>(this.InstanceView.InstanceKeys);
                    view = new InstanceKeyView(key);
                    view.AccumulatedMetadataWrites.Add(name, value);
                    view.InstanceKeyMetadataConsistency = InstanceValueConsistency.Partial;
                    dictionary[view.InstanceKey] = view;
                    this.InstanceView.InstanceKeys = new ReadOnlyDictionary<Guid, InstanceKeyView>(dictionary, false);
                    System.Runtime.DurableInstancing.InstanceView instanceView = this.InstanceView;
                    instanceView.InstanceKeysConsistency |= InstanceValueConsistency.Partial;
                }
            }
            else
            {
                view.AccumulatedMetadataWrites.Add(name, value);
            }
        }

        public void WroteInstanceMetadataValue(XName name, InstanceValue value)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            this.ThrowIfNotLocked();
            this.ThrowIfCompleted();
            this.ThrowIfNotTransactional("WroteInstanceMetadataValue");
            this.InstanceView.AccumulatedMetadataWrites[name] = value;
        }

        public void WroteInstanceOwnerMetadataValue(XName name, InstanceValue value)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            if (value == null)
            {
                throw Fx.Exception.ArgumentNull("value");
            }
            this.ThrowIfNoOwner();
            this.ThrowIfNotTransactional("WroteInstanceOwnerMetadataValue");
            this.InstanceView.AccumulatedOwnerMetadataWrites.Add(name, value);
        }

        private bool Active
        {
            get
            {
                return (this.RootAsyncResult != null);
            }
        }

        private bool CancelRequested { get; set; }

        public System.Runtime.DurableInstancing.InstanceHandle InstanceHandle
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceHandle>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceHandle>k__BackingField = value;
            }
        }

        public long InstanceVersion
        {
            get
            {
                return this.InstanceHandle.Version;
            }
        }

        public System.Runtime.DurableInstancing.InstanceView InstanceView
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceView>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceView>k__BackingField = value;
            }
        }

        internal bool IsHandleDoomedByRollback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsHandleDoomedByRollback>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<IsHandleDoomedByRollback>k__BackingField = value;
            }
        }

        private bool IsHostTransaction { get; set; }

        private ExecuteAsyncResult LastAsyncResult { get; set; }

        public Guid LockToken
        {
            get
            {
                if (this.InstanceHandle.Owner != null)
                {
                    return this.InstanceHandle.Owner.OwnerToken;
                }
                return Guid.Empty;
            }
        }

        private ExecuteAsyncResult RootAsyncResult { get; set; }

        internal System.Transactions.Transaction Transaction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transaction;
            }
        }

        public object UserContext
        {
            get
            {
                return this.InstanceHandle.ProviderObject;
            }
        }

        private class BindReclaimedLockAsyncResult : AsyncResult
        {
            private readonly InstancePersistenceContext context;
            private static Action<object, TimeoutException> waitComplete = new Action<object, TimeoutException>(InstancePersistenceContext.BindReclaimedLockAsyncResult.OnWaitComplete);

            public BindReclaimedLockAsyncResult(InstancePersistenceContext context, AsyncWaitHandle wait, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                if (wait.WaitAsync(waitComplete, this, timeout))
                {
                    this.context.ConcludeBindReclaimedLockHelper();
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<InstancePersistenceContext.BindReclaimedLockAsyncResult>(result);
            }

            private static void OnWaitComplete(object state, TimeoutException timeoutException)
            {
                InstancePersistenceContext.BindReclaimedLockAsyncResult result = (InstancePersistenceContext.BindReclaimedLockAsyncResult) state;
                Exception exception = null;
                try
                {
                    if (timeoutException != null)
                    {
                        result.context.InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
                    }
                    result.context.ConcludeBindReclaimedLockHelper();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }
        }

        [Serializable]
        private class BindReclaimedLockException : Exception
        {
            public BindReclaimedLockException()
            {
            }

            internal BindReclaimedLockException(AsyncWaitHandle markerWaitHandle) : base(SRCore.BindReclaimedLockException)
            {
                this.MarkerWaitHandle = markerWaitHandle;
            }

            [SecurityCritical]
            protected BindReclaimedLockException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            internal AsyncWaitHandle MarkerWaitHandle { get; private set; }
        }

        private class ExecuteAsyncResult : AsyncResult, ISinglePhaseNotification, IEnlistmentNotification
        {
            private Action<InstancePersistenceContext> cancellationHandler;
            private InstancePersistenceContext context;
            private IEnumerator<InstancePersistenceCommand> currentExecution;
            private bool executeCalledByCurrentCommand;
            private readonly Stack<IEnumerator<InstancePersistenceCommand>> executionStack;
            private InstanceView finalState;
            private bool inDoubt;
            private readonly InstanceHandle initialInstanceHandle;
            private static AsyncResult.AsyncCompletion onAcquireContext = new AsyncResult.AsyncCompletion(InstancePersistenceContext.ExecuteAsyncResult.OnAcquireContext);
            private static Action<object, TimeoutException> onBindReclaimed = new Action<object, TimeoutException>(InstancePersistenceContext.ExecuteAsyncResult.OnBindReclaimed);
            private static AsyncResult.AsyncCompletion onCommit = new AsyncResult.AsyncCompletion(InstancePersistenceContext.ExecuteAsyncResult.OnCommit);
            private static Action<object, TimeoutException> onCommitWait = new Action<object, TimeoutException>(InstancePersistenceContext.ExecuteAsyncResult.OnCommitWait);
            private static AsyncResult.AsyncCompletion onTryCommand = new AsyncResult.AsyncCompletion(InstancePersistenceContext.ExecuteAsyncResult.OnTryCommand);
            private readonly InstancePersistenceContext.ExecuteAsyncResult priorAsyncResult;
            private bool rolledBack;
            private readonly TimeoutHelper timeoutHelper;
            private CommittableTransaction transactionToCommit;
            private AsyncWaitHandle waitForTransaction;

            public ExecuteAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout) : this(command, timeout, null, null)
            {
                this.context = context;
                this.priorAsyncResult = this.context.LastAsyncResult;
                this.priorAsyncResult.executeCalledByCurrentCommand = true;
                bool flag = false;
                try
                {
                    this.context.LastAsyncResult = this;
                    this.RunLoopCore(true);
                    flag = true;
                }
                finally
                {
                    this.context.LastAsyncResult = this.priorAsyncResult;
                    if (!flag && this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                }
                base.Complete(true);
            }

            public ExecuteAsyncResult(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout) : this(command, timeout, null, null)
            {
                this.initialInstanceHandle = initialInstanceHandle;
                this.context = this.initialInstanceHandle.AcquireExecutionContext(transaction, this.timeoutHelper.RemainingTime());
                Exception exception = null;
                try
                {
                    this.context.RootAsyncResult = this;
                    this.context.LastAsyncResult = this;
                    base.OnCompleting = new Action<AsyncResult, Exception>(this.Cleanup);
                    this.RunLoopCore(true);
                    if (this.transactionToCommit != null)
                    {
                        try
                        {
                            this.transactionToCommit.Commit();
                        }
                        catch (TransactionException)
                        {
                        }
                        this.transactionToCommit = null;
                    }
                    this.DoWaitForTransaction(true);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                base.Complete(true, exception);
            }

            private ExecuteAsyncResult(InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.executionStack = new Stack<IEnumerator<InstancePersistenceCommand>>(2);
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.currentExecution = new List<InstancePersistenceCommand> { command }.GetEnumerator();
            }

            public ExecuteAsyncResult(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state) : this(command, timeout, callback, state)
            {
                this.context = context;
                this.priorAsyncResult = this.context.LastAsyncResult;
                this.priorAsyncResult.executeCalledByCurrentCommand = true;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.SimpleCleanup);
                bool flag = false;
                bool flag2 = false;
                try
                {
                    this.context.LastAsyncResult = this;
                    if (this.RunLoop())
                    {
                        flag = true;
                    }
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.context.LastAsyncResult = this.priorAsyncResult;
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            public ExecuteAsyncResult(InstanceHandle initialInstanceHandle, InstancePersistenceCommand command, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state) : this(command, timeout, callback, state)
            {
                this.initialInstanceHandle = initialInstanceHandle;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.SimpleCleanup);
                IAsyncResult result = this.initialInstanceHandle.BeginAcquireExecutionContext(transaction, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(onAcquireContext), this);
                if (result.CompletedSynchronously)
                {
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = OnAcquireContext(result);
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
                        base.Complete(true, exception);
                    }
                }
            }

            private void AfterCommand(bool commandProcessed)
            {
                if (!object.ReferenceEquals(this.context.LastAsyncResult, this))
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ExecuteMustBeNested));
                }
                if (!commandProcessed)
                {
                    if (this.executeCalledByCurrentCommand)
                    {
                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.TryCommandCannotExecuteSubCommandsAndReduce));
                    }
                    IEnumerable<InstancePersistenceCommand> enumerable = this.CurrentCommand.Reduce(this.context.InstanceView);
                    if (enumerable == null)
                    {
                        throw Fx.Exception.AsError(new NotSupportedException(SRCore.ProviderDoesNotSupportCommand(this.CurrentCommand.Name)));
                    }
                    this.executionStack.Push(this.currentExecution);
                    this.currentExecution = enumerable.GetEnumerator();
                }
            }

            private Exception AfterCommitWait()
            {
                if (this.inDoubt)
                {
                    this.context.InstanceHandle.Free();
                    return new TransactionInDoubtException(SRCore.TransactionInDoubtNonHost);
                }
                if (this.rolledBack)
                {
                    if (this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                    return new TransactionAbortedException(SRCore.TransactionRolledBackNonHost);
                }
                if (this.finalState == null)
                {
                    this.context.InstanceHandle.Free();
                    return new InstanceHandleConflictException(null, this.context.InstanceView.InstanceId);
                }
                return null;
            }

            private void BindReclaimed(bool timedOut)
            {
                if (timedOut)
                {
                    this.context.InstanceHandle.CancelReclaim(new TimeoutException(SRCore.TimedOutWaitingForLockResolution));
                }
                this.context.ConcludeBindReclaimedLockHelper();
                this.context.InstanceHandle.Free();
                throw Fx.Exception.AsError(new OperationCanceledException(SRCore.BindReclaimSucceeded));
            }

            private void Cleanup(AsyncResult result, Exception exception)
            {
                try
                {
                    this.SimpleCleanup(result, exception);
                    if (this.transactionToCommit != null)
                    {
                        try
                        {
                            this.transactionToCommit.Rollback(exception);
                        }
                        catch (TransactionException)
                        {
                        }
                    }
                }
                finally
                {
                    Fx.AssertAndThrowFatal(this.context.Active, "Out-of-sync between InstanceExecutionContext and ExecutionAsyncResult.");
                    this.context.LastAsyncResult = null;
                    this.context.RootAsyncResult = null;
                    this.context.InstanceHandle.ReleaseExecutionContext();
                }
            }

            private void CommitHelper()
            {
                this.finalState = this.context.InstanceHandle.Commit(this.context.InstanceView);
            }

            private bool DoEndCommand(IAsyncResult result)
            {
                bool flag;
                InstancePersistenceContext.BindReclaimedLockException exception = null;
                try
                {
                    flag = this.context.InstanceHandle.Store.EndTryCommand(result);
                }
                catch (InstancePersistenceContext.BindReclaimedLockException exception2)
                {
                    exception = exception2;
                    flag = true;
                }
                this.AfterCommand(flag);
                if (exception != null)
                {
                    if (!exception.MarkerWaitHandle.WaitAsync(onBindReclaimed, this, this.timeoutHelper.RemainingTime()))
                    {
                        return false;
                    }
                    this.BindReclaimed(false);
                }
                return true;
            }

            private bool DoWaitForTransaction(bool synchronous)
            {
                if (this.waitForTransaction != null)
                {
                    if (synchronous)
                    {
                        TimeSpan timeout = this.timeoutHelper.RemainingTime();
                        if (!this.waitForTransaction.Wait(timeout))
                        {
                            throw Fx.Exception.AsError(new TimeoutException(SRCore.TimeoutOnOperation(timeout)));
                        }
                    }
                    else if (!this.waitForTransaction.WaitAsync(onCommitWait, this, this.timeoutHelper.RemainingTime()))
                    {
                        return false;
                    }
                    Exception exception = this.AfterCommitWait();
                    if (exception != null)
                    {
                        throw Fx.Exception.AsError(exception);
                    }
                }
                else if (this.context.IsHostTransaction)
                {
                    this.finalState = this.context.InstanceView.Clone();
                    this.finalState.MakeReadOnly();
                    this.context.InstanceView.InstanceStoreQueryResults = null;
                }
                else
                {
                    this.CommitHelper();
                    if (this.finalState == null)
                    {
                        this.context.InstanceHandle.Free();
                        throw Fx.Exception.AsError(new InstanceHandleConflictException(null, this.context.InstanceView.InstanceId));
                    }
                }
                return true;
            }

            public static InstanceView End(IAsyncResult result)
            {
                return AsyncResult.End<InstancePersistenceContext.ExecuteAsyncResult>(result).finalState;
            }

            private static bool OnAcquireContext(IAsyncResult result)
            {
                InstancePersistenceContext.ExecuteAsyncResult asyncState = (InstancePersistenceContext.ExecuteAsyncResult) result.AsyncState;
                asyncState.context = asyncState.initialInstanceHandle.EndAcquireExecutionContext(result);
                asyncState.context.RootAsyncResult = asyncState;
                asyncState.context.LastAsyncResult = asyncState;
                asyncState.OnCompleting = new Action<AsyncResult, Exception>(asyncState.Cleanup);
                return asyncState.RunLoop();
            }

            private static void OnBindReclaimed(object state, TimeoutException timeoutException)
            {
                bool flag;
                InstancePersistenceContext.ExecuteAsyncResult result = (InstancePersistenceContext.ExecuteAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.BindReclaimed(timeoutException != null);
                    flag = result.RunLoop();
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

            private static bool OnCommit(IAsyncResult result)
            {
                InstancePersistenceContext.ExecuteAsyncResult asyncState = (InstancePersistenceContext.ExecuteAsyncResult) result.AsyncState;
                try
                {
                    asyncState.transactionToCommit.EndCommit(result);
                }
                catch (TransactionException)
                {
                }
                asyncState.transactionToCommit = null;
                return asyncState.DoWaitForTransaction(false);
            }

            private static void OnCommitWait(object state, TimeoutException exception)
            {
                InstancePersistenceContext.ExecuteAsyncResult result = (InstancePersistenceContext.ExecuteAsyncResult) state;
                result.Complete(false, exception ?? result.AfterCommitWait());
            }

            private static bool OnTryCommand(IAsyncResult result)
            {
                InstancePersistenceContext.ExecuteAsyncResult asyncState = (InstancePersistenceContext.ExecuteAsyncResult) result.AsyncState;
                return (asyncState.DoEndCommand(result) && asyncState.RunLoop());
            }

            private bool RunLoop()
            {
                if (!this.RunLoopCore(false))
                {
                    return false;
                }
                if (this.initialInstanceHandle == null)
                {
                    return true;
                }
                if (this.transactionToCommit != null)
                {
                    IAsyncResult result = null;
                    try
                    {
                        result = this.transactionToCommit.BeginCommit(base.PrepareAsyncCompletion(onCommit), this);
                    }
                    catch (TransactionException)
                    {
                        this.transactionToCommit = null;
                    }
                    if (result != null)
                    {
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        return OnCommit(result);
                    }
                }
                return this.DoWaitForTransaction(false);
            }

            private bool RunLoopCore(bool synchronous)
            {
                while (this.currentExecution != null)
                {
                    if (this.currentExecution.MoveNext())
                    {
                        bool flag = this.CurrentCommand == null;
                        this.executeCalledByCurrentCommand = false;
                        this.CurrentCommand = this.currentExecution.Current;
                        if (flag)
                        {
                            if (((this.priorAsyncResult != null) && this.priorAsyncResult.CurrentCommand.IsTransactionEnlistmentOptional) && !this.CurrentCommand.IsTransactionEnlistmentOptional)
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeTransactionalFromNonTransactional));
                            }
                        }
                        else if (this.executionStack.Peek().Current.IsTransactionEnlistmentOptional)
                        {
                            if (!this.CurrentCommand.IsTransactionEnlistmentOptional)
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeTransactionalFromNonTransactional));
                            }
                        }
                        else if (this.priorAsyncResult == null)
                        {
                            this.context.RequireTransaction();
                        }
                        bool automaticallyAcquiringLock = this.CurrentCommand.AutomaticallyAcquiringLock;
                        this.CurrentCommand.Validate(this.context.InstanceView);
                        if (automaticallyAcquiringLock)
                        {
                            if (flag)
                            {
                                if (this.priorAsyncResult != null)
                                {
                                    if (!this.priorAsyncResult.CurrentCommand.AutomaticallyAcquiringLock)
                                    {
                                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeBindingFromNonBinding));
                                    }
                                }
                                else
                                {
                                    if (!this.context.InstanceView.IsBoundToInstanceOwner)
                                    {
                                        throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MayBindLockCommandShouldValidateOwner));
                                    }
                                    if (!this.context.InstanceView.IsBoundToLock)
                                    {
                                        this.context.InstanceHandle.StartPotentialBind();
                                    }
                                }
                            }
                            else if (!this.executionStack.Peek().Current.AutomaticallyAcquiringLock)
                            {
                                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.CannotInvokeBindingFromNonBinding));
                            }
                        }
                        if (this.context.CancelRequested)
                        {
                            throw Fx.Exception.AsError(new OperationCanceledException(SRCore.HandleFreed));
                        }
                        InstancePersistenceContext.BindReclaimedLockException exception = null;
                        if (!synchronous)
                        {
                            IAsyncResult result;
                            IDisposable disposable = base.PrepareTransactionalCall(this.context.Transaction);
                            try
                            {
                                result = this.context.InstanceHandle.Store.BeginTryCommand(this.context, this.CurrentCommand, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(onTryCommand), this);
                            }
                            catch (InstancePersistenceContext.BindReclaimedLockException exception3)
                            {
                                exception = exception3;
                                result = null;
                            }
                            finally
                            {
                                if (disposable != null)
                                {
                                    disposable.Dispose();
                                }
                            }
                            if (result != null)
                            {
                                if (!base.CheckSyncContinue(result) || !this.DoEndCommand(result))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                this.AfterCommand(true);
                                if (!exception.MarkerWaitHandle.WaitAsync(onBindReclaimed, this, this.timeoutHelper.RemainingTime()))
                                {
                                    return false;
                                }
                                this.BindReclaimed(false);
                            }
                        }
                        else
                        {
                            bool flag3;
                            TransactionScope scope = null;
                            try
                            {
                                scope = Fx.CreateTransactionScope(this.context.Transaction);
                                flag3 = this.context.InstanceHandle.Store.TryCommand(this.context, this.CurrentCommand, this.timeoutHelper.RemainingTime());
                            }
                            catch (InstancePersistenceContext.BindReclaimedLockException exception2)
                            {
                                exception = exception2;
                                flag3 = true;
                            }
                            finally
                            {
                                Fx.CompleteTransactionScope(ref scope);
                            }
                            this.AfterCommand(flag3);
                            if (exception != null)
                            {
                                this.BindReclaimed(!exception.MarkerWaitHandle.Wait(this.timeoutHelper.RemainingTime()));
                            }
                        }
                    }
                    else if (this.executionStack.Count > 0)
                    {
                        this.currentExecution = this.executionStack.Pop();
                    }
                    else
                    {
                        this.currentExecution = null;
                    }
                }
                this.CurrentCommand = null;
                return true;
            }

            public void SetInteriorTransaction(Transaction interiorTransaction, bool needsCommit)
            {
                if (this.waitForTransaction != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ExecuteMustBeNested));
                }
                bool flag = false;
                try
                {
                    this.waitForTransaction = new AsyncWaitHandle(EventResetMode.ManualReset);
                    interiorTransaction.EnlistVolatile((ISinglePhaseNotification) this, EnlistmentOptions.None);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        if (this.waitForTransaction != null)
                        {
                            this.waitForTransaction.Set();
                        }
                    }
                    else if (needsCommit)
                    {
                        this.transactionToCommit = (CommittableTransaction) interiorTransaction;
                    }
                }
            }

            private void SimpleCleanup(AsyncResult result, Exception exception)
            {
                if (this.initialInstanceHandle == null)
                {
                    this.context.LastAsyncResult = this.priorAsyncResult;
                }
                if (exception != null)
                {
                    if ((this.context != null) && this.context.IsHandleDoomedByRollback)
                    {
                        this.context.InstanceHandle.Free();
                    }
                    else if ((exception is TimeoutException) || (exception is OperationCanceledException))
                    {
                        if (this.context == null)
                        {
                            this.initialInstanceHandle.Free();
                        }
                        else
                        {
                            this.context.InstanceHandle.Free();
                        }
                    }
                }
            }

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                this.CommitHelper();
                enlistment.Done();
                this.waitForTransaction.Set();
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.inDoubt = true;
                this.waitForTransaction.Set();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.rolledBack = true;
                this.waitForTransaction.Set();
            }

            void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
            {
                this.CommitHelper();
                singlePhaseEnlistment.Committed();
                this.waitForTransaction.Set();
            }

            internal Action<InstancePersistenceContext> CancellationHandler
            {
                get
                {
                    Action<InstancePersistenceContext> cancellationHandler = this.cancellationHandler;
                    InstancePersistenceContext.ExecuteAsyncResult priorAsyncResult = this;
                    while (cancellationHandler == null)
                    {
                        priorAsyncResult = priorAsyncResult.priorAsyncResult;
                        if (priorAsyncResult == null)
                        {
                            return cancellationHandler;
                        }
                        cancellationHandler = priorAsyncResult.cancellationHandler;
                    }
                    return cancellationHandler;
                }
                set
                {
                    this.cancellationHandler = value;
                }
            }

            internal InstancePersistenceCommand CurrentCommand { get; private set; }
        }
    }
}

