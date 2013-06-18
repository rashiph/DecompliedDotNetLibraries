namespace System.Management
{
    using System;
    using System.Runtime;

    public class StoppedEventArgs : ManagementEventArgs
    {
        private int status;

        internal StoppedEventArgs(object context, int status) : base(context)
        {
            this.status = status;
        }

        public ManagementStatus Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return (ManagementStatus) this.status;
            }
        }
    }
}

