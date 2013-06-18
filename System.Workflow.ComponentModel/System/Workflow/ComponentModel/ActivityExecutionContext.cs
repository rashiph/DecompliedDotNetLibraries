namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;

    public sealed class ActivityExecutionContext : IServiceProvider, IDisposable
    {
        private bool allowSignalsOnCurrentActivity;
        internal static readonly DependencyProperty CachedGrantedLocksProperty = DependencyProperty.RegisterAttached("CachedGrantedLocks", typeof(Dictionary<string, GrantedLock>), typeof(ActivityExecutionContext), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        private ActivityExecutionContextManager contextManager;
        private System.Workflow.ComponentModel.Activity currentActivity;
        public static readonly DependencyProperty CurrentExceptionProperty = DependencyProperty.RegisterAttached("CurrentException", typeof(Exception), typeof(ActivityExecutionContext), new PropertyMetadata(null, DependencyPropertyOptions.Default, null, new SetValueOverride(ActivityExecutionContext.EnforceExceptionSemantics), true, new Attribute[0]));
        internal static readonly DependencyProperty GrantedLocksProperty = DependencyProperty.RegisterAttached("GrantedLocks", typeof(Dictionary<string, GrantedLock>), typeof(ActivityExecutionContext));
        private static Type loaderServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowLoaderService, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL");
        internal static readonly DependencyProperty LockAcquiredCallbackProperty = DependencyProperty.RegisterAttached("LockAcquiredCallback", typeof(ActivityExecutorDelegateInfo<EventArgs>), typeof(ActivityExecutionContext));
        private static Type persistenceServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowPersistenceService, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL");
        private static Type schedulerServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowSchedulerService, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL");
        private IStartWorkflow startWorkflowService;
        private static Type trackingServiceType = Type.GetType("System.Workflow.Runtime.Tracking.TrackingService, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL");
        private static Type transactionServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowCommitWorkBatchService, System.Workflow.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL");

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ActivityExecutionContext(System.Workflow.ComponentModel.Activity activity)
        {
            this.currentActivity = activity;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ActivityExecutionContext(System.Workflow.ComponentModel.Activity activity, bool allowSignalsOnCurrentActivity) : this(activity)
        {
            this.allowSignalsOnCurrentActivity = allowSignalsOnCurrentActivity;
        }

        private bool AcquireLocks(System.Workflow.ComponentModel.Activity activity)
        {
            ICollection<string> allSynchronizationHandles = this.GetAllSynchronizationHandles(activity);
            if ((allSynchronizationHandles != null) && (allSynchronizationHandles.Count != 0))
            {
                for (System.Workflow.ComponentModel.Activity activity2 = activity.Parent; activity2 != null; activity2 = activity2.Parent)
                {
                    if (activity2.SupportsSynchronization || (activity2.Parent == null))
                    {
                        Dictionary<string, GrantedLock> dictionary = (Dictionary<string, GrantedLock>) activity2.GetValue(GrantedLocksProperty);
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, GrantedLock>();
                            activity2.SetValue(GrantedLocksProperty, dictionary);
                        }
                        foreach (string str in allSynchronizationHandles)
                        {
                            bool flag = true;
                            if (!dictionary.ContainsKey(str))
                            {
                                dictionary[str] = new GrantedLock(activity);
                            }
                            else if (dictionary[str].Holder != activity)
                            {
                                dictionary[str].WaitList.Add(activity);
                                flag = false;
                            }
                            if (!flag)
                            {
                                return false;
                            }
                        }
                    }
                    ICollection<string> is3 = (ICollection<string>) activity2.GetValue(System.Workflow.ComponentModel.Activity.SynchronizationHandlesProperty);
                    if ((is3 != null) && (is3.Count != 0))
                    {
                        break;
                    }
                }
            }
            return true;
        }

        internal bool AcquireLocks(IActivityEventListener<EventArgs> locksAcquiredCallback)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.Activity.SetValue(LockAcquiredCallbackProperty, new ActivityExecutorDelegateInfo<EventArgs>(true, locksAcquiredCallback, this.Activity.ContextActivity));
            return this.AcquireLocks(this.Activity);
        }

        public void CancelActivity(System.Workflow.ComponentModel.Activity activity)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.allowSignalsOnCurrentActivity && ((this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Initialized) || (this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Closed)))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidStateToExecuteChild"));
            }
            if (!this.IsValidChild(activity, false))
            {
                throw new ArgumentException(SR.GetString("AEC_InvalidActivity"), "activity");
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Executing)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidCancelingState"));
            }
            try
            {
                activity.SetStatus(ActivityExecutionStatus.Canceling, false);
            }
            finally
            {
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Cancel, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }

        internal void CheckpointInstanceState()
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.currentActivity.WorkflowCoreRuntime.CheckpointInstanceState(this.currentActivity);
        }

        public void CloseActivity()
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            switch (this.currentActivity.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    this.currentActivity.MarkCompleted();
                    return;

                case ActivityExecutionStatus.Canceling:
                    this.currentActivity.MarkCanceled();
                    return;

                case ActivityExecutionStatus.Closed:
                    return;

                case ActivityExecutionStatus.Compensating:
                    this.currentActivity.MarkCompensated();
                    return;

                case ActivityExecutionStatus.Faulting:
                    this.currentActivity.MarkFaulted();
                    return;
            }
            throw new InvalidOperationException(SR.GetString("Error_InvalidClosingState"));
        }

        internal void CompensateActivity(System.Workflow.ComponentModel.Activity activity)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.IsValidNestedChild(activity))
            {
                throw new ArgumentException(SR.GetString("AEC_InvalidNestedActivity"), "activity");
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidCompensatingState"));
            }
            try
            {
                activity.SetStatus(ActivityExecutionStatus.Compensating, false);
            }
            finally
            {
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Compensate, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }

        internal void DisposeCheckpointState()
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.currentActivity.WorkflowCoreRuntime.DisposeCheckpointState();
        }

        private static void EnforceExceptionSemantics(DependencyObject d, object value)
        {
            if (!(d is System.Workflow.ComponentModel.Activity))
            {
                throw new ArgumentException(SR.GetString(CultureInfo.CurrentCulture, "Error_DOIsNotAnActivity"));
            }
            if (value != null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_PropertyCanBeOnlyCleared"));
            }
            d.SetValueCommon(CurrentExceptionProperty, null, CurrentExceptionProperty.DefaultMetadata, false);
        }

        public void ExecuteActivity(System.Workflow.ComponentModel.Activity activity)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.allowSignalsOnCurrentActivity && ((this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Initialized) || (this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Closed)))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidStateToExecuteChild"));
            }
            if (!this.IsValidChild(activity, false))
            {
                throw new ArgumentException(SR.GetString("AEC_InvalidActivity"), "activity");
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidExecutionState"));
            }
            try
            {
                activity.SetStatus(ActivityExecutionStatus.Executing, false);
            }
            finally
            {
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Execute, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }

        internal void FaultActivity(Exception e)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (this.currentActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                if (this.currentActivity.Parent == null)
                {
                    this.currentActivity.WorkflowCoreRuntime.TerminateInstance(e);
                }
                else
                {
                    this.currentActivity.WorkflowCoreRuntime.RaiseException(e, this.currentActivity.Parent, string.Empty);
                }
            }
            else
            {
                try
                {
                    this.currentActivity.SetValueCommon(CurrentExceptionProperty, e, CurrentExceptionProperty.DefaultMetadata, false);
                    this.currentActivity.SetStatus(ActivityExecutionStatus.Faulting, false);
                }
                finally
                {
                    this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(this.currentActivity, ActivityOperationType.HandleFault, this.ContextId, e), IsInAtomicTransaction(this.currentActivity), false, false);
                }
            }
        }

        private ICollection<string> GetAllSynchronizationHandles(System.Workflow.ComponentModel.Activity activity)
        {
            WalkerEventHandler handler = null;
            ICollection<string> is2 = (ICollection<string>) activity.GetValue(System.Workflow.ComponentModel.Activity.SynchronizationHandlesProperty);
            if ((is2 == null) || (is2.Count == 0))
            {
                return is2;
            }
            List<string> handles = new List<string>(is2);
            if (activity is CompositeActivity)
            {
                Walker walker = new Walker();
                if (handler == null)
                {
                    handler = delegate (Walker w, WalkerEventArgs e) {
                        if (e.CurrentActivity != activity)
                        {
                            ICollection<string> collection = (ICollection<string>) e.CurrentActivity.GetValue(System.Workflow.ComponentModel.Activity.SynchronizationHandlesProperty);
                            if (collection != null)
                            {
                                handles.AddRange(collection);
                            }
                        }
                    };
                }
                walker.FoundActivity += handler;
                walker.Walk(activity);
            }
            handles.Sort();
            for (int i = 1; i < handles.Count; i++)
            {
                if (handles[i] == handles[i - 1])
                {
                    handles.RemoveAt(--i);
                }
            }
            handles.TrimExcess();
            return handles;
        }

        public T GetService<T>()
        {
            return (T) this.GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (serviceType == typeof(IStartWorkflow))
            {
                if (this.startWorkflowService == null)
                {
                    this.startWorkflowService = new StartWorkflow(this);
                }
                return this.startWorkflowService;
            }
            if ((schedulerServiceType != null) && schedulerServiceType.IsAssignableFrom(serviceType))
            {
                return null;
            }
            if ((persistenceServiceType != null) && persistenceServiceType.IsAssignableFrom(serviceType))
            {
                return null;
            }
            if ((trackingServiceType != null) && trackingServiceType.IsAssignableFrom(serviceType))
            {
                return null;
            }
            if ((transactionServiceType != null) && transactionServiceType.IsAssignableFrom(serviceType))
            {
                return null;
            }
            if ((loaderServiceType != null) && loaderServiceType.IsAssignableFrom(serviceType))
            {
                return null;
            }
            return this.currentActivity.WorkflowCoreRuntime.GetService(this.currentActivity, serviceType);
        }

        internal void InitializeActivity(System.Workflow.ComponentModel.Activity activity)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.IsValidChild(activity, false))
            {
                throw new ArgumentException(SR.GetString("AEC_InvalidActivity"), "activity");
            }
            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidInitializingState"));
            }
            using (ActivityExecutionContext context = new ActivityExecutionContext(activity))
            {
                using (this.currentActivity.WorkflowCoreRuntime.SetCurrentActivity(activity))
                {
                    activity.Initialize(context);
                }
            }
        }

        internal void Invoke<T>(EventHandler<T> handler, T e) where T: EventArgs
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.currentActivity.Invoke<T>(handler, e);
        }

        internal static bool IsInAtomicTransaction(System.Workflow.ComponentModel.Activity activity)
        {
            while (activity != null)
            {
                if (activity == activity.WorkflowCoreRuntime.CurrentAtomicActivity)
                {
                    return true;
                }
                activity = activity.Parent;
            }
            return false;
        }

        internal bool IsValidChild(System.Workflow.ComponentModel.Activity activity, bool allowContextVariance)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (((activity != this.currentActivity.WorkflowCoreRuntime.CurrentActivity) || !this.allowSignalsOnCurrentActivity) && ((!activity.Enabled || (activity.Parent != this.currentActivity.WorkflowCoreRuntime.CurrentActivity)) || (!allowContextVariance && !activity.Equals(this.Activity.GetActivityByName(activity.QualifiedName, true)))))
            {
                return false;
            }
            return true;
        }

        internal bool IsValidNestedChild(System.Workflow.ComponentModel.Activity activity)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (activity == this.currentActivity)
            {
                return true;
            }
            System.Workflow.ComponentModel.Activity parent = activity;
            while (((parent != null) && parent.Enabled) && (parent.Parent != this.currentActivity.ContextActivity))
            {
                parent = parent.Parent;
            }
            return ((parent != null) && parent.Enabled);
        }

        internal void ReleaseLocks(bool transactional)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.Activity.RemoveProperty(LockAcquiredCallbackProperty);
            ICollection<string> allSynchronizationHandles = this.GetAllSynchronizationHandles(this.Activity);
            if ((allSynchronizationHandles != null) && (allSynchronizationHandles.Count != 0))
            {
                List<System.Workflow.ComponentModel.Activity> list = new List<System.Workflow.ComponentModel.Activity>();
                for (System.Workflow.ComponentModel.Activity activity = this.Activity.Parent; activity != null; activity = activity.Parent)
                {
                    if (activity.SupportsSynchronization || (activity.Parent == null))
                    {
                        Dictionary<string, GrantedLock> dictionary = (Dictionary<string, GrantedLock>) activity.GetValue(GrantedLocksProperty);
                        if (transactional)
                        {
                            Dictionary<string, GrantedLock> dictionary2 = new Dictionary<string, GrantedLock>();
                            if (dictionary != null)
                            {
                                foreach (KeyValuePair<string, GrantedLock> pair in dictionary)
                                {
                                    dictionary2.Add(pair.Key, (GrantedLock) pair.Value.Clone());
                                }
                            }
                            activity.SetValue(CachedGrantedLocksProperty, dictionary2);
                        }
                        if (dictionary != null)
                        {
                            foreach (string str in allSynchronizationHandles)
                            {
                                if (dictionary.ContainsKey(str))
                                {
                                    if (dictionary[str].WaitList.Count == 0)
                                    {
                                        dictionary.Remove(str);
                                    }
                                    else if (dictionary[str].Holder != this.Activity)
                                    {
                                        dictionary[str].WaitList.Remove(this.Activity);
                                    }
                                    else
                                    {
                                        System.Workflow.ComponentModel.Activity item = dictionary[str].WaitList[0];
                                        dictionary[str].WaitList.RemoveAt(0);
                                        dictionary[str].Holder = item;
                                        if (!list.Contains(item))
                                        {
                                            list.Add(item);
                                        }
                                    }
                                }
                            }
                            if (dictionary.Count == 0)
                            {
                                activity.RemoveProperty(GrantedLocksProperty);
                            }
                        }
                    }
                    ICollection<string> is3 = (ICollection<string>) activity.GetValue(System.Workflow.ComponentModel.Activity.SynchronizationHandlesProperty);
                    if ((is3 != null) && (is3.Count != 0))
                    {
                        break;
                    }
                }
                foreach (System.Workflow.ComponentModel.Activity activity3 in list)
                {
                    if (this.AcquireLocks(activity3))
                    {
                        ((ActivityExecutorDelegateInfo<EventArgs>) activity3.GetValue(LockAcquiredCallbackProperty)).InvokeDelegate(this.Activity.ContextActivity, EventArgs.Empty, false, transactional);
                    }
                }
            }
        }

        internal void RequestRevertToCheckpointState(EventHandler<EventArgs> handler, EventArgs data, bool suspendOnRevert, string suspendOnRevertInfo)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.currentActivity.WorkflowCoreRuntime.RequestRevertToCheckpointState(this.currentActivity, handler, data, suspendOnRevert, suspendOnRevertInfo);
        }

        internal void SuspendWorkflowInstance(string suspendDescription)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            this.currentActivity.WorkflowCoreRuntime.SuspendInstance(suspendDescription);
        }

        void IDisposable.Dispose()
        {
            if (this.currentActivity != null)
            {
                if (this.contextManager != null)
                {
                    this.contextManager.Dispose();
                    this.contextManager = null;
                }
                this.currentActivity = null;
            }
        }

        internal void TerminateWorkflowInstance(Exception e)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            this.currentActivity.WorkflowCoreRuntime.TerminateInstance(e);
        }

        public void TrackData(object userData)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            this.currentActivity.WorkflowCoreRuntime.Track(null, userData);
        }

        public void TrackData(string userDataKey, object userData)
        {
            if (this.currentActivity == null)
            {
                throw new ObjectDisposedException("ActivityExecutionContext");
            }
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }
            this.currentActivity.WorkflowCoreRuntime.Track(userDataKey, userData);
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            get
            {
                if (this.currentActivity == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContext");
                }
                return this.currentActivity;
            }
        }

        public Guid ContextGuid
        {
            get
            {
                if (this.currentActivity == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContext");
                }
                return this.currentActivity.ContextActivity.ContextGuid;
            }
        }

        internal int ContextId
        {
            get
            {
                if (this.currentActivity == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContext");
                }
                return this.currentActivity.ContextActivity.ContextId;
            }
        }

        public ActivityExecutionContextManager ExecutionContextManager
        {
            get
            {
                if (this.currentActivity == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContext");
                }
                if (this.contextManager == null)
                {
                    this.contextManager = new ActivityExecutionContextManager(this);
                }
                return this.contextManager;
            }
        }

        internal IWorkflowCoreRuntime WorkflowCoreRuntime
        {
            get
            {
                if (this.currentActivity == null)
                {
                    throw new ObjectDisposedException("ActivityExecutionContext");
                }
                return this.GetService<IWorkflowCoreRuntime>();
            }
        }

        internal sealed class StartWorkflow : IStartWorkflow
        {
            private ActivityExecutionContext executionContext;

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal StartWorkflow(ActivityExecutionContext executionContext)
            {
                this.executionContext = executionContext;
            }

            Guid IStartWorkflow.StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
            {
                return this.executionContext.WorkflowCoreRuntime.StartWorkflow(workflowType, namedArgumentValues);
            }
        }
    }
}

