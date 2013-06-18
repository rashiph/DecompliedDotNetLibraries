namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ListControlDesigner : DataBoundControlDesigner
    {
        internal void ConnectToDataSourceAction()
        {
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConnectToDataSourceCallback), null, System.Design.SR.GetString("ListControlDesigner_ConnectToDataSource"));
        }

        private bool ConnectToDataSourceCallback(object context)
        {
            ListControlConnectToDataSourceDialog form = new ListControlConnectToDataSourceDialog(this);
            return (UIServiceHelper.ShowDialog(base.Component.Site, form) == DialogResult.OK);
        }

        protected override void DataBind(BaseDataBoundControl dataBoundControl)
        {
        }

        internal void EditItems()
        {
            PropertyDescriptor context = TypeDescriptor.GetProperties(base.Component)["Items"];
            ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.EditItemsCallback), context, System.Design.SR.GetString("ListControlDesigner_EditItems"), context);
        }

        private bool EditItemsCallback(object context)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            PropertyDescriptor propDesc = (PropertyDescriptor) context;
            new ListItemsCollectionEditor(typeof(ListItemCollection)).EditValue(new System.Web.UI.Design.WebControls.TypeDescriptorContext(service, propDesc, base.Component), new WindowsFormsEditorServiceHelper(this), propDesc.GetValue(base.Component));
            return true;
        }

        public override string GetDesignTimeHtml()
        {
            try
            {
                System.Web.UI.WebControls.ListControl viewControl = (System.Web.UI.WebControls.ListControl) base.ViewControl;
                ListItemCollection items = viewControl.Items;
                bool flag = this.IsDataBound();
                if ((items.Count == 0) || flag)
                {
                    if (flag)
                    {
                        items.Clear();
                        items.Add(System.Design.SR.GetString("Sample_Databound_Text"));
                    }
                    else
                    {
                        items.Add(System.Design.SR.GetString("Sample_Unbound_Text"));
                    }
                }
                return base.GetDesignTimeHtml();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        public IEnumerable GetResolvedSelectedDataSource()
        {
            return ((IDataSourceProvider) this).GetResolvedSelectedDataSource();
        }

        public object GetSelectedDataSource()
        {
            return ((IDataSourceProvider) this).GetSelectedDataSource();
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(System.Web.UI.WebControls.ListControl));
            base.Initialize(component);
        }

        private bool IsDataBound()
        {
            if (base.DataBindings["DataSource"] == null)
            {
                return (base.DataSourceID.Length > 0);
            }
            return true;
        }

        public virtual void OnDataSourceChanged()
        {
            base.OnDataSourceChanged(true);
        }

        protected override void OnDataSourceChanged(bool forceUpdateView)
        {
            this.OnDataSourceChanged();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            Attribute[] attributes = new Attribute[] { new TypeConverterAttribute(typeof(DataFieldConverter)) };
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["DataTextField"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, attributes);
            properties["DataTextField"] = oldPropertyDescriptor;
            oldPropertyDescriptor = (PropertyDescriptor) properties["DataValueField"];
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, attributes);
            properties["DataValueField"] = oldPropertyDescriptor;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new ListControlActionList(this, base.DataSourceDesigner));
                return lists;
            }
        }

        public string DataTextField
        {
            get
            {
                return ((System.Web.UI.WebControls.ListControl) base.Component).DataTextField;
            }
            set
            {
                ((System.Web.UI.WebControls.ListControl) base.Component).DataTextField = value;
            }
        }

        public string DataValueField
        {
            get
            {
                return ((System.Web.UI.WebControls.ListControl) base.Component).DataValueField;
            }
            set
            {
                ((System.Web.UI.WebControls.ListControl) base.Component).DataValueField = value;
            }
        }

        protected override bool UseDataSourcePickerActionList
        {
            get
            {
                return false;
            }
        }
    }
}

