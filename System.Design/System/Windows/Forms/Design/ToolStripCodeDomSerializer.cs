namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class ToolStripCodeDomSerializer : ControlCodeDomSerializer
    {
        protected override bool HasSitedNonReadonlyChildren(Control parent)
        {
            ToolStrip strip = parent as ToolStrip;
            if (strip != null)
            {
                if (strip.Items.Count == 0)
                {
                    return false;
                }
                foreach (ToolStripItem item in strip.Items)
                {
                    if (((item.Site != null) && (strip.Site != null)) && (item.Site.Container == strip.Site.Container))
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(item)[typeof(InheritanceAttribute)];
                        if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

