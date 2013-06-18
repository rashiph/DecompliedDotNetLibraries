namespace System.Web.UI.Design
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public sealed class DataSetViewSchema : IDataSourceViewSchema
    {
        private DataTable _dataTable;

        public DataSetViewSchema(DataTable dataTable)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }
            this._dataTable = dataTable;
        }

        public IDataSourceViewSchema[] GetChildren()
        {
            return null;
        }

        public IDataSourceFieldSchema[] GetFields()
        {
            List<DataSetFieldSchema> list = new List<DataSetFieldSchema>();
            foreach (DataColumn column in this._dataTable.Columns)
            {
                if (column.ColumnMapping != MappingType.Hidden)
                {
                    list.Add(new DataSetFieldSchema(column));
                }
            }
            return list.ToArray();
        }

        public string Name
        {
            get
            {
                return this._dataTable.TableName;
            }
        }
    }
}

