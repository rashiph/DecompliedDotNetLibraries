namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class ExecuteChildStateAction : StateMachineAction
    {
        private string _childStateName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExecuteChildStateAction(string stateName, string childStateName) : base(stateName)
        {
            this._childStateName = childStateName;
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            StateActivity state = (StateActivity) base.State.Activities[this.ChildStateName];
            StateActivity.ExecuteState(context, state);
        }

        internal string ChildStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._childStateName;
            }
        }
    }
}

