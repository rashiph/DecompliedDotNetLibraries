namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ValidationSummaryDesigner : PreviewControlDesigner
    {
        protected override Control CreateViewControl()
        {
            ValidationSummary summary = (ValidationSummary) base.CreateViewControl();
            summary.ForeColor = ((ValidationSummary) base.Component).ForeColor;
            return summary;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (((ValidationSummary) base.Component).RenderingCompatibility >= new Version(4, 0))
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["ForeColor"];
                properties["ForeColor"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { new DefaultValueAttribute(typeof(Color), "") });
            }
        }
    }
}

