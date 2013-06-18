namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Xml.Linq;

    public sealed class SqlWorkflowInstanceStore : InstanceStore
    {
        private TimeSpan bufferedHostLockRenewalPeriod;
        private string cachedConnectionString;
        internal const string CommonConnectionPoolName = "System.Activities.DurableInstancing.SqlWorkflowInstanceStore";
        private string connectionString;
        private static readonly TimeSpan defaultConnectionOpenTime = TimeSpan.FromSeconds(15.0);
        private static readonly TimeSpan defaultInstancePersistenceEventDetectionPeriod = TimeSpan.FromSeconds(5.0);
        private static readonly TimeSpan defaultLockRenewalPeriod = TimeSpan.FromSeconds(30.0);
        internal const int DefaultMaximumRetries = 4;
        private const string DefaultPromotionName = "System.Activities.InstanceMetadata";
        private Dictionary<string, Tuple<List<XName>, List<XName>>> definedPromotions;
        private bool enqueueRunCommands;
        private TimeSpan hostLockRenewalPeriod;
        private System.Activities.DurableInstancing.InstanceCompletionAction instanceCompletionAction;
        private System.Activities.DurableInstancing.InstanceEncodingOption instanceEncodingOption;
        private System.Activities.DurableInstancing.InstanceLockedExceptionAction instanceLockedExceptionAction;
        private TimeSpan instancePersistenceEventDetectionPeriod;
        private bool isReadOnly;
        private static readonly TimeSpan minimumTimeSpanAllowed = TimeSpan.FromSeconds(1.0);
        private Action<object> scheduledUnlockInstance;
        private SqlWorkflowInstanceStoreLock storeLock;
        private AsyncCallback unlockInstanceCallback;

        public SqlWorkflowInstanceStore() : this(null)
        {
        }

        public SqlWorkflowInstanceStore(string connectionString)
        {
            this.InstanceEncodingOption = System.Activities.DurableInstancing.InstanceEncodingOption.GZip;
            this.InstanceCompletionAction = System.Activities.DurableInstancing.InstanceCompletionAction.DeleteAll;
            this.InstanceLockedExceptionAction = System.Activities.DurableInstancing.InstanceLockedExceptionAction.NoRetry;
            this.HostLockRenewalPeriod = defaultLockRenewalPeriod;
            this.RunnableInstancesDetectionPeriod = defaultInstancePersistenceEventDetectionPeriod;
            this.EnqueueRunCommands = false;
            this.LoadRetryHandler = new System.Activities.DurableInstancing.LoadRetryHandler();
            this.ConnectionString = connectionString;
            this.definedPromotions = new Dictionary<string, Tuple<List<XName>, List<XName>>>();
            this.bufferedHostLockRenewalPeriod = TimeSpan.Zero;
            this.unlockInstanceCallback = Fx.ThunkCallback(new AsyncCallback(this.UnlockInstanceCallback));
            this.scheduledUnlockInstance = new Action<object>(this.ScheduledUnlockInstance);
            this.storeLock = new SqlWorkflowInstanceStoreLock(this);
            this.MaxConnectionRetries = 4;
        }

        protected internal override IAsyncResult BeginTryCommand(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            if (command == null)
            {
                throw FxTrace.Exception.ArgumentNull("command");
            }
            if (!this.storeLock.IsValid && !(command is CreateWorkflowOwnerCommand))
            {
                throw FxTrace.Exception.AsError(new InstanceOwnerException(command.Name, this.storeLock.LockOwnerId));
            }
            if (this.IsRetryCommand(command))
            {
                return new LoadRetryAsyncResult(this, context, command, timeout, callback, state);
            }
            return this.BeginTryCommandInternal(context, command, timeout, callback, state);
        }

        internal IAsyncResult BeginTryCommandInternal(InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
        {
            SqlWorkflowInstanceStoreAsyncResult result = null;
            if (command is SaveWorkflowCommand)
            {
                result = new SaveWorkflowAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is TryLoadRunnableWorkflowCommand)
            {
                result = new TryLoadRunnableWorkflowAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is LoadWorkflowCommand)
            {
                result = new LoadWorkflowAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is LoadWorkflowByInstanceKeyCommand)
            {
                result = new LoadWorkflowByKeyAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is ExtendLockCommand)
            {
                result = new ExtendLockAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is DetectRunnableInstancesCommand)
            {
                result = new DetectRunnableInstancesAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is DetectActivatableWorkflowsCommand)
            {
                result = new DetectActivatableWorkflowsAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is RecoverInstanceLocksCommand)
            {
                result = new RecoverInstanceLocksAsyncResult(null, command, this, this.storeLock, null, timeout, callback, state);
            }
            else if (command is UnlockInstanceCommand)
            {
                result = new UnlockInstanceAsyncResult(null, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is CreateWorkflowOwnerCommand)
            {
                result = new CreateWorkflowOwnerAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is DeleteWorkflowOwnerCommand)
            {
                result = new DeleteWorkflowOwnerAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else if (command is QueryActivatableWorkflowsCommand)
            {
                result = new QueryActivatableWorkflowAsyncResult(context, command, this, this.storeLock, Transaction.Current, timeout, callback, state);
            }
            else
            {
                return base.BeginTryCommand(context, command, timeout, callback, state);
            }
            result.ScheduleCallback();
            return result;
        }

        private string CreateCachedConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(this.ConnectionString) {
                AsynchronousProcessing = true,
                ConnectTimeout = (int) defaultConnectionOpenTime.TotalSeconds,
                ApplicationName = "DefaultPool"
            };
            return builder.ToString();
        }

        protected internal override bool EndTryCommand(IAsyncResult result)
        {
            if (result is LoadRetryAsyncResult)
            {
                return LoadRetryAsyncResult.End(result);
            }
            if (result is SqlWorkflowInstanceStoreAsyncResult)
            {
                return SqlWorkflowInstanceStoreAsyncResult.End(result);
            }
            return base.EndTryCommand(result);
        }

        internal bool EnqueueRetry(LoadRetryAsyncResult loadRetryAsyncResult)
        {
            bool flag = false;
            if (this.storeLock.IsValid)
            {
                flag = this.LoadRetryHandler.Enqueue(loadRetryAsyncResult);
            }
            return flag;
        }

        internal InstancePersistenceEvent FindEvent(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner)
        {
            return this.FindEventHelper(eventType, out instanceOwner, false);
        }

        private InstancePersistenceEvent FindEventHelper(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner, bool withReset)
        {
            instanceOwner = null;
            InstanceOwner[] instanceOwners = base.GetInstanceOwners();
            if (instanceOwners.Length > 0)
            {
                foreach (InstanceOwner owner in instanceOwners)
                {
                    if (owner.InstanceOwnerId == this.storeLock.LockOwnerId)
                    {
                        instanceOwner = owner;
                        break;
                    }
                }
                if (instanceOwner != null)
                {
                    if (withReset)
                    {
                        base.ResetEvent(eventType, instanceOwner);
                    }
                    foreach (InstancePersistenceEvent event2 in base.GetEvents(instanceOwner))
                    {
                        if (event2 == eventType)
                        {
                            return event2;
                        }
                    }
                }
            }
            return null;
        }

        internal InstancePersistenceEvent FindEventWithReset(InstancePersistenceEvent eventType, out InstanceOwner instanceOwner)
        {
            return this.FindEventHelper(eventType, out instanceOwner, true);
        }

        internal void GenerateUnlockCommand(InstanceLockTracking instanceLockTracking)
        {
            UnlockInstanceCommand command = new UnlockInstanceCommand {
                SurrogateOwnerId = this.storeLock.SurrogateLockOwnerId,
                InstanceId = instanceLockTracking.InstanceId,
                InstanceVersion = instanceLockTracking.InstanceVersion
            };
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                this.BeginTryCommandInternal(null, command, TimeSpan.MaxValue, this.unlockInstanceCallback, command);
            }
        }

        internal TimeSpan GetNextRetryDelay(int retryAttempt)
        {
            return this.RetryStrategy.RetryDelay(retryAttempt);
        }

        internal bool IsLockRetryEnabled()
        {
            return (this.InstanceLockedExceptionAction != System.Activities.DurableInstancing.InstanceLockedExceptionAction.NoRetry);
        }

        private bool IsRetryCommand(InstancePersistenceCommand command)
        {
            if (!this.IsLockRetryEnabled())
            {
                return false;
            }
            return ((command is LoadWorkflowByInstanceKeyCommand) || (command is LoadWorkflowCommand));
        }

        private void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                lock (this.ThisLock)
                {
                    if (!this.isReadOnly)
                    {
                        this.cachedConnectionString = this.CreateCachedConnectionString();
                        this.SetLoadRetryStrategy();
                        this.isReadOnly = true;
                    }
                }
            }
        }

        protected override void OnFreeInstanceHandle(InstanceHandle instanceHandle, object userContext)
        {
            ((InstanceLockTracking) userContext).HandleFreed();
        }

        protected override object OnNewInstanceHandle(InstanceHandle instanceHandle)
        {
            this.MakeReadOnly();
            return new InstanceLockTracking(this);
        }

        public void Promote(string name, IEnumerable<XName> promoteAsVariant, IEnumerable<XName> promoteAsBinary)
        {
            this.ThrowIfReadOnly();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            if (this.definedPromotions.ContainsKey(name))
            {
                throw FxTrace.Exception.Argument("name", System.Activities.DurableInstancing.SR.PromotionAlreadyDefined(name));
            }
            if ((promoteAsVariant == null) && (promoteAsBinary == null))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.NoPromotionsDefined(name)));
            }
            if ((promoteAsVariant != null) && (promoteAsVariant.Count<XName>() > 0x20))
            {
                throw FxTrace.Exception.Argument("promoteAsVariant", System.Activities.DurableInstancing.SR.PromotionTooManyDefined(name, promoteAsVariant.Count<XName>(), "variant", 0x20));
            }
            if ((promoteAsBinary != null) && (promoteAsBinary.Count<XName>() > 0x20))
            {
                throw FxTrace.Exception.Argument("promoteAsVariant", System.Activities.DurableInstancing.SR.PromotionTooManyDefined(name, promoteAsVariant.Count<XName>(), "binary", 0x20));
            }
            HashSet<XName> set = new HashSet<XName>();
            List<XName> list = new List<XName>();
            if (promoteAsVariant != null)
            {
                foreach (XName name2 in promoteAsVariant)
                {
                    if (name2 == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.CanNotDefineNullForAPromotion("variant", name)));
                    }
                    if (set.Contains(name2))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.CannotPromoteXNameTwiceInPromotion(name2.ToString(), name)));
                    }
                    list.Add(name2);
                    set.Add(name2);
                }
            }
            List<XName> list2 = new List<XName>();
            if (promoteAsBinary != null)
            {
                foreach (XName name3 in promoteAsBinary)
                {
                    if (name == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.CanNotDefineNullForAPromotion("binary", name3)));
                    }
                    if (set.Contains(name3))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.CannotPromoteXNameTwiceInPromotion(name3.ToString(), name)));
                    }
                    list2.Add(name3);
                    set.Add(name3);
                }
            }
            this.definedPromotions.Add(name, new Tuple<List<XName>, List<XName>>(list, list2));
        }

        private void ScheduledUnlockInstance(object state)
        {
            UnlockInstanceState state2 = (UnlockInstanceState) state;
            UnlockInstanceCommand unlockInstanceCommand = state2.UnlockInstanceCommand;
            try
            {
                this.BeginTryCommandInternal(null, unlockInstanceCommand, TimeSpan.MaxValue, this.unlockInstanceCallback, unlockInstanceCommand);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (TD.UnlockInstanceExceptionIsEnabled())
                {
                    TD.UnlockInstanceException(exception.Message);
                }
                state2.BackoffTimeoutHelper.WaitAndBackoff(this.scheduledUnlockInstance, state2);
            }
        }

        private void SetLoadRetryStrategy()
        {
            this.RetryStrategy = LoadRetryStrategyFactory.CreateRetryStrategy(this.InstanceLockedExceptionAction);
        }

        private void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.DurableInstancing.SR.InstanceStoreReadOnly));
            }
        }

        private void UnlockInstanceCallback(IAsyncResult result)
        {
            try
            {
                this.EndTryCommand(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (TD.UnlockInstanceExceptionIsEnabled())
                {
                    TD.UnlockInstanceException(exception.Message);
                }
                UnlockInstanceState state = new UnlockInstanceState {
                    UnlockInstanceCommand = (UnlockInstanceCommand) result.AsyncState,
                    BackoffTimeoutHelper = new BackoffTimeoutHelper(TimeSpan.MaxValue)
                };
                state.BackoffTimeoutHelper.WaitAndBackoff(this.scheduledUnlockInstance, state);
            }
        }

        internal void UpdateEventStatus(bool signalEvent, InstancePersistenceEvent eventToUpdate)
        {
            InstanceOwner owner;
            InstancePersistenceEvent persistenceEvent = this.FindEventWithReset(eventToUpdate, out owner);
            if ((persistenceEvent != null) && signalEvent)
            {
                base.SignalEvent(persistenceEvent, owner);
            }
        }

        internal TimeSpan BufferedHostLockRenewalPeriod
        {
            get
            {
                if (this.bufferedHostLockRenewalPeriod == TimeSpan.Zero)
                {
                    double num = Math.Min(SqlWorkflowInstanceStoreConstants.LockOwnerTimeoutBuffer.TotalSeconds, TimeSpan.MaxValue.Subtract(this.HostLockRenewalPeriod).TotalSeconds);
                    this.bufferedHostLockRenewalPeriod = TimeSpan.FromSeconds(Math.Min((double) 2147483647.0, (double) (num + this.HostLockRenewalPeriod.TotalSeconds)));
                }
                return this.bufferedHostLockRenewalPeriod;
            }
        }

        internal string CachedConnectionString
        {
            get
            {
                return this.cachedConnectionString;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.connectionString = value;
            }
        }

        public bool EnqueueRunCommands
        {
            get
            {
                return this.enqueueRunCommands;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.enqueueRunCommands = value;
            }
        }

        public TimeSpan HostLockRenewalPeriod
        {
            get
            {
                return this.hostLockRenewalPeriod;
            }
            set
            {
                if (value.CompareTo(minimumTimeSpanAllowed) < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("lockRenewalPeriod", value, System.Activities.DurableInstancing.SR.InvalidLockRenewalPeriod(value, minimumTimeSpanAllowed));
                }
                this.ThrowIfReadOnly();
                this.hostLockRenewalPeriod = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceCompletionAction InstanceCompletionAction
        {
            get
            {
                return this.instanceCompletionAction;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.instanceCompletionAction = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceEncodingOption InstanceEncodingOption
        {
            get
            {
                return this.instanceEncodingOption;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.instanceEncodingOption = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get
            {
                return this.instanceLockedExceptionAction;
            }
            set
            {
                this.ThrowIfReadOnly();
                this.instanceLockedExceptionAction = value;
            }
        }

        internal bool InstanceOwnersExist
        {
            get
            {
                return (base.GetInstanceOwners().Length > 0);
            }
        }

        internal System.Activities.DurableInstancing.LoadRetryHandler LoadRetryHandler { get; set; }

        public int MaxConnectionRetries { get; set; }

        internal Dictionary<string, Tuple<List<XName>, List<XName>>> Promotions
        {
            get
            {
                return this.definedPromotions;
            }
        }

        internal ILoadRetryStrategy RetryStrategy { get; set; }

        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get
            {
                return this.instancePersistenceEventDetectionPeriod;
            }
            set
            {
                if (value.CompareTo(minimumTimeSpanAllowed) < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("instancePersistenceEventDetectionPeriod", value, System.Activities.DurableInstancing.SR.InvalidRunnableInstancesDetectionPeriod(value, minimumTimeSpanAllowed));
                }
                this.ThrowIfReadOnly();
                this.instancePersistenceEventDetectionPeriod = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.definedPromotions;
            }
        }

        internal Guid WorkflowHostType { get; set; }

        private class UnlockInstanceState
        {
            public System.Runtime.BackoffTimeoutHelper BackoffTimeoutHelper { get; set; }

            public System.Activities.DurableInstancing.UnlockInstanceCommand UnlockInstanceCommand { get; set; }
        }
    }
}

