namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class PageCatalogPartDesigner : CatalogPartDesigner
    {
        private PageCatalogPart _catalogPart;

        public override string GetDesignTimeHtml()
        {
            if (this._catalogPart.Parent is CatalogZoneBase)
            {
                return string.Empty;
            }
            return base.CreateInvalidParentDesignTimeHtml(typeof(CatalogPart), typeof(CatalogZoneBase));
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(PageCatalogPart));
            this._catalogPart = (PageCatalogPart) component;
            base.Initialize(component);
        }
    }
}

