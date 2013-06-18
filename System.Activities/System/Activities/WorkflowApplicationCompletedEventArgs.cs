namespace System.Activities
{
    using System;
    using System.Collections.Generic;

    public class WorkflowApplicationCompletedEventArgs : WorkflowApplicationEventArgs
    {
        private ActivityInstanceState completionState;
        private IDictionary<string, object> outputs;
        private Exception terminationException;

        internal WorkflowApplicationCompletedEventArgs(System.Activities.WorkflowApplication application, Exception terminationException, ActivityInstanceState completionState, IDictionary<string, object> outputs) : base(application)
        {
            this.terminationException = terminationException;
            this.completionState = completionState;
            this.outputs = outputs;
        }

        public ActivityInstanceState CompletionState
        {
            get
            {
                return this.completionState;
            }
        }

        public IDictionary<string, object> Outputs
        {
            get
            {
                if (this.outputs == null)
                {
                    this.outputs = ActivityUtilities.EmptyParameters;
                }
                return this.outputs;
            }
        }

        public Exception TerminationException
        {
            get
            {
                return this.terminationException;
            }
        }
    }
}

