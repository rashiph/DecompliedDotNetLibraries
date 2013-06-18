namespace System.Data.Design
{
    using System;
    using System.Data;
    using System.Reflection;

    internal class DesignTableCollection : DataSourceCollectionBase
    {
        private DesignDataSource dataSource;

        public DesignTableCollection(DesignDataSource dataSource) : base(dataSource)
        {
            this.dataSource = dataSource;
        }

        public void Add(DesignTable designTable)
        {
            base.List.Add(designTable);
        }

        public bool Contains(DesignTable table)
        {
            return base.List.Contains(table);
        }

        public int IndexOf(DesignTable table)
        {
            return base.List.IndexOf(table);
        }

        protected override void OnInsert(int index, object value)
        {
            base.OnInsert(index, value);
            DesignTable table = (DesignTable) value;
            if ((table.Name == null) || (table.Name.Length == 0))
            {
                table.Name = this.CreateUniqueName(table);
            }
            this.NameService.ValidateUniqueName(this, table.Name);
            if ((this.dataSource == null) || (table.Owner != this.dataSource))
            {
                if ((this.dataSource != null) && (table.Owner != null))
                {
                    throw new InternalException("This table belongs to another DataSource already", 0x4e22);
                }
                System.Data.DataSet dataSet = this.DataSet;
                if ((dataSet != null) && !dataSet.Tables.Contains(table.DataTable.TableName))
                {
                    dataSet.Tables.Add(table.DataTable);
                }
                table.Owner = this.dataSource;
            }
        }

        protected override void OnRemove(int index, object value)
        {
            base.OnRemove(index, value);
            DesignTable table = (DesignTable) value;
            System.Data.DataSet dataSet = this.DataSet;
            if (((dataSet != null) && (table.DataTable != null)) && dataSet.Tables.Contains(table.DataTable.TableName))
            {
                dataSet.Tables.Remove(table.DataTable);
            }
            table.Owner = null;
        }

        public void Remove(DesignTable table)
        {
            base.List.Remove(table);
        }

        private System.Data.DataSet DataSet
        {
            get
            {
                if (this.dataSource != null)
                {
                    return this.dataSource.DataSet;
                }
                return null;
            }
        }

        internal DesignTable this[string name]
        {
            get
            {
                return (DesignTable) this.FindObject(name);
            }
        }

        internal DesignTable this[DataTable dataTable]
        {
            get
            {
                foreach (DesignTable table in this)
                {
                    if (table.DataTable == dataTable)
                    {
                        return table;
                    }
                }
                return null;
            }
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(DesignTable);
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

