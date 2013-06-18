namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class XmlDesigner : ControlDesigner
    {
        private Xml xml;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.xml = null;
            }
            base.Dispose(disposing);
        }

        public override string GetDesignTimeHtml()
        {
            return this.GetEmptyDesignTimeHtml();
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Xml_Inst"));
        }

        internal override string GetPersistInnerHtmlInternal()
        {
            Xml component = (Xml) base.Component;
            string str = (string) ((IControlDesignerAccessor) component).GetDesignModeState()["OriginalContent"];
            if (str != null)
            {
                return str;
            }
            return component.DocumentContent;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Xml));
            this.xml = (Xml) component;
            base.Initialize(component);
        }
    }
}

