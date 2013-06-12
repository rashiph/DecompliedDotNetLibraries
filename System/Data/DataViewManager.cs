namespace System.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    [Designer("Microsoft.VSDesigner.Data.VS.DataViewManagerDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class DataViewManager : MarshalByValueComponent, IBindingList, IList, ICollection, IEnumerable, ITypedList
    {
        private System.Data.DataSet dataSet;
        private DataViewSettingCollection dataViewSettingsCollection;
        private DataViewManagerListItemTypeDescriptor item;
        private bool locked;
        private static NotSupportedException NotSupported = new NotSupportedException();
        internal int nViews;

        public event ListChangedEventHandler ListChanged;

        public DataViewManager() : this(null, false)
        {
        }

        public DataViewManager(System.Data.DataSet dataSet) : this(dataSet, false)
        {
        }

        internal DataViewManager(System.Data.DataSet dataSet, bool locked)
        {
            GC.SuppressFinalize(this);
            this.dataSet = dataSet;
            if (this.dataSet != null)
            {
                this.dataSet.Tables.CollectionChanged += new CollectionChangeEventHandler(this.TableCollectionChanged);
                this.dataSet.Relations.CollectionChanged += new CollectionChangeEventHandler(this.RelationCollectionChanged);
            }
            this.locked = locked;
            this.item = new DataViewManagerListItemTypeDescriptor(this);
            this.dataViewSettingsCollection = new DataViewSettingCollection(this);
        }

        public DataView CreateDataView(DataTable table)
        {
            if (this.dataSet == null)
            {
                throw ExceptionBuilder.CanNotUseDataViewManager();
            }
            DataView view = new DataView(table);
            view.SetDataViewManager(this);
            return view;
        }

        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            try
            {
                if (this.onListChanged != null)
                {
                    this.onListChanged(this, e);
                }
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
            }
        }

        protected virtual void RelationCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            DataRelationPropertyDescriptor propDesc = null;
            this.OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((DataRelation) e.Element)) : null)));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            array.SetValue(new DataViewManagerListItemTypeDescriptor(this), index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            DataViewManagerListItemTypeDescriptor[] array = new DataViewManagerListItemTypeDescriptor[1];
            ((ICollection) this).CopyTo(array, 0);
            return array.GetEnumerator();
        }

        int IList.Add(object value)
        {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.Clear()
        {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        bool IList.Contains(object value)
        {
            return (value == this.item);
        }

        int IList.IndexOf(object value)
        {
            if (value != this.item)
            {
                return -1;
            }
            return 1;
        }

        void IList.Insert(int index, object value)
        {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.Remove(object value)
        {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IList.RemoveAt(int index)
        {
            throw ExceptionBuilder.CannotModifyCollection();
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
        }

        object IBindingList.AddNew()
        {
            throw NotSupported;
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw NotSupported;
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw NotSupported;
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
        }

        void IBindingList.RemoveSort()
        {
            throw NotSupported;
        }

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            System.Data.DataSet dataSet = this.DataSet;
            if (dataSet == null)
            {
                throw ExceptionBuilder.CanNotUseDataViewManager();
            }
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                return ((ICustomTypeDescriptor) new DataViewManagerListItemTypeDescriptor(this)).GetProperties();
            }
            DataTable table = dataSet.FindTable(null, listAccessors, 0);
            if (table != null)
            {
                return table.GetPropertyDescriptorCollection(null);
            }
            return new PropertyDescriptorCollection(null);
        }

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            System.Data.DataSet dataSet = this.DataSet;
            if (dataSet == null)
            {
                throw ExceptionBuilder.CanNotUseDataViewManager();
            }
            if ((listAccessors == null) || (listAccessors.Length == 0))
            {
                return dataSet.DataSetName;
            }
            DataTable table = dataSet.FindTable(null, listAccessors, 0);
            if (table != null)
            {
                return table.TableName;
            }
            return string.Empty;
        }

        protected virtual void TableCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            PropertyDescriptor propDesc = null;
            this.OnListChanged((e.Action == CollectionChangeAction.Add) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataTablePropertyDescriptor((DataTable) e.Element)) : ((e.Action == CollectionChangeAction.Refresh) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, propDesc) : ((e.Action == CollectionChangeAction.Remove) ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataTablePropertyDescriptor((DataTable) e.Element)) : null)));
        }

        [System.Data.ResDescription("DataViewManagerDataSetDescr"), DefaultValue((string) null)]
        public System.Data.DataSet DataSet
        {
            get
            {
                return this.dataSet;
            }
            set
            {
                if (value == null)
                {
                    throw ExceptionBuilder.SetFailed("DataSet to null");
                }
                if (this.locked)
                {
                    throw ExceptionBuilder.SetDataSetFailed();
                }
                if (this.dataSet != null)
                {
                    if (this.nViews > 0)
                    {
                        throw ExceptionBuilder.CanNotSetDataSet();
                    }
                    this.dataSet.Tables.CollectionChanged -= new CollectionChangeEventHandler(this.TableCollectionChanged);
                    this.dataSet.Relations.CollectionChanged -= new CollectionChangeEventHandler(this.RelationCollectionChanged);
                }
                this.dataSet = value;
                this.dataSet.Tables.CollectionChanged += new CollectionChangeEventHandler(this.TableCollectionChanged);
                this.dataSet.Relations.CollectionChanged += new CollectionChangeEventHandler(this.RelationCollectionChanged);
                this.dataViewSettingsCollection = new DataViewSettingCollection(this);
                this.item.Reset();
            }
        }

        public string DataViewSettingCollectionString
        {
            get
            {
                if (this.dataSet == null)
                {
                    return "";
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("<DataViewSettingCollectionString>");
                foreach (DataTable table in this.dataSet.Tables)
                {
                    DataViewSetting setting = this.dataViewSettingsCollection[table];
                    builder.AppendFormat(CultureInfo.InvariantCulture, "<{0} Sort=\"{1}\" RowFilter=\"{2}\" RowStateFilter=\"{3}\"/>", new object[] { table.EncodedTableName, setting.Sort, setting.RowFilter, setting.RowStateFilter });
                }
                builder.Append("</DataViewSettingCollectionString>");
                return builder.ToString();
            }
            set
            {
                if ((value != null) && (value.Length != 0))
                {
                    XmlTextReader reader = new XmlTextReader(new StringReader(value)) {
                        WhitespaceHandling = WhitespaceHandling.None
                    };
                    reader.Read();
                    if (reader.Name != "DataViewSettingCollectionString")
                    {
                        throw ExceptionBuilder.SetFailed("DataViewSettingCollectionString");
                    }
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string str = XmlConvert.DecodeName(reader.LocalName);
                            if (reader.MoveToAttribute("Sort"))
                            {
                                this.dataViewSettingsCollection[str].Sort = reader.Value;
                            }
                            if (reader.MoveToAttribute("RowFilter"))
                            {
                                this.dataViewSettingsCollection[str].RowFilter = reader.Value;
                            }
                            if (reader.MoveToAttribute("RowStateFilter"))
                            {
                                this.dataViewSettingsCollection[str].RowStateFilter = (DataViewRowState) Enum.Parse(typeof(DataViewRowState), reader.Value);
                            }
                        }
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Data.ResDescription("DataViewManagerTableSettingsDescr")]
        public DataViewSettingCollection DataViewSettings
        {
            get
            {
                return this.dataViewSettingsCollection;
            }
        }

        int ICollection.Count
        {
            get
            {
                return 1;
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
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.item;
            }
            set
            {
                throw ExceptionBuilder.CannotModifyCollection();
            }
        }

        bool IBindingList.AllowEdit
        {
            get
            {
                return false;
            }
        }

        bool IBindingList.AllowNew
        {
            get
            {
                return false;
            }
        }

        bool IBindingList.AllowRemove
        {
            get
            {
                return false;
            }
        }

        bool IBindingList.IsSorted
        {
            get
            {
                throw NotSupported;
            }
        }

        ListSortDirection IBindingList.SortDirection
        {
            get
            {
                throw NotSupported;
            }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get
            {
                throw NotSupported;
            }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get
            {
                return true;
            }
        }

        bool IBindingList.SupportsSearching
        {
            get
            {
                return false;
            }
        }

        bool IBindingList.SupportsSorting
        {
            get
            {
                return false;
            }
        }
    }
}

