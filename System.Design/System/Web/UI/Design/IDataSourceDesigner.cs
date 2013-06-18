namespace System.Web.UI.Design
{
    using System;

    public interface IDataSourceDesigner
    {
        event EventHandler DataSourceChanged;

        event EventHandler SchemaRefreshed;

        void Configure();
        DesignerDataSourceView GetView(string viewName);
        string[] GetViewNames();
        void RefreshSchema(bool preferSilent);
        void ResumeDataSourceEvents();
        void SuppressDataSourceEvents();

        bool CanConfigure { get; }

        bool CanRefreshSchema { get; }
    }
}

