namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public class DataRowView : ICustomTypeDescriptor, IEditableObject, IDataErrorInfo, INotifyPropertyChanged
    {
        private readonly DataRow _row;
        private readonly System.Data.DataView dataView;
        private bool delayBeginEdit;
        private static PropertyDescriptorCollection zeroPropertyDescriptorCollection = new PropertyDescriptorCollection(null);

        public event PropertyChangedEventHandler PropertyChanged;

        internal DataRowView(System.Data.DataView dataView, DataRow row)
        {
            this.dataView = dataView;
            this._row = row;
        }

        public void BeginEdit()
        {
            this.delayBeginEdit = true;
        }

        public void CancelEdit()
        {
            DataRow row = this.Row;
            if (this.IsNew)
            {
                this.dataView.FinishAddNew(false);
            }
            else
            {
                row.CancelEdit();
            }
            this.delayBeginEdit = false;
        }

        public System.Data.DataView CreateChildView(DataRelation relation)
        {
            if ((relation == null) || (relation.ParentKey.Table != this.DataView.Table))
            {
                throw ExceptionBuilder.CreateChildView();
            }
            int record = this.GetRecord();
            object[] keyValues = relation.ParentKey.GetKeyValues(record);
            RelatedView view = new RelatedView(relation.ChildColumnsReference, keyValues);
            view.SetIndex("", DataViewRowState.CurrentRows, null);
            view.SetDataViewManager(this.DataView.DataViewManager);
            return view;
        }

        public System.Data.DataView CreateChildView(string relationName)
        {
            return this.CreateChildView(this.DataView.Table.ChildRelations[relationName]);
        }

        public void Delete()
        {
            this.dataView.Delete(this.Row);
        }

        public void EndEdit()
        {
            if (this.IsNew)
            {
                this.dataView.FinishAddNew(true);
            }
            else
            {
                this.Row.EndEdit();
            }
            this.delayBeginEdit = false;
        }

        public override bool Equals(object other)
        {
            return object.ReferenceEquals(this, other);
        }

        internal object GetColumnValue(DataColumn column)
        {
            return this.Row[column, this.RowVersionDefault];
        }

        public override int GetHashCode()
        {
            return this.Row.GetHashCode();
        }

        internal int GetRecord()
        {
            return this.Row.GetRecordFromVersion(this.RowVersionDefault);
        }

        internal void RaisePropertyChangedEvent(string propName)
        {
            if (this.onPropertyChanged != null)
            {
                this.onPropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        internal void SetColumnValue(DataColumn column, object value)
        {
            if (this.delayBeginEdit)
            {
                this.delayBeginEdit = false;
                this.Row.BeginEdit();
            }
            if (DataRowVersion.Original == this.RowVersionDefault)
            {
                throw ExceptionBuilder.SetFailed(column.ColumnName);
            }
            this.Row[column] = value;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(null);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(null);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor) this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            if (this.dataView.Table == null)
            {
                return zeroPropertyDescriptorCollection;
            }
            return this.dataView.Table.GetPropertyDescriptorCollection(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public System.Data.DataView DataView
        {
            get
            {
                return this.dataView;
            }
        }

        public bool IsEdit
        {
            get
            {
                if (!this.Row.HasVersion(DataRowVersion.Proposed))
                {
                    return this.delayBeginEdit;
                }
                return true;
            }
        }

        public bool IsNew
        {
            get
            {
                return (this._row == this.dataView.addNewRow);
            }
        }

        public object this[int ndx]
        {
            get
            {
                return this.Row[ndx, this.RowVersionDefault];
            }
            set
            {
                if (!this.dataView.AllowEdit && !this.IsNew)
                {
                    throw ExceptionBuilder.CanNotEdit();
                }
                this.SetColumnValue(this.dataView.Table.Columns[ndx], value);
            }
        }

        public object this[string property]
        {
            get
            {
                DataColumn column = this.dataView.Table.Columns[property];
                if (column != null)
                {
                    return this.Row[column, this.RowVersionDefault];
                }
                if ((this.dataView.Table.DataSet == null) || !this.dataView.Table.DataSet.Relations.Contains(property))
                {
                    throw ExceptionBuilder.PropertyNotFound(property, this.dataView.Table.TableName);
                }
                return this.CreateChildView(property);
            }
            set
            {
                DataColumn column = this.dataView.Table.Columns[property];
                if (column == null)
                {
                    throw ExceptionBuilder.SetFailed(property);
                }
                if (!this.dataView.AllowEdit && !this.IsNew)
                {
                    throw ExceptionBuilder.CanNotEdit();
                }
                this.SetColumnValue(column, value);
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._row.ObjectID;
            }
        }

        public DataRow Row
        {
            get
            {
                return this._row;
            }
        }

        public DataRowVersion RowVersion
        {
            get
            {
                return (this.RowVersionDefault & ~DataRowVersion.Proposed);
            }
        }

        private DataRowVersion RowVersionDefault
        {
            get
            {
                return this.Row.GetDefaultRowVersion(this.dataView.RowStateFilter);
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                return this.Row.RowError;
            }
        }

        string IDataErrorInfo.this[string colName]
        {
            get
            {
                return this.Row.GetColumnError(colName);
            }
        }
    }
}

