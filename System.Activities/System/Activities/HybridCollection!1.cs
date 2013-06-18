namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Serialization;

    [DataContract]
    internal class HybridCollection<T> where T: class
    {
        [DataMember(EmitDefaultValue=false)]
        private List<T> multipleItems;
        [DataMember(EmitDefaultValue=false)]
        private T singleItem;

        public HybridCollection()
        {
        }

        public HybridCollection(T initialItem)
        {
            this.singleItem = initialItem;
        }

        public void Add(T item)
        {
            if (this.multipleItems != null)
            {
                this.multipleItems.Add(item);
            }
            else if (this.singleItem != null)
            {
                this.multipleItems = new List<T>(2);
                this.multipleItems.Add(this.singleItem);
                this.multipleItems.Add(item);
                this.singleItem = default(T);
            }
            else
            {
                this.singleItem = item;
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            if (this.multipleItems != null)
            {
                return new ReadOnlyCollection<T>(this.multipleItems);
            }
            if (this.singleItem != null)
            {
                return new ReadOnlyCollection<T>(new T[] { this.singleItem });
            }
            return new ReadOnlyCollection<T>(new T[0]);
        }

        public void Compress()
        {
            if ((this.multipleItems != null) && (this.multipleItems.Count == 1))
            {
                this.singleItem = this.multipleItems[0];
                this.multipleItems = null;
            }
        }

        public void Remove(T item)
        {
            this.Remove(item, false);
        }

        internal void Remove(T item, bool searchingFromEnd)
        {
            if (this.singleItem != null)
            {
                this.singleItem = default(T);
            }
            else
            {
                int index = searchingFromEnd ? this.multipleItems.LastIndexOf(item) : this.multipleItems.IndexOf(item);
                if (index != -1)
                {
                    this.multipleItems.RemoveAt(index);
                }
            }
        }

        public int Count
        {
            get
            {
                if (this.singleItem != null)
                {
                    return 1;
                }
                if (this.multipleItems != null)
                {
                    return this.multipleItems.Count;
                }
                return 0;
            }
        }

        public T this[int index]
        {
            get
            {
                if (this.singleItem != null)
                {
                    return this.singleItem;
                }
                if (this.multipleItems != null)
                {
                    return this.multipleItems[index];
                }
                return default(T);
            }
        }

        protected IList<T> MultipleItems
        {
            get
            {
                return this.multipleItems;
            }
        }

        protected T SingleItem
        {
            get
            {
                return this.singleItem;
            }
        }
    }
}

