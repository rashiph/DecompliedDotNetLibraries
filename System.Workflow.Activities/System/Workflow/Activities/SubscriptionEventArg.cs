namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    internal sealed class SubscriptionEventArg : EventArgs
    {
        private EventArgs _args;
        private EventType _subscriptionType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SubscriptionEventArg(EventArgs args, EventType subType)
        {
            this._args = args;
            this._subscriptionType = subType;
        }

        public override string ToString()
        {
            return ("SubscriptionEventArg(" + ((this._args == null) ? "null" : this._args.ToString()) + ")");
        }

        internal EventArgs Args
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._args;
            }
        }

        internal EventType SubscriptionType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscriptionType;
            }
        }
    }
}

