namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public class MessageQueuePermissionEntryCollection : CollectionBase
    {
        private MessageQueuePermission owner;

        internal MessageQueuePermissionEntryCollection(MessageQueuePermission owner)
        {
            this.owner = owner;
        }

        public int Add(MessageQueuePermissionEntry value)
        {
            return base.List.Add(value);
        }

        public void AddRange(MessageQueuePermissionEntry[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(MessageQueuePermissionEntryCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(MessageQueuePermissionEntry value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(MessageQueuePermissionEntry[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(MessageQueuePermissionEntry value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, MessageQueuePermissionEntry value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            this.owner.Clear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.owner.Clear();
        }

        protected override void OnRemove(int index, object value)
        {
            this.owner.Clear();
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.owner.Clear();
        }

        public void Remove(MessageQueuePermissionEntry value)
        {
            base.List.Remove(value);
        }

        public MessageQueuePermissionEntry this[int index]
        {
            get
            {
                return (MessageQueuePermissionEntry) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

