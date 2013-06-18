namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DeclarativeCatalogPartDesigner : CatalogPartDesigner
    {
        private DeclarativeCatalogPart _catalogPart;
        private TemplateGroup _templateGroup;
        private const string templateName = "WebPartsTemplate";

        public override string GetDesignTimeHtml()
        {
            if (!(this._catalogPart.Parent is CatalogZoneBase))
            {
                return base.CreateInvalidParentDesignTimeHtml(typeof(CatalogPart), typeof(CatalogZoneBase));
            }
            try
            {
                if (((DeclarativeCatalogPart) base.ViewControl).WebPartsTemplate == null)
                {
                    return this.GetEmptyDesignTimeHtml();
                }
                return string.Empty;
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("DeclarativeCatalogPartDesigner_Empty"));
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(DeclarativeCatalogPart));
            base.Initialize(component);
            this._catalogPart = (DeclarativeCatalogPart) component;
            if (base.View != null)
            {
                base.View.SetFlags(ViewFlags.TemplateEditing, true);
            }
        }

        public override TemplateGroupCollection TemplateGroups
        {
            get
            {
                TemplateGroupCollection templateGroups = base.TemplateGroups;
                if (this._templateGroup == null)
                {
                    this._templateGroup = new TemplateGroup("WebPartsTemplate", this._catalogPart.ControlStyle);
                    this._templateGroup.AddTemplateDefinition(new TemplateDefinition(this, "WebPartsTemplate", this._catalogPart, "WebPartsTemplate", this._catalogPart.ControlStyle));
                }
                templateGroups.Add(this._templateGroup);
                return templateGroups;
            }
        }
    }
}

