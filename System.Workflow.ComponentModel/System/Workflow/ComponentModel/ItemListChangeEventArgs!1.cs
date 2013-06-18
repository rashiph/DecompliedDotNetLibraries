namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class ItemListChangeEventArgs<T> : EventArgs
    {
        private ItemListChangeAction action;
        private ICollection<T> addedItems;
        private int index;
        private object owner;
        private ICollection<T> removedItems;

        public ItemListChangeEventArgs(int index, ICollection<T> removedItems, ICollection<T> addedItems, object owner, ItemListChangeAction action)
        {
            this.action = ItemListChangeAction.Add;
            this.index = index;
            this.removedItems = removedItems;
            this.addedItems = addedItems;
            this.action = action;
            this.owner = owner;
        }

        public ItemListChangeEventArgs(int index, T removedActivity, T addedActivity, object owner, ItemListChangeAction action)
        {
            this.action = ItemListChangeAction.Add;
            this.index = index;
            if (removedActivity != null)
            {
                this.removedItems = new List<T>();
                ((List<T>) this.removedItems).Add(removedActivity);
            }
            if (addedActivity != null)
            {
                this.addedItems = new List<T>();
                ((List<T>) this.addedItems).Add(addedActivity);
            }
            this.action = action;
            this.owner = owner;
        }

        public ItemListChangeAction Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
        }

        public IList<T> AddedItems
        {
            get
            {
                if (this.addedItems == null)
                {
                    return new List<T>().AsReadOnly();
                }
                return new List<T>(this.addedItems).AsReadOnly();
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

        public IList<T> RemovedItems
        {
            get
            {
                if (this.removedItems == null)
                {
                    return new List<T>().AsReadOnly();
                }
                return new List<T>(this.removedItems).AsReadOnly();
            }
        }
    }
}

