namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class TreeViewActionList : DesignerActionList
    {
        private TreeViewDesigner _designer;

        public TreeViewActionList(TreeViewDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "InvokeNodesDialog", System.Design.SR.GetString("InvokeNodesDialogDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("InvokeNodesDialogDescription"), true));
            items.Add(new DesignerActionPropertyItem("ImageList", System.Design.SR.GetString("ImageListDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ImageListDescription")));
            return items;
        }

        public void InvokeNodesDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Nodes");
        }

        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return ((TreeView) base.Component).ImageList;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["ImageList"].SetValue(base.Component, value);
            }
        }
    }
}

