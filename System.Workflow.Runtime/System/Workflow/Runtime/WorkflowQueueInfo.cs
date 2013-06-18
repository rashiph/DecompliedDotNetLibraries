namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Runtime;

    public class WorkflowQueueInfo
    {
        private ICollection _items;
        private IComparable _queueName;
        private ReadOnlyCollection<string> _subscribedActivityNames;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowQueueInfo(IComparable queueName, ICollection items, ReadOnlyCollection<string> subscribedActivityNames)
        {
            this._queueName = queueName;
            this._items = items;
            this._subscribedActivityNames = subscribedActivityNames;
        }

        public ICollection Items
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._items;
            }
        }

        public IComparable QueueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._queueName;
            }
        }

        public ReadOnlyCollection<string> SubscribedActivityNames
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscribedActivityNames;
            }
        }
    }
}

