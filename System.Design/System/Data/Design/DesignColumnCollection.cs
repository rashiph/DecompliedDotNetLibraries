namespace System.Data.Design
{
    using System;
    using System.Data;
    using System.Reflection;

    internal class DesignColumnCollection : DataSourceCollectionBase
    {
        private DesignTable designTable;
        private DesignTable table;

        public DesignColumnCollection(DesignTable designTable) : base(designTable)
        {
            this.designTable = designTable;
            if ((designTable != null) && (designTable.DataTable != null))
            {
                foreach (DataColumn column in designTable.DataTable.Columns)
                {
                    this.Add(new DesignColumn(column));
                }
            }
            this.table = designTable;
        }

        public void Add(DesignColumn designColumn)
        {
            if ((designColumn.DesignTable != null) && (designColumn.DesignTable != this.designTable))
            {
                throw new InternalException("Cannot insert a DesignColumn object in two collections.");
            }
            designColumn.DesignTable = this.designTable;
            base.List.Add(designColumn);
            if (((designColumn.DataColumn != null) && (this.designTable != null)) && ((this.designTable.DataTable != null) && !this.designTable.DataTable.Columns.Contains(designColumn.Name)))
            {
                this.designTable.DataTable.Columns.Add(designColumn.DataColumn);
            }
        }

        public int IndexOf(DesignColumn column)
        {
            return base.List.IndexOf(column);
        }

        protected override void OnInsert(int index, object value)
        {
            base.OnInsert(index, value);
            base.ValidateType(value);
            DesignColumn column = (DesignColumn) value;
            if (((column.DataColumn != null) && (this.table != null)) && !this.table.DataTable.Columns.Contains(column.DataColumn.ColumnName))
            {
                this.table.DataTable.Columns.Add(column.DataColumn);
            }
            column.DesignTable = this.designTable;
        }

        protected override void OnRemove(int index, object value)
        {
            base.OnRemove(index, value);
            base.ValidateType(value);
            DesignColumn column = (DesignColumn) value;
            if ((this.table != null) && (column.DataColumn != null))
            {
                this.table.DataTable.Columns.Remove(column.DataColumn);
            }
            column.DesignTable = null;
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            base.OnSet(index, oldValue, newValue);
            base.ValidateType(newValue);
            base.ValidateType(oldValue);
            DesignColumn column = (DesignColumn) oldValue;
            DesignColumn column2 = (DesignColumn) newValue;
            if ((this.table != null) && (oldValue != newValue))
            {
                if (column.DataColumn != null)
                {
                    this.table.DataTable.Columns.Remove(column.DataColumn);
                    column.DesignTable = null;
                }
                if ((column2.DataColumn != null) && !this.table.DataTable.Columns.Contains(column2.DataColumn.ColumnName))
                {
                    this.table.DataTable.Columns.Add(column2.DataColumn);
                    column2.DesignTable = this.designTable;
                }
            }
        }

        public void Remove(DesignColumn column)
        {
            base.List.Remove(column);
        }

        public DesignColumn this[string columnName]
        {
            get
            {
                return (DesignColumn) this.FindObject(columnName);
            }
        }

        public DesignColumn this[int index]
        {
            get
            {
                int num = 0;
                foreach (DesignColumn column in base.InnerList)
                {
                    if (index == num)
                    {
                        return column;
                    }
                    num++;
                }
                throw new InternalException("Index out of range in getting DesignColumn", 0x4e2b);
            }
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(DesignColumn);
            }
        }

        protected override INameService NameService
        {
            get
            {
                return DataSetNameService.DefaultInstance;
            }
        }
    }
}

