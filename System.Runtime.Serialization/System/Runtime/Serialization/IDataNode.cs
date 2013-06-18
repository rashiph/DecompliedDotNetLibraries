namespace System.Runtime.Serialization
{
    using System;

    internal interface IDataNode
    {
        void Clear();
        void GetData(ElementData element);

        string ClrAssemblyName { get; set; }

        string ClrTypeName { get; set; }

        string DataContractName { get; set; }

        string DataContractNamespace { get; set; }

        Type DataType { get; }

        string Id { get; set; }

        bool IsFinalValue { get; set; }

        bool PreservesReferences { get; }

        object Value { get; set; }
    }
}

