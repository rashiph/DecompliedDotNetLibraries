namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [DefaultEvent("CollectionChanged")]
    public class BindingContext : ICollection, IEnumerable
    {
        private Hashtable listManagers = new Hashtable();

        [Browsable(false), System.Windows.Forms.SRDescription("collectionChangedEventDescr"), EditorBrowsable(EditorBrowsableState.Never)]
        public event CollectionChangeEventHandler CollectionChanged
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
            }
        }

        protected internal void Add(object dataSource, BindingManagerBase listManager)
        {
            this.AddCore(dataSource, listManager);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, dataSource));
        }

        protected virtual void AddCore(object dataSource, BindingManagerBase listManager)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            if (listManager == null)
            {
                throw new ArgumentNullException("listManager");
            }
            this.listManagers[this.GetKey(dataSource, "")] = new WeakReference(listManager, false);
        }

        private static void CheckPropertyBindingCycles(BindingContext newBindingContext, Binding propBinding)
        {
            if (((newBindingContext != null) && (propBinding != null)) && newBindingContext.Contains(propBinding.BindableComponent, ""))
            {
                BindingManagerBase base2 = newBindingContext.EnsureListManager(propBinding.BindableComponent, "");
                for (int i = 0; i < base2.Bindings.Count; i++)
                {
                    Binding binding = base2.Bindings[i];
                    if (binding.DataSource == propBinding.BindableComponent)
                    {
                        if (propBinding.BindToObject.BindingMemberInfo.BindingMember.Equals(binding.PropertyName))
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("DataBindingCycle", new object[] { binding.PropertyName }), "propBinding");
                        }
                    }
                    else if (propBinding.BindToObject.BindingManagerBase is PropertyManager)
                    {
                        CheckPropertyBindingCycles(newBindingContext, binding);
                    }
                }
            }
        }

        protected internal void Clear()
        {
            this.ClearCore();
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
        }

        protected virtual void ClearCore()
        {
            this.listManagers.Clear();
        }

        public bool Contains(object dataSource)
        {
            return this.Contains(dataSource, "");
        }

        public bool Contains(object dataSource, string dataMember)
        {
            return this.listManagers.ContainsKey(this.GetKey(dataSource, dataMember));
        }

        internal BindingManagerBase EnsureListManager(object dataSource, string dataMember)
        {
            BindingManagerBase target = null;
            if (dataMember == null)
            {
                dataMember = "";
            }
            if (dataSource is ICurrencyManagerProvider)
            {
                target = (dataSource as ICurrencyManagerProvider).GetRelatedCurrencyManager(dataMember);
                if (target != null)
                {
                    return target;
                }
            }
            HashKey key = this.GetKey(dataSource, dataMember);
            WeakReference reference = this.listManagers[key] as WeakReference;
            if (reference != null)
            {
                target = (BindingManagerBase) reference.Target;
            }
            if (target == null)
            {
                if (dataMember.Length == 0)
                {
                    if ((dataSource is IList) || (dataSource is IListSource))
                    {
                        target = new CurrencyManager(dataSource);
                    }
                    else
                    {
                        target = new PropertyManager(dataSource);
                    }
                }
                else
                {
                    int length = dataMember.LastIndexOf(".");
                    string str = (length == -1) ? "" : dataMember.Substring(0, length);
                    string name = dataMember.Substring(length + 1);
                    BindingManagerBase parentManager = this.EnsureListManager(dataSource, str);
                    PropertyDescriptor descriptor = parentManager.GetItemProperties().Find(name, true);
                    if (descriptor == null)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("RelatedListManagerChild", new object[] { name }));
                    }
                    if (typeof(IList).IsAssignableFrom(descriptor.PropertyType))
                    {
                        target = new RelatedCurrencyManager(parentManager, name);
                    }
                    else
                    {
                        target = new RelatedPropertyManager(parentManager, name);
                    }
                }
                if (reference == null)
                {
                    this.listManagers.Add(key, new WeakReference(target, false));
                    return target;
                }
                reference.Target = target;
            }
            return target;
        }

        internal HashKey GetKey(object dataSource, string dataMember)
        {
            return new HashKey(dataSource, dataMember);
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
        }

        protected internal void Remove(object dataSource)
        {
            this.RemoveCore(dataSource);
            this.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, dataSource));
        }

        protected virtual void RemoveCore(object dataSource)
        {
            this.listManagers.Remove(this.GetKey(dataSource, ""));
        }

        private void ScrubWeakRefs()
        {
            object[] array = new object[this.listManagers.Count];
            this.listManagers.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                DictionaryEntry entry = (DictionaryEntry) array[i];
                WeakReference reference = (WeakReference) entry.Value;
                if (reference.Target == null)
                {
                    this.listManagers.Remove(entry.Key);
                }
            }
        }

        void ICollection.CopyTo(Array ar, int index)
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            this.ScrubWeakRefs();
            this.listManagers.CopyTo(ar, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            System.Windows.Forms.IntSecurity.UnmanagedCode.Demand();
            this.ScrubWeakRefs();
            return this.listManagers.GetEnumerator();
        }

        public static void UpdateBinding(BindingContext newBindingContext, Binding binding)
        {
            BindingManagerBase bindingManagerBase = binding.BindingManagerBase;
            if (bindingManagerBase != null)
            {
                bindingManagerBase.Bindings.Remove(binding);
            }
            if (newBindingContext != null)
            {
                if (binding.BindToObject.BindingManagerBase is PropertyManager)
                {
                    CheckPropertyBindingCycles(newBindingContext, binding);
                }
                BindToObject bindToObject = binding.BindToObject;
                newBindingContext.EnsureListManager(bindToObject.DataSource, bindToObject.BindingMemberInfo.BindingPath).Bindings.Add(binding);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public BindingManagerBase this[object dataSource]
        {
            get
            {
                return this[dataSource, ""];
            }
        }

        public BindingManagerBase this[object dataSource, string dataMember]
        {
            get
            {
                return this.EnsureListManager(dataSource, dataMember);
            }
        }

        int ICollection.Count
        {
            get
            {
                this.ScrubWeakRefs();
                return this.listManagers.Count;
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
                return null;
            }
        }

        internal class HashKey
        {
            private string dataMember;
            private int dataSourceHashCode;
            private WeakReference wRef;

            internal HashKey(object dataSource, string dataMember)
            {
                if (dataSource == null)
                {
                    throw new ArgumentNullException("dataSource");
                }
                if (dataMember == null)
                {
                    dataMember = "";
                }
                this.wRef = new WeakReference(dataSource, false);
                this.dataSourceHashCode = dataSource.GetHashCode();
                this.dataMember = dataMember.ToLower(CultureInfo.InvariantCulture);
            }

            public override bool Equals(object target)
            {
                if (!(target is BindingContext.HashKey))
                {
                    return false;
                }
                BindingContext.HashKey key = (BindingContext.HashKey) target;
                return ((this.wRef.Target == key.wRef.Target) && (this.dataMember == key.dataMember));
            }

            public override int GetHashCode()
            {
                return (this.dataSourceHashCode * this.dataMember.GetHashCode());
            }
        }
    }
}

