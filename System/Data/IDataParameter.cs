namespace System.Data
{
    using System;

    public interface IDataParameter
    {
        System.Data.DbType DbType { get; set; }

        ParameterDirection Direction { get; set; }

        bool IsNullable { get; }

        string ParameterName { get; set; }

        string SourceColumn { get; set; }

        DataRowVersion SourceVersion { get; set; }

        object Value { get; set; }
    }
}

