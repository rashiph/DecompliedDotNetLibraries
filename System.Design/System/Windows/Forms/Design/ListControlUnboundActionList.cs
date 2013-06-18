namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;

    internal class ListControlUnboundActionList : DesignerActionList
    {
        private ComponentDesigner _designer;

        public ListControlUnboundActionList(ComponentDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "InvokeItemsDialog", System.Design.SR.GetString("ListControlUnboundActionListEditItemsDisplayName"), System.Design.SR.GetString("ItemsCategoryName"), System.Design.SR.GetString("ListControlUnboundActionListEditItemsDescription"), true));
            return items;
        }

        public void InvokeItemsDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Items");
        }
    }
}

