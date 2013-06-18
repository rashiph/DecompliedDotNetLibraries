namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class BaseDataListDesigner : TemplatedControlDesigner, IDataBindingSchemaProvider, IDataSourceProvider
    {
        private IDataSourceDesigner _dataSourceDesigner;
        private bool _keepDataSourceBrowsable;
        private BaseDataList bdl;
        private DataTable designTimeDataTable;
        private DataTable dummyDataTable;

        protected BaseDataListDesigner()
        {
        }

        private bool ConnectToDataSource()
        {
            IDataSourceDesigner dataSourceDesigner = this.GetDataSourceDesigner();
            if (this._dataSourceDesigner == dataSourceDesigner)
            {
                return false;
            }
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.DataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.SchemaRefreshed);
            }
            this._dataSourceDesigner = dataSourceDesigner;
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged += new EventHandler(this.DataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed += new EventHandler(this.SchemaRefreshed);
            }
            return true;
        }

        private void CreateDataSource()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.CreateDataSourceCallback), null, System.Design.SR.GetString("BaseDataBoundControl_CreateDataSourceTransaction"));
        }

        private bool CreateDataSourceCallback(object context)
        {
            CreateDataSourceDialog form = new CreateDataSourceDialog(this, typeof(IDataSource), true);
            DialogResult result = UIServiceHelper.ShowDialog(base.Component.Site, form);
            string dataSourceID = form.DataSourceID;
            if (dataSourceID.Length > 0)
            {
                this.DataSourceID = dataSourceID;
            }
            return (result == DialogResult.OK);
        }

        private void DataSourceChanged(object sender, EventArgs e)
        {
            this.designTimeDataTable = null;
            this.UpdateDesignTimeHtml();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((base.Component != null) && (base.Component.Site != null))
                {
                    if (base.RootDesigner != null)
                    {
                        base.RootDesigner.LoadComplete -= new EventHandler(this.OnDesignerLoadComplete);
                    }
                    IComponentChangeService service = (IComponentChangeService) base.Component.Site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                        service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                        service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                        service.ComponentChanged -= new ComponentChangedEventHandler(this.OnAnyComponentChanged);
                    }
                }
                this.bdl = null;
                if (this._dataSourceDesigner != null)
                {
                    this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.DataSourceChanged);
                    this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.SchemaRefreshed);
                    this._dataSourceDesigner = null;
                }
            }
            base.Dispose(disposing);
        }

        private IDataSourceDesigner GetDataSourceDesigner()
        {
            IDataSourceDesigner designer = null;
            string dataSourceID = this.DataSourceID;
            if (!string.IsNullOrEmpty(dataSourceID))
            {
                System.Web.UI.Control component = ControlHelper.FindControl(base.Component.Site, (System.Web.UI.Control) base.Component, dataSourceID);
                if ((component != null) && (component.Site != null))
                {
                    IDesignerHost service = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        designer = service.GetDesigner(component) as IDataSourceDesigner;
                    }
                }
            }
            return designer;
        }

        protected IEnumerable GetDesignTimeDataSource(int minimumRows, out bool dummyDataSource)
        {
            IEnumerable resolvedSelectedDataSource = this.GetResolvedSelectedDataSource();
            return this.GetDesignTimeDataSource(resolvedSelectedDataSource, minimumRows, out dummyDataSource);
        }

        protected IEnumerable GetDesignTimeDataSource(IEnumerable selectedDataSource, int minimumRows, out bool dummyDataSource)
        {
            DataTable designTimeDataTable = this.designTimeDataTable;
            dummyDataSource = false;
            if (designTimeDataTable == null)
            {
                if (selectedDataSource != null)
                {
                    this.designTimeDataTable = DesignTimeData.CreateSampleDataTable(selectedDataSource);
                    designTimeDataTable = this.designTimeDataTable;
                }
                if (designTimeDataTable == null)
                {
                    if (this.dummyDataTable == null)
                    {
                        this.dummyDataTable = DesignTimeData.CreateDummyDataTable();
                    }
                    designTimeDataTable = this.dummyDataTable;
                    dummyDataSource = true;
                }
            }
            return DesignTimeData.GetDesignTimeDataSource(designTimeDataTable, minimumRows);
        }

        public IEnumerable GetResolvedSelectedDataSource()
        {
            IEnumerable enumerable = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                enumerable = DesignTimeData.GetSelectedDataSource(this.bdl, binding.Expression, this.DataMember);
            }
            return enumerable;
        }

        public object GetSelectedDataSource()
        {
            object selectedDataSource = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                selectedDataSource = DesignTimeData.GetSelectedDataSource(this.bdl, binding.Expression);
            }
            return selectedDataSource;
        }

        [Obsolete("Use of this method is not recommended because template editing is handled in ControlDesigner. To support template editing expose template data in the TemplateGroups property and call SetViewFlags(ViewFlags.TemplateEditing, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public override IEnumerable GetTemplateContainerDataSource(string templateName)
        {
            return this.GetResolvedSelectedDataSource();
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(BaseDataList));
            this.bdl = (BaseDataList) component;
            base.Initialize(component);
            base.SetViewFlags(ViewFlags.DesignTimeHtmlRequiresLoadComplete, true);
            if (base.RootDesigner != null)
            {
                if (base.RootDesigner.IsLoading)
                {
                    base.RootDesigner.LoadComplete += new EventHandler(this.OnDesignerLoadComplete);
                }
                else
                {
                    this.OnDesignerLoadComplete(null, EventArgs.Empty);
                }
            }
            IComponentChangeService service = (IComponentChangeService) component.Site.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnAnyComponentChanged);
            }
        }

        protected internal void InvokePropertyBuilder(int initialPage)
        {
            ComponentEditor editor;
            if (this.bdl is System.Web.UI.WebControls.DataGrid)
            {
                editor = new DataGridComponentEditor(initialPage);
            }
            else
            {
                editor = new DataListComponentEditor(initialPage);
            }
            editor.EditComponent(this.bdl);
        }

        private void OnAnyComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Member != null)
            {
                IDataSource component = e.Component as IDataSource;
                if ((((component != null) && (component is System.Web.UI.Control)) && ((e.Member.Name == "ID") && (base.Component != null))) && ((((string) e.OldValue) == this.DataSourceID) || (((string) e.NewValue) == this.DataSourceID)))
                {
                    this.ConnectToDataSource();
                    this.UpdateDesignTimeHtml();
                }
            }
        }

        [Obsolete("Use of this method is not recommended because the AutoFormat dialog is launched by the designer host. The list of available AutoFormats is exposed on the ControlDesigner in the AutoFormats property. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected void OnAutoFormat(object sender, EventArgs e)
        {
        }

        public override void OnAutoFormatApplied(DesignerAutoFormat appliedAutoFormat)
        {
            this.OnStylesChanged();
            base.OnAutoFormatApplied(appliedAutoFormat);
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            IComponent component = e.Component;
            IDataSource source = component as IDataSource;
            if (((source != null) && (component is System.Web.UI.Control)) && (((System.Web.UI.Control) source).ID == this.DataSourceID))
            {
                this.ConnectToDataSource();
                this.UpdateDesignTimeHtml();
            }
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Member != null)
            {
                string name = e.Member.Name;
                if (name.Equals("DataSource") || name.Equals("DataMember"))
                {
                    this.OnDataSourceChanged();
                }
                else if (((name.Equals("ItemStyle") || name.Equals("AlternatingItemStyle")) || (name.Equals("SelectedItemStyle") || name.Equals("EditItemStyle"))) || (((name.Equals("HeaderStyle") || name.Equals("FooterStyle")) || (name.Equals("SeparatorStyle") || name.Equals("Font"))) || (name.Equals("ForeColor") || name.Equals("BackColor"))))
                {
                    this.OnStylesChanged();
                }
            }
            base.OnComponentChanged(sender, e);
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            IDataSource component = e.Component as IDataSource;
            if (((component != null) && (component is System.Web.UI.Control)) && ((base.Component != null) && (((System.Web.UI.Control) component).ID == this.DataSourceID)))
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((service != null) && !service.Loading)
                {
                    this.ConnectToDataSource();
                    this.UpdateDesignTimeHtml();
                }
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            IDataSource component = e.Component as IDataSource;
            if ((((component != null) && (component is System.Web.UI.Control)) && ((base.Component != null) && (((System.Web.UI.Control) component).ID == this.DataSourceID))) && (this._dataSourceDesigner != null))
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.DataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.SchemaRefreshed);
                this._dataSourceDesigner = null;
            }
        }

        protected internal virtual void OnDataSourceChanged()
        {
            this.ConnectToDataSource();
            this.designTimeDataTable = null;
        }

        private void OnDesignerLoadComplete(object sender, EventArgs e)
        {
            this.ConnectToDataSource();
        }

        protected void OnPropertyBuilder(object sender, EventArgs e)
        {
            this.InvokePropertyBuilder(0);
        }

        protected virtual void OnSchemaRefreshed()
        {
            this.UpdateDesignTimeHtml();
        }

        protected internal void OnStylesChanged()
        {
            this.OnTemplateEditingVerbsChanged();
        }

        protected abstract void OnTemplateEditingVerbsChanged();
        protected override void PreFilterProperties(IDictionary properties)
        {
            int num2;
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataSource"];
            System.ComponentModel.AttributeCollection attributes = oldPropertyDescriptor.Attributes;
            int index = -1;
            int count = attributes.Count;
            string dataSource = this.DataSource;
            if (dataSource.Length > 0)
            {
                this._keepDataSourceBrowsable = true;
            }
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is BrowsableAttribute)
                {
                    index = i;
                    break;
                }
            }
            if (((index == -1) && (dataSource.Length == 0)) && !this._keepDataSourceBrowsable)
            {
                num2 = count + 2;
            }
            else
            {
                num2 = count + 1;
            }
            Attribute[] array = new Attribute[num2];
            attributes.CopyTo(array, 0);
            array[count] = new TypeConverterAttribute(typeof(DataSourceConverter));
            if ((dataSource.Length == 0) && !this._keepDataSourceBrowsable)
            {
                if (index == -1)
                {
                    array[count + 1] = BrowsableAttribute.No;
                }
                else
                {
                    array[index] = BrowsableAttribute.No;
                }
            }
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), "DataSource", typeof(string), array);
            properties["DataSource"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataMember"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataMemberConverter)) });
            properties["DataMember"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataKeyField"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataFieldConverter)) });
            properties["DataKeyField"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataSourceID"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataSourceIDConverter)) });
            properties["DataSourceID"] = oldPropertyDescriptor;
        }

        private void SchemaRefreshed(object sender, EventArgs e)
        {
            this.OnSchemaRefreshed();
        }

        void IDataBindingSchemaProvider.RefreshSchema(bool preferSilent)
        {
            IDataSourceDesigner dataSourceDesigner = this.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.RefreshSchema(preferSilent);
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new BaseDataListActionList(this, this.DataSourceDesigner));
                return lists;
            }
        }

        public string DataKeyField
        {
            get
            {
                return this.bdl.DataKeyField;
            }
            set
            {
                this.bdl.DataKeyField = value;
            }
        }

        public string DataMember
        {
            get
            {
                return this.bdl.DataMember;
            }
            set
            {
                this.bdl.DataMember = value;
                this.OnDataSourceChanged();
            }
        }

        public string DataSource
        {
            get
            {
                DataBinding binding = base.DataBindings["DataSource"];
                if (binding != null)
                {
                    return binding.Expression;
                }
                return string.Empty;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    base.DataBindings.Remove("DataSource");
                }
                else
                {
                    DataBinding binding = base.DataBindings["DataSource"];
                    if (binding == null)
                    {
                        binding = new DataBinding("DataSource", typeof(IEnumerable), value);
                    }
                    else
                    {
                        binding.Expression = value;
                    }
                    base.DataBindings.Add(binding);
                }
                this.OnDataSourceChanged();
                base.OnBindingsCollectionChangedInternal("DataSource");
            }
        }

        public IDataSourceDesigner DataSourceDesigner
        {
            get
            {
                return this._dataSourceDesigner;
            }
        }

        public string DataSourceID
        {
            get
            {
                return this.bdl.DataSourceID;
            }
            set
            {
                if (value != this.DataSourceID)
                {
                    if (value == System.Design.SR.GetString("DataSourceIDChromeConverter_NewDataSource"))
                    {
                        this.CreateDataSource();
                    }
                    else
                    {
                        if (value == System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource"))
                        {
                            value = string.Empty;
                        }
                        this.bdl.DataSourceID = value;
                        this.OnDataSourceChanged();
                        this.OnSchemaRefreshed();
                    }
                }
            }
        }

        public DesignerDataSourceView DesignerView
        {
            get
            {
                DesignerDataSourceView view = null;
                if (this.DataSourceDesigner != null)
                {
                    view = this.DataSourceDesigner.GetView(this.DataMember);
                    if ((view == null) && string.IsNullOrEmpty(this.DataMember))
                    {
                        string[] viewNames = this.DataSourceDesigner.GetViewNames();
                        if ((viewNames != null) && (viewNames.Length > 0))
                        {
                            view = this.DataSourceDesigner.GetView(viewNames[0]);
                        }
                    }
                }
                return view;
            }
        }

        bool IDataBindingSchemaProvider.CanRefreshSchema
        {
            get
            {
                IDataSourceDesigner dataSourceDesigner = this.DataSourceDesigner;
                return ((dataSourceDesigner != null) && dataSourceDesigner.CanRefreshSchema);
            }
        }

        IDataSourceViewSchema IDataBindingSchemaProvider.Schema
        {
            get
            {
                DesignerDataSourceView designerView = this.DesignerView;
                if (designerView != null)
                {
                    return designerView.Schema;
                }
                return null;
            }
        }
    }
}

