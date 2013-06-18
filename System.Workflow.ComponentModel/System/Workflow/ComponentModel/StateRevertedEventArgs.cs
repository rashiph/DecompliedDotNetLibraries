namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    [Serializable]
    internal class StateRevertedEventArgs : EventArgs
    {
        public System.Exception Exception;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateRevertedEventArgs(System.Exception exception)
        {
            this.Exception = exception;
        }
    }
}

