namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class CloseStateAction : StateMachineAction
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CloseStateAction(string stateName) : base(stateName)
        {
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            StateActivity.LeavingState(context);
        }
    }
}

