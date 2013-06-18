namespace System.Collections.Generic
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public class KeyedByTypeCollection<TItem> : KeyedCollection<Type, TItem>
    {
        public KeyedByTypeCollection() : base(null, 4)
        {
        }

        public KeyedByTypeCollection(IEnumerable<TItem> items) : base(null, 4)
        {
            if (items == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("items");
            }
            foreach (TItem local in items)
            {
                base.Add(local);
            }
        }

        public T Find<T>()
        {
            return this.Find<T>(false);
        }

        private T Find<T>(bool remove)
        {
            for (int i = 0; i < base.Count; i++)
            {
                TItem item = base[i];
                if (item is T)
                {
                    if (remove)
                    {
                        base.Remove(item);
                    }
                    return item;
                }
            }
            return default(T);
        }

        public Collection<T> FindAll<T>()
        {
            return this.FindAll<T>(false);
        }

        private Collection<T> FindAll<T>(bool remove)
        {
            Collection<T> collection = new Collection<T>();
            foreach (TItem local in this)
            {
                if (local is T)
                {
                    collection.Add(local);
                }
            }
            if (remove)
            {
                foreach (T local2 in collection)
                {
                    base.Remove(local2);
                }
            }
            return collection;
        }

        protected override Type GetKeyForItem(TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            return item.GetType();
        }

        protected override void InsertItem(int index, TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            if (base.Contains(item.GetType()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", System.ServiceModel.SR.GetString("DuplicateBehavior1", new object[] { item.GetType().FullName }));
            }
            base.InsertItem(index, item);
        }

        public T Remove<T>()
        {
            return this.Find<T>(true);
        }

        public Collection<T> RemoveAll<T>()
        {
            return this.FindAll<T>(true);
        }

        protected override void SetItem(int index, TItem item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

