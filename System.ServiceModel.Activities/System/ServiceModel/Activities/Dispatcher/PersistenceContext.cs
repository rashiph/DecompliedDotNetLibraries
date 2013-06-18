namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities.DurableInstancing;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.Transactions;
    using System.Xml.Linq;

    internal sealed class PersistenceContext : CommunicationObject
    {
        private static TimeSpan defaultCloseTimeout = TimeSpan.FromSeconds(90.0);
        private static TimeSpan defaultOpenTimeout = TimeSpan.FromSeconds(90.0);
        private readonly PersistenceProviderDirectory directory;
        internal static Dictionary<int, PersistenceContextEnlistment> Enlistments = new Dictionary<int, PersistenceContextEnlistment>();
        private readonly InstanceHandle handle;
        private readonly HashSet<InstanceKey> keysToAssociate;
        private readonly HashSet<InstanceKey> keysToDisassociate;
        private int lockingTransaction;
        private Transaction lockingTransactionObject;
        private bool operationInProgress;
        private readonly InstanceStore store;
        private Queue<TransactionWaitAsyncResult> transactionWaiterQueue;
        private WorkflowServiceInstance workflowInstance;

        internal PersistenceContext(PersistenceProviderDirectory directory, Guid instanceId, InstanceKey key, IEnumerable<InstanceKey> associatedKeys)
        {
            this.directory = directory;
            this.InstanceId = instanceId;
            this.AssociatedKeys = (associatedKeys != null) ? new HashSet<InstanceKey>(associatedKeys) : new HashSet<InstanceKey>();
            if ((key != null) && !this.AssociatedKeys.Contains(key))
            {
                this.AssociatedKeys.Add(key);
            }
            this.keysToAssociate = new HashSet<InstanceKey>(this.AssociatedKeys);
            this.keysToDisassociate = new HashSet<InstanceKey>();
            this.lockingTransaction = 0;
            this.Detaching = false;
            this.transactionWaiterQueue = new Queue<TransactionWaitAsyncResult>();
        }

        internal PersistenceContext(PersistenceProviderDirectory directory, InstanceStore store, InstanceHandle handle, Guid instanceId, IEnumerable<InstanceKey> associatedKeys, bool newInstance, bool locked, InstanceView view) : this(directory, instanceId, null, associatedKeys)
        {
            this.store = store;
            this.handle = handle;
            this.IsInitialized = !newInstance;
            this.IsLocked = locked;
            if (view != null)
            {
                this.ReadSuspendedInfo(view);
            }
            if (this.IsInitialized || this.IsLocked)
            {
                this.RationalizeSavedKeys(false);
            }
            if (this.IsInitialized)
            {
                this.workflowInstance = this.directory.InitializeInstance(this.InstanceId, this, view.InstanceData, null);
            }
        }

        internal IAsyncResult BeginAssociateInfrastructureKeys(ICollection<InstanceKey> associatedKeys, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginAssociateKeysHelper(associatedKeys, timeout, true, callback, state);
        }

        public IAsyncResult BeginAssociateKeys(ICollection<InstanceKey> associatedKeys, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginAssociateKeysHelper(associatedKeys, timeout, true, callback, state);
        }

        private IAsyncResult BeginAssociateKeysHelper(ICollection<InstanceKey> associatedKeys, TimeSpan timeout, bool applicationKeys, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new AssociateKeysAsyncResult(this, associatedKeys, timeout, applicationKeys, callback, state);
        }

        internal TransactionWaitAsyncResult BeginEnlist(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new TransactionWaitAsyncResult(Transaction.Current, this, timeout, callback, state);
        }

        public IAsyncResult BeginRelease(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new ReleaseAsyncResult(this, timeout, callback, state);
        }

        public IAsyncResult BeginSave(IDictionary<XName, InstanceValue> instance, SaveStatus saveStatus, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            Fx.AssertAndThrow(instance != null, "'instance' parameter to BeginSave cannot be null.");
            return new SaveAsyncResult(this, instance, saveStatus, timeout, callback, state);
        }

        public IAsyncResult BeginUpdateSuspendMetadata(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new UpdateSuspendMetadataAsyncResult(this, reason, timeout, callback, state);
        }

        public void DisassociateKeys(ICollection<InstanceKey> expiredKeys)
        {
            base.ThrowIfDisposedOrNotOpen();
            try
            {
                this.StartOperation();
                this.ThrowIfCompleted();
                this.ThrowIfNotVisible();
                foreach (InstanceKey key in expiredKeys)
                {
                    if (this.AssociatedKeys.Contains(key) && !this.keysToDisassociate.Contains(key))
                    {
                        this.keysToDisassociate.Add(key);
                        this.keysToAssociate.Remove(key);
                    }
                }
            }
            finally
            {
                this.FinishOperation();
            }
        }

        internal void EndAssociateInfrastructureKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        public void EndAssociateKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        internal void EndEnlist(IAsyncResult result)
        {
            TransactionWaitAsyncResult.End(result);
            base.ThrowIfDisposedOrNotOpen();
        }

        public void EndRelease(IAsyncResult result)
        {
            ReleaseAsyncResult.End(result);
        }

        public void EndSave(IAsyncResult result)
        {
            SaveAsyncResult.End(result);
        }

        public void EndUpdateSuspendMetadata(IAsyncResult result)
        {
            UpdateSuspendMetadataAsyncResult.End(result);
        }

        private void FinishOperation()
        {
            this.operationInProgress = false;
        }

        public WorkflowServiceInstance GetInstance(WorkflowGetInstanceContext parameters)
        {
            if ((this.workflowInstance == null) && (parameters != null))
            {
                lock (base.ThisLock)
                {
                    base.ThrowIfDisposedOrNotOpen();
                    if (this.workflowInstance == null)
                    {
                        try
                        {
                            WorkflowServiceInstance instance;
                            if (parameters.WorkflowHostingEndpoint != null)
                            {
                                WorkflowHostingResponseContext responseContext = new WorkflowHostingResponseContext();
                                WorkflowCreationContext creationContext = parameters.WorkflowHostingEndpoint.OnGetCreationContext(parameters.Inputs, parameters.OperationContext, this.InstanceId, responseContext);
                                if (creationContext == null)
                                {
                                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(WorkflowHostingEndpoint.CreateDispatchFaultException());
                                }
                                instance = this.directory.InitializeInstance(this.InstanceId, this, null, creationContext);
                                parameters.WorkflowCreationContext = creationContext;
                                parameters.WorkflowHostingResponseContext = responseContext;
                            }
                            else
                            {
                                instance = this.directory.InitializeInstance(this.InstanceId, this, null, null);
                            }
                            this.workflowInstance = instance;
                        }
                        finally
                        {
                            if (this.workflowInstance == null)
                            {
                                base.Fault();
                            }
                        }
                    }
                }
            }
            return this.workflowInstance;
        }

        protected override void OnAbort()
        {
            if (this.handle != null)
            {
                this.handle.Free();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                this.StartOperation();
                if (this.store != null)
                {
                    this.handle.Free();
                }
            }
            finally
            {
                this.FinishOperation();
            }
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.directory.RemoveInstance(this, true);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.directory.RemoveInstance(this, true);
        }

        private void OnFinishOperationHelper(Exception exception, bool ownsThrottle)
        {
            try
            {
                if (exception is OperationCanceledException)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.HandleFreedInDirectory, exception));
                }
                if (exception is TimeoutException)
                {
                    base.Fault();
                }
            }
            finally
            {
                if (ownsThrottle)
                {
                    this.directory.ReleaseThrottle();
                }
                this.FinishOperation();
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        private void PopulateActivationMetadata(SaveWorkflowCommand saveCommand)
        {
            if (!this.IsInitialized)
            {
                foreach (KeyValuePair<XName, InstanceValue> pair in this.directory.InstanceMetadataChanges)
                {
                    saveCommand.InstanceMetadataChanges.Add(pair.Key, pair.Value);
                }
            }
        }

        internal bool QueueForTransactionLock(Transaction requestingTransaction, TransactionWaitAsyncResult txWaitAsyncResult)
        {
            lock (base.ThisLock)
            {
                if (this.lockingTransaction == 0)
                {
                    if (null != requestingTransaction)
                    {
                        this.lockingTransaction = requestingTransaction.GetHashCode();
                        this.lockingTransactionObject = requestingTransaction.Clone();
                    }
                    return true;
                }
                if ((null != requestingTransaction) && (this.lockingTransaction == requestingTransaction.GetHashCode()))
                {
                    return true;
                }
                this.transactionWaiterQueue.Enqueue(txWaitAsyncResult);
                return false;
            }
        }

        private void RationalizeSavedKeys(bool updateDirectory)
        {
            if (updateDirectory)
            {
                this.directory.RemoveAssociations(this, this.keysToDisassociate);
            }
            else
            {
                foreach (InstanceKey key in this.keysToDisassociate)
                {
                    this.AssociatedKeys.Remove(key);
                }
            }
            this.keysToAssociate.Clear();
            this.keysToDisassociate.Clear();
        }

        private void ReadSuspendedInfo(InstanceView view)
        {
            string str = null;
            if (TryGetValue<string>(view.InstanceMetadata, WorkflowServiceNamespace.SuspendReason, out str))
            {
                this.IsSuspended = true;
                this.SuspendedReason = str;
            }
            else
            {
                this.IsSuspended = false;
                this.SuspendedReason = null;
            }
        }

        private bool ScheduleDetach()
        {
            lock (base.ThisLock)
            {
                if (this.lockingTransaction != 0)
                {
                    this.Detaching = true;
                    return true;
                }
            }
            return false;
        }

        internal void ScheduleNextTransactionWaiter()
        {
            TransactionWaitAsyncResult result = null;
            bool flag = false;
            lock (base.ThisLock)
            {
                bool flag2 = false;
                if (0 < this.transactionWaiterQueue.Count)
                {
                    while ((0 < this.transactionWaiterQueue.Count) && !flag2)
                    {
                        result = this.transactionWaiterQueue.Dequeue();
                        if (null != result.Transaction)
                        {
                            this.lockingTransactionObject = result.Transaction;
                            this.lockingTransaction = this.lockingTransactionObject.GetHashCode();
                        }
                        else
                        {
                            this.lockingTransaction = 0;
                            this.lockingTransactionObject = null;
                        }
                        flag2 = result.Complete() || flag2;
                        if (this.Detaching)
                        {
                            flag = true;
                            this.Detaching = false;
                        }
                        if (this.IsPermanentlyRemoved)
                        {
                            this.lockingTransaction = 0;
                            this.lockingTransactionObject = null;
                            while (0 < this.transactionWaiterQueue.Count)
                            {
                                flag2 = this.transactionWaiterQueue.Dequeue().Complete() || flag2;
                            }
                        }
                        while (0 < this.transactionWaiterQueue.Count)
                        {
                            TransactionWaitAsyncResult result2 = this.transactionWaiterQueue.Peek();
                            if (this.lockingTransaction == 0)
                            {
                                if (null != result2.Transaction)
                                {
                                    this.lockingTransactionObject = result2.Transaction;
                                    this.lockingTransaction = this.lockingTransactionObject.GetHashCode();
                                }
                            }
                            else if ((null == result2.Transaction) || (this.lockingTransaction != result2.Transaction.GetHashCode()))
                            {
                                continue;
                            }
                            flag2 = this.transactionWaiterQueue.Dequeue().Complete() || flag2;
                        }
                    }
                }
                if (!flag2)
                {
                    this.lockingTransaction = 0;
                    this.lockingTransactionObject = null;
                }
            }
            if (flag)
            {
                this.directory.RemoveInstance(this, false);
            }
        }

        private void StartOperation()
        {
            Fx.AssertAndThrow(!this.operationInProgress, "PersistenceContext doesn't support multiple operations.");
            this.operationInProgress = true;
        }

        private void ThrowIfCompleted()
        {
            Fx.AssertAndThrow(!this.IsCompleted, "PersistenceContext operation invalid: instance already completed.");
        }

        private void ThrowIfNotVisible()
        {
            if (!this.IsVisible)
            {
                lock (base.ThisLock)
                {
                    Fx.AssertAndThrow(base.State != CommunicationState.Opened, "PersistenceContext operation invalid: instance must be visible.");
                }
            }
        }

        internal static bool TryGetValue<T>(IDictionary<XName, InstanceValue> data, XName key, out T value)
        {
            InstanceValue value2;
            value = default(T);
            if (!data.TryGetValue(key, out value2) || value2.IsDeletedValue)
            {
                return false;
            }
            if (value2.Value is T)
            {
                value = (T) value2.Value;
                return true;
            }
            if ((value2.Value == null) && !(((T) value) is ValueType))
            {
                return true;
            }
            if (value2.Value == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceException(SRCore.NullAssignedToValueType(typeof(T))));
            }
            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceException(SRCore.IncorrectValueType(typeof(T), value2.Value.GetType())));
        }

        internal HashSet<InstanceKey> AssociatedKeys { get; private set; }

        internal ReadOnlyCollection<BookmarkInfo> Bookmarks { get; set; }

        public bool CanPersist
        {
            get
            {
                return (this.store != null);
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return defaultCloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return defaultOpenTimeout;
            }
        }

        internal bool Detaching { get; set; }

        public Guid InstanceId { get; private set; }

        public bool IsCompleted { get; private set; }

        public bool IsHandleValid
        {
            get
            {
                if (this.handle != null)
                {
                    return this.handle.IsValid;
                }
                return true;
            }
        }

        public bool IsInitialized { get; private set; }

        public bool IsLocked { get; private set; }

        internal bool IsPermanentlyRemoved { get; set; }

        public bool IsSuspended { get; set; }

        public bool IsVisible { get; internal set; }

        internal Transaction LockingTransaction
        {
            get
            {
                lock (base.ThisLock)
                {
                    base.ThrowIfDisposedOrNotOpen();
                    return this.lockingTransactionObject;
                }
            }
        }

        public string SuspendedReason { get; set; }

        private class AssociateKeysAsyncResult : AsyncResult
        {
            private readonly bool applicationKeys;
            private static readonly AsyncResult.AsyncCompletion handleEndEnlist = new AsyncResult.AsyncCompletion(PersistenceContext.AssociateKeysAsyncResult.HandleEndEnlist);
            private static readonly AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(PersistenceContext.AssociateKeysAsyncResult.HandleEndExecute);
            private readonly ICollection<InstanceKey> keysToAssociate;
            private readonly PersistenceContext persistenceContext;
            private readonly TimeoutHelper timeoutHelper;
            private readonly DependentTransaction transaction;

            public AssociateKeysAsyncResult(PersistenceContext persistenceContext, ICollection<InstanceKey> associatedKeys, TimeSpan timeout, bool applicationKeys, AsyncCallback callback, object state) : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                this.applicationKeys = applicationKeys;
                this.keysToAssociate = associatedKeys;
                this.timeoutHelper = new TimeoutHelper(timeout);
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnFinishOperation);
                bool flag = false;
                try
                {
                    this.persistenceContext.StartOperation();
                    this.persistenceContext.ThrowIfCompleted();
                    this.persistenceContext.ThrowIfNotVisible();
                    Transaction current = Transaction.Current;
                    if (current != null)
                    {
                        this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }
                    IAsyncResult result = persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndEnlist), this);
                    if (base.SyncContinue(result))
                    {
                        base.Complete(true);
                    }
                    flag = true;
                }
                catch (InstancePersistenceException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                catch (OperationCanceledException exception)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.HandleFreedInDirectory, exception));
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            private bool AfterUpdate()
            {
                if (this.applicationKeys)
                {
                    this.persistenceContext.RationalizeSavedKeys(true);
                }
                else
                {
                    this.persistenceContext.keysToAssociate.Clear();
                }
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistenceContext.AssociateKeysAsyncResult>(result);
            }

            private static bool HandleEndEnlist(IAsyncResult result)
            {
                PersistenceContext.AssociateKeysAsyncResult asyncState = (PersistenceContext.AssociateKeysAsyncResult) result.AsyncState;
                bool flag = false;
                if (!asyncState.persistenceContext.directory.TryAddAssociations(asyncState.persistenceContext, asyncState.keysToAssociate, asyncState.persistenceContext.keysToAssociate, asyncState.applicationKeys ? asyncState.persistenceContext.keysToDisassociate : null))
                {
                    lock (asyncState.persistenceContext.ThisLock)
                    {
                        asyncState.persistenceContext.ThrowIfDisposedOrNotOpen();
                    }
                    throw Fx.AssertAndThrow("Should only fail to add keys in a race with abort.");
                }
                if (asyncState.persistenceContext.directory.ConsistencyScope == DurableConsistencyScope.Global)
                {
                    if ((asyncState.persistenceContext.keysToAssociate.Count == 0) && ((asyncState.persistenceContext.keysToDisassociate.Count == 0) || !asyncState.applicationKeys))
                    {
                        return asyncState.AfterUpdate();
                    }
                    if (asyncState.persistenceContext.store == null)
                    {
                        return flag;
                    }
                    SaveWorkflowCommand command = new SaveWorkflowCommand();
                    foreach (InstanceKey key in asyncState.persistenceContext.keysToAssociate)
                    {
                        command.InstanceKeysToAssociate.Add(key.Value, key.Metadata);
                    }
                    if (asyncState.applicationKeys)
                    {
                        foreach (InstanceKey key2 in asyncState.persistenceContext.keysToDisassociate)
                        {
                            command.InstanceKeysToFree.Add(key2.Value);
                        }
                    }
                    IAsyncResult result3 = null;
                    using (asyncState.PrepareTransactionalCall(asyncState.transaction))
                    {
                        result3 = asyncState.persistenceContext.store.BeginExecute(asyncState.persistenceContext.handle, command, asyncState.timeoutHelper.RemainingTime(), asyncState.PrepareAsyncCompletion(handleEndExecute), asyncState);
                    }
                    return asyncState.SyncContinue(result3);
                }
                return asyncState.AfterUpdate();
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                PersistenceContext.AssociateKeysAsyncResult asyncState = (PersistenceContext.AssociateKeysAsyncResult) result.AsyncState;
                asyncState.persistenceContext.store.EndExecute(result);
                return asyncState.AfterUpdate();
            }

            private void OnFinishOperation(AsyncResult result, Exception exception)
            {
                if (exception is InstancePersistenceException)
                {
                    this.persistenceContext.Fault();
                }
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private PersistenceContext persistenceContext;

            public CloseAsyncResult(PersistenceContext persistenceContext, AsyncCallback callback, object state) : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnFinishOperation);
                bool flag = false;
                bool flag2 = false;
                try
                {
                    this.persistenceContext.StartOperation();
                    if (this.persistenceContext.store != null)
                    {
                        this.persistenceContext.handle.Free();
                    }
                    flag2 = true;
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.persistenceContext.FinishOperation();
                    }
                }
                if (flag2)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistenceContext.CloseAsyncResult>(result);
            }

            private void OnFinishOperation(AsyncResult result, Exception exception)
            {
                this.persistenceContext.FinishOperation();
            }
        }

        private class ReleaseAsyncResult : AsyncResult
        {
            private static readonly AsyncResult.AsyncCompletion handleEndEnlist = new AsyncResult.AsyncCompletion(PersistenceContext.ReleaseAsyncResult.HandleEndEnlist);
            private static readonly AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(PersistenceContext.ReleaseAsyncResult.HandleEndExecute);
            private readonly PersistenceContext persistenceContext;
            private readonly TimeoutHelper timeoutHelper;
            private readonly DependentTransaction transaction;

            public ReleaseAsyncResult(PersistenceContext persistenceContext, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnFinishOperation);
                bool flag = false;
                try
                {
                    this.persistenceContext.StartOperation();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    Transaction current = Transaction.Current;
                    if (current != null)
                    {
                        this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }
                    if (this.persistenceContext.IsVisible)
                    {
                        if ((this.persistenceContext.store != null) && this.persistenceContext.IsLocked)
                        {
                            SaveWorkflowCommand saveCommand = new SaveWorkflowCommand {
                                UnlockInstance = true
                            };
                            this.persistenceContext.PopulateActivationMetadata(saveCommand);
                            IAsyncResult result = this.persistenceContext.store.BeginExecute(this.persistenceContext.handle, saveCommand, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndExecute), this);
                            if (base.SyncContinue(result))
                            {
                                base.Complete(true);
                            }
                        }
                        else if (this.AfterUnlock())
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        lock (this.persistenceContext.ThisLock)
                        {
                            this.persistenceContext.ThrowIfDisposedOrNotOpen();
                        }
                        base.Complete(true);
                    }
                    flag = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.HandleFreedInDirectory, exception));
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            private bool AfterUnlock()
            {
                IAsyncResult result;
                this.persistenceContext.IsLocked = false;
                using (base.PrepareTransactionalCall(this.transaction))
                {
                    result = this.persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndEnlist), this);
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistenceContext.ReleaseAsyncResult>(result);
            }

            private static bool HandleEndEnlist(IAsyncResult result)
            {
                PersistenceContext.ReleaseAsyncResult asyncState = (PersistenceContext.ReleaseAsyncResult) result.AsyncState;
                asyncState.persistenceContext.EndEnlist(result);
                if (!asyncState.persistenceContext.ScheduleDetach())
                {
                    asyncState.persistenceContext.directory.RemoveInstance(asyncState.persistenceContext);
                }
                foreach (InstanceKey key in asyncState.persistenceContext.keysToAssociate)
                {
                    asyncState.persistenceContext.AssociatedKeys.Remove(key);
                }
                asyncState.persistenceContext.keysToAssociate.Clear();
                asyncState.persistenceContext.keysToDisassociate.Clear();
                return true;
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                PersistenceContext.ReleaseAsyncResult asyncState = (PersistenceContext.ReleaseAsyncResult) result.AsyncState;
                asyncState.persistenceContext.store.EndExecute(result);
                return asyncState.AfterUnlock();
            }

            private void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        private class SaveAsyncResult : AsyncResult
        {
            private static readonly AsyncResult.AsyncCompletion handleEndEnlist = new AsyncResult.AsyncCompletion(PersistenceContext.SaveAsyncResult.HandleEndEnlist);
            private static readonly AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(PersistenceContext.SaveAsyncResult.HandleEndExecute);
            private readonly PersistenceContext persistenceContext;
            private readonly SaveStatus saveStatus;
            private readonly TimeoutHelper timeoutHelper;
            private readonly DependentTransaction transaction;

            public SaveAsyncResult(PersistenceContext persistenceContext, IDictionary<XName, InstanceValue> instance, SaveStatus saveStatus, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnFinishOperation);
                this.saveStatus = saveStatus;
                bool flag = false;
                try
                {
                    this.persistenceContext.StartOperation();
                    this.persistenceContext.ThrowIfCompleted();
                    this.persistenceContext.ThrowIfNotVisible();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    Transaction current = Transaction.Current;
                    if (current != null)
                    {
                        this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }
                    if (this.persistenceContext.store != null)
                    {
                        SaveWorkflowCommand saveCommand = new SaveWorkflowCommand();
                        foreach (KeyValuePair<XName, InstanceValue> pair in instance)
                        {
                            saveCommand.InstanceData.Add(pair);
                        }
                        this.persistenceContext.PopulateActivationMetadata(saveCommand);
                        if (this.persistenceContext.IsSuspended)
                        {
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendReason, new InstanceValue(this.persistenceContext.SuspendedReason));
                        }
                        else
                        {
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendReason, InstanceValue.DeletedValue);
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendException, InstanceValue.DeletedValue);
                        }
                        foreach (InstanceKey key in this.persistenceContext.keysToAssociate)
                        {
                            saveCommand.InstanceKeysToAssociate.Add(key.Value, key.Metadata);
                        }
                        foreach (InstanceKey key2 in this.persistenceContext.keysToDisassociate)
                        {
                            saveCommand.InstanceKeysToFree.Add(key2.Value);
                        }
                        if (this.saveStatus == SaveStatus.Completed)
                        {
                            saveCommand.CompleteInstance = true;
                            saveCommand.UnlockInstance = true;
                        }
                        else
                        {
                            saveCommand.UnlockInstance = this.saveStatus == SaveStatus.Unlocked;
                        }
                        IAsyncResult result = this.persistenceContext.store.BeginExecute(this.persistenceContext.handle, saveCommand, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndExecute), this);
                        if (base.SyncContinue(result))
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        if (this.saveStatus == SaveStatus.Completed)
                        {
                            this.persistenceContext.IsCompleted = true;
                            this.persistenceContext.IsLocked = false;
                        }
                        else
                        {
                            this.persistenceContext.IsLocked = this.saveStatus != SaveStatus.Unlocked;
                        }
                        if (this.AfterSave())
                        {
                            base.Complete(true);
                        }
                    }
                    flag = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.HandleFreedInDirectory, exception));
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            private bool AfterEnlist()
            {
                this.persistenceContext.RationalizeSavedKeys(this.saveStatus == SaveStatus.Locked);
                return true;
            }

            private bool AfterSave()
            {
                IAsyncResult result;
                this.persistenceContext.IsInitialized = true;
                if (this.saveStatus == SaveStatus.Locked)
                {
                    return this.AfterEnlist();
                }
                using (base.PrepareTransactionalCall(this.transaction))
                {
                    result = this.persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndEnlist), this);
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistenceContext.SaveAsyncResult>(result);
            }

            private static bool HandleEndEnlist(IAsyncResult result)
            {
                PersistenceContext.SaveAsyncResult asyncState = (PersistenceContext.SaveAsyncResult) result.AsyncState;
                asyncState.persistenceContext.EndEnlist(result);
                if (!asyncState.persistenceContext.ScheduleDetach())
                {
                    asyncState.persistenceContext.directory.RemoveInstance(asyncState.persistenceContext);
                }
                return asyncState.AfterEnlist();
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                PersistenceContext.SaveAsyncResult asyncState = (PersistenceContext.SaveAsyncResult) result.AsyncState;
                asyncState.persistenceContext.store.EndExecute(result);
                asyncState.persistenceContext.IsCompleted = asyncState.saveStatus == SaveStatus.Completed;
                asyncState.persistenceContext.IsLocked = asyncState.saveStatus == SaveStatus.Locked;
                return asyncState.AfterSave();
            }

            private void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        private class UpdateSuspendMetadataAsyncResult : AsyncResult
        {
            private static readonly AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(PersistenceContext.UpdateSuspendMetadataAsyncResult.HandleEndExecute);
            private readonly PersistenceContext persistenceContext;
            private readonly TimeoutHelper timeoutHelper;
            private readonly DependentTransaction transaction;

            public UpdateSuspendMetadataAsyncResult(PersistenceContext persistenceContext, Exception reason, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                base.OnCompleting = new Action<AsyncResult, Exception>(this.OnFinishOperation);
                bool flag = false;
                try
                {
                    this.persistenceContext.StartOperation();
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    Transaction current = Transaction.Current;
                    if (current != null)
                    {
                        this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }
                    if (this.persistenceContext.store != null)
                    {
                        SaveWorkflowCommand saveCommand = new SaveWorkflowCommand();
                        this.persistenceContext.PopulateActivationMetadata(saveCommand);
                        saveCommand.InstanceMetadataChanges[WorkflowServiceNamespace.SuspendReason] = new InstanceValue(reason.Message);
                        saveCommand.InstanceMetadataChanges[WorkflowServiceNamespace.SuspendException] = new InstanceValue(reason, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                        saveCommand.UnlockInstance = true;
                        IAsyncResult result = this.persistenceContext.store.BeginExecute(this.persistenceContext.handle, saveCommand, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndExecute), this);
                        if (base.SyncContinue(result))
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        base.Complete(true);
                    }
                    flag = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.HandleFreedInDirectory, exception));
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!flag)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistenceContext.UpdateSuspendMetadataAsyncResult>(result);
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                PersistenceContext.UpdateSuspendMetadataAsyncResult asyncState = (PersistenceContext.UpdateSuspendMetadataAsyncResult) result.AsyncState;
                asyncState.persistenceContext.store.EndExecute(result);
                return true;
            }

            private void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }
    }
}

