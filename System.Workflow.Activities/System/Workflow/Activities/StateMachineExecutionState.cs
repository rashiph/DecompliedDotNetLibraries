namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class StateMachineExecutionState
    {
        private Queue<StateMachineAction> _actions;
        private bool _completed;
        private string _currentStateName;
        private string _nextStateName;
        private string _previousStateName;
        private bool _queueLocked;
        private bool _schedulerBusy;
        private StateMachineSubscriptionManager _subscriptionManager;
        internal const string StateMachineExecutionStateKey = "StateMachineExecutionState";

        internal StateMachineExecutionState(Guid instanceId)
        {
            this._subscriptionManager = new StateMachineSubscriptionManager(this, instanceId);
        }

        internal void CalculateStateTransition(StateActivity currentState, string targetStateName)
        {
            if (currentState == null)
            {
                throw new ArgumentNullException("currentState");
            }
            if (string.IsNullOrEmpty(targetStateName))
            {
                throw new ArgumentNullException("targetStateName");
            }
            while ((currentState != null) && (currentState.QualifiedName.Equals(targetStateName) || !StateMachineHelpers.ContainsState(currentState, targetStateName)))
            {
                CloseStateAction item = new CloseStateAction(currentState.QualifiedName);
                this.Actions.Enqueue(item);
                currentState = currentState.Parent as StateActivity;
            }
            if (currentState == null)
            {
                throw new InvalidOperationException(SR.GetUnableToTransitionToState(targetStateName));
            }
            while (!currentState.QualifiedName.Equals(targetStateName))
            {
                foreach (Activity activity in currentState.EnabledActivities)
                {
                    StateActivity state = activity as StateActivity;
                    if ((state != null) && StateMachineHelpers.ContainsState(state, targetStateName))
                    {
                        ExecuteChildStateAction action2 = new ExecuteChildStateAction(currentState.QualifiedName, state.QualifiedName);
                        this.Actions.Enqueue(action2);
                        currentState = state;
                        break;
                    }
                }
            }
            if (!StateMachineHelpers.IsLeafState(currentState))
            {
                throw new InvalidOperationException(SR.GetInvalidStateTransitionPath());
            }
        }

        internal StateMachineAction DequeueAction()
        {
            StateMachineAction action = this.Actions.Dequeue();
            if (this.Actions.Count == 0)
            {
                this._queueLocked = false;
            }
            return action;
        }

        internal void EnqueueAction(StateMachineAction action)
        {
            this.Actions.Enqueue(action);
        }

        internal static StateMachineExecutionState Get(StateActivity state)
        {
            return (StateMachineExecutionState) state.GetValue(StateActivity.StateMachineExecutionStateProperty);
        }

        internal void LockQueue()
        {
            this._queueLocked = true;
        }

        internal void ProcessActions(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (!this.SchedulerBusy)
            {
                StateActivity state = (StateActivity) context.Activity;
                if (this.Actions.Count == 0)
                {
                    this.SubscriptionManager.ProcessQueue(context);
                }
                else
                {
                    StateMachineAction action = this.Actions.Peek();
                    while (action.StateName.Equals(state.QualifiedName))
                    {
                        action = this.DequeueAction();
                        action.Execute(context);
                        if (this.SchedulerBusy)
                        {
                            return;
                        }
                        if (this.Actions.Count == 0)
                        {
                            break;
                        }
                        action = this.Actions.Peek();
                    }
                    if (this.Actions.Count > 0)
                    {
                        StateActivity activity3 = StateMachineHelpers.FindDynamicStateByName(StateMachineHelpers.GetRootState(state), action.StateName);
                        if (activity3 == null)
                        {
                            throw new InvalidOperationException(SR.GetInvalidStateMachineAction(action.StateName));
                        }
                        activity3.RaiseProcessActionEvent(context);
                    }
                    else
                    {
                        this.SubscriptionManager.ProcessQueue(context);
                    }
                }
            }
        }

        internal void ProcessTransitionRequest(ActivityExecutionContext context)
        {
            if (!string.IsNullOrEmpty(this.NextStateName))
            {
                StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
                this.CalculateStateTransition(currentState, this.NextStateName);
                this.LockQueue();
                this.NextStateName = null;
            }
        }

        private Queue<StateMachineAction> Actions
        {
            get
            {
                if (this._actions == null)
                {
                    this._actions = new Queue<StateMachineAction>();
                }
                return this._actions;
            }
        }

        internal bool Completed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._completed;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._completed = value;
            }
        }

        internal string CurrentStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._currentStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._currentStateName = value;
            }
        }

        internal bool HasEnqueuedActions
        {
            get
            {
                return (this.Actions.Count > 0);
            }
        }

        internal string NextStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._nextStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._nextStateName = value;
            }
        }

        internal string PreviousStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._previousStateName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._previousStateName = value;
            }
        }

        internal bool SchedulerBusy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._schedulerBusy;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._schedulerBusy = value;
            }
        }

        internal StateMachineSubscriptionManager SubscriptionManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscriptionManager;
            }
        }
    }
}

