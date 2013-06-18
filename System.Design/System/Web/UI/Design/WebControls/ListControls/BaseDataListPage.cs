namespace System.Web.UI.Design.WebControls.ListControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal abstract class BaseDataListPage : ComponentEditorPage
    {
        private bool dataGridMode;

        protected BaseDataListPage()
        {
        }

        protected BaseDataList GetBaseControl()
        {
            return (BaseDataList) base.GetSelectedComponent();
        }

        protected BaseDataListDesigner GetBaseDesigner()
        {
            BaseDataListDesigner designer = null;
            IComponent selectedComponent = base.GetSelectedComponent();
            IDesignerHost service = (IDesignerHost) selectedComponent.Site.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                designer = (BaseDataListDesigner) service.GetDesigner(selectedComponent);
            }
            return designer;
        }

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);
            this.dataGridMode = this.GetBaseControl() is System.Web.UI.WebControls.DataGrid;
            if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
            {
                this.RightToLeft = RightToLeft.Yes;
            }
        }

        public override void ShowHelp()
        {
            IHelpService service = (IHelpService) base.GetSelectedComponent().Site.GetService(typeof(IHelpService));
            if (service != null)
            {
                service.ShowHelpFromKeyword(this.HelpKeyword);
            }
        }

        public override bool SupportsHelp()
        {
            return true;
        }

        protected abstract string HelpKeyword { get; }

        protected bool IsDataGridMode
        {
            get
            {
                return this.dataGridMode;
            }
        }

        protected class DataSourceItem
        {
            private PropertyDescriptorCollection dataFields;
            private string dataSourceName;
            private IEnumerable runtimeDataSource;

            public DataSourceItem(string dataSourceName, IEnumerable runtimeDataSource)
            {
                this.runtimeDataSource = runtimeDataSource;
                this.dataSourceName = dataSourceName;
            }

            protected void ClearFields()
            {
                this.dataFields = null;
            }

            public override string ToString()
            {
                return this.Name;
            }

            public PropertyDescriptorCollection Fields
            {
                get
                {
                    if (this.dataFields == null)
                    {
                        IEnumerable runtimeDataSource = this.RuntimeDataSource;
                        if (runtimeDataSource != null)
                        {
                            this.dataFields = DesignTimeData.GetDataFields(runtimeDataSource);
                        }
                    }
                    if (this.dataFields == null)
                    {
                        this.dataFields = new PropertyDescriptorCollection(null);
                    }
                    return this.dataFields;
                }
            }

            public virtual bool HasDataMembers
            {
                get
                {
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    return this.dataSourceName;
                }
            }

            protected virtual object RuntimeComponent
            {
                get
                {
                    return this.runtimeDataSource;
                }
            }

            protected virtual IEnumerable RuntimeDataSource
            {
                get
                {
                    return this.runtimeDataSource;
                }
            }
        }

        protected class ListSourceDataSourceItem : BaseDataListPage.DataSourceItem
        {
            private string currentDataMember;
            private IListSource runtimeListSource;

            public ListSourceDataSourceItem(string dataSourceName, IListSource runtimeListSource) : base(dataSourceName, null)
            {
                this.runtimeListSource = runtimeListSource;
            }

            public string CurrentDataMember
            {
                get
                {
                    return this.currentDataMember;
                }
                set
                {
                    this.currentDataMember = value;
                    base.ClearFields();
                }
            }

            public override bool HasDataMembers
            {
                get
                {
                    return this.runtimeListSource.ContainsListCollection;
                }
            }

            protected override object RuntimeComponent
            {
                get
                {
                    return this.runtimeListSource;
                }
            }

            protected override IEnumerable RuntimeDataSource
            {
                get
                {
                    if (this.HasDataMembers)
                    {
                        return DesignTimeData.GetDataMember(this.runtimeListSource, this.currentDataMember);
                    }
                    return this.runtimeListSource.GetList();
                }
            }
        }
    }
}

