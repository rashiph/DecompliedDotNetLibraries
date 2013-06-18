namespace System.Web.UI.Design
{
    using System;

    public interface IDataSourceViewSchema
    {
        IDataSourceViewSchema[] GetChildren();
        IDataSourceFieldSchema[] GetFields();

        string Name { get; }
    }
}

