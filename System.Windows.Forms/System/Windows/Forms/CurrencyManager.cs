namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    public class CurrencyManager : BindingManagerBase
    {
        private bool bound;
        private object dataSource;
        protected System.Type finalType;
        private bool inChangeRecordState;
        private int lastGoodKnownRow = -1;
        private IList list;
        protected int listposition = -1;
        private bool pullingData;
        private ItemChangedEventArgs resetEvent = new ItemChangedEventArgs(-1);
        private bool shouldBind = true;
        private bool suspendPushDataInCurrentChanged;

        [System.Windows.Forms.SRCategory("CatData")]
        public event ItemChangedEventHandler ItemChanged;

        public event ListChangedEventHandler ListChanged;

        [System.Windows.Forms.SRCategory("CatData")]
        public event EventHandler MetaDataChanged;

        internal CurrencyManager(object dataSource)
        {
            this.SetDataSource(dataSource);
        }

        public override void AddNew()
        {
            IBindingList list = this.list as IBindingList;
            if (list == null)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("CurrencyManagerCantAddNew"));
            }
            list.AddNew();
            this.ChangeRecordState(this.list.Count - 1, this.Position != (this.list.Count - 1), this.Position != (this.list.Count - 1), true, true);
        }

        public override void CancelCurrentEdit()
        {
            if (this.Count > 0)
            {
                object obj2 = ((this.Position >= 0) && (this.Position < this.list.Count)) ? this.list[this.Position] : null;
                IEditableObject obj3 = obj2 as IEditableObject;
                if (obj3 != null)
                {
                    obj3.CancelEdit();
                }
                ICancelAddNew list = this.list as ICancelAddNew;
                if (list != null)
                {
                    list.CancelNew(this.Position);
                }
                this.OnItemChanged(new ItemChangedEventArgs(this.Position));
                if (this.Position != -1)
                {
                    this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, this.Position));
                }
            }
        }

        private void ChangeRecordState(int newPosition, bool validating, bool endCurrentEdit, bool firePositionChange, bool pullData)
        {
            if ((newPosition == -1) && (this.list.Count == 0))
            {
                if (this.listposition != -1)
                {
                    this.listposition = -1;
                    this.OnPositionChanged(EventArgs.Empty);
                }
            }
            else
            {
                if (((newPosition < 0) || (newPosition >= this.Count)) && this.IsBinding)
                {
                    throw new IndexOutOfRangeException(System.Windows.Forms.SR.GetString("ListManagerBadPosition"));
                }
                int listposition = this.listposition;
                if (endCurrentEdit)
                {
                    this.inChangeRecordState = true;
                    try
                    {
                        this.EndCurrentEdit();
                    }
                    finally
                    {
                        this.inChangeRecordState = false;
                    }
                }
                if (validating && pullData)
                {
                    this.CurrencyManager_PullData();
                }
                this.listposition = Math.Min(newPosition, this.Count - 1);
                if (validating)
                {
                    this.OnCurrentChanged(EventArgs.Empty);
                }
                if ((listposition != this.listposition) && firePositionChange)
                {
                    this.OnPositionChanged(EventArgs.Empty);
                }
            }
        }

        protected void CheckEmpty()
        {
            if (((this.dataSource == null) || (this.list == null)) || (this.list.Count == 0))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListManagerEmptyList"));
            }
        }

        private bool CurrencyManager_PullData()
        {
            bool success = true;
            this.pullingData = true;
            try
            {
                base.PullData(out success);
            }
            finally
            {
                this.pullingData = false;
            }
            return success;
        }

        private bool CurrencyManager_PushData()
        {
            if (this.pullingData)
            {
                return false;
            }
            int listposition = this.listposition;
            if (this.lastGoodKnownRow == -1)
            {
                try
                {
                    base.PushData();
                }
                catch (Exception exception)
                {
                    base.OnDataError(exception);
                    this.FindGoodRow();
                }
                this.lastGoodKnownRow = this.listposition;
            }
            else
            {
                try
                {
                    base.PushData();
                }
                catch (Exception exception2)
                {
                    base.OnDataError(exception2);
                    this.listposition = this.lastGoodKnownRow;
                    base.PushData();
                }
                this.lastGoodKnownRow = this.listposition;
            }
            return (listposition != this.listposition);
        }

        public override void EndCurrentEdit()
        {
            if ((this.Count > 0) && this.CurrencyManager_PullData())
            {
                object obj2 = ((this.Position >= 0) && (this.Position < this.list.Count)) ? this.list[this.Position] : null;
                IEditableObject obj3 = obj2 as IEditableObject;
                if (obj3 != null)
                {
                    obj3.EndEdit();
                }
                ICancelAddNew list = this.list as ICancelAddNew;
                if (list != null)
                {
                    list.EndNew(this.Position);
                }
            }
        }

        internal int Find(PropertyDescriptor property, object key, bool keepIndex)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (((property != null) && (this.list is IBindingList)) && ((IBindingList) this.list).SupportsSearching)
            {
                return ((IBindingList) this.list).Find(property, key);
            }
            for (int i = 0; i < this.list.Count; i++)
            {
                object obj2 = property.GetValue(this.list[i]);
                if (key.Equals(obj2))
                {
                    return i;
                }
            }
            return -1;
        }

        private void FindGoodRow()
        {
            int count = this.list.Count;
            for (int i = 0; i < count; i++)
            {
                this.listposition = i;
                try
                {
                    base.PushData();
                }
                catch (Exception exception)
                {
                    base.OnDataError(exception);
                    continue;
                }
                this.listposition = i;
                return;
            }
            this.SuspendBinding();
            throw new InvalidOperationException(System.Windows.Forms.SR.GetString("DataBindingPushDataException"));
        }

        public override PropertyDescriptorCollection GetItemProperties()
        {
            return this.GetItemProperties(null);
        }

        internal override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return ListBindingHelper.GetListItemProperties(this.list, listAccessors);
        }

        internal override string GetListName()
        {
            if (this.list is ITypedList)
            {
                return ((ITypedList) this.list).GetListName(null);
            }
            return this.finalType.Name;
        }

        protected internal override string GetListName(ArrayList listAccessors)
        {
            if (this.list is ITypedList)
            {
                PropertyDescriptor[] array = new PropertyDescriptor[listAccessors.Count];
                listAccessors.CopyTo(array, 0);
                return ((ITypedList) this.list).GetListName(array);
            }
            return "";
        }

        internal ListSortDirection GetSortDirection()
        {
            if ((this.list is IBindingList) && ((IBindingList) this.list).SupportsSorting)
            {
                return ((IBindingList) this.list).SortDirection;
            }
            return ListSortDirection.Ascending;
        }

        internal PropertyDescriptor GetSortProperty()
        {
            if ((this.list is IBindingList) && ((IBindingList) this.list).SupportsSorting)
            {
                return ((IBindingList) this.list).SortProperty;
            }
            return null;
        }

        private void List_ListChanged(object sender, ListChangedEventArgs e)
        {
            ListChangedEventArgs args;
            if ((e.ListChangedType == ListChangedType.ItemMoved) && (e.OldIndex < 0))
            {
                args = new ListChangedEventArgs(ListChangedType.ItemAdded, e.NewIndex, e.OldIndex);
            }
            else if ((e.ListChangedType == ListChangedType.ItemMoved) && (e.NewIndex < 0))
            {
                args = new ListChangedEventArgs(ListChangedType.ItemDeleted, e.OldIndex, e.NewIndex);
            }
            else
            {
                args = e;
            }
            int listposition = this.listposition;
            this.UpdateLastGoodKnownRow(args);
            this.UpdateIsBinding();
            if (this.list.Count == 0)
            {
                this.listposition = -1;
                if (listposition != -1)
                {
                    this.OnPositionChanged(EventArgs.Empty);
                    this.OnCurrentChanged(EventArgs.Empty);
                }
                if ((args.ListChangedType == ListChangedType.Reset) && (e.NewIndex == -1))
                {
                    this.OnItemChanged(this.resetEvent);
                }
                if (args.ListChangedType == ListChangedType.ItemDeleted)
                {
                    this.OnItemChanged(this.resetEvent);
                }
                if (((e.ListChangedType == ListChangedType.PropertyDescriptorAdded) || (e.ListChangedType == ListChangedType.PropertyDescriptorDeleted)) || (e.ListChangedType == ListChangedType.PropertyDescriptorChanged))
                {
                    this.OnMetaDataChanged(EventArgs.Empty);
                }
                this.OnListChanged(args);
            }
            else
            {
                this.suspendPushDataInCurrentChanged = true;
                try
                {
                    switch (args.ListChangedType)
                    {
                        case ListChangedType.Reset:
                            if ((this.listposition != -1) || (this.list.Count <= 0))
                            {
                                break;
                            }
                            this.ChangeRecordState(0, true, false, true, false);
                            goto Label_0174;

                        case ListChangedType.ItemAdded:
                            if ((args.NewIndex > this.listposition) || (this.listposition >= (this.list.Count - 1)))
                            {
                                goto Label_0212;
                            }
                            this.ChangeRecordState(this.listposition + 1, true, true, this.listposition != (this.list.Count - 2), false);
                            this.UpdateIsBinding();
                            this.OnItemChanged(this.resetEvent);
                            if (this.listposition == (this.list.Count - 1))
                            {
                                this.OnPositionChanged(EventArgs.Empty);
                            }
                            goto Label_040B;

                        case ListChangedType.ItemDeleted:
                            if (args.NewIndex != this.listposition)
                            {
                                goto Label_02B0;
                            }
                            this.ChangeRecordState(Math.Min(this.listposition, this.Count - 1), true, false, true, false);
                            this.OnItemChanged(this.resetEvent);
                            goto Label_040B;

                        case ListChangedType.ItemMoved:
                            if (args.OldIndex != this.listposition)
                            {
                                goto Label_035F;
                            }
                            this.ChangeRecordState(args.NewIndex, true, (this.Position > -1) && (this.Position < this.list.Count), true, false);
                            goto Label_039B;

                        case ListChangedType.ItemChanged:
                            if (args.NewIndex == this.listposition)
                            {
                                this.OnCurrentItemChanged(EventArgs.Empty);
                            }
                            this.OnItemChanged(new ItemChangedEventArgs(args.NewIndex));
                            goto Label_040B;

                        case ListChangedType.PropertyDescriptorAdded:
                        case ListChangedType.PropertyDescriptorDeleted:
                        case ListChangedType.PropertyDescriptorChanged:
                            this.lastGoodKnownRow = -1;
                            if ((this.listposition != -1) || (this.list.Count <= 0))
                            {
                                goto Label_03D4;
                            }
                            this.ChangeRecordState(0, true, false, true, false);
                            goto Label_0400;

                        default:
                            goto Label_040B;
                    }
                    this.ChangeRecordState(Math.Min(this.listposition, this.list.Count - 1), true, false, true, false);
                Label_0174:
                    this.UpdateIsBinding(false);
                    this.OnItemChanged(this.resetEvent);
                    goto Label_040B;
                Label_0212:
                    if (((args.NewIndex == this.listposition) && (this.listposition == (this.list.Count - 1))) && (this.listposition != -1))
                    {
                        this.OnCurrentItemChanged(EventArgs.Empty);
                    }
                    if (this.listposition == -1)
                    {
                        this.ChangeRecordState(0, false, false, true, false);
                    }
                    this.UpdateIsBinding();
                    this.OnItemChanged(this.resetEvent);
                    goto Label_040B;
                Label_02B0:
                    if (args.NewIndex < this.listposition)
                    {
                        this.ChangeRecordState(this.listposition - 1, true, false, true, false);
                        this.OnItemChanged(this.resetEvent);
                    }
                    else
                    {
                        this.OnItemChanged(this.resetEvent);
                    }
                    goto Label_040B;
                Label_035F:
                    if (args.NewIndex == this.listposition)
                    {
                        this.ChangeRecordState(args.OldIndex, true, (this.Position > -1) && (this.Position < this.list.Count), true, false);
                    }
                Label_039B:
                    this.OnItemChanged(this.resetEvent);
                    goto Label_040B;
                Label_03D4:
                    if (this.listposition > (this.list.Count - 1))
                    {
                        this.ChangeRecordState(this.list.Count - 1, true, false, true, false);
                    }
                Label_0400:
                    this.OnMetaDataChanged(EventArgs.Empty);
                Label_040B:
                    this.OnListChanged(args);
                }
                finally
                {
                    this.suspendPushDataInCurrentChanged = false;
                }
            }
        }

        protected internal override void OnCurrentChanged(EventArgs e)
        {
            if (!this.inChangeRecordState)
            {
                int lastGoodKnownRow = this.lastGoodKnownRow;
                bool flag = false;
                if (!this.suspendPushDataInCurrentChanged)
                {
                    flag = this.CurrencyManager_PushData();
                }
                if (this.Count > 0)
                {
                    object obj2 = this.list[this.Position];
                    if (obj2 is IEditableObject)
                    {
                        ((IEditableObject) obj2).BeginEdit();
                    }
                }
                try
                {
                    if (!flag || (flag && (lastGoodKnownRow != -1)))
                    {
                        if (base.onCurrentChangedHandler != null)
                        {
                            base.onCurrentChangedHandler(this, e);
                        }
                        if (base.onCurrentItemChangedHandler != null)
                        {
                            base.onCurrentItemChangedHandler(this, e);
                        }
                    }
                }
                catch (Exception exception)
                {
                    base.OnDataError(exception);
                }
            }
        }

        protected internal override void OnCurrentItemChanged(EventArgs e)
        {
            if (base.onCurrentItemChangedHandler != null)
            {
                base.onCurrentItemChangedHandler(this, e);
            }
        }

        protected virtual void OnItemChanged(ItemChangedEventArgs e)
        {
            bool flag = false;
            if (((e.Index == this.listposition) || ((e.Index == -1) && (this.Position < this.Count))) && !this.inChangeRecordState)
            {
                flag = this.CurrencyManager_PushData();
            }
            try
            {
                if (this.onItemChanged != null)
                {
                    this.onItemChanged(this, e);
                }
            }
            catch (Exception exception)
            {
                base.OnDataError(exception);
            }
            if (flag)
            {
                this.OnPositionChanged(EventArgs.Empty);
            }
        }

        private void OnListChanged(ListChangedEventArgs e)
        {
            if (this.onListChanged != null)
            {
                this.onListChanged(this, e);
            }
        }

        protected internal void OnMetaDataChanged(EventArgs e)
        {
            if (this.onMetaDataChangedHandler != null)
            {
                this.onMetaDataChangedHandler(this, e);
            }
        }

        protected virtual void OnPositionChanged(EventArgs e)
        {
            try
            {
                if (base.onPositionChangedHandler != null)
                {
                    base.onPositionChangedHandler(this, e);
                }
            }
            catch (Exception exception)
            {
                base.OnDataError(exception);
            }
        }

        public void Refresh()
        {
            if (this.list.Count > 0)
            {
                if (this.listposition >= this.list.Count)
                {
                    this.lastGoodKnownRow = -1;
                    this.listposition = 0;
                }
            }
            else
            {
                this.listposition = -1;
            }
            this.List_ListChanged(this.list, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        internal void Release()
        {
            this.UnwireEvents(this.list);
        }

        public override void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        public override void ResumeBinding()
        {
            this.lastGoodKnownRow = -1;
            try
            {
                if (!this.shouldBind)
                {
                    this.shouldBind = true;
                    this.listposition = ((this.list != null) && (this.list.Count != 0)) ? 0 : -1;
                    this.UpdateIsBinding();
                }
            }
            catch
            {
                this.shouldBind = false;
                this.UpdateIsBinding();
                throw;
            }
        }

        internal override void SetDataSource(object dataSource)
        {
            if (this.dataSource != dataSource)
            {
                this.Release();
                this.dataSource = dataSource;
                this.list = null;
                this.finalType = null;
                object list = dataSource;
                if (list is Array)
                {
                    this.finalType = list.GetType();
                    list = (Array) list;
                }
                if (list is IListSource)
                {
                    list = ((IListSource) list).GetList();
                }
                if (list is IList)
                {
                    if (this.finalType == null)
                    {
                        this.finalType = list.GetType();
                    }
                    this.list = (IList) list;
                    this.WireEvents(this.list);
                    if (this.list.Count > 0)
                    {
                        this.listposition = 0;
                    }
                    else
                    {
                        this.listposition = -1;
                    }
                    this.OnItemChanged(this.resetEvent);
                    this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1, -1));
                    this.UpdateIsBinding();
                }
                else
                {
                    if (list == null)
                    {
                        throw new ArgumentNullException("dataSource");
                    }
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ListManagerSetDataSource", new object[] { list.GetType().FullName }), "dataSource");
                }
            }
        }

        internal void SetSort(PropertyDescriptor property, ListSortDirection sortDirection)
        {
            if ((this.list is IBindingList) && ((IBindingList) this.list).SupportsSorting)
            {
                ((IBindingList) this.list).ApplySort(property, sortDirection);
            }
        }

        public override void SuspendBinding()
        {
            this.lastGoodKnownRow = -1;
            if (this.shouldBind)
            {
                this.shouldBind = false;
                this.UpdateIsBinding();
            }
        }

        internal void UnwireEvents(IList list)
        {
            if ((list is IBindingList) && ((IBindingList) list).SupportsChangeNotification)
            {
                ((IBindingList) list).ListChanged -= new ListChangedEventHandler(this.List_ListChanged);
            }
        }

        protected override void UpdateIsBinding()
        {
            this.UpdateIsBinding(true);
        }

        private void UpdateIsBinding(bool raiseItemChangedEvent)
        {
            bool flag = (((this.list != null) && (this.list.Count > 0)) && this.shouldBind) && (this.listposition != -1);
            if ((this.list != null) && (this.bound != flag))
            {
                this.bound = flag;
                int newPosition = flag ? 0 : -1;
                this.ChangeRecordState(newPosition, this.bound, this.Position != newPosition, true, false);
                int count = base.Bindings.Count;
                for (int i = 0; i < count; i++)
                {
                    base.Bindings[i].UpdateIsBinding();
                }
                if (raiseItemChangedEvent)
                {
                    this.OnItemChanged(this.resetEvent);
                }
            }
        }

        private void UpdateLastGoodKnownRow(ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    this.lastGoodKnownRow = -1;
                    return;

                case ListChangedType.ItemAdded:
                    if ((e.NewIndex > this.lastGoodKnownRow) || (this.lastGoodKnownRow >= (this.List.Count - 1)))
                    {
                        break;
                    }
                    this.lastGoodKnownRow++;
                    return;

                case ListChangedType.ItemDeleted:
                    if (e.NewIndex != this.lastGoodKnownRow)
                    {
                        break;
                    }
                    this.lastGoodKnownRow = -1;
                    return;

                case ListChangedType.ItemMoved:
                    if (e.OldIndex != this.lastGoodKnownRow)
                    {
                        break;
                    }
                    this.lastGoodKnownRow = e.NewIndex;
                    return;

                case ListChangedType.ItemChanged:
                    if (e.NewIndex == this.lastGoodKnownRow)
                    {
                        this.lastGoodKnownRow = -1;
                    }
                    break;

                default:
                    return;
            }
        }

        internal void WireEvents(IList list)
        {
            if ((list is IBindingList) && ((IBindingList) list).SupportsChangeNotification)
            {
                ((IBindingList) list).ListChanged += new ListChangedEventHandler(this.List_ListChanged);
            }
        }

        internal bool AllowAdd
        {
            get
            {
                if (this.list is IBindingList)
                {
                    return ((IBindingList) this.list).AllowNew;
                }
                if (this.list == null)
                {
                    return false;
                }
                return (!this.list.IsReadOnly && !this.list.IsFixedSize);
            }
        }

        internal bool AllowEdit
        {
            get
            {
                if (this.list is IBindingList)
                {
                    return ((IBindingList) this.list).AllowEdit;
                }
                if (this.list == null)
                {
                    return false;
                }
                return !this.list.IsReadOnly;
            }
        }

        internal bool AllowRemove
        {
            get
            {
                if (this.list is IBindingList)
                {
                    return ((IBindingList) this.list).AllowRemove;
                }
                if (this.list == null)
                {
                    return false;
                }
                return (!this.list.IsReadOnly && !this.list.IsFixedSize);
            }
        }

        internal override System.Type BindType
        {
            get
            {
                return ListBindingHelper.GetListItemType(this.List);
            }
        }

        public override int Count
        {
            get
            {
                if (this.list == null)
                {
                    return 0;
                }
                return this.list.Count;
            }
        }

        public override object Current
        {
            get
            {
                return this[this.Position];
            }
        }

        internal override object DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        internal override bool IsBinding
        {
            get
            {
                return this.bound;
            }
        }

        internal object this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.list.Count))
                {
                    throw new IndexOutOfRangeException(System.Windows.Forms.SR.GetString("ListManagerNoValue", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
                }
                return this.list[index];
            }
            set
            {
                if ((index < 0) || (index >= this.list.Count))
                {
                    throw new IndexOutOfRangeException(System.Windows.Forms.SR.GetString("ListManagerNoValue", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
                }
                this.list[index] = value;
            }
        }

        public IList List
        {
            get
            {
                return this.list;
            }
        }

        public override int Position
        {
            get
            {
                return this.listposition;
            }
            set
            {
                if (this.listposition != -1)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    int count = this.list.Count;
                    if (value >= count)
                    {
                        value = count - 1;
                    }
                    this.ChangeRecordState(value, this.listposition != value, true, true, false);
                }
            }
        }

        internal bool ShouldBind
        {
            get
            {
                return this.shouldBind;
            }
        }
    }
}

