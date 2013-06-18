namespace System.Data
{
    using System;
    using System.Collections;
    using System.Reflection;

    public interface ITableMappingCollection : IList, ICollection, IEnumerable
    {
        ITableMapping Add(string sourceTableName, string dataSetTableName);
        bool Contains(string sourceTableName);
        ITableMapping GetByDataSetTable(string dataSetTableName);
        int IndexOf(string sourceTableName);
        void RemoveAt(string sourceTableName);

        object this[string index] { get; set; }
    }
}

