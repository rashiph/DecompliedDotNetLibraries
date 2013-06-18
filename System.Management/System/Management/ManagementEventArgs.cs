namespace System.Management
{
    using System;
    using System.Runtime;

    public abstract class ManagementEventArgs : EventArgs
    {
        private object context;

        internal ManagementEventArgs(object context)
        {
            this.context = context;
        }

        public object Context
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
        }
    }
}

