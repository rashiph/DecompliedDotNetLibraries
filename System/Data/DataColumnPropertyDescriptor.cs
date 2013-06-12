namespace System.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;

    internal sealed class DataColumnPropertyDescriptor : PropertyDescriptor
    {
        private DataColumn column;

        internal DataColumnPropertyDescriptor(DataColumn dataColumn) : base(dataColumn.ColumnName, null)
        {
            this.column = dataColumn;
        }

        public override bool CanResetValue(object component)
        {
            DataRowView view = (DataRowView) component;
            if (!this.column.IsSqlType)
            {
                return (view.GetColumnValue(this.column) != DBNull.Value);
            }
            return !DataStorage.IsObjectNull(view.GetColumnValue(this.column));
        }

        public override bool Equals(object other)
        {
            if (other is DataColumnPropertyDescriptor)
            {
                DataColumnPropertyDescriptor descriptor = (DataColumnPropertyDescriptor) other;
                return (descriptor.Column == this.Column);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Column.GetHashCode();
        }

        public override object GetValue(object component)
        {
            DataRowView view = (DataRowView) component;
            return view.GetColumnValue(this.column);
        }

        public override void ResetValue(object component)
        {
            ((DataRowView) component).SetColumnValue(this.column, DBNull.Value);
        }

        public override void SetValue(object component, object value)
        {
            ((DataRowView) component).SetColumnValue(this.column, value);
            this.OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                if (typeof(IList).IsAssignableFrom(this.PropertyType))
                {
                    Attribute[] array = new Attribute[base.Attributes.Count + 1];
                    base.Attributes.CopyTo(array, 0);
                    array[array.Length - 1] = new ListBindableAttribute(false);
                    return new AttributeCollection(array);
                }
                return base.Attributes;
            }
        }

        internal DataColumn Column
        {
            get
            {
                return this.column;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(DataRowView);
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return ((this.column.ColumnMapping != MappingType.Hidden) && base.IsBrowsable);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.column.ReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.column.DataType;
            }
        }
    }
}

