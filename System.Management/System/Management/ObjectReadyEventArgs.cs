namespace System.Management
{
    using System;
    using System.Runtime;

    public class ObjectReadyEventArgs : ManagementEventArgs
    {
        private ManagementBaseObject wmiObject;

        internal ObjectReadyEventArgs(object context, ManagementBaseObject wmiObject) : base(context)
        {
            this.wmiObject = wmiObject;
        }

        public ManagementBaseObject NewObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.wmiObject;
            }
        }
    }
}

