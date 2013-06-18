namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    public class SqlWorkflowPersistenceService : WorkflowPersistenceService, IPendingWork
    {
        private DbResourceAllocator _dbResourceAllocator;
        private static int _deadlock = 0x4b5;
        private bool _enableRetries;
        private bool _ignoreCommonEnableRetries;
        private TimeSpan _ownershipDelta;
        private Guid _serviceInstanceId;
        private WorkflowCommitWorkBatchService _transactionService;
        private bool _unloadOnIdle;
        private NameValueCollection configParameters;
        private const string EnableRetriesToken = "EnableRetries";
        private TimeSpan infinite;
        private const string InstanceOwnershipTimeoutSecondsToken = "OwnershipTimeoutSeconds";
        private TimeSpan loadingInterval;
        private const string LoadingIntervalToken = "LoadIntervalSeconds";
        private SmartTimer loadingTimer;
        private TimeSpan maxLoadingInterval;
        private object timerLock;
        private const string UnloadOnIdleToken = "UnloadOnIdle";
        private string unvalidatedConnectionString;

        public SqlWorkflowPersistenceService(NameValueCollection parameters)
        {
            this._serviceInstanceId = Guid.Empty;
            this.loadingInterval = new TimeSpan(0, 2, 0);
            this.maxLoadingInterval = new TimeSpan(0x16d, 0, 0, 0, 0);
            this.timerLock = new object();
            this.infinite = new TimeSpan(-1L);
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);
            }
            this._ownershipDelta = TimeSpan.MaxValue;
            if (parameters != null)
            {
                foreach (string str in parameters.Keys)
                {
                    if (!str.Equals("ConnectionString", StringComparison.OrdinalIgnoreCase))
                    {
                        if (str.Equals("OwnershipTimeoutSeconds", StringComparison.OrdinalIgnoreCase))
                        {
                            int actualValue = Convert.ToInt32(parameters["OwnershipTimeoutSeconds"], CultureInfo.CurrentCulture);
                            if (actualValue < 0)
                            {
                                throw new ArgumentOutOfRangeException("OwnershipTimeoutSeconds", actualValue, ExecutionStringManager.InvalidOwnershipTimeoutValue);
                            }
                            this._ownershipDelta = new TimeSpan(0, 0, actualValue);
                            this._serviceInstanceId = Guid.NewGuid();
                        }
                        else if (str.Equals("UnloadOnIdle", StringComparison.OrdinalIgnoreCase))
                        {
                            this._unloadOnIdle = bool.Parse(parameters[str]);
                        }
                        else if (str.Equals("LoadIntervalSeconds", StringComparison.OrdinalIgnoreCase))
                        {
                            int seconds = int.Parse(parameters[str], CultureInfo.CurrentCulture);
                            if (seconds > 0)
                            {
                                this.loadingInterval = new TimeSpan(0, 0, seconds);
                            }
                            else
                            {
                                this.loadingInterval = TimeSpan.Zero;
                            }
                            if (this.loadingInterval > this.maxLoadingInterval)
                            {
                                throw new ArgumentOutOfRangeException("LoadIntervalSeconds", this.LoadingInterval, ExecutionStringManager.LoadingIntervalTooLarge);
                            }
                        }
                        else
                        {
                            if (!str.Equals("EnableRetries", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, new object[] { str }), "parameters");
                            }
                            this._enableRetries = bool.Parse(parameters[str]);
                            this._ignoreCommonEnableRetries = true;
                        }
                    }
                }
            }
            this.configParameters = parameters;
        }

        public SqlWorkflowPersistenceService(string connectionString)
        {
            this._serviceInstanceId = Guid.Empty;
            this.loadingInterval = new TimeSpan(0, 2, 0);
            this.maxLoadingInterval = new TimeSpan(0x16d, 0, 0, 0, 0);
            this.timerLock = new object();
            this.infinite = new TimeSpan(-1L);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            }
            this.unvalidatedConnectionString = connectionString;
        }

        public SqlWorkflowPersistenceService(string connectionString, bool unloadOnIdle, TimeSpan instanceOwnershipDuration, TimeSpan loadingInterval)
        {
            this._serviceInstanceId = Guid.Empty;
            this.loadingInterval = new TimeSpan(0, 2, 0);
            this.maxLoadingInterval = new TimeSpan(0x16d, 0, 0, 0, 0);
            this.timerLock = new object();
            this.infinite = new TimeSpan(-1L);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            }
            if (loadingInterval > this.maxLoadingInterval)
            {
                throw new ArgumentOutOfRangeException("loadingInterval", loadingInterval, ExecutionStringManager.LoadingIntervalTooLarge);
            }
            if (instanceOwnershipDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("instanceOwnershipDuration", instanceOwnershipDuration, ExecutionStringManager.InvalidOwnershipTimeoutValue);
            }
            this._ownershipDelta = instanceOwnershipDuration;
            this._unloadOnIdle = unloadOnIdle;
            this.loadingInterval = loadingInterval;
            this.unvalidatedConnectionString = connectionString;
            this._serviceInstanceId = Guid.NewGuid();
        }

        public IEnumerable<SqlPersistenceWorkflowInstanceDescription> GetAllWorkflows()
        {
            if (base.State == WorkflowRuntimeServiceState.Started)
            {
                using (PersistenceDBAccessor accessor = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
                {
                    return accessor.RetrieveAllInstanceDescriptions();
                }
            }
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowRuntimeNotStarted, new object[0]));
        }

        protected internal override Activity LoadCompletedContextActivity(Guid id, Activity outerActivity)
        {
            using (PersistenceDBAccessor accessor = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
            {
                return WorkflowPersistenceService.RestoreFromDefaultSerializedForm(accessor.RetrieveCompletedScope(id), outerActivity);
            }
        }

        private IList<Guid> LoadExpiredTimerIds()
        {
            using (PersistenceDBAccessor accessor = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
            {
                return accessor.RetrieveExpiredTimerIds(this._serviceInstanceId, this.OwnershipTimeout);
            }
        }

        public IList<Guid> LoadExpiredTimerWorkflowIds()
        {
            if (base.State != WorkflowRuntimeServiceState.Started)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowRuntimeNotStarted, new object[0]));
            }
            return this.LoadExpiredTimerIds();
        }

        protected internal override Activity LoadWorkflowInstanceState(Guid id)
        {
            using (PersistenceDBAccessor accessor = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}):Loading instance {1}", new object[] { this._serviceInstanceId.ToString(), id.ToString() });
                return WorkflowPersistenceService.RestoreFromDefaultSerializedForm(accessor.RetrieveInstanceState(id, this._serviceInstanceId, this.OwnershipTimeout), null);
            }
        }

        private void LoadWorkflowsWithExpiredTimers(object ignored)
        {
            lock (this.timerLock)
            {
                if (base.State == WorkflowRuntimeServiceState.Started)
                {
                    IList<Guid> list = null;
                    try
                    {
                        list = this.LoadExpiredTimerIds();
                    }
                    catch (Exception exception)
                    {
                        base.RaiseServicesExceptionNotHandledEvent(exception, Guid.Empty);
                    }
                    if (list != null)
                    {
                        foreach (Guid guid in list)
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({1}): Loading instance with expired timers {0}", new object[] { guid, this._serviceInstanceId.ToString() });
                            try
                            {
                                base.Runtime.GetWorkflow(guid).Load();
                            }
                            catch (WorkflowOwnershipException)
                            {
                            }
                            catch (ObjectDisposedException)
                            {
                                throw;
                            }
                            catch (InvalidOperationException exception2)
                            {
                                if (!exception2.Data.Contains("WorkflowNotFound"))
                                {
                                    base.RaiseServicesExceptionNotHandledEvent(exception2, guid);
                                }
                            }
                            catch (Exception exception3)
                            {
                                base.RaiseServicesExceptionNotHandledEvent(exception3, guid);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnStarted()
        {
            if (this.loadingInterval > TimeSpan.Zero)
            {
                lock (this.timerLock)
                {
                    base.OnStarted();
                    this.loadingTimer = new SmartTimer(new TimerCallback(this.LoadWorkflowsWithExpiredTimers), null, this.loadingInterval, this.loadingInterval);
                }
            }
            this.RecoverRunningWorkflowInstances();
        }

        private void RecoverRunningWorkflowInstances()
        {
            if (Guid.Empty == this._serviceInstanceId)
            {
                IList<Guid> list = null;
                using (PersistenceDBAccessor accessor = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
                {
                    list = accessor.RetrieveNonblockingInstanceStateIds(this._serviceInstanceId, this.OwnershipTimeout);
                }
                foreach (Guid guid in list)
                {
                    try
                    {
                        base.Runtime.GetWorkflow(guid).Load();
                    }
                    catch (Exception exception)
                    {
                        base.RaiseServicesExceptionNotHandledEvent(exception, guid);
                    }
                }
            }
            else
            {
                using (PersistenceDBAccessor accessor2 = new PersistenceDBAccessor(this._dbResourceAllocator, this._enableRetries))
                {
                    Guid guid2;
                    while (accessor2.TryRetrieveANonblockingInstanceStateId(this._serviceInstanceId, this.OwnershipTimeout, out guid2))
                    {
                        try
                        {
                            base.Runtime.GetWorkflow(guid2).Load();
                            continue;
                        }
                        catch (Exception exception2)
                        {
                            base.RaiseServicesExceptionNotHandledEvent(exception2, guid2);
                            continue;
                        }
                    }
                }
            }
        }

        protected internal override void SaveCompletedContextActivity(Activity completedScopeActivity)
        {
            PendingWorkItem workItem = new PendingWorkItem {
                Type = PendingWorkItem.ItemType.CompletedScope,
                SerializedActivity = WorkflowPersistenceService.GetDefaultSerializedForm(completedScopeActivity),
                InstanceId = WorkflowEnvironment.WorkflowInstanceId,
                StateId = ((ActivityExecutionContextInfo) completedScopeActivity.GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid
            };
            WorkflowEnvironment.WorkBatch.Add(this, workItem);
        }

        protected internal override void SaveWorkflowInstanceState(Activity rootActivity, bool unlock)
        {
            if (rootActivity == null)
            {
                throw new ArgumentNullException("rootActivity");
            }
            WorkflowStatus workflowStatus = WorkflowPersistenceService.GetWorkflowStatus(rootActivity);
            bool isBlocked = WorkflowPersistenceService.GetIsBlocked(rootActivity);
            string suspendOrTerminateInfo = WorkflowPersistenceService.GetSuspendOrTerminateInfo(rootActivity);
            Guid guid = (Guid) rootActivity.GetValue(Activity.ActivityContextGuidProperty);
            PendingWorkItem workItem = new PendingWorkItem {
                Type = PendingWorkItem.ItemType.Instance,
                InstanceId = WorkflowEnvironment.WorkflowInstanceId
            };
            if ((workflowStatus != WorkflowStatus.Completed) && (workflowStatus != WorkflowStatus.Terminated))
            {
                workItem.SerializedActivity = WorkflowPersistenceService.GetDefaultSerializedForm(rootActivity);
            }
            else
            {
                workItem.SerializedActivity = new byte[0];
            }
            workItem.Status = (int) workflowStatus;
            workItem.Blocked = isBlocked ? 1 : 0;
            workItem.Info = suspendOrTerminateInfo;
            workItem.StateId = guid;
            workItem.Unlocked = unlock;
            TimerEventSubscription subscription = ((TimerEventSubscriptionCollection) rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty)).Peek();
            workItem.NextTimer = (subscription == null) ? SqlDateTime.MaxValue : subscription.ExpiresAt;
            if (workItem.Info == null)
            {
                workItem.Info = "";
            }
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({4}):Committing instance {0}, Blocked={1}, Unlocked={2}, NextTimer={3}", new object[] { guid.ToString(), workItem.Blocked, workItem.Unlocked, workItem.NextTimer.Value.ToLocalTime(), this._serviceInstanceId.ToString() });
            WorkflowEnvironment.WorkBatch.Add(this, workItem);
        }

        protected internal override void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({1}): Starting, LoadInternalSeconds={0}", new object[] { this.loadingInterval.TotalSeconds, this._serviceInstanceId.ToString() });
            this._dbResourceAllocator = new DbResourceAllocator(base.Runtime, this.configParameters, this.unvalidatedConnectionString);
            this._transactionService = base.Runtime.GetService<WorkflowCommitWorkBatchService>();
            this._dbResourceAllocator.DetectSharedConnectionConflict(this._transactionService);
            if (!this._ignoreCommonEnableRetries && (base.Runtime != null))
            {
                NameValueConfigurationCollection commonParameters = base.Runtime.CommonParameters;
                if (commonParameters != null)
                {
                    foreach (string str in commonParameters.AllKeys)
                    {
                        if (string.Compare("EnableRetries", str, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._enableRetries = bool.Parse(commonParameters[str].Value);
                            break;
                        }
                    }
                }
            }
            base.Start();
        }

        protected internal override void Stop()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): Stopping", new object[] { this._serviceInstanceId.ToString() });
            lock (this.timerLock)
            {
                base.Stop();
                if (this.loadingTimer != null)
                {
                    this.loadingTimer.Dispose();
                    this.loadingTimer = null;
                }
            }
        }

        void IPendingWork.Commit(Transaction transaction, ICollection items)
        {
            PersistenceDBAccessor accessor = null;
            try
            {
                accessor = new PersistenceDBAccessor(this._dbResourceAllocator, transaction, this._transactionService);
                foreach (PendingWorkItem item in items)
                {
                    switch (item.Type)
                    {
                        case PendingWorkItem.ItemType.Instance:
                            accessor.InsertInstanceState(item, this._serviceInstanceId, this.OwnershipTimeout);
                            break;

                        case PendingWorkItem.ItemType.CompletedScope:
                            accessor.InsertCompletedScope(item.InstanceId, item.StateId, item.SerializedActivity);
                            break;

                        case PendingWorkItem.ItemType.ActivationComplete:
                            accessor.ActivationComplete(item.InstanceId, this._serviceInstanceId);
                            break;
                    }
                }
            }
            catch (SqlException exception)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService({1})Exception thrown while persisting instance: {0}", new object[] { exception.Message, this._serviceInstanceId.ToString() });
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "stacktrace : {0}", new object[] { exception.StackTrace });
                if (exception.Number == _deadlock)
                {
                    PersistenceException exception2 = new PersistenceException(exception.Message, exception);
                    throw exception2;
                }
                throw;
            }
            catch (Exception exception3)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService({1}): Exception thrown while persisting instance: {0}", new object[] { exception3.Message, this._serviceInstanceId.ToString() });
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "stacktrace : {0}", new object[] { exception3.StackTrace });
                throw exception3;
            }
            finally
            {
                if (accessor != null)
                {
                    accessor.Dispose();
                }
            }
        }

        void IPendingWork.Complete(bool succeeded, ICollection items)
        {
            if ((this.loadingTimer != null) && succeeded)
            {
                foreach (PendingWorkItem item in items)
                {
                    if (item.Type.Equals(PendingWorkItem.ItemType.Instance))
                    {
                        this.loadingTimer.Update((DateTime) item.NextTimer);
                    }
                }
            }
        }

        bool IPendingWork.MustCommit(ICollection items)
        {
            return true;
        }

        protected internal override bool UnloadOnIdle(Activity activity)
        {
            return this._unloadOnIdle;
        }

        protected internal override void UnlockWorkflowInstanceState(Activity rootActivity)
        {
            PendingWorkItem workItem = new PendingWorkItem {
                Type = PendingWorkItem.ItemType.ActivationComplete,
                InstanceId = WorkflowEnvironment.WorkflowInstanceId
            };
            WorkflowEnvironment.WorkBatch.Add(this, workItem);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}):Unlocking instance {1}", new object[] { this._serviceInstanceId.ToString(), workItem.InstanceId.ToString() });
        }

        public bool EnableRetries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._enableRetries;
            }
            set
            {
                this._enableRetries = value;
                this._ignoreCommonEnableRetries = true;
            }
        }

        public TimeSpan LoadingInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.loadingInterval;
            }
        }

        private DateTime OwnershipTimeout
        {
            get
            {
                if (this._ownershipDelta == TimeSpan.MaxValue)
                {
                    return DateTime.MaxValue;
                }
                return (DateTime.UtcNow + this._ownershipDelta);
            }
        }

        public Guid ServiceInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._serviceInstanceId;
            }
        }
    }
}

