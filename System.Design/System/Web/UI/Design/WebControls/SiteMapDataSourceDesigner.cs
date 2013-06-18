namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SiteMapDataSourceDesigner : HierarchicalDataSourceDesigner, IDataSourceDesigner
    {
        private System.Web.UI.WebControls.SiteMapDataSource _siteMapDataSource;
        private static readonly string _siteMapNodeType = typeof(SiteMapNode).Name;
        private SiteMapProvider _siteMapProvider;
        internal static readonly SiteMapSchema SiteMapHierarchicalSchema = new SiteMapSchema();

        event EventHandler IDataSourceDesigner.DataSourceChanged
        {
            add
            {
                base.DataSourceChanged += value;
            }
            remove
            {
                base.DataSourceChanged -= value;
            }
        }

        event EventHandler IDataSourceDesigner.SchemaRefreshed
        {
            add
            {
                base.SchemaRefreshed += value;
            }
            remove
            {
                base.SchemaRefreshed -= value;
            }
        }

        public override DesignerHierarchicalDataSourceView GetView(string viewPath)
        {
            return new SiteMapDesignerHierarchicalDataSourceView(this, viewPath);
        }

        public virtual string[] GetViewNames()
        {
            return new string[0];
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(System.Web.UI.WebControls.SiteMapDataSource));
            base.Initialize(component);
            this._siteMapDataSource = (System.Web.UI.WebControls.SiteMapDataSource) component;
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            base.OnComponentChanged(sender, e);
            this.OnDataSourceChanged(EventArgs.Empty);
        }

        public override void RefreshSchema(bool preferSilent)
        {
            try
            {
                this.SuppressDataSourceEvents();
                this._siteMapProvider = null;
                this.OnDataSourceChanged(EventArgs.Empty);
            }
            finally
            {
                this.ResumeDataSourceEvents();
            }
        }

        void IDataSourceDesigner.Configure()
        {
            this.Configure();
        }

        DesignerDataSourceView IDataSourceDesigner.GetView(string viewName)
        {
            return new SiteMapDesignerDataSourceView(this, viewName);
        }

        string[] IDataSourceDesigner.GetViewNames()
        {
            return this.GetViewNames();
        }

        void IDataSourceDesigner.RefreshSchema(bool preferSilent)
        {
            this.RefreshSchema(preferSilent);
        }

        void IDataSourceDesigner.ResumeDataSourceEvents()
        {
            this.ResumeDataSourceEvents();
        }

        void IDataSourceDesigner.SuppressDataSourceEvents()
        {
            this.SuppressDataSourceEvents();
        }

        public override bool CanRefreshSchema
        {
            get
            {
                return true;
            }
        }

        internal SiteMapProvider DesignTimeSiteMapProvider
        {
            get
            {
                if (this._siteMapProvider == null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    this._siteMapProvider = new System.Web.UI.Design.WebControls.DesignTimeSiteMapProvider(service);
                }
                return this._siteMapProvider;
            }
        }

        internal System.Web.UI.WebControls.SiteMapDataSource SiteMapDataSource
        {
            get
            {
                return this._siteMapDataSource;
            }
        }

        bool IDataSourceDesigner.CanConfigure
        {
            get
            {
                return this.CanConfigure;
            }
        }

        bool IDataSourceDesigner.CanRefreshSchema
        {
            get
            {
                return this.CanRefreshSchema;
            }
        }

        private class SiteMapDataSourceTextField : IDataSourceFieldSchema
        {
            private string _fieldName;
            internal static readonly SiteMapDataSourceDesigner.SiteMapDataSourceTextField DescriptionField = new SiteMapDataSourceDesigner.SiteMapDataSourceTextField("Description");
            internal static readonly SiteMapDataSourceDesigner.SiteMapDataSourceTextField TitleField = new SiteMapDataSourceDesigner.SiteMapDataSourceTextField("Title");
            internal static readonly SiteMapDataSourceDesigner.SiteMapDataSourceTextField UrlField = new SiteMapDataSourceDesigner.SiteMapDataSourceTextField("Url");

            internal SiteMapDataSourceTextField(string fieldName)
            {
                this._fieldName = fieldName;
            }

            Type IDataSourceFieldSchema.DataType
            {
                get
                {
                    return typeof(string);
                }
            }

            bool IDataSourceFieldSchema.Identity
            {
                get
                {
                    return false;
                }
            }

            bool IDataSourceFieldSchema.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            bool IDataSourceFieldSchema.IsUnique
            {
                get
                {
                    return false;
                }
            }

            int IDataSourceFieldSchema.Length
            {
                get
                {
                    return -1;
                }
            }

            string IDataSourceFieldSchema.Name
            {
                get
                {
                    return this._fieldName;
                }
            }

            bool IDataSourceFieldSchema.Nullable
            {
                get
                {
                    return true;
                }
            }

            int IDataSourceFieldSchema.Precision
            {
                get
                {
                    return -1;
                }
            }

            bool IDataSourceFieldSchema.PrimaryKey
            {
                get
                {
                    return false;
                }
            }

            int IDataSourceFieldSchema.Scale
            {
                get
                {
                    return -1;
                }
            }
        }

        internal class SiteMapDataSourceViewSchema : IDataSourceViewSchema
        {
            IDataSourceViewSchema[] IDataSourceViewSchema.GetChildren()
            {
                return null;
            }

            IDataSourceFieldSchema[] IDataSourceViewSchema.GetFields()
            {
                return new SiteMapDataSourceDesigner.SiteMapDataSourceTextField[] { SiteMapDataSourceDesigner.SiteMapDataSourceTextField.DescriptionField, SiteMapDataSourceDesigner.SiteMapDataSourceTextField.TitleField, SiteMapDataSourceDesigner.SiteMapDataSourceTextField.UrlField };
            }

            string IDataSourceViewSchema.Name
            {
                get
                {
                    return SiteMapDataSourceDesigner._siteMapNodeType;
                }
            }
        }

        internal class SiteMapSchema : IDataSourceSchema
        {
            IDataSourceViewSchema[] IDataSourceSchema.GetViews()
            {
                return new SiteMapDataSourceDesigner.SiteMapDataSourceViewSchema[] { new SiteMapDataSourceDesigner.SiteMapDataSourceViewSchema() };
            }
        }
    }
}

