namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Web.UI.Design;

    internal class BaseDataListActionList : DataBoundControlActionList
    {
        private ControlDesigner _controlDesigner;
        private IDataSourceDesigner _dataSourceDesigner;

        public BaseDataListActionList(ControlDesigner controlDesigner, IDataSourceDesigner dataSourceDesigner) : base(controlDesigner, dataSourceDesigner)
        {
            this._controlDesigner = controlDesigner;
            this._dataSourceDesigner = dataSourceDesigner;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection sortedActionItems = base.GetSortedActionItems();
            if (sortedActionItems == null)
            {
                sortedActionItems = new DesignerActionItemCollection();
            }
            sortedActionItems.Add(new DesignerActionMethodItem(this, "InvokePropertyBuilder", System.Design.SR.GetString("BDL_PropertyBuilderVerb"), System.Design.SR.GetString("BDL_BehaviorGroup"), System.Design.SR.GetString("BDL_PropertyBuilderDesc")));
            return sortedActionItems;
        }

        public void InvokePropertyBuilder()
        {
            ((BaseDataListDesigner) this._controlDesigner).InvokePropertyBuilder(0);
        }
    }
}

