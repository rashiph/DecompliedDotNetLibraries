namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;

    internal class TextBoxActionList : DesignerActionList
    {
        public TextBoxActionList(TextBoxDesigner designer) : base(designer.Component)
        {
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionPropertyItem("Multiline", System.Design.SR.GetString("MultiLineDisplayName"), System.Design.SR.GetString("PropertiesCategoryName"), System.Design.SR.GetString("MultiLineDescription")));
            return items;
        }

        public bool Multiline
        {
            get
            {
                return ((TextBox) base.Component).Multiline;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["Multiline"].SetValue(base.Component, value);
            }
        }
    }
}

