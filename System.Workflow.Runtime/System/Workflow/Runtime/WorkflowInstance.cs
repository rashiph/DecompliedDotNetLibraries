namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class WorkflowInstance
    {
        private WorkflowExecutor _deadWorkflow;
        private Guid _instanceId;
        private System.Workflow.Runtime.WorkflowRuntime _runtime;

        internal WorkflowInstance(Guid instanceId, System.Workflow.Runtime.WorkflowRuntime workflowRuntime)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, new object[] { "instanceId" }));
            }
            if (workflowRuntime == null)
            {
                throw new ArgumentNullException("workflowRuntime");
            }
            this._instanceId = instanceId;
            this._runtime = workflowRuntime;
        }

        public void Abort()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                if (executor.WorkflowStatus == WorkflowStatus.Created)
                {
                    throw new InvalidOperationException(ExecutionStringManager.CannotAbortBeforeStart);
                }
                try
                {
                    executor.Abort();
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.ApplyWorkflowChanges(workflowChanges);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void EnqueueItem(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                try
                {
                    executor.EnqueueItem(queueName, item, pendingWork, workItem);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void EnqueueItemOnIdle(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.EnqueueItemOnIdle(queueName, item, pendingWork, workItem);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public override bool Equals(object obj)
        {
            WorkflowInstance instance = obj as WorkflowInstance;
            if (instance == null)
            {
                return false;
            }
            return (this._instanceId == instance._instanceId);
        }

        public override int GetHashCode()
        {
            return this._instanceId.GetHashCode();
        }

        public Activity GetWorkflowDefinition()
        {
            WorkflowExecutor executor;
            Activity workflowDefinitionClone;
        Label_0000:
            executor = this._runtime.Load(this);
            if (!executor.IsInstanceValid)
            {
                goto Label_0000;
            }
            try
            {
                workflowDefinitionClone = executor.GetWorkflowDefinitionClone("");
            }
            catch (InvalidOperationException)
            {
                if (executor.IsInstanceValid)
                {
                    throw;
                }
                goto Label_0000;
            }
            return workflowDefinitionClone;
        }

        public DateTime GetWorkflowNextTimerExpiration()
        {
            WorkflowExecutor executor;
            DateTime workflowNextTimerExpiration;
        Label_0000:
            executor = this._runtime.Load(this);
            if (!executor.IsInstanceValid)
            {
                goto Label_0000;
            }
            try
            {
                workflowNextTimerExpiration = executor.GetWorkflowNextTimerExpiration();
            }
            catch (InvalidOperationException)
            {
                if (executor.IsInstanceValid)
                {
                    throw;
                }
                goto Label_0000;
            }
            return workflowNextTimerExpiration;
        }

        public ReadOnlyCollection<WorkflowQueueInfo> GetWorkflowQueueData()
        {
            WorkflowExecutor executor;
            ReadOnlyCollection<WorkflowQueueInfo> workflowQueueInfos;
            if (this._deadWorkflow != null)
            {
                return this._deadWorkflow.GetWorkflowQueueInfos();
            }
        Label_0014:
            executor = this._runtime.Load(this);
            if (!executor.IsInstanceValid)
            {
                goto Label_0014;
            }
            try
            {
                workflowQueueInfos = executor.GetWorkflowQueueInfos();
            }
            catch (InvalidOperationException)
            {
                if (executor.IsInstanceValid)
                {
                    throw;
                }
                goto Label_0014;
            }
            return workflowQueueInfos;
        }

        internal WorkflowExecutor GetWorkflowResourceUNSAFE()
        {
            WorkflowExecutor executor;
            WorkflowExecutor executor2;
        Label_0000:
            executor = this._runtime.Load(this);
            if (!executor.IsInstanceValid)
            {
                goto Label_0000;
            }
            try
            {
                executor2 = executor;
            }
            catch (InvalidOperationException)
            {
                if (executor.IsInstanceValid)
                {
                    throw;
                }
                goto Label_0000;
            }
            return executor2;
        }

        public void Load()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                this._runtime.Load(this);
            }
        }

        internal void ProcessTimers()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = null;
                try
                {
                    executor = this._runtime.Load(this);
                }
                catch (InvalidOperationException)
                {
                    return;
                }
                if ((executor == null) || !executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.DeliverTimerSubscriptions();
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void ProcessTimers(object ignored)
        {
            this.ProcessTimers();
        }

        public void ReloadTrackingProfiles()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    this._runtime.TrackingListenerFactory.ReloadProfiles(executor);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void Resume()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.Resume();
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void Start()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.Start();
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public void Suspend(string error)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                if (executor.WorkflowStatus == WorkflowStatus.Created)
                {
                    throw new InvalidOperationException(ExecutionStringManager.CannotSuspendBeforeStart);
                }
                try
                {
                    executor.Suspend(error);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
                catch (ExecutorLocksHeldException exception)
                {
                    try
                    {
                        exception.Handle.WaitOne();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    goto Label_000C;
                }
            }
        }

        public void Terminate(string error)
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
            Label_000C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_000C;
                }
                try
                {
                    executor.Terminate(error);
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_000C;
                }
            }
        }

        public bool TryUnload()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor = this._runtime.Load(this);
                using (executor.ExecutorLock.Enter())
                {
                    if (executor.IsInstanceValid)
                    {
                        try
                        {
                            return executor.TryUnload();
                        }
                        catch (InvalidOperationException)
                        {
                            if (executor.IsInstanceValid)
                            {
                                throw;
                            }
                        }
                    }
                }
                return false;
            }
        }

        public void Unload()
        {
            using (new WorkflowTraceTransfer(this.InstanceId))
            {
                WorkflowExecutor executor;
                if ((this._runtime == null) || (this._runtime.GetService<WorkflowPersistenceService>() == null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, new object[] { this.InstanceId }));
                }
            Label_004C:
                executor = this._runtime.Load(this);
                if (!executor.IsInstanceValid)
                {
                    goto Label_004C;
                }
                try
                {
                    executor.Unload();
                }
                catch (InvalidOperationException)
                {
                    if (executor.IsInstanceValid)
                    {
                        throw;
                    }
                    goto Label_004C;
                }
                catch (ExecutorLocksHeldException exception)
                {
                    try
                    {
                        exception.Handle.WaitOne();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    goto Label_004C;
                }
            }
        }

        internal WorkflowExecutor DeadWorkflow
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._deadWorkflow = value;
            }
        }

        public Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._instanceId;
            }
        }

        public System.Workflow.Runtime.WorkflowRuntime WorkflowRuntime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._runtime;
            }
        }
    }
}

