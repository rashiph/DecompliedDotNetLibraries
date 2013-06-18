namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class ExternalEventAction : StateMachineAction
    {
        private string _eventDrivenName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExternalEventAction(string stateName, string eventDrivenName) : base(stateName)
        {
            this._eventDrivenName = eventDrivenName;
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            EventDrivenActivity eventDriven = (EventDrivenActivity) base.State.Activities[this.EventDrivenName];
            StateActivity.ExecuteEventDriven(context, eventDriven);
        }

        internal string EventDrivenName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventDrivenName;
            }
        }
    }
}

