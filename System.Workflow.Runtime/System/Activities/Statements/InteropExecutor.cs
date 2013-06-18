namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [DataContract]
    internal class InteropExecutor : IWorkflowCoreRuntime, IServiceProvider, ISupportInterop
    {
        private bool abortTransaction;
        [DataMember(EmitDefaultValue=false)]
        private int atomicActivityContextId;
        [DataMember(EmitDefaultValue=false)]
        private string atomicActivityName;
        [DataMember]
        private Dictionary<Bookmark, IComparable> bookmarkQueueMap;
        private Dictionary<int, System.Workflow.ComponentModel.Activity> contextActivityMap;
        private System.Workflow.ComponentModel.Activity currentActivity;
        private System.Workflow.ComponentModel.Activity currentAtomicActivity;
        [DataMember]
        private int currentContextId;
        [DataMember]
        private int eventCounter;
        private bool hasCheckedForTrackingParticipant;
        [DataMember]
        private Guid instanceId;
        private System.Workflow.ComponentModel.Activity internalCurrentActivity;
        [DataMember(EmitDefaultValue=false)]
        private int internalCurrentActivityContextId;
        [DataMember(EmitDefaultValue=false)]
        private string internalCurrentActivityName;
        private Exception lastExceptionThrown;
        private IList<PropertyInfo> outputProperties;
        private IDictionary<string, object> outputs;
        private Exception outstandingException;
        [DataMember]
        private byte[] persistedActivityData;
        private VolatileResourceManager resourceManager;
        private System.Workflow.ComponentModel.Activity rootActivity;
        private Scheduler scheduler;
        private ServiceEnvironment serviceEnvironment;
        private TimerEventSubscriptionCollection timerQueue;
        private TimerSchedulerService timerSchedulerSerivce;
        private bool trackingEnabled;
        private WorkflowQueuingService workflowQueuingService;

        public InteropExecutor(Guid instanceId, System.Workflow.ComponentModel.Activity rootActivity, IList<PropertyInfo> outputProperties, System.Workflow.ComponentModel.Activity activityDefinition)
        {
            this.PrivateInitialize(rootActivity, instanceId, outputProperties, activityDefinition);
        }

        public void ActivityStatusChanged(System.Workflow.ComponentModel.Activity activity, bool transacted, bool committed)
        {
            if (!committed)
            {
                if (this.trackingEnabled)
                {
                    this.ServiceProvider.TrackActivityStatusChange(activity, this.eventCounter++);
                }
                if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    this.ScheduleDelayedItems(activity);
                }
            }
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Closed) && (!(activity is ICompensatableActivity) || ((activity is ICompensatableActivity) && activity.CanUninitializeNow)))
            {
                CorrelationTokenCollection.UninitializeCorrelationTokens(activity);
            }
        }

        private void AddItemToBeScheduledLater(System.Workflow.ComponentModel.Activity atomicActivity, SchedulableItem item)
        {
            if ((atomicActivity != null) && atomicActivity.SupportsTransaction)
            {
                TransactionalProperties properties = (TransactionalProperties) atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                if (properties != null)
                {
                    lock (properties)
                    {
                        List<SchedulableItem> itemsToBeScheduledAtCompletion = null;
                        itemsToBeScheduledAtCompletion = properties.ItemsToBeScheduledAtCompletion;
                        if (itemsToBeScheduledAtCompletion == null)
                        {
                            itemsToBeScheduledAtCompletion = new List<SchedulableItem>();
                            properties.ItemsToBeScheduledAtCompletion = itemsToBeScheduledAtCompletion;
                        }
                        itemsToBeScheduledAtCompletion.Add(item);
                    }
                }
            }
        }

        public ActivityExecutionStatus Cancel()
        {
            using (ActivityExecutionContext context = new ActivityExecutionContext(this.rootActivity, true))
            {
                if (this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    context.CancelActivity(this.rootActivity);
                }
            }
            this.scheduler.Run();
            return this.TranslateExecutionStatus();
        }

        public bool CheckAndProcessTransactionAborted(TransactionalProperties transactionalProperties)
        {
            if ((transactionalProperties.Transaction != null) && (transactionalProperties.Transaction.TransactionInformation.Status != TransactionStatus.Aborted))
            {
                return false;
            }
            if (transactionalProperties.TransactionState != TransactionProcessState.AbortProcessed)
            {
                this.scheduler.Pause();
                transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
            }
            return true;
        }

        public void CheckpointInstanceState(System.Workflow.ComponentModel.Activity atomicActivity)
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            WorkflowTransactionOptions options2 = TransactedContextFilter.GetTransactionOptions(atomicActivity);
            transactionOptions.IsolationLevel = options2.IsolationLevel;
            if (transactionOptions.IsolationLevel == IsolationLevel.Unspecified)
            {
                transactionOptions.IsolationLevel = IsolationLevel.Serializable;
            }
            transactionOptions.Timeout = options2.TimeoutDuration;
            TransactionalProperties properties = new TransactionalProperties();
            atomicActivity.SetValue(WorkflowExecutor.TransactionalPropertiesProperty, properties);
            this.ServiceProvider.CreateTransaction(transactionOptions);
            this.currentAtomicActivity = atomicActivity;
            this.scheduler.Pause();
        }

        public void ClearAmbientTransactionAndServiceEnvironment()
        {
            try
            {
                if (this.resourceManager.IsBatchDirty)
                {
                    this.ServiceProvider.AddResourceManager(this.resourceManager);
                }
                if (this.currentAtomicActivity != null)
                {
                    TransactionalProperties properties = (TransactionalProperties) this.currentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                    properties.Transaction = null;
                    if (properties.TransactionScope != null)
                    {
                        properties.TransactionScope.Complete();
                        properties.TransactionScope.Dispose();
                        properties.TransactionScope = null;
                    }
                }
            }
            finally
            {
                ((IDisposable) this.serviceEnvironment).Dispose();
                this.serviceEnvironment = null;
            }
        }

        private static System.Workflow.ComponentModel.Activity ContextActivity(System.Workflow.ComponentModel.Activity activity)
        {
            System.Workflow.ComponentModel.Activity parent = activity;
            while ((parent != null) && (parent.GetValue(System.Workflow.ComponentModel.Activity.ActivityExecutionContextInfoProperty) == null))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        private static int ContextId(System.Workflow.ComponentModel.Activity activity)
        {
            return ((ActivityExecutionContextInfo) ContextActivity(activity).GetValue(System.Workflow.ComponentModel.Activity.ActivityExecutionContextInfoProperty)).ContextId;
        }

        public void DisposeCheckpointState()
        {
        }

        public ActivityExecutionStatus EnqueueEvent(IComparable queueName, object item)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            this.workflowQueuingService.EnqueueEvent(queueName, item);
            try
            {
                Guid timerSubscriptionId = new Guid(queueName.ToString());
                this.TimerQueue.Remove(timerSubscriptionId);
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            this.scheduler.Run();
            return this.TranslateExecutionStatus();
        }

        internal void EnsureReload(Interop activity)
        {
            if (this.rootActivity == null)
            {
                this.Reload(activity.ComponentModelActivity, activity.OutputPropertyDefinitions);
            }
        }

        public ActivityExecutionStatus Execute()
        {
            using (ActivityExecutionContext context = new ActivityExecutionContext(this.rootActivity, true))
            {
                context.ExecuteActivity(this.rootActivity);
            }
            this.scheduler.Run();
            return this.TranslateExecutionStatus();
        }

        public System.Workflow.ComponentModel.Activity GetContextActivityForId(int id)
        {
            return this.contextActivityMap[id];
        }

        public int GetNewContextActivityId()
        {
            return this.currentContextId++;
        }

        public object GetService(System.Workflow.ComponentModel.Activity currentActivity, Type serviceType)
        {
            if (serviceType == typeof(IWorkflowCoreRuntime))
            {
                return this;
            }
            if (serviceType == typeof(WorkflowQueuingService))
            {
                this.workflowQueuingService.CallingActivity = ContextActivity(currentActivity);
                return this.workflowQueuingService;
            }
            if (!(serviceType == typeof(ITimerService)))
            {
                return ((IServiceProvider) this.ServiceProvider).GetService(serviceType);
            }
            if (this.timerSchedulerSerivce == null)
            {
                this.timerSchedulerSerivce = new TimerSchedulerService(this);
            }
            return this.timerSchedulerSerivce;
        }

        public void Initialize(System.Workflow.ComponentModel.Activity definition, IDictionary<string, object> inputs, bool hasNameCollision)
        {
            this.rootActivity.SetValue(System.Workflow.ComponentModel.Activity.ActivityExecutionContextInfoProperty, new ActivityExecutionContextInfo(this.rootActivity.QualifiedName, this.GetNewContextActivityId(), this.instanceId, -1));
            this.rootActivity.SetValue(System.Workflow.ComponentModel.Activity.ActivityContextGuidProperty, this.instanceId);
            SetInputParameters(definition, this.rootActivity, inputs, hasNameCollision);
            ((IDependencyObjectAccessor) this.rootActivity).InitializeActivatingInstanceForRuntime(null, this);
            this.rootActivity.FixUpMetaProperties(definition);
            this.TimerQueue = new TimerEventSubscriptionCollection(this, this.instanceId);
            using (new ServiceEnvironment(this.rootActivity))
            {
                using (this.SetCurrentActivity(this.rootActivity))
                {
                    this.RegisterContextActivity(this.rootActivity);
                    using (ActivityExecutionContext context = new ActivityExecutionContext(this.rootActivity, true))
                    {
                        context.InitializeActivity(this.rootActivity);
                    }
                }
            }
        }

        public bool IsActivityInAtomicContext(System.Workflow.ComponentModel.Activity activity, out System.Workflow.ComponentModel.Activity atomicActivity)
        {
            atomicActivity = null;
            while (activity != null)
            {
                if (activity == this.currentAtomicActivity)
                {
                    atomicActivity = activity;
                    return true;
                }
                activity = activity.Parent;
            }
            return false;
        }

        public System.Workflow.ComponentModel.Activity LoadContextActivity(ActivityExecutionContextInfo contextInfo, System.Workflow.ComponentModel.Activity outerContextActivity)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        public void OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        public bool OnBeforeDynamicChange(IList<WorkflowChangeAction> changes)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (this.rootActivity != null)
            {
                using (MemoryStream stream = new MemoryStream(0x2800))
                {
                    stream.Position = 0L;
                    this.rootActivity.Save(stream);
                    this.persistedActivityData = stream.GetBuffer();
                    Array.Resize<byte>(ref this.persistedActivityData, Convert.ToInt32(stream.Length));
                }
                if (this.internalCurrentActivity != null)
                {
                    this.internalCurrentActivityContextId = this.internalCurrentActivity.ContextId;
                    this.internalCurrentActivityName = this.internalCurrentActivity.QualifiedName;
                }
                if (this.CurrentAtomicActivity != null)
                {
                    this.atomicActivityContextId = this.CurrentAtomicActivity.ContextId;
                    this.atomicActivityName = this.CurrentAtomicActivity.QualifiedName;
                }
            }
        }

        public void PersistInstanceState(System.Workflow.ComponentModel.Activity activity)
        {
            this.lastExceptionThrown = null;
            this.abortTransaction = false;
            this.ScheduleDelayedItems(activity);
            if (this.currentAtomicActivity == null)
            {
                if ((activity == this.rootActivity) && !activity.PersistOnClose)
                {
                    return;
                }
                this.ServiceProvider.Persist();
            }
            else
            {
                TransactionalProperties transactionalProperties = null;
                transactionalProperties = (TransactionalProperties) activity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                if (this.CheckAndProcessTransactionAborted(transactionalProperties))
                {
                    return;
                }
                transactionalProperties.TransactionScope.Complete();
                transactionalProperties.TransactionScope.Dispose();
                transactionalProperties.TransactionScope = null;
                this.ServiceProvider.CommitTransaction();
                transactionalProperties.Transaction = null;
                this.currentAtomicActivity = null;
            }
            this.internalCurrentActivity = activity;
            this.scheduler.Pause();
        }

        private void PrivateInitialize(System.Workflow.ComponentModel.Activity rootActivity, Guid instanceId, IList<PropertyInfo> outputProperties, System.Workflow.ComponentModel.Activity workflowDefinition)
        {
            this.instanceId = instanceId;
            this.rootActivity = rootActivity;
            this.contextActivityMap = new Dictionary<int, System.Workflow.ComponentModel.Activity>();
            this.scheduler = new Scheduler(this);
            this.workflowQueuingService = new WorkflowQueuingService(this);
            this.outputProperties = outputProperties;
            this.resourceManager = new VolatileResourceManager();
            this.rootActivity.SetValue(System.Workflow.ComponentModel.Activity.WorkflowDefinitionProperty, workflowDefinition);
            this.rootActivity.SetValue(WorkflowExecutor.WorkflowExecutorProperty, this);
        }

        private void ProcessTimers(object ignored)
        {
        }

        public void RaiseActivityExecuting(System.Workflow.ComponentModel.Activity activity)
        {
        }

        public void RaiseException(Exception e, System.Workflow.ComponentModel.Activity activity, string responsibleActivity)
        {
            using (this.SetCurrentActivity(activity))
            {
                using (ActivityExecutionContext context = new ActivityExecutionContext(activity, true))
                {
                    context.FaultActivity(e);
                }
            }
        }

        public void RaiseHandlerInvoked()
        {
        }

        public void RaiseHandlerInvoking(Delegate delegateHandler)
        {
        }

        public void RegisterContextActivity(System.Workflow.ComponentModel.Activity activity)
        {
            int key = ContextId(activity);
            this.contextActivityMap.Add(key, activity);
            activity.OnActivityExecutionContextLoad(this);
        }

        public void Reload(System.Workflow.ComponentModel.Activity definitionActivity, IList<PropertyInfo> outputProperties)
        {
            MemoryStream stream = new MemoryStream(this.persistedActivityData);
            System.Workflow.ComponentModel.Activity rootActivity = null;
            stream.Position = 0L;
            using (new ActivityDefinitionResolution(definitionActivity))
            {
                rootActivity = System.Workflow.ComponentModel.Activity.Load(stream, null);
            }
            this.PrivateInitialize(rootActivity, this.instanceId, outputProperties, definitionActivity);
            Queue<System.Workflow.ComponentModel.Activity> queue = new Queue<System.Workflow.ComponentModel.Activity>();
            queue.Enqueue(rootActivity);
            while (queue.Count > 0)
            {
                System.Workflow.ComponentModel.Activity activity = queue.Dequeue();
                ((IDependencyObjectAccessor) activity).InitializeInstanceForRuntime(this);
                this.RegisterContextActivity(activity);
                IList<System.Workflow.ComponentModel.Activity> list = (IList<System.Workflow.ComponentModel.Activity>) activity.GetValue(System.Workflow.ComponentModel.Activity.ActiveExecutionContextsProperty);
                if (list != null)
                {
                    foreach (System.Workflow.ComponentModel.Activity activity3 in list)
                    {
                        queue.Enqueue(activity3);
                    }
                }
            }
            if (!string.IsNullOrEmpty(this.internalCurrentActivityName))
            {
                this.internalCurrentActivity = this.GetContextActivityForId(this.internalCurrentActivityContextId).GetActivityByName(this.internalCurrentActivityName);
            }
            if (!string.IsNullOrEmpty(this.atomicActivityName))
            {
                this.currentAtomicActivity = this.GetContextActivityForId(this.atomicActivityContextId).GetActivityByName(this.atomicActivityName);
            }
            this.TimerQueue.Executor = this;
        }

        public void RequestRevertToCheckpointState(System.Workflow.ComponentModel.Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendReason)
        {
            if (this.lastExceptionThrown != null)
            {
                this.abortTransaction = true;
                this.scheduler.Pause();
            }
        }

        public ActivityExecutionStatus Resume()
        {
            this.scheduler.Run();
            return this.TranslateExecutionStatus();
        }

        public void SaveContextActivity(System.Workflow.ComponentModel.Activity contextActivity)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        private void ScheduleDelayedItems(System.Workflow.ComponentModel.Activity atomicActivity)
        {
            List<SchedulableItem> itemsToBeScheduledAtCompletion = null;
            TransactionalProperties properties = (TransactionalProperties) atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
            if (properties != null)
            {
                lock (properties)
                {
                    itemsToBeScheduledAtCompletion = properties.ItemsToBeScheduledAtCompletion;
                    if (itemsToBeScheduledAtCompletion != null)
                    {
                        foreach (SchedulableItem item in itemsToBeScheduledAtCompletion)
                        {
                            this.scheduler.ScheduleItem(item, false);
                        }
                        itemsToBeScheduledAtCompletion.Clear();
                        properties.ItemsToBeScheduledAtCompletion = null;
                    }
                }
            }
        }

        public void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction)
        {
            if (queueInTransaction)
            {
                this.AddItemToBeScheduledLater(this.CurrentActivity, item);
            }
            else
            {
                this.scheduler.ScheduleItem(item, isInAtomicTransaction);
            }
        }

        public void SetAmbientTransactionAndServiceEnvironment(Transaction transaction)
        {
            this.serviceEnvironment = new ServiceEnvironment(this.RootActivity);
            if ((transaction != null) && (this.currentAtomicActivity != null))
            {
                TransactionalProperties properties = (TransactionalProperties) this.currentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                properties.Transaction = transaction;
                properties.TransactionScope = new System.Transactions.TransactionScope(properties.Transaction, TimeSpan.Zero, EnterpriseServicesInteropOption.Full);
            }
        }

        public IDisposable SetCurrentActivity(System.Workflow.ComponentModel.Activity activity)
        {
            System.Workflow.ComponentModel.Activity currentActivity = this.CurrentActivity;
            this.CurrentActivity = activity;
            return new ResetCurrentActivity(this, currentActivity);
        }

        private static void SetInputParameters(System.Workflow.ComponentModel.Activity definition, System.Workflow.ComponentModel.Activity rootActivity, IDictionary<string, object> inputs, bool hasNameCollision)
        {
            if (inputs != null)
            {
                int length = "In".Length;
                foreach (KeyValuePair<string, object> pair in inputs)
                {
                    PropertyInfo property;
                    if (hasNameCollision)
                    {
                        string name = pair.Key.Substring(0, pair.Key.Length - length);
                        property = definition.GetType().GetProperty(name);
                    }
                    else
                    {
                        property = definition.GetType().GetProperty(pair.Key);
                    }
                    if ((property != null) && property.CanWrite)
                    {
                        property.SetValue(rootActivity, pair.Value, null);
                    }
                }
            }
        }

        public Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        public bool SuspendInstance(string suspendDescription)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(this.rootActivity, serviceType);
        }

        bool IWorkflowCoreRuntime.Resume()
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, new object[] { this.ServiceProvider.Activity.DisplayName }));
        }

        public void TerminateInstance(Exception e)
        {
            this.outstandingException = e;
        }

        public void Track(string key, object data)
        {
            if (this.trackingEnabled)
            {
                this.ServiceProvider.TrackData(this.CurrentActivity, this.eventCounter++, key, data);
            }
        }

        private ActivityExecutionStatus TranslateExecutionStatus()
        {
            if (this.abortTransaction)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0x467, string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropExceptionTraceMessage, new object[] { this.ServiceProvider.Activity.DisplayName, this.lastExceptionThrown.ToString() }));
                throw this.lastExceptionThrown;
            }
            if (this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                if (this.outputProperties.Count != 0)
                {
                    this.outputs = new Dictionary<string, object>(this.outputProperties.Count);
                    foreach (PropertyInfo info in this.outputProperties)
                    {
                        if (info.CanRead && (info.GetGetMethod() != null))
                        {
                            this.outputs.Add(info.Name + "Out", info.GetValue(this.rootActivity, null));
                        }
                    }
                }
                if (this.outstandingException != null)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0x467, string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropExceptionTraceMessage, new object[] { this.ServiceProvider.Activity.DisplayName, this.outstandingException.ToString() }));
                    throw this.outstandingException;
                }
            }
            return this.rootActivity.ExecutionStatus;
        }

        public void UnregisterContextActivity(System.Workflow.ComponentModel.Activity activity)
        {
            int key = ContextId(activity);
            this.contextActivityMap.Remove(key);
            activity.OnActivityExecutionContextUnload(this);
        }

        public WorkBatchCollection BatchCollection
        {
            get
            {
                return this.resourceManager.BatchCollection;
            }
        }

        public Dictionary<Bookmark, IComparable> BookmarkQueueMap
        {
            get
            {
                if (this.bookmarkQueueMap == null)
                {
                    this.bookmarkQueueMap = new Dictionary<Bookmark, IComparable>();
                }
                return this.bookmarkQueueMap;
            }
        }

        public System.Workflow.ComponentModel.Activity CurrentActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentActivity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.currentActivity = value;
            }
        }

        public System.Workflow.ComponentModel.Activity CurrentAtomicActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.currentAtomicActivity;
            }
        }

        public bool HasCheckedForTrackingParticipant
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hasCheckedForTrackingParticipant;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.hasCheckedForTrackingParticipant = value;
            }
        }

        public Guid InstanceID
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instanceId;
            }
        }

        public bool IsDynamicallyUpdated
        {
            get
            {
                return false;
            }
        }

        public IDictionary<string, object> Outputs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outputs;
            }
        }

        public WaitCallback ProcessTimersCallback
        {
            get
            {
                return new WaitCallback(this.ProcessTimers);
            }
        }

        public IEnumerable<IComparable> Queues
        {
            get
            {
                return this.workflowQueuingService.QueueNames;
            }
        }

        public System.Workflow.ComponentModel.Activity RootActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootActivity;
            }
        }

        public InteropEnvironment ServiceProvider
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ServiceProvider>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<ServiceProvider>k__BackingField = value;
            }
        }

        private TimerEventSubscriptionCollection TimerQueue
        {
            get
            {
                if (this.timerQueue == null)
                {
                    this.timerQueue = (TimerEventSubscriptionCollection) this.rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
                }
                return this.timerQueue;
            }
            set
            {
                this.timerQueue = value;
                this.rootActivity.SetValue(TimerEventSubscriptionCollection.TimerCollectionProperty, this.timerQueue);
            }
        }

        public bool TrackingEnabled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.trackingEnabled;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.trackingEnabled = value;
            }
        }

        private class ActivityDefinitionResolution : IDisposable
        {
            private static ActivityResolveEventHandler activityResolveEventHandler = new ActivityResolveEventHandler(InteropExecutor.ActivityDefinitionResolution.OnActivityResolve);
            [ThreadStatic]
            private static System.Workflow.ComponentModel.Activity definitionActivity;

            static ActivityDefinitionResolution()
            {
                System.Workflow.ComponentModel.Activity.ActivityResolve += activityResolveEventHandler;
            }

            public ActivityDefinitionResolution(System.Workflow.ComponentModel.Activity definitionActivity)
            {
                InteropExecutor.ActivityDefinitionResolution.definitionActivity = definitionActivity;
            }

            private static System.Workflow.ComponentModel.Activity OnActivityResolve(object sender, ActivityResolveEventArgs e)
            {
                return definitionActivity;
            }

            void IDisposable.Dispose()
            {
                definitionActivity = null;
            }
        }

        private class ResetCurrentActivity : IDisposable
        {
            private InteropExecutor activityExecutor;
            private System.Workflow.ComponentModel.Activity oldCurrentActivity;

            internal ResetCurrentActivity(InteropExecutor activityExecutor, System.Workflow.ComponentModel.Activity oldCurrentActivity)
            {
                this.activityExecutor = activityExecutor;
                this.oldCurrentActivity = oldCurrentActivity;
            }

            void IDisposable.Dispose()
            {
                this.activityExecutor.CurrentActivity = this.oldCurrentActivity;
            }
        }

        private class Scheduler
        {
            private Queue<SchedulableItem> atomicActivityQueue;
            internal static DependencyProperty AtomicActivityQueueProperty = DependencyProperty.RegisterAttached("AtomicActivityQueue", typeof(Queue<SchedulableItem>), typeof(InteropExecutor.Scheduler));
            private InteropExecutor owner;
            private bool pause;
            private Queue<SchedulableItem> schedulerQueue;
            internal static DependencyProperty SchedulerQueueProperty = DependencyProperty.RegisterAttached("SchedulerQueue", typeof(Queue<SchedulableItem>), typeof(InteropExecutor.Scheduler));

            public Scheduler(InteropExecutor owner)
            {
                this.owner = owner;
                this.schedulerQueue = (Queue<SchedulableItem>) owner.RootActivity.GetValue(SchedulerQueueProperty);
                if (this.schedulerQueue == null)
                {
                    this.schedulerQueue = new Queue<SchedulableItem>();
                    owner.RootActivity.SetValue(SchedulerQueueProperty, this.schedulerQueue);
                }
                this.atomicActivityQueue = (Queue<SchedulableItem>) owner.RootActivity.GetValue(AtomicActivityQueueProperty);
                if (this.atomicActivityQueue == null)
                {
                    this.atomicActivityQueue = new Queue<SchedulableItem>();
                    owner.RootActivity.SetValue(AtomicActivityQueueProperty, this.atomicActivityQueue);
                }
            }

            public void Pause()
            {
                this.pause = true;
            }

            public void Run()
            {
                this.pause = false;
                while (!this.pause)
                {
                    SchedulableItem item;
                    System.Workflow.ComponentModel.Activity activity2;
                    if (this.atomicActivityQueue.Count > 0)
                    {
                        item = this.atomicActivityQueue.Dequeue();
                    }
                    else
                    {
                        if ((this.owner.CurrentAtomicActivity != null) || (this.schedulerQueue.Count <= 0))
                        {
                            break;
                        }
                        item = this.schedulerQueue.Dequeue();
                    }
                    System.Workflow.ComponentModel.Activity activityByName = this.owner.GetContextActivityForId(item.ContextId).GetActivityByName(item.ActivityId);
                    TransactionalProperties transactionalProperties = null;
                    if (this.owner.IsActivityInAtomicContext(activityByName, out activity2))
                    {
                        transactionalProperties = (TransactionalProperties) activity2.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                        if (this.owner.CheckAndProcessTransactionAborted(transactionalProperties))
                        {
                            return;
                        }
                    }
                    try
                    {
                        item.Run(this.owner);
                        continue;
                    }
                    catch (Exception exception)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(exception))
                        {
                            throw;
                        }
                        if (transactionalProperties != null)
                        {
                            transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                            this.owner.lastExceptionThrown = exception;
                        }
                        this.owner.RaiseException(exception, activityByName, null);
                        continue;
                    }
                }
            }

            public void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction)
            {
                (isInAtomicTransaction ? this.atomicActivityQueue : this.schedulerQueue).Enqueue(item);
            }
        }

        private class TimerSchedulerService : ITimerService
        {
            private IWorkflowCoreRuntime executor;

            public TimerSchedulerService(IWorkflowCoreRuntime executor)
            {
                this.executor = executor;
            }

            public void CancelTimer(Guid timerId)
            {
                if (timerId == Guid.Empty)
                {
                    throw new ArgumentException(ExecutionStringManager.InteropTimerIdCantBeEmpty, "timerId");
                }
                this.GetTimerExtension().CancelTimer(new Bookmark(timerId.ToString()));
            }

            private TimerExtension GetTimerExtension()
            {
                TimerExtension service = this.executor.GetService(typeof(TimerExtension)) as TimerExtension;
                if (service == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InteropCantFindTimerExtension);
                }
                return service;
            }

            public void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
            {
                if (timerId == Guid.Empty)
                {
                    throw new ArgumentException(ExecutionStringManager.InteropTimerIdCantBeEmpty, "timerId");
                }
                TimeSpan timeout = (TimeSpan) (whenUtc - DateTime.UtcNow);
                if (timeout < TimeSpan.Zero)
                {
                    timeout = TimeSpan.Zero;
                }
                this.GetTimerExtension().RegisterTimer(timeout, new Bookmark(timerId.ToString()));
            }
        }
    }
}

