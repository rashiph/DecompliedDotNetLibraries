namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Runtime.CompilerServices;

    internal sealed class TransitionData
    {
        public Activity Action { get; set; }

        public Activity<bool> Condition { get; set; }

        public InternalState To { get; set; }
    }
}

