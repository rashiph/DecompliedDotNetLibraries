namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class ExtensionCollection<T> : SynchronizedCollection<IExtension<T>>, IExtensionCollection<T>, ICollection<IExtension<T>>, IEnumerable<IExtension<T>>, IEnumerable where T: IExtensibleObject<T>
    {
        private T owner;

        public ExtensionCollection(T owner)
        {
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");
            }
            this.owner = owner;
        }

        public ExtensionCollection(T owner, object syncRoot) : base(syncRoot)
        {
            if (owner == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("owner");
            }
            this.owner = owner;
        }

        protected override void ClearItems()
        {
            lock (base.SyncRoot)
            {
                IExtension<T>[] array = new IExtension<T>[base.Count];
                base.CopyTo(array, 0);
                base.ClearItems();
                foreach (IExtension<T> extension in array)
                {
                    extension.Detach(this.owner);
                }
            }
        }

        public E Find<E>()
        {
            List<IExtension<T>> items = base.Items;
            lock (base.SyncRoot)
            {
                for (int i = base.Count - 1; i >= 0; i--)
                {
                    IExtension<T> extension = items[i];
                    if (extension is E)
                    {
                        return (E) extension;
                    }
                }
            }
            return default(E);
        }

        public Collection<E> FindAll<E>()
        {
            Collection<E> collection = new Collection<E>();
            List<IExtension<T>> items = base.Items;
            lock (base.SyncRoot)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    IExtension<T> extension = items[i];
                    if (extension is E)
                    {
                        collection.Add((E) extension);
                    }
                }
            }
            return collection;
        }

        protected override void InsertItem(int index, IExtension<T> item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            lock (base.SyncRoot)
            {
                item.Attach(this.owner);
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (base.SyncRoot)
            {
                base.Items[index].Detach(this.owner);
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, IExtension<T> item)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCannotSetExtensionsByIndex")));
        }

        bool ICollection<IExtension<T>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}

