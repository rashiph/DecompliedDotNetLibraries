namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class ListViewActionList : DesignerActionList
    {
        private ComponentDesigner _designer;

        public ListViewActionList(ComponentDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "InvokeItemsDialog", System.Design.SR.GetString("ListViewActionListEditItemsDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListEditItemsDescription"), true));
            items.Add(new DesignerActionMethodItem(this, "InvokeColumnsDialog", System.Design.SR.GetString("ListViewActionListEditColumnsDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListEditColumnsDescription"), true));
            items.Add(new DesignerActionMethodItem(this, "InvokeGroupsDialog", System.Design.SR.GetString("ListViewActionListEditGroupsDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListEditGroupsDescription"), true));
            items.Add(new DesignerActionPropertyItem("View", System.Design.SR.GetString("ListViewActionListViewDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListViewDescription")));
            items.Add(new DesignerActionPropertyItem("SmallImageList", System.Design.SR.GetString("ListViewActionListSmallImagesDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListSmallImagesDescription")));
            items.Add(new DesignerActionPropertyItem("LargeImageList", System.Design.SR.GetString("ListViewActionListLargeImagesDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ListViewActionListLargeImagesDescription")));
            return items;
        }

        public void InvokeColumnsDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Columns");
        }

        public void InvokeGroupsDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Groups");
        }

        public void InvokeItemsDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Items");
        }

        public ImageList LargeImageList
        {
            get
            {
                return ((ListView) base.Component).LargeImageList;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["LargeImageList"].SetValue(base.Component, value);
            }
        }

        public ImageList SmallImageList
        {
            get
            {
                return ((ListView) base.Component).SmallImageList;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["SmallImageList"].SetValue(base.Component, value);
            }
        }

        public System.Windows.Forms.View View
        {
            get
            {
                return ((ListView) base.Component).View;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["View"].SetValue(base.Component, value);
            }
        }
    }
}

