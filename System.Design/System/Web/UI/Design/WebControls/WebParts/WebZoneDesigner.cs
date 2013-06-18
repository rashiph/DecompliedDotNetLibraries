namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class WebZoneDesigner : ControlDesigner
    {
        internal const string _templateName = "ZoneTemplate";

        internal WebZoneDesigner()
        {
        }

        internal TemplateGroup CreateZoneTemplateGroup()
        {
            TemplateGroup group = new TemplateGroup("ZoneTemplate", ((WebControl) base.ViewControl).ControlStyle);
            group.AddTemplateDefinition(new System.Web.UI.Design.TemplateDefinition(this, "ZoneTemplate", base.Component, "ZoneTemplate", ((WebControl) base.ViewControl).ControlStyle));
            return group;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(WebZone));
            base.Initialize(component);
        }

        internal System.Web.UI.Design.TemplateDefinition TemplateDefinition
        {
            get
            {
                return new System.Web.UI.Design.TemplateDefinition(this, "ZoneTemplate", base.Component, "ZoneTemplate", ((WebControl) base.ViewControl).ControlStyle, true);
            }
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }
    }
}

