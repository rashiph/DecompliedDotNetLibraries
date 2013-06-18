namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public class HierarchicalDataBoundControlDesigner : BaseDataBoundControlDesigner
    {
        private IHierarchicalDataSourceDesigner _dataSourceDesigner;

        protected override bool ConnectToDataSource()
        {
            IHierarchicalDataSourceDesigner dataSourceDesigner = this.GetDataSourceDesigner();
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
            DialogResult result = BaseDataBoundControlDesigner.ShowCreateDataSourceDialog(this, typeof(IHierarchicalDataSource), true, out str);
            if (str.Length > 0)
            {
                base.DataSourceID = str;
            }
            return (result == DialogResult.OK);
        }

        protected override void DataBind(BaseDataBoundControl dataBoundControl)
        {
            IHierarchicalEnumerable designTimeDataSource = this.GetDesignTimeDataSource();
            string dataSourceID = dataBoundControl.DataSourceID;
            object dataSource = dataBoundControl.DataSource;
            HierarchicalDataBoundControl control = (HierarchicalDataBoundControl) dataBoundControl;
            control.DataSource = designTimeDataSource;
            control.DataSourceID = string.Empty;
            try
            {
                if (designTimeDataSource != null)
                {
                    control.DataBind();
                }
            }
            finally
            {
                control.DataSource = dataSource;
                control.DataSourceID = dataSourceID;
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

        private IHierarchicalDataSourceDesigner GetDataSourceDesigner()
        {
            IHierarchicalDataSourceDesigner designer = null;
            string dataSourceID = base.DataSourceID;
            if (!string.IsNullOrEmpty(dataSourceID))
            {
                System.Web.UI.Control component = ControlHelper.FindControl(base.Component.Site, (System.Web.UI.Control) base.Component, dataSourceID);
                if ((component != null) && (component.Site != null))
                {
                    IDesignerHost service = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        designer = service.GetDesigner(component) as IHierarchicalDataSourceDesigner;
                    }
                }
            }
            return designer;
        }

        protected virtual IHierarchicalEnumerable GetDesignTimeDataSource()
        {
            bool flag;
            IHierarchicalEnumerable designTimeData = null;
            DesignerHierarchicalDataSourceView designerView = this.DesignerView;
            if (designerView != null)
            {
                try
                {
                    designTimeData = designerView.GetDesignTimeData(out flag);
                }
                catch (Exception exception)
                {
                    if (base.Component.Site != null)
                    {
                        IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.Component.Site.GetService(typeof(IComponentDesignerDebugService));
                        if (service != null)
                        {
                            service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerHierarchicalDataSourceView.GetDesignTimeData", exception.Message }));
                        }
                    }
                }
            }
            else
            {
                DataBinding binding = base.DataBindings["DataSource"];
                if (binding != null)
                {
                    designTimeData = DesignTimeData.GetSelectedDataSource(base.Component, binding.Expression, null) as IHierarchicalEnumerable;
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

        protected virtual IHierarchicalEnumerable GetSampleDataSource()
        {
            return new HierarchicalSampleData(0, string.Empty);
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
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataSource"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(HierarchicalDataSourceConverter)) });
            properties["DataSource"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataSourceID"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(HierarchicalDataSourceIDConverter)) });
            properties["DataSourceID"] = oldPropertyDescriptor;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                if (this.UseDataSourcePickerActionList)
                {
                    lists.Add(new HierarchicalDataBoundControlActionList(this, this.DataSourceDesigner));
                }
                lists.AddRange(base.ActionLists);
                return lists;
            }
        }

        public IHierarchicalDataSourceDesigner DataSourceDesigner
        {
            get
            {
                return this._dataSourceDesigner;
            }
        }

        public DesignerHierarchicalDataSourceView DesignerView
        {
            get
            {
                DesignerHierarchicalDataSourceView view = null;
                if (this.DataSourceDesigner != null)
                {
                    view = this.DataSourceDesigner.GetView(string.Empty);
                }
                return view;
            }
        }

        protected virtual bool UseDataSourcePickerActionList
        {
            get
            {
                return true;
            }
        }

        private class HierarchicalSampleData : IHierarchicalEnumerable, IEnumerable
        {
            private ArrayList _list = new ArrayList();

            public HierarchicalSampleData(int depth, string path)
            {
                if (depth == 0)
                {
                    this._list.Add(new HierarchicalDataBoundControlDesigner.HierarchicalSampleDataNode(System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleRoot"), depth, path));
                }
                else if (depth == 2)
                {
                    this._list.Add(new HierarchicalDataBoundControlDesigner.HierarchicalSampleDataNode(System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 1 }), depth, path));
                    this._list.Add(new HierarchicalDataBoundControlDesigner.HierarchicalSampleDataNode(System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleLeaf", new object[] { 2 }), depth, path));
                }
                else
                {
                    this._list.Add(new HierarchicalDataBoundControlDesigner.HierarchicalSampleDataNode(System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleParent", new object[] { 1 }), depth, path));
                    this._list.Add(new HierarchicalDataBoundControlDesigner.HierarchicalSampleDataNode(System.Design.SR.GetString("HierarchicalDataBoundControlDesigner_SampleParent", new object[] { 2 }), depth, path));
                }
            }

            public IEnumerator GetEnumerator()
            {
                return this._list.GetEnumerator();
            }

            public IHierarchyData GetHierarchyData(object enumeratedItem)
            {
                return (IHierarchyData) enumeratedItem;
            }
        }

        private class HierarchicalSampleDataNode : IHierarchyData
        {
            private int _depth;
            private string _path;
            private string _text;

            public HierarchicalSampleDataNode(string text, int depth, string path)
            {
                this._text = text;
                this._depth = depth;
                this._path = path + '\\' + text;
            }

            public IHierarchicalEnumerable GetChildren()
            {
                return new HierarchicalDataBoundControlDesigner.HierarchicalSampleData(this._depth + 1, this._path);
            }

            public IHierarchyData GetParent()
            {
                return null;
            }

            public override string ToString()
            {
                return this._text;
            }

            public bool HasChildren
            {
                get
                {
                    return (this._depth < 2);
                }
            }

            public object Item
            {
                get
                {
                    return this;
                }
            }

            public string Path
            {
                get
                {
                    return this._path;
                }
            }

            public string Type
            {
                get
                {
                    return "SampleData";
                }
            }
        }
    }
}

