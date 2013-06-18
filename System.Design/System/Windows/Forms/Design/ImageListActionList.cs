namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ImageListActionList : DesignerActionList
    {
        private ImageListDesigner _designer;

        public ImageListActionList(ImageListDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public void ChooseImages()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Images");
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionPropertyItem("ImageSize", System.Design.SR.GetString("ImageListActionListImageSizeDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ImageListActionListImageSizeDescription")));
            items.Add(new DesignerActionPropertyItem("ColorDepth", System.Design.SR.GetString("ImageListActionListColorDepthDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("ImageListActionListColorDepthDescription")));
            items.Add(new DesignerActionMethodItem(this, "ChooseImages", System.Design.SR.GetString("ImageListActionListChooseImagesDisplayName"), System.Design.SR.GetString("LinksCategoryName"), System.Design.SR.GetString("ImageListActionListChooseImagesDescription"), true));
            return items;
        }

        public System.Windows.Forms.ColorDepth ColorDepth
        {
            get
            {
                return ((ImageList) base.Component).ColorDepth;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["ColorDepth"].SetValue(base.Component, value);
            }
        }

        public Size ImageSize
        {
            get
            {
                return ((ImageList) base.Component).ImageSize;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["ImageSize"].SetValue(base.Component, value);
            }
        }
    }
}

