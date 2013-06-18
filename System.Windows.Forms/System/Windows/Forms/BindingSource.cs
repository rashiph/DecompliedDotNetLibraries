namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    [System.Windows.Forms.SRDescription("DescriptionBindingSource"), DefaultProperty("DataSource"), DefaultEvent("CurrentChanged"), Designer("System.Windows.Forms.Design.BindingSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComplexBindingProperties("DataSource", "DataMember")]
    public class BindingSource : Component, IBindingListView, IBindingList, IList, ICollection, IEnumerable, ITypedList, ICancelAddNew, ISupportInitializeNotification, ISupportInitialize, ICurrencyManagerProvider
    {
        private IList _innerList;
        private int addNewPos;
        private bool allowNewIsSet;
        private bool allowNewSetValue;
        private System.Windows.Forms.CurrencyManager currencyManager;
        private object currentItemHookedForItemChange;
        private string dataMember;
        private object dataSource;
        private bool disposedOrFinalized;
        private bool endingEdit;
        private static readonly object EVENT_ADDINGNEW = new object();
        private static readonly object EVENT_BINDINGCOMPLETE = new object();
        private static readonly object EVENT_CURRENTCHANGED = new object();
        private static readonly object EVENT_CURRENTITEMCHANGED = new object();
        private static readonly object EVENT_DATAERROR = new object();
        private static readonly object EVENT_DATAMEMBERCHANGED = new object();
        private static readonly object EVENT_DATASOURCECHANGED = new object();
        private static readonly object EVENT_INITIALIZED = new object();
        private static readonly object EVENT_LISTCHANGED = new object();
        private static readonly object EVENT_POSITIONCHANGED = new object();
        private string filter;
        private bool initializing;
        private bool innerListChanging;
        private bool isBindingList;
        private ConstructorInfo itemConstructor;
        private PropertyDescriptorCollection itemShape;
        private System.Type itemType;
        private object lastCurrentItem;
        private bool listExtractedFromEnumerable;
        private EventHandler listItemPropertyChangedHandler;
        private bool listRaisesItemChangedEvents;
        private bool needToSetList;
        private bool parentsCurrentItemChanging;
        private bool raiseListChangedEvents;
        private bool recursionDetectionFlag;
        private Dictionary<string, BindingSource> relatedBindingSources;
        private string sort;

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingSourceAddingNewEventHandlerDescr")]
        public event AddingNewEventHandler AddingNew
        {
            add
            {
                base.Events.AddHandler(EVENT_ADDINGNEW, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ADDINGNEW, value);
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourceBindingCompleteEventHandlerDescr"), System.Windows.Forms.SRCategory("CatData")]
        public event BindingCompleteEventHandler BindingComplete
        {
            add
            {
                base.Events.AddHandler(EVENT_BINDINGCOMPLETE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BINDINGCOMPLETE, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingSourceCurrentChangedEventHandlerDescr")]
        public event EventHandler CurrentChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CURRENTCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CURRENTCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourceCurrentItemChangedEventHandlerDescr"), System.Windows.Forms.SRCategory("CatData")]
        public event EventHandler CurrentItemChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CURRENTITEMCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CURRENTITEMCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourceDataErrorEventHandlerDescr"), System.Windows.Forms.SRCategory("CatData")]
        public event BindingManagerDataErrorEventHandler DataError
        {
            add
            {
                base.Events.AddHandler(EVENT_DATAERROR, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DATAERROR, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingSourceDataMemberChangedEventHandlerDescr")]
        public event EventHandler DataMemberChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DATAMEMBERCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DATAMEMBERCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingSourceDataSourceChangedEventHandlerDescr")]
        public event EventHandler DataSourceChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DATASOURCECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DATASOURCECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("BindingSourceListChangedEventHandlerDescr")]
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_LISTCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_LISTCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourcePositionChangedEventHandlerDescr"), System.Windows.Forms.SRCategory("CatData")]
        public event EventHandler PositionChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_POSITIONCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_POSITIONCHANGED, value);
            }
        }

        event EventHandler ISupportInitializeNotification.Initialized
        {
            add
            {
                base.Events.AddHandler(EVENT_INITIALIZED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_INITIALIZED, value);
            }
        }

        public BindingSource() : this(null, string.Empty)
        {
        }

        public BindingSource(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        public BindingSource(object dataSource, string dataMember)
        {
            this.dataMember = string.Empty;
            this.raiseListChangedEvents = true;
            this.allowNewSetValue = true;
            this.addNewPos = -1;
            this.dataSource = dataSource;
            this.dataMember = dataMember;
            this._innerList = new ArrayList();
            this.currencyManager = new System.Windows.Forms.CurrencyManager(this);
            this.WireCurrencyManager(this.currencyManager);
            this.ResetList();
            this.listItemPropertyChangedHandler = new EventHandler(this.ListItem_PropertyChanged);
            this.WireDataSource();
        }

        public virtual int Add(object value)
        {
            int newIndex = -1;
            if ((this.dataSource == null) && (this.List.Count == 0))
            {
                this.SetList(CreateBindingList((value == null) ? typeof(object) : value.GetType()), true, true);
            }
            if ((value != null) && !this.itemType.IsAssignableFrom(value.GetType()))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceItemTypeMismatchOnAdd"));
            }
            if ((value == null) && this.itemType.IsValueType)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceItemTypeIsValueType"));
            }
            newIndex = this.List.Add(value);
            this.OnSimpleListChanged(ListChangedType.ItemAdded, newIndex);
            return newIndex;
        }

        public virtual object AddNew()
        {
            if (!this.AllowNewInternal(false))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceBindingListWrapperAddToReadOnlyList"));
            }
            if (!this.AllowNewInternal(true))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceBindingListWrapperNeedToSetAllowNew", new object[] { (this.itemType == null) ? "(null)" : this.itemType.FullName }));
            }
            int addNewPos = this.addNewPos;
            this.EndEdit();
            if (addNewPos != -1)
            {
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, addNewPos));
            }
            AddingNewEventArgs e = new AddingNewEventArgs();
            int count = this.List.Count;
            this.OnAddingNew(e);
            object newObject = e.NewObject;
            if (newObject == null)
            {
                if (this.isBindingList)
                {
                    newObject = (this.List as IBindingList).AddNew();
                    this.Position = this.Count - 1;
                    return newObject;
                }
                if (this.itemConstructor == null)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceBindingListWrapperNeedAParameterlessConstructor", new object[] { (this.itemType == null) ? "(null)" : this.itemType.FullName }));
                }
                newObject = this.itemConstructor.Invoke(null);
            }
            if (this.List.Count > count)
            {
                this.addNewPos = this.Position;
                return newObject;
            }
            this.addNewPos = this.Add(newObject);
            this.Position = this.addNewPos;
            return newObject;
        }

        private bool AllowNewInternal(bool checkconstructor)
        {
            if (this.disposedOrFinalized)
            {
                return false;
            }
            if (this.allowNewIsSet)
            {
                return this.allowNewSetValue;
            }
            if (this.listExtractedFromEnumerable)
            {
                return false;
            }
            if (this.isBindingList)
            {
                return ((IBindingList) this.List).AllowNew;
            }
            return this.IsListWriteable(checkconstructor);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ApplySort(ListSortDescriptionCollection sorts)
        {
            IBindingListView list = this.List as IBindingListView;
            if (list == null)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("OperationRequiresIBindingListView"));
            }
            list.ApplySort(sorts);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ApplySort(PropertyDescriptor property, ListSortDirection sort)
        {
            if (!this.isBindingList)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("OperationRequiresIBindingList"));
            }
            ((IBindingList) this.List).ApplySort(property, sort);
        }

        private static string BuildSortString(ListSortDescriptionCollection sortsColln)
        {
            if (sortsColln == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(sortsColln.Count);
            for (int i = 0; i < sortsColln.Count; i++)
            {
                builder.Append(sortsColln[i].PropertyDescriptor.Name + ((sortsColln[i].SortDirection == ListSortDirection.Ascending) ? " ASC" : " DESC") + ((i < (sortsColln.Count - 1)) ? "," : string.Empty));
            }
            return builder.ToString();
        }

        public void CancelEdit()
        {
            this.currencyManager.CancelCurrentEdit();
        }

        public virtual void Clear()
        {
            this.UnhookItemChangedEventsForOldCurrent();
            this.List.Clear();
            this.OnSimpleListChanged(ListChangedType.Reset, -1);
        }

        private void ClearInvalidDataMember()
        {
            if (!this.IsDataMemberValid())
            {
                this.dataMember = "";
                this.OnDataMemberChanged(EventArgs.Empty);
            }
        }

        public virtual bool Contains(object value)
        {
            return this.List.Contains(value);
        }

        public virtual void CopyTo(Array arr, int index)
        {
            this.List.CopyTo(arr, index);
        }

        private static IList CreateBindingList(System.Type type)
        {
            System.Type type2 = typeof(BindingList<>);
            return (IList) System.Windows.Forms.SecurityUtils.SecureCreateInstance(type2.MakeGenericType(new System.Type[] { type }));
        }

        private static object CreateInstanceOfType(System.Type type)
        {
            object obj2 = null;
            Exception innerException = null;
            try
            {
                obj2 = System.Windows.Forms.SecurityUtils.SecureCreateInstance(type);
            }
            catch (TargetInvocationException exception2)
            {
                innerException = exception2;
            }
            catch (MethodAccessException exception3)
            {
                innerException = exception3;
            }
            catch (MissingMethodException exception4)
            {
                innerException = exception4;
            }
            if (innerException != null)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("BindingSourceInstanceError"), innerException);
            }
            return obj2;
        }

        private void CurrencyManager_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            this.OnBindingComplete(e);
        }

        private void CurrencyManager_CurrentChanged(object sender, EventArgs e)
        {
            this.OnCurrentChanged(EventArgs.Empty);
        }

        private void CurrencyManager_CurrentItemChanged(object sender, EventArgs e)
        {
            this.OnCurrentItemChanged(EventArgs.Empty);
        }

        private void CurrencyManager_DataError(object sender, BindingManagerDataErrorEventArgs e)
        {
            this.OnDataError(e);
        }

        private void CurrencyManager_PositionChanged(object sender, EventArgs e)
        {
            this.OnPositionChanged(e);
        }

        private void DataSource_Initialized(object sender, EventArgs e)
        {
            ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
            if (dataSource != null)
            {
                dataSource.Initialized -= new EventHandler(this.DataSource_Initialized);
            }
            this.EndInitCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UnwireDataSource();
                this.UnwireInnerList();
                this.UnhookItemChangedEventsForOldCurrent();
                this.UnwireCurrencyManager(this.currencyManager);
                this.dataSource = null;
                this.sort = null;
                this.dataMember = null;
                this._innerList = null;
                this.isBindingList = false;
                this.needToSetList = true;
                this.raiseListChangedEvents = false;
            }
            this.disposedOrFinalized = true;
            base.Dispose(disposing);
        }

        public void EndEdit()
        {
            if (!this.endingEdit)
            {
                try
                {
                    this.endingEdit = true;
                    this.currencyManager.EndCurrentEdit();
                }
                finally
                {
                    this.endingEdit = false;
                }
            }
        }

        private void EndInitCore()
        {
            this.initializing = false;
            this.EnsureInnerList();
            this.OnInitialized();
        }

        private void EnsureInnerList()
        {
            if (!this.initializing && this.needToSetList)
            {
                this.needToSetList = false;
                this.ResetList();
            }
        }

        public virtual int Find(PropertyDescriptor prop, object key)
        {
            if (!this.isBindingList)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("OperationRequiresIBindingList"));
            }
            return ((IBindingList) this.List).Find(prop, key);
        }

        public int Find(string propertyName, object key)
        {
            PropertyDescriptor property = (this.itemShape == null) ? null : this.itemShape.Find(propertyName, true);
            if (property == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("DataSourceDataMemberPropNotFound", new object[] { propertyName }));
            }
            return this.Find(property, key);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        public virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            object list = ListBindingHelper.GetList(this.dataSource);
            if ((list is ITypedList) && !string.IsNullOrEmpty(this.dataMember))
            {
                return ListBindingHelper.GetListItemProperties(list, this.dataMember, listAccessors);
            }
            return ListBindingHelper.GetListItemProperties(this.List, listAccessors);
        }

        private static IList GetListFromEnumerable(IEnumerable enumerable)
        {
            IList list = null;
            foreach (object obj2 in enumerable)
            {
                if (list == null)
                {
                    list = CreateBindingList(obj2.GetType());
                }
                list.Add(obj2);
            }
            return list;
        }

        private static IList GetListFromType(System.Type type)
        {
            if (typeof(ITypedList).IsAssignableFrom(type) && typeof(IList).IsAssignableFrom(type))
            {
                return (CreateInstanceOfType(type) as IList);
            }
            if (typeof(IListSource).IsAssignableFrom(type))
            {
                return (CreateInstanceOfType(type) as IListSource).GetList();
            }
            return CreateBindingList(ListBindingHelper.GetListItemType(type));
        }

        public virtual string GetListName(PropertyDescriptor[] listAccessors)
        {
            return ListBindingHelper.GetListName(this.List, listAccessors);
        }

        private BindingSource GetRelatedBindingSource(string dataMember)
        {
            if (this.relatedBindingSources == null)
            {
                this.relatedBindingSources = new Dictionary<string, BindingSource>();
            }
            foreach (string str in this.relatedBindingSources.Keys)
            {
                if (string.Equals(str, dataMember, StringComparison.OrdinalIgnoreCase))
                {
                    return this.relatedBindingSources[str];
                }
            }
            BindingSource source = new BindingSource(this, dataMember);
            this.relatedBindingSources[dataMember] = source;
            return source;
        }

        public virtual System.Windows.Forms.CurrencyManager GetRelatedCurrencyManager(string dataMember)
        {
            this.EnsureInnerList();
            if (string.IsNullOrEmpty(dataMember))
            {
                return this.currencyManager;
            }
            if (dataMember.IndexOf(".") != -1)
            {
                return null;
            }
            return this.GetRelatedBindingSource(dataMember).CurrencyManager;
        }

        private void HookItemChangedEventsForNewCurrent()
        {
            if (!this.listRaisesItemChangedEvents)
            {
                if ((this.Position >= 0) && (this.Position <= (this.Count - 1)))
                {
                    this.currentItemHookedForItemChange = this.Current;
                    this.WirePropertyChangedEvents(this.currentItemHookedForItemChange);
                }
                else
                {
                    this.currentItemHookedForItemChange = null;
                }
            }
        }

        public virtual int IndexOf(object value)
        {
            return this.List.IndexOf(value);
        }

        private void InnerList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (!this.innerListChanging)
            {
                try
                {
                    this.innerListChanging = true;
                    this.OnListChanged(e);
                }
                finally
                {
                    this.innerListChanging = false;
                }
            }
        }

        public virtual void Insert(int index, object value)
        {
            this.List.Insert(index, value);
            this.OnSimpleListChanged(ListChangedType.ItemAdded, index);
        }

        private bool IsDataMemberValid()
        {
            return (this.initializing || (string.IsNullOrEmpty(this.dataMember) || (ListBindingHelper.GetListItemProperties(this.dataSource)[this.dataMember] != null)));
        }

        private bool IsListWriteable(bool checkconstructor)
        {
            if (this.List.IsReadOnly || this.List.IsFixedSize)
            {
                return false;
            }
            if (checkconstructor)
            {
                return (this.itemConstructor != null);
            }
            return true;
        }

        private void ListItem_PropertyChanged(object sender, EventArgs e)
        {
            int position;
            if (sender == this.currentItemHookedForItemChange)
            {
                position = this.Position;
            }
            else
            {
                position = this.IndexOf(sender);
            }
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, position));
        }

        public void MoveFirst()
        {
            this.Position = 0;
        }

        public void MoveLast()
        {
            this.Position = this.Count - 1;
        }

        public void MoveNext()
        {
            this.Position++;
        }

        public void MovePrevious()
        {
            this.Position--;
        }

        protected virtual void OnAddingNew(AddingNewEventArgs e)
        {
            AddingNewEventHandler handler = (AddingNewEventHandler) base.Events[EVENT_ADDINGNEW];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnBindingComplete(BindingCompleteEventArgs e)
        {
            BindingCompleteEventHandler handler = (BindingCompleteEventHandler) base.Events[EVENT_BINDINGCOMPLETE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCurrentChanged(EventArgs e)
        {
            this.UnhookItemChangedEventsForOldCurrent();
            this.HookItemChangedEventsForNewCurrent();
            EventHandler handler = (EventHandler) base.Events[EVENT_CURRENTCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCurrentItemChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_CURRENTITEMCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDataError(BindingManagerDataErrorEventArgs e)
        {
            BindingManagerDataErrorEventHandler handler = base.Events[EVENT_DATAERROR] as BindingManagerDataErrorEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDataMemberChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_DATAMEMBERCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_DATASOURCECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnInitialized()
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_INITIALIZED];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            if (this.raiseListChangedEvents && !this.initializing)
            {
                ListChangedEventHandler handler = (ListChangedEventHandler) base.Events[EVENT_LISTCHANGED];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        protected virtual void OnPositionChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_POSITIONCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnSimpleListChanged(ListChangedType listChangedType, int newIndex)
        {
            if (!this.isBindingList)
            {
                this.OnListChanged(new ListChangedEventArgs(listChangedType, newIndex));
            }
        }

        private void ParentCurrencyManager_CurrentItemChanged(object sender, EventArgs e)
        {
            if (!this.initializing && !this.parentsCurrentItemChanging)
            {
                try
                {
                    bool flag;
                    this.parentsCurrentItemChanging = true;
                    this.currencyManager.PullData(out flag);
                }
                finally
                {
                    this.parentsCurrentItemChanging = false;
                }
                System.Windows.Forms.CurrencyManager manager = (System.Windows.Forms.CurrencyManager) sender;
                bool flag2 = true;
                if (!string.IsNullOrEmpty(this.dataMember))
                {
                    object obj2 = null;
                    IList list = null;
                    if (manager.Count > 0)
                    {
                        PropertyDescriptor descriptor = manager.GetItemProperties()[this.dataMember];
                        if (descriptor != null)
                        {
                            obj2 = ListBindingHelper.GetList(descriptor.GetValue(manager.Current));
                            list = obj2 as IList;
                        }
                    }
                    if (list != null)
                    {
                        this.SetList(list, false, true);
                    }
                    else if (obj2 != null)
                    {
                        this.SetList(WrapObjectInBindingList(obj2), false, false);
                    }
                    else
                    {
                        this.SetList(CreateBindingList(this.itemType), false, false);
                    }
                    flag2 = (((this.lastCurrentItem == null) || (manager.Count == 0)) || (this.lastCurrentItem != manager.Current)) || (this.Position >= this.Count);
                    this.lastCurrentItem = (manager.Count > 0) ? manager.Current : null;
                    if (flag2)
                    {
                        this.Position = (this.Count > 0) ? 0 : -1;
                    }
                }
                this.OnCurrentItemChanged(EventArgs.Empty);
            }
        }

        private void ParentCurrencyManager_MetaDataChanged(object sender, EventArgs e)
        {
            this.ClearInvalidDataMember();
            this.ResetList();
        }

        private ListSortDescriptionCollection ParseSortString(string sortString)
        {
            if (string.IsNullOrEmpty(sortString))
            {
                return new ListSortDescriptionCollection();
            }
            ArrayList list = new ArrayList();
            PropertyDescriptorCollection itemProperties = this.currencyManager.GetItemProperties();
            string[] strArray = sortString.Split(new char[] { ',' });
            for (int i = 0; i < strArray.Length; i++)
            {
                string strA = strArray[i].Trim();
                int length = strA.Length;
                bool flag = true;
                if ((length >= 5) && (string.Compare(strA, length - 4, " ASC", 0, 4, true, CultureInfo.InvariantCulture) == 0))
                {
                    strA = strA.Substring(0, length - 4).Trim();
                }
                else if ((length >= 6) && (string.Compare(strA, length - 5, " DESC", 0, 5, true, CultureInfo.InvariantCulture) == 0))
                {
                    flag = false;
                    strA = strA.Substring(0, length - 5).Trim();
                }
                if (strA.StartsWith("["))
                {
                    if (!strA.EndsWith("]"))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingSourceBadSortString"));
                    }
                    strA = strA.Substring(1, strA.Length - 2);
                }
                PropertyDescriptor property = itemProperties.Find(strA, true);
                if (property == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("BindingSourceSortStringPropertyNotInIBindingList"));
                }
                list.Add(new ListSortDescription(property, flag ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }
            ListSortDescription[] array = new ListSortDescription[list.Count];
            list.CopyTo(array);
            return new ListSortDescriptionCollection(array);
        }

        public virtual void Remove(object value)
        {
            int index = this.IndexOf(value);
            this.List.Remove(value);
            if (index != -1)
            {
                this.OnSimpleListChanged(ListChangedType.ItemDeleted, index);
            }
        }

        public virtual void RemoveAt(int index)
        {
            object obj1 = this[index];
            this.List.RemoveAt(index);
            this.OnSimpleListChanged(ListChangedType.ItemDeleted, index);
        }

        public void RemoveCurrent()
        {
            if (!this.AllowRemove)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceRemoveCurrentNotAllowed"));
            }
            if ((this.Position < 0) || (this.Position >= this.Count))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceRemoveCurrentNoCurrentItem"));
            }
            this.RemoveAt(this.Position);
        }

        public virtual void RemoveFilter()
        {
            this.filter = null;
            IBindingListView list = this.List as IBindingListView;
            if (list != null)
            {
                list.RemoveFilter();
            }
        }

        public virtual void RemoveSort()
        {
            this.sort = null;
            if (this.isBindingList)
            {
                ((IBindingList) this.List).RemoveSort();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual void ResetAllowNew()
        {
            this.allowNewIsSet = false;
            this.allowNewSetValue = true;
        }

        public void ResetBindings(bool metadataChanged)
        {
            if (metadataChanged)
            {
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, null));
            }
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public void ResetCurrentItem()
        {
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, this.Position));
        }

        public void ResetItem(int itemIndex)
        {
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, itemIndex));
        }

        private void ResetList()
        {
            if (this.initializing)
            {
                this.needToSetList = true;
            }
            else
            {
                this.needToSetList = false;
                object dataSource = (this.dataSource is System.Type) ? GetListFromType(this.dataSource as System.Type) : this.dataSource;
                object obj3 = ListBindingHelper.GetList(dataSource, this.dataMember);
                this.listExtractedFromEnumerable = false;
                IList listFromEnumerable = null;
                if (obj3 is IList)
                {
                    listFromEnumerable = obj3 as IList;
                }
                else
                {
                    if (obj3 is IListSource)
                    {
                        listFromEnumerable = (obj3 as IListSource).GetList();
                    }
                    else if (obj3 is IEnumerable)
                    {
                        listFromEnumerable = GetListFromEnumerable(obj3 as IEnumerable);
                        if (listFromEnumerable != null)
                        {
                            this.listExtractedFromEnumerable = true;
                        }
                    }
                    if (listFromEnumerable == null)
                    {
                        if (obj3 != null)
                        {
                            listFromEnumerable = WrapObjectInBindingList(obj3);
                        }
                        else
                        {
                            System.Type listItemType = ListBindingHelper.GetListItemType(this.dataSource, this.dataMember);
                            listFromEnumerable = GetListFromType(listItemType);
                            if (listFromEnumerable == null)
                            {
                                listFromEnumerable = CreateBindingList(listItemType);
                            }
                        }
                    }
                }
                this.SetList(listFromEnumerable, true, true);
            }
        }

        public void ResumeBinding()
        {
            this.currencyManager.ResumeBinding();
        }

        private void SetList(IList list, bool metaDataChanged, bool applySortAndFilter)
        {
            if (list == null)
            {
                list = CreateBindingList(this.itemType);
            }
            this.UnwireInnerList();
            this.UnhookItemChangedEventsForOldCurrent();
            this._innerList = list;
            this.isBindingList = list is IBindingList;
            if (list is IRaiseItemChangedEvents)
            {
                this.listRaisesItemChangedEvents = (list as IRaiseItemChangedEvents).RaisesItemChangedEvents;
            }
            else
            {
                this.listRaisesItemChangedEvents = this.isBindingList;
            }
            if (metaDataChanged)
            {
                this.itemType = ListBindingHelper.GetListItemType(this.List);
                this.itemShape = ListBindingHelper.GetListItemProperties(this.List);
                this.itemConstructor = this.itemType.GetConstructor(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, new System.Type[0], null);
            }
            this.WireInnerList();
            this.HookItemChangedEventsForNewCurrent();
            this.ResetBindings(metaDataChanged);
            if (applySortAndFilter)
            {
                if (this.Sort != null)
                {
                    this.InnerListSort = this.Sort;
                }
                if (this.Filter != null)
                {
                    this.InnerListFilter = this.Filter;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeAllowNew()
        {
            return this.allowNewIsSet;
        }

        public void SuspendBinding()
        {
            this.currencyManager.SuspendBinding();
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            if (!this.isBindingList)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("OperationRequiresIBindingList"));
            }
            ((IBindingList) this.List).AddIndex(property);
        }

        void IBindingList.RemoveIndex(PropertyDescriptor prop)
        {
            if (!this.isBindingList)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("OperationRequiresIBindingList"));
            }
            ((IBindingList) this.List).RemoveIndex(prop);
        }

        void ICancelAddNew.CancelNew(int position)
        {
            if ((this.addNewPos >= 0) && (this.addNewPos == position))
            {
                this.RemoveAt(this.addNewPos);
                this.addNewPos = -1;
            }
            else
            {
                ICancelAddNew list = this.List as ICancelAddNew;
                if (list != null)
                {
                    list.CancelNew(position);
                }
            }
        }

        void ICancelAddNew.EndNew(int position)
        {
            if ((this.addNewPos >= 0) && (this.addNewPos == position))
            {
                this.addNewPos = -1;
            }
            else
            {
                ICancelAddNew list = this.List as ICancelAddNew;
                if (list != null)
                {
                    list.EndNew(position);
                }
            }
        }

        void ISupportInitialize.BeginInit()
        {
            this.initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            ISupportInitializeNotification dataSource = this.DataSource as ISupportInitializeNotification;
            if ((dataSource != null) && !dataSource.IsInitialized)
            {
                dataSource.Initialized += new EventHandler(this.DataSource_Initialized);
            }
            else
            {
                this.EndInitCore();
            }
        }

        private void ThrowIfBindingSourceRecursionDetected(object newDataSource)
        {
            for (BindingSource source = newDataSource as BindingSource; source != null; source = source.DataSource as BindingSource)
            {
                if (source == this)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceRecursionDetected"));
                }
            }
        }

        private void UnhookItemChangedEventsForOldCurrent()
        {
            if (!this.listRaisesItemChangedEvents)
            {
                this.UnwirePropertyChangedEvents(this.currentItemHookedForItemChange);
                this.currentItemHookedForItemChange = null;
            }
        }

        private void UnwireCurrencyManager(System.Windows.Forms.CurrencyManager cm)
        {
            if (cm != null)
            {
                cm.PositionChanged -= new EventHandler(this.CurrencyManager_PositionChanged);
                cm.CurrentChanged -= new EventHandler(this.CurrencyManager_CurrentChanged);
                cm.CurrentItemChanged -= new EventHandler(this.CurrencyManager_CurrentItemChanged);
                cm.BindingComplete -= new BindingCompleteEventHandler(this.CurrencyManager_BindingComplete);
                cm.DataError -= new BindingManagerDataErrorEventHandler(this.CurrencyManager_DataError);
            }
        }

        private void UnwireDataSource()
        {
            if (this.dataSource is ICurrencyManagerProvider)
            {
                System.Windows.Forms.CurrencyManager currencyManager = (this.dataSource as ICurrencyManagerProvider).CurrencyManager;
                currencyManager.CurrentItemChanged -= new EventHandler(this.ParentCurrencyManager_CurrentItemChanged);
                currencyManager.MetaDataChanged -= new EventHandler(this.ParentCurrencyManager_MetaDataChanged);
            }
        }

        private void UnwireInnerList()
        {
            if (this._innerList is IBindingList)
            {
                IBindingList list = this._innerList as IBindingList;
                list.ListChanged -= new ListChangedEventHandler(this.InnerList_ListChanged);
            }
        }

        private void UnwirePropertyChangedEvents(object item)
        {
            if ((item != null) && (this.itemShape != null))
            {
                for (int i = 0; i < this.itemShape.Count; i++)
                {
                    this.itemShape[i].RemoveValueChanged(item, this.listItemPropertyChangedHandler);
                }
            }
        }

        private void WireCurrencyManager(System.Windows.Forms.CurrencyManager cm)
        {
            if (cm != null)
            {
                cm.PositionChanged += new EventHandler(this.CurrencyManager_PositionChanged);
                cm.CurrentChanged += new EventHandler(this.CurrencyManager_CurrentChanged);
                cm.CurrentItemChanged += new EventHandler(this.CurrencyManager_CurrentItemChanged);
                cm.BindingComplete += new BindingCompleteEventHandler(this.CurrencyManager_BindingComplete);
                cm.DataError += new BindingManagerDataErrorEventHandler(this.CurrencyManager_DataError);
            }
        }

        private void WireDataSource()
        {
            if (this.dataSource is ICurrencyManagerProvider)
            {
                System.Windows.Forms.CurrencyManager currencyManager = (this.dataSource as ICurrencyManagerProvider).CurrencyManager;
                currencyManager.CurrentItemChanged += new EventHandler(this.ParentCurrencyManager_CurrentItemChanged);
                currencyManager.MetaDataChanged += new EventHandler(this.ParentCurrencyManager_MetaDataChanged);
            }
        }

        private void WireInnerList()
        {
            if (this._innerList is IBindingList)
            {
                IBindingList list = this._innerList as IBindingList;
                list.ListChanged += new ListChangedEventHandler(this.InnerList_ListChanged);
            }
        }

        private void WirePropertyChangedEvents(object item)
        {
            if ((item != null) && (this.itemShape != null))
            {
                for (int i = 0; i < this.itemShape.Count; i++)
                {
                    this.itemShape[i].AddValueChanged(item, this.listItemPropertyChangedHandler);
                }
            }
        }

        private static IList WrapObjectInBindingList(object obj)
        {
            IList list = CreateBindingList(obj.GetType());
            list.Add(obj);
            return list;
        }

        [Browsable(false)]
        public virtual bool AllowEdit
        {
            get
            {
                if (this.isBindingList)
                {
                    return ((IBindingList) this.List).AllowEdit;
                }
                return !this.List.IsReadOnly;
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourceAllowNewDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public virtual bool AllowNew
        {
            get
            {
                return this.AllowNewInternal(true);
            }
            set
            {
                if (!this.allowNewIsSet || (value != this.allowNewSetValue))
                {
                    if ((value && !this.isBindingList) && !this.IsListWriteable(false))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("NoAllowNewOnReadOnlyList"));
                    }
                    this.allowNewIsSet = true;
                    this.allowNewSetValue = value;
                    this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                }
            }
        }

        [Browsable(false)]
        public virtual bool AllowRemove
        {
            get
            {
                if (this.isBindingList)
                {
                    return ((IBindingList) this.List).AllowRemove;
                }
                return (!this.List.IsReadOnly && !this.List.IsFixedSize);
            }
        }

        [Browsable(false)]
        public virtual int Count
        {
            get
            {
                int count;
                try
                {
                    if (this.disposedOrFinalized)
                    {
                        return 0;
                    }
                    if (this.recursionDetectionFlag)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("BindingSourceRecursionDetected"));
                    }
                    this.recursionDetectionFlag = true;
                    count = this.List.Count;
                }
                finally
                {
                    this.recursionDetectionFlag = false;
                }
                return count;
            }
        }

        [Browsable(false)]
        public virtual System.Windows.Forms.CurrencyManager CurrencyManager
        {
            get
            {
                return this.GetRelatedCurrencyManager(null);
            }
        }

        [Browsable(false)]
        public object Current
        {
            get
            {
                if (this.currencyManager.Count <= 0)
                {
                    return null;
                }
                return this.currencyManager.Current;
            }
        }

        [Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("BindingSourceDataMemberDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue(""), RefreshProperties(RefreshProperties.Repaint)]
        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!this.dataMember.Equals(value))
                {
                    this.dataMember = value;
                    this.ResetList();
                    this.OnDataMemberChanged(EventArgs.Empty);
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("BindingSourceDataSourceDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue((string) null), AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (this.dataSource != value)
                {
                    this.ThrowIfBindingSourceRecursionDetected(value);
                    this.UnwireDataSource();
                    this.dataSource = value;
                    this.ClearInvalidDataMember();
                    this.ResetList();
                    this.WireDataSource();
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRDescription("BindingSourceFilterDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue((string) null)]
        public virtual string Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                this.filter = value;
                this.InnerListFilter = value;
            }
        }

        private string InnerListFilter
        {
            get
            {
                IBindingListView list = this.List as IBindingListView;
                if ((list != null) && list.SupportsFiltering)
                {
                    return list.Filter;
                }
                return string.Empty;
            }
            set
            {
                if ((!this.initializing && !base.DesignMode) && !string.Equals(value, this.InnerListFilter, StringComparison.Ordinal))
                {
                    IBindingListView list = this.List as IBindingListView;
                    if ((list != null) && list.SupportsFiltering)
                    {
                        list.Filter = value;
                    }
                }
            }
        }

        private string InnerListSort
        {
            get
            {
                ListSortDescriptionCollection sortsColln = null;
                IBindingListView view = this.List as IBindingListView;
                IBindingList list = this.List as IBindingList;
                if ((view != null) && view.SupportsAdvancedSorting)
                {
                    sortsColln = view.SortDescriptions;
                }
                else if (((list != null) && list.SupportsSorting) && list.IsSorted)
                {
                    sortsColln = new ListSortDescriptionCollection(new ListSortDescription[] { new ListSortDescription(list.SortProperty, list.SortDirection) });
                }
                return BuildSortString(sortsColln);
            }
            set
            {
                if ((!this.initializing && !base.DesignMode) && (string.Compare(value, this.InnerListSort, false, CultureInfo.InvariantCulture) != 0))
                {
                    ListSortDescriptionCollection sorts = this.ParseSortString(value);
                    IBindingListView view = this.List as IBindingListView;
                    IBindingList list = this.List as IBindingList;
                    if ((view != null) && view.SupportsAdvancedSorting)
                    {
                        if (sorts.Count == 0)
                        {
                            view.RemoveSort();
                        }
                        else
                        {
                            view.ApplySort(sorts);
                        }
                    }
                    else if ((list != null) && list.SupportsSorting)
                    {
                        if (sorts.Count == 0)
                        {
                            list.RemoveSort();
                        }
                        else if (sorts.Count == 1)
                        {
                            list.ApplySort(sorts[0].PropertyDescriptor, sorts[0].SortDirection);
                        }
                    }
                }
            }
        }

        [Browsable(false)]
        public bool IsBindingSuspended
        {
            get
            {
                return this.currencyManager.IsBindingSuspended;
            }
        }

        [Browsable(false)]
        public virtual bool IsFixedSize
        {
            get
            {
                return this.List.IsFixedSize;
            }
        }

        [Browsable(false)]
        public virtual bool IsReadOnly
        {
            get
            {
                return this.List.IsReadOnly;
            }
        }

        [Browsable(false)]
        public virtual bool IsSorted
        {
            get
            {
                return (this.isBindingList && ((IBindingList) this.List).IsSorted);
            }
        }

        [Browsable(false)]
        public virtual bool IsSynchronized
        {
            get
            {
                return this.List.IsSynchronized;
            }
        }

        [Browsable(false)]
        public virtual object this[int index]
        {
            get
            {
                return this.List[index];
            }
            set
            {
                this.List[index] = value;
                if (!this.isBindingList)
                {
                    this.OnSimpleListChanged(ListChangedType.ItemChanged, index);
                }
            }
        }

        [Browsable(false)]
        public IList List
        {
            get
            {
                this.EnsureInnerList();
                return this._innerList;
            }
        }

        [Browsable(false), DefaultValue(-1)]
        public int Position
        {
            get
            {
                return this.currencyManager.Position;
            }
            set
            {
                if (this.currencyManager.Position != value)
                {
                    this.currencyManager.Position = value;
                }
            }
        }

        [Browsable(false), DefaultValue(true)]
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

        [System.Windows.Forms.SRDescription("BindingSourceSortDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatData")]
        public string Sort
        {
            get
            {
                return this.sort;
            }
            set
            {
                this.sort = value;
                this.InnerListSort = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public virtual ListSortDescriptionCollection SortDescriptions
        {
            get
            {
                IBindingListView list = this.List as IBindingListView;
                if (list != null)
                {
                    return list.SortDescriptions;
                }
                return null;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual ListSortDirection SortDirection
        {
            get
            {
                if (this.isBindingList)
                {
                    return ((IBindingList) this.List).SortDirection;
                }
                return ListSortDirection.Ascending;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual PropertyDescriptor SortProperty
        {
            get
            {
                if (this.isBindingList)
                {
                    return ((IBindingList) this.List).SortProperty;
                }
                return null;
            }
        }

        [Browsable(false)]
        public virtual bool SupportsAdvancedSorting
        {
            get
            {
                IBindingListView list = this.List as IBindingListView;
                return ((list != null) && list.SupportsAdvancedSorting);
            }
        }

        [Browsable(false)]
        public virtual bool SupportsChangeNotification
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        public virtual bool SupportsFiltering
        {
            get
            {
                IBindingListView list = this.List as IBindingListView;
                return ((list != null) && list.SupportsFiltering);
            }
        }

        [Browsable(false)]
        public virtual bool SupportsSearching
        {
            get
            {
                return (this.isBindingList && ((IBindingList) this.List).SupportsSearching);
            }
        }

        [Browsable(false)]
        public virtual bool SupportsSorting
        {
            get
            {
                return (this.isBindingList && ((IBindingList) this.List).SupportsSorting);
            }
        }

        [Browsable(false)]
        public virtual object SyncRoot
        {
            get
            {
                return this.List.SyncRoot;
            }
        }

        bool ISupportInitializeNotification.IsInitialized
        {
            get
            {
                return !this.initializing;
            }
        }
    }
}

