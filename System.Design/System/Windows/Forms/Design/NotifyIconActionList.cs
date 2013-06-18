namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;

    internal class NotifyIconActionList : DesignerActionList
    {
        private NotifyIconDesigner _designer;

        public NotifyIconActionList(NotifyIconDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public void ChooseIcon()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Icon");
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "ChooseIcon", System.Design.SR.GetString("ChooseIconDisplayName"), true));
            return items;
        }
    }
}

