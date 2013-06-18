namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    [ToolboxItemFilter("System.Windows.Forms.MainMenu", ToolboxItemFilterType.Prevent), ToolboxItemFilter("System.Windows.Forms.UserControl", ToolboxItemFilterType.Custom)]
    internal class UserControlDocumentDesigner : DocumentDesigner
    {
        public UserControlDocumentDesigner()
        {
            base.AutoResizeHandles = true;
        }

        internal override bool CanDropComponents(DragEventArgs de)
        {
            bool flag = base.CanDropComponents(de);
            if (flag)
            {
                object[] draggingObjects = base.GetOleDragHandler().GetDraggingObjects(de);
                if (draggingObjects == null)
                {
                    return flag;
                }
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                for (int i = 0; i < draggingObjects.Length; i++)
                {
                    if (((service != null) && (draggingObjects[i] != null)) && ((draggingObjects[i] is IComponent) && (draggingObjects[i] is MainMenu)))
                    {
                        return false;
                    }
                }
            }
            return flag;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Size" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(UserControlDocumentDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private System.Drawing.Size Size
        {
            get
            {
                return this.Control.ClientSize;
            }
            set
            {
                this.Control.ClientSize = value;
            }
        }
    }
}

