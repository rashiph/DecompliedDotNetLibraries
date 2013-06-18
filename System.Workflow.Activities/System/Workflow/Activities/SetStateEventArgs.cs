namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    public class SetStateEventArgs : EventArgs
    {
        private string targetStateName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SetStateEventArgs(string targetStateName)
        {
            this.targetStateName = targetStateName;
        }

        public string TargetStateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetStateName;
            }
        }
    }
}

