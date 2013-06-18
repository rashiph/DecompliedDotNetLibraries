namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal class ExecutorLocksHeldException : Exception
    {
        private ManualResetEvent handle;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExecutorLocksHeldException(ManualResetEvent handle)
        {
            this.handle = handle;
        }

        internal ManualResetEvent Handle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.handle;
            }
        }
    }
}

