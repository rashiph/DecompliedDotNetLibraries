namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    internal class GrantedLock : ICloneable
    {
        private Activity holder;
        private List<Activity> waitList;

        public GrantedLock(Activity holder)
        {
            this.holder = holder;
            this.waitList = new List<Activity>();
        }

        public object Clone()
        {
            GrantedLock @lock = new GrantedLock(this.holder);
            @lock.waitList.InsertRange(0, this.waitList);
            return @lock;
        }

        public Activity Holder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.holder;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.holder = value;
            }
        }

        public IList<Activity> WaitList
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.waitList;
            }
        }
    }
}

