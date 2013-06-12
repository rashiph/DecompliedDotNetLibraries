namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class BindingList<T> : Collection<T>, IBindingList, IList, ICollection, IEnumerable, ICancelAddNew, IRaiseItemChangedEvents
    {
        private int addNewPos;
        private bool allowEdit;
        private bool allowNew;
        private bool allowRemove;
        [NonSerialized]
        private PropertyDescriptorCollection itemTypeProperties;
        [NonSerialized]
        private int lastChangeIndex;
        [NonSerialized]
        private AddingNewEventHandler onAddingNew;
        [NonSerialized]
        private PropertyChangedEventHandler propertyChangedEventHandler;
        private bool raiseItemChangedEvents;
        private bool raiseListChangedEvents;
        private bool userSetAllowNew;

        public event AddingNewEventHandler AddingNew
        {
            add
            {
                bool allowNew = this.AllowNew;
                this.onAddingNew = (AddingNewEventHandler) Delegate.Combine(this.onAddingNew, value);
                if (allowNew != this.AllowNew)
                {
                    this.FireListChanged(ListChangedType.Reset, -1);
                }
            }
            remove
            {
                bool allowNew = this.AllowNew;
                this.onAddingNew = (AddingNewEventHandler) Delegate.Remove(this.onAddingNew, value);
                if (allowNew != this.AllowNew)
                {
                    this.FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        [field: NonSerialized]
        public event ListChangedEventHandler ListChanged;

        public BindingList()
        {
            this.addNewPos = -1;
            this.raiseListChangedEvents = true;
            this.lastChangeIndex = -1;
            this.allowNew = true;
            this.allowEdit = true;
            this.allowRemove = true;
            this.Initialize();
        }

        public BindingList(IList<T> list) : base(list)
        {
            this.addNewPos = -1;
            this.raiseListChangedEvents = true;
            this.lastChangeIndex = -1;
            this.allowNew = true;
            this.allowEdit = true;
            this.allowRemove = true;
            this.Initialize();
        }

        public T AddNew()
        {
            return (T) ((IBindingList) this).AddNew();
        }

        protected virtual object AddNewCore()
        {
            object obj2 = this.FireAddingNew();
            if (obj2 == null)
            {
                Type type = typeof(T);
                obj2 = SecurityUtils.SecureCreateInstance(type);
            }
            base.Add((T) obj2);
            return obj2;
        }

        protected virtual void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        public virtual void CancelNew(int itemIndex)
        {
            if ((this.addNewPos >= 0) && (this.addNewPos == itemIndex))
            {
                this.RemoveItem(this.addNewPos);
                this.addNewPos = -1;
            }
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T local;
            if (!this.RaiseListChangedEvents)
            {
                return;
            }
            if (((sender == null) || (e == null)) || string.IsNullOrEmpty(e.PropertyName))
            {
                this.ResetBindings();
                return;
            }
            try
            {
                local = (T) sender;
            }
            catch (InvalidCastException)
            {
                this.ResetBindings();
                return;
            }
            int lastChangeIndex = this.lastChangeIndex;
            if ((lastChangeIndex >= 0) && (lastChangeIndex < base.Count))
            {
                T local2 = base[lastChangeIndex];
                if (local2.Equals(local))
                {
                    goto Label_007B;
                }
            }
            lastChangeIndex = base.IndexOf(local);
            this.lastChangeIndex = lastChangeIndex;
        Label_007B:
            if (lastChangeIndex == -1)
            {
                this.UnhookPropertyChanged(local);
                this.ResetBindings();
            }
            else
            {
                if (this.itemTypeProperties == null)
                {
                    this.itemTypeProperties = TypeDescriptor.GetProperties(typeof(T));
                }
                PropertyDescriptor propDesc = this.itemTypeProperties.Find(e.PropertyName, true);
                ListChangedEventArgs args = new ListChangedEventArgs(ListChangedType.ItemChanged, lastChangeIndex, propDesc);
                this.OnListChanged(args);
            }
        }

        protected override void ClearItems()
        {
            this.EndNew(this.addNewPos);
            if (this.raiseItemChangedEvents)
            {
                foreach (T local in base.Items)
                {
                    this.UnhookPropertyChanged(local);
                }
            }
            base.ClearItems();
            this.FireListChanged(ListChangedType.Reset, -1);
        }

        public virtual void EndNew(int itemIndex)
        {
            if ((this.addNewPos >= 0) && (this.addNewPos == itemIndex))
            {
                this.addNewPos = -1;
            }
        }

        protected virtual int FindCore(PropertyDescriptor prop, object key)
        {
            throw new NotSupportedException();
        }

        private object FireAddingNew()
        {
            AddingNewEventArgs e = new AddingNewEventArgs(null);
            this.OnAddingNew(e);
            return e.NewObject;
        }

        private void FireListChanged(ListChangedType type, int index)
        {
            if (this.raiseListChangedEvents)
            {
                this.OnListChanged(new ListChangedEventArgs(type, index));
            }
        }

        private void HookPropertyChanged(T item)
        {
            INotifyPropertyChanged changed = item as INotifyPropertyChanged;
            if (changed != null)
            {
                if (this.propertyChangedEventHandler == null)
                {
                    this.propertyChangedEventHandler = new PropertyChangedEventHandler(this.Child_PropertyChanged);
                }
                changed.PropertyChanged += this.propertyChangedEventHandler;
            }
        }

        private void Initialize()
        {
            this.allowNew = this.ItemTypeHasDefaultConstructor;
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
            {
                this.raiseItemChangedEvents = true;
                foreach (T local in base.Items)
                {
                    this.HookPropertyChanged(local);
                }
            }
        }

        protected override void InsertItem(int index, T item)
        {
            this.EndNew(this.addNewPos);
            base.InsertItem(index, item);
            if (this.raiseItemChangedEvents)
            {
                this.HookPropertyChanged(item);
            }
            this.FireListChanged(ListChangedType.ItemAdded, index);
        }

        protected virtual void OnAddingNew(AddingNewEventArgs e)
        {
            if (this.onAddingNew != null)
            {
                this.onAddingNew(this, e);
            }
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            if (this.onListChanged != null)
            {
                this.onListChanged(this, e);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (!this.allowRemove && ((this.addNewPos < 0) || (this.addNewPos != index)))
            {
                throw new NotSupportedException();
            }
            this.EndNew(this.addNewPos);
            if (this.raiseItemChangedEvents)
            {
                this.UnhookPropertyChanged(base[index]);
            }
            base.RemoveItem(index);
            this.FireListChanged(ListChangedType.ItemDeleted, index);
        }

        protected virtual void RemoveSortCore()
        {
            throw new NotSupportedException();
        }

        public void ResetBindings()
        {
            this.FireListChanged(ListChangedType.Reset, -1);
        }

        public void ResetItem(int position)
        {
            this.FireListChanged(ListChangedType.ItemChanged, position);
        }

        protected override void SetItem(int index, T item)
        {
            if (this.raiseItemChangedEvents)
            {
                this.UnhookPropertyChanged(base[index]);
            }
            base.SetItem(index, item);
            if (this.raiseItemChangedEvents)
            {
                this.HookPropertyChanged(item);
            }
            this.FireListChanged(ListChangedType.ItemChanged, index);
        }

        void IBindingList.AddIndex(PropertyDescriptor prop)
        {
        }

        object IBindingList.AddNew()
        {
            object obj2 = this.AddNewCore();
            this.addNewPos = (obj2 != null) ? base.IndexOf((T) obj2) : -1;
            return obj2;
        }

        void IBindingList.ApplySort(PropertyDescriptor prop, ListSortDirection direction)
        {
            this.ApplySortCore(prop, direction);
        }

        int IBindingList.Find(PropertyDescriptor prop, object key)
        {
            return this.FindCore(prop, key);
        }

        void IBindingList.RemoveIndex(PropertyDescriptor prop)
        {
        }

        void IBindingList.RemoveSort()
        {
            this.RemoveSortCore();
        }

        private void UnhookPropertyChanged(T item)
        {
            INotifyPropertyChanged changed = item as INotifyPropertyChanged;
            if ((changed != null) && (this.propertyChangedEventHandler != null))
            {
                changed.PropertyChanged -= this.propertyChangedEventHandler;
            }
        }

        private bool AddingNewHandled
        {
            get
            {
                return ((this.onAddingNew != null) && (this.onAddingNew.GetInvocationList().Length > 0));
            }
        }

        public bool AllowEdit
        {
            get
            {
                return this.allowEdit;
            }
            set
            {
                if (this.allowEdit != value)
                {
                    this.allowEdit = value;
                    this.FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        public bool AllowNew
        {
            get
            {
                if (!this.userSetAllowNew && !this.allowNew)
                {
                    return this.AddingNewHandled;
                }
                return this.allowNew;
            }
            set
            {
                bool allowNew = this.AllowNew;
                this.userSetAllowNew = true;
                this.allowNew = value;
                if (allowNew != value)
                {
                    this.FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        public bool AllowRemove
        {
            get
            {
                return this.allowRemove;
            }
            set
            {
                if (this.allowRemove != value)
                {
                    this.allowRemove = value;
                    this.FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        protected virtual bool IsSortedCore
        {
            get
            {
                return false;
            }
        }

        private bool ItemTypeHasDefaultConstructor
        {
            get
            {
                Type type = typeof(T);
                return (type.IsPrimitive || (type.GetConstructor(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) != null));
            }
        }

        public bool RaiseListChangedEvents
        {
            get
            {
                return this.raiseListChangedEvents;
            }
            set
            {
                if (this.raiseListChangedEvents != value)
                {
                    this.raiseListChangedEvents = value;
                }
            }
        }

        protected virtual ListSortDirection SortDirectionCore
        {
            get
            {
                return ListSortDirection.Ascending;
            }
        }

        protected virtual PropertyDescriptor SortPropertyCore
        {
            get
            {
                return null;
            }
        }

        protected virtual bool SupportsChangeNotificationCore
        {
            get
            {
                return true;
            }
        }

        protected virtual bool SupportsSearchingCore
        {
            get
            {
                return false;
            }
        }

        protected virtual bool SupportsSortingCore
        {
            get
            {
                return false;
            }
        }

        bool IBindingList.AllowEdit
        {
            get
            {
                return this.AllowEdit;
            }
        }

        bool IBindingList.AllowNew
        {
            get
            {
                return this.AllowNew;
            }
        }

        bool IBindingList.AllowRemove
        {
            get
            {
                return this.AllowRemove;
            }
        }

        bool IBindingList.IsSorted
        {
            get
            {
                return this.IsSortedCore;
            }
        }

        ListSortDirection IBindingList.SortDirection
        {
            get
            {
                return this.SortDirectionCore;
            }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get
            {
                return this.SortPropertyCore;
            }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get
            {
                return this.SupportsChangeNotificationCore;
            }
        }

        bool IBindingList.SupportsSearching
        {
            get
            {
                return this.SupportsSearchingCore;
            }
        }

        bool IBindingList.SupportsSorting
        {
            get
            {
                return this.SupportsSortingCore;
            }
        }

        bool IRaiseItemChangedEvents.RaisesItemChangedEvents
        {
            get
            {
                return this.raiseItemChangedEvents;
            }
        }
    }
}

