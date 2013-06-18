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
    public class CatalogPartDesigner : PartDesigner
    {
        private CatalogPart _catalogPart;

        protected override Control CreateViewControl()
        {
            Control control = base.CreateViewControl();
            IDictionary designModeState = ((IControlDesignerAccessor) this._catalogPart).GetDesignModeState();
            ((IControlDesignerAccessor) control).SetDesignModeState(designModeState);
            return control;
        }

        public override string GetDesignTimeHtml()
        {
            if (this._catalogPart.Parent is CatalogZoneBase)
            {
                return base.GetDesignTimeHtml();
            }
            return base.CreateInvalidParentDesignTimeHtml(typeof(CatalogPart), typeof(CatalogZoneBase));
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(CatalogPart));
            this._catalogPart = (CatalogPart) component;
            base.Initialize(component);
        }
    }
}

