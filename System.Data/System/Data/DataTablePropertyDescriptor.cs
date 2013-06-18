namespace System.Data
{
    using System;
    using System.ComponentModel;

    internal sealed class DataTablePropertyDescriptor : PropertyDescriptor
    {
        private DataTable table;

        internal DataTablePropertyDescriptor(DataTable dataTable) : base(dataTable.TableName, null)
        {
            this.table = dataTable;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override bool Equals(object other)
        {
            if (other is DataTablePropertyDescriptor)
            {
                DataTablePropertyDescriptor descriptor = (DataTablePropertyDescriptor) other;
                return (descriptor.Table == this.Table);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Table.GetHashCode();
        }

        public override object GetValue(object component)
        {
            DataViewManagerListItemTypeDescriptor descriptor = (DataViewManagerListItemTypeDescriptor) component;
            return descriptor.GetDataView(this.table);
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(DataRowView);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(IBindingList);
            }
        }

        public DataTable Table
        {
            get
            {
                return this.table;
            }
        }
    }
}

