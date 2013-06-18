namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class ToolBarDesigner : ControlDesigner
    {
        public ToolBarDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ToolBar control = this.Control as ToolBar;
                if (control != null)
                {
                    return control.Buttons;
                }
                return base.AssociatedComponents;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Dock"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["AutoSize"];
                if ((descriptor != null) && (descriptor2 != null))
                {
                    DockStyle style = (DockStyle) descriptor.GetValue(component);
                    if ((bool) descriptor2.GetValue(component))
                    {
                        selectionRules &= ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable);
                        if (style != DockStyle.None)
                        {
                            selectionRules &= ~System.Windows.Forms.Design.SelectionRules.AllSizeable;
                        }
                    }
                }
                return selectionRules;
            }
        }
    }
}

