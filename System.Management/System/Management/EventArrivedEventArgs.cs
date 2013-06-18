namespace System.Management
{
    using System;
    using System.Runtime;

    public class EventArrivedEventArgs : ManagementEventArgs
    {
        private ManagementBaseObject eventObject;

        internal EventArrivedEventArgs(object context, ManagementBaseObject eventObject) : base(context)
        {
            this.eventObject = eventObject;
        }

        public ManagementBaseObject NewEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventObject;
            }
        }
    }
}

