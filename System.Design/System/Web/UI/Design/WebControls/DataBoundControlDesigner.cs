namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class DataBoundControlDesigner : BaseDataBoundControlDesigner, IDataBindingSchemaProvider, IDataSourceProvider
    {
        private IDataSourceDesigner _dataSourceDesigner;

        protected override bool ConnectToDataSource()
        {
            IDataSourceDesigner dataSourceDesigner = this.GetDataSourceDesigner();
            if (this._dataSourceDesigner == dataSourceDesigner)
            {
                return false;
            }
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.OnDataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.OnSchemaRefreshed);
            }
            this._dataSourceDesigner = dataSourceDesigner;
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged += new EventHandler(this.OnDataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed += new EventHandler(this.OnSchemaRefreshed);
            }
            return true;
        }

        protected override void CreateDataSource()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.CreateDataSourceCallback), null, System.Design.SR.GetString("BaseDataBoundControl_CreateDataSourceTransaction"));
        }

        private bool CreateDataSourceCallback(object context)
        {
            string str;
            DialogResult result = BaseDataBoundControlDesigner.ShowCreateDataSourceDialog(this, typeof(IDataSource), true, out str);
            if (str.Length > 0)
            {
                base.DataSourceID = str;
            }
            return (result == DialogResult.OK);
        }

        protected override void DataBind(BaseDataBoundControl dataBoundControl)
        {
            IEnumerable designTimeDataSource = this.GetDesignTimeDataSource();
            string dataSourceID = dataBoundControl.DataSourceID;
            object dataSource = dataBoundControl.DataSource;
            dataBoundControl.DataSource = designTimeDataSource;
            dataBoundControl.DataSourceID = string.Empty;
            try
            {
                if (designTimeDataSource != null)
                {
                    dataBoundControl.DataBind();
                }
            }
            finally
            {
                dataBoundControl.DataSource = dataSource;
                dataBoundControl.DataSourceID = dataSourceID;
            }
        }

        protected override void DisconnectFromDataSource()
        {
            if (this._dataSourceDesigner != null)
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.OnDataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.OnSchemaRefreshed);
                this._dataSourceDesigner = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this._dataSourceDesigner != null))
            {
                this._dataSourceDesigner.DataSourceChanged -= new EventHandler(this.OnDataSourceChanged);
                this._dataSourceDesigner.SchemaRefreshed -= new EventHandler(this.OnSchemaRefreshed);
                this._dataSourceDesigner = null;
            }
            base.Dispose(disposing);
        }

        private IDataSourceDesigner GetDataSourceDesigner()
        {
            IDataSourceDesigner designer = null;
            string dataSourceID = base.DataSourceID;
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

        protected virtual IEnumerable GetDesignTimeDataSource()
        {
            bool flag;
            IEnumerable designTimeData = null;
            DesignerDataSourceView designerView = this.DesignerView;
            if (designerView != null)
            {
                try
                {
                    designTimeData = designerView.GetDesignTimeData(this.SampleRowCount, out flag);
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
            else
            {
                IEnumerable resolvedSelectedDataSource = ((IDataSourceProvider) this).GetResolvedSelectedDataSource();
                if (resolvedSelectedDataSource != null)
                {
                    designTimeData = DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateSampleDataTable(resolvedSelectedDataSource), this.SampleRowCount);
                    flag = true;
                }
            }
            if (designTimeData != null)
            {
                ICollection is2 = designTimeData as ICollection;
                if ((is2 == null) || (is2.Count > 0))
                {
                    return designTimeData;
                }
            }
            flag = true;
            return this.GetSampleDataSource();
        }

        protected virtual IEnumerable GetSampleDataSource()
        {
            DataTable dataTable = null;
            if (((DataBoundControl) base.Component).DataSourceID.Length > 0)
            {
                dataTable = DesignTimeData.CreateDummyDataBoundDataTable();
            }
            else
            {
                dataTable = DesignTimeData.CreateDummyDataTable();
            }
            return DesignTimeData.GetDesignTimeDataSource(dataTable, this.SampleRowCount);
        }

        private void OnDataSourceChanged(object sender, EventArgs e)
        {
            this.OnDataSourceChanged(true);
        }

        private void OnSchemaRefreshed(object sender, EventArgs e)
        {
            this.OnSchemaRefreshed();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataMember"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataMemberConverter)) });
            properties["DataMember"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataSource"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataSourceConverter)) });
            properties["DataSource"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataSourceID"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(DataSourceIDConverter)) });
            properties["DataSourceID"] = oldPropertyDescriptor;
        }

        void IDataBindingSchemaProvider.RefreshSchema(bool preferSilent)
        {
            IDataSourceDesigner dataSourceDesigner = this.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.RefreshSchema(preferSilent);
            }
        }

        IEnumerable IDataSourceProvider.GetResolvedSelectedDataSource()
        {
            IEnumerable enumerable = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                enumerable = DesignTimeData.GetSelectedDataSource((DataBoundControl) base.Component, binding.Expression, this.DataMember);
            }
            return enumerable;
        }

        object IDataSourceProvider.GetSelectedDataSource()
        {
            object selectedDataSource = null;
            DataBinding binding = base.DataBindings["DataSource"];
            if (binding != null)
            {
                selectedDataSource = DesignTimeData.GetSelectedDataSource((DataBoundControl) base.Component, binding.Expression);
            }
            return selectedDataSource;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                if (this.UseDataSourcePickerActionList)
                {
                    lists.Add(new DataBoundControlActionList(this, this.DataSourceDesigner));
                }
                lists.AddRange(base.ActionLists);
                return lists;
            }
        }

        public string DataMember
        {
            get
            {
                return ((DataBoundControl) base.Component).DataMember;
            }
            set
            {
                ((DataBoundControl) base.Component).DataMember = value;
                this.OnDataSourceChanged(true);
            }
        }

        public IDataSourceDesigner DataSourceDesigner
        {
            get
            {
                return this._dataSourceDesigner;
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

        protected virtual int SampleRowCount
        {
            get
            {
                return 5;
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

        protected virtual bool UseDataSourcePickerActionList
        {
            get
            {
                return true;
            }
        }
    }
}

