namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Data;
    using System.Security.Permissions;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class AccessDataSourceDesigner : SqlDataSourceDesigner
    {
        internal override SqlDataSourceWizardForm CreateConfigureDataSourceWizardForm(IServiceProvider serviceProvider, IDataEnvironment dataEnvironment)
        {
            return new AccessDataSourceWizardForm(serviceProvider, this, dataEnvironment);
        }

        protected override string GetConnectionString()
        {
            return GetConnectionString(base.Component.Site, this.AccessDataSource);
        }

        internal static string GetConnectionString(IServiceProvider serviceProvider, System.Web.UI.WebControls.AccessDataSource dataSource)
        {
            string connectionString;
            string dataFile = dataSource.DataFile;
            try
            {
                if (dataFile.Length == 0)
                {
                    return null;
                }
                dataSource.DataFile = UrlPath.MapPath(serviceProvider, dataFile);
                connectionString = dataSource.ConnectionString;
            }
            finally
            {
                dataSource.DataFile = dataFile;
            }
            return connectionString;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataFile"];
            properties["DataFile"] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[0]);
        }

        private System.Web.UI.WebControls.AccessDataSource AccessDataSource
        {
            get
            {
                return (System.Web.UI.WebControls.AccessDataSource) base.Component;
            }
        }

        public string DataFile
        {
            get
            {
                return this.AccessDataSource.DataFile;
            }
            set
            {
                if (value != this.DataFile)
                {
                    this.AccessDataSource.DataFile = value;
                    this.UpdateDesignTimeHtml();
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }
    }
}

