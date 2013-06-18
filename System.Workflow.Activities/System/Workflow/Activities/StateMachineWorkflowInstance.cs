namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Tracking;

    public sealed class StateMachineWorkflowInstance
    {
        private Guid _instanceId;
        private WorkflowRuntime _runtime;
        private SqlTrackingQuery _sqlTrackingQuery;
        private SqlTrackingService _sqlTrackingService;
        private SqlTrackingWorkflowInstance _sqlTrackingWorkflowInstance;
        private StateMachineWorkflowActivity _stateMachineWorkflow;
        private System.Workflow.Runtime.WorkflowInstance _workflowInstance;
        internal const string StateHistoryPropertyName = "StateHistory";

        public StateMachineWorkflowInstance(WorkflowRuntime runtime, Guid instanceId)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException("runtime");
            }
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentNullException("instanceId");
            }
            this._runtime = runtime;
            this._instanceId = instanceId;
            this._workflowInstance = runtime.GetWorkflow(instanceId);
            this._stateMachineWorkflow = this._workflowInstance.GetWorkflowDefinition() as StateMachineWorkflowActivity;
            if (this._stateMachineWorkflow == null)
            {
                throw new ArgumentException(SR.GetStateMachineWorkflowRequired(), "instanceId");
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void EnqueueItem(IComparable queueName, object item)
        {
            this.EnqueueItem(queueName, item, null, null);
        }

        public void EnqueueItem(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            this.WorkflowInstance.EnqueueItemOnIdle(queueName, item, pendingWork, workItem);
        }

        internal Activity FindActivityByQualifiedName(string id)
        {
            return StateMachineHelpers.FindActivityByName(this.StateMachineWorkflow, id);
        }

        private StateActivity GetCurrentState()
        {
            foreach (WorkflowQueueInfo info in this.WorkflowInstance.GetWorkflowQueueData())
            {
                if (info.QueueName.Equals("SetStateQueue"))
                {
                    if (info.SubscribedActivityNames.Count == 0)
                    {
                        return null;
                    }
                    return StateMachineHelpers.FindStateByName(this.StateMachineWorkflow, info.SubscribedActivityNames[0]);
                }
            }
            return null;
        }

        private static ReadOnlyCollection<StateActivity> GetLeafStates(StateActivity parentState)
        {
            if (parentState == null)
            {
                throw new ArgumentNullException("parentState");
            }
            List<StateActivity> list = new List<StateActivity>();
            Queue<StateActivity> queue = new Queue<StateActivity>();
            queue.Enqueue(parentState);
            while (queue.Count > 0)
            {
                foreach (Activity activity2 in queue.Dequeue().EnabledActivities)
                {
                    StateActivity state = activity2 as StateActivity;
                    if (state != null)
                    {
                        if (StateMachineHelpers.IsLeafState(state))
                        {
                            list.Add(state);
                        }
                        else
                        {
                            queue.Enqueue(state);
                        }
                    }
                }
            }
            return list.AsReadOnly();
        }

        private ReadOnlyCollection<string> GetPossibleStateTransitions()
        {
            List<string> list = new List<string>();
            ReadOnlyCollection<WorkflowQueueInfo> workflowQueueData = this.WorkflowInstance.GetWorkflowQueueData();
            StateMachineWorkflowActivity stateMachineWorkflow = this.StateMachineWorkflow;
            foreach (WorkflowQueueInfo info in workflowQueueData)
            {
                foreach (string str in info.SubscribedActivityNames)
                {
                    IEventActivity eventActivity = StateMachineHelpers.FindActivityByName(stateMachineWorkflow, str) as IEventActivity;
                    if (eventActivity != null)
                    {
                        EventDrivenActivity parentEventDriven = StateMachineHelpers.GetParentEventDriven(eventActivity);
                        Queue<Activity> queue = new Queue<Activity>();
                        queue.Enqueue(parentEventDriven);
                        while (queue.Count > 0)
                        {
                            Activity activity5 = queue.Dequeue();
                            SetStateActivity activity6 = activity5 as SetStateActivity;
                            if (activity6 != null)
                            {
                                list.Add(activity6.TargetStateName);
                            }
                            else
                            {
                                CompositeActivity activity7 = activity5 as CompositeActivity;
                                if (activity7 != null)
                                {
                                    foreach (Activity activity8 in activity7.EnabledActivities)
                                    {
                                        queue.Enqueue(activity8);
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            return list.AsReadOnly();
        }

        private ReadOnlyCollection<string> GetStateHistory()
        {
            StateMachineWorkflowActivity stateMachineWorkflow;
            if (this._sqlTrackingService == null)
            {
                this._sqlTrackingService = this._runtime.GetService<SqlTrackingService>();
                if (this._sqlTrackingService == null)
                {
                    throw new InvalidOperationException(SR.GetSqlTrackingServiceRequired());
                }
            }
            if (this._sqlTrackingQuery == null)
            {
                this._sqlTrackingQuery = new SqlTrackingQuery(this._sqlTrackingService.ConnectionString);
            }
            Stack<string> stack = new Stack<string>();
            try
            {
                stateMachineWorkflow = this.StateMachineWorkflow;
            }
            catch (InvalidOperationException)
            {
                return new ReadOnlyCollection<string>(stack.ToArray());
            }
            if ((this._sqlTrackingWorkflowInstance != null) || this._sqlTrackingQuery.TryGetWorkflow(this._instanceId, out this._sqlTrackingWorkflowInstance))
            {
                this._sqlTrackingWorkflowInstance.Refresh();
                foreach (UserTrackingRecord record in this._sqlTrackingWorkflowInstance.UserEvents)
                {
                    if (record.UserDataKey == "StateActivity.StateChange")
                    {
                        string userData = record.UserData as string;
                        if (userData == null)
                        {
                            throw new InvalidOperationException(SR.GetInvalidUserDataInStateChangeTrackingRecord());
                        }
                        StateActivity state = StateMachineHelpers.FindStateByName(stateMachineWorkflow, record.QualifiedName);
                        if (state == null)
                        {
                            throw new InvalidOperationException(SR.GetInvalidUserDataInStateChangeTrackingRecord());
                        }
                        if (StateMachineHelpers.IsLeafState(state))
                        {
                            stack.Push(userData);
                        }
                    }
                }
            }
            return new ReadOnlyCollection<string>(stack.ToArray());
        }

        public void SetState(string targetStateName)
        {
            if (targetStateName == null)
            {
                throw new ArgumentNullException("targetStateName");
            }
            if (!(this.FindActivityByQualifiedName(targetStateName) is StateActivity))
            {
                throw new ArgumentOutOfRangeException("targetStateName");
            }
            SetStateEventArgs item = new SetStateEventArgs(targetStateName);
            this.WorkflowInstance.EnqueueItemOnIdle("SetStateQueue", item, null, null);
        }

        public void SetState(StateActivity targetState)
        {
            if (targetState == null)
            {
                throw new ArgumentNullException("targetState");
            }
            this.SetState(targetState.QualifiedName);
        }

        public StateActivity CurrentState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetCurrentState();
            }
        }

        public string CurrentStateName
        {
            get
            {
                StateActivity currentState = this.CurrentState;
                if (currentState == null)
                {
                    return null;
                }
                return currentState.QualifiedName;
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

        public ReadOnlyCollection<string> PossibleStateTransitions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetPossibleStateTransitions();
            }
        }

        public ReadOnlyCollection<string> StateHistory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetStateHistory();
            }
        }

        public StateMachineWorkflowActivity StateMachineWorkflow
        {
            get
            {
                try
                {
                    this._stateMachineWorkflow = (StateMachineWorkflowActivity) this.WorkflowInstance.GetWorkflowDefinition();
                }
                catch (InvalidOperationException)
                {
                }
                return this._stateMachineWorkflow;
            }
        }

        public ReadOnlyCollection<StateActivity> States
        {
            get
            {
                StateMachineWorkflowActivity stateMachineWorkflow = this.StateMachineWorkflow;
                if (stateMachineWorkflow == null)
                {
                    throw new InvalidOperationException();
                }
                return GetLeafStates(stateMachineWorkflow);
            }
        }

        public System.Workflow.Runtime.WorkflowInstance WorkflowInstance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowInstance;
            }
        }
    }
}

