namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;

    internal class RichTextBoxActionList : DesignerActionList
    {
        private RichTextBoxDesigner _designer;

        public RichTextBoxActionList(RichTextBoxDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public void EditLines()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Lines");
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "EditLines", System.Design.SR.GetString("EditLinesDisplayName"), System.Design.SR.GetString("LinksCategoryName"), System.Design.SR.GetString("EditLinesDescription"), true));
            return items;
        }
    }
}

