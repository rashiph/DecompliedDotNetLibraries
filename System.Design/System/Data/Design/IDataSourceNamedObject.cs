namespace System.Data.Design
{
    using System;

    internal interface IDataSourceNamedObject : INamedObject
    {
        string PublicTypeName { get; }
    }
}

