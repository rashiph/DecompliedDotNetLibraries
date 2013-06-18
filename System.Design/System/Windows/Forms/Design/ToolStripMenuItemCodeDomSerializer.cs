namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Windows.Forms;

    internal class ToolStripMenuItemCodeDomSerializer : CodeDomSerializer
    {
        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            return this.GetBaseSerializer(manager).Deserialize(manager, codeObject);
        }

        private CodeDomSerializer GetBaseSerializer(IDesignerSerializationManager manager)
        {
            return (CodeDomSerializer) manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer));
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            ToolStripMenuItem item = value as ToolStripMenuItem;
            ToolStrip currentParent = item.GetCurrentParent();
            if (((item != null) && !item.IsOnDropDown) && ((currentParent != null) && (currentParent.Site == null)))
            {
                return null;
            }
            CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(typeof(ImageList).BaseType, typeof(CodeDomSerializer));
            return serializer.Serialize(manager, value);
        }
    }
}

