namespace System.Web.UI.Design
{
    using System;
    using System.Data;

    public sealed class DataSetSchema : IDataSourceSchema
    {
        private DataSet _dataSet;

        public DataSetSchema(DataSet dataSet)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            this._dataSet = dataSet;
        }

        public IDataSourceViewSchema[] GetViews()
        {
            DataTableCollection tables = this._dataSet.Tables;
            DataSetViewSchema[] schemaArray = new DataSetViewSchema[tables.Count];
            for (int i = 0; i < tables.Count; i++)
            {
                schemaArray[i] = new DataSetViewSchema(tables[i]);
            }
            return schemaArray;
        }
    }
}

