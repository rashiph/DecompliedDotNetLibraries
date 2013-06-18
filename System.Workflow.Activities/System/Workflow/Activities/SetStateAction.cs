namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class SetStateAction : StateMachineAction
    {
        private string _targetStateName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SetStateAction(string stateName, string targetStateName) : base(stateName)
        {
            this._targetStateName = targetStateName;
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            base.ExecutionState.CalculateStateTransition(base.CurrentState, this.TargetStateName);
        }

        internal string TargetStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._targetStateName;
            }
        }
    }
}

