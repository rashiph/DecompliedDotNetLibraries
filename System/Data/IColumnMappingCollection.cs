namespace System.Data
{
    using System;
    using System.Collections;
    using System.Reflection;

    public interface IColumnMappingCollection : IList, ICollection, IEnumerable
    {
        IColumnMapping Add(string sourceColumnName, string dataSetColumnName);
        bool Contains(string sourceColumnName);
        IColumnMapping GetByDataSetColumn(string dataSetColumnName);
        int IndexOf(string sourceColumnName);
        void RemoveAt(string sourceColumnName);

        object this[string index] { get; set; }
    }
}

