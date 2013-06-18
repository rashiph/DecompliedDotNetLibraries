namespace System.Data
{
    using System;

    public interface IDataAdapter
    {
        int Fill(DataSet dataSet);
        DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType);
        IDataParameter[] GetFillParameters();
        int Update(DataSet dataSet);

        System.Data.MissingMappingAction MissingMappingAction { get; set; }

        System.Data.MissingSchemaAction MissingSchemaAction { get; set; }

        ITableMappingCollection TableMappings { get; }
    }
}

