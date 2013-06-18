namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class StateMachineAction
    {
        [NonSerialized]
        private StateActivity _currentState;
        private StateMachineExecutionState _executionState;
        [NonSerialized]
        private StateActivity _state;
        private string _stateName;
        private StateMachineSubscriptionManager _subscriptionManager;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal StateMachineAction(string stateName)
        {
            this._stateName = stateName;
        }

        internal virtual void Execute(ActivityExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._state = (StateActivity) context.Activity;
            this._currentState = StateMachineHelpers.GetCurrentState(context);
            StateActivity rootState = StateMachineHelpers.GetRootState(this._state);
            this._executionState = StateMachineExecutionState.Get(rootState);
            this._subscriptionManager = this._executionState.SubscriptionManager;
        }

        protected StateActivity CurrentState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._currentState;
            }
        }

        protected StateMachineExecutionState ExecutionState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._executionState;
            }
        }

        protected StateActivity State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._state;
            }
        }

        internal string StateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._stateName;
            }
        }

        protected StateMachineSubscriptionManager SubscriptionManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscriptionManager;
            }
        }
    }
}

