namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    public sealed class CorrelationTokenEventArgs : EventArgs
    {
        private System.Workflow.Runtime.CorrelationToken correlator;
        private bool initialized;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CorrelationTokenEventArgs(System.Workflow.Runtime.CorrelationToken correlator, bool initialized)
        {
            this.correlator = correlator;
            this.initialized = initialized;
        }

        public System.Workflow.Runtime.CorrelationToken CorrelationToken
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.correlator;
            }
        }

        public bool IsInitializing
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.initialized;
            }
        }
    }
}

