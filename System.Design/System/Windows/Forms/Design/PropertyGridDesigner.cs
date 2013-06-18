namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;

    internal class PropertyGridDesigner : ControlDesigner
    {
        public PropertyGridDesigner()
        {
            base.AutoResizeHandles = true;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            properties.Remove("AutoScroll");
            properties.Remove("AutoScrollMargin");
            properties.Remove("DockPadding");
            properties.Remove("AutoScrollMinSize");
            base.PreFilterProperties(properties);
        }
    }
}

