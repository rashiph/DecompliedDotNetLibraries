namespace System.Web.UI.Design
{
    using System;

    public interface IHierarchicalDataSourceDesigner
    {
        event EventHandler DataSourceChanged;

        event EventHandler SchemaRefreshed;

        void Configure();
        DesignerHierarchicalDataSourceView GetView(string viewPath);
        void RefreshSchema(bool preferSilent);
        void ResumeDataSourceEvents();
        void SuppressDataSourceEvents();

        bool CanConfigure { get; }

        bool CanRefreshSchema { get; }
    }
}

