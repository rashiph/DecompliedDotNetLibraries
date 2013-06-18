namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class TrackBarDesigner : ControlDesigner
    {
        public TrackBarDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                selectionRules |= System.Windows.Forms.Design.SelectionRules.AllSizeable;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["AutoSize"];
                if (descriptor != null)
                {
                    bool flag = (bool) descriptor.GetValue(component);
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["Orientation"];
                    Orientation horizontal = Orientation.Horizontal;
                    if (descriptor2 != null)
                    {
                        horizontal = (Orientation) descriptor2.GetValue(component);
                    }
                    if (!flag)
                    {
                        return selectionRules;
                    }
                    switch (horizontal)
                    {
                        case Orientation.Horizontal:
                            return (selectionRules & ~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));

                        case Orientation.Vertical:
                            return (selectionRules & ~(System.Windows.Forms.Design.SelectionRules.RightSizeable | System.Windows.Forms.Design.SelectionRules.LeftSizeable));
                    }
                }
                return selectionRules;
            }
        }
    }
}

