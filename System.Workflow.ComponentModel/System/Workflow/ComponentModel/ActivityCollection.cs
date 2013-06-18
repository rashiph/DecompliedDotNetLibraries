namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(ActivityCollectionMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public sealed class ActivityCollection : List<Activity>, IList<Activity>, ICollection<Activity>, IEnumerable<Activity>, IList, ICollection, IEnumerable
    {
        private Activity owner;

        public event EventHandler<ActivityCollectionChangeEventArgs> ListChanged;

        internal event EventHandler<ActivityCollectionChangeEventArgs> ListChanging;

        public ActivityCollection(Activity owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (owner == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "owner");
            }
            this.owner = owner;
        }

        public void Add(Activity item)
        {
            ((ICollection<Activity>) this).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Activity>) this).Clear();
        }

        public bool Contains(Activity item)
        {
            return ((ICollection<Activity>) this).Contains(item);
        }

        private void FireListChanged(ActivityCollectionChangeEventArgs eventArgs)
        {
            if (this.ListChanged != null)
            {
                this.ListChanged(this, eventArgs);
            }
        }

        private void FireListChanging(ActivityCollectionChangeEventArgs eventArgs)
        {
            if (this.ListChanging != null)
            {
                this.ListChanging(this, eventArgs);
            }
        }

        public IEnumerator<Activity> GetEnumerator()
        {
            return ((IEnumerable<Activity>) this).GetEnumerator();
        }

        public int IndexOf(Activity item)
        {
            return ((IList<Activity>) this).IndexOf(item);
        }

        internal void InnerAdd(Activity activity)
        {
            base.Add(activity);
        }

        public void Insert(int index, Activity item)
        {
            ((IList<Activity>) this).Insert(index, item);
        }

        public bool Remove(Activity item)
        {
            return ((ICollection<Activity>) this).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Activity>) this).RemoveAt(index);
        }

        void ICollection<Activity>.Add(Activity item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(base.Count, null, item, this.owner, ActivityCollectionChangeAction.Add);
            this.FireListChanging(eventArgs);
            base.Add(item);
            this.FireListChanged(eventArgs);
        }

        void ICollection<Activity>.Clear()
        {
            ICollection<Activity> range = base.GetRange(0, base.Count);
            ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(-1, range, null, this.owner, ActivityCollectionChangeAction.Remove);
            this.FireListChanging(eventArgs);
            base.Clear();
            this.FireListChanged(eventArgs);
        }

        bool ICollection<Activity>.Contains(Activity item)
        {
            return base.Contains(item);
        }

        void ICollection<Activity>.CopyTo(Activity[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<Activity>.Remove(Activity item)
        {
            if (base.Contains(item))
            {
                int index = base.IndexOf(item);
                if (index >= 0)
                {
                    ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(index, item, null, this.owner, ActivityCollectionChangeAction.Remove);
                    this.FireListChanging(eventArgs);
                    base.Remove(item);
                    this.FireListChanged(eventArgs);
                    return true;
                }
            }
            return false;
        }

        IEnumerator<Activity> IEnumerable<Activity>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        int IList<Activity>.IndexOf(Activity item)
        {
            return base.IndexOf(item);
        }

        void IList<Activity>.Insert(int index, Activity item)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(index, null, item, this.owner, ActivityCollectionChangeAction.Add);
            this.FireListChanging(eventArgs);
            base.Insert(index, item);
            this.FireListChanged(eventArgs);
        }

        void IList<Activity>.RemoveAt(int index)
        {
            if ((index < 0) || (index >= base.Count))
            {
                throw new ArgumentOutOfRangeException("Index");
            }
            Activity removedActivity = base[index];
            ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(index, removedActivity, null, this.owner, ActivityCollectionChangeAction.Remove);
            this.FireListChanging(eventArgs);
            base.RemoveAt(index);
            this.FireListChanged(eventArgs);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], (int) (i + index));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Activity>) this).GetEnumerator();
        }

        int IList.Add(object value)
        {
            if (!(value is Activity))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((ICollection<Activity>) this).Add((Activity) value);
            return (this.Count - 1);
        }

        void IList.Clear()
        {
            ((ICollection<Activity>) this).Clear();
        }

        bool IList.Contains(object value)
        {
            if (!(value is Activity))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            return ((ICollection<Activity>) this).Contains((Activity) value);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is Activity))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            return ((IList<Activity>) this).IndexOf((Activity) value);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is Activity))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((IList<Activity>) this).Insert(index, (Activity) value);
        }

        void IList.Remove(object value)
        {
            if (!(value is Activity))
            {
                throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
            }
            ((ICollection<Activity>) this).Remove((Activity) value);
        }

        public int Count
        {
            get
            {
                return this.Count;
            }
        }

        public Activity this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value;
            }
        }

        public Activity this[string key]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Name.Equals(key) || this[i].QualifiedName.Equals(key))
                    {
                        return this[i];
                    }
                }
                return null;
            }
        }

        internal Activity Owner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.owner;
            }
        }

        int ICollection<Activity>.Count
        {
            get
            {
                return base.Count;
            }
        }

        bool ICollection<Activity>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        Activity IList<Activity>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("item");
                }
                Activity removedActivity = base[index];
                ActivityCollectionChangeEventArgs eventArgs = new ActivityCollectionChangeEventArgs(index, removedActivity, value, this.owner, ActivityCollectionChangeAction.Replace);
                this.FireListChanging(eventArgs);
                base[index] = value;
                this.FireListChanged(eventArgs);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (!(value is Activity))
                {
                    throw new Exception(SR.GetString("Error_InvalidListItem", new object[] { base.GetType().GetGenericArguments()[0].FullName }));
                }
                this[index] = (Activity) value;
            }
        }
    }
}

