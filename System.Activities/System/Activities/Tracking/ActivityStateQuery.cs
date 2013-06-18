namespace System.Activities.Tracking
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public class ActivityStateQuery : TrackingQuery
    {
        private Collection<string> arguments;
        private Collection<string> states;
        private Collection<string> variables;

        public ActivityStateQuery()
        {
            this.ActivityName = "*";
        }

        public string ActivityName { get; set; }

        public Collection<string> Arguments
        {
            get
            {
                if (this.arguments == null)
                {
                    this.arguments = new Collection<string>();
                }
                return this.arguments;
            }
        }

        internal bool HasArguments
        {
            get
            {
                return ((this.arguments != null) && (this.arguments.Count > 0));
            }
        }

        internal bool HasStates
        {
            get
            {
                return ((this.states != null) && (this.states.Count > 0));
            }
        }

        internal bool HasVariables
        {
            get
            {
                return ((this.variables != null) && (this.variables.Count > 0));
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

        public Collection<string> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new Collection<string>();
                }
                return this.variables;
            }
        }
    }
}

