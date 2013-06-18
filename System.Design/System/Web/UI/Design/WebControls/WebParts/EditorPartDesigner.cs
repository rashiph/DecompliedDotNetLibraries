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
    public class EditorPartDesigner : PartDesigner
    {
        private EditorPart _editorPart;

        protected override Control CreateViewControl()
        {
            Control control = base.CreateViewControl();
            IDictionary designModeState = ((IControlDesignerAccessor) this._editorPart).GetDesignModeState();
            ((IControlDesignerAccessor) control).SetDesignModeState(designModeState);
            return control;
        }

        public override string GetDesignTimeHtml()
        {
            if (this._editorPart.Parent is EditorZoneBase)
            {
                return base.GetDesignTimeHtml();
            }
            return base.CreateInvalidParentDesignTimeHtml(typeof(EditorPart), typeof(EditorZoneBase));
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(EditorPart));
            this._editorPart = (EditorPart) component;
            base.Initialize(component);
        }
    }
}

