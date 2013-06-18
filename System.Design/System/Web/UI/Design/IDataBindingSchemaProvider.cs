namespace System.Web.UI.Design
{
    using System;

    public interface IDataBindingSchemaProvider
    {
        void RefreshSchema(bool preferSilent);

        bool CanRefreshSchema { get; }

        IDataSourceViewSchema Schema { get; }
    }
}

