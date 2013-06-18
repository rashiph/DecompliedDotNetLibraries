namespace System.Management
{
    using System;
    using System.Runtime;

    public class ObjectPutEventArgs : ManagementEventArgs
    {
        private ManagementPath wmiPath;

        internal ObjectPutEventArgs(object context, ManagementPath path) : base(context)
        {
            this.wmiPath = path;
        }

        public ManagementPath Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.wmiPath;
            }
        }
    }
}

