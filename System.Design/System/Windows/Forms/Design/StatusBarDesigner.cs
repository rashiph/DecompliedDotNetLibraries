namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    internal class StatusBarDesigner : ControlDesigner
    {
        public StatusBarDesigner()
        {
            base.AutoResizeHandles = true;
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                StatusBar control = this.Control as StatusBar;
                if (control != null)
                {
                    return control.Panels;
                }
                return base.AssociatedComponents;
            }
        }
    }
}

