namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class WorkflowApplicationEventArgs : EventArgs
    {
        internal WorkflowApplicationEventArgs(System.Activities.WorkflowApplication application)
        {
            this.Owner = application;
        }

        public IEnumerable<T> GetInstanceExtensions<T>() where T: class
        {
            return this.Owner.InternalGetExtensions<T>();
        }

        public Guid InstanceId
        {
            get
            {
                return this.Owner.Id;
            }
        }

        internal System.Activities.WorkflowApplication Owner { get; private set; }
    }
}

