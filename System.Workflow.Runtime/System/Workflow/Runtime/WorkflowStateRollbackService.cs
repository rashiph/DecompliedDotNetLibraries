namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class WorkflowStateRollbackService
    {
        private int activityContextId;
        private string activityQualifiedName;
        private EventArgs callbackData;
        private EventHandler<EventArgs> callbackHandler;
        private MemoryStream clonedInstanceStateStream;
        private Hashtable completedContextActivities = new Hashtable();
        private bool isInstanceStateRevertRequested;
        private bool suspendOnRevert;
        private string suspendOnRevertInfo;
        private Activity workflowDefinition;
        private WorkflowExecutor workflowExecutor;

        public WorkflowStateRollbackService(WorkflowExecutor workflowExecutor)
        {
            this.workflowExecutor = workflowExecutor;
        }

        internal void CheckpointInstanceState()
        {
            this.clonedInstanceStateStream = new MemoryStream(0x2800);
            this.workflowExecutor.RootActivity.Save(this.clonedInstanceStateStream);
            this.workflowDefinition = this.workflowExecutor.WorkflowDefinition;
            this.completedContextActivities = (Hashtable) this.workflowExecutor.CompletedContextActivities.Clone();
            this.clonedInstanceStateStream.Position = 0L;
        }

        internal void DisposeCheckpointState()
        {
            this.clonedInstanceStateStream = null;
        }

        internal void RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendInfo)
        {
            if (this.clonedInstanceStateStream == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.InvalidRevertRequest);
            }
            this.activityContextId = ContextActivityUtils.ContextId(ContextActivityUtils.ContextActivity(currentActivity));
            this.activityQualifiedName = currentActivity.QualifiedName;
            this.callbackData = callbackData;
            this.callbackHandler = callbackHandler;
            this.suspendOnRevert = suspendOnRevert;
            this.suspendOnRevertInfo = suspendInfo;
            this.isInstanceStateRevertRequested = true;
            this.workflowExecutor.Scheduler.CanRun = false;
        }

        internal void RevertToCheckpointState()
        {
            Activity rootActivity = null;
            this.clonedInstanceStateStream.Position = 0L;
            using (new RuntimeEnvironment(this.workflowExecutor.WorkflowRuntime))
            {
                rootActivity = Activity.Load(this.clonedInstanceStateStream, this.workflowDefinition);
            }
            rootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, this.workflowExecutor.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty));
            WorkflowExecutor newWorkflowExecutor = new WorkflowExecutor(Guid.Empty);
            newWorkflowExecutor.Initialize(rootActivity, this.workflowExecutor.WorkflowRuntime, this.workflowExecutor);
            Activity contextActivityForId = newWorkflowExecutor.GetContextActivityForId(this.activityContextId);
            Activity activityByName = contextActivityForId.GetActivityByName(this.activityQualifiedName);
            using (new ServiceEnvironment(activityByName))
            {
                using (newWorkflowExecutor.SetCurrentActivity(activityByName))
                {
                    using (ActivityExecutionContext context = new ActivityExecutionContext(activityByName))
                    {
                        context.Invoke<EventArgs>(this.callbackHandler, this.callbackData);
                    }
                }
            }
            newWorkflowExecutor.BatchCollection.WorkItemOrderId = this.workflowExecutor.BatchCollection.WorkItemOrderId;
            foreach (KeyValuePair<object, WorkBatch> pair in this.workflowExecutor.BatchCollection)
            {
                pair.Value.SetWorkBatchCollection(newWorkflowExecutor.BatchCollection);
                Activity key = pair.Key as Activity;
                if (key != null)
                {
                    Activity activity5 = contextActivityForId.GetActivityByName(key.QualifiedName);
                    newWorkflowExecutor.BatchCollection.Add(activity5, pair.Value);
                }
            }
            this.workflowExecutor.BatchCollection.Clear();
            newWorkflowExecutor.CompletedContextActivities = this.completedContextActivities;
            this.workflowExecutor.WorkflowRuntime.ReplaceWorkflowExecutor(this.workflowExecutor.InstanceId, this.workflowExecutor, newWorkflowExecutor);
            if (!this.suspendOnRevert)
            {
                newWorkflowExecutor.Scheduler.Resume();
            }
            else
            {
                newWorkflowExecutor.SuspendOnIdle(this.suspendOnRevertInfo);
            }
            this.DisposeCheckpointState();
        }

        internal bool IsInstanceStateRevertRequested
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isInstanceStateRevertRequested;
            }
        }
    }
}

