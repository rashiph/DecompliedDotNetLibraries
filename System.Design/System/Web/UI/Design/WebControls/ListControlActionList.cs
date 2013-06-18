namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    internal class ListControlActionList : DesignerActionList
    {
        private IDataSourceDesigner _dataSourceDesigner;
        private ListControlDesigner _listControlDesigner;

        public ListControlActionList(ListControlDesigner listControlDesigner, IDataSourceDesigner dataSourceDesigner) : base(listControlDesigner.Component)
        {
            this._listControlDesigner = listControlDesigner;
            this._dataSourceDesigner = dataSourceDesigner;
        }

        public void ConnectToDataSource()
        {
            this._listControlDesigner.ConnectToDataSourceAction();
        }

        public void EditItems()
        {
            this._listControlDesigner.EditItems();
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this._listControlDesigner.Component);
            PropertyDescriptor descriptor = properties["DataSourceID"];
            if ((descriptor != null) && descriptor.IsBrowsable)
            {
                items.Add(new DesignerActionMethodItem(this, "ConnectToDataSource", System.Design.SR.GetString("ListControl_ConfigureDataVerb"), System.Design.SR.GetString("BaseDataBoundControl_DataActionGroup"), System.Design.SR.GetString("BaseDataBoundControl_ConfigureDataVerbDesc")));
            }
            ControlDesigner designer = this._dataSourceDesigner as ControlDesigner;
            if (designer != null)
            {
                ((DesignerActionMethodItem) items[0]).RelatedComponent = designer.Component;
            }
            descriptor = properties["Items"];
            if ((descriptor != null) && descriptor.IsBrowsable)
            {
                items.Add(new DesignerActionMethodItem(this, "EditItems", System.Design.SR.GetString("ListControl_EditItems"), "Actions", System.Design.SR.GetString("ListControl_EditItemsDesc")));
            }
            descriptor = properties["AutoPostBack"];
            if ((descriptor != null) && descriptor.IsBrowsable)
            {
                items.Add(new DesignerActionPropertyItem("AutoPostBack", System.Design.SR.GetString("ListControl_EnableAutoPostBack"), "Behavior", System.Design.SR.GetString("ListControl_EnableAutoPostBackDesc")));
            }
            return items;
        }

        public bool AutoPostBack
        {
            get
            {
                return ((ListControl) this._listControlDesigner.Component).AutoPostBack;
            }
            set
            {
                TypeDescriptor.GetProperties(this._listControlDesigner.Component)["AutoPostBack"].SetValue(this._listControlDesigner.Component, value);
            }
        }

        public override bool AutoShow
        {
            get
            {
                return true;
            }
            set
            {
            }
        }
    }
}

