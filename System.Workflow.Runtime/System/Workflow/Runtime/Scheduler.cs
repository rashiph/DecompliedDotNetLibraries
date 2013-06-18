namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Transactions;
    using System.Workflow.ComponentModel;

    internal class Scheduler
    {
        private bool abortOrTerminateRequested;
        private bool canRun;
        private bool empty;
        private Queue<SchedulableItem> highPriorityEntriesQueue;
        internal static DependencyProperty HighPriorityEntriesQueueProperty = DependencyProperty.RegisterAttached("HighPriorityEntriesQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));
        private Queue<SchedulableItem> normalPriorityEntriesQueue;
        internal static DependencyProperty NormalPriorityEntriesQueueProperty = DependencyProperty.RegisterAttached("NormalPriorityEntriesQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));
        private WorkflowExecutor rootWorkflowExecutor;
        private object syncObject = new object();
        private bool threadRequested;
        private Queue<SchedulableItem> transactedEntries;

        public Scheduler(WorkflowExecutor rootExec, bool canRun)
        {
            this.rootWorkflowExecutor = rootExec;
            this.threadRequested = false;
            this.canRun = canRun;
            this.highPriorityEntriesQueue = (Queue<SchedulableItem>) rootExec.RootActivity.GetValue(HighPriorityEntriesQueueProperty);
            this.normalPriorityEntriesQueue = (Queue<SchedulableItem>) rootExec.RootActivity.GetValue(NormalPriorityEntriesQueueProperty);
            if (this.highPriorityEntriesQueue == null)
            {
                this.highPriorityEntriesQueue = new Queue<SchedulableItem>();
                rootExec.RootActivity.SetValue(HighPriorityEntriesQueueProperty, this.highPriorityEntriesQueue);
            }
            if (this.normalPriorityEntriesQueue == null)
            {
                this.normalPriorityEntriesQueue = new Queue<SchedulableItem>();
                rootExec.RootActivity.SetValue(NormalPriorityEntriesQueueProperty, this.normalPriorityEntriesQueue);
            }
            this.empty = (this.normalPriorityEntriesQueue.Count == 0) && (this.highPriorityEntriesQueue.Count == 0);
        }

        private SchedulableItem GetItemToRun()
        {
            SchedulableItem item = null;
            lock (this.syncObject)
            {
                bool flag = false;
                if ((this.highPriorityEntriesQueue.Count > 0) || (this.normalPriorityEntriesQueue.Count > 0))
                {
                    flag = true;
                    if (this.AbortOrTerminateRequested)
                    {
                        item = null;
                    }
                    else if (this.highPriorityEntriesQueue.Count > 0)
                    {
                        item = this.highPriorityEntriesQueue.Dequeue();
                    }
                    else if (this.CanRun)
                    {
                        if ((((IWorkflowCoreRuntime) this.RootWorkflowExecutor).CurrentAtomicActivity == null) && (this.normalPriorityEntriesQueue.Count > 0))
                        {
                            item = this.normalPriorityEntriesQueue.Dequeue();
                        }
                    }
                    else
                    {
                        item = null;
                    }
                }
                if (!flag)
                {
                    this.empty = true;
                }
                this.threadRequested = item != null;
            }
            return item;
        }

        public void PostPersist()
        {
            this.transactedEntries = null;
        }

        public void Resume()
        {
            this.canRun = true;
            if (!this.empty)
            {
                this.RootWorkflowExecutor.ScheduleForWork();
            }
        }

        public void ResumeIfRunnable()
        {
            if (this.canRun && !this.empty)
            {
                this.RootWorkflowExecutor.ScheduleForWork();
            }
        }

        public void Rollback()
        {
            if ((this.transactedEntries != null) && (this.transactedEntries.Count > 0))
            {
                IEnumerator<SchedulableItem> enumerator = this.normalPriorityEntriesQueue.GetEnumerator();
                Queue<SchedulableItem> queue = new Queue<SchedulableItem>();
                while (enumerator.MoveNext())
                {
                    if (!this.transactedEntries.Contains(enumerator.Current))
                    {
                        queue.Enqueue(enumerator.Current);
                    }
                }
                this.normalPriorityEntriesQueue.Clear();
                enumerator = queue.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.normalPriorityEntriesQueue.Enqueue(enumerator.Current);
                }
                this.transactedEntries = null;
            }
        }

        public void Run()
        {
        Label_0000:
            this.RootWorkflowExecutor.ProcessQueuedEvents();
            SchedulableItem itemToRun = this.GetItemToRun();
            bool flag = false;
            if (itemToRun != null)
            {
                Activity currentActivity = null;
                Exception exp = null;
                TransactionalProperties transactionalProperties = null;
                int contextId = itemToRun.ContextId;
                Activity contextActivityForId = this.RootWorkflowExecutor.GetContextActivityForId(contextId);
                if (contextActivityForId == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InvalidExecutionContext);
                }
                currentActivity = contextActivityForId.GetActivityByName(itemToRun.ActivityId);
                using (new ServiceEnvironment(currentActivity))
                {
                    exp = null;
                    bool flag2 = false;
                    try
                    {
                        if (currentActivity == null)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidActivityName, new object[] { itemToRun.ActivityId }));
                        }
                        Activity atomicActivity = null;
                        if (this.RootWorkflowExecutor.IsActivityInAtomicContext(currentActivity, out atomicActivity))
                        {
                            transactionalProperties = (TransactionalProperties) atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                            if (!WorkflowExecutor.CheckAndProcessTransactionAborted(transactionalProperties) && (transactionalProperties.TransactionScope == null))
                            {
                                transactionalProperties.TransactionScope = new TransactionScope(transactionalProperties.Transaction, TimeSpan.Zero, EnterpriseServicesInteropOption.Full);
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: Scheduler: instanceId: " + this.RootWorkflowExecutor.InstanceIdString + "Entered into TransactionScope, Current atomic acitivity " + atomicActivity.Name);
                            }
                        }
                        flag = true;
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Running scheduled entry: {1}", new object[] { this.RootWorkflowExecutor.InstanceIdString, itemToRun.ToString() });
                        this.RootWorkflowExecutor.stateChangedSincePersistence = true;
                        itemToRun.Run(this.RootWorkflowExecutor);
                    }
                    catch (Exception exception2)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(exception2))
                        {
                            flag2 = true;
                            throw;
                        }
                        if (transactionalProperties != null)
                        {
                            transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                        }
                        exp = exception2;
                    }
                    finally
                    {
                        if (!flag2)
                        {
                            if (flag)
                            {
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Done with running scheduled entry: {1}", new object[] { this.RootWorkflowExecutor.InstanceIdString, itemToRun.ToString() });
                            }
                            if (exp != null)
                            {
                                this.RootWorkflowExecutor.ExceptionOccured(exp, (currentActivity == null) ? contextActivityForId : currentActivity, null);
                                exp = null;
                            }
                        }
                    }
                    goto Label_0000;
                }
            }
        }

        public void ScheduleItem(SchedulableItem s, bool isInAtomicTransaction, bool transacted)
        {
            lock (this.syncObject)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1, "Workflow Runtime: Scheduler: InstanceId: {0} : Scheduling entry: {1}", new object[] { this.RootWorkflowExecutor.InstanceIdString, s.ToString() });
                (isInAtomicTransaction ? this.highPriorityEntriesQueue : this.normalPriorityEntriesQueue).Enqueue(s);
                if (transacted)
                {
                    if (this.transactedEntries == null)
                    {
                        this.transactedEntries = new Queue<SchedulableItem>();
                    }
                    this.transactedEntries.Enqueue(s);
                }
                if (!this.threadRequested && this.CanRun)
                {
                    this.RootWorkflowExecutor.ScheduleForWork();
                    this.threadRequested = true;
                }
                this.empty = false;
            }
        }

        public override string ToString()
        {
            return ("Scheduler('" + this.RootWorkflowExecutor.WorkflowDefinition.QualifiedName + "')");
        }

        internal bool AbortOrTerminateRequested
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.abortOrTerminateRequested;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.abortOrTerminateRequested = value;
            }
        }

        public bool CanRun
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.canRun;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.canRun = value;
            }
        }

        public bool IsStalledNow
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.empty;
            }
        }

        protected WorkflowExecutor RootWorkflowExecutor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootWorkflowExecutor;
            }
        }
    }
}

