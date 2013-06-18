namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class PictureBoxActionList : DesignerActionList
    {
        private PictureBoxDesigner _designer;

        public PictureBoxActionList(PictureBoxDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public void ChooseImage()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Image");
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "ChooseImage", System.Design.SR.GetString("ChooseImageDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ChooseImageDescription"), true));
            items.Add(new DesignerActionPropertyItem("SizeMode", System.Design.SR.GetString("SizeModeDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("SizeModeDescription")));
            return items;
        }

        public PictureBoxSizeMode SizeMode
        {
            get
            {
                return ((PictureBox) base.Component).SizeMode;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["SizeMode"].SetValue(base.Component, value);
            }
        }
    }
}

