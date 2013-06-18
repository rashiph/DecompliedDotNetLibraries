namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public abstract class BindingManagerBase
    {
        private BindingsCollection bindings;
        private bool pullingData;

        public event BindingCompleteEventHandler BindingComplete;

        public event EventHandler CurrentChanged;

        public event EventHandler CurrentItemChanged;

        public event BindingManagerDataErrorEventHandler DataError;

        public event EventHandler PositionChanged;

        public BindingManagerBase()
        {
        }

        internal BindingManagerBase(object dataSource)
        {
            this.SetDataSource(dataSource);
        }

        public abstract void AddNew();
        internal void Binding_BindingComplete(object sender, BindingCompleteEventArgs args)
        {
            this.OnBindingComplete(args);
        }

        public abstract void CancelCurrentEdit();
        public abstract void EndCurrentEdit();
        public virtual PropertyDescriptorCollection GetItemProperties()
        {
            return this.GetItemProperties(null);
        }

        internal abstract PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors);
        protected internal virtual PropertyDescriptorCollection GetItemProperties(ArrayList dataSources, ArrayList listAccessors)
        {
            IList list = null;
            if (this is CurrencyManager)
            {
                list = ((CurrencyManager) this).List;
            }
            if (list is ITypedList)
            {
                PropertyDescriptor[] array = new PropertyDescriptor[listAccessors.Count];
                listAccessors.CopyTo(array, 0);
                return ((ITypedList) list).GetItemProperties(array);
            }
            return this.GetItemProperties(this.BindType, 0, dataSources, listAccessors);
        }

        protected virtual PropertyDescriptorCollection GetItemProperties(System.Type listType, int offset, ArrayList dataSources, ArrayList listAccessors)
        {
            if (listAccessors.Count >= offset)
            {
                if (listAccessors.Count == offset)
                {
                    if (!typeof(IList).IsAssignableFrom(listType))
                    {
                        return TypeDescriptor.GetProperties(listType);
                    }
                    PropertyInfo[] infoArray = listType.GetProperties();
                    for (int i = 0; i < infoArray.Length; i++)
                    {
                        if ("Item".Equals(infoArray[i].Name) && (infoArray[i].PropertyType != typeof(object)))
                        {
                            return TypeDescriptor.GetProperties(infoArray[i].PropertyType, new Attribute[] { new BrowsableAttribute(true) });
                        }
                    }
                    IList list = dataSources[offset - 1] as IList;
                    if ((list == null) || (list.Count <= 0))
                    {
                        return null;
                    }
                    return TypeDescriptor.GetProperties(list[0]);
                }
                PropertyInfo[] properties = listType.GetProperties();
                if (typeof(IList).IsAssignableFrom(listType))
                {
                    PropertyDescriptorCollection descriptors = null;
                    for (int j = 0; j < properties.Length; j++)
                    {
                        if ("Item".Equals(properties[j].Name) && (properties[j].PropertyType != typeof(object)))
                        {
                            descriptors = TypeDescriptor.GetProperties(properties[j].PropertyType, new Attribute[] { new BrowsableAttribute(true) });
                        }
                    }
                    if (descriptors == null)
                    {
                        IList dataSource;
                        if (offset == 0)
                        {
                            dataSource = this.DataSource as IList;
                        }
                        else
                        {
                            dataSource = dataSources[offset - 1] as IList;
                        }
                        if ((dataSource != null) && (dataSource.Count > 0))
                        {
                            descriptors = TypeDescriptor.GetProperties(dataSource[0]);
                        }
                    }
                    if (descriptors != null)
                    {
                        for (int k = 0; k < descriptors.Count; k++)
                        {
                            if (descriptors[k].Equals(listAccessors[offset]))
                            {
                                return this.GetItemProperties(descriptors[k].PropertyType, offset + 1, dataSources, listAccessors);
                            }
                        }
                    }
                }
                else
                {
                    for (int m = 0; m < properties.Length; m++)
                    {
                        if (properties[m].Name.Equals(((PropertyDescriptor) listAccessors[offset]).Name))
                        {
                            return this.GetItemProperties(properties[m].PropertyType, offset + 1, dataSources, listAccessors);
                        }
                    }
                }
            }
            return null;
        }

        internal abstract string GetListName();
        protected internal abstract string GetListName(ArrayList listAccessors);
        protected internal void OnBindingComplete(BindingCompleteEventArgs args)
        {
            if (this.onBindingCompleteHandler != null)
            {
                this.onBindingCompleteHandler(this, args);
            }
        }

        private void OnBindingsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            Binding element = e.Element as Binding;
            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    element.BindingComplete += new BindingCompleteEventHandler(this.Binding_BindingComplete);
                    return;

                case CollectionChangeAction.Remove:
                    element.BindingComplete -= new BindingCompleteEventHandler(this.Binding_BindingComplete);
                    return;

                case CollectionChangeAction.Refresh:
                    foreach (Binding binding2 in this.bindings)
                    {
                        binding2.BindingComplete += new BindingCompleteEventHandler(this.Binding_BindingComplete);
                    }
                    return;
            }
        }

        private void OnBindingsCollectionChanging(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Refresh)
            {
                foreach (Binding binding in this.bindings)
                {
                    binding.BindingComplete -= new BindingCompleteEventHandler(this.Binding_BindingComplete);
                }
            }
        }

        protected internal abstract void OnCurrentChanged(EventArgs e);
        protected internal abstract void OnCurrentItemChanged(EventArgs e);
        protected internal void OnDataError(Exception e)
        {
            if (this.onDataErrorHandler != null)
            {
                this.onDataErrorHandler(this, new BindingManagerDataErrorEventArgs(e));
            }
        }

        protected void PullData()
        {
            bool flag;
            this.PullData(out flag);
        }

        internal void PullData(out bool success)
        {
            success = true;
            this.pullingData = true;
            try
            {
                this.UpdateIsBinding();
                int count = this.Bindings.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.Bindings[i].PullData())
                    {
                        success = false;
                    }
                }
            }
            finally
            {
                this.pullingData = false;
            }
        }

        protected void PushData()
        {
            bool flag;
            this.PushData(out flag);
        }

        internal void PushData(out bool success)
        {
            success = true;
            if (!this.pullingData)
            {
                this.UpdateIsBinding();
                int count = this.Bindings.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this.Bindings[i].PushData())
                    {
                        success = false;
                    }
                }
            }
        }

        public abstract void RemoveAt(int index);
        public abstract void ResumeBinding();
        internal abstract void SetDataSource(object dataSource);
        public abstract void SuspendBinding();
        protected abstract void UpdateIsBinding();

        public BindingsCollection Bindings
        {
            get
            {
                if (this.bindings == null)
                {
                    this.bindings = new ListManagerBindingsCollection(this);
                    this.bindings.CollectionChanging += new CollectionChangeEventHandler(this.OnBindingsCollectionChanging);
                    this.bindings.CollectionChanged += new CollectionChangeEventHandler(this.OnBindingsCollectionChanged);
                }
                return this.bindings;
            }
        }

        internal abstract System.Type BindType { get; }

        public abstract int Count { get; }

        public abstract object Current { get; }

        internal abstract object DataSource { get; }

        internal abstract bool IsBinding { get; }

        public bool IsBindingSuspended
        {
            get
            {
                return !this.IsBinding;
            }
        }

        public abstract int Position { get; set; }
    }
}

