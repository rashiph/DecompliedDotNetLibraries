namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI.Design;

    internal class HierarchicalDataBoundControlActionList : DesignerActionList
    {
        private HierarchicalDataBoundControlDesigner _controlDesigner;
        private IHierarchicalDataSourceDesigner _dataSourceDesigner;

        public HierarchicalDataBoundControlActionList(HierarchicalDataBoundControlDesigner controlDesigner, IHierarchicalDataSourceDesigner dataSourceDesigner) : base(controlDesigner.Component)
        {
            this._controlDesigner = controlDesigner;
            this._dataSourceDesigner = dataSourceDesigner;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._controlDesigner.Component)["DataSourceID"];
            if ((descriptor != null) && descriptor.IsBrowsable)
            {
                items.Add(new DesignerActionPropertyItem("DataSourceID", System.Design.SR.GetString("BaseDataBoundControl_ConfigureDataVerb"), System.Design.SR.GetString("BaseDataBoundControl_DataActionGroup"), System.Design.SR.GetString("BaseDataBoundControl_ConfigureDataVerbDesc")));
            }
            ControlDesigner designer = this._dataSourceDesigner as ControlDesigner;
            if (designer != null)
            {
                ((DesignerActionPropertyItem) items[0]).RelatedComponent = designer.Component;
            }
            return items;
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

        [TypeConverter(typeof(HierarchicalDataSourceIDConverter))]
        public string DataSourceID
        {
            get
            {
                string dataSourceID = this._controlDesigner.DataSourceID;
                if (string.IsNullOrEmpty(dataSourceID))
                {
                    return System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource");
                }
                return dataSourceID;
            }
            set
            {
                this._controlDesigner.DataSourceID = value;
            }
        }
    }
}

