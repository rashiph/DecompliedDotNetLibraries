namespace System.ComponentModel.Design.Data
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    [Guid("ABE5C1F0-C96E-40c4-A22D-4A5CEC899BDC")]
    public abstract class DataSourceProviderService
    {
        protected DataSourceProviderService()
        {
        }

        public abstract object AddDataSourceInstance(IDesignerHost host, DataSourceDescriptor dataSourceDescriptor);
        public abstract DataSourceGroupCollection GetDataSources();
        public abstract DataSourceGroup InvokeAddNewDataSource(IWin32Window parentWindow, FormStartPosition startPosition);
        public abstract bool InvokeConfigureDataSource(IWin32Window parentWindow, FormStartPosition startPosition, DataSourceDescriptor dataSourceDescriptor);
        public abstract void NotifyDataSourceComponentAdded(object dsc);

        public abstract bool SupportsAddNewDataSource { get; }

        public abstract bool SupportsConfigureDataSource { get; }
    }
}

