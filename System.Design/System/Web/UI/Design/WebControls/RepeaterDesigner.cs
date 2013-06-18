namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class RepeaterDesigner : ControlDesigner, IDataSourceProvider
    {
        private IDataSourceDesigner _dataSourceDesigner;
        private DataTable designTimeDataTable;
        private DataTable dummyDataTable;
        internal static TraceSwitch RepeaterDesignerSwitch = new TraceSwitch("RepeaterDesigner", "Enable Repeater designer general purpose traces.");

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
            }
            this._dataSourceDesigner = dataSourceDesigner;
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged += new EventHandler(this.DataSourceChanged);
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
            if ((disposing && (base.Component != null)) && (base.Component.Site != null))
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
            base.Dispose(disposing);
        }

        protected virtual void ExecuteChooseDataSourcePostSteps()
        {
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

        protected IEnumerable GetDesignTimeDataSource(int minimumRows)
        {
            IEnumerable resolvedSelectedDataSource = this.GetResolvedSelectedDataSource();
            return this.GetDesignTimeDataSource(resolvedSelectedDataSource, minimumRows);
        }

        protected IEnumerable GetDesignTimeDataSource(IEnumerable selectedDataSource, int minimumRows)
        {
            DataTable designTimeDataTable = this.designTimeDataTable;
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
                }
            }
            return DesignTimeData.GetDesignTimeDataSource(designTimeDataTable, minimumRows);
        }

        public override string GetDesignTimeHtml()
        {
            IEnumerable selectedDataSource = null;
            bool templatesExist = this.TemplatesExist;
            Repeater viewControl = (Repeater) base.ViewControl;
            if (templatesExist)
            {
                DesignerDataSourceView designerView = this.DesignerView;
                IEnumerable designTimeDataSource = null;
                bool flag2 = false;
                string dataSourceID = string.Empty;
                if (designerView == null)
                {
                    selectedDataSource = this.GetResolvedSelectedDataSource();
                    designTimeDataSource = this.GetDesignTimeDataSource(selectedDataSource, 5);
                }
                else
                {
                    try
                    {
                        bool flag3;
                        designTimeDataSource = designerView.GetDesignTimeData(5, out flag3);
                    }
                    catch (Exception exception)
                    {
                        if (base.Component.Site != null)
                        {
                            IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.Component.Site.GetService(typeof(IComponentDesignerDebugService));
                            if (service != null)
                            {
                                service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.GetDesignTimeData", exception.Message }));
                            }
                        }
                    }
                }
                try
                {
                    viewControl.DataSource = designTimeDataSource;
                    dataSourceID = viewControl.DataSourceID;
                    viewControl.DataSourceID = string.Empty;
                    flag2 = true;
                    viewControl.DataBind();
                    return base.GetDesignTimeHtml();
                }
                catch (Exception exception2)
                {
                    return this.GetErrorDesignTimeHtml(exception2);
                }
                finally
                {
                    viewControl.DataSource = null;
                    if (flag2)
                    {
                        viewControl.DataSourceID = dataSourceID;
                    }
                }
            }
            return this.GetEmptyDesignTimeHtml();
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Repeater_NoTemplatesInst"));
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRendering"));
        }

        public IEnumerable GetResolvedSelectedDataSource()
        {
            IEnumerable enumerable = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                enumerable = DesignTimeData.GetSelectedDataSource(base.Component, binding.Expression, this.DataMember);
            }
            return enumerable;
        }

        public object GetSelectedDataSource()
        {
            object selectedDataSource = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                selectedDataSource = DesignTimeData.GetSelectedDataSource(base.Component, binding.Expression);
            }
            return selectedDataSource;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Repeater));
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

        private void OnAnyComponentChanged(object source, ComponentChangedEventArgs ce)
        {
            if ((((ce.Member != null) && (ce.Component is System.Web.UI.Control)) && ((ce.Member.Name == "ID") && (base.Component != null))) && ((((string) ce.OldValue) == this.DataSourceID) || (((string) ce.NewValue) == this.DataSourceID)))
            {
                this.ConnectToDataSource();
                this.UpdateDesignTimeHtml();
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if ((component != null) && (component.ID == this.DataSourceID))
            {
                this.ConnectToDataSource();
                this.UpdateDesignTimeHtml();
            }
        }

        public override void OnComponentChanged(object source, ComponentChangedEventArgs ce)
        {
            if (ce.Member != null)
            {
                string name = ce.Member.Name;
                if (name.Equals("DataSource") || name.Equals("DataMember"))
                {
                    this.OnDataSourceChanged();
                }
            }
            base.OnComponentChanged(source, ce);
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if (((component != null) && (base.Component != null)) && (component.ID == this.DataSourceID))
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
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if (((component != null) && (component.ID == this.DataSourceID)) && ((base.Component != null) && (this._dataSourceDesigner != null)))
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.DataSourceChanged);
                this._dataSourceDesigner = null;
            }
        }

        public virtual void OnDataSourceChanged()
        {
            this.ConnectToDataSource();
            this.designTimeDataTable = null;
        }

        private void OnDesignerLoadComplete(object sender, EventArgs e)
        {
            this.ConnectToDataSource();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            int num2;
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataSource"];
            System.ComponentModel.AttributeCollection attributes = oldPropertyDescriptor.Attributes;
            int index = -1;
            int count = attributes.Count;
            string dataSource = this.DataSource;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is BrowsableAttribute)
                {
                    index = i;
                }
            }
            if ((index == -1) && (dataSource.Length == 0))
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
            if (dataSource.Length == 0)
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
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataSourceID"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataSourceIDConverter)) });
            properties["DataSourceID"] = oldPropertyDescriptor;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new DataBoundControlActionList(this, this.DataSourceDesigner));
                return lists;
            }
        }

        public string DataMember
        {
            get
            {
                return ((Repeater) base.Component).DataMember;
            }
            set
            {
                ((Repeater) base.Component).DataMember = value;
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
                return ((Repeater) base.Component).DataSourceID;
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
                        ((Repeater) base.Component).DataSourceID = value;
                        this.OnDataSourceChanged();
                        this.ExecuteChooseDataSourcePostSteps();
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

        protected bool TemplatesExist
        {
            get
            {
                Repeater viewControl = (Repeater) base.ViewControl;
                if (((viewControl.ItemTemplate == null) && (viewControl.HeaderTemplate == null)) && (viewControl.FooterTemplate == null))
                {
                    return (viewControl.AlternatingItemTemplate != null);
                }
                return true;
            }
        }
    }
}

