namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    [Serializable]
    public class QueueEventArgs : EventArgs
    {
        private IComparable queueName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal QueueEventArgs(IComparable queueName)
        {
            this.queueName = queueName;
        }

        public IComparable QueueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.queueName;
            }
        }
    }
}

