namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class WebPartZoneBaseDesigner : WebZoneDesigner
    {
        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(WebPartZoneBase));
            base.Initialize(component);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            Attribute[] attributes = new Attribute[] { new BrowsableAttribute(false), new EditorBrowsableAttribute(EditorBrowsableState.Never), new ThemeableAttribute(false) };
            string str = "VerbStyle";
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[str];
            if (oldPropertyDescriptor != null)
            {
                properties[str] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, attributes);
            }
        }
    }
}

