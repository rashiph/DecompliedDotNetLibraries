namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [Serializable]
    public sealed class ConditionalEventArgs : EventArgs
    {
        private bool result;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConditionalEventArgs() : this(false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConditionalEventArgs(bool result)
        {
            this.result = result;
        }

        public bool Result
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.result;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.result = value;
            }
        }
    }
}

