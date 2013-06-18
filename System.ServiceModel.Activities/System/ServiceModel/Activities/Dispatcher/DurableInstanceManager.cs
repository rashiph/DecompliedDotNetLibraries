namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities.DurableInstancing;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    internal sealed class DurableInstanceManager
    {
        private CreateWorkflowOwnerCommand createOwnerCommand;
        private InstanceHandle handle;
        private IDictionary<XName, InstanceValue> instanceMetadataChanges;
        private InstanceOwner owner;
        private int state;
        private System.Runtime.DurableInstancing.InstanceStore store;
        private object thisLock;
        private static AsyncCallback waitAndHandleStoreEventsCallback = Fx.ThunkCallback(new AsyncCallback(DurableInstanceManager.WaitAndHandleStoreEventsCallback));
        private AsyncWaitHandle waitForStoreEventsLoop;

        internal DurableInstanceManager(WorkflowServiceHost host)
        {
            this.DurableInstancingOptions = new System.ServiceModel.Activities.DurableInstancingOptions(this);
            this.createOwnerCommand = new CreateWorkflowOwnerCommand();
            this.instanceMetadataChanges = new Dictionary<XName, InstanceValue>();
            this.thisLock = new object();
            InstanceValue value2 = new InstanceValue(XNamespace.Get("http://tempuri.org").GetName("Sentinel"));
            this.createOwnerCommand.InstanceOwnerMetadata.Add(WorkflowNamespace.WorkflowHostType, value2);
            this.instanceMetadataChanges.Add(WorkflowNamespace.WorkflowHostType, value2);
            this.instanceMetadataChanges.Add(PersistenceMetadataNamespace.InstanceType, new InstanceValue(WorkflowNamespace.WorkflowHostType, InstanceValueOptions.WriteOnly));
            this.Host = host;
        }

        public void Abort()
        {
            this.AbortDirectory();
        }

        private void AbortDirectory()
        {
            lock (this.thisLock)
            {
                if (this.state == 3)
                {
                    return;
                }
                this.state = 3;
            }
            if (this.handle != null)
            {
                this.handle.Free();
            }
            if (this.PersistenceProviderDirectory != null)
            {
                this.PersistenceProviderDirectory.Abort();
            }
        }

        public void AddInitialInstanceValues(IDictionary<XName, object> writeOnlyValues)
        {
            ThrowIfDisposedOrImmutable(this.state);
            if (writeOnlyValues != null)
            {
                foreach (KeyValuePair<XName, object> pair in writeOnlyValues)
                {
                    if (this.instanceMetadataChanges.ContainsKey(pair.Key))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.Argument("writeOnlyValues", System.ServiceModel.Activities.SR.ConflictingValueName(pair.Key));
                    }
                    this.instanceMetadataChanges.Add(pair.Key, new InstanceValue(pair.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));
                }
            }
        }

        public void AddInstanceOwnerValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            ThrowIfDisposedOrImmutable(this.state);
            if (readWriteValues != null)
            {
                foreach (KeyValuePair<XName, object> pair in readWriteValues)
                {
                    if (this.createOwnerCommand.InstanceOwnerMetadata.ContainsKey(pair.Key))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.Argument("readWriteValues", System.ServiceModel.Activities.SR.ConflictingValueName(pair.Key));
                    }
                    this.createOwnerCommand.InstanceOwnerMetadata.Add(pair.Key, new InstanceValue(pair.Value));
                }
            }
            if (writeOnlyValues != null)
            {
                foreach (KeyValuePair<XName, object> pair2 in writeOnlyValues)
                {
                    if (this.createOwnerCommand.InstanceOwnerMetadata.ContainsKey(pair2.Key))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.Argument("writeOnlyValues", System.ServiceModel.Activities.SR.ConflictingValueName(pair2.Key));
                    }
                    this.createOwnerCommand.InstanceOwnerMetadata.Add(pair2.Key, new InstanceValue(pair2.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));
                }
            }
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        internal IAsyncResult BeginGetInstance(Guid instanceId, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfClosedOrAborted(this.state);
            return new GetInstanceAsyncResult(this, instanceId, parameters, timeout, callback, state);
        }

        internal IAsyncResult BeginGetInstance(InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfClosedOrAborted(this.state);
            return new GetInstanceAsyncResult(this, instanceKey, additionalKeys, parameters, timeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                return new OpenInstanceStoreAsyncResult(this, timeout, callback, state);
            }
        }

        private IAsyncResult BeginWaitAndHandleStoreEvents(AsyncCallback callback, object state)
        {
            return new WaitAndHandleStoreEventsAsyncResult(this, callback, state);
        }

        private void CheckPersistenceProviderBehavior()
        {
            foreach (IServiceBehavior behavior in this.Host.Description.Behaviors)
            {
                if (behavior.GetType().FullName == "System.ServiceModel.Description.PersistenceProviderBehavior")
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationException(System.ServiceModel.Activities.SR.UseInstanceStoreInsteadOfPersistenceProvider));
                }
            }
        }

        public void Close(TimeSpan timeout)
        {
            CloseAsyncResult.End(new CloseAsyncResult(this, timeout, null, null));
        }

        public void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        internal WorkflowServiceInstance EndGetInstance(IAsyncResult result)
        {
            return GetInstanceAsyncResult.End(result);
        }

        public void EndOpen(IAsyncResult result)
        {
            OpenInstanceStoreAsyncResult.End(result);
        }

        private void EndWaitAndHandleStoreEvents(IAsyncResult result)
        {
            WaitAndHandleStoreEventsAsyncResult.End(result);
        }

        private bool HandleException(Exception exception)
        {
            if (((!(exception is TimeoutException) && !(exception is OperationCanceledException)) && (!(exception is TransactionException) && !(exception is CommunicationObjectAbortedException))) && (!(exception is FaultException) && !(exception is InstancePersistenceException)))
            {
                return false;
            }
            System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
            this.Host.FaultServiceHostIfNecessary(exception);
            return true;
        }

        private void InitializePersistenceProviderDirectory()
        {
            bool flag;
            WorkflowServiceBehavior behavior = this.Host.Description.Behaviors.Find<WorkflowServiceBehavior>();
            int defaultMaxConcurrentInstances = ServiceThrottlingBehavior.DefaultMaxConcurrentInstances;
            ServiceThrottlingBehavior behavior2 = this.Host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (behavior2 != null)
            {
                defaultMaxConcurrentInstances = behavior2.MaxConcurrentInstances;
            }
            if (this.InstanceStore != null)
            {
                this.PersistenceProviderDirectory = new System.ServiceModel.Activities.Dispatcher.PersistenceProviderDirectory(this.InstanceStore, this.owner, this.instanceMetadataChanges, behavior.Activity, this.Host, DurableConsistencyScope.Global, defaultMaxConcurrentInstances);
            }
            else
            {
                this.PersistenceProviderDirectory = new System.ServiceModel.Activities.Dispatcher.PersistenceProviderDirectory(behavior.Activity, this.Host, defaultMaxConcurrentInstances);
            }
            lock (this.thisLock)
            {
                flag = this.state == 3;
            }
            if (flag)
            {
                if (this.handle != null)
                {
                    this.handle.Free();
                }
                this.PersistenceProviderDirectory.Abort();
            }
            if ((this.InstanceStore != null) && !flag)
            {
                this.waitForStoreEventsLoop = new AsyncWaitHandle(EventResetMode.ManualReset);
                this.BeginWaitAndHandleStoreEvents(waitAndHandleStoreEventsCallback, this);
            }
        }

        public void Open(TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                ThrowIfDisposedOrImmutable(this.state);
                this.state = 1;
            }
            this.CheckPersistenceProviderBehavior();
            this.SetDefaultOwnerMetadata();
            if (this.InstanceStore != null)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    InstanceHandle handle = null;
                    try
                    {
                        handle = this.InstanceStore.CreateInstanceHandle((InstanceOwner) null);
                        this.owner = this.InstanceStore.Execute(handle, this.createOwnerCommand, helper.RemainingTime()).InstanceOwner;
                        this.handle = handle;
                        handle = null;
                    }
                    catch (InstancePersistenceException exception)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationException(System.ServiceModel.Activities.SR.UnableToOpenAndRegisterStore, exception));
                    }
                    finally
                    {
                        if (handle != null)
                        {
                            handle.Free();
                        }
                    }
                }
            }
            this.InitializePersistenceProviderDirectory();
        }

        private void SetDefaultOwnerMetadata()
        {
            this.createOwnerCommand.InstanceOwnerMetadata[WorkflowNamespace.WorkflowHostType] = new InstanceValue(this.Host.DurableInstancingOptions.ScopeName);
            this.instanceMetadataChanges[WorkflowNamespace.WorkflowHostType] = new InstanceValue(this.Host.DurableInstancingOptions.ScopeName);
            if (!this.instanceMetadataChanges.ContainsKey(WorkflowServiceNamespace.Service))
            {
                this.instanceMetadataChanges[WorkflowServiceNamespace.Service] = new InstanceValue(this.Host.ServiceName, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }
            foreach (ServiceEndpoint endpoint in this.Host.Description.Endpoints)
            {
                if (endpoint.Name != null)
                {
                    if (endpoint is WorkflowControlEndpoint)
                    {
                        if (!this.createOwnerCommand.InstanceOwnerMetadata.ContainsKey(WorkflowServiceNamespace.ControlEndpoint))
                        {
                            this.createOwnerCommand.InstanceOwnerMetadata.Add(WorkflowServiceNamespace.ControlEndpoint, new InstanceValue(endpoint.ListenUri));
                        }
                    }
                    else
                    {
                        XName key = WorkflowServiceNamespace.EndpointsPath.GetName(endpoint.Name);
                        if (!this.createOwnerCommand.InstanceOwnerMetadata.ContainsKey(key))
                        {
                            this.createOwnerCommand.InstanceOwnerMetadata.Add(key, new InstanceValue(endpoint.ListenUri));
                        }
                    }
                }
            }
            VirtualPathExtension extension = this.Host.Extensions.Find<VirtualPathExtension>();
            if ((extension != null) && !this.instanceMetadataChanges.ContainsKey(PersistenceMetadataNamespace.ActivationType))
            {
                this.instanceMetadataChanges.Add(PersistenceMetadataNamespace.ActivationType, new InstanceValue(PersistenceMetadataNamespace.ActivationTypes.WAS, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));
                this.instanceMetadataChanges[WorkflowServiceNamespace.SiteName] = new InstanceValue(extension.SiteName, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                this.instanceMetadataChanges[WorkflowServiceNamespace.RelativeApplicationPath] = new InstanceValue(extension.ApplicationVirtualPath, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                string str = extension.VirtualPath.Substring(1);
                string str2 = ("/" == extension.ApplicationVirtualPath) ? str : (extension.ApplicationVirtualPath + str);
                this.instanceMetadataChanges[WorkflowServiceNamespace.RelativeServicePath] = new InstanceValue(str2, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }
        }

        private static void ThrowIfClosedOrAborted(int state)
        {
            if (state == 3)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.ServiceHostExtensionAborted));
            }
            if (state == 2)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ObjectDisposedException(typeof(DurableInstanceManager).Name));
            }
        }

        private static void ThrowIfDisposedOrImmutable(int state)
        {
            if (state == 3)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationObjectAbortedException(System.ServiceModel.Activities.SR.ServiceHostExtensionAborted));
            }
            if (state == 2)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ObjectDisposedException(typeof(DurableInstanceManager).Name));
            }
            if (state == 1)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ServiceHostExtensionImmutable));
            }
        }

        private static void WaitAndHandleStoreEventsCallback(IAsyncResult result)
        {
            DurableInstanceManager asyncState = (DurableInstanceManager) result.AsyncState;
            bool flag = false;
            try
            {
                asyncState.EndWaitAndHandleStoreEvents(result);
            }
            catch (OperationCanceledException exception)
            {
                System.ServiceModel.Activities.FxTrace.Exception.AsWarning(exception);
                if ((asyncState.Host.State == CommunicationState.Opening) || (asyncState.Host.State == CommunicationState.Opened))
                {
                    asyncState.Host.Fault(exception);
                }
                flag = true;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2) || !asyncState.HandleException(exception2))
                {
                    throw;
                }
            }
            if (!flag && (asyncState.state == 1))
            {
                asyncState.BeginWaitAndHandleStoreEvents(waitAndHandleStoreEventsCallback, asyncState);
            }
            else
            {
                asyncState.waitForStoreEventsLoop.Set();
            }
        }

        public System.ServiceModel.Activities.DurableInstancingOptions DurableInstancingOptions { get; private set; }

        private WorkflowServiceHost Host { get; set; }

        public System.Runtime.DurableInstancing.InstanceStore InstanceStore
        {
            get
            {
                return this.store;
            }
            set
            {
                ThrowIfDisposedOrImmutable(this.state);
                this.store = value;
            }
        }

        internal System.ServiceModel.Activities.Dispatcher.PersistenceProviderDirectory PersistenceProviderDirectory { get; set; }

        private class CloseAsyncResult : AsyncResult
        {
            private WorkflowServiceInstance currentInstance;
            private InstanceHandle handle;
            private static AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(DurableInstanceManager.CloseAsyncResult.HandleEndExecute);
            private static AsyncResult.AsyncCompletion handleEndReleaseInstance = new AsyncResult.AsyncCompletion(DurableInstanceManager.CloseAsyncResult.HandleEndReleaseInstance);
            private static Action<object, TimeoutException> handleWaitForStoreEvents = new Action<object, TimeoutException>(DurableInstanceManager.CloseAsyncResult.HandleWaitForStoreEvents);
            private DurableInstanceManager instanceManager;
            private TimeoutHelper timeoutHelper;
            private IEnumerator<PersistenceContext> workflowServiceInstances;

            public CloseAsyncResult(DurableInstanceManager instanceManager, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instanceManager = instanceManager;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if ((this.instanceManager.state == 1) && (this.instanceManager.handle != null))
                {
                    this.instanceManager.handle.Free();
                    if (this.WaitForStoreEventsLoop())
                    {
                        base.Complete(true);
                    }
                }
                else if (this.PerformClose())
                {
                    base.Complete(true);
                }
            }

            private bool CloseProviderDirectory()
            {
                bool flag = false;
                try
                {
                    this.instanceManager.PersistenceProviderDirectory.Close();
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.instanceManager.AbortDirectory();
                    }
                }
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DurableInstanceManager.CloseAsyncResult>(result);
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                DurableInstanceManager.CloseAsyncResult asyncState = (DurableInstanceManager.CloseAsyncResult) result.AsyncState;
                try
                {
                    asyncState.instanceManager.owner = asyncState.instanceManager.InstanceStore.EndExecute(result).InstanceOwner;
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
                    asyncState.handle.Free();
                    asyncState.handle = null;
                }
                return asyncState.CloseProviderDirectory();
            }

            private static bool HandleEndReleaseInstance(IAsyncResult result)
            {
                DurableInstanceManager.CloseAsyncResult asyncState = (DurableInstanceManager.CloseAsyncResult) result.AsyncState;
                asyncState.currentInstance.EndReleaseInstance(result);
                return asyncState.Process();
            }

            private static void HandleWaitForStoreEvents(object state, TimeoutException exception)
            {
                DurableInstanceManager.CloseAsyncResult result = (DurableInstanceManager.CloseAsyncResult) state;
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
                        flag = result.PerformClose();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        exception2 = exception;
                        flag = true;
                    }
                    if (flag)
                    {
                        result.Complete(false, exception2);
                    }
                }
            }

            private bool PerformClose()
            {
                bool flag;
                bool flag2;
                bool flag3;
                lock (this.instanceManager.thisLock)
                {
                    flag = this.instanceManager.state == 2;
                    flag2 = this.instanceManager.state == 1;
                    flag3 = this.instanceManager.state == 3;
                    if (flag2)
                    {
                        this.instanceManager.state = 2;
                    }
                }
                if (flag)
                {
                    return true;
                }
                if (!flag2)
                {
                    if (!flag3)
                    {
                        this.instanceManager.AbortDirectory();
                    }
                    return true;
                }
                this.workflowServiceInstances = this.instanceManager.PersistenceProviderDirectory.GetContexts().GetEnumerator();
                return this.Process();
            }

            private bool Process()
            {
                while (this.workflowServiceInstances.MoveNext())
                {
                    this.currentInstance = this.workflowServiceInstances.Current.GetInstance(null);
                    if (this.currentInstance != null)
                    {
                        IAsyncResult result = this.currentInstance.BeginReleaseInstance(false, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndReleaseInstance), this);
                        if (!base.CheckSyncContinue(result))
                        {
                            return false;
                        }
                        this.currentInstance.EndReleaseInstance(result);
                    }
                }
                BufferedReceiveManager manager = this.instanceManager.Host.Extensions.Find<BufferedReceiveManager>();
                if (manager != null)
                {
                    manager.AbandonBufferedReceives();
                }
                if (this.instanceManager.InstanceStore == null)
                {
                    return this.CloseProviderDirectory();
                }
                IAsyncResult result2 = null;
                this.handle = this.instanceManager.InstanceStore.CreateInstanceHandle(this.instanceManager.owner);
                try
                {
                    result2 = this.instanceManager.InstanceStore.BeginExecute(this.handle, new DeleteWorkflowOwnerCommand(), this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndExecute), this);
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
                    if (result2 == null)
                    {
                        this.handle.Free();
                        this.handle = null;
                    }
                }
                return base.SyncContinue(result2);
            }

            private bool WaitForStoreEventsLoop()
            {
                if ((this.instanceManager.waitForStoreEventsLoop != null) && !this.instanceManager.waitForStoreEventsLoop.WaitAsync(handleWaitForStoreEvents, this, this.timeoutHelper.RemainingTime()))
                {
                    return false;
                }
                return this.PerformClose();
            }
        }

        private class GetInstanceAsyncResult : AsyncResult
        {
            private ICollection<InstanceKey> additionalKeys;
            private CommittableTransaction committableTransaction;
            private WorkflowServiceInstance durableInstance;
            private static AsyncResult.AsyncCompletion handleAssociateInfrastructureKeys = new AsyncResult.AsyncCompletion(DurableInstanceManager.GetInstanceAsyncResult.HandleAssociateInfrastructureKeys);
            private static AsyncResult.AsyncCompletion handleCommit = new AsyncResult.AsyncCompletion(DurableInstanceManager.GetInstanceAsyncResult.HandleCommit);
            private static AsyncResult.AsyncCompletion handleEndAcquireReference = new AsyncResult.AsyncCompletion(DurableInstanceManager.GetInstanceAsyncResult.HandleEndAcquireReference);
            private static AsyncResult.AsyncCompletion handleEndEnlistContext = new AsyncResult.AsyncCompletion(DurableInstanceManager.GetInstanceAsyncResult.HandleEndEnlistContext);
            private static AsyncResult.AsyncCompletion handleEndLoad = new AsyncResult.AsyncCompletion(DurableInstanceManager.GetInstanceAsyncResult.HandleEndLoad);
            private Guid instanceId;
            private InstanceKey instanceKey;
            private DurableInstanceManager instanceManager;
            private bool loadAny;
            private static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(DurableInstanceManager.GetInstanceAsyncResult.Finally);
            private WorkflowGetInstanceContext parameters;
            private PersistenceContext persistenceContext;
            private bool referenceAcquired;
            private TimeSpan timeout;
            private DependentTransaction transaction;

            private GetInstanceAsyncResult(DurableInstanceManager instanceManager, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instanceManager = instanceManager;
                this.parameters = parameters;
                this.timeout = timeout;
                this.loadAny = parameters == null;
                base.OnCompleting = onCompleting;
                Transaction current = Transaction.Current;
                if ((current == null) && this.instanceManager.Host.IsLoadTransactionRequired)
                {
                    this.committableTransaction = new CommittableTransaction(this.timeout);
                    current = this.committableTransaction;
                }
                if (current != null)
                {
                    this.transaction = current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }
            }

            public GetInstanceAsyncResult(DurableInstanceManager instanceManager, Guid instanceId, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state) : this(instanceManager, parameters, timeout, callback, state)
            {
                this.instanceId = instanceId;
                if (this.GetInstance())
                {
                    base.Complete(true);
                }
            }

            public GetInstanceAsyncResult(DurableInstanceManager instanceManager, InstanceKey instanceKey, ICollection<InstanceKey> additionalKeys, WorkflowGetInstanceContext parameters, TimeSpan timeout, AsyncCallback callback, object state) : this(instanceManager, parameters, timeout, callback, state)
            {
                this.instanceKey = instanceKey;
                this.additionalKeys = additionalKeys;
                if (this.GetInstance())
                {
                    base.Complete(true);
                }
            }

            private bool AssociateKeys()
            {
                IAsyncResult result;
                if ((this.additionalKeys == null) || (this.additionalKeys.Count <= 0))
                {
                    return this.CommitTransaction();
                }
                try
                {
                    result = this.durableInstance.BeginAssociateInfrastructureKeys(this.additionalKeys, this.transaction, this.timeout, base.PrepareAsyncCompletion(handleAssociateInfrastructureKeys), this);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.persistenceContext.Abort();
                    throw;
                }
                return base.SyncContinue(result);
            }

            private bool CommitTransaction()
            {
                if (this.transaction != null)
                {
                    this.transaction.Complete();
                }
                if (this.committableTransaction != null)
                {
                    IAsyncResult result = this.committableTransaction.BeginCommit(base.PrepareAsyncCompletion(handleCommit), this);
                    return base.SyncContinue(result);
                }
                return true;
            }

            public static WorkflowServiceInstance End(IAsyncResult result)
            {
                return AsyncResult.End<DurableInstanceManager.GetInstanceAsyncResult>(result).durableInstance;
            }

            private static void Finally(AsyncResult result, Exception exception)
            {
                DurableInstanceManager.GetInstanceAsyncResult result2 = (DurableInstanceManager.GetInstanceAsyncResult) result;
                if (result2.committableTransaction != null)
                {
                    try
                    {
                        result2.committableTransaction.Rollback(exception);
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
                if (result2.referenceAcquired && (exception != null))
                {
                    result2.durableInstance.ReleaseReference();
                }
            }

            private bool GetInstance()
            {
                IAsyncResult result = null;
                if (!this.loadAny && this.parameters.CanCreateInstance)
                {
                    if ((this.instanceKey != null) && this.instanceKey.IsValid)
                    {
                        result = this.instanceManager.PersistenceProviderDirectory.BeginLoadOrCreate(this.instanceKey, Guid.Empty, this.additionalKeys, this.transaction, this.timeout, base.PrepareAsyncCompletion(handleEndLoad), this);
                    }
                    else
                    {
                        result = this.instanceManager.PersistenceProviderDirectory.BeginLoadOrCreate(this.instanceId, this.additionalKeys, this.transaction, this.timeout, base.PrepareAsyncCompletion(handleEndLoad), this);
                    }
                }
                else if (this.instanceKey != null)
                {
                    result = this.instanceManager.PersistenceProviderDirectory.BeginLoad(this.instanceKey, this.additionalKeys, this.transaction, this.timeout, base.PrepareAsyncCompletion(handleEndLoad), this);
                }
                else
                {
                    result = this.instanceManager.PersistenceProviderDirectory.BeginLoad(this.instanceId, null, this.transaction, this.loadAny, this.timeout, base.PrepareAsyncCompletion(handleEndLoad), this);
                }
                return base.SyncContinue(result);
            }

            private static bool HandleAssociateInfrastructureKeys(IAsyncResult result)
            {
                DurableInstanceManager.GetInstanceAsyncResult asyncState = (DurableInstanceManager.GetInstanceAsyncResult) result.AsyncState;
                try
                {
                    asyncState.durableInstance.EndAssociateInfrastructureKeys(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.persistenceContext.Abort();
                    throw;
                }
                return asyncState.CommitTransaction();
            }

            private static bool HandleCommit(IAsyncResult result)
            {
                DurableInstanceManager.GetInstanceAsyncResult asyncState = (DurableInstanceManager.GetInstanceAsyncResult) result.AsyncState;
                asyncState.committableTransaction.EndCommit(result);
                asyncState.committableTransaction = null;
                return true;
            }

            private static bool HandleEndAcquireReference(IAsyncResult result)
            {
                DurableInstanceManager.GetInstanceAsyncResult asyncState = (DurableInstanceManager.GetInstanceAsyncResult) result.AsyncState;
                if (asyncState.durableInstance.EndTryAcquireReference(result))
                {
                    asyncState.referenceAcquired = true;
                    return asyncState.TryEnlistContext();
                }
                asyncState.referenceAcquired = false;
                asyncState.durableInstance = null;
                return asyncState.GetInstance();
            }

            private static bool HandleEndEnlistContext(IAsyncResult result)
            {
                DurableInstanceManager.GetInstanceAsyncResult asyncState = (DurableInstanceManager.GetInstanceAsyncResult) result.AsyncState;
                try
                {
                    asyncState.persistenceContext.EndEnlist(result);
                }
                catch (ObjectDisposedException)
                {
                    asyncState.referenceAcquired = false;
                    asyncState.durableInstance = null;
                    return asyncState.GetInstance();
                }
                catch (CommunicationObjectAbortedException)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.DefaultAbortReason));
                }
                return asyncState.AssociateKeys();
            }

            private static bool HandleEndLoad(IAsyncResult result)
            {
                bool flag;
                DurableInstanceManager.GetInstanceAsyncResult asyncState = (DurableInstanceManager.GetInstanceAsyncResult) result.AsyncState;
                PersistenceContext persistenceContext = asyncState.persistenceContext;
                if (!asyncState.loadAny && asyncState.parameters.CanCreateInstance)
                {
                    asyncState.persistenceContext = asyncState.instanceManager.PersistenceProviderDirectory.EndLoadOrCreate(result, out flag);
                }
                else
                {
                    asyncState.persistenceContext = asyncState.instanceManager.PersistenceProviderDirectory.EndLoad(result, out flag);
                }
                Fx.AssertAndThrow(persistenceContext != asyncState.persistenceContext, "PPD should not load same PersistenceContext for the same GetInstanceAsyncResult!");
                return asyncState.TryAcquire(flag);
            }

            private bool TryAcquire(bool fromCache)
            {
                this.durableInstance = this.persistenceContext.GetInstance(this.parameters);
                if (!fromCache)
                {
                    this.referenceAcquired = true;
                    return this.AssociateKeys();
                }
                IAsyncResult result = this.durableInstance.BeginTryAcquireReference(this.timeout, base.PrepareAsyncCompletion(handleEndAcquireReference), this);
                return base.SyncContinue(result);
            }

            private bool TryEnlistContext()
            {
                IAsyncResult result = null;
                bool flag = false;
                IDisposable disposable = base.PrepareTransactionalCall(this.transaction);
                try
                {
                    result = this.persistenceContext.BeginEnlist(this.timeout, base.PrepareAsyncCompletion(handleEndEnlistContext), this);
                }
                catch (ObjectDisposedException)
                {
                    flag = true;
                }
                catch (CommunicationObjectAbortedException)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new OperationCanceledException(System.ServiceModel.Activities.SR.DefaultAbortReason));
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                if (flag)
                {
                    this.referenceAcquired = false;
                    this.durableInstance = null;
                    return this.GetInstance();
                }
                return base.SyncContinue(result);
            }
        }

        private class OpenInstanceStoreAsyncResult : AsyncResult
        {
            private InstanceHandle handle;
            private static AsyncResult.AsyncCompletion handleEndExecute = new AsyncResult.AsyncCompletion(DurableInstanceManager.OpenInstanceStoreAsyncResult.HandleEndExecute);
            private DurableInstanceManager instanceManager;
            private static Action<AsyncResult, Exception> onFinally = new Action<AsyncResult, Exception>(DurableInstanceManager.OpenInstanceStoreAsyncResult.OnFinally);
            private TimeoutHelper timeoutHelper;

            public OpenInstanceStoreAsyncResult(DurableInstanceManager instanceManager, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                bool flag;
                this.instanceManager = instanceManager;
                this.timeoutHelper = new TimeoutHelper(timeout);
                lock (this.instanceManager.thisLock)
                {
                    DurableInstanceManager.ThrowIfDisposedOrImmutable(this.instanceManager.state);
                    this.instanceManager.state = 1;
                }
                instanceManager.CheckPersistenceProviderBehavior();
                this.instanceManager.SetDefaultOwnerMetadata();
                base.OnCompleting = onFinally;
                Exception exception = null;
                try
                {
                    if (instanceManager.InstanceStore == null)
                    {
                        flag = this.CreateDirectory();
                    }
                    else
                    {
                        this.handle = this.instanceManager.InstanceStore.CreateInstanceHandle((InstanceOwner) null);
                        IAsyncResult result = this.instanceManager.InstanceStore.BeginExecute(this.handle, this.instanceManager.createOwnerCommand, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndExecute), this);
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

            private bool CreateDirectory()
            {
                this.instanceManager.InitializePersistenceProviderDirectory();
                this.instanceManager.handle = this.handle;
                this.handle = null;
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DurableInstanceManager.OpenInstanceStoreAsyncResult>(result);
            }

            private static bool HandleEndExecute(IAsyncResult result)
            {
                DurableInstanceManager.OpenInstanceStoreAsyncResult asyncState = (DurableInstanceManager.OpenInstanceStoreAsyncResult) result.AsyncState;
                asyncState.instanceManager.owner = asyncState.instanceManager.InstanceStore.EndExecute(result).InstanceOwner;
                return asyncState.CreateDirectory();
            }

            private static void OnFinally(AsyncResult result, Exception exception)
            {
                if (exception != null)
                {
                    try
                    {
                        if (exception is InstancePersistenceException)
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new CommunicationException(System.ServiceModel.Activities.SR.UnableToOpenAndRegisterStore, exception));
                        }
                    }
                    finally
                    {
                        DurableInstanceManager.OpenInstanceStoreAsyncResult result2 = (DurableInstanceManager.OpenInstanceStoreAsyncResult) result;
                        if (result2.handle != null)
                        {
                            result2.handle.Free();
                        }
                    }
                }
            }
        }

        private static class States
        {
            public const int Aborted = 3;
            public const int Closed = 2;
            public const int Created = 0;
            public const int Opened = 1;
        }

        private class WaitAndHandleStoreEventsAsyncResult : AsyncResult
        {
            private WorkflowServiceInstance currentInstance;
            private IEnumerator<InstancePersistenceEvent> events;
            private static AsyncResult.AsyncCompletion handleEndGetInstance = new AsyncResult.AsyncCompletion(DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult.HandleEndGetInstance);
            private static AsyncResult.AsyncCompletion handleEndRunInstance = new AsyncResult.AsyncCompletion(DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult.HandleEndRunInstance);
            private static AsyncResult.AsyncCompletion handleEndWaitForStoreEvents = new AsyncResult.AsyncCompletion(DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult.HandleEndWaitForStoreEvents);
            private DurableInstanceManager instanceManager;
            private static Action<object> waitAndHandleStoreEvents = new Action<object>(DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult.WaitAndHandleStoreEvents);

            public WaitAndHandleStoreEventsAsyncResult(DurableInstanceManager instanceManager, AsyncCallback callback, object state) : base(callback, state)
            {
                this.instanceManager = instanceManager;
                ActionItem.Schedule(waitAndHandleStoreEvents, this);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult>(result);
            }

            private static bool HandleEndGetInstance(IAsyncResult result)
            {
                DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult asyncState = (DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult) result.AsyncState;
                try
                {
                    asyncState.currentInstance = asyncState.instanceManager.EndGetInstance(result);
                    return asyncState.RunInstance();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !asyncState.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                return asyncState.HandleStoreEvents();
            }

            private static bool HandleEndRunInstance(IAsyncResult result)
            {
                DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult asyncState = (DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult) result.AsyncState;
                try
                {
                    asyncState.currentInstance.EndRun(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !asyncState.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    asyncState.currentInstance.ReleaseReference();
                    asyncState.currentInstance = null;
                }
                return asyncState.HandleStoreEvents();
            }

            private static bool HandleEndWaitForStoreEvents(IAsyncResult result)
            {
                DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult asyncState = (DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult) result.AsyncState;
                asyncState.events = asyncState.instanceManager.InstanceStore.EndWaitForEvents(result).GetEnumerator();
                return asyncState.HandleStoreEvents();
            }

            private bool HandleStoreEvents()
            {
                if (!this.events.MoveNext())
                {
                    return true;
                }
                InstancePersistenceEvent current = this.events.Current;
                if (current.Name == InstancePersistenceEvent<HasRunnableWorkflowEvent>.Value.Name)
                {
                    try
                    {
                        IAsyncResult result = this.instanceManager.BeginGetInstance(Guid.Empty, null, this.instanceManager.Host.PersistTimeout, base.PrepareAsyncCompletion(handleEndGetInstance), this);
                        return base.SyncContinue(result);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception) || !this.instanceManager.HandleException(exception))
                        {
                            throw;
                        }
                        goto Label_00A0;
                    }
                }
                Fx.AssertAndThrow("Unknown InstancePersistenceEvent (" + current.Name + ")!");
            Label_00A0:
                return this.HandleStoreEvents();
            }

            private bool RunInstance()
            {
                try
                {
                    IAsyncResult result = this.currentInstance.BeginRun(null, TimeSpan.MaxValue, base.PrepareAsyncCompletion(handleEndRunInstance), this);
                    return base.SyncContinue(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (this.currentInstance != null)
                    {
                        this.currentInstance.ReleaseReference();
                        this.currentInstance = null;
                    }
                    if (!this.instanceManager.HandleException(exception))
                    {
                        throw;
                    }
                }
                return this.HandleStoreEvents();
            }

            private static void WaitAndHandleStoreEvents(object state)
            {
                bool flag;
                DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult result = (DurableInstanceManager.WaitAndHandleStoreEventsAsyncResult) state;
                Exception exception = null;
                try
                {
                    flag = result.WaitForStoreEvents();
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

            private bool WaitForStoreEvents()
            {
                IAsyncResult result = this.instanceManager.InstanceStore.BeginWaitForEvents(this.instanceManager.handle, TimeSpan.FromSeconds(600.0), base.PrepareAsyncCompletion(handleEndWaitForStoreEvents), this);
                return base.SyncContinue(result);
            }
        }
    }
}

