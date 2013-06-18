namespace System.Activities.Tracking
{
    using System;
    using System.Collections.ObjectModel;

    public class WorkflowInstanceQuery : TrackingQuery
    {
        private Collection<string> states;

        internal bool HasStates
        {
            get
            {
                return ((this.states != null) && (this.states.Count > 0));
            }
        }

        public Collection<string> States
        {
            get
            {
                if (this.states == null)
                {
                    this.states = new Collection<string>();
                }
                return this.states;
            }
        }
    }
}

