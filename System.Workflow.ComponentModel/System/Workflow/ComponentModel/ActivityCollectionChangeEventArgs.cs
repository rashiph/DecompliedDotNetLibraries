namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    public sealed class ActivityCollectionChangeEventArgs : EventArgs
    {
        private ActivityCollectionChangeAction action;
        private ICollection<Activity> addedItems;
        private int index;
        private object owner;
        private ICollection<Activity> removedItems;

        public ActivityCollectionChangeEventArgs(int index, Activity removedActivity, Activity addedActivity, object owner, ActivityCollectionChangeAction action)
        {
            this.index = index;
            if (removedActivity != null)
            {
                this.removedItems = new List<Activity>();
                ((List<Activity>) this.removedItems).Add(removedActivity);
            }
            if (addedActivity != null)
            {
                this.addedItems = new List<Activity>();
                ((List<Activity>) this.addedItems).Add(addedActivity);
            }
            this.action = action;
            this.owner = owner;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityCollectionChangeEventArgs(int index, ICollection<Activity> removedItems, ICollection<Activity> addedItems, object owner, ActivityCollectionChangeAction action)
        {
            this.index = index;
            this.removedItems = removedItems;
            this.addedItems = addedItems;
            this.action = action;
            this.owner = owner;
        }

        public ActivityCollectionChangeAction Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
        }

        public IList<Activity> AddedItems
        {
            get
            {
                if (this.addedItems == null)
                {
                    return new List<Activity>().AsReadOnly();
                }
                return new List<Activity>(this.addedItems).AsReadOnly();
            }
        }

        public int Index
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.index;
            }
        }

        public object Owner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.owner;
            }
        }

        public IList<Activity> RemovedItems
        {
            get
            {
                if (this.removedItems == null)
                {
                    return new List<Activity>().AsReadOnly();
                }
                return new List<Activity>(this.removedItems).AsReadOnly();
            }
        }
    }
}

